using Microsoft.Xna.Framework;
using RiverHollow.Characters;
using RiverHollow.Game_Managers;
using RiverHollow.GUIComponents.GUIObjects;
using RiverHollow.GUIComponents.GUIObjects.GUIWindows;
using RiverHollow.GUIComponents.Screens.HUDComponents;
using System.Collections.Generic;
using System.Linq;
using static RiverHollow.Utilities.Enums;

namespace RiverHollow.GUIComponents.Screens.HUDWindows
{
    internal class HUDCodex : GUIMainObject
    {
        const int MAX_ACTOR_DISPLAY = 15;
        const int MAX_ITEM_DISPLAY = 35;
        const int ITEM_COLUMNS = 7;
        const int ACTOR_COLUMNS = 5;

        GUIWindow _gWindow;
        List<NPCDisplayWindow> _liActorDisplay;
        List<ItemDisplayWindow> _liItemDisplay;
        readonly GUIButton _btnLeft;
        readonly GUIButton _btnRight;

        GUIText _gLabel;
        GUIText _gTotal;

        GUIToggle[] _gTabToggles;
        GUIToggle[] _gItemToggles;

        GUIWindow _gInfoWindow;

        int _iIndex = 0;
        int _iHoverIndex = -1;
        CodexPageEnum _eCurrentPage = CodexPageEnum.Villagers;
        ItemEnum _eItemDisplay = ItemEnum.Resource;
        private bool IsItemPage() { return _eCurrentPage == CodexPageEnum.Items; }
        int DisplayValue => _eCurrentPage == CodexPageEnum.Items ? MAX_ITEM_DISPLAY : MAX_ACTOR_DISPLAY;
        public HUDCodex()
        {
            _liActorDisplay = new List<NPCDisplayWindow>();
            _liItemDisplay = new List<ItemDisplayWindow>();
            _gWindow = SetMainWindow(GUIWindow.DarkBlue_Window, GameManager.ScaleIt(186), GameManager.ScaleIt(177));

            _btnLeft = new GUIButton(new Rectangle(102, 34, 10, 13), DataManager.DIALOGUE_TEXTURE, BtnLeft);
            _btnLeft.Position(this);
            _btnLeft.ScaledMoveBy(7, 158);
            _btnLeft.Enable(false);
            _gWindow.AddControl(_btnLeft);

            _btnRight = new GUIButton(new Rectangle(112, 34, 10, 13), DataManager.DIALOGUE_TEXTURE, BtnRight);
            _btnRight.Position(this);
            _btnRight.ScaledMoveBy(169, 158);
            _gWindow.AddControl(_btnRight);

            _gTabToggles = new GUIToggle[5];
            AddTab(0, ShowVillagers, new Rectangle(23, 139, 22, 21), new Rectangle(0, 143, 22, 17));
            AddTab(1, ShowMerchants, new Rectangle(71, 139, 22, 21), new Rectangle(48, 143, 22, 17));
            AddTab(2, ShowTravelers, new Rectangle(23, 171, 22, 21), new Rectangle(0, 175, 22, 17));
            AddTab(3, ShowMobs, new Rectangle(71, 171, 22, 21), new Rectangle(48, 175, 22, 17));
            AddTab(4, ShowItems, new Rectangle(23, 203, 22, 21), new Rectangle(0, 207, 22, 17));

            _gItemToggles = new GUIToggle[4];
            AddItemToggle(0, ItemResourceToggle, new Point(96, 160), new Point(96, 176));
            AddItemToggle(1, ItemPotionToggle, new Point(112, 160), new Point(112, 176));
            AddItemToggle(2, ItemToolToggle, new Point(128, 160), new Point(128, 176));
            AddItemToggle(3, ItemSpecialToggle, new Point(144, 160), new Point(144, 176));

            _gTabToggles[0].AssignToggleGroup(_gTabToggles[1], _gTabToggles[2], _gTabToggles[3], _gTabToggles[4]);
            _gItemToggles[0].AssignToggleGroup(_gItemToggles[1], _gItemToggles[2], _gItemToggles[3]);

            SetUpActorWindows();
        }
        private void AddTab(int index, EmptyDelegate del, Rectangle unselected, Rectangle selected)
        {
            _gTabToggles[index] = new GUIToggle(unselected, selected, DataManager.HUD_COMPONENTS, del);
            if (index == 0)
            {
                _gTabToggles[index].Position(_gWindow);
                _gTabToggles[index].ScaledMoveBy(10, -16);
            }
            else
            {
                _gTabToggles[index].Position(_gTabToggles[index - 1]);
                _gTabToggles[index].MoveBy(_gTabToggles[index].Width - GameManager.ScaledPixel, 0);
            }
            AddControl(_gTabToggles[index]);
        }
        private void AddItemToggle(int index, EmptyDelegate del, Point unselected, Point selected)
        {
            _gItemToggles[index] = new GUIToggle(unselected, selected, new Point(16, 16), DataManager.HUD_COMPONENTS, del);
            if (index == 0)
            {
                _gItemToggles[index].Position(_gWindow);
                _gItemToggles[index].ScaledMoveBy(58, 19);
            }
            else
            {
                _gItemToggles[index].AnchorAndAlignToObject(_gItemToggles[index - 1], SideEnum.Right, SideEnum.Bottom, GameManager.ScaleIt(2));
            }
            _gWindow.AddControl(_gItemToggles[index]);
        }

