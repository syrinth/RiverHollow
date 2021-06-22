using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.Characters;
using RiverHollow.Game_Managers;
using RiverHollow.GUIComponents.GUIObjects;
using RiverHollow.GUIComponents.GUIObjects.GUIWindows;
using RiverHollow.GUIComponents.MainObjects;
using RiverHollow.GUIComponents.Screens;
using RiverHollow.Misc;
using RiverHollow.SpriteAnimations;
using RiverHollow.Tile_Engine;
using RiverHollow.Utilities;
using static RiverHollow.Game_Managers.DataManager;
using static RiverHollow.Game_Managers.GameManager;
using static RiverHollow.Game_Managers.SaveManager;
using static RiverHollow.GUIComponents.Screens.BuildScreen;
using static RiverHollow.Items.Buildable.AdjustableObject;

namespace RiverHollow.Items
{
    public abstract class WorldObject
    {
        #region Properties
        protected ObjectTypeEnum _eObjectType;
        public ObjectTypeEnum Type => _eObjectType;

        protected AnimatedSprite _sprite;
        public AnimatedSprite Sprite => _sprite;

        public List<RHTile> Tiles;

        protected string MapName => Tiles[0].MapName;
        protected bool _bWalkable = false;
        public bool Walkable => _bWalkable;
        protected bool _wallObject;
        public bool WallObject => _wallObject;

        protected KeyValuePair<int, int> _kvpDrop; //itemID, # of items dropped

        protected bool _bDrawUnder = false;

        protected Point _pImagePos;
        public Vector2 PickupOffset { get; private set; }

        protected Vector2 _vMapPosition;
        public virtual Vector2 MapPosition => _vMapPosition;

        protected int _iSpriteWidth = TileSize;
        public int Width => _iSpriteWidth;
        protected int _iSpriteHeight = TileSize;
        public int Height => _iSpriteHeight;

        protected int _iBaseWidth = 1;
        public int BaseWidth => _iBaseWidth;
        protected int _iBaseHeight = 1;
        public int BaseHeight => _iBaseHeight;

        protected int _iBaseXOffset = 0;
        protected int _iBaseYOffset = 0;

        //The ClickBox is always the Sprite itself
        public Rectangle ClickBox => new Rectangle((int)MapPosition.X, (int)MapPosition.Y, _iSpriteWidth, _iSpriteHeight);

        //Base is always described in # of Tiles so we must multiply by the TileSize
        public Rectangle CollisionBox => Util.FloatRectangle(MapPosition.X + (_iBaseXOffset * TileSize), MapPosition.Y + (_iBaseYOffset * TileSize), (_iBaseWidth * TileSize), (_iBaseHeight * TileSize));

        protected int _iID;
        public int ID  => _iID;

        protected string _sName;
        public string Name => _sName;
        #endregion

        protected WorldObject(int id)
        {
            Tiles = new List<RHTile>();

            _iID = id;
            _wallObject = false;

            DataManager.GetTextData("WorldObject", _iID, ref _sName, "Name");
        }

        protected virtual void LoadDictionaryData(Dictionary<string, string> stringData, bool loadSprite = true)
        {
            Util.AssignValue(ref _pImagePos, "Image", stringData);

            Util.AssignValue(ref _iSpriteWidth, "Width", stringData);
            Util.AssignValue(ref _iSpriteHeight, "Height", stringData);

            Util.AssignValues(ref _iBaseXOffset, ref _iBaseYOffset, "BaseOffset", stringData);
            Util.AssignValues(ref _iBaseWidth, ref _iBaseHeight, "Base", stringData);

            Util.AssignValue(ref _eObjectType, "Type", stringData);

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
                _sprite.AddAnimation(AnimationEnum.ObjectIdle, _pImagePos.X, _pImagePos.Y, _iSpriteWidth, _iSpriteHeight, int.Parse(idleSplit[0]), float.Parse(idleSplit[1]));
            }
            else
            {
                _sprite.AddAnimation(AnimationEnum.ObjectIdle, _pImagePos.X, _pImagePos.Y, _iSpriteWidth, _iSpriteHeight);
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
            return PlaceOnMap(this.MapPosition, map);
        }

        public virtual bool PlaceOnMap(Vector2 pos, RHMap map)
        {
            pos = new Vector2(pos.X - (_iBaseXOffset * TileSize), pos.Y - (_iBaseYOffset * TileSize));
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
            int xOffset = (Width > TileSize) ? (int)(mousePosition.X - _sprite.Position.X) : 0;
            int yOffset = (Height > TileSize) ? (int)(mousePosition.Y - _sprite.Position.Y) : 0;

            xOffset = (xOffset / TileSize) * TileSize;
            yOffset = (yOffset / TileSize) * TileSize;
            PickupOffset = new Vector2(xOffset, yOffset);
            
        }

