﻿using Microsoft.Xna.Framework.Graphics;
using RiverHollow.Characters;
using RiverHollow.Game_Managers;
using RiverHollow.SpriteAnimations;
using static RiverHollow.Game_Managers.GameManager;

namespace RiverHollow.CombatStuff
{
    public class Summon : Character
    {
        ElementEnum _element = ElementEnum.None;
        public ElementEnum Element => _element;
        int _iHP;
        int _iDmg;
        public int Dmg => _iDmg;

        public Summon()
        {
            _bodySprite = new AnimatedSprite(GameContentManager.GetTexture(@"Textures\Eye"));
            _bodySprite.AddAnimation("Idle", 0, 0, 16, 16, 2, 0.9f);
            _bodySprite.AddAnimation("Attack", 32, 0, 16, 16, 4, 0.1f);
            _bodySprite.SetCurrentAnimation("Idle");
            _bodySprite.SetScale(5);
        }
    }
}
