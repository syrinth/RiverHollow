using Microsoft.Xna.Framework;

namespace RiverHollow.Utilities
{
    static class Constants
    {
        public static Point BASIC_TILE = new Point(TILE_SIZE, TILE_SIZE);
        public const bool AUTO_TOOL = true;
        public const bool DRAW_COLLISION = false;
        public const bool DRAW_HITBOX = false;
#if DEBUG
        public const bool DRAW_ADJACENCY = false;
#else
        public const bool DRAW_ADJACENCY = false;
#endif
        public const bool WRITE_TO_ERROR_LOG = true;

        public const int PLAYER_ADJACENCY_SIZE = TILE_SIZE / 2;
        public const int GRAB_REACH = 1;
        public const float MOUSE_PRESS_INTERVAL = 0.4f;

        public const string TOWN_MAP_NAME = "mapTown";
        public const string INN_MAP_NAME = "mapInn";
        public const string PLAYER_HOME_NAME = "mapHouse_Player";
        public const float PLAYER_STARTING_HP = 10;
        public const float PLAYER_STARTING_ENERGY = 100;
        public const float PLAYER_STARTING_MAGIC = 20;
        public const int PLAYER_GEAR_ROWS = 3;
        public const int PLAYER_GEAR_COLUMNS = 2;
        public const int ENERGY_NAP_RECOVERY = 20;
        public const int BUILDING_TRAVELER_BOOST = 70;
        public const int PLAYER_EXTRAS_COLUMNS = 5;
        public const int PLAYER_HAT_OFFSET = -4;
        public const int PLAYER_SHIRT_OFFSET = 9;
        public const int PLAYER_PANTS_OFFSET = 16;
        public const int MERCHANT_BASE_CAPACITY = 5;

        public const float PLAYER_GRACE_PERIOD = 0.75f;
        public const float PLAYER_INVULN_PERIOD = 1;
        public const float MOB_INVULN_PERIOD = 0.3f;
        public const float FLICKER_PERIOD = 0.08f;
        public const float MOB_STUN_TIME = 5;
        public const float WANDER_COUNTDOWN = 2.5f;
        public const float PUSH_COOLDOWN = 0.1f;

        public const float ITEM_POP_VELOCITY = -3f;
        public const float ITEM_POP_BOUNCE = -1.5f;
        public const float ITEM_POP_DECAY = 0.2f;
        public const int ITEM_POP_VARIANCE = 10;

        public const int FLIER_MAX_DIST = 6 * TILE_SIZE;
        public const int FLIER_MIN_DIST = 3 * TILE_SIZE;
        public const float FLIER_RATE_OF_CHANGE = 0.4f;

        public const float GUI_TEXT_DELAY = 0.06f;
        public const float GUI_TEXT_MARKER_FLASH_RATE = 0.5f;
        public const float GUI_WINDOW_OPEN_SPEED = 0.004f;

        public const int MAX_ITEM_DESC_SIZE = 550;
        public const int NORMAL_SCALE = 4;
        public const int TILE_SIZE = 16;
        public const int BUILDING_SHADOW_HEIGHT = 2 * TILE_SIZE;
        public const int MAX_LAYER_DEPTH = 999999;
        public const int MAX_STACK_SIZE = 999;
        public const int MAX_STACK_COMPARE = 99;
        public const float ITEM_BOUNCE_SPEED = 0.2f;
        public const float BUILDING_SCORE_MULTIPLIER = 0.1f;

        public const int SOUND_MAX_SAME_SOUND = 3;

        public const int CALENDAR_DAYS_IN_MONTH = 28;
        public const int CALENDAR_MINUTES_PER_SECOND = 1;
        public const int CALENDAR_NEW_DAY_HOUR = 8;
        public const int CALENDAR_NEW_DAY_MIN = 0;
        public const int NIGHTFALL_STANDARD = 18;
        public const int NIGHTFALL_LATE = 21;
        public const int NIGHTFALL_EARLY = 16;
        public const int MINIMUM_DAYS_OF_PRECIPITATION = 6;

        public const string TRIGGER_MOB_OPEN = "MOB";
        public const string TRIGGER_ITEM_OPEN = "VALID_ITEM";
        public const string TRIGGER_KEY_OPEN = "KEY_USED";

        public const float NORMAL_SPEED = 1.5f;
        public const float PUSH_SPEED = 0.5f;
        public const float NUDGE_RATE = 0.5f;
        public const float IMPEDED_SPEED = 0.6f;
        public const float NPC_WALK_SPEED = 0.6f;

        public const int ACTION_COST = 2;
        public const int FISH_MISSES = 3;

        public const int HUMAN_HEIGHT = (TILE_SIZE * 2) + 2;
        public const float SPRITE_LINKED_MOD = 0.0001f;

