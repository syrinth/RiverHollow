using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.Characters;
using RiverHollow.Game_Managers;
using RiverHollow.GUIComponents.GUIObjects;
using RiverHollow.GUIComponents.MainObjects;
using RiverHollow.GUIComponents.Screens;
using RiverHollow.Misc;
using RiverHollow.SpriteAnimations;
using RiverHollow.Tile_Engine;
using RiverHollow.Utilities;
using System.Collections.Generic;
using static RiverHollow.Game_Managers.DataManager;
using static RiverHollow.Game_Managers.GameManager;
using static RiverHollow.Game_Managers.SaveManager;
using static RiverHollow.WorldObjects.Buildable.AdjustableObject;

namespace RiverHollow.WorldObjects
{
    public class WorldObject
    {
        #region Properties
        protected ObjectTypeEnum _eObjectType;
        public ObjectTypeEnum Type => _eObjectType;

        protected AnimatedSprite _sprite;
        public AnimatedSprite Sprite => _sprite;

        public List<RHTile> Tiles;

        protected bool AssignedToTiles => Tiles.Count > 0;

        protected string MapName => AssignedToTiles ? Tiles[0].MapName : string.Empty;
        public RHMap CurrentMap => AssignedToTiles ? MapManager.Maps[Tiles[0].MapName] : null;

        protected bool _bWalkable = false;
        public bool Walkable => _bWalkable;
        protected bool _bWallObject;
        public bool WallObject => _bWallObject;

        protected KeyValuePair<int, int> _kvpDrop; //itemID, # of items dropped

        protected bool _bDrawUnder = false;

        protected Point _pImagePos;
        public Vector2 PickupOffset { get; private set; }

        protected Vector2 _vMapPosition;
        public virtual Vector2 MapPosition => _vMapPosition;

        protected RHSize _uSize = new RHSize(1, 1);
        public int Width => _uSize.Width * TILE_SIZE;
        public int Height => _uSize.Height * TILE_SIZE;

        public int BaseWidth => _rBase.Width;
        public int BaseHeight => _rBase.Height;

        protected List<LightInfo> _liLights;
        public IList<LightInfo> Lights => _liLights.AsReadOnly();

        protected Rectangle _rBase = new Rectangle(0, 0, 1, 1);

        //The ClickBox is always the Sprite itself
        public Rectangle ClickBox => Util.FloatRectangle(MapPosition, _uSize);

        //Base is always described in # of Tiles so we must multiply by the TileSize
        public Rectangle CollisionBox => Util.FloatRectangle(MapPosition.X + (_rBase.X * TILE_SIZE), MapPosition.Y + (_rBase.Y * TILE_SIZE), (_rBase.Width * TILE_SIZE), (_rBase.Height * TILE_SIZE));

        protected int _iID;
        public int ID  => _iID;

        protected string _sName;
        public string Name => _sName;
        #endregion

        protected WorldObject(int id)
        {
            Tiles = new List<RHTile>();

            _iID = id;
            _bWallObject = false;

            DataManager.GetTextData("WorldObject", _iID, ref _sName, "Name");
        }

        public WorldObject(int id, Dictionary<string, string> stringData) : this(id)
        {
            LoadDictionaryData(stringData);
        }

        protected virtual void LoadDictionaryData(Dictionary<string, string> stringData, bool loadSprite = true)
        {
            Util.AssignValue(ref _pImagePos, "Image", stringData);

            Util.AssignValue(ref _uSize, "Size", stringData);

            Vector2 baseOffset = Vector2.Zero;
            Util.AssignValue(ref baseOffset, "BaseOffset", stringData);

            RHSize baseSize = new RHSize(1, 1);
            Util.AssignValue(ref baseSize, "Base", stringData);

            _rBase = Util.FloatRectangle(baseOffset, baseSize);

            Util.AssignValue(ref _eObjectType, "Type", stringData);
            Util.AssignValue(ref _bWallObject, "WallObject", stringData);

            if (stringData.ContainsKey("LightID"))
            {
                _liLights = new List<LightInfo>();

                foreach (string s in Util.FindParams(stringData["LightID"]))
                {
                    string[] split = s.Split('-');

                    LightInfo info;
                    info.LightObject = DataManager.GetLight(int.Parse(split[0]));
                    info.Offset = new Vector2(int.Parse(split[1]), int.Parse(split[2]));

                    SyncLightPositions();
                    _liLights.Add(info);
                }
            }

            if (loadSprite)
            {
                if (stringData.ContainsKey("Texture")) { LoadSprite(stringData, stringData["Texture"]); }
                else { LoadSprite(stringData); }
            }
        }

        protected virtual void LoadSprite(Dictionary<string, string> stringData, string textureName = DataManager.FILE_WORLDOBJECTS)
        {
            _sprite = new AnimatedSprite(textureName);
            if (stringData.ContainsKey("Idle"))
            {
                string[] idleSplit = stringData["Idle"].Split('-');
                _sprite.AddAnimation(AnimationEnum.ObjectIdle, _pImagePos.X, _pImagePos.Y, _uSize, int.Parse(idleSplit[0]), float.Parse(idleSplit[1]));
            }
            else
            {
                _sprite.AddAnimation(AnimationEnum.ObjectIdle, _pImagePos.X, _pImagePos.Y, _uSize);
            }

            //MAR
            //if (stringData.ContainsKey("Gathered"))
            //{
            //    string[] gatherSplit = stringData["Gathered"].Split('-');
            //    _sprite.AddAnimation(WorldObjAnimEnum.Gathered, startX, startY, _iWidth, _iHeight, int.Parse(gatherSplit[0]), float.Parse(gatherSplit[1]));
            //}
            SetSpritePos(_vMapPosition);
        }

        public virtual void Update(GameTime gTime) {
            _sprite.Update(gTime);
            SyncLightPositions();
            if (_liLights != null)
            {
                foreach (LightInfo info in _liLights)
                {
                    info.LightObject.Update(gTime);
                }
            }
        }

        public virtual void Draw(SpriteBatch spriteBatch)
        {
            if (_bDrawUnder) { _sprite.Draw(spriteBatch, 1); }
            else { _sprite.Draw(spriteBatch); }
        }

        public virtual void ProcessLeftClick() { }
        public virtual void ProcessRightClick() { }

        public virtual void Rollover() { }

        public virtual bool IntersectsWith(Rectangle r)
        {
            return CollisionBox.Intersects(r);
        }

        public virtual bool Contains(Point m)
        {
            return CollisionBox.Contains(m);
        }

        public virtual bool PlaceOnMap(RHMap map)
        {
            bool rv = PlaceOnMap(this.MapPosition, map);
            map.AddLights(GetLights());
            SyncLightPositions();
            return rv;
        }

        public virtual bool PlaceOnMap(Vector2 pos, RHMap map)
        {
            pos = new Vector2(pos.X - (_rBase.X * TILE_SIZE), pos.Y - (_rBase.Y * TILE_SIZE));
            SnapPositionToGrid(pos);
            return map.PlaceWorldObject(this);
        }

        protected void SetSpritePos(Vector2 position)
        {
            if (_sprite != null)
            {
                _sprite.Position = position;
            }
        }

        public virtual void SnapPositionToGrid(Point position) { SnapPositionToGrid(position.ToVector2()); }
        public virtual void SnapPositionToGrid(Vector2 position)
        {
            _vMapPosition = Util.SnapToGrid(position);
            SetSpritePos(_vMapPosition);
        }

        /// <summary>
        /// If the given RHTile is not present in the list of Tiles, add it
        /// </summary>
        /// <param name="t">The Tile to add to the list of known RHTiles</param>
        public void AddTile(RHTile t)
        {
            if (!Tiles.Contains(t))
            {
                Tiles.Add(t);
            }
        }

        /// <summary>
        /// Removes the object from the Tiles this Object sits upon
        /// then clears the Tile list that belongs to the WorldObject
        /// </summary>
        public virtual void RemoveSelfFromTiles()
        {
            foreach (RHTile t in Tiles)
            {
                if (t.Flooring == this) { t.RemoveFlooring(); }
                if (t.WorldObject == this) { t.RemoveWorldObject(); }
                if (t.ShadowObject == this) { t.RemoveShadowObject(); }
            }

            Tiles.Clear();
        }

        /// <summary>
        /// Sets the offset of the mouse based on the 
        /// </summary>
        /// <param name="mousePosition">The current mousePosition</param>
        public void SetPickupOffset(Vector2 mousePosition)
        {
            int xOffset = (Width > TILE_SIZE) ? (int)(mousePosition.X - _sprite.Position.X) : 0;
            int yOffset = (Height > TILE_SIZE) ? (int)(mousePosition.Y - _sprite.Position.Y) : 0;

            xOffset = (xOffset / TILE_SIZE) * TILE_SIZE;
            yOffset = (yOffset / TILE_SIZE) * TILE_SIZE;
            PickupOffset = new Vector2(xOffset, yOffset);
            
        }

        /// <summary>
        /// Sets the default PickupOffset if the Width of height
        /// is greater than a single RHTile
        /// </summary>
        public void SetPickupOffset()
        {
            int xOffset = (_rBase.Width > 1) ? (_rBase.Width - 1) / 2 : 0;
            int yOffset = (_rBase.Height > 1) ? (_rBase.Height -1) / 2 : 0;
            PickupOffset = new Vector2((_rBase.X + xOffset) * TILE_SIZE, (_rBase.Y + yOffset) * TILE_SIZE);
            PickupOffset = (PickupOffset / TILE_SIZE) * TILE_SIZE;
        }

        public List<Item> GetDroppedItems()
        {
            List<Item> itemList = new List<Item>();
            for (int i = 0; i < _kvpDrop.Value; i++)
            {
                itemList.Add(DataManager.GetItem(_kvpDrop.Key, 1));
            }

            return itemList;
        }

