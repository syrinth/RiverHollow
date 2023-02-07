namespace RiverHollow.Utilities
{
    static class Constants
    {
        public const bool DRAW_COLLISION = true;

        public const string TOWN_MAP_NAME = "mapTown";
        public const string PLAYER_HOME_NAME = "mapHouse_Player";
        public const float PLAYER_STARTING_STAMINA = 100f;
        public const float PLAYER_STARTING_HP = 10f;

        public const double INVULN_PERIOD = 1;
        public const double FLICKER_PERIOD = 0.2;
        public const double MOB_STUN_TIME = 5;
        public const double WANDER_COUNTDOWN = 2.5;
        public const double PUSH_COOLDOWN = 0.1;

        public const double GUI_TEXT_DELAY = 0.06;
        public const double GUI_TEXT_MARKER_FLASH_RATE = 0.5;
        public const double GUI_WINDOW_OPEN_SPEED = 0.004;

        public const int MINIMUM_DAYS_OF_PRECIPITATION = 6;
        public const int NORMAL_SCALE = 4;
        public const int TILE_SIZE = 16;
        public const int MAX_LAYER_DEPTH = 999999;
        public const double ITEM_BOUNCE_SPEED = 0.2;

        public const int CALENDAR_DAYS_IN_MONTH = 28;
        public const int CALENDAR_MINUTES_PER_SECOND = 1;
        public const int CALENDAR_NEW_DAY_HOUR = 6;
        public const int CALENDAR_NEW_DAY_MIN = 0;

        public const string TRIGGER_MOB_OPEN = "MOB";
        public const string TRIGGER_ITEM_OPEN = "VALID_ITEM";
        public const string TRIGGER_KEY_OPEN = "KEY_USED";

        public const float NORMAL_SPEED = 1.5f;
        public const float PUSH_SPEED = 0.5f;
        public const float NPC_WALK_SPEED = 0.6f;
        public const float NUDGE_RATE = 0.5f;
        public const float IMPEDED_SPEED = 0.6f;

        public const int ACTION_COST = 2;

        public const int HUMAN_HEIGHT = (TILE_SIZE * 2) + 2;
        public const float EYE_DEPTH = 0.001f;
        public const float HAIR_DEPTH = 0.003f;

        public const int KITCHEN_STOCK_SIZE = 3;
        public const int MERCHANT_REQUEST_NUM = 3;
        public const int MAX_RECIPE_DISPLAY = 5;

        public const int HUNGER_MOD = 0;

        public const int BASE_TRAVELER_RATE = 100;
        public const int EXTRA_TRAVELER_THRESHOLD = 10;
        public const int GROUP_DIVISOR = 3;
        public const int MEMBER_DIVISOR = 2;

        public const int FOLLOW_ALERT_THRESHOLD = TILE_SIZE * 8;
        public const int FOLLOW_ARRIVED = TILE_SIZE * 3;

        public const int TASK_ICON_OFFSET = 15;

        public const string STRING_NULL = "null";
    }
}
