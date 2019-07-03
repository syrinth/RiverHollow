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
        public override int BaseWidth => _iBaseWidth * TileSize; //In Pixels
        public override int BaseHeight => _iBaseHeight * TileSize; //In Pixels
        public int PxWidth => _iWidth * TileSize;
        public int PxHeight => _iHeight * TileSize;

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

        protected Rectangle _rExit;
        public Rectangle BoxToExit { get => _rExit; }

        protected Rectangle _rEntrance;
        public Rectangle BoxToEnter { get => _rEntrance; }

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

        public Building(Dictionary<string, string> data, int id)
        {
            ImportBasics(data, id);
        }

        protected void ImportBasics(Dictionary<string, string> data, int id)
        {
            _id = id;
            GameContentManager.GetBuildingText(_id, ref _sName, ref _sDescription);

            _texture = GameContentManager.GetTexture(GameContentManager.BUILDING_FOLDER + data["Texture"]);

            //The dimensions of the Building in tiles
            string[] dimensions = data["Dimensions"].Split('-');
            _iWidth = int.Parse(dimensions[0]);
            _iHeight = int.Parse(dimensions[1]);

            //The top-left most tile that forms the base of the building
            string[] baseSq = data["FirstBase"].Split('-');
            _iBaseX = int.Parse(baseSq[0]);
            _iBaseY = int.Parse(baseSq[1]);

            //Width and Height of the building's base
            _iBaseWidth = int.Parse(data["Width"]);
            _iBaseHeight = int.Parse(data["Height"]);

            //The rectangle, in pixels, that forms the entrance to the building
            string[] ent = data["Entrance"].Split('-');
            _iEntX = int.Parse(ent[0]);
            _iEntY = int.Parse(ent[1]);
            _iEntWidth = int.Parse(ent[2]);
            _iEntHeight = int.Parse(ent[3]);

            //Worker data for the building, if appropriate
            if (data.ContainsKey("Workers"))
            {
                _iBldgLvl = 1;
                _bHoldsWorkers = true;
                _arrWorkerTypes = new int[2];

                string[] workerTypes = data["Workers"].Split('-');
                _arrWorkerTypes[0] = int.Parse(workerTypes[0]);
                _arrWorkerTypes[1] = int.Parse(workerTypes[1]);

                //These should only be present in worker buildings
                _buildingChest = (Container)ObjectManager.GetWorldObject(190);
                _pantry = (Container)ObjectManager.GetWorldObject(190);
            }

            //Default is 3, but some buildings may allow more or less
            if (data.ContainsKey("WorkersPerLevel")) { _iWorkersPerLevel = int.Parse(data["WorkersPerLevel"]); }

            //Flag for whether or not this building is the Manor
            _bManor = data.ContainsKey("Manor");

            _iPersonalID = PlayerManager.GetNewBuildingID();
            _liWorkers = new List<WorldAdventurer>();
            _liPlacedObjects = new List<WorldObject>();

            _rSource = new Rectangle(0, 0, PxWidth, PxHeight);
        }

        /// <summary>
        /// Gets the Building to draw itself, the source rectangle needs to move along the x-axis
        /// in order to get the proper building image. Building levels start at 1, so need to subtract 1
        /// in order to get a start position of 0,0
        /// </summary>
        /// <param name="spriteBatch"></param>
        public override void Draw(SpriteBatch spriteBatch)
        {
            Rectangle destRect = new Rectangle((int)this.MapPosition.X, (int)this.MapPosition.Y, PxWidth, PxHeight);
            spriteBatch.Draw(_texture, destRect, _rSource, _selected ? Color.Green : Color.White, 0, Vector2.Zero, SpriteEffects.None, MapPosition.Y + Texture.Height);
        }

        /// <summary>
        /// Generates a new Rectangle that represents the area of the building that is collidabke.
        /// </summary>
        public Rectangle GenerateCollisionBox()
        {
            //Start at the top left corner of the building, then move over and
            //down by the number of pixels required to get to the base.
            int startX = (int)_vMapPosition.X + (_iBaseX * TileSize);
            int startY = (int)_vMapPosition.Y + (_iBaseY * TileSize);

            return new Rectangle(startX, startY, BaseWidth, BaseHeight);
        }

        /// <summary>
        /// Sets the name of the building
        /// </summary>
        /// <param name="val">The name to assign</param>
        public void SetName(string val)
        {
            _sGivenName = val;
        }

        /// <summary>
        /// Sets up the map position of the map based off of its screen position. Called
        /// when placing the Building onto the map.
        /// </summary>
        /// <param name="position"></param>
        public override void SetCoordinatesByGrid(Vector2 position)
        {
            //Set the top-left corner of the building position 
            _vMapPosition = position;

            //Determine where the top-left corner of the entrance Rectangle should be
            int startX = (int)_vMapPosition.X + _iEntX;
            int startY = (int)_vMapPosition.Y + _iEntY;

            //Create the entrance and exit rectangles attached to the building
            _rEntrance = new Rectangle(startX, startY, _iEntWidth, _iEntHeight);
            _rExit = new Rectangle(_rEntrance.Left, _rEntrance.Bottom, TileSize, TileSize);
        }

        /// <summary>
        /// Checks to ensure that the building is valid for the worker type.
        /// Manor can hold all worker types.
        /// Array checks against the two types of workers the building can hold.
        /// </summary>
        /// <param name="w">The Adventurer to compare against.</param>
        /// <returns>True if the building will accept the type of Worker</returns>
        internal bool CanHold(WorldAdventurer w)
        {
            return _bManor || w.WorkerID == _arrWorkerTypes[0] || w.WorkerID == _arrWorkerTypes[1];
        }

        /// <summary>
        /// Checks against the size of the worker list
        /// </summary>
        /// <returns>True if the worker list isn't full yet.</returns>
        public bool HasSpace()
        {
            bool rv = false;

            rv = _liWorkers.Count < _iCurrWorkerMax;

            return rv;
        }

        /// <summary>
        /// Call to add the worker to the building. The # of workers must not
        /// exceed the max number of workers and the worker must be of the
        /// appropriate type.
        /// </summary>
        /// <param name="worker"></param>
        /// <returns>True if the worker has been successfully added</returns>
        public bool AddWorker(WorldAdventurer worker)
        {
            bool rv = false;

            if (worker != null && CanHold(worker) && HasSpace())
            {
                worker.SetBuilding(this);
                _liWorkers.Add(worker);

                rv = true;
            }

            return rv;
        }

        /// <summary>
        /// Call to remove a worker from the building
        /// </summary>
        /// <param name="worker"></param>
        public void RemoveWorker(WorldAdventurer worker)
        {
            _liWorkers.Remove(worker);
        }

        /// <summary>
        /// During rollover, the worker only makes their item if they were not
        /// adventuring that day, and if they are assigned to a production building.
        /// </summary>
        public void Rollover()
        {
            foreach (WorldAdventurer w in _liWorkers)
            {
                if (w.Rollover() && MapManager.Maps[MapName].Production)
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

        /// <summary>
        /// Increases the building level as long as it will not exceed the Building's max level
        /// Also move the source rectangle over by the width of the building to change exterior
        /// </summary>
        internal void Upgrade()
        {
            if (_iBldgLvl + 1 < MaxBldgLevel)
            {
                _iBldgLvl++;
                _rSource.X += PxWidth;
            }
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
            SetCoordinatesByGrid(new Vector2(data.positionX, data.positionY));
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
    }
}
