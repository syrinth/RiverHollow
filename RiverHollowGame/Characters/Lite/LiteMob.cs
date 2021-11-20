using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.Game_Managers;
using RiverHollow.SpriteAnimations;
using RiverHollow.Utilities;
using RiverHollow.WorldObjects;
using System;
using System.Collections.Generic;
using System.Linq;

using static RiverHollow.Game_Managers.GameManager;

namespace RiverHollow.Characters
{
    public class LiteMob : WorldActor
    {
        #region Properties
        public override Vector2 Position
        {
            get { return new Vector2(_sprBody.Position.X, _sprBody.Position.Y + _sprBody.Height - TILE_SIZE); } //MAR this is fucked up
            set
            {
                _sprBody.Position = new Vector2(value.X, value.Y - _sprBody.Height + TILE_SIZE);
                //_sprAlert.Position = _sprBody.Position - new Vector2(0, TILE_SIZE);
            }
        }

        protected int _id;
        public int ID { get => _id; }
        protected double _dIdleFor;
        protected int _iLeash = 7;

        protected Vector2 _vMoveTo = Vector2.Zero;
        protected List<LiteCombatActor> _liMonsters;
        public List<LiteCombatActor> Monsters { get => _liMonsters; }

        double _dStun;
        int _iMaxRange = TILE_SIZE * 10;
        bool _bAlert;
        bool _bLockedOn;
        bool _bLeashed;
        bool _bJump;
        Vector2 _vJumpTo;
        const double MAX_ALERT = 1;
        double _dAlertTimer;
        //AnimatedSprite _sprAlert;

        FieldOfVision _FoV;
        Vector2 _vLeashPoint;
        float _fLeashRange = TILE_SIZE * 30;
        int _iMoveFailures = 0;

        List<SpawnConditionEnum> _liSpawnConditions;

        int _iXP;
        int _iXPToGive;

        #endregion

        public LiteMob(int id, Dictionary<string, string> data)
        {
            _liSpawnConditions = new List<SpawnConditionEnum>();
            _eActorType = ActorEnum.Mob;
            _liMonsters = new List<LiteCombatActor>();
            ImportBasics(data, id);
            //LoadContent(DataManager.FOLDER_MOBS + data["Texture"];);

            _iXP = 0;
            foreach (LiteMonster mon in _liMonsters)
            {
                _iXP += mon.XP;
            }
            _iXPToGive = _iXP;
        }

        //public void LoadContent(string texture)
        //{
        //    _sprBody = new AnimatedSprite(texture);

        //    if (!_bJump)
        //    {
        //        _sprBody.AddAnimation(WActorBaseAnim.IdleDown, 0, 0, TILE_SIZE, TILE_SIZE * 2, 4, 0.2f);
        //        _sprBody.AddAnimation(WActorWalkAnim.WalkDown, 0, 0, TILE_SIZE, TILE_SIZE * 2, 4, 0.2f);
        //        _sprBody.AddAnimation(WActorBaseAnim.IdleUp, 64, 0, TILE_SIZE, TILE_SIZE * 2, 4, 0.2f);
        //        _sprBody.AddAnimation(WActorWalkAnim.WalkUp, 64, 0, TILE_SIZE, TILE_SIZE * 2, 4, 0.2f);
        //        _sprBody.AddAnimation(WActorBaseAnim.IdleLeft, 128, 0, TILE_SIZE, TILE_SIZE * 2, 4, 0.2f);
        //        _sprBody.AddAnimation(WActorWalkAnim.WalkLeft, 128, 0, TILE_SIZE, TILE_SIZE * 2, 4, 0.2f);
        //        _sprBody.AddAnimation(WActorBaseAnim.IdleRight, 192, 0, TILE_SIZE, TILE_SIZE * 2, 4, 0.2f);
        //        _sprBody.AddAnimation(WActorWalkAnim.WalkRight, 192, 0, TILE_SIZE, TILE_SIZE * 2, 4, 0.2f);
        //        _sprBody.PlayAnimation(WActorWalkAnim.WalkDown);
        //    }
        //    #region Jumping Code
        //    else
        //    {
        //        //_sprBody.AddAnimation(WActorBaseAnim.IdleDown, 0, 0, TILE_SIZE, TILE_SIZE * 2, 2, 0.2f);
        //        //_sprBody.AddAnimation(WActorJumpAnim.GroundDown, 0, 0, TILE_SIZE, TILE_SIZE * 2, 2, 0.2f);
        //        //_sprBody.AddAnimation(WActorJumpAnim.AirDown, 32, 0, TILE_SIZE, TILE_SIZE * 2, 2, 0.2f);

