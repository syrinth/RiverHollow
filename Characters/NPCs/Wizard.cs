using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ItemIDs = Adventure.Items.ItemList.ItemIDs;
namespace Adventure.Characters.NPCs
{
    public class Wizard : Worker
    {
        const string _wizTexture = @"Textures\Wizard";
        public Wizard(Vector2 position)
        {
            LoadContent(_wizTexture, 32, 64, 1, 1);

            _currFood = 0;
            _dailyFoodReq = 3;
            _dailyItemID = ItemIDs.ARCANE_ESSENCE;
            _heldItem = null;
            _mood = 0;
            Position = position;
        }
    }
}
