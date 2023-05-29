using Microsoft.Xna.Framework;
using MonoGame.Extended.Tiled;
using RiverHollow.Game_Managers;
using RiverHollow.Utilities;
using RiverHollow.WorldObjects;
using System.Collections.Generic;
using MonoGame.Extended;
using RiverHollow.Characters;
using System;
using System.Linq;

using static RiverHollow.Utilities.Enums;

namespace RiverHollow.Map_Handling
{
    public abstract class SpawnPoint
    {
        protected RHMap _map;
        protected Size2 _szDimensions;
        protected Vector2 _vPosition;
        protected Dictionary<RarityEnum, List<SpawnData>> _diSpawnData;

        public SpawnPoint(RHMap map)
        {
            _diSpawnData = new Dictionary<RarityEnum, List<SpawnData>>();
            _map = map;
        }

        public SpawnPoint(RHMap map, TiledMapObject obj) : this(map)
        {
            _vPosition = map.GetTileByGridCoords(Util.GetGridCoords(obj.Position.ToPoint())).Position.ToVector2();
            _szDimensions = obj.Size;
            _szDimensions = obj.Size;
        }

        protected void AssignSpawnData(string spawnData, SpawnTypeEnum t)
        {
            string[] spawnResources = Util.FindParams(spawnData);
            foreach (string s in spawnResources)
            {
                int resourceID = -1;
                RarityEnum rarity = RarityEnum.C;
                Util.GetRarity(s, ref resourceID, ref rarity);

                if (!_diSpawnData.ContainsKey(rarity))
                {
                    _diSpawnData[rarity] = new List<SpawnData>();
                }

                _diSpawnData[rarity].Add(new SpawnData(resourceID, t));
            }

            if (!_diSpawnData.ContainsKey(RarityEnum.C))
            {
                Util.AddToListDictionary(ref _diSpawnData, RarityEnum.C, new SpawnData(-1, t));
            }
        }

        protected List<RHTile> TilesInArea(bool onlyValid)
        {
            List<RHTile> validTiles = new List<RHTile>();
            foreach (Point v in Util.GetAllPointsInArea(_vPosition, _szDimensions, Constants.TILE_SIZE))
            {
                RHTile tile = _map.GetTileByPixelPosition(v);
                if (!onlyValid || tile.Passable())
                {
                    Util.AddUniquelyToList(ref validTiles, tile);
                }
            }

            return validTiles;
        }

        public virtual void Spawn() { }

        public virtual void Rollover(bool reset = false) { }

        protected readonly struct SpawnData
        {
            public readonly int ID;
            public readonly SpawnTypeEnum Type;

            public SpawnData(int id, SpawnTypeEnum t)
            {
                ID = id;
                Type = t;
            }
        }
    }

    public class ResourceSpawn : SpawnPoint
    {
        bool _bFreshSpawn = true;
        int _iCurrentObjects;
        int _iMin;
        int _iMax;

        public ResourceSpawn(RHMap map) : base(map)
        {
            _vPosition = map.GetTileByGridCoords(0, 0).Position.ToVector2();
            _szDimensions = new Size2(map.MapWidthTiles * Constants.TILE_SIZE, map.MapHeightTiles * Constants.TILE_SIZE);

            LoadData(map.GetMapProperties());
        }

        public ResourceSpawn(RHMap map, TiledMapObject obj) : base(map, obj)
        {
            LoadData(obj.Properties);
        }

        private void LoadData(Dictionary<string, string> props)
        {
            string resourceNumbers = "1";
            Util.AssignValue(ref resourceNumbers, "ResourcesToSpawn", props);

            string[] val = Util.FindArguments(resourceNumbers);
            _iMin = int.Parse(val[0]);

            if (val.Length > 1) { _iMax = int.Parse(val[1]); }
            else { _iMax = _iMin; }

            if (props.ContainsKey("ItemID"))
            {
                AssignSpawnData(props["ItemID"], SpawnTypeEnum.Item);
            }
            if (props.ContainsKey("ObjectID"))
            {
                AssignSpawnData(props["ObjectID"], SpawnTypeEnum.Object);
            }
        }

        public override void Spawn()
        {
            if (_bFreshSpawn)
            {
                SpawnObject(RHRandom.Instance().Next(_iMin, _iMax));
                _bFreshSpawn = false;
            }
        }

