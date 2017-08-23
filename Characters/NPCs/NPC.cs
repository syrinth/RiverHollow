using Adventure.Game_Managers;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure.Characters
{
    public class NPC : Character
    {
        private List<ObjectManager.ItemIDs> _likedItems;
        private List<ObjectManager.ItemIDs> _hatedItems;

        private List<Vector2> _schedule;
        private Vector2 _moveTo;

        private string _name;
        public string Name { get => _name; }

        public NPC()
        {

        }

        public NPC(Vector2 pos)
        {
            _name = "Ralph";
            LoadContent();
            Position = pos;
            _schedule = new List<Vector2>();
            _schedule.Add(new Vector2(Position.X-100, Position.Y +100));
            _schedule.Add(Position);
            _moveTo = _schedule[0];
        }

        public override void Update(GameTime theGameTime)
        {
            base.Update(theGameTime);
        }

        public void LoadContent()
        {
            base.LoadContent(@"Textures\NPC", 32, 64, 4, 0.2f);
        }

        public void Talk()
        {
            GUIManager.OpenTextWindow("O Romeo, Romeo, wherefore art thou Romeo? Deny thy father and refuse thy name. Or if thou wilt not, be but sworn my love And I'll no longer be a Capulet. 'Tis but thy name that is my enemy: Thou art thyself, though not a Montague. What's Montague? It is nor hand nor foot Nor arm nor face nor any other part Belonging to a man. O be some other name. What's in a name? That which we call a rose By any other name would smell as sweet; So Romeo would, were he not Romeo call'd, Retain that dear perfection which he owes Without that title. Romeo, doff thy name, And for that name, which is no part of thee, Take all myself.");
        }
    }
}
