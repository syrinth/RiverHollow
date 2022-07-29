using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.Game_Managers;
using RiverHollow.Items;
using RiverHollow.Map_Handling;
using RiverHollow.Utilities;
using System;
using System.Collections.Generic;

using static RiverHollow.Game_Managers.GameManager;
using static RiverHollow.Utilities.Enums;

namespace RiverHollow.Characters
{
    public class Mob : WorldActor
    {
        #region Properties
        protected double _dIdleFor;
        protected int _iLeash = 7;

        protected List<CombatActor> _liMonsters;
        public List<CombatActor> Monsters { get => _liMonsters; }

        double _dStun;
        int _iMaxRange = TILE_SIZE * 10;
        bool _bAlert;
        bool _bJump;

        bool _bDefeated = false;
        Vector2 _vJumpTo;
        const double MAX_ALERT = 1;
        double _dAlertTimer;
        //AnimatedSprite _sprAlert;

        FieldOfVision _FoV;
        Vector2 _vLeashPoint;
        float _fLeashRange = TILE_SIZE * 30;

        List<SpawnConditionEnum> _liSpawnConditions;

        int _iXP;
        public int XP { get; private set; }

        #endregion

        public Mob(int id, Dictionary<string, string> data) : base(id)
        {
            _liSpawnConditions = new List<SpawnConditionEnum>();
            _eActorType = WorldActorTypeEnum.Mob;
            _liMonsters = new List<CombatActor>();

            _fBaseSpeed = 1;
            ImportBasics(data);


            _iXP = 0;
            foreach (Monster mon in _liMonsters)
            {
                _iXP += mon.XP;
            }
            XP = _iXP;

            NewFoV();
        }

        protected int ImportBasics(Dictionary<string, string> data)
        {
            string[] monsterPool = Util.FindParams(data["MonsterID"]);
            int index = RHRandom.Instance().Next(0, monsterPool.Length -1);

            string[] split = monsterPool[index].Split('-');
            for (int i = 0; i < split.Length; i++)
            {
                int mID = int.Parse(split[i]);
                _liMonsters.Add(DataManager.GetLiteMonsterByIndex(mID));
            }

            //split = data["Condition"].Split('-');
            //for (int i = 0; i < split.Length; i++)
            //{
            //    _liSpawnConditions.Add(Util.ParseEnum<SpawnConditionEnum>(split[i]));
            //}

            //_bJump = data.ContainsKey("Jump");

            foreach (CombatActor m in _liMonsters)
            {
                List<CombatActor> match = _liMonsters.FindAll(x => ((Monster)x).ID == ((Monster)m).ID);
                if (match.Count > 1)
                {
                    for (int i = 0; i < match.Count; i++)
                    {
                        match[i].SetUnique(Util.NumToString(i + 1, true));
                    }
                }
            }

            LoadSpriteAnimations(ref _sprBody, Util.LoadWorldAnimations(data), DataManager.FOLDER_MOBS + data["Texture"]);
            return 0;
        }

        public void NewFoV()
        {
            _FoV = new FieldOfVision(this, _iMaxRange);
        }

        public override void Update(GameTime gTime)
        {
            base.Update(gTime);

            if (!CutsceneManager.Playing && !GamePaused())
            {
                //Check if the mob is still stunned
                if (_dStun > 0)
                {
                    _dStun -= gTime.ElapsedGameTime.TotalSeconds;
                    if (_dStun < 0) { _dStun = 0; }
                }
                else
                {
                    switch (_eCurrentState)
                    {
                        case NPCStateEnum.Wander:
                            if (_bBumpedIntoSomething)
                            {
                                _bBumpedIntoSomething = false;
                                MoveToLocation = Vector2.Zero;
                                ChangeState(NPCStateEnum.Idle);
                                break;
                            }
                            else {
                                goto case NPCStateEnum.Idle;
                            }
                        case NPCStateEnum.Idle:
                            if (PlayerManager.PlayerInRange(CollisionBox.Center, TILE_SIZE * 8))
                            {
                                ChangeState(NPCStateEnum.TrackPlayer);
                                _vLeashPoint = new Vector2(CollisionBox.Left, CollisionBox.Top);
                            }
                            break;
                        case NPCStateEnum.TrackPlayer:
                            int distance = (int)Util.GetDistance(Position, _vLeashPoint);

                            if (distance > TILE_SIZE * 25)
                            {
                                ChangeState(NPCStateEnum.Leashing);
                            }
                            break;
                        case NPCStateEnum.Leashing:
                            if(Position == _vLeashPoint)
                            {
                                ChangeState(NPCStateEnum.Wander);
                            }
                            break;

                    }

                    ProcessStateEnum(gTime, false);
                    if (CollisionBox.Intersects(PlayerManager.PlayerActor.CollisionBox))
                    {
                        if (!_bDefeated)
                        {
                            _bAlert = false;
                            CombatManager.NewBattle(this);
                            MoveToLocation = Vector2.Zero;
                            _liTilePath.Clear();
                        }
                    }
                }
            }
        }

        public override void Draw(SpriteBatch spriteBatch, bool userLayerDepth = false)
        {
            base.Draw(spriteBatch, userLayerDepth);
            //if (_bAlert) { _sprAlert.Draw(spriteBatch, userLayerDepth); }
        }

