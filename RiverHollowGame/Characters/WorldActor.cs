using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.Game_Managers;
using RiverHollow.Misc;
using RiverHollow.SpriteAnimations;
using RiverHollow.Map_Handling;
using RiverHollow.Utilities;
using System;
using System.Collections.Generic;
using System.Threading;

using static RiverHollow.Utilities.Enums;
using static RiverHollow.Game_Managers.GameManager;

namespace RiverHollow.Characters
{
    public abstract class WorldActor : Actor
    {
        #region Properties

        public int ID { get; } = -1;

        public bool Invulnerable { get; protected set; } = true;

        public virtual float MaxHP { get; protected set; } = 2;
        public float CurrentHP { get; protected set; } = 2;

        protected RHTimer _flickerTimer;
        protected RHTimer _damageTimer;

        public bool Moving { get; set; } = false;
        public DirectionEnum Facing = DirectionEnum.Down;
        public ActorStateEnum State { get; protected set; } = ActorStateEnum.Walk;

        public WorldActorTypeEnum ActorType { get; protected set; }

        public Vector2 MoveToLocation { get; private set; }
        protected Vector2 _vLeashPoint;
        protected Vector2 _vVelocity;
        protected float _fDecay;

        public string CurrentMapName;
        public RHMap CurrentMap => (!string.IsNullOrEmpty(CurrentMapName) ? MapManager.Maps[CurrentMapName] : null);
        public Vector2 NewMapPosition;

        /// <summary>
        /// For World Actors, the Position is the top-left corner of the Actor's bounding box. Because the bounding
        /// box of the Acotr is not located at the same position as the top-left of the sprite, calculations need to be
        /// made to set the sprite's position value above the given position, and retrieving the Actor's Position value must
        /// likewise work backwards from the Sprite's Position to find where it is below.
        /// </summary>
        public override Vector2 Position
        {
            get
            {
                return new Vector2(_sprBody.Position.X, _sprBody.Position.Y + _sprBody.Height - Constants.TILE_SIZE);
            } //MAR this is fucked up
            set
            {
                _sprBody.Position = new Vector2(value.X, value.Y - _sprBody.Height + Constants.TILE_SIZE);
            }
        }

        protected Thread _pathingThread;

        public bool FollowingPath => _liTilePath.Count > 0;
        protected List<RHTile> _liTilePath;

        protected bool _bBumpedIntoSomething = false;
        public bool IgnoreCollisions { get; protected set; } = false;
        public bool SlowDontBlock { get; protected set; } = false;

        protected double _dCooldown = 0;

        public virtual Vector2 CollisionBoxPosition => Position;
        public virtual Rectangle CollisionBox => new Rectangle((int)Position.X, (int)Position.Y, Width, Constants.TILE_SIZE);

        public Point CollisionCenter => CollisionBox.Center;
        public virtual Rectangle HoverBox => new Rectangle((int)_sprBody.Position.X, (int)_sprBody.Position.Y, _sprBody.Width, _sprBody.Height);

        protected bool _bOnTheMap = true;
        public virtual bool OnTheMap => _bOnTheMap;

        protected bool _bHover => DataManager.GetBoolByIDKey(ID, "Hover", DataType.NPC);

        protected float _fBaseSpeed = 1f;
        public float BuffedSpeed => _fBaseSpeed * SpdMult;
        public float SpdMult = Constants.NPC_WALK_SPEED;

        #region Wander Properties
        RHTimer _movementTimer;
        protected bool _bFollow = false;
        protected bool _bIdleCooldown = false;

        public bool Wandering { get; protected set; } = false;
        protected NPCStateEnum _eCurrentState = NPCStateEnum.Idle;
        #endregion

        #endregion

        public WorldActor() : base()
        {
            _iBodyWidth = Constants.TILE_SIZE;
            _iBodyHeight = Constants.HUMAN_HEIGHT;

            _liTilePath = new List<RHTile>();
        }

        public WorldActor(int id, Dictionary<string, string> stringData) : base()
        {
            ID = id;
            ActorType = Util.ParseEnum<WorldActorTypeEnum>(stringData["Type"]);

            _liTilePath = new List<RHTile>();
            _movementTimer = new RHTimer(Constants.WANDER_COUNTDOWN);

            if (stringData.ContainsKey("Size"))
            {
                RHSize size = new RHSize();
                Util.AssignValue(ref size, "Size", stringData);

                _iBodyWidth = size.Width;
                _iBodyHeight = size.Height;
            }

            Wandering = stringData.ContainsKey("Wander");
        }

