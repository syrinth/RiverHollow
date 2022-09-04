using Microsoft.Xna.Framework;
using RiverHollow.Game_Managers;
using RiverHollow.Map_Handling;
using RiverHollow.Utilities;
using System.Collections.Generic;

namespace RiverHollow.Items
{
    public class CapeOfBlinking : Tool
    {
        public CapeOfBlinking(int id, Dictionary<string, string> stringData) : base(id, stringData) { }

        public override bool ItemBeingUsed()
        {
            Vector2 initialPosition = PlayerManager.PlayerActor.Position;
            PlayerManager.PlayerActor.Position += Util.GetVectorFromDirection(PlayerManager.PlayerActor.Facing) * Constants.TILE_SIZE * 2;

            //Need to get all tiles touched by the player Rectangle as we are only touching on the Position and not the entirety
            List<RHTile> tiles = PlayerManager.GetTiles();
            while (tiles.Find(x => !x.Passable()) != null)
            {
                PlayerManager.PlayerActor.Position += Util.GetVectorFromDirection(Util.GetOppositeDirection(PlayerManager.PlayerActor.Facing));
                tiles = PlayerManager.GetTiles();
            }

            return true;
        }
    }
}
