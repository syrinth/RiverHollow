using System.Collections.Generic;
using RiverHollow.Game_Managers;
using RiverHollow.Map_Handling;
using RiverHollow.WorldObjects;

namespace RiverHollow.Items
{
    public class IceStaff : Tool
    {
        WorldObject _objSummoned;
        public IceStaff(int id, Dictionary<string, string> stringData) : base(id, stringData) { }

        public override bool ItemBeingUsed()
        {
            if (_objSummoned == null)
            {
                RHTile playerTile = MapManager.CurrentMap.GetTileByPixelPosition(PlayerManager.PlayerActor.CollisionCenter);
                RHTile tile = playerTile.GetTileByDirection(PlayerManager.PlayerActor.Facing);

                _objSummoned = DataManager.CreateWorldObjectByID(144);
                _objSummoned.PlaceOnMap(tile.Position, MapManager.CurrentMap);
            }
            else
            {
                MapManager.CurrentMap.RemoveWorldObject(_objSummoned);
                _objSummoned = null;
            }

            return true;
        }
    }
}
