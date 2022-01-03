﻿using System.Collections.Generic;
using Microsoft.Xna.Framework;
using RiverHollow.Characters;
using RiverHollow.GUIComponents.Screens;
using RiverHollow.GUIComponents.GUIObjects.GUIWindows;
using RiverHollow.WorldObjects;
using RiverHollow.Misc;

using RiverHollow.GUIComponents.GUIObjects;
using RiverHollow.Game_Managers.GUIObjects;
using RiverHollow.Items;

namespace RiverHollow.Game_Managers
{
    public static class GameManager
    {
        #region const strings for triggers
        public const string MOB_OPEN = "MOBS";
        public const string ITEM_OPEN = "VALID_ITEM";
        public const string KEY_OPEN = "KEY_USED";
        #endregion

        #region Defined Values
        public const int NORMAL_SCALE = 4;
        public const float TOOL_ANIM_SPEED = 0.08f;
        public const int MAX_BUILDING_LEVEL = 3;
        public const int TILE_SIZE = 16;

        public static int CurrentScale = NORMAL_SCALE;
        public static int ScaledTileSize => (int)(TILE_SIZE * CurrentScale);
        public static int ScaledPixel => (int)CurrentScale;
        #endregion

        #region Game Enums
        public enum TextEntryVerbEnum { None, Yes, No, Talk, Gift, Party, ShipGoods, Buy, ShowRequests, Propose, Date, Option_0, Option_1, Option_2, Option_3 };
        public enum TextEntrySelectionEnum { None, VillageTalk, MerchantTalk, YesNo, Shop, Party };
        public enum TextEntryTriggerEnum { None, UseItem, ConfirmGift, EndDay, Exit, Donate, PetFollow, PetUnfollow, GetBaby }

        public enum RarityEnum { C, U, R, M };
        public enum WeatherEnum { Sunny, Raining, Snowing };

        public enum DisplayTypeEnum { Inventory, Gift, Ship };

        public enum SeasonEnum { None, Spring, Summer, Fall, Winter };
        public enum ZoneEnum { Forest, Mountain, Field, Swamp, Town };
        public enum DirectionEnum { Down, Right, Up, Left };
        public enum CardinalDirectionsEnum { North, NorthEast, East, SouthEast, South, SouthWest, West, NorthWest };
        public enum VerbEnum { Walk, Idle, Hurt, Critical, Ground, Air, UseTool, Action1, Action2, Action3, Action4, Cast, MakeItem };
        public enum AnimationEnum { None, Spawn, KO, Win, PlayAnimation, Rain, Snow, ObjectIdle, Action_One, Action_Two, Action_Finished };
        public enum CombatActionEnum { Idle, Cast, Hurt, Attack, Critical, Spawn, KO, Victory };

        public enum ToolAnimEnum { Down, Up, Left, Right };
        public enum WorldObjAnimEnum { Idle, Working, Shake, Gathered };
        public enum PlantEnum { Seeds, Seedling, Adult, Ripe };

        public enum ItemEnum { Resource, Equipment, Tool, Food, Consumable, Clothes, MonsterFood, Blueprint, Special };
        public enum ToolEnum { None, Pick, Axe, Shovel, WateringCan, Harp, Lantern, Return, Scythe };
        public enum SpecialItemEnum { None, Marriage, Class, Map, DungeonKey, Task };
        public enum EquipmentEnum { Armor, Weapon, Accessory, Head, Wrist };
        public enum WeaponEnum { None, Spear, Shield, Rapier, Bow, Wand, Knife, Orb, Staff };
        public enum ArmorEnum { None, Cloth, Light, Heavy };
        public enum ArmorSlotEnum { None, Head, Armor, Wrist };
        public enum ClothingEnum { None, Body, Legs, Hat };

        public enum ActorMovementStateEnum { Idle, Walking };
        public enum ActorFaceEnum { Default, Happy, Angry, Sad };
        public enum TaskTypeEnum { None, GroupSlay, Slay, Fetch, Talk, Build };
        public enum ActorEnum { Actor, PartyMember, Environmental, Merchant, Mob, Monster, Mount, Pet, Child, ShippingGremlin, Spirit, Summon, Villager };
        public enum AttributeEnum { Agility, Damage, Defense, Evasion, Magic, MaxHealth, Resistance, Speed, Strength };
        public enum PotencyBonusEnum { None, Conditions, Summon};
        public enum PlayerColorEnum { None, Eyes, Hair, Skin };
        public enum ActionEnum { Action, Item, Move };
        public enum SkillTagsEnum { Bonus, Harm, Heal, NPC_ID, Push, Pull, Remove, Retreat, Step, StatusEffectID };
        public enum TargetEnum { Enemy, Ally };
        public enum DamageTypeEnum { Physical, Magical };
        public enum MovementTypeEnum { None, Forward, Backward };
        public enum RangeEnum { Self, Melee, Ranged, Row, Column, Adjacent};
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
        public enum NPCStateEnum { Alert, Idle, TrackPlayer, Pathing, Wander };
        public enum ChildStageEnum { Newborn, Infant, Toddler };

