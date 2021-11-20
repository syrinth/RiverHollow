using RiverHollow.Game_Managers;
using RiverHollow.GUIComponents.Screens;
using RiverHollow.Misc;
using RiverHollow.Utilities;
using RiverHollow.WorldObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static RiverHollow.Game_Managers.GameManager;
using static RiverHollow.Game_Managers.SaveManager;

namespace RiverHollow.Characters
{
    /// <summary>
    /// The Merchant is a class of Actor that appear periodically to both sell items
    /// to the player character as well as requesting specific items at a premium.
    /// </summary>
    public class Merchant : TravellingNPC
    {
        List<RequestItem> _liRequestItems;
        public Dictionary<Item, bool> DiChosenItems;
        private bool _bRequestsComplete = false;

        public Merchant(int index, Dictionary<string, string> stringData, bool loadanimations = true)
        {
            _eActorType = ActorEnum.Merchant;

            _liRequiredBuildingIDs = new List<int>();
            _diRequiredObjectIDs = new Dictionary<int, int>();

            _liRequestItems = new List<RequestItem>();
            DiChosenItems = new Dictionary<Item, bool>();
            ImportBasics(index, stringData, loadanimations);

            _bOnTheMap = false;
            _bShopIsOpen = true;
        }

        protected override void ImportBasics(int index, Dictionary<string, string> stringData, bool loadanimations = true)
        {
            base.ImportBasics(index, stringData, loadanimations);

            foreach (string s in Util.FindParams(stringData["RequestIDs"]))
            {
                RequestItem request = new RequestItem();
                string[] split = s.Split('-');
                request.ItemID = int.Parse(split[0]);
                request.Number = (split.Length > 1) ? int.Parse(split[1]) : 1;

                _liRequestItems.Add(request);
            }
        }

        public override void RollOver()
        {
            if (!_bOnTheMap)
            {
                if (TownRequirementsMet() && HandleTravelTiming())
                {
                    if (!_bArrivedOnce) { GameManager.MerchantQueue.Add(this); }
                    else { GameManager.MerchantQueue.Insert(0, this); }
                }
            }
            else
            {
                _bOnTheMap = false;
                _iNextArrival = _iArrivalPeriod;
                CurrentMap?.RemoveCharacterImmediately(this);
            }
        }

        public void ArriveInTown()
        {
            MoveToSpawn();
            _bArrivedOnce = true;

            DiChosenItems.Clear();
            List<RequestItem> copy = new List<RequestItem>(_liRequestItems);
            for (int i = 0; i < 3; i++)
            {
                int chosenValue = RHRandom.Instance().Next(0, copy.Count - 1);

                RequestItem request = copy[chosenValue];
                Item it = DataManager.GetItem(request.ItemID, request.Number);

                DiChosenItems[it] = false;
                copy.RemoveAt(chosenValue);
            }

            //ClearPath();
            //CalculatePathing();

            CanGiveGift = true;
        }

        /// <summary>
        /// Returns the initial text for when the Actor is first talked to.
        /// </summary>
        /// <returns>The text string to display</returns>
        public override TextEntry GetOpeningText()
        {
            TextEntry rv = null;

            if (!Introduced)
            {
                rv = GetDialogEntry("Introduction");
                Relationship = RelationShipStatusEnum.Friends;
            }
            else
            {
                if (!_bHasTalked) { rv = GetDailyDialogue(); }
                else
                {
                    rv = _diDialogue["Selection"];
                }
            }

            return rv;
        }

        public override TextEntry OpenRequests()
        {
            TextEntry rv = null;

            if (!_bRequestsComplete) { GUIManager.OpenMainObject(new HUDRequestWindow(_diDialogue["Requests"], this)); }
            else { rv = _diDialogue["RequestsComplete"]; }

            return rv;
        }

        /// <summary>
        /// Set the FinishedRequest flag to true.
        /// Ensures that we do not display the requests list after.
        /// </summary>
        public void FinishRequests()
        {
            _bRequestsComplete = true;
        }

        public override void MoveToSpawn()
        {
            _bOnTheMap = true;

            CurrentMapName = MapManager.TownMapName;
            MapManager.Maps[CurrentMapName].AddCharacterImmediately(this);

            Position = Util.SnapToGrid(GameManager.MarketPosition);
        }

        private struct RequestItem
        {
            public int ItemID;
            public int Number;
        }

        public MerchantData SaveData()
        {
            MerchantData npcData = new MerchantData()
            {
                npcID = ID,
                arrivalDelay = _iDaysToFirstArrival,
                timeToNextArrival = _iNextArrival,
                introduced = Introduced,
                spokenKeys = _liSpokenKeys,
                arrivedOnce = _bArrivedOnce
            };

            return npcData;
        }
        public void LoadData(MerchantData data)
        {
            Relationship = (RelationShipStatusEnum)data.relationShipStatus;
            _iDaysToFirstArrival = data.arrivalDelay;
            _iNextArrival = data.timeToNextArrival;
            _bArrivedOnce = data.arrivedOnce;

            if (_iNextArrival == 0)
            {
                ArriveInTown();
            }

            foreach (string s in data.spokenKeys)
            {
                _diDialogue[s].Spoken(this);
            }
        }
    }
}
