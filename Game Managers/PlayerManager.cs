﻿using RiverHollow.WorldObjects;
using RiverHollow.Tile_Engine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using RiverHollow.Actors;
using Microsoft.Xna.Framework.Input;
using RiverHollow.Actors.CombatStuff;
using RiverHollow.Misc;

using static RiverHollow.Game_Managers.GameManager;
using RiverHollow.Buildings;
using RiverHollow.Characters;
using static RiverHollow.Game_Managers.SaveManager;

namespace RiverHollow.Game_Managers
{
    public static class PlayerManager
    {
        #region Properties
        private static bool _bBusy;
        public static bool Busy => _bBusy;
        public static Tool ToolInUse;

        private static List<Quest> _questLog;
        public static List<Quest> QuestLog  => _questLog;
        
        public static int Stamina = 50;
        public static int MaxStamina = 50;
        public static int _iBuildingID = -1;
        private static List<int> _canMake;
        public static List<int> CanMake { get => _canMake; }
        private static string _currentMap;
        public static string CurrentMap
        {
            get { return _currentMap; }
            set {
                _currentMap = value;
                World.CurrentMapName = _currentMap;
            }
        }

        public static PlayerCharacter World;

        public static int HitPoints => World.CurrentHP;
        public static int MaxHitPoints  => World.MaxHP;

        private static List<Building> _liBuildings;
        public static List<Building> Buildings { get => _liBuildings; }

        public static bool ReadyToSleep = false;

        private static List<ClassedCombatant> _liParty;

        public static string Name;
        public static string ManorName;

        private static int _iMoney = 2000;
        public static int Money => _iMoney;

        private static int _iMonsterEnergy = 0;
        public static int MonsterEnergy => _iMonsterEnergy;

        private static int _iMonsterEnergyQueue;

        public static bool AllowMovement = true;

        private static EligibleNPC _marriedTo;
        #endregion

        public static void Initialize()
        {
            _liParty = new List<ClassedCombatant>();
            _questLog = new List<Quest>();
            World = new PlayerCharacter();
            _liParty.Add(World);
            _liBuildings = new List<Building>();
            _canMake = new List<int>();
        }

        public static void NewPlayer()
        {
            World.Position = new Vector2(200, 200);
            _canMake.Add(190);

            CurrentMap = MapManager.CurrentMap.Name;
            World.Position = Util.SnapToGrid(MapManager.Maps[CurrentMap].GetCharacterSpawn("PlayerSpawn"));

            AddTesting();
        }

        public static void AddTesting()
        {
            InventoryManager.AddToInventory(0, 990);
            InventoryManager.AddToInventory(2, 990);
            InventoryManager.AddToInventory(240, 99);
            InventoryManager.AddToInventory(20);
            InventoryManager.AddToInventory(21);
            InventoryManager.AddToInventory(22);
            InventoryManager.AddToInventory(23);
            InventoryManager.AddToInventory(25);
            InventoryManager.AddToInventory(61, 5);

            AddToQuestLog(new Quest("Gathering Wood", Quest.QuestType.Fetch, "Getwood, dumbass", 1, null, DataManager.GetItem(2)));
        }

        public static void SetPath(List<RHTile> list)
        {
            PlayerManager.AllowMovement = false;
            ReadyToSleep = true;
            World.SetPath(list);
        }

