using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.Buildings;
using RiverHollow.Characters;
using RiverHollow.WorldObjects;
using RiverHollow.Map_Handling;
using RiverHollow.Utilities;
using RiverHollow.Items;
using RiverHollow.Misc;

using RiverHollow.SpriteAnimations;
using RiverHollow.GUIComponents.Screens;
using RiverHollow.Items.Tools;

using static RiverHollow.Utilities.Enums;
using static RiverHollow.Game_Managers.SaveManager;
using RiverHollow.GUIComponents.GUIObjects;
using System.Linq;

namespace RiverHollow.Game_Managers
{
    public static class PlayerManager
    {
        #region Properties
        public static bool Busy { get; private set; }

        public static float CurrentEnergy;
        public static float CurrentMagic;
        public static bool CodexUnlocked = false;
        public static bool MagicUnlocked = false;

        #region Increases
        public static int EnergyIncrease { get; set; } = 0;
        public static float MaxEnergy()
        {
            return Constants.PLAYER_STARTING_ENERGY + EnergyIncrease * 20;
        }
        public static int HPIncrease { get; private set; } = 0;
        public static float MaxPlayerHP()
        {
            return Constants.PLAYER_STARTING_HP + (HPIncrease * 10);
        }
        public static int HPBonus() { return HPIncrease * 10; }
        public static int MagicIncrease { get; private set; } = 0;
        public static float MaxMagic()
        {
            return Constants.PLAYER_STARTING_MAGIC + (MagicIncrease * 10);
        }

        public static void IncreaseValue(PlayerResourceEnum e)
        {
            switch (e)
            {
                case PlayerResourceEnum.Energy:
                    EnergyIncrease++;
                    CurrentEnergy = MaxEnergy();
                    break;
                case PlayerResourceEnum.Health:
                    HPIncrease++;
                    PlayerActor.IncreaseHealth(PlayerActor.MaxHP);
                    break;
                case PlayerResourceEnum.Magic:
                    MagicIncrease++;
                    CurrentMagic = MaxMagic();
                    break;
            }

        }
        #endregion

        private static string _currentMap;
        public static string CurrentMap
        {
            get { return _currentMap; }
            set {
                _currentMap = value;
                PlayerActor.CurrentMapName = _currentMap;
            }
        }

        public static PlayerCharacter PlayerActor;
        

        private static List<int> _liCrafting;

        private static Dictionary<CosmeticSlotEnum, List<KeyValuePair<int, bool>>> _diCosmetics;

        private static DirectionEnum _eHorizontal = DirectionEnum.None;
        private static DirectionEnum _eVertical = DirectionEnum.None;

        public static Building PlayerHome => TownManager.GetBuildingByID(27);

        public static bool ReadyToSleep = false;

        public static List<Pet> Pets { get; private set; }

        private static List<Mount> _liMounts;

        public static string Name;
        public static int Money { get; private set; } = 0;

        public static bool AllowMovement = true;

        public static WorldObject GrabbedObject;
        public static Point MoveObjectToPosition;
        private static VectorBuffer _vbMovement;

        public static int WeddingCountdown { get; set; } = 0;
        public static Villager Spouse { get; set; }
        public static List<Child> Children { get; set; }

        public static int BabyCountdown { get; set; }

        private static List<int> _liUniqueItemsBought;

        private static ExpectingChildEnum _eChildStatus;
        public static ExpectingChildEnum ChildStatus {
            get => _eChildStatus;
            set {
                _eChildStatus = value;
                BabyCountdown = _eChildStatus == ExpectingChildEnum.None ? 0 : 7;
            }
        }

        public static List<Rectangle> AdjacencyRects => new List<Rectangle>{GetAdjacencyRectangle(DirectionEnum.Down),
            GetAdjacencyRectangle(DirectionEnum.Right),
            GetAdjacencyRectangle(DirectionEnum.Up),
            GetAdjacencyRectangle(DirectionEnum.Left)
        };
        public static Rectangle GetAdjacencyRectangle(DirectionEnum e)
        {
            switch (e)
            {
                case DirectionEnum.Down:
                    return new Rectangle(PlayerActor.CollisionBox.Left, PlayerActor.CollisionBox.Bottom, PlayerActor.CollisionBox.Width, Constants.PLAYER_ADJACENCY_SIZE);
                case DirectionEnum.Right:
                    return new Rectangle(PlayerActor.CollisionBox.Right, PlayerActor.CollisionBox.Top, Constants.PLAYER_ADJACENCY_SIZE, PlayerActor.CollisionBox.Height);
                case DirectionEnum.Up:
                    return new Rectangle(PlayerActor.CollisionBox.Left, PlayerActor.CollisionBox.Top - Constants.PLAYER_ADJACENCY_SIZE, PlayerActor.CollisionBox.Width, Constants.PLAYER_ADJACENCY_SIZE);
                case DirectionEnum.Left:
                    return new Rectangle(PlayerActor.CollisionBox.Left - Constants.PLAYER_ADJACENCY_SIZE, PlayerActor.CollisionBox.Top, Constants.PLAYER_ADJACENCY_SIZE, PlayerActor.CollisionBox.Height);
            }

            return Rectangle.Empty;
        }

