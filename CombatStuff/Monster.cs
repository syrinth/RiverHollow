using Microsoft.Xna.Framework;
using RiverHollow.Characters.CombatStuff;
using RiverHollow.Game_Managers;
using RiverHollow.Misc;
using static RiverHollow.Game_Managers.GameManager;

namespace RiverHollow
{
    public class Monster : CombatCharacter
    {
        #region Properties
        private int _id;
        public int ID { get => _id; }
        private int _xp;
        public int XP { get => _xp; }
        protected string _textureName;
        protected Vector2 _moveTo = Vector2.Zero;

        #endregion

        public Monster(int id, string[] stringData)
        {
            _characterType = CharacterEnum.Monster;
            ImportBasics(stringData, id);
            LoadContent(_textureName, 100, 100, 2, 0.2f);
        }

        protected void ImportBasics(string[] stringData, int id)
        {
            _id = id;
            _sName = GameContentManager.GetGameText("Monster " + _id);

            foreach (string s in stringData)
            {
                string[] tagType = s.Split(':');
                if (tagType[0].Equals("Texture"))
                {
                    _textureName = @"Textures\" + tagType[1];
                }
                else if (tagType[0].Equals("XP"))
                {
                    _xp = int.Parse(tagType[1]);
                }
                else if (tagType[0].Equals("Dmg"))
                {
                    _statDmg = int.Parse(tagType[1]);
                }
                else if (tagType[0].Equals("Def"))
                {
                    _statDef = int.Parse(tagType[1]);
                }
                else if (tagType[0].Equals("Hp"))
                {
                    _statHP = int.Parse(tagType[1]);
                }
                else if (tagType[0].Equals("Magic"))
                {
                    _statMagic = int.Parse(tagType[1]);
                }
                else if (tagType[0].Equals("Spd"))
                {
                    _statSpd = int.Parse(tagType[1]);
                }
                else if (tagType[0].Equals("Resist"))
                {
                    string[] elemSplit = tagType[1].Split('-');
                    foreach (string elem in elemSplit)
                    {
                        _diElementalAlignment[Util.ParseEnum<ElementEnum>(elem)] = ElementAlignment.Resists;
                    }
                }
                else if (tagType[0].Equals("Vuln"))
                {
                    string[] elemSplit = tagType[1].Split('-');
                    foreach (string elem in elemSplit)
                    {
                        _diElementalAlignment[Util.ParseEnum<ElementEnum>(elem)] = ElementAlignment.Vulnerable;
                    }
                }
            }

            _currentHP = MaxHP;
            _currentMP = MaxMP;
        }

        public void LoadContent(int textureWidth, int textureHeight, int numFrames, float frameSpeed)
        {
            base.LoadContent(_textureName, textureWidth, textureHeight, numFrames, frameSpeed);
        }
    }
}
