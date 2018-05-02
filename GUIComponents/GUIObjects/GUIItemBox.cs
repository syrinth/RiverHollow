using RiverHollow.Game_Managers.GUIObjects;
using RiverHollow.WorldObjects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.GUIObjects;
using RiverHollow.Characters.NPCs;

namespace RiverHollow.Game_Managers.GUIComponents.GUIObjects
{
    public class GUIItemBox : GUIImage
    {
        private static SpriteFont _displayFont = GameContentManager.GetFont(@"Fonts\DisplayFont");
        private Item _item;
        public Item Item { get => _item; set => _item = value; }
        public bool Open = true;
        private bool _hover;
        private GUITextWindow _textWindow;

        public GUIItemBox(Vector2 position, Rectangle sourceRect, int width, int height, string texture, Item item) : base(position, sourceRect, width, height, texture)
        {
            _item = item;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
            if (_item != null)
            {
                _item.Draw(spriteBatch, _drawRect);
                if (_item.DoesItStack)
                {
                    spriteBatch.DrawString(_displayFont, _item.Number.ToString(), new Vector2(_drawRect.X + 22, _drawRect.Y + 22), Color.White);
                }
            }
            if (_hover)
            {
                if (_textWindow != null) { _textWindow.Draw(spriteBatch, true); }
            }
        }

        public bool DrawDescription(SpriteBatch spriteBatch)
        {
            if (_textWindow != null) {
                _textWindow.Draw(spriteBatch, true);
            }

            return _hover;
        }

        public bool ProcessHover(Point mouse)
        {
            bool rv = false;
            if (Contains(mouse))
            {
                _hover = true;
                if (_item != null)
                {
                    _textWindow = new GUITextWindow(mouse.ToVector2(), _item.GetDescription());
                    _textWindow.Position(new Vector2(mouse.ToVector2().X, mouse.ToVector2().Y + 32));
                    _textWindow.Resize();
                }
                rv = true;
            }
            else
            {
                _hover = false;
                _textWindow = null;
            }
            return rv;
        }
    }

    public class WorkerBox : GUIObject
    {
        private SpriteFont _font;
        private GUIWindow _workerWindow;
        private GUIWindow _costWindow;
        public WorldAdventurer _w;
        public int Cost;

        public WorkerBox(Vector2 p, WorldAdventurer w, int cost)
        {
            _font = GameContentManager.GetFont(@"Fonts\Font");
            Cost = cost;
            _w = w;
            _workerWindow = new GUIWindow(p, GUIWindow.RedWin, 64, 96);
            _costWindow = new GUIWindow(new Vector2(p.X, p.Y + 96), GUIWindow.RedWin, 64, 32);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            _workerWindow.Draw(spriteBatch);
            _costWindow.Draw(spriteBatch);
            _w.Position = new Vector2(_workerWindow.Position().X + _workerWindow.EdgeSize, (int)_workerWindow.Position().Y + _workerWindow.EdgeSize);
            _w.Draw(spriteBatch);
            spriteBatch.DrawString(_font, Cost.ToString(), _costWindow.Position() + new Vector2(_costWindow.EdgeSize/2, _costWindow.EdgeSize/2), Color.White);
        }

        public override bool Contains(Point mouse)
        {
            return _workerWindow.Contains(mouse);
        }
    }
}