        public static MapItem ObtainedItem;

        static RHTimer _timer;
        #endregion

        public static void Initialize()
        {
            _vbMovement = new VectorBuffer();
            _liCrafting = new List<int>();

            Children = new List<Child>();
            Pets = new List<Pet>();
            _liMounts = new List<Mount>();

            _liUniqueItemsBought = new List<int>();
            _diTools = new Dictionary<ToolEnum, Tool>();
            InitializeCosmeticDictionary();
            

            PlayerActor = new PlayerCharacter();
            CurrentEnergy = MaxEnergy();
            CurrentMagic = MaxMagic();

            FinishedWithTool();
        }

        public static void NewPlayer()
        {
            MoveToSpawn();
            AddTesting();
        }

        public static void Update(GameTime gTime)
        {
            ToolInUse?.Update(gTime);

            if (PlayerActor.HasKnockbackVelocity() || Defeated() || (Mouse.GetState().RightButton == ButtonState.Released && PlayerActor.State == ActorStateEnum.Grab && AllowMovement))
            {
                ReleaseTile();
            }
            
            if (!GameManager.GamePaused() && AllowMovement && PlayerActor.HasHP)
            {
                if(ObtainedItem != null)
                {
                    ObtainedItem = null;
                    PlayerActor.SetFacing(DirectionEnum.Down);
                }

                Vector2 newMovement = Vector2.Zero;
                MovementHelper(ref _eHorizontal, ref newMovement, true, Keys.A, DirectionEnum.Left, Keys.D, DirectionEnum.Right);
                MovementHelper(ref _eVertical, ref newMovement, false, Keys.W, DirectionEnum.Up, Keys.S, DirectionEnum.Down);

                //Only change facing if the current Facing is not a direction we're moving in
                bool facingNewDirection = PlayerActor.Facing == _eHorizontal || PlayerActor.Facing == _eVertical;
                if (!PlayerActor.HasKnockbackVelocity() && !facingNewDirection && PlayerActor.State != ActorStateEnum.Grab)
                {
                    PlayerActor.DetermineFacing(newMovement.ToPoint());
                    PlayerActor.ClearMovementBuffer();
                }

                PlayerActor.DetermineAnimationState(newMovement);

                if (PlayerActor.State == ActorStateEnum.Grab && newMovement != Vector2.Zero)
                {
                    if (_timer == null || _timer.TickDown(gTime))
                    {
                        GrabbedObject?.InitiateMove(newMovement);
                    }
                }
                else if (newMovement != Vector2.Zero)
                {
                    bool impeded = false;
                    if (MapManager.CurrentMap.CheckForCollisions(PlayerActor, ref newMovement, ref impeded))
                    {
                        newMovement *= impeded ? Constants.IMPEDED_SPEED : 1f;
                        PlayerActor.MoveActor(newMovement, newMovement != Vector2.Zero);
                    }

                    ObtainedItem = null;
                }
            }
            else if (PlayerActor.State == ActorStateEnum.Grab && !PlayerActor.HasMovement() && GrabbedObject != null)
            {
                FinishedMovingObject();
            }

            PlayerActor.Update(gTime);

            if (Defeated())
            {
                PlayerActor.DamageTimerEnd();
                if (_timer == null)
                {
                    _timer = new RHTimer(2);
                }
                else if(RHTimer.TimerCheck(_timer, gTime))
                {
                    _timer = null;
                    GUIManager.SetScreen(new DefeatedScreen());
                }
            }
        }