        public const int MAX_RECIPE_DISPLAY = 5;

        public const int MERCHANT_REQUEST_NUM = 3;
        public const float MERCHANT_REQUEST_MOD = 1.5f;
        public const float MERCHANT_NEED_MOD = 1.25f;
        public const float MERCHANT_WANT_MOD = 1f;

        public const float HUNGER_MOD = -0.5f;

        public const int BASE_TRAVELER_RATE = 30;
        public const int EXTRA_TRAVELER_THRESHOLD = 10;
        public const int GROUP_DIVISOR = 3;
        public const int MEMBER_DIVISOR = 2;

        public const int FISH_PUDDLE = 68;

        public const int ACTION_DELAY = 10;
        public const string MAPOBJ_CRAFT = "Craft";
        public const string MAPOBJ_HOME = "NPC_";
        public const string MAPOBJ_SHOP = "Shop";

        public const string VILLAGER_SHOP_DEFAULT = "09:00";
        public const string VILLAGER_INN_DEFAULT = "12:00";
        public const string VILLAGER_CRAFT_DEFAULT = "14:00";
        public const string VILLAGER_HOME_DEFAULT = "22:00";
        public const string VILLAGER_MARKET_DEFAULT = "Skip";
        public const string VILLAGER_VISIT_DEFAULT = "Skip";
        public const string VILLAGER_PETCAFE_DEFAULT = "Skip";

        public const string TRAVELER_SHOP_DEFAULT = "09:00";
        public const string TRAVELER_INN_DEFAULT = "12:00";
        public const string TRAVELER_MARKET_DEFAULT = "Skip";

        public const int TALK_FRIENDSHIP = 10;
        public const int GIFT_COLLECTION = 50;
        public const int GIFT_HAPPY = 20;
        public const int GIFT_PLEASED = 10;
        public const int GIFT_NEUTRAL = 5;
        public const int GIFT_SAD = -2;
        public const int GIFT_MISERABLE = -10;

        public const int FOLLOW_ALERT_THRESHOLD = TILE_SIZE * 8;
        public const int FOLLOW_ARRIVED = TILE_SIZE * 3;

        public const int TASK_ICON_OFFSET = 15;

        public const int BUILDABLE_ID_OFFSET = 8000;

        public static Rectangle ITEM_SHADOW = new Rectangle(0, 0, 16, 16);

        public static Rectangle RECTANGLE_EMOJI_HAPPY = new Rectangle(0, 48, 16, 16);
        public static Rectangle RECTANGLE_EMOJI_SING = new Rectangle(16, 48, 16, 16);
        public static Rectangle RECTANGLE_EMOJI_TALK = new Rectangle(32, 48, 16, 16);
        public static Rectangle RECTANGLE_EMOJI_DOTS = new Rectangle(48, 48, 16, 16);
        public static Rectangle RECTANGLE_EMOJI_HEART = new Rectangle(64, 48, 16, 16);
        public static Rectangle RECTANGLE_EMOJI_SLEEPY = new Rectangle(0, 64, 16, 16);

        public const int WALK_TO_FRIEND_PERCENT = 60;

        public const int EMOJI_CHAT_DEFAULT_RATE = 25;
        public const int EMOJI_SING_DEFAULT_RATE = 15;
        public const int EMOJI_SLEEPY_DEFAULT_RATE = 10;
        public const int EMOJI_WORK_FINISHED_DEFAULT_RATE = 25;

        public const int TRAIT_CHATTY_BONUS = 15;
        public const int TRAIT_MUSICAL_BONUS = 30;
        public const int TRAIT_ANXIOUS_CHANCE = 20;
        public const int TRAIT_RECLUSE_CHANCE = 10;

        public const int MAIL_PERCENT = 5;
        public const int MAILBOX_EDGE = 2;

        public const string STR_ALERT_INVENTORY = "Alert_Inventory";
        public const string STR_ALERT_MISSING = "Alert_Missing";
        public const string STR_ALERT_BLUEPRINT = "Alert_BP";
        public const string STR_ALERT_BLUEPRINTS = "Alert_BPs";
        public const string STR_ALERT_UPGRADE = "Alert_Upgrade";
        public const string STR_ALERT_TASK = "Alert_Task";
        public const string STR_ALERT_FINISHED = "Alert_Finished";
        public const string STR_ALERT_ENERGY = "Alert_Energy";
        public const string STR_ALERT_CODEX = "Codex_Unlocked";
        public const string STR_ALERT_PET_CAFE = "Alert_Needs_Pet_Cafe";

        public const string INFO_FILE_NAME = "SaveInfo";
        public const string RIVER_HOLLOW_LOG = "Logs";
        public const string RIVER_HOLLOW_SAVES = "Save Games";

        public const string MAILBOX_ITEMS = "Sent Items";
    }
}
