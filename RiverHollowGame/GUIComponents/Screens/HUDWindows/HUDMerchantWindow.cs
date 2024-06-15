using Microsoft.Xna.Framework;
using RiverHollow.Characters;
using RiverHollow.Game_Managers;
using RiverHollow.GUIComponents.GUIObjects;
using RiverHollow.GUIComponents.GUIObjects.GUIWindows;
using RiverHollow.Items;
using RiverHollow.Utilities;
using static RiverHollow.Utilities.Enums;

namespace RiverHollow.GUIComponents.Screens.HUDWindows
{
    public class HUDMerchantWindow : GUIMainObject
    {
        readonly Item[,] _arrToSell;
        readonly Merchant _merchant;

        readonly GUIWindow _gMerchantWindow;
        readonly GUIInventoryWindow _inventory;
        readonly GUIText _gSellValue;
        readonly GUIText _gCapacity;
        readonly GUIButton _btnSell;

        public HUDMerchantWindow(Merchant m)
        {
            _merchant = m;
            _arrToSell = new Item[1, 1];
            InventoryManager.InitExtraInventory(_arrToSell);

            _gMerchantWindow = new GUIWindow(GUIUtils.WINDOW_DARKBLUE, GameManager.ScaleIt(114), GameManager.ScaleIt(73));
            GUISprite spr = new GUISprite(m.BodySprite, true);
            spr.ScaledMoveBy(9, 8);
            spr.PlayAnimation(VerbEnum.Idle, DirectionEnum.Down);
            _gMerchantWindow.AddControl(spr);

            GUIItemBox[] boxes = new GUIItemBox[3];
            for (int i = 0; i < Constants.MERCHANT_REQUEST_NUM; i++)
            {
                boxes[i] = new GUIItemBox(DataManager.GetItem(_merchant.GetCurrentRequests()[i]), ItemBoxDraw.Never);
                if (i == 0) { boxes[i].ScaledMoveBy(32, 20); }
                else { boxes[i].AnchorAndAlignWithSpacing(boxes[i - 1], SideEnum.Right, SideEnum.Top, 5); }

                _gMerchantWindow.AddControl(boxes[i]);
            }

            GUIText text = new GUIText("Requests");
            text.ScaledMoveBy(44, 6);
            _gMerchantWindow.AddControl(text);

            GUIImage img = new GUIImage(GUIUtils.HUD_SCROLL_S);
            img.ScaledMoveBy(7, 42);
            _gMerchantWindow.AddControl(img);

            var sellInventory = new GUIInventory();
            sellInventory.ScaledMoveBy(7, 47);
            _gMerchantWindow.AddControl(sellInventory);

            _btnSell = new GUIButton(GUIUtils.BTN_BUY, BtnSell);
            _btnSell.ScaledMoveBy(89, 48);
            _btnSell.Enable(false);
            _gMerchantWindow.AddControl(_btnSell);

            _gSellValue = new GUIText(0);
            _gMerchantWindow.AddControl(_gSellValue);
            AddControl(_gMerchantWindow);

            _gMerchantWindow.ScaledMoveBy(54, 0);
            _inventory = new GUIInventoryWindow(true);
            _inventory.AnchorToObject(_gMerchantWindow, SideEnum.Bottom, GameManager.ScaleIt(2));
            AddControl(_inventory);

            var capacityWindow = new GUIWindow(GUIUtils.WINDOW_DARKBLUE, GameManager.ScaleIt(40), GameManager.ScaleIt(26));
            capacityWindow.PositionAndMove(_gMerchantWindow, 115, 47);
            AddControl(capacityWindow);

            var icon = new GUIIcon(GUIUtils.ICON_BAG, GameIconEnum.Bag);
            icon.AnchorToInnerSide(capacityWindow, SideEnum.TopLeft);
            capacityWindow.AddControl(icon);

            _gCapacity = new GUIText(_merchant.Capacity);
            _gCapacity.AnchorAndAlignWithSpacing(icon, SideEnum.Right, SideEnum.CenterY, 4);

            CapacityCheck();

            Width = _inventory.Width;
            Height = _inventory.Bottom - _gMerchantWindow.Top;

            CenterOnScreen();
            RefreshOffer();
        }

        public override bool ProcessLeftButtonClick(Point mouse)
        {
            bool rv = base.ProcessLeftButtonClick(mouse);

            RefreshOffer();

            return rv;
        }

        public override bool ProcessRightButtonClick(Point mouse)
        {
            bool rv = false;
            if (_inventory.Contains(mouse) || _gMerchantWindow.Contains(mouse))
            {
               rv = base.ProcessRightButtonClick(mouse);
            }

            RefreshOffer();

            return rv;
        }

        private void CapacityCheck()
        {
            if (_merchant.Capacity == 0)
            {
                _gCapacity.SetColor(Color.Red);
            }
        }

        public void BtnSell()
        {
            var merch = _arrToSell[0, 0];
            int capacity = _merchant.Capacity;

            int profit = _merchant.EvaluateItem(merch);
            PlayerManager.AddMoney(profit);

            _merchant.UpdateCapacity(merch.Number);
            _gCapacity.SetText(_merchant.Capacity);

            TownManager.AddToSoldGoods(merch.ID, profit, capacity - _merchant.Capacity);

            CapacityCheck();

            if (merch.Number < capacity)
            {
                InventoryManager.RemoveItemFromInventory(merch, false);
            }
            else
            {
                merch.Remove(capacity, false);
            }

            _gSellValue.SetText(0);
            _gSellValue.AnchorAndAlignWithSpacing(_btnSell, SideEnum.Left, SideEnum.CenterY, 2);
        }

        private void RefreshOffer()
        {
            Color c = Color.White;
            int offer = _merchant.EvaluateItem(_arrToSell[0, 0], ref c);
            _gSellValue.SetText(offer);
            _gSellValue.SetColor(c);
            _gSellValue.AnchorAndAlignWithSpacing(_btnSell, SideEnum.Left, SideEnum.CenterY, 2);

            _btnSell.Enable(offer > 0);
        }

        public override void CloseMainWindow()
        {
            InventoryManager.AddToInventory(_arrToSell[0, 0], true, true);
            GameManager.SetCurrentNPC(null);
            InventoryManager.ClearExtraInventory();
        }
    }
}
