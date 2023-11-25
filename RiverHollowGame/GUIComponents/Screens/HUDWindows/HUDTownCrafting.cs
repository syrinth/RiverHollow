using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.Game_Managers;
using RiverHollow.GUIComponents.GUIObjects;
using RiverHollow.GUIComponents.GUIObjects.GUIWindows;
using RiverHollow.Items;
using RiverHollow.Utilities;
using RiverHollow.WorldObjects;
using System.Collections.Generic;
using System.Linq;
using static RiverHollow.GUIComponents.Screens.HUDComponents.HUDMenu;
using static RiverHollow.Utilities.Enums;

namespace RiverHollow.GUIComponents.Screens.HUDWindows
{
    internal class HUDTownCrafting : GUIMainObject
    {
        const int MAX_DISPLAY = 28;
        const int COLUMNS = 7;

        int _iIndex = 0;

        readonly List<int> _liUniqueRecipes;
        readonly List<int> _liCraftingRecipes;
        readonly List<GUIItemBoxHover> _liItemDisplay;

        GUIImage _gSelection;
        GUIImage _gBackgroundBox;
        GUIImage _gScroll;

        GUIButton _btnLeft;
        GUIButton _btnRight;
        GUIButton _btnBuild;
        List<GUIItemBox> _liRequiredItems;

        GUIText _gName;
        GUISprite _gStructure;

        GUIToggle[] _gTabToggles;

        BuildPageEnum _eCurrentPage = BuildPageEnum.Structures;

        private bool IsStructurePage() { return _eCurrentPage == BuildPageEnum.Structures; }

        private CloseMenuDelegate _closeMenu;
        public HUDTownCrafting(CloseMenuDelegate closeMenu)
        {
            _closeMenu = closeMenu;

            _liUniqueRecipes = new List<int>();
            _liRequiredItems = new List<GUIItemBox>();
            _liItemDisplay = new List<GUIItemBoxHover>();

            _liCraftingRecipes = PlayerManager.GetCraftingList().FindAll(x => !((Buildable)DataManager.CreateWorldObjectByID(x)).Unique);
            _winMain = SetMainWindow(GUIUtils.WINDOW_DARKBLUE, GameManager.ScaleIt(186), GameManager.ScaleIt(152));
            _gSelection = new GUIImage(GUIUtils.SELECT_CORNER);
            _gSelection.Show(false);

            _btnLeft = new GUIButton(GUIUtils.BTN_LEFT_SMALL, BtnLeft);
            _btnRight = new GUIButton(GUIUtils.BTN_RIGHT_SMALL, BtnRight);

            _gScroll = new GUIImage(GUIUtils.HUD_SCROLL_L);

            _gTabToggles = new GUIToggle[5];
            AddTab(0, ShowStructures, GUIUtils.TOGGLE_STRUCTURES_OFF, GUIUtils.TOGGLE_STRUCTURES_ON);
            AddTab(1, ShowFlooring, GUIUtils.TOGGLE_FLOORING_OFF, GUIUtils.TOGGLE_FLOORING_ON);
            AddTab(2, ShowWalls, GUIUtils.TOGGLE_WALLS_OFF, GUIUtils.TOGGLE_WALLS_ON);
            AddTab(3, ShowFurniture, GUIUtils.TOGGLE_FURNITURE_OFF, GUIUtils.TOGGLE_FURNITURE_ON);
            AddTab(4, ShowLighting, GUIUtils.TOGGLE_LIGHTING_OFF, GUIUtils.TOGGLE_LIGHTING_ON);

            _gTabToggles[0].AssignToggleGroup(_gTabToggles.Where(x => x != _gTabToggles[0]).ToArray());

            var idList = PlayerManager.GetCraftingList().FindAll(x => ((Buildable)DataManager.CreateWorldObjectByID(x)).Unique);
            foreach (int i in idList)
            {
                Buildable obj = (Buildable)DataManager.CreateWorldObjectByID(i);
                if (obj.UniqueNotBuilt())
                {
                    _liUniqueRecipes.Add(obj.ID);
                }
            }

            _gName = new GUIText("");
            SetupStructureWindow();
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
            _gSelection?.Draw(spriteBatch);
        }

        public override bool ProcessLeftButtonClick(Point mouse)
        {
            bool rv = GUIUtils.ProcessLeftMouseButton(mouse, _btnLeft, _btnRight, _btnBuild, _gTabToggles[0], _gTabToggles[1], _gTabToggles[2], _gTabToggles[3]);

            if (!rv)
            { 
                for (int i = 0; i < _liItemDisplay.Count; i++)
                {
                    if (_liItemDisplay[i].Contains(mouse))
                    {
                        rv = true;

                        if (_liItemDisplay[i].BoxItem != null)
                        {
                            int objID = _liItemDisplay[i].BoxItem.ID;
                            Buildable obj = (Buildable)DataManager.CreateWorldObjectByID(objID);
                            Dictionary<int, int> requiredToMake = obj.RequiredToMake;

                            if (!obj.IsDirectBuild()) {
                                if (PlayerManager.ExpendResources(requiredToMake))
                                {
                                    InventoryManager.AddToInventory(DataManager.GetItem(obj));
                                    SetupCraftingWindows();
                                    SoundManager.PlayEffect(SoundEffectEnum.Thump);
                                }
                            }
                            else
                            {
                                BuildInTownMode(requiredToMake, obj);
                            }
                        }
                        break;
                    }
                }
            }

            return rv;
        }

