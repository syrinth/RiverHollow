﻿using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace RiverHollow.Game_Managers
{
    public static class GameContentManager
    {
        private static ContentManager _content;
        private static Dictionary<string, Texture2D> _diTextures;
        private static Dictionary<string, SpriteFont> _diFonts;
        private static Dictionary<string, string> _diNPCDialogue;
        private static Dictionary<string, Dictionary<int, string>> _diMerchandise;
        private static Dictionary<string, string> _diUpgrades;
        public static Dictionary<string, string> DiUpgrades { get => _diUpgrades; }

        public static void LoadContent(ContentManager Content)
        {
            _content = Content;
            _diTextures = new Dictionary<string, Texture2D>();
            _diFonts = new Dictionary<string, SpriteFont>();
            _diMerchandise = new Dictionary<string, Dictionary<int, string>>();
            _diUpgrades = _content.Load<Dictionary<string, string>>(@"Data\TownUpgrades");

            _diNPCDialogue = LoadDialogue(@"Data\Dialogue\NPCDialogue");

            LoadCharacters(_content);
            LoadGUIs(_content);
            LoadIcons(_content);
            LoadMerchandise(_content);

            LoadFont(_content);
        }

        #region Load Methods
        public static void LoadCharacters(ContentManager Content)
        {
            AddTexture(Content, @"Textures\Eggplant");
            AddTexture(Content, @"Textures\Wizard");
            AddTexture(Content, @"Textures\Knight");
            AddTexture(Content, @"Textures\Weapons Master");
            AddTexture(Content, @"Textures\NPC1");
            AddTexture(Content, @"Textures\Monsters\Goblin Scout");
            AddTexture(Content, @"Textures\Monsters\Goblin Soldier");
            AddTexture(Content, @"Textures\GoblinCombat");
            AddTexture(Content, @"Textures\GoblinSoldier");
        }

        public static void LoadGUIs(ContentManager Content)
        {
            AddTexture(Content, @"Textures\Dialog");
            AddTexture(Content, @"Textures\MiniInventory");
        }

        public static void LoadIcons(ContentManager Content)
        {
            AddTexture(Content, @"Textures\AbilityIcons");
            AddTexture(Content, @"Textures\battle");
            AddTexture(Content, @"Textures\weapons");
            AddTexture(Content, @"Textures\ArcaneTower");
            AddTexture(Content, @"Textures\Armory");
            AddTexture(Content, @"Textures\tools");
            AddTexture(Content, @"Textures\worldObjects");
            AddTexture(Content, @"Textures\chest");
            AddTexture(Content, @"Textures\portraits");
            AddTexture(Content, @"Textures\tree");
            AddTexture(Content, @"Textures\items");
            AddTexture(Content, @"Textures\AbilityAnimations");
        }

        public static void LoadFont(ContentManager Content)
        {
            AddFont(Content, @"Fonts\DisplayFont");
            AddFont(Content, @"Fonts\Font");
            AddFont(Content, @"Fonts\MenuFont");
        }
        #endregion

        public static void LoadMerchandise(ContentManager Content)
        {
            LoadMerchandiseByFile(Content, @"Data\Shops\Buildings");
            LoadMerchandiseByFile(Content, @"Data\Shops\Adventurers");
        }

        public static void LoadMerchandiseByFile(ContentManager Content, string file)
        {
            _diMerchandise.Add(file.Replace(@"Data\Shops\", ""), Content.Load<Dictionary<int, string>>(file));
        }

        #region AddMethods
        private static void AddTexture(ContentManager Content, string texture)
        {
            _diTextures.Add(texture, Content.Load<Texture2D>(texture));
        }

        private static void AddFont(ContentManager Content, string font)
        {
            _diFonts.Add(font, Content.Load<SpriteFont>(font));
        }
        #endregion

        #region Get Methods
        public static Texture2D GetTexture(string texture)
        {
            return _diTextures[texture];
        }

        public static SpriteFont GetFont(string font)
        {
            return _diFonts[font];
        }

        public static string GetDialogue(string key)
        {
            return _diNPCDialogue[key];
        }

        public static Dictionary<int, string> GetMerchandise(string file)
        {
            return _diMerchandise[file];
        }

        public static Dictionary<string, string> LoadDialogue(string file)
        {
            return _content.Load<Dictionary<string, string>>(file);
        }
        #endregion
    }
}
