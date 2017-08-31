using Adventure.Buildings;
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
using System.Xml.Serialization;
using static Adventure.Game_Managers.PlayerManager;

namespace Adventure
{
    public abstract class Building
    {
        protected const int MaxWorkers = 9;
        public abstract string _map { get; }
        public abstract ObjectManager.BuildingID BuildingID { get; }

        public bool _selected = false;

        protected List<Worker> _workers;
        public List<Worker> Workers { get => _workers; }

        protected int _id;
        public int ID { get => _id; }

        protected int _baseWidth; //In Tiles
        public int BaseWidth { get => _baseWidth * TileMap.TileSize; } //In Pixels
        protected int _baseHeight; //In Tiles
        public int BaseHeight { get => _baseHeight * TileMap.TileSize; } //In Pixels

        protected int _reqGold;
        public int ReqGold { get => _reqGold; }

        protected Texture2D _texture;
        public Texture2D Texture { get => _texture; }

        protected Vector2 _position;
        public Vector2 Position { get => _position; }

        public Rectangle CollisionBox { get => new Rectangle((int)Position.X, (int)(Position.Y + (_texture.Height - BaseHeight)), BaseWidth, BaseHeight); }
        public Rectangle SelectionBox { get => new Rectangle((int)Position.X, (int)Position.Y, _texture.Width, _texture.Height); }

        protected Rectangle _boxToExit;
        public Rectangle BoxToExit { get => _boxToExit; }

        protected Rectangle _boxToEnter;
        public Rectangle BoxToEnter { get => _boxToEnter; }

        public Building() {}

        public bool HasSpace()
        {
            bool rv = false;

            rv = _workers.Count < 9;

            return rv;
        }

        //call HasSpace before adding
        public bool AddWorker(Worker worker, Random r)
        {
            bool rv = false;

            if(worker != null &&  _workers.Count < MaxWorkers)
            {
                
                worker.MakeDailyItem();
                _workers.Add(worker);
                Vector2 pos = new Vector2(r.Next(1160, 1860), r.Next(990, 1340));
                worker.Position = pos;
                rv = true;
            }

            return rv;
        }

        public abstract bool SetCoordinates(Vector2 position);

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(_texture, new Rectangle((int)this.Position.X, (int)this.Position.Y, _texture.Width, _texture.Height), null, _selected ? Color.Green : Color.White, 0, new Vector2(0, 0), SpriteEffects.None, Position.Y+Texture.Height);
        }

        public void AddBuildingDetails(BuildingData data)
        {
            _position = new Vector2(data.positionX, data.positionY);
            _id = data.id;

            Random r = new Random();
            foreach (WorkerData wData in data.Workers)
            {
                Worker w = ObjectManager.GetWorker(wData.workerID);
                w.SetName(wData.name);
                w.SetMood(wData.mood);
                AddWorker(w, r);
            }
        }
    }
}
