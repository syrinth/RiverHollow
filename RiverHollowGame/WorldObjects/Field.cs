using Microsoft.Xna.Framework;
using RiverHollow.Map_Handling;
using System.Net.Security;

namespace RiverHollow.WorldObjects
{
    public class Field : Buildable
    {
        public Field(int id) : base(id)
        {
            _ePlacement = Utilities.Enums.ObjectPlacementEnum.Floor;
            _bWalkable = true;
            _bDrawUnder = true;
            OutsideOnly = true;
            Unique = true;
        }

        public override bool PlayerCanEdit()
        {
            bool rv = base.PlayerCanEdit();

            foreach(RHTile t in Tiles)
            {
                if (t.WorldObject != null)
                {
                    rv = false;
                    break;
                }
            }

            return rv;
        }

        public override bool PlaceOnMap(Point pos, RHMap map, bool ignoreActors = false)
        {
            bool rv = base.PlaceOnMap(pos, map, ignoreActors);

            foreach(RHTile t in Tiles)
            {
                t.WaterTile();
                t.TillTile(false);
            }

            return rv;
        }

        public override void RemoveSelfFromTiles()
        {
            foreach (RHTile t in Tiles)
            {
                t.UntillTile();
            }

            base.RemoveSelfFromTiles();
        }

        public override void Rollover()
        {
            base.Rollover();
            foreach (RHTile t in Tiles)
            {
                t.WaterTile();
            }
        }
    }
}
