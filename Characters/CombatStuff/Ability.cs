using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.Game_Managers;
using RiverHollow.Game_Managers.GUIObjects;
using RiverHollow.SpriteAnimations;
using System;
using System.Collections.Generic;

namespace RiverHollow.Characters.CombatStuff
{
    public class Ability
    {
        private int _id;
        private string _name;
        public string Name { get => _name; }
        private string _description;
        public string Description { get => _description; }
        private int _effect;
        public int Effect { get => _effect; }
        private Rectangle _sourceRect;
        public Rectangle SourceRect { get => _sourceRect; }
        private List<String> _abiltyTags;

        private Texture2D _texture;
        private int _textureRow;
        private float _frameSpeed;

        public CombatCharacter SkillUser;
        public Vector2 TargetPosition;
        public bool _used;

        public AnimatedSprite Sprite;
        public Ability(int id, string[] stringData)
        {
            _abiltyTags = new List<string>();
            ImportBasics(id, stringData);

            _texture = GameContentManager.GetTexture(@"Textures\AbilityIcons");
            Sprite = new AnimatedSprite(GameContentManager.GetTexture(@"Textures\AbilityAnimations"));
            Sprite.AddAnimation("Play", 100, 100, 4, _frameSpeed, 0, _textureRow * 100);
            Sprite.SetCurrentAnimation("Play");
            if (_abiltyTags.Contains("Direct"))
            {
                Sprite.PlaysOnce = true;
            }
        }

        protected int ImportBasics(int id, string[] stringData)
        {
            int i = 0;
            _name = stringData[i++];
            _description = stringData[i++];
            string[] split = stringData[i++].Split(' ');
            int x = int.Parse(split[0]);
            int y = int.Parse(split[1]);
            _sourceRect = new Rectangle(x * 100, y * 100, 100, 100);
            _textureRow = int.Parse(stringData[i++]);
            _frameSpeed = float.Parse(stringData[i++]);
            split = stringData[i++].Split(' ');
            foreach (string s in split)
            {
                //Parsing for important data
                if (s.Contains(":"))
                {
                    string[] parse = s.Split(':');
                    if (parse[0] == "Harm" || parse[0] == "Heal")
                    {
                        _effect = int.Parse(parse[1]);
                    }
                    _abiltyTags.Add(parse[0]);
                }
                else
                {
                    _abiltyTags.Add(s);
                }

            }

            _id = id;

            return i;
        }

        public void PreEffect(BattleLocation target)
        {
            _used = true;
            if (_abiltyTags.Contains("Direct"))
            {
                Sprite.Position = target.Character.Position;
            }
            else if (_abiltyTags.Contains("Projectile"))
            {
                Sprite.Position = SkillUser.Position;
                TargetPosition = target.Character.Position;
            }
            else {
                Sprite.Position = new Vector2(-100, -100);
            }
        }

        public void ApplyEffect(BattleLocation target)
        {
            if (_used)
            {
                if (_abiltyTags.Contains("Harm"))
                {
                    target.Character.DecreaseHealth(_effect);
                    target.AssignDamage(_effect);
                }
                else if (_abiltyTags.Contains("Heal"))
                {
                    target.Character.IncreaseHealth(_effect);
                    target.AssignDamage(_effect);
                }
            }
        }

        public bool IsFinished()
        {
            bool rv = false;
            if(_abiltyTags.Contains("Projectile") && Sprite.Position == TargetPosition)
            {
                rv = true;
            }
            else if (_abiltyTags.Contains("Direct"))
            {
                rv = true;
            }
            return rv;
        }

        public double GetDelay()
        {
            double rv = 0;
            if (_abiltyTags.Contains("Projectile"))
            {
                rv = 0.2;
            }
            else if (_abiltyTags.Contains("Direct"))
            {
                rv = Sprite.CurrentFrameAnimation.FrameCount * Sprite.CurrentFrameAnimation.FrameLength;
            }

            return rv;
        }
    }
}