        //        //_sprBody.AddAnimation(WActorBaseAnim.IdleUp, 64, 0, TILE_SIZE, TILE_SIZE * 2, 2, 0.2f);
        //        //_sprBody.AddAnimation(WActorJumpAnim.GroundUp, 64, 0, TILE_SIZE, TILE_SIZE * 2, 2, 0.2f);
        //        //_sprBody.AddAnimation(WActorJumpAnim.AirUp, 96, 0, TILE_SIZE, TILE_SIZE * 2, 2, 0.2f);

        //        //_sprBody.AddAnimation(WActorBaseAnim.IdleLeft, 128, 0, TILE_SIZE, TILE_SIZE * 2, 2, 0.2f);
        //        //_sprBody.AddAnimation(WActorJumpAnim.GroundLeft, 128, 0, TILE_SIZE, TILE_SIZE * 2, 2, 0.2f);
        //        //_sprBody.AddAnimation(WActorJumpAnim.AirLeft, 160, 0, TILE_SIZE, TILE_SIZE * 2, 2, 0.2f);

        //        //_sprBody.AddAnimation(WActorBaseAnim.IdleRight, 192, 0, TILE_SIZE, TILE_SIZE * 2, 2, 0.2f);
        //        //_sprBody.AddAnimation(WActorJumpAnim.GroundRight, 192, 0, TILE_SIZE, TILE_SIZE * 2, 2, 0.2f);
        //        //_sprBody.AddAnimation(WActorJumpAnim.AirRight, 224, 0, TILE_SIZE, TILE_SIZE * 2, 2, 0.2f);
        //        //_sprBody.SetCurrentAnimation(WActorBaseAnim.IdleDown);
        //    }
        //    #endregion
        //    Facing = DirectionEnum.Down;

        //    _iBodyWidth = _sprBody.Width;
        //    _iBodyHeight = _sprBody.Height;

        //    //_sprAlert = new AnimatedSprite(@"Textures\Dialog");
        //    //_sprAlert.AddAnimation(GenAnimEnum.Play, 64, 64, 16, 16, 3, 0.2f, true);
        //    //_sprAlert.PlayAnimation(GenAnimEnum.Play);
        //    //_sprAlert.Position = (Position - new Vector2(0, TILE_SIZE));
        //}

        protected int ImportBasics(Dictionary<string, string> data, int id)
        {
            string[] split = data["Monster"].Split('-');
            for (int i = 0; i < split.Length; i++)
            {
                int mID = int.Parse(split[i]);
                _liMonsters.Add(DataManager.GetLiteMonsterByIndex(mID));
            }

            split = data["Condition"].Split('-');
            for (int i = 0; i < split.Length; i++)
            {
                _liSpawnConditions.Add(Util.ParseEnum<SpawnConditionEnum>(split[i]));
            }

            _bJump = data.ContainsKey("Jump");

            foreach (LiteCombatActor m in _liMonsters)
            {
                List<LiteCombatActor> match = _liMonsters.FindAll(x => ((LiteMonster)x).ID == ((LiteMonster)m).ID);
                if (match.Count > 1)
                {
                    for (int i = 0; i < match.Count; i++)
                    {
                        match[i].SetUnique(Util.NumToString(i + 1, true));
                    }
                }
            }
            _id = id;

            LoadSpriteAnimations(ref _sprBody, LoadWorldAndCombatAnimations(data), DataManager.FOLDER_MOBS + data["Texture"]);
            return 0;
        }

        public void NewFoV()
        {
            _FoV = new FieldOfVision(this, _iMaxRange);
        }

        public override void Update(GameTime theGameTime)
        {
            //Check if the mob is still stunned
            if (_dStun > 0)
            {
                _dStun -= theGameTime.ElapsedGameTime.TotalSeconds;
                if (_dStun < 0) { _dStun = 0; }
            }
            else
            {
               //UpdateAlertTimer(theGameTime);
                UpdateMovement(theGameTime);
            }

            base.Update(theGameTime);
        }

        private void UpdateAlertTimer(GameTime theGameTime)
        {
            //If mob is on alert, but not locked on to
            //the player, increment the timer.
            if (_bAlert && !_bLockedOn)
            {
                _dAlertTimer += theGameTime.ElapsedGameTime.TotalSeconds;
               // _sprAlert.Update(theGameTime);
            }
        }