        /// <summary>
        /// Sets the default PickupOffset if the Width of height
        /// is greater than a single RHTile
        /// </summary>
        public void SetPickupOffset()
        {
            int xOffset = (_iBaseWidth > 1) ? (_iBaseWidth - 1) / 2 : 0;
            int yOffset = (_iBaseHeight > 1) ? (_iBaseHeight -1) / 2 : 0;
            PickupOffset = new Vector2((_iBaseXOffset + xOffset) * TileSize, (_iBaseYOffset + yOffset) * TileSize);
            PickupOffset = (PickupOffset / TileSize) * TileSize;
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

        public bool CompareType(ObjectTypeEnum t) { return Type == t; }
        public bool IsDestructible() { return CompareType(ObjectTypeEnum.Destructible) || CompareType(ObjectTypeEnum.Plant); }
        public bool IsBuildable()
        {
            bool rv = false;
            switch (_eObjectType)
            {
                case ObjectTypeEnum.Building:
                case ObjectTypeEnum.Container:
                case ObjectTypeEnum.Floor:
                case ObjectTypeEnum.Garden:
                case ObjectTypeEnum.Light:
                case ObjectTypeEnum.Mailbox:
                case ObjectTypeEnum.Structure:
                case ObjectTypeEnum.Wall:
                    rv = true;
                    break;
            }

            return rv;
        }

        public virtual bool CanPickUp() { return false; }
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

            _wallObject = false;

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
                _sprite.AddAnimation(AnimationEnum.KO, int.Parse(splitString[0]), int.Parse(splitString[1]), TileSize, TileSize, int.Parse(splitString[2]), float.Parse(splitString[3]), false, true);
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

        Garden _objGarden;

        public Plant(int id, Dictionary<string, string> stringData) : base(id, stringData, false)
        {
            _diTransitionTimes = new Dictionary<int, int>();

            LoadDictionaryData(stringData);

            _bWalkable = true;

            _iCurrentState = 0;
            _iBaseYOffset = (_iSpriteHeight / TileSize) - 1;

            Util.AssignValue(ref _iSeedID, "SeedID", stringData);
            Util.AssignValue(ref _iResourceID, "Item", stringData);
            Util.AssignValue(ref _iMaxStates, "TrNum", stringData); //Number of growth phases

            _bPopItem = false;

            //The amount of time for each phase
            string[] dayStr = stringData["TrTime"].Split('-');
            for (int j = 0; j < _iMaxStates - 1; j++)
            {
                _diTransitionTimes.Add(j, int.Parse(dayStr[j]));
                _sprite.AddAnimation((j + 1).ToString(), _pImagePos.X + (TileSize * (j+1)), _pImagePos.Y, _iSpriteWidth, _iSpriteHeight);
            }
            _iDaysLeft = _diTransitionTimes[0];

            if (stringData.ContainsKey("DestructionAnim"))
            {
                string[] splitString = stringData["DestructionAnim"].Split('-');
                _sprite.AddAnimation(AnimationEnum.KO, int.Parse(splitString[0]), int.Parse(splitString[1]), _iSpriteWidth, _iSpriteHeight, int.Parse(splitString[2]), float.Parse(splitString[3]), false, true);
            }

            _sprite.SetRotationOrigin(new Vector2(_iSpriteWidth / 2, _iSpriteHeight - 1));    //Subtract one to keep it in the bounds of the rectangle
        }

        protected override void LoadSprite(Dictionary<string, string> stringData, string textureName = DataManager.FILE_WORLDOBJECTS)
        {
            _sprite = new AnimatedSprite(DataManager.FILE_WORLDOBJECTS);
            _sprite.AddAnimation(0.ToString(), (int)_pImagePos.X, (int)_pImagePos.Y, _iSpriteWidth, _iSpriteHeight);
            for (int j = 1; j < _diTransitionTimes.Count + 1; j++)
            {
                _sprite.AddAnimation(j.ToString(), (int)_pImagePos.X + (TileSize * j), (int)_pImagePos.Y, _iSpriteWidth, _iSpriteHeight);
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
            else if (!FinishedGrowing()) //If it hasn't finished growing, and there'sno days left, go to the next phase
            {
                _iCurrentState++;
                _sprite.PlayAnimation(_iCurrentState.ToString());
                if (_diTransitionTimes.ContainsKey(_iCurrentState))
                {
                    _iDaysLeft = _diTransitionTimes[_iCurrentState];
                }
            }
        }
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
            _iBaseXOffset = 1;
            _iBaseYOffset = 4;

            LoadDictionaryData(stringData);
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
            _sprite.SetDepthMod(_bDrawOver ? 1 : -999);

            _iInit = 0;
            if (_eHazardType == HazardTypeEnum.Passive) { Active = true; }
            else { Activate(false); }
        }

        protected override void LoadSprite(Dictionary<string, string> stringData, string textureName = DataManager.FILE_WORLDOBJECTS)
        {
            base.LoadSprite(stringData, textureName);
            _sprite.AddAnimation(AnimationEnum.Action_One, _pImagePos.X + TileSize, _pImagePos.Y, _iSpriteWidth, _iSpriteHeight);
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
    public abstract class Buildable : WorldObject
    {
        protected Dictionary<int, int> _diReqToMake;
        public Dictionary<int, int> RequiredToMake => _diReqToMake;

        protected bool _bSelected = false;

        protected Buildable(int id) : base(id) {
            _iBaseYOffset = (_iSpriteHeight / TileSize) - BaseHeight;
        }

        protected override void LoadDictionaryData(Dictionary<string, string> stringData, bool loadSprite = true)
        {
            base.LoadDictionaryData(stringData, loadSprite);

            Util.AssignValue(ref _diReqToMake, "ReqItems", stringData);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            _sprite.SetColor(_bSelected ? Color.Green : Color.White);
            base.Draw(spriteBatch);
        }

        public void SelectObject(bool val) { _bSelected = val; }

        public class Structure : Buildable
        {
            List<SubObjectInfo> _liSubObjectInfo;
            Vector2 _vecSpecialCoords = Vector2.Zero;
            public Structure(int id, Dictionary<string, string> stringData) : base (id)
            {
                _liSubObjectInfo = new List<SubObjectInfo>();
                LoadDictionaryData(stringData);

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

            public Container(int id, Dictionary<string, string> stringData) : base(id)
            {
                LoadDictionaryData(stringData);

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

        public class Light : Buildable
        {
            public Light(int id, Dictionary<string, string> stringData) : base(id)
            {
                LoadDictionaryData(stringData);

                string[] idleSplit = stringData["Idle"].Split('-');
                _sprite.PlayAnimation(AnimationEnum.ObjectIdle);
                _sprite.Drawing = true;
            }
        }

        public class Mailbox : Buildable
        {
            private AnimatedSprite _alertSprite;
            private List<string> _liCurrentMessages;
            private List<string> _liSentMessages;

            public Mailbox(int id, Dictionary<string, string> stringData) : base(id)
            {
                _liCurrentMessages = new List<string>();
                _liSentMessages = new List<string>();
                LoadDictionaryData(stringData);
                PlayerManager.PlayerMailbox = this;

                _iBaseYOffset = (_iSpriteHeight / TileSize) - BaseHeight;
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
                    _alertSprite.AddAnimation(AnimationEnum.ObjectIdle, 64, 64, TileSize, TileSize, 3, 0.150f, true);
                    _alertSprite.Position = new Vector2(_vMapPosition.X, _vMapPosition.Y - TileSize);
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
                spr.AddAnimation("None", (int)_pImagePos.X, (int)_pImagePos.Y, _iSpriteWidth, _iSpriteHeight);
                spr.AddAnimation("NS", (int)_pImagePos.X + TileSize, (int)_pImagePos.Y, _iSpriteWidth, _iSpriteHeight);
                spr.AddAnimation("EW", (int)_pImagePos.X + TileSize * 2, (int)_pImagePos.Y, _iSpriteWidth, _iSpriteHeight);
                spr.AddAnimation("SW", (int)_pImagePos.X + TileSize * 3, (int)_pImagePos.Y, _iSpriteWidth, _iSpriteHeight);
                spr.AddAnimation("NW", (int)_pImagePos.X + TileSize * 4, (int)_pImagePos.Y, _iSpriteWidth, _iSpriteHeight);
                spr.AddAnimation("NE", (int)_pImagePos.X + TileSize * 5, (int)_pImagePos.Y, _iSpriteWidth, _iSpriteHeight);
                spr.AddAnimation("SE", (int)_pImagePos.X + TileSize * 6, (int)_pImagePos.Y, _iSpriteWidth, _iSpriteHeight);
                spr.AddAnimation("NSE", (int)_pImagePos.X + TileSize * 7, (int)_pImagePos.Y, _iSpriteWidth, _iSpriteHeight);
                spr.AddAnimation("NSW", (int)_pImagePos.X + TileSize * 8, (int)_pImagePos.Y, _iSpriteWidth, _iSpriteHeight);
                spr.AddAnimation("NEW", (int)_pImagePos.X + TileSize * 9, (int)_pImagePos.Y, _iSpriteWidth, _iSpriteHeight);
                spr.AddAnimation("SEW", (int)_pImagePos.X + TileSize * 10, (int)_pImagePos.Y, _iSpriteWidth, _iSpriteHeight);
                spr.AddAnimation("NSEW", (int)_pImagePos.X + TileSize * 11, (int)_pImagePos.Y, _iSpriteWidth, _iSpriteHeight);
                spr.AddAnimation("W", (int)_pImagePos.X + TileSize * 12, (int)_pImagePos.Y, _iSpriteWidth, _iSpriteHeight);
                spr.AddAnimation("E", (int)_pImagePos.X + TileSize * 13, (int)_pImagePos.Y, _iSpriteWidth, _iSpriteHeight);
                spr.AddAnimation("S", (int)_pImagePos.X + TileSize * 14, (int)_pImagePos.Y, _iSpriteWidth, _iSpriteHeight);
                spr.AddAnimation("N", (int)_pImagePos.X + TileSize * 15, (int)_pImagePos.Y, _iSpriteWidth, _iSpriteHeight);
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
                    _eObjectType = ObjectTypeEnum.Garden;

                    LoadDictionaryData(stringData, false);

                    LoadAdjustableSprite(ref _sprite, DataManager.FILE_WORLDOBJECTS);
                    _pImagePos.Y += TileSize;

                    LoadAdjustableSprite(ref _sprWatered, DataManager.FILE_WORLDOBJECTS);
                    _pImagePos.Y -= TileSize;

                    WaterGardenBed(EnvironmentManager.IsRaining());
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
                    _objPlant = obj;
                    _objPlant?.SetGarden(this);
                    _objPlant?.SnapPositionToGrid(new Vector2(_vMapPosition.X, _vMapPosition.Y - (_objPlant.Sprite.Height - TileSize)));
                }
                public Plant GetPlant() { return _objPlant; }

                /// <summary>
                /// Syncs up the _sprWatered and the plant with the new position
                /// </summary>
                /// <param name="position"></param>
                public override void SnapPositionToGrid(Vector2 position)
                {
                    base.SnapPositionToGrid(position);
                    _sprWatered.Position = position;
                    _objPlant?.SnapPositionToGrid(_vMapPosition);
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
            _sprite.AddAnimation(AnimationEnum.ObjectIdle, (int)_pImagePos.X, (int)_pImagePos.Y, _iSpriteWidth, _iSpriteHeight, 1, 0.3f, false);
            _sprite.AddAnimation(AnimationEnum.PlayAnimation, (int)_pImagePos.X + _iSpriteWidth, (int)_pImagePos.Y, _iSpriteWidth, _iSpriteHeight, _iWorkingFrames, _fFrameSpeed, false);
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
                    _sprite.AddAnimation(AnimationEnum.Action_One, _pImagePos.X + Width, _pImagePos.Y, _iSpriteWidth, _iSpriteHeight);
                }
            }

            public override void Draw(SpriteBatch spriteBatch)
            {
                base.Draw(spriteBatch);
                if(_item != null)
                {
                    float visibility = _bHasBeenTriggered ? 1f : 0.25f;
                    _item.Draw(spriteBatch, new Rectangle((int)(_vMapPosition.X), (int)(_vMapPosition.Y - 6), TileSize, TileSize), true, _sprite.LayerDepth + 1, visibility);
                }
            }

            /// <summary>
            /// Called when the player interacts with the object.
            /// 
            /// If it's already triggered, do nothing.
            /// </summary>
            public override void ProcessRightClick()
            {
                GameManager.CurrentTriggerObject = this;

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

                _iBaseYOffset = (_iSpriteHeight / TileSize) - BaseHeight;
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
                GameManager.CurrentTriggerObject = this;
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

    public class StructureUpgrader : WorldObject
    {
        int _iStructureID = -1;
        int _iBuildingID = -1;

        public StructureUpgrader(int id, Dictionary<string, string> stringData) : base (id) {
            LoadDictionaryData(stringData);
        }

        public override void ProcessLeftClick()
        {
            GUIManager.OpenMainObject(new HUDUpgradeWindow(PlayerManager.GetBuildingByID(_iBuildingID)));
        }

        public override bool PlaceOnMap(Vector2 pos, RHMap map)
        {
            bool rv = false;
            if (base.PlaceOnMap(pos, map))
            {
                rv = true;
                if (map.BuildingID != -1)
                {
                    SetBuildingID(map.BuildingID);
                }
            }

            return rv;
        }

        public void SetBuildingID(int ID)
        {
            _iBuildingID = ID;
            _iStructureID = -1;
        }

        public void SetStructureId(int ID)
        {
            _iStructureID = ID;
            _iBuildingID = -1;
        }

    }
}