        public virtual List<Light> GetLights()
        {
            List<Light> lights = null;
            if (_liLights != null)
            {
                lights = new List<Light>();
                foreach (LightInfo info in _liLights)
                {
                    lights.Add(info.LightObject);
                }
            }

            return lights;
        }
        public virtual void SyncLightPositions()
        {
            if (_liLights != null)
            {
                foreach (LightInfo info in _liLights)
                {
                    info.LightObject.Position = new Vector2(MapPosition.X - info.LightObject.Width / 2, MapPosition.Y - info.LightObject.Height / 2);
                    info.LightObject.Position += info.Offset;
                }
            }
        }

        public bool CompareType(ObjectTypeEnum t) { return Type == t; }
        public bool IsDestructible() { return CompareType(ObjectTypeEnum.Destructible) || CompareType(ObjectTypeEnum.Plant); }
        public bool IsBuildable()
        {
            bool rv = false;
            switch (_eObjectType)
            {
                case ObjectTypeEnum.Beehive:
                case ObjectTypeEnum.Building:
                case ObjectTypeEnum.Buildable:
                case ObjectTypeEnum.Container:
                case ObjectTypeEnum.Decor:
                case ObjectTypeEnum.Floor:
                case ObjectTypeEnum.Garden:
                case ObjectTypeEnum.Mailbox:
                case ObjectTypeEnum.Structure:
                case ObjectTypeEnum.Wall:
                    rv = true;
                    break;
            }

            return rv;
        }

        public virtual bool CanPickUp() { return false; }

        public struct LightInfo
        {
            public Light LightObject;
            public Vector2 Offset;
        }
    }

    public class Destructible : WorldObject
    {
        protected int _iHP = 1;
        public int HP => _iHP;

        protected ToolEnum _eToolType;
        public ToolEnum NeededTool => _eToolType;

        protected int _lvltoDmg;
        public int LvlToDmg => _lvltoDmg;

        public Destructible(int id, Dictionary<string, string> stringData, bool loadSprite = true) : base(id)
        {
            LoadDictionaryData(stringData, loadSprite);

            _bWallObject = false;

            if (stringData.ContainsKey("ItemID")) {
                string[] split = stringData["ItemID"].Split('-');
                int itemID = int.Parse(split[0]);
                int num = 1;

                if (split.Length == 2) { num = int.Parse(split[1]); }
                _kvpDrop = new KeyValuePair<int, int>(itemID, num);
            }

            if (stringData.ContainsKey("Tool")) { _eToolType = Util.ParseEnum<ToolEnum>(stringData["Tool"]);}
            if (stringData.ContainsKey("Hp")) { _iHP = int.Parse(stringData["Hp"]); }
            if (stringData.ContainsKey("ReqLvl")) { _lvltoDmg = int.Parse(stringData["ReqLvl"]); }

            if (loadSprite && stringData.ContainsKey("DestructionAnim"))
            {
                string[] splitString = stringData["DestructionAnim"].Split('-');
                _sprite.AddAnimation(AnimationEnum.KO, int.Parse(splitString[0]), int.Parse(splitString[1]), TILE_SIZE, TILE_SIZE, int.Parse(splitString[2]), float.Parse(splitString[3]), false, true);
            }
        }

        public override void Update(GameTime gTime)
        {
            base.Update(gTime);
            if(_sprite.Position != _vMapPosition) { _sprite.Position = _vMapPosition; }
            if (_iHP <= 0)
            {
                if (!_sprite.ContainsAnimation(AnimationEnum.KO) || _sprite.AnimationFinished(AnimationEnum.KO))
                {
                    MapManager.Maps[Tiles[0].MapName].RemoveWorldObject(this);
                }
            }
        }

        public override void ProcessLeftClick() {
            if (_iHP > 0)
            {
                PlayerManager.SetTool(PlayerManager.RetrieveTool(NeededTool));
            }
        }

        public void DealDamage(int dmg)
        {
            if (_iHP > 0)
            {
                _iHP -= dmg;

                if (_iHP <= 0)
                {
                    _bWalkable = true;
                    _sprite.PlayAnimation(AnimationEnum.KO);

                    MapManager.DropItemsOnMap(GetDroppedItems(), CollisionBox.Location.ToVector2());
                }
            }

            //Nudge the Object in the direction of the 'attack'
            int xMod = 0, yMod = 0;
            if (PlayerManager.World.Facing == DirectionEnum.Left) { xMod = -1; }
            else if (PlayerManager.World.Facing == DirectionEnum.Right) { xMod = 1; }

            if (PlayerManager.World.Facing == DirectionEnum.Up) { yMod = -1; }
            else if (PlayerManager.World.Facing == DirectionEnum.Down) { yMod = 1; }
            
            _sprite.Position = new Vector2(_sprite.Position.X + xMod, _sprite.Position.Y + yMod);
        }
    }

    public class Plant : Destructible
    {
        #region consts
        const float MAX_ROTATION = 0.15f;
        const float ROTATION_MOD = 0.02f;
        const float MAX_BOUNCE = 3;
        #endregion

        bool _bShaking = false;
        DirectionEnum dir = DirectionEnum.Right;
        float _fCurrentRotation = 0f;
        int _iBounceCount = 0;

        readonly bool _bPopItem;
        int _iCurrentState;
        readonly int _iMaxStates;
        readonly int _iResourceID;
        int _iDaysLeft;
        Dictionary<int, int> _diTransitionTimes;
        int _iSeedID = 0;
        public int SeedID => _iSeedID;

        int _iHoneyID = -1;
        public int HoneyID => _iHoneyID;

        Garden _objGarden;
        SeasonEnum _eSeason;

        public Plant(int id, Dictionary<string, string> stringData) : base(id, stringData, false)
        {
            _diTransitionTimes = new Dictionary<int, int>();

            LoadDictionaryData(stringData);

            _bWalkable = true;

            _iCurrentState = 0;
            _rBase.Y = _uSize.Height - 1;

            Util.AssignValue(ref _eSeason, "Season", stringData);
            Util.AssignValue(ref _iHoneyID, "HoneyID", stringData);
            Util.AssignValue(ref _iSeedID, "SeedID", stringData);
            Util.AssignValue(ref _iResourceID, "ItemID", stringData);
            Util.AssignValue(ref _iMaxStates, "TrNum", stringData); //Number of growth phases

            _bPopItem = false;

            //The amount of time for each phase
            string[] dayStr = stringData["TrTime"].Split('-');
            for (int j = 0; j < _iMaxStates - 1; j++)
            {
                _diTransitionTimes.Add(j, int.Parse(dayStr[j]));
                _sprite.AddAnimation((j + 1).ToString(), _pImagePos.X + (TILE_SIZE * (j+1)), _pImagePos.Y, _uSize);
            }
            _iDaysLeft = _diTransitionTimes[0];

            if (stringData.ContainsKey("DestructionAnim"))
            {
                string[] splitString = stringData["DestructionAnim"].Split('-');
                _sprite.AddAnimation(AnimationEnum.KO, int.Parse(splitString[0]), int.Parse(splitString[1]), _uSize, int.Parse(splitString[2]), float.Parse(splitString[3]), false, true);
            }

            _sprite.SetRotationOrigin(new Vector2((_uSize.Width * TILE_SIZE) / 2, (_uSize.Height * TILE_SIZE) - 1));    //Subtract one to keep it in the bounds of the rectangle
        }

        protected override void LoadSprite(Dictionary<string, string> stringData, string textureName = DataManager.FILE_WORLDOBJECTS)
        {
            _sprite = new AnimatedSprite(DataManager.FILE_WORLDOBJECTS);
            _sprite.AddAnimation(0.ToString(), (int)_pImagePos.X, (int)_pImagePos.Y, _uSize);
            for (int j = 1; j < _diTransitionTimes.Count + 1; j++)
            {
                _sprite.AddAnimation(j.ToString(), (int)_pImagePos.X + (TILE_SIZE * j), (int)_pImagePos.Y, _uSize);
            }
        }

        public override void Update(GameTime gTime)
        {
            //If the object is shaking, we need to determine what step it's in
            if (_bShaking)
            {
                if (dir == DirectionEnum.Right) { _fCurrentRotation += ROTATION_MOD; }
                else if (dir == DirectionEnum.Left) { _fCurrentRotation -= ROTATION_MOD; }

                _sprite.SetRotationAngle(_fCurrentRotation);

                //If we've reached the end of our bounce, increment the bounce count
                //and set us to just below the trigger value for the statement we just hit.
                if (_iBounceCount == MAX_BOUNCE && _fCurrentRotation >= -ROTATION_MOD && _fCurrentRotation <= ROTATION_MOD)
                {
                    _bShaking = false;
                    _iBounceCount = 0;
                }
                else if (_fCurrentRotation >= MAX_ROTATION)
                {
                    dir = DirectionEnum.Left;
                    _iBounceCount++;
                    _fCurrentRotation = MAX_ROTATION - ROTATION_MOD;
                }
                else if (_fCurrentRotation <= -MAX_ROTATION)
                {
                    dir = DirectionEnum.Right;
                    _iBounceCount++;
                    _fCurrentRotation = -MAX_ROTATION + ROTATION_MOD;
                }
            }

            base.Update(gTime);
        }

        public override void ProcessLeftClick() {
            if (FinishedGrowing() && _iHP > 0)
            {
                Harvest();
            }
        }
        //public override void ProcessRightClick() { Harvest(); }

        /// <summary>
        /// Call to tell the plant that it is being Harvested, and follow any logic
        /// that needs to happen for this to occur.
        /// 
        /// Can only Harvest plants that are finished growing.
        /// </summary>
        public void Harvest()
        {
            if (NeededTool != ToolEnum.None) { PlayerManager.SetTool(PlayerManager.RetrieveTool(NeededTool)); }
            else
            {
                Item it = null;
                if (FinishedGrowing())
                {
                    PlayerManager.DecreaseStamina(1);
                    it = DataManager.GetItem(_iResourceID);
                    if (_bPopItem)
                    {
                        it.Pop(MapPosition);
                    }
                    else
                    {
                        InventoryManager.AddToInventory(it);
                    }

                    MapManager.RemoveWorldObject(this);
                    RemoveSelfFromTiles();
                    _objGarden.SetPlant(null);
                }
            }
        }

