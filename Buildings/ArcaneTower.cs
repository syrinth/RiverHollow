using Adventure.Characters.NPCs;
using Adventure.Game_Managers;
using Adventure.Items;
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
        public override ObjectManager.BuildingID BuildingID { get { return ObjectManager.BuildingID.ArcaneTower; } }
        public override string _map { get { return "ArcaneTower"; } }

        public ArcaneTower()
        {
            _id = PlayerManager.GetNewBuildingID();
            _workers = new List<Worker>();
            _baseWidth = 3;
            _baseHeight = 3;
            _buildingChest = (Container)ObjectManager.GetItem(6);
            _pantry = (Container)ObjectManager.GetItem(6);

            _staticItemList = new List<StaticItem>();

            _reqGold = 10000;
            _texture = GameContentManager.GetTexture(@"Textures\ArcaneTower");
        }

        public override bool SetCoordinates(Vector2 position)
        {
            bool rv = true;
            _position = position;

            _boxToEnter = new Rectangle((int)_position.X+RHTileMap.TileSize, (int)(_position.Y + (_texture.Height - BaseHeight) + RHTileMap.TileSize*2), RHTileMap.TileSize, RHTileMap.TileSize);
            _boxToExit = new Rectangle(_boxToEnter.X, _boxToEnter.Y + RHTileMap.TileSize, RHTileMap.TileSize, RHTileMap.TileSize);
            return rv;

        }
    }
}
