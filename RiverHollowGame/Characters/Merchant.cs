using Microsoft.Xna.Framework;
using RiverHollow.Game_Managers;
using RiverHollow.GUIComponents.Screens;
using RiverHollow.Items;
using RiverHollow.Map_Handling;
using RiverHollow.Misc;
using RiverHollow.Utilities;
using RiverHollow.WorldObjects;
using System.Collections.Generic;
using static RiverHollow.Game_Managers.GameManager;
using static RiverHollow.Game_Managers.SaveManager;
using static RiverHollow.Utilities.Enums;

namespace RiverHollow.Characters
{
    /// <summary>
    /// The Merchant is a class of Actor that appear periodically to both sell items
    /// to the player character as well as requesting specific items at a premium.
    /// </summary>
    public class Merchant : TravellingNPC
    {
        MerchType Needs => DataManager.GetEnumByIDKey<MerchType>(ID, "Needs", DataType.Character);
        MerchType Wants => DataManager.GetEnumByIDKey<MerchType>(ID, "Wants", DataType.Character);

        List<RequestItem> _liRequestItems;
        public int[] ChosenRequests;
        public int _iShopID = -1;
        public int ShopID => _iShopID;

        public Merchant(int index, Dictionary<string, string> stringData, bool loadanimations = true) : base(index, stringData, loadanimations)
        {
            _eActorType = WorldActorTypeEnum.Merchant;

            _diRequiredObjectIDs = new Dictionary<int, int>();

            _liRequestItems = new List<RequestItem>();
            ChosenRequests = new int[3] { -1, -1, -1 };

            _bOnTheMap = false;

            Util.AssignValue(ref _iShopID, "ShopData", stringData);

            foreach (string s in Util.FindParams(stringData["RequestIDs"]))
            {
                RequestItem request = new RequestItem();
                string[] split = s.Split('-');
                request.ID = int.Parse(split[0]);
                request.Number = (split.Length > 1) ? int.Parse(split[1]) : 1;

                _liRequestItems.Add(request);
            }
        }

        public override void RollOver()
        {
            if (!_bOnTheMap)
            {
                if (TownRequirementsMet())
                {
                    if (ChosenRequests[0] == -1)
                    {
                        //When the Merchant is ready to come to town, determine requests
                        List<RequestItem> copy = new List<RequestItem>(_liRequestItems);
                        for (int i = 0; i < Constants.MERCHANT_REQUEST_NUM; i++)
                        {
                            int chosenValue = RHRandom.Instance().Next(0, copy.Count - 1);

                            RequestItem request = copy[chosenValue];
                            Item it = DataManager.GetItem(request.ID, request.Number);

                            ChosenRequests[i] = request.ID;
                            copy.RemoveAt(chosenValue);
                        }
                    }

                    if (HandleTravelTiming())
                    {
                        if (!_bArrivedOnce) { GameManager.MerchantQueue.Add(this); }
                        else { GameManager.MerchantQueue.Insert(0, this); }
                    }
                }
            }
            else
            {
                for (int i = 0; i < Constants.MERCHANT_REQUEST_NUM; i++)
                {
                    ChosenRequests[i] = -1;
                }

                _bOnTheMap = false;
                _iNextArrival = ArrivalPeriod;
                CurrentMap?.RemoveCharacterImmediately(this);
                if (_iShopID != -1)
                {
                    DIShops[_iShopID].ClearItemSpots();
                    DIShops[_iShopID].ClearRandom();
                }
            }
        }

        public void ArriveInTown()
        {
            _bArrivedOnce = true;

            MoveToSpawn();

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
                RelationshipState = RelationShipStatusEnum.Friends;
                _bHasTalked = true;
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

        public override void OpenMerchantWindow()
        {
            GUIManager.OpenMainObject(new HUDMerchantWindow(this));
        }

        public int EvaluateItem(Item it)
        {
            Color c = Color.Black;
            return EvaluateItem(it, ref c);
            
        }
        public int EvaluateItem(Item it, ref Color c)
        {
            if (it == null || it.MerchType == MerchType.None)
            {
                return 0;
            }

            int offer = it.Value > 0 ? it.Value : 0;

            bool requested = false;
            for (int i = 0; i < Constants.MERCHANT_REQUEST_NUM; i++)
            {
                if(ChosenRequests[i] == it.ID)
                {
                    c = Color.Purple;
                    requested = true;
                    offer *= 2;
                    break;
                }
            }

            if (!requested)
            {
                if (it.MerchType == Needs)
                {
                    c = Color.Blue;
                    offer = (int)(offer * 1.5);
                }
                else if (it.MerchType == Wants)
                {
                    c = Color.Green;
                    offer = (int)(offer * 1.2);
                }
            }

            return offer * it.Number;
        }

        public override void MoveToSpawn()
        {
            _bOnTheMap = true;

            CurrentMapName = Constants.TOWN_MAP_NAME;
            MapManager.Maps[CurrentMapName].AddCharacterImmediately(this);

            Structure market = (Structure)PlayerManager.GetTownObjectsByID(int.Parse(DataManager.Config[15]["ObjectID"]))[0];
            Position = Util.SnapToGrid(new Vector2(market.MapPosition.X + market.SpecialCoords.X, market.MapPosition.Y + market.SpecialCoords.Y));

            if (_iShopID != -1)
            {
                Shop marketShop = DIShops[_iShopID];
                marketShop.ClearItemSpots();
                foreach (Structure.SubObjectInfo info in market.ObjectInfo)
                {
                    marketShop.AddItemSpot(new ShopItemSpot(CurrentMapName, market.MapPosition + info.Position + new Vector2(8, -13)));
                }
                DIShops[ShopID].Randomize();
                marketShop.PlaceStock(true);
            }
        }

        private struct RequestItem
        {
            public int ID;
            public int Number;
        }

        public MerchantData SaveData()
        {
            MerchantData npcData = new MerchantData()
            {
                npcID = ID,
                timeToNextArrival = _iNextArrival,
                introduced = Introduced,
                spokenKeys = _liSpokenKeys,
                arrivedOnce = _bArrivedOnce,
                requestString = string.Join("|", ChosenRequests)
            };

            return npcData;
        }
        public void LoadData(MerchantData data)
        {
            RelationshipState = (RelationShipStatusEnum)data.relationShipStatus;
            _iNextArrival = data.timeToNextArrival;
            _bArrivedOnce = data.arrivedOnce;

            string[] split = Util.FindParams(data.requestString);
            for(int i = 0; i < Constants.MERCHANT_REQUEST_NUM; i++)
            {
                ChosenRequests[i] = int.Parse(split[i]);
            }

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
