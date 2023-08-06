using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.BitmapFonts;
using RiverHollow.Game_Managers;
using RiverHollow.GUIComponents.GUIObjects;
using RiverHollow.GUIComponents.GUIObjects.GUIWindows;
using RiverHollow.Items;
using RiverHollow.Misc;
using RiverHollow.Utilities;
using System.Collections.Generic;
using static RiverHollow.GUIComponents.GUIObjects.GUIObject;

namespace RiverHollow.Map_Handling
{
    public class ShopItemSpot
    {
        string _sMapName;
        Point _pPosition;
        Merchandise _merch;
        MapItem _merchItem;
        public int MerchID => _merch != null ? _merch.MerchID : -1;
        public Rectangle Box { get; private set; }

        public ShopItemSpot(string mapName, Vector2 position, float width = Constants.TILE_SIZE, float height = Constants.TILE_SIZE * 2)
        {
            _sMapName = mapName;
            _pPosition = position.ToPoint();
            Box = new Rectangle(_pPosition.X, _pPosition.Y, (int)width, (int)height);
        }

        public void Draw(SpriteBatch spritebatch)
        {
            if (_merchItem != null)
            {
                if (!GameManager.GamePaused() && !CutsceneManager.Playing && !GameManager.InTownMode() && Box.Contains(GUICursor.GetWorldMousePosition()))
                {
                    BitmapFont font = DataManager.GetBitMapFont(@"Fonts\FontBattle");
                    Size2 size = font.MeasureString(_merch.Price.ToString());
                    int delta = (int)size.Width - Box.Width;
                    spritebatch.DrawString(font, _merch.Price.ToString(), _pPosition.ToVector2() + new Vector2(-delta / 2, -8), Color.White, Constants.MAX_LAYER_DEPTH);

                    if (!GUIManager.IsHoverWindowOpen())
                    {
                        var win = new GUIItemDescriptionWindow(_merchItem.WrappedItem, Point.Zero);
                        win.AnchorToScreen(SideEnum.BottomRight);
                        GUIManager.OpenHoverObject(win, Box, false);
                    }
                }

                List<RHTile> t = MapManager.Maps[_sMapName].GetTilesFromRectangleExcludeEdgePoints(Box);
                if (t.Count == 2)
                {
                    t[1].WorldObject?.DrawItem(spritebatch, _merchItem);
                }
                else {
                    _merchItem.Draw(spritebatch);
                }
            }
        }

        internal void SetMerchandise(Merchandise m)
        {
            _merch = m;
            if (m != null)
            {
                if (m.MerchType == Merchandise.MerchTypeEnum.Item)
                {
                    _merchItem = new MapItem(DataManager.GetItem(m.MerchID))
                    {
                        Position = _pPosition
                    };
                }
            }
            else
            {
                _merchItem = null;
            }
        }

        internal bool Contains(Point mouseLocation)
        {
            return Box.Contains(mouseLocation);
        }

        internal void Buy()
        {
            if (_merch != null)
            {
                if (PlayerManager.Money < _merch.Price)
                {
                    GUIManager.OpenTextWindow("BuyMerch_NoMoney");
                }
                else if (!InventoryManager.HasSpaceInInventory(_merchItem.WrappedItem.ID, 1))
                {
                    GUIManager.OpenTextWindow("BuyMerch_NoSpace");
                }
                else
                {
                    GUIManager.CloseHoverWindow();
                    MapManager.CurrentMap.TheShop.SetSelectedMerchandise(_merch);
                    if (!_merchItem.WrappedItem.Stacks())
                    {
                        GUIManager.OpenTextWindow("BuyMerch_Confirm", _merchItem.WrappedItem.Name(), _merch.Price);
                    }
                    else
                    {
                        GUIManager.OpenMainObject(new QuantityWindow());
                    }
                }
            }
        }
    }
}