        /// <summary>
        /// Tell the object to shake
        /// </summary>
        public void Shake()
        {
            if (!_bShaking)
            {
                if (PlayerManager.World.CollisionBox.Center.X > CollisionBox.Center.X) { dir = DirectionEnum.Left; }
                else if (PlayerManager.World.CollisionBox.Center.X < CollisionBox.Center.X) { dir = DirectionEnum.Right; }
                _bShaking = true;
            }
        }

        /// <summary>
        /// On rollover, increase the plant's growth cycle if it has been watered.
        /// </summary>
        public override void Rollover()
        {
            if (_iDaysLeft > 0) //Decrement the number of days until the next phase
            {
                _iDaysLeft--;
            }
            else if (!FinishedGrowing()) //If it hasn't finished growing, and there's no days left, go to the next phase
            {
                _iCurrentState++;
                _sprite.PlayAnimation(_iCurrentState.ToString());
                if (_diTransitionTimes.ContainsKey(_iCurrentState))
                {
                    _iDaysLeft = _diTransitionTimes[_iCurrentState];
                }
            }
            else if(_objGarden == null)
            {
                CurrentMap.AddLights(GetLights());
            }
        }

        public bool InSeason() { return Util.GetEnumString(_eSeason).Equals(GameCalendar.GetSeason(GameCalendar.CurrentSeason)); }

        /// <summary>
        /// Check if the plant has finished growing or not.
        /// </summary>
        /// <returns>True if it's on the last phase</returns>
        public bool FinishedGrowing() { return _iCurrentState == _iMaxStates - 1; }

        public void FinishGrowth()
        {
            _iCurrentState = _iMaxStates - 1;
            //_rSource.X += _iWidth * _iCurrentState;
        }

        public override bool CanPickUp()
        {
            return FinishedGrowing();
        }

        public void SetGarden(Garden g)
        {
            _objGarden = g;
        }

        internal PlantData SaveData()
        {
            PlantData plantData = new PlantData
            {
                ID = _iID,
                x = CollisionBox.X,
                y = CollisionBox.Y,
                currentState = _iCurrentState,
                daysLeft = _iDaysLeft
            };

            return plantData;
        }

        internal void LoadData(PlantData data)
        {
            SnapPositionToGrid(new Vector2(data.x, data.y));
            _iCurrentState = data.currentState;
            _iDaysLeft = data.daysLeft;

            _sprite.PlayAnimation(_iCurrentState.ToString());
        }
    }

    public class Tree : Destructible
    {
        public Tree(int id, Dictionary<string, string> stringData) : base(id, stringData)
        {
            _eToolType = ToolEnum.Axe;
            LoadDictionaryData(stringData);
            _rBase.X = 1;
            _rBase.Y = 4;
        }
    }

    public class CombatHazard : WorldObject
    {
        public enum HazardTypeEnum { Passive, Timed, Triggered };
        readonly HazardTypeEnum _eHazardType;

        int _iInit;
        bool _bDrawOver;
        public int Damage { get; }
        public bool Active { get; private set; }

        public CombatHazard(int id, Dictionary<string, string> stringData) : base(id)
        {
            LoadDictionaryData(stringData);
            _eHazardType = Util.ParseEnum<HazardTypeEnum>(stringData["Subtype"]);
            Damage = int.Parse(stringData["Damage"]);
            Util.AssignValue(ref _bDrawOver, "DrawOver", stringData);
            _sprite.SetLayerDepthMod(_bDrawOver ? 1 : -999);

            _iInit = 0;
            if (_eHazardType == HazardTypeEnum.Passive) { Active = true; }
            else { Activate(false); }
        }

        protected override void LoadSprite(Dictionary<string, string> stringData, string textureName = DataManager.FILE_WORLDOBJECTS)
        {
            base.LoadSprite(stringData, textureName);
            _sprite.AddAnimation(AnimationEnum.Action_One, _pImagePos.X + TILE_SIZE, _pImagePos.Y, _uSize);
        }

        public bool Charge() {
            bool rv = false;

            _iInit += 3;

            if(_iInit >= 100)
            {
                _iInit = 0;
                Activate(!Active);
            }

            return rv;
        }

        private void Activate(bool value)
        {
            Active = value;
            _sprite.PlayAnimation(value ? AnimationEnum.Action_One : AnimationEnum.ObjectIdle);
        }

        public bool SubtypeMatch(HazardTypeEnum cmp)
        {
            return _eHazardType == cmp;
        }
    }

    //public class Staircase : WorldObject
    //{
    //    protected string _toMap;
    //    public string ToMap { get => _toMap; }

    //    public Staircase(int id, Vector2 pos, int width, int height) : base(id, pos, width, height)
    //    {
    //        _eObjectType = ObjectTypeEnum.WorldObject;
    //        _wallObject = true;
    //        _sprite = new AnimatedSprite(DataManager.FILE_WORLDOBJECTS);
    //        _sprite.AddAnimation(AnimationEnum.ObjectIdle, 96, 0, TileSize, TileSize);
    //    }

    //    public void SetExit(string map)
    //    {
    //        _toMap = map;
    //    }
    //}

    public class Gatherable : WorldObject
    {
        int _iItemID;

        public Gatherable(int id, Dictionary<string, string> stringData) : base(id)
        {
            Util.AssignValue(ref _iItemID, "ItemID", stringData);
            LoadDictionaryData(stringData);
        }

        public override void ProcessLeftClick() { Gather(); }
        public override void ProcessRightClick() { Gather(); }

        public void Gather()
        {
            PlayerManager.DecreaseStamina(1);
            InventoryManager.AddToInventory(DataManager.GetItem(_iItemID));
            MapManager.RemoveWorldObject(this);
            RemoveSelfFromTiles();
        }

        public override bool CanPickUp()
        {
            return true;
        }
    }

    /// <summary>
    /// Buildable represent WorldObjects that are built by the player
    /// </summary>
    public class Buildable : WorldObject
    {
        int _iValue = 0;
        public int Value => _iValue;

        protected Dictionary<int, int> _diReqToMake;
        public Dictionary<int, int> RequiredToMake => _diReqToMake;

        public bool OutsideOnly { get; private set; } = false;
        protected bool _bSelected = false;

        protected Buildable(int id) : base(id) { }

        public Buildable(int id, Dictionary<string, string> stringData) : base(id) {
            _rBase.Y = _uSize.Height - BaseHeight;
            LoadDictionaryData(stringData);
        }

        protected override void LoadDictionaryData(Dictionary<string, string> stringData, bool loadSprite = true)
        {
            base.LoadDictionaryData(stringData, loadSprite);

            Util.AssignValue(ref _iValue, "Value", stringData);
            Util.AssignValue(ref _diReqToMake, "ReqItems", stringData);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            _sprite.SetColor(_bSelected ? Color.Green : Color.White);
            base.Draw(spriteBatch);
        }

        public void SelectObject(bool val) { _bSelected = val; }

        public bool CanBuild()
        {
            return !OutsideOnly || (OutsideOnly && MapManager.CurrentMap.IsOutside);
        }

        public class Decor : Buildable
        {
            protected enum RotationalEnum { None, FourWay, TwoWay };
            protected RotationalEnum _eRotationType = RotationalEnum.None;

            protected DirectionEnum _eFacingDir = DirectionEnum.Down;
            public DirectionEnum Facing => _eFacingDir;

            protected Vector2 _vDisplayOffset = Vector2.Zero;
            protected Vector2 _vRotatedDisplayOffset = Vector2.Zero;

            protected int _iRotationBaseOffsetX;
            protected int _iRotationBaseOffsetY;
            protected RHSize _uRotationSize;

            private readonly bool _bDisplaysObject = false;
            public bool CanDisplay => _bDisplaysObject;

            private readonly bool _bCanBeDisplayed = false;
            public bool CanBeDisplayed => _bCanBeDisplayed;
            private Item _itemDisplay;
            private Decor _objDisplay;
            public bool HasDisplay => _objDisplay != null || _itemDisplay != null;

            public Decor (int id, Dictionary<string, string> stringData) : base(id, stringData)
            {
                _eObjectType = ObjectTypeEnum.Decor;
                Util.AssignValue(ref _eRotationType, "Rotation", stringData);
                Util.AssignValues(ref _iRotationBaseOffsetX, ref _iRotationBaseOffsetY, "RotationBaseOffset", stringData);
                Util.AssignValue(ref _uRotationSize, "RotationSize", stringData);
                Util.AssignValue(ref _bDisplaysObject, "Display", stringData);
                Util.AssignValue(ref _bCanBeDisplayed, "CanBeDisplayed", stringData);
                Util.AssignValue(ref _vDisplayOffset, "DisplayOffset", stringData);
                Util.AssignValue(ref _vRotatedDisplayOffset, "RotatedDisplayOffset", stringData); 
            }

            public override void Draw(SpriteBatch spriteBatch)
            {
                base.Draw(spriteBatch);
                if (_objDisplay != null)
                {
                    _objDisplay.Sprite.SetColor(_bSelected ? Color.Green : Color.White);
                    _objDisplay.Sprite.Draw(spriteBatch, _sprite.LayerDepth + 1);
                }
                else if(_itemDisplay != null)
                {
                    //Because Items don't exist directly on the map, we only need to tell it where to draw itself here
                    _itemDisplay.SetColor(_bSelected ? Color.Green : Color.White);
                    _itemDisplay.Draw(spriteBatch, new Rectangle((int)(_vMapPosition.X + _vDisplayOffset.X), (int)(_vMapPosition.Y + _vDisplayOffset.Y), TILE_SIZE, TILE_SIZE), true, _sprite.LayerDepth + 1);
                }
            }

            /// <summary>
            /// Handler for when a Decor object hasbeen right-clicked
            /// </summary>
            public override void ProcessRightClick()
            {
                //Currently, only display Decor objects can be interacted with.
                if (CanDisplay)
                {
                    GameManager.CurrentWorldObject = this;
                    GUIManager.OpenMainObject(new HUDInventoryDisplay());
                }
            }

