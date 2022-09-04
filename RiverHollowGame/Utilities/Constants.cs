namespace RiverHollow.Utilities
{
    static class Constants
    {
        public const string TOWN_MAP_NAME = "mapTown";

        public const double GAME_PLAYER_INVULN_TIME = 0.5;
        public const double MOB_STUN_TIME = 5;
        public const double WANDER_COUNTDOWN = 2.5;
        public const double PUSH_COOLDOWN = 0.1;

        public const double COMBAT_DAMAGE_FLOAT_TIMER = 0.55;
        public const double COMBAT_STATUS_REFRESH_RATE = 3;

        public const double GUI_TEXT_DELAY = 0.06;
        public const double GUI_TEXT_MARKER_FLASH_RATE = 0.5;
        public const double GUI_WINDOW_OPEN_SPEED = 0.004;

        public const int MINIMUM_DAYS_OF_PRECIPITATION = 6;
        public const int NORMAL_SCALE = 4;
        public const int MAX_BUILDING_LEVEL = 3;
        public const int TILE_SIZE = 16;
        public const int MAX_LAYER_DEPTH = 999999;
        public const float TOOL_ANIM_SPEED = 0.08f;
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

        public const int HUMAN_HEIGHT = (TILE_SIZE * 2) + 2;
        public const float EYE_DEPTH = 0.001f;
        public const float HAIR_DEPTH = 0.003f;
    }
}
