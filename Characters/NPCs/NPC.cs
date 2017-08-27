using Adventure.Game_Managers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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

        protected Texture2D _portrait;
        public Texture2D Portrait { get => _portrait; }
        protected Rectangle _portraitRect;
        public Rectangle PortraitRectangle { get => _portraitRect; }

        private string _name;
        public string Name { get => _name; }

        protected string _text;

        public NPC()
        {

        }

        public NPC(Vector2 pos)
        {
            _name = "Amanda";
            _portrait = GameContentManager.GetTexture(@"Textures\portraits");
            _portraitRect = new Rectangle(0, 192, 160, 192);
            _text = "O Romeo, Romeo, wherefore art thou Romeo? Deny thy father and refuse thy name. Or if thou wilt not, be but sworn my love And I'll no longer be a Capulet. 'Tis but thy name that is my enemy: Thou art thyself, though not a Montague. What's Montague? It is nor hand nor foot Nor arm nor face nor any other part Belonging to a man. O be some other name. What's in a name? That which we call a rose By any other name would smell as sweet; So Romeo would, we -done";
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
            GUIManager.LoadScreen(GUIManager.Screens.Text, this, _text);
        }

        public void DrawPortrait(SpriteBatch spriteBatch, Vector2 dest)
        {
            if (_portrait != null)
            {
                spriteBatch.Draw(_portrait, new Vector2(dest.X, dest.Y - PortraitRectangle.Height), PortraitRectangle, Color.White);
            }
        }
    }
}
