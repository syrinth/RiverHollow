using Microsoft.Xna.Framework;
using RiverHollow.Actors.CombatStuff;
using RiverHollow.Characters;
using RiverHollow.Characters.Lite;
using RiverHollow.Game_Managers;
using RiverHollow.GUIComponents.GUIObjects;
using RiverHollow.GUIComponents.GUIObjects.GUIWindows;
using RiverHollow.SpriteAnimations;
using RiverHollow.Utilities;
using System;
using System.Collections.Generic;
using static RiverHollow.Game_Managers.GameManager;
using static RiverHollow.GUIComponents.GUIObjects.GUIObject;

namespace RiverHollow.CombatStuff
{
    public class LiteCombatAction : LiteMenuAction
    {
        const int moveSpeed = 60;

        PotencyBonusEnum _eBonusType;
        ElementEnum _eElement = ElementEnum.None;
        List<ConditionEnum> _liCondition;
        public List<ConditionEnum> LiCondition { get => _liCondition; }
        int _iChargeCost;
        public int ChargeCost => _iChargeCost;
        int _iMPCost = 0;
        public int MPCost => _iMPCost;
        public int Potency { get; private set; }

        int _iReqLevel = 0;
        public int ReqLevel => _iReqLevel;

        private RHSize _uSize;
        public RHSize Dimensions => _uSize;
        public bool Harm { get; private set; }
        public bool Heal { get; private set; }

        string _sAnimation;
        VerbEnum _eUserAnimationVerb = VerbEnum.Action1;
        TargetEnum _eTarget;
        public TargetEnum Target => _eTarget;
        RangeEnum _eRange;
        public RangeEnum Range => _eRange;
        AreaTypeEnum _eAreaType;
        public AreaTypeEnum AreaType => _eAreaType;
        List<String> _liActionTags;
        int _iCurrentAction = 0;
        List<SkillTagsEnum> _liEffects;
        List<LiteStatusEffectData> _liStatusEffects;         //Key = Buff ID, string = Duration/<Tag> <Tag>

        int _iSummonID;
        public Vector2 SummonStartPosition;

        bool _bSummonSpell = false;
        float _fFrameSpeed;
        int _iAnimWidth;
        int _iAnimHeight;
        int _iFrames;
        int _iAnimOffsetX;
        int _iAnimOffsetY;
        int _iBonusMod;

        public List<LiteCombatTile> TileTargetList;
        public bool _bCounter;

        bool _bPauseActionHandler;
        LiteCombatActor counteringChar;
        LiteSummon counteringSummon;

        int _iPushVal;
        int _iPullVal;
        int _iRetreatVal;
        int _iStepVal;

        int _iCritRating;
        int _iAccuracy;

        string _sRemoveString;

        public GUISprite Sprite;
        public LiteCombatAction(int id, Dictionary<string, string> stringData)
        {
            TileTargetList = new List<LiteCombatTile>();
            _liCondition = new List<ConditionEnum>();
            _liEffects = new List<SkillTagsEnum>();
            _liStatusEffects = new List<LiteStatusEffectData>();
            _liActionTags = new List<string>();

            _iChargeCost = 100;
            ImportBasics(id, stringData);
        }

