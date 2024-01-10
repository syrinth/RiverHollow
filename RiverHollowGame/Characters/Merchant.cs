using Microsoft.Xna.Framework;
using RiverHollow.Game_Managers;
using RiverHollow.GUIComponents.Screens.HUDWindows;
using RiverHollow.Items;
using RiverHollow.Misc;
using RiverHollow.Utilities;
using System.Collections.Generic;
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

        public override Rectangle HoverBox => new Rectangle(Position, Size + new Point(0, Constants.TILE_SIZE));

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

        public int Capacity { get; private set; } = 0;

        public int ShopID => DataManager.GetIntByIDKey(ID, "ShopData", DataType.Actor);

        public Merchant(int index, Dictionary<string, string> stringData) : base(index, stringData)
        {
            OnTheMap = false;
            _diRequiredObjectIDs = new Dictionary<int, int>();
        }

        protected override string SpriteName()
        {
            return DataManager.MERCHANT_FOLDER + GetStringByIDKey("Key");
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
                    }
                }
            }
        }

        public void Cleanup()
        {
            OnTheMap = false;
            _iRequestIndex = Util.GetLoopingValue(_iRequestIndex, 0, RequestIDs.Length - 1, 1);
            CurrentMap?.RemoveCharacterImmediately(this);
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
                    rv = GetDialogEntry("Selection");
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

        public void UpdateCapacity(int numSold)
        {
            if(numSold <= Capacity)
            {
                Capacity -= numSold;
            }
            else
            {
                Capacity = 0;
            }
        }

        public int EvaluateItem(Item it)
        {
            Color c = Color.Black;
            return EvaluateItem(it, ref c);
        }
        public int EvaluateItem(Item it, ref Color c)
        {
            if (it == null || it.CompareType(ItemEnum.Tool, ItemEnum.NPCToken, ItemEnum.Blueprint, ItemEnum.Buildable, ItemEnum.Special, ItemEnum.Clothing))
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
                else
                {
                    offer = it.Value;
                }
            }

            return offer * (it.Number <= Capacity ? it.Number : Capacity);
        }

        public override void MoveToSpawn()
        {
            Capacity = Constants.MERCHANT_BASE_CAPACITY;
            OnTheMap = true;

            CurrentMapName = Constants.TOWN_MAP_NAME;
            MapManager.Maps[CurrentMapName].AddCharacterImmediately(this);

            SetPosition(Util.SnapToGrid(new Point(TownManager.Market.MapPosition.X + TownManager.Market.SpecialCoords.X, TownManager.Market.MapPosition.Y + TownManager.Market.SpecialCoords.Y)));
            PlayAnimation(VerbEnum.Idle, DirectionEnum.Down);
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
                GetDialogEntry(s).Spoken(this);
            }
        }
    }
}
