using RiverHollow.WorldObjects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using static RiverHollow.WorldObjects.WorldItem;
using static RiverHollow.Game_Managers.GameManager;
using RiverHollow.Misc;
using RiverHollow.Actors;
using RiverHollow.Buildings;
using RiverHollow.Actors.CombatStuff;
using System.IO;
using RiverHollow.Characters;
using MonoGame.Extended.BitmapFonts;
using static RiverHollow.WorldObjects.DungeonObject;

namespace RiverHollow.Game_Managers
{
    public static class DataManager
    {
        #region Constants
        public const string FILE_WORLDOBJECTS = @"Textures\worldObjects";
        public const string FILE_FLOORING = @"Textures\texFlooring";
        public const string FOLDER_ACTOR = @"Textures\Actors\";
        public const string FOLDER_BUILDINGS = @"Textures\Buildings\";
        public const string FOLDER_ITEMS = @"Textures\Items\";
        public const string FOLDER_MONSTERS = @"Textures\Actors\Monsters\";
        public const string FOLDER_PLAYER = @"Textures\Actors\Player\";
        public const string FOLDER_TEXTFILES = @"Data\Text Files\";
        public const string FONT_MAIN = @"Fonts\Font_Main";
        public const string FONT_NUMBER_DISPLAY = @"Fonts\Font_Number_Display";
        public const string FONT_STAT_DISPLAY = @"Fonts\Font_Stat_Display";
        #endregion

        #region Dictionaries
        static Dictionary<string, Texture2D> _diTextures;
        static Dictionary<string, BitmapFont> _diBMFonts;
        static Dictionary<string, string> _diGameText;
        static Dictionary<int, string> _diMonsterInfo;
        static Dictionary<int, string> _diStatusEffectText;
        static Dictionary<string, string> _diCombatSkillsText;
        static Dictionary<string, string> _diAdventurerDialogue;

        static Dictionary<int, string> _diUpgrades;
        public static Dictionary<int, string> DiUpgrades  => _diUpgrades;
        static Dictionary<int, string> _diQuests;
        public static Dictionary<int, string> DiQuests => _diQuests;
        static Dictionary<int, string> _diItemText;
        static Dictionary<int, string> _diClassText;
        static Dictionary<string, string> _diMonsterTraits;

        static Dictionary<int, List<string>> _diSongs;
        static Dictionary<int, Dictionary<string, string>> _diNPCDialogue;
        static Dictionary<string, Dictionary<int, string>> _diMerchandise;

        static Dictionary<int, Dictionary<string, string>> _diVillagerData;
        static Dictionary<int, Dictionary<string, string>> _diPlayerAnimationData;
        public static Dictionary<int, Dictionary<string, string>> PlayerAnimationData => _diPlayerAnimationData;

        static Dictionary<int, Dictionary<string, string>> _diBuildings;
        static Dictionary<int, Dictionary<string, string>> _diItemData;
        static Dictionary<int, Dictionary<string, string>> _diStatusEffects;
        static Dictionary<int, Dictionary<string, string>> _diWorkers;
        static Dictionary<int, Dictionary<string, string>> _diWorldObjects;

        static Dictionary<int, Dictionary<string, string>> _diSpiritInfo;
        public static Dictionary<int, Dictionary<string, string>> DiSpiritInfo => _diSpiritInfo;

        static Dictionary<int, Dictionary<string, string>> _diMonsterData;
        static Dictionary<int, Dictionary<string, string>> _diSummonData;
        static Dictionary<int, Villager> _diNPCs;
        public static Dictionary<int, Villager> DiNPC => _diNPCs;
        static Dictionary<int, Dictionary<string, string>> _diActions;
        
        static Dictionary<int, string> _diClasses;
        static Dictionary<string, Dictionary<string, string>> _diSchedule;
        #endregion

        public static BitmapFont _bmFont;
        static List<int> _liForest;
        static List<int> _liMountain;
        static List<int> _liNight;