        private static void MovementHelper(ref DirectionEnum mainEnum, ref Vector2 v, bool horizontal, Keys key1, DirectionEnum dir1, Keys key2, DirectionEnum dir2)
        {
            if (mainEnum == DirectionEnum.None)
            {
                if (InputManager.IsKeyDown(key1))
                {
                    mainEnum = dir1;
                }
                else if (InputManager.IsKeyDown(key2))
                {
                    mainEnum = dir2;
                }
            }
            else
            {
                if (!InputManager.IsKeyDown(key1) && mainEnum == dir1)
                {
                    mainEnum = DirectionEnum.None;
                }
                if (!InputManager.IsKeyDown(key2) && mainEnum == dir2)
                {
                    mainEnum = DirectionEnum.None;
                }
            }

            if (mainEnum != DirectionEnum.None)
            {
                float value = 0;
                if (mainEnum == dir1) { value = -PlayerActor.BuffedSpeed; }
                else if (mainEnum == dir2) { value = PlayerActor.BuffedSpeed; }

                if (horizontal) { v.X = value; }
                else { v.Y = value; }
            }
        }

        public static void Draw(SpriteBatch spriteBatch)
        {
            if (_currentMap == MapManager.CurrentMap.Name)
            {
                PlayerActor.Draw(spriteBatch, true);
                ToolInUse?.DrawToolAnimation(spriteBatch);
                ObtainedItem?.Draw(spriteBatch, PlayerActor.BodySprite.LayerDepth * 2);
            }
        }

        public static void DrawLight(SpriteBatch spriteBatch)
        {
            PlayerActor.DrawLight(spriteBatch);
        }

        public static void AddTesting()
        {
            Dictionary<string, string> diTesting = DataManager.Config[0];
            string[] splitItemValues = Util.FindParams(diTesting["ItemID"]);
            foreach (string s in splitItemValues)
            {
                string[] splitString = Util.FindArguments(s);
                for (int i = 0; i < (splitString.Length > 1 ? int.Parse(splitString[1]) : 1); i++)
                {
                    InventoryManager.AddToInventory(int.Parse(splitString[0]), 1, true, true);
                }
            }
        }

        public static void SetPath(List<RHTile> list)
        {
            PlayerManager.AllowMovement = false;
            ReadyToSleep = true;
            PlayerActor.SetPath(list);
        }

        public static void FaceCursor()
        {
            PlayerActor.DetermineFacing(MapManager.CurrentMap.GetTileByPixelPosition(GUICursor.GetWorldMousePosition()));
        }

        #region Cosmetics Dictionary
        public static void InitializeCosmeticDictionary()
        {
            _diCosmetics = new Dictionary<CosmeticSlotEnum, List<KeyValuePair<int, bool>>>();
            foreach (CosmeticSlotEnum e in GetEnumArray<CosmeticSlotEnum>())
            {
                _diCosmetics[e] = new List<KeyValuePair<int, bool>>();
            }

            foreach (var kvp in DataManager.Cosmetics)
            {
                var enumType = DataManager.GetEnumByIDKey<CosmeticSlotEnum>(kvp.Key, "Type", DataType.Cosmetic);
                var startUnlocked = DataManager.GetBoolByIDKey(kvp.Key, "Default", DataType.Cosmetic);

                Util.AddToListDictionary(ref _diCosmetics, enumType, new KeyValuePair<int, bool>(kvp.Key, startUnlocked));
            }
        }
        
        public static void AddToCosmeticDictionary(int[] unlocks)
        {
            bool displayAlert = false;
            for (int i = 0; i < unlocks.Length; i++)
            {
                displayAlert = AddToCosmeticDictionary(unlocks[i], false);
            }

            if (displayAlert)
            {
                string str = unlocks.Length == 1 ? Constants.STR_ALERT_BLUEPRINT : Constants.STR_ALERT_BLUEPRINTS;
                GUIManager.NewInfoAlertIcon(str);
            }
        }

        public static bool AddToCosmeticDictionary(int id, bool displayAlert = true)
        {
            bool rv = false;
            var list = _diCosmetics[DataManager.GetEnumByIDKey<CosmeticSlotEnum>(id, "Type", DataType.Cosmetic)];

            if (!list.Any(x => x.Key == id))
            {
                rv = true;

                list.Add(new KeyValuePair<int, bool>(id, true));

                if (displayAlert)
                {
                    GUIManager.NewInfoAlertIcon(Constants.STR_ALERT_BLUEPRINT);
                }
            }

            return rv;
        }

