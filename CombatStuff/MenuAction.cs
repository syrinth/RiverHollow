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

        protected ActionEnum _actionType;
        protected MenuEnum _menuType;
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

        protected void ImportBasics(string[] stringData, int id)
        {
            _id = id;

            GameContentManager.GetActionText(_id, ref _name, ref _description);

            foreach (string s in stringData)
            {
                string[] tagType = s.Split(':');
                if (tagType[0].Equals("Type"))
                {
                    _actionType = Util.ParseEnum<ActionEnum>(tagType[1]);
                }
                else if (tagType[0].Equals("Menu"))
                {
                    _menuType = Util.ParseEnum<MenuEnum>(tagType[1]);
                }
            }
        }

        public bool IsMenu() { return _actionType == ActionEnum.Menu; }
        public bool IsAction() { return _actionType == ActionEnum.Action; }
        public bool IsSpell() { return _actionType == ActionEnum.Spell; }

        public bool IsCastSpell() { return _menuType == MenuEnum.Cast; }
        public bool IsUseItem() { return _menuType == MenuEnum.UseItem; }
    }

    public class CombatAction : MenuAction
    {
        const int moveSpeed = 60;

        ElementEnum _element = ElementEnum.None;
        AttackTypeEnum _attackType = AttackTypeEnum.Physical;
        List<ConditionEnum> _liCondition;
        public List<ConditionEnum> LiCondition { get => _liCondition; }
        int _iChargeCost;
        public int ChargeCost => _iChargeCost;
        int _mpCost;
        public int MPCost { get => _mpCost; }
        int _iSize;
        public int Size { get => _iSize; }
        int _effectHarm;
        public int EffectHarm { get => _effectHarm; }
        int _effectHeal;
        public int EffectHeal { get => _effectHeal; }

        ForceMoveEnum _forceMove;
        int _iMoveStr;

        TargetEnum _target;
        public TargetEnum Target { get => _target; }
        RangeEnum _range;
        public RangeEnum Range { get => _range; }
        AreaEffectEnum _areaOfEffect;
        public AreaEffectEnum AreaOfEffect { get => _areaOfEffect; }
        List<String> _actionTags;
        int _currentActionTag = 0;
        List<String> _effectTags;
        List<BuffData> _buffs;         //Key = Buff ID, string = Duration/<Tag> <Tag>

        Summon _summon;
        bool _bSummoned = false;
        bool _alreadyApplied = false;

        int _textureRow;
        float _frameSpeed;

        public List<CombatManager.CombatTile> TileTargetList;
        public Vector2 UserStartPosition;
        public bool _used;

        public AnimatedSprite Sprite;
        public CombatAction(int id, string[] stringData)
        {
            TileTargetList = new List<CombatManager.CombatTile>();
            _liCondition = new List<ConditionEnum>();
            _effectTags = new List<string>();
            _buffs = new List<BuffData>();
            _actionTags = new List<string>();

            _iChargeCost = 100;
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

        protected void ImportBasics(int id, string[] stringData)
        {
            _id = id;
            GameContentManager.GetActionText(_id, ref _name, ref _description);

            //This is where we parse for tags
            foreach (string s in stringData)
            {
                string[] tagType = s.Split(':');
                //Parsing for important data
                if (tagType[0].Equals("Type"))
                {
                    _actionType = Util.ParseEnum<ActionEnum>(tagType[1]);
                }
                else if (tagType[0].Equals("Element"))
                {
                    _element = Util.ParseEnum<ElementEnum>(tagType[1]);
                }
                else if (tagType[0].Equals("Type"))
                {
                    _attackType = Util.ParseEnum<AttackTypeEnum>(tagType[1]);
                }
                else if (tagType[0].Equals("Target"))
                {
                    _target = Util.ParseEnum<TargetEnum>(tagType[1]);
                }
                else if (tagType[0].Equals("Range"))
                {
                    _range = Util.ParseEnum<RangeEnum>(tagType[1]);
                }
                else if (tagType[0].Equals("Area"))
                {
                    _areaOfEffect = Util.ParseEnum<AreaEffectEnum>(tagType[1]);
                }
                else if (tagType[0].Equals("Charge"))
                {
                    _iChargeCost = int.Parse(tagType[1]);
                }
                else if (tagType[0].Equals("Size"))
                {
                    _iSize = int.Parse(tagType[1]);
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
                            else if (parse[0] == "Move")
                            {
                                _forceMove = Util.ParseEnum<ForceMoveEnum>(parse[1]);
                                _iMoveStr = int.Parse(parse[2]);
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
        }

        //Sets the _used tag to be true so that it's known that we've started using it
        public void AnimationSetup()
        {
            _used = true;
            TileTargetList.AddRange(CombatManager.SelectedAction.GetEffectedTiles());
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
                    foreach (CombatManager.CombatTile ct in TileTargetList)
                    {
                        int x = ct.Character.ProcessAttack(SkillUser.StatDmg, _effectHarm, _element);
                        ct.GUITile.AssignDamage(x);
                    }
                }
                else if (_effectTags.Contains("Heal"))
                {
                    foreach (CombatManager.CombatTile ct in TileTargetList)
                    {
                        int val = _effectHeal;
                        ct.Character.IncreaseHealth(val);
                        if (val > 0)
                        {
                            ct.GUITile.AssignDamage(_effectHarm);
                        }
                    }
                }
                if (_effectTags.Contains("Status"))
                {
                    foreach (CombatManager.CombatTile ct in TileTargetList)
                    {
                        foreach (ConditionEnum e in _liCondition)
                        {
                            ct.Character.ChangeConditionStatus(e, Target.Equals(TargetEnum.Enemy));
                            ct.GUITile.ChangeCondition(e, Target);
                        }
                    }
                }
                if (_effectTags.Contains("Summon") && !_bSummoned)
                {
                    foreach (CombatManager.CombatTile ct in TileTargetList)
                    {
                        _bSummoned = true;
                        ct.Character.LinkSummon(_summon);
                    }
                }

                if (_effectTags.Contains("Move"))
                {
                    foreach (CombatManager.CombatTile ct in TileTargetList)
                    {
                        bool loop = true;
                        int temp = _iMoveStr;
                        CombatManager.CombatTile test;
                        do
                        {
                            test = DetermineMovementTile(ct.GUITile.MapTile);
                            if(test != null && !test.Occupied() && test.TargetType == ct.TargetType) {
                                test.SetCombatant(ct.Character);
                                temp--;
                            }
                            else
                            {
                                loop = false;
                            }
                            if(temp == 0) { loop = false; }
                        }while(loop);
                    }
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

        private CombatManager.CombatTile DetermineMovementTile(CombatManager.CombatTile tile)
        {
            CombatManager.CombatTile rv = null;

            if (_target.Equals(TargetEnum.Ally))
            {
                if (_forceMove.Equals(ForceMoveEnum.Back)) { rv = CombatManager.GetLeft(tile.GUITile.MapTile); }
                else if (_forceMove.Equals(ForceMoveEnum.Forward)) { rv = CombatManager.GetRight(tile.GUITile.MapTile); }
            }
            else if (_target.Equals(TargetEnum.Enemy))
            {
                if (_forceMove.Equals(ForceMoveEnum.Back)) { rv = CombatManager.GetRight(tile.GUITile.MapTile); }
                else if (_forceMove.Equals(ForceMoveEnum.Forward)) { rv = CombatManager.GetLeft(tile.GUITile.MapTile); }
            }

            return rv;
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
                        //if (SkillUser.Position != TargetLocation.GetAttackVec(UserStartPosition, new Vector2(SkillUser.Width, SkillUser.Height)))
                        //{
                        //    Vector2 direction = Vector2.Zero;
                        //    Util.GetMoveSpeed(SkillUser.Position, TargetLocation.GetAttackVec(UserStartPosition, new Vector2(SkillUser.Width, SkillUser.Height)), moveSpeed, ref direction);
                        //    SkillUser.BodySprite.Position += direction;
                        //}
                        //else
                        //{
                            _currentActionTag++;
                        //
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
                        Sprite.Position = TileTargetList[0].GUITile.Position();
                    }
                    else if (Sprite.IsAnimating) { Sprite.Update(gameTime); }
                    else if (Sprite.PlayedOnce) {
                        _currentActionTag++;
                    }
                    break;
                case "Apply":
                    if (!_alreadyApplied)
                    {
                        if (!CombatManager.SelectedAction.SelfOnly()) { ApplyEffect(); }
                        else { ApplyEffectToSelf(); }

                        _alreadyApplied = true;
                    }

                    Summon s = CombatManager.ActiveCharacter.LinkedSummon;
                    if (s != null && _actionTags.Contains("UserAttack") && TileTargetList[0].Character != null && TileTargetList[0].Character.CurrentHP > 0)
                    {
                        if (!s.IsCurrentAnimation("Attack"))
                        {
                            s.PlayAnimation("Attack");
                        }
                        else if (s.AnimationPlayedXTimes(1))
                        {
                            int x = TileTargetList[0].Character.ProcessAttack(s.Dmg, 1, s.Element);
                            TileTargetList[0].GUITile.AssignDamage(x);

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
                    if(Sprite.Position != TileTargetList[0].Character.Position)
                    {
                        Vector2 direction = Vector2.Zero;
                        Util.GetMoveSpeed(Sprite.Position, TileTargetList[0].Character.Position, 80, ref direction);
                        Sprite.Position += direction;
                    }
                    else
                    {
                        _currentActionTag++;
                    }
                    break;
                case "Move":
                    TileTargetList[0].SetCombatant(SkillUser);
                    UserStartPosition = SkillUser.Position;
                    _currentActionTag++;
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
                        TileTargetList.Clear();
                        CombatManager.EndTurn();
                    }

                    break;
            }
        }

        public bool IsHelpful() { return _target == TargetEnum.Ally; }
    }

    internal struct BuffData
    {
        public int BuffID;
        public int Duration;
        public string Tags;
    }
}
