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
using RiverHollow.GUIComponents;

namespace RiverHollow.Characters
{
    public abstract class Actor
    {
        #region Properties
        public int ID { get; } = -1;
        public AnimatedSprite BodySprite { get; protected set; }
        public Point Position => BodySprite.Position;
        public void SetPosition(Point value)
        {
            BodySprite.Position = new Point(value.X - CollisionOffset.X, value.Y - CollisionOffset.Y);
        }
        public void MovePosition(Point value)
        {
            BodySprite.Position += value;
        }

        protected Point Size = new Point(Constants.TILE_SIZE, Constants.TILE_SIZE * 2);
        public int Width => Size.X;
        public int Height => Size.Y;

        public Rectangle HoverBox => new Rectangle(Position, Size);
        public Point Center => HoverBox.Center;

        #region CollisionBox
        public virtual Rectangle CollisionBox => new Rectangle(Position.X + CollisionOffset.X, Position.Y + CollisionOffset.Y, CollisionSize.X, CollisionSize.Y);
        public virtual Point CollisionOffset => GetPointByIDKey("CollisionOffset", new Point(0, Height - Constants.TILE_SIZE));
        public virtual Point CollisionSize => GetPointByIDKey("CollisionSize", new Point(Width, Constants.TILE_SIZE));
        public Point CollisionBoxLocation => CollisionBox.Location;
        public Point CollisionCenter => CollisionBox.Center;
        #endregion

        public bool Moving { get; set; } = false;
        public DirectionEnum Facing = DirectionEnum.Down;
        public ActorStateEnum State { get; protected set; } = ActorStateEnum.Walk;
        public ActorTypeEnum ActorType { get; protected set; }

        public string CurrentMapName;
        public RHMap CurrentMap => (!string.IsNullOrEmpty(CurrentMapName) ? MapManager.Maps[CurrentMapName] : null);
        public bool OnTheMap { get; protected set; } = true;
        public Point NewMapPosition;
        public bool OnScreen() { return CurrentMap == MapManager.CurrentMap; }

        #region Knockback
        protected VectorBuffer _vbMovement;
        protected Vector2 _vKnockbackVelocity;
        protected Vector2 _vInitialKnockback;
        public Vector2 AccumulatedMovement => _vbMovement.AccumulatedMovement;
        protected float _fDecay;
        #endregion

        #region Pathing
        protected Thread _pathingThread;
        public bool FollowingPath => _liTilePath.Count > 0;
        protected List<RHTile> _liTilePath;

        public Point MoveToLocation { get; private set; }
        public Point _pLeashPoint { get; protected set; }

        protected bool _bBumpedIntoSomething = false;
        public bool IgnoreCollisions { get; protected set; } = false;
        public bool SlowDontBlock { get; protected set; } = false;
        #endregion

        public float BuffedSpeed => _fBaseSpeed * SpdMult;
        public float SpdMult = Constants.NPC_WALK_SPEED;
        protected float _fBaseSpeed = 1f;

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

        public Actor() : base()
        {
            Size = new Point(Constants.TILE_SIZE, Constants.HUMAN_HEIGHT);

            _liTilePath = new List<RHTile>();
            _vbMovement = new VectorBuffer();
        }

        public Actor(int id, Dictionary<string, string> stringData) : base()
        {
            ID = id;
            ActorType = Util.ParseEnum<ActorTypeEnum>(stringData["Type"]);

            _vbMovement = new VectorBuffer();
            _liTilePath = new List<RHTile>();
            _movementTimer = new RHTimer(_fBaseWanderTimer);

            if (stringData.ContainsKey("Size"))
            {
                Size = Util.ParsePoint(stringData["Size"]);
            }

            Wandering = stringData.ContainsKey("Wander");
        }

        public virtual void Draw(SpriteBatch spriteBatch, bool useLayerDepth = false)
        {
            if (OnTheMap)
            {
                BodySprite.Draw(spriteBatch, useLayerDepth);

                if (Constants.DRAW_COLLISION)
                {
                    spriteBatch.Draw(DataManager.GetTexture(DataManager.HUD_COMPONENTS), CollisionBox, GUIUtils.BLACK_BOX, Color.White * 0.5f, 0f, Vector2.Zero, SpriteEffects.None, GetSprites()[0].LayerDepth - 1);
                }
            }
        }

        public virtual void Update(GameTime gTime)
        {
            foreach (AnimatedSprite spr in GetSprites())
            {
                spr.Update(gTime);
            }

            if (!GamePaused())
            {
                HandleMove();
            }
        }

