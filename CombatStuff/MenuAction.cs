using Microsoft.Xna.Framework;
using RiverHollow.Game_Managers;
using RiverHollow.Game_Managers.GUIObjects;
using RiverHollow.GUIComponents.GUIObjects.GUIWindows;
using RiverHollow.Misc;
using RiverHollow.SpriteAnimations;
using RiverHollow.Tile_Engine;
using System;
using System.Collections.Generic;
using System.Threading;
using static RiverHollow.Game_Managers.GameManager;

namespace RiverHollow.Actors.CombatStuff
{
    public class MenuAction
    {
        protected int _id;

        protected ActionEnum _actionType;
        protected MenuEnum _menuType;
        protected string _name;
        public string Name => _name;
        protected string _description;
        public string Description => _description;

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

        PotencyBonusEnum _eBonusType;
        ElementEnum _element = ElementEnum.None;
        List<ConditionEnum> _liCondition;
        public List<ConditionEnum> LiCondition => _liCondition;
        int _iChargeCost;
        public int ChargeCost => _iChargeCost;
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

        TargetEnum _target;
        public TargetEnum Target => _target;
        int _iRange;
        public int Range => _iRange;
        int _iArea;
        public int AreaOfEffect => _iArea;
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
        int _iAnimContactX;
        int _iAnimContactY;
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

        public AnimatedSprite Sprite;
        public CombatAction(int id, Dictionary<string, string> stringData)
        {
            TileTargetList = new List<RHTile>();
            _liCondition = new List<ConditionEnum>();
            _liEffects = new List<SkillTagsEnum>();
            _liStatusEffects = new List<StatusEffectData>();
            _liActionTags = new List<string>();

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
            if (stringData.ContainsKey("Range")) { _iRange = int.Parse(stringData["Range"]); }
            if (stringData.ContainsKey("Charge")) { _iChargeCost = int.Parse(stringData["Charge"]); }
            if (stringData.ContainsKey("Crit")) { _iCritRating = int.Parse(stringData["Crit"]); }
            if (stringData.ContainsKey("Accuracy")) { _iAccuracy = int.Parse(stringData["Accuracy"]); }
            if (stringData.ContainsKey("Cost")) { _iMPcost = int.Parse(stringData["Cost"]); }
            if (stringData.ContainsKey("Level")) { _iReqLevel = int.Parse(stringData["Level"]); }
           
            if (stringData.ContainsKey("Area"))
            {
                string[] tags = stringData["Area"].Split('-');
                _iArea = int.Parse(tags[0]);
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
                Sprite.AddAnimation(GenAnimEnum.Play, 0, 0, _iAnimWidth, _iAnimHeight, _iFrames, _fFrameSpeed);
                Sprite.SetCurrentAnimation(GenAnimEnum.Play);
                Sprite.IsAnimating = false;
                Sprite.PlaysOnce = true;
            }

            if (stringData.ContainsKey("AnimContact"))
            {
                string[] parse = stringData["AnimContact"].Split('-');
                _iAnimContactX = int.Parse(parse[0]);
                _iAnimContactY = int.Parse(parse[1]);
            }
        }