        public static void Update(GameTime gTime)
        {
            Vector2 moveDir = Vector2.Zero;

            AddByIncrement(ref _iMonsterEnergyQueue, ref _iMonsterEnergy);

            if (AllowMovement)
            {
                if (ToolInUse == null)
                {
                    KeyboardState ks = Keyboard.GetState();
                    if (ks.IsKeyDown(Keys.W))
                    {
                        moveDir += new Vector2(0, -World.Speed);
                    }
                    else if (ks.IsKeyDown(Keys.S))
                    {
                        moveDir += new Vector2(0, World.Speed);
                    }

                    if (ks.IsKeyDown(Keys.A))
                    {
                        moveDir += new Vector2(-World.Speed, 0);
                    }
                    else if (ks.IsKeyDown(Keys.D))
                    {
                        moveDir += new Vector2(World.Speed, 0);
                    }

                    World.DetermineFacing(moveDir);

                    if (moveDir.Length() != 0)
                    {
                        Rectangle testRectX = new Rectangle((int)World.CollisionBox.X + (int)moveDir.X, (int)World.CollisionBox.Y, World.CollisionBox.Width, World.CollisionBox.Height);
                        Rectangle testRectY = new Rectangle((int)World.CollisionBox.X, (int)World.CollisionBox.Y + (int)moveDir.Y, World.CollisionBox.Width, World.CollisionBox.Height);

                        if (MapManager.CurrentMap.CheckForCollisions(World, testRectX, testRectY, ref moveDir))
                        {
                            //Might be technically correct but FEELS wrong
                            //moveDir.Normalize();
                            //moveDir *= World.Speed;
                            World.MoveBy((int)moveDir.X, (int)moveDir.Y);
                        }
                    }
                }
                else
                {
                    ToolInUse.Update(gTime);

                    bool finished = !World.BodySprite.CurrentAnimation.StartsWith("Tool");

                    RHTile target = MapManager.CurrentMap.TargetTile;
                    //UseTool
                    if (target != null && finished)
                    {
                        if (PlayerManager.ToolIsAxe() || PlayerManager.ToolIsPick() || PlayerManager.ToolIsLantern())
                        {
                            target.DamageObject(PlayerManager.ToolInUse);
                        }
                        else if (PlayerManager.ToolIsShovel() && target.CanDig())
                        {
                            target.Dig();
                            MapManager.CurrentMap.TilledTiles.Add(target);
                        }
                        else if (PlayerManager.ToolIsWateringCan() && target.Flooring != null && target.Flooring.IsEarth())
                        {
                            target.Water(true);
                        }

                        target = null;
                        PlayerManager.UnsetTool();
                        World.PlayAnimation(VerbEnum.Idle, DirectionEnum.Down);
                    }
                }
            }
            World.Update(gTime);
        }

        public static void AddByIncrement(ref int queue, ref int addTo)
        {
            if (queue > 0)
            {
                int cap = 5;
                int toGive = 0;

                if (queue <= cap) { toGive = _iMonsterEnergyQueue; }
                else { toGive = cap; }

                queue -= toGive;
                addTo += toGive;
            }
        }

        public static void Draw(SpriteBatch spriteBatch)
        {
            if (_currentMap == MapManager.CurrentMap.Name)
            {
                World.Draw(spriteBatch, true);
                if (ToolInUse != null)
                {
                    ToolInUse.Draw(spriteBatch);
                }
            }
        }

        public static List<ClassedCombatant> GetParty()
        {
            return _liParty;
        }
        public static void AddToParty(ClassedCombatant c)
        {
            foreach (ClassedCombatant oldChar in _liParty)
            {
                if (oldChar.StartPosition.Equals(c.StartPosition))
                {
                    c.IncreaseStartPos();
                }
            }

            if (!_liParty.Contains(c))
            {
                _liParty.Add(c);
            }
        }
        public static void RemoveFromParty(ClassedCombatant c)
        {
            if (_liParty.Contains(c))
            {
                _liParty.Remove(c);
            }
        }

        /// <summary>
        /// Iterates through all buildings owned by the Player, and 
        /// adds all of the workers to the total number.
        /// </summary>
        /// <returns>The total number of workers</returns>
        public static int GetTotalWorkers()
        {
            int rv = 0;
            foreach(Building b in _liBuildings)
            {
                rv += b.Workers.Count;
            }

            return rv;
        }

