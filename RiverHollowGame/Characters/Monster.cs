using Microsoft.Xna.Framework;
using RiverHollow.Game_Managers;
using RiverHollow.GUIComponents.GUIObjects;
using RiverHollow.Items;
using RiverHollow.SpriteAnimations;
using RiverHollow.Utilities;
using System;
using System.Collections.Generic;
using static RiverHollow.Game_Managers.GameManager;

namespace RiverHollow.Characters
{
    public class Monster : CombatActor
    {
        public override string Name => String.IsNullOrEmpty(_sUnique) ? _sName : _sName + " " + _sUnique;

        public int ID { get; private set; }
        int _iRating;
        int _xp;
        public int XP { get => _xp; }
        protected Vector2 _moveTo = Vector2.Zero;
        int _iLootID;

        int _iWidth;
        int _iHeight;

        public override int MaxHP => (int)((((Math.Pow(_iRating, 2)) * 10) + 20) * Math.Pow(Math.Max(1, (double)_iRating / 14), 2));

        public Monster(int id, Dictionary<string, string> data)
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
            _diAttributes[AttributeEnum.Damage] = 20 + (_iRating * 10);
            _diAttributes[AttributeEnum.Strength] = 1 + _iRating;
            _diAttributes[AttributeEnum.Defense] = 8 + (_iRating * 3);
            _diAttributes[AttributeEnum.MaxHealth] = 2 * _iRating + 10;
            _diAttributes[AttributeEnum.Magic] = 2 * _iRating + 2;
            _diAttributes[AttributeEnum.Resistance] = 2 * _iRating + 10;
            _diAttributes[AttributeEnum.Speed] = 10;

            foreach (string ability in data["Ability"].Split('-'))
            {
                Actions.Add(DataManager.GetCombatActionByIndex(int.Parse(ability)));
            }

            if (data.ContainsKey("Trait"))
            {
                //HandleTrait(DataManager.GetMonsterTraitData(data["Trait"]));
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
                idle[0] = float.Parse(split[2]);
                idle[1] = float.Parse(split[3]);
            }

            if (data.ContainsKey("Attack"))
            {
                string[] split = data["Attack"].Split('-');
                attack[0] = float.Parse(split[2]);
                attack[1] = float.Parse(split[3]);
            }

            if (data.ContainsKey("Hurt"))
            {
                string[] split = data["Hurt"].Split('-');
                hurt[0] = float.Parse(split[2]);
                hurt[1] = float.Parse(split[3]);
            }

            if (data.ContainsKey("Cast"))
            {
                string[] split = data["Cast"].Split('-');
                cast[0] = float.Parse(split[2]);
                cast[1] = float.Parse(split[3]);
            }

            if (data.ContainsKey("Loot"))
            {
                _iLootID = int.Parse(data["Loot"]);
            }

            LoadContent(DataManager.FOLDER_MONSTERS + data["Texture"], idle, attack, hurt, cast);

            _iCurrentHP = MaxHP;
        }

        public override void Update(GameTime theGameTime)
        {
            base.Update(theGameTime);
            if (BodySprite.CurrentAnimation == Util.GetEnumString(LiteCombatActionEnum.KO) && BodySprite.CurrentFrameAnimation.PlayCount == 1)
            {
                CombatManager.Kill(this);
            }
        }

        public override GUIImage GetIcon()
        {
            return new GUIImage(new Rectangle(0, 0, 18, 18), ScaleIt(18), ScaleIt(18), DataManager.GetTexture(_sCombatPortraits + "M_" + ID.ToString("00")));
        }

        private void HandleTrait(string traitData)
        {
            string[] traits = Util.FindTags(traitData);
            foreach (string s in traits)
            {
                string[] tagType = s.Split(':');
                ApplyTrait(Util.ParseEnum<AttributeEnum>(tagType[0]), tagType[1]);
            }
        }

        private void ApplyTrait(AttributeEnum e, string data)
        {
            if (data.Equals("+"))
            {
                _diAttributes[e] = (int)(_diAttributes[e] * 1.1);
            }
            else if (data.Equals("-"))
            {
                _diAttributes[e] = (int)(_diAttributes[e] * 0.9);
            }
        }

        public void LoadContent(string texture, float[] idle, float[] attack, float[] hurt, float[] cast)
        {
            _sprBody = new AnimatedSprite(texture.Replace(" ", ""));

            int xCrawl = 0;
            RHSize frameSize = new RHSize(_iWidth/TILE_SIZE, _iHeight / TILE_SIZE);

            _sprBody.AddAnimation(LiteCombatActionEnum.Idle, xCrawl * TILE_SIZE, 0, frameSize, (int)idle[0], idle[1]);
            xCrawl += (int)idle[0] + frameSize.Width;
            _sprBody.AddAnimation(LiteCombatActionEnum.Attack, xCrawl * TILE_SIZE, 0, frameSize, (int)attack[0], attack[1]);
            xCrawl += (int)attack[0] + frameSize.Width;
            _sprBody.AddAnimation(LiteCombatActionEnum.Hurt, xCrawl * TILE_SIZE, 0, frameSize, (int)hurt[0], hurt[1]);
            xCrawl += (int)hurt[0] + frameSize.Width;
            _sprBody.AddAnimation(LiteCombatActionEnum.Cast, xCrawl * TILE_SIZE, 0, frameSize, (int)cast[0], cast[1]);
            xCrawl += (int)cast[0] + frameSize.Width;

            _sprBody.AddAnimation(LiteCombatActionEnum.KO, (xCrawl * frameSize.Width), 0, frameSize, 3, 0.2f);

            _sprBody.PlayAnimation(LiteCombatActionEnum.Idle);
            _sprBody.SetScale((int)GameManager.NORMAL_SCALE);
            _iBodyWidth = frameSize.Width * (int)GameManager.NORMAL_SCALE;
            _iBodyHeight = frameSize.Height * (int)GameManager.NORMAL_SCALE;
        }

        public Item GetLoot()
        {
            return DataManager.GetItem(_iLootID);
        }
    }
}
