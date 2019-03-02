using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.Actors;
using RiverHollow.Game_Managers;
using RiverHollow.Misc;
using RiverHollow.WorldObjects;
using System.Collections.Generic;

using static RiverHollow.Game_Managers.GameManager;
using static RiverHollow.WorldObjects.WorldItem;

namespace RiverHollow.Buildings
{
    public class Building : WorldObject
    {
        protected int _iEntX;
        protected int _iEntY;
        protected int _iEntWidth;
        protected int _iEntHeight;
        protected int _iBaseX;
        protected int _iBaseY;
        protected int _iBaseWidth; //In Tiles
        public int BaseWidth { get => _iBaseWidth * TileSize; } //In Pixels
        protected int _iBaseHeight; //In Tiles
        public int BaseHeight { get => _iBaseHeight * TileSize; } //In Pixels

        protected int _iBldgLvl = 0;
        public int Level => _iBldgLvl;

        protected string _sDescription;
        public string Description => _sDescription;

        protected string _sName;
        public string Name => _sName;
        public string MapName => "map" +_sName.Replace(" ", "") + (_iBldgLvl == 0 ? "" : _iBldgLvl.ToString());

        protected string _sGivenName;
        public string GivenName=> _sGivenName;

        public override Rectangle CollisionBox { get => GenerateCollisionBox(); }
        public Rectangle SelectionBox { get => new Rectangle((int)MapPosition.X, (int)MapPosition.Y, _texture.Width, _texture.Height); }

        protected Rectangle _leaveLocation;
        public Rectangle BoxToExit { get => _leaveLocation; }

        protected Rectangle _boxToEnter;
        public Rectangle BoxToEnter { get => _boxToEnter; }

        protected int _iPersonalID;
        public int PersonalID { get => _iPersonalID; }

        protected bool _bManor;
        public bool IsManor => _bManor;

        #region Worker Info
        private bool _bHoldsWorkers;
        public bool HoldsWorkers => _bHoldsWorkers;
        private int[] _arrWorkerTypes;
        public bool _selected = false;

        protected int _iWorkersPerLevel = 3;
        protected int _iMaxWorkers = 9;
        protected int _iCurrWorkerMax => _iWorkersPerLevel * _iBldgLvl;
        public int MaxWorkers => _iCurrWorkerMax;

        protected List<WorldAdventurer> _liWorkers;
        public List<WorldAdventurer> Workers { get => _liWorkers; }

        protected Container _buildingChest;
        public Container BuildingChest { get => _buildingChest; set => _buildingChest = value; }

        protected Container _pantry;
        public Container Pantry { get => _pantry; set => _pantry = value; }

        protected List<WorldObject> _liPlacedObjects;
        public List<WorldObject> PlacedObjects { get => _liPlacedObjects; }
        #endregion

        public Building() { }

        public Building(string[] stringData, int id)
        {
            ImportBasics(stringData, id);
        }

