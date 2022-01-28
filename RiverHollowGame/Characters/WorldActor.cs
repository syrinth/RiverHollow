using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.Game_Managers;
using RiverHollow.Misc;
using RiverHollow.SpriteAnimations;
using RiverHollow.Tile_Engine;
using RiverHollow.Utilities;
using RiverHollow.WorldObjects;
using System;
using System.Collections.Generic;
using static RiverHollow.Game_Managers.GameManager;

namespace RiverHollow.Characters
{
    public abstract class WorldActor : Actor
    {
        #region Properties

        public int ID { get; } = -1;

        protected ActorMovementStateEnum _eMovementState = ActorMovementStateEnum.Idle;

        protected WorldActorTypeEnum _eActorType = WorldActorTypeEnum.Actor;
        public WorldActorTypeEnum ActorType => _eActorType;

        protected Vector2 _vMoveTo;
        public Vector2 MoveToLocation => _vMoveTo;
        public string CurrentMapName;
        public RHMap CurrentMap => (!string.IsNullOrEmpty(CurrentMapName) ? MapManager.Maps[CurrentMapName] : null);
        public Vector2 NewMapPosition;
        public Point CharCenter => GetRectangle().Center;

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
                return new Vector2(_sprBody.Position.X, _sprBody.Position.Y + _sprBody.Height - (TILE_SIZE * _iSize));
            } //MAR this is fucked up
            set
            {
                _sprBody.Position = new Vector2(value.X, value.Y - _sprBody.Height + (TILE_SIZE * _iSize));
            }
        }

        public bool FollowingPath => _liTilePath.Count > 0;
        protected List<RHTile> _liTilePath;

        protected bool _bBumpedIntoSomething = false;
        protected double _dEtherealCD;
        protected bool _bIgnoreCollisions;

        protected double _dCooldown = 0;

        public virtual Rectangle CollisionBox => new Rectangle((int)Position.X, (int)Position.Y, Width, TILE_SIZE);
        public virtual Rectangle HoverBox => new Rectangle((int)Position.X, (int)Position.Y - TILE_SIZE, Width, Height);

        protected bool _bOnTheMap = true;
        public virtual bool OnTheMap => _bOnTheMap;

        protected bool _bHover;

        float _fBaseSpeed = 2;
        public float BuffedSpeed => _fBaseSpeed * SpdMult;
        public float SpdMult = NPC_WALK_SPEED;

        protected int _iSize = 1;
        public int Size => _iSize;

        #region Wander Properties
        protected const double MOVE_COUNTDOWN = 2.5;
        protected bool _bFollow = false;
        protected bool _bIdleCooldown = false;

        protected double _dWanderCountdown = 0;
        protected bool _bCanWander = false;
        protected NPCStateEnum _eCurrentState = NPCStateEnum.Idle;
        #endregion

        #endregion

        public WorldActor() : base()
        {
            _iBodyWidth = TILE_SIZE;
            _iBodyHeight = HUMAN_HEIGHT;

            _liTilePath = new List<RHTile>();
        }

        public WorldActor(int id) : base()
        {
            ID = id;

            _iBodyWidth = TILE_SIZE;
            _iBodyHeight = HUMAN_HEIGHT;

            _liTilePath = new List<RHTile>();
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
            if (IsRunning())
            {
                if (_dEtherealCD != 0)
                {
                    _dEtherealCD -= gTime.ElapsedGameTime.TotalSeconds;
                    if (_dEtherealCD <= 0)
                    {
                        if (!_bIgnoreCollisions)
                        {
                            _dEtherealCD = 5;
                            _bIgnoreCollisions = true;
                        }
                        else
                        {
                            _dEtherealCD = 0;
                            _bIgnoreCollisions = false;
                        }
                    }
                }

                HandleMove(_vMoveTo);
            }
        }

        public virtual void ProcessRightButtonClick() { }

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

        public void SetWalkingDir(DirectionEnum d)
        {
            Facing = d;
            _sprBody.PlayAnimation(VerbEnum.Walk, Facing);
        }

        public void DetermineFacing(RHTile tile)
        {
            DetermineFacing(new Vector2(tile.Position.X - Position.X, tile.Position.Y - Position.Y));
        }
        public virtual void DetermineFacing(Vector2 direction)
        {
            bool walk = false;

            DirectionEnum initialFacing = Facing;
            ActorMovementStateEnum initialState = _eMovementState;
            if (direction.Length() != 0)
            {
                SetMovementState(ActorMovementStateEnum.Walking);
                walk = true;
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

                List<RHTile> cornerTiles = new List<RHTile>();
                cornerTiles.Add(CurrentMap.GetTileByGridCoords(Util.GetGridCoords(new Vector2(CollisionBox.Left, CollisionBox.Top)).ToPoint()));
                cornerTiles.Add(CurrentMap.GetTileByGridCoords(Util.GetGridCoords(new Vector2(CollisionBox.Right, CollisionBox.Top)).ToPoint()));
                cornerTiles.Add(CurrentMap.GetTileByGridCoords(Util.GetGridCoords(new Vector2(CollisionBox.Left, CollisionBox.Bottom)).ToPoint()));
                cornerTiles.Add(CurrentMap.GetTileByGridCoords(Util.GetGridCoords(new Vector2(CollisionBox.Right, CollisionBox.Bottom)).ToPoint()));
                foreach (RHTile tile in cornerTiles)
                {
                    if (tile != null && tile.WorldObject != null && tile.WorldObject.CompareType(ObjectTypeEnum.Plant))
                    {
                        Plant f = (Plant)tile.WorldObject;
                        f.Shake();
                    }
                }
            }
            else { SetMovementState(ActorMovementStateEnum.Idle); }

            if (initialState != _eMovementState || initialFacing != Facing)
            {
                PlayAnimationVerb(walk ? VerbEnum.Walk : VerbEnum.Idle);
            }
        }

        public void SetMovementState(ActorMovementStateEnum e)
        {
            _eMovementState = e;
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
        protected bool CheckMapForCollisionsAndMove(Vector2 direction, bool ignoreCollisions = false)
        {
            bool rv = false;
            //Create the X and Y rectangles to test for collisions
            Rectangle testRectX = Util.FloatRectangle(Position.X + direction.X, Position.Y, CollisionBox.Width, CollisionBox.Height);
            Rectangle testRectY = Util.FloatRectangle(Position.X, Position.Y + direction.Y, CollisionBox.Width, CollisionBox.Height);

            //Check for collisions against the map and, if none are detected, move. Do not move if the direction Vector2 is Zero
            if (CurrentMap.CheckForCollisions(this, testRectX, testRectY, ref direction, ignoreCollisions) && direction != Vector2.Zero)
            {
                DetermineFacing(direction);
                Position += new Vector2(direction.X, direction.Y);
                rv = true;
            }

            return rv;
        }

        /// <summary>
        /// Attempts to move the Actor to the indicated location
        /// </summary>
        /// <param name="target">The target location on the world map to move to</param>
        protected void HandleMove(Vector2 target)
        {
            if (target != Vector2.Zero)
            {
                //Determines the distance that needs to be traveled from the current position to the target
                Vector2 direction = Vector2.Zero;
                float deltaX = Math.Abs(target.X - this.Position.X);
                float deltaY = Math.Abs(target.Y - this.Position.Y);

                //Determines how much of the needed position we're capable of  in one movement
                Util.GetMoveSpeed(Position, target, BuffedSpeed, ref direction);

                //If we're following a path and there's more than one tile left, we don't want to cut
                //short on individual steps, so recalculate based on the next target
                float length = direction.Length();
                if (_liTilePath.Count > 1 && length <= BuffedSpeed)
                {
                    _liTilePath.RemoveAt(0);

                    if (DoorCheck())
                    {
                        return;
                    }

                    //Recalculate for the next target
                    _vMoveTo = _liTilePath[0].Position;
                    Util.GetMoveSpeed(Position, _vMoveTo, BuffedSpeed, ref direction);
                }

                //Attempt to move
                if (!CheckMapForCollisionsAndMove(direction, _bIgnoreCollisions))
                {
                    _bBumpedIntoSomething = true;

                    //If we can't move, set a timer to go Ethereal
                    if (_dEtherealCD == 0) { _dEtherealCD = 5; }
                }

                //If, after movement, we've reached the given location, zero it.
                if (_vMoveTo == Position && !CutsceneManager.Playing)
                {
                    _vMoveTo = Vector2.Zero;
                }
            }
        }

        /// <summary>
        /// This method checks to see whether the next RHTile is a door and handles it.
        /// </summary>
        /// <returns>True if the next RHTIle is a door</returns>
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
            }
        }

        public void SetMoveObj(Vector2 vec)
        {
            _vMoveTo = vec;
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
            PlayAnimation(VerbEnum.Walk, Facing);
            SetMoveObj(Vector2.Zero);
            _dWanderCountdown = 0;
        }

        #region Wander Logic
        public void SetFollow(bool value)
        {
            _bFollow = value;
        }

        protected void ProcessStateEnum(GameTime gTime)
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
                    TrackPlayer();
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
            if (_dWanderCountdown < 5 + RHRandom.Instance().Next(10)) { _dWanderCountdown += gTime.ElapsedGameTime.TotalSeconds; }
            else
            {
                if (RHRandom.Instance().RollPercent(50))
                {
                    _dWanderCountdown = 0;
                    _bIdleCooldown = true;
                    ChangeState(NPCStateEnum.Wander);
                }
            }
        }

        protected virtual void TrackPlayer()
        {
            Vector2 delta = Position - PlayerManager.PlayerActor.Position;
            HandleMove(Position - delta);

            if (PlayerManager.PlayerInRange(CollisionBox.Center, TILE_SIZE))
            {
                ChangeState(NPCStateEnum.Wander);
            }
        }

        protected void Wander(GameTime gTime)
        {
            if (_dWanderCountdown < MOVE_COUNTDOWN + (RHRandom.Instance().Next(4) * 0.25)) { _dWanderCountdown += gTime.ElapsedGameTime.TotalSeconds; }
            else if (MoveToLocation == Vector2.Zero)
            {
                Vector2 moveTo = Vector2.Zero;
                while (moveTo == Vector2.Zero)
                {
                    _dWanderCountdown = 0;

                    if (!_bIdleCooldown && RHRandom.Instance().RollPercent(20))
                    {
                        ChangeState(NPCStateEnum.Idle);
                        return;
                    }

                    _bIdleCooldown = false;

                    bool moveX = RHRandom.Instance().Next(0, 1) == 0;

                    if (moveX) {
                        moveTo = new Vector2(RHRandom.Instance().Next(8, 32), 0);
                        if (RHRandom.Instance().Next(1, 2) == 1) {
                            moveTo.X *= -1;
                        }
                    }
                    else {
                        moveTo = new Vector2(0, RHRandom.Instance().Next(8, 32));
                        if (RHRandom.Instance().Next(1, 2) == 1) {
                            moveTo.Y *= -1;
                        }
                    }

                    moveTo = CurrentMap.GetFarthestUnblockedPath(Position + moveTo, this);
                }

                SetMoveObj(moveTo);
            }

            if (MoveToLocation != Vector2.Zero)
            {
                HandleMove(_vMoveTo);
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
                    ChangeMovement(NORMAL_SPEED);
                    break;
                case NPCStateEnum.Wander:
                    ChangeMovement(NPC_WALK_SPEED);
                    break;
            }
        }
        #endregion
    }
}
