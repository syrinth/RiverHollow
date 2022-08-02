using System.Collections.Generic;
using Microsoft.Xna.Framework;
using RiverHollow.Characters;
using RiverHollow.GUIComponents.Screens;
using RiverHollow.GUIComponents.GUIObjects.GUIWindows;
using RiverHollow.WorldObjects;
using RiverHollow.Misc;

using RiverHollow.GUIComponents.GUIObjects;
using RiverHollow.Game_Managers.GUIObjects;
using RiverHollow.Items;
using static RiverHollow.Utilities.Enums;
using static RiverHollow.GUIComponents.GUIObjects.GUIObject;
using RiverHollow.Utilities;

namespace RiverHollow.Game_Managers
{
    public static class GameManager
    {
        #region Defined Values
        public static int CurrentScale = Constants.NORMAL_SCALE;
        public static int ScaledTileSize => (int)(Constants.TILE_SIZE * CurrentScale);
        public static int ScaledPixel => (int)CurrentScale;
        #endregion

        #region Game Enums
        public static GameIconEnum GetGameIconFromAttribute(AttributeEnum e)
        {
            GameIconEnum rv = GameIconEnum.None;
            switch (e)
            {
                case AttributeEnum.Agility:
                    return GameIconEnum.Agility;
                case AttributeEnum.Damage:
                    return GameIconEnum.Damage;
                case AttributeEnum.Defence:
                    return GameIconEnum.Defence;
                case AttributeEnum.Evasion:
                    return GameIconEnum.Evasion;
                case AttributeEnum.Magic:
                    return GameIconEnum.Magic;
                case AttributeEnum.Vitality:
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
        public static AttributeEnum GetDefenceType(AttributeEnum e)
        {
            switch (e)
            {
                case AttributeEnum.Magic:
                    return AttributeEnum.Resistance;
                default:
                    return AttributeEnum.Defence;
            }
        }
        #endregion

        #region Managed Data Lists
        private static Villager[] InnPositions;
        public static List<Merchant> MerchantQueue;
        private static List<TriggerObject> _liTriggerObjects;
        private static List<Spirit> _liSpirits;
        private static List<Machine> _liMachines;
        public static Dictionary<int, Shop> DIShops;
        #endregion

        #region Interaction Objects
        public static TalkingActor CurrentNPC { get; private set; }
        public static Merchandise CurrentMerch { get; private set; }
        public static Item CurrentItem { get; private set; }
        public static Spirit CurrentSpirit { get; private set; }
        public static WorldObject CurrentWorldObject { get; private set; }
        #endregion

        #region Game State Values
        public static int MAX_NAME_LEN = 8;

        public static int TotalExperience = 0;
        public static List<GUISprite> SlainMonsters;

        public static bool HideMiniInventory = true;

        public static ShippingGremlin ShippingGremlin;
        public static DisplayTypeEnum CurrentInventoryDisplay;

        public static GameScreenEnum CurrentScreen;
        #endregion

        public static int HUDItemRow;
        public static int HUDItemCol;

        public static void Initialize()
        {
            InnPositions = new Villager[3];
            MerchantQueue = new List<Merchant>();
            _liMachines = new List<Machine>();
            _liSpirits = new List<Spirit>();
            _liTriggerObjects = new List<TriggerObject>();
            SlainMonsters = new List<GUISprite>();
        }

        public static void LoadManagedDataLists()
        {
            DIShops = DataManager.GetShopInfoList();
        }

        public static void SetCurrentNPC(TalkingActor npc)
        { 
             CurrentNPC = npc;
        }
        public static void SetSelectedItem(Item i)
        {
            CurrentItem = i;
        }

        public static void SetSelectedWorldObject(WorldObject o)
        {
            CurrentWorldObject = o;
        }
        public static void ClearGMObjects()
        {
            SetSelectedWorldObject(null);
            SetSelectedItem(null);
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

        #region Inn Handling
        public static void ClearInn()
        {
            for (int i = 0; i < InnPositions.Length; i++)
            {
                InnPositions[i] = null;
            }
        }
        public static List<Villager> GetInnGuests()
        {
            List<Villager> rv = new List<Villager>();
            for (int i = 0; i < InnPositions.Length; i++)
            {
                if(InnPositions[i] != null)
                {
                    rv.Add(InnPositions[i]);
                }
            }

            return rv;
        }
        public static int GetInnPosition(Villager v){
            for (int i = 0; i < InnPositions.Length; i++)
            {
                if(InnPositions[i]== v)
                {
                    return i;
                }
            }

            return -1;
        }
        public static bool RoomAtTheInn() {
            bool rv = false;
            for (int i = 0; i < InnPositions.Length; i++)
            {
                if (InnPositions[i] == null)
                {
                    rv = true;
                    break;
                }
            }

            return rv;
        }
        public static void AssignToPosition(Villager v, int i)
        {
            if(i > -1 && i < InnPositions.Length && InnPositions[i] == null)
            {
                InnPositions[i] = v;
            }
        }
        public static bool AddToInnQueue(Villager v)
        {
            bool rv = false;

            if (GetInnPosition(v) == -1)
            {
                for (int i = 0; i < InnPositions.Length; i++)
                {
                    if (InnPositions[i] == null)
                    {
                        InnPositions[i] = v;
                        rv = true;
                        break;
                    }
                }
            }

            return rv;
        }
        public static void RemoveFromInnQueue(Villager v)
        {
            for (int i = 0; i < InnPositions.Length; i++)
            {
                if (InnPositions[i] == v)
                {
                    InnPositions[i] = null;
                }
            }
        }
        #endregion

        #region Trigger Handling
        public static void AddTriggerObject(TriggerObject t)
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
        public static bool GamePaused() {
            bool rv = false;
            switch (CurrentScreen)
            {
                case GameScreenEnum.Combat:
                case GameScreenEnum.Info:
                    rv = true;
                    break;
                case GameScreenEnum.World:
                    if (GUIManager.IsTextWindowOpen()) { rv = true; }
                    else if(GUIManager.IsMainObjectOpen()) { rv = true; }
                    else if (GUIManager.IsMenuOpen()) { rv = true; }
                    break;
            }

            return rv;
        }
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

        public static void GoToCombatScreen(EmptyDelegate combatSwitch)
        {
            GUIManager.SetScreen(new CombatScreen(combatSwitch));
        }

        public static void GoToHUDScreen()
        {
            GUIManager.SetScreen(new HUDScreen());
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
    }
}
