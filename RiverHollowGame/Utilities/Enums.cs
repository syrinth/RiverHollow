namespace RiverHollow.Utilities
{
    public static class Enums
    {
        public enum TextEntryVerbEnum { None, Yes, No, Talk, Gift, Party, ShipGoods, Buy, ShowRequests, Propose, Date, Option_0, Option_1, Option_2, Option_3 };
        public enum TextEntrySelectionEnum { None, VillageTalk, MerchantTalk, YesNo, Shop, Party };
        public enum TextEntryTriggerEnum { None, UseItem, ConfirmGift, ConfirmPurchase, EndDay, Exit, Donate, PetFollow, PetUnfollow, GetBaby }

        public enum SpawnTypeEnum { Item, Object, Mob };
        public enum RarityEnum { C, U, R, M };
        public enum WeatherEnum { Sunny, Raining, Snowing };

        public enum GameScreenEnum { Info, Combat, World };
        public enum DisplayTypeEnum { Inventory, Gift, Ship };

        public enum SeasonEnum { None, Spring, Summer, Fall, Winter };
        public enum ZoneEnum { Forest, Mountain, Field, Swamp, Town };
        public enum DirectionEnum { None, Down, Right, Up, Left };
        public enum CardinalDirectionsEnum { North, NorthEast, East, SouthEast, South, SouthWest, West, NorthWest };
        public enum VerbEnum { Idle, Walk, Ground, Air, UseTool, MakeItem, Alert, Action1, Action2 };
        public enum AnimationEnum { None, PlayAnimation, Rain, Snow, ObjectIdle, Action_Finished, Idle, Action1, Action2, Action3, Action4, Critical, Hurt, KO, Spawn, Victory };

        public enum WorldObjAnimEnum { Idle, Working, Shake, Gathered };
        public enum PlantEnum { Seeds, Seedling, Adult, Ripe };

        public enum ItemEnum { Resource, Equipment, Tool, Food, Consumable, Clothing, MonsterFood, NPCToken, Blueprint, Special };
        public enum ToolEnum { None, Backpack, Pick, Axe, Shovel, WateringCan, Harp, Lantern, Return, Scythe };
        public enum SpecialItemEnum { None, Marriage, Class, Map, DungeonKey, Task };
        public enum GearTypeEnum { None, Accessory, Chest, Head, Weapon };
        public enum WeaponEnum { None, Spear, Shield, Rapier, Bow, Wand, Knife, Orb, Staff };
        public enum ArmorTypeEnum { None, Cloth, Light, Heavy };
        public enum ClothingEnum { None, Chest, Legs, Hat };
        public enum NPCTokenTypeEnum { Mount, Pet };

        public enum ActorMovementStateEnum { Idle, Walking };
        public enum ActorFaceEnum { Default, Happy, Angry, Sad };
        public enum WorldActorTypeEnum { Actor, Child, Critter, Merchant, Mob, Mount, Pet, ShippingGremlin, Spirit, Villager };
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
        public enum ObjectTypeEnum { WorldObject, Beehive, Buildable, Building, CombatHazard, Container, Decor, Destructible, DungeonObject, Floor, Gatherable, Garden, Machine, Mailbox, Plant, Structure, Wall, Wallpaper, WarpPoint };
        public enum SpawnConditionEnum { Spring, Summer, Winter, Fall, Precipitation, Night, Forest, Mountain, Swamp, Plains };
        public enum ExpectingChildEnum { None, Adoption, Pregnant };
        public enum RelationShipStatusEnum { None, Friends, Dating, Engaged, Married };
        public enum VillagerSpawnStatus { OffMap, WaitAtInn, VisitInn, HasHome, NonTownMap };
        public enum SatisfactionStateEnum { Miserable, Sad, Neutral, Pleased, Happy, Ecastatic };
        public enum NPCStateEnum { Alert, Idle, Leashing, TrackPlayer, Wander };
        public enum ChildStageEnum { Newborn, Infant, Toddler };
       
        public enum TaskStateEnum { Waiting, Assigned, Talking, TaskLog, Completed };
        public enum TaskTriggerEnum { GameStart, FriendLevel, Building, Task };
        public enum TaskTypeEnum { None, GroupSlay, Slay, Fetch, Talk, Build };

        public enum VillagerRequestEnum { Close, Far, TownWide };

        public enum GameIconEnum { None, AreaAll, AreaColumnAlly, AreaColumnEnemy, AreaSelf, AreaSingle, AreaRow, AreaSquare, Agility, BuffArrow, Damage, DebuffArrow, Defence, ElementFire, ElementIce, ElementLightning, Evasion, Experience, Heal, Magic, MaxHealth, MagicDamage, Melee, MoveDown, MoveLeft, MoveRight, MoveUp, PhysicalDamage, Ranged, Resistance, Speed, Strength, Timer };
    }
}