        public override void Draw(SpriteBatch spriteBatch, bool userLayerDepth = false)
        {
            base.Draw(spriteBatch, userLayerDepth);
            //if (_bAlert) { _sprAlert.Draw(spriteBatch, userLayerDepth); }
        }

        private void UpdateMovement(GameTime theGameTime)
        {
            bool move = true;
            Vector2 direction = Vector2.Zero;

            //Handle Leashing and Idle Movement targetting
            HandlePassivity(theGameTime);

            if (_bLockedOn)
            {
                if (Math.Abs(_vLeashPoint.X - Position.X) <= _fLeashRange && Math.Abs(_vLeashPoint.Y - Position.Y) <= _fLeashRange)
                {
                    _vMoveTo = Vector2.Zero;

                    if (_bJump)
                    {
                        if (_vJumpTo == Vector2.Zero)
                        {
                            _vJumpTo = PlayerManager.World.Position;
                        }
                        _vMoveTo = _vJumpTo;
                    }
                    else
                    {
                        _vMoveTo = PlayerManager.World.Position;
                    }
                }
                else if (!BodySprite.CurrentAnimation.StartsWith("Air"))
                {
                    _bLeashed = true;
                    _bLockedOn = false;
                    _vMoveTo = _vLeashPoint;
                    DetermineFacing(new Vector2(_vMoveTo.X - Position.X, _vMoveTo.Y - Position.Y));
                }
            }

            #region Jumping Code
            //if (_bJump)
            //{
            //    if (_vMoveTo != Vector2.Zero && BodySprite.CurrentAnimation.StartsWith("Idle"))
            //    {
            //        move = false;

            //        string animation = "";
            //        switch (Facing)
            //        {
            //            case DirectionEnum.Down:
            //                animation = Util.GetEnumString(WActorJumpAnim.GroundDown);
            //                break;
            //            case DirectionEnum.Up:
            //                animation = Util.GetEnumString(WActorJumpAnim.GroundUp);
            //                break;
            //            case DirectionEnum.Left:
            //                animation = Util.GetEnumString(WActorJumpAnim.GroundLeft);
            //                break;
            //            case DirectionEnum.Right:
            //                animation = Util.GetEnumString(WActorJumpAnim.GroundRight);
            //                break;
            //        }

            //        PlayAnimation(animation);
            //    }
            //    else if (BodySprite.CurrentAnimation.StartsWith("Ground") && BodySprite.CurrentFrameAnimation.PlayCount < 1)
            //    {
            //        move = false;
            //    }
            //    else if (!BodySprite.CurrentAnimation.StartsWith("Idle") && BodySprite.CurrentFrameAnimation.PlayCount >= 1)
            //    {
            //        bool jumping = BodySprite.CurrentAnimation.StartsWith("Air");
            //        string animation = "";
            //        switch (Facing)
            //        {
            //            case DirectionEnum.Down:
            //                animation = jumping ? Util.GetEnumString(WActorJumpAnim.GroundDown) : Util.GetEnumString(WActorJumpAnim.AirDown);
            //                break;
            //            case DirectionEnum.Up:
            //                animation = jumping ? Util.GetEnumString(WActorJumpAnim.GroundUp) : Util.GetEnumString(WActorJumpAnim.AirUp); ;
            //                break;
            //            case DirectionEnum.Left:
            //                animation = jumping ? Util.GetEnumString(WActorJumpAnim.GroundLeft) : Util.GetEnumString(WActorJumpAnim.AirLeft); ;
            //                break;
            //            case DirectionEnum.Right:
            //                animation = jumping ? Util.GetEnumString(WActorJumpAnim.GroundRight) : Util.GetEnumString(WActorJumpAnim.AirRight); ;
            //                break;
            //        }
            //        _vJumpTo = Vector2.Zero;
            //        PlayAnimation(animation);
            //    }
            //}
            #endregion

            if (move && _vMoveTo != Vector2.Zero)
            {
                float deltaX = Math.Abs(_vMoveTo.X - this.Position.X);
                float deltaY = Math.Abs(_vMoveTo.Y - this.Position.Y);

                Util.GetMoveSpeed(Position, _vMoveTo, BuffedSpeed, ref direction);
                DetermineFacing(direction);
                if (!CheckMapForCollisionsAndMove(direction, false))
                {
                    _iMoveFailures++;
                }
                NewFoV();

                //We have finished moving
                if (Position.X == _vMoveTo.X && Position.Y == _vMoveTo.Y)
                {
                    //If we were peashing back to our start point unset it.
                    if (_bLeashed && _vMoveTo == _vLeashPoint)
                    {
                        _vLeashPoint = Vector2.Zero;
                        _bLeashed = false;
                    }

                    _vMoveTo = Vector2.Zero;
                    Idle();
                }
            }

            if (CollisionBox.Intersects(PlayerManager.World.CollisionBox))
            {
                _bAlert = false;
                _bLockedOn = false;
                LiteCombatManager.NewBattle(this);
            }
        }

