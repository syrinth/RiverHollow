using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.Game_Managers;
using RiverHollow.GUIComponents.GUIObjects;
using RiverHollow.GUIComponents.GUIObjects.GUIWindows;
using RiverHollow.Utilities;
using System;

namespace RiverHollow.GUIComponents.Screens
{
    public class HUDMiniInventory : GUIWindow
    {
        private enum StateEnum { None, FadeOut, FadeIn };
        readonly GUIItemBox[] _liItems;
        readonly GUIButton _btnChangeRow;
        readonly GUIImage _gSelected;

        StateEnum _eFadeState = StateEnum.FadeIn;
        SideEnum _eSnapPosition = SideEnum.Center;

        float _fAlphaValue;
        const float MIN_FADE = 0.1f;

        public HUDMiniInventory() : base(GUIUtils.WINDOW_DARKBLUE, GameManager.ScaleIt(221), GameManager.ScaleIt(30))
        {
            _liItems = new GUIItemBox[InventoryManager.maxItemColumns];

            for (int i = 0; i < InventoryManager.maxItemColumns; i++)
            {
                GUIItemBox ib = new GUIItemBox(InventoryManager.PlayerInventory[GameManager.HUDItemRow, i], Enums.ItemBoxDraw.MoreThanOne);
                _liItems[i] = ib;

                if (i == 0) { ib.AnchorToInnerSide(this, SideEnum.TopLeft); }
                else { ib.AnchorAndAlignWithSpacing(_liItems[i - 1], SideEnum.Right, SideEnum.Bottom, GUIManager.STANDARD_MARGIN); }

                ib.SetAlpha(_fAlphaValue);
            }

            _btnChangeRow = new GUIButton(GUIUtils.BTN_DOWN_SMALL, RowUp);
            _btnChangeRow.AnchorAndAlign(this, SideEnum.Right, SideEnum.CenterY, GUIUtils.ParentRuleEnum.ForceToObject);
            _btnChangeRow.FadeOnDisable(false);
            _btnChangeRow.Show(PlayerManager.BackpackLevel != 1);

            _fAlphaValue = GameManager.HideMiniInventory ? MIN_FADE : 1.0f;

            _gSelected = new GUIImage(GUIUtils.SELECT_CORNER);
            AddControl(_gSelected);
            Alpha(_fAlphaValue);

            MoveSelector(GameManager.HUDItemCol);

            Snap(SideEnum.Bottom);
        }

        private bool Functional()
        {
            return !CutsceneManager.Playing && !GUIManager.IsMainObjectOpen() && !GUIManager.IsTextWindowOpen();
        }
        public override void Draw(SpriteBatch spriteBatch)
        {
            if (Functional())
            {
                base.Draw(spriteBatch);
            }
        }

        public override void Update(GameTime gTime)
        {
            if (Functional())
            {
                HandleInput();

                base.Update(gTime);
                float startFade = _fAlphaValue;
                if (GameManager.HideMiniInventory)
                {
                    switch (_eFadeState)
                    {
                        case StateEnum.FadeOut:
                            _fAlphaValue = Math.Max(MIN_FADE, _fAlphaValue -= MIN_FADE);
                            break;
                        case StateEnum.FadeIn:
                            _fAlphaValue = Math.Min(1, _fAlphaValue += MIN_FADE);
                            UpdateItemFade(gTime);
                            break;
                        case StateEnum.None:
                            _eFadeState = StateEnum.FadeOut;
                            break;
                    }
                }
                else
                {
                    _fAlphaValue = 1;
                }
                SetAlpha(startFade);
            }

            for (int i = 0; i < _liItems.Length; i++)
            {
                _liItems[i].SetItem(InventoryManager.PlayerInventory[GameManager.HUDItemRow, i]);
                _liItems[i].SetAlpha(Alpha());
            }

            int playerHeight = GameManager.ScaleIt(PlayerManager.PlayerActor.Position.Y);
            int mapHeight = MapManager.CurrentMap.GetMapHeightInScaledPixels();
            int screenHeight = RiverHollow.ScreenHeight;

            if(playerHeight > mapHeight - (screenHeight / 2))
            {
                Snap(SideEnum.Top);
            }
            else
            {
                Snap(SideEnum.Bottom);
            }
        }

        internal override void Show(bool val)
        {
            base.Show(val);
            _btnChangeRow.Show(PlayerManager.BackpackLevel != 1);
        }

        private void HandleInput()
        {
            if (!GameManager.GamePaused() && GameManager.HeldObject == null)
            {
                foreach (var kvp in InputManager.Numbers)
                {
                    if (InputManager.CheckForInitialKeyDown(kvp))
                    {
                        int value = int.Parse(kvp.ToString().Remove(0, 1));
                        if (value > 0) { value--; }
                        else { value = 9; }
                        SelectGuiItemBox(_liItems[value]);
                    }
                }

                int wheelValue = InputManager.ScrollWheelChanged();
                int index = Util.GetLoopingValue(GameManager.HUDItemCol, 0, _liItems.Length - 1, (wheelValue * -1));
                SelectGuiItemBox(_liItems[index]);
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

            if (Functional() && Contains(mouse))
            {
                rv = true;

                foreach (GUIItemBox gib in _liItems)
                {
                    if (gib.Contains(mouse))
                    {
                        SelectGuiItemBox(gib);
                        if (gib.BoxItem != null && gib.BoxItem.CompareType(Enums.ItemTypeEnum.Buildable))
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

            if (Functional() && Contains(mouse))
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

        public override bool ProcessHover(Point mouse)
        {
            if (Functional())
            {
                bool rv = base.ProcessHover(mouse);

                if (rv && !GameManager.GamePaused())
                {
                    _eFadeState = StateEnum.FadeIn;
                }

                return rv;
            }
            else
            {
                return false;
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
            GameManager.HUDItemCol = Array.FindIndex(_liItems, x => x == box);
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
            for (int i = 0; i < _liItems.Length; i++)
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

                GUIManager.CloseHoverWindow();
            }
        }
    }
}
