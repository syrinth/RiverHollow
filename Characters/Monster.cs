using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.Actors;
using RiverHollow.Actors.CombatStuff;
using RiverHollow.Game_Managers;
using RiverHollow.Misc;
using RiverHollow.SpriteAnimations;
using RiverHollow.Tile_Engine;
using RiverHollow.WorldObjects;
using System;
using System.Collections.Generic;
using static RiverHollow.Game_Managers.GameManager;

namespace RiverHollow.Characters
{
    public class Monster : CombatActor
    {
        #region Properties
        int _id;
        public int ID => _id;
        int _iRating;
        int _iXP;
        public int XP => _iXP;
        protected Vector2 _moveTo = Vector2.Zero;
        List<int> _liLootIDs;

        public override int Attack => 20 + (_iRating * 10);

        public override int MaxHP => (int)((((Math.Pow(_iRating, 2)) * 10) + 20) * Math.Pow(Math.Max(1, (double)_iRating / 14), 2));

        protected double _dIdleFor;

        bool _bJump;
        Vector2 _vJumpTo;

        int _iMoveFailures = 0;

        List<SpawnConditionEnum> _liSpawnConditions;
        List<CombatAction> _liCombatActions;

        #endregion

        public Monster(int id, Dictionary<string, string> data)
        {
            _liLootIDs = new List<int>();
            _liCombatActions = new List<CombatAction>();
            _liSpawnConditions = new List<SpawnConditionEnum>();

            _eActorType = ActorEnum.Monster;
            ImportBasics(data, id);
        }

        protected void ImportBasics(Dictionary<string, string> data, int id)
        {
            _id = id;
            _sName = DataManager.GetMonsterTestInfo(_id);

            _iRating = int.Parse(data["Lvl"]);
            _iXP = _iRating * 10;
            _iStrength = 1 + _iRating;
            _iDefense = 8 + (_iRating * 3);
            _iVitality = 2 * _iRating + 10;
            _iMagic = 0; // 2 * _iRating + 2;
            _iResistance = 2 * _iRating + 10;
            _iSpeed = 10;

            if (data.ContainsKey("Width")) { _iWidth = int.Parse(data["Width"]); }
            else { _iWidth = TileSize; }

            if (data.ContainsKey("Height")) { _iHeight = int.Parse(data["Height"]); }
            else { _iHeight = TileSize * 2; }

            if (data.ContainsKey("Size")) {
                _iSize = int.Parse(data["Size"]);
                _arrTiles = new RHTile[_iSize, _iSize];
            }
            string[] split;
            if (data.ContainsKey("Condition"))
            {
                split = data["Condition"].Split('-');
                for (int i = 0; i < split.Length; i++)
                {
                    _liSpawnConditions.Add(Util.ParseEnum<SpawnConditionEnum>(split[i]));
                }
            }

            if (data.ContainsKey("Loot"))
            {
                split = data["Loot"].Split(' ');
                for (int i = 0; i < split.Length; i++)
                {
                    _liLootIDs.Add(int.Parse(split[i]));
                }
            }

            _bJump = false; // data.ContainsKey("Jump"); Turning off jump logic for now

            foreach (string ability in data["Ability"].Split('-'))
            {
                _liCombatActions.Add((CombatAction)DataManager.GetActionByIndex(int.Parse(ability)));
            }

            if (data.ContainsKey("Trait"))
            {
                HandleTrait(DataManager.GetMonsterTraitData(data["Trait"]));
            }

            if (data.ContainsKey("Resist"))
            {
                foreach (string elem in data["Resist"].Split('-'))
                {
                    _diElementalAlignment[Util.ParseEnum<ElementEnum>(elem)] = ElementAlignment.Resists;
                }
            }

            if (data.ContainsKey("Vuln"))
            {
                foreach (string elem in data["Vuln"].Split('-'))
                {
                    _diElementalAlignment[Util.ParseEnum<ElementEnum>(elem)] = ElementAlignment.Vulnerable;
                }
            }

            _iCurrentHP = MaxHP;
            _iCurrentMP = MaxMP;
            LoadSpriteAnimations(ref _sprBody, LoadWorldAndCombatAnimations(data), DataManager.FOLDER_MONSTERS + data["Texture"]);
        }

