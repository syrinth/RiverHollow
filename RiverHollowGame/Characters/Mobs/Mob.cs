using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.Game_Managers;
using RiverHollow.Items;
using RiverHollow.Misc;
using RiverHollow.Utilities;
using System;
using System.Collections.Generic;
using static RiverHollow.Game_Managers.GameManager;
using static RiverHollow.Utilities.Enums;

namespace RiverHollow.Characters
{
    public class Mob : CombatActor
    {
        #region Properties
        public bool HasProjectiles => GetBoolByIDKey("Projectile");
        public int Damage => GetIntByIDKey("Damage");
        protected float Cooldown => GetFloatByIDKey("Cooldown");
        public override float MaxHP => GetIntByIDKey("HP");
        private string[] LootData => Util.FindParams(GetStringByIDKey("ItemID"));

        protected bool TracksPlayer => GetBoolByIDKey("TracksPlayer");
        protected bool MaintainDistance => GetBoolByIDKey("MaintainDistance");

        protected bool _bUsingAction = false;

        bool _bJump;
        int _iMaxRange = Constants.TILE_SIZE * 10;

        Vector2 _vMovementVelocity;
        Point _vJumpTo;

        FieldOfVision _FoV;

        #endregion

        public Mob(int id, Dictionary<string, string> stringData) : base(id, stringData)
        {
            SpdMult = 1;
            CurrentHP = MaxHP;
            IgnoreCollisions = GetBoolByIDKey("Fly");

            _fBaseSpeed = GetFloatByIDKey("Speed", 1);
            _fWanderSpeed = _fBaseSpeed;
            _cooldownTimer = new RHTimer(Cooldown, true);
            
            //_bJump = data.ContainsKey("Jump");

            BodySprite = LoadSpriteAnimations(Util.LoadWorldAnimations(stringData), DataManager.FOLDER_MOBS + stringData["Key"]);

            NewFoV();

            if (GetBoolByIDKey("Wander"))
            {
                var strParams = Util.FindArguments(GetStringByIDKey("Wander"));
                _fBaseWanderTimer = float.Parse(strParams[0]);
                _iMinWander = int.Parse(strParams[1]);
                _iMaxWander = int.Parse(strParams[2]);
                _movementTimer = new RHTimer(_fBaseWanderTimer);

                if(strParams.Length == 4)
                {
                    _fWanderSpeed = float.Parse(strParams[3]);
                }
            }
        }

        public void NewFoV()
        {
            _FoV = new FieldOfVision(this, _iMaxRange);
        }

        public override void Update(GameTime gTime)
        {
            BodySprite.Update(gTime);
            
            if (!GamePaused())
            {
                if (BodySprite.AnimationFinished(AnimationEnum.KO))
                {
                    DamageTimerEnd();
                    DropLoot();
                    CurrentMap.RemoveActor(this);
                    TaskManager.AdvanceTaskProgress(this);
                    TownManager.TrackDefeatedMob(this);
                    return;
                }

                if (HasKnockbackVelocity())
                {
                    ApplyKnockbackVelocity();
                }
                else if (HasHP && ! _bUsingAction)
                {
                    HandleMove();
                }

                CheckDamageTimers(gTime);

                if (HasHP && HitBox.Intersects(PlayerManager.PlayerActor.HitBox))
                {
                    PlayerManager.PlayerActor.DealDamage(Damage, CollisionBox);
                }

                if (CanAct())
                {
                    DetermineMovement(gTime);
                    DetermineAction(gTime);
                }
            }
        }

        public override void Draw(SpriteBatch spriteBatch, bool userLayerDepth = false)
        {
            base.Draw(spriteBatch, userLayerDepth); 
            //if (_bAlert) { _sprAlert.Draw(spriteBatch, userLayerDepth); }
        }

        protected void DetermineMovement(GameTime gTime)
        {
            if (_bUsingAction)
            {
                return;
            }

            switch (_eCurrentState)
            {
                case NPCStateEnum.Wander:
                    if (!ScanForPlayer())
                    {
                        if (_bBumpedIntoSomething)
                        {
                            SetMoveTo(Point.Zero);
                            _bBumpedIntoSomething = false;
                        }

                        if (!HasMovement() || _bBumpedIntoSomething)
                        {
                            Wander(gTime, 0);
                        }
                    }
                    break;
                case NPCStateEnum.Idle:
                    ScanForPlayer();
                    break;
                case NPCStateEnum.TrackPlayer:
                    if (!HasKnockbackVelocity())
                    {
                        bool impeded = false;
                        if (CurrentMap.CheckForCollisions(this, ref _vMovementVelocity, ref impeded))
                        {
                            MoveActor(_vMovementVelocity);
                        }
                        PlayAnimation(VerbEnum.Walk);

                        Vector2 mod = GetPlayerDirection() * 0.015f;
                        _vMovementVelocity += mod;
                        _vMovementVelocity.Normalize();
                        _vMovementVelocity *= _fBaseSpeed;
                    }
                    break;
                case NPCStateEnum.MaintainDistance:
                    if (OnScreen() && PlayerManager.PlayerInRange(CollisionCenter, Constants.TILE_SIZE * 8))
                    {
                        if (!HasKnockbackVelocity())
                        {
                            bool impeded = false;
                            if (CurrentMap.CheckForCollisions(this, ref _vMovementVelocity, ref impeded))
                            {
                                MoveActor(_vMovementVelocity);
                            }

                            PlayAnimation(VerbEnum.Walk); 

                            Vector2 mod = GetPlayerDirection() * -0.015f;
                            _vMovementVelocity += mod;
                            _vMovementVelocity.Normalize();
                            _vMovementVelocity *= _fBaseSpeed;
                        }
                    }
                    else
                    {
                        ResetState();
                    }
                    break;
            }
        }

