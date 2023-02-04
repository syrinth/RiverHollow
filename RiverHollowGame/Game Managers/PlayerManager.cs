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
using RiverHollow.Characters.Lite;
using RiverHollow.Items;
using RiverHollow.Misc;

using static RiverHollow.Utilities.Enums;
using static RiverHollow.Game_Managers.SaveManager;
using System.Linq;

namespace RiverHollow.Game_Managers
{
    public static class PlayerManager
    {
        #region Properties
        public static bool Busy { get; private set; }

        public static double MaxStamina = Constants.PLAYER_STARTING_STAMINA;
        public static double Stamina = MaxStamina;
        private static string _currentMap;
        public static string CurrentMap
        {
            get { return _currentMap; }
            set {
                _currentMap = value;
                PlayerActor.CurrentMapName = _currentMap;
            }
        }

        public static ClassedCombatant PlayerCombatant;
        public static PlayerCharacter PlayerActor;

        public static int HitPoints => PlayerCombatant.CurrentHP;
        public static int MaxHitPoints  => PlayerCombatant.MaxHP;

        private static Dictionary<ObjectTypeEnum, List<int>> _diCrafting;

        private static DirectionEnum _eHorizontal = DirectionEnum.None;
        private static DirectionEnum _eVertical = DirectionEnum.None;

        public static Building PlayerHome => TownManager.GetBuildingByID(27);

        public static bool ReadyToSleep = false;

        private static List<Pet> _liPets;

        private static List<Mount> _liMounts;
        private static ClassedCombatant[] _arrParty;


        public static string Name;

        public static int TotalMoneyEarned { get; private set; } = 0;
        public static int Money { get; private set; } = 0;

        public static bool AllowMovement = true;

        public static WorldObject GrabbedObject;
        public static Vector2 MoveObjectToPosition;

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

        static RHTimer _pushTimer;
        static RHTimer _damageTimer;
        static FloatingText _hazardDamage;
        #endregion

        public static void Initialize()
        {
            _diCrafting = new Dictionary<ObjectTypeEnum, List<int>>
            {
                [ObjectTypeEnum.Building] = new List<int>(),
                [ObjectTypeEnum.Floor] = new List<int>(),
                [ObjectTypeEnum.Structure] = new List<int>(),
                [ObjectTypeEnum.Plant] = new List<int>(),
                [ObjectTypeEnum.Wall] = new List<int>()
            };

            Children = new List<Child>();
            _liPets = new List<Pet>();
            _liMounts = new List<Mount>();

            _liUniqueItemsBought = new List<int>();
            _diTools = new Dictionary<ToolEnum, Tool>();

            PlayerActor = new PlayerCharacter();
            PlayerCombatant = new ClassedCombatant();

            //Sets a default class so we can load and display the character to start
            PlayerCombatant.SetClass(DataManager.GetJobByIndex(1));

            _arrParty = new ClassedCombatant[4] { PlayerCombatant, null, null, null};
        }

        public static void NewPlayer()
        {
            MoveToSpawn();
            AddTesting();
        }

