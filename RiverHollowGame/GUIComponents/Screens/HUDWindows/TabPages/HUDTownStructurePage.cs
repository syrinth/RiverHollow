using Microsoft.Xna.Framework;
using RiverHollow.Game_Managers;
using RiverHollow.GUIComponents.GUIObjects;
using RiverHollow.GUIComponents.GUIObjects.GUIWindows;
using RiverHollow.WorldObjects;
using System.Collections.Generic;
using static RiverHollow.GUIComponents.Screens.HUDComponents.HUDMenu;

namespace RiverHollow.GUIComponents.Screens.HUDWindows.TabPages
{
    internal class HUDTownStructurePage : GUIObject
    {
        int _iIndex = 0;
        GUISprite _gStructure;

        readonly GUIWindow _mainWindow;
        readonly GUIImage _gBackgroundBox;

        readonly GUIButton _btnLeft;
        readonly GUIButton _btnRight;
        readonly GUIButton _btnBuild;
        readonly GUIImage _gScroll;
        GUIText _gName;

        List<GUIItemBox> _liRequiredItems;
        readonly List<int> _liUniqueRecipes;

        private CloseMenuDelegate _closeMenu;

        public HUDTownStructurePage(GUIWindow winMain, CloseMenuDelegate closeMenu)
        {
            _mainWindow = winMain;
            _closeMenu = closeMenu;

            _liRequiredItems = new List<GUIItemBox>();
            _liUniqueRecipes = new List<int>();
            var idList = PlayerManager.GetCraftingList().FindAll(x => ((Buildable)DataManager.CreateWorldObjectByID(x)).Unique);
            foreach (int i in idList)
            {
                Buildable obj = (Buildable)DataManager.CreateWorldObjectByID(i);
                if (obj.UniqueNotBuilt())
                {
                    _liUniqueRecipes.Add(obj.ID);
                }
            }

            _gBackgroundBox = new GUIImage(GUIUtils.STRUCTURE_BOX);
            _gBackgroundBox.PositionAndMove(_mainWindow, 59, 12);

            _btnLeft = new GUIButton(GUIUtils.BTN_LEFT_SMALL, BtnLeft);
            _btnLeft.AnchorAndAlignWithSpacing(_gBackgroundBox, SideEnum.Left, SideEnum.CenterY, 4);
            _btnLeft.Enable(false);

            _btnRight = new GUIButton(GUIUtils.BTN_RIGHT_SMALL, BtnRight);
            _btnRight.AnchorAndAlignWithSpacing(_gBackgroundBox, SideEnum.Right, SideEnum.CenterY, 4);
            _btnRight.Enable(_liUniqueRecipes.Count > 1);

            _gName = new GUIText("");
            _gScroll = new GUIImage(GUIUtils.HUD_SCROLL_L);
            _gScroll.AlignToObject(_mainWindow, SideEnum.CenterX);
            _gScroll.AnchorToObject(_gBackgroundBox, SideEnum.Bottom, 4);

            _btnBuild = new GUIButton("Build", BtnBuild);
            _btnBuild.Position(_mainWindow);
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

            GUIUtils.CreateSpacedRowAgainstObject(new List<GUIObject>(_liRequiredItems), _mainWindow, _gScroll, 2, 22);

        }
        public void BtnLeft()
        {
            if (_iIndex > 0)
            {
                _iIndex--;
                DisplayStructureInfo();
            }

        }
        public void BtnRight()
        {
            if (_iIndex < _liUniqueRecipes.Count)
            {
                _iIndex++;
                DisplayStructureInfo();
            }
        }
        public void BtnBuild()
        {
            Buildable obj;

            obj = (Buildable)DataManager.CreateWorldObjectByID(_liUniqueRecipes[_iIndex]);
            var requiredToMake = obj.RequiredToMake;

            GameManager.BuildInTownMode(requiredToMake, obj, _closeMenu);
        }
    }
}