        protected void ImportBasics(int id, Dictionary<string, string> stringData)
        {
            _iId = id;
            DataManager.GetTextData("Action", _iId, ref _sName, "Name");
            DataManager.GetTextData("Action", _iId, ref _sDescription, "Description");

            Util.AssignValue(ref _eActionType, "Type", stringData);
            Util.AssignValue(ref _eElement, "Element", stringData);
            Util.AssignValue(ref _eTarget, "Target", stringData);
            Util.AssignValue(ref _eAreaType, "AreaType", stringData);
            Util.AssignValue(ref _eRange, "LiteRange", stringData);
            Util.AssignValue(ref _iCritRating, "Crit", stringData);
            Util.AssignValue(ref _iChargeCost, "Charge", stringData);
            Util.AssignValue(ref _iAccuracy, "Accuracy", stringData);
            Util.AssignValue(ref _iMPCost, "Cost", stringData);
            Util.AssignValue(ref _iReqLevel, "Level", stringData);
            Util.AssignValue(ref _uSize, "Dimensions", stringData);

            Util.AssignValue(ref _eUserAnimationVerb, "UserAnimation", stringData);

            if (stringData.ContainsKey("Icon"))
            {
                string[] icon = stringData["Icon"].Split('-');
                _vIconGrid = new Vector2(int.Parse(icon[0]), int.Parse(icon[1]));
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

                AnimatedSprite ActionSprite = new AnimatedSprite(_sAnimation);
                ActionSprite.AddAnimation(AnimationEnum.PlayAnimation, 0, 0, _iAnimWidth, _iAnimHeight, _iFrames, _fFrameSpeed, false, true);
                ActionSprite.Drawing = false;
                ActionSprite.SetScale(LiteCombatManager.CombatScale);

                Sprite = new GUISprite(ActionSprite);
            }

            //Effect tags
            {
                if (stringData.ContainsKey(Util.GetEnumString(SkillTagsEnum.Harm)))
                {
                    Harm = true;
                    Potency = int.Parse(stringData[Util.GetEnumString(SkillTagsEnum.Harm)]);
                    _liEffects.Add(SkillTagsEnum.Harm);
                }
                if (stringData.ContainsKey(Util.GetEnumString(SkillTagsEnum.Heal)))
                {
                    Heal = true;
                    Potency = int.Parse(stringData[Util.GetEnumString(SkillTagsEnum.Heal)]);
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
                if (stringData.ContainsKey(Util.GetEnumString(SkillTagsEnum.Remove)))
                {
                    _sRemoveString = stringData[Util.GetEnumString(SkillTagsEnum.Remove)];
                    _liEffects.Add(SkillTagsEnum.Remove);
                }

                if (stringData.ContainsKey(Util.GetEnumString(SkillTagsEnum.StatusEffectID)))
                {
                    string[] parse = stringData[Util.GetEnumString(SkillTagsEnum.StatusEffectID)].Split('-');

                    LiteStatusEffectData statEffect = new LiteStatusEffectData() { BuffID = int.Parse(parse[0]), Duration = int.Parse(parse[1]), Tags = parse[2] };
                    statEffect.Sprite = Sprite;
                    _liStatusEffects.Add(statEffect);
                    _liEffects.Add(SkillTagsEnum.StatusEffectID);
                }

                if (stringData.ContainsKey(Util.GetEnumString(SkillTagsEnum.Bonus)))
                {
                    string[] split = stringData[Util.GetEnumString(SkillTagsEnum.Bonus)].Split('-');
                    _eBonusType = Util.ParseEnum<PotencyBonusEnum>(split[0]);
                    _iBonusMod = int.Parse(split[1]);
                    _liEffects.Add(SkillTagsEnum.Bonus);
                }

                if (stringData.ContainsKey(Util.GetEnumString(SkillTagsEnum.NPC_ID)))
                {
                    _bSummonSpell = true;
                    _iSummonID = int.Parse(stringData[Util.GetEnumString(SkillTagsEnum.NPC_ID)]);
                }
            }

            //Action tags
            if (stringData.ContainsKey("Action"))
            {
                _liActionTags.AddRange(stringData["Action"].Split(' '));
                if (_liActionTags.Contains("UserMove"))
                {
                    //Since we've moved, add the Return action to theend of the ability.
                    _liActionTags.Add("Return");
                }
                _liActionTags.Add("End");
            }

            if (stringData.ContainsKey("AnimOffset"))
            {
                string[] parse = stringData["AnimOffset"].Split('-');
                _iAnimOffsetX = int.Parse(parse[0]);
                _iAnimOffsetY = int.Parse(parse[1]);
            }
        }

        /// <summary>
        /// Sets the _bUsed tag to be true so that it's known that we've started using the Action
        /// Then retrieve the list of tiles that will be effected by this skill
        /// </summary>
        public void AnimationSetup()
        {
            TileTargetList.AddRange(LiteCombatManager.SelectedAction.GetEffectedTiles());
        }

        /// <summary>
        /// This method applies the effects of the CombatAction upon the appropriate targets
        /// We do this by checking to see if specific tags are int he _liEffects list and performing
        /// the appropriate action
        /// </summary>
        public void ApplyEffect()
        {
            //If the action has some type of bonus associated, we need to figure out what it is
            //and get the appropriate bonus
            int bonus = 0;
            if (_eBonusType != PotencyBonusEnum.None)
            {
                //Bonus Summons increased the bonus amount for each summon attached to the party
                //Note that this currently does not support enemy summons
                if (_eBonusType == PotencyBonusEnum.Summon)
                {
                    foreach (LitePartyMember c in LiteCombatManager.Party)
                    {
                        if (c.LinkedSummon != null)
                        {
                            bonus += _iBonusMod;
                        }
                    }
                }
            }

            //This tag means that the action harms whatever is targetted
            if (_liEffects.Contains(SkillTagsEnum.Harm))
            {
                int totalPotency = Potency + bonus;
                //Iterate over each tile in the target list
                foreach (LiteCombatTile ct in TileTargetList)
                {
                    LiteCombatTile targetTile = ct;
                    LiteCombatActor targetActor = ct.Character;
                    //If the tile is unoccupied, don't do anything
                    if (targetActor == null) { continue; }

                    //If the target has a guard, swap them
                    if (targetActor.MyGuard != null)
                    {
                        targetActor = targetActor.MyGuard;
                        targetTile = targetActor.Tile;
                    }

                    //Lot more logic has to go into skills then spells
                    if (!Compare(ActionEnum.Spell))
                    {
                        //Roll randomly between 1-100 to determine the chance of hir
                        RHRandom random = RHRandom.Instance();
                        int attackRoll = random.Next(1, 100);
                        attackRoll -= _iAccuracy;                       //Modify the chance to hit by the skill's accuracy. Rolling low is good, so subtract a positive and add a negative
                        if (attackRoll <= 90 - targetActor.Evasion)    //If the modified attack roll is less than 90 minus the character's evasion, then we hit
                        {
                            int x = targetActor.ProcessAttack(SkillUser, totalPotency, _iCritRating, targetActor.GetAttackElement());
                            targetTile.GUITile.AssignEffect(x, true);

                            //If the target has a Summon linked to them, and they take
                            //any area damage, hit the Summon as well
                            LiteSummon summ = targetActor.LinkedSummon;
                            if (_eAreaType != AreaTypeEnum.Single && summ != null)
                            {
                                summ.ProcessAttack(SkillUser, Potency + bonus, _iCritRating, targetActor.GetAttackElement());
                                targetTile.GUITile.AssignEffectToSummon(x.ToString());
                            }

                            #region Countering setup
                            //If the target has Counter turn on, prepare to counterattack
                            //Only counter melee attacks
                            if (_eRange == RangeEnum.Melee)
                            {
                                if (targetActor.Counter)
                                {
                                    targetActor.GoToCounter = true;    //Sets the character to be ready for countering
                                    counteringChar = targetActor;      //Sets who the countering character is
                                    _bPauseActionHandler = true;            //Tells the game to pause to allow for a counter
                                }

                                //If there is a summon and it can counter, prepare it for countering.
                                if (summ != null && summ.Counter)
                                {
                                    summ.GoToCounter = true;
                                    counteringSummon = summ;
                                    _bPauseActionHandler = true;
                                }
                            }
                            #endregion
                        }
                        else    //Handling for when an attack is dodged
                        {
                            targetTile.GUITile.AssignEffect("Dodge!", true);
                        }

                        //This code handles when someone is guarding the target and takes the damage for them instead
                        //Damage has already been applied above, so we need to unset the Swapped flag, and reset the
                        //image positions of the guard and its guarded character.
                        if (targetActor.Guard && targetActor.Swapped)
                        {
                            LiteCombatActor actor = targetActor;
                            actor.Swapped = false;

                            //Put the GuardTarget back firstin case the guard is a Summon
                            actor.GuardTarget.GetSprite().Position(actor.GuardTarget.Tile.GUITile.GetIdleLocation(actor.GuardTarget.GetSprite()));

                            if (actor.IsSummon()) { actor.GetSprite().Position(actor.GuardTarget.Tile.GUITile.GetIdleSummonLocation()); }
                            else { actor.GetSprite().Position(actor.Tile.GUITile.GetIdleLocation(actor.GetSprite())); }

                            actor.GuardTarget.MyGuard = null;
                            actor.GuardTarget = null;
                        }
                    }
                    else   //Handling for spells
                    {
                        //Process the damage of the spell, then apply it to the targeted tile
                        int x = targetActor.ProcessSpell(SkillUser, totalPotency, _eElement);
                        targetTile.GUITile.AssignEffect(x, true);
                    }
                }
            }
            else if (_liEffects.Contains(SkillTagsEnum.Heal))       //Handling for healing
            {
                foreach (LiteCombatTile ct in TileTargetList)
                {
                    int val = ct.Character.ProcessHealingSpell(SkillUser, Potency);
                    if (val > 0)    //Don't bother saying it healed for 0
                    {
                        ct.GUITile.AssignEffect(Potency, false);
                    }
                }
            }

            //Handles when the action applies status effects
            if (_liEffects.Contains(SkillTagsEnum.StatusEffectID))
            {
                foreach (LiteStatusEffectData effect in _liStatusEffects)
                {
                    //Makes a new StatusEffectobject from the data held in the action
                    StatusEffect status = DataManager.GetStatusEffectByIndex(effect.BuffID);
                    status.Duration = effect.Duration;
                    status.AssignCaster(SkillUser);

                    //Search for relevant tags, primarily targetting
                    string[] tags = effect.Tags.Split(' ');
                    foreach (string s in tags)
                    {
                        //Handle the targetting of where to send the status effect
                        List<LiteCombatActor> targets = new List<LiteCombatActor>();

                        //Apply it to self
                        if (s.Equals("Self"))
                        {
                            targets.Add(SkillUser);
                        }

                        //Apply to all targets of the skill
                        if (s.Equals("Target"))
                        {
                            foreach (LiteCombatTile ct in TileTargetList)
                            {
                                if (ct.Character != null && !ct.Character.KnockedOut())
                                {
                                    targets.Add(ct.Character);
                                }
                            }
                        }

                        //Apply to all allies of the user
                        if (s.Equals("Allies"))
                        {
                            if (SkillUser.IsActorType(ActorEnum.PartyMember))
                            {
                                targets.AddRange(LiteCombatManager.Party);
                            }
                            else
                            {
                                targets.AddRange(LiteCombatManager.Monsters);
                            }
                        }

                        //Apply to all enemies of the user
                        if (s.Equals("Enemies"))
                        {
                            if (SkillUser.IsActorType(ActorEnum.PartyMember))
                            {
                                targets.AddRange(LiteCombatManager.Monsters);

                            }
                            else
                            {
                                targets.AddRange(LiteCombatManager.Party);
                            }
                        }

                        //actualyl apply the StatusEffect to the targets
                        foreach (LiteCombatActor actor in targets)
                        {
                            actor.AddStatusEffect(status);
                        }
                    }
                }
            }

            //Handler for if the action Summons something
            if (IsSummonSpell())
            {
                //This should only ever be one, butjust in case
                foreach (LiteCombatTile ct in TileTargetList)
                {
                    LiteSummon newSummon = DataManager.GetSummonByIndex(_iSummonID);
                    newSummon.SetStats(SkillUser.StatMag);                //Summon stats are based off the Magic stat
                    ct.Character.LinkSummon(newSummon);                 //Links the summon to the character
                    newSummon.linkedChar = ct.Character;                //Links the character to the new summon
                    _bPauseActionHandler = true;
                }
            }

            //Handler to push back the target
            if (_liEffects.Contains(SkillTagsEnum.Push))
            {
                foreach (LiteCombatTile ct in TileTargetList)
                {
                    if (ct.Character != null && !ct.Character.KnockedOut())
                    {
                        Push(ct);
                    }
                }
            }

            //Handler to pull the target forwards
            if (_liEffects.Contains(SkillTagsEnum.Pull))
            {
                foreach (LiteCombatTile ct in TileTargetList)
                {
                    if (ct.Character != null && !ct.Character.KnockedOut())
                    {
                        Pull(ct);
                    }
                }
            }

            //Handler for the SkillUser to retreat
            if (_liEffects.Contains(SkillTagsEnum.Retreat))
            {
                Retreat(SkillUser.Tile);
            }

            //Handler for the SkillUser to move forwrd
            if (_liEffects.Contains(SkillTagsEnum.Step))
            {
                Step(SkillUser.Tile);
            }

            //Handler for when the action removes things
            if (_liEffects.Contains(SkillTagsEnum.Remove))
            {
                string strRemovewhat = _sRemoveString.Split('-')[0];
                string strArea = _sRemoveString.Split('-')[1];

                if (strRemovewhat == "Summons")
                {
                    if (strArea == "Every")
                    {
                        foreach (LitePartyMember adv in LiteCombatManager.Party)
                        {
                            adv.UnlinkSummon();
                        }
                    }
                }
            }
        }

        #region Combat Action Movement
        /// <summary>
        /// Attempts to reassign the occupant of the targeted tile to the
        /// tile right behind it. If there is an occupant of that tile, push
        /// themback too if possible.
        /// </summary>
        /// <param name="ct">Tile to push backf rom</param>
        /// <returns>True if the target was successfully pushed</returns>
        private bool Push(LiteCombatTile ct)
        {
            bool rv = false;
            int i = _iPushVal;
            while (i > 0)
            {
                LiteCombatTile test = DetermineMovementTile(ct.GUITile.MapTile, SkillTagsEnum.Push);
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

        private bool Pull(LiteCombatTile ct)
        {
            bool rv = false;
            int i = _iPullVal;
            while (i > 0)
            {
                LiteCombatTile test;
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

        private bool Step(LiteCombatTile ct)
        {
            bool rv = false;
            int i = _iStepVal;
            while (i > 0)
            {
                LiteCombatTile test;
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

        private bool Retreat(LiteCombatTile ct)
        {
            bool rv = false;
            int i = _iRetreatVal;
            while (i > 0)
            {
                LiteCombatTile test;
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
        private LiteCombatTile DetermineMovementTile(LiteCombatTile tile, SkillTagsEnum action)
        {
            LiteCombatTile rv = null;

            //The meaning of push and pull is dependent on whether or not
            //it's an ally or enemy tile.
            if (tile.Character.IsActorType(ActorEnum.PartyMember))
            {
                if (action == SkillTagsEnum.Push) { rv = LiteCombatManager.GetLeft(tile.GUITile.MapTile); }
                else if (action == SkillTagsEnum.Pull) { rv = LiteCombatManager.GetRight(tile.GUITile.MapTile); }
                else if (action == SkillTagsEnum.Retreat) { rv = LiteCombatManager.GetLeft(tile.GUITile.MapTile); }
                else if (action == SkillTagsEnum.Step) { rv = LiteCombatManager.GetRight(tile.GUITile.MapTile); }
            }
            else
            {
                if (action == SkillTagsEnum.Push) { rv = LiteCombatManager.GetRight(tile.GUITile.MapTile); }
                else if (action == SkillTagsEnum.Pull) { rv = LiteCombatManager.GetLeft(tile.GUITile.MapTile); }
                else if (action == SkillTagsEnum.Retreat) { rv = LiteCombatManager.GetRight(tile.GUITile.MapTile); }
                else if (action == SkillTagsEnum.Step) { rv = LiteCombatManager.GetLeft(tile.GUITile.MapTile); }
            }

            return rv;
        }
        #endregion

        /// <summary>
        /// The method that makes the abilities actually do things. Each ability has a list of action tags
        /// that describe, in order, what steps need to take place to have the ability function.
        /// 
        /// We maintain an index to which action is the current action so we know which action we are currently processing
        /// </summary>
        /// <param name="gameTime"></param>
        public void HandlePhase(GameTime gameTime)
        {
            switch (_liActionTags[_iCurrentAction])
            {
                case "Escape":
                    {
                        int spd = LiteCombatManager.ActiveCharacter.StatSpd;
                        if (RHRandom.Instance().Next(1, 20) + spd / 2 > 15)
                        {
                            LiteCombatManager.EndCombatEscape();
                        }
                        break;
                    }
                case "UserMove":
                    {
                        GUISprite sprite = SkillUser.GetSprite();
                        GUICombatTile moveToTile = TileTargetList[0].GUITile;
                        bool targetsEnemy = TileTargetList[0].GUITile.MapTile.TargetType == TargetEnum.Enemy;

                        //If we're in Critical HP, start walking first.
                        if (SkillUser.IsCurrentAnimation(LiteCombatActionEnum.Critical)) { SkillUser.Tile.PlayAnimation(LiteCombatActionEnum.Idle); }

                        if (MoveSpriteTo(sprite, GetAttackTargetPosition(sprite, targetsEnemy, moveToTile)))
                        {
                            _iCurrentAction++;
                        }

                        if (!SkillUser.IsSummon() && SkillUser.Tile.GUITile.CharacterWeaponSprite != null)
                        {
                            SkillUser.Tile.GUITile.CharacterWeaponSprite.CenterOnObject(sprite);
                        }

                        break;
                    }
                case "UserAttack":
                    foreach (LiteCombatTile tile in TileTargetList)
                    {
                        List<LiteCombatActor> liPotentialGuards = new List<LiteCombatActor>();
                        LiteCombatActor original = tile.Character;

                        //Only assign a guard to the character if they are not guarding and they have no guard
                        if (original != null && !original.Guard & original.MyGuard == null)
                        {
                            LiteCombatTile top = LiteCombatManager.GetTop(tile);
                            if (top != null && top.Character != null && top.Character.Guard) { liPotentialGuards.Add(top.Character); }

                            LiteCombatTile bottom = LiteCombatManager.GetBottom(tile);
                            if (bottom != null && bottom.Character != null && bottom.Character.Guard) { liPotentialGuards.Add(bottom.Character); }

                            LiteSummon summ = tile.Character.LinkedSummon;
                            if (summ != null && !summ.Swapped && summ.Guard) { liPotentialGuards.Add(summ); }

                            if (liPotentialGuards.Count > 0)
                            {
                                int which = RHRandom.Instance().Next(0, liPotentialGuards.Count - 1);

                                LiteCombatActor guard = liPotentialGuards[which];
                                guard.Swapped = true;
                                guard.GuardTarget = original;
                                original.MyGuard = guard;

                                guard.GetSprite().Position(original.GetSprite().Position());
                                original.GetSprite().PositionSub(new Vector2(100, 0));
                            }
                        }
                    }

                    if (!SkillUser.IsCurrentAnimation(LiteCombatActionEnum.Attack))
                    {
                        if (SkillUser.IsSummon()) { SkillUser.PlayAnimation(LiteCombatActionEnum.Attack); }
                        else { SkillUser.Tile.PlayAnimation(LiteCombatActionEnum.Attack); }
                    }
                    else if (SkillUser.AnimationPlayedXTimes(1))
                    {
                        if (SkillUser.IsSummon()) { SkillUser.PlayAnimation(LiteCombatActionEnum.Idle); }
                        else { SkillUser.Tile.PlayAnimation(LiteCombatActionEnum.Idle); }
                        _iCurrentAction++;
                    }
                    break;
                case "UserCast":
                    if (!SkillUser.IsCurrentAnimation(LiteCombatActionEnum.Cast))
                    {
                        if (SkillUser.IsSummon()) { SkillUser.PlayAnimation(LiteCombatActionEnum.Cast); }
                        else { SkillUser.Tile.PlayAnimation(LiteCombatActionEnum.Cast); }
                    }
                    else if (SkillUser.AnimationPlayedXTimes(2))
                    {
                        if (SkillUser.IsSummon()) { SkillUser.PlayAnimation(LiteCombatActionEnum.Idle); }
                        else { SkillUser.Tile.PlayAnimation(LiteCombatActionEnum.Idle); }
                        _iCurrentAction++;
                    }
                    break;
                case "Direct":
                    if (Sprite != null && !Sprite.PlayedOnce && !Sprite.IsAnimating)
                    {
                        Sprite.IsAnimating = true;
                        Sprite.AlignToObject(TileTargetList[0].GUITile, SideEnum.Bottom);
                        Sprite.AlignToObject(TileTargetList[0].GUITile, SideEnum.CenterX);
                    }
                    else if (Sprite != null && Sprite.IsAnimating) { Sprite.Update(gameTime); }
                    else if (Sprite == null || Sprite.PlayedOnce)
                    {
                        Sprite.Reset();
                        _iCurrentAction++;
                    }
                    break;
                case "Apply":
                    if (!_bPauseActionHandler)
                    {
                        ApplyEffect();
                    }

                    //It's set in the above block, so we need to check again
                    if (!_bPauseActionHandler)
                    {
                        _iCurrentAction++;
                    }
                    else
                    {
                        if (counteringChar == null && counteringSummon == null && TileTargetList[0].Character.LinkedSummon != null)
                        {
                            if (TileTargetList[0].Character.LinkedSummon.BodySprite.CurrentAnimation == "Idle")
                            {
                                _bPauseActionHandler = false;
                                _iCurrentAction++;
                            }
                        }
                        else
                        {
                            if (counteringChar != null)
                            {
                                if (!counteringChar.IsCurrentAnimation(LiteCombatActionEnum.Attack))
                                {
                                    counteringChar.Tile.PlayAnimation(LiteCombatActionEnum.Attack);
                                }
                                else if (counteringChar.AnimationPlayedXTimes(1))
                                {
                                    counteringChar.Tile.PlayAnimation(LiteCombatActionEnum.Idle);
                                    int x = SkillUser.ProcessAttack(counteringChar, ((LiteCombatAction)DataManager.GetLiteActionByIndex(1)).Potency, _iCritRating, counteringChar.GetAttackElement());
                                    SkillUser.Tile.GUITile.AssignEffect(x, true);
                                    counteringChar = null;
                                    _bPauseActionHandler = false;
                                    _iCurrentAction++;
                                }
                            }
                            else if (counteringSummon != null)
                            {
                                if (!counteringSummon.IsCurrentAnimation(LiteCombatActionEnum.Attack))
                                {
                                    counteringSummon.PlayAnimation(LiteCombatActionEnum.Attack);
                                }
                                else if (counteringSummon.AnimationPlayedXTimes(1))
                                {
                                    counteringSummon.PlayAnimation(LiteCombatActionEnum.Idle);
                                    int x = SkillUser.ProcessAttack(counteringSummon, ((LiteCombatAction)DataManager.GetLiteActionByIndex(1)).Potency, _iCritRating, counteringSummon.GetAttackElement());
                                    SkillUser.Tile.GUITile.AssignEffect(x, true);
                                    counteringSummon = null;
                                    _bPauseActionHandler = false;
                                    _iCurrentAction++;
                                }
                            }
                            else
                            {
                                _bPauseActionHandler = false;
                            }
                        }
                    }

                    break;
                case "Return":
                    Vector2 moveTo = !SkillUser.IsSummon() ? SkillUser.Tile.GUITile.GetIdleLocation(SkillUser.GetSprite()) : SkillUser.Tile.GUITile.GetIdleSummonLocation();
                    if (MoveSpriteTo(SkillUser.GetSprite(), moveTo))
                    {
                        //If we're in Critical HP, go back down.
                        if (SkillUser.IsCritical())
                        {
                            if (SkillUser.IsSummon()) { SkillUser.PlayAnimation(LiteCombatActionEnum.Critical); }
                            else { SkillUser.Tile.PlayAnimation(LiteCombatActionEnum.Critical); }
                        }
                        _iCurrentAction++;
                    }

                    if (!SkillUser.IsSummon() && SkillUser.Tile.GUITile.CharacterWeaponSprite != null)
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
                    _iCurrentAction++;
                    break;
                case "End":
                    _iCurrentAction = 0;
                    if (Sprite != null)
                    {
                        Sprite.IsAnimating = false;
                    }
                    LiteCombatManager.EndTurn();

                    break;
            }
        }

        public Vector2 GetAttackTargetPosition(GUISprite sprite, bool targetsEnemy, GUICombatTile moveToTile)
        {
            Vector2 targetPosition = Vector2.Zero;
            int mod = targetsEnemy ? -1 : 1;

            if (targetsEnemy && TileTargetList[0].GUITile.MapTile.Col - 1 >= LiteCombatManager.ENEMY_FRONT)
            {
                GUICombatTile target = LiteCombatManager.GetLeft(moveToTile.MapTile).GUITile;
                targetPosition = target.GetIdleLocation(sprite);
            }
            else if (!targetsEnemy && TileTargetList[0].GUITile.MapTile.Col + 1 < LiteCombatManager.ENEMY_FRONT)
            {
                GUICombatTile target = LiteCombatManager.GetRight(moveToTile.MapTile).GUITile;
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

        public bool TargetsEach() { return _eAreaType == AreaTypeEnum.All; }
        public bool IsHelpful() { return _eTarget == TargetEnum.Ally; }
        public bool IsSummonSpell() { return _bSummonSpell; }
    }
}