        public override bool ProcessRightButtonClick(Point mouse)
        {
            if (_gInfoWindow != null)
            {
                RemoveControl(_gInfoWindow);
                return true;
            }
            return base.ProcessRightButtonClick(mouse);
        }
        public override bool ProcessHover(Point mouse)
        {
            bool rv = false;
            int initIndex = _iHoverIndex;

            List<GUIObject> boxList = IsItemPage() ? _liItemDisplay.Cast<GUIObject>().ToList() : _liActorDisplay.Cast<GUIObject>().ToList();
            for (int i = 0; i < boxList.Count; i++)
            {
                if (boxList[i].Contains(mouse))
                {
                    rv = true;
                    _iHoverIndex = i;
                }
            }

            if (!rv)
            {
                RemoveControl(_gInfoWindow);
                _gInfoWindow = null;
                _iHoverIndex = -1;
            }
            else if (initIndex != _iHoverIndex)
            {
                if (IsItemPage()) { CreateItemHoverWindow(); }
                else { CreateActorHoverWindow(); }

                AddControl(_gInfoWindow);
            }

            return false;
        }
        public void CreateActorHoverWindow()
        {
            NPCDisplayWindow hover = _liActorDisplay[_iHoverIndex];
            RemoveControl(_gInfoWindow);
            _gInfoWindow = new GUIWindow(GUIWindow.WoodenPanel, 10, 10);

            string strText = hover.Found ? DataManager.GetTextData(hover.ID, "Name", DataType.Actor) : "???";
            GUIText text = new GUIText(strText);
            text.AnchorToInnerSide(_gInfoWindow, SideEnum.TopLeft);

            if (hover.Found)
            {
                string strDescText = DataManager.GetTextData(hover.ID, "Description", DataType.Actor);
                if (!string.IsNullOrEmpty(strDescText))
                {
                    GUIText descText = new GUIText(strDescText);
                    descText.AnchorAndAlignToObject(text, SideEnum.Bottom, SideEnum.Left, GameManager.ScaleIt(2));
                }
            }

            _gInfoWindow.Resize(false);
            _gInfoWindow.AnchorAndAlignToObject(hover, SideEnum.Bottom, SideEnum.CenterX, GameManager.ScaleIt(-4));
            text.AlignToObject(_gInfoWindow, SideEnum.CenterX);
        }
        public void CreateItemHoverWindow()
        {
            ItemDisplayWindow hover = _liItemDisplay[_iHoverIndex];
            RemoveControl(_gInfoWindow);
            _gInfoWindow = new GUIWindow(GUIWindow.WoodenPanel, 10, 10);

            string strText = hover.Found ? DataManager.GetTextData(hover.ID, "Name", DataType.Item) : "???";
            GUIText text = new GUIText(strText);
            text.AnchorToInnerSide(_gInfoWindow, SideEnum.TopLeft);

            if (hover.Found)
            {
                string strDescText = DataManager.GetTextData(hover.ID, "Description", DataType.Item);
                if (!string.IsNullOrEmpty(strDescText))
                {
                    GUIText descText = new GUIText(strDescText);
                    descText.AnchorAndAlignToObject(text, SideEnum.Bottom, SideEnum.Left, GameManager.ScaleIt(2));
                }
            }

            _gInfoWindow.Resize(false);
            _gInfoWindow.AnchorAndAlignToObject(hover, SideEnum.Bottom, SideEnum.CenterX, GameManager.ScaleIt(-4));
            text.AlignToObject(_gInfoWindow, SideEnum.CenterX);
        }

