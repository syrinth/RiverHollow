using Microsoft.Xna.Framework;
using RiverHollow.Actors.CombatStuff;
using RiverHollow.Game_Managers;
using RiverHollow.Misc;
using static RiverHollow.Game_Managers.GameManager;

namespace RiverHollow
{
    public class Monster : CombatCharacter
    {
        #region Properties
        int _id;
        public int ID { get => _id; }
        int _iLvl;
        int _xp;
        public int XP { get => _xp; }
        protected string _textureName;
        protected Vector2 _moveTo = Vector2.Zero;

        #endregion

        public Monster(int id, string[] stringData)
        {
            _actorType = ActorEnum.Monster;
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
                else if (tagType[0].Equals("Lvl"))
                {
                    _iLvl = int.Parse(tagType[1]);
                    _xp = _iLvl * 10;
                    _statStr = 2 * _iLvl + 10;
                    _statDef = 2 * _iLvl + 10;
                    _statVit = (3 * _iLvl) + 80;
                    _statMag = 2 * _iLvl + 10;
                    _statRes = 2 * _iLvl + 10;
                    _statSpd = 10;
                }
                else if (tagType[0].Equals("Trait"))
                {
                    HandleTrait(GameContentManager.GetMonsterTraitData(tagType[1]));
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

        private void HandleTrait(string traitData)
        {
            string[] traits = Util.FindTags(traitData);
            foreach(string s in traits)
            {
                string[] tagType = s.Split(':');
                if (tagType[0].Equals("Str"))
                {
                    ApplyTrait(ref _statStr, tagType[1]);
                }
                else if (tagType[0].Equals("Def"))
                {
                    ApplyTrait(ref _statDef, tagType[1]);
                }
                else if (tagType[0].Equals("Vit"))
                {
                    ApplyTrait(ref _statVit, tagType[1]);
                }
                else if (tagType[0].Equals("Mag"))
                {
                    ApplyTrait(ref _statMag, tagType[1]);
                }
                else if (tagType[0].Equals("Res"))
                {
                    ApplyTrait(ref _statRes, tagType[1]);
                }
                else if (tagType[0].Equals("Spd"))
                {
                    ApplyTrait(ref _statSpd, tagType[1]);
                }
            }
        }

        private void ApplyTrait(ref int value, string data)
        {
            if (data.Equals("+"))
            {
                value = (int)(value * 1.1);
            }
            else if (data.Equals("-"))
            {
                value = (int)(value * 0.9);
            }
        }
    }
}