        public override void Draw(SpriteBatch spriteBatch, bool useLayerDepth = false)
        {
            if (_bOnTheMap)
            {
                base.Draw(spriteBatch, useLayerDepth);
                if (Constants.DRAW_COLLISION)
                {
                    spriteBatch.Draw(DataManager.GetTexture(DataManager.DIALOGUE_TEXTURE), CollisionBox, new Rectangle(160, 128, 2, 2), Color.White * 0.5f);
                }
            }
        }

        public override void Update(GameTime gTime)
        {
            base.Update(gTime);
            if (!GamePaused() && CurrentHP > 0)
            {
                if (HasVelocity()) { ApplyVelocity(); }
                else { HandleMove(); }

                if (_damageTimer != null)
                {
                    if (_damageTimer.TickDown(gTime))
                    {
                        _eCurrentState = NPCStateEnum.Idle;
                        _damageTimer = null;
                        _vVelocity = Vector2.Zero;
                    }
                }
            }
        }

        public virtual void ProcessRightButtonClick() { }

        public virtual void RollOver() { }

        public override string Name()
        {
            return DataManager.GetTextData(ID, "Name", DataType.NPC);
        }

        protected string SpriteName()
        {
            return DataManager.NPC_FOLDER + DataManager.GetStringByIDKey(ID, "Key", DataType.NPC);
        }

        /// <summary>
        /// Creates a new Animatedsprite object for the given texture string, and adds
        /// all of the given animations to the new AnimatedSprite
        /// </summary>
        /// <param name="listAnimations">A list of AnimationData to add to the sprite</param>
        /// <param name="textureName">The texture name for the AnimatedSprite</param>
        protected void LoadSpriteAnimations(ref AnimatedSprite sprite, List<AnimationData> listAnimations, string textureName)
        {
            sprite = new AnimatedSprite(textureName);

            foreach (AnimationData data in listAnimations)
            {
                if (data.Directional)
                {
                    AddDirectionalAnimations(ref sprite, data, _iBodyWidth, _iBodyHeight, data.PingPong, data.BackToIdle);
                }
                else
                {
                    sprite.AddAnimation(data.Animation, data.XLocation, data.YLocation, _iBodyWidth, _iBodyHeight, data.Frames, data.FrameSpeed, data.PingPong, data.Animation == AnimationEnum.KO);
                }
            }

            PlayAnimationVerb(VerbEnum.Idle);
        }

        public virtual bool HoverContains(Point mouse)
        {
            return HoverBox.Contains(mouse);
        }

        public virtual bool CollisionContains(Point mouse)
        {
            return CollisionBox.Contains(mouse);
        }

        public virtual bool CollisionIntersects(Rectangle rect)
        {
            return CollisionBox.Intersects(rect);
        }

        protected void FacePlayer(bool facePlayer)
        {
            //Determine the position based off of where the player is and then have the NPC face the player
            //Only do this if they are idle so as to not disturb other animations they may be performing.
            if (facePlayer && BodySprite.CurrentAnimation.StartsWith("Idle"))
            {
                Vector2 diff = Center - PlayerManager.PlayerActor.Center;
                if (Math.Abs(diff.X) > Math.Abs(diff.Y))
                {
                    if (diff.X > 0)  //The player is to the left
                    {
                        Facing = DirectionEnum.Left;
                    }
                    else
                    {
                        Facing = DirectionEnum.Right;
                    }
                }
                else
                {
                    if (diff.Y > 0)  //The player is above
                    {
                        Facing = DirectionEnum.Up;
                    }
                    else
                    {
                        Facing = DirectionEnum.Down;
                    }
                }

                PlayAnimationVerb(VerbEnum.Idle);
            }
        }

        public void DetermineFacing(RHTile tile)
        {
            if (tile != null)
            {
                DetermineFacing(new Vector2(tile.Position.X - Position.X, tile.Position.Y - Position.Y));
            }
        }

        public void DetermineFacing(Vector2 direction)
        {
            DirectionEnum newFacing = Util.GetDirectionFromNormalVector(direction);
            if (newFacing != DirectionEnum.None) {
                Facing = newFacing;
            }
        }
        public virtual void DetermineAnimationState(Vector2 direction)
        {
            Moving = direction.Length() != 0;

            switch (State)
            {
                case ActorStateEnum.Climb:
                    break;
                case ActorStateEnum.Grab:
                    if (Moving)
                    {
                        DirectionEnum moveDir = Util.GetDirectionFromNormalVector(direction);
                        if(moveDir == Facing) { PlayAnimationVerb(VerbEnum.Push); }
                        else if(moveDir == Util.GetOppositeDirection(Facing)) { PlayAnimationVerb(VerbEnum.Pull); }
                    }
                    else { PlayAnimationVerb(VerbEnum.GrabIdle); }
                    break;
                case ActorStateEnum.Swim:
                    break;
                case ActorStateEnum.Walk:
                    PlayAnimationVerb(Moving ? VerbEnum.Walk : VerbEnum.Idle);
                    break;
            }
        }

