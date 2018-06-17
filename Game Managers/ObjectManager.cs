using RiverHollow.Characters.NPCs;
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

namespace RiverHollow.Game_Managers
{
    public static class ObjectManager
    {
        private static Dictionary<int, string> _dictBuilding;
        private static Dictionary<int, Recipe> _dictCrafting;
        private static Dictionary<int, string> _dictItem;
        private static Dictionary<int, string> _dictWorkers;
        private static Dictionary<int, string> _dictWorldObjects;
        public static Dictionary<int, Recipe> DictCrafting { get => _dictCrafting; }

        public static void LoadContent(ContentManager Content)
        {
            _dictBuilding = Content.Load<Dictionary<int, string>>(@"Data\Buildings");
            _dictItem = Content.Load<Dictionary<int, string>>(@"Data\ItemData");
            _dictWorkers = Content.Load<Dictionary<int, string>>(@"Data\Workers");
            _dictWorldObjects = Content.Load<Dictionary<int, string>>(@"Data\WorldObjects");
            LoadRecipes(Content);
        }

        private static void LoadRecipes(ContentManager Content)
        {
            _dictCrafting = new Dictionary<int, Recipe>();
            foreach (KeyValuePair<int, string> kvp in Content.Load<Dictionary<int, string>>(@"Data\CraftingData"))
            {
                _dictCrafting.Add(kvp.Key, new Recipe(kvp.Key, kvp.Value));
            }
        }

        public static WorkerBuilding GetBuilding(int id)
        {
            if (_dictBuilding.ContainsKey(id))
            {
                string buildingData = _dictBuilding[id];
                string[] _buildingDataValues = Util.FindTags(buildingData);
                return new WorkerBuilding(_buildingDataValues, id);
            }
            return null;     
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
                string _itemData = _dictItem[id];
                string[] _itemTags = Util.FindTags(_itemData);
                switch (_itemTags[0].Split(':')[1])
                {
                    case "Resource":
                        return new Item(id, _itemTags, num);
                    case "Tool":
                        return new Tool(id, _itemTags);
                    case "Equipment":
                        return new Equipment(id, _itemTags);
                    case "StaticItem":
                        return new StaticItem(id, _itemTags);
                    case "Food":
                        return new Food(id, _itemTags, num);
                    case "Map":
                        return new AdventureMap(id, _itemTags, num);
                    case "Combat":
                        return new CombatItem(id, _itemTags, num);
                    case "Class":
                        return new ClassItem(id, _itemTags, num);
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
                        return new Tree(id, pos, new Rectangle(0, 0, 48, 64), GetTexture(@"Textures\tree"), TileSize * 3, TileSize * 4, false, true, 1, 10);
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


        public class Recipe
        {
            private int _iXP;
            public int XP => _iXP;
            private int _iProcessingTime;
            public int ProcessingTime => _iProcessingTime;
            private int _iOutput;
            public int Output => _iOutput;
            private Dictionary<int, int> _requiredItems;
            public Dictionary<int, int> RequiredItems { get => _requiredItems; }

            public Recipe(int id, string stringData)
            {
                _iOutput = id;
                _requiredItems = new Dictionary<int, int>();

                string[] splitData = Util.FindTags(stringData);
                foreach (string s in splitData)
                {
                    string[] tagType = s.Split(':');
                    if (tagType[0].Equals("Time"))
                    {
                        _iProcessingTime = int.Parse(tagType[1]);
                    }
                    else if (tagType[0].Equals("XP"))
                    {
                        _iXP = int.Parse(tagType[1]);
                    }
                    else if (tagType[0].Equals("ReqItem"))
                    {
                        string[] itemParams = tagType[1].Split('-');
                        _requiredItems.Add(int.Parse(itemParams[0]), int.Parse(itemParams[1]));
                    }
                }
            }

            public override string ToString()
            {
                return ObjectManager.GetItem(_iOutput).Name;
            }
        }
    }
}
