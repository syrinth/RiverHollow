using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.BitmapFonts;
using RiverHollow.Game_Managers;
using RiverHollow.Items;
using RiverHollow.Misc;
using RiverHollow.Utilities;
using System.Collections.Generic;

namespace RiverHollow.Map_Handling
{
    public class ShopItemSpot
    {
        string _sMapName;
        Vector2 _vPos;
        Merchandise _merch;
        Item merchItem;
        public int MerchID => _merch.MerchID;
        public bool ShowPrice = false;
        public Rectangle Box { get; private set; }

        public ShopItemSpot(string mapName, Vector2 position, float width, float height)
        {
            _sMapName = mapName;
            _vPos = new Vector2((int)position.X, (int)position.Y);
            Box = Util.FloatRectangle(_vPos, width, height);
        }

        public void Draw(SpriteBatch spritebatch)
        {
            if (merchItem != null)
            {
                if (ShowPrice)
                {
                    BitmapFont font = DataManager.GetBitMapFont(@"Fonts\FontBattle");
                    Size2 size = font.MeasureString(merchItem.Value.ToString());
                    float delta = size.Width - Box.Width;
                    spritebatch.DrawString(font, merchItem.Value.ToString(), _vPos + new Vector2(-delta / 2, -8), Color.White, GameManager.MAX_LAYER_DEPTH);
                }

                List<RHTile> t = MapManager.Maps[_sMapName].GetTilesFromRectangle(Box);
                if (t.Count == 2)
                {
                    t[1].WorldObject.DrawItem(spritebatch, merchItem);
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
                    GameManager.SetSelectedItem(DataManager.GetItem(merchItem.ItemID));
                    TextEntry entry = DataManager.GetGameTextEntry("BuyMerch_Confirm");
                    entry.FormatText(merchItem.Name(), merchItem.Value);
                    GUIManager.OpenTextWindow(entry);
                }
                else
                {
                    GUIManager.OpenTextWindow(DataManager.GetGameTextEntry("BuyMerch_NoMoney"));
                }
            }
        }
    }
}