        public static Cosmetic RandomCosmetic(CosmeticSlotEnum e)
        {
            var cosmetics = _diCosmetics[e];
            return DataManager.GetCosmetic(Util.GetRandomItem(cosmetics.Where(x => x.Value).Select(x => x.Key).ToList()));
        }

        public static List<KeyValuePair<int, bool>> GetCosmetics(CosmeticSlotEnum e)
        {
            return new List<KeyValuePair<int, bool>>(_diCosmetics[e]);
        }
        #endregion

        #region Crafting Dictionary
        public static List<int> GetCraftingList()
        {
            return _liCrafting;
        }

        public static List<int> GetNonUniqueByTypes(List<BuildableEnum> enumTypes)
        {
            var rv = GetCraftingList().FindAll(x => !((Buildable)DataManager.CreateWorldObjectByID(x)).Unique);
            rv = rv.Where(x => enumTypes.Contains(DataManager.GetEnumByIDKey<BuildableEnum>(x, "Subtype", DataType.WorldObject))).ToList();

            return rv;
        }

        public static bool AddToCraftingDictionary(int id, bool displayAlert = true)
        {
            bool rv = false;

            if (!_liCrafting.Contains(id))
            {
                rv = true;
                _liCrafting.Add(id);

                if (displayAlert)
                {
                    GUIManager.NewInfoAlertIcon(Constants.STR_ALERT_BLUEPRINT);
                }
            }

            return rv;
        }
        public static void AddToCraftingDictionary(int[] unlocks)
        {
            bool displayAlert = false;
            for (int i = 0; i < unlocks.Length; i++)
            {
                displayAlert = AddToCraftingDictionary(unlocks[i], false);
            }

            if (displayAlert)
            {
                string str = unlocks.Length == 1 ? Constants.STR_ALERT_BLUEPRINT : Constants.STR_ALERT_BLUEPRINTS;
                GUIManager.NewInfoAlertIcon(str);
            }
        }
        #endregion

        #region PlayerInRange
        public static bool InRangeOfPlayer(Rectangle testRectangle)
        {
            DirectionEnum ignore = DirectionEnum.None;
            return InRangeOfPlayer(testRectangle, ref ignore);
        }
        public static bool InRangeOfPlayer(Rectangle testRectangle, ref DirectionEnum facing)
        {
            var playerCenter = PlayerActor.CollisionCenter;
            var playerCollision = PlayerActor.CollisionBox;

            bool rv = false;
            List<Rectangle> list = AdjacencyRects;

            for (int i = 0; i < list.Count; i++)
            {
                var r = list[i];
                if (r.Intersects(testRectangle))// && (Util.CenterInRange(playerCenter, testRectangle) || Util.EdgeInRange(playerCollision, testRectangle)))
                {
                    rv = true;
                    facing = (DirectionEnum)(i + 1);
                    break;
                }
            }

            return rv;
        }
        public static bool PlayerInRange(Point centre, int range)
        {
            int distance = (int)Util.GetDistance(PlayerActor.CollisionCenter, centre);
            return distance <= range;
        }
        public static bool PlayerInRangeGetDist(Point centre, int range, ref int distance)
        {
            bool rv = false;

            Rectangle playerRect = PlayerActor.CollisionBox;
            distance = (int)Util.GetDistance(playerRect.Center, centre);

            rv = distance <= range;

            return rv;
        }
        #endregion

        public static void TakeMoney(int x)
        {
            Money -= x;
        }
        public static void AddMoney(int x)
        {
            Money += x;
        }
        public static void SetMoney(int x)
        {
            Money = x;
        }
        public static void SetName(string x)
        {
            Name = x;
        }

        public static bool Defeated()
        {
            AnimatedSprite spr = PlayerActor.GetSprites()[0];
            return PlayerActor != null && !PlayerActor.HasHP && spr.AnimationFinished(AnimationEnum.KO);
        }

        private static void NewDayRecovery()
        {
            CurrentEnergy = MaxEnergy();
            CurrentMagic = MaxMagic();
            PlayerActor.RefillHealth();
        }

        public static bool HasEnergy(float x)
        {
            bool rv = false;
            if (CurrentEnergy >= x)
            {
                rv = true;
            }
            return rv;
        }
        public static void ToolLoseEnergy()
        {
            if (ToolInUse != null)
            {
                LoseEnergy(ToolInUse.EnergyCost);
            }
        }

