﻿using System.IO;
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
using RiverHollow.Items;
using RiverHollow.WorldObjects.Trigger_Objects;
using RiverHollow.Items.Tools;

using RiverHollow.Characters.Mobs;
using System.Linq;

using static RiverHollow.Utilities.Enums;
using System;

namespace RiverHollow.Game_Managers
{
    public static class DataManager
    {
        #region Constants
        private const string TEXTURES = @"Textures\";
        public const string FOLDER_ACTOR = TEXTURES + @"Actors\";

        public const string NPC_FOLDER = FOLDER_ACTOR + @"NPCs\";
        public const string MERCHANT_FOLDER = FOLDER_ACTOR + @"NPCs\Merchants\";
        public const string TRAVELER_FOLDER = FOLDER_ACTOR + @"NPCs\Travelers\";
        public const string VILLAGER_FOLDER = FOLDER_ACTOR + @"NPCs\Villagers\";
        public const string PORTRAIT_FOLDER = FOLDER_ACTOR + @"Portraits\";

        public const string FOLDER_ITEMS = TEXTURES + @"Items\";
        public const string FOLDER_COSMETICS = TEXTURES + @"Cosmetics\";
        public const string FOLDER_EFFECTS = TEXTURES + @"ActionEffects\";
        public const string FOLDER_ENVIRONMENT = TEXTURES + @"Environmental\";
        public const string FOLDER_MOBS = TEXTURES + @"Actors\Mobs\";
        public const string FOLDER_SUMMONS = TEXTURES + @"Actors\Summons\";
        public const string FOLDER_PLAYER = TEXTURES + @"Actors\Player\";
        public const string FOLDER_PARTY = TEXTURES + @"Actors\PartyMembers\";
        public const string FOLDER_TEXTFILES = @"Data\Text Files\";
        public const string FONT_MAIN = @"Fonts\Font_Main\Font_Main";
        public const string FONT_MAIN_DROPSHADOW = @"Fonts\Font_Main\Drop_Shadow\Font_Drop_Shadow";
        public const string FONT_NUMBERS = @"Fonts\Font_Numbers\Font_Numbers";

        public const string FOLDER_WORLDOBJECTS = TEXTURES + @"WorldObjects\";
        public const string FOLDER_BUILDINGS = FOLDER_WORLDOBJECTS + @"Buildings\";
        public const string FILE_WORLDOBJECTS = FOLDER_WORLDOBJECTS + @"World_Objects";
        public const string FILE_MISC_SPRITES = TEXTURES + @"Misc_Sprites";
        public const string FILE_DECOR = FOLDER_WORLDOBJECTS + @"Decor";
        public const string FILE_PLANTS = FOLDER_WORLDOBJECTS + @"Plants";
        public const string FILE_FLOORING = FOLDER_WORLDOBJECTS + @"texFlooring";
        public const string FILE_WALLS = FOLDER_WORLDOBJECTS + @"Walls";

        public const string UPGRADE_ICONS = GUI_COMPONENTS + @"\GUI_Upgrade_Icons";
        public const string GUI_COMPONENTS = TEXTURES + @"GUI Components";
        public const string ACTION_ICONS = GUI_COMPONENTS + @"\GUI_Action_Icons";
        public const string HUD_COMPONENTS = GUI_COMPONENTS + @"\GUI_HUD_Components";
        public const string PROJECTILE_TEXTURE = TEXTURES + @"Projectiles";

        public const string FACES_TEXTURE = PORTRAIT_FOLDER + @"Faces";

        public const string DATA_FOLDER = @"Data\";

        public const string CONFIG_FILE_NAME = "Config";
        public const string PLAYER_ANIMATION_FILE_NAME = "PlayerClassAnimationConfig";
        public const string ACTOR_FILE_NAME = "ActorData";
        public const string ADVENTURE_FILE_NAME = "AdventureData";
        public const string COSMETIC_FILE_NAME = "CosmeticData";
        public const string CUTSCENE_FILE_NAME = "CutsceneData";
        public const string ITEM_FILE_NAME = "ItemData";
        public const string LIGHT_FILE_NAME = "LightData";
        public const string SHOP_FILE_NAME = "ShopData";
        public const string STATUS_EFFECT_FILE_NAME = "StatusEffectData";
        public const string TASK_FILE_NAME = "TaskData";
        public const string UPGRADE_FILE_NAME = "UpgradeData";
        public const string WORLD_OBJECT_FILE_NAME = "WorldObjectData";