        public void BtnLeft()
        {
            if (_eCurrentPage == BuildPageEnum.Structures)
            {
                if (_iIndex > 0)
                {
                    _iIndex--;
                    DisplayStructureInfo();
                }
            }
            else
            {
                _iIndex -= MAX_DISPLAY;
                _btnRight.Enable(true);
                SetupCraftingWindows();
            }
        }
        public void BtnRight()
        {
            if (_eCurrentPage == BuildPageEnum.Structures)
            {
                if (_iIndex < _liUniqueRecipes.Count)
                {
                    _iIndex++;
                    DisplayStructureInfo();
                }
            }
            else
            {
                _iIndex += MAX_DISPLAY;
                _btnLeft.Enable(true);
                SetupCraftingWindows();
            }
        }
        public void BtnBuild()
        {
            Buildable obj;

            obj = (Buildable)DataManager.CreateWorldObjectByID(_liUniqueRecipes[_iIndex]);
            var requiredToMake = obj.RequiredToMake;

            BuildInTownMode(requiredToMake, obj);
        }

        private void BuildInTownMode(Dictionary<int, int> requiredToMake, Buildable obj)
        {
            if (InventoryManager.HasSufficientItems(requiredToMake))
            {
                GameManager.EnterTownModeBuild(false);
                GameManager.PickUpWorldObject(obj);

                GUIManager.CloseMainObject();
                _closeMenu();
            }
        }

        #region PageToggles
        public void ShowStructures()
        {
            SetupNewPage(BuildPageEnum.Structures);
        }
        public void ShowFlooring()
        {
            SetupNewPage(BuildPageEnum.Flooring);
        }
        public void ShowWalls()
        {
            SetupNewPage(BuildPageEnum.Walls);
        }
        public void ShowFurniture()
        {
            SetupNewPage(BuildPageEnum.Furniture);
        }
        public void ShowLighting()
        {
            SetupNewPage(BuildPageEnum.Lighting);
        }
        #endregion

        #region Setup
        private void ClearWindows()
        {
            _winMain.RemoveControl(_gBackgroundBox);
            _winMain.RemoveControl(_btnBuild);
            _gSelection.Show(false);

            _liRequiredItems.ForEach(x => x.RemoveSelfFromControl());
            _liRequiredItems.Clear();

            _liItemDisplay.ForEach(x => x.RemoveSelfFromControl());
            _liItemDisplay.Clear();

            _gName.SetText("");
        }

        private void SetupNewPage(BuildPageEnum e)
        {
            _iIndex = 0;
            _eCurrentPage = e;

            if (IsStructurePage()) { SetupStructureWindow(); }
            else { SetupCraftingWindows(); }
        }
        private void SetupCraftingWindows()
        {
            ClearWindows();

            //int found = 0;
            List<int> objects = new List<int>();
            switch (_eCurrentPage)
            {
                case BuildPageEnum.Flooring:
                    objects = _liCraftingRecipes.Where(x => DataManager.GetEnumByIDKey<BuildableEnum>(x, "Subtype", DataType.WorldObject) == BuildableEnum.Floor).ToList();
                    //found = TownManager.DIVillagers.Values.Count(x => x.Introduced);
                    break;
                case BuildPageEnum.Walls:
                    objects = _liCraftingRecipes.Where(x => DataManager.GetEnumByIDKey<BuildableEnum>(x, "Subtype", DataType.WorldObject) == BuildableEnum.Wall).ToList();
                    //found = TownManager.DIMerchants.Values.Count(x => x.Introduced);
                    break;
                case BuildPageEnum.Furniture:
                    var showThese = new List<BuildableEnum>() { BuildableEnum.Decor, BuildableEnum.Basic, BuildableEnum.Container };
                    objects = _liCraftingRecipes.Where(x => showThese.Contains(DataManager.GetEnumByIDKey<BuildableEnum>(x, "Subtype", DataType.WorldObject))).ToList();
                    //found = TownManager.DITravelerInfo.Values.Count(x => x.Item1);
                    break;
                case BuildPageEnum.Lighting:
                    //PlayerManager.DIMobInfo.Keys.ToList().ForEach(x => objects.Add(DataManager.CreateMob(x)));
                   // found = PlayerManager.DIMobInfo.Values.Count(x => x > 0);
                    break;
            }

            for (int i = _iIndex; i < _iIndex + MAX_DISPLAY; i++)
            {
                Item newItem = null;
                if (objects.Count > i)
                {
                    newItem = DataManager.GetItem(objects[i] + Constants.BUILDABLE_ID_OFFSET);
                }

                var displayWindow = new GUIItemBoxHover(newItem, ItemBoxDraw.Never, UpdateInfo);

                if (newItem != null)
                {
                    Buildable obj = (Buildable)DataManager.CreateWorldObjectByID(newItem.ID);
                    if (!InventoryManager.HasSufficientItems(obj.RequiredToMake))
                    {
                        displayWindow.SetItemAlpha(0.3f);
                    }
                }
                else
                {
                    displayWindow.Enable(false);
                }

                _liItemDisplay.Add(displayWindow);
            }

            GUIUtils.CreateSpacedGrid(new List<GUIObject>(_liItemDisplay), _winMain, new Point(14, 10), COLUMNS, 3, 3);

            _gScroll.AlignToObject(_winMain, SideEnum.CenterX);
            _gScroll.AnchorToObject(_liItemDisplay[MAX_DISPLAY - 1], SideEnum.Bottom, 4);

            _btnLeft.AnchorToInnerSide(_winMain, SideEnum.BottomLeft, 2);
            _btnRight.AnchorToInnerSide(_winMain, SideEnum.BottomRight, 2);

            _btnLeft.Enable(_iIndex >= MAX_DISPLAY);
            _btnRight.Enable(_iIndex + MAX_DISPLAY < _liCraftingRecipes.Count);
        }
        private void SetupStructureWindow()
        {
            ClearWindows();
            _gBackgroundBox = new GUIImage(GUIUtils.STRUCTURE_BOX);
            _gBackgroundBox.PositionAndMove(_winMain, 59, 12);

            _btnLeft.AnchorAndAlignWithSpacing(_gBackgroundBox, SideEnum.Left, SideEnum.CenterY, 4);
            _btnLeft.Enable(false);
            
            _btnRight.AnchorAndAlignWithSpacing(_gBackgroundBox, SideEnum.Right, SideEnum.CenterY, 4);
            _btnRight.Enable(_liUniqueRecipes.Count > 1);

            _gScroll.AlignToObject(_winMain, SideEnum.CenterX);
            _gScroll.AnchorToObject(_gBackgroundBox, SideEnum.Bottom, 4);

            _btnBuild = new GUIButton("Build", BtnBuild);
            _btnBuild.Position(_winMain);
            _btnBuild.AnchorAndAlignWithSpacing(_gScroll, SideEnum.Bottom, SideEnum.CenterX, 43);

            DisplayStructureInfo();
        }

