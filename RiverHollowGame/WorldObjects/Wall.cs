using RiverHollow.Game_Managers;
using System.Collections.Generic;

namespace RiverHollow.WorldObjects
{
    /// <summary>
    /// Wall object that can adjust themselves based off of other, adjacent walls
    /// </summary>
    public class Wall : AdjustableObject
    {
        public Wall(int id) : base(id)
        {
        }
    }
}
