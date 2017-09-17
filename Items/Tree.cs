using Adventure.Game_Managers;
using Adventure.Tile_Engine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Adventure.Items
{
    public class Tree : WorldObject
    {
        public override Rectangle CollisionBox {  get=> new Rectangle((int)Position.X + RHTileMap.TileSize, (int)Position.Y + RHTileMap.TileSize * 3, RHTileMap.TileSize, RHTileMap.TileSize); }

        public Tree(ObjectManager.ObjectIDs id, float hp, bool breakIt, bool chopIt, Vector2 pos, Rectangle sourceRectangle, Texture2D tex, int lvl, int width, int height):base(id, hp, true, breakIt, chopIt, pos, sourceRectangle, tex, lvl, width, height)
        {
        }
    }
}
