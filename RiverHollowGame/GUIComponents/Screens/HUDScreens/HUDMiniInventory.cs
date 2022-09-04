using Microsoft.Xna.Framework;
using RiverHollow.Game_Managers;
using RiverHollow.GUIComponents.GUIObjects;
using RiverHollow.GUIComponents.GUIObjects.GUIWindows;
using RiverHollow.Utilities;
using System.Collections.Generic;

using static RiverHollow.Game_Managers.GameManager;

namespace RiverHollow.GUIComponents.Screens.HUDScreens
{
    public class HUDMiniInventory : GUIWindow
    {
        List<GUIItemBox> _liItems;
        GUIButton _btnChangeRow;

        bool _bFadeOutBar = true;
        bool _bFadeItemsOut;
        bool _bFadeItemsIn;
        float _fBarFade;
        float _fItemFade = 1.0f;
        const float FADE_OUT = 0.1f;

        static Rectangle RECT_SELECT_IMG = new Rectangle(260, 0, 20, 20);
        GUIImage _gSelected;

        public HUDMiniInventory() : base(GUIWindow.Window_2, Constants.TILE_SIZE, Constants.TILE_SIZE)
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

                ib.SetAlpha(_fBarFade);
            }

            Resize();

            _btnChangeRow.AnchorAndAlignToObject(this, SideEnum.Right, SideEnum.CenterY);
            AddControl(_btnChangeRow);

            _fBarFade = GameManager.HideMiniInventory ? FADE_OUT : 1.0f;

            _gSelected = new GUIImage(RECT_SELECT_IMG, ScaleIt(RECT_SELECT_IMG.Width), ScaleIt(RECT_SELECT_IMG.Height), DataManager.DIALOGUE_TEXTURE);
            AddControl(_gSelected);

            Alpha(_fBarFade);
            MoveSelector(0);
        }

        public override void Update(GameTime gTime)
        {
            if (Show())
            {
                _btnChangeRow.Show(PlayerManager.BackpackLevel != 1);

                base.Update(gTime);
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
                    if (_fBarFade < 1)
                    {
                        _fBarFade += FADE_OUT;
                    }

                    UpdateItemFade(gTime);

                }
                if (startFade != _fBarFade)
                {
                    Alpha(_fBarFade);

                    foreach (GUIItemBox gib in _liItems)
                    {
                        gib.SetAlpha(Alpha());
                    }
                    _btnChangeRow.Alpha(Alpha());
                }
            }

            for (int i = 0; i < _liItems.Count; i++)
            {
                _liItems[i].SetItem(InventoryManager.PlayerInventory[GameManager.HUDItemRow, i]);
                _liItems[i].SetAlpha(Alpha());
            }
        }

        /// <summary>
        /// Handles the fading in and out of Items for when we switch rows
        /// </summary>
        /// <param name="gTime"></param>
        private void UpdateItemFade(GameTime gTime)
        {
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
                    rv = gib.ProcessRightButtonClick(mouse);
                    if (rv)
                    {
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
                if (Contains(mouse) && Alpha() != 1)
                {
                    rv = true;
                    _bFadeOutBar = false;
                }
                else if (!Contains(mouse) && GameManager.HideMiniInventory && Alpha() != 0.1f)
                {
                    _bFadeOutBar = true;
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

            _bFadeItemsOut = true;
            _bFadeItemsIn = false;
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
    }
}