        public virtual void ProcessRightButtonClick() { }

        public virtual void RollOver() { }

        public string Name()
        {
            return DataManager.GetTextData(ID, "Name", DataType.Actor);
        }

        protected string SpriteName()
        {
            return DataManager.NPC_FOLDER + GetStringByIDKey("Key");
        }

        /// <summary>
        /// Creates a new Animatedsprite object for the given texture string, and adds
        /// all of the given animations to the new AnimatedSprite
        /// </summary>
        /// <param name="listAnimations">A list of AnimationData to add to the sprite</param>
        /// <param name="textureName">The texture name for the AnimatedSprite</param>
        protected AnimatedSprite LoadSpriteAnimations(List<AnimationData> listAnimations, string textureName)
        {
            AnimatedSprite sprite = new AnimatedSprite(textureName);

            foreach (AnimationData data in listAnimations)
            {
                if (data.Directional)
                {
                    AddDirectionalAnimations(ref sprite, data, Width, Height, data.PingPong, data.BackToIdle);
                }
                else
                {
                    sprite.AddAnimation(data.Animation, data.XLocation, data.YLocation, Width, Height, data.Frames, data.FrameSpeed, data.PingPong, data.Animation == AnimationEnum.KO);
                }
            }

            sprite.PlayAnimation(VerbEnum.Idle, Facing);

            return sprite;
        }

        public virtual bool HoverContains(Point mouse)
        {
            return HoverBox.Contains(mouse);
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
                DetermineFacing(new Point(tile.Position.X - CollisionBoxLocation.X, tile.Position.Y - CollisionBoxLocation.Y));
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
            BodySprite.PlayAnimation(VerbEnum.Walk, Facing);
        }

        public virtual void SetMoveTo(Point v, bool update = true)
        {
            MoveToLocation = v;
            _vbMovement.Clear();
            if (update)
            {
                if (v != Point.Zero)
                {
                    DetermineFacing(v - CollisionBoxLocation);
                }
                DetermineAnimationState(v);
            }
        }

        public bool HasMovement()
        {
            return MoveToLocation != Point.Zero;
        }

        /// <summary>
        /// Constructs the proper animation string for the current facing.
        /// </summary>
        public void PlayAnimationVerb(VerbEnum verb) { PlayAnimation(verb, Facing); }

        /// <summary>
        /// Sets the active status of the WorldActor
        /// </summary>
        /// <param name="value">Whether the actor is active or not.</param>
        public void Activate(bool value)
        {
            OnTheMap = value;
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
                case ActorTypeEnum.Critter:
                case ActorTypeEnum.Mob:
                case ActorTypeEnum.Mount:
                case ActorTypeEnum.Projectile:
                    return false;
            }

            return true;
        }
        public bool IsActorType(ActorTypeEnum act) { return ActorType == act; }
        public virtual void ChangeState(NPCStateEnum state)
        {
            _eCurrentState = state;
            switch (state)
            {
                case NPCStateEnum.Alert:
                    PlayAnimation(AnimationEnum.Alert);
                    break;
                case NPCStateEnum.Idle:
                    PlayAnimation(VerbEnum.Idle);
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
            MovePosition(_vbMovement.AddMovement(p.ToVector2()));
        }
        public void MoveActor(Vector2 v, bool faceDir = true)
        {
            MovePosition(_vbMovement.AddMovement(v));
            if (faceDir)
            {
                DetermineAnimationState(v);
            }
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

            Point startPosition = CollisionBoxLocation;
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
                Vector2  direction = Util.GetMoveSpeed(CollisionBoxLocation, MoveToLocation, BuffedSpeed);

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
                    MoveActor(direction * (impeded ? Constants.IMPEDED_SPEED : 1f));
                }
                else { _bBumpedIntoSomething = true; }

                if(initial != direction)
                {
                    _bBumpedIntoSomething = true;
                }

                //If, after movement, we've reached the given location, zero it.
                if (MoveToLocation == CollisionBoxLocation && !CutsceneManager.Playing)
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
                SoundManager.PlayEffectAtLoc(SoundEffectEnum.Door, this.CurrentMapName, potentialTravelPoint.Center);
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
            if (BodySprite.AnimationFinished(AnimationEnum.Alert))
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
                    if (tileDir != null)
                    {
                        moveTo = CollisionBoxLocation + Util.MultiplyPoint((tileDir.Position - myTile.Position), RHRandom.Instance().Next(_iMinWander, _iMaxWander));
                    }
                }

