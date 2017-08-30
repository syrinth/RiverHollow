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
        //private List<ObjectManager.ItemIDs> _likedItems;
        //private List<ObjectManager.ItemIDs> _hatedItems;

        private List<Vector2> _schedule;
        private Vector2 _moveTo;

        protected Texture2D _portrait;
        public Texture2D Portrait { get => _portrait; }
        protected Rectangle _portraitRect;
        public Rectangle PortraitRectangle { get => _portraitRect; }

        public string _name;
        public string Name { get => _name; }

        private static Dictionary<string, string> _dialogueDictionary;

        public NPC()
        {
        }

        public NPC(string name, Vector2 pos)
        {
            _name = name;
            _dialogueDictionary = GameContentManager.LoadDialogue(@"Data\Dialogue\"+_name);
            _portrait = GameContentManager.GetTexture(@"Textures\portraits");
            _portraitRect = new Rectangle(0, 192, 160, 192);
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

        public virtual void Talk()
        {
            string _text = string.Empty;
            if (PlayerManager._talkedTo.ContainsKey(_name) && PlayerManager._talkedTo[_name] == false) {
                _text = _dialogueDictionary["Introduction"];
                PlayerManager._talkedTo[_name] = true;
            }
            else
            {
                Random r = new Random();
                _text = _dialogueDictionary[_name + r.Next(1, 3)];
            }
            GUIManager.LoadScreen(GUIManager.Screens.Text, this, _text);
        }

        public void LoadContent()
        {
            base.LoadContent(@"Textures\NPC", 32, 64, 4, 0.2f);
        }

        public void DrawPortrait(SpriteBatch spriteBatch, Vector2 dest)
        {
            if (_portrait != null)
            {
                spriteBatch.Draw(_portrait, new Vector2(dest.X, dest.Y - PortraitRectangle.Height), PortraitRectangle, Color.White);
            }
        }

        public string GetDialogEntry(string entry)
        {
            return _dialogueDictionary[entry];
        }
    }
}
