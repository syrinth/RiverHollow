namespace RiverHollow.Utilities
{
    public static class Enums
    {
        public enum DataType { Action, NPC, Job, Item, Light, Monster, StatusEffect, Task, WorldObject, Upgrade };

        public enum TextEntryVerbEnum { None, Yes, No, Talk, Gift, Party, ShipGoods, Buy, Sell, Propose, Date, Option_0, Option_1, Option_2, Option_3 };
        public enum TextEntrySelectionEnum { None, VillageTalk, MerchantTalk, YesNo, Shop, Party };
        public enum TextEntryTriggerEnum { None, UseItem, ConfirmPurchase, EndDay, Exit, Donate, PetFollow, PetUnfollow, GetBaby }

        public enum SpawnTypeEnum { Item, Object, Mob };
        public enum RarityEnum { C, U, R, M };
        public enum WeatherEnum { Sunny, Raining, Snowing };

        public enum TriggerObjectEnum { ColorBlocker, ColorSwitch, FloorSwitch, Trigger, KeyDoor, MobDoor, TriggerDoor };
        public enum HazardTypeEnum { Passive, Timed, Triggered };

        public enum GameScreenEnum { Info, Combat, World };
        public enum DisplayTypeEnum { Inventory, Ship };

        public enum SeasonEnum { None, Spring, Summer, Fall, Winter };
        public enum ZoneEnum { Forest, Mountain, Field, Swamp, Town };
        public enum DirectionEnum { None, Down, Right, Up, Left };
        public enum CardinalDirectionsEnum { North, NorthEast, East, SouthEast, South, SouthWest, West, NorthWest };
        public enum VerbEnum { Idle, Walk, GrabIdle, Pull, Push, Ground, Air, UseTool, MakeItem, Alert, Action1, Action2 };
        public enum AnimationEnum { None, PlayAnimation, Rain, Snow, ObjectIdle, Action_Finished, Angry, Sad, Neutral, Happy, Idle, Action1, Action2, Action3, Action4, Critical, Hurt, KO, Spawn, Victory };

        public enum WorldObjAnimEnum { Idle, Working, Shake, Gathered };
        public enum PlantEnum { Seeds, Seedling, Adult, Ripe };

        public enum ItemGroupEnum { None, Clothes, Flower, Gem, Herb, Magic, Medicine, Metal, Ore };
        public enum ItemEnum { Resource, Equipment, Tool, Food, Consumable, Clothing, MonsterFood, NPCToken, Blueprint, Seed, Special };
        public enum FoodTypeEnum { Dessert, Expensive, Forage, Healthy, Plain };
        public enum ToolEnum { None, Backpack, CapeOfBlinking, Pick, Axe, Shovel, WateringCan, Harp, Lantern, Return, Scythe, StaffOfIce };
        public enum GearTypeEnum { None, Accessory, Chest, Head, Weapon };
        public enum WeaponEnum { None, Spear, Shield, Rapier, Bow, Wand, Knife, Orb, Staff };
        public enum ArmorTypeEnum { None, Cloth, Light, Heavy };
        public enum ClothingEnum { None, Chest, Legs, Hat };
        public enum NPCTokenTypeEnum { Mount, Pet };

        public enum ActorStateEnum { Climb, Grab, Swim, Walk };
        public enum ActorFaceEnum { Default, Happy, Angry, Sad };
        public enum WorldActorTypeEnum { Actor, Animal, Child, Critter, Merchant, Mob, Mount, Pet, ShippingGremlin, Spirit, TalkingActor, Traveler, Villager };
        public enum TravelerGroupEnum { None, Adventurer, Dwarf, Goblin, Human, Noble};
        public enum CombatActorTypeEnum { Monster, PartyMember };
        public enum AttributeEnum { Damage, Vitality, Agility, Magic, Strength, Defence, Resistance, Evasion, Speed };
        public enum AttributeBonusEnum { Minor, Moderate, Major };
        public enum PotencyBonusEnum { None, Conditions, Summon };
        public enum PlayerColorEnum { None, Eyes, Hair, Skin };
        public enum ActionEnum { Action, Item, Move };
        public enum SkillTagsEnum { Bonus, Harm, Heal, NPC_ID, Displace, Move, Remove, StatusEffectID };
        public enum TargetEnum { Enemy, Ally, Self };
        public enum DamageTypeEnum { Physical, Magical };
        public enum RangeEnum { Melee, Ranged, Row, Column, Adjacent };
        public enum AreaTypeEnum { Self, Single, Row, Column, Square, All };
        public enum StatusTypeEnum { Buff, Debuff, DoT, HoT };
        public enum ElementEnum { None, Fire, Ice, Lightning };
        public enum AttackTypeEnum { Physical, Magical };
        public enum ElementAlignment { Neutral, Vulnerable, Resists };
        public enum AdventurerTypeEnum { Magic, Martial };
        public enum ObjectPlacementEnum { Ground, Floor, Wall };
        public enum ObjectTypeEnum { WorldObject, Beehive, Buildable, Building, Hazard, Container, Decor, Destructible, DungeonObject, Floor, Gatherable, Garden, Machine, Mailbox, Plant, Structure, Wall, Wallpaper, WarpPoint };
        public enum SpawnConditionEnum { Spring, Summer, Winter, Fall, Precipitation, Night, Forest, Mountain, Swamp, Plains };
        public enum ExpectingChildEnum { None, Adoption, Pregnant };
        public enum RelationShipStatusEnum { None, Friends, Dating, Engaged, Married };
        public enum SpawnStateEnum { OffMap, WaitAtInn, VisitInn, HasHome, NonTownMap, SendingToInn };
        public enum MoodEnum { Miserable, Sad, Neutral, Pleased, Happy, Ecstatic };
        public enum NPCStateEnum { Alert, Idle, Leashing, TrackPlayer, Wander };
        public enum ChildStageEnum { Newborn, Infant, Toddler };
       
        public enum TaskStateEnum { Waiting, Assigned, Talking, TaskLog, Completed };
        public enum TaskTriggerEnum { GameStart, FriendLevel, Building, Task };
        public enum TaskTypeEnum { Build, Fetch, GroupSlay, Population, Slay, Talk };

        public enum VillagerRequestEnum { Close, Far, TownWide };

        public enum GameIconEnum { None, AreaAll, AreaColumnAlly, AreaColumnEnemy, AreaSelf, AreaSingle, AreaRow, AreaSquare, Agility, BuffArrow, Coin, Damage, DebuffArrow, Defence, ElementFire, ElementIce, ElementLightning, Evasion, Experience, Heal, Key, Magic, MaxHealth, MagicDamage, Melee, MoveDown, MoveLeft, MoveRight, MoveUp, PhysicalDamage, Ranged, Resistance, Speed, Strength, Timer };

        public enum ColorStateEnum { None, Blue, Red };
    }
}