        private void SpawnObject(int number)
        {
            List<RHTile> validTiles = TilesInArea(true);
            List<RHTile> usedTiles = new List<RHTile>();

            for (int i = 0; i < number && i < validTiles.Count; i++)
            {
                //from the array as it gets filled so that we bounce less.
                RHTile targetTile = validTiles[RHRandom.Instance().Next(0, validTiles.Count - 1)];

                //If the object could not be placed, keep trying until you find one that can be
                bool objectIsValid;
                do
                {
                    objectIsValid = true;

                    SpawnData sData = Util.RollOnRarityTable(_diSpawnData);
                    if (sData.ID == -1)
                    {
                        targetTile = null;
                        continue;
                    }
                    else if (sData.Type == SpawnTypeEnum.Item)
                    {
                        var item = DataManager.GetItem(sData.ID);
                        if (item != null)
                        {
                            new WrappedItem(item.ID).PlaceOnMap(targetTile.Position, _map);
                        }
                    }
                    else
                    {
                        var wObj = DataManager.CreateWorldObjectByID(sData.ID);
                        if (wObj != null)
                        {
                            wObj.SnapPositionToGrid(new Point(targetTile.Position.X, targetTile.Position.Y));

                            if (wObj.CompareType(ObjectTypeEnum.Plant) && _bFreshSpawn)
                            {
                                ((Plant)wObj).FinishGrowth();
                            }

                            wObj.PlaceOnMap(_map);
                            _iCurrentObjects++;

                            //If the object is larger than one tile, we need to ensure it can actually fit on the tile(s) we've placed it
                            if (wObj.CollisionBox.Width > Constants.TILE_SIZE || wObj.CollisionBox.Height > Constants.TILE_SIZE)
                            {
                                foreach (RHTile t in wObj.Tiles)
                                {
                                    if (!validTiles.Contains(t) || usedTiles.Contains(t))
                                    {
                                        objectIsValid = false;
                                        wObj.RemoveSelfFromTiles();
                                        _map.RemoveWorldObject(wObj);
                                        _iCurrentObjects--;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
                while (!objectIsValid);

                if (targetTile != null)
                {
                    //Remove the targetTile once it has been properly used
                    validTiles.Remove(targetTile);

                    //Keep track of which tiles were used
                    usedTiles.Add(targetTile);
                }

                if (validTiles.Count == 0)
                {
                    break;
                }
            }
        }

        //TODO
        //Should store as string first, then convert to dictionary data during Spawn
        public bool AlertSpawnPoint(WorldObject obj)
        {
            Rectangle area = new Rectangle((int)_vPosition.X, (int)_vPosition.Y, (int)_szDimensions.Width, (int)_szDimensions.Height);
            if (area.Contains(obj.CollisionCenter))
            {
                for (int i = 0; i < Enum.GetValues(typeof(RarityEnum)).Length; i++)
                {
                    RarityEnum e = (RarityEnum)Enum.GetValues(typeof(RarityEnum)).GetValue(i);
                    if (_diSpawnData.ContainsKey(e))
                    {
                        for (int j = 0; j < _diSpawnData[e].Count; j++)
                        {
                            SpawnData data = _diSpawnData[e][j];
                            if ((obj.ID != -1 && obj.ID == data.ID) || (obj.ID == -1 && ((WrappedItem)obj).ItemID == data.ID))
                            {
                                _iCurrentObjects--;
                                return true;
                            }
                        }
                    }
                }
            }

            return false;
        }

        public override void Rollover(bool reset)
        {
            if (reset) { _bFreshSpawn = true; }
            else
            {
                if (_iCurrentObjects < _iMax)
                {
                    SpawnObject(1);
                }
            }
        }
    }

    public class MobSpawn : SpawnPoint
    {
        public MobSpawn(RHMap map, TiledMapObject obj) : base(map, obj)
        {
            if (obj.Properties.ContainsKey("MobID"))
            {
                AssignSpawnData(obj.Properties["MobID"], SpawnTypeEnum.Mob);
            }
        }

        public override void Spawn()
        {
            var copy = _diSpawnData.ToDictionary(entry => entry.Key, entry => new List<SpawnData>(entry.Value));
            foreach (var key in _diSpawnData.Keys)
            {
                foreach (SpawnData data in _diSpawnData[key])
                {
                    if (!Validate(data.ID))
                    {
                        copy[key].Remove(data);
                        if (copy[key].Count == 0)
                        {
                            copy.Remove(key);
                        }
                    }
                }
            }

            List<RHTile> validTiles = TilesInArea(true);
            SpawnData copyData = Util.RollOnRarityTable(copy);

            Mob m = DataManager.CreateMob(copyData.ID);
            RHTile t = validTiles[RHRandom.Instance().Next(0, validTiles.Count - 1)];
            _map.AddMobByPosition(m, t.Position);
            m.SetInitialPoint(t.Position);
        }

        public bool Validate(int id)
        {

            if (DataManager.GetBoolByIDKey(id, "Day", DataType.Actor) && GameCalendar.IsNight()) { return false; }
            else if (DataManager.GetBoolByIDKey(id, "Night", DataType.Actor) && !GameCalendar.IsNight()) { return false; }

            var seasonList = DataManager.GetEnumListByIDKey<SeasonEnum>(id, "Season", DataType.Actor);

            bool validSeason = seasonList[0] == SeasonEnum.None;
            if (!validSeason)
            {
                foreach (var season in seasonList)
                {
                    if (season == GameCalendar.CurrentSeason)
                    {
                        validSeason = true;
                        break;
                    }
                }
            }

            if (!validSeason)
            {
                return false;
            }

            return true;
        }
    }
}
