using RiverHollow.Characters.NPCs;
using RiverHollow.Game_Managers;
using RiverHollow.Items;
using RiverHollow.Tile_Engine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using static RiverHollow.Game_Managers.PlayerManager;
using RiverHollow.Misc;

namespace RiverHollow
{
    public class Building : WorldObject
    {
        private static int Size = RHMap.TileSize;

        public enum WorkerType { Magic, Craftsmen};
        private WorkerType _buildingWorker;
        public string _name;
        public string Name { get => _name; }

        public bool _selected = false;

        protected int _personalId;
        public int PersonalID { get => _personalId; }

        private int _entranceX;
        private int _entranceY;
        private int _baseStartX;
        private int _baseStartY;
        protected int _baseWidth; //In Tiles
        public int BaseWidth { get => _baseWidth * Size; } //In Pixels
        protected int _baseHeight; //In Tiles
        public int BaseHeight { get => _baseHeight * Size; } //In Pixels

        public override Rectangle CollisionBox { get => GenerateCollisionBox(); }
        public Rectangle SelectionBox { get => new Rectangle((int)Position.X, (int)Position.Y, _texture.Width, _texture.Height); }

        protected Rectangle _leaveLocation;
        public Rectangle BoxToExit { get => _leaveLocation; }

        protected Rectangle _boxToEnter;
        public Rectangle BoxToEnter { get => _boxToEnter; }

        #region Data Lists
        protected const int MaxWorkers = 9;
        protected List<WorldAdventurer> _workers;
        public List<WorldAdventurer> Workers { get => _workers; }

        protected Container _buildingChest;
        public Container BuildingChest { get => _buildingChest; set => _buildingChest = value; }

        protected Container _pantry;
        public Container Pantry { get => _pantry; set => _pantry = value; }

        protected List<StaticItem> _staticItemList;
        public List<StaticItem> StaticItems { get => _staticItemList; }
        #endregion

        public Building(string[] buildingData, int id) {
            ImportBasics(buildingData, id);
            _personalId = PlayerManager.GetNewBuildingID();
            _workers = new List<WorldAdventurer>();
            _staticItemList = new List<StaticItem>();

            _buildingChest = (Container)ObjectManager.GetItem(6);
            _pantry = (Container)ObjectManager.GetItem(6);

            _sourceRectangle = new Rectangle(0, 0, Texture.Width, Texture.Height);
        }

        public bool HasSpace()
        {
            bool rv = false;

            rv = _workers.Count < 9;

            return rv;
        }
        
        public bool AddWorker(WorldAdventurer worker, RHRandom r)
        {
            bool rv = false;

            if(worker != null &&  _workers.Count < MaxWorkers)
            {
                worker.SetBuilding(this);
                _workers.Add(worker);
                Vector2 pos = new Vector2(r.Next(1160, 1860), r.Next(990, 1340));
                worker.Position = pos;
                rv = true;
            }

            return rv;
        }

        public void Rollover()
        {
            foreach (WorldAdventurer w in _workers)
            {
                if (w.Rollover())
                {
                    bool eaten = false;
                    for (int i = 0; i < Pantry.Rows; i++)
                    {
                        for (int j = 0; j < Pantry.Rows; j++)
                        {
                            Item item = Pantry.Inventory[i, j];
                            if (item != null && item.Type == Item.ItemType.Food)
                            {
                                Pantry.RemoveItemFromInventory(i, j);
                                w.MakeDailyItem();
                                eaten = true;
                                break;
                            }
                        }
                        if (!eaten)
                        {
                            break;
                        }
                    }
                }
            }
        }

        public override void SetCoordinates(Vector2 position)
        {
            _position = position;

            int startX = (int)_position.X + (_entranceX * Size);
            int startY = (int)_position.Y + (_entranceY * Size);

            _boxToEnter = new Rectangle(startX, startY, Size, Size);
            _leaveLocation = new Rectangle(_boxToEnter.X, _boxToEnter.Y + Size, Size, Size);
        }

        public Rectangle GenerateCollisionBox()
        {
            int startX = (int)_position.X + (_baseStartX * Size);
            int startY = (int)_position.Y + (_baseStartY * Size);

            return new Rectangle(startX, startY, _baseWidth * Size, _baseHeight * Size);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(_texture, new Rectangle((int)this.Position.X, (int)this.Position.Y, _texture.Width, _texture.Height), null, _selected ? Color.Green : Color.White, 0, new Vector2(0, 0), SpriteEffects.None, Position.Y+Texture.Height);
        }

        protected int ImportBasics(string[] stringData, int id)
        {
            _id = id;
            int i = 0;
            _name = stringData[i++];
            _buildingWorker = (WorkerType)Enum.Parse(typeof(WorkerType), stringData[i++]);
            _texture = GameContentManager.GetTexture(@"Textures\"+ stringData[i++]);
            string[] split = stringData[i++].Split(' ');
            _width = int.Parse(split[0]);
            _height = int.Parse(split[1]);
            split = stringData[i++].Split(' ');
            _baseStartX = int.Parse(split[0]);
            _baseStartY = int.Parse(split[1]);
            _baseWidth = int.Parse(split[2]);
            _baseHeight = int.Parse(split[3]);
            split = stringData[i++].Split(' ');
            _entranceX = int.Parse(split[0]);
            _entranceY = int.Parse(split[1]);

            return i;
        }

        public void AddBuildingDetails(BuildingData data)
        {
            SetCoordinates(new Vector2(data.positionX, data.positionY));
            _personalId = data.id;

            RHRandom r = new RHRandom();
            foreach (WorkerData wData in data.Workers)
            {
                WorldAdventurer w = ObjectManager.GetWorker(wData.workerID, wData.name, wData.mood);
                AddWorker(w, r);
            }
        }
    }
}
