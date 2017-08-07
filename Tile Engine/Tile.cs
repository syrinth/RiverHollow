using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace Adventure
{
    static class Tile
    {
        static public Texture2D TileSetTexture;
        static public int TILE_WIDTH = 48;
        static public int TILE_HEIGHT = 48;

        static public Rectangle GetSourceRectangle(int tileIndex)
        {
            int tileY = tileIndex / (TileSetTexture.Width / TILE_WIDTH);
            int tileX = tileIndex % (TileSetTexture.Width / TILE_WIDTH);

            return new Rectangle(tileX * TILE_WIDTH, tileY * TILE_HEIGHT, TILE_WIDTH, TILE_HEIGHT);
        }
    }
}
