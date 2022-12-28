//using System.Collections.Generic;
//using System.Linq;
//using System.Threading;
//using RiverHollow.Characters;
//using RiverHollow.CombatStuff;
//using RiverHollow.WorldObjects;
//using RiverHollow.Utilities;

//using static RiverHollow.Game_Managers.SaveManager;
//using RiverHollow.Items;

//namespace RiverHollow.Game_Managers
//{
//    public static class MissionManager
//    {
//        private static List<Mission> _liAvailableMissions;
//        public static List<Mission> AvailableMissions => _liAvailableMissions;

//        private static List<Mission> _liCurrentMissions;
//        public static List<Mission> CurrentMissions => _liCurrentMissions;

//        static Mission _selectedMission;          //The selected mission that is curently being acted on.
//        public static Mission SelectedMission => _selectedMission;

//        static List<Adventurer> _liAdventurers;
//        public static int CurrentPartySize => _liAdventurers.Count();

//        /// <summary>
//        /// Initializes the lists and generates a starting queue of missions.
//        /// 
//        /// Only called at the beginning of the game.
//        /// </summary>
//        public static void Load()
//        {
//            _liAvailableMissions = new List<Mission>();
//            _liCurrentMissions = new List<Mission>();

//            _liAdventurers = new List<Adventurer>();

//            GenerateNewMissions();
//        }

//        /// <summary>
//        /// Call during rollover to add new missions to the queue
//        /// </summary>
//        public static void GenerateNewMissions(int num = 5)
//        {
//            for(int i = 0; i < num; i++)
//            {
//                Thread.Sleep(500);
//                _liAvailableMissions.Add(new Mission());
//            }
//        }

//        /// <summary>
//        /// Accepts the chosen mission by removing it from the available
//        /// list, and adding it to the current list.
//        /// </summary>
//        public static void AcceptMission()
//        {
//            _liAvailableMissions.Remove(_selectedMission);
//            _liCurrentMissions.Add(_selectedMission);
//            _selectedMission.AssignToMission(_liAdventurers);
//            CancelMissionSelection();
//        }

//        /// <summary>
//        /// Cancels the chosen mission by removing it from the current
//        /// list and then calling the CancelMission command to free
//        /// and WorldAdventurers currently on the mission.
//        /// </summary>
//        /// <param name="m"></param>
//        public static void AbortMission(Mission m)
//        {
//            _liCurrentMissions.Remove(m);
//            m.CancelMission();
//        }

//        /// <summary>
//        /// Called when the user selects a Mission from the Mission table.
//        /// </summary>
//        /// <param name="m">The mission to select</param>
//        public static void SelectMission(Mission m)
//        {
//            _selectedMission = m;
//        }

//        /// <summary>
//        /// Sets the _selectedMission to null and clears the temp adventurer list.
//        /// Used after accepting or choosing to not accept a mission.
//        /// </summary>
//        public static void CancelMissionSelection()
//        {
//            _liAdventurers.Clear();
//            _selectedMission = null;
//        }

//        /// <summary>
//        /// Adds the given WorldAdventurer to the temp list of
//        /// adventurers to assign to the mission.
//        /// 
//        /// Adventurers are not actually assigned until the mission is accepted
//        /// </summary>
//        /// <param name="adv">Adventurer to assign to the party.</param>
//        public static void AddToParty(Adventurer adv)
//        {
//            if (_liAdventurers.Count < _selectedMission.PartySize)
//            {
//                _liAdventurers.Add(adv);
//            }
//        }

//        /// <summary>
//        /// Removes the indicated WorldAdventurer from the temp list
//        /// of adventurers to assign to the mission.
//        /// </summary>
//        /// <param name="adv">Adventurer to remove.</param>
//        public static void RemoveFromParty(Adventurer adv)
//        {
//            _liAdventurers.Remove(adv);
//        }

//        /// <summary>
//        /// Called on Rollover to advance current missions and update
//        /// expiry dates.
//        /// 
//        /// After missions expire, replace them with that many missions
//        /// </summary>
//        public static void Rollover()
//        {
//            List<Mission> toRemove = new List<Mission>();

