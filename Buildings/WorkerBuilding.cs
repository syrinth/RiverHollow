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
using RiverHollow.Buildings;

namespace RiverHollow
{
    public class WorkerBuilding : Building
    {
        public enum WorkerType { Magic, Craftsmen};
        private WorkerType _buildingWorker;

        public bool _selected = false;

        protected int _personalId;
        public int PersonalID { get => _personalId; }

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

        public WorkerBuilding(string[] stringData, int id){
            int i = ImportBasics(stringData, id);
            _buildingWorker = (WorkerType)Enum.Parse(typeof(WorkerType), stringData[i++]);
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

        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(_texture, new Rectangle((int)this.Position.X, (int)this.Position.Y, _texture.Width, _texture.Height), null, _selected ? Color.Green : Color.White, 0, new Vector2(0, 0), SpriteEffects.None, Position.Y+Texture.Height);
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
