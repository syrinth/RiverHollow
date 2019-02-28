using RiverHollow.WorldObjects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using static RiverHollow.WorldObjects.WorldItem;
using static RiverHollow.WorldObjects.WorldItem.Machine;

using static RiverHollow.Game_Managers.GameManager;
using RiverHollow.Misc;
using static RiverHollow.WorldObjects.Door;
using RiverHollow.Actors;
using RiverHollow.Buildings;

namespace RiverHollow.Game_Managers
{
    public static class ObjectManager
    {
        private static Dictionary<int, string> _dictBuilding;
        private static Dictionary<int, Dictionary<string, string>> _dictItem;
        private static Dictionary<int, string> _dictWorkers;
        private static Dictionary<int, string> _dictWorldObjects;

        public static void LoadContent(ContentManager Content)
        {
            _dictItem = new Dictionary<int, Dictionary<string, string>>();
            _dictBuilding = Content.Load<Dictionary<int, string>>(@"Data\Buildings");
            _dictWorkers = Content.Load<Dictionary<int, string>>(@"Data\Workers");
            _dictWorldObjects = Content.Load<Dictionary<int, string>>(@"Data\WorldObjects");

            Dictionary<int, string> itemData = Content.Load<Dictionary<int, string>>(@"Data\ItemData");
            foreach(KeyValuePair<int, string> kvp in itemData)
            {
                Dictionary<string, string> dss = new Dictionary<string, string>();
                foreach (string s in Util.FindTags(kvp.Value))
                {
                    string[] tagSplit = s.Split(':');
                    dss[tagSplit[0]] = tagSplit[1];
                }
                _dictItem[kvp.Key] = dss;
            }
        }

        public static Building GetBuilding(int id)
        {
            if (_dictBuilding.ContainsKey(id))
            {
                string buildingData = _dictBuilding[id];
                string[] _buildingDataValues = Util.FindTags(buildingData);
                return new Building(_buildingDataValues, id);
            }
            return null;     
        }

        public static Building GetManor()
        {
            return new Building(Util.FindTags(_dictBuilding[0]), 0);
        }

        public static WorldAdventurer GetWorker(int id)
        {
            if (_dictWorkers.ContainsKey(id))
            {
                string stringData = _dictWorkers[id];
                string[] stringDataValues = Util.FindTags(stringData);
                return new WorldAdventurer(stringDataValues, id);
            }
            return null;
        }

        public static Item GetItem(int id)
        {
            return GetItem(id, 1);
        }

        public static Item GetItem(int id, int num)
        {
            if (id != -1)
            {
                Dictionary<string, string> liData = _dictItem[id];
                switch (liData["Type"])
                {
                    case "Resource":
                        return new Item(id, liData, num);
                    case "Tool":
                        return new Tool(id, liData);
                    case "Equipment":
                        return new Equipment(id, liData);
                    case "StaticItem":
                        return new StaticItem(id, liData);
                    case "Food":
                        return new Food(id, liData, num);
                    case "Map":
                        return new AdventureMap(id, liData, num);
                    case "Combat":
                        return new CombatItem(id, liData, num);
                    case "Class":
                        return new ClassItem(id, liData, num);
                    case "Marriage":
                        return new MarriageItem(id, liData);
                    case "Clothes":
                        return new Clothes(id, liData);
                }
            }
            return null;
        }

        public static WorldObject GetWorldObject(int id)
        {
            return GetWorldObject(id, Vector2.Zero);
        }
        public static WorldObject GetWorldObject(int id, Vector2 pos)
        {
            if (id != -1)
            {
                string _stringData = _dictWorldObjects[id];
                string[] _stringDataValues = Util.FindTags(_stringData);
                switch (_stringDataValues[0].Split(':')[1])
                {
                    case "Destructible":
                        return new Destructible(id, _stringDataValues, pos);
                    case "Tree":
                        return new Tree(id, pos, new Rectangle(0, 0, 48, 80), GetTexture(@"Textures\tree"), TileSize * 3, TileSize * 5, false, true, 1, 10);
                    case "Staircase":
                        return new Staircase(id, pos, new Rectangle(96, 0, 16, 16), GetTexture(@"Textures\worldObjects"), TileSize, TileSize);
                    case "Container":
                        return new Container(id, _stringDataValues);
                    case "Processor":
                        return new Processor(id, _stringDataValues);
                    case "ClassChanger":
                        return new ClassChanger(id, pos);
                    case "Plant":
                        return new Plant(id, _stringDataValues);
                    case "Crafter":
                        return new Crafter(id, _stringDataValues);
                }
            }

            return null;
        }

        public static Door GetDoor(string doorType, Vector2 pos)
        {
            if (doorType.Equals("MobDoor"))
            {
                return new MobDoor(Util.SnapToGrid(pos), new Rectangle(64, 0, 16, 32), GetTexture(@"Textures\worldObjects"), TileSize, TileSize * 2);
            }
            else if (doorType.Equals("KeyDoor"))
            {
                return new KeyDoor(Util.SnapToGrid(pos), new Rectangle(64, 0, 16, 32), GetTexture(@"Textures\worldObjects"), TileSize, TileSize * 2);
            }
            else if (doorType.Equals("SeasonDoor"))
            {
                return new SeasonDoor(Util.SnapToGrid(pos), new Rectangle(64, 0, 16, 32), GetTexture(@"Textures\worldObjects"), TileSize, TileSize * 2);
            }
            return null;
        }

        private static Texture2D GetTexture(string texture)
        {
            return GameContentManager.GetTexture(texture);
        }

        public static int GetWorkerNum()
        {
            return _dictWorkers.Count;
        }
    }
}
