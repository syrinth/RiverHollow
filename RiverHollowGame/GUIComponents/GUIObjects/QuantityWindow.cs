using Microsoft.Xna.Framework;
using RiverHollow.Characters;
using RiverHollow.Game_Managers;
using RiverHollow.GUIComponents.GUIObjects.GUIWindows;
using RiverHollow.Items;
using RiverHollow.Misc;
using RiverHollow.Utilities;
using static RiverHollow.Utilities.Enums;

namespace RiverHollow.GUIComponents.GUIObjects
{
    class QuantityWindow : GUIMainObject
    {
        int MAX_VALUE => (GameManager.CurrentItem != null ? PlayerManager.Money / GameManager.CurrentItem.BuyPrice : 0);
        GUIButton _btnUp;
        GUIButton _btnDown;

        GUIText _gSellValue;
        GUIButton _btnBuy;

        public QuantityWindow()
        {
            _winMain = SetMainWindow(GUIUtils.WINDOW_DARKBLUE, GameManager.ScaleIt(114), GameManager.ScaleIt(73));

            GUIItemBox box = new GUIItemBox(GameManager.CurrentItem);
            box.Position(_winMain);
            box.ScaledMoveBy(47, 20);
            box.AlignToObject(_winMain, SideEnum.CenterX);
            AddControl(box);

            GUIText text = new GUIText(GameManager.CurrentItem.Name());
            text.Position(_winMain);
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
            _btnBuy.Position(_winMain);
            _btnBuy.ScaledMoveBy(89, 48);
            AddControl(_btnBuy);

            _gSellValue = new GUIText(GameManager.CurrentItem.TotalBuyValue);
            _gSellValue.AnchorAndAlignWithSpacing(_btnBuy, SideEnum.Left, SideEnum.CenterY, 2);
            AddControl(_gSellValue);

            CenterOnScreen();
        }

        public override bool ProcessRightButtonClick(Point mouse)
        {
            GameManager.SetSelectedItem(null);
            return base.ProcessRightButtonClick(mouse);
        }

        public void Decrement()
        {
            if (GameManager.CurrentItem.Number > 1) { GameManager.CurrentItem.Remove(1, false); }
            else { GameManager.CurrentItem.SetNumber(MAX_VALUE); }

            RefreshPrice();
        }
        public void Increment()
        {
            if (GameManager.CurrentItem.Number < MAX_VALUE) { GameManager.CurrentItem.Add(1, false); }
            else { GameManager.CurrentItem.SetNumber(1); }

            RefreshPrice();
        }

        private void RefreshPrice()
        {
            Color c = Color.Black;
            int offer = GameManager.CurrentItem.TotalBuyValue;
            _gSellValue.SetText(offer);
            _gSellValue.SetColor(c);
            _gSellValue.AnchorAndAlignWithSpacing(_btnBuy, SideEnum.Left, SideEnum.CenterY, 2);

            _btnBuy.Enable(offer > 0);
        }

        public void ProceedToPurchase()
        {
            if (InventoryManager.HasSpaceInInventory(GameManager.CurrentItem.ID, GameManager.CurrentItem.Number))
            {
                MapManager.CurrentMap.TheShop.Purchase(GameManager.CurrentItem);
                GUIManager.CloseMainObject();
            }
            else
            {
                GUIManager.OpenTextWindow("BuyMerch_NoSpace");
            }
        }
    }
}
