using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.Game_Managers;
using RiverHollow.Map_Handling;
using RiverHollow.Utilities;
using System.Collections.Generic;
using static RiverHollow.Game_Managers.SaveManager;
using static RiverHollow.Utilities.Enums;

namespace RiverHollow.WorldObjects
{
    public class Floor : AdjustableObject
    {
        /// <summary>
        /// Base Constructor to hard define the Height and Width
        /// </summary>
        public Floor(int id) : base(id)
        {
            _ePlacement = ObjectPlacementEnum.Floor;
        }

        protected override void LoadSprite()
        {
            base.LoadSprite(DataManager.FILE_FLOORING);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            Sprite.SetColor(Selected ? Color.Green : Color.White);
            Sprite.Draw(spriteBatch, 0);
        }

        /// <summary>
        /// Check to see that the tile exists, has an AdjustableObject and that AdjustableObject matches the initial type
        /// </summary>
        /// <param name="tile">Tile to test against</param>
        /// <returns>True if the tile exists and contains a Wall</returns>
        protected override bool MatchingObjectTest(RHTile tile, ref AdjustableObject obj)
        {
            bool rv = false;

            if (tile != null)
            {
                WorldObject floorObj = tile.GetFloorObject();
                if (floorObj != null && floorObj.BuildableType(BuildableEnum.Floor)) { obj = (Floor)floorObj; }

                if (obj != null && obj.Type == Type)
                {
                    rv = true;
                }
            }

            return rv;
        }
    }
}
