using System.Collections.Generic;
using Microsoft.Xna.Framework;
using RiverHollow.Characters;
using RiverHollow.Game_Managers;
using RiverHollow.GUIComponents.GUIObjects;
using RiverHollow.GUIComponents.GUIObjects.GUIWindows;
using RiverHollow.WorldObjects;
using RiverHollow.Misc;
using RiverHollow.Items;
using RiverHollow.Utilities;

using static RiverHollow.Utilities.Enums;
using System.Linq;

namespace RiverHollow.GUIComponents.Screens
{
    public class HUDShopSlateWindow : GUIMainObject
    {
        readonly Point startPoint = new Point(32, 53);

        private readonly List<GUIObject> _liItems;
        private readonly List<GUIToggle> _gSlateToggles;
        private readonly GUIImage _gTop;
        private readonly GUIImage[] _gMiddle;
        private readonly GUIImage _gBottom;
        private readonly GUIInventoryWindow _gPlayerInventory;
        private GUIShopInventory _gShopInventory;

        public HUDShopSlateWindow()
        {
            Position(Util.MultiplyPoint(startPoint, GameManager.CurrentScale));
            GameManager.SetDescriptionDrawPoint(new Point(240, 48));

            _liItems = new List<GUIObject>();

            //HEADER
            _gTop = new GUIImage(GUIUtils.HUD_SHOP_TOP);
            _gTop.ScaledMoveBy(48, 32);
            AddControl(_gTop);

            //MID
            _gMiddle = new GUIImage[3];
            var temp = _gTop;
            for (int i = 0; i < 3; i++)
            {
                _gMiddle[i] = new GUIImage(GUIUtils.HUD_SHOP_MID);
                _gMiddle[i].AnchorAndAlign(temp, SideEnum.Bottom, SideEnum.Left, GUIUtils.ParentRuleEnum.ForceToParent);
                temp = _gMiddle[i];
            }

            //BOTTOM
            _gBottom = new GUIImage(GUIUtils.HUD_SHOP_BOT);
            _gBottom.AnchorAndAlign(_gMiddle.Last(), SideEnum.Bottom, SideEnum.Left, GUIUtils.ParentRuleEnum.ForceToParent);

            _gPlayerInventory = new GUIInventoryWindow(true);
            _gPlayerInventory.ScaledMoveBy(236, 144);
            AddControl(_gPlayerInventory);

            _gSlateToggles = new List<GUIToggle>();

            if (IsShop())
            {   
                AddSlateToggle(DisplayShopInfo, GUIUtils.ICON_COIN);
            }

            if (MakesItems())
            {   
                AddSlateToggle(DisplayMerchandiseInfo, GUIUtils.ICON_BAG);
                AddSlateToggle(DisplaySupplyInfo, GUIUtils.ICON_CHEST);
            }

            if (IsShop()) { DisplayShopInfo(); }
            else { DisplayMerchandiseInfo(); }

            _gSlateToggles[0].AssignToggleGroup(false, _gSlateToggles.Where(x => x != _gSlateToggles[0]).ToArray());

            Width = _gTop.Width;
            Height = _gBottom.Bottom - _gTop.Top;
        }

        public override bool ProcessLeftButtonClick(Point mouse)
        {
            bool rv = false;

            if (_gShopInventory != null)
            {
                rv = _gShopInventory.ProcessLeftButtonClick(mouse);
            }

            if(!rv)
            {
                rv = base.ProcessLeftButtonClick(mouse);
            }

            return rv;
        }

        public override bool ProcessRightButtonClick(Point mouse)
        {
            bool rv = false;

            List<GUIObject> list = new List<GUIObject>() { _gPlayerInventory, _gShopInventory };
            list.AddRange(_gSlateToggles);
            list.AddRange(_gMiddle);
            list.Add(_gTop);
            list.Add(_gBottom);

            foreach (var obj in list)
            {
                if (obj != null)
                {
                    rv = obj.ProcessRightButtonClick(mouse);
                    if (rv) { break; }
                }
            }

            return rv;
        }

        private void ClearShelves()
        {
            _liItems.ForEach(x => x.RemoveSelfFromControl());
            _liItems.Clear();

            RemoveControl(_gShopInventory);
            _gShopInventory = null;

            InventoryManager.ClearExtraInventory();
            InventoryManager.CleanupInventoryDisplay();
        }

        private void DisplayShopInfo()
        {
            ClearShelves();

            var merch = MapManager.CurrentMap.TheShop.GetUnlockedMerchandise();

            ///Item Placement
            for (int i = 0; i < merch.Count; i++)
            {
                var m = merch[i];
                PurchaseBox newBox;
                if (!PlayerManager.AlreadyBoughtUniqueItem(m.MerchID))
                {
                    Item it = DataManager.GetItem(m.MerchType == ShopItem.MerchTypeEnum.WorldObject ? m.MerchID + Constants.BUILDABLE_ID_OFFSET : m.MerchID);
                    it.ApplyUniqueData(m.UniqueData);

                    newBox = new PurchaseBox(it, m.Price);
                }
                else { continue; }

                if (i == 0) { newBox.PositionAndMove(_gTop, startPoint); }
                else if (i % 5 == 0) { newBox.AnchorAndAlignWithSpacing(_liItems[i - 5], SideEnum.Bottom, SideEnum.Left, 16); }
                else { newBox.AnchorAndAlignWithSpacing(_liItems[i - 1], SideEnum.Right, SideEnum.Top, 8); }

                _liItems.Add(newBox);
                AddControl(newBox);
            }
        }

