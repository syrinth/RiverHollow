using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Database_Editor.Classes
{
    public static class Constants
    {
        public enum EditableNPCDataEnum { Dialogue, Schedule };
        public enum XMLTypeEnum { None, Actor, Task, WorldObject, Item, Shop, StatusEffect, Cutscene, Light, Dungeon, Upgrade, TextFile };

        public enum ComponentTypeEnum { TextBoxName, TextBoxID, TextBoxDescription, DataGrid, DataGridTags, TabIndex, ColumnId, ColumnName, ColumnTags, ComboBoxType, ComboBoxSubtype };

        public static readonly string SPECIAL_CHARACTER = "^";
        public static readonly string LOOKUP_CHARACTER = "$";
        public static readonly string PATH_TO_CONTENT = string.Format(@"{0}\..\..\..\..\RiverHollow\RiverHollowGame\Content", System.Environment.CurrentDirectory);
        public static readonly string PATH_TO_MAPS = PATH_TO_CONTENT + @"\Maps";
        public static readonly string PATH_TO_DATA = PATH_TO_CONTENT + @"\Data";
        public static readonly string PATH_TO_BACKUP = @"E:\Programming\RiverHollow Backups";
        public static readonly string PATH_TO_TEXT_FILES = PATH_TO_DATA + @"\Text Files";
        public static readonly string PATH_TO_DIALOGUE = PATH_TO_TEXT_FILES + @"\Dialogue";
        public static readonly string PATH_TO_VILLAGER_DIALOGUE = PATH_TO_DIALOGUE + @"\Villagers";
        public static readonly string PATH_TO_CUTSCENE_DIALOGUE = PATH_TO_DIALOGUE + @"\Cutscenes";
        public static readonly string PATH_TO_SCHEDULES = PATH_TO_DATA + @"\Schedules";

        public static readonly string DEFAULT_ACTOR_TAGS = "Key:,Size:16-32,Idle:0-0-1-0-T,Walk:64-0-3-.2-T";
        public static readonly string DEFAULT_TASK_TAGS = "StartNPC:,AssignTrigger:";
        public static readonly string DEFAULT_WORLD_OBJECT_TAGS = "Image:0-0,Size:1-1,BaseOffset:0-0,Base:1-1";
        public static readonly string DEFAULT_ITEM_TAGS = "Image:,Value:";
        public static readonly string DEFAULT_SHOP_TAGS = "Shopkeeper:,ItemID:";
        public static readonly string DEFAULT_LIGHT_TAGS = "Texture:,Idle:1-1,Dimensions:";
        public static readonly string DEFAULT_UPGRADE_TAGS = "Icon:,Cost:100,ItemID:";

        #region XML Files
        public static readonly string ACTOR_XML_FILE = PATH_TO_DATA + @"\ActorData.xml";
        public static readonly string CONFIG_XML_FILE = PATH_TO_DATA + @"\Config.xml";
        public static readonly string CUTSCENE_XML_FILE = PATH_TO_DATA + @"\CutScenes.xml";
        public static readonly string DUNGEON_XML_FILE = PATH_TO_DATA + @"\DungeonData.xml";
        public static readonly string ITEM_DATA_XML_FILE = PATH_TO_DATA + @"\ItemData.xml";
        public static readonly string LIGHTS_XML_FILE = PATH_TO_DATA + @"\LightData.xml";
        public static readonly string UPGRADES_XML_FILE = PATH_TO_DATA + @"\Upgrades.xml";
        public static readonly string SHOPS_XML_FILE = PATH_TO_DATA + @"\Shops.xml";
        public static readonly string STATUS_EFFECTS_XML_FILE = PATH_TO_DATA + @"\StatusEffects.xml";
        public static readonly string TASK_XML_FILE = PATH_TO_DATA + @"\Tasks.xml";
        public static readonly string WORLD_OBJECTS_DATA_XML_FILE = PATH_TO_DATA + @"\WorldObjects.xml";

        public static readonly string OBJECT_TEXT_XML_FILE = PATH_TO_TEXT_FILES + @"\Object_Text.xml";
        #endregion

        #region Tags
        public static readonly string TAGS_FOR_ITEMS = "ItemKeyID,ReqItems,ItemID,GoalItem,ItemRewardID,Collection,Makes,Processes,GearID,RequestIDs,SeedID,HoneyID,UnlockItemID";
        public static readonly string TAGS_FOR_WORLD_OBJECTS = "BuildingID,HouseID,RequiredBuildingID,UnlockObjectID,ObjectID,Wall,Floor,Resources,Place,SubObjects,TargetObjectID,RequiredObjectID,EntranceID";
        public static readonly string TAGS_FOR_SHOPDATA = "ShopData,TargetShopID";
        public static readonly string TAGS_FOR_ACTORS = "NPC_ID,Shopkeeper,MobID,Actors,Move,Face,Speak,Speed,Activate,Deactivate,Join,Combat,StartNPC,GoalNPC";
        public static readonly string TAGS_FOR_STATUS_EFFECTS = "StatusEffectID";
        public static readonly string TAGS_FOR_LIGHTS = "LightID";
        public static readonly string TAGS_FOR_UPGRADES = "UpgradeID";
        public static readonly string TAGS_FOR_DUNGEONS = "DungeonID";
        public static readonly string TAGS_FOR_TASKS = "TaskID";

        public static readonly string ITEM_REF_TAGS = "ReqItems,Place";
        public static readonly string TASK_REF_TAGS = "GoalItem,ItemRewardID,BuildingID,UnlockBuildingID,TargetObjectID,RequiredObjectID,StartNPC,GoalNPC";
        public static readonly string ACTOR_REF_TAGS = "BuildingID,Collection,Class,MobID,ShopData,HouseID,RequiredBuildingID,RequiredObjectID,RequestIDs";
        public static readonly string WORLD_OBJECT_REF_TAGS = "ReqItems,LightID,Makes,Processes,ItemID,SubObjects,SeedID,HoneyID,LightID";
        public static readonly string SHOPDATA_REF_TAGS = "ItemID,BuildingID,ObjectID,NPC_ID";
        public static readonly string CONFIG_REF_TAG = "ItemID,ObjectID";
        public static readonly string DUNGEON_REF_TAGS = "ObjectID,MobID,EntranceID";
        public static readonly string TEXTFILE_REF_TAGS = "ItemID,UnlockObjectID,UnlockItemID,TargetShopID,TaskID";
        public static readonly string CUTSCENE_REF_TAGS = "Actors,Move,Face,Speak,Speed,Activate,Deactivate,Join";

        public static readonly string MAP_REF_TAGS = "ItemKeyID,ItemID,Resources,ObjectID,NPC_ID,MobID";
        #endregion
    }
}