        public const string CONFIG_DATA_FILE = DATA_FOLDER + CONFIG_FILE_NAME;
        public const string PLAYER_ANIMATION_DATA_FILE = DATA_FOLDER + PLAYER_ANIMATION_FILE_NAME;
        public const string ACTOR_DATA_FILE = DATA_FOLDER + ACTOR_FILE_NAME;
        public const string ADVENTURE_DATA_FILE = DATA_FOLDER + ADVENTURE_FILE_NAME;
        public const string COSMETIC_DATA_FILE = DATA_FOLDER + COSMETIC_FILE_NAME;
        public const string CUTSCENE_DATA_FILE = DATA_FOLDER + CUTSCENE_FILE_NAME;
        public const string ITEM_DATA_FILE = DATA_FOLDER + ITEM_FILE_NAME;
        public const string LIGHT_DATA_FILE = DATA_FOLDER + LIGHT_FILE_NAME;
        public const string SHOP_DATA_FILE = DATA_FOLDER + SHOP_FILE_NAME;
        public const string STATUS_EFFECT_DATA_FILE = DATA_FOLDER + STATUS_EFFECT_FILE_NAME;
        public const string TASK_DATA_FILE = DATA_FOLDER + TASK_FILE_NAME;
        public const string UPGRADE_DATA_FILE = DATA_FOLDER + UPGRADE_FILE_NAME;
        public const string WORLD_OBJECT_DATA_FILE = DATA_FOLDER + WORLD_OBJECT_FILE_NAME;
        #endregion

        #region Dictionaries
        static Dictionary<string, Texture2D> _diTextures;
        static Dictionary<string, BitmapFont> _diBMFonts;
        static Dictionary<string, string> _diGameText;

        static Dictionary<int, string> _diLetterData;
        public static IReadOnlyDictionary<int, string> LetterData => _diLetterData;

        static Dictionary<int, List<string>> _diSongs;
        static Dictionary<string, Dictionary<string, string>> _diNPCDialogue;
        static Dictionary<int, Shop> _diShops;

        static Dictionary<int, Dictionary<string, string>> _diActorData;
        public static Dictionary<int, Dictionary<string, string>> ActorData => _diActorData;
        static Dictionary<int, Dictionary<string, string>> _diPlayerAnimationData;

        static Dictionary<string, Dictionary<string, string>> _diObjectText;

        static Dictionary<int, Dictionary<string, string>> _diItemData;

        public static List<int> ItemKeys => _diItemData.Keys.ToList();

        static Dictionary<int, Dictionary<string, string>> _diAdventureData;
        static Dictionary<int, Dictionary<string, string>> _diDungeonData;
        static Dictionary<int, Dictionary<string, string>> _diLightData;
        static Dictionary<int, Dictionary<string, string>> _diUpgradeData;
        static Dictionary<int, Dictionary<string, string>> _diStatusEffects;
        static Dictionary<int, Dictionary<string, string>> _diWorldObjects;

        static Dictionary<int, Dictionary<string, string>> _diCosmeticData;
        static Dictionary<RarityEnum, List<int>> _diRelics;
        static Dictionary<int, Dictionary<string, string>> _diTaskData;
        public static IReadOnlyDictionary<int, Dictionary<string, string>> Cosmetics => _diCosmeticData;
        public static IReadOnlyDictionary<RarityEnum, List<int>> Relics => _diRelics;
        public static IReadOnlyDictionary<int, Dictionary<string, string>> TaskData => _diTaskData;

        static Dictionary<string, Dictionary<string, List<string>>> _diSchedule;

        public static Dictionary<int, Dictionary<string, string>> Config;
        #endregion

