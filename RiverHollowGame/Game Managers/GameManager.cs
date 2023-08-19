using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using RiverHollow.Buildings;
using RiverHollow.Characters;
using RiverHollow.GUIComponents.GUIObjects;
using RiverHollow.GUIComponents.Screens;
using RiverHollow.Items;
using RiverHollow.Misc;
using RiverHollow.Utilities;
using RiverHollow.WorldObjects;
using System.Collections.Generic;
using static RiverHollow.Utilities.Enums;

namespace RiverHollow.Game_Managers
{
    public static class GameManager
    {
        #region Defined Values
        public static int CurrentScale = Constants.NORMAL_SCALE;
        public static int ScaledTileSize => (int)(Constants.TILE_SIZE * CurrentScale);
        public static int ScaledPixel => (int)CurrentScale;
        #endregion

        #region Managed Data Lists
        private static List<TriggerObject> _liTriggerObjects;
        private static List<Spirit> _liSpirits;
        public static Dictionary<int, Shop> DIShops;
        #endregion

        #region HoyKeys
        public static Keys HotkeyBuild { get; private set; } = Keys.B;
        public static Keys HotkeyCodex { get; private set; } = Keys.C;
        public static Keys HotkeyInventory { get; private set; } = Keys.I;
        public static Keys HotkeyOptions { get; private set; } = Keys.O;
        public static Keys HotkeyTasks { get; private set; } = Keys.T;
        #endregion

        #region Interaction Objects
        public static TalkingActor CurrentNPC { get; private set; }
        public static Item CurrentItem { get; private set; }
        public static Spirit CurrentSpirit { get; private set; }
        public static WorldObject CurrentWorldObject { get; private set; }
        public static Building CurrentBuilding { get; set; }

        public static Merchandise CurrentMerchandise => MapManager.CurrentMap.TheShop.SelectedMerchandise;
        #endregion

        #region Game State Values
        public static int MAX_NAME_LEN = 8;

        public static int TotalExperience = 0;

        public static bool HideMiniInventory = false;

        public static GameScreenEnum CurrentScreen;
        #endregion

        public static int HUDItemRow;
        public static int HUDItemCol;

        public static void Initialize()
        {
            _liSpirits = new List<Spirit>();
            _liTriggerObjects = new List<TriggerObject>();
            GameManager.HUDItemCol = 0;
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

        public static Point ScaleIt(Point val)
        {
            return new Point(ScaleIt(val.X), ScaleIt(val.Y));
        }

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
                rv = true;
                _heldWorldObject = obj;
                obj.SetPickupOffset();
            }

            return rv;
        }
        public static void EmptyHeldObject()
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
        private enum EnumBuildType { None, BuildMode, Edit };
        private static EnumBuildType _eBuildType;

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

        public static void GoToHUDScreen()
        {
            GUIManager.SetScreen(new HUDScreen());
            ShowMap();
        }

        public static bool InTownMode() { return TownModeBuild() || TownModeEdit(); }
        public static bool TownModeBuild() { return _eBuildType == EnumBuildType.BuildMode; }
        public static bool TownModeEdit() { return _eBuildType == EnumBuildType.Edit; }
        public static bool CanMoveObject() { return HeldObject == null && TownModeEdit(); }

        public static void EnterTownModeBuild(bool scry)
        {
            _eBuildType = EnumBuildType.BuildMode;
            GUIManager.CloseMainObject();
        }
        public static void EnterTownModeEdit() { _eBuildType = EnumBuildType.Edit; }

        public static void ExitTownMode() {
            if (InTownMode())
            {
                EmptyHeldObject();
                _eBuildType = EnumBuildType.None;

                foreach (Villager v in TownManager.DIVillagers.Values)
                {
                    v.DetermineValidSchedule();
                    v.RecalculatePath();
                }
            }
        }
        #endregion
    }
}
