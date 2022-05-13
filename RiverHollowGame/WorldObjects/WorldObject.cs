using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.Game_Managers;
using RiverHollow.Items;
using RiverHollow.SpriteAnimations;
using RiverHollow.Map_Handling;
using RiverHollow.Utilities;
using System.Collections.Generic;
using static RiverHollow.Game_Managers.GameManager;
using static RiverHollow.Utilities.Enums;

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

        //Base is always described in # of Tiles so we must multiply by the TILE_SIZE
        public Rectangle CollisionBox => Util.FloatRectangle(MapPosition.X + (_rBase.X * TILE_SIZE), MapPosition.Y + (_rBase.Y * TILE_SIZE), (_rBase.Width * TILE_SIZE), (_rBase.Height * TILE_SIZE));

        protected int _iID;
        public int ID  => _iID;
        #endregion

        public virtual string Name()
        {
            return DataManager.GetTextData("WorldObject", _iID, "Name");
        }

        protected WorldObject(int id)
        {
            Tiles = new List<RHTile>();

            _iID = id;
            _bWallObject = false;
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
            else {
                float alpha = 1f;
                if(new Rectangle((int)Sprite.Position.X, (int)Sprite.Position.Y, Sprite.Width, Sprite.Height).Contains(PlayerManager.PlayerActor.CollisionBox.Center))
                {
                    alpha = 0.7f;
                }
                _sprite.Draw(spriteBatch, true, alpha);
            }
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
            Util.AddUniquelyToList(ref Tiles, t);
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
    }
}
