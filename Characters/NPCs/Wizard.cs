using Adventure.Game_Managers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ItemIDs = Adventure.Game_Managers.ObjectManager.ItemIDs;
namespace Adventure.Characters.NPCs
{
    public class Wizard : Worker
    {
        public override ObjectManager.WorkerID WorkerID { get { return ObjectManager.WorkerID.Wizard; } }
        const string _texture = @"Textures\Wizard";

        public Wizard(Vector2 position)
        {
            LoadContent(_texture, 32, 64, 1, 1);
            _portrait = GameContentManager.GetTexture(@"Textures\portraits");
            _portraitRect = new Rectangle(0, 0, 160, 192);

            _text = "Argh? Bah! Fnafh gragh doodle wop!";

            _currFood = 0;
            _dailyFoodReq = 3;
            _dailyItemID = ItemIDs.ArcaneEssence;
            _heldItem = null;
            _mood = 0;
            Position = position;
        }
    }
}
