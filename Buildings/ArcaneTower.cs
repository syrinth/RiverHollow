using Adventure.Characters.NPCs;
using Adventure.Game_Managers;
using Adventure.Tile_Engine;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure.Buildings
{
    public class ArcaneTower : Building
    {
        public override string _map { get { return "ArcaneTower"; } }

        public ArcaneTower()
        {
            _id = PlayerManager.GetInstance().GetNewBuildingID();
            _workers = new List<Worker>();
            _baseWidth = 3;
            _baseHeight = 3;

            _reqGold = 10000;
            _texture = GameContentManager.GetInstance().GetTexture(@"Textures\ArcaneTower");
        }

        public override bool SetCoordinates(Vector2 position)
        {
            bool rv = true;
            _position = position;

            _boxToEnter = new Rectangle((int)_position.X+TileMap.TileSize, (int)(_position.Y + (_texture.Height - BaseHeight) + TileMap.TileSize*2), TileMap.TileSize, TileMap.TileSize);
            _boxToExit = new Rectangle(_boxToEnter.X, _boxToEnter.Y + TileMap.TileSize, TileMap.TileSize, TileMap.TileSize);
            return rv;

        }
    }
}