        //If mob is not returning to leash point and player in in range
        private void HandlePassivity(GameTime theGameTime)
        {
            if (!_bLeashed && !_bLockedOn && _FoV.Contains(PlayerManager.World))
            {
                //If alert if not on, set alert
                if (!_bAlert)
                {
                    _bAlert = true;
                    _dAlertTimer = 0;
                }

                if (_dAlertTimer >= MAX_ALERT)
                {
                    if (_vLeashPoint == Vector2.Zero) { _vLeashPoint = Position; }
                    _bLockedOn = true;
                    _bAlert = false;
                }
            }
            else
            {
                if (_bAlert)
                {
                    _bAlert = false;
                    _dAlertTimer = 0;
                }
                GetIdleMovementTarget(theGameTime);
            }
        }
        private void GetIdleMovementTarget(GameTime theGameTime)
        {
            if ((_vMoveTo == Vector2.Zero && _dIdleFor <= 0 && !BodySprite.CurrentAnimation.StartsWith("Air")) || _iMoveFailures == 5)
            {
                _iMoveFailures = 0;
                int howFar = 2;
                bool skip = false;
                RHRandom r = RHRandom.Instance();
                int decision = r.Next(1, 5);
                if (decision == 1) { _vMoveTo = new Vector2(Position.X - r.Next(1, howFar) * TILE_SIZE, Position.Y); }
                else if (decision == 2) { _vMoveTo = new Vector2(Position.X + r.Next(1, howFar) * TILE_SIZE, Position.Y); }
                else if (decision == 3) { _vMoveTo = new Vector2(Position.X, Position.Y - r.Next(1, howFar) * TILE_SIZE); }
                else if (decision == 4) { _vMoveTo = new Vector2(Position.X, Position.Y + r.Next(1, howFar) * TILE_SIZE); }
                else
                {
                    _vMoveTo = Vector2.Zero;
                    _dIdleFor = 4;
                    Idle();
                    skip = true;
                }

                if (!skip)
                {
                    DetermineFacing(new Vector2(_vMoveTo.X - Position.X, _vMoveTo.Y - Position.Y));
                }
            }
            else if (_vMoveTo == Vector2.Zero)
            {
                _dIdleFor -= theGameTime.ElapsedGameTime.TotalSeconds;
            }
        }

