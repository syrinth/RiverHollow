using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.Misc;
using System.Collections.Generic;
using System.IO;
using MonoGame.Extended.BitmapFonts;

namespace RiverHollow.Game_Managers
{
    public static class GameContentManager
    {
        public const string FILE_WORLDOBJECTS = @"Textures\worldObjects";
        public const string FILE_FLOORING = @"Textures\texFlooring";
        public const string FOLDER_ACTOR = @"Textures\Actors\";
        public const string FOLDER_BUILDINGS = @"Textures\Buildings\";
        public const string FOLDER_ITEMS = @"Textures\Items\";
        public const string FOLDER_MONSTERS = @"Textures\Actors\Monsters\";
        public const string FOLDER_PLAYER = @"Textures\Actors\Player\";
        public const string FOLDER_TEXTFILES = @"Data\Text Files\";

        private static ContentManager _content;
        private static Dictionary<string, Texture2D> _diTextures;
        private static Dictionary<string, SpriteFont> _diFonts;
        private static Dictionary<string, BitmapFont> _diBMFonts;
        private static Dictionary<string, string> _diGameText;
        private static Dictionary<int, string> _diMonsterInfo;
        private static Dictionary<int, string> _diStatusEffectText;
        private static Dictionary<string, string> _diCombatSkillsText;
        private static Dictionary<string, string> _diAdventurerDialogue;
        
        private static Dictionary<int, string> _diUpgrades;
        private static Dictionary<string, string> _diSpiritLoot;
        public static Dictionary<string, string> DiSpiritLoot { get => _diSpiritLoot; }
        public static Dictionary<int, string> DiUpgrades { get => _diUpgrades; }
        private static Dictionary<int, string> _diQuests;
        public static Dictionary<int, string> DiQuests { get => _diQuests; }
        private static Dictionary<int, string> _diItemText;
        private static Dictionary<int, string> _diClassText;
        private static Dictionary<string, string> _diMonsterTraits;

        private static Dictionary<int, List<string>> _diSongs;
        private static Dictionary<int, Dictionary<string, string>> _diNPCDialogue;
        private static Dictionary<string, Dictionary<int, string>> _diMerchandise;

        public static BitmapFont _bmFont;

        public static void LoadContent(ContentManager Content)
        {
            _content = Content;
            _diTextures = new Dictionary<string, Texture2D>();
            _diFonts = new Dictionary<string, SpriteFont>();
            _diBMFonts = new Dictionary<string, BitmapFont>();
            _diMerchandise = new Dictionary<string, Dictionary<int, string>>();
            _diSpiritLoot = _content.Load<Dictionary<string, string>>(@"Data\SpiritLoot");
            _diUpgrades = _content.Load<Dictionary<int, string>>(@"Data\TownUpgrades");
            _diQuests = _content.Load<Dictionary<int, string>>(@"Data\Quests");
            _diMonsterTraits = _content.Load<Dictionary<string, string>>(@"Data\MonsterTraitTable");

            LoadTextFiles();

            LoadCharacters();
            LoadGUIs();
            LoadIcons();
            LoadMerchandise();
            AddDirectoryTextures(FOLDER_BUILDINGS);
            AddDirectoryTextures(FOLDER_ITEMS);

            LoadFont(_content);
            LoadBMFont(_content);
        }

