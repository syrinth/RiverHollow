﻿using Microsoft.Xna.Framework;

namespace RiverHollow.Utilities
{
    static class Constants
    {
        public const bool AUTO_TOOL = true;
        public const bool DRAW_COLLISION = false;
        public const bool DRAW_HITBOX = false;
#if DEBUG
        public const bool DRAW_ADJACENCY = false;
#else
        public const bool DRAW_ADJACENCY = false;
#endif
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

        public const int MAX_ITEM_DESC_SIZE = 550;
        public const int NORMAL_SCALE = 4;
        public const int TILE_SIZE = 16;
        public const int BUILDING_SHADOW_HEIGHT = 2 * TILE_SIZE;
        public const int MAX_LAYER_DEPTH = 999999;
        public const int MAX_STACK_SIZE = 999;
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

        internal static Rectangle ITEM_SHADOW = new Rectangle(0, 0, 16, 16);

        public const string STR_ALERT_INVENTORY = "Alert_Inventory";
        public const string STR_ALERT_MISSING = "Alert_Missing";
        public const string STR_ALERT_BLUEPRINT = "Alert_BP";
        public const string STR_ALERT_BLUEPRINTS = "Alert_BPs";
        public const string STR_ALERT_UPGRADE = "Alert_Upgrade";
        public const string STR_ALERT_TASK = "Alert_Task";
        public const string STR_ALERT_FINISHED = "Alert_Finished";
        public const string STR_ALERT_ENERGY = "Alert_Energy";
        public const string STR_ALERT_CODEX = "Codex_Unlocked";
    }
}