            /// <summary>
            /// When snapping, we need to call SyncDisplayObject to make sure it
            /// matches the new position
            /// </summary>
            /// <param name="position">The position to snap to.</param>
            public override void SnapPositionToGrid(Vector2 position)
            {
                base.SnapPositionToGrid(position);
                SyncDisplayObject();
            }

            /// <summary>
            /// This override handles the situation where we are attempting to place a decor object
            /// on top of another one.
            /// 
            /// If we are not attempting to make a valid display placement, call the vase PlaceOnMap method
            /// and then sync any display object we may have.
            /// </summary>
            /// <param name="map">The map to place the object on</param>
            /// <returns></returns>
            public override bool PlaceOnMap(RHMap map)
            {
                bool rv = false;

                RHTile tile = map.GetTileByPixelPosition(_vMapPosition);
                if (tile.CanPlaceOnTabletop(this)) {
                    rv = ((Decor)tile.WorldObject).SetDisplayObject(this);
                }
                else {
                    rv = base.PlaceOnMap(map);
                    SyncDisplayObject();
                }

                return rv;
            }

            /// <summary>
            /// Assuming the object is capable of rotation, this method does the math
            /// required to change the sprite and base tiles accordingly.
            /// </summary>
            public void Rotate()
            {
                if (_eRotationType != RotationalEnum.None)
                {
                    //We don't need to do any swaps if the object has the same base and height
                    if (_rBase.Width != _rBase.Height)
                    {
                        Vector2 temp = _vDisplayOffset;
                        _vDisplayOffset = _vRotatedDisplayOffset;
                        _vRotatedDisplayOffset = temp;

                        Util.SwitchValues(ref _rBase.Width, ref _rBase.Height);
                        Util.SwitchValues(ref _uSize, ref _uRotationSize);
                        Util.SwitchValues(ref _rBase.X, ref _iRotationBaseOffsetX);
                        Util.SwitchValues(ref _rBase.Y, ref _iRotationBaseOffsetY);
                    }

                    Rectangle spriteFrameRectangle = _sprite.CurrentFrameAnimation.FrameRectangle;
                    Point newImage = spriteFrameRectangle.Location + new Point(spriteFrameRectangle.Width, 0);

                    //Direction handling for the different rotation types
                    if (_eRotationType == RotationalEnum.FourWay)
                    {
                        switch (_eFacingDir)
                        {
                            case DirectionEnum.Down:
                                _eFacingDir = DirectionEnum.Right;
                                break;
                            case DirectionEnum.Right:
                                _eFacingDir = DirectionEnum.Up;
                                break;
                            case DirectionEnum.Up:
                                _eFacingDir = DirectionEnum.Left;
                                break;
                            case DirectionEnum.Left:
                                newImage = _pImagePos;
                                _eFacingDir = DirectionEnum.Down;
                                break;
                        }
                    }
                    else if (_eRotationType == RotationalEnum.TwoWay)
                    {
                        switch (_eFacingDir)
                        {
                            case DirectionEnum.Down:
                                _eFacingDir = DirectionEnum.Right;
                                break;
                            case DirectionEnum.Right:
                                newImage = _pImagePos;
                                _eFacingDir = DirectionEnum.Down;
                                break;
                        }
                    }

                    //Updates the sprite info
                    _sprite = new AnimatedSprite(DataManager.FILE_WORLDOBJECTS);
                    _sprite.AddAnimation(AnimationEnum.ObjectIdle, newImage.X, newImage.Y, _uSize);
                    SetSpritePos(_vMapPosition);

                    //Sets the pickup offset to the center of the object.
                    SetPickupOffset();

                    //Important to not have a flicker before the game asserts where the obeject's new location is
                    GUICursor.UpdateTownObjectLocation();
                }
            }

            /// <summary>
            /// Helper methods for loading data. Just keeps rotating in sequence
            /// until we get to the appropriate Facing direction.
            /// </summary>
            /// <param name="dir"></param>
            public void RotateToDirection(DirectionEnum dir) { RotateToDirection((int)dir); }
            private void RotateToDirection(int dir)
            {
                for (int i = 0; i < dir; i++)
                {
                    Rotate();
                }
            }

            /// <summary>
            /// Handler for removing, and not swapping out, the display entity.
            /// If we're in destroy mode, destroy the display object. Otherwise, send
            /// the entity to storage
            /// </summary>
            public void RemoveDisplayEntity()
            {
                if (_objDisplay != null && GameManager.TownModeDestroy())
                {
                    foreach (KeyValuePair<int, int> kvp in _objDisplay.RequiredToMake)
                    {
                        InventoryManager.AddToInventory(kvp.Key, kvp.Value);
                    }
                }
                else { StoreDisplayEntity(); }
            }

            /// <summary>
            /// Sets the display Decor object, swaps out any pre-existing display entity
            /// for the given Decor.
            /// </summary>
            /// <param name="obj">The Decor object to display</param>
            public bool SetDisplayObject(Decor obj)
            {
                bool rv = false;
                if (StoreDisplayEntity())
                {
                    rv = true;
                    _objDisplay = obj;
                    SyncDisplayObject();
                }

                return rv;
            }

            /// <summary>
            /// Sets the display Item, swaps out any pre-existing display entity
            /// for the given item.
            /// </summary>
            /// <param name="it">The Item object to display</param>
            public void SetDisplayItem(Item it)
            {
                if (StoreDisplayEntity())
                {
                    _itemDisplay = DataManager.GetItem(it.ItemID);
                    InventoryManager.RemoveItemsFromInventory(it.ItemID, 1);
                    GUIManager.CloseMainObject();
                }
            }

            /// <summary>
            /// This method sends any display entity to the appropriate storage 
            /// and blanks out its reference on the Displaying Decor.
            /// 
            /// Only store entity if there is space in storage
            /// </summary>
            /// <returns>True as long as there is space in storage</returns>
            private bool StoreDisplayEntity() {
                bool rv = true;
                if (_itemDisplay != null) {
                    if (InventoryManager.HasSpaceInInventory(_itemDisplay.ItemID, 1))
                    {
                        InventoryManager.AddToInventory(_itemDisplay);
                        _itemDisplay = null;
                    }
                    else { rv = false; }
                }
                if (_objDisplay != null) {
                    PlayerManager.AddToStorage(_objDisplay.ID);
                    _objDisplay = null;
                }

                return rv;
            }

            /// <summary>
            /// This method ensures that the DisplayObject's location is always synced up relative to the Decor object it's placed on.
            /// </summary>
            private void SyncDisplayObject()
            {
                if (_objDisplay != null)
                {
                    _objDisplay.SnapPositionToGrid(new Vector2(_vMapPosition.X, _vMapPosition.Y - (_objDisplay.Sprite.Height - TILE_SIZE)));
                    _objDisplay._vMapPosition += _vDisplayOffset;
                    _objDisplay.SetSpritePos(_objDisplay._vMapPosition);
                }
            }

            internal DecorData SaveData()
            {
                DecorData data = new DecorData
                {
                    ID = _iID,
                    x = CollisionBox.X,
                    y = CollisionBox.Y,
                    dir = (int)_eFacingDir,
                    objDisplayID = _objDisplay == null ? -1 : _objDisplay.ID,
                    itemDisplayID = _itemDisplay == null ? -1 : _itemDisplay.ItemID,
                };

                return data;
            }

            internal void LoadData(DecorData data)
            {
                SnapPositionToGrid(new Vector2(data.x, data.y));
                RotateToDirection(data.dir);

                if(data.objDisplayID != -1) { SetDisplayObject((Decor)DataManager.GetWorldObjectByID(data.objDisplayID)); }
                if(data.itemDisplayID != -1) { SetDisplayItem(DataManager.GetItem(data.itemDisplayID)); }
            }
        }

        public class Structure : Buildable
        {
            List<SubObjectInfo> _liSubObjectInfo;
            Vector2 _vecSpecialCoords = Vector2.Zero;
            public Structure(int id, Dictionary<string, string> stringData) : base (id, stringData)
            {
                _liSubObjectInfo = new List<SubObjectInfo>();

                Util.AssignValue(ref _vecSpecialCoords, "SpecialCoords", stringData);

                if (stringData.ContainsKey("SubObjects"))
                {
                    foreach(string s in Util.FindParams(stringData["SubObjects"]))
                    {
                        string[] split = s.Split('-');
                        _liSubObjectInfo.Add(new SubObjectInfo() { ObjectID = int.Parse(split[0]), Position = new Vector2(int.Parse(split[1]), int.Parse(split[2])) });
                    }
                }

                _bWalkable = true;
                _bDrawUnder = true;
            }

            public override bool PlaceOnMap(Vector2 pos, RHMap map)
            {
                bool rv = false;
                if (base.PlaceOnMap(pos, map)) {
                    rv = true;
                    if (_iID == int.Parse(DataManager.Config[15]["ObjectID"]))
                    {
                        GameManager.MarketPosition = new Vector2(pos.X + _vecSpecialCoords.X, pos.Y + _vecSpecialCoords.Y);
                        foreach (Merchant m in DIMerchants.Values)
                        {
                            if (m.OnTheMap)
                            {
                                m.MoveToSpawn();
                            }
                        }
                    }

                    foreach (SubObjectInfo info in _liSubObjectInfo)
                    {
                        WorldObject obj = DataManager.GetWorldObjectByID(info.ObjectID);
                        RHTile targetTile = MapManager.Maps[MapName].GetTileByPixelPosition(new Vector2(pos.X + info.Position.X, pos.Y + info.Position.Y));
                        targetTile.RemoveWorldObject();
                        obj.PlaceOnMap(targetTile.Position, MapManager.Maps[MapName]);
                    }
                }

                return rv;
            }

