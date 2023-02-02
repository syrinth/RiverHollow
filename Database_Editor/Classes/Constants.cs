using System.Collections.Generic;

namespace Database_Editor.Classes
{
    public static class Constants
    {
        public enum EditableNPCDataEnum { Dialogue, Schedule };
        public enum XMLTypeEnum { None, NPC, Task, Job, WorldObject, Item, Monster, Action, Shop, StatusEffect, Cutscene, Light, Dungeon, Upgrade, TextFile };

        public enum ComponentTypeEnum { TextBoxName, TextBoxID, TextBoxDescription, DataGrid, DataGridTags, TabIndex, ColumnId, ColumnName, ColumnTags, ComboBoxType };

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

        public static readonly string DEFAULT_WORLD_OBJECT_TAGS = "Image:0-0";
        public static readonly string DEFAULT_ITEM_TAGS = "Image:,MerchType:None,Value:";
        public static readonly string DEFAULT_NPC_TAGS = "Key:,PortRow:1,Size:16-34,Idle:0-0-1-0-T,Walk:0-0-1-0-T";
        public static readonly string DEFAULT_SHOP_TAGS = "ItemID:,Shopkeeper:";
        public static readonly string DEFAULT_LIGHT_TAGS = "Texture:,Idle:1-1,Dimensions:";
        public static readonly string DEFAULT_UPGRADE_TAGS = "Icon:,Cost:100,ItemID:";
        public static readonly string DEFAULT_ACTION_TAGS = "Icon:,DamageAttribute:,DamageType:,Target:,Range:,AreaType:,Harm:100,Action:,Animation:,AnimOffset:";
        public static readonly string DEFAULT_MONSTER_TAGS = "Texture:,Condition:,Lvl,Ability:,Loot:,Trait:,Walk:0-0-3-0.15-T,Action1:0-0-3-0.15-T,Cast:0-0-3-0.15-T,Hurt:0-0-3-0.15-T,Critical:0-0-3-0.15-T,KO:0-0-3-0.15-T";

        #region XML Files
        public static readonly string ACTIONS_XML_FILE = PATH_TO_DATA + @"\Combat_Actions.xml";
        public static readonly string JOBS_XML_FILE = PATH_TO_DATA + @"\Classes.xml";
        public static readonly string NPC_XML_FILE = PATH_TO_DATA + @"\NPCData.xml";
        public static readonly string CONFIG_XML_FILE = PATH_TO_DATA + @"\Config.xml";
        public static readonly string CUTSCENE_XML_FILE = PATH_TO_DATA + @"\CutScenes.xml";
        public static readonly string DUNGEON_XML_FILE = PATH_TO_DATA + @"\DungeonData.xml";
        public static readonly string ITEM_DATA_XML_FILE = PATH_TO_DATA + @"\ItemData.xml";
        public static readonly string LIGHTS_XML_FILE = PATH_TO_DATA + @"\LightData.xml";
        public static readonly string UPGRADES_XML_FILE = PATH_TO_DATA + @"\Upgrades.xml";
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
        public static readonly string TAGS_FOR_JOBS = "Class";
        public static readonly string TAGS_FOR_SHOPDATA = "ShopData,TargetShopID";
        public static readonly string TAGS_FOR_NPCS = "NPC_ID,Shopkeeper,MobID,Actors,Move,Face,Speak,Speed,Activate,Deactivate,Join,Combat,StartNPC,GoalNPC";
        public static readonly string TAGS_FOR_STATUS_EFFECTS = "StatusEffectID";
        public static readonly string TAGS_FOR_LIGHTS = "LightID";
        public static readonly string TAGS_FOR_UPGRADES = "UpgradeID";
        public static readonly string TAGS_FOR_MONSTERS = "MonsterID";
        public static readonly string TAGS_FOR_DUNGEONS = "DungeonID";
        public static readonly string TAGS_FOR_TASKS = "TaskID";

        public static readonly string ITEM_REF_TAGS = "ReqItems,Place";
        public static readonly string TASK_REF_TAGS = "GoalItem,ItemRewardID,BuildingID,UnlockBuildingID,TargetObjectID,RequiredObjectID,StartNPC,GoalNPC";
        public static readonly string NPC_REF_TAGS = "BuildingID,Collection,Class,MobID,MonsterID,ShopData,HouseID,RequiredBuildingID,RequiredObjectID,RequestIDs";
        public static readonly string WORLD_OBJECT_REF_TAGS = "ReqItems,LightID,Makes,Processes,ItemID,SubObjects,SeedID,HoneyID,LightID";
        public static readonly string JOBS_REF_TAGS = "GearID,Ability,Spell";
        public static readonly string SHOPDATA_REF_TAGS = "ItemID,BuildingID,ObjectID,NPC_ID";
        public static readonly string CONFIG_REF_TAG = "ItemID,ObjectID";
        public static readonly string MONSTERS_REF_TAGS = "Loot,Ability,Spell";
        public static readonly string ACTIONS_REF_TAGS = "StatusEffectID,NPC_ID";
        public static readonly string DUNGEON_REF_TAGS = "ObjectID,MonsterID,EntranceID";
        public static readonly string TEXTFILE_REF_TAGS = "ItemID,UnlockObjectID,UnlockItemID,TargetShopID,TaskID";
        public static readonly string CUTSCENE_REF_TAGS = "Actors,Move,Face,Speak,Speed,Activate,Deactivate,Join,Combat";

        public static readonly string MAP_REF_TAGS = "ItemKeyID,ItemID,Resources,ObjectID,NPC_ID,MobID";
        #endregion
    }
}