        public static void LoseEnergy(float x)
        {
            if (CurrentEnergy >= x)
            {
                CurrentEnergy -= x;
            }
        }
        public static void RecoverEnergy(float x)
        {
            if (CurrentEnergy + x <= MaxEnergy())
            {
                CurrentEnergy += x;
            }
            else
            {
                CurrentEnergy = MaxEnergy();
            }
        }
        public static void RefillEnergy()
        {
            CurrentEnergy = MaxEnergy();
        }

        public static bool LoseMagic(float x)
        {
            bool rv = false;
            if (CurrentMagic >= x)
            {
                CurrentMagic -= x;
                rv = true;
            }
            return rv;
        }
        public static void RecoverMagic(float x)
        {
            if (CurrentMagic + x <= MaxMagic())
            {
                CurrentMagic += x;
            }
            else
            {
                CurrentMagic = MaxMagic();
            }
        }
        public static void RefillMagic()
        {
            CurrentMagic = MaxMagic();
        }

        public static void MoveToSpawn()
        {
            MapManager.CurrentMap = MapManager.Maps[Constants.PLAYER_HOME_NAME];
            CurrentMap = MapManager.CurrentMap.Name;
            PlayerActor.SetPosition(Util.SnapToGrid(MapManager.Maps[CurrentMap].GetCharacterSpawn("PlayerSpawn")));
            PlayerActor.SetFacing(DirectionEnum.Down);
        }

        public static void Rollover()
        {
            if(BabyCountdown > 0)
            {
                BabyCountdown--;
                if (BabyCountdown == 0)
                {
                    ChildStatus = ExpectingChildEnum.None;
                    if (PlayerActor.Pregnant) { PlayerActor.Pregnant = false; }
                    else if (Spouse.Pregnant) { Spouse.Pregnant = false; }

                    Child p = DataManager.CreateActor<Child>(10);
                    p.SpawnNearPlayer();
                    Children.Add(p);
                }
            }

            if (WeddingCountdown > 0)
            {
                WeddingCountdown--;
                if (WeddingCountdown == 0)
                {
                    Spouse.RelationshipState = RelationShipStatusEnum.Married;
                }
            }

            PlayerActor.ActivePet?.SpawnNearPlayer();

            foreach(Child c in Children) { c.Rollover(); }

            NewDayRecovery();

            MoveToSpawn();
        }

        public static void GetEnergy(ref float curr, ref float max)
        {
            curr = CurrentEnergy;
            max = MaxEnergy();
        }

        public static void GetHP(ref float curr, ref float max)
        {
            curr = PlayerActor.CurrentHP;
            max = PlayerActor.MaxHP;
        }

        public static void GetMagic(ref float curr, ref float max)
        {
            curr = CurrentMagic;
            max = MaxMagic();
        }

        public static void DetermineBabyAcquisition()
        {
            if (PlayerActor.CanBecomePregnant == Spouse.CanBecomePregnant) { PlayerManager.ChildStatus = ExpectingChildEnum.Adoption; }
            else
            {
                PlayerManager.ChildStatus = ExpectingChildEnum.Pregnant;

                if (PlayerActor.CanBecomePregnant) { PlayerActor.Pregnant = true; }
                else if (Spouse.CanBecomePregnant) { Spouse.Pregnant = true; }
            }
        }

        public static void AddUniqueItemToList(int id)
        {
            Util.AddUniquelyToList(ref _liUniqueItemsBought, id);
        }
        public static bool AlreadyBoughtUniqueItem(int id)
        {
            return _liUniqueItemsBought.Contains(id);
        }

        public static List<RHTile> GetTiles()
        {
            List<RHTile> rv = new List<RHTile>
            {
                MapManager.CurrentMap.GetTileByPixelPosition(new Point(PlayerActor.CollisionBox.Left, PlayerActor.CollisionBox.Top)),
                MapManager.CurrentMap.GetTileByPixelPosition(new Point(PlayerActor.CollisionBox.Right, PlayerActor.CollisionBox.Top)),
                MapManager.CurrentMap.GetTileByPixelPosition(new Point(PlayerActor.CollisionBox.Left, PlayerActor.CollisionBox.Bottom)),
                MapManager.CurrentMap.GetTileByPixelPosition(new Point(PlayerActor.CollisionBox.Right, PlayerActor.CollisionBox.Bottom))
            };


            return rv;
        }