            public override void RemoveSelfFromTiles()
            {
                foreach (SubObjectInfo info in _liSubObjectInfo)
                {
                    RHTile targetTile = MapManager.Maps[MapName].GetTileByPixelPosition(new Vector2(_vMapPosition.X + info.Position.X, _vMapPosition.Y + info.Position.Y));
                    if (targetTile.WorldObject != null)
                    {
                        targetTile.WorldObject.Sprite.Drawing = false;
                        MapManager.Maps[MapName].RemoveWorldObject(targetTile.WorldObject);
                    }
                }
                base.RemoveSelfFromTiles();
            }

            private struct SubObjectInfo
            {
                public int ObjectID;
                public Vector2 Position;
            }
        }

        public class Container : Buildable
        {
            public int Rows { get; }
            public int Columns { get; }
            public Item[,] Inventory { get; }

            public Container(int id, Dictionary<string, string> stringData) : base(id, stringData)
            {
                Rows = int.Parse(stringData["Rows"]);
                Columns = int.Parse(stringData["Cols"]);

                Inventory = new Item[Rows, Columns];
            }

            public override void ProcessRightClick()
            {
                GUIManager.OpenMainObject(new HUDInventoryDisplay(Inventory));
            }

            internal ContainerData SaveData()
            {
                ContainerData containerData = new ContainerData
                {
                    containerID = this.ID,
                    rows = Rows,
                    cols = Columns,
                    x = (int)this.MapPosition.X,
                    y = (int)this.MapPosition.Y
                };

                containerData.Items = new List<ItemData>();
                foreach (Item i in (this.Inventory))
                {
                    ItemData itemData = Item.SaveData(i);
                    containerData.Items.Add(itemData);
                }
                return containerData;
            }
            internal void LoadData(ContainerData data)
            {
                SnapPositionToGrid(new Vector2(data.x, data.y));
                for (int i = 0; i < Rows; i++)
                {
                    for (int j = 0; j < Columns; j++)
                    {
                        ItemData item = data.Items[i * InventoryManager.maxItemRows + j];
                        Item newItem = GetItem(item.itemID, item.num);
                        if (newItem != null) { newItem.ApplyUniqueData(item.strData); }

                        InventoryManager.InitContainerInventory(this.Inventory);
                        InventoryManager.AddItemToInventorySpot(newItem, i, j, false);
                        InventoryManager.ClearExtraInventory();
                    }
                }
            }
        }

        public class Mailbox : Buildable
        {
            private AnimatedSprite _alertSprite;
            private List<string> _liCurrentMessages;
            private List<string> _liSentMessages;

            public Mailbox(int id, Dictionary<string, string> stringData) : base(id, stringData)
            {
                _liCurrentMessages = new List<string>();
                _liSentMessages = new List<string>();
                PlayerManager.PlayerMailbox = this;

                _rBase.Y = _uSize.Height- BaseHeight;
            }

            public override void Update(GameTime gTime)
            {
                base.Update(gTime);
                _alertSprite?.Update(gTime);
            }
            public override void Draw(SpriteBatch spriteBatch)
            {
                base.Draw(spriteBatch);
                _alertSprite?.Draw(spriteBatch, 99999);
            }

            public override void ProcessRightClick()
            {
                TakeMessage();
            }

            public void SendMessage(string messageID)
            {
                _liSentMessages.Add(messageID);
            }

            public void TakeMessage()
            {
                if (_liCurrentMessages.Count > 0)
                {
                    TextEntry tEntry = DataManager.GetMailboxMessage(_liCurrentMessages[0]);
                    _liCurrentMessages.RemoveAt(0);

                    if (_liCurrentMessages.Count == 0)
                    {
                        _alertSprite = null;
                    }

                    GUIManager.OpenTextWindow(tEntry);
                }
            }

            public override void Rollover()
            {
                foreach (string strID in _liSentMessages)
                {
                    _liCurrentMessages.Add(strID);
                }
                _liSentMessages.Clear();

                if (_liCurrentMessages.Count > 0)
                {
                    _alertSprite = new AnimatedSprite(DataManager.DIALOGUE_TEXTURE);
                    _alertSprite.AddAnimation(AnimationEnum.ObjectIdle, 64, 64, TILE_SIZE, TILE_SIZE, 3, 0.150f, true);
                    _alertSprite.Position = new Vector2(_vMapPosition.X, _vMapPosition.Y - TILE_SIZE);
                }
            }

            public MailboxData SaveData()
            {
                MailboxData data = new MailboxData();
                data.MailboxMessages = new List<string>();
                foreach (string strID in _liCurrentMessages)
                {
                    data.MailboxMessages.Add(strID);
                }

                return data;
            }
            public void LoadData(MailboxData data)
            {
                foreach (string strID in data.MailboxMessages)
                {
                    _liSentMessages.Add(strID);
                }

                Rollover();
            }
        }

        public class Beehive : Buildable
        {
            int _iPeriod = -1;
            int _iDaysToHoney = -1;
            int _iItemID = -1;

            int _iHoneyToGather = -1;
            bool _bReady = false;
            public Beehive(int id, Dictionary<string, string> stringData) : base(id, stringData)
            {
                _sprite.AddAnimation(AnimationEnum.Action_Finished, _pImagePos.X + TILE_SIZE, _pImagePos.Y, _uSize);

                Util.AssignValue(ref _iItemID, "ItemID", stringData);
                Util.AssignValue(ref _iPeriod, "Period", stringData);
                _iDaysToHoney = _iPeriod;
            }

            public override void ProcessRightClick()
            {
                if (_bReady)
                {
                    InventoryManager.AddToInventory(_iHoneyToGather);
                    _bReady = false;
                    _iDaysToHoney = _iPeriod;
                    _iHoneyToGather = -1;
                    _sprite.PlayAnimation(AnimationEnum.ObjectIdle);
                }
            }

            public override void Rollover()
            {
                if (_iDaysToHoney == 0 && !_bReady)
                {
                    RHTile closestFlowerTile = Tiles[0];
                    foreach (RHTile t in MapManager.Maps[Tiles[0].MapName].GetAllTilesInRange(Tiles[0], 7)){
                        if(t.WorldObject!= null && t.WorldObject.CompareType(ObjectTypeEnum.Garden))
                        {
                            Plant p = ((Garden)t.WorldObject).GetPlant();
                            if(p!= null && p.HoneyID != -1 && p.FinishedGrowing() &&  (closestFlowerTile == Tiles[0] || Util.GetRHTileDelta(Tiles[0], t) < Util.GetRHTileDelta(Tiles[0], closestFlowerTile)))
                            {
                                closestFlowerTile = t;
                            }
                        }
                    }

                    if (closestFlowerTile == Tiles[0]) { _iHoneyToGather = _iItemID; }
                    else { _iHoneyToGather = ((Garden)closestFlowerTile.WorldObject).GetPlant().HoneyID; }

                    _bReady = true;
                    _sprite.PlayAnimation(AnimationEnum.Action_Finished);
                }
                else
                {
                    _iDaysToHoney--;
                }
            }

            public BeehiveData SaveData()
            {
                BeehiveData data = new BeehiveData
                {
                    ID = this.ID,
                    x = (int)this.CollisionBox.X,
                    y = (int)this.CollisionBox.Y,
                    timeLeft = this._iDaysToHoney,
                    ready = this._bReady,
                    honeyType = _bReady ? _iHoneyToGather : -1
                };

                return data;
            }
            public void LoadData(BeehiveData data)
            {
                _iID = data.ID;
                SnapPositionToGrid(new Vector2(data.x, data.y));
                _bReady = data.ready;
                _iDaysToHoney = data.timeLeft;
                _iHoneyToGather = data.honeyType;
                
                if(_iHoneyToGather != -1)
                {
                    _bReady = true;
                    _sprite.PlayAnimation(AnimationEnum.Action_Finished);
                }
            }
        }

        public class Wallpaper : Buildable
        {
            public Wallpaper(int id, Dictionary<string, string> stringData) : base(id, stringData)
            {
                _bWallObject = true;
            }
        }

        public abstract class AdjustableObject : Buildable
        {
            public AdjustableObject(int id) : base(id) { }

            /// <summary>
            /// Loads in the different sprite versions required for an AdjustableObject
            /// so that they can be easily played and referenced in the future.
            /// </summary>
            /// <param name="sprite">The AnimatedSprite to load the animations into</param>
            /// <param name="vStart">The source position for this texture series</param>
            protected void LoadAdjustableSprite(ref AnimatedSprite spr, string textureName = DataManager.FILE_FLOORING)
            {
                spr = new AnimatedSprite(textureName);
                spr.AddAnimation("None", (int)_pImagePos.X, (int)_pImagePos.Y, _uSize);
                spr.AddAnimation("NS", (int)_pImagePos.X + TILE_SIZE, (int)_pImagePos.Y, _uSize);
                spr.AddAnimation("EW", (int)_pImagePos.X + TILE_SIZE * 2, (int)_pImagePos.Y, _uSize);
                spr.AddAnimation("SW", (int)_pImagePos.X + TILE_SIZE * 3, (int)_pImagePos.Y, _uSize);
                spr.AddAnimation("NW", (int)_pImagePos.X + TILE_SIZE * 4, (int)_pImagePos.Y, _uSize);
                spr.AddAnimation("NE", (int)_pImagePos.X + TILE_SIZE * 5, (int)_pImagePos.Y, _uSize);
                spr.AddAnimation("SE", (int)_pImagePos.X + TILE_SIZE * 6, (int)_pImagePos.Y, _uSize);
                spr.AddAnimation("NSE", (int)_pImagePos.X + TILE_SIZE * 7, (int)_pImagePos.Y, _uSize);
                spr.AddAnimation("NSW", (int)_pImagePos.X + TILE_SIZE * 8, (int)_pImagePos.Y, _uSize);
                spr.AddAnimation("NEW", (int)_pImagePos.X + TILE_SIZE * 9, (int)_pImagePos.Y, _uSize);
                spr.AddAnimation("SEW", (int)_pImagePos.X + TILE_SIZE * 10, (int)_pImagePos.Y, _uSize);
                spr.AddAnimation("NSEW", (int)_pImagePos.X + TILE_SIZE * 11, (int)_pImagePos.Y, _uSize);
                spr.AddAnimation("W", (int)_pImagePos.X + TILE_SIZE * 12, (int)_pImagePos.Y, _uSize);
                spr.AddAnimation("E", (int)_pImagePos.X + TILE_SIZE * 13, (int)_pImagePos.Y, _uSize);
                spr.AddAnimation("S", (int)_pImagePos.X + TILE_SIZE * 14, (int)_pImagePos.Y, _uSize);
                spr.AddAnimation("N", (int)_pImagePos.X + TILE_SIZE * 15, (int)_pImagePos.Y, _uSize);
            }

