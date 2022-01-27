using System.Collections.Generic;

namespace Database_Editor.Classes
{
    public static class Constants
    {
        public enum EditableCharacterDataEnum { Dialogue, Schedule };
        public enum XMLTypeEnum { None, Task, NPC, Class, Building, WorldObject, Item, Monster, Action, Shop, StatusEffect, Cutscene, Light, Dungeon, TextFile };

        public static readonly string SPECIAL_CHARACTER = "^";
        public static readonly string LOOKUP_CHARACTER = "$";
        public static readonly string PATH_TO_CONTENT = string.Format(@"{0}\..\..\..\..\RiverHollow\RiverHollowGame\Content", System.Environment.CurrentDirectory);
        public static readonly string PATH_TO_MAPS = PATH_TO_CONTENT + @"\Maps";
        public static readonly string PATH_TO_DATA = PATH_TO_CONTENT + @"\Data";
        public static readonly string PATH_TO_BACKUP = PATH_TO_CONTENT + @"\Data\Backups";
        public static readonly string PATH_TO_TEXT_FILES = PATH_TO_DATA + @"\Text Files";
        public static readonly string PATH_TO_DIALOGUE = PATH_TO_TEXT_FILES + @"\Dialogue";
        public static readonly string PATH_TO_VILLAGER_DIALOGUE = PATH_TO_DIALOGUE + @"\Villagers";
        public static readonly string PATH_TO_CUTSCENE_DIALOGUE = PATH_TO_DIALOGUE + @"\Cutscenes";
        public static readonly string PATH_TO_SCHEDULES = PATH_TO_DATA + @"\Schedules";

        #region XML Files
        public static readonly string ACTIONS_XML_FILE = PATH_TO_DATA + @"\CombatActions.xml";
        public static readonly string CLASSES_XML_FILE = PATH_TO_DATA + @"\Classes.xml";
        public static readonly string NPC_XML_FILE = PATH_TO_DATA + @"\CharacterData.xml";
        public static readonly string CONFIG_XML_FILE = PATH_TO_DATA + @"\Config.xml";
        public static readonly string CUTSCENE_XML_FILE = PATH_TO_DATA + @"\CutScenes.xml";
        public static readonly string DUNGEON_XML_FILE = PATH_TO_DATA + @"\DungeonData.xml";
        public static readonly string ITEM_DATA_XML_FILE = PATH_TO_DATA + @"\ItemData.xml";
        public static readonly string LIGHTS_XML_FILE = PATH_TO_DATA + @"\LightData.xml";
        public static readonly string MONSTERS_XML_FILE = PATH_TO_DATA + @"\Monsters.xml";
        public static readonly string SHOPS_XML_FILE = PATH_TO_DATA + @"\Shops.xml";
        public static readonly string STATUS_EFFECTS_XML_FILE = PATH_TO_DATA + @"\StatusEffects.xml";
        public static readonly string TASK_XML_FILE = PATH_TO_DATA + @"\Tasks.xml";
        public static readonly string WORLD_OBJECTS_DATA_XML_FILE = PATH_TO_DATA + @"\WorldObjects.xml";

        public static readonly string OBJECT_TEXT_XML_FILE = PATH_TO_TEXT_FILES + @"\Object_Text.xml";
        #endregion

        #region Tags
        public static readonly string TAGS_FOR_ITEMS = "ItemKeyID,ReqItems,ItemID,GoalItem,ItemRewardID,Collection,Makes,Processes,GearID,RequestIDs,SeedID,HoneyID,UnlockItemID";
        public static readonly string TAGS_FOR_WORLD_OBJECTS = "BuildingID,HouseID,RequiredBuildingID,UnlockObjectID,ObjectID,Wall,Floor,Resources,Place,SubObjects,TargetObjectID,RequiredObjectID,EntranceID";
        public static readonly string TAGS_FOR_COMBAT_ACTIONS = "Ability,Spell";
        public static readonly string TAGS_FOR_CLASSES = "Class";
        public static readonly string TAGS_FOR_SHOPDATA = "ShopData,TargetShopID";
        public static readonly string TAGS_FOR_CHARACTERS = "NPC_ID,MobID,Actors,Move,Face,Speak,Speed,Activate,Deactivate";
        public static readonly string TAGS_FOR_STATUS_EFFECTS = "StatusEffectID";
        public static readonly string TAGS_FOR_LIGHTS = "LightID";
        public static readonly string TAGS_FOR_MONSTERS = "MonsterID";
        public static readonly string TAGS_FOR_DUNGEONS = "DungeonID";
        public static readonly string TAGS_FOR_TASKS = "TaskID";

        public static readonly string ITEM_REF_TAGS = "ReqItems,Place";
        public static readonly string TASK_REF_TAGS = "GoalItem,ItemRewardID,BuildingID,UnlockBuildingID,TargetObjectID,RequiredObjectID";
        public static readonly string CHARACTER_REF_TAGS = "BuildingID,Collection,Class,MonsterID,ShopData,HouseID,RequiredBuildingID,RequiredObjectID,RequestIDs";
        public static readonly string WORLD_OBJECT_REF_TAGS = "ReqItems,LightID,Makes,Processes,ItemID,SubObjects,SeedID,HoneyID,LightID";
        public static readonly string CLASSES_REF_TAGS = "GearID,Ability,Spell";
        public static readonly string SHOPDATA_REF_TAGS = "ItemID,BuildingID,ObjectID,NPC_ID";
        public static readonly string CONFIG_REF_TAG = "ItemID,ObjectID";
        public static readonly string MONSTERS_REF_TAGS = "Loot,Ability,Spell";
        public static readonly string ACTIONS_REF_TAGS = "StatusEffectID,NPC_ID";
        public static readonly string DUNGEON_REF_TAGS = "ObjectID,MonsterID,EntranceID";
        public static readonly string TEXTFILE_REF_TAGS = "ItemID,UnlockObjectID,UnlockItemID,TargetShopID,TaskID";
        public static readonly string CUTSCENE_REF_TAGS = "";

        public static readonly string MAP_REF_TAGS = "ItemKeyID,ItemID,Resources,ObjectID,NPCID";
        #endregion
    }
}
