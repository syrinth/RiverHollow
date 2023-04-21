using Microsoft.Xna.Framework;
using RiverHollow.Game_Managers;
using RiverHollow.SpriteAnimations;
using RiverHollow.Map_Handling;
using System.Collections.Generic;
using RiverHollow.Utilities;

namespace RiverHollow.WorldObjects
{
    public abstract class AdjustableObject : Buildable
    {
        public AdjustableObject(int id) : base(id) { }

        /// <summary>
        /// Loads in the different sprite versions required for an AdjustableObject
        /// so that they can be easily played and referenced in the future.
        /// </summary>
        /// <param name="sprite">The AnimatedSprite to load the animations into</param>
        /// <param name="vStart">The source position for this texture series</param>
        protected AnimatedSprite LoadAdjustableSprite(string textureName = DataManager.FILE_FLOORING)
        {
            AnimatedSprite spr = new AnimatedSprite(textureName);
            spr.AddAnimation("None", _pImagePos.X, _pImagePos.Y, _pSize);
            spr.AddAnimation("NS", _pImagePos.X + Constants.TILE_SIZE, _pImagePos.Y, _pSize);
            spr.AddAnimation("EW", _pImagePos.X + Constants.TILE_SIZE * 2, _pImagePos.Y, _pSize);
            spr.AddAnimation("SW", _pImagePos.X + Constants.TILE_SIZE * 3, _pImagePos.Y, _pSize);
            spr.AddAnimation("NW", _pImagePos.X + Constants.TILE_SIZE * 4, _pImagePos.Y, _pSize);
            spr.AddAnimation("NE", _pImagePos.X + Constants.TILE_SIZE * 5, _pImagePos.Y, _pSize);
            spr.AddAnimation("SE", _pImagePos.X + Constants.TILE_SIZE * 6, _pImagePos.Y, _pSize);
            spr.AddAnimation("NSE", _pImagePos.X + Constants.TILE_SIZE * 7, _pImagePos.Y, _pSize);
            spr.AddAnimation("NSW", _pImagePos.X + Constants.TILE_SIZE * 8, _pImagePos.Y, _pSize);
            spr.AddAnimation("NEW", _pImagePos.X + Constants.TILE_SIZE * 9, _pImagePos.Y, _pSize);
            spr.AddAnimation("SEW", _pImagePos.X + Constants.TILE_SIZE * 10, _pImagePos.Y, _pSize);
            spr.AddAnimation("NSEW", _pImagePos.X + Constants.TILE_SIZE * 11, _pImagePos.Y, _pSize);
            spr.AddAnimation("W", _pImagePos.X + Constants.TILE_SIZE * 12, _pImagePos.Y, _pSize);
            spr.AddAnimation("E", _pImagePos.X + Constants.TILE_SIZE * 13, _pImagePos.Y, _pSize);
            spr.AddAnimation("S", _pImagePos.X + Constants.TILE_SIZE * 14, _pImagePos.Y, _pSize);
            spr.AddAnimation("N", _pImagePos.X + Constants.TILE_SIZE * 15, _pImagePos.Y, _pSize);

            return spr;
        }

        public override bool PlaceOnMap(Point pos, RHMap map, bool ignoreActors = false)
        {
            bool rv = false;
            if (base.PlaceOnMap(pos, map))
            {
                rv = true;
                AdjustObject();
            }
            return rv;
        }

        /// <summary>
        /// Calls the AdjustmentHelper on the main base RHTile after first
        /// removing it from the map.
        /// </summary>
        public override void RemoveSelfFromTiles()
        {
            if (Tiles.Count > 0)
            {
                RHTile startTile = Tiles[0];
                base.RemoveSelfFromTiles();
                AdjustmentHelper(startTile);
                Sprite.PlayAnimation("None");
            }
        }

        /// <summary>
        /// Calls the AdjustmentHelper on the main base RHTile
        /// </summary>
        public void AdjustObject()
        {
            AdjustmentHelper(Tiles[0]);
        }

