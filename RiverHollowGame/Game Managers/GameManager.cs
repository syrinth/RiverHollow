using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using RiverHollow.Buildings;
using RiverHollow.Characters;
using RiverHollow.GUIComponents.Screens;
using RiverHollow.GUIComponents.GUIObjects.GUIWindows;
using RiverHollow.Items;
using RiverHollow.Misc;
using RiverHollow.Utilities;

using static RiverHollow.Characters.ShopKeeper;
using static RiverHollow.Characters.Actor;
using RiverHollow.GUIComponents.GUIObjects;
using static RiverHollow.Items.WorldItem;

namespace RiverHollow.Game_Managers
{
    public static class GameManager
    {
        #region const strings for triggers
        public const string MOB_OPEN = "MOBS";
        public const string ITEM_OPEN = "VALID_ITEM";
        public const string KEY_OPEN = "KEY_USED";
        #endregion

        public enum RarityEnum { C, U, R, M };
        public enum WeatherEnum { Sunny, Raining, Snowing };

        public enum DisplayTypeEnum { Inventory, Gift, Ship };

        public enum ZoneEnum { Forest, Mountain, Field, Swamp, Town };
        public enum DirectionEnum { Up, Down, Right, Left };
        public enum CardinalDirectionsEnum { North, NorthEast, East, SouthEast, South, SouthWest, West, NorthWest };
        public enum VerbEnum { Walk, Idle, Hurt, Critical, Ground, Air, UseTool, Action1, Action2, Action3, Action4, Cast, MakeItem };
        public enum AnimationEnum { None, Spawn, KO, Win, PlayAnimation, Rain, Snow, ObjectIdle, ObjectAction1, ObjectAction2, ObjectActionFinished };

        public enum ToolAnimEnum { Down, Up, Left, Right }
        public enum WorldObjAnimEnum { Idle, Working, Shake, Gathered };
        public enum PlantEnum { Seeds, Seedling, Adult, Ripe };

        public enum QuestTypeEnum { GroupSlay, Slay, Fetch, Talk }
        public enum ActorEnum { Actor, Adventurer, CombatActor, Monster, NPC, ShippingGremlin, Spirit, Summon, WorldCharacter };
        public enum NPCTypeEnum { Villager, Eligible, Shopkeeper, Ranger, Worker, Mason, ShippingGremlin }
        public enum StatEnum { Atk, Str, Def, Mag, Res, Spd, Vit, Crit, Evade };
        public enum PotencyBonusEnum { None, Conditions, Summon};
        public enum EquipmentEnum { Armor, Weapon, Accessory, Head, Wrist };
        public enum SpecialItemEnum { None, Marriage, Class, Map, DungeonKey, Quest };
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
        public enum ObjectTypeEnum { WorldObject, Building, ClassChanger, Machine, Container, Earth, Floor, Destructible, Gatherable, Plant, Wall, Light, DungeonObject, CombatHazard };
        public enum WeaponEnum { None, Spear, Shield, Rapier, Bow, Wand, Knife, Orb, Staff };
        public enum ArmorEnum { None, Cloth, Light, Heavy };
        public enum ArmorSlotEnum { None, Head, Armor, Wrist };
        public enum SpawnConditionEnum { Spring, Summer, Winter, Fall, Precipitation, Night, Forest, Mountain, Swamp, Plains };
        public enum ToolEnum { Pick, Axe, Shovel, WateringCan, Harp, Lantern, Return };
        public enum ClothesEnum { None, Body, Legs, Hat };
        public enum MachineTypeEnum { Processer, CraftingMachine };
        public static float Scale = 4f;
        public const int TileSize = 16;
        public static int ScaledTileSize => (int)(TileSize * Scale);
        public static int ScaledPixel => (int)Scale;
        public static int MaxBldgLevel = 3;
        public static Dictionary<int, Upgrade> DiUpgrades;
        public static Dictionary<int, Quest> DiQuests;
        private static List<TriggerObject> _liTriggerObjects;
        private static List<Spirit> _liSpirits;
        private static List<Machine> _liMachines;
        public static TalkingActor CurrentNPC => interactionLock?.CurrentActor;
        public static ShippingGremlin ShippingGremlin;
        public static DisplayTypeEnum CurrentInventoryDisplay;

        #region Interaction Objects
        public static Adventurer CurrentAdventurer;
        public static Merchandise CurrentMerch;
        public static Item CurrentItem;
        public static Spirit CurrentSpirit;
        public static TriggerObject CurrentTriggerObject;