        private void ClearWindows()
        {
            RemoveControl(_gInfoWindow);
            _gWindow.RemoveControl(_gLabel);
            _gWindow.RemoveControl(_gTotal);

            _liActorDisplay.ForEach(x => RemoveControl(x));
            _liActorDisplay.Clear();

            _liItemDisplay.ForEach(x => RemoveControl(x));
            _liItemDisplay.Clear();
        }
        private void ShowItemToggles(bool value)
        {
            for (int i = 0; i < _gItemToggles.Length; i++)
            {
                _gItemToggles[i].Show(value);
            }
        }

        private void SetupNewPage(CodexPageEnum e)
        {
            _iIndex = 0;
            _eCurrentPage = e;
            RemoveControl(_gInfoWindow);

            if (IsItemPage()) { SetUpItemWindows(_eItemDisplay, false); }
            else { SetUpActorWindows(); }
        }
        private void SetUpActorWindows()
        {
            ClearWindows();
            ShowItemToggles(false);

            int found = 0;
            List<Actor> actors = new List<Actor>();
            switch (_eCurrentPage)
            {
                case CodexPageEnum.Villagers:
                    actors = TownManager.DIVillagers.Values.Cast<Actor>().ToList();
                    found = TownManager.DIVillagers.Values.Count(x => x.Introduced);
                    _gLabel = new GUIText("Villagers");
                    break;
                case CodexPageEnum.Merchants:
                    actors = TownManager.DIMerchants.Values.Cast<Actor>().ToList();
                    found = TownManager.DIMerchants.Values.Count(x => x.Introduced);
                    _gLabel = new GUIText("Merchants");
                    break;
                case CodexPageEnum.Travelers:
                    _gLabel = new GUIText("Travelers");
                    TownManager.DITravelerInfo.Keys.ToList().ForEach(x => actors.Add(DataManager.CreateTraveler(x)));
                    found = TownManager.DITravelerInfo.Values.Count(x => x.Item1);
                    break;
                case CodexPageEnum.Mobs:
                    PlayerManager.DIMobInfo.Keys.ToList().ForEach(x => actors.Add(DataManager.CreateMob(x)));
                    found = PlayerManager.DIMobInfo.Values.Count(x => x > 0);
                    _gLabel = new GUIText("Enemies");
                    break;
                case CodexPageEnum.Items:
                    _gLabel = new GUIText("Items");
                    break;
            }
            _gLabel.AnchorToInnerSide(_gWindow, SideEnum.Top);

            for (int i = _iIndex; i < _iIndex + MAX_ACTOR_DISPLAY; i++)
            {
                if (actors.Count <= i)
                {
                    break;
                }

                var npc = new NPCDisplayWindow(actors[i]);
                _liActorDisplay.Add(npc);
                AddControl(npc);
            }

            GUIUtils.CreateSpacedGrid(new List<GUIObject>(_liActorDisplay), this.Position() + GameManager.ScaleIt(new Point(9, 18)), _iIndex, _iIndex + MAX_ACTOR_DISPLAY, ACTOR_COLUMNS, 2, 3);


            _gTotal = new GUIText(string.Format("{0}/{1}", found, actors.Count));
            _gTotal.AlignToObject(_gWindow, SideEnum.Center);
            _gTotal.AlignToObject(_btnLeft, SideEnum.CenterY);

            _btnLeft.Enable(_iIndex >= MAX_ACTOR_DISPLAY);
            _btnRight.Enable(_iIndex + MAX_ACTOR_DISPLAY < actors.Count);
        }
        private void SetUpItemWindows(ItemEnum itemWindow, bool reset)
        {
            if (reset)
            {
                _iIndex = 0;
                _eItemDisplay = itemWindow;
            }
            ClearWindows();
            ShowItemToggles(true);

            int found = 0;
            _gLabel = new GUIText("Items");
            _gLabel.AnchorToInnerSide(_gWindow, SideEnum.Top);

            List<int> itemIDs = new List<int>(TownManager.DIArchive.Keys.ToList().Where(x => DataManager.GetEnumByIDKey<ItemEnum>(x, "Type", DataType.Item) == _eItemDisplay));
            found = itemIDs.Count(x => TownManager.DIArchive[x].Item1);

            for (int i = _iIndex; i < _iIndex + MAX_ITEM_DISPLAY; i++)
            {
                if (itemIDs.Count <= i)
                {
                    break;
                }

                int museumIndex = itemIDs[i];
                var displayWindow = new ItemDisplayWindow(museumIndex, TownManager.DIArchive[museumIndex]);
                _liItemDisplay.Add(displayWindow);
                AddControl(displayWindow);
            }

            GUIUtils.CreateSpacedGrid(new List<GUIObject>(_liItemDisplay), this.Position() + GameManager.ScaleIt(new Point(14, 40)), _iIndex, _iIndex + MAX_ITEM_DISPLAY, ITEM_COLUMNS, 3, 3);

            _gTotal = new GUIText(string.Format("{0}/{1}", found, itemIDs.Count));
            _gTotal.AlignToObject(_gWindow, SideEnum.Center);
            _gTotal.AlignToObject(_btnLeft, SideEnum.CenterY);

            _btnLeft.Enable(_iIndex >= MAX_ITEM_DISPLAY);
            _btnRight.Enable(_iIndex + MAX_ITEM_DISPLAY < itemIDs.Count);
        }