//            //Advance the completion dates of the CurrentMissions and
//            //if the mission is completed, reward the player.
//            foreach (Mission m in _liCurrentMissions) {
//                m.IncreaseDays();
//                if(m.Completed())
//                {
//                    toRemove.Add(m);
//                    PlayerManager.AddMoney(m.Money);
//                    foreach(Item i in m.Items)
//                    {
//                        InventoryManager.AddToInventory(i);
//                    }
//                }
//            }
//            foreach (Mission m in toRemove)
//            {
//                _liCurrentMissions.Remove(m);
//            }
//            toRemove.Clear();

//            //Advance the expiry dates on available missions and
//            //remove any that have expired
//            foreach (Mission m in _liAvailableMissions)
//            {
//                m.IncreaseExpiry();
//                if (m.DaysExpired == m.TotalDaysToExpire)
//                {
//                    toRemove.Add(m);
//                }
//            }
//            foreach (Mission m in toRemove)
//            {
//                _liAvailableMissions.Remove(m);
//            }

//            GenerateNewMissions(toRemove.Count());
//            toRemove.Clear();
//        }

//        /// <summary>
//        /// Used to wipe out the temp data for Mission selection
//        /// </summary>
//        public static void ClearMissionAcceptance()
//        {
//            _selectedMission = null;
//            _liAdventurers.Clear();
//        }

//        /// <summary>
//        /// Returns true if the CurrentPartySize has enough WorldAdventurers, and
//        /// if the party contains the required class.
//        /// </summary>
//        /// <returns></returns>
//        public static bool MissionReady()
//        {
//            return (CurrentPartySize == SelectedMission.PartySize) && PartyContainsClass();
//        }

//        /// <summary>
//        /// Returns true if the temp party contains the required class or if there is no required class
//        /// </summary>
//        public static bool PartyContainsClass()
//        {
//            bool rv = false;
//            if (SelectedMission.CharClass != null)
//            {
//                rv = _liAdventurers.Find(adv => adv.CharacterClass.ID == SelectedMission.CharClass.ID) != null;
//            }
//            else { rv = true; }

//            return rv;
//        }
//    }

//    public class Mission
//    {
//        string _sName;
//        public string Name => _sName;
//        int _iDaysToComplete;
//        public int DaysToComplete => _iDaysToComplete;
//        int _iDaysFinished;
//        public int DaysFinished => _iDaysFinished;
//        int _iTotalDaysToExpire;
//        public int TotalDaysToExpire => _iTotalDaysToExpire;
//        int _iDaysExpired;
//        public int DaysExpired => _iDaysExpired;
//        int _iMoney;
//        public int Money => _iMoney;
//        int _iReqLevel;
//        public int ReqLevel => _iReqLevel;
//        int _iPartySize;
//        public int PartySize => _iPartySize;
//        CharacterClass _charClass;
//        public CharacterClass CharClass => _charClass;

//        List<Item> _liItems;
//        public List<Item> Items => _liItems;
//        List<Adventurer> _liAdventurers;

//        List<string> _liMissionNames;
      
//        public Mission()
//        {
//            _liMissionNames = new List<string>(new string[] { "Dungeon Delve", "Rescue Mission", "Defeat the Monster" });

//            _liItems = new List<Item>();
//            _liAdventurers = new List<Adventurer>();

//            //Temp just for testing
//            RHRandom r = RHRandom.Instance();
//            _sName = _liMissionNames[r.Next(0, _liMissionNames.Count - 1)];
//            _iDaysToComplete = r.Next(2, 5);
//            _iTotalDaysToExpire = r.Next(2, 7);
//            _iReqLevel = r.Next(1, PlayerManager.PlayerActor.ClassLevel + 1);
//            _iPartySize = r.Next(1, PlayerManager.GetTotalWorkers());

//            int missionLvl = _iPartySize * _iReqLevel * _iDaysToComplete;

