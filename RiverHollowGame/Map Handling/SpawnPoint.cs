using Microsoft.Xna.Framework;
using MonoGame.Extended.Tiled;
using RiverHollow.Game_Managers;
using RiverHollow.Utilities;
using RiverHollow.WorldObjects;
using System.Collections.Generic;
using static RiverHollow.Utilities.Enums;
using static RiverHollow.Game_Managers.GameManager;
using MonoGame.Extended;
using RiverHollow.Characters;
using System;
using System.Linq;

namespace RiverHollow.Map_Handling
{
    public abstract class SpawnPoint
    {
        protected RHMap _map;
        protected Size2 _szDimensions;
        protected Vector2 _vPosition;
        protected Dictionary<RarityEnum, List<SpawnData>> _diSpawnData;

        public SpawnPoint(RHMap map, TiledMapObject obj)
        {
            _diSpawnData = new Dictionary<RarityEnum, List<SpawnData>>();

            _map = map;
            _vPosition = map.GetTileByGridCoords(Util.GetGridCoords(obj.Position)).Position;
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
        }

        protected List<RHTile> TilesInArea(bool onlyValid)
        {
            List<RHTile> validTiles = new List<RHTile>();
            foreach (Vector2 v in Util.GetAllPointsInArea(_vPosition, _szDimensions, Constants.TILE_SIZE))
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
        bool _bHasSpawned = false;
        int _iRespawnCountdown = 0;
        int _iCurrent;
        readonly int _iMin;
        readonly int _iMax;

        public ResourceSpawn(RHMap map, TiledMapObject obj) : base(map, obj)
        {
            string resourceNumbers = string.Empty;
            if (obj.Properties.ContainsKey("Number")) { resourceNumbers = obj.Properties["Number"]; }
            else { resourceNumbers = "1"; }

            string[] val = resourceNumbers.Split('-');
            _iMin = int.Parse(val[0]);

            if (val.Length > 1) { _iMax = int.Parse(val[1]); }
            else { _iMax = _iMin; }

            if (obj.Properties.ContainsKey("ItemID"))
            {
                AssignSpawnData(obj.Properties["ItemID"], SpawnTypeEnum.Item);
            }
            if (obj.Properties.ContainsKey("ObjectID"))
            {
                AssignSpawnData(obj.Properties["ObjectID"], SpawnTypeEnum.Object);
            }
        }

        public override void Spawn()
        {
            if (!_bHasSpawned)
            {
                _bHasSpawned = true;
                _iCurrent = RHRandom.Instance().Next(_iMin, _iMax);
                SpawnObject(_iCurrent);
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
                    RarityEnum rarityKey = Util.RollAgainstRarity(_diSpawnData);

                    int roll = RHRandom.Instance().Next(0, _diSpawnData[rarityKey].Count - 1);

                    SpawnData sData = _diSpawnData[rarityKey][roll];

                    if (sData.Type == SpawnTypeEnum.Item)
                    {
                        new WrappedItem(sData.ID).PlaceOnMap(targetTile.Position, _map);
                    }
                    else
                    {
                        WorldObject wObj = DataManager.CreateWorldObjectByID(sData.ID);
                        wObj.SnapPositionToGrid(new Vector2(targetTile.Position.X, targetTile.Position.Y));

                        if (wObj.CompareType(ObjectTypeEnum.Plant) && !_bHasSpawned)
                        {
                            ((Plant)wObj).FinishGrowth();
                        }

                        wObj.PlaceOnMap(_map);

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
                                    break;
                                }
                            }
                        }
                    }
                }
                while (!objectIsValid);

                //Remove the targetTile once it has been properly used
                validTiles.Remove(targetTile);

                //Keep track of which tiles were used
                usedTiles.Add(targetTile);

                if (validTiles.Count == 0)
                {
                    break;
                }
            }
        }

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
                                if (_iRespawnCountdown == 0) { _iRespawnCountdown = 3; }
                                _iCurrent--;

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
            if (reset) { _bHasSpawned = false; }
            else
            {
                if (_iCurrent < _iMax)
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

            RarityEnum rarityKey = Util.RollAgainstRarity(_diSpawnData);
            if (copy.ContainsKey(rarityKey) && copy[rarityKey].Count > 0)
            {
                List<RHTile> validTiles = TilesInArea(true);

                int roll = RHRandom.Instance().Next(0, copy[rarityKey].Count - 1);
                Mob m = DataManager.CreateMob(copy[rarityKey][roll].ID);
                RHTile t = validTiles[RHRandom.Instance().Next(0, validTiles.Count - 1)];
                _map.AddMobByPosition(m, t.Position);
            }
        }

        public bool Validate(int id)
        {

            if (DataManager.GetBoolByIDKey(id, "Day", DataType.NPC) && GameCalendar.IsNight()) { return false; }
            else if (DataManager.GetBoolByIDKey(id, "Night", DataType.NPC) && !GameCalendar.IsNight()) { return false; }

            var seasonList = DataManager.GetEnumListByIDKey<SeasonEnum>(id, "Season", DataType.NPC);

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