        protected int ImportBasics(string[] stringData, int id)
        {
            _id = id;
            GameContentManager.GetBuildingText(_id, ref _sName, ref _sDescription);

            int i = 0;
            int totalCount = 0;
            for (; i < stringData.Length; i++)
            {
                string[] tagType = stringData[i].Split(':');
                if (tagType[0].Equals("Texture"))
                {
                    _texture = GameContentManager.GetTexture(GameContentManager.BUILDING_FOLDER + tagType[1]);
                    totalCount++;
                }
                else if (tagType[0].Equals("Dimensions"))
                {
                    string[] dimensions = tagType[1].Split('-');
                    _iWidth = int.Parse(dimensions[0]);
                    _iHeight = int.Parse(dimensions[1]);
                    totalCount++;
                }
                else if (tagType[0].Equals("FirstBase"))
                {
                    string[] baseSq = tagType[1].Split('-');
                    _iBaseX = int.Parse(baseSq[0]);
                    _iBaseY = int.Parse(baseSq[1]);
                    totalCount++;
                }
                else if (tagType[0].Equals("Width"))
                {
                    _iBaseWidth = int.Parse(tagType[1]);
                    totalCount++;
                }
                else if (tagType[0].Equals("Height"))
                {
                    _iBaseHeight = int.Parse(tagType[1]);
                    totalCount++;
                }
                else if (tagType[0].Equals("Entrance"))
                {
                    string[] ent = tagType[1].Split('-');
                    _iEntX = int.Parse(ent[0]);
                    _iEntY = int.Parse(ent[1]);
                    _iEntWidth = int.Parse(ent[2]);
                    _iEntHeight = int.Parse(ent[3]);
                    totalCount++;
                }
                else if (tagType[0].Equals("Manor"))
                {
                    _bManor = true;
                    totalCount++;
                }
                else if (tagType[0].Equals("Workers"))
                {
                    _iBldgLvl = 1;
                    _bHoldsWorkers = true;
                    _arrWorkerTypes = new int[2];

                    string[] workerTypes = tagType[1].Split('-');
                    _arrWorkerTypes[0] = int.Parse(workerTypes[0]);
                    _arrWorkerTypes[1] = int.Parse(workerTypes[1]);
                }
                else if (tagType[0].Equals("WorkersPerLevel"))
                {
                    _iWorkersPerLevel = int.Parse(tagType[1]);
                }

                _iPersonalID = PlayerManager.GetNewBuildingID();
                _liWorkers = new List<WorldAdventurer>();
                _liPlacedObjects = new List<WorldObject>();

                _buildingChest = (Container)ObjectManager.GetWorldObject(190);
                _pantry = (Container)ObjectManager.GetWorldObject(190);

                _rSource = new Rectangle(0, 0, Texture.Width, Texture.Height);
            }

            return i;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(_texture, new Rectangle((int)this.MapPosition.X, (int)this.MapPosition.Y, _texture.Width, _texture.Height), null, _selected ? Color.Green : Color.White, 0, Vector2.Zero, SpriteEffects.None, MapPosition.Y + Texture.Height);
        }

        public Rectangle GenerateCollisionBox()
        {
            int startX = (int)_vMapPosition.X + (_iBaseX * TileSize);
            int startY = (int)_vMapPosition.Y + (_iBaseY * TileSize);

            return new Rectangle(startX, startY,BaseWidth, BaseHeight);
        }

        public void SetName(string val)
        {
            _sGivenName = val;
        }

        public override void SetCoordinates(Vector2 position)
        {
            _vMapPosition = position;

            int startX = (int)_vMapPosition.X + _iEntX;
            int startY = (int)_vMapPosition.Y + _iEntY;

            _boxToEnter = new Rectangle(startX, startY, _iEntWidth, _iEntHeight);
            _leaveLocation = new Rectangle(_boxToEnter.X, _boxToEnter.Y + TileSize, TileSize, TileSize);
        }

        public bool HasSpace()
        {
            bool rv = false;

            rv = _liWorkers.Count < 9;

            return rv;
        }

        public bool AddWorker(WorldAdventurer worker)
        {
            bool rv = false;

            if (worker != null && _liWorkers.Count < _iCurrWorkerMax)
            {
                worker.SetBuilding(this);
                _liWorkers.Add(worker);

                rv = true;
            }

            return rv;
        }
        public void RemoveWorker(WorldAdventurer worker)
        {
            _liWorkers.Remove(worker);
        }

        public void Rollover()
        {
            if (MapManager.Maps[MapName].Production)
            {
                foreach (WorldAdventurer w in _liWorkers)
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
        }

        internal void Upgrade()
        {
            _iBldgLvl++;
        }

        public BuildingData SaveData()
        {
            BuildingData buildingData = new BuildingData
            {
                bldgLvl = this._iBldgLvl,
                buildingID = this.ID,
                positionX = (int)this.MapPosition.X,
                positionY = (int)this.MapPosition.Y,
                id = this.PersonalID,
                name = this._sGivenName,

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
            _iBldgLvl = data.bldgLvl == 0 ? 1 : data.bldgLvl;

            foreach (WorkerData wData in data.Workers)
            {
                WorldAdventurer w = ObjectManager.GetWorker(wData.workerID);
                w.LoadData(wData);
                AddWorker(w);
            }
            this._sGivenName = data.name;
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

        internal bool CanHold(WorldAdventurer w)
        {
            return w.AdventurerID == _arrWorkerTypes[0] || w.AdventurerID == _arrWorkerTypes[1];
        }
    }
}