//            _iMoney = missionLvl * 10;
//            int total = r.Next(0, 4);
//            for (int i = 0; i < total; i++)
//            {
//                _liItems.Add(DataManager.GetItem(r.Next(9, 17)));
//            }

//            _iDaysFinished = 0;
//            _iDaysExpired = 0;

//            //Only one fourth of missions have a required class
//            if (r.Next(1, 4) == 1)
//            {
//                _charClass = DataManager.GetClassByIndex(r.Next(1, DataManager.GetClassCount() - 1));
//            }
//        }

//        /// <summary>
//        /// Sets the given party to be the Missions party.
//        /// Loops over each party member and assigns them to the mission.
//        /// </summary>
//        /// <param name="party"></param>
//        public void AssignToMission(List<Adventurer> party)
//        {
//            _liAdventurers.AddRange(party);
//            foreach(Adventurer adv in party)
//            {
//                adv.AssignToMission(this);
//            }
//        }

//        /// <summary>
//        /// Called to cancel the mission and return WorldAdventurers
//        /// to their original buildings.
//        /// </summary>
//        public void CancelMission()
//        {
//            foreach (Adventurer adv in _liAdventurers)
//            {
//                adv.EndMission();
//            }
//        }

//        /// <summary>
//        /// Increments the days that have been spent on this Mission
//        /// </summary>
//        public void IncreaseDays()
//        {
//            _iDaysFinished++;
//        }

//        /// <summary>
//        /// Increases the number of days the mission has been waiting
//        /// to pass before it expires and it removed.
//        /// </summary>
//        public void IncreaseExpiry()
//        {
//            _iDaysExpired++;
//        }

//        /// <summary>
//        /// Returns if the Mission number of days finished is equal to the 
//        /// days required to complete.
//        /// </summary>
//        public bool Completed() { return _iDaysFinished == _iDaysToComplete; }

//        public MissionData SaveData()
//        {
//            MissionData data = new MissionData
//            {
//                Name = this.Name,
//                Money = this.Money,
//                DaysExpired = this.DaysExpired,
//                DaysFinished = this.DaysFinished,
//                DaysToComplete = this.DaysToComplete,
//                TotalDaysToExpire = this.TotalDaysToExpire,
//                PartySize = this.PartySize,
//                ReqClassID = (this.CharClass == null ? 0 : this.CharClass.ID),
//                ReqLevel = this.ReqLevel
//            };

//            data.Items = new List<ItemData>();
//            foreach (Item i in this.Items)
//            {
//                ItemData itemData = Item.SaveData(i);
//                data.Items.Add(itemData);
//            }

//            data.ListAdventurerIDs = new List<int>();
//            foreach(Adventurer adv in _liAdventurers)
//            {
//                data.ListAdventurerIDs.Add(adv.PersonalID);
//            }

//            return data;
//        }
//        public void LoadData(MissionData data)
//        {
//            _sName = data.Name;
//            _iMoney = data.Money;
//            _iDaysExpired = data.DaysExpired;
//            _iDaysFinished = data.DaysFinished;
//            _iDaysToComplete = data.DaysToComplete;
//            _iTotalDaysToExpire = data.TotalDaysToExpire;
//            _iPartySize = data.PartySize;
//            _charClass = (data.ReqClassID != 0 ? DataManager.GetClassByIndex(data.ReqClassID) : null);
//            _iReqLevel = data.ReqLevel;

//            _liItems.Clear();
//            foreach (ItemData id in data.Items)
//            {
//                _liItems.Add(DataManager.GetItem(id.ID, id.num));
//            }

//            List<Adventurer> advList = new List<Adventurer>();
//            foreach (int personalID in data.ListAdventurerIDs)
//            {
//                Adventurer adv = PlayerManager.GetWorkerByPersonalID(personalID);
//                if(adv != null)
//                {
//                    advList.Add(adv);
//                }
//            }

//            AssignToMission(advList);
//        }
//    //}
//}
