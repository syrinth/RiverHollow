using System.Collections.Generic;

namespace RiverHollow.WorldObjects
{
    public class Wallpaper : Buildable
    {
        public Wallpaper(int id, Dictionary<string, string> stringData) : base(id, stringData)
        {
            _bWallObject = true;
        }
    }
}
