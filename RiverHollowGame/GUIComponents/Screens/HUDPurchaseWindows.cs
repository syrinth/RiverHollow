using System.Collections.Generic;
using Microsoft.Xna.Framework;
using MonoGame.Extended.BitmapFonts;
using RiverHollow.Characters;
using RiverHollow.Game_Managers;
using RiverHollow.GUIComponents.GUIObjects;
using RiverHollow.GUIComponents.GUIObjects.GUIWindows;
using RiverHollow.WorldObjects;
using RiverHollow.Misc;
using static RiverHollow.Game_Managers.GameManager;
using RiverHollow.Items;

namespace RiverHollow.GUIComponents.Screens
{
    public class HUDShopWindow : GUIMainObject
    {
        List<Merchandise> _liMerch;
        GUIMoneyDisplay _gMoney;
        GUIList _gList;

        public HUDShopWindow(List<Merchandise> merch)
        {
            _liMerch = merch;
            _winMain = SetMainWindow();

            ShowDisplay();

            _gMoney = new GUIMoneyDisplay();
            _gMoney.AnchorAndAlignToObject(_winMain, GUIObject.SideEnum.Top, GUIObject.SideEnum.Left);

            AddControl(_gMoney);
        }

        private void ShowDisplay()
        {
            _winMain.Controls.Clear();
            RemoveControl(_gList);

            List<GUIObject> items = new List<GUIObject>();

            int i = 0;
            foreach (Merchandise m in _liMerch)
            {
                PurchaseBox newBox = null;
                if (m.MerchType == Merchandise.MerchTypeEnum.Item)
                {
                    if (!PlayerManager.AlreadyBoughtUniqueItem(m.MerchID))
                    {
                        Item it = DataManager.GetItem(m.MerchID);
                        it.ApplyUniqueData(m.UniqueData);

                        newBox = new PurchaseBox(it, m.MoneyCost, _winMain.MidWidth() - GUIList.BTNSIZE, ShowDisplay);
                    }
                    else { continue; }
                }
                else if (m.MerchType == Merchandise.MerchTypeEnum.WorldObject)
                {
                    newBox = new PurchaseBox(DataManager.GetWorldObjectByID(m.MerchID), m.MoneyCost, _winMain.MidWidth() - GUIList.BTNSIZE, ShowDisplay);
                }
                else if (m.MerchType == Merchandise.MerchTypeEnum.Actor)
                {
                    //newBox = new PurchaseBox(DataManager.GetNPCByIndex(m.MerchID), m.MoneyCost, _winMain.MidWidth() - GUIList.BTNSIZE, ShowDisplay);
                }

                items.Add(newBox);

                if (i == 0) { items[i].AnchorToInnerSide(_winMain, GUIObject.SideEnum.TopLeft); }
                else { items[i].AnchorAndAlignToObject(items[i - 1], GUIObject.SideEnum.Bottom, GUIObject.SideEnum.Left); }
                i++;
            }

            _gList = new GUIList(items, 10, ScaleIt(2), _winMain.MidHeight());
            _gList.CenterOnObject(_winMain);

            AddControl(_gList);
        }
    }

    internal class PurchaseBox : GUIWindow
    {
        private BitmapFont _font;
        GUIImage _giItem;
        GUIText _gTextName;
        GUIMoneyDisplay _gMoney;

        public Item ShopItem { get; }
        public WorldObject WorldObject { get; }
        public WorldActor Actor { get; }

        public int Cost { get; }
        public bool CanBuy { get; private set; }

        ClickDelegate _action;

        public PurchaseBox(Item i, int cost, int mainWidth, ClickDelegate action) : base(GUIWindow.GreyWin, mainWidth, ScaledTileSize + ScaleIt(4))
        {
            _action = action;
            _font = DataManager.GetBitMapFont(DataManager.FONT_MAIN);
            Cost = cost;
            ShopItem = i;
            _giItem = new GUIImage(ShopItem.SourceRectangle, ScaledTileSize, ScaledTileSize, ShopItem.Texture);
            _giItem.SetColor(i.ItemColor);
            _gTextName = new GUIText(ShopItem.Name);

            _giItem.AnchorToInnerSide(this, SideEnum.Left);
            _gTextName.AnchorAndAlignToObject(_giItem, SideEnum.Right, SideEnum.CenterY);

            _gMoney = new GUIMoneyDisplay(Cost);
            _gMoney.AnchorToInnerSide(this, SideEnum.Right, ScaleIt(2));
        }

        public PurchaseBox(WorldObject obj, int cost, int mainWidth, ClickDelegate action) : base(GUIWindow.GreyWin, mainWidth, ScaledTileSize + ScaleIt(4))
        {
            _action = action;
            WorldObject = obj;
            _font = DataManager.GetBitMapFont(DataManager.FONT_MAIN);
            Cost = cost;
            _gTextName = new GUIText(obj.Name);

            _gTextName.AnchorToInnerSide(this, SideEnum.Left);

            _gMoney = new GUIMoneyDisplay(Cost);
            _gMoney.AnchorToInnerSide(this, SideEnum.Right, ScaleIt(2));
        }

        public PurchaseBox(WorldActor actor, int cost, int mainWidth, ClickDelegate action) : base(GUIWindow.GreyWin, mainWidth, ScaledTileSize + ScaleIt(4))
        {
            _action = action;
            Actor = actor;
            _font = DataManager.GetBitMapFont(DataManager.FONT_MAIN);
            Cost = cost;
            _gTextName = new GUIText(actor.Name);

            _gTextName.AnchorToInnerSide(this, SideEnum.Left);

            _gMoney = new GUIMoneyDisplay(Cost);
            _gMoney.AnchorToInnerSide(this, SideEnum.Right, ScaleIt(2));
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
                if (ShopItem != null) { PurchaseItem(ShopItem.ItemID); }
                if (WorldObject != null) { PlayerManager.AddToStorage(WorldObject.ID); }
                if (Actor != null)
                {
                    if (Actor.IsActorType(WorldActorTypeEnum.Mount))
                    {
                        Mount act = (Mount)Actor;
                        PlayerManager.AddMount(act);
                        act.SpawnInHome();
                    }
                    else if (Actor.IsActorType(WorldActorTypeEnum.Pet))
                    {
                        Pet act = (Pet)Actor;
                        PlayerManager.AddPet(act);
                        act.SpawnNearPlayer();
                        if (PlayerManager.PlayerActor.ActivePet == null)
                        {
                            PlayerManager.PlayerActor.SetPet(act);
                        }
                    }
                }

                rv = true;
            }

            return rv;
        }

        public override bool ProcessHover(Point mouse)
        {
            Enable(Contains(mouse) && CanBuyMerch());

            //Return false here to not skip any other ProcessHovers that are coming
            return false;
        }

        private void PurchaseItem(int itemID)
        {
            Item purchase = DataManager.GetItem(itemID);
            if (purchase.CompareType(ItemEnum.Blueprint))
            {
                PlayerManager.AddToUniqueBoughtItems(itemID);
                _action();
            }
            InventoryManager.AddToInventory(purchase);
        }

        private bool CanBuyMerch()
        {
            bool rv = true;
            if (ShopItem != null && !InventoryManager.HasSpaceInInventory(ShopItem.ItemID, ShopItem.Number)){
                rv = false;
            }
            else if (Actor != null && Actor.IsActorType(WorldActorTypeEnum.Mount) && !((Mount)Actor).StableBuilt())
            {
                rv = false;
            }

            return rv;
        }
    }
}