        private void DisplayMerchandiseInfo()
        {
            ClearShelves();

            List<int> validIDs = null;
            var machines = MapManager.CurrentMap.GetObjectsByType<Machine>();
            if (machines.Count > 0)
            {
                validIDs = new List<int>();
                foreach (var obj in machines)
                {
                    if (obj is Machine m)
                    {
                        validIDs.AddRange(m.GetCurrentCraftingList());
                    }
                }
            }

            InventoryManager.ValidIDs = validIDs;
            InventoryManager.ClearExtraInventory();
            InventoryManager.InitExtraInventory(TownManager.GetCurrentBuilding().Merchandise);

            _gShopInventory = new GUIShopInventory();
            AddControl(_gShopInventory);
            _gShopInventory.PositionAndMove(_gTop, startPoint);
        }

        private void DisplaySupplyInfo()
        {
            ClearShelves();

            var building = TownManager.GetCurrentBuilding();

            InventoryManager.InitExtraInventory(building.Stash);

            _gShopInventory = new GUIShopInventory();
            AddControl(_gShopInventory);
            _gShopInventory.PositionAndMove(_gTop, startPoint);
        }

        private void AddSlateToggle(EmptyDelegate del, Rectangle icon)
        {
            int index = _gSlateToggles.Count;
            var toggle = new GUIToggle(icon, GUIToggle.ToggleTypeEnum.Fade, DataManager.HUD_COMPONENTS, del);
            _gSlateToggles.Add(toggle);

            if (index == 0)
            {
                if (IsShop() && MakesItems())
                {
                    _gSlateToggles[index].PositionAndMove(_gTop, 64, 20);
                }
                else if (MakesItems())
                {
                    _gSlateToggles[index].PositionAndMove(_gTop, 72, 20);
                }
                else
                {
                    _gSlateToggles[index].PositionAndMove(_gTop, 80, 20);
                }
            }
            else
            {
                _gSlateToggles[index].AnchorAndAlign(_gSlateToggles[index - 1], SideEnum.Right, SideEnum.Bottom);
            }
        }

        private bool IsShop()
        {
            return MapManager.CurrentMap.TheShop.GetUnlockedMerchandise().Count > 0;
        }

        private bool MakesItems()
        {
            return TownManager.GetCurrentBuilding() != null;
        }

        public override void CloseMainWindow()
        {
            GameManager.SetDescriptionDrawPoint(Point.Zero);
            InventoryManager.ClearExtraInventory();
            InventoryManager.CleanupInventoryDisplay();
            base.CloseMainWindow();

            MapManager.CurrentMap.AssignMerchandise();
        }
    }

    internal class PurchaseBox : GUIObject
    {
        readonly GUIItem _giItem;        
        readonly GUIMoneyDisplay _gMoney;

        public Item ShopItem { get; }
        public Actor Actor { get; }

        public int Cost { get; }
        public bool CanBuy { get; private set; }


        public PurchaseBox(Item i, int cost)
        {
            Cost = cost;
            ShopItem = i;
            _giItem = new GUIItem(i, ItemBoxDraw.Never, !(i is WrappedObjectItem));
            AddControl(_giItem);

            _giItem.SetColor(i.ItemColor);

            _gMoney = new GUIMoneyDisplay(Cost, DirectionEnum.Right, true);
            _gMoney.AnchorAndAlign(_giItem, SideEnum.Bottom, SideEnum.Right);
            _gMoney.ScaledMoveBy(5, -5);
            AddControl(_gMoney);

            Width = _giItem.Width;
            Height = _giItem.Height;
        }

        public override void Update(GameTime gTime)
        {
            if (PlayerManager.Money < Cost || !CanBuyMerch())
            {
                _gMoney.SetColor(Color.Red);
                CanBuy = false;
            }
            else
            {
                _gMoney.SetColor(Color.White);
                CanBuy = true;
            }
        }

        public override bool ProcessLeftButtonClick(Point mouse)
        {
            bool rv = false;

            if (CanBuy && PlayerManager.Money >= Cost)
            {
                PlayerManager.TakeMoney(Cost);
                if (ShopItem != null) { PurchaseItem(ShopItem.ID, ShopItem.Number); }

                rv = true;
            }

            return rv;
        }

        protected override void BeginHover()
        {
            Enable(CanBuyMerch());
        }

        protected override void EndHover()
        {
            Enable(false);
        }

        private void PurchaseItem(int ID, int num)
        {
            Item purchaseItem = DataManager.GetItem(ID, num);
            if (purchaseItem.IsUnique())
            {
                PlayerManager.AddUniqueItemToList(ID);
                GUIManager.CloseMainObject();
                purchaseItem.StrikeAPose();
            }
            InventoryManager.AddToInventory(purchaseItem);
        }

        private bool CanBuyMerch()
        {
            bool rv = true;
            if (ShopItem != null && !InventoryManager.HasSpaceInInventory(ShopItem.ID, ShopItem.Number)){
                rv = false;
            }
            else if (Actor != null && Actor.IsActorType(ActorTypeEnum.Mount) && !((Mount)Actor).StableBuilt())
            {
                rv = false;
            }

            return rv;
        }
    }
}