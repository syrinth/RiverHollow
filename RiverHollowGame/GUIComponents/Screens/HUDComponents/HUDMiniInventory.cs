using Microsoft.Xna.Framework;
using RiverHollow.Game_Managers;
using RiverHollow.GUIComponents.GUIObjects;
using RiverHollow.GUIComponents.GUIObjects.GUIWindows;
using RiverHollow.Utilities;
using System.Collections.Generic;

using static RiverHollow.Game_Managers.GameManager;

namespace RiverHollow.GUIComponents.Screens
{
    public class HUDMiniInventory : GUIWindow
    {
        private enum StateEnum { None, FadeOut, FadeIn };
        List<GUIItemBox> _liItems;
        GUIButton _btnChangeRow;

        StateEnum _eFadeState = StateEnum.FadeIn;

        float _fAlphaValue;
        float _fItemFade = 1.0f;
        const float MIN_FADE = 0.1f;

        SideEnum _eSnapPosition = SideEnum.Center;

        static Rectangle RECT_SELECT_IMG = new Rectangle(260, 0, 20, 20);
        GUIImage _gSelected;

        public HUDMiniInventory() : base(GUIWindow.DarkBlue_Window, Constants.TILE_SIZE, Constants.TILE_SIZE)
        {
            _btnChangeRow = new GUIButton(new Rectangle(256, 96, 16, 16), ScaledTileSize, ScaledTileSize, DataManager.DIALOGUE_TEXTURE, RowUp);
            _btnChangeRow.FadeOnDisable(false);
            _liItems = new List<GUIItemBox>();

            for (int i = 0; i < InventoryManager.maxItemColumns; i++)
            {
                GUIItemBox ib = new GUIItemBox(InventoryManager.PlayerInventory[GameManager.HUDItemRow, i]);
                _liItems.Add(ib);

                if (i == 0) { ib.AnchorToInnerSide(this, SideEnum.TopLeft); }
                else { ib.AnchorAndAlignToObject(_liItems[i - 1], SideEnum.Right, SideEnum.Bottom, GUIManager.STANDARD_MARGIN); }

                ib.SetAlpha(_fAlphaValue);
            }

            Resize();

            _btnChangeRow.AnchorAndAlignToObject(this, SideEnum.Right, SideEnum.CenterY);
            AddControl(_btnChangeRow);

            _fAlphaValue = GameManager.HideMiniInventory ? MIN_FADE : 1.0f;

            _gSelected = new GUIImage(RECT_SELECT_IMG, ScaleIt(RECT_SELECT_IMG.Width), ScaleIt(RECT_SELECT_IMG.Height), DataManager.DIALOGUE_TEXTURE);
            AddControl(_gSelected);

            Alpha(_fAlphaValue);
            MoveSelector(0);

            Snap(SideEnum.Bottom);
        }

        public override void Update(GameTime gTime)
        {
            if (Show())
            {
                _btnChangeRow.Show(PlayerManager.BackpackLevel != 1);

                base.Update(gTime);
                float startFade = _fAlphaValue;
                if (GameManager.HideMiniInventory)
                {
                    if (_eFadeState == StateEnum.FadeOut)
                    {
                        if (_fAlphaValue - MIN_FADE > MIN_FADE) { _fAlphaValue -= MIN_FADE; }
                        else
                        {
                            _fAlphaValue = MIN_FADE;
                        }
                    }
                    else
                    {
                        if (_eFadeState == StateEnum.FadeIn && _fAlphaValue < 1)
                        {
                            _fAlphaValue += MIN_FADE;
                        }

                        UpdateItemFade(gTime);

                    }
                }
                else
                {
                    _fAlphaValue = 1;
                }
                SetAlpha(startFade);
            }

            for (int i = 0; i < _liItems.Count; i++)
            {
                _liItems[i].SetItem(InventoryManager.PlayerInventory[GameManager.HUDItemRow, i]);
                _liItems[i].SetAlpha(Alpha());
            }

            int playerHeight = GameManager.ScaleIt(PlayerManager.PlayerActor.Position.Y);
            int mapHeight = MapManager.CurrentMap.GetMapHeightInScaledPixels();
            int screenHeight = RiverHollow.ScreenHeight;

            if(mapHeight > screenHeight && playerHeight > mapHeight - (screenHeight / 2))
            {
                Snap(SideEnum.Top);
            }
            else
            {
                Snap(SideEnum.Bottom);
            }
        }