        public enum GameIconEnum { None, AreaAll, AreaColumnAlly, AreaColumnEnemy, AreaSelf, AreaSingle, AreaRow, AreaSquare, Agility, BuffArrow, DebuffArrow, Defense, ElementFire, ElementIce, ElementLightning, Evasion, Heal, Magic, MaxHealth, MagicDamage, Melee, MoveLeft, MoveRight, PhysicalDamage, Ranged, Resistance, Speed, Strength, Timer};

        public static GameIconEnum GetGameIconFromAttribute(AttributeEnum e)
        {
            GameIconEnum rv = GameIconEnum.None;
            switch (e)
            {
                case AttributeEnum.Agility:
                    return GameIconEnum.Agility;
                case AttributeEnum.Defense:
                    return GameIconEnum.Defense;
                case AttributeEnum.Evasion:
                    return GameIconEnum.Evasion;
                case AttributeEnum.Magic:
                    return GameIconEnum.Magic;
                case AttributeEnum.MaxHealth:
                    return GameIconEnum.MaxHealth;
                case AttributeEnum.Resistance:
                    return GameIconEnum.Resistance;
                case AttributeEnum.Speed:
                    return GameIconEnum.Speed;
                case AttributeEnum.Strength:
                    return GameIconEnum.Strength;
            }

            return rv;
        }
        public static AttributeEnum GetDefenseType(AttributeEnum e)
        {
            switch (e)
            {
                case AttributeEnum.Magic:
                    return AttributeEnum.Resistance;
                default:
                    return AttributeEnum.Defense;
            }
        }
        #endregion

        #region Managed Data Lists
        public static List<Merchant> MerchantQueue;
        private static List<TriggerObject> _liTriggerObjects;
        private static List<Spirit> _liSpirits;
        private static List<Machine> _liMachines;

        public static Dictionary<int, RHTask> DITasks;
        public static Dictionary<int, Shop> DIShops;
        #endregion

        #region Interaction Objects
        public static Merchandise CurrentMerch;
        public static Item CurrentItem;
        public static Spirit CurrentSpirit;
        public static WorldObject CurrentWorldObject;
        #endregion

        #region Game State Values
        public static int VillagersInTheInn = 0;
        public static int MAX_NAME_LEN = 10;

        public static int TotalExperience = 0;
        public static List<GUISprite> SlainMonsters;

        public static bool AutoDisband;
        public static bool HideMiniInventory = true;

        public static TalkingActor CurrentNPC => interactionLock?.CurrentActor;
        public static ShippingGremlin ShippingGremlin;
        public static DisplayTypeEnum CurrentInventoryDisplay;
        #endregion

        public static int HUDItemRow;
        public static int HUDItemCol;

        public static void Initialize()
        {
            MerchantQueue = new List<Merchant>();
            _liMachines = new List<Machine>();
            _liSpirits = new List<Spirit>();
            _liTriggerObjects = new List<TriggerObject>();
            SlainMonsters = new List<GUISprite>();
        }

        public static void LoadManagedDataLists()
        {
            DITasks = new Dictionary<int, RHTask>();
            foreach (KeyValuePair<int, Dictionary<string, string>> kvp in DataManager.DiTaskData)
            {
                DITasks.Add(kvp.Key, new RHTask(kvp.Key, kvp.Value));
            }

            DIShops = DataManager.GetShopInfoList();
        }

        public static void ClearGMObjects()
        {
            CurrentWorldObject = null;
            CurrentItem = null;
            CurrentSpirit = null;
        }

        /// <summary>
        /// Returns an int value of the given float times the Scale
        /// </summary>
        public static int ScaleIt(int val)
        {
            return CurrentScale * val;
        }
        public static Vector2 ScaleIt(Vector2 val)
        {
            return new Vector2(ScaleIt((int)val.X), ScaleIt((int)val.Y));
        }

        #region Machine Handling
        public static void AddMachine(Machine m, string mapName)
        {
            _liMachines.Add(m);
        }
        public static void RemoveMachine(Machine m)
        {
            _liMachines.Remove(m);
        }

        public static void RollOver()
        {
            foreach (Machine m in _liMachines)
            {
                m.Rollover();
            }
        }
        #endregion

        #region Trigger Handling
        public static void AddTrigger(TriggerObject t)
        {
            _liTriggerObjects.Add(t);
        }

        public static void AddSpirit(Spirit s)
        {
            _liSpirits.Add(s);
        }

        public static void ActivateTriggers(string triggerName)
        {
            foreach (TriggerObject t in _liTriggerObjects)
            {
                t.AttemptToTrigger(triggerName);
            }

            foreach (Spirit s in _liSpirits)
            {
                s.AttemptToAwaken(triggerName);
            }
        }
        #endregion

        #region Held Objects
        public static Vector2 MarketPosition = new Vector2(-1, -1);
        static Item _heldItem;
        public static Item HeldItem { get => _heldItem; }
        static WorldObject _heldWorldObject;
        public static WorldObject HeldObject { get => _heldWorldObject; }