            public override bool PlaceOnMap(Vector2 pos, RHMap map)
            {
                bool rv = false;
                if (base.PlaceOnMap(pos, map))
                {
                    rv = true;
                    AdjustObject();
                }
                return rv;
            }

            #region Adjustment
            /// <summary>
            /// Calls the AdjustmentHelper on the main base RHTile after first
            /// removing it from the map.
            /// </summary>
            public override void RemoveSelfFromTiles()
            {
                if (Tiles.Count > 0)
                {
                    RHTile startTile = Tiles[0];
                    base.RemoveSelfFromTiles();
                    AdjustmentHelper(startTile);
                }
            }

            /// <summary>
            /// Calls the AdjustmentHelper on the main base RHTile
            /// </summary>
            public void AdjustObject()
            {
                AdjustmentHelper(Tiles[0]);                
            }

            /// <summary>
            /// Adjusts the source rectangle for the AdjustableObject compared to nearby AdjustableObjects
            /// 
            /// First thing to do is to determine how many AdjustableObjects are adjacent to the AdjustableObject and where, we then
            /// create a string in NSEW order to determine which segment we need to get.
            /// 
            /// Then compare the string and determine which piece it corresponds to.
            /// 
            /// Finally, run this method again on each of the adjacent AdjustableObjects to update their appearance
            /// </summary>
            /// <param name="startTile">The RHTile to center the adjustments on.</param>
            /// <param name="adjustAdjacent">Whether or not to call this method against the adjacent tiles</param>
            protected virtual void AdjustmentHelper(RHTile startTile, bool adjustAdjacent = true)
            {
                string mapName = startTile.MapName;
                string sAdjacent = string.Empty;
                List<RHTile> liAdjacentTiles = new List<RHTile>();

                //Create the adjacent tiles string
                MakeAdjustments("N", ref sAdjacent, ref liAdjacentTiles, MapManager.Maps[mapName].GetTileByGridCoords(new Point((int)(startTile.X), (int)(startTile.Y - 1))));
                MakeAdjustments("S", ref sAdjacent, ref liAdjacentTiles, MapManager.Maps[mapName].GetTileByGridCoords(new Point((int)(startTile.X), (int)(startTile.Y + 1))));
                MakeAdjustments("E", ref sAdjacent, ref liAdjacentTiles, MapManager.Maps[mapName].GetTileByGridCoords(new Point((int)(startTile.X + 1), (int)(startTile.Y))));
                MakeAdjustments("W", ref sAdjacent, ref liAdjacentTiles, MapManager.Maps[mapName].GetTileByGridCoords(new Point((int)(startTile.X - 1), (int)(startTile.Y))));

                _sprite.PlayAnimation(string.IsNullOrEmpty(sAdjacent) ? "None" : sAdjacent);

                //Find all matching objects in the adjacent tiles and call
                //this method without recursion on them.
                if (adjustAdjacent)
                {
                    foreach (RHTile t in liAdjacentTiles)
                    {
                        AdjustableObject obj = null;
                        if (MatchingObjectTest(t, ref obj))
                        {
                            obj.AdjustmentHelper(t, false);
                        }
                    }
                }
            }

            /// <summary>
            /// If the given tile passes the AdjustableObject test, add the if Valdid
            /// string to the adjacency string then add to the list
            /// </summary>
            /// <param name="ifValid">Which directional value to add on a pass</param>
            /// <param name="adjacentStr">Ref to the adjacency string</param>
            /// <param name="adjacentList">ref to the adjacency list</param>
            /// <param name="tile">The tile to test</param>
            protected void MakeAdjustments(string ifValid, ref string adjacentStr, ref List<RHTile> adjacentList, RHTile tile)
            {
                if (MatchingObjectTest(tile))
                {
                    adjacentStr += ifValid;
                    adjacentList.Add(tile);
                }
            }

            /// <summary>
            /// Helper for the MatchingObjectTest method for when we don't care about the object
            /// </summary>
            /// <param name="tile">Tile to test against</param>
            /// <returns>True if the tile exists and contains a matching AdjustableObject</returns>
            protected virtual bool MatchingObjectTest(RHTile tile)
            {
                AdjustableObject obj = null;
                return MatchingObjectTest(tile, ref obj);
            }

            /// <summary>
            /// Check to see that the tile exists, has an AdjustableObject and that AdjustableObject matches the initial type
            /// </summary>
            /// <param name="tile">Tile to test against</param>
            /// <param name="obj">Reference to any AdjustableObject that may be found</param>
            /// <returns>True if the tile exists and contains a matching AdjustableObject</returns>
            protected virtual bool MatchingObjectTest(RHTile tile, ref AdjustableObject obj)
            {
                bool rv = false;

                if (tile != null)
                {
                    WorldObject wObj = tile.GetWorldObject(false);
                    if (wObj != null && wObj.Type == Type)
                    {
                        obj = (AdjustableObject)wObj;
                        rv = true;
                    }
                }

                return rv;
            }
            #endregion

            public class Floor : AdjustableObject
            {
                /// <summary>
                /// Base Constructor to hard define the Height and Width
                /// </summary>
                public Floor(int id, Dictionary<string, string> stringData) : base(id)
                {
                    LoadDictionaryData(stringData, false);
                    LoadAdjustableSprite(ref _sprite, DataManager.FILE_FLOORING);

                    _eObjectType = ObjectTypeEnum.Floor;
                }

                public override void Draw(SpriteBatch spriteBatch)
                {
                    _sprite.SetColor(_bSelected ? Color.Green : Color.White);
                    _sprite.Draw(spriteBatch, 0);
                }

                /// <summary>
                /// Check to see that the tile exists, has an AdjustableObject and that AdjustableObject matches the initial type
                /// </summary>
                /// <param name="tile">Tile to test against</param>
                /// <returns>True if the tile exists and contains a Wall</returns>
                protected override bool MatchingObjectTest(RHTile tile, ref AdjustableObject obj)
                {
                    bool rv = false;

                    if (tile != null)
                    {
                        obj = tile.GetFloorObject();
                        if (obj != null && obj.Type == Type)
                        {
                            rv = true;
                        }
                    }

                    return rv;
                }

                internal FloorData SaveData()
                {
                    FloorData floorData = new FloorData
                    {
                        ID = _iID,
                        x = (int)MapPosition.X,
                        y = (int)MapPosition.Y
                    };

                    return floorData;
                }
                internal void LoadData(FloorData data)
                {
                    _iID = data.ID;
                    SnapPositionToGrid(new Vector2(data.x, data.y));
                }
            }

            /// <summary>
            /// Wall object that can adjust themselves based off of other, adjacent walls
            /// </summary>
            public class Wall : AdjustableObject
            {
                public Wall(int id, Dictionary<string, string> stringData) : base(id)
                {
                    LoadDictionaryData(stringData, false);
                    LoadAdjustableSprite(ref _sprite, DataManager.FILE_WORLDOBJECTS);
                }
            }

            public class Garden : AdjustableObject
            {
                Plant _objPlant;
                AnimatedSprite _sprWatered;
                bool _bWatered;

                public Garden(int id, Dictionary<string, string> stringData) : base(id)
                {
                    OutsideOnly = true;
                    _eObjectType = ObjectTypeEnum.Garden;

                    LoadDictionaryData(stringData, false);

                    LoadAdjustableSprite(ref _sprite, DataManager.FILE_WORLDOBJECTS);
                    _pImagePos.Y += TILE_SIZE;

                    LoadAdjustableSprite(ref _sprWatered, DataManager.FILE_WORLDOBJECTS);
                    _pImagePos.Y -= TILE_SIZE;

                    WaterGardenBed(EnvironmentManager.IsRaining());
                }

                public override void Update(GameTime gTime)
                {
                    base.Update(gTime);
                    _objPlant?.Update(gTime);
                }

                /// <summary>
                /// Overriding because weneed to set the Depth to 0 for drawing since
                /// this is a floor object and needs to beon the bottom.
                /// </summary>
                public override void Draw(SpriteBatch spriteBatch)
                {
                    _sprite.SetColor(_bSelected ? Color.Green : Color.White);
                    _sprWatered.SetColor(_bSelected ? Color.Green : Color.White);

                    if (_bWatered) { _sprWatered.Draw(spriteBatch, 0); }
                    else { _sprite.Draw(spriteBatch, 0); }

                    _objPlant?.Draw(spriteBatch);
                }

                /// <summary>
                /// Override to ensure that _sprWatered stays in sync with _sprite
                /// </summary>
                protected override void AdjustmentHelper(RHTile startTile, bool adjustAdjacent = true)
                {
                    base.AdjustmentHelper(startTile, adjustAdjacent);
                    _sprWatered.PlayAnimation(_sprite.CurrentAnimation.ToString());
                }

                public override bool PlaceOnMap(Vector2 pos, RHMap map)
                {
                    bool rv = base.PlaceOnMap(pos, map);

                    if (_objPlant != null)
                    {
                        _objPlant?.SnapPositionToGrid(new Vector2(_vMapPosition.X, _vMapPosition.Y - (_objPlant.Sprite.Height - TILE_SIZE)));
                        _objPlant?.SyncLightPositions();

                        if (_objPlant.FinishedGrowing())
                        {
                            //Need to do this here for loading because the plant is
                            //set before it's placed
                            CurrentMap.AddLights(_objPlant?.GetLights());
                        }
                    }
                    return rv;
                }

