using Microsoft.Xna.Framework;
using RiverHollow.Game_Managers;
using RiverHollow.Misc;
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

        protected string _sDescription;
        public string Description => _sDescription;

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
            GameContentManager.GetBuildingText(_id, ref _sName, ref _sDescription);

            int i = 0;
            int totalCount = 0;
            for (; i < stringData.Length; i++)
            {
                string[] tagType = stringData[i].Split(':');
                if (tagType[0].Equals("Texture"))
                {
                    _texture = GameContentManager.GetTexture(@"Textures\" + tagType[1]);
                    totalCount++;
                }
                else if (tagType[0].Equals("Dimensions"))
                {
                    string[] dimensions = tagType[1].Split('-');
                    _width = int.Parse(dimensions[0]);
                    _height = int.Parse(dimensions[1]);
                    totalCount++;
                }
                else if (tagType[0].Equals("FirstBase"))
                {
                    string[] baseSq = tagType[1].Split('-');
                    _baseStartX = int.Parse(baseSq[0]);
                    _baseStartY = int.Parse(baseSq[1]);
                    totalCount++;
                }
                else if (tagType[0].Equals("Width"))
                {
                    _baseWidth = int.Parse(tagType[1]);
                    totalCount++;
                }
                else if (tagType[0].Equals("Height"))
                {
                    _baseHeight = int.Parse(tagType[1]);
                    totalCount++;
                }
                else if (tagType[0].Equals("Entrance"))
                {
                    string[] ent = tagType[1].Split('-');
                    _entranceX = int.Parse(ent[0]);
                    _entranceY = int.Parse(ent[1]);
                    totalCount++;
                }

                if (totalCount == 6)
                {
                    break;
                }
            }

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
