using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.GUIComponents.GUIObjects;
using static RiverHollow.GUIObjects.GUIObject;
using static RiverHollow.Game_Managers.GameManager;
using RiverHollow.Game_Managers.GUIComponents.GUIObjects;
using System.Collections.Generic;
using RiverHollow.GUIObjects;

namespace RiverHollow.Game_Managers.GUIObjects
{
    public class HUDScreen : GUIScreen
    {
        GUIItemBox _gSelectedBox;

        GUIStatDisplay _healthDisplay;
        GUIStatDisplay _staminaDisplay;
        GUIMoneyDisplay _gMoney;

        HUDInventory _gInventory;

        public HUDScreen()
        {
            _healthDisplay = new GUIStatDisplay(GUIStatDisplay.DisplayEnum.Health);
            _healthDisplay.AnchorToScreen(this, SideEnum.TopLeft, 10);
            _staminaDisplay = new GUIStatDisplay(GUIStatDisplay.DisplayEnum.Energy);
            _staminaDisplay.AnchorAndAlignToObject(_healthDisplay, SideEnum.Bottom, SideEnum.Left);

            _gMoney = new GUIMoneyDisplay();
            _gMoney.AnchorAndAlignToObject(_staminaDisplay, SideEnum.Bottom, SideEnum.Left);

            _gInventory = new HUDInventory();
            _gInventory.AnchorToScreen(SideEnum.Bottom);
        }

        public override bool ProcessLeftButtonClick(Point mouse)
        {
            bool rv = false;

            if (base.ProcessLeftButtonClick(mouse))
            {
                if (_gSelectionWindow.SelectedAction.Contains("UseItem"))
                {
                    GameManager.UseItem();
                }
            }
            else
            {
                rv = _gInventory.ProcessLeftButtonClick(mouse);
            }

            return rv;
        }

        public override bool ProcessRightButtonClick(Point mouse)
        {
            bool rv = false;
            rv = _gInventory.ProcessRightButtonClick(mouse);
            return rv;
        }

        public override bool ProcessHover(Point mouse)
        {
            bool rv = false;
            if (_healthDisplay.ProcessHover(mouse)) { rv = true; }
            if (_staminaDisplay.ProcessHover(mouse)) { rv = true; }
            if ( _gInventory.ProcessHover(mouse)) { rv = true; }
            return rv;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            _gMoney.Update(gameTime);
            _gInventory.Update(gameTime);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
            _gInventory.Draw(spriteBatch);
        }

        public override void Sync()
        {
            _gInventory.SyncItems();
        }

        public override bool IsHUD() { return true; }
    }

    public class HUDInventory : GUIWindow
    {
        List<GUIItemBox> _liItems;
        GUIButton _btnChangeRow;

        bool _bFadeOutBar = true;
        bool _bFadeItemsOut;
        bool _bFadeItemsIn;
        float _fBarFade;
        float _fItemFade = 1.0f;
        float FADE_OUT = 0.1f;