                public override void ProcessLeftClick() { HandleGarden(); }

                /// <summary>
                /// Handles for when the Garden is clicked on to perform
                /// the work that needs to get done
                /// </summary>
                private void HandleGarden()
                {
                    //If no plant, open the Garden window
                    if(_objPlant == null){ GUIManager.OpenMainObject(new GardenWindow(this)); }
                    else
                    {
                        //If the plant is finished growing, harvest it. Otherwise, water it.
                        if (_objPlant.FinishedGrowing()) { _objPlant.ProcessLeftClick(); }
                        else if(!_bWatered) { WaterGardenBed(true); }
                    }
                }

                /// <summary>
                /// Assigns a Plant to the Garden
                /// </summary>
                /// <param name="obj">The plant to assign to the garden</param>
                public void SetPlant(Plant obj)
                {
                    if (obj != null) {
                        PlayerManager.AddToTownObjects(obj);
                        if (_objPlant != null && _objPlant.FinishedGrowing())
                        {
                            CurrentMap?.AddLights(_objPlant?.GetLights());
                        }
                    }
                    else if(_objPlant != null) {
                        PlayerManager.RemoveTownObjects(_objPlant);
                        CurrentMap.RemoveLights(_objPlant.GetLights());
                    }

                    _objPlant = obj;
                    _objPlant?.SetGarden(this);
                    _objPlant?.SnapPositionToGrid(new Vector2(_vMapPosition.X, _vMapPosition.Y - (_objPlant.Sprite.Height - TILE_SIZE)));
                    _objPlant?.SyncLightPositions();
                }
                public Plant GetPlant() { return _objPlant; }

                /// <summary>
                /// Syncs up the _sprWatered and the plant with the new position
                /// </summary>
                /// <param name="position"></param>
                public override void SnapPositionToGrid(Vector2 position)
                {
                    base.SnapPositionToGrid(position);
                    _sprWatered.Position = _vMapPosition;
                    _objPlant?.SnapPositionToGrid(new Vector2(_vMapPosition.X, _vMapPosition.Y - (_objPlant.Sprite.Height - TILE_SIZE)));
                }

                public override void Rollover()
                {
                    if (_bWatered) { _objPlant?.Rollover(); }

                    WaterGardenBed(EnvironmentManager.IsRaining());
                }

                public void WaterGardenBed(bool value)
                {
                    _bWatered = value;
                    _sprWatered.PlayAnimation(_sprite.CurrentAnimation.ToString());
                }

                public override List<Light> GetLights()
                {
                    if(_objPlant!= null) { return _objPlant.GetLights(); }
                    else { return base.GetLights(); }
                }

                public override void SyncLightPositions()
                {
                    if (_objPlant != null) { _objPlant.SyncLightPositions(); }
                    else { base.SyncLightPositions(); }
                    
                }

                public GardenData SaveData()
                {
                    GardenData g = new GardenData
                    {
                        ID = this.ID,
                        x = (int)this.MapPosition.X,
                        y = (int)this.MapPosition.Y
                    };

                    if (_objPlant != null) { g.plantData = _objPlant.SaveData(); }
                    else { g.plantData = new PlantData { ID = -1 }; };

                    return g;
                }

                public void LoadData(GardenData garden)
                {
                    _iID = garden.ID;
                    SnapPositionToGrid(new Vector2(garden.x, garden.y));

                    if (garden.plantData.ID != -1)
                    {
                        _objPlant = (Plant)DataManager.GetWorldObjectByID(garden.plantData.ID);
                        _objPlant.LoadData(garden.plantData);

                        SetPlant(_objPlant);
                    }
                }
            }
        }
    }

    public class Machine : WorldObject
    {
        readonly string _sEffectWorking = "";

        protected int _iContainingBuildingID = -1;

        protected double _dProcessedTime = 0;
        int _iCurrentlyMaking = -1;

        protected int _iWorkingFrames = 2;
        protected float _fFrameSpeed = 0.3f;

        public Dictionary<int, int> CraftingDictionary { get; }
        private bool _bWorking = false;

        public Machine(int id, Dictionary<string, string> stringData) : base(id)
        {
            if (stringData.ContainsKey("WorkAnimation"))
            {
                string[] split = stringData["WorkAnimation"].Split('-');
                _iWorkingFrames = int.Parse(split[0]);
                _fFrameSpeed = float.Parse(split[1]);
            }

            Util.AssignValue(ref _sEffectWorking, "WorkEffect", stringData);

            CraftingDictionary = new Dictionary<int, int>();
            if (stringData.ContainsKey("Makes"))
            {
                //Read in what items the machine can make
                string[] processes = Util.FindParams(stringData["Makes"]);
                foreach (string recipe in processes)
                {
                    //Each entry is in written like ID-NumDays
                    string[] pieces = recipe.Split('-');
                    CraftingDictionary.Add(int.Parse(pieces[0]), int.Parse(pieces[1]));
                }
            }

            LoadDictionaryData(stringData);
        }

        protected override void LoadSprite(Dictionary<string, string> stringData, string textureName = "Textures\\texMachines")
        {
            _sprite = new AnimatedSprite(@"Textures\texMachines");
            _sprite.AddAnimation(AnimationEnum.ObjectIdle, (int)_pImagePos.X, (int)_pImagePos.Y, _uSize, 1, 0.3f, false);
            _sprite.AddAnimation(AnimationEnum.PlayAnimation, (int)_pImagePos.X + _uSize.Width, (int)_pImagePos.Y, _uSize, _iWorkingFrames, _fFrameSpeed, false);
            _sprite.PlayAnimation(AnimationEnum.ObjectIdle);
            _sprite.Drawing = true;

            SetSpritePos(_vMapPosition);
        }

        public override void Update(GameTime gTime)
        {
            if (_iCurrentlyMaking != -1)       //Crafting Handling
            {
                _sprite.Update(gTime);

                _dProcessedTime += gTime.ElapsedGameTime.TotalSeconds;
                // CheckFinishedCrafting();
            }
        }

        public override void ProcessLeftClick() { ClickProcess(); }
        public override void ProcessRightClick() { ClickProcess(); }

        private void ClickProcess()
        {
            if (!MakingSomething())
            {
                GUIManager.OpenMainObject(new HUDCraftingDisplay(this));
            }
        }

        /// <summary>
        /// Not currently used
        /// </summary>
        //private void CheckFinishedCrafting()
        //{
        //    if (_iCurrentlyMaking != -1 && _dProcessedTime >= CraftingDictionary[_iCurrentlyMaking])
        //    {
        //        InventoryManager.AddToInventory(_iCurrentlyMaking);
        //        SoundManager.StopEffect(this);
        //        SoundManager.PlayEffectAtLoc("126426__cabeeno-rossley__timer-ends-time-up", _sMapName, MapPosition, this);
        //        _dProcessedTime = 0;
        //        _iCurrentlyMaking = -1;
        //        _sprite.PlayAnimation(AnimationEnum.ObjectIdle);
        //    }
        //}


        /// <summary>
        /// Called by the HUDCraftingMenu to craft the selected item.
        /// 
        /// Ensure that the Player has enough space in their inventory for the item
        /// as well as they have the required items to make it.
        /// 
        /// Perform the Crafting steps and add the item to the inventory.
        /// </summary>
        /// <param name="itemToCraft">The Item object to craft</param>
        public void AttemptToCraftChosenItem(Item itemToCraft)
        {
            if (InventoryManager.HasSpaceInInventory(itemToCraft.ItemID, 1) && PlayerManager.ExpendResources(itemToCraft.GetRequiredItems()))
            {
                double mod = 0;
                if (_iContainingBuildingID != -1)
                {
                    mod = 0.1 * (PlayerManager.GetBuildingByID(_iContainingBuildingID).Level - 1);
                }

                PlayerManager.DecreaseStamina(1 - mod);

                //_iCurrentlyMaking = itemToCraft.ItemID;
                //_sprite.PlayAnimation(AnimationEnum.PlayAnimation);

                InventoryManager.AddToInventory(itemToCraft.ItemID);
                if (!string.IsNullOrEmpty(_sEffectWorking))
                {
                    SoundManager.PlayEffect(_sEffectWorking);
                }
            }
        }

        /// <summary>
        /// OVerride method for Rollover. Shouldn't matter since item crafting should take no time
        /// but for future proofing we'll have this here.
        /// </summary>
        public override void Rollover()
        {
            if (_bWorking)
            {
                _dProcessedTime += GameCalendar.GetMinutesToNextMorning();
                //CheckFinishedCrafting();

                _bWorking = false;
            }
        }

        public bool MakingSomething() { return _iCurrentlyMaking != -1; }

        public override bool PlaceOnMap(Vector2 pos, RHMap map)
        {
            bool rv = false;
            if (base.PlaceOnMap(pos, map))
            {
                rv = true;
                GameManager.AddMachine(this, Name);

                if (map.BuildingID != -1)
                {
                    _iContainingBuildingID = map.BuildingID;
                }
            }

            return rv;
        }

        public MachineData SaveData()
        {
            MachineData m = new MachineData
            {
                ID = this.ID,
                x = (int)this.MapPosition.X,
                y = (int)this.MapPosition.Y,
                processedTime = this._dProcessedTime,
                currentItemID = _iCurrentlyMaking
            };

            return m;
        }
        public void LoadData(MachineData mac)
        {
            _iID = mac.ID;
            SnapPositionToGrid(new Vector2(mac.x, mac.y));
            _dProcessedTime = mac.processedTime;
            _iCurrentlyMaking = mac.currentItemID;

            // if (CurrentlyProcessing != null) { _sprite.PlayAnimation(AnimationEnum.ObjectIdle); }
        }
    }

    public abstract class TriggerObject : WorldObject
    {
        #region constants
        const string MATCH_TRIGGER = "MatchTrigger";
        const string TRIGGER_NUMBER = "TriggerNumber";
        const string ITEM_KEY_ID = "ItemKeyID";
        const string OUT_TRIGGER = "OutTrigger";
        #endregion

