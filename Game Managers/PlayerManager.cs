using Adventure.Tile_Engine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure.Game_Managers
{
    public class PlayerManager
    {
        static PlayerManager instance;

        private TileMap _currentMap;
        public TileMap CurrentMap { get => _currentMap; set => _currentMap = value; }

        private Player _player;
        public Player Player { get => _player; }

        private PlayerManager()
        {

        }

        public static PlayerManager GetInstance()
        {
            if (instance == null)
            {
                instance = new PlayerManager();
            }
            return instance;
        }

        public void NewPlayer()
        {
            _player = new Player();
        }

        public void Update(GameTime gameTime)
        {
            _player.Update(gameTime);
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            _player.Draw(gameTime, spriteBatch);
        }
    }
}
