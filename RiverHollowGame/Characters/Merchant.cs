using Microsoft.Xna.Framework;
using RiverHollow.Game_Managers;
using RiverHollow.GUIComponents.Screens.HUDWindows;
using RiverHollow.Items;
using RiverHollow.Map_Handling;
using RiverHollow.Misc;
using RiverHollow.Utilities;
using RiverHollow.WorldObjects;
using System;
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
        ItemGroupEnum Needs => GetEnumByIDKey<ItemGroupEnum>("Needs");
        ItemGroupEnum Wants => GetEnumByIDKey<ItemGroupEnum>("Wants");
        private DayEnum MerchantDay()
        {
            var split = Util.FindArguments(GetStringByIDKey("Day"));

            return Util.ParseEnum<DayEnum>(split[0]);
        }
        private bool DefaultMerchant()
        {
            return Util.FindArguments(GetStringByIDKey("Day")).Length == 1;
        }
        private bool MyWeek()
        {
            var split = Util.FindArguments(GetStringByIDKey("Day"));

            return GameCalendar.CurrentWeek % 2 == int.Parse(split[1]);
        }

        private int[] RequestIDs => DataManager.GetIntParamsByIDKey(ID, "RequestIDs", DataType.Actor);
        private int _iRequestIndex = 0;

        public int ShopID => DataManager.GetIntByIDKey(ID, "ShopData", DataType.Actor);

        public Merchant(int index, Dictionary<string, string> stringData) : base(index, stringData)
        {
            OnTheMap = false;
            _diRequiredObjectIDs = new Dictionary<int, int>();
        }

        public override void RollOver()
        {
            if (TownManager.Merchant == null)
            {
                if (CheckTriggers() && GameCalendar.DayOfWeek == MerchantDay())
                {
                    if (DefaultMerchant() || MyWeek())
                    {
                        TownManager.SetMerchant(this);
                        DIShops[ShopID].Randomize();
                    }
                }
            }
        }

        public void Cleanup()
        {
            OnTheMap = false;
            _iRequestIndex = Util.GetLoopingValue(_iRequestIndex, 0, RequestIDs.Length - 1, 1);
            CurrentMap?.RemoveCharacterImmediately(this);
            if (ShopID != -1)
            {
                DIShops[ShopID].ClearItemSpots();
                DIShops[ShopID].ClearRandom();
            }
        }

        /// <summary>
        /// Returns the initial text for when the Actor is first talked to.
        /// </summary>
        /// <returns>The text string to display</returns>
        public override TextEntry GetOpeningText()
        {
            TextEntry rv;
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

        public int[] GetCurrentRequests()
        {
            int[] rv = new int[Constants.MERCHANT_REQUEST_NUM];
            for (int i = 0; i < Constants.MERCHANT_REQUEST_NUM; i++)
            {
                rv[i] = RequestIDs[Util.GetLoopingValue(_iRequestIndex, 0, RequestIDs.Length - 1, i)];
            }

            return rv;
        }

        public int EvaluateItem(Item it)
        {
            Color c = Color.Black;
            return EvaluateItem(it, ref c);
        }
        public int EvaluateItem(Item it, ref Color c)
        {
            if (it == null || it.CompareType(ItemEnum.Tool) || it.IsItemGroup(ItemGroupEnum.None))
            {
                return 0;
            }

            int offer = 0;
            bool requested = false;
            foreach(int i in GetCurrentRequests())
            {
                if (i == it.ID)
                {
                    c = Color.Purple;
                    requested = true;
                    offer = (int)(it.Value * Constants.MERCHANT_REQUEST_MOD);
                    break;
                }
            }

            if (!requested)
            {
                if (it.IsItemGroup(Needs))
                {
                    c = Color.Blue;
                    offer = (int)(it.Value * Constants.MERCHANT_NEED_MOD);
                }
                else if (it.IsItemGroup(Wants))
                {
                    c = Color.Green;
                    offer = (int)(it.Value * Constants.MERCHANT_WANT_MOD);
                }
            }

            return offer * it.Number;
        }

        public override void MoveToSpawn()
        {
            OnTheMap = true;

            CurrentMapName = Constants.TOWN_MAP_NAME;
            MapManager.Maps[CurrentMapName].AddCharacterImmediately(this);

            SetPosition(Util.SnapToGrid(new Point(TownManager.Market.MapPosition.X + TownManager.Market.SpecialCoords.X, TownManager.Market.MapPosition.Y + TownManager.Market.SpecialCoords.Y)));
            PlayAnimation(VerbEnum.Idle, DirectionEnum.Down);

            if (ShopID != -1)
            {
                Shop marketShop = DIShops[ShopID];
                marketShop.ClearItemSpots();
                foreach (Structure.SubObjectInfo info in TownManager.Market.ObjectInfo)
                {
                    marketShop.AddItemSpot(new ShopItemSpot(CurrentMapName, (TownManager.Market.MapPosition + info.Position + new Point(8, -13)).ToVector2()));
                }
                marketShop.PlaceStock(true);
            }
        }

        public MerchantData SaveData()
        {
            MerchantData npcData = new MerchantData()
            {
                npcID = ID,
                relationShipStatus = (int)RelationshipState,
                spokenKeys = _liSpokenKeys,
                reqIndex = _iRequestIndex
            };

            return npcData;
        }
        public void LoadData(MerchantData data)
        {
            RelationshipState = (RelationShipStatusEnum)data.relationShipStatus;
            _iRequestIndex = data.reqIndex;

            foreach (string s in data.spokenKeys)
            {
                _diDialogue[s].Spoken(this);
            }
        }
    }
}
