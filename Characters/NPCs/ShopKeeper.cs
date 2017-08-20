using Adventure.Buildings;
using Adventure.Game_Managers;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BuildingID = Adventure.Game_Managers.ObjectManager.BuildingID;
using WorkerID = Adventure.Game_Managers.ObjectManager.WorkerID;
namespace Adventure.Characters.NPCs
{
    public class ShopKeeper : NPC
    {
        protected bool _isOpen;
        public bool IsOpen { get => _isOpen; set => _isOpen = value; }

        protected List<object> _merchandise;
        public List<object> Buildings { get => _merchandise; }

        public ShopKeeper(Vector2 position)
        {
            LoadContent(@"Textures\Taylor", 32, 64, 1, 1);
            Position = position;
            _isOpen = true;

            _merchandise = new List<object>();
            _merchandise.Add(BuildingID.ArcaneTower);
            _merchandise.Add(WorkerID.Wizard);
        }
    }
}