        public static void LoadContent(ContentManager Content)
        {
            //Allocate Dictionaries
            _diTextures = new Dictionary<string, Texture2D>();
            _diUpgrades = Content.Load<Dictionary<int, string>>(@"Data\TownUpgrades");
            _diQuests = Content.Load<Dictionary<int, string>>(@"Data\Quests");
            _diMonsterTraits = Content.Load<Dictionary<string, string>>(@"Data\MonsterTraitTable");
            _diClasses = Content.Load<Dictionary<int, string>>(@"Data\Classes");

            //Read in Content and allocate the appropriate Dictionaries
            LoadGUIs(Content);
            LoadIcons(Content);
            LoadBMFonts(Content);
            LoadTextFiles(Content);
            LoadCharacters(Content);
            LoadMerchandise(Content);
            LoadDictionaries(Content);

            LoadNPCSchedules(Content);
            LoadNPCs(Content);

            AddDirectoryTextures(FOLDER_BUILDINGS, Content);
            AddDirectoryTextures(FOLDER_ITEMS, Content);

            _liForest = new List<int>();
            _liMountain = new List<int>();
            _liNight = new List<int>();
        }

        #region Load Methods
        private static void LoadDictionaries(ContentManager Content)
        {
            LoadDictionary(ref _diPlayerAnimationData, @"Data\PlayerClassAnimationConfig", Content);
            LoadDictionary(ref _diItemData, @"Data\ItemData", Content);
            LoadDictionary(ref _diWorldObjects, @"Data\WorldObjects", Content);
            LoadDictionary(ref _diActions, @"Data\CombatActions", Content);
            LoadDictionary(ref _diVillagerData, @"Data\CharacterData", Content);
            LoadDictionary(ref _diMonsterData, @"Data\Monsters", Content);
            LoadDictionary(ref _diSummonData, @"Data\Summons", Content);
            LoadDictionary(ref _diBuildings, @"Data\Buildings", Content);
            LoadDictionary(ref _diStatusEffects, @"Data\StatusEffects", Content);
            LoadDictionary(ref _diWorkers, @"Data\Workers", Content);
            LoadDictionary(ref _diSpiritInfo, @"Data\SpiritInfo", Content);
        }
        private static void LoadDictionary(ref Dictionary<int, Dictionary<string, string>> dictionaryAddTo, string dataFile, ContentManager Content)
        {
            dictionaryAddTo = new Dictionary<int, Dictionary<string, string>>();
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
                    else
                    {
                        dss[s] = "";
                    }
                }
                dictionaryAddTo[kvp.Key] = dss;
            }
        }

        private static void LoadTextFiles(ContentManager Content)
        {
            _diItemText = Content.Load<Dictionary<int, string>>(FOLDER_TEXTFILES + "ItemText");
            _diGameText = Content.Load<Dictionary<string, string>>(FOLDER_TEXTFILES + "GameText");
            _diMonsterInfo = Content.Load<Dictionary<int, string>>(FOLDER_TEXTFILES + "MonsterInfo");
            _diStatusEffectText = Content.Load<Dictionary<int, string>>(FOLDER_TEXTFILES + "StatusText");
            _diCombatSkillsText = Content.Load<Dictionary<string, string>>(FOLDER_TEXTFILES + "CombatSkillsText");

            _diSongs = Content.Load<Dictionary<int, List<string>>>(@"Data\Songs");
            _diAdventurerDialogue = Content.Load<Dictionary<string, string>>(FOLDER_TEXTFILES + @"Dialogue\Adventurers");
            _diNPCDialogue = new Dictionary<int, Dictionary<string, string>>();
            foreach (string s in Directory.GetFiles(@"Content\" + FOLDER_TEXTFILES + "Dialogue"))
            {
                string fileName = Path.GetFileName(s).Replace("NPC_", "").Split('.')[0];
                int file = -1;
                if (int.TryParse(fileName, out file))
                {
                    fileName = s;
                    Util.ParseContentFile(ref fileName);
                    _diNPCDialogue.Add(file, Content.Load<Dictionary<string, string>>(fileName));
                }
            }
        }
        private static void LoadCharacters(ContentManager Content)
        {
            AddDirectoryTextures(FOLDER_ACTOR, Content);
            AddTexture(@"Textures\texFlooring", Content);
            AddTexture(@"Textures\texWeather", Content);
            AddTexture(@"Textures\lightmask", Content);
            AddTexture(@"Textures\texClothes", Content);
        }
        private static void LoadGUIs(ContentManager Content)
        {
            AddTexture(@"Textures\Dialog", Content);
            AddTexture(@"Textures\Valley", Content);
        }
        private static void LoadIcons(ContentManager Content)
        {
            AddDirectoryTextures(@"Textures\ActionEffects", Content);
            AddTexture(@"Textures\battle", Content);
            AddTexture(@"Textures\worldObjects", Content);
            AddTexture(@"Textures\portraits", Content);
            AddTexture(@"Textures\tree", Content);
            AddTexture(@"Textures\DarkWoodTree", Content);
            AddTexture(@"Textures\items", Content);
            AddTexture(@"Textures\AbilityAnimations", Content);
            AddTexture(@"Textures\texMachines", Content);
            AddTexture(@"Textures\texCmbtActions", Content);
        }
        private static void AddDirectoryTextures(string directory, ContentManager Content, bool AddContent = true)
        {
            string folder = AddContent ? @"Content\" + directory : directory;
            foreach (string s in Directory.GetFiles(folder))
            {
                AddTexture(s, Content);
            }
            foreach (string s in Directory.GetDirectories(folder))
            {
                AddDirectoryTextures(s, Content, false);
            }
        }
        private static void AddTexture(string texture, ContentManager Content)
        {
            Util.ParseContentFile(ref texture);
            _diTextures.Add(texture, Content.Load<Texture2D>(texture));
        }


        private static void LoadBMFonts(ContentManager Content)
        {
            _diBMFonts = new Dictionary<string, BitmapFont>();
            AddBMFont(@"Fonts\FontBattle", Content);
            AddBMFont(FONT_MAIN, Content);
            AddBMFont(FONT_NUMBER_DISPLAY, Content);
            AddBMFont(FONT_STAT_DISPLAY, Content);
        }
        private static void AddBMFont(string font, ContentManager Content)
        {
            _diBMFonts.Add(font, Content.Load<BitmapFont>(font));
        }

        private static void LoadMerchandise(ContentManager Content)
        {
            _diMerchandise = new Dictionary<string, Dictionary<int, string>>();
            LoadMerchFile(@"Data\Shops\Buildings", Content);
            LoadMerchFile(@"Data\Shops\Adventurers", Content);
            LoadMerchFile(@"Data\Shops\MagicShop", Content);
        }
        private static void LoadMerchFile(string file, ContentManager Content)
        {
            _diMerchandise.Add(file.Replace(@"Data\Shops\", ""), Content.Load<Dictionary<int, string>>(file));
        }

        private static void LoadNPCs(ContentManager Content)
        {
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
                    case "ShippingGremlin":
                        GameManager.ShippingGremlin = new ShippingGremlin(npcData.Key, diData);
                        n = GameManager.ShippingGremlin;
                        break;
                    default:
                        n = new Villager(npcData.Key, diData);
                        break;
                }
                _diNPCs.Add(npcData.Key, n);
            }
        }
        private static void LoadNPCSchedules(ContentManager Content)
        {
            _diSchedule = new Dictionary<string, Dictionary<string, string>>();
            foreach (string s in Directory.GetFiles(@"Content\Data\Schedules"))
            {
                string temp = Path.GetFileNameWithoutExtension(s);
                _diSchedule.Add(temp, Content.Load<Dictionary<string, string>>(@"Data\Schedules\" + temp));
            }
        }
        #endregion

        #region GetMethods
        public static void GetBuildingText(int id, ref string name, ref string desc)
        {
            string val = "Building " + id;
            name = _diGameText[val].Split('/')[0];
            desc = _diGameText[val].Split('/')[1];
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

        public static string GetAdventurerDialogue(string key)
        {
            string rv = string.Empty;
            if (_diAdventurerDialogue.ContainsKey(key))
            {
                rv = _diAdventurerDialogue[key];
            }

            return rv;
        }
        public static Adventurer GetAdventurer(int id)
        {
            if (_diWorkers.ContainsKey(id))
            {
                return new Adventurer(_diWorkers[id], id);
            }
            return null;
        }

        public static void GetItemText(int id, ref string name, ref string desc)
        {
            name = _diItemText[id].Split('/')[0];
            desc = _diItemText[id].Split('/')[1];
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
                    case "EchoNode":
                        return new EchoNode(id, liData, pos);
                    case "Light":
                        return new Light(id, liData, pos);
                }
            }

            return null;
        }

        public static DungeonObject GetDungeonObject(int id, Vector2 pos)
        {
            if (id != -1 && _diWorldObjects.ContainsKey(id))
            {
                Dictionary<string, string> liData = _diWorldObjects[id];
                if (liData["Subtype"].Contains("Door")){
                    return new Door(id, liData, pos);
                }
                else if (liData["Subtype"].Equals("Trigger")){
                    return new TriggerObject(id, liData, pos);
                }
            }

            return null;
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

        public static string GetMonsterTraitData(string trait)
        {
            return _diMonsterTraits[trait];
        }
        public static string GetMonsterTestInfo(int id)
        {
            string rv = string.Empty;
            if (_diMonsterInfo.ContainsKey(id))
            {
                rv = _diMonsterInfo[id];
            }

            return rv;
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

        public static void GetActionText(int id, ref string name, ref string desc)
        {
            string val = "Action " + id;
            name = _diCombatSkillsText[val].Split('/')[0];
            desc = _diCombatSkillsText[val].Split('/')[1];
        }
        public static CombatAction GetActionByIndex(int id)
        {
            if (id != -1)
            {
                Dictionary<string, string> liData = _diActions[id];
                if (liData["Type"] == "Action" || liData["Type"] == "Spell")
                {
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

        public static void GetClassText(int id, ref string name, ref string desc)
        {
            string val = "Class " + id;
            name = _diGameText[val].Split('/')[0];
            desc = _diGameText[val].Split('/')[1];
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

        public static Texture2D GetTexture(string texture)
        {
            return _diTextures[texture];
        }

        public static BitmapFont GetBitMapFont(string font)
        {
            return _diBMFonts[font];
        }

        public static Dictionary<int, string> GetMerchandise(string file)
        {
            return _diMerchandise[file];
        }
        public static string GetGameText(string key)
        {
            string rv = string.Empty;
            if (_diGameText.ContainsKey(key))
            {
                rv = _diGameText[key];
            }

            return rv;
        }
        public static void GetQuestText(int id, ref string name, ref string desc)
        {
            string val = "Quest " + id;
            name = _diGameText[val].Split('/')[0];
            desc = _diGameText[val].Split('/')[1];
        }
        public static void GetUpgradeText(int id, ref string name, ref string desc)
        {
            string val = "Upgrade " + id;
            name = _diGameText[val].Split('/')[0];
            desc = _diGameText[val].Split('/')[1];
        }
        public static void GetStatusEffectText(int id, ref string name, ref string desc)
        {
            if (_diStatusEffectText.ContainsKey(id))
            {
                name = _diStatusEffectText[id].Split('/')[0];
                desc = _diStatusEffectText[id].Split('/')[1];
            }
        }

        public static List<string> GetSong(int id)
        {
            if (_diSongs.ContainsKey(id))
            {
                return _diSongs[id];
            }
            else
            {
                return null;
            }
        }
        public static Dictionary<string, string> GetNPCDialogue(int id)
        {
            Dictionary<string, string> rv = null;

            if (_diNPCDialogue.ContainsKey(id))
            {
                rv = _diNPCDialogue[id];
            }

            return rv;
        }
        #endregion

        #region Spawn Code
        public static void AddToForest(int ID) { _liForest.Add(ID); }
        public static void AddToMountain(int ID) { _liMountain.Add(ID); }
        public static void AddToNight(int ID) { _liNight.Add(ID); }

        #endregion

        #region Helper Objects
        public class AnimationData
        {
            VerbEnum _eVerb;
            AnimationEnum _eAnim;
            bool _bPingPong;
            bool _bDirectional;
            int _iXLocation;
            int _iYLocation;
            int _iFrames;
            float _fFrameSpeed;

            public int XLocation => _iXLocation;
            public int YLocation => _iYLocation;
            public int Frames => _iFrames;
            public float FrameSpeed => _fFrameSpeed;
            public bool Directional => _bDirectional;
            public bool PingPong => _bPingPong;
            public VerbEnum Verb => _eVerb;
            public AnimationEnum Animation => _eAnim;

            public AnimationData(string value, VerbEnum verb, bool directional) : base()
            {
                _bDirectional = directional;
                _eVerb = verb;
                StoreData(value);
            }

            public AnimationData(string value, AnimationEnum anim)
            {
                _eAnim = anim;
                StoreData(value);
            }

            public void StoreData(string value)
            {
                string[] splitString = value.Split('-');
                _iXLocation = int.Parse(splitString[0]);
                _iYLocation = int.Parse(splitString[1]);
                _iFrames = int.Parse(splitString[2]);
                _fFrameSpeed = float.Parse(splitString[3]);
                _bPingPong = splitString[4].Equals("T");
            }
        }
        #endregion
    }
}