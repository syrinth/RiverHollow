using System.IO;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.BitmapFonts;
using RiverHollow.Characters;
using RiverHollow.Buildings;
using RiverHollow.WorldObjects;
using RiverHollow.Utilities;
using RiverHollow.Misc;
using RiverHollow.Map_Handling;
using RiverHollow.GUIComponents.GUIObjects;
using RiverHollow.Items;
using RiverHollow.WorldObjects.Trigger_Objects;
using RiverHollow.Items.Tools;

using static RiverHollow.Utilities.Enums;
using RiverHollow.Characters.Mobs;

namespace RiverHollow.Game_Managers
{
    public static class DataManager
    {
        #region Constants
        public const string FOLDER_ACTOR = @"Textures\Actors\";

        public const string NPC_FOLDER = FOLDER_ACTOR + @"NPCs\";
        public const string PORTRAIT_FOLDER = FOLDER_ACTOR + @"Portraits\";

        public const string FILE_WORLDOBJECTS = @"Textures\worldObjects";
        public const string FILE_FLOORING = @"Textures\texFlooring";

        public const string FOLDER_BUILDINGS = @"Textures\Buildings\";
        public const string FOLDER_ITEMS = @"Textures\Items\";
        public const string FOLDER_ENVIRONMENT = @"Textures\Environmental\";
        public const string FOLDER_MONSTERS = @"Textures\Actors\Monsters\";
        public const string FOLDER_MOBS = @"Textures\Actors\Mobs\";
        public const string FOLDER_SUMMONS = @"Textures\Actors\Summons\";
        public const string FOLDER_PLAYER = @"Textures\Actors\Player\";
        public const string FOLDER_PARTY = @"Textures\Actors\PartyMembers\";
        public const string FOLDER_TEXTFILES = @"Data\Text Files\";
        public const string FONT_NEW = @"Fonts\Font_New\Font_New";
        public const string FONT_MAIN = @"Fonts\Font_Main";
        public const string FONT_NUMBER_DISPLAY = @"Fonts\Font_Number_Display";
        public const string FONT_STAT_DISPLAY = @"Fonts\Font_Stat_Display";

        public const string DIALOGUE_TEXTURE = @"Textures\Dialog";
        public const string UPGRADE_ICONS = GUI_COMPONENTS + @"\GUI_Upgrade_Icons";
        public const string GUI_COMPONENTS = @"Textures\GUI Components";
        public const string ACTION_ICONS = GUI_COMPONENTS + @"\GUI_Action_Icons";
        public const string HUD_COMPONENTS = GUI_COMPONENTS + @"\GUI_HUD_Components";
        #endregion

        #region Dictionaries
        static Dictionary<string, Texture2D> _diTextures;
        static Dictionary<string, BitmapFont> _diBMFonts;
        static Dictionary<string, string> _diGameText;
        static Dictionary<string, string> _diMonsterTraits;

        static Dictionary<int, List<string>> _diSongs;
        static Dictionary<string, Dictionary<string, string>> _diNPCDialogue;
        static Dictionary<int, Shop> _diShops;

        static Dictionary<int, Dictionary<string, string>> _diNPCData;
        public static Dictionary<int, Dictionary<string, string>> NPCData => _diNPCData;
        static Dictionary<int, Dictionary<string, string>> _diPlayerAnimationData;

        static Dictionary<string, Dictionary<string, string>> _diObjectText;
        static Dictionary<string, string> _diMailboxMessages;

        static Dictionary<int, Dictionary<string, string>> _diItemData;

        static Dictionary<int, Dictionary<string, string>> _diDungeonData;
        static Dictionary<int, Dictionary<string, string>> _diLightData;
        static Dictionary<int, Dictionary<string, string>> _diUpgradeData;
        static Dictionary<int, Dictionary<string, string>> _diStatusEffects;
        static Dictionary<int, Dictionary<string, string>> _diWorldObjects;