        /// <summary>
        /// Iterates through every building, searching for the adventurer 
        /// with the matching PersonalID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static Adventurer GetWorkerByPersonalID(int id)
        {
            Adventurer adv = null;

            foreach (Building b in _liBuildings)
            {
                foreach(Adventurer w in b.Workers)
                {
                    if(w.PersonalID == id)
                    {
                        adv = w;
                        break;
                    }
                }
            }

            return adv;
        }
        public static int GetDuelistsInParty()
        {
            return GetClassInParty(5);
        }
        public static int GetKnightsInParty()
        {
            return GetClassInParty(6);
        }
        public static int GetRoguesInParty()
        {
            return GetClassInParty(7);
        }
        public static int GetBardsInParty()
        {
            return GetClassInParty(8);
        }
        public static int GetClassInParty(int classID)
        {
            int rv = 0;
            foreach (ClassedCombatant c in _liParty)
            {
                if (c.CharacterClass.ID == classID)
                {
                    rv++;
                }
            }

            return rv;
        }

        //Random quests should not generate a quest with the same goal as a pre-existing quest
        public static void AddToQuestLog(Quest q)
        {
            foreach(Item i in InventoryManager.PlayerInventory)
            {
                if (i != null) { q.AttemptProgress(i); }
            }
            q.SpawnQuestMobs();
            _questLog.Add(q);
        }
        public static void AdvanceQuestProgress(Monster m)
        {
            foreach (Quest q in _questLog)
            {
                if (q.AttemptProgress(m))
                {
                    break;
                }
            }
        }
        public static void AdvanceQuestProgress(Item i)
        {
            foreach(Quest q in _questLog)
            {
                if (q.AttemptProgress(i))
                {
                    break;
                }
            }
        }
        public static void RemoveQuestProgress(Item i)
        {
            foreach (Quest q in _questLog)
            {
                if (q.RemoveProgress(i))
                {
                    break;
                }
            }
        }

        public static bool SetTool(Tool t, Point mouse)
        {
            bool rv = false;
            PlayerManager.World.PlayAnimation(CombatManager.InCombat ? VerbEnum.Walk : VerbEnum.Idle);

            if (t != null && ToolInUse == null)
            {
                rv = true;
                ToolInUse = t;
                ToolInUse.Position = World.BodyPosition;
                if (ToolInUse != null && !_bBusy)
                {
                    _bBusy = true;
                    if (DecreaseStamina(ToolInUse.StaminaCost))
                    {
                        ToolInUse.ToolAnimation.IsAnimating = true;
                        PlayerManager.World.PlayAnimation(VerbEnum.UseTool, DirectionEnum.Down);
                    }
                    else
                    {
                        ToolInUse = null;
                    }
                }
            }

            return rv;
        }
        public static void UnsetTool()
        {
            PlayerManager.ToolInUse = null;
            _bBusy = false;
        }

        public static void AddBuilding(Building b)
        {
            _liBuildings.Add(b);
        }
        public static void RemoveBuilding(Building b)
        {
            _liBuildings.Remove(b);
        }
        public static int GetNewBuildingID()
        {
            return _liBuildings.Count +1 ;
        }

        public static bool PlayerInRange(Rectangle rect)
        {
            return PlayerInRange(rect, TileSize);
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
            return PlayerInRange(centre, TileSize*2);
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

            Rectangle playerRect = World.CollisionBox;
            int a = Math.Abs(playerRect.Center.X - centre.X);
            int b = Math.Abs(playerRect.Center.Y - centre.Y);
            int c = (int)Math.Sqrt(a * a + b * b);

            rv = c <= range;

            return rv;
        }

        public static bool PlayerInRangeGetDist(Point centre, int range, ref int distance)
        {
            bool rv = false;

            Rectangle playerRect = World.GetRectangle();
            int a = Math.Abs(playerRect.Center.X - centre.X);
            int b = Math.Abs(playerRect.Center.Y - centre.Y);
            distance = (int)Math.Sqrt(a * a + b * b);

            rv = distance <= range;

            return rv;
        }

        public static bool PlayerInRange(Point centre, int minRange, int maxRange)
        {
            bool rv = false;

            Rectangle playerRect = World.GetRectangle();
            int a = Math.Abs(playerRect.Center.X - centre.X);
            int b = Math.Abs(playerRect.Center.Y - centre.Y);
            int c = (int)Math.Sqrt(a*a + b*b);

            rv = c > minRange && c <= maxRange;

            return rv;
        }

