using Microsoft.Xna.Framework;
using RiverHollow.Characters;
using RiverHollow.Game_Managers;
using RiverHollow.Items;
using RiverHollow.Utilities;
using System.Collections.Generic;
using static RiverHollow.Utilities.Enums;

namespace RiverHollow.Misc
{
    internal class Adventure
    {
        public readonly int ID = -1;

        public Point Location => GetPointByIDKey("Location");
        private readonly Dictionary<RarityEnum, List<Mob>> _dictMobs;
        private readonly Dictionary<RarityEnum, List<Item>> _dictResources;

        public virtual string Name => GetTextData("Name");

        public Adventure(int id)
        {
            ID = id;

            _dictMobs = new Dictionary<RarityEnum, List<Mob>>();
            _dictResources = new Dictionary<RarityEnum, List<Item>>();

            //Fills out the natural resources of the adventure
            foreach (string s in GetStringParamsByIDKey("ItemID"))
            {
                int resourceID = -1;
                RarityEnum rarity = RarityEnum.C;
                Util.GetRarity(s, ref resourceID, ref rarity);

                Util.AddToListDictionary(ref _dictResources, rarity, DataManager.GetItem(resourceID));
            }

            //Fills out the mobs of the adventure
            foreach (string s in GetStringParamsByIDKey("MobID"))
            {
                int mobID = -1;
                RarityEnum rarity = RarityEnum.C;
                Util.GetRarity(s, ref mobID, ref rarity);

                Util.AddToListDictionary(ref _dictMobs, rarity, DataManager.CreateActor<Mob>(mobID));
            }
        }

        //Retrieve all items that the adventure can possibly drop and group them by rarity
        public Dictionary<RarityEnum, List<Item>> GetAllLoot()
        {
            var allLoot = new Dictionary<RarityEnum, List<Item>>(_dictResources);
            foreach (var mobList in _dictMobs.Values)
            {
                foreach (var mob in mobList)
                {
                    foreach (string loot in mob.LootData)
                    {
                        int resourceID = -1;
                        RarityEnum rarity = RarityEnum.C;
                        Util.GetRarity(loot, ref resourceID, ref rarity);

                        Util.AddToListDictionary(ref allLoot, rarity, DataManager.GetItem(resourceID));
                    }
                }
            }

            return allLoot;
        }

        public Mob GetRandomMob()
        {
            return Util.RollOnRarityTable(_dictMobs);
        }

        public Item GetRandomResource()
        {
            return Util.RollOnRarityTable(_dictResources);
        }

        #region Lookup Handlers
        public string GetTextData(string key)
        {
            return DataManager.GetTextData(ID, key, DataType.Adventure); 
        }
        public bool GetBoolByIDKey(string key)
        {
            return DataManager.GetBoolByIDKey(ID, key, DataType.Adventure);
        }
        public int GetIntByIDKey(string key, int defaultValue = -1)
        {
            return DataManager.GetIntByIDKey(ID, key, DataType.Adventure, defaultValue);
        }
        public Dictionary<int, int> GetIntDictionaryByIDKey(string key)
        {
            return DataManager.IntDictionaryFromLookup(ID, key, DataType.Adventure);
        }
        public float GetFloatByIDKey(string key, int defaultValue = -1)
        {
            return DataManager.GetFloatByIDKey(ID, key, DataType.Adventure, defaultValue);
        }
        public string GetStringByIDKey(string key, string defaultValue = "")
        {
            return DataManager.GetStringByIDKey(ID, key, DataType.Adventure, defaultValue);
        }
        public string[] GetStringArgsByIDKey(string key, string defaultValue = "")
        {
            return DataManager.GetStringArgsByIDKey(ID, key, DataType.Adventure, defaultValue);
        }
        public string[] GetStringParamsByIDKey(string key, string defaultValue = "")
        {
            return DataManager.GetStringParamsByIDKey(ID, key, DataType.Adventure, defaultValue);
        }

        public TEnum GetEnumByIDKey<TEnum>(string key) where TEnum : struct
        {
            return DataManager.GetEnumByIDKey<TEnum>(ID, key, DataType.Adventure);
        }
        protected Point GetPointByIDKey(string key, Point defaultValue = default)
        {
            return DataManager.GetPointByIDKey(ID, key, DataType.Adventure, defaultValue);
        }
        protected Rectangle GetRectangleByIDKey(string key, Rectangle defaultValue = default)
        {
            return DataManager.GetRectangleByIDKey(ID, key, DataType.Adventure, defaultValue);
        }
        #endregion
    }
}