        public static Machine ConstructionObject;
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
            DiUpgrades = new Dictionary<int, Upgrade>();
            foreach (KeyValuePair<int, string> kvp in DataManager.DiUpgrades)
            {
                DiUpgrades.Add(kvp.Key, new Upgrade(kvp.Key, kvp.Value));
            }
        }

        public static void LoadQuests(ContentManager Content)
        {
            DiQuests = new Dictionary<int, Quest>();
            foreach (KeyValuePair<int, Dictionary<string, string>> kvp in DataManager.DiQuestData)
            {
                DiQuests.Add(kvp.Key, new Quest(kvp.Key, kvp.Value));
            }
        }

        public static void ProcessTextInteraction(string selectedAction)
        {
            if (selectedAction.Equals("EndDay"))
            {
                Vector2 pos = PlayerManager.World.CollisionBox.Center.ToVector2();
                PlayerManager.SetPath(TravelManager.FindPathToLocation(ref pos, MapManager.CurrentMap.DictionaryCharacterLayer["PlayerSpawn"]));
                GUIManager.SetScreen(new DayEndScreen());
            }
            else if (selectedAction.Contains("SellContract") && GameManager.CurrentAdventurer != null)
            {
                if (GameManager.CurrentAdventurer.IsActorType(ActorEnum.Adventurer))
                {
                    ((Adventurer)GameManager.CurrentAdventurer).Building.RemoveWorker((Adventurer)GameManager.CurrentAdventurer);
                    PlayerManager.AddMoney(1000);
                    GUIManager.CloseMainObject();
                }
            }
        }

        public static void ClearGMObjects()
        {
            CurrentAdventurer = null;
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
            m.SetMapName(mapName);
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
        static Item _heldItem;
        public static Item HeldItem { get => _heldItem; }
        static Building _heldBuilding;
        public static Building HeldBuilding { get => _heldBuilding; }

        /// <summary>
        /// Grabs a building to be placed and/or moved.
        /// </summary>
        /// <returns>True if the building exists</returns>
        public static bool PickUpBuilding(Building bldg)
        {
            bool rv = false;
            if (bldg != null)
            {
                _heldBuilding = bldg;
                rv = true;
            }

            return rv;
        }
        public static void DropBuilding()
        {
            _heldBuilding = null;
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
        private enum EnumBuildType { None, Construct, Destroy, Move }
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

        public static bool Constructing() { return _buildType == EnumBuildType.Construct; }
        public static void ConstructBuilding() { _buildType = EnumBuildType.Construct; }
        public static bool MovingBuildings() { return _buildType == EnumBuildType.Move; }
        public static void MoveBuilding() { _buildType = EnumBuildType.Move; }
        public static bool DestroyingBuildings() { return _buildType == EnumBuildType.Destroy; }
        public static void DestroyBuilding() { _buildType = EnumBuildType.Destroy; }
        public static void LeaveBuildMode() { _buildType = EnumBuildType.None; }
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

    public class Upgrade
    {
        enum UpgradeTypeEnum { Building }
        UpgradeTypeEnum _type;
        int _id;
        public int ID { get => _id; }
        string _name;
        public string Name { get => _name; }
        string _description;
        public string Description { get => _description; }
        int _iCost;
        public int MoneyCost { get => _iCost; }
        List<KeyValuePair<int, int>> _liRequiredItems;
        public List<KeyValuePair<int, int>> LiRquiredItems { get => _liRequiredItems; }
        public bool Enabled;

        public Upgrade(int id, string strData)
        {
            _id = id;
            _liRequiredItems = new List<KeyValuePair<int, int>>();

            DataManager.GetUpgradeText(_id, ref _name, ref _description);

            string[] strSplit = Util.FindTags(strData);
            foreach (string s in strSplit)
            {
                string[] tagType = s.Split(':');
                if (tagType[0].Equals("Type"))
                {
                    _type = Util.ParseEnum<UpgradeTypeEnum>(tagType[1]);
                }
                else if (tagType[0].Equals("Cost"))
                {
                    _iCost = int.Parse(tagType[1]);
                }
                else if (tagType[0].Equals("ItemReq"))
                {
                    string[] itemSplit = tagType[1].Split(':');
                    for (int i = 0; i < itemSplit.Length; i++)
                    {
                        string[] entrySplit = itemSplit[i].Split('-');
                        _liRequiredItems.Add(new KeyValuePair<int, int>(int.Parse(entrySplit[0]), int.Parse(entrySplit[1])));
                    }
                }
            } 
        }
    }
}
