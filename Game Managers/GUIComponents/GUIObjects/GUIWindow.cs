using Adventure.Game_Managers.GUIObjects;
using Adventure.GUIObjects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace Adventure.Game_Managers.GUIComponents.GUIObjects
{
    public class GUIWindow : GUIObject
    {
        private GUIImage[] _ninePatches;
        private int _midWidth;
        public int MiddleWidth { get => _midWidth; }
        private int _midHeight;
        public int MiddleHeight{ get => _midHeight; }

        public GUIWindow()
        {
            _position = new Vector2(AdventureGame.ScreenWidth / 4, AdventureGame.ScreenHeight - 180);
            _width = AdventureGame.ScreenWidth / 2;
            _ninePatches = new GUIImage[9];
            _texture = GameContentManager.GetTexture(@"Textures\Dialog");

            _height = 148;
            _midWidth = _width - 64;
            _midHeight = _height - 64;
        }
        public GUIWindow(Vector2 position)
        {
            _position = position;
        }

        public override void Draw(SpriteBatch spritebatch)
        {
            {
                spritebatch.Draw(_texture, new Rectangle((int)_position.X, (int)_position.Y, 32, 32), new Rectangle(0, 0, 32, 32), Color.White);
                spritebatch.Draw(_texture, new Rectangle((int)_position.X + 32, (int)_position.Y, _midWidth, 32), new Rectangle(32, 0, 32, 32), Color.White);
                spritebatch.Draw(_texture, new Rectangle((int)_position.X + _midWidth + 32, (int)_position.Y, 32, 32), new Rectangle(64, 0, 32, 32), Color.White);

                spritebatch.Draw(_texture, new Rectangle((int)_position.X, (int)_position.Y + 32, 32, _midHeight), new Rectangle(0, 32, 32, 32), Color.White);
                spritebatch.Draw(_texture, new Rectangle((int)_position.X + 32, (int)_position.Y + 32, _midWidth, _midHeight), new Rectangle(32, 32, 32, 32), Color.White);
                spritebatch.Draw(_texture, new Rectangle((int)_position.X + _midWidth + 32, (int)_position.Y + 32, 32, _midHeight), new Rectangle(64, 32, 32, 32), Color.White);

                spritebatch.Draw(_texture, new Rectangle((int)_position.X, (int)_position.Y + _midHeight + 32, 32, 32), new Rectangle(0, 64, 32, 32), Color.White);
                spritebatch.Draw(_texture, new Rectangle((int)_position.X + 32, (int)_position.Y + _midHeight + 32, _midWidth, 32), new Rectangle(32, 64, 32, 32), Color.White);
                spritebatch.Draw(_texture, new Rectangle((int)_position.X + _midWidth + 32, (int)_position.Y + _midHeight + 32, 32, 32), new Rectangle(64, 64, 32, 32), Color.White);
            }
        }
    }
}