        public void BtnLeft()
        {
            _iIndex -= DisplayValue;
            _btnRight.Enable(true);
            if (IsItemPage()) { SetUpItemWindows(_eItemDisplay, false); }
            else { SetUpActorWindows(); }
        }
        public void BtnRight()
        {
            _iIndex += DisplayValue;
            _btnLeft.Enable(true);
            if (IsItemPage()) { SetUpItemWindows(_eItemDisplay, false); }
            else { SetUpActorWindows(); }
        }

        #region PageToggles
        public void ShowVillagers()
        {
            SetupNewPage(CodexPageEnum.Villagers);
        }
        public void ShowMerchants()
        {
            SetupNewPage(CodexPageEnum.Merchants);
        }
        public void ShowTravelers()
        {
            SetupNewPage(CodexPageEnum.Travelers);
        }
        public void ShowMobs()
        {
            SetupNewPage(CodexPageEnum.Mobs);
        }
        public void ShowItems()
        {
            SetupNewPage(CodexPageEnum.Items);
        }
        #endregion

        #region ItemTypeToggles
        public void ItemResourceToggle()
        {
            SetUpItemWindows(ItemEnum.Resource, true);
        }
        public void ItemPotionToggle()
        {
            SetUpItemWindows(ItemEnum.Consumable, true);
        }
        public void ItemToolToggle()
        {
            SetUpItemWindows(ItemEnum.Tool, true);
        }
        public void ItemSpecialToggle()
        {
            SetUpItemWindows(ItemEnum.Special, true);
        }
        #endregion
    }
}
