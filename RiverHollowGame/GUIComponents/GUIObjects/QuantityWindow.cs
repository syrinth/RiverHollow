using Microsoft.Xna.Framework;
using RiverHollow.Game_Managers;
using RiverHollow.GUIComponents.GUIObjects.GUIWindows;

namespace RiverHollow.GUIComponents.GUIObjects
{
    class QuantityWindow : GUIWindow
    {
        int MAX_VALUE => (GameManager.CurrentMerchandise.MerchItem != null ? PlayerManager.Money / GameManager.CurrentMerchandise.Price : 0);
        readonly GUIButton _btnUp;
        readonly GUIButton _btnDown;

        readonly GUIText _gSellValue;
        readonly GUIButton _btnBuy;

        public QuantityWindow() : base(GUIUtils.WINDOW_DARKBLUE, GameManager.ScaleIt(114), GameManager.ScaleIt(73))
        {
            GUIItemBox box = new GUIItemBox(GameManager.CurrentMerchandise.MerchItem);
            box.Position(this);
            box.ScaledMoveBy(47, 20);
            box.AlignToObject(this, SideEnum.CenterX);
            AddControl(box);

            GUIText text = new GUIText(GameManager.CurrentMerchandise.MerchItem.Name());
            text.Position(this);
            text.AlignToObject(box, SideEnum.CenterX);
            text.ScaledMoveBy(0, 7);
            AddControl(text);

            _btnDown = new GUIButton(GUIUtils.BTN_DECREASE, Decrement);
            _btnDown.AnchorAndAlignWithSpacing(box, SideEnum.Left, SideEnum.CenterY, 2);
            AddControl(_btnDown);

            _btnUp = new GUIButton(GUIUtils.BTN_INCREASE, Increment);
            _btnUp.AnchorAndAlignWithSpacing(box, SideEnum.Right, SideEnum.CenterY, 2);
            AddControl(_btnUp);

            GUIImage img = new GUIImage(GUIUtils.HUD_SCROLL_S);
            img.AnchorAndAlignWithSpacing(box, SideEnum.Bottom, SideEnum.CenterX, 2);
            AddControl(img);

            _btnBuy = new GUIButton(GUIUtils.BTN_BUY, ProceedToPurchase);
            _btnBuy.Position(this);
            _btnBuy.ScaledMoveBy(89, 48);
            AddControl(_btnBuy);

            _gSellValue = new GUIText(GameManager.CurrentMerchandise.Price);
            _gSellValue.AnchorAndAlignWithSpacing(_btnBuy, SideEnum.Left, SideEnum.CenterY, 2);
            AddControl(_gSellValue);

            CenterOnScreen();
        }

        public override bool ProcessRightButtonClick(Point mouse)
        {
            return base.ProcessRightButtonClick(mouse);
        }

        public void Decrement()
        {
            var merchItem = GameManager.CurrentMerchandise.MerchItem;
            if (merchItem.Number > 1) { merchItem.Remove(1, false); }
            else { merchItem.SetNumber(MAX_VALUE); }

            RefreshPrice();
        }
        public void Increment()
        {
            var merchItem = GameManager.CurrentMerchandise.MerchItem;
            if (merchItem.Number < MAX_VALUE) { merchItem.Add(1, false); }
            else { merchItem.SetNumber(1); }

            RefreshPrice();
        }

        private void RefreshPrice()
        {
            Color c = Color.Black;
            int offer = GameManager.CurrentMerchandise.TotalPrice;
            _gSellValue.SetText(offer);
            _gSellValue.SetColor(c);
            _gSellValue.AnchorAndAlignWithSpacing(_btnBuy, SideEnum.Left, SideEnum.CenterY, 2);

            _btnBuy.Enable(offer > 0);
        }

        public void ProceedToPurchase()
        {
            var merch = GameManager.CurrentMerchandise;
            if (InventoryManager.HasSpaceInInventory(merch.MerchID, merch.MerchItem.Number))
            {
                MapManager.CurrentMap.TheShop.Purchase(merch);
                GUIManager.CloseMainObject();
            }
            else
            {
                GUIManager.OpenTextWindow("BuyMerch_NoSpace");
            }
        }
    }
}