        public override void Update(GameTime gTime)
        {
            base.Update(gTime);

            ///When the Monster has finished playing the KO animation, let the CombatManager know so it can do any final actions
            if (IsCurrentAnimation(AnimationEnum.KO) && BodySprite.CurrentFrameAnimation.PlayCount == 1)
            {
                CombatManager.MonsterKOAnimFinished(this);
            }

            // Only comment this out for now in case we implement stealth or something so you can sneak past enemies without aggroing them
            //if (!CombatManager.InCombat) {
            //    UpdateMovement(gTime);
            //}
        }

        public override void Draw(SpriteBatch spriteBatch, bool useLayerDepth = false)
        {
            base.Draw(spriteBatch, useLayerDepth);
        }

        private void HandleTrait(string traitData)
        {
            string[] traits = Util.FindTags(traitData);
            foreach (string s in traits)
            {
                string[] tagType = s.Split(':');
                if (tagType[0].Equals(Util.GetEnumString(StatEnum.Str)))
                {
                    ApplyTrait(ref _iStrength, tagType[1]);
                }
                else if (tagType[0].Equals(Util.GetEnumString(StatEnum.Def)))
                {
                    ApplyTrait(ref _iDefense, tagType[1]);
                }
                else if (tagType[0].Equals(Util.GetEnumString(StatEnum.Vit)))
                {
                    ApplyTrait(ref _iVitality, tagType[1]);
                }
                else if (tagType[0].Equals(Util.GetEnumString(StatEnum.Mag)))
                {
                    ApplyTrait(ref _iMagic, tagType[1]);
                }
                else if (tagType[0].Equals(Util.GetEnumString(StatEnum.Res)))
                {
                    ApplyTrait(ref _iResistance, tagType[1]);
                }
                else if (tagType[0].Equals(Util.GetEnumString(StatEnum.Spd)))
                {
                    ApplyTrait(ref _iSpeed, tagType[1]);
                }
            }
        }

        private void ApplyTrait(ref int value, string data)
        {
            if (data.Equals("+"))
            {
                value = (int)(value * 1.1);
            }
            else if (data.Equals("-"))
            {
                value = (int)(value * 0.9);
            }
        }

        private void UpdateMovement(GameTime gTime)
        {
            bool move = true;
            Vector2 direction = Vector2.Zero;

            GetIdleMovementTarget(gTime);

            if (_bJump)
            {
                if (_vMoveTo != Vector2.Zero && BodySprite.CurrentAnimation.StartsWith("Idle"))
                {
                    move = false;

                    PlayAnimation(VerbEnum.Ground);
                }
                else if (BodySprite.CurrentAnimation.StartsWith("Ground") && BodySprite.CurrentFrameAnimation.PlayCount < 1)
                {
                    move = false;
                }
                else if (!BodySprite.CurrentAnimation.StartsWith("Idle") && BodySprite.CurrentFrameAnimation.PlayCount >= 1)
                {
                    bool jumping = BodySprite.CurrentAnimation.StartsWith("Air");
                    PlayAnimation(jumping ? VerbEnum.Ground : VerbEnum.Air);
                    _vJumpTo = Vector2.Zero;
                }
            }

            if (move && _vMoveTo != Vector2.Zero)
            {
                float deltaX = Math.Abs(_vMoveTo.X - this.Position.X);
                float deltaY = Math.Abs(_vMoveTo.Y - this.Position.Y);

                Util.GetMoveSpeed(Position, _vMoveTo, Speed, ref direction);
                DetermineFacing(direction);
                if (!CheckMapForCollisionsAndMove(direction, false))
                {
                    _iMoveFailures++;
                }

                //We have finished moving
                if (Position.X == _vMoveTo.X && Position.Y == _vMoveTo.Y)
                {
                    _vMoveTo = Vector2.Zero;
                    PlayAnimation(CombatManager.InCombat ? VerbEnum.Walk : VerbEnum.Idle);
                }
            }
        }