        #region Grabbing Objects
        public static void MoveGrabbedObject(Vector2 dir)
        {
            GrabbedObject.MoveObject(_vbMovement.AddMovement(dir));
        }
        public static void TryGrab(RHTile t, DirectionEnum facing)
        {
            var pBox = PlayerActor.CollisionBox;
            var grabBox = t.WorldObject != null ? t.WorldObject.BaseRectangle : t.CollisionBox;

            if (Math.Abs(pBox.Left - grabBox.Right) <= Constants.GRAB_REACH ||
                Math.Abs(pBox.Right - grabBox.Left) <= Constants.GRAB_REACH ||
                Math.Abs(pBox.Top - grabBox.Bottom) <= Constants.GRAB_REACH ||
                Math.Abs(pBox.Bottom - grabBox.Top) <= Constants.GRAB_REACH)
            {

                if (t.WorldObject != null)
                {
                    GrabbedObject = t.WorldObject;
                }

                PlayerActor.SetFacing(facing);
                PlayerActor.SetState(ActorStateEnum.Grab);
            }
        }
        public static void ReleaseTile()
        {
            if (PlayerActor.State == ActorStateEnum.Grab)
            {
                _timer = null;

                //Safety for if we hit a map change while holding a tile
                if (GrabbedObject?.Tiles().Count == 0) { GrabbedObject.PlaceOnMap(MoveObjectToPosition, GrabbedObject.CurrentMap, true); }

                GrabbedObject = null;
                PlayerActor.SetState(ActorStateEnum.Walk);
                MoveObjectToPosition = Point.Zero;
                _vbMovement.Clear();
            }
        }
        public static void HandleGrabMovement(RHTile nextObjectTile, RHTile nextPlayerTile)
        {
            MoveObjectToPosition = nextObjectTile.Position;

            AllowMovement = false;
            PlayerActor.SetMoveTo(nextPlayerTile.Position);
            PlayerActor.SpdMult = Constants.PUSH_SPEED;
            GrabbedObject.RemoveSelfFromTiles();
        }
        private static void FinishedMovingObject()
        {
            _timer = new RHTimer(Constants.PUSH_COOLDOWN);
            AllowMovement = true;
            PlayerActor.SpdMult = Constants.NORMAL_SPEED;
            GrabbedObject.PlaceOnMap(MoveObjectToPosition, GrabbedObject.CurrentMap, true);
        }
        #endregion

        #region Tool Management
        public static Tool ToolInUse;
        public static int BackpackLevel => RetrieveTool(ToolEnum.Backpack) != null ? RetrieveTool(ToolEnum.Backpack).ToolLevel : 2;
        public static int LanternLevel => RetrieveTool(ToolEnum.Lantern) != null ? RetrieveTool(ToolEnum.Lantern).ToolLevel : 0;

        private static Dictionary<ToolEnum, Tool> _diTools;

        /// <summary>
        /// Given a Tool, compare is against the current tools, if it's better than the
        /// old tool of it's type, replace the tool.
        /// 
        /// This should always be the case, since there should only ever be one instance
        /// of each tool level in the game, but safety.
        /// </summary>
        /// <param name="newTool">The prospective new tool</param>
        public static bool AddTool(Tool newTool)
        {
            bool rv = newTool != null && newTool.IsAutomatic;

            if(newTool != null && CompareTools(newTool))
            {
                _diTools[newTool.ToolType] = newTool;

                if(newTool.ToolType == ToolEnum.Backpack)
                {
                    InventoryManager.InitPlayerInventory(true);
                }
                if(newTool.ToolType == ToolEnum.Lantern)
                {
                    PlayerActor.SetLightSource();
                }
            }

            return rv;
        }

        /// <summary>
        /// Performs a comparison between the new tool and the original tool.
        /// 
        /// Returns True when there is no tool in that slot, or the newTools level is higher than the original's
        /// </summary>
        /// <param name="newTool">The prospective new tool</param>
        /// <returns>True if the newTool is better or there is no original tool.</returns>
        private static bool CompareTools(Tool newTool)
        {
            return (RetrieveTool(newTool.ToolType) == null) || newTool.ToolLevel > RetrieveTool(newTool.ToolType).ToolLevel;
        }

        /// <summary>
        /// Given a ToolEnum type, determine which managed Tool to return
        /// </summary>
        /// <param name="toolType">The type of tool to retrieve</param>
        /// <returns>The managed Tool held by the PlayerManager</returns>
        public static Tool RetrieveTool(ToolEnum toolType)
        {
            Tool rv = null;

            if (_diTools.ContainsKey(toolType)) {
                rv = _diTools[toolType];
            }

            return rv;
        }

