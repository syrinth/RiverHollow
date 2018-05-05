using Microsoft.Xna.Framework;
using RiverHollow.Game_Managers;
using RiverHollow.WorldObjects;

using static RiverHollow.Game_Managers.GameManager;
namespace RiverHollow.Buildings
{
    public class Building : WorldObject
    {
        protected int _entranceX;
        protected int _entranceY;
        protected int _baseStartX;
        protected int _baseStartY;
        protected int _baseWidth; //In Tiles
        public int BaseWidth { get => _baseWidth * TileSize; } //In Pixels
        protected int _baseHeight; //In Tiles
        public int BaseHeight { get => _baseHeight * TileSize; } //In Pixels

        protected int _iBldgLvl = 0;
        public int Level => _iBldgLvl;

        protected string _sName;
        public string Name => _sName;
        public string MapName => "map"+_sName.Replace(" ", "") + (_iBldgLvl == 0 ? "" : _iBldgLvl.ToString());

        protected string _sGivenName;
        public string GivenName=> _sGivenName;

        public override Rectangle CollisionBox { get => GenerateCollisionBox(); }
        public Rectangle SelectionBox { get => new Rectangle((int)MapPosition.X, (int)MapPosition.Y, _texture.Width, _texture.Height); }

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
            _sName = stringData[i++];
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
            int startX = (int)_vMapPosition.X + (_baseStartX * TileSize);
            int startY = (int)_vMapPosition.Y + (_baseStartY * TileSize);

            return new Rectangle(startX, startY, _baseWidth * TileSize, _baseHeight * TileSize);
        }

        public void SetName(string val)
        {
            _sGivenName = val;
        }
    }
}
