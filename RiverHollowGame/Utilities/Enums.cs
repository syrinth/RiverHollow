﻿using System;
using System.Linq;

namespace RiverHollow.Utilities
{
    public static class Enums
    {
        public static T[] GetEnumArray<T>()
        {
            return Enum.GetValues(typeof(T)).Cast<T>().ToArray();
        }

        public enum LogEnum { Info, Warning, Error };
        public enum ButtonEnum { Left, Right, Middle };
        public enum DataType { Actor, Adventure, Cosmetic, Item, Light, StatusEffect, Task, WorldObject, Upgrade };
        public enum BuildTypeEnum { Structure, WorldObject };
        public enum TextEntryVerbEnum { None, Yes, No, Talk, Gift, Party, Propose, Date, EndDay, Option_0, Option_1, Option_2, Option_3 };
        public enum TextEntrySelectionEnum { None, VillageTalk, YesNo, Bed };
        public enum TextEntryTriggerEnum { None, ConfirmPurchase, Donate, PetFollow, PetUnfollow, GetBaby }

        public enum SpawnTypeEnum { Item, Object, Mob };
        public enum RarityEnum { C, U, R, M };
        public enum WeatherEnum { Sunny, Raining, Snowing };

        public enum TriggerObjectEnum { ColorBlocker, ColorSwitch, FloorSwitch, Trigger, KeyDoor, MobDoor, TriggerDoor };
        public enum HazardTypeEnum { Passive, Timed, Triggered };

        public enum GameScreenEnum { Info, World };
        public enum DisplayTypeEnum { None, Inventory, PlayerInventory, ShopTable };
        public enum ItemBoxDraw { Always, Never, MoreThanOne };
        public enum CodexPageEnum { Villagers, Merchants, Travelers, Mobs, Items };
        public enum BuildPageEnum { Structures, Flooring, Walls, Furniture, Lighting };

        public enum SeasonEnum { None, Spring, Summer, Fall, Winter };
        public enum DayEnum { Monday, Tuesday, Wednesday, Thursday, Friday, Saturday, Sunday };
        public enum ZoneEnum { Forest, Mountain, Field, Swamp, Town };
        public enum DirectionEnum { None, Down, Right, Up, Left };
        public enum CardinalDirectionsEnum { North, NorthEast, East, SouthEast, South, SouthWest, West, NorthWest };
        public enum VerbEnum { Idle, Walk, GrabIdle, Pull, Push, Ground, Air, UseTool, FinishTool, MakeItem, Action1, Action2 };
        public enum AnimationEnum { None, PlayAnimation, Rain, Snow, Fish_Nibble, ObjectIdle, Action1, Action2, Action_Finished, Alert, KO, Spawn, Pose };
        public enum ItemPickupState { Auto, Manual, None };
        public enum ItemMovementState { None, Bounce, Magnet };

        public enum WorldObjAnimEnum { Idle, Working, Shake, Gathered };
        public enum PlantEnum { Seeds, Seedling, Adult, Ripe };
        public enum FishingStateEnum { None, Cast, Waiting, Reeling, Finish, Get };

        public enum AffinityEnum { None, Arcane, Battle, Mercantile, Natural };
        public enum ClassTypeEnum { None, Healer, Mage, Fighter, Rogue };
        public enum ResourceTypeEnum { None, Construction, Fabric, Fish, Flower, Food, Gem, Herb, Ingredient, Meal, Metal, MonsterPart, Ore};
        public enum ItemTypeEnum { None, Resource, Blueprint, Buildable, Consumable, Cosmetic, Food, Merchandise, MonsterFood, NPCToken, Relic, Seed, Special, Tool };
        public enum FoodTypeEnum { Dessert, Fancy, Healthy, Plain, Seafood };
        public enum CosmeticSlotEnum { Eyes, Hair, Head, Body, Legs, Feet, };
        public enum MerchandiseTypeEnum { None, Accessory, Equipment, Recovery, Utility, Weapon };
        public enum NPCTokenTypeEnum { Mount, Pet };
        public enum ToolEnum { None, Axe, Backpack, CapeOfBlinking, FishingRod, Harp, Lantern, Pick, Return, Scythe, Hoe, StaffOfIce, Sword, WateringCan };
        public enum UpgradeTypeEnum { Building, Global };
        public enum UpgradeStatusEnum { Locked, Unlocked, InProgress, Completed };

        public enum ActorEmojiEnum { Dots, Happy, Heart, Sing, Sleepy, Talk };
        public enum ActorStateEnum { Climb, Grab, Swim, Walk };
        public enum ActorCollisionState { Block, Slow, PassThrough };
        public enum ActorTraitsEnum { Early, Late, Prompt, Chatty, Anxious, Recluse, Musical };
        public enum WeightEnum { Light, Medium, Heavy, Immovable};
        public enum ActorFaceEnum { Default, Happy, Angry, Sad };
        public enum ActorTypeEnum { Animal, Child, Critter, Effect, Fish, Merchant, Mob, Mount, Pet, Projectile, Spirit, TalkingActor, Traveler, Villager };
        public enum MobTypeEnum { Basic, Mage, Shooter, Summoner };
        public enum TravelerGroupEnum { None, Adventurer, Dwarf, Goblin, Human, Magi, Noble};
        public enum StatusTypeEnum { Buff, Debuff, DoT, HoT };

        public enum ObjectPlacementEnum { Ground, Floor, Impassable, Wall };
        public enum ObjectTypeEnum { WorldObject, Buildable, Hazard, Destructible, DungeonObject,Gatherable, Machine, Plant, WarpPoint };
        public enum BuildableEnum { Basic, Beehive, Building, Container, Decor, Field, Floor, Mailbox, Structure, Wall, Wallpaper };
        public enum ExpectingChildEnum { None, Adoption, Pregnant };
        public enum RelationShipStatusEnum { None, Friends, Dating, Engaged, Married };
        public enum SpawnStateEnum { OffMap, WaitAtInn, HasHome, NonTownMap, SendingToInn };
        public enum MoodEnum { Miserable, Sad, Neutral, Pleased, Happy, Ecstatic };
        public enum TravelerMoodEnum { Angry, Sad, Neutral, Happy };
        public enum NPCStateEnum { Alert, Idle, Leashing, MaintainDistance, TrackPlayer, Wander };
        public enum ChildStageEnum { Newborn, Infant, Toddler };
        public enum NPCActionState { Craft, Home, Inn, Market, PetCafe, Shop, Visit };
       
        public enum TaskStateEnum { Waiting, Assigned, Talking, TaskLog, Completed };
        public enum TaskTriggerEnum { None, Date, GameStart, FriendLevel, Building, Task };
        public enum TaskTypeEnum { Build, Fetch, Talk, TownState, Slay, Craft };

        public enum VillagerRequestEnum { Close, Far, TownWide };
        public enum LetterTemplateEnum {Sent, Unsent, Repeatable };
        public enum LetterItemStateEnum { None, Waiting, Received };

        public enum GameIconEnum { None, Book, Coin, Hammer, Traveler, Bag, Health, Energy, Time, Level };

        public enum PlayerResourceEnum { Energy, Health, Magic };

        public enum ColorStateEnum { None, Blue, Red };

        public enum MapTypeEnum { Invalid, Town, Cave };
        public enum SoundEffectEnum { Invalid, Item, Axe, Button, Cancel, Cauldron, Door, GrabBuilding, GrindStone, Kitchen, Pick, Rainfall, Scythe, Success_Fish, Switch, Text, Thump, Trigger_1 };
        public enum SongEnum { Invalid, Cave, Swamp, Town };
    }
}