        /// <summary>
        /// Adjusts the source rectangle for the AdjustableObject compared to nearby AdjustableObjects
        /// 
        /// First thing to do is to determine how many AdjustableObjects are adjacent to the AdjustableObject and where, we then
        /// create a string in NSEW order to determine which segment we need to get.
        /// 
        /// Then compare the string and determine which piece it corresponds to.
        /// 
        /// Finally, run this method again on each of the adjacent AdjustableObjects to update their appearance
        /// </summary>
        /// <param name="startTile">The RHTile to center the adjustments on.</param>
        /// <param name="adjustAdjacent">Whether or not to call this method against the adjacent tiles</param>
        protected virtual void AdjustmentHelper(RHTile startTile, bool adjustAdjacent = true)
        {
            string mapName = startTile.MapName;
            string sAdjacent = string.Empty;
            List<RHTile> liAdjacentTiles = new List<RHTile>();

            //Create the adjacent tiles string
            MakeAdjustments("N", ref sAdjacent, ref liAdjacentTiles, MapManager.Maps[mapName].GetTileByGridCoords(new Point((int)(startTile.X), (int)(startTile.Y - 1))));
            MakeAdjustments("S", ref sAdjacent, ref liAdjacentTiles, MapManager.Maps[mapName].GetTileByGridCoords(new Point((int)(startTile.X), (int)(startTile.Y + 1))));
            MakeAdjustments("E", ref sAdjacent, ref liAdjacentTiles, MapManager.Maps[mapName].GetTileByGridCoords(new Point((int)(startTile.X + 1), (int)(startTile.Y))));
            MakeAdjustments("W", ref sAdjacent, ref liAdjacentTiles, MapManager.Maps[mapName].GetTileByGridCoords(new Point((int)(startTile.X - 1), (int)(startTile.Y))));

            Sprite.PlayAnimation(string.IsNullOrEmpty(sAdjacent) ? "None" : sAdjacent);

            //Find all matching objects in the adjacent tiles and call
            //this method without recursion on them.
            if (adjustAdjacent)
            {
                foreach (RHTile t in liAdjacentTiles)
                {
                    AdjustableObject obj = null;
                    if (MatchingObjectTest(t, ref obj))
                    {
                        obj.AdjustmentHelper(t, false);
                    }
                }
            }
        }

        /// <summary>
        /// If the given tile passes the AdjustableObject test, add the if Valdid
        /// string to the adjacency string then add to the list
        /// </summary>
        /// <param name="ifValid">Which directional value to add on a pass</param>
        /// <param name="adjacentStr">Ref to the adjacency string</param>
        /// <param name="adjacentList">ref to the adjacency list</param>
        /// <param name="tile">The tile to test</param>
        protected void MakeAdjustments(string ifValid, ref string adjacentStr, ref List<RHTile> adjacentList, RHTile tile)
        {
            if (MatchingObjectTest(tile))
            {
                adjacentStr += ifValid;
                adjacentList.Add(tile);
            }
        }

        /// <summary>
        /// Helper for the MatchingObjectTest method for when we don't care about the object
        /// </summary>
        /// <param name="tile">Tile to test against</param>
        /// <returns>True if the tile exists and contains a matching AdjustableObject</returns>
        protected virtual bool MatchingObjectTest(RHTile tile)
        {
            AdjustableObject obj = null;
            return MatchingObjectTest(tile, ref obj);
        }

        /// <summary>
        /// Check to see that the tile exists, has an AdjustableObject and that AdjustableObject matches the initial type
        /// </summary>
        /// <param name="tile">Tile to test against</param>
        /// <param name="obj">Reference to any AdjustableObject that may be found</param>
        /// <returns>True if the tile exists and contains a matching AdjustableObject</returns>
        protected virtual bool MatchingObjectTest(RHTile tile, ref AdjustableObject obj)
        {
            bool rv = false;

            if (tile != null)
            {
                WorldObject wObj = tile.GetWorldObject(false);
                if (wObj != null && wObj.Type == Type)
                {
                    obj = (AdjustableObject)wObj;
                    rv = true;
                }
            }

            return rv;
        }
    }
}
