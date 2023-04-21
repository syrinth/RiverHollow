namespace RiverHollow.Utilities
{
    static class Constants
    {
        public const bool DRAW_COLLISION = false;
        public const bool DRAW_HITBOX = true;

        public const string TOWN_MAP_NAME = "mapTown";
        public const string PLAYER_HOME_NAME = "mapHouse_Player";
        public const float PLAYER_STARTING_HP = 10;
        public const float PLAYER_STARTING_ENERGY = 80;
        public const int ENERGY_NAP_RECOVERY = 20;
        public const float PLAYER_STARTING_MAGIC = 20;

        public const float PLAYER_INVULN_PERIOD = 1;
        public const float MOB_INVULN_PERIOD = 0.3f;
        public const float FLICKER_PERIOD = 0.08f;
        public const float MOB_STUN_TIME = 5;
        public const float WANDER_COUNTDOWN = 2.5f;
        public const float PUSH_COOLDOWN = 0.1f;

        public const int FLIER_MAX_DIST = 6 * TILE_SIZE;
        public const int FLIER_MIN_DIST = 3 * TILE_SIZE;
        public const float FLIER_RATE_OF_CHANGE = 0.4f;

        public const float GUI_TEXT_DELAY = 0.06f;
        public const float GUI_TEXT_MARKER_FLASH_RATE = 0.5f;
        public const float GUI_WINDOW_OPEN_SPEED = 0.004f;

        public const int MINIMUM_DAYS_OF_PRECIPITATION = 6;
        public const int NORMAL_SCALE = 4;
        public const int TILE_SIZE = 16;
        public const int MAX_LAYER_DEPTH = 999999;
        public const float ITEM_BOUNCE_SPEED = 0.2f;

        public const int CALENDAR_DAYS_IN_MONTH = 28;
        public const int CALENDAR_MINUTES_PER_SECOND = 1;
        public const int CALENDAR_NEW_DAY_HOUR = 8;
        public const int CALENDAR_NEW_DAY_MIN = 0;
        public const int NIGHTFALL_STANDARD = 18;
        public const int NIGHTFALL_LATE = 21;
        public const int NIGHTFALL_EARLY = 16;

        public const string TRIGGER_MOB_OPEN = "MOB";
        public const string TRIGGER_ITEM_OPEN = "VALID_ITEM";
        public const string TRIGGER_KEY_OPEN = "KEY_USED";

        public const float NORMAL_SPEED = 1.5f;
        public const float PUSH_SPEED = 0.5f;
        public const float NUDGE_RATE = 0.5f;
        public const float IMPEDED_SPEED = 0.6f;
        public const float NPC_WALK_SPEED = 0.6f;

        public const int ACTION_COST = 2;

        public const int HUMAN_HEIGHT = (TILE_SIZE * 2) + 2;
        public const float EYE_DEPTH = 0.001f;
        public const float HAIR_DEPTH = 0.003f;

        public const int KITCHEN_STOCK_SIZE = 3;
        public const int MERCHANT_REQUEST_NUM = 3;
        public const int MAX_RECIPE_DISPLAY = 5;

        public const float HUNGER_MOD = -0.5f;

        public const int BASE_TRAVELER_RATE = 30;
        public const int EXTRA_TRAVELER_THRESHOLD = 10;
        public const int GROUP_DIVISOR = 3;
        public const int MEMBER_DIVISOR = 2;

        public const int FOLLOW_ALERT_THRESHOLD = TILE_SIZE * 8;
        public const int FOLLOW_ARRIVED = TILE_SIZE * 3;

        public const int TASK_ICON_OFFSET = 15;

        public const int BUILDABLE_ID_OFFSET = 8000;

        public const string STRING_NULL = "null";
    }
}
