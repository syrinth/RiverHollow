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

        private const int PortraitWidth = 160;
        private const int PortraitHeight = 192;
        protected Texture2D _portrait;
        public Texture2D Portrait { get => _portrait; }
        protected Rectangle _portraitRect;
        public Rectangle PortraitRectangle { get => _portraitRect; }

        public string _name;
        public string Name { get => _name; }

        private string _currentMap;
        public string CurrentMap { get => _currentMap; }

        private Dictionary<string, string> _dialogueDictionary;

        public NPC()
        {
        }

        public NPC(string[] data)
        {
            LoadContent();
            if (data.Length == 4)
            {
                int i = 0;
                _name = data[i++];
                string[] vectorSplit = data[i++].Split(' ');
                _portraitRect = new Rectangle(int.Parse(vectorSplit[0]), int.Parse(vectorSplit[1]), PortraitWidth, PortraitHeight);
                _currentMap = data[i++];
                vectorSplit = data[i++].Split(' ');
                Position = new Vector2(int.Parse(vectorSplit[0]), int.Parse(vectorSplit[1]));

                _dialogueDictionary = GameContentManager.LoadDialogue(@"Data\Dialogue\" + _name);
                _portrait = GameContentManager.GetTexture(@"Textures\portraits");

                _schedule = new List<Vector2>();
                _schedule.Add(new Vector2(Position.X - 100, Position.Y + 100));
                _schedule.Add(Position);
                _moveTo = _schedule[0];

                MapManager.Maps[_currentMap].AddCharacter(this);
            }
        }

        public override void Update(GameTime theGameTime)
        {
            base.Update(theGameTime);
        }

        public virtual void Talk()
        {
            string text = string.Empty;
            if (PlayerManager._talkedTo.ContainsKey(_name) && PlayerManager._talkedTo[_name] == false) {
                text = _dialogueDictionary["Introduction"];
                PlayerManager._talkedTo[_name] = true;
            }
            else
            {
                Random r = new Random();
                text = _dialogueDictionary[_name + r.Next(1, 3)];
            }
            text = ProcessText(text);
            GUIManager.LoadScreen(GUIManager.Screens.Text, this, text);
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
            return ProcessText(_dialogueDictionary[entry]);
        }

        public string ProcessText(string text)
        {
            string rv = string.Empty;
            string[] sections = text.Split(new[] { '$' }, StringSplitOptions.RemoveEmptyEntries);
            for(int i=0; i< sections.Length; i++)
            {
                if (int.TryParse(sections[i], out int val))
                {
                    if (val == 0)
                    {
                        sections[i] = _name;
                    }
                    else
                    {
                        sections[i] = CharacterManager.GetCharacterNameByIndex(val);
                    }
                }
                else if(sections[i] == "^"){
                    sections[i] = PlayerManager.Player.Name;
                }
                rv += sections[i];
            }

            return rv;
        }
    }
}