        private void GetIdleMovementTarget(GameTime gTime)
        {
            if ((_vMoveTo == Vector2.Zero && _dIdleFor <= 0 && !BodySprite.CurrentAnimation.StartsWith("Air")) || _iMoveFailures == 5)
            {
                _iMoveFailures = 0;
                int howFar = 2;
                bool skip = false;
                RHRandom rand = RHRandom.Instance;
                int decision = rand.Next(1, 5);
                if (decision == 1) { _vMoveTo = new Vector2(Position.X - rand.Next(1, howFar) * TileSize, Position.Y); }
                else if (decision == 2) { _vMoveTo = new Vector2(Position.X + rand.Next(1, howFar) * TileSize, Position.Y); }
                else if (decision == 3) { _vMoveTo = new Vector2(Position.X, Position.Y - rand.Next(1, howFar) * TileSize); }
                else if (decision == 4) { _vMoveTo = new Vector2(Position.X, Position.Y + rand.Next(1, howFar) * TileSize); }
                else
                {
                    _vMoveTo = Vector2.Zero;
                    _dIdleFor = 4;
                    PlayAnimation(CombatManager.InCombat ? VerbEnum.Walk : VerbEnum.Idle);
                    skip = true;
                }

                if (!skip)
                {
                    DetermineFacing(new Vector2(_vMoveTo.X - Position.X, _vMoveTo.Y - Position.Y));
                }
            }
            else if (_vMoveTo == Vector2.Zero)
            {
                _dIdleFor -= gTime.ElapsedGameTime.TotalSeconds;
            }
        }

        public override void DetermineFacing(Vector2 direction)
        {
            string animation = string.Empty;

            if (direction.Length() == 0)
            {
                PlayAnimation(CombatManager.InCombat ? VerbEnum.Walk : VerbEnum.Idle);
            }
            else
            {
                if (!_bJump || (_bJump && !BodySprite.CurrentAnimation.StartsWith("Air")))
                {
                    if (Math.Abs((int)direction.X) > Math.Abs((int)direction.Y))
                    {
                        if (direction.X > 0) { Facing = DirectionEnum.Right; }
                        else if (direction.X < 0) { Facing = DirectionEnum.Left; }
                    }
                    else
                    {
                        if (direction.Y > 0) { Facing = DirectionEnum.Down; }
                        else if (direction.Y < 0) { Facing = DirectionEnum.Up; }
                    }

                    PlayAnimation(_bJump ? VerbEnum.Ground : VerbEnum.Walk);
                }
            }
        }

        public bool CheckValidConditions(SpawnConditionEnum s)
        {
            bool rv = true;

            if (_liSpawnConditions.Contains(s))
            {
                foreach (SpawnConditionEnum e in _liSpawnConditions)
                {
                    if (e.Equals(SpawnConditionEnum.Night) && !GameCalendar.IsNight())
                    {
                        rv = false;
                    }
                    else if (CompareSpawnSeason(e, SpawnConditionEnum.Spring))
                    {
                        rv = false;
                    }
                    else if (CompareSpawnSeason(e, SpawnConditionEnum.Summer))
                    {
                        rv = false;
                    }
                    else if (CompareSpawnSeason(e, SpawnConditionEnum.Winter))
                    {
                        rv = false;
                    }
                    else if (CompareSpawnSeason(e, SpawnConditionEnum.Fall))
                    {
                        rv = false;
                    }

                    if (!rv) { break; }
                }
            }

            return rv;
        }

        private bool CompareSpawnSeason(SpawnConditionEnum check, SpawnConditionEnum season)
        {
            return check.Equals(season) && !Util.ParseEnum<SpawnConditionEnum>(GameCalendar.GetSeason()).Equals(season);
        }

        public int GetRandomLootItem()
        {
            return _liLootIDs[RHRandom.Instance.Next(0, _liLootIDs.Count-1)];
        } 

        public override void KO()
        {
            base.KO();
            CombatManager.GiveXP(this);
        }

        public override List<CombatAction> GetCurrentSpecials()
        {
            return _liCombatActions;
        }

