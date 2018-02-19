using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.Game_Managers;
using RiverHollow.Game_Managers.GUIObjects;
using RiverHollow.Misc;
using RiverHollow.SpriteAnimations;
using System;
using System.Collections.Generic;

namespace RiverHollow.Characters.CombatStuff
{
    public class MenuAction
    {
        protected int _id;
        public int ActionID { get => _id; }

        public enum ActionType { Action, Menu, Spell };
        protected ActionType _actionType;
        protected string _name;
        public string Name { get => _name; }
        protected string _description;
        public string Description { get => _description; }

        public CombatCharacter SkillUser;

        public MenuAction() { }
        public MenuAction(int id, string[] stringData)
        {
            ImportBasics(stringData,id);
        }

        protected int ImportBasics(string[] stringData, int id)
        {
            _id = id;
            int i = 0;
            _actionType = (ActionType)Enum.Parse(typeof(ActionType), stringData[i++]);
            _name = stringData[i++];
            _description = stringData[i++];

            return i;
        }

        public bool IsMenu() { return _actionType == ActionType.Menu; }
        public bool IsAction() { return _actionType == ActionType.Action; }
        public bool IsSpell() { return _actionType == ActionType.Spell; }
    }

    public class CombatAction : MenuAction
    {
        const int moveSpeed = 60;

        private int _mpCost;
        public int MPCost { get => _mpCost; }
        private int _effectHarm;
        public int EffectHarm { get => _effectHarm; }
        private int _effectHeal;
        public int EffectHeal { get => _effectHeal; }
        private string _target;
        public string Target { get => _target; }
        private List<String> _actionTags;
        private int _currentActionTag = 0;
        private List<String> _effectTags;
        private List<BuffData> _buffs;         //Key = Buff ID, string = Duration/<Tag> <Tag>

        private int _textureRow;
        private float _frameSpeed;

       
        public BattleLocation TargetLocation;
        public Vector2 UserStartPosition;
        public bool _used;

        public AnimatedSprite Sprite;
        public CombatAction(int id, string[] stringData)
        {
            _effectTags = new List<string>();
            _buffs = new List<BuffData>();
            _actionTags = new List<string>();
            ImportBasics(id, stringData);

            Sprite = new AnimatedSprite(GameContentManager.GetTexture(@"Textures\AbilityAnimations"));
            Sprite.AddAnimation("Play", 100, 100, 4, _frameSpeed, 0, _textureRow * 100);
            Sprite.SetCurrentAnimation("Play");
            Sprite.IsAnimating = false;
            if (_actionTags.Contains("Direct"))
            {
                Sprite.PlaysOnce = true;
            }
        }

        protected int ImportBasics(int id, string[] stringData)
        {
            int i = 0;
            _actionType = (ActionType)Enum.Parse(typeof(ActionType), stringData[i++]);
            _name = stringData[i++];
            _description = stringData[i++];
            _textureRow = int.Parse(stringData[i++]);
            _frameSpeed = float.Parse(stringData[i++]);
            //This is where we parse for tags

            string[] split = stringData[i++].Split(new[] { '[', ']' }, StringSplitOptions.RemoveEmptyEntries);
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
                            else if (parse[0] == "Cost")
                            {
                                _mpCost = int.Parse(parse[1]);
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
                    _buffs.Add(new BuffData() { BuffID = int.Parse(details[0]), Duration = int.Parse(details[1]), Tags = details[2] });
                }
                else if (tagType[0].Equals("Action"))
                {
                    _actionTags.AddRange(tagType[1].Split(' '));
                    _actionTags.Add("End");
                }
            }

            _id = id;

            return i;
        }

        //Sets the _used tag to be true so that it's known that we've started using it
        public void AnimationSetup(BattleLocation target)
        {
            _used = true;
            TargetLocation = target;
        }

        public void ApplyEffectToSelf()
        {
            if (_buffs.Count > 0)
            {
                Buff b = null;
                foreach (BuffData data in _buffs)
                {
                    b = CharacterManager.GetBuffByIndex(data.BuffID);
                    b.Duration = data.Duration;
                    string[] tags = data.Tags.Split(' ');
                    foreach (string s in tags)
                    {
                        if (s.Equals("Self")) {SkillUser.AddBuff(b); }
                    }
                }
            }
        }