        /// <summary>
        /// Handles the fading in and out of Items for when we switch rows
        /// </summary>
        /// <param name="gTime"></param>
        private void UpdateItemFade(GameTime gTime)
        {
            if (_eFadeState == StateEnum.FadeOut)
            {
                if (_fItemFade - MIN_FADE > MIN_FADE)
                {
                    _fItemFade -= MIN_FADE;
                    foreach (GUIItemBox gib in _liItems)
                    {
                        gib.SetItemAlpha(_fItemFade);
                    }
                }
                else
                {
                    _eFadeState = StateEnum.None;
                    SyncItems();
                }
            }

            if (_eFadeState == StateEnum.FadeIn)
            {
                if (_fItemFade < 1)
                {
                    _fItemFade += MIN_FADE;
                }
                else
                {
                    _eFadeState = StateEnum.None;
                }

                foreach (GUIItemBox gib in _liItems)
                {
                    gib.SetItemAlpha(_fItemFade);
                }
            }
        }

        private void SetAlpha(float startFade)
        {
            if (startFade != _fAlphaValue)
            {
                Alpha(_fAlphaValue);

                foreach (GUIItemBox gib in _liItems)
                {
                    gib.SetAlpha(Alpha());
                }
                _btnChangeRow.Alpha(Alpha());
            }
        }

        public override bool Contains(Point mouse)
        {
            return base.Contains(mouse) || _btnChangeRow.Contains(mouse);
        }

        public override bool ProcessLeftButtonClick(Point mouse)
        {
            bool rv = false;

            if (!GameManager.GamePaused() && Contains(mouse))
            {
                rv = true;

                foreach (GUIItemBox gib in _liItems)
                {
                    if (gib.Contains(mouse))
                    {
                        GameManager.HUDItemCol = _liItems.IndexOf(gib);
                        MoveSelector(GameManager.HUDItemCol);
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

            if (!GameManager.GamePaused() && Contains(mouse))
            {
                rv = true;

                foreach (GUIItemBox gib in _liItems)
                {
                    if (gib.Contains(mouse) && gib.BoxItem != null)
                    {
                        rv = gib.BoxItem.ItemBeingUsed();
                        break;
                    }
                }
            }

            return rv;
        }

        public override bool ProcessHover(Point mouse)
        {
            bool rv = false;
            if (!GameManager.GamePaused())
            {
                if (Contains(mouse))
                {
                    _eFadeState = StateEnum.FadeIn;
                }
                else if (!Contains(mouse) && GameManager.HideMiniInventory && Alpha() != 0.1f)
                {
                    _eFadeState = StateEnum.FadeOut;
                }
            }

            return rv;
        }

        public void RowUp()
        {
            if (GameManager.HUDItemRow < PlayerManager.BackpackLevel - 1)
            {
                GameManager.HUDItemRow++;
            }
            else
            {
                GameManager.HUDItemRow = 0;
            }

            _eFadeState = StateEnum.FadeOut;
        }

        public void SyncItems()
        {
            for (int i = 0; i < _liItems.Count; i++)
            {
                GUIItemBox ib = _liItems[i];
                ib.SetItem(InventoryManager.PlayerInventory[GameManager.HUDItemRow, i]);
            }
        }

        private void MoveSelector(int val)
        {
            _gSelected.CenterOnObject(_liItems[val]);
        }

        public void Snap(SideEnum snapPosition)
        {
            if (_eSnapPosition != snapPosition)
            {
                _eSnapPosition = snapPosition;
                AnchorToScreen(_eSnapPosition, ScaleIt(2));

                _eFadeState = StateEnum.None;
                float startFade = _fAlphaValue;
                _fAlphaValue = MIN_FADE;
                SetAlpha(startFade);
            }
        }
    }
}
