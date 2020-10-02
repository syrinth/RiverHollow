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

        public enum DisplayTypeEnum { Inventory, Gift, Ship };

        public enum ZoneEnum { Forest, Mountain, Field, Swamp, Town };
        public enum DirectionEnum { Up, Down, Right, Left };
        public enum CardinalDirectionsEnum { North, NorthEast, East, SouthEast, South,  SouthWest, West, NorthWest};
        public enum VerbEnum { Walk, Idle, Hurt, Critical, Ground, Air, UseTool, Attack, Cast, MakeItem };
        public enum AnimationEnum { None, Spawn, KO, Win, PlayAnimation, Rain, Snow, ObjectIdle, ObjectAction1, ObjectAction2, ObjectActionFinished };

        public enum ToolAnimEnum { Down, Up, Left, Right }
        public enum WorldObjAnimEnum { Idle, Working, Shake, Gathered };
        public enum PlantEnum { Seeds, Seedling, Adult, Ripe };

        public enum ActorEnum { Actor, Adventurer, CombatActor, Monster, NPC, ShippingGremlin, Spirit, WorldCharacter };
        public enum NPCTypeEnum { Villager, Eligible, Shopkeeper, Ranger, Worker, Mason }
        public enum StatEnum { Atk, Str, Def, Mag, Res, Spd, Vit, Crit, Evade};
        public enum PotencyBonusEnum { None, Conditions, Summons };
        public enum EquipmentEnum { Armor, Weapon, Accessory, Head, Wrist};
        public enum SpecialItemEnum { None, Marriage, Class, Map, DungeonKey, Quest };
        public enum PlayerColorEnum { None, Eyes, Hair, Skin };
        public enum ActionEnum { Action, Item, Spell, MenuItem, MenuSpell, MenuAction, Move, EndTurn };
        public enum SkillTagsEnum { Bonus, Harm, Heal, Push, Pull, Remove, Retreat, Step, Status, Summon};
        public enum TargetEnum { Enemy, Ally};
        public enum AreaTypeEnum { Single, Cross, Ring, Line };
        public enum ElementEnum { None, Fire, Ice, Lightning };
        public enum AttackTypeEnum { Physical, Magical };
        public enum ElementAlignment { Neutral, Vulnerable, Resists };
        public enum ConditionEnum { None, KO, Poisoned, Silenced };
        public enum WorkerTypeEnum { None, Magic, Martial };
        public enum ObjectTypeEnum { WorldObject, Building, ClassChanger, Machine, Container, Earth, Floor, Destructible, Plant, Wall, Light, DungeonObject, CombatHazard };
        public enum WeaponEnum { None, Spear, Shield, Rapier, Bow, Wand, Knife, Orb, Staff };
        public enum ArmorEnum { None, Cloth, Light, Heavy };
        public enum ArmorSlotEnum { None, Head, Armor, Wrist };
        public enum SpawnConditionEnum { Spring, Summer, Winter, Fall, Precipitation, Night, Forest, Mountain, Swamp, Plains };
        public enum ToolEnum { Pick, Axe, Shovel, WateringCan, Harp, Lantern, Return };
        public enum ClothesEnum { None, Body, Legs, Hat };
        public static float Scale = 4f;
        public const int TileSize = 16;
        public static int ScaledTileSize => (int)(TileSize * Scale);
        public static int MaxBldgLevel = 3;
        public static Dictionary<int, Upgrade> DiUpgrades;
        public static Dictionary<int, Quest> DiQuests;
        private static List<TriggerObject> _liTriggerObjects;
        private static List<Spirit> _liSpirits;
        private static List<Machine> _liMachines;

        public static ShippingGremlin ShippingGremlin;
        public static Merchandise gmMerchandise;
        public static TalkingActor CurrentNPC;
        public static Item gmActiveItem;
        public static Spirit gmSpirit;
        public static TriggerObject gmDungeonObject;
        public static DisplayTypeEnum CurrentInventoryDisplay;

        public static int MAX_NAME_LEN = 10;

        public static int TotalExperience = 50;
        public static List<GUISprite> SlainMonsters;

        public static bool AutoDisband;
        public static bool HideMiniInventory = true;

        public const float TOOL_ANIM_SPEED = 0.15f;

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
            else if (selectedAction.Contains("SellContract") && GameManager.CurrentNPC != null)
            {
                if (GameManager.CurrentNPC.IsActorType(ActorEnum.Adventurer))
                {
                    ((Adventurer)GameManager.CurrentNPC).Building.RemoveWorker((Adventurer)GameManager.CurrentNPC);
                    PlayerManager.AddMoney(1000);
                    GUIManager.CloseMainObject();
                }
            }
        }

        public static void ClearGMObjects()
        {
            ClearCurrentNPC();
            gmDungeonObject = null;
            gmActiveItem = null;
            gmSpirit = null;
        }

        /// <summary>
        /// Increments the number of active objects in the CurrentNPC if it exists
        /// </summary>
        public static void AddCurrentNPCLockObject()
        {
            CurrentNPC?.AddCurrentNPCLockObject();
        }

        /// <summary>
        /// Decrements the number of active objects in the CurrentNPC if it exists
        /// </summary>
        public static void RemoveCurrentNPCLockObject()
        {
            CurrentNPC?.RemoveCurrentNPCLockObject();
        }

        /// <summary>
        /// Tells the CurrentNPC to StopTalking and then sets the CurrentNPC to null
        /// </summary>
        public static void ClearCurrentNPC()
        {
            CurrentNPC?.StopTalking();
            CurrentNPC = null;
        }

        /// <summary>
        /// Returns an int value of the given float times the Scale
        /// </summary>
        public static int ScaleIt(int val)
        {
            return (int)(Scale * val);
        }

        #region Machine Handling
        public static void AddMachine(Machine m)
        {
            _liMachines.Add(m);
        }

        public static void RollOver()
        {
            foreach(Machine m in _liMachines)
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
            foreach(TriggerObject t in _liTriggerObjects)
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
                rv = true;
            }

            return rv;
        }
        public static void DropItem()
        {
            _heldItem = null;
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
        private static bool _bRunning;
        public static void Pause() {
            _bRunning = false;
            GUICursor._CursorType = GUICursor.EnumCursorType.Normal;
        }
        public static void Unpause() {
            _bRunning = true;
        }
        public static bool IsPaused() { return !_bRunning; }
        public static bool IsRunning() { return _bRunning; }
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
            ClearGMObjects();
        }

        public static bool Constructing() { return _buildType == EnumBuildType.Construct; }
        public static void ConstructBuilding() { _buildType = EnumBuildType.Construct; }
        public static bool MovingBuildings() { return _buildType == EnumBuildType.Move; }
        public static void MoveBuilding() { _buildType = EnumBuildType.Move; }
        public static bool DestroyingBuildings() { return _buildType == EnumBuildType.Destroy; }
        public static void DestroyBuilding() { _buildType = EnumBuildType.Destroy; }
        public static void LeaveBuildMode() { _buildType = EnumBuildType.None; }
        #endregion
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