        public void ApplyEffect()
        {
            if (_used)
            {
                if (_effectTags.Contains("Harm"))
                {
                    int x = TargetLocation.Character.DecreaseHealth(SkillUser.StatDmg, _effectHarm);
                    TargetLocation.AssignDamage(x);
                }
                else if (_effectTags.Contains("Heal"))
                {
                    TargetLocation.Character.IncreaseHealth(_effectHarm);
                    TargetLocation.AssignDamage(_effectHarm);
                }

                if(_buffs.Count > 0)
                {
                    Buff b = null;
                    foreach (BuffData data in _buffs) {
                        b = CharacterManager.GetBuffByIndex(data.BuffID);
                        b.Duration = data.Duration;
                        string[] tags = data.Tags.Split(' ');
                        foreach (string s in tags)
                        {
                            if (s.Equals("Self"))
                            {
                                SkillUser.AddBuff(b);
                            }
                        }
                    }
                }
            }
        }

        //public double GetDelay()
        //{
        //    double rv = 0;
        //    if (_effectTags.Contains("Projectile"))
        //    {
        //        rv = 0.2;
        //    }
        //    else if (_effectTags.Contains("Direct"))
        //    {
        //        rv = Sprite.CurrentFrameAnimation.FrameCount * Sprite.CurrentFrameAnimation.FrameLength;
        //    }

        //    return rv;
        //}

        public void HandlePhase(GameTime gameTime)
        {
            switch (_actionTags[_currentActionTag])
            {
                case "UserMove":
                    {
                        if (SkillUser.Position != TargetLocation.GetAttackVec(UserStartPosition, new Vector2(SkillUser.Width, SkillUser.Height)))
                        {
                            Vector2 direction = Vector2.Zero;
                            Utilities.GetMoveSpeed(SkillUser.Position, TargetLocation.GetAttackVec(UserStartPosition, new Vector2(SkillUser.Width, SkillUser.Height)), moveSpeed, ref direction);
                            SkillUser.Sprite.Position += direction;
                        }
                        else
                        {
                            _currentActionTag++;
                        }
                        break;
                    }
                case "UserAttack":
                    if (!SkillUser.IsCurrentAnimation("Attack"))
                    {
                        SkillUser.PlayAnimation("Attack");
                    }
                    else if (SkillUser.AnimationPlayedXTimes(1))
                    {
                        SkillUser.PlayAnimation("Walk");
                        _currentActionTag++;
                    }
                    break;
                case "UserCast":
                    if (!SkillUser.IsCurrentAnimation("Cast"))
                    {
                        SkillUser.PlayAnimation("Cast");
                    }
                    else if (SkillUser.AnimationPlayedXTimes(2))
                    {
                        SkillUser.PlayAnimation("Walk");
                        _currentActionTag++;
                    }
                    break;
                case "Direct":
                    if (!Sprite.PlayedOnce && !Sprite.IsAnimating)
                    {
                        Sprite.IsAnimating = true;
                        Sprite.Position = TargetLocation.Character.Position;
                    }
                    else if (Sprite.IsAnimating) { Sprite.Update(gameTime); }
                    else if (Sprite.PlayedOnce) {
                        _currentActionTag++;
                    }
                    break;
                case "Apply":
                    ApplyEffect();
                    _currentActionTag++;
                    break;
                case "Projectile":
                    if (!Sprite.IsAnimating)
                    {
                        Sprite.IsAnimating = true;
                        Sprite.Position = SkillUser.Position;
                    }
                    if(Sprite.Position != TargetLocation.Character.Position)
                    {
                        Vector2 direction = Vector2.Zero;
                        Utilities.GetMoveSpeed(Sprite.Position, TargetLocation.Character.Position, 80, ref direction);
                        Sprite.Position += direction;
                    }
                    else
                    {
                        _currentActionTag++;
                    }
                    break;
                case "End":
                    if (SkillUser.Position != UserStartPosition)
                    {
                        Vector2 direction = Vector2.Zero;
                        Utilities.GetMoveSpeed(SkillUser.Position, UserStartPosition, moveSpeed, ref direction);
                        SkillUser.Sprite.Position += direction;
                    }
                    else
                    {
                        _currentActionTag = 0;
                        Sprite.IsAnimating = false;
                        Sprite.PlayedOnce = false;
                        CombatManager.NextTurn();
                    }

                    break;
            }
        }
    }

    internal struct BuffData
    {
        public int BuffID;
        public int Duration;
        public string Tags;
    }
}