        #region Load Methods
        private static void LoadTextFiles()
        {
            _diItemText = _content.Load<Dictionary<int, string>>(FOLDER_TEXTFILES + "ItemText");
            _diGameText = _content.Load<Dictionary<string, string>>(FOLDER_TEXTFILES + "GameText");
            _diMonsterInfo = _content.Load<Dictionary<int, string>>(FOLDER_TEXTFILES + "MonsterInfo");
            _diStatusEffectText = _content.Load<Dictionary<int, string>>(FOLDER_TEXTFILES + "StatusText");
            _diCombatSkillsText = _content.Load<Dictionary<string, string>>(FOLDER_TEXTFILES + "CombatSkillsText");

            _diSongs = _content.Load<Dictionary<int, List<string>>>(@"Data\Songs");
            _diAdventurerDialogue = _content.Load<Dictionary<string, string>>(FOLDER_TEXTFILES + @"Dialogue\Adventurers");
            _diNPCDialogue = new Dictionary<int, Dictionary<string, string>>();
            foreach (string s in Directory.GetFiles(@"Content\" + FOLDER_TEXTFILES + "Dialogue"))
            {
                string fileName = Path.GetFileName(s).Replace("NPC", "").Split('.')[0];
                int file = -1;
                if (int.TryParse(fileName, out file))
                {
                    fileName = s;
                    Util.ParseContentFile(ref fileName);
                    _diNPCDialogue.Add(file, _content.Load<Dictionary<string, string>>(fileName));
                }
            }
        }
        private static void LoadCharacters()
        {
            AddDirectoryTextures(FOLDER_ACTOR);

            AddTexture(@"Textures\texPlayer");
            AddTexture(@"Textures\texFlooring");
            AddTexture(@"Textures\texWeather");
            AddTexture(@"Textures\lightmask");
            AddTexture(@"Textures\texPlayerHair");
            AddTexture(@"Textures\texClothes");
        }
        private static void AddDirectoryTextures(string directory, bool AddContent = true)
        {
            string folder = AddContent ? @"Content\" + directory : directory;
            foreach (string s in Directory.GetFiles(folder))
            {
                AddTexture(s);
            }
            foreach (string s in Directory.GetDirectories(folder))
            {
                AddDirectoryTextures(s, false);
            }
        }
        private static void LoadGUIs()
        {
            AddTexture(@"Textures\Dialog");
            AddTexture(@"Textures\Valley");
        }
        private static void LoadIcons()
        {
            AddDirectoryTextures(@"Textures\ActionEffects");
            AddTexture(@"Textures\battle");
            AddTexture(@"Textures\worldObjects");
            AddTexture(@"Textures\portraits");
            AddTexture(@"Textures\tree");
            AddTexture(@"Textures\DarkWoodTree");
            AddTexture(@"Textures\items");
            AddTexture(@"Textures\AbilityAnimations");
            AddTexture(@"Textures\texMachines");
            AddTexture(@"Textures\texCmbtActions");
        }
        private static void LoadFont(ContentManager Content)
        {
            AddFont(@"Fonts\DisplayFont");
            AddFont(@"Fonts\Font");
            AddFont(@"Fonts\MenuFont");
        }
        private static void LoadBMFont(ContentManager Content)
        {
            AddBMFont(@"Fonts\FontBattle");
            AddBMFont(@"Fonts\FontDefault");
        }
        private static void LoadMerchandise()
        {
            LoadMerchandiseByFile(@"Data\Shops\Buildings");
            LoadMerchandiseByFile(@"Data\Shops\Adventurers");
            LoadMerchandiseByFile(@"Data\Shops\MagicShop");
        }
        private static void LoadMerchandiseByFile(string file)
        {
            _diMerchandise.Add(file.Replace(@"Data\Shops\", ""), _content.Load<Dictionary<int, string>>(file));
        }
        #endregion

        #region Add Methods
        private static void AddTexture(string texture)
        {
            Util.ParseContentFile(ref texture);
            _diTextures.Add(texture, _content.Load<Texture2D>(texture));
        }
        private static void AddFont(string font)
        {
            _diFonts.Add(font, _content.Load<SpriteFont>(font));
        }
        private static void AddBMFont(string font)
        {
            _diBMFonts.Add(font, _content.Load<BitmapFont>(font));
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

        public static BitmapFont GetBitMapFont(string font)
        {
            return _diBMFonts[font];
        }

        public static Dictionary<string, string> GetNPCDialogue(int id)
        {
            Dictionary<string, string> rv = null;

            if (_diNPCDialogue.ContainsKey(id))
            {
                rv = _diNPCDialogue[id];
            }

            return rv;
        }

        public static string GetMonsterInfo(int id)
        {
            string rv = string.Empty;
            if (_diMonsterInfo.ContainsKey(id))
            {
                rv = _diMonsterInfo[id];
            }

            return rv;
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

        public static string GetAdventurerDialogue(string key)
        {
            string rv = string.Empty;
            if (_diAdventurerDialogue.ContainsKey(key))
            {
                rv = _diAdventurerDialogue[key];
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
        public static void GetStatusEffectText(int id, ref string name, ref string desc)
        {
            if (_diStatusEffectText.ContainsKey(id))
            {
                name = _diStatusEffectText[id].Split('/')[0];
                desc = _diStatusEffectText[id].Split('/')[1];
            }
        }

        public static List<string> GetSong(int id)
        {
            if (_diSongs.ContainsKey(id))
            {
                return _diSongs[id];
            }
            else
            {
                return null;
            }
        }

        public static void GetActionText(int id, ref string name, ref string desc)
        {
            string val = "Action " + id;
            name = _diCombatSkillsText[val].Split('/')[0];
            desc = _diCombatSkillsText[val].Split('/')[1];
        }

        public static string GetMonsterTraitData(string trait)
        {
            return _diMonsterTraits[trait];
        }
        #endregion
    }
}
