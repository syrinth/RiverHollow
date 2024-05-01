using RiverHollow.Game_Managers;
using System.Linq;

namespace RiverHollow.WorldObjects
{
    /// <summary>
    /// Wall object that can adjust themselves based off of other, adjacent walls
    /// </summary>
    public class Wall : AdjustableObject
    {
        public Wall(int id) : base(id) { }

        protected override void LoadSprite()
        {
            base.LoadSprite(DataManager.FILE_WALLS);
        }

        public override float GetTownScore()
        {
            float value = base.GetTownScore();
            float rv = value;

            var t = FirstTile();
            var adj = t.GetAdjacentTiles();
            var count = adj.Where(x => x.WorldObject != null && x.WorldObject.ID == ID).Count();

            if (count <= 2) { rv += value * (0.1f * count); }
            else if (count == 4) { rv = 0; }

            return rv;
        }
    }
}