        public void SetState(ActorStateEnum e) { State = e; }
        public void SetWalkingDir(DirectionEnum d)
        {
            Facing = d;
            _sprBody.PlayAnimation(VerbEnum.Walk, Facing);
        }

        public virtual void SetMoveTo(Vector2 v, bool update = true)
        {
            MoveToLocation = v;
            if (update)
            {
                if (v != Vector2.Zero)
                {
                    DetermineFacing(v - Position);
                }
                DetermineAnimationState(v);
            }
        }

        /// <summary>
        /// Checks to see if the current animation is that of the verb and current facing
        /// </summary>
        /// <param name="verb">Verb to compare against</param>
        /// <returns>Returns true if the current animation is the verb and facing</returns>
        public bool IsDirectionalAnimation(VerbEnum verb)
        {
            return IsCurrentAnimation(verb, Facing);
        }

        /// <summary>
        /// Constructs the proper animation string for the current facing.
        /// </summary>
        public void PlayAnimationVerb(VerbEnum verb) { PlayAnimation(verb, Facing); }
        public bool IsCurrentAnimationVerb(VerbEnum verb) { return _sprBody.IsCurrentAnimation(verb, Facing); }
        public bool IsCurrentAnimationVerbFinished(VerbEnum verb) { return _sprBody.AnimationVerbFinished(verb, Facing); }

        /// <summary>
        /// Sets the active status of the WorldActor
        /// </summary>
        /// <param name="value">Whether the actor is active or not.</param>
        public void Activate(bool value)
        {
            _bOnTheMap = value;
        }
        protected void ChangeMovement(float speed)
        {
            SpdMult = speed;
            SetMoveTo(Vector2.Zero);
            _movementTimer.Reset(Constants.WANDER_COUNTDOWN);
        }
        public bool IsActorType(WorldActorTypeEnum act) { return ActorType == act; }
        public void ChangeState(NPCStateEnum state)
        {
            _eCurrentState = state;
            switch (state)
            {
                case NPCStateEnum.Alert:
                    PlayAnimation(VerbEnum.Alert, Facing);
                    break;
                case NPCStateEnum.Idle:
                    PlayAnimation(VerbEnum.Idle, Facing);
                    ChangeMovement(Constants.NPC_WALK_SPEED);
                    break;
                case NPCStateEnum.TrackPlayer:
                    ChangeMovement(Constants.NORMAL_SPEED);
                    break;
                case NPCStateEnum.Wander:
                    ChangeMovement(Constants.NPC_WALK_SPEED);
                    break;
            }
        }


        #region Pathing
        public Thread CalculatePathThreaded()
        {
            _pathingThread = new Thread(CalculatePath);
            _pathingThread.Start();

            return _pathingThread;
        }

        protected virtual void CalculatePath()
        {
            if (PlayerManager.CurrentMap != CurrentMapName || _eCurrentState != NPCStateEnum.TrackPlayer)
            {
                TravelManager.FinishThreading(ref _pathingThread);
                return;
            }

            Vector2 startPosition = Position;
            Vector2 target = _eCurrentState == NPCStateEnum.TrackPlayer ? PlayerManager.PlayerActor.CollisionCenter.ToVector2() : _vLeashPoint; ;
            //RHTile lastTile = _liTilePath.Count > 0 ? _liTilePath[0] : null;
            _liTilePath = TravelManager.FindPathToLocation(ref startPosition, target, null, false, false);

            if (_liTilePath?.Count > 0 && _liTilePath?.Count < 30)
            {
                SetMoveTo(_liTilePath[0].Position);
            }
            else
            {
                _liTilePath = new List<RHTile>();
            }

            TravelManager.FinishThreading(ref _pathingThread);
        }