        public static void SetTool(Tool t)
        {
            if (t != null && ToolInUse == null)
            {
                ToolInUse = t;
                ToolInUse.Position = new Point(PlayerActor.CollisionBoxLocation.X - Constants.TILE_SIZE, PlayerActor.CollisionBoxLocation.Y - (Constants.TILE_SIZE * 2));
                if (ToolInUse != null && !Busy)
                {
                    if (HasEnergy(ToolInUse.EnergyCost))
                    {
                        Busy = true;
                        AllowMovement = false;
                        PlayerActor.PlayAnimationVerb(VerbEnum.Idle);
                        ToolInUse.ToolSprite.PlayAnimation(PlayerActor.Facing);

                        if (ToolInUse.ToolType == ToolEnum.FishingRod)
                        {
                            FishingManager.BeginFishing((FishingRod)ToolInUse, MapManager.CurrentMap.TargetTile.Position);
                        }
                    }
                    else
                    {
                        ToolInUse = null;
                        GUIManager.NewWarningAlertIcon(Constants.STR_ALERT_ENERGY);
                    }
                }
            }
        }

        /// <summary>
        /// Called when we're finished with the Tool.
        /// </summary>
        public static void FinishedWithTool()
        {
            ToolInUse = null;
            Busy = false;
            AllowMovement = true;
        }

        public static bool ToolIsAxe() { return ToolInUse.ToolType == ToolEnum.Axe; }
        public static bool ToolIsPick() { return ToolInUse.ToolType == ToolEnum.Pick; }
        public static bool ToolIsScythe() { return ToolInUse.ToolType == ToolEnum.Scythe; }
        public static bool ToolIsHoe() { return ToolInUse.ToolType == ToolEnum.Hoe; }
        public static bool ToolIsWateringCan() { return ToolInUse.ToolType == ToolEnum.WateringCan; }

        public static ToolData SaveToolData()
        {
            ToolData d = new ToolData()
            {
                pickID = RetrieveTool(ToolEnum.Pick) != null ? RetrieveTool(ToolEnum.Pick).ID : -1,
                axeID = RetrieveTool(ToolEnum.Axe) != null ? RetrieveTool(ToolEnum.Axe).ID : -1,
                scytheID = RetrieveTool(ToolEnum.Scythe) != null ? RetrieveTool(ToolEnum.Scythe).ID : -1,
                wateringCanID = RetrieveTool(ToolEnum.WateringCan) != null ? RetrieveTool(ToolEnum.WateringCan).ID : -1,
                backpackID = RetrieveTool(ToolEnum.Backpack) != null ? RetrieveTool(ToolEnum.Backpack).ID : -1,
                lanternID = RetrieveTool(ToolEnum.Lantern) != null ? RetrieveTool(ToolEnum.Lantern).ID : -1,
            };

            return d;
        }
        public static void LoadToolData(ToolData d)
        {
            AddTool((Tool)DataManager.GetItem(d.pickID));
            AddTool((Tool)DataManager.GetItem(d.axeID));
            AddTool((Tool)DataManager.GetItem(d.scytheID));
            AddTool((Tool)DataManager.GetItem(d.backpackID));
            AddTool((Tool)DataManager.GetItem(d.wateringCanID));
            AddTool((Tool)DataManager.GetItem(d.lanternID));
        }
        #endregion


        public static void AddChild(Child actor) { Children.Add(actor); }
        public static void AddPet(Pet actor) {
            if (!Pets.Any(x => x.ID == actor.ID)){
                Pets.Add(actor);
            }
        }
        public static void AddMount(Mount actor) { _liMounts.Add(actor); }
        public static void SpawnMounts()
        {
            foreach(Mount actor in _liMounts)
            {
                actor.SpawnInHome();
            }
        }


