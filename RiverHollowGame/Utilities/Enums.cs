namespace RiverHollow.Utilities
{
    public static class Enums
    {
        public enum ButtonEnum { Left, Right, Middle };
        public enum DataType { Actor, Job, Item, Light, Monster, StatusEffect, Task, WorldObject, Upgrade };
        public enum BuildTypeEnum { Structure, WorldObject };
        public enum TextEntryVerbEnum { None, Yes, No, Talk, Gift, Party, ShipGoods, Buy, Sell, Propose, Date, EndDay, GoToNight, Option_0, Option_1, Option_2, Option_3 };
        public enum TextEntrySelectionEnum { None, VillageTalk, MerchantTalk, YesNo, Shop, Bed };
        public enum TextEntryTriggerEnum { None, UseItem, ConfirmPurchase, Donate, PetFollow, PetUnfollow, GetBaby }

        public enum HUDMenuEnum { Main, Build };

        public enum SpawnTypeEnum { Item, Object, Mob };
        public enum RarityEnum { C, U, R, M };
        public enum WeatherEnum { Sunny, Raining, Snowing };

        public enum TriggerObjectEnum { ColorBlocker, ColorSwitch, FloorSwitch, Trigger, KeyDoor, MobDoor, TriggerDoor };
        public enum HazardTypeEnum { Passive, Timed, Triggered };

        public enum GameScreenEnum { Info, World };
        public enum DisplayTypeEnum { Inventory, Ship };
        public enum ItemBoxDraw { Always, Never, OnlyStacks };
        public enum CodexPageEnum { Villagers, Merchants, Travelers, Mobs, Items };

        public enum SeasonEnum { None, Spring, Summer, Fall, Winter };
        public enum DayEnum { Monday, Tuesday, Wednesday, Thursday, Friday, Saturday, Sunday };
        public enum ZoneEnum { Forest, Mountain, Field, Swamp, Town };
        public enum DirectionEnum { None, Down, Right, Up, Left };
        public enum CardinalDirectionsEnum { North, NorthEast, East, SouthEast, South, SouthWest, West, NorthWest };
        public enum VerbEnum { Idle, Walk, GrabIdle, Pull, Push, Ground, Air, UseTool, FinishTool, MakeItem, Action1, Action2 };
        public enum AnimationEnum { None, PlayAnimation, Rain, Snow, ObjectIdle, Action1, Action2, Action_Finished, Alert, KO, Spawn, Pose };
        public enum ItemPickupState { Auto, Manual, None };

        public enum WorldObjAnimEnum { Idle, Working, Shake, Gathered };
        public enum PlantEnum { Seeds, Seedling, Adult, Ripe };
        public enum FishingStateEnum { None, Cast, WaitForFish, AttemptToCatch, Finish, Get };

        public enum ItemGroupEnum { Ceramic, Construction, Clothing, Fish, Flower, Food, Gem, Herb, Ingredient, Magic, Meal, Medicine, Metal, Ore, None };
        public enum ItemEnum { Resource, Tool, Food, Consumable, Clothing, Buildable, MonsterFood, NPCToken, Blueprint, Seed, Special };
        public enum FoodTypeEnum { Dessert, Fancy, Healthy, Plain, Seafood };
        public enum ToolEnum { None, Axe, Backpack, CapeOfBlinking, FishingRod, Harp, Lantern, Pick, Return, Scythe, Shovel, StaffOfIce, Sword, WateringCan };
        public enum GearTypeEnum { None, Accessory, Chest, Head, Weapon };
        public enum ClothingEnum { None, Chest, Legs, Hat };
        public enum NPCTokenTypeEnum { Mount, Pet };
        public enum CraftFilterEnum { All };

        public enum ActorStateEnum { Climb, Grab, Swim, Walk };
        public enum WeightEnum { Light, Medium, Heavy, Immovable};
        public enum ActorFaceEnum { Default, Happy, Angry, Sad };
        public enum ActorTypeEnum { Animal, Child, Critter, Merchant, Mob, Mount, Pet, Projectile, ShippingGremlin, Spirit, TalkingActor, Traveler, Villager };
        public enum MobTypeEnum { Basic, Shooter, Summoner };
        public enum TravelerGroupEnum { None, Adventurer, Dwarf, Goblin, Human, Noble};
        public enum PlayerColorEnum { None, yes, Hair, Skin };
        public enum StatusTypeEnum { Buff, Debuff, DoT, HoT };

        public enum ObjectPlacementEnum { Ground, Floor, Wall };
        public enum ObjectTypeEnum { WorldObject, Beehive, Buildable, Building, Hazard, Container, Decor, Destructible, DungeonObject, Floor, Gatherable, Garden, Machine, Mailbox, Plant, Structure, Wall, Wallpaper, WarpPoint };
        public enum MobSpawnStateEnum { None, Day, Night };
        public enum ExpectingChildEnum { None, Adoption, Pregnant };
        public enum RelationShipStatusEnum { None, Friends, Dating, Engaged, Married };
        public enum SpawnStateEnum { OffMap, WaitAtInn, VisitInn, HasHome, NonTownMap, SendingToInn };
        public enum MoodEnum { Miserable, Sad, Neutral, Pleased, Happy, Ecstatic };
        public enum TravelerMoodEnum { Angry, Sad, Neutral, Happy };
        public enum NPCStateEnum { Alert, Idle, Leashing, TrackPlayer, Wander };
        public enum ChildStageEnum { Newborn, Infant, Toddler };
       
        public enum TaskStateEnum { Waiting, Assigned, Talking, TaskLog, Completed };
        public enum TaskTriggerEnum { GameStart, FriendLevel, Building, Task };
        public enum TaskTypeEnum { Build, Fetch, GroupSlay, Population, Slay, Talk };

        public enum VillagerRequestEnum { Close, Far, TownWide };

        public enum GameIconEnum { None, Book, Coin, Hammer, Traveler };

        public enum PlayerResourceEnum { Energy, Health, Magic };

        public enum ColorStateEnum { None, Blue, Red };

        public enum MapTypeEnum { Invalid, Town, Cave };
        public enum SoundEffectEnum { Invalid, Axe, Button, Cancel, Cauldron, Door, GrabBuilding, GrindStone, Kitchen, Pick, Rainfall, Scythe, Success_Fish, Switch, Text, Thump};
        public enum SongEnum { Invalid, Cave, Swamp, Town };
    }
}
