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
        public MenuAction(int id, Dictionary<string, string> stringData)
        {
            ImportBasics(stringData, id);
        }

        protected void ImportBasics(Dictionary<string, string> stringData, int id)
        {
            _id = id;
            GameContentManager.GetActionText(_id, ref _name, ref _description);

            _actionType = Util.ParseEnum<ActionEnum>(stringData["Type"]);
            _menuType = Util.ParseEnum<MenuEnum>(stringData["Menu"]);

            string[] tags = stringData["Icon"].Split('-');
            _vIconGrid = new Vector2(int.Parse(tags[0]), int.Parse(tags[1]));
        }

        public bool IsMenu() { return _actionType == ActionEnum.Menu; }
        public bool IsAction() { return _actionType == ActionEnum.Action; }
        public bool IsSpell() { return _actionType == ActionEnum.Spell; }

        public bool IsSpecial() { return _menuType == MenuEnum.Special; }
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
        int _iPotency;
        public int Potency => _iPotency;
        KeyValuePair<int, int> _kvpAreaDimensions;
        public KeyValuePair<int, int> Dimensions => _kvpAreaDimensions;

        bool _bHarm;
        public bool Harm => _bHarm;
        bool _bHeal;
        public bool Heal => _bHeal;

        string _sAnimation;

        TargetEnum _target;
        public TargetEnum Target => _target;
        RangeEnum _range;
        public RangeEnum Range => _range;
        AreaEffectEnum _areaOfEffect;
        public AreaEffectEnum AreaOfEffect => _areaOfEffect;
        List<String> _actionTags;
        int _currentActionTag = 0;
        List<SkillTagsEnum> _liEffects;
        List<StatusEffectData> _liStatusEffects;         //Key = Buff ID, string = Duration/<Tag> <Tag>

        Summon _summon;
        public Vector2 SummonStartPosition;

        float _fFrameSpeed;
        int _iAnimWidth;
        int _iAnimHeight;
        int _iFrames;
        int _iAnimOffset;

        public List<CombatManager.CombatTile> TileTargetList;
        public bool _bUsed;
        public bool _bCounter;

        bool _pauseForCounter;
        CombatActor counteringChar;
        Summon counteringSummon;

        int _iPushVal;
        int _iPullVal;
        int _iRetreatVal;
        int _iStepVal;

        public GUISprite Sprite;
        public CombatAction(int id, Dictionary<string, string> stringData)
        {
            TileTargetList = new List<CombatManager.CombatTile>();
            _liCondition = new List<ConditionEnum>();
            _liEffects = new List<SkillTagsEnum>();
            _liStatusEffects = new List<StatusEffectData>();
            _actionTags = new List<string>();

            _iChargeCost = 100;
            ImportBasics(id, stringData);
        }

        protected void ImportBasics(int id, Dictionary<string, string> stringData)
        {
            _id = id;
            GameContentManager.GetActionText(_id, ref _name, ref _description);

            _actionType = Util.ParseEnum<ActionEnum>(stringData["Type"]);
            if (stringData.ContainsKey("Element")) { _element = Util.ParseEnum<ElementEnum>(stringData["Element"]); }
            if (stringData.ContainsKey("Target")) { _target = Util.ParseEnum<TargetEnum>(stringData["Target"]); }
            if (stringData.ContainsKey("Range")) { _range = Util.ParseEnum<RangeEnum>(stringData["Range"]); }
            if (stringData.ContainsKey("Charge")) { _iChargeCost = int.Parse(stringData["Charge"]); }
            if (stringData.ContainsKey("Area")) {
                string[] tags = stringData["Area"].Split('-');
                _areaOfEffect = Util.ParseEnum<AreaEffectEnum>(tags[0]);
                if(_areaOfEffect == AreaEffectEnum.Rectangle)
                {
                    _kvpAreaDimensions = new KeyValuePair<int, int>(int.Parse(tags[1]), int.Parse(tags[2]));
                }
            }

            if (stringData.ContainsKey("Icon"))
            {
                string[] icon = stringData["Icon"].Split('-');
                _vIconGrid = new Vector2(int.Parse(icon[0]), int.Parse(icon[1]));
            }

            //Effect tags
            {
                if (stringData.ContainsKey(Util.GetEnumString(SkillTagsEnum.Harm)))
                {
                    _bHarm = true;
                    _iPotency = int.Parse(stringData[Util.GetEnumString(SkillTagsEnum.Harm)]);
                    _liEffects.Add(SkillTagsEnum.Harm);
                }
                if (stringData.ContainsKey(Util.GetEnumString(SkillTagsEnum.Heal)))
                {
                    _bHeal = true;
                    _iPotency = int.Parse(stringData[Util.GetEnumString(SkillTagsEnum.Heal)]);
                    _liEffects.Add(SkillTagsEnum.Heal);
                }

                if (stringData.ContainsKey(Util.GetEnumString(SkillTagsEnum.Push)))
                {
                    _iPushVal = int.Parse(stringData[Util.GetEnumString(SkillTagsEnum.Push)]);
                    _liEffects.Add(SkillTagsEnum.Push);
                }
                if (stringData.ContainsKey(Util.GetEnumString(SkillTagsEnum.Pull)))
                {
                    _iPullVal = int.Parse(stringData[Util.GetEnumString(SkillTagsEnum.Pull)]);
                    _liEffects.Add(SkillTagsEnum.Pull);
                }
                if (stringData.ContainsKey(Util.GetEnumString(SkillTagsEnum.Step)))
                {
                    _iStepVal = int.Parse(stringData[Util.GetEnumString(SkillTagsEnum.Step)]);
                    _liEffects.Add(SkillTagsEnum.Step);
                }
                if (stringData.ContainsKey(Util.GetEnumString(SkillTagsEnum.Retreat)))
                {
                    _iRetreatVal = int.Parse(stringData[Util.GetEnumString(SkillTagsEnum.Retreat)]);
                    _liEffects.Add(SkillTagsEnum.Retreat);
                }

                if (stringData.ContainsKey(Util.GetEnumString(SkillTagsEnum.Status)))
                {
                    string[] parse = stringData[Util.GetEnumString(SkillTagsEnum.Status)].Split('-');
                    StatusEffectData buff = new StatusEffectData() { BuffID = int.Parse(parse[0]), Duration = int.Parse(parse[1]), Tags = parse[2] };
                    if (buff.Tags == "DoT")
                    {
                        buff.Potency = _iPotency;
                    }
                    buff.Sprite = Sprite;
                    _liStatusEffects.Add(buff);
                    _liEffects.Add(SkillTagsEnum.Status);
                }

                if (stringData.ContainsKey(Util.GetEnumString(SkillTagsEnum.Bonus)))
                {
                    _bonusType = Util.ParseEnum<PotencyBonusEnum>(stringData[Util.GetEnumString(SkillTagsEnum.Bonus)]);
                    _liEffects.Add(SkillTagsEnum.Bonus);
                }

                if (stringData.ContainsKey(Util.GetEnumString(SkillTagsEnum.Summon)))
                {
                    string[] tags = stringData[Util.GetEnumString(SkillTagsEnum.Summon)].Split('-');
                    _summon = new Summon();

                    foreach (string summonTag in tags)
                    {
                        if (summonTag.Equals("TwinCast"))
                        {
                            _summon.SetTwincast();
                        }
                        else if (summonTag.Equals("Aggressive"))
                        {
                            _summon.SetAggressive();
                        }
                        else if (summonTag.Equals("Defensive"))
                        {
                            _summon.SetDefensive();
                        }
                        else if (summonTag.Equals("Counter"))
                        {
                            _summon.Counter = true;
                        }

                        if (_element != ElementEnum.None)
                        {
                            _summon.SetElement(_element);
                        }
                    }
                }
            }

            //Action tags
            if (stringData.ContainsKey("Action"))
            {
                _actionTags.AddRange(stringData["Action"].Split(' '));
                if (_actionTags.Contains("UserMove"))
                {
                    //Since we've moved, add the Return action to theend of the ability.
                    _actionTags.Add("Return");
                }
                _actionTags.Add("End");
            }

            //Animation tags
            if (stringData.ContainsKey("Animation"))
            {
                string[] parse = stringData["Animation"].Split('-');
                int i = 0;
                _sAnimation = @"Textures\ActionEffects\" + parse[i++];
                _iAnimWidth = int.Parse(parse[i++]);
                _iAnimHeight = int.Parse(parse[i++]);
                _iFrames = int.Parse(parse[i++]);
                _fFrameSpeed = float.Parse(parse[i++]);
                if (parse.Length == i + 1)
                {
                    _iAnimOffset = int.Parse(parse[i++]);
                }

                AnimatedSprite sprite = new AnimatedSprite(_sAnimation);
                sprite.AddAnimation(GenAnimEnum.Play, _iAnimWidth, _iAnimHeight, _iFrames, _fFrameSpeed);
                sprite.SetCurrentAnimation(GenAnimEnum.Play);
                sprite.IsAnimating = false;
                sprite.SetScale(CombatManager.CombatScale);
                sprite.PlaysOnce = true;

                Sprite = new GUISprite(sprite);
            }
        }

        /// <summary>
        /// Sets the _bUsed tag to be true so that it's known that we've started using the Action
        /// Then retrieve the list of tiles that will be effected by this skill
        /// </summary>
        public void AnimationSetup()
        {
            _bUsed = true;
            TileTargetList.AddRange(CombatManager.SelectedAction.GetEffectedTiles());
        }

        public void ApplyEffectToSelf()
        {
            if (_liStatusEffects.Count > 0)
            {
                StatusEffect b = null;
                foreach (StatusEffectData data in _liStatusEffects)
                {
                    b = ObjectManager.GetStatusEffectByIndex(data.BuffID);
                    b.Duration = data.Duration;
                    b.Potency = data.Potency;
                    b.Caster = SkillUser;
                    string[] tags = data.Tags.Split(' ');
                    foreach (string s in tags)
                    {
                        if (s.Equals("Self")) { SkillUser.AddStatusEffect(b); }
                        else if (s.Equals("DoT")) { b.DoT = true; }
                    }
                }
            }
        }

        public void ApplyEffect()
        {
            if (_bUsed)
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
                if (_liEffects.Contains(SkillTagsEnum.Harm))
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
                                int x = ct.Character.ProcessAttack(SkillUser, _iPotency + bonus, ct.Character.GetAttackElement());
                                ct.GUITile.AssignEffect(x, true);

                                Summon summ = ct.Character.LinkedSummon;
                                if (_areaOfEffect == AreaEffectEnum.Single && summ != null)
                                {
                                    summ.ProcessAttack(SkillUser, _iPotency + bonus, ct.Character.GetAttackElement());
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
                            int x = ct.Character.ProcessSpell(SkillUser, _iPotency, _element);
                            ct.GUITile.AssignEffect(x, true);
                        }
                    }
                }
                else if (_liEffects.Contains(SkillTagsEnum.Heal))
                {
                    foreach (CombatManager.CombatTile ct in TileTargetList)
                    {
                        int val = _iPotency;
                        ct.Character.IncreaseHealth(val);
                        if (val > 0)
                        {
                            ct.GUITile.AssignEffect(_iPotency, false);
                        }
                    }
                }
                if (_liEffects.Contains(SkillTagsEnum.Status))
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
                if (_liEffects.Contains(SkillTagsEnum.Summon))
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

                if (_liEffects.Contains(SkillTagsEnum.Push))
                {
                    foreach (CombatManager.CombatTile ct in TileTargetList)
                    {
                        Push(ct);
                    }
                }

                if (_liEffects.Contains(SkillTagsEnum.Pull))
                {
                    foreach (CombatManager.CombatTile ct in TileTargetList)
                    {
                        Pull(ct);
                    }
                }

                if (_liEffects.Contains(SkillTagsEnum.Retreat))
                {
                    Retreat(SkillUser.Tile);
                }

                if (_liEffects.Contains(SkillTagsEnum.Step))
                {
                    Step(SkillUser.Tile);
                }

                if (_liStatusEffects.Count > 0)
                {
                    StatusEffect b = null;
                    foreach (CombatManager.CombatTile ct in TileTargetList)
                    {
                        foreach (StatusEffectData data in _liStatusEffects)
                        {
                            b = ObjectManager.GetStatusEffectByIndex(data.BuffID);
                            b.Duration = data.Duration;
                            b.Caster = SkillUser;
                            b.Potency = data.Potency;
                            string[] tags = data.Tags.Split(' ');
                            foreach (string s in tags)
                            {
                                ct.Character.AddStatusEffect(b);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Attempts to reassign the occupant of the targeted tile to the
        /// tile right behind it. If there is an occupant of that tile, push
        /// themback too if possible.
        /// </summary>
        /// <param name="ct">Tile to push backf rom</param>
        /// <returns>True if the target was successfully pushed</returns>
        private bool Push(CombatManager.CombatTile ct)
        {
            bool rv = false;
            int i = _iPushVal;
            while (i > 0)
            {
                CombatManager.CombatTile test = DetermineMovementTile(ct.GUITile.MapTile, SkillTagsEnum.Push);
                if (test != null)
                {
                    if (!test.Occupied())
                    {
                        test.SetCombatant(ct.Character);
                        ct = test;
                        rv = true;
                    }
                    else if (test.Occupied())
                    {
                        if (Push(test))
                        {
                            test.SetCombatant(ct.Character);
                        }
                    }
                }
                i--;
            }

            return rv;
        }

        private bool Pull(CombatManager.CombatTile ct)
        {
            bool rv = false;
            int i = _iPullVal;
            while (i > 0)
            {
                CombatManager.CombatTile test;
                test = DetermineMovementTile(ct.GUITile.MapTile, SkillTagsEnum.Pull);
                if (test != null)
                {
                    if (!test.Occupied())
                    {
                        test.SetCombatant(ct.Character);
                        ct = test;
                        rv = true;
                    }
                }
                i--;
            }

            return rv;
        }

        private bool Step(CombatManager.CombatTile ct)
        {
            bool rv = false;
            int i = _iStepVal;
            while (i > 0)
            {
                CombatManager.CombatTile test;
                test = DetermineMovementTile(ct.GUITile.MapTile, SkillTagsEnum.Step);
                if (test != null)
                {
                    if (!test.Occupied())
                    {
                        test.SetCombatant(ct.Character);
                        ct = test;
                        rv = true;
                    }
                }
                i--;
            }

            return rv;
        }

        private bool Retreat(CombatManager.CombatTile ct)
        {
            bool rv = false;
            int i = _iRetreatVal;
            while (i > 0)
            {
                CombatManager.CombatTile test;
                test = DetermineMovementTile(ct.GUITile.MapTile, SkillTagsEnum.Retreat);
                if (test != null)
                {
                    if (!test.Occupied())
                    {
                        test.SetCombatant(ct.Character);
                        ct = test;
                        rv = true;
                    }
                }
                i--;
            }

            return rv;
        }

        /// <summary>
        /// Figures out the next tile in the given direction of the movement tag
        /// </summary>
        /// <param name="tile">The tile to move from</param>
        /// <param name="action">The skill tag describing the movement type</param>
        /// <returns></returns>
        private CombatManager.CombatTile DetermineMovementTile(CombatManager.CombatTile tile, SkillTagsEnum action)
        {
            CombatManager.CombatTile rv = null;

            //The meaning of push and pull is dependent on whether or not
            //it's an ally or enemy tile.
            if (tile.Character.IsCombatAdventurer())
            {
                if (action == SkillTagsEnum.Push) { rv = CombatManager.GetLeft(tile.GUITile.MapTile); }
                else if (action == SkillTagsEnum.Pull) { rv = CombatManager.GetRight(tile.GUITile.MapTile); }
                else if (action == SkillTagsEnum.Retreat) { rv = CombatManager.GetLeft(tile.GUITile.MapTile); }
                else if (action == SkillTagsEnum.Step) { rv = CombatManager.GetRight(tile.GUITile.MapTile); }
            }
            else
            {
                if (action == SkillTagsEnum.Push) { rv = CombatManager.GetRight(tile.GUITile.MapTile); }
                else if (action == SkillTagsEnum.Pull) { rv = CombatManager.GetLeft(tile.GUITile.MapTile); }
                else if (action == SkillTagsEnum.Retreat) { rv = CombatManager.GetRight(tile.GUITile.MapTile); }
                else if (action == SkillTagsEnum.Step) { rv = CombatManager.GetLeft(tile.GUITile.MapTile); }
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
                        if (SkillUser.IsCurrentAnimation(CActorAnimEnum.Critical)) { SkillUser.Tile.PlayAnimation(CActorAnimEnum.Idle); }

                        if (MoveSpriteTo(sprite, GetAttackTargetPosition(sprite, targetsEnemy, moveToTile)))
                        {
                            _currentActionTag++;
                        }

                        if (SkillUser.Tile.GUITile.CharacterWeaponSprite != null)
                        {
                            SkillUser.Tile.GUITile.CharacterWeaponSprite.CenterOnObject(sprite);
                        }                     

                        break;
                    }
                case "UserAttack":
                    CombatActor original = TileTargetList[0].Character;
                    if (original != null)
                    {
                        Summon summ = original.LinkedSummon;
                        if (summ != null && !summ.Swapped && summ.Defensive)
                        {
                            summ.Swapped = true;
                            Vector2 swap = summ.GetSprite().Position();
                            summ.GetSprite().Position(original.GetSprite().Position());
                            original.GetSprite().Position(swap);
                            original.Tile.TargetPlayer = false;
                        }
                    }
                    if (!SkillUser.IsCurrentAnimation(CActorAnimEnum.Attack))
                    {
                        SkillUser.Tile.PlayAnimation(CActorAnimEnum.Attack);
                    }
                    else if (SkillUser.AnimationPlayedXTimes(1))
                    {
                        SkillUser.Tile.PlayAnimation(CActorAnimEnum.Idle);
                        _currentActionTag++;
                    }
                    break;
                case "UserCast":
                    if (!SkillUser.IsCurrentAnimation(CActorAnimEnum.Cast))
                    {
                        SkillUser.Tile.PlayAnimation(CActorAnimEnum.Cast);
                    }
                    else if (SkillUser.AnimationPlayedXTimes(2))
                    {
                        SkillUser.Tile.PlayAnimation(CActorAnimEnum.Idle);
                        _currentActionTag++;
                    }
                    break;
                case "Direct":
                    if (Sprite != null && !Sprite.PlayedOnce && !Sprite.IsAnimating)
                    {
                        Sprite.IsAnimating = true;
                        Sprite.AlignToObject(TileTargetList[0].GUITile, SideEnum.Bottom);
                        Sprite.AlignToObject(TileTargetList[0].GUITile, SideEnum.CenterX);
                        Sprite.MoveBy(new Vector2(0, _iAnimOffset * CombatManager.CombatScale));
                    }
                    else if (Sprite != null && Sprite.IsAnimating) { Sprite.Update(gameTime); }
                    else if (Sprite == null || Sprite.PlayedOnce)
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
                                counteringChar.Tile.PlayAnimation(CActorAnimEnum.Attack);
                            }
                            else if (counteringChar.AnimationPlayedXTimes(1))
                            {
                                counteringChar.Tile.PlayAnimation(CActorAnimEnum.Idle);
                                int x = SkillUser.ProcessAttack(counteringChar, ((CombatAction)ObjectManager.GetActionByIndex(1)).Potency, counteringChar.GetAttackElement());
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
                                counteringSummon.PlayAnimation(CActorAnimEnum.Idle);
                                int x = SkillUser.ProcessAttack(counteringSummon, ((CombatAction)ObjectManager.GetActionByIndex(1)).Potency, counteringSummon.GetAttackElement());
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
                    if (MoveSpriteTo(SkillUser.GetSprite(), SkillUser.Tile.GUITile.GetIdleLocation(SkillUser.GetSprite())))
                    {
                        //If we're in Critical HP, go back down.
                        if (SkillUser.IsCritical()) { SkillUser.Tile.PlayAnimation(CActorAnimEnum.Critical); }
                        _currentActionTag++;
                    }

                    if (SkillUser.Tile.GUITile.CharacterWeaponSprite != null)
                    {
                        SkillUser.Tile.GUITile.CharacterWeaponSprite.CenterOnObject(SkillUser.GetSprite());
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
                    _currentActionTag++;
                    break;
                case "End":
                    _currentActionTag = 0;
                    if (Sprite != null)
                    {
                        Sprite.IsAnimating = false;
                        Sprite.PlayedOnce = false;
                    }
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
                GUICmbtTile target = CombatManager.GetLeft(moveToTile.MapTile).GUITile;
                targetPosition = target.GetIdleLocation(sprite);
            }
            else if (!targetsEnemy && TileTargetList[0].GUITile.MapTile.Col + 1 < CombatManager.ENEMY_FRONT)
            {
                GUICmbtTile target = CombatManager.GetRight(moveToTile.MapTile).GUITile;
                targetPosition = target.GetIdleLocation(sprite);
            }
            else
            {
                targetPosition = sprite.GetAnchorAndAlignToObject(moveToTile, targetsEnemy ? SideEnum.Left : SideEnum.Right, SideEnum.Bottom);
                targetPosition += new Vector2(0, -(moveToTile.Height / 3));
            }

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

        public bool TargetsEach() { return _areaOfEffect == AreaEffectEnum.Each; }
        public bool IsHelpful() { return _target == TargetEnum.Ally; }
        public bool IsSummonSpell() { return _liEffects.Contains(SkillTagsEnum.Summon); }
    }

    internal struct StatusEffectData
    {
        public int BuffID;
        public int Duration;
        public int Potency;
        public string Tags;
        public GUISprite Sprite;
    }
}