        public override void DetermineFacing(Vector2 direction)
        {
            string animation = string.Empty;

            if (direction.Length() == 0)
            {
                Idle();
            }
            else
            {
                if (!_bJump || (_bJump && !BodySprite.CurrentAnimation.StartsWith("Air")))
                {
                    if (Math.Abs((int)direction.X) > Math.Abs((int)direction.Y))
                    {
                        if (direction.X > 0)
                        {
                            Facing = DirectionEnum.Right;
                            //animation = Util.GetEnumString(WActorWalkAnim.WalkRight); //_bJump ? Util.GetEnumString(WActorJumpAnim.GroundRight) : Util.GetEnumString(WActorWalkAnim.WalkRight);
                        }
                        else if (direction.X < 0)
                        {
                            Facing = DirectionEnum.Left;
                            //animation = Util.GetEnumString(WActorWalkAnim.WalkLeft); //_bJump ? Util.GetEnumString(WActorJumpAnim.GroundLeft) : Util.GetEnumString(WActorWalkAnim.WalkLeft);
                        }
                    }
                    else
                    {
                        if (direction.Y > 0)
                        {
                            Facing = DirectionEnum.Down;
                            //animation = Util.GetEnumString(WActorWalkAnim.WalkDown);//_bJump ? Util.GetEnumString(WActorJumpAnim.GroundDown) : Util.GetEnumString(WActorWalkAnim.WalkDown);
                        }
                        else if (direction.Y < 0)
                        {
                            Facing = DirectionEnum.Up;
                            // animation = Util.GetEnumString(WActorWalkAnim.WalkUp);// _bJump ? Util.GetEnumString(WActorJumpAnim.GroundUp) : Util.GetEnumString(WActorWalkAnim.WalkUp);
                        }
                    }

                    PlayAnimationVerb(VerbEnum.Walk);
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

        public void Stun()
        {
            _dStun = 5.0f;
        }

        /// <summary>
        /// Gets all the items that the Monsters drop. Each monster gives one item
        /// </summary>
        /// <returns>The list of items to be given to the player</returns>
        public List<Item> GetLoot()
        {
            List<Item> items = new List<Item>();

            foreach (LiteMonster m in _liMonsters)
            {
                items.Add(m.GetLoot());
            }
            return items;
        }

        /// <summary>
        /// Delegate method to retrieve XP data
        /// </summary>
        /// <param name="xpLeftToGive">The amount of xp left to give</param>
        /// <param name="totalXP">The total amount of XP to give</param>
        public void GetXP(ref double xpLeftToGive, ref double totalXP)
        {
            xpLeftToGive = _iXPToGive;
            totalXP = _iXP;
        }

        /// <summary>
        /// Drains away the given amount of XP fromt he pool of XP left to give.
        /// If it would go below zero, we instead drain whatever remains.
        /// </summary>
        /// <param name="v"></param>
        public void DrainXP(int v)
        {
            _iXPToGive -= Math.Min(v, _iXPToGive);
        }

        private class FieldOfVision
        {
            int _iMaxRange;
            Vector2 _vFirst;            //The LeftMost of the TopMost
            Vector2 _vSecond;           //The RightMost of the BottomMost
            DirectionEnum _eDir;

            public FieldOfVision(LiteMob theMob, int maxRange)
            {
                int sideRange = TILE_SIZE * 2;
                _iMaxRange = maxRange;
                _eDir = theMob.Facing;
                if (_eDir == DirectionEnum.Up || _eDir == DirectionEnum.Down)
                {
                    _vFirst = theMob.Center - new Vector2(sideRange, 0);
                    _vSecond = theMob.Center + new Vector2(sideRange, 0);
                }
                else
                {
                    _vFirst = theMob.Center - new Vector2(0, sideRange);
                    _vSecond = theMob.Center + new Vector2(0, sideRange);
                }
            }

            public void MoveBy(Vector2 v)
            {
                _vFirst += v;
                _vSecond += v;
            }

            public bool Contains(WorldActor actor)
            {
                bool rv = false;
                Vector2 center = actor.CollisionBox.Center.ToVector2();

                Vector2 firstFoV = _vFirst;
                Vector2 secondFoV = _vSecond;
                //Make sure the actor could be in range
                if (_eDir == DirectionEnum.Up && Util.InBetween(center.Y, firstFoV.Y - _iMaxRange, firstFoV.Y))
                {
                    float yMod = Math.Abs(center.Y - firstFoV.Y);
                    firstFoV += new Vector2(-yMod, -yMod);
                    secondFoV += new Vector2(yMod, -yMod);

                    rv = Util.InBetween(center.X, firstFoV.X, secondFoV.X) && Util.InBetween(center.Y, firstFoV.Y, _vFirst.Y);
                }
                else if (_eDir == DirectionEnum.Down && Util.InBetween(center.Y, firstFoV.Y, firstFoV.Y + _iMaxRange))
                {
                    float yMod = Math.Abs(center.Y - firstFoV.Y);
                    firstFoV += new Vector2(-yMod, yMod);
                    secondFoV += new Vector2(yMod, yMod);

                    rv = Util.InBetween(center.X, firstFoV.X, secondFoV.X) && Util.InBetween(center.Y, _vFirst.Y, firstFoV.Y);
                }
                else if (_eDir == DirectionEnum.Left && Util.InBetween(center.X, firstFoV.X - _iMaxRange, firstFoV.X))
                {
                    float xMod = Math.Abs(center.X - firstFoV.X);
                    firstFoV += new Vector2(-xMod, -xMod);
                    secondFoV += new Vector2(-xMod, xMod);

                    rv = Util.InBetween(center.Y, firstFoV.Y, secondFoV.Y) && Util.InBetween(center.X, firstFoV.X, _vFirst.X);
                }
                else if (_eDir == DirectionEnum.Right && Util.InBetween(center.X, firstFoV.X, firstFoV.X + _iMaxRange))
                {
                    float xMod = Math.Abs(center.X - firstFoV.X);
                    firstFoV += new Vector2(xMod, -xMod);
                    secondFoV += new Vector2(xMod, xMod);

                    rv = Util.InBetween(center.Y, firstFoV.Y, secondFoV.Y) && Util.InBetween(center.X, _vFirst.X, firstFoV.X);
                }

                return rv;
            }
        }
    }
}