        public void TakeTurn()
        {
            CombatManager.EndTurn();
            //TravelManager.NewTravelLog(_sName);
            //TravelManager.SetParams(_iSize, this);
            //int maxPathLength = 5;
            ////First, determine which action we would prefer to use
            //CombatAction preferredAction = GetPreferredAction();

            ////Define the list of characters we wish to interact with
            //List<CombatActor> potentialTargets = (preferredAction.Target == TargetEnum.Enemy) ? CombatManager.Party : CombatManager.Monsters;

            ////Determine how far away all potential targets are and keep tabs on which is the closest.
            //CombatActor targetActor = CombatManager.Party[0];
            //Dictionary<CombatActor, List<RHTile>> howFar = new Dictionary<CombatActor, List<RHTile>>();
            //foreach (CombatActor act in CombatManager.Party)
            //{
            //    if (act != this)
            //    {
            //        Vector2 start = this.BaseTile.Position;
            //        RHTile target = act.BaseTile;
            //        target.SetCombatant(null);
            //        howFar[act] = TravelManager.FindPathToLocation(ref start, target.Position, MapManager.CurrentMap.Name);
            //        target.SetCombatant(act);
            //        TravelManager.Clear();

            //        //Keep track of which CombatActor is closest
            //        if (howFar[act]?.Count < howFar[targetActor].Count)
            //        {
            //            targetActor = act;
            //        }
            //    }
            //}

            ////Pick which party member to target
            //ChooseTargetActor(ref targetActor, howFar);

            //bool validPath = false;
            //List<RHTile> blockedTiles = new List<RHTile>();
            //List<RHTile> pathToSkillTarget = new List<RHTile>();
            //do {
            //    //Find the path to the tile we want to move to in order to engage the adventurer with the selected CombatAction
            //    pathToSkillTarget = TravelManager.FindClosestValidTile(this.BaseTile, targetActor.BaseTile, CurrentMapName);

            //    //If the found path is within range, then 
            //    if (pathToSkillTarget?.Count <= maxPathLength)
            //    {
            //        validPath = true;
            //    }
            //    else
            //    {
            //        //Check to see if the last tile in the path can fit the character, if so, remove everything after it
            //        if(TravelManager.TestNodeForSize(pathToSkillTarget[maxPathLength - 1], true))
            //        {
            //            pathToSkillTarget.RemoveRange(maxPathLength, pathToSkillTarget.Count - maxPathLength);
            //            validPath = true;
            //        }
            //        else
            //        {
            //            //If the character cannot fit on the last tile of the allowed path, block that tile and
            //            //then check the next tile behind it in the path.
            //            pathToSkillTarget[maxPathLength - 1].PathingBlocked = true;
            //            blockedTiles.Add(pathToSkillTarget[maxPathLength - 1]);
            //            if (TravelManager.TestNodeForSize(pathToSkillTarget[maxPathLength - 2], true))
            //            {
            //                pathToSkillTarget.RemoveRange(maxPathLength - 2, pathToSkillTarget.Count - maxPathLength + 1);
            //                validPath = true;
            //            }
            //            else
            //            {
            //                //If it doesn't fit in either of the first two tiles, recalculate the path and null
            //                //the one given.
            //                pathToSkillTarget = null;
            //            }
            //        }
            //    }
            //} while (validPath);

            //CombatManager.ChangePhase(CombatManager.PhaseEnum.Moving);
            //SetPath(pathToSkillTarget);

            //foreach (RHTile t in blockedTiles) { t.PathingBlocked = false; }

            ////After everything is done, clear the TravelManager data
            //TravelManager.Clear();
            //TravelManager.ClearParams();
            //TravelManager.CloseTravelLog();
        }

        private CombatAction GetPreferredAction()
        {
            CombatAction rv = null;

            //Get Healing action if appropriate
            //Get debuff Action if appropriate
            //Get Buff action if appropriate
            //Else get Attack action
            //For now, pick a random action the monster can use that has the Harm property
            int actionNo = RHRandom.Instance.Next(0, this.GetCurrentSpecials().FindAll(x => x.Harm == true).Count -1);
            rv = this.GetCurrentSpecials().FindAll(x => x.Harm == true)[actionNo];

            return rv;
        }

        private void ChooseTargetActor(ref CombatActor targetActor, Dictionary<CombatActor, List<RHTile>> howFar)
        {
            //ToDo: add logic to determine who we will effect with our skill.
            //Currently, only selecting closestActor
        }
    }
}