        public static BitmapFont _bmFont;

        public static void LoadContent(ContentManager Content)
        {
            //Allocate Dictionaries
            _diTextures = new Dictionary<string, Texture2D>();

            _diRelics = new Dictionary<RarityEnum, List<int>>(); 

            _diLetterData = new Dictionary<int, string>();
            var letterTemplates = Content.Load<Dictionary<int, string>>(FOLDER_TEXTFILES + @"Mailbox_Text");
            foreach(var l in letterTemplates)
            {
                _diLetterData[l.Key] = l.Value;
            }

            //Read in Content and allocate the appropriate Dictionaries
            LoadGUIs(Content);
            LoadIcons(Content);
            LoadBMFonts(Content);
            LoadTextFiles(Content);
            LoadCharacters(Content);
            LoadDictionaries(Content);

            AddDirectoryTextures(GUI_COMPONENTS, Content);
            AddDirectoryTextures(FOLDER_ITEMS, Content);
            AddDirectoryTextures(FOLDER_COSMETICS, Content);
            AddDirectoryTextures(FOLDER_WORLDOBJECTS, Content);
            AddDirectoryTextures(FOLDER_BUILDINGS, Content);
            AddDirectoryTextures(FOLDER_ENVIRONMENT, Content);

            LoadNPCSchedules(Content);
        }

