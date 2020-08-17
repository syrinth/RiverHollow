﻿using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.Characters;
using RiverHollow.Game_Managers;
using RiverHollow.GUIComponents.GUIObjects.GUIWindows;
using RiverHollow.GUIComponents.Screens;
using RiverHollow.SpriteAnimations;
using RiverHollow.Tile_Engine;
using RiverHollow.Items;
using RiverHollow.Utilities;

using static RiverHollow.Characters.Actor;
using static RiverHollow.Game_Managers.GameManager;


namespace RiverHollow.CombatStuff
{
    public class MenuAction
    {
        protected int _id;

        protected ActionEnum _eActionType;
        protected string _sName;
        public string Name => _sName;
        protected string _description;
        public string Description => _description;

        protected Vector2 _vIconGrid;
        public Vector2 IconGrid => _vIconGrid;

        public MenuAction() { }
        public MenuAction(int id, ActionEnum actionType, Vector2 vGrid)
        {
            _id = id;
            _eActionType = actionType;
            _vIconGrid = vGrid;
            DataManager.GetActionText(_id, ref _sName, ref _description);
        }

        public bool IsActionMenu() { return _eActionType == ActionEnum.MenuAction; }
        public bool IsSpellMenu() { return _eActionType == ActionEnum.MenuSpell; }
        public bool IsUseItem() { return _eActionType == ActionEnum.MenuItem; }
        public bool IsEndTurn() { return _eActionType == ActionEnum.EndTurn; }
        public bool IsMove() { return _eActionType == ActionEnum.Move; }

        public bool IsAction() { return _eActionType == ActionEnum.Action; }
        public bool IsSpell() { return _eActionType == ActionEnum.Spell; }
    }

    public class CombatAction : MenuAction
    {
        #region Properties
        const int moveSpeed = 60;

        PotencyBonusEnum _eBonusType;
        ElementEnum _eElement = ElementEnum.None;
        List<ConditionEnum> _liCondition;
        public List<ConditionEnum> LiCondition => _liCondition;
        int _iMPcost;
        public int MPCost => _iMPcost;
        int _iPotency;
        public int Potency => _iPotency;

        int _iReqLevel;
        public int ReqLevel => _iReqLevel;

        bool _bHarm;
        public bool Harm => _bHarm;
        bool _bHeal;
        public bool Heal => _bHeal;

        string _sAnimation;

        TargetEnum _eTarget;
        public TargetEnum Target => _eTarget;
        int _iRange;
        public int Range => _iRange;
        AreaTypeEnum _eAreaType;
        public AreaTypeEnum AreaType => _eAreaType;
        List<String> _liActionTags;
        int _iCurrentAction = 0;
        List<SkillTagsEnum> _liEffects;
        List<StatusEffectData> _liStatusEffects;         //Key = Buff ID, string = Duration/<Tag> <Tag>

        int _iSummonID;
        public Vector2 SummonStartPosition;

        float _fFrameSpeed;
        int _iAnimWidth;
        int _iAnimHeight;
        int _iFrames;
        int _iAnimOffsetX;
        int _iAnimOffsetY;
        int _iBonusMod;

        public List<RHTile> TileTargetList;
        public bool _bCounter;

        bool _bPauseActionHandler;
        CombatActor counteringChar;
        Summon counteringSummon;

        int _iPushVal;
        int _iPullVal;
        int _iRetreatVal;
        int _iStepVal;

        int _iCritRating;
        int _iAccuracy;

        string _sRemoveString;

        private CombatActor _cmbtUser;
        public AnimatedSprite Sprite;
        private Consumable _chosenItem;
        #endregion