        public override void DetermineFacing(Vector2 direction)
        {
            string animation = string.Empty;

            if (direction.Length() == 0)
            {
                PlayAnimationVerb(VerbEnum.Idle);
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

        public void Defeat()
        {
            _bDefeated = true;
            CurrentMap.RemoveActor(this);
        }

        public void Stun()
        {
            _dStun = 5.0f;
        }

        /// <summary>
        /// Gets all the items that the Monsters drop. Each monster gives one item
        /// </summary>
        /// <returns>The list of items to be given to the player</returns>
        public Item[,] GetLoot()
        {
            List<Item> items = new List<Item>();
            foreach(Monster m in _liMonsters)
            {
                Item newItem = m.GetLoot();
                int index = items.FindIndex(x => x.ItemID == newItem.ItemID);

                if (index != -1) { items[index].Add(newItem.Number); }
                else { items.Add(newItem); }
            }

            Item[,] rv = new Item[1, items.Count];
            for (int i = 0; i < items.Count; i++)
            {
                rv[0, i] = items[i];
            }
            return rv;
        }

        /// <summary>
        /// Delegate method to retrieve XP data
        /// </summary>
        /// <param name="xpLeftToGive">The amount of xp left to give</param>
        /// <param name="totalXP">The total amount of XP to give</param>
        public void GetXP(ref double xpLeftToGive, ref double totalXP)
        {
            xpLeftToGive = XP;
            totalXP = _iXP;
        }

        /// <summary>
        /// Drains away the given amount of XP fromt he pool of XP left to give.
        /// If it would go below zero, we instead drain whatever remains.
        /// </summary>
        /// <param name="v"></param>
        public void DrainXP(int v)
        {
            XP -= Math.Min(v, XP);
        }

        protected override void CalculatePath()
        {
            Vector2 startPosition = Position;
            Vector2 target = _eCurrentState == NPCStateEnum.TrackPlayer ? PlayerManager.PlayerActor.CollisionBox.Center.ToVector2() : _vLeashPoint;
            //RHTile lastTile = _liTilePath.Count > 0 ? _liTilePath[0] : null;
            _liTilePath = TravelManager.FindPathToLocation(ref startPosition, target, null, false, false);

            if (_liTilePath?.Count > 0 && _liTilePath?.Count < 30)
            {
                MoveToLocation = _liTilePath[0].Position;
            }
            else
            {
                _liTilePath = new List<RHTile>();
            }

            TravelManager.FinishThreading(ref _pathingThread);
        }

        public void ResetPathing()
        {
            if(_eCurrentState == NPCStateEnum.TrackPlayer)
            {
                ChangeState(NPCStateEnum.Idle);
                Position = _vLeashPoint;
                _liTilePath.Clear();
                MoveToLocation = Vector2.Zero;
                _vLeashPoint = Vector2.Zero;
            }
        }

        private class FieldOfVision
        {
            int _iMaxRange;
            Vector2 _vFirst;            //The LeftMost of the TopMost
            Vector2 _vSecond;           //The RightMost of the BottomMost
            DirectionEnum _eDir;

            public FieldOfVision(Mob theMob, int maxRange)
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


        #region Jumping Code
        //private void UpdateMovement(GameTime theGameTime)
        //{
        //    bool move = true;
        //    Vector2 direction = Vector2.Zero;


        //    if (Math.Abs(_vLeashPoint.X - Position.X) <= _fLeashRange && Math.Abs(_vLeashPoint.Y - Position.Y) <= _fLeashRange)
        //    {
        //        _vMoveTo = Vector2.Zero;

        //        if (_bJump)
        //        {
        //            if (_vJumpTo == Vector2.Zero)
        //            {
        //                _vJumpTo = PlayerManager.PlayerActor.Position;
        //            }
        //            _vMoveTo = _vJumpTo;
        //        }
        //        else
        //        {
        //            _vMoveTo = PlayerManager.PlayerActor.Position;
        //        }
        //    }
        //    else if (!BodySprite.CurrentAnimation.StartsWith("Air"))
        //    {
        //        _bLeashed = true;
        //        _vMoveTo = _vLeashPoint;
        //        DetermineFacing(new Vector2(_vMoveTo.X - Position.X, _vMoveTo.Y - Position.Y));
        //    }

        //    if (move && _vMoveTo != Vector2.Zero)
        //    {
        //        float deltaX = Math.Abs(_vMoveTo.X - this.Position.X);
        //        float deltaY = Math.Abs(_vMoveTo.Y - this.Position.Y);

        //        Util.GetMoveSpeed(Position, _vMoveTo, BuffedSpeed, ref direction);
        //        DetermineFacing(direction);

        //        HandleMove(_vMoveTo);
        //        NewFoV();

        //        //We have finished moving
        //        if (Position.X == _vMoveTo.X && Position.Y == _vMoveTo.Y)
        //        {
        //            //If we were peashing back to our start point unset it.
        //            if (_bLeashed && _vMoveTo == _vLeashPoint)
        //            {
        //                _vLeashPoint = Vector2.Zero;
        //                _bLeashed = false;
        //            }

        //            _vMoveTo = Vector2.Zero;
        //            PlayAnimationVerb(VerbEnum.Idle);
        //            _dIdleFor = RHRandom.Instance().Next(1, 4);
        //        }
        //    }
        //}

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

        //LoadContent(DataManager.FOLDER_MOBS + data["Texture"];);
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
        #endregion
    }
}
