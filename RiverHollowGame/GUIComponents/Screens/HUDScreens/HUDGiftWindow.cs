namespace RiverHollow.GUIComponents.Screens.HUDScreens
{
    using global::RiverHollow.Characters;
    using global::RiverHollow.Game_Managers;
    using global::RiverHollow.GUIComponents.GUIObjects.GUIWindows;
    using global::RiverHollow.GUIComponents.GUIObjects;
    using global::RiverHollow.Items;
    using Microsoft.Xna.Framework;
    using static global::RiverHollow.Utilities.Enums;
    using global::RiverHollow.Misc;

    namespace RiverHollow.GUIComponents.Screens
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

                _gVillagerWindow = new GUIWindow(GUIWindow.DarkBlue_Window, GameManager.ScaleIt(114), GameManager.ScaleIt(73));
 
                GUIText text = new GUIText(_npc.Name());
                text.AnchorToInnerSide(_gVillagerWindow, SideEnum.Top, GameManager.ScaleIt(1));
                _gVillagerWindow.AddControl(text);

                GUIImage img = new GUIImage(new Rectangle(2, 120, 100, 3), GameManager.ScaleIt(100), GameManager.ScaleIt(3), DataManager.HUD_COMPONENTS);
                img.ScaledMoveBy(7, 42);
                _gVillagerWindow.AddControl(img);

                GUISprite spr = new GUISprite(npc.BodySprite, true);
                spr.AnchorAndAlignToObject(img, SideEnum.Top, SideEnum.Left, GameManager.ScaleIt(2));
                spr.ScaledMoveBy(1, 0);
                spr.PlayAnimation(VerbEnum.Idle, DirectionEnum.Down);
                _gVillagerWindow.AddControl(spr);

                _gToGive = new GUIInventory();
                _gToGive.ScaledMoveBy(7, 47);
                _gVillagerWindow.AddControl(_gToGive);

                _btnGive = new GUIButton(new Rectangle(164, 58, 18, 19), DataManager.HUD_COMPONENTS, BtnGift);
                _btnGive.ScaledMoveBy(89, 48);
                _btnGive.Enable(false);
                _gVillagerWindow.AddControl(_btnGive);
                AddControl(_gVillagerWindow);

                _gVillagerWindow.ScaledMoveBy(54, 0);
                _inventory = new GUIInventoryWindow(true);
                _inventory.AnchorToObject(_gVillagerWindow, SideEnum.Bottom, GameManager.ScaleIt(2));
                AddControl(_inventory);
                

                Width = _inventory.Width;
                Height = _inventory.Bottom - _gVillagerWindow.Top;

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
                TextEntry giftEntry = _npc.Gift(_arrToSell[0, 0]);
                GUIManager.CloseMainObject();
                GUIManager.OpenTextWindow(giftEntry);
            }

            private void Refresh()
            {
                Item it = _arrToSell[0, 0];
                if(it == null || !it.Giftable())
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

}
