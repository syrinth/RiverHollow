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
        private int _effectHarm;
        public int EffectHarm { get => _effectHarm; }
        private int _effectHeal;
        public int EffectHeal { get => _effectHeal; }
        private Rectangle _sourceRect;
        public Rectangle SourceRect { get => _sourceRect; }
        private string _target;
        public string Target { get => _target; }
        private List<String> _effectTags;
        private List<KeyValuePair<int, string>> _buffs;         //Key = Buff ID, string = Duration/<Tag> <Tag>

        private Texture2D _texture;
        private int _textureRow;
        private float _frameSpeed;

        public CombatCharacter SkillUser;
        public Vector2 TargetPosition;
        public bool _used;

        public AnimatedSprite Sprite;
        public Ability(int id, string[] stringData)
        {
            _effectTags = new List<string>();
            _buffs = new List<KeyValuePair<int, string>>();
            ImportBasics(id, stringData);

            _texture = GameContentManager.GetTexture(@"Textures\AbilityIcons");
            Sprite = new AnimatedSprite(GameContentManager.GetTexture(@"Textures\AbilityAnimations"));
            Sprite.AddAnimation("Play", 100, 100, 4, _frameSpeed, 0, _textureRow * 100);
            Sprite.SetCurrentAnimation("Play");
            if (_effectTags.Contains("Direct"))
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
            //This is where we parse for tags

            split = stringData[i++].Split(new[] { '[', ']' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string s in split)
            {
                string[] tagType = s.Split(':');
                //Parsing for important data
                if (tagType[0].Equals("Target"))
                {
                    _target = tagType[1];
                }
                else if (tagType[0].Equals("Effect"))
                {
                    string[] tags = tagType[1].Split(' ');
                    foreach (string tag in tags)
                    {
                        string[] parse = tag.Split('-');
                        if (parse.Length > 1)
                        {
                            if (parse[0] == "Harm")
                            {
                                _effectHarm = int.Parse(parse[1]);
                            }
                            else if (parse[0] == "Heal")
                            {
                                _effectHeal = int.Parse(parse[1]);
                            }
                            _effectTags.Add(parse[0]);
                        }
                        else
                        {
                            _effectTags.Add(tag);
                        }
                    }
                }
                else if (tagType[0].Equals("Buff"))
                {
                    string[] details = tagType[1].Split('|');
                    _buffs.Add(new KeyValuePair<int, string>(int.Parse(details[0]), string.Format(@"{0}/{1}", details[1], details[2])));
                }
            }

            _id = id;

            return i;
        }

        public void PreEffect(BattleLocation target)
        {
            _used = true;
            if (_effectTags.Contains("Direct"))
            {
                Sprite.Position = target.Character.Position;
            }
            else if (_effectTags.Contains("Projectile"))
            {
                Sprite.Position = SkillUser.Position;
                TargetPosition = target.Character.Position;
            }
            else {
                Sprite.Position = new Vector2(-100, -100);
            }
        }
        public void ApplyEffectToSelf(CombatCharacter user)
        {
            if (_buffs.Count > 0)
            {
                Buff b = null;
                foreach (KeyValuePair<int, string> kvp in _buffs)
                {
                    b = CharacterManager.GetBuffByIndex(kvp.Key);
                    string[] split = kvp.Value.Split('/');
                    b.Duration = int.Parse(split[0]);
                    string[] tags = split[1].Split(' ');
                    foreach (string s in tags)
                    {
                        if (s.Equals("Self"))
                        {
                            user.AddBuff(b);
                        }
                    }
                }
            }
        }

        public void ApplyEffect(BattleLocation target, CombatCharacter user)
        {
            if (_used)
            {
                if (_effectTags.Contains("Harm"))
                {
                    target.Character.DecreaseHealth(user.StatDmg, _effectHarm);
                    target.AssignDamage(_effectHarm);
                }
                else if (_effectTags.Contains("Heal"))
                {
                    target.Character.IncreaseHealth(_effectHarm);
                    target.AssignDamage(_effectHarm);
                }

                if(_buffs.Count > 0)
                {
                    Buff b = null;
                    foreach (KeyValuePair<int, string> kvp in _buffs) {
                        b = CharacterManager.GetBuffByIndex(kvp.Key);
                        string[] split = kvp.Value.Split('/');
                        b.Duration = int.Parse(split[0]);
                        string[] tags = split[1].Split(' ');
                        foreach (string s in tags)
                        {
                            if (s.Equals("Self"))
                            {
                                user.AddBuff(b);
                            }
                        }
                    }
                }
            }
        }

        public bool IsFinished()
        {
            bool rv = false;
            if(_effectTags.Contains("Projectile") && Sprite.Position == TargetPosition)
            {
                rv = true;
            }
            else if (_effectTags.Contains("Direct"))
            {
                rv = true;
            }
            return rv;
        }

        public double GetDelay()
        {
            double rv = 0;
            if (_effectTags.Contains("Projectile"))
            {
                rv = 0.2;
            }
            else if (_effectTags.Contains("Direct"))
            {
                rv = Sprite.CurrentFrameAnimation.FrameCount * Sprite.CurrentFrameAnimation.FrameLength;
            }

            return rv;
        }
    }
}
