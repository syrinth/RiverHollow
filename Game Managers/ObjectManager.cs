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
using RiverHollow.Actors.CombatStuff;
using System.IO;
using RiverHollow.Characters;

namespace RiverHollow.Game_Managers
{
    public static class ObjectManager
    {
        static Dictionary<int, Dictionary<string, string>> _diVillagerData;

        static Dictionary<int, Dictionary<string, string>> _diBuildings;
        static Dictionary<int, Dictionary<string, string>> _diItemData;
        static Dictionary<int, Dictionary<string, string>> _diStatusEffects;
        static Dictionary<int, string> _diWorkers;
        static Dictionary<int, Dictionary<string, string>> _diWorldObjects;

        static Dictionary<int, Dictionary<string, string>> _diMonsterData;
        static Dictionary<int, Dictionary<string, string>> _diSummonData;
        static Dictionary<int, Villager> _diNPCs;
        public static Dictionary<int, Villager> DiNPC { get => _diNPCs; }
        private static Dictionary<int, Dictionary<string, string>> _diActions;
        
        private static Dictionary<int, string> _diClasses;
        private static Dictionary<string, Dictionary<string, string>> _diSchedule;

        static List<int> _liForest;
        static List<int> _liMountain;
        static List<int> _liNight;

        public static void LoadContent(ContentManager Content)
        {
            _diVillagerData = new Dictionary<int, Dictionary<string, string>>();
            _diItemData = new Dictionary<int, Dictionary<string, string>>();
            _diActions = new Dictionary<int, Dictionary<string, string>>();
            _diWorldObjects = new Dictionary<int, Dictionary<string, string>>();
            _diMonsterData = new Dictionary<int, Dictionary<string, string>>();
            _diSummonData = new Dictionary<int, Dictionary<string, string>>();
            _diBuildings = new Dictionary<int, Dictionary<string, string>>();
            _diStatusEffects = new Dictionary<int, Dictionary<string, string>>();
            _diWorkers = Content.Load<Dictionary<int, string>>(@"Data\Workers");

            AddToDictionary(_diItemData, @"Data\ItemData", Content);
            AddToDictionary(_diWorldObjects, @"Data\WorldObjects", Content);
            AddToDictionary(_diActions, @"Data\CombatActions", Content);
            AddToDictionary(_diVillagerData, @"Data\NPCData\Characters", Content);
            AddToDictionary(_diMonsterData, @"Data\Monsters", Content);
            AddToDictionary(_diSummonData, @"Data\Summons", Content);
            AddToDictionary(_diBuildings, @"Data\Buildings", Content);
            AddToDictionary(_diStatusEffects, @"Data\StatusEffects", Content);

            _liForest = new List<int>();
            _liMountain = new List<int>();
            _liNight = new List<int>();
            _diSchedule = new Dictionary<string, Dictionary<string, string>>();

            _diClasses = Content.Load<Dictionary<int, string>>(@"Data\Classes");

            foreach (string s in Directory.GetFiles(@"Content\Data\NPCData\Schedules"))
            {
                string temp = Path.GetFileNameWithoutExtension(s);
                _diSchedule.Add(temp, Content.Load<Dictionary<string, string>>(@"Data\NPCData\Schedules\" + temp));
            }

            _diNPCs = new Dictionary<int, Villager>();
            foreach (KeyValuePair<int, Dictionary<string, string>> npcData in _diVillagerData)
            {
                Villager n = null;

                Dictionary<string, string> diData = _diVillagerData[npcData.Key];
                switch (diData["Type"])
                {
                    case "ShopKeeper":
                        n = new ShopKeeper(npcData.Key, diData);
                        break;
                    case "Eligible":
                        n = new EligibleNPC(npcData.Key, diData);
                        break;
                    default:
                        n = new Villager(npcData.Key, diData);
                        break;
                }
                _diNPCs.Add(npcData.Key, n);
            }
        }

        //@"Data\ItemData"
        private static void AddToDictionary(Dictionary<int, Dictionary<string, string>> dictionaryAddTo, string dataFile, ContentManager Content)
        {
            Dictionary<int, string> dictionaryData = Content.Load<Dictionary<int, string>>(dataFile);
            foreach (KeyValuePair<int, string> kvp in dictionaryData)
            {
                Dictionary<string, string> dss = new Dictionary<string, string>();
                foreach (string s in Util.FindTags(kvp.Value))
                {
                    if (s.Contains(":"))
                    {
                        string[] tagSplit = s.Split(':');
                        dss[tagSplit[0]] = tagSplit[1];
                    }
                    else {
                        dss[s] = "";
                    }
                }
                dictionaryAddTo[kvp.Key] = dss;
            }
        }

        public static Building GetBuilding(int id)
        {
            if (_diBuildings.ContainsKey(id))
            {
                return new Building(_diBuildings[id], id);
            }
            return null;
        }

        /// <summary>
        /// The Manor is Building 0, so make a new building from the data
        /// </summary>
        /// <returns>A new building representing the Manor house.</returns>
        public static Building GetManor()
        {
            return new Building(_diBuildings[0], 0);
        }

        public static Adventurer GetWorker(int id)
        {
            if (_diWorkers.ContainsKey(id))
            {
                string stringData = _diWorkers[id];
                string[] stringDataValues = Util.FindTags(stringData);
                return new Adventurer(stringDataValues, id);
            }
            return null;
        }

        public static Item GetItem(int id)
        {
            return GetItem(id, 1);
        }

        public static Item GetItem(int id, int num)
        {
            if (id != -1 && _diItemData.ContainsKey(id))
            {
                Dictionary<string, string> liData = _diItemData[id];
                switch (liData["Type"])
                {
                    case "Resource":
                        return new Item(id, liData, num);
                    case "Tool":
                        return new Tool(id, liData);
                    case "Equipment":
                        return new Equipment(id, liData);
                    case "StaticItem":
                        return new StaticItem(id, liData, num);
                    case "Food":
                        return new Food(id, liData, num);
                    case "Map":
                        return new AdventureMap(id, liData, num);
                    case "Consumable":
                        return new Consumable(id, liData, num);
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
            if (id != -1 && _diWorldObjects.ContainsKey(id))
            {
                Dictionary<string, string> liData = _diWorldObjects[id];
                switch (liData["Type"])
                {
                    case "Destructible":
                        return new Destructible(id, liData, pos);
                    case "Tree":
                        return new Tree(id, liData, pos);
                    case "Staircase":
                        return new Staircase(id, pos, TileSize, TileSize);
                    case "Container":
                        return new Container(id, liData);
                    case "ClassChanger":
                        return new ClassChanger(id, pos);
                    case "Plant":
                        return new Plant(id, liData);
                    case "Machine":
                        return new Machine(id, liData);
                    case "Wall":
                        return new Wall(id, liData, pos);
                    case "Floor":
                        return new Floor(id, liData, pos);
                    case "Forageable":
                        return new Forageable(id, liData, pos);
                    case "EchoNode":
                        return new EchoNode(id, liData, pos);
                    case "Light":
                        return new Light(id, liData, pos);
                }
            }

            return null;
        }

        public static Door GetDoor(string doorType, Vector2 pos)
        {
            if (doorType.Equals("MobDoor"))
            {
                return new MobDoor(Util.SnapToGrid(pos), TileSize, TileSize * 2);
            }
            else if (doorType.Equals("KeyDoor"))
            {
                return new KeyDoor(Util.SnapToGrid(pos), TileSize, TileSize * 2);
            }
            else if (doorType.Equals("SeasonDoor"))
            {
                return new SeasonDoor(Util.SnapToGrid(pos), TileSize, TileSize * 2);
            }
            return null;
        }

        private static Texture2D GetTexture(string texture)
        {
            return GameContentManager.GetTexture(texture);
        }

        public static int GetWorkerNum()
        {
            return _diWorkers.Count;
        }

        public static string GetCharacterNameByIndex(int i)
        {
            return _diNPCs[i].Name;
        }

        public static Summon GetSummonByIndex(int id)
        {
            Summon m = null;

            if (_diSummonData.ContainsKey(id))
            {
                m = new Summon(id, _diSummonData[id]);
            }
            return m;
        }

        public static Monster GetMonsterByIndex(int id)
        {
            Monster m = null;

            if (_diMonsterData.ContainsKey(id))
            {
                m = new Monster(id, _diMonsterData[id]);
            }
            return m;
        }

        public static Monster GetMonster(int id, Vector2 pos)
        {
            Monster m = GetMonsterByIndex(id);
            m.Position = pos;
            return m;
        }

        public static MenuAction GetActionByIndex(int id)
        {
            if (id != -1)
            {
                Dictionary<string, string> liData = _diActions[id];
                switch (liData["Type"])
                {
                    case "Menu":
                        return new MenuAction(id, liData);
                    case "Spell":
                        return new CombatAction(id, liData);
                    case "Action":
                        return new CombatAction(id, liData);
                }
            }

            return null;
        }
        public static StatusEffect GetStatusEffectByIndex(int id)
        {
            StatusEffect b = null;
            if (id != -1)
            {
                b = new StatusEffect(id, _diStatusEffects[id]);
            }
            return b;
        }

        public static int GetClassCount()
        {
            return _diClasses.Count;
        }
        public static CharacterClass GetClassByIndex(int id)
        {
            CharacterClass c = null;
            if (id != -1)
            {
                string strData = _diClasses[id];
                string[] strDataValues = Util.FindTags(strData);
                c = new CharacterClass(id, strDataValues);
            }
            return c;
        }

        public static Dictionary<string, string> GetSchedule(string npc)
        {
            Dictionary<string, string> rv = null;
            if (_diSchedule.ContainsKey(npc))
            {
                rv = _diSchedule[npc];
            }

            return rv;
        }

        public static void Rollover()
        {
            foreach (Villager n in _diNPCs.Values)
            {
                n.RollOver();
            }
        }

        #region Spawn Code
        public static void AddToForest(int ID) { _liForest.Add(ID); }
        public static void AddToMountain(int ID) { _liMountain.Add(ID); }
        public static void AddToNight(int ID) { _liNight.Add(ID); }
        internal static Monster GetMonsterToSpawn(SpawnConditionEnum eSpawnType)
        {
            List<Monster> allowedMobs = new List<Monster>();

            //foreach(Mob m in _diMobs.Values)
            //{
            //    if (m.CheckValidConditions(eSpawnType)){
            //        allowedMobs.Add(m);
            //    }
            //}

            return GetMonsterByIndex(4);// new RHRandom().Next(1, allowedMobs.Count-1));
        }

        #endregion
    }
}