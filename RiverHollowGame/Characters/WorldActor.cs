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


        public bool Moving { get; set; } = false;
        public DirectionEnum Facing = DirectionEnum.Down;
        public ActorStateEnum State { get; protected set; } = ActorStateEnum.Walk;

        public WorldActorTypeEnum ActorType { get; protected set; }

        /// <summary>
        /// For World Actors, the Position is the top-left corner of the Actor's bounding box. Because the bounding
        /// box of the Acotr is not located at the same position as the top-left of the sprite, calculations need to be
        /// made to set the sprite's position value above the given position, and retrieving the Actor's Position value must
        /// likewise work backwards from the Sprite's Position to find where it is below.
        /// </summary>
        public override Point Position
        {
            get
            {
                return new Point(_sprBody.Position.X, _sprBody.Position.Y + _sprBody.Height - Constants.TILE_SIZE);
            } //MAR this is fucked up
            set
            {
                _sprBody.Position = new Point(value.X, value.Y - _sprBody.Height + Constants.TILE_SIZE);
            }
        }
        public Point MoveToLocation { get; private set; }
        public Point _pLeashPoint { get; protected set; }
        protected Vector2 _vKnockbackVelocity;
        protected float _fDecay;

        protected VectorBuffer _vbMovement;
        public Vector2 AccumulatedMovement => _vbMovement.AccumulatedMovement;

        public string CurrentMapName;
        public RHMap CurrentMap => (!string.IsNullOrEmpty(CurrentMapName) ? MapManager.Maps[CurrentMapName] : null);
        public bool OnScreen() { return CurrentMap == MapManager.CurrentMap; }

        public Point NewMapPosition;

        protected Thread _pathingThread;

        public bool FollowingPath => _liTilePath.Count > 0;
        protected List<RHTile> _liTilePath;

        protected bool _bBumpedIntoSomething = false;
        public bool IgnoreCollisions { get; protected set; } = false;
        public bool SlowDontBlock { get; protected set; } = false;

        protected double _dCooldown = 0;

        #region CollisionBox
        public virtual Rectangle CollisionBox => new Rectangle((int)_sprBody.Position.X + CollisionOffset.X, (int)_sprBody.Position.Y + CollisionOffset.Y, Width, Constants.TILE_SIZE);
        public Point CollisionOffset => new Point(0, Height - Constants.TILE_SIZE);
        public Point CollisionBoxLocation => CollisionBox.Location;
        public Point CollisionCenter => CollisionBox.Center;
        #endregion

        public virtual Rectangle HoverBox => new Rectangle((int)_sprBody.Position.X, (int)_sprBody.Position.Y, _sprBody.Width, _sprBody.Height);

        protected bool _bOnTheMap = true;
        public virtual bool OnTheMap => _bOnTheMap;

        protected bool _bHover => DataManager.GetBoolByIDKey(ID, "Hover", DataType.NPC);

        protected float _fBaseSpeed = 1f;
        public float BuffedSpeed => _fBaseSpeed * SpdMult;
        public float SpdMult = Constants.NPC_WALK_SPEED;

        #region Wander Properties
        protected RHTimer _movementTimer;
        protected bool _bFollow = false;
        protected bool _bIdleCooldown = false;
        protected int _iMinWander = 8;
        protected int _iMaxWander = 32;
        protected float _fBaseWanderTimer = Constants.WANDER_COUNTDOWN;

        public bool Wandering { get; protected set; } = false;
        protected NPCStateEnum _eCurrentState = NPCStateEnum.Idle;
        #endregion

        #endregion

        public WorldActor() : base()
        {
            _iBodyWidth = Constants.TILE_SIZE;
            _iBodyHeight = Constants.HUMAN_HEIGHT;

            _liTilePath = new List<RHTile>();
            _vbMovement = new VectorBuffer();
        }

        public WorldActor(int id, Dictionary<string, string> stringData) : base()
        {
            ID = id;
            ActorType = Util.ParseEnum<WorldActorTypeEnum>(stringData["Type"]);

            _vbMovement = new VectorBuffer();
            _liTilePath = new List<RHTile>();
            _movementTimer = new RHTimer(_fBaseWanderTimer);

            if (stringData.ContainsKey("Size"))
            {
                Point size = new Point();
                Util.AssignValue(ref size, "Size", stringData);

                _iBodyWidth = size.X;
                _iBodyHeight = size.Y;
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
                    spriteBatch.Draw(DataManager.GetTexture(DataManager.DIALOGUE_TEXTURE), CollisionBox, new Rectangle(160, 128, 2, 2), Color.White * 0.5f, 0f, Vector2.Zero, SpriteEffects.None, GetSprites()[0].LayerDepth - 1);
                }
            }
        }

        public override void Update(GameTime gTime)
        {
            base.Update(gTime);
            if (!GamePaused())
            {
                HandleMove();
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
                Point diff = Center - PlayerManager.PlayerActor.Center;
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
                DetermineFacing(new Point(tile.Position.X - Position.X, tile.Position.Y - Position.Y));
            }
        }

        public void DetermineFacing(Point direction)
        {
            DirectionEnum newFacing = Util.GetDirection(direction);
            if (newFacing != DirectionEnum.None) {
                Facing = newFacing;
            }
        }
        public void DetermineAnimationState(Point direction)
        {
            DetermineAnimationState(direction.ToVector2());
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
                        DirectionEnum moveDir = Util.GetDirection(direction);
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

        public virtual void SetMoveTo(Point v, bool update = true)
        {
            MoveToLocation = v;
            _vbMovement.Clear();
            if (update)
            {
                if (v != Point.Zero)
                {
                    DetermineFacing(v - Position);
                }
                DetermineAnimationState(v);
            }
        }

        public bool HasMovement()
        {
            return MoveToLocation != Point.Zero;
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
        protected void ChangeMovement()
        {
            SetMoveTo(Point.Zero);
            _movementTimer.Reset(Constants.WANDER_COUNTDOWN);
        }

        public bool CollidesWithPlayer()
        {
            switch (ActorType)
            {
                case WorldActorTypeEnum.Mob:
                case WorldActorTypeEnum.Mount:
                case WorldActorTypeEnum.Critter:
                    return false;
            }

            return true;
        }
        public bool IsActorType(WorldActorTypeEnum act) { return ActorType == act; }
        public virtual void ChangeState(NPCStateEnum state)
        {
            _eCurrentState = state;
            switch (state)
            {
                case NPCStateEnum.Alert:
                    PlayAnimation(VerbEnum.Alert, Facing);
                    break;
                case NPCStateEnum.Idle:
                    PlayAnimation(VerbEnum.Idle, Facing);
                    ChangeMovement();
                    break;
                case NPCStateEnum.TrackPlayer:
                    ChangeMovement();
                    break;
                case NPCStateEnum.Wander:
                    ChangeMovement();
                    break;
            }
        }

        #region MoveBuffer Methods
        public void MoveActor(Point p)
        {
            Position += _vbMovement.AddMovement(p.ToVector2());
        }
        public void MoveActor(Vector2 v)
        {
            Position += _vbMovement.AddMovement(v);
        }

        public Point ProjectedMovement(Vector2 dir)
        {
            return _vbMovement.ProjectMovement(dir); ;
        }

        public void ClearBuffer()
        {
            _vbMovement.Clear();
        }
        #endregion

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

            Point startPosition = Position;
            Point target = _eCurrentState == NPCStateEnum.TrackPlayer ? PlayerManager.PlayerActor.CollisionCenter : _pLeashPoint; ;
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
            if (HasMovement())
            {
                //Determines how much of the needed position we're capable of in one movement
                Vector2  direction = Util.GetMoveSpeed(Position, MoveToLocation, BuffedSpeed);

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
                if (CurrentMap.CheckForCollisions(this, ref direction, ref impeded) && direction != Vector2.Zero)
                {
                    DetermineAnimationState(direction);
                    MoveActor(direction * (impeded ? Constants.IMPEDED_SPEED : 1f));
                }
                else { _bBumpedIntoSomething = true; }

                if(initial != direction)
                {
                    _bBumpedIntoSomething = true;
                }

                //If, after movement, we've reached the given location, zero it.
                if (MoveToLocation == Position && !CutsceneManager.Playing)
                {
                    SetMoveTo(Point.Zero);
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
                MapManager.ChangeMaps(this, CurrentMapName, potentialTravelPoint);
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

        protected virtual void ProcessStateEnum(GameTime gTime, bool getInRange)
        {
            if (Wandering)
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

        protected void Wander(GameTime gTime, int rollChance = 20)
        {
            if (_movementTimer.TickDown(gTime) && !HasMovement())
            {
                Point moveTo = Point.Zero;
                while (moveTo == Point.Zero)
                {
                    _movementTimer.Reset(_fBaseWanderTimer + RHRandom.Instance().Next(4) * 0.25);

                    if (!_bIdleCooldown && RHRandom.Instance().RollPercent(rollChance))
                    {
                        ChangeState(NPCStateEnum.Idle);
                        return;
                    }

                    _bIdleCooldown = false;

                    RHTile myTile = CurrentMap.GetTileByPixelPosition(CollisionCenter);
                    var tileDir = Util.GetRandomItem(myTile.GetWalkableNeighbours());

                    moveTo = Position + Util.MultiplyPoint((tileDir.Position - myTile.Position), RHRandom.Instance().Next(_iMinWander, _iMaxWander));
                }

                SetMoveTo(moveTo);
            }

            if (HasMovement())
            {
                HandleMove();
            }
        }
        #endregion
    }
}