        public CombatAction(Consumable it)
        {
            _chosenItem = it;
            _sName = it.Name;
            _iRange = 1;
            _eActionType = ActionEnum.Item;
            _eTarget = it.Helpful ? TargetEnum.Ally : TargetEnum.Enemy;

            TileTargetList = new List<RHTile>();
            _liActionTags = new List<string>() { "UseItem", "Apply", "End" };
        }
        public CombatAction(int id, Dictionary<string, string> stringData)
        {
            TileTargetList = new List<RHTile>();
            _liCondition = new List<ConditionEnum>();
            _liEffects = new List<SkillTagsEnum>();
            _liStatusEffects = new List<StatusEffectData>();
            _liActionTags = new List<string>();

            ImportBasics(id, stringData);
        }
        protected void ImportBasics(int id, Dictionary<string, string> stringData)
        {
            _id = id;
            DataManager.GetActionText(_id, ref _sName, ref _description);

            _eActionType = Util.ParseEnum<ActionEnum>(stringData["Type"]);
            if (stringData.ContainsKey("Element")) { _eElement = Util.ParseEnum<ElementEnum>(stringData["Element"]); }
            if (stringData.ContainsKey("Target")) { _eTarget = Util.ParseEnum<TargetEnum>(stringData["Target"]); }
            if (stringData.ContainsKey("AreaType")) { _eAreaType = Util.ParseEnum<AreaTypeEnum>(stringData["AreaType"]); }
            if (stringData.ContainsKey("Range")) { _iRange = int.Parse(stringData["Range"]); }
            if (stringData.ContainsKey("Crit")) { _iCritRating = int.Parse(stringData["Crit"]); }
            if (stringData.ContainsKey("Accuracy")) { _iAccuracy = int.Parse(stringData["Accuracy"]); }
            if (stringData.ContainsKey("Cost")) { _iMPcost = int.Parse(stringData["Cost"]); }
            if (stringData.ContainsKey("Level")) { _iReqLevel = int.Parse(stringData["Level"]); }

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
                if (stringData.ContainsKey(Util.GetEnumString(SkillTagsEnum.Remove)))
                {
                    _sRemoveString = stringData[Util.GetEnumString(SkillTagsEnum.Remove)];
                    _liEffects.Add(SkillTagsEnum.Remove);
                }

                if (stringData.ContainsKey(Util.GetEnumString(SkillTagsEnum.Status)))
                {
                    string[] parse = stringData[Util.GetEnumString(SkillTagsEnum.Status)].Split('-');

                    StatusEffectData statEffect = new StatusEffectData() { BuffID = int.Parse(parse[0]), Duration = int.Parse(parse[1]), Tags = parse[2] };
                    statEffect.Sprite = Sprite;
                    _liStatusEffects.Add(statEffect);
                    _liEffects.Add(SkillTagsEnum.Status);
                }

                if (stringData.ContainsKey(Util.GetEnumString(SkillTagsEnum.Bonus)))
                {
                    string[] split = stringData[Util.GetEnumString(SkillTagsEnum.Bonus)].Split('-');
                    _eBonusType = Util.ParseEnum<PotencyBonusEnum>(split[0]);
                    _iBonusMod = int.Parse(split[1]);
                    _liEffects.Add(SkillTagsEnum.Bonus);
                }

                if (stringData.ContainsKey(Util.GetEnumString(SkillTagsEnum.Summon)))
                {
                    _liEffects.Add(SkillTagsEnum.Summon);
                    _iSummonID = int.Parse(stringData[Util.GetEnumString(SkillTagsEnum.Summon)]);
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

                Sprite = new AnimatedSprite(_sAnimation);
                Sprite.AddAnimation(AnimationEnum.PlayAnimation, 0, 0, _iAnimWidth, _iAnimHeight, _iFrames, _fFrameSpeed);
                Sprite.PlayAnimation(AnimationEnum.PlayAnimation);
                Sprite.IsAnimating = false;
                Sprite.PlaysOnce = true;
            }

            if (stringData.ContainsKey("AnimOffset"))
            {
                string[] parse = stringData["AnimOffset"].Split('-');
                _iAnimOffsetX = int.Parse(parse[0]);
                _iAnimOffsetY = int.Parse(parse[1]);
            }
        }

        public void Draw(SpriteBatch spritebatch)
        {
            //if (_bDrawItem && _eAreaType == ActionEnum.Item)     //We want to draw the item above the character's head
            //{
            //    int size = TileSize * CombatManager.CombatScale;
            //    //GUIImage gItem = new GUIImage(_chosenItem.SourceRectangle, size, size, _chosenItem.Texture);
            //    //CombatActor c = CombatManager.ActiveCharacter;

            //    //gItem.AnchorAndAlignToObject(c.BodySprite, SideEnum.Top, SideEnum.CenterX);
            //    //gItem.Draw(spritebatch);
            //}

            Sprite?.Draw(spritebatch);
        }

