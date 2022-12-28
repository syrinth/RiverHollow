using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.Game_Managers;
using RiverHollow.Misc;
using RiverHollow.SpriteAnimations;
using RiverHollow.Map_Handling;
using RiverHollow.Utilities;
using RiverHollow.WorldObjects;
using System;
using System.Collections.Generic;
using static RiverHollow.Game_Managers.GameManager;
using static RiverHollow.Utilities.Enums;
using System.Threading;
using RiverHollow.GUIComponents.GUIObjects;

namespace RiverHollow.Characters
{
    public abstract class WorldActor : Actor
    {
        #region Properties

        public int ID { get; } = -1;

        public bool Moving { get; set; } = false;
        public DirectionEnum Facing = DirectionEnum.Down;
        public ActorStateEnum State { get; protected set; } = ActorStateEnum.Walk;

        protected WorldActorTypeEnum _eActorType = WorldActorTypeEnum.Actor;
        public WorldActorTypeEnum ActorType => _eActorType;
        public Vector2 MoveToLocation { get; private set; }

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
        protected bool _bIgnoreCollisions;

        protected double _dCooldown = 0;

        public virtual Vector2 CollisionBoxPosition => Position;
        public virtual Rectangle CollisionBox => new Rectangle((int)Position.X, (int)Position.Y, Width, Constants.TILE_SIZE);
        public Point CollisionCenter => CollisionBox.Center;
        public virtual Rectangle HoverBox => new Rectangle((int)Position.X, (int)Position.Y - Constants.TILE_SIZE, Width, Height);

        protected bool _bOnTheMap = true;
        public virtual bool OnTheMap => _bOnTheMap;

        protected bool _bHover;

        protected float _fBaseSpeed = 1f;
        public float BuffedSpeed => _fBaseSpeed * SpdMult;
        public float SpdMult = Constants.NPC_WALK_SPEED;

        #region Wander Properties
        RHTimer _movementTimer;
        protected bool _bFollow = false;
        protected bool _bIdleCooldown = false;

        protected bool _bCanWander = false;
        protected NPCStateEnum _eCurrentState = NPCStateEnum.Idle;
        #endregion

        #endregion

        public WorldActor() : base()
        {
            _iBodyWidth = Constants.TILE_SIZE;
            _iBodyHeight = Constants.HUMAN_HEIGHT;

            _liTilePath = new List<RHTile>();
        }

        public WorldActor(int id) : base()
        {
            ID = id;

            _iBodyWidth = Constants.TILE_SIZE;
            _iBodyHeight = Constants.HUMAN_HEIGHT;

            _liTilePath = new List<RHTile>();
            _movementTimer = new RHTimer(Constants.WANDER_COUNTDOWN);
        }

        public override void Draw(SpriteBatch spriteBatch, bool useLayerDepth = false)
        {
            if (_bOnTheMap)
            {
                base.Draw(spriteBatch, useLayerDepth);
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

        public override string Name()
        {
            return DataManager.GetTextData("NPC", ID, "Name");
        }

        protected string SpriteName()
        {
            return DataManager.NPC_FOLDER + DataManager.GetStringByIDKey(ID, "Key", DataType.Character);
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
                    sprite.AddAnimation(data.Animation, data.XLocation, data.YLocation, _iBodyWidth, _iBodyHeight, data.Frames, data.FrameSpeed, data.PingPong);
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
            bool initiallyMoving = Moving;
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
                DetermineFacing(v - Position);
                DetermineAnimationState(v - Position);
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
        /// During Combat, the Idle animation is the Walk animation.
        /// </summary>
        public void PlayAnimationVerb(VerbEnum verb) { PlayAnimation(verb, Facing); }
        public bool IsCurrentAnimationVerb(VerbEnum verb) { return _sprBody.IsCurrentAnimation(verb, Facing); }
        public bool IsCurrentAnimationVerbFinished(VerbEnum verb) { return _sprBody.AnimationVerbFinished(verb, Facing); }

        /// <summary>
        /// Check the direction in which we wish to move the Actor for any possible collisions.
        /// 
        /// If none are found, move the Actor andthen recalculate the facing based on the moved direction.
        /// </summary>
        /// <param name="direction">The direction to move the Actor in pixels</param>
        /// <param name="ignoreCollisions">Whether or not we are to ignore collisions</param>
        /// <returns></returns>
        protected bool CheckMapForCollisionsAndMove(Vector2 direction)
        {
            bool rv = false;

            //Create the X and Y rectangles to test for collisions
            Rectangle testRectX = Util.FloatRectangle(Position.X + direction.X, Position.Y, CollisionBox.Width, CollisionBox.Height);
            Rectangle testRectY = Util.FloatRectangle(Position.X, Position.Y + direction.Y, CollisionBox.Width, CollisionBox.Height);

            //Check for collisions against the map and, if none are detected, move. Do not move if the direction Vector2 is Zero
            if (CurrentMap.CheckForCollisions(this, testRectX, testRectY, ref direction) && direction != Vector2.Zero)
            {
                DetermineAnimationState(direction);
                Position += new Vector2(direction.X, direction.Y);
                rv = true;
            }

            return rv;
        }

        public Thread CalculatePathThreaded()
        {
            _pathingThread = new Thread(CalculatePath);
            _pathingThread.Start();

            return _pathingThread;
        }

        protected virtual void CalculatePath() { }

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
                float deltaX = Math.Abs(MoveToLocation.X - this.Position.X);
                float deltaY = Math.Abs(MoveToLocation.Y - this.Position.Y);

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

                //Attempt to move
                if (!CheckMapForCollisionsAndMove(direction))
                {
                    _bBumpedIntoSomething = true;
                    if( this == PlayerManager.PlayerActor)
                    {
                        PlayerManager.ClearDamagedMovement();
                    }
                }

                //If, after movement, we've reached the given location, zero it.
                if (MoveToLocation == Position && !CutsceneManager.Playing)
                {
                    SetMoveTo(Vector2.Zero, false);
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
        /// Sets the active status of the WorldActor
        /// </summary>
        /// <param name="value">Whether the actor is active or not.</param>
        public void Activate(bool value)
        {
            _bOnTheMap = value;
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

        protected void ChangeMovement(float speed)
        {
            SpdMult = speed;
            SetMoveTo(Vector2.Zero);
            PlayAnimation(VerbEnum.Walk, Facing);
            _movementTimer.Reset(Constants.WANDER_COUNTDOWN);
        }

        #region Wander Logic
        public void SetFollow(bool value)
        {
            _bFollow = value;
        }

        protected void ProcessStateEnum(GameTime gTime, bool getInRange)
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

        protected void Alert()
        {
            if (_sprBody.PlayedOnce)
            {
                ChangeState(NPCStateEnum.TrackPlayer);
            }
        }

        protected void Idle(GameTime gTime)
        {
            _movementTimer.TickDown(gTime);
            if (_movementTimer.Finished())
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
            _movementTimer.TickDown(gTime);
            if (_movementTimer.Finished() && MoveToLocation == Vector2.Zero)
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

        public bool IsActorType(WorldActorTypeEnum act) { return _eActorType == act; }
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
                    break;
                case NPCStateEnum.TrackPlayer:
                    ChangeMovement(Constants.NORMAL_SPEED);
                    break;
                case NPCStateEnum.Wander:
                    ChangeMovement(Constants.NPC_WALK_SPEED);
                    break;
            }
        }
        #endregion
    }
}