        /// <summary>
        /// Attempts to move the Actor to the indicated location
        /// </summary>
        /// <param name="target">The target location on the world map to move to</param>
        protected virtual void HandleMove()
        {
            if (MoveToLocation != Vector2.Zero)
            {
                //Determines the distance that needs to be traveled from the current position to the target
                Vector2 direction = Vector2.Zero;

                //Determines how much of the needed position we're capable of  in one movement
                Util.GetMoveSpeed(Position, MoveToLocation, BuffedSpeed, ref direction);

                //If we're following a path and there's more than one tile left, we don't want to cut
                //short on individual steps, so recalculate based on the next target
                float length = direction.Length();
                if (_liTilePath.Count > 1 && length < BuffedSpeed)
                {
                    if (DoorCheck())
                    {
                        return;
                    }
                }

                bool impeded = false;
                Vector2 initial = direction;
                if (CurrentMap.CheckForCollisions(this, Position, CollisionBox, ref direction, ref impeded) && direction != Vector2.Zero)
                {
                    DetermineAnimationState(direction);
                    Position += direction * (impeded ? Constants.IMPEDED_SPEED : 1f);
                }

                if(initial != direction)
                {
                    _bBumpedIntoSomething = true;
                }

                //If, after movement, we've reached the given location, zero it.
                if (MoveToLocation == Position && !CutsceneManager.Playing)
                {
                    SetMoveTo(Vector2.Zero);
                    if (_liTilePath.Count > 0)
                    {
                        _liTilePath.RemoveAt(0);
                        if (_liTilePath.Count > 0)
                        {
                            SetMoveTo(_liTilePath[0].Position);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// This method checks to see whether the next RHTile is a door and handles it.
        /// </summary>
        /// <returns>True if the next RHTIle is a door</returns>sssssssss
        protected bool DoorCheck()
        {
            bool rv = false;
            TravelPoint potentialTravelPoint = _liTilePath[0].GetTravelPoint();
            if (potentialTravelPoint != null && potentialTravelPoint.IsDoor)
            {
                SoundManager.PlayEffectAtLoc("close_door_1", this.CurrentMapName, potentialTravelPoint.Center);
                MapManager.ChangeMaps(this, this.CurrentMapName, potentialTravelPoint);
                rv = true;
            }

            return rv;
        }

        /// <summary>
        /// Because the Actor pathfinds based off of objective locations of the exit object
        /// it is possible, and probable, that they will enter the object, triggering a map
        /// change, a tile or two earlier than anticipated. In which case, we need to wipe
        /// any tiles that are on that map from the remaining path to follow.
        /// </summary>
        public void ClearTileForMapChange()
        {
            while (_liTilePath.Count > 0 && _liTilePath[0].MapName == CurrentMapName)
            {
                _liTilePath.RemoveAt(0);
                SetMoveTo(_liTilePath[0].Position);
            }
        }

        /// <summary>
        /// Sets the WorldActors TilePath to follow.
        /// </summary>
        public void SetPath(List<RHTile> list)
        {
            _liTilePath = list;
        }

        /// <summary>
        /// Wipes out the path the CombatActor is currently on.
        /// </summary>
        public void ClearPath()
        {
            _liTilePath.Clear();
        }
        #endregion

        #region Wander Logic
        public void SetFollow(bool value)
        {
            _bFollow = value;
        }

        protected void ProcessStateEnum(GameTime gTime, bool getInRange)
        {
            if (Wandering && _damageTimer == null)
            {
                switch (_eCurrentState)
                {
                    case NPCStateEnum.Alert:
                        Alert();
                        break;
                    case NPCStateEnum.Idle:
                        Idle(gTime);
                        break;
                    case NPCStateEnum.Wander:
                        Wander(gTime);
                        break;
                    case NPCStateEnum.TrackPlayer:
                        TrackPlayer(getInRange);
                        break;
                    case NPCStateEnum.Leashing:
                        TravelManager.RequestPathing(this);
                        break;
                }
            }
        }

        protected void Alert()
        {
            if (_sprBody.PlayedOnce)
            {
                ChangeState(NPCStateEnum.TrackPlayer);
            }
        }

        protected void Idle(GameTime gTime)
        {
            if (_movementTimer.TickDown(gTime))
            {
                if (RHRandom.Instance().RollPercent(50))
                {
                    _movementTimer.Reset(Constants.WANDER_COUNTDOWN + RHRandom.Instance().Next(10));
                    _bIdleCooldown = true;
                    ChangeState(NPCStateEnum.Wander);
                }
            }
        }

        protected void TrackPlayer(bool getInRange)
        {
            if (_liTilePath.Find(x => x.Contains(PlayerManager.PlayerActor)) == null)
            {
                TravelManager.RequestPathing(this);
            }
        }

        protected void Wander(GameTime gTime)
        {
            if (_movementTimer.TickDown(gTime) && MoveToLocation == Vector2.Zero)
            {
                Vector2 moveTo = Vector2.Zero;
                while (moveTo == Vector2.Zero)
                {
                    _movementTimer.Reset(Constants.WANDER_COUNTDOWN + RHRandom.Instance().Next(4) * 0.25);

                    if (!_bIdleCooldown && RHRandom.Instance().RollPercent(20))
                    {
                        ChangeState(NPCStateEnum.Idle);
                        return;
                    }

                    _bIdleCooldown = false;

                    bool moveX = RHRandom.Instance().Next(0, 1) == 0;

                    if (moveX)
                    {
                        moveTo = new Vector2(RHRandom.Instance().Next(8, 32), 0);
                        if (RHRandom.Instance().Next(1, 2) == 1)
                        {
                            moveTo.X *= -1;
                        }
                    }
                    else
                    {
                        moveTo = new Vector2(0, RHRandom.Instance().Next(8, 32));
                        if (RHRandom.Instance().Next(1, 2) == 1)
                        {
                            moveTo.Y *= -1;
                        }
                    }

                    moveTo = CurrentMap.GetFarthestUnblockedPath(Position + moveTo, this);
                }

                SetMoveTo(moveTo);
            }

            if (MoveToLocation != Vector2.Zero)
            {
                HandleMove();
            }
        }
        #endregion

        #region Combat Logic
        public void RefillHealth()
        {
            CurrentHP = MaxHP;
        }

        /// <summary>
        /// Reduces health by the given value. Cannot deal more damage than health exists.
        /// </summary>
        public virtual bool DealDamage(int value, Rectangle hitbox)
        {
            bool rv = false;

            if (!Invulnerable && _damageTimer == null)
            {
                _damageTimer = new RHTimer(Constants.INVULN_PERIOD);
                DecreaseHealth(value);

                if (CurrentHP == 0) { PlayAnimation(AnimationEnum.KO); }
                else
                {
                    rv = true;
                    _flickerTimer = new RHTimer(Constants.FLICKER_PERIOD);

                    AssignVelocity(hitbox);
                }
            }

            return rv;
        }

        public void DecreaseHealth(int value)
        {
            CurrentHP -= (CurrentHP - value >= 0) ? value : CurrentHP;
        }

        /// <summary>
        /// As long as the target is not KnockedOut, recover the given amount of HP up to max
        /// </summary>
        public float IncreaseHealth(float x)
        {
            float amountHealed = x;
            if (CurrentHP + x <= MaxHP)
            {
                CurrentHP += x;
            }
            else
            {
                amountHealed = MaxHP - CurrentHP;
                CurrentHP = MaxHP;
            }

            return amountHealed;
        }

        /// <summary>
        /// Adds the StatusEffect objectto the character's list of status effects.
        /// </summary>
        /// <param name="effect">Effect toadd</param>
        public void ApplyStatusEffect(StatusEffect effect)
        {
            //if (effect.EffectType == StatusTypeEnum.DoT || effect.EffectType == StatusTypeEnum.HoT)
            //{
            //    StatusEffect find = _liStatusEffects.Find(status => status.ID == effect.ID);
            //    if (find != null)
            //    {
            //        _liStatusEffects.Remove(find);
            //    }
            //    _liStatusEffects.Add(effect);
            //}
            //else
            //{
            //    foreach (KeyValuePair<AttributeEnum, string> kvp in effect.AffectedAttributes)
            //    {
            //        AssignAttributeEffect(kvp.Key, kvp.Value, effect.Duration, effect.EffectType);
            //    }
            //    _liStatusEffects.Add(effect);
            //}
        }

        public bool HasVelocity()
        {
            return _vVelocity != Vector2.Zero;
        }
        public void ApplyVelocity()
        {
            Vector2 startPos = Position;
            Vector2 dir = _vVelocity;

            bool impeded = false;
            if (CurrentMap.CheckForCollisions(this, Position, CollisionBox, ref dir, ref impeded))
            {
                Position += dir;
            }
            _vVelocity *= 0.98f;
        }

        private void AssignVelocity(Rectangle hitbox)
        {
            _vVelocity = hitbox.Center.ToVector2() - CollisionBox.Center.ToVector2();
            _vVelocity.Normalize();
            switch (GetWeight())
            {
                case WeightEnum.Light:
                    _vVelocity *= -4;
                    _fDecay = 0.98f;
                    goto default;
                case WeightEnum.Medium:
                    _vVelocity *= -2;
                    _fDecay = 0.97f;
                    goto default;
                case WeightEnum.Heavy:
                    _vVelocity *= -1;
                    _fDecay = 0.90f;
                    goto default;
                case WeightEnum.Immovable:
                    _vVelocity *= 0;
                    break;
                default:
                    SetMoveTo(Vector2.Zero);
                    ClearPath();
                    break;
            }
        }

        protected virtual WeightEnum GetWeight()
        {
            return WeightEnum.Medium;
        }

        #endregion
    }
}
