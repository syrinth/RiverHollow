﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.CombatStuff;
using RiverHollow.Game_Managers;
using RiverHollow.Game_Managers.GUIObjects;
using RiverHollow.Misc;
using RiverHollow.SpriteAnimations;
using System;
using System.Collections.Generic;
using static RiverHollow.Game_Managers.GameManager;

namespace RiverHollow.Characters.CombatStuff
{
    public class MenuAction
    {
        protected int _id;
        public int ActionID { get => _id; }

        public enum ActionEnum { Action, Menu, Spell };
        protected ActionEnum _actionType;
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
            _actionType = Util.ParseEnum<ActionEnum>(stringData[i++]);
            _name = stringData[i++];
            _description = stringData[i++];

            return i;
        }

        public bool IsMenu() { return _actionType == ActionEnum.Menu; }
        public bool IsAction() { return _actionType == ActionEnum.Action; }
        public bool IsSpell() { return _actionType == ActionEnum.Spell; }
    }

    public class CombatAction : MenuAction
    {
        const int moveSpeed = 60;

        ElementEnum _element = ElementEnum.None;
        AttackTypeEnum _attackType = AttackTypeEnum.Physical;
        List<ConditionEnum> _liCondition;
        public List<ConditionEnum> LiCondition { get => _liCondition; }
        int _mpCost;
        public int MPCost { get => _mpCost; }
        int _effectHarm;
        public int EffectHarm { get => _effectHarm; }
        int _effectHeal;
        public int EffectHeal { get => _effectHeal; }
        string _target;
        public string Target { get => _target; }
        List<String> _actionTags;
        int _currentActionTag = 0;
        List<String> _effectTags;
        List<BuffData> _buffs;         //Key = Buff ID, string = Duration/<Tag> <Tag>

        Summon _summon;
        bool _bSummoned = false;
        bool _alreadyApplied = false;

        int _textureRow;
        float _frameSpeed;

        public BattleLocation TargetLocation;
        public Vector2 UserStartPosition;
        public bool _used;

        public AnimatedSprite Sprite;
        public CombatAction(int id, string[] stringData)
        {
            _liCondition = new List<ConditionEnum>();
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
            _actionType = Util.ParseEnum<ActionEnum>(stringData[i++]);
            _name = stringData[i++];
            _description = stringData[i++];
            //This is where we parse for tags

            string[] split = Util.FindTags(stringData[i++]);
            foreach (string s in split)
            {
                string[] tagType = s.Split(':');
                //Parsing for important data
                if (tagType[0].Equals("Element"))
                {
                    _element = Util.ParseEnum<ElementEnum>(tagType[1]);
                }
                else if (tagType[0].Equals("Type"))
                {
                    _attackType = Util.ParseEnum<AttackTypeEnum>(tagType[1]);
                }
                else if (tagType[0].Equals("Target"))
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
                            else if (parse[0] == "Status")
                            {
                                for (int j = 1; j < parse.Length; j++)
                                {
                                    _liCondition.Add(Util.ParseEnum<ConditionEnum>(parse[j]));
                                }
                            }
                            _effectTags.Add(parse[0]);
                        }
                        else
                        {
                            if (parse[0] == "Summon")
                            {
                                _summon = new Summon();
                            }

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
                else if (tagType[0].Equals("SpellAnimation"))
                {
                    string[] parse = tagType[1].Split('-');
                    if (parse.Length > 1)
                    {
                        _textureRow = int.Parse(parse[0]);
                        _frameSpeed = float.Parse(parse[1]);
                    }
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
                    int x = TargetLocation.Character.ProcessAttack(SkillUser.StatDmg, _effectHarm, _element);
                    TargetLocation.AssignDamage(x);
                }
                else if (_effectTags.Contains("Heal"))
                {
                    int val = _effectHeal;
                    TargetLocation.Character.IncreaseHealth(val);
                    if (val > 0)
                    {
                        TargetLocation.AssignDamage(_effectHarm);
                    }
                }
                if (_effectTags.Contains("Status"))
                {
                    foreach (ConditionEnum e in _liCondition)
                    {
                        TargetLocation.Character.ChangeConditionStatus(e, Target.Equals("Enemy"));
                    }
                }
                else if (_effectTags.Contains("Summon") && !_bSummoned)
                {
                    _bSummoned = true;
                    TargetLocation.Character.LinkSummon(_summon);
                }

                if (_buffs.Count > 0)
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
                            Util.GetMoveSpeed(SkillUser.Position, TargetLocation.GetAttackVec(UserStartPosition, new Vector2(SkillUser.Width, SkillUser.Height)), moveSpeed, ref direction);
                            SkillUser.BodySprite.Position += direction;
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
                    if (!_alreadyApplied)
                    {
                        if (!CombatManager.ChosenSkill.Target.Equals("Self")) { ApplyEffect(); }
                        else { ApplyEffectToSelf(); }

                        _alreadyApplied = true;
                    }

                    Summon s = CombatManager.ActiveCharacter.LinkedSummon;
                    if (s != null && _actionTags.Contains("UserAttack") && TargetLocation.Character != null && TargetLocation.Character.CurrentHP > 0)
                    {
                        if (!s.IsCurrentAnimation("Attack"))
                        {
                            s.PlayAnimation("Attack");
                        }
                        else if (s.AnimationPlayedXTimes(1))
                        {
                            int x = TargetLocation.Character.ProcessAttack(s.Dmg, 1, s.Element);
                            TargetLocation.AssignDamage(x);

                            s.PlayAnimation("Idle");
                            _alreadyApplied = false;
                            _currentActionTag++;
                        }
                    }
                    else
                    {
                        _alreadyApplied = false;
                        _currentActionTag++;
                    }
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
                        Util.GetMoveSpeed(Sprite.Position, TargetLocation.Character.Position, 80, ref direction);
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
                        Util.GetMoveSpeed(SkillUser.Position, UserStartPosition, moveSpeed, ref direction);
                        SkillUser.BodySprite.Position += direction;
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
