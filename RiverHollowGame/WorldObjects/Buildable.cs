using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.Game_Managers;
using RiverHollow.Items;
using RiverHollow.Utilities;
using System.Collections.Generic;
using static RiverHollow.Utilities.Enums;

namespace RiverHollow.WorldObjects
{
    /// <summary>
    /// Buildable represent WorldObjects that are built by the player
    /// </summary>
    public class Buildable : WorldObject
    {
        public int Value => GetIntByIDKey("Value");

        public Dictionary<int, int> RequiredToMake => GetIntDictionaryByIDKey("ReqItems");

        public bool OutsideOnly { get; protected set; } = false;

        public bool Unique { get; protected set; }

        public Buildable(int id) : base(id)
        {
            _rBase.Y = _pSize.Y - BaseHeight;

            Unique = GetBoolByIDKey("Unique");
        }

        protected override void LoadSprite()
        {
            if (GetBoolByIDKey("Texture"))
            {
                LoadSprite(DataManager.FOLDER_WORLDOBJECTS + GetStringByIDKey("Texture"));
            }
            else
            {
                LoadSprite(DataManager.FILE_DECOR);
            }
        }

        public bool CanBuild()
        {
            if (Unique && TownManager.GetNumberTownObjects(ID) != 0)
            {
                return false;
            }

            return !OutsideOnly || (OutsideOnly && MapManager.CurrentMap.IsOutside);
        }

        public bool BuildOnScreen()
        {
            switch (GetEnumByIDKey<BuildableEnum>("Subtype"))
            {
                case BuildableEnum.Building:
                case BuildableEnum.Structure:
                    return true;
                default:
                    return false;
            }
        }
    }
}
