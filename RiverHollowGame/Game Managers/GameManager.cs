using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using RiverHollow.Buildings;
using RiverHollow.Characters;
using RiverHollow.GUIComponents.Screens;
using RiverHollow.GUIComponents.GUIObjects.GUIWindows;
using RiverHollow.Items;
using RiverHollow.Misc;

using RiverHollow.GUIComponents.GUIObjects;
using static RiverHollow.Items.Structure;

namespace RiverHollow.Game_Managers
{
    public static class GameManager
    {
        #region const strings for triggers
        public const string MOB_OPEN = "MOBS";
        public const string ITEM_OPEN = "VALID_ITEM";
        public const string KEY_OPEN = "KEY_USED";
        #endregion

        public const float NORMAL_SCALE = 4f;

        public enum TextEntryVerbEnum { None, Yes, No, Talk, Gift, Party, ShipGoods, Buy, ShowRequests, Option_0, Option_1, Option_2, Option_3 };
        public enum TextEntrySelectionEnum { None, VillageTalk, MerchantTalk, YesNo, Shop, Party };
        public enum TextEntryTriggerEnum { None, UseItem, ConfirmGift, EndDay, Exit, Donate }

        public enum RarityEnum { C, U, R, M };
        public enum WeatherEnum { Sunny, Raining, Snowing };

        public enum DisplayTypeEnum { Inventory, Gift, Ship };

        public enum ZoneEnum { Forest, Mountain, Field, Swamp, Town };
        public enum DirectionEnum { Up, Down, Right, Left };
        public enum CardinalDirectionsEnum { North, NorthEast, East, SouthEast, South, SouthWest, West, NorthWest };
        public enum VerbEnum { Walk, Idle, Hurt, Critical, Ground, Air, UseTool, Action1, Action2, Action3, Action4, Cast, MakeItem };
        public enum AnimationEnum { None, Spawn, KO, Win, PlayAnimation, Rain, Snow, ObjectIdle, Action_One, Action_Two, Action_Finished };

        public enum ToolAnimEnum { Down, Up, Left, Right };
        public enum WorldObjAnimEnum { Idle, Working, Shake, Gathered };
        public enum PlantEnum { Seeds, Seedling, Adult, Ripe };

        public enum ItemEnum { Resource, Equipment, Tool, Food, Consumable, StaticItem, Clothes, MonsterFood, Blueprint, Special };
        public enum ToolEnum { None, Pick, Axe, Shovel, WateringCan, Harp, Lantern, Return, Scythe };
        public enum SpecialItemEnum { None, Marriage, Class, Map, DungeonKey, Task };
        public enum EquipmentEnum { Armor, Weapon, Accessory, Head, Wrist };
        public enum WeaponEnum { None, Spear, Shield, Rapier, Bow, Wand, Knife, Orb, Staff };
        public enum ArmorEnum { None, Cloth, Light, Heavy };
        public enum ArmorSlotEnum { None, Head, Armor, Wrist };
        public enum ClothesEnum { None, Body, Legs, Hat };

        public enum ActorMovementStateEnum { Idle, Walking };
        public enum ActorFaceEnum { Default, Happy, Angry, Sad };
        public enum TaskTypeEnum { None, GroupSlay, Slay, Fetch, Talk, Build };
        public enum ActorEnum { Actor, Merchant, Monster, ShippingGremlin, Spirit, Summon, Villager };
        public enum StatEnum { Atk, Str, Def, Mag, Res, Spd, Vit, Crit, Evade };
        public enum PotencyBonusEnum { None, Conditions, Summon};
        public enum PlayerColorEnum { None, Eyes, Hair, Skin };
        public enum ActionEnum { Action, Item, Spell, MenuItem, MenuSpell, MenuAction, Move, EndTurn };
        public enum SkillTagsEnum { Bonus, Harm, Heal, Push, Pull, Remove, Retreat, Step, StatusEffectID, SummonID };
        public enum TargetEnum { Enemy, Ally };
        public enum AreaTypeEnum { Single, Cross, Ring, Line, Diamond };
        public enum ElementEnum { None, Fire, Ice, Lightning };
        public enum AttackTypeEnum { Physical, Magical };
        public enum ElementAlignment { Neutral, Vulnerable, Resists };
        public enum ConditionEnum { None, KO, Poisoned, Silenced };
        public enum AdventurerTypeEnum { Magic, Martial };
        public enum ObjectTypeEnum { WorldObject, Building, ClassChanger, Machine, Container, Earth, Floor, Destructible, Gatherable, Plant, Wall, Light, DungeonObject, CombatHazard, Mailbox, StructureUpgrader, WalkableStructure};
        public enum SpawnConditionEnum { Spring, Summer, Winter, Fall, Precipitation, Night, Forest, Mountain, Swamp, Plains };

