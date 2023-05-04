using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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
        CodexPageEnum _eCurrentPage = CodexPageEnum.Villagers;
        ItemEnum _eItemDisplay = ItemEnum.Resource;
        private bool IsItemPage() { return _eCurrentPage == CodexPageEnum.Items; }
        int DisplayValue => _eCurrentPage == CodexPageEnum.Items ? MAX_ITEM_DISPLAY : MAX_ACTOR_DISPLAY;
        public HUDCodex()
        {
            _liActorDisplay = new List<NPCDisplayWindow>();
            _liItemDisplay = new List<ItemDisplayWindow>();
            _winMain = SetMainWindow(GUIUtils.DarkBlue_Window, GameManager.ScaleIt(186), GameManager.ScaleIt(177));

            _btnLeft = new GUIButton(GUIUtils.BTN_LEFT_SMALL, BtnLeft);
            _btnLeft.PositionAndMove(_winMain, 7, 158);
            _btnLeft.Enable(false);

            _btnRight = new GUIButton(GUIUtils.BTN_RIGHT_SMALL, BtnRight);
            _btnRight.PositionAndMove(_winMain, 169, 158);

            _gTabToggles = new GUIToggle[5];
            AddTab(0, ShowVillagers, GUIUtils.TOGGLE_VILLAGERS_ON, GUIUtils.TOGGLE_VILLAGERS_OFF);
            AddTab(1, ShowMerchants, GUIUtils.TOGGLE_MERCHANTS_ON, GUIUtils.TOGGLE_MERCHANTS_OFF);
            AddTab(2, ShowTravelers, GUIUtils.TOGGLE_TRAVELERS_ON, GUIUtils.TOGGLE_TRAVELERS_OFF);
            AddTab(3, ShowMobs, GUIUtils.TOGGLE_MOBS_ON, GUIUtils.TOGGLE_MOBS_OFF);
            AddTab(4, ShowItems, GUIUtils.TOGGLE_ITEMS_ON, GUIUtils.TOGGLE_ITEMS_OFF);

            _gItemToggles = new GUIToggle[4];
            AddItemToggle(0, ItemResourceToggle, GUIUtils.TOGGLE_RESOURCE_OFF, GUIUtils.TOGGLE_RESOURCE_ON);
            AddItemToggle(1, ItemPotionToggle, GUIUtils.TOGGLE_POTIONS_OFF, GUIUtils.TOGGLE_POTIONS_ON);
            AddItemToggle(2, ItemToolToggle, GUIUtils.TOGGLE_TOOLS_OFF, GUIUtils.TOGGLE_TOOLS_ON);
            AddItemToggle(3, ItemSpecialToggle, GUIUtils.TOGGLE_SPECIAL_OFF, GUIUtils.TOGGLE_SPECIAL_ON);

            _gTabToggles[0].AssignToggleGroup(_gTabToggles[1], _gTabToggles[2], _gTabToggles[3], _gTabToggles[4]);
            _gItemToggles[0].AssignToggleGroup(_gItemToggles[1], _gItemToggles[2], _gItemToggles[3]);

            SetUpActorWindows();
        }
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
        private void AddItemToggle(int index, EmptyDelegate del, Rectangle unselected, Rectangle selected)
        {
            _gItemToggles[index] = new GUIToggle(unselected.Location, selected.Location, new Point(16, 16), DataManager.HUD_COMPONENTS, del);
            if (index == 0)
            {
                _gItemToggles[index].PositionAndMove(_winMain, 58, 19);
            }
            else
            {
                _gItemToggles[index].AnchorAndAlignWithSpacing(_gItemToggles[index - 1], SideEnum.Right, SideEnum.Bottom, 2);
            }
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

        private void ClearWindows()
        {
            RemoveControl(_gInfoWindow);
            _winMain.RemoveControl(_gLabel);
            _winMain.RemoveControl(_gTotal);

            _liActorDisplay.ForEach(x => x.RemoveSelfFromControl());
            _liActorDisplay.Clear();

            _liItemDisplay.ForEach(x => x.RemoveSelfFromControl());
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
            _gLabel.AnchorToInnerSide(_winMain, SideEnum.Top);

            for (int i = _iIndex; i < _iIndex + MAX_ACTOR_DISPLAY; i++)
            {
                if (actors.Count <= i)
                {
                    break;
                }

                var npc = new NPCDisplayWindow(actors[i]);
                _liActorDisplay.Add(npc);
            }

            GUIUtils.CreateSpacedGrid(new List<GUIObject>(_liActorDisplay), _winMain, new Point(9, 18), ACTOR_COLUMNS, 2, 3);

            _gTotal = new GUIText(string.Format("{0}/{1}", found, actors.Count));
            _gTotal.AlignToObject(_winMain, SideEnum.Center);
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
            _gLabel.AnchorToInnerSide(_winMain, SideEnum.Top);

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
            }

            GUIUtils.CreateSpacedGrid(new List<GUIObject>(_liItemDisplay), _winMain, new Point(14, 40), ITEM_COLUMNS, 3, 3);

            _gTotal = new GUIText(string.Format("{0}/{1}", found, itemIDs.Count));
            _gTotal.AlignToObject(_winMain, SideEnum.Center);
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
