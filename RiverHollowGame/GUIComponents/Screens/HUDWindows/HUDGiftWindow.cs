using RiverHollow.Characters;
using RiverHollow.Game_Managers;
using RiverHollow.GUIComponents.GUIObjects.GUIWindows;
using RiverHollow.GUIComponents.GUIObjects;
using RiverHollow.Items;
using Microsoft.Xna.Framework;
using static RiverHollow.Utilities.Enums;
using RiverHollow.Misc;

namespace RiverHollow.GUIComponents.Screens.HUDWindows
{
    public class HUDGiftWindow : GUIMainObject
    {
        private Item[,] _arrToSell;
        private Villager _npc;
        private GUIInventoryWindow _inventory;
        private GUIButton _btnGive;

        private GUIWindow _gVillagerWindow;
        private GUIInventory _gToGive;

        public HUDGiftWindow(Villager npc)
        {
            _npc = npc;
            _arrToSell = new Item[1, 1];
            InventoryManager.InitExtraInventory(_arrToSell);
            InventoryManager.ExtraHoldSingular = true;

            _gVillagerWindow = new GUIWindow(GUIUtils.WINDOW_DARKBLUE, GameManager.ScaleIt(114), GameManager.ScaleIt(73));

            GUIText text = new GUIText(_npc.Name());
            text.AnchorToInnerSide(_gVillagerWindow, SideEnum.Top, 1);

            GUIImage img = new GUIImage(GUIUtils.HUD_SCROLL_S);
            img.PositionAndMove(_gVillagerWindow, 7, 42);

            GUISprite spr = new GUISprite(npc.BodySprite, true);
            spr.AnchorAndAlignThenMove(img, SideEnum.Top, SideEnum.Left, 1, -2);
            spr.PlayAnimation(VerbEnum.Idle, DirectionEnum.Down);

            _gToGive = new GUIInventory();
            _gToGive.PositionAndMove(_gVillagerWindow, 7, 47);

            _btnGive = new GUIButton(GUIUtils.BTN_GIVE, BtnGift);
            _btnGive.PositionAndMove(_gVillagerWindow, 89, 48);
            _btnGive.Enable(false);

            _gVillagerWindow.ScaledMoveBy(54, 0);
            _inventory = new GUIInventoryWindow(true);
            _inventory.AnchorToObject(_gVillagerWindow, SideEnum.Bottom, 2);
            
            Width = _inventory.Width;
            Height = _inventory.Bottom - _gVillagerWindow.Top;

            AddControls(_gVillagerWindow, _inventory);
            CenterOnScreen();
        }

        public override bool ProcessLeftButtonClick(Point mouse)
        {
            bool rv = _btnGive.ProcessLeftButtonClick(mouse);
            if (!rv)
            {
                rv = base.ProcessRightButtonClick(mouse);
            }

            Refresh();
            return rv;
        }

        public override bool ProcessRightButtonClick(Point mouse)
        {
            bool rv = false;
            if (_inventory.Contains(mouse) || _gVillagerWindow.Contains(mouse))
            {
                rv = base.ProcessRightButtonClick(mouse);
                Refresh();
            }

            return rv;
        }

        public void BtnGift()
        {
            _npc.Gift(_arrToSell[0, 0]);
        }

        private void Refresh()
        {
            Item it = _arrToSell[0, 0];
            if (it == null || !it.Giftable())
            {
                _btnGive.Enable(false);
            }
            else
            {
                _btnGive.Enable(true);
            }
        }

        public override void CloseMainWindow()
        {
            InventoryManager.AddToInventory(_arrToSell[0, 0], true, true);
            GameManager.SetCurrentNPC(null);
            InventoryManager.ExtraHoldSingular = false;
        }
    }
}
