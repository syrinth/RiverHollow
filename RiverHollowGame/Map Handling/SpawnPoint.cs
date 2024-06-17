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

        public SpawnPoint(RHMap map, TiledMapObject obj)
        {
            _map = map;
            _diSpawnData = new Dictionary<RarityEnum, List<SpawnData>>();

            _vPosition = map.GetTileByGridCoords(Util.GetGridCoords(obj.Position.ToPoint())).Position.ToVector2();
            _szDimensions = obj.Size;
            _szDimensions = obj.Size;
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

            if (onlyValid)
            {
                _map.RemoveTilesNearTravelPoints(ref validTiles);
                _map.RemoveTilesNearSpecialObjects(ref validTiles);
            }

            return validTiles;
        }

        public Rectangle GetRectangle()
        {
            return new Rectangle((int)_vPosition.X, (int)_vPosition.Y, (int)_szDimensions.Width, (int)_szDimensions.Height);
        }

        public bool ContainsTile(RHTile t)
        {
            return (GetRectangle().Contains(t.Center));
        }

        public virtual void Spawn() { }

        public virtual void Rollover(bool reset = false) { }
    }

    public class ResourceSpawn : SpawnPoint
    {
        public bool FishingHole { get; private set; } = false;
        bool _bFreshSpawn = true;
        int _iMin;
        int _iMax;

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
                Util.AssignSpawnData(ref _diSpawnData, props["ItemID"], SpawnTypeEnum.Item);
            }
            if (props.ContainsKey("ObjectID"))
            {
                Util.AssignSpawnData(ref _diSpawnData, props["ObjectID"], SpawnTypeEnum.Object);
            }

            if (!_diSpawnData.ContainsKey(RarityEnum.C))
            {
                Util.AddToListDictionary(ref _diSpawnData, RarityEnum.C, new SpawnData(-1, SpawnTypeEnum.Object));
            }

            if (props.ContainsKey("FishingHole"))
            {
                FishingHole = true;
            }
        }

        public override void Spawn()
        {
            if (_bFreshSpawn && !FishingHole)
            {
                SpawnObject(RHRandom.Instance().Next(_iMin, _iMax));
                _bFreshSpawn = false;
            }
        }

        public int GetRandomItemID()
        {
            int rv = -1;
            SpawnData sData = Util.RollOnRarityTable(_diSpawnData);
            if (sData.Type == SpawnTypeEnum.Item)
            {
                rv = sData.ID;
            }

            return rv;
        }

        private void SpawnObject(int number)
        {
            List<RHTile> validTiles = TilesInArea(true);

            for (int i = 0; i < number; i++)
            {
                if (validTiles.Count == 0)
                {
                    break;
                }

                WorldObject obj = Util.RollOnRarityTable(_diSpawnData).GetDataObject();
                _map.PlaceGeneratedObject(obj, ref validTiles, false);
            }
        }

        public override void Rollover(bool reset)
        {
            if (reset) { _bFreshSpawn = true; }
            else
            {
                var tiles = TilesInArea(false);
                int checkSum = 0;

                for (int i = 0; i < tiles.Count; i++)
                {
                    if (tiles[i].WorldObject != null)
                    {
                        var obj = tiles[i].WorldObject;
                        if(obj.ID == -1)
                        {
                            var wrappedItem = (WrappedItem)obj;
                            CheckData(wrappedItem.ItemID, SpawnTypeEnum.Item, ref checkSum);
                        }
                        else
                        {
                            CheckData(obj.ID, SpawnTypeEnum.Object, ref checkSum);
                        }
                    }
                }

                if (checkSum < _iMax)
                {
                    SpawnObject(1);
                }
            }
        }

        private void CheckData(int id, SpawnTypeEnum type, ref int checkSum)
        {
            foreach (var value in _diSpawnData.Values)
            {
                foreach (var data in value)
                {
                    if (data.Type == type && id == data.ID)
                    {
                        checkSum++;
                    }
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
                Util.AssignSpawnData(ref _diSpawnData, obj.Properties["MobID"], SpawnTypeEnum.Mob);
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

            if (copy.Count > 0)
            {
                List<RHTile> validTiles = TilesInArea(true);
                if (validTiles.Count > 0)
                {
                    SpawnData copyData = Util.RollOnRarityTable(copy);

                    Mob m = DataManager.CreateActor<Mob>(copyData.ID);
                    RHTile t = validTiles[RHRandom.Instance().Next(0, validTiles.Count - 1)];
                    _map.AddMobByPosition(m, t.Position);
                    m.SetInitialPoint(t.Position);
                }
            }
        }

        public bool Validate(int id)
        {
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