        enum DungeonObjectType { Trigger, Door };
        readonly DungeonObjectType _eSubType;
        readonly string _sOutTrigger;   //What trigger response is sent
        protected string _sMatchTrigger; //What, if anything, the object responds to
        protected int _iTriggerNumber = 1;
        protected int _iTriggersLeft = 1;
        bool _bVisible = true;
        readonly int _iItemKeyID = -1;
        bool _bHasBeenTriggered = false;

        protected TriggerObject(int id, Dictionary<string, string> stringData) : base(id)
        {
            LoadDictionaryData(stringData);
            _eObjectType = ObjectTypeEnum.DungeonObject;
            _eSubType = Util.ParseEnum<DungeonObjectType>(stringData["Subtype"]);

            Util.AssignValue(ref _sOutTrigger, OUT_TRIGGER, stringData);
            Util.AssignValue(ref _sMatchTrigger, MATCH_TRIGGER, stringData);
            Util.AssignValue(ref _iTriggerNumber, TRIGGER_NUMBER, stringData);
            Util.AssignValue(ref _iItemKeyID, ITEM_KEY_ID, stringData);

            _iTriggersLeft = _iTriggerNumber;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (_bVisible)
            {
                base.Draw(spriteBatch);
            }
        }

        /// <summary>
        /// This method is called when something attempts to trigger it
        /// </summary>
        /// <param name="name">The name of the trigger</param>
        public virtual void AttemptToTrigger(string name) {}

        /// <summary>
        /// Call to see if the object will be triggered by the sent trigger
        /// </summary>
        /// <param name="triggerName">The trigger name to match against the response trigger</param>
        /// <returns>True if theo object can trigger</returns>
        private bool CanTrigger(string triggerName)
        {
            bool rv = false;
            if(triggerName == _sMatchTrigger)
            {
                rv = CanTrigger();
            }
             
            return rv;
        }

        /// <summary>
        /// Call to see if the object can trigger. Only valid if the object hasn't been triggered
        /// and the object hasno more remaining trigger numbers to wait on
        /// </summary>
        /// <returns>True if the object can trigger</returns>
        private bool CanTrigger()
        {
            bool rv = false;
            if (!_bHasBeenTriggered && UpdateTriggerNumber())
            {
                rv = true;
            }

            return rv;
        }

        /// <summary>
        /// This method is called to trigger the object and make it send its trigger
        /// </summary>
        public virtual void FireTrigger() { }

        /// <summary>
        /// Call this to reset the DungeonObject to its original state.
        /// </summary>
        public virtual void Reset() { }

        /// <summary>
        /// Given an item type, check it against the key for the DungeonObject
        /// </summary>
        /// <param name="item">The Item to check against</param>
        /// <returns>True if the item is the key</returns>
        public bool CheckForKey(Item item)
        {
            bool rv = false;
            if (_iItemKeyID == item.ItemID)
            {
                rv = true;
                item.Remove(1);
            }

            return rv;
        }

        /// <summary>
        /// This checks whether or not the object should trigger
        /// </summary>
        /// <returns>Returns true if the object can trigger</returns>
        private bool UpdateTriggerNumber()
        {
            if (_iTriggersLeft > 0)
            {
                _iTriggersLeft--;
            }

            return _iTriggersLeft == 0;
        }

        public class Trigger : TriggerObject
        {
            string _sSoundEffect;
            Item _item;
            public Trigger(int id, Dictionary<string, string> stringData) : base(id, stringData)
            {
                _item = DataManager.GetItem(_iItemKeyID);

                Util.AssignValue(ref _sSoundEffect, "SoundEffect", stringData);

                if (_iItemKeyID == -1)
                {
                    _sprite.AddAnimation(AnimationEnum.Action_One, _pImagePos.X + Width, _pImagePos.Y, _uSize);
                }
            }

            public override void Draw(SpriteBatch spriteBatch)
            {
                base.Draw(spriteBatch);
                if(_item != null)
                {
                    float visibility = _bHasBeenTriggered ? 1f : 0.25f;
                    _item.Draw(spriteBatch, new Rectangle((int)(_vMapPosition.X), (int)(_vMapPosition.Y - 6), TILE_SIZE, TILE_SIZE), true, _sprite.LayerDepth + 1, visibility);
                }
            }

            /// <summary>
            /// Called when the player interacts with the object.
            /// 
            /// If it's already triggered, do nothing.
            /// </summary>
            public override void ProcessRightClick()
            {
                GameManager.CurrentWorldObject = this;

                if (!_bHasBeenTriggered)
                {
                    //If there's an itemKeyID, display appropriate text
                    if (_iItemKeyID != -1)
                    {
                        GUIManager.OpenMainObject(new HUDInventoryDisplay());
                    }
                    else {
                        SoundManager.PlayEffectAtLoc(_sSoundEffect, MapName, MapPosition, this);
                        FireTrigger();
                    }
                }
            }

            public override void AttemptToTrigger(string name)
            {
                if (CanTrigger(name))
                {
                    FireTrigger();
                }
            }

            public override void FireTrigger()
            {
                if(CanTrigger())
                {
                    _bHasBeenTriggered = true;
                    _sprite.PlayAnimation(AnimationEnum.Action_One);
                    GameManager.ActivateTriggers(_sOutTrigger);
                }
            }
        }

        public class Door : TriggerObject
        {
            readonly bool _bKeyDoor;
            public Door(int id, Dictionary<string, string> stringData) : base(id, stringData)
            {
                if (stringData.ContainsKey("KeyDoor"))
                {
                    _bKeyDoor = true;
                    _sMatchTrigger = GameManager.KEY_OPEN;
                }

                _rBase.Y = _uSize.Height - BaseHeight;
            }

            /// <summary>
            /// When a door is triggered, it becomes passable and invisible.
            /// </summary>
            /// <param name="name"></param>
            public override void AttemptToTrigger(string name)
            {
                if (CanTrigger(name))
                {
                    if (!string.IsNullOrEmpty(_sOutTrigger))
                    {
                        GameManager.ActivateTriggers(_sOutTrigger);
                    }
                    _bHasBeenTriggered = true;
                    _bWalkable = true;
                    _bVisible = false;
                }
            }

            /// <summary>
            /// When triggered, makes doors impassable again
            /// </summary>
            public override void Reset()
            {
                _bWalkable = false;
                _bVisible = true;
                _iTriggersLeft = _iTriggerNumber;
            }

            /// <summary>
            /// Handles the response from whent he player attempts to Interact with the Door object.
            /// Primarily just handles the output for the doors and the type of triggers required to use it.
            /// </summary>
            public override void ProcessRightClick()
            {
                GameManager.CurrentWorldObject = this;
                if (_bKeyDoor)
                {
                    if (DungeonManager.DungeonKeys() > 0)
                    {
                        DungeonManager.UseDungeonKey();
                        AttemptToTrigger(KEY_OPEN);
                    }
                    else
                    {
                        GUIManager.OpenTextWindow(DataManager.GetGameTextEntry("Key_Door"));
                    }
                }
                else if (_iItemKeyID != -1)
                {
                    GUIManager.OpenMainObject(new HUDInventoryDisplay());
                }
                else
                {
                    GUIManager.OpenTextWindow(DataManager.GetGameTextEntry("Trigger_Door"));
                }
            }
        }
    }

    public class WarpPoint : WorldObject
    {
        public bool Active { get; private set; } = false;
        private string _sDungeonName;

        public WarpPoint(int id, Dictionary<string, string> stringData) : base(id)
        {
            LoadDictionaryData(stringData);
            _sprite.AddAnimation(AnimationEnum.Action_One, _pImagePos.X + (TILE_SIZE * 2), _pImagePos.Y, _uSize);
        }

        public override bool PlaceOnMap(Vector2 pos, RHMap map)
        {
            bool rv = base.PlaceOnMap(pos, map);
            _sDungeonName = map.DungeonName;
            DungeonManager.AddWarpPoint(this, _sDungeonName);

            return rv;
        }

        public override void ProcessRightClick()
        {
            if (!Active)
            {
                Active = true;
                _sprite.PlayAnimation(AnimationEnum.Action_One);
            }
            else
            {
                GUIManager.OpenMainObject(new WarpPointWindow(this));
            }
        }

        public WarpPointData SaveData()
        {
            WarpPointData w = new WarpPointData
            {
                ID = this.ID,
                x = (int)this.CollisionBox.X,
                y = (int)this.CollisionBox.Y,
                active = this.Active
            };

            return w;
        }
        public void LoadData(WarpPointData warpPt)
        {
            _iID = warpPt.ID;
            SnapPositionToGrid(new Vector2(warpPt.x, warpPt.y));
            Active = warpPt.active;

            if (Active) { _sprite.PlayAnimation(AnimationEnum.Action_One); }
        }
    }

    public class Light
    {
        AnimatedSprite _sprite;
        public Vector2 Position
        {
            get { return _sprite.Position; }
            set { _sprite.Position = value; }
        }

        private Vector2 _vecDimensions;

        public int Width => (int)_vecDimensions.X;
        public int Height => (int)_vecDimensions.Y;

        public Light(int id, Dictionary<string, string> stringData)
        {
            string lightTex = string.Empty;
            Util.AssignValue(ref lightTex, "Texture", stringData);

            _sprite = new AnimatedSprite(DataManager.FOLDER_ENVIRONMENT + lightTex);

            Vector2 animDescriptor = new Vector2(1, 1);
            Util.AssignValue(ref animDescriptor, "Idle", stringData);
            Util.AssignValue(ref _vecDimensions, "Dimensions", stringData);
            _sprite.AddAnimation(AnimationEnum.ObjectIdle, 0, 0, Width, Height, (int)animDescriptor.X, animDescriptor.Y, true);
        }

        public void Update(GameTime gTime)
        {
            _sprite.Update(gTime);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            _sprite.Draw(spriteBatch);
        }
    }
}
