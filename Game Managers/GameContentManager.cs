using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.Misc;
using System.Collections.Generic;
using System.IO;

namespace RiverHollow.Game_Managers
{
    public static class GameContentManager
    {
        public const string ACTOR_FOLDER = @"Textures\Actors\";
        public const string BUILDING_FOLDER = @"Textures\Buildings\";
        public const string ITEM_FOLDER = @"Textures\Items\";
        public const string PLAYER_FOLDER = @"Textures\Actors\Player\";

        private static ContentManager _content;
        private static Dictionary<string, Texture2D> _diTextures;
        private static Dictionary<string, SpriteFont> _diFonts;
        private static Dictionary<string, string> _diGameText;
        private static Dictionary<string, Dictionary<int, string>> _diMerchandise;
        private static Dictionary<int, string> _diUpgrades;
        private static Dictionary<string, string> _diSpiritLoot;
        public static Dictionary<string, string> DiSpiritLoot { get => _diSpiritLoot; }
        public static Dictionary<int, string> DiUpgrades { get => _diUpgrades; }
        private static Dictionary<int, string> _diQuests;
        public static Dictionary<int, string> DiQuests { get => _diQuests; }
        private static Dictionary<int, string> _diItemText;
        private static Dictionary<int, string> _diClassText;
        private static Dictionary<string, string> _diMonsterTraits;

        public static void LoadContent(ContentManager Content)
        {
            _content = Content;
            _diTextures = new Dictionary<string, Texture2D>();
            _diFonts = new Dictionary<string, SpriteFont>();
            _diMerchandise = new Dictionary<string, Dictionary<int, string>>();
            _diSpiritLoot = _content.Load<Dictionary<string, string>>(@"Data\SpiritLoot");
            _diUpgrades = _content.Load<Dictionary<int, string>>(@"Data\TownUpgrades");
            _diQuests = _content.Load<Dictionary<int, string>>(@"Data\Quests");
            _diItemText = _content.Load<Dictionary<int, string>>(@"Data\ItemText");
            _diMonsterTraits = _content.Load<Dictionary<string, string>>(@"Data\MonsterTraitTable");

            _diGameText = LoadDialogue(@"Data\Dialogue\GameText");

            LoadCharacters(_content);
            LoadGUIs(_content);
            LoadIcons(_content);
            LoadMerchandise(_content);
            AddDirectoryTextures(Content, BUILDING_FOLDER);
            AddDirectoryTextures(Content, ITEM_FOLDER);

            LoadFont(_content);
        }

        #region Load Methods
        public static void LoadCharacters(ContentManager Content)
        {
            AddDirectoryTextures(Content, ACTOR_FOLDER);

            AddTexture(Content, @"Textures\texPlayer");
            AddTexture(Content, @"Textures\texFlooring");
            AddTexture(Content, @"Textures\texWeather");
            AddTexture(Content, @"Textures\lightmask");
            AddTexture(Content, @"Textures\texPlayerHair");
            AddTexture(Content, @"Textures\texClothes");
            AddTexture(Content, @"Textures\Eye");
        }

        private static void AddDirectoryTextures(ContentManager Content, string directory, bool AddContent = true)
        {
            string folder = AddContent ? @"Content\" + directory : directory;
            foreach (string s in Directory.GetFiles(folder))
            {
                AddTexture(Content, s);
            }
            foreach (string s in Directory.GetDirectories(folder))
            {
                AddDirectoryTextures(Content, s, false);
            }
        }

        public static void LoadGUIs(ContentManager Content)
        {
            AddTexture(Content, @"Textures\Dialog");
        }

        public static void LoadIcons(ContentManager Content)
        {
            AddDirectoryTextures(Content, @"Textures\ActionEffects");
            AddTexture(Content, @"Textures\battle");
            AddTexture(Content, @"Textures\weapons");
            AddTexture(Content, @"Textures\armor");
            AddTexture(Content, @"Textures\tools");
            AddTexture(Content, @"Textures\worldObjects");
            AddTexture(Content, @"Textures\portraits");
            AddTexture(Content, @"Textures\tree");
            AddTexture(Content, @"Textures\DarkWoodTree");
            AddTexture(Content, @"Textures\items");
            AddTexture(Content, @"Textures\AbilityAnimations");
            AddTexture(Content, @"Textures\texMachines");
            AddTexture(Content, @"Textures\texCmbtActions");
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
            LoadMerchandiseByFile(Content, @"Data\Shops\MagicShop");
        }

        public static void LoadMerchandiseByFile(ContentManager Content, string file)
        {
            _diMerchandise.Add(file.Replace(@"Data\Shops\", ""), Content.Load<Dictionary<int, string>>(file));
        }

        #region AddMethods
        private static void AddTexture(ContentManager Content, string texture)
        {
            Util.ParseContentFile(ref texture);
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

        public static string GetGameText(string key)
        {
            string rv = string.Empty;
            if (_diGameText.ContainsKey(key))
            {
                rv =  _diGameText[key];
            }

            return rv;
        }

        public static Dictionary<int, string> GetMerchandise(string file)
        {
            return _diMerchandise[file];
        }

        public static void GetItemText(int id, ref string name, ref string desc)
        {
            name = _diItemText[id].Split('/')[0];
            desc = _diItemText[id].Split('/')[1];
        }

        public static void GetBuildingText(int id, ref string name, ref string desc)
        {
            string val = "Building " + id;
            name = _diGameText[val].Split('/')[0];
            desc = _diGameText[val].Split('/')[1];
        }
        public static void GetClassText(int id, ref string name, ref string desc) {
            string val = "Class " + id;
            name = _diGameText[val].Split('/')[0];
            desc = _diGameText[val].Split('/')[1];
        }
        public static void GetQuestText(int id, ref string name, ref string desc)
        {
            string val = "Quest " + id;
            name = _diGameText[val].Split('/')[0];
            desc = _diGameText[val].Split('/')[1];
        }
        public static void GetUpgradeText(int id, ref string name, ref string desc)
        {
            string val = "Upgrade " + id;
            name = _diGameText[val].Split('/')[0];
            desc = _diGameText[val].Split('/')[1];
        }

        public static void GetActionText(int id, ref string name, ref string desc)
        {
            string val = "Action " + id;
            name = _diGameText[val].Split('/')[0];
            desc = _diGameText[val].Split('/')[1];
        }

        public static string GetMonsterTraitData(string trait)
        {
            return _diMonsterTraits[trait];
        }

        public static Dictionary<string, string> LoadDialogue(string file)
        {
            return _content.Load<Dictionary<string, string>>(file);
        }
        #endregion
    }
}