        public void Update(GameTime gTime)
        {
            HandlePhase(gTime);
        }

        /// <summary>
        /// Assigns the CombatManager's SelectedTileand AreaTiles to the CombatAction
        /// </summary>
        public void AssignTargetTile(RHTile target)
        {
            TileTargetList.Add(target);
            TileTargetList.AddRange(DetermineTargetTiles(target));
        }

        /// <summary>
        /// This method applies the effects of the CombatAction upon the appropriate targets
        /// We do this by checking to see if specific tags are int he _liEffects list and performing
        /// the appropriate action
        /// </summary>
        public void ApplyEffect()
        {
            List<CombatActor> targetActors = new List<CombatActor>();
            foreach (RHTile tile in TileTargetList)
            {
                if (tile.Character != null && !targetActors.Contains(tile.Character))
                {
                    targetActors.Add(tile.Character);
                }
            }

            if (_eActionType == ActionEnum.Item)
            {
                foreach (CombatActor act in targetActors)
                {
                    if (_chosenItem.Condition != ConditionEnum.None)
                    {
                        act.ChangeConditionStatus(_chosenItem.Condition, !_chosenItem.Helpful);
                    }
                    act.ModifyHealth(_chosenItem.Health, false);
                }
                _chosenItem.Remove(1);
                return;
            }
            //If the action has some type of bonus associated, we need to figure out what it is
            //and get the appropriate bonus
            int bonus = 0;
            if (_eBonusType != PotencyBonusEnum.None)
            {
                //Bonus Summons increased the bonus amount for each summon attached to the party
                //Note that this currently does not support enemy summons
                if (_eBonusType == PotencyBonusEnum.Summons)
                {
                    foreach (ClassedCombatant c in CombatManager.Party)
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
                ApplyEffectHarm(targetActors, bonus);
            }
            else if (_liEffects.Contains(SkillTagsEnum.Heal))       //Handling for healing
            {
                foreach (CombatActor act in targetActors)
                {
                    act.ProcessHealingSpell(_cmbtUser, _iPotency);
                }
            }

            //Handles when the action applies status effects
            if (_liEffects.Contains(SkillTagsEnum.Status))
            {
                ApplyEffectStatus(targetActors);
            }

            //Handler for if the action Summons something
            if (_liEffects.Contains(SkillTagsEnum.Summon))
            {
                //This should only ever be one, butjust in case
                foreach (CombatActor act in targetActors)
                {
                    Summon newSummon = DataManager.GetSummonByIndex(_iSummonID);
                    newSummon.SetStats(_cmbtUser.StatMag);                //Summon stats are based off the Magic stat
                    act.LinkSummon(newSummon);                 //Links the summon to the character
                    newSummon.linkedChar = act;                //Links the character to the new summon
                    _bPauseActionHandler = true;
                }
            }

            //Handler to push back the target
            if (_liEffects.Contains(SkillTagsEnum.Push))
            {
                foreach (CombatActor act in targetActors)
                {
                    if (act != null && !act.KnockedOut())
                    {
                        MoveAction(act.BaseTile, _iPushVal, SkillTagsEnum.Push, true);
                    }
                }
            }

            //Handler to pull the target forwards
            if (_liEffects.Contains(SkillTagsEnum.Pull))
            {
                foreach (CombatActor act in targetActors)
                {
                    if (act != null && !act.KnockedOut())
                    {
                        MoveAction(act.BaseTile, _iPullVal, SkillTagsEnum.Pull);
                    }
                }
            }

            //Handler for the SkillUser to retreat
            if (_liEffects.Contains(SkillTagsEnum.Retreat))
            {
                MoveAction(TileTargetList[0], _iRetreatVal, SkillTagsEnum.Retreat);
            }

            //Handler for the SkillUser to move forwrd
            if (_liEffects.Contains(SkillTagsEnum.Step))
            {
                MoveAction(TileTargetList[0], _iStepVal, SkillTagsEnum.Step);
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
                        foreach (ClassedCombatant adv in CombatManager.Party)
                        {
                            adv.UnlinkSummon();
                        }
                    }
                }
            }
        }
        private void ApplyEffectHarm(List<CombatActor> targetActors, int bonus)
        {
            int totalPotency = _iPotency + bonus;
            //Iterate over each tile in the target list
            foreach (CombatActor act in targetActors)
            {
                CombatActor targetActor = act;
                //If the tile is unoccupied, don't do anything
                if (targetActor == null) { continue; }

                //If the target has a guard, swap them
                if (targetActor.MyGuard != null)
                {
                    targetActor = targetActor.MyGuard;
                }

                //Lot more logic has to go into skills then spells
                if (!IsSpell())
                {
                    //Roll randomly between 1-100 to determine the chance of hir
                    int attackRoll = RHRandom.Instance.Next(1, 100);
                    attackRoll -= _iAccuracy;                       //Modify the chance to hit by the skill's accuracy. Rolling low is good, so subtract a positive and add a negative
                    if (attackRoll <= 90 - targetActor.Evasion)    //If the modified attack roll is less than 90 minus the character's evasion, then we hit
                    {
                        targetActor.ProcessAttack(_cmbtUser, totalPotency, _iCritRating, targetActor.GetAttackElement());

                        //If the target has a Summon linked to them, and they take
                        ///any area damage, hit the Summon as well
                        //Summon summ = targetActor.LinkedSummon;
                        //if (_iArea > 0 && summ != null)
                        //{
                        //    summ.ProcessAttack(SkillUser, _iPotency + bonus, _iCritRating, targetActor.GetAttackElement());
                        //}

                        #region Countering setup
                        //If the target has Counter turn on, prepare to counterattack
                        //Only counter melee attacks
                        if (_iRange == 0)
                        {
                            if (targetActor.Counter)
                            {
                                targetActor.GoToCounter = true;    //Sets the character to be ready for countering
                                counteringChar = targetActor;      //Sets who the countering character is
                                _bPauseActionHandler = true;            //Tells the game to pause to allow for a counter
                            }

                            //If there is a summon and it can counter, prepare it for countering.
                            //if (summ != null && summ.Counter)
                            //{
                            //    summ.GoToCounter = true;
                            //    counteringSummon = summ;
                            //    _bPauseActionHandler = true;
                            //}
                        }
                        #endregion
                    }
                    else    //Handling for when an attack is dodged
                    {
                        CombatManager.AddFloatingText(new FloatingText(targetActor.Position, targetActor.SpriteWidth, "MISS", Color.White));
                    }

                    //This code handles when someone is guarding the target and takes the damage for them instead
                    //Damage has already been applied above, so we need to unset the Swapped flag, and reset the
                    //image positions of the guard and its guarded character.
                    if (targetActor.Guard && targetActor.Swapped)
                    {
                        CombatActor actor = targetActor;
                        actor.Swapped = false;

                        //Put the GuardTarget back firstin case the guard is a Summon
                        //actor.GuardTarget.GetSprite().Position(actor.GuardTarget.Tile.GUITile.GetIdleLocation(actor.GuardTarget.GetSprite()));

                        //if (actor.IsSummon()) { actor.GetSprite().Position(actor.GuardTarget.Tile.GUITile.GetIdleSummonLocation()); }
                        //else { actor.GetSprite().Position(actor.Tile.GUITile.GetIdleLocation(actor.GetSprite())); }

                        actor.GuardTarget.MyGuard = null;
                        actor.GuardTarget = null;
                    }
                }
                else   //Handling for spells
                {
                    //Process the damage of the spell, then apply it to the targeted tile
                    targetActor.ProcessSpell(_cmbtUser, totalPotency, _eElement);
                }
            }
        }
        private void ApplyEffectStatus(List<CombatActor> targetActors)
        {
            foreach (StatusEffectData effect in _liStatusEffects)
            {
                //Makes a new StatusEffectobject from the data held in the action
                StatusEffect status = DataManager.GetStatusEffectByIndex(effect.BuffID);
                status.Duration = effect.Duration;
                status.Caster = _cmbtUser;

                //Search for relevant tags, primarily targetting
                string[] tags = effect.Tags.Split(' ');
                foreach (string s in tags)
                {
                    //Handle the targetting of where to send the status effect
                    List<CombatActor> targets = new List<CombatActor>();

                    //Apply it to self
                    if (s.Equals("Self"))
                    {
                        targets.Add(_cmbtUser);
                    }

                    //Apply to all targets of the skill
                    if (s.Equals("Target"))
                    {
                        foreach (CombatActor act in targetActors)
                        {
                            if (act != null && !act.KnockedOut())
                            {
                                targets.Add(act);
                            }
                        }
                    }

                    //Apply to all allies of the user
                    if (s.Equals("Allies"))
                    {
                        if (_cmbtUser.IsActorType(ActorEnum.Adventurer))
                        {
                            targets.AddRange(CombatManager.Party);
                        }
                        else
                        {
                            targets.AddRange(CombatManager.Monsters);
                        }
                    }

                    //Apply to all enemies of the user
                    if (s.Equals("Enemies"))
                    {
                        if (_cmbtUser.IsActorType(ActorEnum.Adventurer))
                        {
                            targets.AddRange(CombatManager.Monsters);

                        }
                        else
                        {
                            targets.AddRange(CombatManager.Party);
                        }
                    }

                    //actually apply the StatusEffect to the targets
                    foreach (CombatActor actor in targets)
                    {
                        actor.AddStatusEffect(status);
                    }
                }
            }
        }

