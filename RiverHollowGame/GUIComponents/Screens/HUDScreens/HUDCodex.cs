using Microsoft.Xna.Framework;
using RiverHollow.Characters;
using RiverHollow.Game_Managers;
using RiverHollow.GUIComponents.GUIObjects;
using RiverHollow.GUIComponents.GUIObjects.GUIWindows;
using RiverHollow.GUIComponents.Screens.HUDComponents;
using RiverHollow.Items;
using RiverHollow.Utilities;
using RiverHollow.WorldObjects;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using static RiverHollow.Utilities.Enums;

namespace RiverHollow.GUIComponents.Screens.HUDScreens
{
    internal class HUDCodex : GUIMainObject
    {
        const int MAX_ACTOR_DISPLAY = 15;
        const int MAX_ITEM_DISPLAY = 35;

        GUIWindow _gWindow;
        List<NPCDisplayWindow> _liActorDisplay;
        List<ItemDisplayWindow> _liItemDisplay;
        readonly GUIButton _btnLeft;
        readonly GUIButton _btnRight;

        GUIText _gLabel;
        GUIText _gTotal;

        GUITabButton _btnVillagers;
        GUITabButton _btnMerchants;
        GUITabButton _btnTravelers;
        GUITabButton _btnMobs;
        GUITabButton _btnItems;

        GUIToggle _gResourceToggle;
        GUIToggle _gConsumableToggle;
        GUIToggle _gToolToggle;
        GUIToggle _gSpecialToggle;

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

            _btnVillagers = new GUITabButton(new Rectangle(0, 143, 22, 17), new Rectangle(23, 139, 22, 21), DataManager.HUD_COMPONENTS, ShowVillagers);
            _btnVillagers.Position(_gWindow);
            _btnVillagers.ScaledMoveBy(10, -16);
            AddControl(_btnVillagers);

            _btnMerchants = new GUITabButton(new Rectangle(48, 143, 22, 17), new Rectangle(71, 139, 22, 21), DataManager.HUD_COMPONENTS, ShowMerchants);
            _btnMerchants.Position(_btnVillagers);
            _btnMerchants.MoveBy(_btnMerchants.Width - GameManager.ScaledPixel, 0);
            AddControl(_btnMerchants);

            _btnTravelers = new GUITabButton(new Rectangle(0, 175, 22, 17), new Rectangle(23, 171, 22, 21), DataManager.HUD_COMPONENTS, ShowTravelers);
            _btnTravelers.Position(_btnMerchants);
            _btnTravelers.MoveBy(_btnTravelers.Width - GameManager.ScaledPixel, 0);
            AddControl(_btnTravelers);

            _btnMobs = new GUITabButton(new Rectangle(48, 175, 22, 17), new Rectangle(71, 171, 22, 21), DataManager.HUD_COMPONENTS, ShowMobs);
            _btnMobs.Position(_btnTravelers);
            _btnMobs.MoveBy(_btnMobs.Width - GameManager.ScaledPixel, 0);
            AddControl(_btnMobs);

            _btnItems = new GUITabButton(new Rectangle(0, 207, 22, 17), new Rectangle(23, 203, 22, 21), DataManager.HUD_COMPONENTS, ShowItems);
            _btnItems.Position(_btnMobs);
            _btnItems.MoveBy(_btnItems.Width - GameManager.ScaledPixel, 0);
            AddControl(_btnItems);

            _gResourceToggle = new GUIToggle(new Point(96, 160), new Point(96, 176), new Point(16, 16), DataManager.HUD_COMPONENTS, ItemResourceToggle);
            _gResourceToggle.Position(_gWindow);
            _gResourceToggle.ScaledMoveBy(58, 19);
            _gWindow.AddControl(_gResourceToggle);

            _gConsumableToggle = new GUIToggle(new Point(112, 160), new Point(112, 176), new Point(16, 16), DataManager.HUD_COMPONENTS, ItemPotionToggle);
            _gConsumableToggle.AnchorAndAlignToObject(_gResourceToggle, SideEnum.Right, SideEnum.Bottom, GameManager.ScaleIt(2));
            _gWindow.AddControl(_gConsumableToggle);

            _gToolToggle = new GUIToggle(new Point(128, 160), new Point(128, 176), new Point(16, 16), DataManager.HUD_COMPONENTS, ItemToolToggle);
            _gToolToggle.AnchorAndAlignToObject(_gConsumableToggle, SideEnum.Right, SideEnum.Bottom, GameManager.ScaleIt(2));
            _gWindow.AddControl(_gToolToggle);

