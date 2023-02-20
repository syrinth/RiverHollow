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
            _winMain = SetMainWindow(GUIWindow.Window_2, GameManager.ScaleIt(114), GameManager.ScaleIt(73));

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

            _btnDown = new GUIButton(new Rectangle(137, 48, 7, 7), DataManager.DIALOGUE_TEXTURE, Decrement);
            _btnDown.AnchorAndAlignToObject(box, SideEnum.Left, SideEnum.CenterY, GameManager.ScaleIt(2));
            AddControl(_btnDown);

            _btnUp = new GUIButton(new Rectangle(128, 48, 7, 7), DataManager.DIALOGUE_TEXTURE, Increment);
            _btnUp.AnchorAndAlignToObject(box, SideEnum.Right, SideEnum.CenterY, GameManager.ScaleIt(2));
            AddControl(_btnUp);

            GUIImage img = new GUIImage(new Rectangle(2, 120, 100, 3), GameManager.ScaleIt(100), GameManager.ScaleIt(3), DataManager.HUD_COMPONENTS);
            img.AnchorAndAlignToObject(box, SideEnum.Bottom, SideEnum.CenterX, GameManager.ScaleIt(2));
            AddControl(img);

            _btnBuy = new GUIButton(new Rectangle(164, 0, 18, 19), DataManager.HUD_COMPONENTS, ProceedToPurchase);
            _btnBuy.Position(_winMain);
            _btnBuy.ScaledMoveBy(89, 48);
            AddControl(_btnBuy);

            _gSellValue = new GUIText(GameManager.CurrentItem.TotalBuyValue);
            _gSellValue.AnchorAndAlignToObject(_btnBuy, SideEnum.Left, SideEnum.CenterY, GameManager.ScaleIt(2));
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
            _gSellValue.AnchorAndAlignToObject(_btnBuy, SideEnum.Left, SideEnum.CenterY, GameManager.ScaleIt(2));

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
                GUIManager.OpenTextWindow(DataManager.GetGameTextEntry("BuyMerch_NoSpace"));
            }
        }
    }
}