        #region Combat Action Movement

        private bool MoveAction(RHTile t, int moveVal, SkillTagsEnum moveAction, bool repeat = false)
        {
            bool rv = false;
            int i = moveVal;
            while (i > 0)
            {
                RHTile test = null;
                test = DetermineMovementTile(t, _cmbtUser.BaseTile, moveAction);
                if (test != null)
                {
                    //Not working with Step or Retreat because this is moving the targeted character and NOT the active character
                    if (!test.HasCombatant() || test.Character == t.Character)
                    {
                        t.Character.SetBaseTile(test, true);
                        t = test;
                        rv = true;
                    }
                    else if(repeat)
                    {
                        if (MoveAction(test, moveVal, moveAction, repeat))
                        {
                            t.Character.SetBaseTile(test, true);
                            rv = true;
                        }
                    }
                }
                i--;
            }

            return rv;
        }
        /// <summary>
        /// Attempts to reassign the occupant of the targeted tile to the
        /// tile right behind it. If there is an occupant of that tile, push
        /// themback too if possible.
        /// </summary>
        /// <param name="t">The target tile</param>
        /// <returns>True if the target was successfully pushed</returns>
        private bool Push(RHTile t)
        {
            bool rv = false;
            int i = _iPushVal;
            while (i > 0)
            {
                RHTile test = null;
                test = DetermineMovementTile(t, _cmbtUser.BaseTile, SkillTagsEnum.Push);
                if (test != null)
                {
                    if (!test.HasCombatant() || test.Character == t.Character)
                    {
                        t.Character.SetBaseTile(test, true);
                        t = test;
                        rv = true;
                    }
                    else
                    {
                        if (Push(test))
                        {
                            t.Character.SetBaseTile(test, true);
                            rv = true;
                        }
                    }
                }
                i--;
            }

            return rv;
        }

