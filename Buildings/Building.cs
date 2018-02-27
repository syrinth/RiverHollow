using Microsoft.Xna.Framework;
using RiverHollow.Game_Managers;
using RiverHollow.Items;
using RiverHollow.Tile_Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace RiverHollow.Buildings
{
    public class Building : WorldObject
    {
        protected static int Size = RHMap.TileSize;

        protected int _entranceX;
        protected int _entranceY;
        protected int _baseStartX;
        protected int _baseStartY;
        protected int _baseWidth; //In Tiles
        public int BaseWidth { get => _baseWidth * Size; } //In Pixels
        protected int _baseHeight; //In Tiles
        public int BaseHeight { get => _baseHeight * Size; } //In Pixels
        public string _name;
        public string Name { get => _name; }
        public string MapName { get => "map"+_name.Replace(" ", ""); }

        public override Rectangle CollisionBox { get => GenerateCollisionBox(); }
        public Rectangle SelectionBox { get => new Rectangle((int)Position.X, (int)Position.Y, _texture.Width, _texture.Height); }

        protected Rectangle _leaveLocation;
        public Rectangle BoxToExit { get => _leaveLocation; }

        protected Rectangle _boxToEnter;
        public Rectangle BoxToEnter { get => _boxToEnter; }

        public Building() { }

        public Building(string[] stringData, int id)
        {
            ImportBasics(stringData, id);
        }

        protected int ImportBasics(string[] stringData, int id)
        {
            _id = id;
            int i = 0;
            _name = stringData[i++];
            _texture = GameContentManager.GetTexture(@"Textures\" + stringData[i++]);
            string[] split = stringData[i++].Split(' ');
            _width = int.Parse(split[0]);
            _height = int.Parse(split[1]);
            split = stringData[i++].Split(' ');
            _baseStartX = int.Parse(split[0]);
            _baseStartY = int.Parse(split[1]);
            _baseWidth = int.Parse(split[2]);
            _baseHeight = int.Parse(split[3]);
            split = stringData[i++].Split(' ');
            _entranceX = int.Parse(split[0]);
            _entranceY = int.Parse(split[1]);

            return i;
        }

        public Rectangle GenerateCollisionBox()
        {
            int startX = (int)_position.X + (_baseStartX * Size);
            int startY = (int)_position.Y + (_baseStartY * Size);

            return new Rectangle(startX, startY, _baseWidth * Size, _baseHeight * Size);
        }
    }
}
