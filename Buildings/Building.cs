using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.Actors;
using RiverHollow.Game_Managers;
using RiverHollow.Misc;
using RiverHollow.SpriteAnimations;
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
        public override int BaseWidth => _iBaseWidth;
        public override int BaseHeight => _iBaseHeight;

        protected int _iBldgLvl = 0;
        public int Level => _iBldgLvl;

        protected string _sDescription;
        public string Description => _sDescription;

        protected string _sHomeMap;
        protected string _sName;
        public string Name => _sName;
        public string MapName => "map" +_sName.Replace(" ", "") + (_iBldgLvl == 0 ? "" : _iBldgLvl.ToString());

        protected string _sGivenName;
        public string GivenName=> _sGivenName;

        public override Rectangle CollisionBox { get => GenerateCollisionBox(); }
        public Rectangle SelectionBox { get => new Rectangle((int)MapPosition.X, (int)MapPosition.Y, _sprite.Width, _sprite.Height); }

        protected Rectangle _rExit;
        public Rectangle BoxToExit { get => _rExit; }

        protected Rectangle _rEntrance;
        public Rectangle BoxToEnter { get => _rEntrance; }

        protected int _iPersonalID;
        public int PersonalID { get => _iPersonalID; }

        protected bool _bUnique;
        public bool Unique => _bUnique;
        protected bool _bManor;
        public bool IsManor => _bManor;

        protected int _iUpgradeTime;
        protected int _iUpgradeTimer;

        protected Vector2 _vecBuildspot;
        public Vector2 BuildFromPosition => _vecBuildspot;

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
        public List<WorldAdventurer> Workers => _liWorkers;

        protected Container _buildingChest;
        public Container BuildingChest { get => _buildingChest; set => _buildingChest = value; }

        protected Container _pantry;
        public Container Pantry { get => _pantry; set => _pantry = value; }

        protected List<WorldObject> _liPlacedObjects;
        public List<WorldObject> PlacedObjects => _liPlacedObjects;
        #endregion

        public Building(Dictionary<string, string> data, int id)
        {
            ImportBasics(data, id);
        }

        protected void ImportBasics(Dictionary<string, string> stringData, int id)
        {
            _id = id;
            GameContentManager.GetBuildingText(_id, ref _sName, ref _sDescription);

            //The dimensions of the Building in tiles
            string[] dimensions = stringData["Dimensions"].Split('-');
            _iWidth = int.Parse(dimensions[0]);
            _iHeight = int.Parse(dimensions[1]);

            //The top-left most tile that forms the base of the building
            string[] baseSq = stringData["FirstBase"].Split('-');
            _iBaseX = int.Parse(baseSq[0]);
            _iBaseY = int.Parse(baseSq[1]);

            //Width and Height of the building's base
            _iBaseWidth = int.Parse(stringData["Width"]);
            _iBaseHeight = int.Parse(stringData["Height"]);

            //The rectangle, in pixels, that forms the entrance to the building
            string[] ent = stringData["Entrance"].Split('-');
            _iEntX = int.Parse(ent[0]);
            _iEntY = int.Parse(ent[1]);
            _iEntWidth = int.Parse(ent[2]);
            _iEntHeight = int.Parse(ent[3]);

            //The amount of time it takes for a building to change stages
            if (stringData.ContainsKey("UpgradeTime")) { _iUpgradeTime = int.Parse(stringData["UpgradeTime"]); }

            //Sets the position from which the Mason will spawn tobuild the building
            if (stringData.ContainsKey("BuildSpot")) {
                string[] split = stringData["BuildSpot"].Split('-');
                _vecBuildspot = new Vector2(int.Parse(split[0]), int.Parse(split[1]));
            }

            //Worker data for the building, if appropriate
            if (stringData.ContainsKey("Workers"))
            {
                //Start level is 1 so that we display in built state
                _iBldgLvl = 1;
                _bHoldsWorkers = true;
                _arrWorkerTypes = new int[2];

                string[] workerTypes = stringData["Workers"].Split('-');
                _arrWorkerTypes[0] = int.Parse(workerTypes[0]);
                _arrWorkerTypes[1] = int.Parse(workerTypes[1]);

                //These should only be present in worker buildings
                _buildingChest = (Container)ObjectManager.GetWorldObject(190);
                _pantry = (Container)ObjectManager.GetWorldObject(190);
            }

            //Default is 3, but some buildings may allow more or less
            if (stringData.ContainsKey("WorkersPerLevel")) { _iWorkersPerLevel = int.Parse(stringData["WorkersPerLevel"]); }

            //Flag for whether or not this building is the Manor
            _bManor = stringData.ContainsKey("Manor");

            //Flag for whether or not this building is unique
            _bUnique = stringData.ContainsKey("Unique");

            _iPersonalID = PlayerManager.GetNewBuildingID();
            _liWorkers = new List<WorldAdventurer>();
            _liPlacedObjects = new List<WorldObject>();

            LoadSprite(stringData, GameContentManager.FOLDER_BUILDINGS + stringData["Texture"]);
        }

        protected override void LoadSprite(Dictionary<string, string> stringData, string textureName = "Textures\\worldObjects")
        {
            int startX = 0;
            int startY = 0;

            _sprite = new AnimatedSprite(textureName);
            for(int i = 0; i < MaxBldgLevel; i++)
            {
                _sprite.AddAnimation(i.ToString(), startX, startY, _iWidth, _iHeight);
                startX += _iWidth;
            }
            _sprite.SetCurrentAnimation(_iBldgLvl.ToString());
        }

        /// <summary>
        /// Gets the Building to draw itself, the source rectangle needs to move along the x-axis
        /// in order to get the proper building image
        /// </summary>
        /// <param name="spriteBatch"></param>
        public override void Draw(SpriteBatch spriteBatch)
        {
            _sprite.SetColor(_selected ? Color.Green : Color.White);
            base.Draw(spriteBatch);
        }

        /// <summary>
        /// Generates a new Rectangle that represents the area of the building that is collidabke.
        /// </summary>
        public Rectangle GenerateCollisionBox()
        {
            //Start at the top left corner of the building, then move over and
            //down by the number of pixels required to get to the base.
            int startX = (int)_vMapPosition.X + _iBaseX;
            int startY = (int)_vMapPosition.Y + _iBaseY;

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
            MapPosition = position;

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
            //If we are upgrading the building, get closer to the build count
            if (_iUpgradeTimer > 0) {
                _iUpgradeTimer--;

                //When the timer reaches 0, Upgrade the Building
                if (_iUpgradeTimer == 0)
                {
                    Upgrade();
                }
            }

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
        /// Sets the upgrade timer on the building so we know how long we have
        /// until we need to upgrade it to the next stage.
        /// 
        /// Increment it by one to account for the fact that the upgrade shouldn't
        /// officially start until the day after we call this method.
        /// 
        /// Parameter only used when building is first being built, otherwise leave
        /// the original building level.
        /// </summary>
        /// <param name="startAtZero"> Whether to reset the building's value to 0 or not</param>
        public void StartBuilding(bool startAtZero = true)
        {
            if (startAtZero) { _iBldgLvl = 0; }
            _iUpgradeTimer = _iUpgradeTime + 1;
            _sprite.SetCurrentAnimation(_iBldgLvl.ToString());

            GameManager.TownMason.SetBuildTarget(this);
        }

        /// <summary>
        /// Increases the building level as long as it will not exceed the Building's max level
        /// Also move the source rectangle over by the width of the building to change exterior and,
        /// if the building was at level 0, tell the map to create the building entrance.
        /// 
        /// Once we've finished upgrading the building, unset the Mason's build target.
        /// </summary>
        public void Upgrade()
        {
            if(_iBldgLvl == 0)
            {
                MapManager.Maps[_sHomeMap].CreateBuildingEntrance(this);
            }

            if (_iBldgLvl + 1 < MaxBldgLevel)
            {
                _iBldgLvl++;
            }

            _sprite.SetCurrentAnimation(_iBldgLvl.ToString());

            GameManager.TownMason.SetBuildTarget(null);
        }

        /// <summary>
        /// Sets the name of the map the building lives on
        /// </summary>
        /// <param name="name"></param>
        public void SetHomeMap(string name)
        {
            _sHomeMap = name;
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
            _iBldgLvl = data.bldgLvl;

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