        public static PlayerData SaveData()
        {
            PlayerData data = new PlayerData()
            {
                name = Name,
                money = Money,
                bodyTypeIndex = PlayerActor.BodyType,
                hair = PlayerActor.SaveCosmeticData(CosmeticSlotEnum.Hair),
                eyes = PlayerActor.SaveCosmeticData(CosmeticSlotEnum.Eyes),
                hat = PlayerActor.SaveCosmeticData(CosmeticSlotEnum.Head),
                shirt = PlayerActor.SaveCosmeticData(CosmeticSlotEnum.Body),
                pants = PlayerActor.SaveCosmeticData(CosmeticSlotEnum.Legs),
                feet = PlayerActor.SaveCosmeticData(CosmeticSlotEnum.Feet),
                weddingCountdown = WeddingCountdown,
                babyCountdown = BabyCountdown,
                hpIncreases = HPIncrease,
                energyIncreases = EnergyIncrease,
                magicIncreases = MagicIncrease,
                codex = CodexUnlocked,
                hasMagic = MagicUnlocked,
                Items = new List<ItemData>(),
                liPets = new List<int>(),
                MountList = new List<int>(),
                ChildList = new List<ChildData>(),
                CraftingList = new List<int>()
            };

            // Initialize the new data values.
            foreach (Item i in InventoryManager.PlayerInventory)
            {
                ItemData itemData = Item.SaveData(i);
                data.Items.Add(itemData);
            }

            data.activePet = PlayerManager.PlayerActor.ActivePet == null ? -1 : PlayerManager.PlayerActor.ActivePet.ID;
            foreach (Pet p in Pets)
            {
                data.liPets.Add(p.ID);
            }

            foreach (Mount m in _liMounts)
            {
                data.MountList.Add(m.ID);
            }

            foreach (Child c in Children)
            {
                data.ChildList.Add(c.SaveData());
            }

            data.CraftingList.AddRange(_liCrafting);

            for (int i = 0; i < _liUniqueItemsBought.Count; i++)
            {
                if (i == 0) { data.UniqueItemsBought = _liUniqueItemsBought[i].ToString(); }
                else { data.UniqueItemsBought += "/" + _liUniqueItemsBought[i].ToString(); }
            }

            return data;
        }

        public static void LoadData(PlayerData saveData)
        {
            SetName(saveData.name);
            SetMoney(saveData.money);

            BabyCountdown = saveData.babyCountdown;
            WeddingCountdown = saveData.weddingCountdown;

            HPIncrease = saveData.hpIncreases;
            EnergyIncrease = saveData.energyIncreases;
            MagicIncrease = saveData.magicIncreases;
            MagicUnlocked = saveData.hasMagic;
            
            CodexUnlocked = saveData.codex;

            NewDayRecovery();

            PlayerActor.LoadCosmeticData(saveData.hair);
            PlayerActor.LoadCosmeticData(saveData.eyes);
            PlayerActor.LoadCosmeticData(saveData.hat);
            PlayerActor.LoadCosmeticData(saveData.shirt);
            PlayerActor.LoadCosmeticData(saveData.pants);
            PlayerActor.LoadCosmeticData(saveData.feet);
            PlayerActor.LinkSprites();

            PlayerActor.SetBodyType(saveData.bodyTypeIndex);

            MoveToSpawn();

            for (int i = 0; i < BackpackLevel; i++)
            {
                for (int j = 0; j < InventoryManager.maxItemColumns; j++)
                {
                    int index = Util.ListIndexFromMultiArray(i, j, InventoryManager.maxItemColumns);
                    ItemData item = saveData.Items[index];
                    Item newItem = DataManager.GetItem(item.itemID, item.num);

                    newItem?.ApplyUniqueData(item.strData);
                    InventoryManager.AddItemToInventorySpot(newItem, i, j);
                }
            }

            foreach (int i in saveData.liPets)
            {
                Pet p = DataManager.CreateActor<Pet>(i);
                if (p.ID == saveData.activePet)
                {
                    p.SpawnNearPlayer();
                    PlayerActor.SetPet(p);
                }
                AddPet(p);
            }

            foreach (int i in saveData.MountList)
            {
                Mount m = DataManager.CreateActor<Mount>(i);
                AddMount(m);
            }

            foreach (ChildData data in saveData.ChildList)
            {
                Child m = DataManager.CreateActor<Child>(data.childID);
                m.SpawnNearPlayer();
                AddChild(m);
            }

            foreach (int i in saveData.CraftingList)
            {
                AddToCraftingDictionary(i, false);
            }

            if (saveData.UniqueItemsBought != null)
            {
                string[] uniqueSplit = Util.FindParams(saveData.UniqueItemsBought);
                for (int i = 0; i < uniqueSplit.Length; i++)
                {
                    _liUniqueItemsBought.Add(int.Parse(uniqueSplit[i]));
                }
            }
        }
        internal static bool HasActiveItem()
        {
            return PlayerActor.HasActiveItem();
        }
        internal static void SetActiveItem(Item i)
        {
            PlayerActor.SetActiveItem(i);
        }
    }
}
