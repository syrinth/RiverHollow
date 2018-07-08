using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace RiverHollow.Game_Managers
{
    public static class GameContentManager
    {
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
            _diClassText = _content.Load<Dictionary<int, string>>(@"Data\ClassesText");

            _diGameText = LoadDialogue(@"Data\Dialogue\GameText");

            LoadCharacters(_content);
            LoadGUIs(_content);
            LoadIcons(_content);
            LoadMerchandise(_content);

            LoadFont(_content);
        }

        #region Load Methods
        public static void LoadCharacters(ContentManager Content)
        {
            AddTexture(Content, @"Textures\texPlayer");
            AddTexture(Content, @"Textures\Wizard");
            AddTexture(Content, @"Textures\Healer");
            AddTexture(Content, @"Textures\Knight");
            AddTexture(Content, @"Textures\Champion");
            AddTexture(Content, @"Textures\Summoner");
            AddTexture(Content, @"Textures\Rogue");
            AddTexture(Content, @"Textures\Bard");
            AddTexture(Content, @"Textures\Witch");
            AddTexture(Content, @"Textures\NPC1");
            AddTexture(Content, @"Textures\NPC8");
            AddTexture(Content, @"Textures\Monsters\Goblin Scout");
            AddTexture(Content, @"Textures\Monsters\Goblin Soldier");
            AddTexture(Content, @"Textures\GoblinCombat");
            AddTexture(Content, @"Textures\GoblinSoldier");
            AddTexture(Content, @"Textures\texFlooring");
            AddTexture(Content, @"Textures\NPCs\Spirit_Forest_2");
            AddTexture(Content, @"Textures\NPCs\Spirit_Water_1");
            AddTexture(Content, @"Textures\texWeather");
            AddTexture(Content, @"Textures\lightmask");
            AddTexture(Content, @"Textures\texPlayerHair");
            AddTexture(Content, @"Textures\texClothes");
            AddTexture(Content, @"Textures\Eye");
        }

        public static void LoadGUIs(ContentManager Content)
        {
            AddTexture(Content, @"Textures\Dialog");
            AddTexture(Content, @"Textures\MiniInventory");
        }

        public static void LoadIcons(ContentManager Content)
        {
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
            AddTexture(Content, @"Textures\texMachines");
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
            return _diGameText[key];
        }

        public static Dictionary<int, string> GetMerchandise(string file)
        {
            return _diMerchandise[file];
        }

        public static void GetIemText(int id, ref string name, ref string desc)
        {
            name = _diItemText[id].Split('/')[0];
            desc = _diItemText[id].Split('/')[1];
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

        public static Dictionary<string, string> LoadDialogue(string file)
        {
            return _content.Load<Dictionary<string, string>>(file);
        }
        #endregion
    }
}
