using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure.Game_Managers
{
    public static class GameContentManager
    {
        private static ContentManager _content;
        private static Dictionary<string, Texture2D> _textureDictionary;
        private static Dictionary<string, SpriteFont> _fontDictionary;
        private static Dictionary<string, string> _npcDialogueDictionary;

        public static void LoadContent(ContentManager Content)
        {
            _content = Content;
            _textureDictionary = new Dictionary<string, Texture2D>();
            _fontDictionary = new Dictionary<string, SpriteFont>();
            _npcDialogueDictionary = LoadDialogue(@"Data\Dialogue\NPCDialogue");

            LoadCharacters(_content);
            LoadGUIs(_content);
            LoadIcons(_content);

            LoadFont(_content);
        }

        #region Load Methods
        public static void LoadCharacters(ContentManager Content)
        {
            AddTexture(Content, @"Textures\Eggplant");
            AddTexture(Content, @"Textures\Wizard");
            AddTexture(Content, @"Textures\Taylor");
            AddTexture(Content, @"Textures\NPC");
        }

        public static void LoadGUIs(ContentManager Content)
        {
            AddTexture(Content, @"Textures\cursor");
            AddTexture(Content, @"Textures\MiniInventory");
            AddTexture(Content, @"Textures\ShopWindow");
        }

        public static void LoadIcons(ContentManager Content)
        {
            AddTexture(Content, @"Textures\weapons");
            AddTexture(Content, @"Textures\ValidSquare");
            AddTexture(Content, @"Textures\ArcaneTower");
            AddTexture(Content, @"Textures\New");
            AddTexture(Content, @"Textures\Load");
            AddTexture(Content, @"Textures\tools");
            AddTexture(Content, @"Textures\rock");
            AddTexture(Content, @"Textures\ok");
            AddTexture(Content, @"Textures\Selection");
            AddTexture(Content, @"Textures\Dialog");
            AddTexture(Content, @"Textures\Text");
            AddTexture(Content, @"Textures\chest");
            AddTexture(Content, @"Textures\portraits");
            AddTexture(Content, @"Textures\tree");
            AddTexture(Content, @"Textures\items");
        }

        public static void LoadFont(ContentManager Content)
        {
            AddFont(Content, @"Fonts\DisplayFont");
            AddFont(Content, @"Fonts\Font");
        }
            #endregion

            #region AddMethods
            private static void AddTexture(ContentManager Content, string texture)
        {
            _textureDictionary.Add(texture, Content.Load<Texture2D>(texture));
        }

        private static void AddFont(ContentManager Content, string font)
        {
            _fontDictionary.Add(font, Content.Load<SpriteFont>(font));
        }
        #endregion

        #region Get Methods
        public static Texture2D GetTexture(string texture)
        {
            return _textureDictionary[texture];
        }

        public static SpriteFont GetFont(string font)
        {
            return _fontDictionary[font];
        }

        public static string GetDialogue(string key)
        {
            return _npcDialogueDictionary[key];
        }

        public static Dictionary<string, string> LoadDialogue(string file)
        {
            return _content.Load<Dictionary<string, string>>(file);
        }
        #endregion
    }
}
