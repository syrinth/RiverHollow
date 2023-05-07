using RiverHollow.Utilities;

namespace RiverHollow.WorldObjects
{
    public class Wallpaper : Buildable
    {
        public Wallpaper(int id) : base(id)
        {
            _ePlacement = Enums.ObjectPlacementEnum.Wall;
        }
    }
}