        private bool Pull(RHTile t)
        {
            bool rv = false;
            int i = _iPullVal;
            while (i > 0)
            {
                RHTile test = null;
                test = DetermineMovementTile(t, _cmbtUser.BaseTile, SkillTagsEnum.Pull);
                if (test != null)
                {
                    if (!test.HasCombatant())
                    {
                        t.Character.SetBaseTile(test);
                        t = test;
                        rv = true;
                    }
                }
                i--;
            }

            return rv;
        }

        private bool Step(RHTile t)
        {
            bool rv = false;
            int i = _iStepVal;
            while (i > 0)
            {
                RHTile test = null;
                test = DetermineMovementTile(t, _cmbtUser.BaseTile, SkillTagsEnum.Step);
                if (test != null)
                {
                    if (!test.HasCombatant())
                    {
                        t.Character.SetBaseTile(test);
                        t = test;
                        rv = true;
                    }
                }
                i--;
            }

            return rv;
        }

        private bool Retreat(RHTile t)
        {
            bool rv = false;
            int i = _iRetreatVal;
            while (i > 0)
            {
                RHTile test = null;
                test = DetermineMovementTile(t, _cmbtUser.BaseTile, SkillTagsEnum.Retreat);
                if (test != null)
                {
                    if (!test.HasCombatant())
                    {
                        t.Character.SetBaseTile(test);
                        t = test;
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
        /// <param name="tile">The tile the move is acting upon</param>
        /// <param name="action">The skill tag describing the movement type</param>
        /// <returns></returns>
        private RHTile DetermineMovementTile(RHTile targetTile, RHTile userTile, SkillTagsEnum action)
        {
            RHTile rv = null;
            bool moveForward = (action == SkillTagsEnum.Push || action == SkillTagsEnum.Step);
            RHTile moveTile = (action == SkillTagsEnum.Step || action == SkillTagsEnum.Retreat) ? userTile : targetTile;

            if (moveForward)
            {
                if (targetTile.X > userTile.X) { rv = moveTile.GetTileByDirection(DirectionEnum.Right); }
                else if (targetTile.X < userTile.X) { rv = moveTile.GetTileByDirection(DirectionEnum.Left); }
                else if (targetTile.Y > userTile.Y) { rv = moveTile.GetTileByDirection(DirectionEnum.Down); }
                else if (targetTile.Y < userTile.Y) { rv = moveTile.GetTileByDirection(DirectionEnum.Up); }
            }
            else
            {
                if (targetTile.X > userTile.X) { rv = moveTile.GetTileByDirection(DirectionEnum.Left); }
                else if (targetTile.X < userTile.X) { rv = moveTile.GetTileByDirection(DirectionEnum.Right); }
                else if (targetTile.Y > userTile.Y) { rv = moveTile.GetTileByDirection(DirectionEnum.Up); }
                else if (targetTile.Y < userTile.Y) { rv = moveTile.GetTileByDirection(DirectionEnum.Down); }
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
        /// <param name="gTime"></param>
        public void HandlePhase(GameTime gTime)
        {
            switch (_liActionTags[_iCurrentAction])
            {
                case "Escape":
                    {
                        int spd = CombatManager.ActiveCharacter.StatSpd;
                        if (RHRandom.Instance.Next(1, 20) + spd/2 > 15){
                            CombatManager.EndCombatEscape();
                        }
                        break;
                    }
                case "UserAttack":
                    foreach (RHTile tile in TileTargetList)
                    {
                        List<CombatActor> liPotentialGuards = new List<CombatActor>();
                        CombatActor original = tile.Character;

                        //Only assign a guard to the character if they are not guarding and they have no guard
                        if (original != null && !original.Guard & original.MyGuard == null)
                        {
                            List<RHTile> adj = tile.GetAdjacentTiles();
                            foreach(RHTile t in adj)
                            {
                                if (t.Character != null && t.Character.Guard) {
                                    liPotentialGuards.Add(t.Character);
                                }
                            }
                            
                            Summon summ = tile.Character.LinkedSummon;
                            if (summ != null && !summ.Swapped && summ.Guard) { liPotentialGuards.Add(summ); }

                            if (liPotentialGuards.Count > 0)
                            {
                                int which = RHRandom.Instance.Next(0, liPotentialGuards.Count - 1);

                                CombatActor guard = liPotentialGuards[which];
                                guard.Swapped = true;
                                guard.GuardTarget = original;
                                original.MyGuard = guard;

                                //guard.GetSprite().Position(original.GetSprite().Position());
                                //original.GetSprite().PositionSub(new Vector2(100, 0));
                            }
                        }
                    }

                    if (!_cmbtUser.IsCurrentAnimation(VerbEnum.Attack))
                    {
                        _cmbtUser.PlayAnimation(VerbEnum.Attack);
                    }
                    else if (_cmbtUser.AnimationPlayedXTimes(1))
                    {
                        _cmbtUser.PlayAnimation(VerbEnum.Walk);
                        _iCurrentAction++;
                    }
                    break;
                case "UserCast":
                    if (!_cmbtUser.IsDirectionalAnimation(VerbEnum.Cast))
                    {
                        _cmbtUser.PlayAnimation(VerbEnum.Cast);
                    }
                    else if (_cmbtUser.AnimationPlayedXTimes(2))
                    {
                        _cmbtUser.PlayAnimation(VerbEnum.Walk);
                        _iCurrentAction++;
                    }
                    break;
                case "Direct":
                    if (Sprite != null && !Sprite.PlayedOnce && !Sprite.IsAnimating)
                    {
                        Sprite.IsAnimating = true;
                        Sprite.Position = TileTargetList[0].Position;
                        Sprite.Position -= new Vector2(_iAnimOffsetX, _iAnimOffsetY);
                    }
                    else if (Sprite != null && Sprite.IsAnimating) { Sprite.Update(gTime); }
                    else if (Sprite == null || Sprite.PlayedOnce)
                    {
                        _iCurrentAction++;
                    }
                    break;
                case "UseItem":
                    if (!_cmbtUser.IsDirectionalAnimation(VerbEnum.Cast))
                    {
                        _cmbtUser.PlayAnimation(VerbEnum.Cast);
                        //_bDrawItem = true;
                    }
                    else if (_cmbtUser.AnimationPlayedXTimes(3))
                    {
                        _cmbtUser.PlayAnimation(VerbEnum.Walk);
                        //_bDrawItem = false;
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
                                if (!counteringChar.IsDirectionalAnimation(VerbEnum.Attack))
                                {
                                    counteringChar.PlayAnimation(VerbEnum.Attack);
                                }
                                else if (counteringChar.AnimationPlayedXTimes(1))
                                {
                                    counteringChar.PlayAnimation(VerbEnum.Walk);
                                    _cmbtUser.ProcessAttack(counteringChar, ((CombatAction)DataManager.GetActionByIndex(1)).Potency, _iCritRating, counteringChar.GetAttackElement());
                                    counteringChar = null;
                                    _bPauseActionHandler = false;
                                    _iCurrentAction++;
                                }
                            }
                            else if (counteringSummon != null)
                            {
                                if (!counteringSummon.IsDirectionalAnimation(VerbEnum.Attack))
                                {
                                    counteringSummon.PlayAnimation(VerbEnum.Attack);
                                }
                                else if (counteringSummon.AnimationPlayedXTimes(1))
                                {
                                    counteringSummon.PlayAnimation(VerbEnum.Walk);
                                    _cmbtUser.ProcessAttack(counteringSummon, ((CombatAction)DataManager.GetActionByIndex(1)).Potency, _iCritRating, counteringSummon.GetAttackElement());
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
                case "End":
                    _iCurrentAction = 0;
                    if (Sprite != null)
                    {
                        Sprite.IsAnimating = false;
                        Sprite.PlayedOnce = false;
                    }
                    CombatManager.CurrentTurnInfo.Acted();

                    if (!CombatManager.CheckForEndOfCombat())
                    {
                        if (!CombatManager.CheckForForcedEndOfTurn())
                        {
                            if (CombatManager.ActiveCharacter.IsActorType(ActorEnum.Monster) && CombatManager.SelectedAction != null)
                            {
                                CombatManager.ChangePhase(CombatManager.CmbtPhaseEnum.ChooseMoveTarget);
                            }
                            else if (CombatManager.ActiveCharacter.IsActorType(ActorEnum.Adventurer))
                            {
                                CombatManager.GoToMainSelection();
                            }
                        }
                    }

                    break;
            }
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

        public void UseSkillOnTarget()
        {
            CombatManager.ActiveCharacter.CurrentMP -= _iMPcost;          //Checked before Processing
            AssignTargetTile(CombatManager.SelectedTile);

            CombatManager.ChangePhase(CombatManager.CmbtPhaseEnum.PerformAction);
            CombatManager.ClearToPerformAction();
        }
        public void ClearTargets()
        {
            TileTargetList.Clear();
        }
        public List<RHTile> GetTargetTiles()
        {
            return TileTargetList;
        }
        public List<RHTile> DetermineTargetTiles(RHTile targetedTile)
        {
            List<RHTile> rvList = new List<RHTile>();

            switch (_eAreaType)
            {
                case AreaTypeEnum.Cross:
                    foreach (RHTile t in targetedTile.GetAdjacentTiles())
                    {
                        if (t.CanTargetTile())
                        {
                            rvList.Add(t);
                        }
                    }
                    break;
                default:
                    break;
            }

            return rvList;
        }

        public void AssignUser(CombatActor user)
        {
            _cmbtUser = user;
        }

        public bool IsHelpful() { return _eTarget == TargetEnum.Ally; }
        public bool IsSummonSpell() { return _liEffects.Contains(SkillTagsEnum.Summon); }
    }

    internal struct StatusEffectData
    {
        public int BuffID;
        public int Duration;
        public string Tags;
        public AnimatedSprite Sprite;
    }
}
