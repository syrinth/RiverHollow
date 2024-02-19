using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.Game_Managers;
using RiverHollow.Map_Handling;
using RiverHollow.Utilities;
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

                if (obj != null && obj.ID == ID)
                {
                    rv = true;
                }
            }

            return rv;
        }
    }

    public class Earth : Floor
    {
        const string WATERED = "Watered-";
        public bool HasBeenWatered { get; private set; }
        public Earth(int id) : base(id) { }

        protected override void LoadSprite()
        {
            Sprite = LoadAdjustableSprite(DataManager.FILE_FLOORING, WATERED, 64);
        }

        protected override void PlayAnimation(string adjString)
        {
            if(HasBeenWatered)
            {
                Sprite.PlayAnimation(WATERED + adjString);
            }
            else
            {
                base.PlayAnimation(adjString);
            }
        }

        public override bool PlayerCanEdit()
        {
            return Tiles[0].WorldObject == null;
        }

        public override void Rollover()
        {
            base.Rollover();

            if (HasBeenWatered && Tiles[0].WorldObject != null && Tiles[0].WorldObject.CompareType(ObjectTypeEnum.Plant))
            {
                (Tiles[0].WorldObject as Plant).Grow();
            }

            if (EnvironmentManager.IsRaining() && CurrentMap.IsOutside) { SetWatered(true); }
            else { SetWatered(false); }

            if (Tiles[0].WorldObject == null)
            {
                if (RHRandom.Instance().RollPercent(10))
                {
                    CurrentMap.RemoveWorldObject(this);
                }
            }
        }

        public override bool PlaceOnMap(RHMap map, bool ignoreActors = false)
        {
            bool rv = base.PlaceOnMap(map, ignoreActors);

            if (rv)
            {
                CurrentMap.AddSpecialTile(Tiles[0]);
                if (EnvironmentManager.IsRaining() && CurrentMap.IsOutside)
                {
                    SetWatered(true);
                }
            }

            return rv;
        }

        public void SetWatered(bool value)
        {
            HasBeenWatered = value;
            if (HasBeenWatered)
            {
                Sprite.PlayAnimation(WATERED + Sprite.CurrentAnimation);
            }
            else
            {
                var split =Sprite.CurrentAnimation.Split('-');
                if (split.Length > 1)
                {
                    Sprite.PlayAnimation(split[1]);
                }
            }
        }

        public override float GetTownScore()
        {
            return 0;
        }
    }
}