        private void DisplayStructureInfo()
        {
            if (_liUniqueRecipes.Count == 0)
            {
                _btnBuild.Enable(false);
                return;
            }

            _btnRight.Enable(_iIndex < _liUniqueRecipes.Count - 1);
            _btnLeft.Enable(_iIndex > 0);

            _gStructure?.RemoveSelfFromControl();
            _gName?.RemoveSelfFromControl();

            Buildable obj = (Buildable)DataManager.CreateWorldObjectByID(_liUniqueRecipes[_iIndex]);
            _gStructure = new GUISprite(obj.Sprite, true);

            GUIUtils.SetObjectScale(_gStructure, obj.Width, obj.Height, 4);
            _gStructure.CenterOnObject(_gBackgroundBox);

            bool sufficientItems = GUIUtils.CreateRequiredItemsList(ref _liRequiredItems, obj.RequiredToMake);

            _btnBuild.Enable(sufficientItems && obj.CanBuild());
            _gStructure.Alpha(sufficientItems ? 1 : 0.3f);

            _gName = new GUIText(obj.Name());
            _gName.SetColor(sufficientItems ? Color.Black : Color.Red);
            _gName.AnchorAndAlignWithSpacing(_gScroll, SideEnum.Bottom, SideEnum.CenterX, 4);

            GUIUtils.CreateSpacedRowAgainstObject(new List<GUIObject>(_liRequiredItems), _winMain, _gScroll, 2, 22);
        }

        private void UpdateInfo(GUIItemBoxHover obj)
        {
            if (obj.BoxItem != null)
            {
                _gSelection.CenterOnObject(obj);
                _gSelection.Show(true);

                int objID = obj.BoxItem.ID;
                Buildable building = (Buildable)DataManager.CreateWorldObjectByID(objID);
                bool sufficientItems = GUIUtils.CreateRequiredItemsList(ref _liRequiredItems, building.RequiredToMake);

                _gName.SetText(building.Name());
                _gName.SetColor(sufficientItems ? Color.Black : Color.Red);
                _gName.AnchorAndAlignWithSpacing(_gScroll, SideEnum.Bottom, SideEnum.CenterX, 4);

                GUIUtils.CreateSpacedRowAgainstObject(new List<GUIObject>(_liRequiredItems), base._winMain, _gScroll, 2, 22);
            }
        }
        #endregion

        private void AddTab(int index, EmptyDelegate del, Rectangle unselected, Rectangle selected)
        {
            _gTabToggles[index] = new GUIToggle(unselected, selected, DataManager.HUD_COMPONENTS, del);
            AddControl(_gTabToggles[index]);
            if (index == 0)
            {
                _gTabToggles[index].PositionAndMove(_winMain, 10, -16);
            }
            else
            {
                _gTabToggles[index].AnchorAndAlign(_gTabToggles[index - 1], SideEnum.Right, SideEnum.Bottom);
            }

        }
    }
}