                SetMoveTo(moveTo);
            }

            if (HasMovement())
            {
                HandleMove();
            }
        }
        #endregion

        #region Animation Logic
        public virtual List<AnimatedSprite> GetSprites()
        {
            List<AnimatedSprite> liRv = new List<AnimatedSprite>() { BodySprite };
            return liRv;
        }

        public virtual void PlayAnimation<TEnum>(TEnum e) { BodySprite.PlayAnimation(e); }
        public virtual void PlayAnimation(VerbEnum verb) { BodySprite.PlayAnimation(verb, Facing); }
        public virtual void PlayAnimation(VerbEnum verb, DirectionEnum dir) { BodySprite.PlayAnimation(verb, dir); }
        public virtual void PlayAnimation(string verb, DirectionEnum dir) { BodySprite.PlayAnimation(verb, dir); }

        /// <summary>
        /// Adds a set of animations to the indicated Sprite for the given verb for each direction.
        /// </summary>
        /// <param name="sprite">Reference to the Sprite to add the animation to</param>
        /// <param name="data">The AnimationData to use</param>
        /// <param name="width">Frame width</param>
        /// <param name="height">Frame height</param>
        /// <param name="pingpong">Whether the animation pingpong or not</param>
        /// <param name="backToIdle">Whether or not the animation should go back to Idle after playing</param>
        /// <returns>The amount of pixels this animation sequence has crawled</returns>
        protected int AddDirectionalAnimations(ref AnimatedSprite sprite, AnimationData data, int width, int height, bool pingpong, bool backToIdle)
        {
            int xCrawl = 0;
            sprite.AddAnimation(data.Verb, DirectionEnum.Down, data.XLocation + xCrawl, data.YLocation, width, height, data.Frames, data.FrameSpeed, pingpong);
            xCrawl += width * data.Frames;
            sprite.AddAnimation(data.Verb, DirectionEnum.Right, data.XLocation + xCrawl, data.YLocation, width, height, data.Frames, data.FrameSpeed, pingpong);
            xCrawl += width * data.Frames;
            sprite.AddAnimation(data.Verb, DirectionEnum.Up, data.XLocation + xCrawl, data.YLocation, width, height, data.Frames, data.FrameSpeed, pingpong);
            xCrawl += width * data.Frames;
            sprite.AddAnimation(data.Verb, DirectionEnum.Left, data.XLocation + xCrawl, data.YLocation, width, height, data.Frames, data.FrameSpeed, pingpong);
            xCrawl += width * data.Frames;

            if (backToIdle)
            {
                SetNextAnimationToIdle(ref sprite, data.Verb, DirectionEnum.Down);
                SetNextAnimationToIdle(ref sprite, data.Verb, DirectionEnum.Right);
                SetNextAnimationToIdle(ref sprite, data.Verb, DirectionEnum.Up);
                SetNextAnimationToIdle(ref sprite, data.Verb, DirectionEnum.Left);
            }

            return xCrawl;
        }

        /// <summary>
        /// Helper function to set the given Verbs next animation to Idle
        /// </summary>
        /// <param name="sprite">The sprite to modify</param>
        /// <param name="verb">The verb to set the next animation of</param>
        /// <param name="dir">The Direction to do it to</param>
        private void SetNextAnimationToIdle(ref AnimatedSprite sprite, VerbEnum verb, DirectionEnum dir)
        {
            sprite.SetNextAnimation(Util.GetActorString(verb, dir), Util.GetActorString(VerbEnum.Idle, dir));
        }
        #endregion

        #region Lookup Handlers
        public bool GetBoolByIDKey(string key)
        {
            return DataManager.GetBoolByIDKey(ID, key, DataType.Actor);
        }
        public int GetIntByIDKey(string key, int defaultValue = -1)
        {
            return DataManager.GetIntByIDKey(ID, key, DataType.Actor, defaultValue);
        }
        public float GetFloatByIDKey(string key, int defaultValue = -1)
        {
            return DataManager.GetFloatByIDKey(ID, key, DataType.Actor, defaultValue);
        }
        public string GetStringByIDKey(string key)
        {
            return DataManager.GetStringByIDKey(ID, key, DataType.Actor);
        }
        protected TEnum GetEnumByIDKey<TEnum>(string key) where TEnum : struct
        {
            return DataManager.GetEnumByIDKey<TEnum>(ID, key, DataType.Actor);
        }
        protected Point GetPointByIDKey(string key, Point defaultPoint = default)
        {
            return DataManager.GetPointByIDKey(ID, key, DataType.Actor, defaultPoint);
        }
        #endregion
    }
}
