
using Adventure.Game_Managers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Adventure.Items
{
    public class Staircase : WorldObject
    {
        protected string _toMap;
        public string ToMap { get => _toMap; }

        public Staircase(ObjectManager.ObjectIDs id, Vector2 pos, Rectangle sourceRectangle, Texture2D tex, int lvl, int width, int height):base(id, 0, false, false, false, pos, sourceRectangle, tex, lvl, width, height)
        {
            _wallObject = true;
        }

        public void SetExit(string map)
        {
            _toMap = map;
        }
    }
}
