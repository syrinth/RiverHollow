using Microsoft.Xna.Framework;
using RiverHollow.Game_Managers;
using RiverHollow.GUIComponents.GUIObjects;
using RiverHollow.GUIComponents.GUIObjects.GUIWindows;
using RiverHollow.Utilities;
using System.Collections.Generic;

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

        GUIImage _gSelected;

        public HUDMiniInventory() : base(GUIUtils.DarkBlue_Window, GameManager.ScaleIt(221), GameManager.ScaleIt(30))
        {
            HoverControls = false;

            _liItems = new List<GUIItemBox>();

            for (int i = 0; i < InventoryManager.maxItemColumns; i++)
            {
                GUIItemBox ib = new GUIItemBox(InventoryManager.PlayerInventory[GameManager.HUDItemRow, i]);
                _liItems.Add(ib);

                if (i == 0) { ib.AnchorToInnerSide(this, SideEnum.TopLeft); }
                else { ib.AnchorAndAlignWithSpacing(_liItems[i - 1], SideEnum.Right, SideEnum.Bottom, GUIManager.STANDARD_MARGIN); }

                ib.SetAlpha(_fAlphaValue);
            }

            _btnChangeRow = new GUIButton(GUIUtils.BTN_DOWN_SMALL, RowUp);
            _btnChangeRow.AnchorAndAlign(this, SideEnum.Right, SideEnum.CenterY, GUIUtils.ParentRuleEnum.ForceToObject);
            _btnChangeRow.FadeOnDisable(false);

            _fAlphaValue = GameManager.HideMiniInventory ? MIN_FADE : 1.0f;

            _gSelected = new GUIImage(GUIUtils.SELECT_CORNER);
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
                var value = _fAlphaValue;
                if (_fAlphaValue - MIN_FADE > MIN_FADE)
                {
                    value -= MIN_FADE;
                    foreach (GUIItemBox gib in _liItems)
                    {
                        gib.SetItemAlpha(value);
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
                var value = _fAlphaValue;
                if (_fAlphaValue < 1)
                {
                    value += MIN_FADE;
                }
                else
                {
                    _eFadeState = StateEnum.None;
                }

                foreach (GUIItemBox gib in _liItems)
                {
                    gib.SetItemAlpha(value);
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
                        SelectGuiItemBox(gib);
                        if (gib.BoxItem != null && gib.BoxItem.CompareType(Enums.ItemEnum.Buildable))
                        {
                            rv = gib.BoxItem.ItemBeingUsed();
                        }
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
                        SelectGuiItemBox(gib);
                        rv = gib.BoxItem.ItemBeingUsed();
                        break;
                    }
                }
            }

            return rv;
        }

        protected override void BeginHover()
        {
            if (!GameManager.GamePaused())
            {
                _eFadeState = StateEnum.FadeIn;
            }
        }

        protected override void EndHover()
        {
            if (!GameManager.GamePaused() && GameManager.HideMiniInventory && Alpha() != 0.1f)
            {
                _eFadeState = StateEnum.FadeOut;
            }
        }

        private void SelectGuiItemBox(GUIItemBox box)
        {
            GameManager.HUDItemCol = _liItems.IndexOf(box);
            MoveSelector(GameManager.HUDItemCol);
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
                AnchorToScreen(_eSnapPosition, 2);

                _eFadeState = StateEnum.None;
                float startFade = _fAlphaValue;
                _fAlphaValue = MIN_FADE;
                SetAlpha(startFade);
            }
        }
    }
}