        public HUDInventory() : base(GUIWindow.BrownWin, TileSize, TileSize)
        {
            _btnChangeRow = new GUIButton(new Rectangle(256, 96, 32, 32), 64, 64, @"Textures\Dialog", RowUp);
            _liItems = new List<GUIItemBox>();
            _fBarFade = GameManager.HideMiniInventory ? FADE_OUT : 1.0f;
            Alpha = _fBarFade;
            for (int i = 0; i < InventoryManager.maxItemColumns; i++)
            {
                GUIItemBox ib = new GUIItemBox(InventoryManager.PlayerInventory[GameManager.HUDItemRow, i]);
                _liItems.Add(ib);

                if(i == 0) { ib.AnchorToInnerSide(this, SideEnum.TopLeft); }
                else { ib.AnchorAndAlignToObject(_liItems[i-1], SideEnum.Right, SideEnum.Bottom); }

                ib.SetAlpha(_fBarFade);
            }

            _liItems[GameManager.HUDItemCol].Select(true);
            Resize();

            _btnChangeRow.AnchorAndAlignToObject(this, SideEnum.Right, SideEnum.CenterY);
            AddControl(_btnChangeRow);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            float startFade = _fBarFade;
            if (_bFadeOutBar && GameManager.HideMiniInventory)
            {
                if (_fBarFade - FADE_OUT > FADE_OUT) { _fBarFade -= FADE_OUT; }
                else
                {
                    _fBarFade = FADE_OUT;
                }
            }
            else
            {
                if (_fBarFade < 1) {
                    _fBarFade += FADE_OUT;
                }

                if (_bFadeItemsOut)
                {
                    float currFade = _fItemFade;
                    if (currFade - FADE_OUT > FADE_OUT)
                    {
                        _fItemFade -= FADE_OUT;
                        foreach (GUIItemBox gib in _liItems)
                        {
                            gib.SetItemAlpha(_fItemFade);
                        }
                    }
                    else
                    {
                        currFade = FADE_OUT;
                        _bFadeItemsOut = false;
                        _bFadeItemsIn = true;
                        SyncItems();
                    }
                }
                if (_bFadeItemsIn)
                {
                    float currFade = _fItemFade;
                    if (currFade < 1)
                    {
                        _fItemFade += FADE_OUT;
                    }
                    else
                    {
                        _bFadeItemsIn = false;
                    }

                    foreach (GUIItemBox gib in _liItems)
                    {
                        gib.SetItemAlpha(_fItemFade);
                    }
                }
            }

            if(startFade != _fBarFade)
            {
                Alpha = _fBarFade;

                foreach (GUIItemBox gib in _liItems)
                {
                    gib.SetAlpha(Alpha);
                }
                _btnChangeRow.Alpha = Alpha;
            }
        }

        public override bool Contains(Point mouse)
        {
            return base.Contains(mouse) || _btnChangeRow.Contains(mouse);
        }

        public override bool ProcessLeftButtonClick(Point mouse)
        {
            bool rv = false;

            if (Contains(mouse))
            {
                rv = true;

                foreach (GUIItemBox gib in _liItems)
                {
                    if (gib.Contains(mouse))
                    {
                        _liItems[GameManager.HUDItemCol].Select(false);
                        GameManager.HUDItemCol = _liItems.IndexOf(gib);
                        _liItems[GameManager.HUDItemCol].Select(true);
                        break;
                    }
                }

                _btnChangeRow.ProcessLeftButtonClick(mouse);
            }

            return rv;
        }

        public override bool ProcessRightButtonClick(Point mouse)
        {
            bool rv = false;

            if (Contains(mouse))
            {
                rv = true;

                foreach (GUIItemBox gib in _liItems)
                {
                    if (gib.Contains(mouse))
                    {
                        if (gib.Item.IsFood() || gib.Item.IsClassItem())
                        {
                            string text = string.Empty;
                            if (gib.Item.IsFood())
                            {
                                text = GameContentManager.GetGameText("FoodConfirm");
                            }
                            else if (gib.Item.IsClassItem())
                            {
                                text = GameContentManager.GetGameText("ClassItemConfirm");
                            }
                            GameManager.gmActiveItem = gib.Item;
                            GUIManager.AddTextSelection(string.Format(text, gib.Item.Name));
                        }
                    }
                }
            }

            return rv;
        }

        public bool ProcessHover(Point mouse)
        {
            bool rv = false;

            if (Contains(mouse) && Alpha != 1)
            {
                rv = true;
                _bFadeOutBar = false;
            }
            else if (!Contains(mouse) && GameManager.HideMiniInventory && Alpha != 0.1f)
            {
                _bFadeOutBar = true;
            }

            return rv;
        }

        public void RowUp()
        {
            if(GameManager.HUDItemRow < InventoryManager.maxItemRows - 1)
            {
                GameManager.HUDItemRow++;
            }
            else
            {
                GameManager.HUDItemRow = 0;
            }

            _bFadeItemsOut = true;
        }

        public void SyncItems()
        {
            for (int i = 0; i < _liItems.Count; i++)
            {
                GUIItemBox ib = _liItems[i];
                ib.SetItem(InventoryManager.PlayerInventory[GameManager.HUDItemRow, i]);
            }
        }
    }
}