        public static void Update(GameTime gTime)
        {
            UpdateTool(gTime);
            
            if (Mouse.GetState().LeftButton == ButtonState.Released && PlayerActor.State == ActorStateEnum.Grab && AllowMovement)
            {
                ReleaseTile();
            }

            if (AllowMovement)
            {
                Vector2 newMovement = Vector2.Zero;
                MovementHelper(ref _eHorizontal, ref newMovement, true, Keys.A, DirectionEnum.Left, Keys.D, DirectionEnum.Right);
                MovementHelper(ref _eVertical, ref newMovement, false, Keys.W, DirectionEnum.Up, Keys.S, DirectionEnum.Down);

                //Only change facing if the current Facing is not a direction we're moving in
                if (PlayerActor.Facing != _eHorizontal && PlayerActor.Facing != _eVertical && PlayerActor.State != ActorStateEnum.Grab)
                {
                    PlayerActor.DetermineFacing(newMovement);
                }
                PlayerActor.DetermineAnimationState(newMovement);

                if(PlayerActor.State == ActorStateEnum.Grab && newMovement != Vector2.Zero) {
                    _pushTimer?.TickDown(gTime);
                    if (_pushTimer == null || _pushTimer.Finished())
                    {
                        GrabbedObject.InitiateMove(newMovement);
                    }
                }
                else if (newMovement != Vector2.Zero)
                {
                    bool impeded = false;
                    if (MapManager.CurrentMap.CheckForCollisions(PlayerActor, PlayerActor.CollisionBox.Location.ToVector2(), PlayerActor.CollisionBox, ref newMovement, ref impeded))
                    {
                        PlayerActor.Position += newMovement * (impeded ? Constants.IMPEDED_SPEED : 1f);
                    }
                }
            }
            else if (PlayerActor.State == ActorStateEnum.Grab && PlayerActor.MoveToLocation == Vector2.Zero && GrabbedObject != null)
            {
                FinishedMovingObject();
            }

            PlayerActor.Update(gTime);

            if (_damageTimer != null)
            {
                _damageTimer.TickDown(gTime);
                if (_damageTimer.Finished())
                {
                    _hazardDamage = null;
                    _damageTimer = null;
                    ClearDamagedMovement();
                }
            }

            if (_damageTimer != null && PlayerActor.MoveToLocation == Vector2.Zero)
            {
                ClearDamagedMovement();
            }

            _hazardDamage?.Update(gTime);
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
                _hazardDamage?.Draw(spriteBatch);
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
                string[] splitString = s.Split('-');
                InventoryManager.AddToInventory(int.Parse(splitString[0]), (splitString.Length > 1 ? int.Parse(splitString[1]) : 1), true, true);
            }
        }

        public static void SetPath(List<RHTile> list)
        {
            PlayerManager.AllowMovement = false;
            ReadyToSleep = true;
            PlayerActor.SetPath(list);
        }

        public static ClassedCombatant[] GetParty()
        {
            return _arrParty;
        }

        public static void AddToParty(ClassedCombatant c)
        {
            if (Array.Find(_arrParty, x => x == c) == null)
            {
                foreach (ClassedCombatant oldChar in _arrParty)
                {
                    if (oldChar != null && oldChar.StartPosition.Equals(c.StartPosition))
                    {
                        c.IncreaseStartPos();
                    }
                }

                _arrParty[Array.IndexOf(_arrParty, null)] = c;
            }
        }
        public static void RemoveFromParty(ClassedCombatant c)
        {
            if (Array.Find(_arrParty, x => x == c) != null)
            {
                _arrParty[Array.IndexOf(_arrParty, c)] = null;
            }
        }

        public static void HazardHarmParty(int damage, Point sourceCenter)
        {
            if(_damageTimer == null) {
                _damageTimer = new RHTimer(Constants.GAME_PLAYER_INVULN_TIME);

                foreach(ClassedCombatant act in GetParty())
                {
                    act?.DecreaseHealth(damage);
                }

                _hazardDamage = new FloatingText(PlayerActor.Position, PlayerActor.BodySprite.Width, damage.ToString(), Color.Red);
                AllowMovement = false;
                PlayerActor.SetMoveTo(PlayerActor.Position + (5 * (PlayerActor.CollisionCenter - sourceCenter).ToVector2()));
                PlayerActor.SpdMult = 2;
            }
        }
        public static void ClearDamagedMovement()
        {
            AllowMovement = true;
            PlayerActor.SpdMult = Constants.NORMAL_SPEED;
            PlayerActor.SetMoveTo(Vector2.Zero);
        }

        /// <summary>
        /// Iterates through all buildings owned by the Player, and 
        /// adds all of the workers to the total number.
        /// </summary>
        /// <returns>The total number of workers</returns>
        public static int GetTotalWorkers()
        {
            int rv = 0;
            //foreach(Building b in Buildings)
            //{
            //    rv += b.Workers.Count;
            //}

            return rv;
        }