        static Dictionary<int, Dictionary<string, string>> _diTaskData;
        public static IReadOnlyDictionary<int, Dictionary<string, string>> TaskData => _diTaskData;

        static Dictionary<int, Dictionary<string, string>> _diMonsterData;
        static Dictionary<int, Dictionary<string, string>> _diActions;

        static Dictionary<int, Dictionary<string, string>> _diJobs;
        static Dictionary<string, Dictionary<string, List<string>>> _diSchedule;

        public static Dictionary<int, Dictionary<string, string>> Config;
        #endregion

        public static BitmapFont _bmFont;
        static List<int> _liForest;
        static List<int> _liMountain;
        static List<int> _liNight;

        public static void LoadContent(ContentManager Content)
        {
            //Allocate Dictionaries
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
            LoadDictionary(ref _diNPCData, @"Data\NPCData", Content, null);
            LoadDictionary(ref _diMonsterData, @"Data\Monsters", Content, null);
            LoadDictionary(ref _diStatusEffects, @"Data\StatusEffects", Content, null);
            LoadDictionary(ref _diTaskData, @"Data\Tasks", Content, null);
            LoadDictionary(ref _diJobs, @"Data\Classes", Content, null);
            LoadDictionary(ref Config, @"Data\Config", Content, null);
            LoadDictionary(ref _diLightData, @"Data\LightData", Content, null);
            LoadDictionary(ref _diUpgradeData, @"Data\Upgrades", Content, null);
            LoadDictionary(ref _diDungeonData, @"Data\DungeonData", Content, null);
        }
        public static void SecondaryLoad(ContentManager Content)
        {
            LoadDictionary(ref _diWorldObjects, @"Data\WorldObjects", Content, LoadWorldObjectsDoWork);
            LoadShopFile(Content);
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
            if (taggedDictionary.ContainsKey("Unlocked"))
            {
                PlayerManager.AddToCraftingDictionary(id, false);
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

            _diGameText = new Dictionary<string, string>();
            Dictionary<string, string> rawTextInfo = Content.Load<Dictionary<string, string>>(FOLDER_TEXTFILES + "GameText");
            Dictionary<string, string> newDialogue = new Dictionary<string, string>();
            foreach (string dialogueTags in rawTextInfo.Values)
            {
                Dictionary<string, string> tags = Util.DictionaryFromTaggedString(dialogueTags);
                string newKey = tags["Name"];
                tags.Remove("Name");
                _diGameText[newKey] = Util.StringFromTaggedDictionary(tags);
            }

            _diSongs = Content.Load<Dictionary<int, List<string>>>(@"Data\Songs");
            
            _diNPCDialogue = new Dictionary<string, Dictionary<string, string>>();
            foreach (string s in Directory.GetFiles(@"Content\" + FOLDER_TEXTFILES + @"Dialogue\Villagers"))
            {
                string fileName = s;

                if (s.Contains("NPC_"))
                {
                    string key = Path.GetFileName(s).Replace("NPC_", "").Split('.')[0];

                    Util.ParseContentFile(ref fileName);
                    Dictionary<int, string> rawInfo = Content.Load<Dictionary<int, string>>(fileName);
                    newDialogue = new Dictionary<string, string>();
                    foreach (string dialogueTags in rawInfo.Values)
                    {
                        Dictionary<string, string> tags = Util.DictionaryFromTaggedString(dialogueTags);
                        string newKey = tags["Name"];
                        tags.Remove("Name");
                        newDialogue[newKey] = Util.StringFromTaggedDictionary(tags);
                    }
                    _diNPCDialogue.Add(key, newDialogue);
                }
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
            AddBMFont(FONT_NEW, Content);
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

        #region Lookup Methods
        public static TEnum GetEnumByIDKey<TEnum>(int id, string key, DataType type) where TEnum : struct
        {
            return Util.ParseEnum<TEnum>(GetStringByIDKey(id, key, type));
        }
        public static int GetIntByIDKey(int id, string key, DataType type, int defaultValue = -1)
        {
            string rv = GetStringByIDKey(id, key, type);

            if (!string.IsNullOrEmpty(rv)) { return Util.ParseInt(rv); }
            else { return defaultValue; }
        }
        public static float GetFloatByIDKey(int id, string key, DataType type, int defaultValue = -1)
        {
            string rv = GetStringByIDKey(id, key, type);

            if (!string.IsNullOrEmpty(rv)) { return Util.ParseFloat(rv); }
            else { return defaultValue; }
        }
        public static Point GetPointByIDKey(int id, string key, DataType type)
        {
            Point rv = Point.Zero;
            string value = GetStringByIDKey(id, key, type);

            if (!string.IsNullOrEmpty(value))
            {
                rv = Util.ParsePoint(value);
            }

            return rv;
        }
        public static string GetStringByIDKey(int id, string key, DataType type)
        {
            switch (type)
            {
                case DataType.Action:
                    if (_diActions[id].ContainsKey(key)) { return _diActions[id][key]; }
                    break;
                case DataType.NPC:
                    if (_diNPCData[id].ContainsKey(key)) { return _diNPCData[id][key]; }
                    break;
                case DataType.Job:
                    if (_diJobs[id].ContainsKey(key)) { return _diJobs[id][key]; }
                    break;
                case DataType.Item:
                    if (_diItemData[id].ContainsKey(key)) { return _diItemData[id][key]; }
                    break;
                case DataType.Light:
                    if (_diLightData[id].ContainsKey(key)) { return _diLightData[id][key]; }
                    break;
                case DataType.Monster:
                    if (_diMonsterData[id].ContainsKey(key)) { return _diMonsterData[id][key]; }
                    break;
                case DataType.StatusEffect:
                    if (_diStatusEffects[id].ContainsKey(key)) { return _diStatusEffects[id][key]; }
                    break;
                case DataType.Task:
                    if (_diTaskData[id].ContainsKey(key)) { return _diTaskData[id][key]; }
                    break;
                case DataType.Upgrade:
                    if (_diUpgradeData[id].ContainsKey(key)) { return _diUpgradeData[id][key]; }
                    break;
                case DataType.WorldObject:
                    if (_diWorldObjects[id].ContainsKey(key)) { return _diWorldObjects[id][key]; }
                    break;
            }

            return string.Empty;
        }
        public static bool GetBoolByIDKey(int id, string key, DataType type)
        {
            switch (type)
            {
                case DataType.Action:
                    if (_diActions[id].ContainsKey(key)) { return _diActions[id].ContainsKey(key); }
                    break;
                case DataType.NPC:
                    if (_diNPCData[id].ContainsKey(key)) { return _diNPCData[id].ContainsKey(key); }
                    break;
                case DataType.Job:
                    if (_diJobs[id].ContainsKey(key)) { return _diJobs[id].ContainsKey(key); }
                    break;
                case DataType.Item:
                    if (_diItemData[id].ContainsKey(key)) { return _diItemData[id].ContainsKey(key); }
                    break;
                case DataType.Light:
                    if (_diLightData[id].ContainsKey(key)) { return _diLightData[id].ContainsKey(key); }
                    break;
                case DataType.Monster:
                    if (_diMonsterData[id].ContainsKey(key)) { return _diMonsterData[id].ContainsKey(key); }
                    break;
                case DataType.StatusEffect:
                    if (_diStatusEffects[id].ContainsKey(key)) { return _diStatusEffects[id].ContainsKey(key); }
                    break;
                case DataType.Task:
                    if (_diTaskData[id].ContainsKey(key)) { return _diTaskData[id].ContainsKey(key); }
                    break;
                case DataType.Upgrade:
                    if (_diUpgradeData[id].ContainsKey(key)) { return _diUpgradeData[id].ContainsKey(key); }
                    break;
                case DataType.WorldObject:
                    if (_diWorldObjects[id].ContainsKey(key)) { return _diWorldObjects[id].ContainsKey(key); }
                    break;
            }

            return false;
        }
        public static List<TEnum> GetEnumListByIDKey<TEnum>(int id, string key, DataType type) where TEnum : struct
        {
            var rv = new List<TEnum>();
            string str = GetStringByIDKey(id, key, type);
            string[] split = Util.FindParams(str);

            if (split.Length == 0)
            {
                rv.Add(default);
            }
            else
            {
                foreach (var s in split)
                {
                    rv.Add(Util.ParseEnum<TEnum>(s));
                }
            }

            return rv;
        }

        public static Dictionary<int, int> IntDictionaryFromLookup(int id, string key, DataType type)
        {
            Dictionary<int, int> dictValue = new Dictionary<int, int>();
            string value = GetStringByIDKey(id, key, type);
            if (!string.IsNullOrEmpty(value))
            {
                //Split by "|" for each set
                string[] split = Util.FindParams(value);
                foreach (string s in split)
                {
                    string[] splitData = s.Split('-');
                    dictValue[Util.ParseInt(splitData[0])] = Util.ParseInt(splitData[1]);
                }
            }
            return dictValue;
        }
        #endregion

        #region GetMethods
        public static Upgrade GetUpgrade(int id)
        {
            if (_diUpgradeData.ContainsKey(id))
            {
                return new Upgrade(id);
            }
            return null;
        }
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

        public static Dictionary<int, Shop> GetShopInfoList()
        {
            return _diShops;
        }

        public static Item CraftItem(int id)
        {
            return GetItem(id, GetIntByIDKey(id, "CraftAmount", DataType.Item, 1));
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
                    case ItemEnum.Clothing:
                        return new Clothing(id, diData); 
                    case ItemEnum.Consumable:
                        return new Consumable(id, diData, num);
                    case ItemEnum.Food:
                        return new Food(id, diData, num);
                    case ItemEnum.MonsterFood:
                        return new MonsterFood(id, diData, num);
                    case ItemEnum.NPCToken:
                        return new NPCToken(id, diData);
                    case ItemEnum.Seed:
                        return new Seed(id, diData, num);
                    case ItemEnum.Special:
                        return new Special(id, diData);
                    case ItemEnum.Tool:
                        switch (Util.ParseEnum<ToolEnum>(diData["Subtype"]))
                        {
                            case ToolEnum.StaffOfIce:
                                return new IceStaff(id, diData);
                            case ToolEnum.Scythe:
                                return new Scythe(id, diData);
                            case ToolEnum.Sword:
                                return new Sword(id, diData);
                            case ToolEnum.CapeOfBlinking:
                                return new CapeOfBlinking(id, diData);
                            default:
                                return new Tool(id, diData);
                        }
                    default:
                        return new Item(id, diData, num);
                }
            }
            return null;
        }
        public static string GetItemDictionaryKey(int id, string key)
        {
            if (_diItemData.ContainsKey(id)) { return _diItemData[id][key]; }
            else { return string.Empty; }
        }
        public static Dictionary<string,string> GetItemDictionaryData(int id)
        {
            if (_diItemData.ContainsKey(id)) { return _diItemData[id]; }
            else { return null; }
        }

        /// <summary>
        /// Creates and returns a WorldObject based on the given ID
        /// </summary>
        /// <param name="id">The ID of the WorldObject</param>
        /// <returns>The WorldObject if it was successfully created, null otherwise</returns>
        public static WorldObject CreateWorldObjectByID(int id, Dictionary<string, string> args = null)
        {
            if (id != -1 && _diWorldObjects.ContainsKey(id))
            {
                Dictionary<string, string> data = new Dictionary<string, string>(_diWorldObjects[id]);
                if (args != null)
                {
                    foreach (KeyValuePair<string, string> kvp in args)
                    {
                        if (!data.ContainsKey(kvp.Key))
                        {
                            data.Add(kvp.Key, kvp.Value);
                        }
                    }
                }

                switch (Util.ParseEnum<ObjectTypeEnum>(data["Type"]))
                {
                    case ObjectTypeEnum.Beehive:
                        return new Beehive(id, data);
                    case ObjectTypeEnum.Buildable:
                        return new Buildable(id, data);
                    case ObjectTypeEnum.Building:
                        return new Building(id, data);
                    case ObjectTypeEnum.Hazard:
                        return new Hazard(id, data);
                    case ObjectTypeEnum.Container:
                        return new Container(id, data);
                    case ObjectTypeEnum.Decor:
                        return new Decor(id, data);
                    case ObjectTypeEnum.Destructible:
                        if (data.ContainsKey("Tree")) { return new Tree(id, data); }
                        else { return new Destructible(id, data); }
                    case ObjectTypeEnum.DungeonObject:
                        switch (Util.ParseEnum<TriggerObjectEnum>(data["Subtype"]))
                        {
                            case TriggerObjectEnum.ColorBlocker:
                                return new ColorBlocker(id, data);
                            case TriggerObjectEnum.ColorSwitch:
                                return new ColorSwitch(id, data);
                            case TriggerObjectEnum.Trigger:
                                return new Trigger(id, data);
                            case TriggerObjectEnum.KeyDoor:
                                return new KeyDoor(id, data);
                            case TriggerObjectEnum.MobDoor:
                                return new MobDoor(id, data);
                            case TriggerObjectEnum.TriggerDoor:
                                return new TriggerDoor(id, data);
                            case TriggerObjectEnum.FloorSwitch:
                                return new FloorSwitch(id, data);
                        }
                        break;
                    case ObjectTypeEnum.Floor:
                        return new Floor(id, data);
                    case ObjectTypeEnum.Garden:
                        return new Garden(id, data);
                    case ObjectTypeEnum.Gatherable:
                        return new WrappedItem(id, data);
                    case ObjectTypeEnum.Machine:
                        return new Machine(id, data);
                    case ObjectTypeEnum.Mailbox:
                        return new Mailbox(id, data);
                    case ObjectTypeEnum.Plant:
                        return new Plant(id, data);
                    case ObjectTypeEnum.Structure:
                        return new Structure(id, data);
                    case ObjectTypeEnum.Wall:
                        return new Wall(id, data);
                    case ObjectTypeEnum.Wallpaper:
                        return new Wallpaper(id, data);
                    case ObjectTypeEnum.WarpPoint:
                        return new WarpPoint(id, data);
                    case ObjectTypeEnum.WorldObject:
                        return new WorldObject(id, data);
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
        public static void CreateAndPlaceNewWorldObject(int id, Point pos, RHMap map)
        {
            CreateWorldObjectByID(id)?.PlaceOnMap(pos, map);
        }

        public static Dictionary<string, string> GetWorldObjectData(int id)
        {
            if (_diWorldObjects.ContainsKey(id)) { return _diWorldObjects[id]; }
            else { return null; }
        }

        public static int NumberOfClasses()
        {
            return _diJobs.Count;
        }

        public static string GetMonsterTraitData(string trait)
        {
            return _diMonsterTraits[trait];
        }

        public static WorldActor CreateNPCByIndex(int id)
        {
            if (id != -1 && _diItemData.ContainsKey(id))
            {
                Dictionary<string, string> diData = _diNPCData[id];
                switch (Util.ParseEnum<WorldActorTypeEnum>(diData["Type"]))
                {
                    case WorldActorTypeEnum.Child:
                        return new Child(id, diData);
                    case WorldActorTypeEnum.Critter:
                        return new Critter(id, diData);
                    case WorldActorTypeEnum.Mob:
                        switch (Util.ParseEnum<MobTypeEnum>(diData["Subtype"]))
                        {
                            case MobTypeEnum.Fly:
                                return new FlyingMob(id, diData);
                            case MobTypeEnum.Skitter:
                                return new SkitterMob(id, diData);
                        }
                        break;
                    case WorldActorTypeEnum.Mount:
                        return new Mount(id, diData);
                    case WorldActorTypeEnum.Pet:
                        return new Pet(id, diData);
                    case WorldActorTypeEnum.Animal:
                        return new Animal(id, diData);
                    case WorldActorTypeEnum.ShippingGremlin:
                        return new ShippingGremlin(id, diData);
                    case WorldActorTypeEnum.Spirit:
                        return new Spirit(diData);
                    case WorldActorTypeEnum.TalkingActor:
                        return new TalkingActor(id, diData);
                    case WorldActorTypeEnum.Traveler:
                        return new Traveler(id, diData);
                }
            }
            return null;
        }
        public static Mob CreateMob(int id)
        {
            WorldActor rv = CreateNPCByIndex(id);
            if (rv != null && !rv.IsActorType(WorldActorTypeEnum.Mob))
            {
                rv = null;
            }
            return (Mob)rv;
        }
        public static Child CreateChild(int id)
        {
            WorldActor rv = CreateNPCByIndex(id);
            if (rv != null && !rv.IsActorType(WorldActorTypeEnum.Child))
            {
                rv = null;
            }
            return (Child)rv;
        }
        public static Mount CreateMount(int id)
        {
            WorldActor rv = CreateNPCByIndex(id);
            if (rv != null && !rv.IsActorType(WorldActorTypeEnum.Mount))
            {
                rv = null;
            }
            return (Mount)rv;
        }
        public static Pet CreatePet(int id)
        {
            WorldActor rv = CreateNPCByIndex(id);
            if (rv != null && !rv.IsActorType(WorldActorTypeEnum.Pet))
            {
                rv = null;
            }
            return (Pet)rv;
        }
        public static Traveler CreateTraveler(int id)
        {
            WorldActor rv = CreateNPCByIndex(id);
            if (rv != null && !rv.IsActorType(WorldActorTypeEnum.Traveler))
            {
                rv = null;
            }
            return (Traveler)rv;
        }
        public static Animal CreateAnimal(int id)
        {
            WorldActor rv = CreateNPCByIndex(id);
            if (rv != null && !rv.IsActorType(WorldActorTypeEnum.Animal))
            {
                rv = null;
            }
            return (Animal)rv;
        }
        public static Critter CreateCritter(int id)
        {
            WorldActor rv = CreateNPCByIndex(id);
            if (rv != null && !rv.IsActorType(WorldActorTypeEnum.Critter))
            {
                rv = null;
            }
            return (Critter)rv;
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
            else
            {
                ErrorManager.TrackError();
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
                case GameIconEnum.Coin:
                    rv = new GUIImage(new Rectangle(0, 32, 16, 16), DIALOGUE_TEXTURE);
                    break;
                case GameIconEnum.Key:
                    rv = new GUIImage(new Rectangle(16, 16, 16, 16), DIALOGUE_TEXTURE);
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
        public static Dictionary<string, TextEntry> GetNPCDialogue(string key)
        {
            Dictionary<string, TextEntry> rv = new Dictionary<string, TextEntry>();

            if (_diNPCDialogue.ContainsKey(key))
            {
                foreach(KeyValuePair<string, string> kvp in _diNPCDialogue[key])
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
        public static string GetTextData(int id, string key, DataType type)
        {
            return GetTextData(Util.GetEnumString(type) + "_" + id, key);
        }
        public static string GetTextData(string textKey, string key)
        {
            string value = string.Empty;
            if (_diObjectText[textKey].ContainsKey(key)) {
                value = _diObjectText[textKey][key];
            }

            return Util.ProcessText(value);
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