using RiverHollow.Characters.NPCs;
using RiverHollow.Game_Managers;
using RiverHollow.WorldObjects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using RiverHollow.Misc;
using RiverHollow.Buildings;
using static RiverHollow.Game_Managers.PlayerManager;
using static RiverHollow.Game_Managers.GameManager;
using static RiverHollow.WorldObjects.WorldItem;

namespace RiverHollow
{
    public class WorkerBuilding : Building
    {
        public enum WorkerTypeEnum { Magic, Craftsmen};
        private WorkerTypeEnum _buildingWorker;

        public bool _selected = false;

        protected int _iPersonalID;
        public int PersonalID { get => _iPersonalID; }

        #region Data Lists
        protected const int MaxWorkers = 9;
        protected List<WorldAdventurer> _workers;
        public List<WorldAdventurer> Workers { get => _workers; }

        protected Container _buildingChest;
        public Container BuildingChest { get => _buildingChest; set => _buildingChest = value; }

        protected Container _pantry;
        public Container Pantry { get => _pantry; set => _pantry = value; }

        protected List<WorldObject> _liPlacedObjects;
        public List<WorldObject> PlacedObjects { get => _liPlacedObjects; }
        #endregion

        public WorkerBuilding(string[] stringData, int id){
            Type = ObjectType.Building;
            int i = ImportBasics(stringData, id);
            _buildingWorker = (WorkerTypeEnum)Enum.Parse(typeof(WorkerTypeEnum), stringData[i++]);
            _iPersonalID = GetNewBuildingID();
            _workers = new List<WorldAdventurer>();
            _liPlacedObjects = new List<WorldObject>();

            _buildingChest = (Container)ObjectManager.GetWorldObject(190);
            _pantry = (Container)ObjectManager.GetWorldObject(190);

            _rSource = new Rectangle(0, 0, Texture.Width, Texture.Height);
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
                    w.MakeDailyItem();
                    //bool eaten = false;
                    //for (int i = 0; i < Pantry.Rows; i++)
                    //{
                    //    for (int j = 0; j < Pantry.Rows; j++)
                    //    {
                    //        Item item = Pantry.Inventory[i, j];
                    //        if (item != null && item.Type == Item.ItemType.Food)
                    //        {
                    //            Pantry.RemoveItemFromInventory(i, j);
                    //            w.MakeDailyItem();
                    //            eaten = true;
                    //            break;
                    //        }
                    //    }
                    //    if (!eaten)
                    //    {
                    //        break;
                    //    }
                    //}
                }
            }
        }

        public override void SetCoordinates(Vector2 position)
        {
            _vMapPosition = position;

            int startX = (int)_vMapPosition.X + (_entranceX * Size);
            int startY = (int)_vMapPosition.Y + (_entranceY * Size);

            _boxToEnter = new Rectangle(startX, startY, Size, Size);
            _leaveLocation = new Rectangle(_boxToEnter.X, _boxToEnter.Y + Size, Size, Size);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(_texture, new Rectangle((int)this.MapPosition.X, (int)this.MapPosition.Y, _texture.Width, _texture.Height), null, _selected ? Color.Green : Color.White, 0, new Vector2(0, 0), SpriteEffects.None, MapPosition.Y+Texture.Height);
        }

        public BuildingData SaveData()
        {
            BuildingData buildingData = new BuildingData
            {
                buildingID = this.ID,
                positionX = (int)this.MapPosition.X,
                positionY = (int)this.MapPosition.Y,
                id = this.PersonalID,

                Workers = new List<WorkerData>()
            };

            foreach (WorldAdventurer w in this.Workers)
            {
                buildingData.Workers.Add(w.SaveData());
            }

            buildingData.pantry = this.Pantry.SaveData();
            buildingData.buildingChest = this.BuildingChest.SaveData();

            buildingData.containers = new List<ContainerData>();
            buildingData.machines = new List<MachineData>();
            foreach (WorldObject w in _liPlacedObjects)
            {
                if (w.IsMachine())
                {
                    buildingData.machines.Add(((Machine)w).SaveData());
                }
                if (w.IsContainer())
                {
                    buildingData.containers.Add(((Container)w).SaveData());
                }
            }

            return buildingData;
        }
        public void LoadData(BuildingData data)
        {
            SetCoordinates(new Vector2(data.positionX, data.positionY));
            _iPersonalID = data.id;

            RHRandom r = new RHRandom();
            foreach (WorkerData wData in data.Workers)
            {
                WorldAdventurer w = ObjectManager.GetWorker(wData.workerID);
                w.LoadData(wData);
                AddWorker(w, r);
            }

            this.Pantry = (Container)ObjectManager.GetWorldObject(data.pantry.containerID);
            Pantry.LoadData(data.pantry);
            this.BuildingChest = (Container)ObjectManager.GetWorldObject(data.pantry.containerID);
            BuildingChest.LoadData(data.buildingChest);

            foreach (ContainerData c in data.containers)
            {
                Container con = (Container)ObjectManager.GetWorldObject(c.containerID);
                con.LoadData(c);
                _liPlacedObjects.Add(con);
            }

            foreach (MachineData mac in data.machines)
            {
                Machine theMachine = (Machine)ObjectManager.GetWorldObject(mac.ID);
                theMachine.LoadData(mac);
                _liPlacedObjects.Add(theMachine);
            }
        }
    }
}