        #region Crafting Dictionary
        public static List<int> GetCraftingList(ObjectTypeEnum e)
        {
            return _diCrafting[e];
        }
        public static bool  AddToCraftingDictionary(int id, bool displayAlert = true)
        {
            bool rv = false;

            ObjectTypeEnum e = DataManager.GetEnumByIDKey<ObjectTypeEnum>(id, "Type", DataType.WorldObject);
            switch (e)
            {
                case ObjectTypeEnum.Building:
                    e = ObjectTypeEnum.Building;
                    break;
                case ObjectTypeEnum.Floor:
                    e = ObjectTypeEnum.Floor;
                    break;
                case ObjectTypeEnum.Beehive:
                case ObjectTypeEnum.Buildable:
                case ObjectTypeEnum.Container:
                case ObjectTypeEnum.Garden:
                case ObjectTypeEnum.Structure:
                case ObjectTypeEnum.Wall:
                    e = ObjectTypeEnum.Structure;
                    break;
                case ObjectTypeEnum.Plant:
                    e = ObjectTypeEnum.Plant;
                    break;
                case ObjectTypeEnum.Wallpaper:
                    e = ObjectTypeEnum.Wallpaper;
                    break;
            }

            if (!_diCrafting[e].Contains(id))
            {
                rv = true;
                _diCrafting[e].Add(id);

                if (displayAlert)
                {
                    GUIManager.NewAlertIcon("New Blueprint Unlocked");
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
                GUIManager.NewAlertIcon("New " + (unlocks.Length > 1 ? "Blueprints" : "Blueprint") + " Unlocked");
            }
        }
        #endregion

        #region PlayerInRange

        public static bool PlayerInRange(Rectangle rect)
        {
            int hypotenuse = (int)Math.Sqrt(Constants.TILE_SIZE * Constants.TILE_SIZE + Constants.TILE_SIZE * Constants.TILE_SIZE);
            return PlayerInRange(rect, hypotenuse);
        }
        public static bool PlayerInRange(Rectangle rect, int range)
        {
            bool rv = false;
            if (PlayerInRange(new Vector2(rect.Center.X, rect.Top), range))
            {
                rv = true;
            }
            else if (PlayerInRange(new Vector2(rect.Center.X, rect.Bottom), range))
            {
                rv = true;
            }
            else if (PlayerInRange(new Vector2(rect.Left, rect.Center.Y), range))
            {
                rv = true;
            }
            else if (PlayerInRange(new Vector2(rect.Right, rect.Center.Y), range))
            {
                rv = true;
            }

            return rv;
        }

        public static bool PlayerInRange(Point centre)
        {
            int hypotenuse = (int)Math.Sqrt(Constants.TILE_SIZE* Constants.TILE_SIZE + Constants.TILE_SIZE* Constants.TILE_SIZE);
            return PlayerInRange(centre, hypotenuse);
        }
        public static bool PlayerInRange(Vector2 centre, int range)
        {
            return PlayerInRange(centre.ToPoint(), range);
        }
        public static bool PlayerInRange(Vector2 centre, int minRange, int maxRange)
        {
            return PlayerInRange(centre.ToPoint(), minRange, maxRange);
        }
        public static bool PlayerInRange(Point centre, int range)
        {
            bool rv = false;

            int distance = (int)Util.GetDistance(PlayerActor.CollisionCenter, centre);

            rv = distance <= range;

            return rv;
        }

        public static bool PlayerInRangeGetDist(Point centre, int range, ref int distance)
        {
            bool rv = false;

            Rectangle playerRect = PlayerActor.CollisionBox;
            distance = (int)Util.GetDistance(playerRect.Center, centre);

            rv = distance <= range;

            return rv;
        }

        public static bool PlayerInRange(Point centre, int minRange, int maxRange)
        {
            bool rv = false;

            Rectangle playerRect = PlayerActor.CollisionBox;
            int a = Math.Abs(playerRect.Center.X - centre.X);
            int b = Math.Abs(playerRect.Center.Y - centre.Y);
            int c = (int)Math.Sqrt(a * a + b * b);

            rv = c > minRange && c <= maxRange;

            return rv;
        }
        #endregion

        public static bool ExpendResources(Dictionary<int, int> requiredItems)
        {
            bool rv = false;
            if (requiredItems == null)
            {
                return false;
            }

            if (InventoryManager.HasSufficientItems(requiredItems))
            {
                rv = true;
                foreach (KeyValuePair<int, int> kvp in requiredItems)
                {
                    InventoryManager.RemoveItemsFromInventory(kvp.Key, kvp.Value);
                }
            }

            return rv;
        }

        public static void TakeMoney(int x)
        {
            Money -= x;
        }
        public static void AddMoney(int x)
        {
            Money += x;
            TotalMoneyEarned += x;
        }
        public static void SetMoney(int x)
        {
            Money = x;
        }
        public static void SetName(string x)
        {
            Name = x;
        }

        public static void SetClass(int x)
        {
            PlayerCombatant.SetClass(DataManager.GetJobByIndex(x));
            PlayerCombatant.SetName(Name);
        }

        public static bool DecreaseStamina(double x)
        {
            bool rv = false;
            if (Stamina >= x)
            {
                Stamina -= x;
                rv = true;
            }
            return rv;
        }

        public static void IncreaseStamina(double x)
        {
            if (Stamina + x <= MaxStamina)
            {
                Stamina += x;
            }
            else
            {
                Stamina = MaxStamina;
            }
        }

        public static void MoveToSpawn()
        {
            MapManager.CurrentMap = MapManager.Maps[Constants.PLAYER_HOME_NAME];
            CurrentMap = MapManager.CurrentMap.Name;
            PlayerActor.Position = Util.SnapToGrid(MapManager.Maps[CurrentMap].GetCharacterSpawn("PlayerSpawn"));
            PlayerActor.DetermineAnimationState(new Vector2(0, 1));
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

                    Child p = DataManager.CreateChild(10);
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

            foreach (Pet p in _liPets)
            {
                if (PlayerActor.ActivePet == p) { p.SpawnNearPlayer(); }
                else { p.SpawnInHome(); }
            }

            foreach(Child c in Children) { c.Rollover(); }

            PlayerMailbox.Rollover();

            MoveToSpawn();
        }

        public static void GetStamina(ref double curr, ref double max)
        {
            curr = Stamina;
            max = MaxStamina;
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

            foreach (Shop s in GameManager.DIShops.Values)
            {
                s.CheckForUniqueItems();
            }
        }
        public static bool AlreadyBoughtUniqueItem(int id)
        {
            return _liUniqueItemsBought.Contains(id);
        }

        public static PlayerData SaveData()
        {
            PlayerData data = new PlayerData()
            {
                name = Name,
                money = Money,
                totalMoneyEarned = TotalMoneyEarned,
                bodyTypeIndex = PlayerActor.BodyType,
                hairColor = PlayerActor.HairColor,
                hairIndex = PlayerActor.HairIndex,
                hat = Item.SaveData(PlayerActor.Hat),
                chest = Item.SaveData(PlayerActor.Chest),
                adventurerData = PlayerCombatant.SaveClassedCharData(),
                currentClass = PlayerCombatant.CharacterClass.ID,
                weddingCountdown = WeddingCountdown,
                babyCountdown = BabyCountdown,
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
            foreach (Pet p in _liPets)
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

            foreach (List<int> craftList in _diCrafting.Values)
            {
                data.CraftingList.AddRange(craftList);
            }

            for (int i = 0; i < _liUniqueItemsBought.Count; i++)
            {
                if (i == 0) { data.UniqueItemsBought = _liUniqueItemsBought[i].ToString(); }
                else { data.UniqueItemsBought += "|" + _liUniqueItemsBought[i].ToString(); }
            }

            return data;
        }

        public static void LoadData(PlayerData saveData)
        {
            MoveToSpawn();

            SetName(saveData.name);
            SetMoney(saveData.money);
            TotalMoneyEarned = saveData.totalMoneyEarned;

            BabyCountdown = saveData.babyCountdown;
            WeddingCountdown = saveData.weddingCountdown;

            PlayerActor.SetHairColor(saveData.hairColor);
            PlayerActor.SetHairType(saveData.hairIndex);

            SetClass(saveData.currentClass);
            PlayerCombatant.LoadClassedCharData(saveData.adventurerData);

            PlayerActor.SetClothes((Clothing)DataManager.GetItem(saveData.hat.itemID));
            PlayerActor.SetClothes((Clothing)DataManager.GetItem(saveData.chest.itemID));

            PlayerCombatant.IncreaseHealth(PlayerCombatant.MaxHP);
            PlayerActor.SetBodyType(saveData.bodyTypeIndex);

            for (int i = 0; i < BackpackLevel; i++)
            {
                for (int j = 0; j < InventoryManager.maxItemColumns; j++)
                {
                    int index = i * InventoryManager.maxItemColumns + j;
                    ItemData item = saveData.Items[index];
                    Item newItem = DataManager.GetItem(item.itemID, item.num);
 
                    newItem?.ApplyUniqueData(item.strData);
                    InventoryManager.AddItemToInventorySpot(newItem, i, j);
                }
            }

            foreach (int i in saveData.liPets)
            {
                Pet p = DataManager.CreatePet(i);
                if (p.ID == saveData.activePet) {
                    p.SpawnNearPlayer();
                    PlayerActor.SetPet(p);
                }
                else { p.SpawnInHome(); }
                AddPet(p);
            }

            foreach (int i in saveData.MountList)
            {
                Mount m = DataManager.CreateMount(i);
                AddMount(m);
            }

            foreach (ChildData data in saveData.ChildList)
            {
                Child m = DataManager.CreateChild(data.childID);
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

        public static List<RHTile> GetTiles()
        {
            List<RHTile> rv = new List<RHTile>
            {
                MapManager.CurrentMap.GetTileByPixelPosition(new Vector2(PlayerActor.CollisionBox.Left, PlayerActor.CollisionBox.Top)),
                MapManager.CurrentMap.GetTileByPixelPosition(new Vector2(PlayerActor.CollisionBox.Right, PlayerActor.CollisionBox.Top)),
                MapManager.CurrentMap.GetTileByPixelPosition(new Vector2(PlayerActor.CollisionBox.Left, PlayerActor.CollisionBox.Bottom)),
                MapManager.CurrentMap.GetTileByPixelPosition(new Vector2(PlayerActor.CollisionBox.Right, PlayerActor.CollisionBox.Bottom))
            };


            return rv;
        }
        public static bool StillMoving()
        {
            switch (PlayerActor.Facing)
            {
                case DirectionEnum.Down:
                    return InputManager.IsKeyDown(Keys.S);
                case DirectionEnum.Left:
                    return InputManager.IsKeyDown(Keys.A);
                case DirectionEnum.Right:
                    return InputManager.IsKeyDown(Keys.D);
                case DirectionEnum.Up:
                    return InputManager.IsKeyDown(Keys.W);

            }
            return false;
        }

        #region Grabbing Objects
        public static void GrabTile(RHTile t)
        {
            GrabbedObject = t.WorldObject;

            PlayerActor.Position = Util.SnapToGrid(PlayerActor.CollisionCenter.ToVector2());
            PlayerActor.DetermineFacing(t);
            PlayerActor.SetState(ActorStateEnum.Grab);
        }
        public static void ReleaseTile()
        {
            if (PlayerActor.State == ActorStateEnum.Grab)
            {
                _pushTimer?.Stop();

                //Safety for if we hit a map change while holding a tile
                if (GrabbedObject?.Tiles.Count == 0) { GrabbedObject.PlaceOnMap(MoveObjectToPosition, GrabbedObject.CurrentMap, true); }

                GrabbedObject = null;
                PlayerActor.SetState(ActorStateEnum.Walk);
                MoveObjectToPosition = Vector2.Zero;
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
            _pushTimer = new RHTimer(Constants.PUSH_COOLDOWN);
            AllowMovement = true;
            PlayerActor.SpdMult = Constants.NORMAL_SPEED;
            GrabbedObject.PlaceOnMap(MoveObjectToPosition, GrabbedObject.CurrentMap, true);
        }
        #endregion

        #region Tool Management
        public static Tool ToolInUse;
        public static int BackpackLevel => PlayerManager.RetrieveTool(ToolEnum.Backpack) != null ? PlayerManager.RetrieveTool(ToolEnum.Backpack).ToolLevel : 1;
        public static int LanternLevel => PlayerManager.RetrieveTool(ToolEnum.Lantern) != null ? PlayerManager.RetrieveTool(ToolEnum.Lantern).ToolLevel : 0;

        private static Dictionary<ToolEnum, Tool> _diTools;

        private static void UpdateTool(GameTime gTime)
        {
            if (ToolInUse != null)
            {
                ToolInUse.Update(gTime);

                RHTile target = MapManager.CurrentMap.TargetTile;

                if (target != null && ToolInUse.ToolAnimation.AnimationVerbFinished(VerbEnum.UseTool, PlayerManager.PlayerActor.Facing))
                {
                    if (PlayerManager.ToolIsAxe() || PlayerManager.ToolIsPick() || PlayerManager.ToolIsLantern())
                    {
                        target.DamageObject(PlayerManager.ToolInUse);
                    }
                    else if (PlayerManager.ToolIsScythe())
                    {
                        target.DamageObject(PlayerManager.ToolInUse);

                        if(PlayerManager.PlayerActor.Facing == DirectionEnum.Left || PlayerManager.PlayerActor.Facing == DirectionEnum.Right)
                        {
                            target.GetTileByDirection(DirectionEnum.Up).DamageObject(PlayerManager.ToolInUse);
                            target.GetTileByDirection(DirectionEnum.Down).DamageObject(PlayerManager.ToolInUse);
                        }
                        else
                        {
                            target.GetTileByDirection(DirectionEnum.Left).DamageObject(PlayerManager.ToolInUse);
                            target.GetTileByDirection(DirectionEnum.Right).DamageObject(PlayerManager.ToolInUse);
                        }
                    }
                    else if (PlayerManager.ToolIsWateringCan() && target.Flooring != null)
                    {
                        //target.Water(true);
                    }

                    target = null;
                    PlayerManager.FinishedWithTool();
                    PlayerActor.PlayAnimation(VerbEnum.Idle, DirectionEnum.Down);
                }
            }
        }

        /// <summary>
        /// Given a Tool, compare is against the current tools, if it's better than the
        /// old tool of it's type, replace the tool.
        /// 
        /// This should always be the case, since there should only ever be one instance
        /// of each tool level in the game, but safety.
        /// </summary>
        /// <param name="newTool">The prospective new tool</param>
        public static void AddTool(Tool newTool)
        {
            if(newTool != null && CompareTools(newTool))
            {
                _diTools[newTool.ToolType] = newTool;

                if(newTool.ToolType == ToolEnum.Backpack)
                {
                    InventoryManager.InitPlayerInventory();
                }
                if(newTool.ToolType == ToolEnum.Lantern)
                {
                    PlayerActor.NewLantern();
                }
            }
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
                ToolInUse.Position = new Vector2(PlayerActor.Position.X - Constants.TILE_SIZE, PlayerActor.Position.Y - (Constants.TILE_SIZE * 2));
                if (ToolInUse != null && !Busy)
                {
                    if (DecreaseStamina(ToolInUse.StaminaCost))
                    {
                        Busy = true;
                        AllowMovement = false;
                        PlayerActor.PlayAnimationVerb(VerbEnum.Idle);
                        ToolInUse.ToolAnimation.PlayAnimation(VerbEnum.UseTool, PlayerActor.Facing);
                    }
                    else
                    {
                        ToolInUse = null;
                    }
                }
            }
        }

        /// <summary>
        /// Called when we're finished with the Tool.
        /// </summary>
        public static void FinishedWithTool()
        {
            PlayerManager.ToolInUse = null;
            Busy = false;
            AllowMovement = true;
        }

        public static bool ToolIsAxe() { return ToolInUse.ToolType == ToolEnum.Axe; }
        public static bool ToolIsPick() { return ToolInUse.ToolType == ToolEnum.Pick; }
        public static bool ToolIsLantern() { return ToolInUse.ToolType == ToolEnum.Lantern; }
        public static bool ToolIsScythe() { return ToolInUse.ToolType == ToolEnum.Scythe; }
        public static bool ToolIsShovel() { return ToolInUse.ToolType == ToolEnum.Shovel; }
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
        public static void AddPet(Pet actor) { _liPets.Add(actor); }
        public static void AddMount(Mount actor) { _liMounts.Add(actor); }
        public static void SpawnMounts()
        {
            foreach(Mount actor in _liMounts)
            {
                actor.SpawnInHome();
            }
        }

        public static Mailbox PlayerMailbox;

        //private struct MovementDir
        //{
        //    public bool Moving;
        //    private DirectionEnum _eDirection;
        //    public DirectionEnum Direction
        //    {
        //        get { return _eDirection; }
        //        set
        //        {
        //            if (value == DirectionEnum.None) { Moving = false; }
        //            else { Moving = true; }
        //            _eDirection = value;
        //        }
        //    }
        //}
    }
}
