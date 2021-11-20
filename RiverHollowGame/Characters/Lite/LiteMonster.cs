using Microsoft.Xna.Framework;
using RiverHollow.Game_Managers;
using RiverHollow.SpriteAnimations;
using RiverHollow.Utilities;
using RiverHollow.WorldObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using static RiverHollow.Game_Managers.GameManager;

namespace RiverHollow.Characters
{
    public class LiteMonster : LiteCombatActor
    {
        public int ID { get; private set; }
        int _iRating;
        int _xp;
        public int XP { get => _xp; }
        protected Vector2 _moveTo = Vector2.Zero;
        int _iLootID;

        int _iWidth;
        int _iHeight;

        public override int Attack => 20 + (_iRating * 10);

        public override int MaxHP => (int)((((Math.Pow(_iRating, 2)) * 10) + 20) * Math.Pow(Math.Max(1, (double)_iRating / 14), 2));

        public LiteMonster(int id, Dictionary<string, string> data)
        {
            _eActorType = ActorEnum.Monster;
            ImportBasics(data, id);
        }

        protected void ImportBasics(Dictionary<string, string> data, int id)
        {
            ID = id;
            DataManager.GetTextData("Monster", ID, ref _sName, "Name");

            float[] idle = new float[2] { 2, 0.5f };
            float[] attack = new float[2] { 2, 0.2f };
            float[] hurt = new float[2] { 1, 0.5f };
            float[] cast = new float[2] { 2, 0.5f };

            _iWidth = int.Parse(data["Width"]);
            _iHeight = int.Parse(data["Height"]);

            _iRating = int.Parse(data["Lvl"]);
            _xp = _iRating * 10;
            _iStrength = 1 + _iRating;
            _iDefense = 8 + (_iRating * 3);
            _iVitality = 2 * _iRating + 10;
            _iMagic = 2 * _iRating + 2;
            _iResistance = 2 * _iRating + 10;
            _iSpeed = 10;

            foreach (string ability in data["Ability"].Split('-'))
            {
                AbilityList.Add(DataManager.GetLiteActionByIndex(int.Parse(ability)));
            }

            if (data.ContainsKey("Trait"))
            {
                HandleTrait(DataManager.GetMonsterTraitData(data["Trait"]));
            }

            if (data.ContainsKey("Resist"))
            {
                foreach (string elem in data["Resist"].Split('-'))
                {
                    _diElementalAlignment[Util.ParseEnum<ElementEnum>(elem)] = ElementAlignment.Resists;
                }
            }

            if (data.ContainsKey("Vuln"))
            {
                foreach (string elem in data["Vuln"].Split('-'))
                {
                    _diElementalAlignment[Util.ParseEnum<ElementEnum>(elem)] = ElementAlignment.Vulnerable;
                }
            }

            if (data.ContainsKey("Idle"))
            {
                string[] split = data["Idle"].Split('-');
                idle[0] = float.Parse(split[0]);
                idle[1] = float.Parse(split[1]);
            }

            if (data.ContainsKey("Attack"))
            {
                string[] split = data["Attack"].Split('-');
                attack[0] = float.Parse(split[0]);
                attack[1] = float.Parse(split[1]);
            }

            if (data.ContainsKey("Hurt"))
            {
                string[] split = data["Hurt"].Split('-');
                hurt[0] = float.Parse(split[0]);
                hurt[1] = float.Parse(split[1]);
            }

            if (data.ContainsKey("Cast"))
            {
                string[] split = data["Cast"].Split('-');
                cast[0] = float.Parse(split[0]);
                cast[1] = float.Parse(split[1]);
            }

            if (data.ContainsKey("Loot"))
            {
                _iLootID = int.Parse(data["Loot"]);
            }

            LoadContent(DataManager.FOLDER_MONSTERS + data["Texture"], idle, attack, hurt, cast);

            _iCurrentHP = MaxHP;
            _iCurrentMP = MaxMP;
        }

        public override void Update(GameTime theGameTime)
        {
            base.Update(theGameTime);
            if (BodySprite.CurrentAnimation == Util.GetEnumString(LiteCombatActionEnum.KO) && BodySprite.CurrentFrameAnimation.PlayCount == 1)
            {
                LiteCombatManager.Kill(this);
            }
        }

        private void HandleTrait(string traitData)
        {
            string[] traits = Util.FindTags(traitData);
            foreach (string s in traits)
            {
                string[] tagType = s.Split(':');
                if (tagType[0].Equals(Util.GetEnumString(StatEnum.Str)))
                {
                    ApplyTrait(ref _iStrength, tagType[1]);
                }
                else if (tagType[0].Equals(Util.GetEnumString(StatEnum.Def)))
                {
                    ApplyTrait(ref _iDefense, tagType[1]);
                }
                else if (tagType[0].Equals(Util.GetEnumString(StatEnum.Vit)))
                {
                    ApplyTrait(ref _iVitality, tagType[1]);
                }
                else if (tagType[0].Equals(Util.GetEnumString(StatEnum.Mag)))
                {
                    ApplyTrait(ref _iMagic, tagType[1]);
                }
                else if (tagType[0].Equals(Util.GetEnumString(StatEnum.Res)))
                {
                    ApplyTrait(ref _iResistance, tagType[1]);
                }
                else if (tagType[0].Equals(Util.GetEnumString(StatEnum.Spd)))
                {
                    ApplyTrait(ref _iSpeed, tagType[1]);
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

        public void LoadContent(string texture, float[] idle, float[] attack, float[] hurt, float[] cast)
        {
            _sprBody = new AnimatedSprite(texture.Replace(" ", ""));

            int xCrawl = 0;
            RHSize frameSize = new RHSize(_iWidth, _iHeight);

            _sprBody.AddAnimation(LiteCombatActionEnum.Idle, (xCrawl * frameSize.Width), 0, frameSize, (int)idle[0], idle[1]);
            xCrawl += (int)idle[0];
            _sprBody.AddAnimation(LiteCombatActionEnum.Attack, (xCrawl * frameSize.Width), 0, frameSize, (int)attack[0], attack[1]);
            xCrawl += (int)attack[0];
            _sprBody.AddAnimation(LiteCombatActionEnum.Hurt, (xCrawl * frameSize.Width), 0, frameSize, (int)hurt[0], hurt[1]);
            xCrawl += (int)hurt[0];
            _sprBody.AddAnimation(LiteCombatActionEnum.Cast, (xCrawl * frameSize.Width), 0, frameSize, (int)cast[0], cast[1]);
            xCrawl += (int)cast[0];

            _sprBody.AddAnimation(LiteCombatActionEnum.KO, (xCrawl * frameSize.Width), 0, frameSize, 3, 0.2f);

            _sprBody.PlayAnimation(LiteCombatActionEnum.Idle);
            _sprBody.SetScale(LiteCombatManager.CombatScale);
            _iBodyWidth = _sprBody.Width;
            _iBodyHeight = _sprBody.Height;
        }

        public Item GetLoot()
        {
            return DataManager.GetItem(_iLootID);
        }
    }
}