        protected bool ScanForPlayer()
        {
            bool rv = false;
            if ((TracksPlayer || MaintainDistance )&& OnScreen() && PlayerManager.PlayerInRange(CollisionCenter, Constants.TILE_SIZE * 6))
            {
                rv = true;
                Wandering = false;
                if (TracksPlayer)
                {
                    _eCurrentState = NPCStateEnum.TrackPlayer;
                    _vMovementVelocity = GetPlayerDirectionNormal();
                }
                else if (MaintainDistance)
                {
                    _eCurrentState = NPCStateEnum.MaintainDistance;
                    _vMovementVelocity = GetPlayerDirectionNormal() * -1;
                }
                SetMoveTo(Point.Zero);
            }

            return rv;
        }
        protected virtual void DetermineAction(GameTime gTime) { }

        protected override void Kill()
        {
            base.Kill();
        }

        public override bool DealDamage(int value, Rectangle hitbox)
        {
            bool rv = base.DealDamage(value, hitbox);

            if (rv)
            {
                SetMoveTo(Point.Zero);
                _bBumpedIntoSomething = true;

                if (!HasHP)
                {
                    IgnoreCollisions = false;
                }
            }

            return rv;
        }

        protected override void Flicker(bool value)
        {
            base.Flicker(value);
            BodySprite.SetColor(_bFlicker ? Color.White : Color.Red);
        }

        public bool CanAct()
        {
            return HasHP && !HasKnockbackVelocity();
        }

        public override void DetermineAnimationState(Vector2 direction)
        {
            if (!HasHP)
            {
                return;
            }

            if (direction.Length() == 0)
            {
                PlayAnimationVerb(VerbEnum.Idle);
            }
            else
            {
                if (!_bJump || (_bJump && !BodySprite.CurrentAnimation.StartsWith("Air")))
                {
                    if (Math.Abs(direction.X) > Math.Abs(direction.Y))
                    {
                        if (direction.X > 0)
                        {
                            SetFacing(DirectionEnum.Right);
                            //animation = Util.GetEnumString(WActorWalkAnim.WalkRight); //_bJump ? Util.GetEnumString(WActorJumpAnim.GroundRight) : Util.GetEnumString(WActorWalkAnim.WalkRight);
                        }
                        else if (direction.X < 0)
                        {
                            SetFacing(DirectionEnum.Left);
                            //animation = Util.GetEnumString(WActorWalkAnim.WalkLeft); //_bJump ? Util.GetEnumString(WActorJumpAnim.GroundLeft) : Util.GetEnumString(WActorWalkAnim.WalkLeft);
                        }
                    }
                    else
                    {
                        if (direction.Y > 0)
                        {
                            SetFacing(DirectionEnum.Down);
                            //animation = Util.GetEnumString(WActorWalkAnim.WalkDown);//_bJump ? Util.GetEnumString(WActorJumpAnim.GroundDown) : Util.GetEnumString(WActorWalkAnim.WalkDown);
                        }
                        else if (direction.Y < 0)
                        {
                            SetFacing(DirectionEnum.Up);
                            // animation = Util.GetEnumString(WActorWalkAnim.WalkUp);// _bJump ? Util.GetEnumString(WActorJumpAnim.GroundUp) : Util.GetEnumString(WActorWalkAnim.WalkUp);
                        }
                    }

                    PlayAnimationVerb(VerbEnum.Walk);
                }
            }
        }

        /// <summary>
        /// Gets all the items that the Monsters drop. Each monster gives one item
        /// </summary>
        /// <returns>The list of items to be given to the player</returns>
        public void DropLoot()
        {
            var lootDictionary = new Dictionary<RarityEnum, List<int>>();

            foreach (string s in LootData)    
            {
                int resourceID = -1;
                RarityEnum rarity = RarityEnum.C;
                Util.GetRarity(s, ref resourceID, ref rarity);

                Util.AddToListDictionary(ref lootDictionary, rarity, resourceID);
            }

            Item drop = DataManager.GetItem(Util.RollOnRarityTable(lootDictionary, -1));
            if (drop != null)
            {
                MapManager.CurrentMap.SpawnItemOnMap(drop, CollisionBoxLocation, false);
            }
        }

        public void Reset()
        {
            ResetState();
            SetPosition(_pLeashPoint);
            SetMoveTo(Point.Zero);
            CurrentHP = MaxHP;
        }