            _gSpecialToggle = new GUIToggle(new Point(144, 160), new Point(144, 176), new Point(16, 16), DataManager.HUD_COMPONENTS, ItemSpecialToggle);
            _gSpecialToggle.AnchorAndAlignToObject(_gToolToggle, SideEnum.Right, SideEnum.Bottom, GameManager.ScaleIt(2));
            _gWindow.AddControl(_gSpecialToggle);

            _btnVillagers.SetSelected(true);

            SyncToggles(_gResourceToggle);
            SetUpActorWindows();
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
            _gResourceToggle.Show(value);
            _gConsumableToggle.Show(value);
            _gToolToggle.Show(value);
            _gSpecialToggle.Show(value);
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
            
            for (int i = _iIndex; i < _iIndex + 15; i++)
            {
                if(actors.Count <= i)
                {
                    break;
                }

                NPCDisplayWindow npc = new NPCDisplayWindow(actors[i], CreateInfoWindow);

                int listIndex = i - _iIndex;
                if (i == _iIndex)
                {
                    npc.Position(this);
                    npc.ScaledMoveBy(9, 18);
                }
                else if (i % 5 == 0)
                {
                    npc.AnchorAndAlignToObject(_liActorDisplay[listIndex - 5], SideEnum.Bottom, SideEnum.Left, GameManager.ScaleIt(3));
                }
                else
                {
                    npc.AnchorAndAlignToObject(_liActorDisplay[listIndex - 1], SideEnum.Right, SideEnum.Bottom, GameManager.ScaleIt(2));
                }

                _liActorDisplay.Add(npc);
                AddControl(npc);
            }

            
            _gTotal = new GUIText(string.Format("{0}/{1}", found, actors.Count));
            _gTotal.AlignToObject(_gWindow, SideEnum.Center);
            _gTotal.AlignToObject(_btnLeft, SideEnum.CenterY);

            _btnLeft.Enable(_iIndex >= MAX_ACTOR_DISPLAY);
            _btnRight.Enable(_iIndex + MAX_ACTOR_DISPLAY < actors.Count);
        }
        private void SetUpItemWindows()
        {
            ClearWindows();
            ShowItemToggles(true);

            int found = 0;
            _gLabel = new GUIText("Items");
            _gLabel.AnchorToInnerSide(_gWindow, SideEnum.Top);

            List<int> itemIDs = new List<int>(TownManager.DICodexItems.Keys.ToList().Where(x => DataManager.GetEnumByIDKey<ItemEnum>(x, "Type", DataType.Item) == _eItemDisplay));
            found = itemIDs.Count(x => TownManager.DICodexItems[x]);

            for (int i = _iIndex; i < _iIndex + MAX_ITEM_DISPLAY; i++)
            {
                if (itemIDs.Count <= i)
                {
                    break;
                }

                int museumIndex = itemIDs[i];
                ItemDisplayWindow npc = new ItemDisplayWindow(museumIndex, TownManager.DICodexItems[museumIndex]);

                int listIndex = i - _iIndex;
                if (i == _iIndex)
                {
                    npc.Position(this);
                    npc.ScaledMoveBy(14, 40);
                }
                else if (i % 7 == 0)
                {
                    npc.AnchorAndAlignToObject(_liItemDisplay[listIndex - 7], SideEnum.Bottom, SideEnum.Left, GameManager.ScaleIt(3));
                }
                else
                {
                    npc.AnchorAndAlignToObject(_liItemDisplay[listIndex - 1], SideEnum.Right, SideEnum.Bottom, GameManager.ScaleIt(3));
                }

                _liItemDisplay.Add(npc);
                AddControl(npc);
            }


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
            if (IsItemPage()) { SetUpItemWindows(); }
            else { SetUpActorWindows(); }
        }

        public void BtnRight()
        {
            _iIndex += DisplayValue;
            _btnLeft.Enable(true);
            if (IsItemPage()) { SetUpItemWindows(); }
            else { SetUpActorWindows(); }
        }