        public static void TakeMoney(int x)
        {
            _iMoney -= x;
        }
        public static void AddMoney(int x)
        {
            _iMoney += x;
        }
        public static void SetMoney(int x)
        {
            _iMoney = x;
        }
        public static void SetName(string x)
        {
            Name = x;
            World.SetName(x);
        }
        public static void SetManorName(string x)
        {
            ManorName = x;
        }
        public static void SetClass(int x)
        {
            CharacterClass combatClass = DataManager.GetClassByIndex(x);
            World.SetClass(combatClass);
        }

        public static bool DecreaseStamina(int x)
        {
            bool rv = false;
            if (Stamina >= x)
            {
                Stamina -= x;
                rv = true;
            }
            return rv;
        }

        public static void IncreaseStamina(int x)
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

        public static void Rollover()
        {
            if (GameManager.AutoDisband)
            {
                _liParty.Clear();
                _liParty.Add(World);
            }
        }

        public static void GetStamina(ref int curr, ref int max)
        {
            curr = Stamina;
            max = MaxStamina;
        }

        public static PlayerData SaveData()
        {
            PlayerData d = new PlayerData()
            {
                name = PlayerManager.Name,
                money = PlayerManager.Money,
                bodyTypeIndex = PlayerManager.World.BodyType,
                hairColor = PlayerManager.World.HairColor,
                hairIndex = PlayerManager.World.HairIndex,
                hat = Item.SaveData(World.Hat),
                chest = Item.SaveData(World.Shirt),
                adventurerData = World.SaveClassedCharData(),
                currentClass = World.CharacterClass.ID,
                Items = new List<ItemData>()
            };

            return d;
        }

        public static void LoadData(PlayerData data)
        {
            SetName(data.name);
            SetMoney(data.money);
            World.SetHairColor(data.hairColor);
            World.SetHairType(data.hairIndex);

            SetClass(data.currentClass);
            World.LoadClassedCharData(data.adventurerData);

            World.SetClothes((Clothes)DataManager.GetItem(data.hat.itemID));
            World.SetClothes((Clothes)DataManager.GetItem(data.chest.itemID));

            World.SetBodyType(data.bodyTypeIndex);

            for (int i = 0; i < InventoryManager.maxItemRows; i++)
            {
                for (int j = 0; j < InventoryManager.maxItemColumns; j++)
                {
                    int index = i * InventoryManager.maxItemColumns + j;
                    ItemData item = data.Items[index];
                    Item newItem = DataManager.GetItem(item.itemID, item.num);
 
                    if (newItem != null) { newItem.ApplyUniqueData(item.strData); }
                    InventoryManager.AddItemToInventorySpot(newItem, i, j);
                }
            }
        }

        /// <summary>
        /// Adds the indicated amount of energy to the Player
        /// </summary>
        public static void AddMonsterEnergyToQueue(int i)
        {
            _iMonsterEnergyQueue += i;
        }

        /// <summary>
        /// Removes the indicated amount of energy from the Player if they have it
        /// </summary>
        public static bool RemoveMonsterEnergy(int i)
        {
            bool rv = false;
            if (_iMonsterEnergy >= i)
            {
                rv = true;
                _iMonsterEnergy += i;
            }
            return rv;
        }

        public static bool ToolIsAxe() { return ToolInUse.ToolType == ToolEnum.Axe; }
        public static bool ToolIsPick() { return ToolInUse.ToolType == ToolEnum.Pick; }
        public static bool ToolIsLantern() { return ToolInUse.ToolType == ToolEnum.Lantern; }
        public static bool ToolIsShovel() { return ToolInUse.ToolType == ToolEnum.Shovel; }
        public static bool ToolIsWateringCan() { return ToolInUse.ToolType == ToolEnum.WateringCan; }
    }
}
