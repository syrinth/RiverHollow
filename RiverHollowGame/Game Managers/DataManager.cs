using System.IO;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.BitmapFonts;
using RiverHollow.Characters;
using RiverHollow.Buildings;
using RiverHollow.CombatStuff;
using RiverHollow.WorldObjects;
using RiverHollow.Utilities;

using static RiverHollow.Game_Managers.GameManager;
using static RiverHollow.WorldObjects.Buildable;
using static RiverHollow.WorldObjects.TriggerObject;
using RiverHollow.Misc;
using static RiverHollow.WorldObjects.Buildable.AdjustableObject;
using RiverHollow.Tile_Engine;
using RiverHollow.GUIComponents.GUIObjects;
using RiverHollow.Items;

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
        public const string FOLDER_ENVIRONMENT = @"Textures\Environmental\";
        public const string FOLDER_MONSTERS = @"Textures\Actors\Monsters\";
        public const string FOLDER_MOBS= @"Textures\Actors\Mobs\";
        public const string FOLDER_SUMMONS = @"Textures\Actors\Summons\";
        public const string FOLDER_PLAYER = @"Textures\Actors\Player\";
        public const string FOLDER_PARTY = @"Textures\Actors\PartyMembers\";
        public const string FOLDER_TEXTFILES = @"Data\Text Files\";
        public const string FONT_MAIN = @"Fonts\Font_Main";
        public const string FONT_NUMBER_DISPLAY = @"Fonts\Font_Number_Display";
        public const string FONT_STAT_DISPLAY = @"Fonts\Font_Stat_Display";

        public const string DIALOGUE_TEXTURE = @"Textures\Dialog";
        public const string GUI_COMPONENTS = @"Textures\GUI Components";
        public const string COMBAT_TEXTURE = GUI_COMPONENTS + @"\GUI_Combat";
        public const string ACTION_ICONS = GUI_COMPONENTS + @"\GUI_Action_Icons";
        #endregion

        #region Dictionaries
        static Dictionary<string, Texture2D> _diTextures;
        static Dictionary<string, BitmapFont> _diBMFonts;
        static Dictionary<string, string> _diGameText;
        static Dictionary<string, string> _diMonsterTraits;

        static Dictionary<int, List<string>> _diSongs;
        static Dictionary<int, Dictionary<string, string>> _diNPCDialogue;
        static Dictionary<int, Shop> _diShops;

        static Dictionary<int, Dictionary<string, string>> _diVillagerData;
        public static Dictionary<int, Dictionary<string, string>> DiVillagerData => _diVillagerData;
        static Dictionary<int, Dictionary<string, string>> _diPlayerAnimationData;
        public static Dictionary<int, Dictionary<string, string>> PlayerCombatAnimationData => _diPlayerAnimationData;

        static Dictionary<string, Dictionary<string, string>> _diObjectText;
        static Dictionary<string, string> _diMailboxMessages;

        static Dictionary<int, Dictionary<string, string>> _diBuildings;
        static Dictionary<int, Dictionary<string, string>> _diItemData;

        static Dictionary<int, Dictionary<string, string>> _diDungeonData;
        static Dictionary<int, Dictionary<string, string>> _diLightData;
        static Dictionary<int, Dictionary<string, string>> _diStatusEffects;
        static Dictionary<int, Dictionary<string, string>> _diWorkers;
        public static Dictionary<int, Dictionary<string, string>> DIWorkers => _diWorkers;
        static Dictionary<int, Dictionary<string, string>> _diWorldObjects;

        static Dictionary<int, Dictionary<string, string>> _diTaskData;
        public static Dictionary<int, Dictionary<string, string>> DiTaskData => _diTaskData;

        static Dictionary<int, Dictionary<string, string>> _diNPCData;

        static Dictionary<int, Dictionary<string, string>> _diMobs;
        static Dictionary<int, Dictionary<string, string>> _diMonsterData;
        public static Dictionary<int, Villager> DIVillagers { get; private set; }
        public static Dictionary<int, Merchant> DIMerchants { get; private set; }
        static Dictionary<int, Dictionary<string, string>> _diActions;

        static Dictionary<int, Dictionary<string, string>> _diClasses;
        public static Dictionary<int, Dictionary<string, string>> DIClasses => _diClasses;
        static Dictionary<string, Dictionary<string, List<string>>> _diSchedule;

        public static List<int> FloorIDs { get; private set; }
        public static List<int> StructureIDs { get; private set; }
        public static List<int> PlantIDs { get; private set; }
        public static List<int> WallpaperIDs { get; private set; }

        public static Dictionary<int, Dictionary<string, string>> Config;
        #endregion

        public static BitmapFont _bmFont;
        static List<int> _liForest;
        static List<int> _liMountain;
        static List<int> _liNight;

        public static void LoadContent(ContentManager Content)
        {
            //Allocate Dictionaries
            FloorIDs = new List<int>();
            StructureIDs = new List<int>();
            PlantIDs = new List<int>();
            WallpaperIDs = new List<int>();
            _diTextures = new Dictionary<string, Texture2D>();

            _diMonsterTraits = Content.Load<Dictionary<string, string>>(@"Data\MonsterTraitTable");

            _diMailboxMessages = Content.Load<Dictionary<string, string>>(FOLDER_TEXTFILES + @"Mailbox_Text");

            //Read in Content and allocate the appropriate Dictionaries
            LoadGUIs(Content);
            LoadIcons(Content);
            LoadBMFonts(Content);
            LoadTextFiles(Content);
            LoadCharacters(Content);
            LoadDictionaries(Content);

            AddDirectoryTextures(GUI_COMPONENTS, Content);
            AddDirectoryTextures(FOLDER_ITEMS, Content);
            AddDirectoryTextures(FOLDER_BUILDINGS, Content);
            AddDirectoryTextures(FOLDER_ENVIRONMENT, Content);

            LoadNPCSchedules(Content);
            LoadNPCs();

            LoadShopFile(Content);

            _liForest = new List<int>();
            _liMountain = new List<int>();
            _liNight = new List<int>();
        }

        #region Load Methods
        private delegate void LoadDictionaryWorkDelegate(int id, Dictionary<string, string> taggedDictionary);
        private static void LoadDictionaries(ContentManager Content)
        {
            LoadDictionary(ref _diPlayerAnimationData, @"Data\PlayerClassAnimationConfig", Content, null);
            LoadDictionary(ref _diItemData, @"Data\ItemData", Content, null);
            LoadDictionary(ref _diWorldObjects, @"Data\WorldObjects", Content, LoadWorldObjectsDoWork);
            LoadDictionary(ref _diActions, @"Data\CombatActions", Content, null);
            LoadDictionary(ref _diVillagerData, @"Data\CharacterData", Content, null);
            LoadDictionary(ref _diMonsterData, @"Data\Monsters", Content, null);
            LoadDictionary(ref _diNPCData, @"Data\NPCs", Content, null);
            LoadDictionary(ref _diBuildings, @"Data\Buildings", Content, null);
            LoadDictionary(ref _diStatusEffects, @"Data\StatusEffects", Content, null);
            LoadDictionary(ref _diMobs, @"Data\Mobs", Content, null);
            LoadDictionary(ref _diWorkers, @"Data\Workers", Content, null);
            LoadDictionary(ref _diTaskData, @"Data\Tasks", Content, null);
            LoadDictionary(ref _diClasses, @"Data\Classes", Content, null);
            LoadDictionary(ref Config, @"Data\Config", Content, null);
            LoadDictionary(ref _diLightData, @"Data\LightData", Content, null);
            LoadDictionary(ref _diDungeonData, @"Data\DungeonData", Content, null);
        }
        private static void LoadDictionary(ref Dictionary<int, Dictionary<string, string>> dictionaryAddTo, string dataFile, ContentManager Content, LoadDictionaryWorkDelegate workDelegate)
        {
            dictionaryAddTo = new Dictionary<int, Dictionary<string, string>>();
            Dictionary<int, string> dictionaryData = Content.Load<Dictionary<int, string>>(dataFile);
            foreach (KeyValuePair<int, string> kvp in dictionaryData)
            {
                Dictionary<string, string> taggedDictionary = TaggedStringToDictionary(kvp.Value);
                dictionaryAddTo[kvp.Key] = taggedDictionary;

                workDelegate?.Invoke(kvp.Key, taggedDictionary);
            }
        }

        private static void LoadWorldObjectsDoWork(int id, Dictionary<string, string> taggedDictionary)
        {
            ObjectTypeEnum type = Util.ParseEnum<ObjectTypeEnum>(taggedDictionary["Type"]);
            switch (type)
            {
                case ObjectTypeEnum.Floor:
                    FloorIDs.Add(id);
                    break;
                case ObjectTypeEnum.Beehive:
                case ObjectTypeEnum.Buildable:
                case ObjectTypeEnum.Container:
                case ObjectTypeEnum.Garden:
                case ObjectTypeEnum.Wall:
                    StructureIDs.Add(id);
                    break;
                case ObjectTypeEnum.Plant:
                    PlantIDs.Add(id);
                    break;
                case ObjectTypeEnum.Wallpaper:
                    WallpaperIDs.Add(id);
                    break;
            }
        }

        private static void LoadDictionary(ref Dictionary<string, Dictionary<string, string>> dictionaryAddTo, string dataFile, ContentManager Content)
        {
            dictionaryAddTo = new Dictionary<string, Dictionary<string, string>>();
            Dictionary<string, string> dictionaryData = Content.Load<Dictionary<string, string>>(dataFile);
            foreach (KeyValuePair<string, string> kvp in dictionaryData)
            {
                dictionaryAddTo[kvp.Key] = TaggedStringToDictionary(kvp.Value);
            }
        }

        public static Dictionary<string, string> TaggedStringToDictionary(string data)
        {
            return Util.DictionaryFromTaggedString(data);
        }

        private static void LoadTextFiles(ContentManager Content)
        {
            LoadDictionary(ref _diObjectText, FOLDER_TEXTFILES + "Object_Text", Content);
            _diGameText = Content.Load<Dictionary<string, string>>(FOLDER_TEXTFILES + "GameText");

            _diSongs = Content.Load<Dictionary<int, List<string>>>(@"Data\Songs");
            
            _diNPCDialogue = new Dictionary<int, Dictionary<string, string>>();
            foreach (string s in Directory.GetFiles(@"Content\" + FOLDER_TEXTFILES + @"Dialogue\Villagers"))
            {
                string fileName = string.Empty;

                if (s.Contains("NPC_")) {
                    fileName = Path.GetFileName(s).Replace("NPC_", "").Split('.')[0];

                    int file = -1;
                    if (int.TryParse(fileName, out file))
                    {
                        fileName = s;
                        Util.ParseContentFile(ref fileName);
                        _diNPCDialogue.Add(file, Content.Load<Dictionary<string, string>>(fileName));
                    }
                }
                //else if (s.Contains("Adventurer_")) {
                //    fileName = Path.GetFileName(s).Replace("Adventurer_", "").Split('.')[0];

                //    int file = -1;
                //    if (int.TryParse(fileName, out file))
                //    {
                //        fileName = s;
                //        Util.ParseContentFile(ref fileName);
                //        _diAdventurerDialogue.Add(file, Content.Load<Dictionary<string, string>>(fileName));
                //    }
                //}
            }
        }
        private static void LoadCharacters(ContentManager Content)
        {
            AddDirectoryTextures(FOLDER_ACTOR, Content);
            AddTexture(@"Textures\texFlooring", Content);
            AddTexture(@"Textures\texWeather", Content);
            AddTexture(@"Textures\texClothes", Content);
        }
        private static void LoadGUIs(ContentManager Content)
        {
            AddTexture(DataManager.DIALOGUE_TEXTURE, Content);
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
            if (!_diTextures.ContainsKey(texture))
            {
                _diTextures.Add(texture, Content.Load<Texture2D>(texture));
            }
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

        private static void LoadShopFile(ContentManager Content)
        {
            Dictionary<int, string> shopFile = Content.Load<Dictionary<int, string>>(@"Data\Shops");

            _diShops = new Dictionary<int, Shop>();

            foreach(KeyValuePair<int, string> kvp in shopFile){
                _diShops[kvp.Key] = new Shop(kvp.Key, Util.DictionaryFromTaggedString(kvp.Value));
            }
        }

        private static void LoadNPCs()
        {
            DIMerchants = new Dictionary<int, Merchant>();
            DIVillagers = new Dictionary<int, Villager>();
            foreach (KeyValuePair<int, Dictionary<string, string>> npcData in _diVillagerData)
            {
                Dictionary<string, string> diData = _diVillagerData[npcData.Key];
                switch (diData["Type"])
                {
                    case "ShippingGremlin":
                        GameManager.ShippingGremlin = new ShippingGremlin(npcData.Key, diData);
                        DIVillagers.Add(npcData.Key, GameManager.ShippingGremlin);
                        break;
                    case "Merchant":
                        DIMerchants.Add(npcData.Key, new Merchant(npcData.Key, diData));
                        break;

                    case "Villager":
                        DIVillagers.Add(npcData.Key, new Villager(npcData.Key, diData));
                        break;
                    default:
                        break;
                }

            }
        }
        private static void LoadNPCSchedules(ContentManager Content)
        {
            _diSchedule = new Dictionary<string, Dictionary<string, List<string>>>();
            foreach (string s in Directory.GetFiles(@"Content\Data\Schedules"))
            {
                string temp = Path.GetFileNameWithoutExtension(s);
                _diSchedule.Add(temp, Content.Load<Dictionary<string, List<string>>>(@"Data\Schedules\" + temp));
            }
        }
        #endregion

        #region GetMethods
        public static string GetItemValueByID(int id, string key) { return _diItemData[id][key]; }
        public static string GetWorldObjectValueByID(int id, string key) { return _diWorldObjects[id][key]; }
        public static string GetNPCValueByID(int id, string key) { return _diNPCData[id][key]; }

        public static Light GetLight(int id)
        {
            if (_diLightData.ContainsKey(id))
            {
                return new Light(id, _diLightData[id]);
            }
            return null;
        }

        public static Dictionary<string, string> GetDungeonInfo(int id)
        {
            if (_diDungeonData.ContainsKey(id))
            {
                return _diDungeonData[id];
            }
            return null;
        }

        public static Building GetBuilding(int id)
        {
            if (_diBuildings.ContainsKey(id))
            {
                return new Building(id, _diBuildings[id]);
            }
            return null;
        }
        public static Dictionary<int, BuildInfo> GetBuildInfoList()
        {
            Dictionary<int, BuildInfo> rvList = new Dictionary<int, BuildInfo>();
            foreach(KeyValuePair<int, Dictionary<string, string>> kvp in _diBuildings)
            {
                rvList.Add(kvp.Key, new BuildInfo(kvp.Key, _diBuildings[kvp.Key]));
            }
            return rvList;
        }

        public static Dictionary<int, Shop> GetShopInfoList()
        {
            return _diShops;
        }

        public static Item GetItem(int id)
        {
            return GetItem(id, 1);
        }
        public static Item GetItem(int id, int num)
        {
            if (id != -1 && _diItemData.ContainsKey(id))
            {
                Dictionary<string, string> diData = _diItemData[id];
                switch (Util.ParseEnum<ItemEnum>(diData["Type"]))
                {
                    case ItemEnum.Blueprint:
                        return new Blueprint(id, diData);
                    case ItemEnum.Clothes:
                        return new Clothing(id, diData); 
                    case ItemEnum.Consumable:
                        return new Consumable(id, diData, num);
                    case ItemEnum.Equipment:
                        return new Equipment(id, diData);
                    case ItemEnum.Food:
                        return new Food(id, diData, num);
                    case ItemEnum.MonsterFood:
                        return new MonsterFood(id, diData, num);
                    case ItemEnum.Resource:
                        return new Item(id, diData, num);
                    case ItemEnum.Special:
                        switch (diData["Subtype"])
                        {
                            case "Class":
                                return new ClassItem(id, diData, num);
                            case "Marriage":
                                return new MarriageItem(id, diData);
                            case "Map":
                                return new AdventureMap(id, diData, num);
                            case "DungeonKey":
                                return new DungeonKey(id, diData);
                        }
                        break;
                    case ItemEnum.Tool:
                        return new Tool(id, diData);
                }
            }
            return null;
        }
        public static Dictionary<string,string> GetItemStringData(int id)
        {
            if (_diItemData.ContainsKey(id)) { return _diItemData[id]; }
            else { return null; }
        }

        /// <summary>
        /// Creates and returns a WorldObject based on the given ID
        /// </summary>
        /// <param name="id">The ID of the WorldObject</param>
        /// <returns>The WorldObject if it was successfully created, null otherwise</returns>
        public static WorldObject GetWorldObjectByID(int id)
        {
            if (id != -1 && _diWorldObjects.ContainsKey(id))
            {
                Dictionary<string, string> diData = _diWorldObjects[id];
                switch (Util.ParseEnum<ObjectTypeEnum>(diData["Type"]))
                {
                    case ObjectTypeEnum.Beehive:
                        return new Beehive(id, diData);
                    case ObjectTypeEnum.Buildable:
                        return new Buildable(id, diData);
                    case ObjectTypeEnum.CombatHazard:
                        return new CombatHazard(id, diData);
                    case ObjectTypeEnum.Container:
                        return new Container(id, diData);
                    case ObjectTypeEnum.Decor:
                        return new Decor(id, diData);
                    case ObjectTypeEnum.Destructible:
                        if (diData.ContainsKey("Tree")) { return new Tree(id, diData); }
                        else { return new Destructible(id, diData); }
                    case ObjectTypeEnum.Floor:
                        return new Floor(id, diData);
                    case ObjectTypeEnum.Garden:
                        return new Garden(id, diData);
                    case ObjectTypeEnum.Gatherable:
                        return new Gatherable(id, diData);
                    case ObjectTypeEnum.Machine:
                        return new Machine(id, diData);
                    case ObjectTypeEnum.Mailbox:
                        return new Mailbox(id, diData);
                    case ObjectTypeEnum.Plant:
                        return new Plant(id, diData);
                    case ObjectTypeEnum.Structure:
                        return new Structure(id, diData);
                    case ObjectTypeEnum.Wall:
                        return new Wall(id, diData);
                    case ObjectTypeEnum.Wallpaper:
                        return new Wallpaper(id, diData);
                    case ObjectTypeEnum.WarpPoint:
                        return new WarpPoint(id, diData);
                    case ObjectTypeEnum.WorldObject:
                        return new WorldObject(id, diData);
                }
            }

            return null;
        }

        /// <summary>
        /// Creates a new WorldObject based off of the given ID and then calls to the object
        /// for it to place itself on the given map at the indicated Vector2.
        /// 
        /// Objects are responsible for placing themselves and may have unique additional handling.
        /// 
        /// The position is the location of the top-left most tile on the base of the object.
        /// </summary>
        /// <param name="id">The ID of the WorldObject</param>
        /// <param name="pos">The position in pixels of the place to put the object</param>
        /// <param name="map">The map the WorldObject is to be put on</param>
        public static void CreateAndPlaceNewWorldObject(int id, Vector2 pos, RHMap map)
        {
            GetWorldObjectByID(id)?.PlaceOnMap(pos, map);
        }

        public static Dictionary<string, string> GetWorldObjectData(int id)
        {
            if (_diWorldObjects.ContainsKey(id)) { return _diWorldObjects[id]; }
            else { return null; }
        }

        public static TriggerObject GetDungeonObject(Dictionary<string, string> data, Vector2 pos)
        {
            int id = int.Parse(data["ObjectID"]);
            if (id != -1 && _diWorldObjects.ContainsKey(id))
            {
                Dictionary<string, string> liData = new Dictionary<string, string>(_diWorldObjects[id]);
                foreach(KeyValuePair<string, string> kvp in data) {
                    if (!liData.ContainsKey(kvp.Key))
                    {
                        liData.Add(kvp.Key, kvp.Value);
                    }
                }

                if (liData["Subtype"].Contains("Door")){
                    return new Door(id, liData);
                }
                else if (liData["Subtype"].Equals("Trigger")){
                    return new Trigger(id, liData);
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
            return DIVillagers[i].Name;
        }

        public static WorldActor GetNPCByIndex(int id)
        {
            if (_diNPCData.ContainsKey(id))
            {
                Dictionary<string, string> diData = _diNPCData[id];
                switch (Util.ParseEnum<ActorEnum>(diData["Type"]))
                {
                    case ActorEnum.Child:
                        return new Child(id, diData);
                    case ActorEnum.Environmental:
                        return new EnvironmentalActor(id, diData);
                    case ActorEnum.Mount:
                        return new Mount(id, diData);
                    case ActorEnum.Pet:
                        return new Pet(id, diData);
                    case ActorEnum.Spirit:
                        return new Spirit(diData);
                }
            }

            return null;
        }

        public static LiteSummon GetSummonByIndex(int id)
        {
            if (_diNPCData.ContainsKey(id))
            {
                Dictionary<string, string> diData = _diNPCData[id];
                switch (Util.ParseEnum<ActorEnum>(diData["Type"]))
                {
                    case ActorEnum.Summon:
                        return new LiteSummon(id, diData);
                }
            }

            return null;
        }

        public static string GetMonsterTraitData(string trait)
        {
            return _diMonsterTraits[trait];
        }

        public static Mob GetMobByIndex(int id)
        {
            Mob m = null;

            if (_diMobs.ContainsKey(id))
            {
                m = new Mob(id, _diMobs[id]);
            }
            return m;
        }
        public static Mob GetMonster(int id, Vector2 pos)
        {
            Mob m = GetMobByIndex(id);
            m.Position = pos;
            return m;
        }
        public static Monster GetLiteMonsterByIndex(int id)
        {
            Monster m = null;

            if (_diMonsterData.ContainsKey(id))
            {
                m = new Monster(id, _diMonsterData[id]);
            }
            return m;
        }
        public static Monster GetLiteMonsterByIndex(int id, Vector2 pos)
        {
            Monster m = GetLiteMonsterByIndex(id);
            m.Position = pos;
            return m;
        }

        public static CombatAction GetCombatActionByIndex(int id)
        {
            if (id != -1)
            {
                return new CombatAction(id, _diActions[id]);
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
                Dictionary<string, string> liData = _diClasses[id];
                c = new CharacterClass(id, liData);
            }
            return c;
        }

        public static Dictionary<string, List<string>> GetSchedule(string npc)
        {
            Dictionary<string, List<string>> rv = null;
            if (_diSchedule.ContainsKey(npc))
            {
                rv = _diSchedule[npc];
            }

            return rv;
        }

        public static bool HasTexture(string texture)
        {
            return _diTextures.ContainsKey(texture);
        }

        public static Texture2D GetTexture(string texture)
        {
            Texture2D rv = null;

            if(_diTextures.ContainsKey(texture))
            {
                rv = _diTextures[texture];
            }

            return rv;
        }

        public static BitmapFont GetBitMapFont(string font)
        {
            return _diBMFonts[font];
        }
        
        public static void GetUpgradeText(int id, ref string name, ref string desc)
        {
            string val = "Upgrade " + id;
            name = _diGameText[val].Split('/')[0];
            desc = _diGameText[val].Split('/')[1];
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

        public static GUIImage GetIcon(GameIconEnum e)
        {
            GUIImage rv = null;

            switch (e)
            {
                case GameIconEnum.Agility:
                    rv = new GUIImage(new Rectangle(112, 80, 8, 9), COMBAT_TEXTURE);
                    break;
                case GameIconEnum.AreaAll:
                    rv = new GUIImage(new Rectangle(145, 65, 13, 10), COMBAT_TEXTURE);
                    break;
                case GameIconEnum.AreaColumnAlly:
                    rv = new GUIImage(new Rectangle(113, 61, 6, 10), COMBAT_TEXTURE);
                    break;
                case GameIconEnum.AreaColumnEnemy:
                    rv = new GUIImage(new Rectangle(121, 61, 6, 10), COMBAT_TEXTURE);
                    break;
                case GameIconEnum.AreaRow:
                    rv = new GUIImage(new Rectangle(129, 75, 13, 4), COMBAT_TEXTURE);
                    break;
                case GameIconEnum.AreaSelf:
                    rv = new GUIImage(new Rectangle(121, 73, 6, 7), COMBAT_TEXTURE);
                    break;
                case GameIconEnum.AreaSingle:
                    rv = new GUIImage(new Rectangle(113, 74, 5, 5), COMBAT_TEXTURE);
                    break;
                case GameIconEnum.AreaSquare:
                    rv = new GUIImage(new Rectangle(129, 54, 9, 9), COMBAT_TEXTURE);
                    break;
                case GameIconEnum.BuffArrow:
                    rv = new GUIImage(new Rectangle(195, 64, 10, 6), COMBAT_TEXTURE);
                    break;
                case GameIconEnum.DebuffArrow:
                    rv = new GUIImage(new Rectangle(195, 74, 10, 6), COMBAT_TEXTURE);
                    break;
                case GameIconEnum.Defense:
                    rv = new GUIImage(new Rectangle(136, 87, 8, 9), COMBAT_TEXTURE);
                    break;
                case GameIconEnum.ElementFire:
                    rv = new GUIImage(new Rectangle(185, 80, 7, 9), COMBAT_TEXTURE);
                    break;
                case GameIconEnum.ElementIce:
                    rv = new GUIImage(new Rectangle(178, 89, 7, 7), COMBAT_TEXTURE);
                    break;
                case GameIconEnum.ElementLightning:
                    rv = new GUIImage(new Rectangle(176, 80, 6, 9), COMBAT_TEXTURE);
                    break;
                case GameIconEnum.Evasion:
                    rv = new GUIImage(new Rectangle(120, 88, 8, 7), COMBAT_TEXTURE);
                    break;
                case GameIconEnum.Heal:
                    rv = new GUIImage(new Rectangle(211, 87, 8, 8), COMBAT_TEXTURE);
                    break;
                case GameIconEnum.MaxHealth:
                    rv = new GUIImage(new Rectangle(179, 103, 10, 9), COMBAT_TEXTURE);
                    break;
                case GameIconEnum.Magic:
                    rv = new GUIImage(new Rectangle(144, 80, 9, 7), COMBAT_TEXTURE);
                    break;
                case GameIconEnum.MagicDamage:
                    rv = new GUIImage(new Rectangle(202, 87, 8, 8), COMBAT_TEXTURE);
                    break;
                case GameIconEnum.Melee:
                    rv = new GUIImage(new Rectangle(199, 55, 9, 9), COMBAT_TEXTURE);
                    break;
                case GameIconEnum.MoveLeft:
                    rv = new GUIImage(new Rectangle(128, 64, 8, 9), COMBAT_TEXTURE);
                    break;
                case GameIconEnum.MoveRight:
                    rv = new GUIImage(new Rectangle(136, 64, 8, 9), COMBAT_TEXTURE);
                    break;
                case GameIconEnum.PhysicalDamage:
                    rv = new GUIImage(new Rectangle(193, 87, 8, 8), COMBAT_TEXTURE);
                    break;
                case GameIconEnum.Ranged:
                    rv = new GUIImage(new Rectangle(192, 48, 9, 9), COMBAT_TEXTURE);
                    break;
                case GameIconEnum.Resistance:
                    rv = new GUIImage(new Rectangle(151, 87, 8, 8), COMBAT_TEXTURE);
                    break;
                case GameIconEnum.Speed:
                    rv = new GUIImage(new Rectangle(166, 80, 10, 8), COMBAT_TEXTURE);
                    break;
                case GameIconEnum.Strength:
                    rv = new GUIImage(new Rectangle(128, 80, 8, 8), COMBAT_TEXTURE);
                    break;
                case GameIconEnum.Timer:
                    rv = new GUIImage(new Rectangle(160, 88, 7, 8), COMBAT_TEXTURE);
                    break;
            }

            return rv;
        }

        #region Text Handlers
        private static string GetGameText(string key)
        {
            string rv = string.Empty;
            if (_diGameText.ContainsKey(key))
            {
                rv = _diGameText[key];
            }

            return rv;
        }

        public static TextEntry GetGameTextEntry(string key)
        {
            return new TextEntry(key, Util.DictionaryFromTaggedString(GetGameText(key)));
        }
        public static TextEntry GetMailboxMessage(string messageID)
        {
            return new TextEntry(messageID, Util.DictionaryFromTaggedString(_diMailboxMessages[messageID]));
        }
        public static Dictionary<string, TextEntry> GetNPCDialogue(int id)
        {
            Dictionary<string, TextEntry> rv = new Dictionary<string, TextEntry>();

            if (_diNPCDialogue.ContainsKey(id))
            {
                foreach(KeyValuePair<string, string> kvp in _diNPCDialogue[id])
                {
                    rv[kvp.Key] = new TextEntry(kvp.Key, Util.DictionaryFromTaggedString(kvp.Value));
                }
            }

            return rv;
        }

        public static bool TextDataHasKey(string identifier)
        {
            return _diObjectText.ContainsKey(identifier);
        }
        public static void GetTextData(string identifier, int id, ref string value, string key)
        {
            string textKey = identifier + "_" + id;
            GetTextData(textKey, ref value, key);
        }
        public static void GetTextData(string textKey, ref string value, string key)
        {
            if (_diObjectText[textKey].ContainsKey(key)) { value = _diObjectText[textKey][key]; }
            else { value = string.Empty; }

            value = Util.ProcessText(value);
        }
        #endregion
        #endregion

        #region Spawn Code
        public static void AddToForest(int ID) { _liForest.Add(ID); }
        public static void AddToMountain(int ID) { _liMountain.Add(ID); }
        public static void AddToNight(int ID) { _liNight.Add(ID); }

        #endregion
    }
}