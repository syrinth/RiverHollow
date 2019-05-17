using RiverHollow.Actors;
using RiverHollow.Misc;
using RiverHollow.WorldObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RiverHollow.Game_Managers
{
    public static class MissionManager
    {
        private static List<Mission> _liAvailableMissions;
        public static List<Mission> AvailableMissions => _liAvailableMissions;

        private static List<Mission> _liCurrentMissions;
        public static List<Mission> CurrentMissions => _liCurrentMissions;

        static Mission _selectedMission;          //The selected mission that is curently being acted on.
        public static Mission SelectedMission => _selectedMission;

        static List<WorldAdventurer> _liAdventurers;
        public static int CurrentPartySize => _liAdventurers.Count();

        /// <summary>
        /// Initializes the lists and generates a starting queue of missions.
        /// 
        /// Only called at the beginning of the game.
        /// </summary>
        public static void Load()
        {
            _liAvailableMissions = new List<Mission>();
            _liCurrentMissions = new List<Mission>();

            _liAdventurers = new List<WorldAdventurer>();

            GenerateNewMissions();
        }

        /// <summary>
        /// Call during rollover to add new missions to the queue
        /// </summary>
        public static void GenerateNewMissions()
        {
            for(int i = 0; i < 5; i++)
            {
                Thread.Sleep(994);
                _liAvailableMissions.Add(new Mission());
            }
        }

        /// <summary>
        /// Accepts the chosen mission by removing it from the available
        /// list, and adding it to the current list.
        /// </summary>
        public static void AcceptMission()
        {
            _liAvailableMissions.Remove(_selectedMission);
            _liCurrentMissions.Add(_selectedMission);
            _selectedMission.AssignToMission(_liAdventurers);
            CancelMissionSelection();
        }

        /// <summary>
        /// Cancels the chosen mission by removing it from the current
        /// list and then calling the CancelMission command to free
        /// and WorldAdventurers currently on the mission.
        /// </summary>
        /// <param name="m"></param>
        public static void AbortMission(Mission m)
        {
            _liCurrentMissions.Remove(m);
            m.CancelMission();
        }

        /// <summary>
        /// Called when the user selects a Mission from the Mission table.
        /// </summary>
        /// <param name="m">The mission to select</param>
        public static void SelectMission(Mission m)
        {
            _selectedMission = m;
        }

        /// <summary>
        /// Sets the _selectedMission to null and clears the temp adventurer list.
        /// Used after accepting or choosing to not accept a mission.
        /// </summary>
        public static void CancelMissionSelection()
        {
            _liAdventurers.Clear();
            _selectedMission = null;
        }

        /// <summary>
        /// Adds the given WorldAdventurer to the temp list of
        /// adventurers to assign to the mission.
        /// 
        /// Adventurers are not actually assigned until the mission is accepted
        /// </summary>
        /// <param name="adv">Adventurer to assign to the party.</param>
        public static void AddToParty(WorldAdventurer adv)
        {
            if (_liAdventurers.Count < _selectedMission.PartySize)
            {
                _liAdventurers.Add(adv);
            }
        }

        /// <summary>
        /// Removes the indicated WorldAdventurer from the temp list
        /// of adventurers to assign to the mission.
        /// </summary>
        /// <param name="adv">Adventurer to remove.</param>
        public static void RemoveFromParty(WorldAdventurer adv)
        {
            _liAdventurers.Remove(adv);
        }

        public static void Rollover()
        {
            List<Mission> toRemove = new List<Mission>();
            foreach (Mission m in _liCurrentMissions) {
                m.IncreaseDays();
                if(m.Completed())
                {
                    toRemove.Add(m);
                    PlayerManager.AddMoney(m.Money);
                    foreach(Item i in m.Items)
                    {
                        InventoryManager.AddItemToInventory(i);
                    }
                }
            }
            foreach (Mission m in toRemove)
            {
                _liCurrentMissions.Remove(m);
            }
            toRemove.Clear();
            foreach (Mission m in _liAvailableMissions)
            {
                m.IncreaseExpiry();
                if (m.DaysExpired == m.DaysToExpiry)
                {
                    toRemove.Add(m);
                }
            }
            foreach (Mission m in toRemove)
            {
                _liAvailableMissions.Remove(m);
            }
            toRemove.Clear();
        }

        public static bool PartyContains(WorldAdventurer adv)
        {
            return _liAdventurers.Contains(adv);
        }

        public static void ClearMissionAcceptance()
        {
            _selectedMission = null;
            _liAdventurers.Clear();
        }
    }

    public class Mission
    {
        string _sName;
        public string Name => _sName;
        int _iDaysToComplete;
        public int DaysToComplete => _iDaysToComplete;
        int _iDaysFinished;
        public int DaysFinished => _iDaysFinished;
        int _iDaysToExpiry;
        public int DaysToExpiry => _iDaysToExpiry;
        int _iDaysExpired;
        public int DaysExpired => _iDaysExpired;
        int _iMoney;
        public int Money => _iMoney;
        int _iRecLvl;
        public int ReqLevel => _iRecLvl;
        int _iPartySize;
        public int PartySize => _iPartySize;

        List<Item> _liItems;
        public List<Item> Items => _liItems;
        List<WorldAdventurer> _liAdventurers;
      
        public Mission()
        {
            _liItems = new List<Item>();
            _liAdventurers = new List<WorldAdventurer>();

            //Temp just for testing
            RHRandom r = new RHRandom();
            _sName = "test" + r.Next(0, 1000);
            _iDaysToComplete = 1;
            _iDaysFinished = 0;
            _iDaysToExpiry = 1;// r.Next(0, 3);
            _iMoney = r.Next(100, 500);
            _iRecLvl = 1;
            _iPartySize = r.Next(1, 4);

            int total = r.Next(0, 4);
            for (int i = 0; i < total; i++)
            {
                _liItems.Add(ObjectManager.GetItem(r.Next(9, 17)));
            }
        }

        /// <summary>
        /// Sets the given party to be the Missions party.
        /// Loops over each party member and assigns them to the mission.
        /// </summary>
        /// <param name="party"></param>
        public void AssignToMission(List<WorldAdventurer> party)
        {
            _liAdventurers = party;
            foreach(WorldAdventurer adv in party)
            {
                adv.AssignToMission(this);
            }
        }

        /// <summary>
        /// Called to cancel the mission and return WorldAdventurers
        /// to their original buildings.
        /// </summary>
        public void CancelMission()
        {
            foreach (WorldAdventurer adv in _liAdventurers)
            {
                adv.EndMission();
            }
        }

        public void IncreaseDays()
        {
            _iDaysFinished++;
        }

        public void IncreaseExpiry()
        {
            _iDaysExpired++;
        }

        public bool Completed() { return _iDaysFinished == _iDaysToComplete; }

        //public WorkerData SaveData()
        //{
        //    WorkerData workerData = new WorkerData
        //    {
        //        workerID = this.AdventurerID,
        //        advData = Combat.SaveData(),
        //        mood = this.Mood,
        //        name = this.Name,
        //        processedTime = this.ProcessedTime,
        //        currentItemID = (this._iCurrentlyMaking == null) ? -1 : this._iCurrentlyMaking,
        //        heldItemID = (this._heldItem == null) ? -1 : this._heldItem.ItemID,
        //        adventuring = Adventuring
        //    };

        //    return workerData;
        //}
        //public void LoadData(WorkerData data)
        //{
        //    _iAdventurerID = data.workerID;
        //    _iMood = data.mood;
        //    _sName = data.name;
        //    _dProcessedTime = data.processedTime;
        //    _iCurrentlyMaking = data.currentItemID;
        //    _heldItem = ObjectManager.GetItem(data.heldItemID);
        //    Adventuring = data.adventuring;

        //    SetCombat();
        //    Combat.LoadData(data.advData);

        //    if (_iCurrentlyMaking != null) { _spriteBody.SetCurrentAnimation(WActorBaseAnim.MakeItem); }
        //    if (Adventuring)
        //    {
        //        DrawIt = false;
        //        PlayerManager.AddToParty(Combat);
        //    }
        //}
    }
}