        /// <summary>
        /// Grabs a building to be placed and/or moved.
        /// </summary>
        /// <returns>True if the building exists</returns>
        public static bool PickUpWorldObject(WorldObject obj)
        {
            bool rv = false;
            if (obj != null)
            {
                _heldWorldObject = obj;
                rv = true;
            }

            return rv;
        }
        public static void DropWorldObject()
        {
            _heldWorldObject = null;
        }

        /// <summary>
        /// Grabs an item to be moved around inventory
        /// </summary>
        /// <returns>True if the item exists</returns>
        public static bool GrabItem(Item item)
        {
            bool rv = false;
            if (item != null)
            {
                _heldItem = item;
                GUICursor.SetGUIItem(_heldItem);
                rv = true;
            }

            return rv;
        }
        public static void DropItem()
        {
            _heldItem = null;
            GUICursor.SetGUIItem(null);
        }
        #endregion

        #region States
        private enum EnumBuildType { None, BuildMode, Destroy, Storage, Move, Upgrade };
        private static EnumBuildType _eBuildType;
        public static bool BuildFromStorage { get; private set; } = false;

        #region TakeInput
        private static bool _bTakeInput;
        public static void TakeInput() { _bTakeInput = true; }
        public static void StopTakingInput() { _bTakeInput = false; }
        /// <summary>
        /// Returns whether or not the game is taking our input for a text box or something.
        /// </summary>
        /// <returns>True if something is taking the input</returns>
        public static bool TakingInput() { return _bTakeInput; }
        #endregion

        #region Running
        private static InteractionLock interactionLock;
        public static void Pause()
        {
            Pause(null);
        }
        public static void Pause(TalkingActor act) {
            GUICursor.ResetCursor();
            if (interactionLock == null) { interactionLock = new InteractionLock(act); }
            else { interactionLock.AddLock(); }
        }
        public static void Unpause() {
            if(interactionLock != null && interactionLock.RemoveLock())
            {
                ClearGMObjects();
                CurrentNPC?.StopTalking();
                interactionLock = null;
            }
        }
        public static bool IsPaused() { return interactionLock != null; }
        public static bool IsRunning() { return interactionLock == null; }
        #endregion

        public static void SetGameScale(int val)
        {
            CurrentScale = val;
        }

        #region Scrying
        private static bool _bScrying;
        public static void Scry(bool val = true) { _bScrying = val; }
        public static bool Scrying() { return _bScrying; }
        #endregion

        #region ShowMap
        private static bool _bShowMap;
        public static void ShowMap(bool val = true) { _bShowMap = val; }
        public static bool IsMapShown() { return _bShowMap; }
        #endregion

        public static void GoToCombatScreen()
        {
            //ShowMap(false);
            Pause();
            GUIManager.SetScreen(new LiteCombatScreen());
        }

        public static void GoToHUDScreen()
        {
            GUIManager.SetScreen(new HUDScreen());
            Unpause();
            ShowMap();
        }

        public static void EnterTownModeBuild(bool fromStorage = false)
        {
            BuildFromStorage = fromStorage;
            _eBuildType = EnumBuildType.BuildMode;

            GUIManager.CloseMainObject();
            Scry();
        }

        public static bool InTownMode() { return TownModeBuild() || TownModeMoving() || TownModeDestroy() || TownModeStorage() || TownModeUpgrade(); }
        public static bool TownModeBuild() { return _eBuildType == EnumBuildType.BuildMode; }
        public static bool TownModeMoving() { return _eBuildType == EnumBuildType.Move; }
        public static bool TownModeDestroy() { return _eBuildType == EnumBuildType.Destroy; }
        public static bool TownModeStorage() { return _eBuildType == EnumBuildType.Storage; }
        public static bool TownModeUpgrade() { return _eBuildType == EnumBuildType.Upgrade; }

        public static void EnterTownModeMoving() { _eBuildType = EnumBuildType.Move; }
        public static void EnterTownModeDestroy() { _eBuildType = EnumBuildType.Destroy; }
        public static void EnterTownModeStorage() { _eBuildType = EnumBuildType.Storage; }
        public static void EnterTownModeUpgrade() { _eBuildType = EnumBuildType.Upgrade; }

        public static void LeaveTownMode() {
            BuildFromStorage = false;
            _eBuildType = EnumBuildType.None;
            
            foreach(Villager v in DataManager.DIVillagers.Values)
            {
                v.DetermineValidSchedule();
                v.RecalculatePath();
            }
        }
        #endregion

        private class InteractionLock
        {
            int _iLocks = 0;
            public TalkingActor CurrentActor { get; }
            public InteractionLock(TalkingActor act = null)
            {
                CurrentActor = act;
                _iLocks = 1;
            }

            public void AddLock()
            {
                _iLocks++;
            }

            public bool RemoveLock()
            {
                bool rv = false;

                if(--_iLocks == 0)
                {
                    rv = true;
                }

                return rv;
            }
        }
    }
}
