using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure.Game_Managers
{
    public class GameContentManager
    {
        static GameContentManager instance;
        private Dictionary<string, Texture2D> _textureDictionary;
        private Dictionary<string, SpriteFont> _fontDictionary;

        private GameContentManager()
        {
            _textureDictionary = new Dictionary<string, Texture2D>();
            _fontDictionary = new Dictionary<string, SpriteFont>();
        }

        public static GameContentManager GetInstance()
        {
            if (instance == null)
            {
                instance = new GameContentManager();
            }
            return instance;
        }

        public void LoadContent(ContentManager Content)
        {
            LoadCharacters(Content);
            LoadGUIs(Content);
            LoadIcons(Content);

            LoadFont(Content);
        }

        #region Load Methods
        public void LoadCharacters(ContentManager Content)
        {
            AddTexture(Content, @"Textures\Eggplant");
            AddTexture(Content, @"Textures\Wizard");
            AddTexture(Content, @"Textures\Taylor");
        }

        public void LoadGUIs(ContentManager Content)
        {
            AddTexture(Content, @"Textures\cursor");
            AddTexture(Content, @"Textures\MiniInventory");
            AddTexture(Content, @"Textures\ShopWindow");
        }

        public void LoadIcons(ContentManager Content)
        {
            AddTexture(Content, @"Textures\Sword");
            AddTexture(Content, @"Textures\arcane_essence");
            AddTexture(Content, @"Textures\ValidSquare");
            AddTexture(Content, @"Textures\ArcaneTower");
        }

        public void LoadFont(ContentManager Content)
        {
            AddFont(Content, @"Fonts\DisplayFont");
            AddFont(Content, @"Fonts\Font");
        }
        #endregion

        #region AddMethods
        private void AddTexture(ContentManager Content, string texture)
        {
            _textureDictionary.Add(texture, Content.Load<Texture2D>(texture));
        }

        private void AddFont(ContentManager Content, string font)
        {
            _fontDictionary.Add(font, Content.Load<SpriteFont>(font));
        }
        #endregion

        #region Get Methods
        public Texture2D GetTexture(string texture)
        {
            return _textureDictionary[texture];
        }

        public SpriteFont GetFont(string font)
        {
            return _fontDictionary[font];
        }
        #endregion
    }
}
