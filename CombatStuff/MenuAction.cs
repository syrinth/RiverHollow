using Microsoft.Xna.Framework;
using RiverHollow.Game_Managers;
using RiverHollow.Game_Managers.GUIObjects;
using RiverHollow.GUIComponents.GUIObjects.GUIWindows;
using RiverHollow.Misc;
using RiverHollow.SpriteAnimations;
using System;
using System.Collections.Generic;
using static RiverHollow.Game_Managers.GameManager;
using static RiverHollow.GUIObjects.GUIObject;

namespace RiverHollow.Actors.CombatStuff
{
    public class MenuAction
    {
        protected int _id;

        protected PotencyBonusEnum _bonusType;
        protected ActionEnum _actionType;
        protected MenuEnum _menuType;
        protected string _name;
        public string Name { get => _name; }
        protected string _description;
        public string Description { get => _description; }

        public CombatActor SkillUser;

        protected Vector2 _vIconGrid;
        public Vector2 IconGrid => _vIconGrid;

        public MenuAction() { }
        public MenuAction(int id, string[] stringData)
        {
            ImportBasics(stringData, id);
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
                else if (tagType[0].Equals("Icon"))
                {
                    string[] tags = tagType[1].Split('-');
                    _vIconGrid = new Vector2(int.Parse(tags[0]), int.Parse(tags[1]));
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
        int _iMoveDistance;

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
        public Vector2 SummonStartPosition;

        float _fFrameSpeed;
        int _iStartX;
        int _iStartY;
        int _iAnimWidth;
        int _iAnimHeight;
        int _iFrames;
        int _iAnimOffset;


        public List<CombatManager.CombatTile> TileTargetList;
        public Vector2 UserStartPosition;
        public bool _used;

        bool _pauseForCounter;
        CombatActor counteringChar;
        Summon counteringSummon;

        public GUISprite Sprite;
        public CombatAction(int id, string[] stringData)
        {
            TileTargetList = new List<CombatManager.CombatTile>();
            _liCondition = new List<ConditionEnum>();
            _effectTags = new List<string>();
            _buffs = new List<BuffData>();
            _actionTags = new List<string>();

            _iChargeCost = 100;
            ImportBasics(id, stringData);

            AnimatedSprite sprite = new AnimatedSprite(@"Textures\LightningSprite");
            sprite.AddAnimation(GenAnimEnum.Play, _iAnimWidth, _iAnimHeight, _iFrames, _fFrameSpeed, _iStartX, _iStartY);
            sprite.SetCurrentAnimation(GenAnimEnum.Play);
            sprite.IsAnimating = false;
            sprite.SetScale(CombatManager.CombatScale);
            if (_actionTags.Contains("Direct"))
            {
                sprite.PlaysOnce = true;
            }

            Sprite = new GUISprite(sprite);
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
                else if (tagType[0].Equals("Icon"))
                {
                    string[] tags = tagType[1].Split('-');
                    _vIconGrid = new Vector2(int.Parse(tags[0]), int.Parse(tags[1]));
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
                                _iMoveDistance = int.Parse(parse[2]);
                            }
                            else if (parse[0] == "Status")
                            {
                                for (int j = 1; j < parse.Length; j++)
                                {
                                    _liCondition.Add(Util.ParseEnum<ConditionEnum>(parse[j]));
                                }
                            }
                            else if (parse[0] == "Bonus")
                            {
                                _bonusType = Util.ParseEnum<PotencyBonusEnum>(parse[1]);
                            }
                            _effectTags.Add(parse[0]);
                        }
                        else
                        {
                            if (parse[0] == "Summon")
                            {
                                _summon = new Summon();

                                for (int summonTags = 2; summonTags < tags.Length; summonTags++)
                                {
                                    if (tags[summonTags].Equals("TwinCast"))
                                    {
                                        _summon.SetTwincast();
                                    }
                                    else if (tags[summonTags].Equals("Aggressive"))
                                    {
                                        _summon.SetAggressive();
                                    }
                                    else if (tags[summonTags].Equals("Defensive"))
                                    {
                                        _summon.SetDefensive();
                                    }
                                    else if (tags[summonTags].Equals("Counter"))
                                    {
                                        _summon.Counter = true;
                                    }
                                    else if (tags[summonTags].StartsWith("Element"))
                                    {
                                        _summon.SetElement(Util.ParseEnum<ElementEnum>(tags[summonTags].Split('-')[1]));
                                    }
                                }
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
                    if (parse.Length == 7)
                    {
                        int i = 0;
                        _iStartX = int.Parse(parse[i++]);
                        _iStartY = int.Parse(parse[i++]);
                        _iAnimWidth = int.Parse(parse[i++]);
                        _iAnimHeight = int.Parse(parse[i++]);
                        _iFrames = int.Parse(parse[i++]);
                        _fFrameSpeed = float.Parse(parse[i++]);
                        _iAnimOffset = int.Parse(parse[i++]);
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
                    b = ActorManager.GetBuffByIndex(data.BuffID);
                    b.Duration = data.Duration;
                    string[] tags = data.Tags.Split(' ');
                    foreach (string s in tags)
                    {
                        if (s.Equals("Self")) { SkillUser.AddBuff(b); }
                    }
                }
            }
        }

        public void ApplyEffect()
        {
            if (_used)
            {
                int bonus = 0;
                if (_bonusType != PotencyBonusEnum.None)
                {
                    if(_bonusType == PotencyBonusEnum.Summons)
                    {
                        foreach(CombatAdventurer c in PlayerManager.GetParty())
                        {
                            if(c.LinkedSummon != null)
                            {
                                bonus++;
                            }
                        }
                    }
                }
                if (_effectTags.Contains("Harm"))
                {
                    foreach (CombatManager.CombatTile ct in TileTargetList)
                    {
                        if(ct.Character == null) { continue; }

                        if (!IsSpell())
                        {
                            RHRandom random = new RHRandom();
                            int evade = random.Next(1, 100);
                            if (evade > ct.Character.Evasion)
                            {
                                int x = ct.Character.ProcessAttack(SkillUser, _effectHarm + bonus, ct.Character.GetAttackElement());
                                ct.GUITile.AssignEffect(x, true);

                                Summon summ = ct.Character.LinkedSummon;
                                if (_areaOfEffect == AreaEffectEnum.Area && summ != null)
                                {
                                    summ.ProcessAttack(SkillUser, _effectHarm + bonus, ct.Character.GetAttackElement());
                                    ct.GUITile.AssignEffectToSummon(x.ToString());
                                }

                                if (ct.Character.Counter)
                                {
                                    ct.Character.GoToCounter = true;
                                    counteringChar = ct.Character;
                                    _pauseForCounter = true;
                                }
                                if (summ != null && summ.Counter) { 
                                    summ.GoToCounter = true;
                                    counteringSummon = summ;
                                    _pauseForCounter = true;
                                }
                            }
                            else
                            {
                                ct.GUITile.AssignEffect("Dodge!", true);
                            }

                            if(ct.Character.IsSummon() && ((Summon)ct.Character).Swapped)
                            {
                                Summon s = ((Summon)ct.Character);
                                s.Swapped = false;

                                Vector2 swap = s.GetSprite().Position();
                                s.GetSprite().Position(s.linkedChar.GetSprite().Position());
                                s.linkedChar.GetSprite().Position(swap);
                                s.linkedChar.Tile.TargetPlayer = true;
                            }
                        }
                        else
                        {
                            int x = ct.Character.ProcessSpell(SkillUser, _effectHarm, _element);
                            ct.GUITile.AssignEffect(x, true);
                        }
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
                            ct.GUITile.AssignEffect(_effectHeal, false);
                        }
                    }
                }
                if (_effectTags.Contains("Status"))
                {
                    foreach (CombatManager.CombatTile ct in TileTargetList)
                    {
                        foreach (ConditionEnum e in _liCondition)
                        {
                            RHRandom random = new RHRandom();
                            int evade = random.Next(1, 100);
                            if (evade > ct.Character.ResistStatus)
                            {
                                ct.Character.ChangeConditionStatus(e, Target.Equals(TargetEnum.Enemy));
                                ct.GUITile.ChangeCondition(e, Target);
                                ct.GUITile.AssignEffect(e.ToString(), true);
                            }
                            else
                            {
                                ct.GUITile.AssignEffect("Resisted", false);
                            }
                        }
                    }
                }
                if (_effectTags.Contains("Summon"))
                {
                    foreach (CombatManager.CombatTile ct in TileTargetList)
                    {
                        Summon newSummon = _summon.Clone();
                        _summon.SetStats(SkillUser.StatMag);
                        ct.Character.Tile.GUITile.LinkSummon(newSummon);
                        ct.Character.LinkSummon(newSummon);
                        newSummon.linkedChar = ct.Character;
                    }
                }

                if (_effectTags.Contains("Move"))
                {
                    foreach (CombatManager.CombatTile ct in TileTargetList)
                    {
                        bool loop = true;
                        int temp = _iMoveDistance;
                        CombatManager.CombatTile test;
                        do
                        {
                            test = DetermineMovementTile(ct.GUITile.MapTile);
                            if (test != null && !test.Occupied() && test.TargetType == ct.TargetType)
                            {
                                test.SetCombatant(ct.Character);
                                temp--;
                            }
                            else
                            {
                                loop = false;
                            }
                            if (temp == 0) { loop = false; }
                        } while (loop);
                    }
                }

                if (_buffs.Count > 0)
                {
                    Buff b = null;
                    foreach (BuffData data in _buffs)
                    {
                        b = ActorManager.GetBuffByIndex(data.BuffID);
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
                case "Escape":
                    {
                        int spd = CombatManager.ActiveCharacter.StatSpd;
                        RHRandom r = new RHRandom();
                        if (r.Next(1, 20) + spd/2 > 15){
                            CombatManager.EndCombatEscape();
                        }
                        break;
                    }
                case "UserMove":
                    {
                        GUISprite sprite = SkillUser.GetSprite();
                        GUICmbtTile moveToTile = TileTargetList[0].GUITile;
                        bool targetsEnemy = TileTargetList[0].GUITile.MapTile.TargetType == TargetEnum.Enemy;

                        //If we're in Critical HP, start walking first.
                        if (SkillUser.IsCurrentAnimation(CActorAnimEnum.Critical)) { SkillUser.PlayAnimation(CActorAnimEnum.Walk); }

                        if (MoveSpriteTo(sprite, GetAttackTargetPosition(sprite, targetsEnemy, moveToTile)))
                        {
                            _currentActionTag++;
                        }

                        break;
                    }
                case "UserAttack":
                    CombatActor original = TileTargetList[0].Character;
                    Summon summ = original.LinkedSummon;
                    if (summ != null && !summ.Swapped && summ.Defensive)
                    {
                        summ.Swapped = true;
                        Vector2 swap = summ.GetSprite().Position();
                        summ.GetSprite().Position(original.GetSprite().Position());
                        original.GetSprite().Position(swap);
                        original.Tile.TargetPlayer = false;
                    }
                    if (!SkillUser.IsCurrentAnimation(CActorAnimEnum.Attack))
                    {
                        SkillUser.PlayAnimation(CActorAnimEnum.Attack);
                    }
                    else if (SkillUser.AnimationPlayedXTimes(1))
                    {
                        SkillUser.PlayAnimation(CActorAnimEnum.Walk);
                        _currentActionTag++;
                    }
                    break;
                case "UserCast":
                    if (!SkillUser.IsCurrentAnimation(CActorAnimEnum.Cast))
                    {
                        SkillUser.PlayAnimation(CActorAnimEnum.Cast);
                    }
                    else if (SkillUser.AnimationPlayedXTimes(2))
                    {
                        SkillUser.PlayAnimation(CActorAnimEnum.Walk);
                        _currentActionTag++;
                    }
                    break;
                case "Direct":
                    if (!Sprite.PlayedOnce && !Sprite.IsAnimating)
                    {
                        Sprite.IsAnimating = true;
                        Sprite.AlignToObject(TileTargetList[0].GUITile, SideEnum.Bottom);
                        Sprite.AlignToObject(TileTargetList[0].GUITile, SideEnum.CenterX);
                        Sprite.MoveBy(new Vector2(0, _iAnimOffset * CombatManager.CombatScale));
                    }
                    else if (Sprite.IsAnimating) { Sprite.Update(gameTime); }
                    else if (Sprite.PlayedOnce)
                    {
                        _currentActionTag++;
                    }
                    break;
                case "Apply":
                    if (!_pauseForCounter)
                    {
                        if (!CombatManager.SelectedAction.SelfOnly()) { ApplyEffect(); }
                        else { ApplyEffectToSelf(); }
                    }

                    //It's set in the above block, so we need to check again
                    if (!_pauseForCounter)
                    {
                        _currentActionTag++;
                    }
                    else
                    {
                        if(counteringChar != null)
                        {
                            if (!counteringChar.IsCurrentAnimation(CActorAnimEnum.Attack))
                            {
                                counteringChar.PlayAnimation(CActorAnimEnum.Attack);
                            }
                            else if (counteringChar.AnimationPlayedXTimes(1))
                            {
                                counteringChar.PlayAnimation(CActorAnimEnum.Walk);
                                int x = SkillUser.ProcessAttack(counteringChar, ((CombatAction)ActorManager.GetActionByIndex(1)).EffectHarm, counteringChar.GetAttackElement());
                                SkillUser.Tile.GUITile.AssignEffect(x, true);
                                counteringChar = null;
                                _pauseForCounter = false;
                                _currentActionTag++;
                            }
                        }
                        else if (counteringSummon != null)
                        {
                            if (!counteringSummon.IsCurrentAnimation(CActorAnimEnum.Attack))
                            {
                                counteringSummon.PlayAnimation(CActorAnimEnum.Attack);
                            }
                            else if (counteringSummon.AnimationPlayedXTimes(1))
                            {
                                counteringSummon.PlayAnimation(CActorAnimEnum.Walk);
                                int x = SkillUser.ProcessAttack(counteringSummon, ((CombatAction)ActorManager.GetActionByIndex(1)).EffectHarm, counteringSummon.GetAttackElement());
                                SkillUser.Tile.GUITile.AssignEffect(x, true);
                                counteringSummon = null;
                                _pauseForCounter = false;
                                _currentActionTag++;
                            }
                        }
                        else
                        {
                            _pauseForCounter = false;
                        }
                    }

                    break;
                case "Return":
                    if (MoveSpriteTo(SkillUser.GetSprite(), UserStartPosition))
                    {
                        //If we're in Critical HP, go back down.
                        if (SkillUser.IsCritical()) { SkillUser.PlayAnimation(CActorAnimEnum.Critical); }
                        _currentActionTag++;
                    }
                    break;
                case "Projectile":
                    //if (!Sprite.IsAnimating)
                    //{
                    //    Sprite.IsAnimating = true;
                    //    Sprite.Position = SkillUser.Position;
                    //}
                    //if (Sprite.Position != TileTargetList[0].Character.Position)
                    //{
                    //    Vector2 direction = Vector2.Zero;
                    //    Util.GetMoveSpeed(Sprite.Position, TileTargetList[0].Character.Position, 80, ref direction);
                    //    Sprite.Position += direction;
                    //}
                    //else
                    //{
                    //    _currentActionTag++;
                    //}
                    break;
                case "Move":
                    TileTargetList[0].SetCombatant(SkillUser);
                    if (SkillUser.LinkedSummon != null)
                    {
                        SkillUser.LinkedSummon.Tile = TileTargetList[0];
                        TileTargetList[0].GUITile.LinkSummon(SkillUser.LinkedSummon);
                    }
                    UserStartPosition = SkillUser.Position;
                    _currentActionTag++;
                    break;
                case "End":
                    _currentActionTag = 0;
                    Sprite.IsAnimating = false;
                    Sprite.PlayedOnce = false;
                    CombatManager.EndTurn();

                    break;
            }
        }

        public Vector2 GetAttackTargetPosition(GUISprite sprite, bool targetsEnemy, GUICmbtTile moveToTile)
        {
            Vector2 targetPosition = Vector2.Zero;
            int mod = targetsEnemy ? -1 : 1;

            if (targetsEnemy && TileTargetList[0].GUITile.MapTile.Col - 1 >= CombatManager.ENEMY_FRONT)
            {
                targetPosition = sprite.GetCenterOnObject(CombatManager.GetLeft(moveToTile.MapTile).GUITile);
            }
            else if (!targetsEnemy && TileTargetList[0].GUITile.MapTile.Col + 1 < CombatManager.ENEMY_FRONT)
            {
                targetPosition = sprite.GetCenterOnObject(CombatManager.GetRight(moveToTile.MapTile).GUITile);
            }
            else
            {
                targetPosition = sprite.GetAnchorAndAlignToObject(moveToTile, targetsEnemy ? SideEnum.Left : SideEnum.Right, SideEnum.Bottom);
            }
            targetPosition += new Vector2(0, -(moveToTile.Height / 3));

            return targetPosition;
        }

        public bool MoveSpriteTo(GUISprite sprite, Vector2 target)
        {
            bool rv = sprite.Position() == target;
            if (!rv)
            {
                Vector2 direction = Vector2.Zero;
                Util.GetMoveSpeed(sprite.Position(), target, moveSpeed, ref direction);
                sprite.MoveBy(direction);
            }
            else { rv = true; }

            return rv;
        }

        public bool IsHelpful() { return _target == TargetEnum.Ally; }
        public bool IsSummonSpell() { return _effectTags.Contains("Summon"); }
    }

    internal struct BuffData
    {
        public int BuffID;
        public int Duration;
        public string Tags;
    }
}
