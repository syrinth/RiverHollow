using Microsoft.Xna.Framework;
using RiverHollow.Buildings;
using RiverHollow.Characters;
using RiverHollow.Game_Managers;
using RiverHollow.GUIComponents.GUIObjects;
using RiverHollow.GUIComponents.GUIObjects.GUIWindows;
using RiverHollow.GUIComponents.MainObjects;
using RiverHollow.Items;
using RiverHollow.Misc;
using RiverHollow.Utilities;
using System.Collections.Generic;
using System.Linq;
using static RiverHollow.Utilities.Enums;

namespace RiverHollow.GUIComponents.Screens
{
    public class HUDShopSlateWindow : GUIMainObject
    {
        private enum BuildingTypeEnum { None, Craft, Produce };
        readonly Point startPoint = new Point(32, 53);
        private readonly bool _bSellsItems;
        private readonly BuildingTypeEnum _eItemType = BuildingTypeEnum.None;

        private readonly List<GUIObject> _liItems;
        private readonly List<GUIToggle> _gSlateToggles;

        private readonly List<GUIImage> _liShelves;
        private GUIImage TopShelf => _liShelves[0];
        private GUIImage BottomShelf => _liShelves[0];
        private readonly GUIInventoryWindow _gPlayerInventory;
        private GUIShopInventory _gShopInventory;

        public HUDShopSlateWindow()
        {
            Position(Util.MultiplyPoint(startPoint, GameManager.CurrentScale));
            GameManager.SetDescriptionDrawPoint(new Point(240, 48));

            _liItems = new List<GUIObject>();

            //HEADER
            _liShelves = new List<GUIImage>
            {
                new GUIImage(GUIUtils.HUD_SHOP_TOP)
            };
            TopShelf.ScaledMoveBy(48, 32);
            AddControl(TopShelf);

            //MID
            var temp = TopShelf;
            for (int i = 0; i < 3; i++)
            {
                var newShelf = new GUIImage(GUIUtils.HUD_SHOP_MID);
                newShelf.AnchorAndAlign(temp, SideEnum.Bottom, SideEnum.Left, GUIUtils.ParentRuleEnum.ForceToParent);
                temp = newShelf;

                _liShelves.Add(newShelf);
            }

            //BOTTOM
            var lastShelf = new GUIImage(GUIUtils.HUD_SHOP_BOT);
            _liShelves.Add(lastShelf);
            lastShelf.AnchorAndAlign(temp, SideEnum.Bottom, SideEnum.Left, GUIUtils.ParentRuleEnum.ForceToParent);

            _gPlayerInventory = new GUIInventoryWindow(true);
            _gPlayerInventory.ScaledMoveBy(236, 144);
            AddControl(_gPlayerInventory);

            _gSlateToggles = new List<GUIToggle>();

            var shop = MapManager.CurrentMap.TheShop;
            _bSellsItems = shop != null && shop.GetUnlockedMerchandise().Count > 0;
            if (_bSellsItems)
            {
                AddSlateToggle(DisplayShopInfo, GUIUtils.ICON_COIN);
            }

            //We go from Produce to Craft because Production has less requirements than Craft.

            if (MapManager.CurrentMap.Building() is Building b)
            {
                _eItemType = BuildingTypeEnum.Produce;
                if (!b.Producer)
                {
                    _eItemType = BuildingTypeEnum.Craft;
                }
            }

            if(TownManager.GetCurrentBuilding() != null)
            {
                AddSlateToggle(DisplayMerchandiseInfo, GUIUtils.ICON_BAG);
                AddSlateToggle(DisplaySupplyInfo, GUIUtils.ICON_CHEST);

                //_recipeBook = new RecipeBook(MapManager.CurrentMap.Building());
                //_recipeBook.AnchorAndAlignWithSpacing(_gPlayerInventory, SideEnum.Top, SideEnum.CenterX, 22);
                //_recipeBook.Show(false);
                //AddControl(_recipeBook);
            }

            ArrangeToggles();

            if (_bSellsItems) { DisplayShopInfo(); }
            else { DisplayMerchandiseInfo(); }

            _gSlateToggles[0].AssignToggleGroup(false, _gSlateToggles.Where(x => x != _gSlateToggles[0]).ToArray());

            Width = TopShelf.Width;
            Height = BottomShelf.Bottom - TopShelf.Top;
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
            list.AddRange(_liShelves);

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

                if (i == 0) { newBox.PositionAndMove(TopShelf, startPoint); }
                else if (i % 5 == 0) { newBox.AnchorAndAlignWithSpacing(_liItems[i - 5], SideEnum.Bottom, SideEnum.Left, 16); }
                else { newBox.AnchorAndAlignWithSpacing(_liItems[i - 1], SideEnum.Right, SideEnum.Top, 8); }

                newBox.RemoveSelfFromControl();

                _liItems.Add(newBox);

                foreach(var x in _liShelves)
                {
                    if (x.Contains(newBox))
                    {
                        x.AddControl(newBox);
                    }
                }
            }
        }

        private void DisplayMerchandiseInfo()
        {
            ClearShelves();

            List<int> validIDs = new List<int>();
            if (MapManager.CurrentMap.Building() is Building b)
            {
                if (b.Producer)
                {
                    var dictionary = b.GetProductionDictionary();
                    foreach (var list in dictionary.Values)
                    {
                        validIDs.AddRange(list);
                    }
                }
            }

            InventoryManager.ValidIDs = validIDs;
            InventoryManager.ClearExtraInventory();
            InventoryManager.InitExtraInventory(TownManager.GetCurrentBuilding().Merchandise);

            _gShopInventory = new GUIShopInventory();
            AddControl(_gShopInventory);
            _gShopInventory.PositionAndMove(TopShelf, startPoint);
        }

        private void DisplaySupplyInfo()
        {
            ClearShelves();

            var building = TownManager.GetCurrentBuilding();

            InventoryManager.InitExtraInventory(building.Stash);

            _gShopInventory = new GUIShopInventory();
            AddControl(_gShopInventory);
            _gShopInventory.PositionAndMove(TopShelf, startPoint);
        }

        private void AddSlateToggle(EmptyDelegate del, Rectangle icon)
        {
            int index = _gSlateToggles.Count;
            var toggle = new GUIToggle(icon, GUIToggle.ToggleTypeEnum.Fade, DataManager.HUD_COMPONENTS, del);
            _gSlateToggles.Add(toggle);
        }

        private void ArrangeToggles()
        {
            for (int i = 0; i < _gSlateToggles.Count; i++)
            {
                if (i == 0)
                {
                    if (_gSlateToggles.Count == 1)
                    {
                        _gSlateToggles[i].PositionAndMove(TopShelf, 80, 20);
                    }
                    else if (_gSlateToggles.Count == 2)
                    {
                        _gSlateToggles[i].PositionAndMove(TopShelf, 72, 20);
                    }
                    else if (_gSlateToggles.Count == 3)
                    {
                        _gSlateToggles[i].PositionAndMove(TopShelf, 64, 20);
                    }
                }
                else
                {
                    _gSlateToggles[i].AnchorAndAlign(_gSlateToggles[i - 1], SideEnum.Right, SideEnum.Bottom);
                }
            }
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