using Microsoft.Xna.Framework;
using RiverHollow.Game_Managers;
using RiverHollow.GUIComponents.GUIObjects;
using RiverHollow.Items;
using RiverHollow.Utilities;
using System;
using System.Collections.Generic;
using static RiverHollow.Game_Managers.GameManager;

namespace RiverHollow.Characters
{
    public class Monster : CombatActor
    {
        public override string Name => String.IsNullOrEmpty(_sUnique) ? _sName : _sName + " " + _sUnique;

        public int ID { get; } = -1;
        int _iRating;
        public int XP { get; private set; }
        protected Vector2 _moveTo = Vector2.Zero;
        int _iLootID;

        public override int MaxHP => Attribute(AttributeEnum.Vitality) * 2 + (int)Math.Pow(_iRating, 2.65);

        public Monster(int id, Dictionary<string, string> data)
        {
            ID = id;

            _eActorType = ActorEnum.Monster;
            ImportBasics(data);
        }

        protected void ImportBasics(Dictionary<string, string> data)
        {

            DataManager.GetTextData("Monster", ID, ref _sName, "Name");

            Util.AssignValue(ref _iBodyWidth, "Width", data);
            Util.AssignValue(ref _iBodyHeight, "Height", data);

            _iRating = int.Parse(data["Lvl"]);
            XP = _iRating * 10;
            _diAttributes[AttributeEnum.Damage] = 5 + (_iRating * 10);
            _diAttributes[AttributeEnum.Vitality] = 5 + (_iRating * 5);

            _diAttributes[AttributeEnum.Strength] = 10 + (_iRating * 5);
            _diAttributes[AttributeEnum.Agility] = 10 + (_iRating * 5);
            _diAttributes[AttributeEnum.Magic] = 10 + (_iRating * 5);

            _diAttributes[AttributeEnum.Defense] = 5 + (_iRating * 5);
            _diAttributes[AttributeEnum.Resistance] = 5 + (_iRating * 5);
            _diAttributes[AttributeEnum.Evasion] = 5;
            _diAttributes[AttributeEnum.Speed] = 5;

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

            LoadSpriteAnimations(ref _sprBody, Util.LoadCombatAnimations(data), DataManager.FOLDER_MONSTERS + data["Texture"]);

            CurrentHP = MaxHP;
        }

        public override void Update(GameTime theGameTime)
        {
            base.Update(theGameTime);
            
            if (BodySprite.CurrentAnimation == Util.GetEnumString(AnimationEnum.KO) && BodySprite.CurrentFrameAnimation.PlayCount == 1)
            {
                CombatManager.RemoveMonster(this);
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

        public Item GetLoot()
        {
            return DataManager.GetItem(_iLootID);
        }
    }
}
