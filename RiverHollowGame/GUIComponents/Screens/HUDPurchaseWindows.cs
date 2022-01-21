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

                        newBox = new PurchaseBox(it, m.MoneyCost, _winMain.MidWidth() - GUIList.BTNSIZE);
                    }
                    else { continue; }
                }
                else if (m.MerchType == Merchandise.MerchTypeEnum.WorldObject)
                {
                    newBox = new PurchaseBox(DataManager.GetWorldObjectByID(m.MerchID), m.MoneyCost, _winMain.MidWidth() - GUIList.BTNSIZE);
                }
                else if (m.MerchType == Merchandise.MerchTypeEnum.Actor)
                {
                    newBox = new PurchaseBox(DataManager.GetNPCByIndex(m.MerchID), m.MoneyCost, _winMain.MidWidth() - GUIList.BTNSIZE);
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

        public override bool ProcessLeftButtonClick(Point mouse)
        {
            bool rv = false;
            for (int i = 0; i < _gList.Objects.Count; i++)
            {
                PurchaseBox pBox = (PurchaseBox)_gList.Objects[i];
                if (pBox.CanBuy && PlayerManager.Money >= pBox.Cost)
                {
                    PlayerManager.TakeMoney(pBox.Cost);
                    if (pBox.ShopItem != null) { PurchaseItem(pBox.ShopItem.ItemID); }
                    if (pBox.WorldObject != null) { PlayerManager.AddToStorage(pBox.WorldObject.ID); }
                    if (pBox.Actor != null)
                    {
                        if (pBox.Actor.IsActorType(ActorEnum.Mount))
                        {
                            Mount act = (Mount)pBox.Actor;
                            PlayerManager.AddMount(act);
                            act.SpawnInHome();
                        }
                        else if (pBox.Actor.IsActorType(ActorEnum.Pet))
                        {
                            Pet act = (Pet)pBox.Actor;
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
            }

            return rv;
        }

        private void PurchaseItem(int itemID)
        {
            Item purchase = DataManager.GetItem(itemID);
            if (purchase.CompareType(ItemEnum.Blueprint))
            {
                PlayerManager.AddToUniqueBoughtItems(itemID);
                _winMain.Controls.Clear();
                RemoveControl(_gList);
                ShowDisplay();
            }
            InventoryManager.AddToInventory(purchase);
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

        public PurchaseBox(Item i, int cost, int mainWidth) : base(GUIWindow.GreyWin, mainWidth, ScaledTileSize + ScaleIt(4))
        {
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

        public PurchaseBox(WorldObject obj, int cost, int mainWidth) : base(GUIWindow.GreyWin, mainWidth, ScaledTileSize + ScaleIt(4))
        {
            WorldObject = obj;
            _font = DataManager.GetBitMapFont(DataManager.FONT_MAIN);
            Cost = cost;
            _gTextName = new GUIText(obj.Name);

            _gTextName.AnchorToInnerSide(this, SideEnum.Left);

            _gMoney = new GUIMoneyDisplay(Cost);
            _gMoney.AnchorToInnerSide(this, SideEnum.Right, ScaleIt(2));
        }

        public PurchaseBox(WorldActor actor, int cost, int mainWidth) : base(GUIWindow.GreyWin, mainWidth, ScaledTileSize + ScaleIt(4))
        {
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

        public override bool ProcessHover(Point mouse)
        {
            Enable(Contains(mouse) && CanBuyMerch());

            //Return false here to not skip any other ProcessHovers that are coming
            return false;
        }
       

        private bool CanBuyMerch()
        {
            bool rv = true;
            if (ShopItem != null && !InventoryManager.HasSpaceInInventory(ShopItem.ItemID, ShopItem.Number)){
                rv = false;
            }
            else if (Actor != null && Actor.IsActorType(ActorEnum.Mount) && !((Mount)Actor).StableBuilt())
            {
                rv = false;
            }

            return rv;
        }
    }
}