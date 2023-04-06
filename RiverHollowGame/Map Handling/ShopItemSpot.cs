﻿using Microsoft.Xna.Framework;
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
        MapItem merchItem;
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
            if (merchItem != null)
            {
                if (!GameManager.GamePaused() && !CutsceneManager.Playing && Box.Contains(GUICursor.GetWorldMousePosition()))
                {
                    BitmapFont font = DataManager.GetBitMapFont(@"Fonts\FontBattle");
                    Size2 size = font.MeasureString(merchItem.WrappedItem.BuyPrice.ToString());
                    int delta = (int)size.Width - Box.Width;
                    spritebatch.DrawString(font, merchItem.WrappedItem.BuyPrice.ToString(), _pPosition.ToVector2() + new Vector2(-delta / 2, -8), Color.White, Constants.MAX_LAYER_DEPTH);

                    if (!GUIManager.IsHoverWindowOpen())
                    {
                        GUIItemDescriptionWindow win = new GUIItemDescriptionWindow(merchItem.WrappedItem, Point.Zero);
                        win.AnchorToScreen(SideEnum.BottomRight);
                        GUIManager.OpenHoverWindow(win, Box, false);
                    }
                }

                List<RHTile> t = MapManager.Maps[_sMapName].GetTilesFromRectangleExcludeEdgePoints(Box);
                if (t.Count == 2)
                {
                    t[1].WorldObject?.DrawItem(spritebatch, merchItem);
                }
                else {
                    merchItem.Draw(spritebatch);
                }
            }
        }

        internal void SetMerchandise(Merchandise m)
        {
            _merch = m;
            if (m != null)
            {
                merchItem = new MapItem(DataManager.GetItem(m.MerchID))
                {
                    Position = _pPosition
                };
            }
            else
            {
                merchItem = null;
            }
        }

        internal bool Contains(Point mouseLocation)
        {
            return Box.Contains(mouseLocation);
        }

        internal void Buy()
        {
            if (merchItem != null)
            {
                if (PlayerManager.Money < merchItem.WrappedItem.BuyPrice)
                {
                    GUIManager.OpenTextWindow("BuyMerch_NoMoney");
                }
                else if (!InventoryManager.HasSpaceInInventory(merchItem.WrappedItem.ID, 1))
                {
                    GUIManager.OpenTextWindow("BuyMerch_NoSpace");
                }
                else
                {
                    GUIManager.CloseHoverWindow();
                    if (!merchItem.WrappedItem.Stacks())
                    {
                        GameManager.SetSelectedItem(DataManager.GetItem(merchItem.WrappedItem.ID));
                        GUIManager.OpenTextWindow("BuyMerch_Confirm", merchItem.WrappedItem.Name(), merchItem.WrappedItem.TotalBuyValue);
                    }
                    else
                    {
                        GameManager.SetSelectedItem(DataManager.GetItem(merchItem.WrappedItem.ID));
                        GUIManager.OpenMainObject(new QuantityWindow());
                    }
                }
            }
        }
    }
}
