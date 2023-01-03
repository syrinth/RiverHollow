﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using RiverHollow.Characters;
using RiverHollow.Game_Managers;
using RiverHollow.GUIComponents.GUIObjects;
using RiverHollow.GUIComponents.GUIObjects.GUIWindows;
using RiverHollow.Items;
using static RiverHollow.Utilities.Enums;

namespace RiverHollow.GUIComponents.Screens
{
    public class HUDMerchantWindow : GUIMainObject
    {
        Item[,] _arrToSell;
        Merchant _merchant;
        GUIInventoryWindow _inventory;
        GUIText _gSellValue;
        GUIButton _btnSell;

        GUIWindow _gMerchantWindow;
        GUIInventory _gToSell;

        public HUDMerchantWindow(Merchant m)
        {
            _merchant = m;
            _arrToSell = new Item[1, 1];
            InventoryManager.InitExtraInventory(_arrToSell);

            _gMerchantWindow = new GUIWindow(GUIWindow.Window_2, GameManager.ScaleIt(114), GameManager.ScaleIt(73));
            GUISprite spr = new GUISprite(m.BodySprite, true);
            spr.SetScale(GameManager.ScaledPixel);
            spr.ScaledMoveBy(9, 8);
            spr.PlayAnimation(VerbEnum.Idle, DirectionEnum.Down);
            _gMerchantWindow.AddControl(spr);

            GUIItemBox[] boxes = new GUIItemBox[3];
            for (int i = 0; i < 3; i++)
            {
                boxes[i] = new GUIItemBox(_merchant.ChosenRequests[i]);
                boxes[i].DrawNumber(false);
                if (i == 0) { boxes[i].ScaledMoveBy(32, 20); }
                else { boxes[i].AnchorAndAlignToObject(boxes[i - 1], SideEnum.Right, SideEnum.Top, GameManager.ScaleIt(5)); }

                _gMerchantWindow.AddControl(boxes[i]);
            }

            GUIText text = new GUIText("Special Requests");
            text.ScaledMoveBy(31, 7);
            _gMerchantWindow.AddControl(text);

            GUIImage img = new GUIImage(new Rectangle(2, 120, 100, 3), GameManager.ScaleIt(100), GameManager.ScaleIt(3), DataManager.HUD_COMPONENTS);
            img.ScaledMoveBy(7, 42);
            _gMerchantWindow.AddControl(img);

            _gToSell = new GUIInventory();
            _gToSell.ScaledMoveBy(7, 47);
            _gMerchantWindow.AddControl(_gToSell);

            _btnSell = new GUIButton(new Rectangle(164, 0, 18, 19), DataManager.HUD_COMPONENTS, BtnSell);
            _btnSell.ScaledMoveBy(89, 48);
            _btnSell.Enable(false);
            _gMerchantWindow.AddControl(_btnSell);

            _gSellValue = new GUIText(0);
            _gSellValue.AnchorAndAlignToObject(_btnSell, SideEnum.Left, SideEnum.CenterY, GameManager.ScaleIt(2));
            _gMerchantWindow.AddControl(_gSellValue);
            AddControl(_gMerchantWindow);

            _gMerchantWindow.ScaledMoveBy(54, 0);
            _inventory = new GUIInventoryWindow(true);
            _inventory.AnchorToObject(_gMerchantWindow, SideEnum.Bottom, GameManager.ScaleIt(2));
            AddControl(_inventory);

            Width = _inventory.Width;
            Height = _inventory.Bottom - _gMerchantWindow.Top;

            CenterOnScreen();
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

        public void BtnSell()
        {
            PlayerManager.AddMoney(_merchant.EvaluateItem(_arrToSell[0,0]));
            InventoryManager.RemoveItemFromInventory(_arrToSell[0, 0], false);

            _gSellValue.SetText(0);
            _gSellValue.AnchorAndAlignToObject(_btnSell, SideEnum.Left, SideEnum.CenterY, GameManager.ScaleIt(2));
        }

        private void RefreshOffer()
        {
            Color c = Color.Black;
            int offer = _merchant.EvaluateItem(_arrToSell[0, 0], ref c);
            _gSellValue.SetText(offer);
            _gSellValue.SetColor(c);
            _gSellValue.AnchorAndAlignToObject(_btnSell, SideEnum.Left, SideEnum.CenterY, GameManager.ScaleIt(2));

            _btnSell.Enable(offer > 0);
        }

        public override void CloseMainWindow()
        {
            InventoryManager.AddToInventory(_arrToSell[0, 0], true, true);
            GameManager.SetCurrentNPC(null);
        }
    }
}
