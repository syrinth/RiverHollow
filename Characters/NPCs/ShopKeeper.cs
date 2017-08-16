using Adventure.Buildings;
using Adventure.Game_Managers;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BuildingID = Adventure.Game_Managers.ItemManager.BuildingID;
namespace Adventure.Characters.NPCs
{
    public class ShopKeeper : NPC
    {
        protected bool _isOpen;
        public bool IsOpen { get => _isOpen; set => _isOpen = value; }

        protected List<BuildingID> _buildings;
        public List<BuildingID> Buildings { get => _buildings; }

        public ShopKeeper(Vector2 position)
        {
            LoadContent(@"Textures\Taylor", 32, 64, 1, 1);
            Position = position;
            _isOpen = true;

            _buildings = new List<BuildingID>();
            _buildings.Add(BuildingID.ArcaneTower);
        }
    }
}
