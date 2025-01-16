using Microsoft.Xna.Framework;
using RiverHollow.Game_Managers;
using RiverHollow.Map_Handling;
using RiverHollow.Utilities;
using System.Collections.Generic;

namespace RiverHollow.Items
{
    public class CapeOfBlinking : Tool
    {
        public CapeOfBlinking(int id) : base(id) { }

        public override void UseItem()
        {
            Point initialPosition = PlayerManager.PlayerActor.CollisionBoxLocation;
            PlayerManager.PlayerActor.MoveActor(Util.MultiplyPoint(Util.GetPointFromDirection(PlayerManager.PlayerActor.Facing), Constants.TILE_SIZE * 2));

            //Need to get all tiles touched by the player Rectangle as we are only touching on the Position and not the entirety
            List<RHTile> tiles = PlayerManager.GetTiles();
            while (tiles.Find(x => !x.Passable()) != null)
            {
                PlayerManager.PlayerActor.MoveActor(Util.GetPointFromDirection(Util.GetOppositeDirection(PlayerManager.PlayerActor.Facing)));
                tiles = PlayerManager.GetTiles();
            }
        }
    }
}
