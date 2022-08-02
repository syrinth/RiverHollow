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
        Vector2 _vPos;
        Merchandise _merch;
        Item merchItem;
        public int MerchID => _merch != null ? _merch.MerchID : -1;
        public Rectangle Box { get; private set; }

        public ShopItemSpot(string mapName, Vector2 position, float width = Constants.TILE_SIZE, float height = Constants.TILE_SIZE * 2)
        {
            _sMapName = mapName;
            _vPos = new Vector2((int)position.X, (int)position.Y);
            Box = Util.FloatRectangle(_vPos, width, height);
        }

        public void Draw(SpriteBatch spritebatch)
        {
            if (merchItem != null)
            {
                if (!GameManager.GamePaused() &&  Box.Contains(GUICursor.GetWorldMousePosition()))
                {
                    BitmapFont font = DataManager.GetBitMapFont(@"Fonts\FontBattle");
                    Size2 size = font.MeasureString(merchItem.Value.ToString());
                    float delta = size.Width - Box.Width;
                    spritebatch.DrawString(font, merchItem.Value.ToString(), _vPos + new Vector2(-delta / 2, -8), Color.White, Constants.MAX_LAYER_DEPTH);

                    if (!GUIManager.IsHoverWindowOpen())
                    {
                        GUIItemDescriptionWindow win = new GUIItemDescriptionWindow(merchItem, Vector2.Zero);
                        win.AnchorToScreen(SideEnum.BottomRight);
                        GUIManager.OpenHoverWindow(win, Box, false);
                    }
                }

                List<RHTile> t = MapManager.Maps[_sMapName].GetTilesFromGridAlignedRectangle(Box);
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
                merchItem = DataManager.GetItem(m.MerchID);

                merchItem.Position = _vPos;
                merchItem.OnTheMap = true;
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
                if (PlayerManager.Money >= merchItem.Value)
                {
                    GUIManager.CloseHoverWindow();
                    if (!merchItem.DoesItStack)
                    {
                        GameManager.SetSelectedItem(DataManager.GetItem(merchItem.ItemID));
                        TextEntry entry = DataManager.GetGameTextEntry("BuyMerch_Confirm");
                        entry.FormatText(merchItem.Name(), merchItem.TotalValue);
                        GUIManager.OpenTextWindow(entry);
                    }
                    else
                    {
                        GameManager.SetSelectedItem(DataManager.GetItem(merchItem.ItemID));
                        GUIManager.OpenMainObject(new QuantityWindow());
                    }
                }
                else
                {
                    GUIManager.OpenTextWindow(DataManager.GetGameTextEntry("BuyMerch_NoMoney"));
                }
            }
        }
    }
}