        private void SetupNewPage(CodexPageEnum e)
        {
            _iIndex = 0;
            _eCurrentPage = e;
            RemoveControl(_gInfoWindow);

            _btnVillagers.SetSelected(_eCurrentPage == CodexPageEnum.Villagers);
            _btnMerchants.SetSelected(_eCurrentPage == CodexPageEnum.Merchants);
            _btnTravelers.SetSelected(_eCurrentPage == CodexPageEnum.Travelers);
            _btnMobs.SetSelected(_eCurrentPage == CodexPageEnum.Mobs);
            _btnItems.SetSelected(_eCurrentPage == CodexPageEnum.Items);

            if (IsItemPage()) { SetUpItemWindows(); }
            else { SetUpActorWindows(); }
        }

        public void ShowVillagers()
        {
            if (_eCurrentPage != CodexPageEnum.Villagers)
            {
                SetupNewPage(CodexPageEnum.Villagers);
            }
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

        private void ToggleHelper(GUIToggle toggle, GUIToggle checkAgainst)
        {
            if (checkAgainst == toggle)
            {
                if (!checkAgainst.Selected)
                {
                    checkAgainst.Select(true);
                }
            }
            else
            {
                checkAgainst.Select(false);
            }
        }
        private void SyncToggles(GUIToggle toggle)
        {
            _iIndex = 0;
            ToggleHelper(toggle, _gResourceToggle);
            ToggleHelper(toggle, _gConsumableToggle);
            ToggleHelper(toggle, _gToolToggle);
            ToggleHelper(toggle, _gSpecialToggle);
        }
        public void ItemResourceToggle()
        {
            SyncToggles(_gResourceToggle);
            _eItemDisplay = ItemEnum.Resource;
            SetUpItemWindows();
        }
        public void ItemPotionToggle()
        {
            SyncToggles(_gConsumableToggle);
            _eItemDisplay = ItemEnum.Consumable;
            SetUpItemWindows();
        }
        public void ItemToolToggle()
        {
            SyncToggles(_gToolToggle);
            _eItemDisplay = ItemEnum.Tool;
            SetUpItemWindows();
        }
        public void ItemSpecialToggle()
        {
            SyncToggles(_gSpecialToggle);
            _eItemDisplay = ItemEnum.Special;
            SetUpItemWindows();
        }

        public void CreateInfoWindow(NPCDisplayWindow window)
        {
            RemoveControl(_gInfoWindow);
            _gInfoWindow = new GUIWindow(GUIWindow.WoodenPanel, 10, 10);

            string strText = window.Found ? DataManager.GetTextData(window.ID, "Name", DataType.Actor) : "???";
            GUIText text = new GUIText(strText);
            text.AnchorToInnerSide(_gInfoWindow, SideEnum.TopLeft);

            if (window.Found)
            {
                string strDescText = DataManager.GetTextData(window.ID, "Description", DataType.Actor);
                if (!string.IsNullOrEmpty(strDescText))
                {
                    GUIText descText = new GUIText(strDescText);
                    descText.AnchorAndAlignToObject(text, SideEnum.Bottom, SideEnum.Left, GameManager.ScaleIt(2));
                }
            }

            _gInfoWindow.Resize(false);
            _gInfoWindow.AnchorAndAlignToObject(window, SideEnum.Bottom, SideEnum.CenterX, GameManager.ScaleIt(-4));
            text.AlignToObject(_gInfoWindow, SideEnum.CenterX);
            AddControl(_gInfoWindow);
        }

        public class GUITabButton : GUIObject
        {
            private bool _bSelected;
            private GUIImage _gSelected;
            private GUIImage _gUnselected;

            protected EmptyDelegate _delAction;

            public GUITabButton(Rectangle rSelected, Rectangle unSelected, string texture, EmptyDelegate del)
            {
                _delAction = del;

                _gUnselected = new GUIImage(unSelected, texture);

                _gSelected = new GUIImage(rSelected, texture);
                _gSelected.Show(false);
                _gSelected.MoveBy(0, _gUnselected.Height - _gSelected.Height);

                AddControl(_gSelected);
                AddControl(_gUnselected);

                Width = _gUnselected.Width;
                Height = _gUnselected.Height;
            }

            public void SetSelected(bool value)
            {
                (_bSelected ? _gSelected : _gUnselected).Show(false);
                _bSelected = value;
                (_bSelected ? _gSelected : _gUnselected).Show(true);
            }

            public override bool ProcessLeftButtonClick(Point mouse)
            {
                bool rv = false;

                if((_gSelected.Contains(mouse) && _gSelected.Show()) ||
                    (_gUnselected.Contains(mouse) && _gUnselected.Show()))
                {
                    rv = true;
                    _delAction();
                }

                return rv;
            }
        }
    }
}