        private void ResetState()
        {
            if (GetBoolByIDKey("Wander"))
            {
                _eCurrentState = NPCStateEnum.Wander;
            }
            else
            {
                _eCurrentState = NPCStateEnum.Idle;
                PlayAnimation(VerbEnum.Idle);
            }
        }

        public void SetInitialPoint(Point p)
        {
            _pLeashPoint = p;
        }

        protected override WeightEnum GetWeight()
        {
            return GetEnumByIDKey<WeightEnum>("Weight");
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
        //        _sprBody.AddAnimation(WActorBaseAnim.IdleDown, 0, 0, Constants.TILE_SIZE, Constants.TILE_SIZE * 2, 4, 0.2f);
        //        _sprBody.AddAnimation(WActorWalkAnim.WalkDown, 0, 0, Constants.TILE_SIZE, Constants.TILE_SIZE * 2, 4, 0.2f);
        //        _sprBody.AddAnimation(WActorBaseAnim.IdleUp, 64, 0, Constants.TILE_SIZE, Constants.TILE_SIZE * 2, 4, 0.2f);
        //        _sprBody.AddAnimation(WActorWalkAnim.WalkUp, 64, 0, Constants.TILE_SIZE, Constants.TILE_SIZE * 2, 4, 0.2f);
        //        _sprBody.AddAnimation(WActorBaseAnim.IdleLeft, 128, 0, Constants.TILE_SIZE, Constants.TILE_SIZE * 2, 4, 0.2f);
        //        _sprBody.AddAnimation(WActorWalkAnim.WalkLeft, 128, 0, Constants.TILE_SIZE, Constants.TILE_SIZE * 2, 4, 0.2f);
        //        _sprBody.AddAnimation(WActorBaseAnim.IdleRight, 192, 0, Constants.TILE_SIZE, Constants.TILE_SIZE * 2, 4, 0.2f);
        //        _sprBody.AddAnimation(WActorWalkAnim.WalkRight, 192, 0, Constants.TILE_SIZE, Constants.TILE_SIZE * 2, 4, 0.2f);
        //        _sprBody.PlayAnimation(WActorWalkAnim.WalkDown);
        //    }
        //    #region Jumping Code
        //    else
        //    {
        //        //_sprBody.AddAnimation(WActorBaseAnim.IdleDown, 0, 0, Constants.TILE_SIZE, Constants.TILE_SIZE * 2, 2, 0.2f);
        //        //_sprBody.AddAnimation(WActorJumpAnim.GroundDown, 0, 0, Constants.TILE_SIZE, Constants.TILE_SIZE * 2, 2, 0.2f);
        //        //_sprBody.AddAnimation(WActorJumpAnim.AirDown, 32, 0, Constants.TILE_SIZE, Constants.TILE_SIZE * 2, 2, 0.2f);

        //        //_sprBody.AddAnimation(WActorBaseAnim.IdleUp, 64, 0, Constants.TILE_SIZE, Constants.TILE_SIZE * 2, 2, 0.2f);
        //        //_sprBody.AddAnimation(WActorJumpAnim.GroundUp, 64, 0, Constants.TILE_SIZE, Constants.TILE_SIZE * 2, 2, 0.2f);
        //        //_sprBody.AddAnimation(WActorJumpAnim.AirUp, 96, 0, Constants.TILE_SIZE, Constants.TILE_SIZE * 2, 2, 0.2f);

        //        //_sprBody.AddAnimation(WActorBaseAnim.IdleLeft, 128, 0, Constants.TILE_SIZE, Constants.TILE_SIZE * 2, 2, 0.2f);
        //        //_sprBody.AddAnimation(WActorJumpAnim.GroundLeft, 128, 0, Constants.TILE_SIZE, Constants.TILE_SIZE * 2, 2, 0.2f);
        //        //_sprBody.AddAnimation(WActorJumpAnim.AirLeft, 160, 0, Constants.TILE_SIZE, Constants.TILE_SIZE * 2, 2, 0.2f);

        //        //_sprBody.AddAnimation(WActorBaseAnim.IdleRight, 192, 0, Constants.TILE_SIZE, Constants.TILE_SIZE * 2, 2, 0.2f);
        //        //_sprBody.AddAnimation(WActorJumpAnim.GroundRight, 192, 0, Constants.TILE_SIZE, Constants.TILE_SIZE * 2, 2, 0.2f);
        //        //_sprBody.AddAnimation(WActorJumpAnim.AirRight, 224, 0, Constants.TILE_SIZE, Constants.TILE_SIZE * 2, 2, 0.2f);
        //        //_sprBody.SetCurrentAnimation(WActorBaseAnim.IdleDown);
        //    }
        //    #endregion
        //    Facing = DirectionEnum.Down;

        //    Width = _sprBody.Width;
        //    Height = _sprBody.Height;

        //    //_sprAlert = new AnimatedSprite(@"Textures\Dialog");
        //    //_sprAlert.AddAnimation(GenAnimEnum.Play, 64, 64, 16, 16, 3, 0.2f, true);
        //    //_sprAlert.PlayAnimation(GenAnimEnum.Play);
        //    //_sprAlert.Position = (Position - new Vector2(0, Constants.TILE_SIZE));
        //} 
        #endregion
    }
}
