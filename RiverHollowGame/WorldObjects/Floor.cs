using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.Game_Managers;
using RiverHollow.Map_Handling;
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
        public Floor(int id, Dictionary<string, string> stringData) : base(id)
        {
            LoadDictionaryData(stringData, false);
            LoadAdjustableSprite(ref _sprite, DataManager.FILE_FLOORING);

            _eObjectType = ObjectTypeEnum.Floor;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            _sprite.SetColor(_bSelected ? Color.Green : Color.White);
            _sprite.Draw(spriteBatch, 0);
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
                obj = tile.GetFloorObject();
                if (obj != null && obj.Type == Type)
                {
                    rv = true;
                }
            }

            return rv;
        }

        internal FloorData SaveData()
        {
            FloorData floorData = new FloorData
            {
                ID = _iID,
                x = (int)MapPosition.X,
                y = (int)MapPosition.Y
            };

            return floorData;
        }
        internal void LoadData(FloorData data)
        {
            _iID = data.ID;
            SnapPositionToGrid(new Vector2(data.x, data.y));
        }
    }
}