        public static float Scale = NORMAL_SCALE;
        public const int TileSize = 16;
        public static int ScaledTileSize => (int)(TileSize * Scale);
        public static int ScaledPixel => (int)Scale;
        public static int MaxBldgLevel = 3;
        private static List<TriggerObject> _liTriggerObjects;
        private static List<Spirit> _liSpirits;
        private static List<Machine> _liMachines;
        public static TalkingActor CurrentNPC => interactionLock?.CurrentActor;
        public static ShippingGremlin ShippingGremlin;
        public static DisplayTypeEnum CurrentInventoryDisplay;

        public static int VillagersInTheInn = 0;

        #region Managed Data Lists
        public static Dictionary<int, BuildInfo> DIBuildInfo;
        public static Dictionary<int, Task> DITasks;
        public static Dictionary<int, List<Merchandise>> DIShops;
        #endregion

        #region Interaction Objects
        public static Merchandise CurrentMerch;
        public static Item CurrentItem;
        public static Spirit CurrentSpirit;
        public static TriggerObject CurrentTriggerObject;
        #endregion

        public static int MAX_NAME_LEN = 10;

        public static int TotalExperience = 0;
        public static List<GUISprite> SlainMonsters;

        public static bool AutoDisband;
        public static bool HideMiniInventory = true;

        public const float TOOL_ANIM_SPEED = 0.08f;

        public static int HUDItemRow;
        public static int HUDItemCol;

        public static void LoadContent(ContentManager Content)
        {
            _liMachines = new List<Machine>();
            _liSpirits = new List<Spirit>();
            _liTriggerObjects = new List<TriggerObject>();
            SlainMonsters = new List<GUISprite>();
        }

        public static void LoadManagedDataLists()
        {
            DITasks = new Dictionary<int, Task>();
            foreach (KeyValuePair<int, Dictionary<string, string>> kvp in DataManager.DiTaskData)
            {
                DITasks.Add(kvp.Key, new Task(kvp.Key, kvp.Value));
            }

            DIBuildInfo = DataManager.GetBuildInfoList();

            DIShops = DataManager.GetShopInfoList();
        }

        public static void ClearGMObjects()
        {
            CurrentTriggerObject = null;
            CurrentItem = null;
            CurrentSpirit = null;
        }

        /// <summary>
        /// Returns an int value of the given float times the Scale
        /// </summary>
        public static int ScaleIt(int val)
        {
            return (int)(Scale * val);
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
        private enum EnumBuildType { None, BuildMode, Destroy, Move }
        private static EnumBuildType _buildType;

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

        public static void SetGameScale(float val)
        {
            Scale = val;
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

        public static void GoToHUDScreen()
        {
            GUIManager.SetScreen(new HUDScreen());
            Unpause();
            ShowMap();
        }

        public static void EnterTownModeBuild()
        {
            _buildType = EnumBuildType.BuildMode;

            GUIManager.CloseMainObject();
            Scry();
        }

        public static bool InTownMode() { return TownModeBuild() || TownModeMoving() || TownModeDestroy(); }
        public static bool TownModeBuild() { return _buildType == EnumBuildType.BuildMode; }
        public static bool TownModeMoving() { return _buildType == EnumBuildType.Move; }
        public static bool TownModeDestroy() { return _buildType == EnumBuildType.Destroy; }

        public static void EnterTownModeMoving() { _buildType = EnumBuildType.Move; }
        public static void EnterTownModeDestroy() { _buildType = EnumBuildType.Destroy; }

        public static void LeaveTownMode() { _buildType = EnumBuildType.None; }
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
