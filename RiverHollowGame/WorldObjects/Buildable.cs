using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.Game_Managers;
using RiverHollow.Items;
using RiverHollow.Utilities;
using System.Collections.Generic;

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
        protected bool _bSelected = false;

        public bool Unique { get; protected set; }

        public Buildable(int id) : base(id)
        {
            _rBase.Y = _pSize.Y - BaseHeight;

            Unique = GetBoolByIDKey("Unique");
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            Sprite.SetColor(_bSelected ? Color.Green : Color.White);
            base.Draw(spriteBatch);
        }

        public void SelectObject(bool val) { _bSelected = val; }

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
            switch (_eObjectType)
            {
                case Enums.ObjectTypeEnum.Building:
                case Enums.ObjectTypeEnum.Structure:
                    return true;
                default:
                    return false;
            }
        }
    }
}