        /// <summary>
        /// Assigns the CombatManager's SelectedTileand AreaTiles to the CombatAction
        /// </summary>
        public void AssignTiles()
        {
            TileTargetList.Add(CombatManager.SelectedTile);
            TileTargetList.AddRange(CombatManager.AreaTiles);
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
                int totalPotency = _iPotency + bonus;
                //Iterate over each tile in the target list
                foreach (RHTile t in TileTargetList)
                {
                    RHTile targetTile = t;
                    CombatActor targetActor = t.Character;
                    //If the tile is unoccupied, don't do anything
                    if (targetActor == null) { continue; }

                    //If the target has a guard, swap them
                    if(targetActor.MyGuard != null)
                    {
                        targetActor = targetActor.MyGuard;
                        targetTile = targetActor.Tile;
                    }

                    //Lot more logic has to go into skills then spells
                    if (!IsSpell())
                    {
                        //Roll randomly between 1-100 to determine the chance of hir
                        int attackRoll = RHRandom.Instance.Next(1, 100);
                        attackRoll -= _iAccuracy;                       //Modify the chance to hit by the skill's accuracy. Rolling low is good, so subtract a positive and add a negative
                        if (attackRoll <= 90 - targetActor.Evasion)    //If the modified attack roll is less than 90 minus the character's evasion, then we hit
                        {
                            targetActor.ProcessAttack(SkillUser, totalPotency, _iCritRating, targetActor.GetAttackElement());

                            //If the target has a Summon linked to them, and they take
                            //any area damage, hit the Summon as well
                            Summon summ = targetActor.LinkedSummon;
                            if (_iArea > 0 && summ != null)
                            {
                                summ.ProcessAttack(SkillUser, _iPotency + bonus, _iCritRating, targetActor.GetAttackElement());
                            }

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
                        targetActor.ProcessSpell(SkillUser, totalPotency, _element);
                    }
                }
            }
            else if (_liEffects.Contains(SkillTagsEnum.Heal))       //Handling for healing
            {
                foreach (RHTile t in TileTargetList)
                {
                    t.Character.ProcessHealingSpell(SkillUser, _iPotency);
                }
            }

            //Handles when the action applies status effects
            if (_liEffects.Contains(SkillTagsEnum.Status))
            {
                foreach (StatusEffectData effect in _liStatusEffects)
                {
                    //Makes a new StatusEffectobject from the data held in the action
                    StatusEffect status = ObjectManager.GetStatusEffectByIndex(effect.BuffID);
                    status.Duration = effect.Duration;
                    status.Caster = SkillUser;

                    //Search for relevant tags, primarily targetting
                    string[] tags = effect.Tags.Split(' ');
                    foreach (string s in tags)
                    {
                        //Handle the targetting of where to send the status effect
                        List<CombatActor> targets = new List<CombatActor>();

                        //Apply it to self
                        if (s.Equals("Self"))
                        {
                            targets.Add(SkillUser);
                        }

                        //Apply to all targets of the skill
                        if (s.Equals("Target"))
                        {
                            foreach (RHTile t in TileTargetList) {
                                if (t.Character != null && !t.Character.KnockedOut())
                                {
                                    targets.Add(t.Character);
                                }
                            }
                        }

                        //Apply to all allies of the user
                        if (s.Equals("Allies"))
                        {
                            if (SkillUser.IsAdventurer())
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
                            if (SkillUser.IsAdventurer())
                            {
                                targets.AddRange(CombatManager.Monsters);

                            }
                            else
                            {
                                targets.AddRange(CombatManager.Party);
                            }
                        }

                        //actualyl apply the StatusEffect to the targets
                        foreach (CombatActor actor in targets)
                        {
                            actor.AddStatusEffect(status);
                        }
                    }
                }
            }

            //Handler for if the action Summons something
            if (_liEffects.Contains(SkillTagsEnum.Summon))
            {
                //This should only ever be one, butjust in case
                foreach (RHTile t in TileTargetList)
                {
                    Summon newSummon = ObjectManager.GetSummonByIndex(_iSummonID);
                    newSummon.SetStats(SkillUser.StatMag);                //Summon stats are based off the Magic stat
                    t.Character.LinkSummon(newSummon);                 //Links the summon to the character
                    newSummon.linkedChar = t.Character;                //Links the character to the new summon
                    _bPauseActionHandler = true;
                }
            }

            //Handler to push back the target
            if (_liEffects.Contains(SkillTagsEnum.Push))
            {
                foreach (RHTile t in TileTargetList)
                {
                    if (t.Character != null && !t.Character.KnockedOut())
                    {
                        Push(t);
                    }
                }
            }

            //Handler to pull the target forwards
            if (_liEffects.Contains(SkillTagsEnum.Pull))
            {
                foreach (RHTile t in TileTargetList)
                {
                    if (t.Character != null && !t.Character.KnockedOut())
                    {
                        Pull(t);
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

                if(strRemovewhat == "Summons")
                {
                    if(strArea == "Every")
                    {
                        foreach(ClassedCombatant adv in CombatManager.Party)
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
        /// <param name="t">Tile to push backf rom</param>
        /// <returns>True if the target was successfully pushed</returns>
        private bool Push(RHTile t)
        {
            bool rv = false;
            int i = _iPushVal;
            while (i > 0)
            {
                RHTile test = null;
                test = DetermineMovementTile(test, SkillTagsEnum.Push);
                if (test != null)
                {
                    if (!test.HasCombatant())
                    {
                        test.SetCombatant(t.Character);
                        t = test;
                        rv = true;
                    }
                    else if (test.HasCombatant())
                    {
                        if (Push(test))
                        {
                            test.SetCombatant(t.Character);
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
                test = DetermineMovementTile(test, SkillTagsEnum.Pull);
                if (test != null)
                {
                    if (!test.HasCombatant())
                    {
                        test.SetCombatant(t.Character);
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
                test = DetermineMovementTile(test, SkillTagsEnum.Step);
                if (test != null)
                {
                    if (!test.HasCombatant())
                    {
                        test.SetCombatant(t.Character);
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
                test = DetermineMovementTile(test, SkillTagsEnum.Retreat);
                if (test != null)
                {
                    if (!test.HasCombatant())
                    {
                        test.SetCombatant(t.Character);
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
        /// <param name="tile">The tile to move from</param>
        /// <param name="action">The skill tag describing the movement type</param>
        /// <returns></returns>
        private RHTile DetermineMovementTile(RHTile tile, SkillTagsEnum action)
        {
            RHTile rv = null;

            ////The meaning of push and pull is dependent on whether or not
            ////it's an ally or enemy tile.
            //if (tile.Character.IsAdventurer())
            //{
            //    if (action == SkillTagsEnum.Push) { rv = tile.GetTileByDirection();
            //    else if (action == SkillTagsEnum.Pull) { rv = CombatManager.GetRight(tile.GUITile.MapTile); }
            //    else if (action == SkillTagsEnum.Retreat) { rv = CombatManager.GetLeft(tile.GUITile.MapTile); }
            //    else if (action == SkillTagsEnum.Step) { rv = CombatManager.GetRight(tile.GUITile.MapTile); }
            //}
            //else
            //{
            //    if (action == SkillTagsEnum.Push) { rv = CombatManager.GetRight(tile.GUITile.MapTile); }
            //    else if (action == SkillTagsEnum.Pull) { rv = CombatManager.GetLeft(tile.GUITile.MapTile); }
            //    else if (action == SkillTagsEnum.Retreat) { rv = CombatManager.GetRight(tile.GUITile.MapTile); }
            //    else if (action == SkillTagsEnum.Step) { rv = CombatManager.GetLeft(tile.GUITile.MapTile); }
            //}

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
                            List<RHTile> adj = tile.GetAdjacent();
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

                    if (!SkillUser.IsCurrentAnimation(Util.GetActorString(VerbEnum.Attack, SkillUser.Facing)))
                    {
                       SkillUser.PlayDirectionalAnimation(VerbEnum.Attack);
                    }
                    else if (SkillUser.AnimationPlayedXTimes(1))
                    {
                        SkillUser.PlayDirectionalAnimation(VerbEnum.Walk);
                        _iCurrentAction++;
                    }
                    break;
                case "UserCast":
                    if (!SkillUser.IsDirectionalAnimation(VerbEnum.Cast))
                    {
                        SkillUser.PlayDirectionalAnimation(VerbEnum.Cast);
                    }
                    else if (SkillUser.AnimationPlayedXTimes(2))
                    {
                        SkillUser.PlayDirectionalAnimation(VerbEnum.Walk);
                        _iCurrentAction++;
                    }
                    break;
                case "Direct":
                    if (Sprite != null && !Sprite.PlayedOnce && !Sprite.IsAnimating)
                    {
                        Sprite.IsAnimating = true;
                        int overFlowX = (Sprite.Width - TileSize) / 2;
                        int overFlowY = (Sprite.Height - _iAnimContactX);
                        Sprite.Position = TileTargetList[0].Center;
                        Sprite.Position -= new Vector2(_iAnimContactX, _iAnimContactY);
                    }
                    else if (Sprite != null && Sprite.IsAnimating) { Sprite.Update(gTime); }
                    else if (Sprite == null || Sprite.PlayedOnce)
                    {
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
                                    counteringChar.PlayDirectionalAnimation(VerbEnum.Attack);
                                }
                                else if (counteringChar.AnimationPlayedXTimes(1))
                                {
                                    counteringChar.PlayDirectionalAnimation(VerbEnum.Walk);
                                    SkillUser.ProcessAttack(counteringChar, ((CombatAction)ObjectManager.GetActionByIndex(1)).Potency, _iCritRating, counteringChar.GetAttackElement());
                                    counteringChar = null;
                                    _bPauseActionHandler = false;
                                    _iCurrentAction++;
                                }
                            }
                            else if (counteringSummon != null)
                            {
                                if (!counteringSummon.IsDirectionalAnimation(VerbEnum.Attack))
                                {
                                    counteringSummon.PlayDirectionalAnimation(VerbEnum.Attack);
                                }
                                else if (counteringSummon.AnimationPlayedXTimes(1))
                                {
                                    counteringSummon.PlayDirectionalAnimation(VerbEnum.Walk);
                                    SkillUser.ProcessAttack(counteringSummon, ((CombatAction)ObjectManager.GetActionByIndex(1)).Potency, _iCritRating, counteringSummon.GetAttackElement());
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
                case "Move":
                    TileTargetList[0].SetCombatant(SkillUser);
                    if (SkillUser.LinkedSummon != null)
                    {
                        SkillUser.LinkedSummon.Tile = TileTargetList[0];
                        //TileTargetList[0].GUITile.LinkSummon(SkillUser.LinkedSummon);
                    }
                    _iCurrentAction++;
                    break;
                case "End":
                    _iCurrentAction = 0;
                    if (Sprite != null)
                    {
                        Sprite.IsAnimating = false;
                        Sprite.PlayedOnce = false;
                    }
                    CombatManager.EndTurn();

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

        public bool TargetsEach() { return _iArea > 0; }
        public bool IsHelpful() { return _target == TargetEnum.Ally; }
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