        #region Load Methods
        private delegate void LoadDictionaryWorkDelegate(int id, Dictionary<string, string> taggedDictionary);
        private static void LoadDictionaries(ContentManager Content)
        {
            LoadDictionary(ref Config, CONFIG_DATA_FILE, Content, null);
            LoadDictionary(ref _diPlayerAnimationData, PLAYER_ANIMATION_DATA_FILE, Content, null);
            LoadDictionary(ref _diItemData, ITEM_DATA_FILE, Content, LoadItemsDoWork);
            LoadDictionary(ref _diActorData, ACTOR_DATA_FILE, Content, null);
            LoadDictionary(ref _diAdventureData, ADVENTURE_DATA_FILE, Content, null);
            LoadDictionary(ref _diStatusEffects, STATUS_EFFECT_DATA_FILE, Content, null);
            LoadDictionary(ref _diTaskData, TASK_DATA_FILE, Content, null);
            LoadDictionary(ref _diLightData, LIGHT_DATA_FILE, Content, null);
            LoadDictionary(ref _diUpgradeData, UPGRADE_DATA_FILE, Content, null);

            LoadDictionary(ref _diCosmeticData, COSMETIC_DATA_FILE, Content, null);
            LoadDictionary(ref _diWorldObjects, WORLD_OBJECT_DATA_FILE, Content, null);
        }
        public static void SecondaryLoad(ContentManager Content)
        {
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

        private static void LoadItemsDoWork(int id, Dictionary<string, string> taggedDictionary)
        {
            if (taggedDictionary["Type"].Equals("Relic"))
            {
                var rarity = Util.ParseEnum<RarityEnum>(taggedDictionary["Rarity"]);
                Util.AddToListDictionary(ref _diRelics, rarity, id);
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
            foreach (string dialogueTags in rawTextInfo.Values)
            {
                Dictionary<string, string> tags = Util.DictionaryFromTaggedString(dialogueTags);
                string newKey = tags["Name"];
                tags.Remove("Name");
                _diGameText[newKey] = Util.StringFromTaggedDictionary(tags);
            }

            _diSongs = Content.Load<Dictionary<int, List<string>>>(@"Data\SongData");
            
            _diNPCDialogue = new Dictionary<string, Dictionary<string, string>>();

            foreach (string s in Directory.GetFiles(@"Content\" + FOLDER_TEXTFILES + @"Dialogue\Villagers"))
            {
                LoadDialogDictionary(Content, s);
            }
            foreach (string s in Directory.GetFiles(@"Content\" + FOLDER_TEXTFILES + @"Dialogue\Travelers"))
            {
                LoadDialogDictionary(Content, s);
            }
        }
        private static void LoadDialogDictionary(ContentManager Content, string fileName)
        {
            Dictionary<string, string> newDialogue;
            if (fileName.Contains(Constants.MAPOBJ_HOME))
            {
                string key = Path.GetFileName(fileName).Replace(Constants.MAPOBJ_HOME, "").Split('.')[0];

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
        private static void LoadCharacters(ContentManager Content)
        {
            AddDirectoryTextures(FOLDER_ACTOR, Content);
            AddTexture(@"Textures\texWeather", Content);
            AddTexture(@"Textures\texClothes", Content);
        }
        private static void LoadGUIs(ContentManager Content)
        {
            AddTexture(PROJECTILE_TEXTURE, Content);
            AddTexture(FILE_MISC_SPRITES, Content);
        }
        private static void LoadIcons(ContentManager Content)
        {
            AddDirectoryTextures(@"Textures\ActionEffects", Content);
            AddTexture(@"Textures\items", Content);
            AddTexture(@"Textures\AbilityAnimations", Content);
            AddTexture(@"Textures\Overworld", Content);
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
            AddBMFont(FONT_MAIN, Content);
            AddBMFont(FONT_MAIN_DROPSHADOW, Content);
            AddBMFont(FONT_NUMBERS, Content);
        }
        private static void AddBMFont(string font, ContentManager Content)
        {
            _diBMFonts.Add(font, Content.Load<BitmapFont>(font));
        }

        private static void LoadShopFile(ContentManager Content)
        {
            Dictionary<int, string> shopFile = Content.Load<Dictionary<int, string>>(SHOP_DATA_FILE);

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
        public static float GetFloatByIDKey(int id, string key, DataType type, float defaultValue = -1)
        {
            string rv = GetStringByIDKey(id, key, type);

            if (!string.IsNullOrEmpty(rv)) { return Util.ParseFloat(rv); }
            else { return defaultValue; }
        }
        public static Point GetPointByIDKey(int id, string key, DataType type, Point defaultValue = default)
        {
            Point rv = defaultValue;
            string value = GetStringByIDKey(id, key, type);

            if (!string.IsNullOrEmpty(value))
            {
                rv = Util.ParsePoint(value);
            }

            return rv;
        }
        public static Rectangle GetRectangleByIDKey(int id, string key, DataType type, Rectangle defaultValue = default)
        {
            Rectangle rv = defaultValue;
            string value = GetStringByIDKey(id, key, type);

            if (!string.IsNullOrEmpty(value))
            {
                rv = Util.ParseRectangle(value);
            }

            return rv;
        }
        public static string GetStringByIDKey(int id, string key, DataType type, string defaultValue = "")
        {
            if (id != -1)
            {
                switch (type)
                {
                    case DataType.Actor:
                        if (_diActorData[id].ContainsKey(key)) { return _diActorData[id][key]; }
                        break;
                    case DataType.Adventure:
                        if (_diAdventureData[id].ContainsKey(key)) { return _diAdventureData[id][key]; }
                        break;
                    case DataType.Cosmetic:
                        if (_diCosmeticData[id].ContainsKey(key)) { return _diCosmeticData[id][key]; }
                        break;
                    case DataType.Item:
                        if (_diItemData[id].ContainsKey(key)) { return _diItemData[id][key]; }
                        break;
                    case DataType.Light:
                        if (_diLightData[id].ContainsKey(key)) { return _diLightData[id][key]; }
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
                        if (id > Constants.BUILDABLE_ID_OFFSET)
                        {
                            id -= Constants.BUILDABLE_ID_OFFSET;
                        }

                        if (_diWorldObjects[id].ContainsKey(key)) { return _diWorldObjects[id][key]; }
                        break;
                }
            }

            return defaultValue;
        }
        public static string[] GetStringArgsByIDKey(int id, string key, DataType type, string defaultValue = "")
        {
            return Util.FindArguments(GetStringByIDKey(id, key, type, defaultValue));
        }
        public static string[] GetStringParamsByIDKey(int id, string key, DataType type, string defaultValue = "")
        {
            return Util.FindParams(GetStringByIDKey(id, key, type, defaultValue));
        }
        public static int[] GetIntParamsByIDKey(int id, string key, DataType type, string defaultValue = "")
        {
            var p = Util.FindParams(GetStringByIDKey(id, key, type, defaultValue));

            if (p.Length > 0)
            {
                int[] rv = new int[p.Length];
                for (int i = 0; i < p.Length; i++)
                {
                    rv[i] = int.Parse(p[i]);
                }

                return rv;
            }

            return new int[0];
        }

        public static bool GetBoolByIDKey(int id, string key, DataType type)
        {
            switch (type)
            {
                case DataType.Actor:
                    if (_diActorData.ContainsKey(id) && _diActorData[id].ContainsKey(key)) { return _diActorData[id].ContainsKey(key); }
                    break;
                case DataType.Cosmetic:
                    if (_diCosmeticData.ContainsKey(id) && _diCosmeticData[id].ContainsKey(key)) { return _diCosmeticData[id].ContainsKey(key); }
                    break;
                case DataType.Item:
                    if (_diItemData.ContainsKey(id) && _diItemData[id].ContainsKey(key)) { return _diItemData[id].ContainsKey(key); }
                    break;
                case DataType.Light:
                    if (_diLightData.ContainsKey(id) && _diLightData[id].ContainsKey(key)) { return _diLightData[id].ContainsKey(key); }
                    break;
                case DataType.StatusEffect:
                    if (_diStatusEffects.ContainsKey(id) && _diStatusEffects[id].ContainsKey(key)) { return _diStatusEffects[id].ContainsKey(key); }
                    break;
                case DataType.Task:
                    if (_diTaskData.ContainsKey(id) && _diTaskData[id].ContainsKey(key)) { return _diTaskData[id].ContainsKey(key); }
                    break;
                case DataType.Upgrade:
                    if (_diUpgradeData.ContainsKey(id) && _diUpgradeData[id].ContainsKey(key)) { return _diUpgradeData[id].ContainsKey(key); }
                    break;
                case DataType.WorldObject:
                    if (_diWorldObjects.ContainsKey(id) && _diWorldObjects[id].ContainsKey(key)) { return _diWorldObjects[id].ContainsKey(key); }
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
                //Split by "/" for each set
                string[] split = Util.FindParams(value);
                foreach (string s in split)
                {
                    string[] splitData = Util.FindArguments(s);
                    dictValue[Util.ParseInt(splitData[0])] = splitData.Length == 2 ? Util.ParseInt(splitData[1]) : 1; ;
                }
            }
            return dictValue;
        }
        #endregion

        #region GetMethods
        public static Dictionary<int, Upgrade> GetGlobalUpgrades()
        {
            var rv = new Dictionary<int, Upgrade>();
            foreach (var upgradeData in _diUpgradeData)
            {
                if (Util.ParseEnum<UpgradeTypeEnum>(upgradeData.Value["Type"]) == UpgradeTypeEnum.Global)
                {
                    rv[upgradeData.Key] = new Upgrade(upgradeData.Key);
                }
            }
            return rv;
        }
        public static Light GetLight(int id)
        {
            if (_diLightData.ContainsKey(id))
            {
                return new Light(id, _diLightData[id]);
            }
            return null;
        }
        public static Cosmetic GetCosmetic(int id)
        {
            if (_diCosmeticData.ContainsKey(id))
            {
                return new Cosmetic(id);
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

        public static Item CraftItem(int id, int batchSize = 1)
        {
            if (id >= Constants.BUILDABLE_ID_OFFSET)
            {
                return GetItem(id, batchSize);
            }
            else
            {
                return GetItem(id, GetIntByIDKey(id, "CraftAmount", DataType.Item, 1) * batchSize);
            }
        }

        public static Item GetItem(Buildable obj, int num = 1)
        {
            if (obj == null || obj.Unique) { return null; }
            else { return GetItem(obj.ID + Constants.BUILDABLE_ID_OFFSET, num); }
        }
        public static Item GetItem(int id)
        {
            return GetItem(id, 1);
        }
        public static Item GetItem(int id, int num)
        {
            if (id >= Constants.BUILDABLE_ID_OFFSET)
            {
                var obj = new WrappedObjectItem(id - Constants.BUILDABLE_ID_OFFSET);
                if (obj.Stacks())
                {
                    obj.SetNumber(num);
                }
                return obj;
            }

            if (id != -1 && _diItemData.ContainsKey(id))
            {
                var diData = _diItemData[id];
                switch (Util.ParseEnum<ItemTypeEnum>(_diItemData[id]["Type"]))
                {
                    case ItemTypeEnum.Blueprint:
                        return new Blueprint(id); 
                    case ItemTypeEnum.Consumable:
                        return new Consumable(id, num);
                    case ItemTypeEnum.Cosmetic:
                        return new CosmeticItem(id);
                    case ItemTypeEnum.Food:
                        return new Food(id, num);
                    case ItemTypeEnum.Merchandise:
                        return new Merchandise(id, num);
                    case ItemTypeEnum.MonsterFood:
                        return new MonsterFood(id, num);
                    case ItemTypeEnum.NPCToken:
                        return new NPCToken(id);
                    case ItemTypeEnum.Relic:
                        return new Relic(id, num);
                    case ItemTypeEnum.Seed:
                        return new Seed(id, num);
                    case ItemTypeEnum.Special:
                        return new Special(id);
                    case ItemTypeEnum.Tool:
                        switch (Util.ParseEnum<ToolEnum>(diData["Subtype"]))
                        {
                            case ToolEnum.FishingRod:
                                return new FishingRod(id);
                            case ToolEnum.StaffOfIce:
                                return new IceStaff(id);
                            case ToolEnum.Scythe:
                                return new Scythe(id);
                            case ToolEnum.Sword:
                                return new Sword(id);
                            case ToolEnum.CapeOfBlinking:
                                return new CapeOfBlinking(id);
                            default:
                                return new Tool(id);
                        }
                    default:
                        return new Item(id, num);
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
            if(args == null)
            {
                args = new Dictionary<string, string>();
            }

            if (id >= Constants.BUILDABLE_ID_OFFSET)
            {
                id -= Constants.BUILDABLE_ID_OFFSET;
            }

            if (id != -1 && _diWorldObjects.ContainsKey(id))
            {
                switch (GetEnumByIDKey<ObjectTypeEnum>(id, "Type", DataType.WorldObject))
                {
                    case ObjectTypeEnum.Buildable:
                        switch (GetEnumByIDKey<BuildableEnum>(id, "Subtype", DataType.WorldObject))
                        {
                            case BuildableEnum.Beehive:
                                return new Beehive(id);
                            case BuildableEnum.Building:
                                return new Building(id);
                            case BuildableEnum.Container:
                                return new Container(id, args);
                            case BuildableEnum.Decor:
                                return new Decor(id, args);
                            case BuildableEnum.Field:
                                return new Field(id);
                            case BuildableEnum.Floor:
                                if (GetBoolByIDKey(id, "Earth", DataType.WorldObject)) { return new Earth(id); }
                                else { return new Floor(id); }
                            case BuildableEnum.Mailbox:
                                return new Mailbox(id);
                            case BuildableEnum.Structure:
                                return new Structure(id);
                            case BuildableEnum.Wall:
                                return new Wall(id);
                            case BuildableEnum.Wallpaper:
                                return new Wallpaper(id);
                            default:
                                return new Buildable(id);
                        }

                    case ObjectTypeEnum.Hazard:
                        return new Hazard(id);
                    case ObjectTypeEnum.Destructible:
                        return new Destructible(id);
                    case ObjectTypeEnum.DungeonObject:
                        switch (GetEnumByIDKey<TriggerObjectEnum>(id, "Subtype", DataType.WorldObject))
                        {
                            case TriggerObjectEnum.ColorBlocker:
                                return new ColorBlocker(id, args);
                            case TriggerObjectEnum.ColorSwitch:
                                return new ColorSwitch(id, args);
                            case TriggerObjectEnum.Trigger:
                                return new Trigger(id, args);
                            case TriggerObjectEnum.KeyDoor:
                                return new KeyDoor(id, args);
                            case TriggerObjectEnum.MobDoor:
                                return new MobDoor(id, args);
                            case TriggerObjectEnum.TriggerDoor:
                                return new TriggerDoor(id, args);
                            case TriggerObjectEnum.FloorSwitch:
                                return new FloorSwitch(id, args);
                        }
                        break;

                    case ObjectTypeEnum.Gatherable:
                        return new WrappedItem(id);
                    case ObjectTypeEnum.Plant:
                        return new Plant(id);
                    case ObjectTypeEnum.WarpPoint:
                        return new WarpPoint(id);
                    case ObjectTypeEnum.WorldObject:
                        return new WorldObject(id, args);
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

        public static List<int> GetWorldObjectsWithKey(string key)
        {
            var list = _diWorldObjects.Where(x => GetBoolByIDKey(x.Key, key, DataType.WorldObject)).Select(y => y.Key).ToList();

            return list;
        }

        private static Actor CreateNPCByIndex(int id)
        {
            if (id != -1 && _diItemData.ContainsKey(id))
            {
                Dictionary<string, string> diData = _diActorData[id];
                switch (Util.ParseEnum<ActorTypeEnum>(diData["Type"]))
                {
                    case ActorTypeEnum.Child:
                        return new Child(id, diData);
                    case ActorTypeEnum.Critter:
                        return new Critter(id, diData);
                    case ActorTypeEnum.Effect:
                        return new ActionEffect(id, diData);
                    case ActorTypeEnum.Mob:
                        switch (Util.ParseEnum<MobTypeEnum>(diData["Subtype"]))
                        {
                            case MobTypeEnum.Basic:
                                return new Mob(id, diData);
                            case MobTypeEnum.Mage:
                                return new Mage(id, diData);
                            case MobTypeEnum.Shooter:
                                return new Shooter(id, diData);
                            case MobTypeEnum.Summoner:
                                return new Summoner(id, diData);
                        }
                        break;
                    case ActorTypeEnum.Mount:
                        return new Mount(id, diData);
                    case ActorTypeEnum.Pet:
                        return new Pet(id, diData);
                    case ActorTypeEnum.Projectile:
                        return new Projectile(id, diData);
                    case ActorTypeEnum.Animal:
                        return new Animal(id, diData);
                    case ActorTypeEnum.Spirit:
                        return new Spirit(diData);
                    case ActorTypeEnum.TalkingActor:
                        return new TalkingActor(id, diData);
                    case ActorTypeEnum.Traveler:
                        return new Traveler(id, diData);
                    case ActorTypeEnum.Fish:
                        return new Fish(id, diData);
                }
            }
            return null;
        }

        public static T CreateActor<T>(int id) where T : class
        {
            T rv = null;
            var actor = CreateNPCByIndex(id);

            if(actor is T typedActor)
            {
                rv = typedActor;
            }
            return rv;
        }

        public static List<Actor> GetAllTravelers()
        {
            var actors = new List<Actor>();
            TownManager.DITravelerInfo.Keys.ToList().ForEach(x => actors.Add(CreateActor<Traveler>(x)));

            return actors;
        }

        public static List<Actor> GetAllMobs()
        {
            var actors = new List<Actor>();
            TownManager.DIMobInfo.Keys.ToList().ForEach(x => actors.Add(CreateActor<Mob>(x)));

            return actors;
        }

        public static StatusEffect GetStatusEffectByIndex(int id)
        {
            StatusEffect b = null;
            if (id != -1)
            {
                b = new StatusEffect(id);
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

            try
            {
                if (_diTextures.ContainsKey(texture))
                {
                    rv = _diTextures[texture];
                }
            }
            catch(Exception ex)
            {
                LogManager.LogException(ex);
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

        public static TextEntry GetGameTextEntry(string key, params object[] formatParameters)
        {
            TextEntry entry = new TextEntry(key, Util.DictionaryFromTaggedString(GetGameText(key)));
            entry.FormatText(formatParameters);
            return entry;
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
    }
}