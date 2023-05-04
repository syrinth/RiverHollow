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
using static RiverHollow.Utilities.Enums;
using RiverHollow.Utilities;
using Microsoft.Xna.Framework.Input;

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
            _gMoney.AnchorAndAlign(_winMain, GUIObject.SideEnum.Top, GUIObject.SideEnum.Left);

            AddControl(_gMoney);
        }

        private void ShowDisplay()
        {
            _winMain.Controls.Clear();
            RemoveControl(_gList);

            List<GUIObject> items = new List<GUIObject>();

            foreach (Merchandise m in _liMerch)
            {
                PurchaseBox newBox = null;
                if (!PlayerManager.AlreadyBoughtUniqueItem(m.MerchID))
                {
                    Item it = DataManager.GetItem(m.MerchType == Merchandise.MerchTypeEnum.WorldObject ? m.MerchID + Constants.BUILDABLE_ID_OFFSET : m.MerchID);
                    it.ApplyUniqueData(m.UniqueData);

                    newBox = new PurchaseBox(it, m.MoneyCost, _winMain.InnerWidth() - GameManager.ScaledTileSize, ShowDisplay);
                }
                else { continue; }

                items.Add(newBox);
            }

            _gList = new GUIList(items, 10, 1, _winMain, _winMain.InnerHeight());
        }

        public override void CloseMainWindow()
        {
            GameManager.CurrentNPC?.StopTalking();
        }
    }

    internal class PurchaseBox : GUIWindow
    {
        private BitmapFont _font;
        GUIItem _giItem;
        GUIText _gTextName;
        GUIMoneyDisplay _gMoney;

        public Item ShopItem { get; }
        public WorldObject WorldObject { get; }
        public Actor Actor { get; }

        public int Cost { get; }
        public bool CanBuy { get; private set; }

        EmptyDelegate _action;

        public PurchaseBox(Item i, int cost, int mainWidth, EmptyDelegate action) : base(GUIUtils.GreyWin, mainWidth, ScaledTileSize + ScaleIt(4))
        {
            _action = action;
            _font = DataManager.GetBitMapFont(DataManager.FONT_NEW);
            Cost = cost;
            ShopItem = i;
            _giItem = new GUIItem(i);
            _giItem.SetColor(i.ItemColor);
            _gTextName = new GUIText(ShopItem.Name());

            _giItem.AnchorToInnerSide(this, SideEnum.Left);
            _gTextName.AnchorAndAlign(_giItem, SideEnum.Right, SideEnum.CenterY);

            _gMoney = new GUIMoneyDisplay(Cost);
            _gMoney.AnchorToInnerSide(this, SideEnum.BottomRight);
            _gMoney.AlignToObject(_giItem, SideEnum.Top);

            Enable(false);
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
                if (ShopItem != null) { PurchaseItem(ShopItem.ID); }

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

        private void PurchaseItem(int ID)
        {
            Item purchaseItem = DataManager.GetItem(ID);
            if (purchaseItem.IsUnique())
            {
                PlayerManager.AddUniqueItemToList(ID);
                _action();
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