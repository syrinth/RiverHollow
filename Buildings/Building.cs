using Adventure.Characters.NPCs;
using Adventure.Game_Managers;
using Adventure.Tile_Engine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure
{
    public class Building
    {
        private const int MaxWorkers = 9;
        protected Worker[] _workers;
        public Worker[] Workers { get => _workers; }

        protected int _id;
        public int ID { get => _id; }

        protected int _baseWidth; //In Tiles
        public int BaseWidth { get => _baseWidth*TileMap.TileSize; } //In Pixels
        protected int _baseHeight; //In Tiles
        public int BaseHeight { get => _baseHeight * TileMap.TileSize; } //In Pixels

        protected int _reqGold;
        public int ReqGold { get => _reqGold; }

        protected Texture2D _texture;
        public Texture2D Texture { get => _texture; }

        protected Vector2 _position;
        public Vector2 Position { get => _position; }

        public Rectangle BoundingBox { get => new Rectangle((int)Position.X, (int)(Position.Y + (_texture.Height - BaseHeight)), BaseWidth, BaseHeight); }

        //returns -1 if there is no room, else returns the first slot that's open
        public int HasSpace()
        {
            int rv = -1;

            for(int i=0; i<_workers.Length; i++)
            {
                if(_workers[i] == null)
                {
                    rv = i;
                }
            }
            return rv;
        }

        //call HasSpace before adding
        public bool AddWorker(Worker worker, int index)
        {
            bool rv = false;

            if(worker != null && index < _workers.Length && _workers[index] == null)
            {
                _workers[index] = worker;
            }

            return rv;
        }

        public bool SetLocation(Vector2 position)
        {
            bool rv = true;
            _position = position;
            return rv;
        }

        public void Draw(SpriteBatch spritebatch)
        {
            spritebatch.Draw(_texture, new Rectangle((int)this.Position.X, (int)this.Position.Y, _texture.Width, _texture.Height), null, Color.White, 0, new Vector2(0, 0), SpriteEffects.None, Position.Y+Texture.Height);
        }
    }
}
