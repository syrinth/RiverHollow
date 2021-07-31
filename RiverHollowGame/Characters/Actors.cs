using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.Buildings;
using RiverHollow.CombatStuff;
using RiverHollow.Game_Managers;
using RiverHollow.GUIComponents.Screens;
using RiverHollow.Items;
using RiverHollow.Misc;
using RiverHollow.SpriteAnimations;
using RiverHollow.Tile_Engine;
using RiverHollow.Utilities;
using System;
using System.Collections.Generic;
using static RiverHollow.Game_Managers.DataManager;
using static RiverHollow.Game_Managers.GameManager;
using static RiverHollow.Game_Managers.SaveManager;
using static RiverHollow.Game_Managers.TravelManager;

namespace RiverHollow.Characters
{
    #region Abstract Supertypes
    ///These abstract classes are separate to compartmentalize the various properties and
    ///methods for each layer of an Actor's existence. Technically could be one large abstract class
    ///but they have been separated for ease of access

    /// <summary>
    /// The base proprties and methods for each Actor
    /// </summary>
    public abstract class Actor
    {
        public const float NORMAL_SPEED = 1f;
        public const float NPC_WALK_SPEED = 0.6f;

        protected const int HUMAN_HEIGHT = (TILE_SIZE * 2);
        protected const float EYE_DEPTH = 0.001f;
        protected const float HAIR_DEPTH = 0.003f;

        protected static string _sMerchantFolder = DataManager.FOLDER_ACTOR + @"Merchants\";
        protected static string _sPortraitFolder = DataManager.FOLDER_ACTOR + @"Portraits\";
        protected static string _sNPCFolder = DataManager.FOLDER_ACTOR + @"NPCs\";
        protected static string _sCreatureFolder = DataManager.FOLDER_ACTOR + @"Creatures\";

        protected ActorMovementStateEnum _eMovementState = ActorMovementStateEnum.Idle;

        protected ActorEnum _eActorType = ActorEnum.Actor;
        public ActorEnum ActorType => _eActorType;

        public DirectionEnum Facing = DirectionEnum.Down;

        protected string _sName;
        public virtual string Name { get => _sName; }

        protected AnimatedSprite _sprBody;
        public AnimatedSprite BodySprite => _sprBody;

        public virtual Vector2 Position
        {
            get { return new Vector2(_sprBody.Position.X, _sprBody.Position.Y); }
            set { _sprBody.Position = value; }
        }
        public virtual Vector2 Center => _sprBody.Center;

        public Rectangle GetRectangle()
        {
            return new Rectangle((int)Position.X, (int)Position.Y, Width, Height);
        }

        protected int _iBodyWidth = TILE_SIZE;
        public int Width => _iBodyWidth;
        protected int _iBodyHeight = TILE_SIZE * 2;
        public int Height => _iBodyHeight;
        public int SpriteWidth => _sprBody.Width;
        public int SpriteHeight => _sprBody.Height;

        protected double _dAccumulatedMovement;

        protected bool _bCanTalk = false;
        public bool CanTalk => _bCanTalk;

        public Actor() { }

        public virtual void Update(GameTime gTime)
        {
            foreach (AnimatedSprite spr in GetSprites())
            {
                spr.Update(gTime);
            }
        }

        public virtual void Draw(SpriteBatch spriteBatch, bool useLayerDepth = false)
        {
            _sprBody.Draw(spriteBatch, useLayerDepth);
        }

        protected virtual List<AnimatedSprite> GetSprites()
        {
            List<AnimatedSprite> liRv = new List<AnimatedSprite>() { _sprBody };
            return liRv;
        }

        public virtual void SetName(string text)
        {
            _sName = text;
        }

        public virtual void PlayAnimation(AnimationEnum verb) { _sprBody.PlayAnimation(verb); }
        public virtual void PlayAnimation(VerbEnum verb) { PlayAnimation(verb, Facing); }
        public virtual void PlayAnimation(VerbEnum verb, DirectionEnum dir) { _sprBody.PlayAnimation(verb, dir); }
        public virtual void PlayAnimation(string verb, DirectionEnum dir) { _sprBody.PlayAnimation(verb, dir); }

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

        protected void RemoveDirectionalAnimations(ref AnimatedSprite sprite, VerbEnum verb)
        {
            sprite.RemoveAnimation(verb, DirectionEnum.Down);
            sprite.RemoveAnimation(verb, DirectionEnum.Right);
            sprite.RemoveAnimation(verb, DirectionEnum.Up);
            sprite.RemoveAnimation(verb, DirectionEnum.Left);
        }

        public bool IsCurrentAnimation(AnimationEnum val) { return _sprBody.IsCurrentAnimation(val); }
        public bool IsCurrentAnimation(VerbEnum verb, DirectionEnum dir) { return _sprBody.IsCurrentAnimation(verb, dir); }
        public bool IsAnimating() { return _sprBody.Drawing; }
        public bool AnimationPlayedXTimes(int x) { return _sprBody.GetPlayCount() >= x; }

        public bool IsActorType(ActorEnum act) { return _eActorType == act; }

        /// <summary>
        /// Helper method for ImportBasics to compile the list of relevant animations
        /// </summary>
        /// <param name="list">List to add to</param>
        /// <param name="data">Data to read form</param>
        /// <param name="verb">Verb to add</param>
        /// <param name="directional">Whether the animation will have a version for each direction</param>
        /// <param name="backToIdle">Whether the animation transitions to the Idle state after playing</param>
        /// <param name="playsOnce">Whether the animation should play once then disappear</param>
        protected void AddToAnimationsList(ref List<AnimationData> list, Dictionary<string, string> data, VerbEnum verb)
        {
            AddToAnimationsList(ref list, data, verb, true, false);
        }
        protected void AddToAnimationsList(ref List<AnimationData> list, Dictionary<string, string> data, VerbEnum verb, bool directional, bool backToIdle)
        {
            if (data.ContainsKey(Util.GetEnumString(verb)))
            {
                list.Add(new AnimationData(data[Util.GetEnumString(verb)], verb, backToIdle, directional));
            }
        }
        protected void AddToAnimationsList(ref List<AnimationData> list, Dictionary<string, string> data, AnimationEnum animation)
        {
            if (data.ContainsKey(Util.GetEnumString(animation)))
            {
                list.Add(new AnimationData(data[Util.GetEnumString(animation)], animation));
            }
        }
    }
    /// <summary>
    /// The properties and methods for each actor that pertain to existing on and
    /// interacting with the WorldMap itself
    /// </summary>
    public abstract class WorldActor : Actor
    {
        #region Properties
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

        #endregion

        public WorldActor() : base()
        {
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
        }

        protected List<AnimationData> LoadWorldAnimations(Dictionary<string, string> data)
        {
            List<AnimationData> listAnimations = new List<AnimationData>();
            AddToAnimationsList(ref listAnimations, data, VerbEnum.Walk);
            AddToAnimationsList(ref listAnimations, data, VerbEnum.Idle);
            return listAnimations;
        }
        protected List<AnimationData> LoadCombatAnimations(Dictionary<string, string> data)
        {
            List<AnimationData> listAnimations = new List<AnimationData>();
            AddToAnimationsList(ref listAnimations, data, VerbEnum.Action1);
            AddToAnimationsList(ref listAnimations, data, VerbEnum.Action2);
            AddToAnimationsList(ref listAnimations, data, VerbEnum.Action3);
            AddToAnimationsList(ref listAnimations, data, VerbEnum.Action4);
            AddToAnimationsList(ref listAnimations, data, VerbEnum.Hurt);
            AddToAnimationsList(ref listAnimations, data, VerbEnum.Critical);
            AddToAnimationsList(ref listAnimations, data, VerbEnum.Cast);
            AddToAnimationsList(ref listAnimations, data, AnimationEnum.KO);
            AddToAnimationsList(ref listAnimations, data, AnimationEnum.Spawn);
            return listAnimations;
        }
        protected List<AnimationData> LoadWorldAndCombatAnimations(Dictionary<string, string> data)
        {
            List<AnimationData> listAnimations = new List<AnimationData>();
            listAnimations.AddRange(LoadWorldAnimations(data));
            listAnimations.AddRange(LoadCombatAnimations(data));
            return listAnimations;
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
                PlayAnimationVerb((walk || CombatManager.InCombat) ? VerbEnum.Walk : VerbEnum.Idle);
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
            //Determines the distance that needs to be traveled from the current position to the target
            Vector2 direction = Vector2.Zero;
            float deltaX = Math.Abs(target.X - this.Position.X);
            float deltaY = Math.Abs(target.Y - this.Position.Y);

            //Determines how much of the needed position we're capable of  in one movement
            Util.GetMoveSpeed(Position, target, BuffedSpeed, ref direction);

            //If we're following a path and there's more than one tile left, we don't want to cut
            //short on individual steps, so recalculate based on the next target
            float length = direction.Length();
            if (_liTilePath.Count > 1 && length < BuffedSpeed)
            {
                _liTilePath.RemoveAt(0);

                if (DoorCheck())
                {
                    return;
                }

                //Recalculate for the next target
                target = _liTilePath[0].Position;
                Util.GetMoveSpeed(Position, target, BuffedSpeed, ref direction);
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
    }

    public abstract class TalkingActor : WorldActor
    {
        protected const int PortraitWidth = 160;
        protected const int PortraitHeight = 192;

        protected ActorFaceEnum _eFaceEnum;
        protected List<ActorFaceEnum> _liActorFaceQueue;
        protected string _sPortrait;
        public string Portrait => _sPortrait;

        protected Dictionary<string, TextEntry> _diDialogue;

        public static List<int> FriendRange = new List<int> { 0, 10, 40, 100, 200, 600, 800, 1200, 1600, 2000 };
        public int FriendshipPoints = 0;

        protected bool _bHasTalked;

        public bool CanGiveGift = true;
        public bool CanJoinParty = false;

        protected List<string> _liSpokenKeys;

        public TalkingActor() : base()
        {
            _bCanTalk = true;
            _liActorFaceQueue = new List<ActorFaceEnum>();
            _liSpokenKeys = new List<string>();
        }

        public virtual TextEntry Gift(Item item) { return null; }
        public virtual TextEntry JoinParty() { return null; }
        public virtual void OpenShop() { }
        public virtual TextEntry OpenRequests() { return null; }

        public virtual void StopTalking() { ResetActorFace(); }

        public override void ProcessRightButtonClick()
        {
            Talk();
        }

        /// <summary>
        /// Used when already talking to an NPC, gets the next dialog tag in the conversation
        /// and opens a new window for it.
        /// </summary>
        /// <param name="dialogTag">The dialog tag to talk with</param>
        public void Talk(string dialogTag)
        {
            TextEntry text = null;
            if (_diDialogue.ContainsKey(dialogTag))
            {
                text = _diDialogue[dialogTag];
            }

            GUIManager.OpenTextWindow(text, this, true, true);
        }

        /// <summary>
        ///  Retrieves any opening text, processes it, then opens a text window
        /// </summary>
        /// <param name="facePlayer">Whether the NPC should face the player. Mainly used to avoid messing up a cutscene</param>
        public virtual void Talk(bool facePlayer = true)
        {
            FacePlayer(true);
            GUIManager.OpenTextWindow(GetOpeningText(), this, true, true);
        }
        protected void FacePlayer(bool facePlayer)
        {
            //Determine the position based off of where the player is and then have the NPC face the player
            //Only do this if they are idle so as to not disturb other animations they may be performing.
            if (facePlayer && BodySprite.CurrentAnimation.StartsWith("Idle"))
            {
                Point diff = GetRectangle().Center - PlayerManager.World.GetRectangle().Center;
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

                PlayAnimationVerb(CombatManager.InCombat ? VerbEnum.Walk : VerbEnum.Idle);
            }
        }

        public void TalkCutscene(TextEntry cutsceneEntry)
        {
            GUIManager.OpenTextWindow(cutsceneEntry, this, true, true);
        }

        /// <summary>
        /// Stand-in Virtual method to be overrriden. Should never get called.
        /// </summary>
        public virtual TextEntry GetOpeningText() { return new TextEntry(); }

        /// <summary>
        /// Base method to get a line of dialog from the dialog dictionary.
        /// 
        /// Mostly used for the "Talk" parameter or if the TalkingActor has no other options.
        /// </summary>
        /// <returns>The dialog string for the entry.</returns>
        public TextEntry GetDailyDialogue()
        {
            _bHasTalked = true;
            PriorityQueue<TextEntry> keyPool = new PriorityQueue<TextEntry>();
            foreach (TextEntry entry in _diDialogue.Values)
            {
                if (entry.Valid())
                {
                    keyPool.Enqueue(entry, entry.Priority);
                }
            }

            List<TextEntry> possibles = keyPool.DequeueAllLowest();

            return possibles[RHRandom.Instance().Next(0, possibles.Count - 1)];
        }

        /// <summary>
        /// Retrieves the specified entry from the _diDictionaryand calls Util.ProcessTexton it.
        /// </summary>
        /// <param name="entry">The key of the entry to get from the Dictionary</param>
        /// <returns>The processed string text for the entry </returns>
        public virtual TextEntry GetDialogEntry(string entry)
        {
            TextEntry rv = null;

            if (_diDialogue.ContainsKey(entry))
            {
                rv = _diDialogue[entry];
            }

            return rv;
        }

        /// <summary>
        /// Determines the level of Friendship based off of how many Friendship points the Actor has.
        /// </summary>
        /// <returns></returns>
        public int GetFriendshipLevel()
        {
            int rv = 0;
            for (int i = 0; i < FriendRange.Count; i++)
            {
                if (FriendshipPoints >= FriendRange[i])
                {
                    rv = i;
                }
            }

            return rv;
        }

        /// <summary>
        /// Creates the queue of faces the TalkingActor will progress through as they talk.
        /// </summary>
        /// <param name="value">A '-' delimited list of facial expressions corresponding to ActoEnumFaces</param>
        public void QueueActorFace(string value)
        {
            foreach (string s in value.Split('-'))
            {
                _liActorFaceQueue.Add(Util.ParseEnum<ActorFaceEnum>(s));
            }
        }

        /// <summary>
        /// Removes the first entry from list of queued faces and sets it as the current face.
        /// Calling this again on an empty list resets the face to default
        /// </summary>
        public void DeQueueActorFace()
        {
            if (_liActorFaceQueue.Count == 0) { ResetActorFace(); }
            else
            {
                _eFaceEnum = _liActorFaceQueue[0];
                _liActorFaceQueue.RemoveAt(0);
            }
        }

        /// <summary>
        /// Clears the FaceQueue and resets the face to Default
        /// </summary>
        public void ResetActorFace()
        {
            _liActorFaceQueue.Clear();
            _eFaceEnum = ActorFaceEnum.Default;
        }

        public Rectangle GetPortraitRectangle()
        {
            int startX = 0;
            int width = 48;
            startX += (int)_eFaceEnum * width;
            return new Rectangle(startX, 0, width, 60);
        }

        public void AddSpokenKey(string key)
        {
            _liSpokenKeys.Add(key);
        }
    }

    /// <summary>
    /// The properties and methodsthat pertain to Combat, stats, gear, etc
    /// </summary>
    public abstract class CombatActor : TalkingActor
    {
        #region Properties

        protected const int MAX_STAT = 99;
        protected string _sUnique;

        protected bool _bPause;
        public bool Paused => _bPause;

        public override string Name => String.IsNullOrEmpty(_sUnique) ? _sName : _sName + " " + _sUnique;

        protected int _iCurrentHP;
        public int CurrentHP
        {
            get { return _iCurrentHP; }
            set { _iCurrentHP = value; }
        }
        public virtual int MaxHP => 20 + (int)Math.Pow(((double)StatVit / 3), 1.98);

        protected int _iCurrentMP;
        public int CurrentMP
        {
            get { return _iCurrentMP; }
            set { _iCurrentMP = value; }
        }
        public virtual int MaxMP => StatMag * 3;

        public int CurrentCharge;
        public int DummyCharge;
        public RHTile BaseTile => _arrTiles[0, 0];
        protected RHTile[,] _arrTiles;
        public PriorityQueue<RHTile> legalTiles;

        #region Stats
        protected int _iMoveSpeed = 5;
        public int MovementSpeed => _iMoveSpeed;

        public virtual int Attack => 9;

        protected int _iStrength;
        public virtual int StatStr => _iStrength + _iBuffStr;
        protected int _iDefense;
        public virtual int StatDef => _iDefense + _iBuffDef;
        protected int _iVitality;
        public virtual int StatVit => _iVitality + _iBuffVit;
        protected int _iMagic;
        public virtual int StatMag => _iMagic + _iBuffMag;
        protected int _iResistance;
        public virtual int StatRes => _iResistance + _iBuffRes;
        protected int _iSpeed;
        public virtual int StatSpd => _iSpeed + _iBuffSpd;

        protected int _iBuffStr;
        protected int _iBuffDef;
        protected int _iBuffVit;
        protected int _iBuffMag;
        protected int _iBuffRes;
        protected int _iBuffSpd;
        protected int _iBuffCrit;
        protected int _iBuffEvade;
        #endregion

        protected int _iCrit = 10;
        public int CritRating => _iCrit + _iBuffCrit;

        public int Evasion => (int)(40 / (1 + 10 * (Math.Pow(Math.E, (-0.05 * StatSpd))))) + _iBuffEvade;
        public int ResistStatus => (int)(50 / (1 + 10 * (Math.Pow(Math.E, (-0.05 * StatRes)))));

        protected List<MenuAction> _liActions;
        public virtual List<MenuAction> AbilityList => _liActions;

        protected List<StatusEffect> _liStatusEffects;
        public List<StatusEffect> LiBuffs { get => _liStatusEffects; }

        protected Dictionary<ConditionEnum, bool> _diConditions;
        public Dictionary<ConditionEnum, bool> DiConditions => _diConditions;

        private ElementEnum _elementAttackEnum;
        protected Dictionary<ElementEnum, ElementAlignment> _diElementalAlignment;
        public Dictionary<ElementEnum, ElementAlignment> DiElementalAlignment => _diElementalAlignment;

        public Summon LinkedSummon { get; private set; }

        public bool Counter;
        public bool GoToCounter;

        public CombatActor MyGuard;
        public CombatActor GuardTarget;
        protected bool _bGuard;
        public bool Guard => _bGuard;

        public bool Swapped;
        #endregion

        public CombatActor() : base()
        {
            legalTiles = new PriorityQueue<RHTile>();
            _arrTiles = new RHTile[_iSize, _iSize];
            _liActions = new List<MenuAction>();
            _liStatusEffects = new List<StatusEffect>();
            _diConditions = new Dictionary<ConditionEnum, bool>
            {
                [ConditionEnum.KO] = false,
                [ConditionEnum.Poisoned] = false,
                [ConditionEnum.Silenced] = false
            };

            _diElementalAlignment = new Dictionary<ElementEnum, ElementAlignment>
            {
                [ElementEnum.Fire] = ElementAlignment.Neutral,
                [ElementEnum.Ice] = ElementAlignment.Neutral,
                [ElementEnum.Lightning] = ElementAlignment.Neutral
            };

        }
        public override void Draw(SpriteBatch spriteBatch, bool useLayerDepth = false)
        {
            base.Draw(spriteBatch, useLayerDepth);

            if (CombatManager.InCombat && _iCurrentHP > 0)
            {
                Texture2D texture = DataManager.GetTexture(DataManager.DIALOGUE_TEXTURE);
                Vector2 pos = Position;
                pos.Y += (TILE_SIZE * _iSize);

                //Do not allow the bar to have less than 2 pixels, one for the border and one to display.
                double totalWidth = TILE_SIZE * Size;
                double percent = (double)CurrentHP / (double)MaxHP;
                int drawWidth = Math.Max((int)(totalWidth * percent), 2);

                DrawDisplayBar(spriteBatch, pos, texture, drawWidth, (int)totalWidth, 16, 0);

                if (MaxMP > 0)
                {
                    totalWidth = TILE_SIZE * Size;
                    percent = (double)CurrentMP / (double)MaxMP;
                    drawWidth = Math.Max((int)(totalWidth * percent), 2);

                    pos.Y += 4;
                    DrawDisplayBar(spriteBatch, pos, texture, drawWidth, (int)totalWidth, 22, 0);
                }
            }
        }

        private void DrawDisplayBar(SpriteBatch spriteBatch, Vector2 pos, Texture2D texture, int drawWidth, int totalWidth, int startX, int startY)
        {
            spriteBatch.Draw(texture, new Rectangle((int)pos.X, (int)pos.Y, drawWidth, 4), new Rectangle(startX + 4, startY, 1, 4), Color.White, 0, Vector2.Zero, SpriteEffects.None, pos.Y);

            //Draw End, then middle, then other end
            spriteBatch.Draw(texture, new Rectangle((int)pos.X, (int)pos.Y, 1, 4), new Rectangle(startX, startY, 1, 4), Color.White, 0, Vector2.Zero, SpriteEffects.None, pos.Y + 1);
            spriteBatch.Draw(texture, new Rectangle((int)pos.X + 1, (int)pos.Y, (int)totalWidth - 2, 4), new Rectangle(startX + 1, startY, 2, 4), Color.White, 0, Vector2.Zero, SpriteEffects.None, pos.Y + 1);
            spriteBatch.Draw(texture, new Rectangle((int)pos.X + (int)totalWidth - 1, (int)pos.Y, 1, 4), new Rectangle(startX + 3, startY, 1, 4), Color.White, 0, Vector2.Zero, SpriteEffects.None, pos.Y + 1);
        }

        public override void Update(GameTime gTime)
        {
            base.Update(gTime);

            //Finished being hit, determine action
            if (IsCurrentAnimationVerb(VerbEnum.Hurt) && BodySprite.GetPlayCount() >= 1)
            {
                if (_bPause) { _bPause = false; }

                if (CurrentHP == 0) { KO(); }
                else { GoToIdle(); }
            }

            ///Stand back up after the KO status has been removed
            if (!_diConditions[ConditionEnum.KO] && IsCurrentAnimation(AnimationEnum.KO))
            {
                GoToIdle();
            }

            if (IsCurrentAnimationVerb(VerbEnum.Critical) && !IsCritical())
            {
                PlayAnimationVerb(VerbEnum.Walk);
            }

            if (!_bPause && (MapManager.Maps.ContainsKey(CurrentMapName) && MapManager.Maps[CurrentMapName].ContainsActor(this) || this == PlayerManager.World))
            {
                if (_vMoveTo != Vector2.Zero)
                {
                    HandleMove(_vMoveTo);
                }
                else if (_liTilePath.Count > 0)
                {
                    if (!DoorCheck())
                    {
                        Vector2 targetPos = _liTilePath[0].Position;
                        if (Position == targetPos)
                        {
                            RHTile newTile = _liTilePath[0];
                            _liTilePath.Remove(newTile);

                            if (_liTilePath.Count == 0)
                            {
                                if (PlayerManager.ReadyToSleep)
                                {
                                    if (_dCooldown == 0)
                                    {
                                        Facing = DirectionEnum.Left;
                                        PlayAnimation(VerbEnum.Idle, DirectionEnum.Left);
                                        _dCooldown = 3;
                                        PlayerManager.AllowMovement = true;
                                    }
                                }
                                else
                                {
                                    DetermineFacing(Vector2.Zero);
                                    GoToIdle();
                                }
                            }
                            else if (CombatManager.InCombat)
                            {
                                CombatManager.CheckTileForActiveHazard(this, newTile);
                            }
                        }
                        else
                        {
                            HandleMove(targetPos);
                        }
                    }
                }
            }
        }

        public virtual void GoToIdle()
        {
            if (IsCritical()) { PlayAnimationVerb(VerbEnum.Critical); }
            else { PlayAnimationVerb(VerbEnum.Walk); }
        }

        public virtual void KO()
        {
            CombatManager.RemoveKnockedOutCharacter(this);
            PlayAnimation(AnimationEnum.KO);
        }

        /// <summary>
        /// Calculates the damage to be dealt against the actor.
        /// 
        /// Run the damage equation against the defender, then apply any 
        /// relevant elemental resistances.
        /// 
        /// Finally, roll against the crit rating. Rolling higher than the 
        /// rating on a percentile roll means no crit. Crit Rating 10 means
        /// roll 10 or less
        /// </summary>
        /// <param name="attacker">Who is attacking</param>
        /// <param name="potency">The potency of the attack</param>
        /// <param name="element">any associated element</param>
        /// <returns></returns>
        public void ProcessAttack(CombatActor attacker, int potency, int critRating, ElementEnum element = ElementEnum.None)
        {
            double compression = 0.8;
            double potencyMod = potency / 100.0;   //100 potency is considered an average attack
            double base_attack = attacker.Attack;  //Attack stat is either weapon damage or mod on monster str
            double StrMult = Math.Round(1 + ((double)attacker.StatStr / 4 * attacker.StatStr / MAX_STAT), 2);

            double dmg = (Math.Max(1, base_attack - StatDef) * compression * StrMult * potencyMod);
            dmg += ApplyResistances(dmg, element);

            if (RHRandom.Instance().Next(1, 100) <= (attacker.CritRating + critRating)) { dmg *= 2; }

            ModifyHealth(dmg, true);
        }
        public void ProcessSpell(CombatActor attacker, int potency, ElementEnum element = ElementEnum.None)
        {
            double maxDmg = (1 + potency) * 3;
            double divisor = 1 + (30 * Math.Pow(Math.E, -0.12 * (attacker.StatMag - StatRes) * Math.Round((double)attacker.StatMag / MAX_STAT, 2)));

            double damage = Math.Round(maxDmg / divisor);
            damage += ApplyResistances(damage, element);

            ModifyHealth(damage, true);
        }
        public double ApplyResistances(double dmg, ElementEnum element = ElementEnum.None)
        {
            double modifiedDmg = 0;
            if (element != ElementEnum.None)
            {
                if (MapManager.CurrentMap.IsOutside && EnvironmentManager.IsRaining())
                {
                    if (element.Equals(ElementEnum.Lightning)) { modifiedDmg += (dmg * 1.2) - dmg; }
                    else if (element.Equals(ElementEnum.Fire)) { modifiedDmg += (dmg * 0.8) - dmg; }
                }
                else if (MapManager.CurrentMap.IsOutside && EnvironmentManager.IsSnowing())
                {
                    if (element.Equals(ElementEnum.Ice)) { modifiedDmg += (dmg * 1.2) - dmg; }
                    else if (element.Equals(ElementEnum.Lightning)) { modifiedDmg += (dmg * 0.8) - dmg; }
                }

                //Should only apply for Summoners
                if (LinkedSummon != null && _diElementalAlignment[element].Equals(ElementAlignment.Neutral))
                {
                    if (LinkedSummon.Element.Equals(element))
                    {
                        modifiedDmg += (dmg * 0.8) - dmg;
                    }
                }

                if (_diElementalAlignment[element].Equals(ElementAlignment.Resists))
                {
                    modifiedDmg += (dmg * 0.8) - dmg;
                }
                else if (_diElementalAlignment[element].Equals(ElementAlignment.Vulnerable))
                {
                    modifiedDmg += (dmg * 1.2) - dmg;
                }
            }

            return modifiedDmg;
        }

        public void ProcessHealingSpell(CombatActor attacker, int potency)
        {
            double maxDmg = (1 + potency) * 3;
            double divisor = 1 + (30 * Math.Pow(Math.E, -0.12 * (attacker.StatMag - StatRes) * Math.Round((double)attacker.StatMag / MAX_STAT, 2)));

            int damage = (int)Math.Round(maxDmg / divisor);

            ModifyHealth(damage, false);
        }

        /// <summary>
        /// Handler for modifying the health of a CombatActor. Ensures
        /// </summary>
        /// <param name="value">The amount to modify HP by</param>
        /// <param name="bHarmful">Whether the modification is harmful or helpful</param>
        public virtual void ModifyHealth(double value, bool bHarmful)
        {
            //Round the value down in case it's not an int due to resistances
            int iValue = (int)Math.Round(value);

            //Handler for when the modification is harmful.
            if (bHarmful)
            {
                //Checks that the current HP is greater than the amount of damage dealt
                //If not, just remove the current HP so that we don't go negative.
                _iCurrentHP -= (_iCurrentHP - iValue >= 0) ? iValue : _iCurrentHP;
                PlayAnimationVerb(VerbEnum.Hurt);

                if (this == CombatManager.ActiveCharacter) { _bPause = true; }

                //If the character goes to 0 hp, give them the KO status and unlink any summons
                if (_iCurrentHP == 0)
                {
                    _diConditions[ConditionEnum.KO] = true;
                    UnlinkSummon();
                }
            }
            else
            {
                //Can't restore HP when the KO condition is present.
                if (!KnockedOut())
                {
                    //Adds only enough life to get to the max. No Overhealing
                    if (_iCurrentHP + iValue <= MaxHP)
                    {
                        _iCurrentHP += iValue;
                    }
                    else
                    {
                        iValue = MaxHP - _iCurrentHP;
                        _iCurrentHP = MaxHP;
                    }
                }
            }

            CombatManager.AddFloatingText(new FloatingText(this.Position, this.Width, iValue.ToString(), bHarmful ? Color.Red : Color.Green));
        }

        public bool IsCritical()
        {
            return (float)CurrentHP / (float)MaxHP <= 0.25;
        }

        public void IncreaseMana(int x)
        {
            if (_iCurrentMP + x <= MaxMP)
            {
                _iCurrentMP += x;
            }
            else
            {
                _iCurrentMP = MaxMP;
            }
        }

        /// <summary>
        /// Reduce the duration of each status effect on the Actor by one
        /// If the effect's duration reaches 0, remove it, otherwise have it run
        /// any upkeep effects it may need to do.
        /// </summary>
        public void TickStatusEffects()
        {
            List<StatusEffect> toRemove = new List<StatusEffect>();
            foreach (StatusEffect b in _liStatusEffects)
            {
                if (--b.Duration == 0)
                {
                    toRemove.Add(b);
                    RemoveStatusEffect(b);
                }
                else
                {
                    if (b.DoT)
                    {
                        ProcessSpell(b.Caster, b.Potency);
                    }
                    if (b.HoT)
                    {
                        ProcessHealingSpell(b.Caster, b.Potency);
                    }
                }
            }

            foreach (StatusEffect b in toRemove)
            {
                _liStatusEffects.Remove(b);
            }
            toRemove.Clear();
        }

        /// <summary>
        /// Adds the StatusEffect objectto the character's list of status effects.
        /// </summary>
        /// <param name="b">Effect toadd</param>
        public void AddStatusEffect(StatusEffect b)
        {
            //Only one song allowed at a time so see if there is another
            //songand,if so, remove it.
            if (b.Song)
            {
                StatusEffect song = _liStatusEffects.Find(status => status.Song);
                if (song != null)
                {
                    RemoveStatusEffect(song);
                    _liStatusEffects.Remove(song);
                }
            }

            //Look to see if the status effect already exists, if so, just
            //set the duration to be the new duration. No stacking.
            StatusEffect find = _liStatusEffects.Find(status => status.Name == b.Name);
            if (find == null) { _liStatusEffects.Add(b); }
            else { find.Duration = b.Duration; }

            foreach (KeyValuePair<StatEnum, int> kvp in b.StatMods)
            {
                HandleStatBuffs(kvp);
            }

            //If the status effect provides counter, turn counter on.
            if (b.Counter) { Counter = true; }

            if (b.Guard) { _bGuard = true; }
        }

        /// <summary>
        /// Removes the status effect from the Actor
        /// </summary>
        /// <param name="b"></param>
        public void RemoveStatusEffect(StatusEffect b)
        {
            foreach (KeyValuePair<StatEnum, int> kvp in b.StatMods)
            {
                HandleStatBuffs(kvp, true);
            }

            if (b.Counter) { Counter = false; }
            if (b.Guard) { _bGuard = false; }
        }

        /// <summary>
        /// Helper to not repeat code for the Stat buffing/debuffing
        /// 
        /// Pass in the statmod kvp and an integer representing positive or negative
        /// and multiply the mod by it. If we are adding, it will remain unchanged, 
        /// if we are subtracting, the positive value will go negative.
        /// </summary>
        /// <param name="kvp">The stat to modifiy and how much</param>
        /// <param name="negative">Whether or not we need to add or remove the value</param>
        private void HandleStatBuffs(KeyValuePair<StatEnum, int> kvp, bool negative = false)
        {
            int modifier = negative ? -1 : 1;
            switch (kvp.Key)
            {
                case StatEnum.Str:
                    _iBuffStr += kvp.Value * modifier;
                    break;
                case StatEnum.Def:
                    _iBuffDef += kvp.Value * modifier;
                    break;
                case StatEnum.Vit:
                    _iBuffVit += kvp.Value * modifier;
                    break;
                case StatEnum.Mag:
                    _iBuffMag += kvp.Value * modifier;
                    break;
                case StatEnum.Res:
                    _iBuffRes += kvp.Value * modifier;
                    break;
                case StatEnum.Spd:
                    _iBuffSpd += kvp.Value * modifier;
                    break;
                case StatEnum.Crit:
                    _iBuffCrit += kvp.Value * modifier;
                    break;
                case StatEnum.Evade:
                    _iBuffEvade += kvp.Value * modifier;
                    break;
            }
        }

        #region Tile Handling
        /// <summary>
        /// Sets the base tile, which will always be the upper-left most tile
        /// to the given tile, then assign the character to the appropiate tiles around it.
        /// </summary>
        /// <param name="newTile">The tile to be the new base tile</param>
        public void SetBaseTile(RHTile newTile, bool setPosition)
        {
            ClearTiles();

            RHTile lastTile = newTile;
            for (int i = 0; i < _iSize; i++)
            {
                for (int j = 0; j < _iSize; j++)
                {
                    _arrTiles[i, j] = lastTile;
                    _arrTiles[i, j].SetCombatant(this);
                    lastTile = lastTile.GetTileByDirection(DirectionEnum.Right);
                }

                //Reset to the first Tile in the current row and go down one
                lastTile = _arrTiles[i, 0].GetTileByDirection(DirectionEnum.Down);
            }

            CombatManager.CheckTileForActiveHazard(this);
            if (setPosition) { Position = BaseTile.Position; }
        }

        /// <summary>
        /// Returns a List of all RHTiles adjacent to the CombatActor. This method
        /// works in tandem with the Actors size value to return the proper RHTiles
        /// </summary>
        /// <returns></returns>
        public List<RHTile> GetAdjacentTiles()
        {
            List<RHTile> rvList = new List<RHTile>();

            foreach (RHTile t in _arrTiles)
            {
                foreach (RHTile adjTile in t.GetAdjacentTiles())
                {
                    //Do not add the same RHTile twice, nor add a RHTile containing ourself.
                    if (!rvList.Contains(adjTile) && adjTile.Character != this)
                    {
                        rvList.Add(adjTile);
                    }
                }
            }

            return rvList;
        }

        public List<RHTile> GetTileList()
        {
            List<RHTile> rvList = new List<RHTile>();

            for (int y = 0; y < _iSize; y++)
            {
                for (int x = 0; x < _iSize; x++)
                {
                    rvList.Add(_arrTiles[x, y]);
                }
            }

            return rvList;
        }

        public RHTile GetTileAt(int x, int y)
        {
            return _arrTiles[x, y];
        }

        public void ClearTiles()
        {
            //Remove self from each tile that they are registered to.
            foreach (RHTile t in _arrTiles)
            {
                t?.SetCombatant(null);
            }
        }
        #endregion

        public void LinkSummon(Summon s)
        {
            LinkedSummon = s;
            s.linkedChar = this;
        }

        public void UnlinkSummon()
        {
            LinkedSummon?.KO();
            LinkedSummon = null;
        }

        /// <summary>
        /// Returns the Elemental type of the attack. In the event that there is
        /// a LinkedSummon, which should only be the case for Summoners, use the Summons
        /// elemental attack instead if none exists.
        /// </summary>
        /// <returns></returns>
        public virtual ElementEnum GetAttackElement()
        {
            ElementEnum e = _elementAttackEnum;

            if (LinkedSummon != null && e.Equals(ElementEnum.None))
            {
                e = LinkedSummon.Element;
            }

            return e;
        }

        public bool CanCast(int x)
        {
            return x <= CurrentMP;
        }

        public void SetUnique(string u)
        {
            _sUnique = u;
        }

        public bool KnockedOut()
        {
            return _diConditions[ConditionEnum.KO];
        }

        public bool Poisoned()
        {
            return _diConditions[ConditionEnum.Poisoned];
        }

        public bool Silenced()
        {
            return _diConditions[ConditionEnum.Silenced];
        }

        public void ChangeConditionStatus(ConditionEnum c, bool setTo)
        {
            _diConditions[c] = setTo;
        }

        public void ClearConditions()
        {
            foreach (ConditionEnum condition in Enum.GetValues(typeof(ConditionEnum)))
            {
                ChangeConditionStatus(condition, false);
            }
        }

        public virtual void EndTurn() { }

        public void GetHP(ref double curr, ref double max)
        {
            curr = _iCurrentHP;
            max = MaxHP;
        }

        public void GetMP(ref double curr, ref double max)
        {
            curr = _iCurrentMP;
            max = MaxMP;
        }

        public virtual List<CombatAction> GetCurrentSpecials()
        {
            return null;
        }

        public virtual bool IsSummon() { return false; }

    }
    #endregion

    public abstract class ClassedCombatant : CombatActor
    {
        #region Properties
        public static List<int> LevelRange = new List<int> { 0, 20, 80, 160, 320, 640, 1280, 2560, 5120, 10240 };

        protected CharacterClass _class;
        public CharacterClass CharacterClass => _class;
        private int _classLevel;
        public int ClassLevel => _classLevel;

        private Vector2 _vStartPosition;
        public Vector2 StartPosition => _vStartPosition;

        private int _iXP;
        public int XP => _iXP;

        public bool Protected;

        public List<GearSlot> _liGearSlots;
        public GearSlot Weapon;
        public GearSlot Armor;
        public GearSlot Head;
        public GearSlot Wrist;
        public GearSlot Accessory1;
        public GearSlot Accessory2;

        public override int Attack => GetGearAtk();
        public override int StatStr => 10 + _iBuffStr + GetGearStat(StatEnum.Str);
        public override int StatDef => 10 + _iBuffDef + GetGearStat(StatEnum.Def) + (Protected ? 10 : 0);
        public override int StatVit => 10 + (_classLevel * _class.StatVit) + GetGearStat(StatEnum.Vit);
        public override int StatMag => 10 + _iBuffMag + GetGearStat(StatEnum.Mag);
        public override int StatRes => 10 + _iBuffRes + GetGearStat(StatEnum.Res);
        public override int StatSpd => 10 + _class.StatSpd + _iBuffSpd + GetGearStat(StatEnum.Spd);

        public int TempStatStr => 10 + _iBuffStr + GetTempGearStat(StatEnum.Str);
        public int TempStatDef => 10 + _iBuffDef + GetTempGearStat(StatEnum.Def) + (Protected ? 10 : 0);
        public int TempStatVit => 10 + (_classLevel * _class.StatVit) + GetTempGearStat(StatEnum.Vit);
        public int TempStatMag => 10 + _iBuffMag + GetTempGearStat(StatEnum.Mag);
        public int TempStatRes => 10 + _iBuffRes + GetTempGearStat(StatEnum.Res);
        public int TempStatSpd => 10 + _class.StatSpd + _iBuffSpd + GetTempGearStat(StatEnum.Spd);

        public override List<MenuAction> AbilityList => _class.ActionList;

        public int GetGearAtk()
        {
            int rv = 0;

            rv += Weapon.GetStat(StatEnum.Atk);
            rv += base.Attack;

            return rv;
        }
        public int GetGearStat(StatEnum stat)
        {
            int rv = 0;
            if (_liGearSlots != null)
            {
                foreach (GearSlot g in _liGearSlots)
                {
                    rv += g.GetStat(stat);
                }
            }

            return rv;
        }
        public int GetTempGearStat(StatEnum stat)
        {
            int rv = 0;

            foreach (GearSlot g in _liGearSlots)
            {
                rv += g.GetTempStat(stat);
            }

            return rv;
        }
        #endregion

        public ClassedCombatant() : base()
        {
            _classLevel = 1;

            _liGearSlots = new List<GearSlot>();
            Weapon = new GearSlot(EquipmentEnum.Weapon);
            Armor = new GearSlot(EquipmentEnum.Armor);
            Head = new GearSlot(EquipmentEnum.Head);
            Wrist = new GearSlot(EquipmentEnum.Wrist);
            Accessory1 = new GearSlot(EquipmentEnum.Accessory);
            Accessory2 = new GearSlot(EquipmentEnum.Accessory);

            _liGearSlots.Add(Weapon);
            _liGearSlots.Add(Armor);
            _liGearSlots.Add(Head);
            _liGearSlots.Add(Wrist);
            _liGearSlots.Add(Accessory1);
            _liGearSlots.Add(Accessory2);
        }

        public virtual void SetClass(CharacterClass x)
        {
            _class = x;
            _iCurrentHP = MaxHP;
            _iCurrentMP = MaxMP;
        }

        /// <summary>
        /// Assigns the starting gear to the Actor as long as the slots are empty.
        /// Should never be called when they can be euipped, checks are for safety.
        /// </summary>
        public void AssignStartingGear()
        {
            if (Weapon.IsEmpty()) { Weapon.SetGear((Equipment)GetItem(_class.WeaponID)); }
            if (Armor.IsEmpty()) { Armor.SetGear((Equipment)GetItem(_class.ArmorID)); }
            if (Head.IsEmpty()) { Head.SetGear((Equipment)GetItem(_class.HeadID)); }
            if (Wrist.IsEmpty()) { Wrist.SetGear((Equipment)GetItem(_class.WristID)); }
        }

        public void AddXP(int x)
        {
            _iXP += x;
            if (_iXP >= LevelRange[_classLevel])
            {
                _classLevel++;
            }
        }

        public override void PlayAnimation(AnimationEnum animation)
        {
            base.PlayAnimation(animation);
        }

        public void GetXP(ref double curr, ref double max)
        {
            curr = _iXP;
            max = ClassedCombatant.LevelRange[this.ClassLevel];
        }

        #region StartPosition
        public void IncreaseStartPos()
        {
            if (_vStartPosition.Y < 2)
            {
                _vStartPosition.Y++;
            }
            else
            {
                _vStartPosition = new Vector2(_vStartPosition.X++, 0);
            }
        }

        public void SetStartPosition(Vector2 pos)
        {
            _vStartPosition = pos;
        }
        #endregion

        /// <summary>
        /// Retrieves te list of skills the character has based off of their class
        /// that is also valid based off of their current level.
        /// </summary>
        /// <returns></returns>
        public override List<CombatAction> GetCurrentSpecials()
        {
            List<CombatAction> rvList = new List<CombatAction>();

            rvList.AddRange(_class._liSpecialActionsList.FindAll(action => action.ReqLevel <= this.ClassLevel));

            return rvList;
        }

        public ClassedCharData SaveClassedCharData()
        {
            ClassedCharData advData = new ClassedCharData
            {
                armor = Item.SaveData(Armor.GetItem()),
                weapon = Item.SaveData(Weapon.GetItem()),
                level = _classLevel,
                xp = _iXP
            };

            return advData;
        }
        public void LoadClassedCharData(ClassedCharData data)
        {
            Armor.SetGear((Equipment)DataManager.GetItem(data.armor.itemID, data.armor.num));
            Weapon.SetGear((Equipment)DataManager.GetItem(data.weapon.itemID, data.weapon.num));
            _classLevel = data.level;
            _iXP = data.xp;
        }

        /// <summary>
        /// Structure that represents the slot for the character.
        /// Holds both the actual item and a temp item to compare against.
        /// </summary>
        public class GearSlot
        {
            EquipmentEnum _enumType;
            Equipment _eGear;
            Equipment _eTempGear;
            public GearSlot(EquipmentEnum type)
            {
                _enumType = type;
            }

            public void SetGear(Equipment e) { _eGear = e; }
            public void SetTemp(Equipment e) { _eTempGear = e; }

            public int GetStat(StatEnum stat)
            {
                int rv = 0;

                if (_eGear != null)
                {
                    rv += _eGear.GetStat(stat);
                }

                return rv;
            }
            public int GetTempStat(StatEnum stat)
            {
                int rv = 0;

                if (_eTempGear != null)
                {
                    rv += _eTempGear.GetStat(stat);
                }
                else if (_eGear != null)
                {
                    rv += _eGear.GetStat(stat);
                }

                return rv;
            }
            public Equipment GetItem() { return _eGear; }
            public bool IsEmpty() { return _eGear == null; }
        }
    }

    public class TravellingNPC : ClassedCombatant
    {
        protected int _iIndex;
        public int ID => _iIndex;

        protected int _iTotalMoneyEarnedReq = -1;

        protected int _iDaysToFirstArrival = 0;
        protected int _iArrivalPeriod = 0;
        protected int _iNextArrival = -1;

        protected bool _bShopIsOpen = false;
        protected int _iShopIndex = -1;

        protected List<int> _liRequiredBuildingIDs;
        protected Dictionary<int, int> _diRequiredObjectIDs;

        public bool Introduced = false;
        protected bool _bArrivedOnce = false;

        protected virtual void ImportBasics(int index, Dictionary<string, string> stringData, bool loadanimations = true)
        {
            _iIndex = index;

            Util.AssignValue(ref _bHover, "Hover", stringData);

            _diDialogue = DataManager.GetNPCDialogue(_iIndex);
            DataManager.GetTextData("Character", _iIndex, ref _sName, "Name");

            if (stringData.ContainsKey("Class"))
            {
                SetClass(DataManager.GetClassByIndex(int.Parse(stringData["Class"])));
                AssignStartingGear();
            }
            else { SetClass(new CharacterClass()); }

            _sPortrait = Util.GetPortraitLocation(_sPortraitFolder, "Villager", _iIndex.ToString("00"));

            if (stringData.ContainsKey("RequiredBuildingID"))
            {
                string[] args = Util.FindParams(stringData["RequiredBuildingID"]);
                foreach (string i in args)
                {
                    _liRequiredBuildingIDs.Add(int.Parse(i));
                }
            }

            if (stringData.ContainsKey("RequiredObjectID"))
            {
                string[] args = Util.FindParams(stringData["RequiredObjectID"]);
                foreach (string i in args)
                {
                    string[] split = i.Split('-');
                    _diRequiredObjectIDs[int.Parse(split[0])] = int.Parse(split[1]);
                }
            }

            Util.AssignValue(ref _iShopIndex, "ShopData", stringData);

            if (loadanimations)
            {
                List<AnimationData> liAnimationData;
                if (stringData.ContainsKey("Class")) { liAnimationData = LoadWorldAndCombatAnimations(stringData); }
                else { liAnimationData = LoadWorldAnimations(stringData); }

                LoadSpriteAnimations(ref _sprBody, liAnimationData, _sNPCFolder + "NPC_" + _iIndex.ToString("00"));
                PlayAnimationVerb(VerbEnum.Idle);
            }

            _bOnTheMap = !stringData.ContainsKey("Inactive");


            Util.AssignValue(ref _iDaysToFirstArrival, "FirstArrival", stringData);
            Util.AssignValue(ref _iArrivalPeriod, "ArrivalPeriod", stringData);
        }

        public virtual void RollOver() { }

        public override void OpenShop()
        {
            GUIManager.OpenMainObject(new HUDShopWindow(GameManager.DIShops[_iShopIndex].FindAll(m => m.Unlocked)));
        }

        protected bool RequirementsMet()
        {
            foreach (KeyValuePair<int, int> kvp in _diRequiredObjectIDs)
            {
                if (PlayerManager.GetNumberTownObjects(kvp.Key) < kvp.Value)
                {
                    return false;
                }
            }

            foreach (int i in _liRequiredBuildingIDs)
            {
                if (!PlayerManager.DIBuildInfo[i].Built)
                {
                    return false;
                }
            }

            //If there is a Money Earned Requirement and we have not reached it, fail the test
            if (_iTotalMoneyEarnedReq != -1 && _iTotalMoneyEarnedReq < PlayerManager.TotalMoneyEarned)
            {
                return false;
            }

            return true;
        }

        #region Travel Methods
        /// <summary>
        /// Counts down the days until the Villager's first arrival, or
        /// time to the next arrival if they do not live in town.
        /// </summary>
        /// <returns></returns>
        public virtual bool HandleTravelTiming()
        {
            bool rv = false;

            if (_iDaysToFirstArrival > 0)
            {
                rv = TravelTimingHelper(ref _iDaysToFirstArrival);
            }
            else if (_iNextArrival > 0)
            {
                rv = TravelTimingHelper(ref _iNextArrival);
            }

            return rv;
        }

        /// <summary>
        /// Given a timer, subtract one from the elapsed time and, if it has become 0
        /// reset the time to next arrival.
        /// </summary>
        /// <param name="arrivalPeriod"></param>
        /// <returns></returns>
        private bool TravelTimingHelper(ref int arrivalPeriod)
        {
            bool rv = --arrivalPeriod == 0;
            if (rv)
            {
                _iNextArrival = 0;
            }

            return rv;
        }

        /// <summary>
        /// Removes the NPC from the current map and moves them back
        /// to their home map and back to their spawn point.
        /// </summary>
        public virtual void MoveToSpawn() { }
        #endregion
    }

    public class Villager : TravellingNPC
    {
        protected int _iHouseBuildingID = -1;

        protected Dictionary<int, bool> _diCollection;

        private bool _bCanMarry = false;
        public bool CanBeMarried => _bCanMarry;
        private bool _bMarried = false;

        protected bool _bLivesInTown = false;
        public bool LivesInTown => _bLivesInTown;

        protected Dictionary<string, List<Dictionary<string, string>>> _diCompleteSchedule;         //Every day with a list of KVP Time/GoToLocations
        List<KeyValuePair<string, PathData>> _liTodayPathing = null;                             //List of Times with the associated pathing                                                     //List of Tiles to currently be traversing
        protected int _iScheduleIndex;

        public Villager()
        {
            _diCollection = new Dictionary<int, bool>();
        }

        //Copy Construcor for Cutscenes
        public Villager(Villager n)
        {
            _eActorType = ActorEnum.Villager;
            _iIndex = n.ID;
            _sName = n.Name;
            _diDialogue = n._diDialogue;
            _sPortrait = n.Portrait;

            _iBodyWidth = n._sprBody.Width;
            _iBodyHeight = n._sprBody.Height;
            _sprBody = new AnimatedSprite(n.BodySprite);
        }

        public Villager(int index, Dictionary<string, string> stringData, bool loadanimations = true) : this()
        {
            _eActorType = ActorEnum.Villager;
            _liRequiredBuildingIDs = new List<int>();
            _diRequiredObjectIDs = new Dictionary<int, int>();
            _diCompleteSchedule = new Dictionary<string, List<Dictionary<string, string>>>();
            _iScheduleIndex = 0;

            ImportBasics(index, stringData, loadanimations);
        }

        protected override void ImportBasics(int index, Dictionary<string, string> stringData, bool loadanimations = true)
        {
            base.ImportBasics(index, stringData, loadanimations);

            Util.AssignValue(ref _bCanMarry, "CanMarry", stringData);

            CanJoinParty = _bCanMarry;

            Util.AssignValue(ref _iHouseBuildingID, "HouseID", stringData);
            Util.AssignValue(ref _iTotalMoneyEarnedReq, "TotalMoneyEarnedReq", stringData);

            Util.AssignValue(ref _bLivesInTown, "InTown", stringData);

            if (stringData.ContainsKey("Collection"))
            {
                string[] vectorSplit = Util.FindParams(stringData["Collection"]);
                foreach (string s in vectorSplit)
                {
                    _diCollection.Add(int.Parse(s), false);
                }
            }

            Dictionary<string, List<string>> schedule = DataManager.GetSchedule("NPC_" + _iIndex.ToString("00"));
            if (schedule != null)
            {
                foreach (KeyValuePair<string, List<string>> kvp in schedule)
                {
                    List<Dictionary<string, string>> pathingData = new List<Dictionary<string, string>>();
                    foreach (string s in kvp.Value)
                    {
                        pathingData.Add(TaggedStringToDictionary(s));
                    }
                    _diCompleteSchedule.Add(kvp.Key, pathingData);
                }
            }
        }

        public override void Update(GameTime gTime)
        {
            //Only follow schedules ATM if they are active and not married
            if (_bOnTheMap && !_bMarried)
            {
                if (_liTodayPathing != null)
                {
                    string currTime = GameCalendar.GetTime();
                    //_scheduleIndex keeps track of which pathing route we're currently following.
                    //Running late code to be implemented later
                    if (_iScheduleIndex < _liTodayPathing.Count && ((_liTodayPathing[_iScheduleIndex].Key == currTime)))// || RunningLate(movementList[_scheduleIndex].Key, currTime)))
                    {
                        _liTilePath = _liTodayPathing[_iScheduleIndex++].Value.Path;
                    }
                }

                //Determine whether or not we are currently moving
                bool stillMoving = _liTilePath.Count > 0;

                //Call up to the base to handle normal Update methods
                //Movement is handled here
                base.Update(gTime);

                //If we ended out movement during the update, process any directional facting
                //And animations that may be requested
                if (stillMoving && _liTilePath.Count == 0)
                {
                    string direction = _liTodayPathing[_iScheduleIndex - 1].Value.Direction;
                    string animation = _liTodayPathing[_iScheduleIndex - 1].Value.Animation;
                    if (!string.IsNullOrEmpty(direction))
                    {
                        Facing = Util.ParseEnum<DirectionEnum>(direction);
                        PlayAnimation(VerbEnum.Idle, Facing);
                    }

                    if (!string.IsNullOrEmpty(animation))
                    {
                        _sprBody.PlayAnimation(animation);
                    }
                }
            }
        }

        public override void RollOver()
        {
            if (RequirementsMet() && CurrentMap != null) { _bLivesInTown = true; }

            if (LivesInTown || HandleTravelTiming())
            {
                ClearPath();
                MoveToSpawn();
                CalculatePathing();

                //Reset on Monday
                if (GameCalendar.DayOfWeek == 0)
                {
                    CanGiveGift = true;
                }
            }
            else if (CurrentMap != null)
            {
                CurrentMap.RemoveCharacter(this);
                CurrentMapName = string.Empty;
                _iNextArrival = _iArrivalPeriod;
            }
        }

        /// <summary>
        /// Returns the initial text for when the Actor is first talked to.
        /// </summary>
        /// <returns>The text string to display</returns>
        public override TextEntry GetOpeningText()
        {
            TextEntry rv = null;

            foreach (Task q in PlayerManager.TaskLog)
            {
                q.AttemptProgress(this);
            }

            if (!Introduced)
            {
                rv = GetDialogEntry("Introduction");
                Introduced = true;
            }
            else
            {
                if (!CheckTaskLog(ref rv))
                {
                    if (_bShopIsOpen) { rv = _diDialogue["ShopOpen"]; }
                    else if (!_bHasTalked) { rv = GetDailyDialogue(); }
                    else
                    {
                        rv = _diDialogue["Selection"];
                    }
                }
            }
            return rv;
        }

        #region Travel Methods
        public override bool HandleTravelTiming()
        {
            bool rv = false;

            if (!_bLivesInTown)
            {
                rv = base.HandleTravelTiming();
            }

            return rv;
        }

        /// <summary>
        /// Call this method to determine if a Villager has decided to stay in town
        /// </summary>
        /// <returns>True if villager wants to stay in town</returns>
        public bool ShouldIStayInTown()
        {
            bool rv = false;
            if (!LivesInTown)
            {
                if (RequirementsMet())
                {
                    if (_iDaysToFirstArrival > 0) { _iDaysToFirstArrival--; }
                    else if (_iDaysToFirstArrival == 0)
                    {
                        _bLivesInTown = true;
                        rv = true;
                    }
                }
            }

            return rv;
        }

        /// <summary>
        /// Quick call to see if the NPC's home is built. Returns false if they have no assigned home.
        /// </summary>
        protected bool IsHomeBuilt()
        {
            return _iHouseBuildingID != -1 && PlayerManager.DIBuildInfo[_iHouseBuildingID].Built;
        }

        /// <summary>
        /// Call to return the name of the map the Villager should return to on Rollover.
        /// 
        /// If the NPC is married, they spawn in the house, otherwise if their home is built, they spawn there.
        /// 
        /// Otherwise, they spawn in the Inn.
        /// </summary>
        /// <returns>The string name of the map to put the NPC on</returns>
        protected string GetSpawnMapName()
        {
            string rv = string.Empty;

            if (_bMarried) { rv = PlayerManager.PlayerHome.MapName; }
            else if (IsHomeBuilt()) { rv = PlayerManager.GetBuildingByID(_iHouseBuildingID).MapName; }
            else if (_iHouseBuildingID != -1) { rv = "mapInn_Upper_1"; }

            return rv;
        }

        /// <summary>
        /// Removes the NPC from the current map and moves them back
        /// to their home map and back to their spawn point.
        /// </summary>
        public override void MoveToSpawn()
        {
            _bOnTheMap = true;
            PlayerManager.RemoveFromParty(this);

            string mapName = GetSpawnMapName();

            if (!string.IsNullOrEmpty(mapName))
            {
                CurrentMap?.RemoveCharacterImmediately(this);
                CurrentMapName = mapName;
                RHMap map = MapManager.Maps[mapName];

                string strSpawn = string.Empty;
                if (IsHomeBuilt() || GetSpawnMapName() == MapManager.TownMapName) { strSpawn = "NPC_" + _iIndex.ToString("00"); }
                else if (GameManager.VillagersInTheInn < 3) { strSpawn = "NPC_Wait_" + ++GameManager.VillagersInTheInn; }

                Position = Util.SnapToGrid(map.GetCharacterSpawn(strSpawn));
                map.AddCharacterImmediately(this);
            }
        }
        #endregion

        #region Pathing Handlers
        public void CalculatePathing()
        {
            string currDay = GameCalendar.GetDayOfWeek();
            string currSeason = GameCalendar.GetSeason();
            string currWeather = EnvironmentManager.GetWeatherString();
            if (_diCompleteSchedule != null && _diCompleteSchedule.Count > 0)
            {
                List<Dictionary<string, string>> listPathingForDay = null;
                #region old
                //string searchVal = currSeason + currDay + currWeather;


                ////Search to see if there exists any pathing instructions for the day.
                ////If so, set the value of listPathingForDay to the list of times/locations
                //if (_diCompleteSchedule.ContainsKey(currSeason + currDay + currWeather))
                //{
                //    listPathingForDay = _diCompleteSchedule[currSeason + currDay + currWeather];
                //}
                //else if (_diCompleteSchedule.ContainsKey(currSeason + currDay))
                //{
                //    listPathingForDay = _diCompleteSchedule[currSeason + currDay];
                //}
                //else if (_diCompleteSchedule.ContainsKey(currDay))
                //{
                //    listPathingForDay = _diCompleteSchedule[currDay];
                //}
                #endregion

                //Iterate through each Key in the Schedule and see if the key values are valid.
                //Lowest index keys are highest priority. Special case keys need to be first.
                foreach (string s in _diCompleteSchedule.Keys)
                {
                    bool valid = true;
                    string[] args = Util.FindParams(s);
                    foreach (string arg in args)
                    {
                        string[] split = arg.Split(':');
                        if (split[0].Equals("Built"))
                        {
                            int buildingID = int.Parse(split[1]);
                            if (!PlayerManager.DIBuildInfo[buildingID].Built)
                            {
                                valid = false;
                                break;
                            }
                        }
                    }

                    if (valid)
                    {
                        listPathingForDay = _diCompleteSchedule[s];
                        break;
                    }
                }

                //If there is pathing instructions for the day, proceed
                //Key = Time, Value = goto Location
                if (listPathingForDay != null)
                {
                    List<KeyValuePair<string, PathData>> lTimetoTilePath = new List<KeyValuePair<string, PathData>>();
                    Vector2 start = Position;
                    string mapName = CurrentMapName;

                    TravelManager.NewTravelLog(_sName);
                    foreach (Dictionary<string, string> pathingData in listPathingForDay)
                    {
                        string timeKey = pathingData["Hour"] + ":" + pathingData["Minute"];
                        string targetLocation = pathingData["Location"];
                        string direction = string.Empty;
                        string animation = string.Empty;

                        Util.AssignValue(ref direction, "Dir", pathingData);
                        Util.AssignValue(ref animation, "Anim", pathingData);

                        List<RHTile> timePath;
                        //If the map we're currently on has the target location, pathfind to it.
                        //Otherwise, we need to pathfind to the map that does first.
                        if (MapManager.Maps[mapName].DictionaryCharacterLayer.ContainsKey(targetLocation))
                        {
                            timePath = TravelManager.FindPathToLocation(ref start, MapManager.Maps[mapName].DictionaryCharacterLayer[targetLocation]);
                        }
                        else
                        {
                            timePath = TravelManager.FindPathToOtherMap(targetLocation, ref mapName, ref start);
                        }

                        PathData data = new PathData(timePath, direction, animation);
                        lTimetoTilePath.Add(new KeyValuePair<string, PathData>(timeKey, data));
                    }
                    TravelManager.CloseTravelLog();

                    _liTodayPathing = lTimetoTilePath;
                }
            }
        }

        public bool RunningLate(string timeToGo, string currTime)
        {
            bool rv = false;
            string[] toGoSplit = timeToGo.Split(':');
            string[] curSplit = currTime.Split(':');

            int intTime = 0;
            int intCurrent = 0;
            if (toGoSplit.Length > 1 && curSplit.Length > 1)
            {
                if (int.TryParse(toGoSplit[0], out intTime) && int.TryParse(curSplit[0], out intCurrent) && intTime < intCurrent)
                {
                    rv = true;
                }
                else if (intTime == intCurrent && int.TryParse(toGoSplit[1], out intTime) && int.TryParse(curSplit[1], out intCurrent) && intTime < intCurrent)
                {
                    rv = true;
                }
            }

            return rv;
        }
        #endregion

        /// <summary>
        /// Flags the Villager that the shop open status is changing, as long as the Villager has Shop Data
        /// </summary>
        /// <param name="val">Whether the shop is open or closed</param>
        public void SetShopOpenStatus(bool val)
        {
            if (_iShopIndex != -1)
            {
                _bShopIsOpen = true;
            }
        }

        protected bool CheckTaskLog(ref TextEntry taskEntry)
        {
            bool rv = false;

            foreach (Task t in PlayerManager.TaskLog)
            {
                if (t.ReadyForHandIn && t.GoalNPC == this)
                {
                    string taskCompleteKey = string.Empty;
                    t.FinishTask(ref taskCompleteKey);

                    taskEntry = _diDialogue[taskCompleteKey];

                    rv = true;
                    break;
                }
            }

            return rv;
        }

        public override TextEntry Gift(Item item)
        {
            TextEntry rv = null;

            bool giftGiven = true;
            if (item != null)
            {
                if (_diCollection.ContainsKey(item.ItemID))
                {
                    FriendshipPoints += _diCollection[item.ItemID] ? 50 : 20;
                    rv = GetDialogEntry("Collection");
                    int index = new List<int>(_diCollection.Keys).FindIndex(x => x == item.ItemID);

                    _diCollection[item.ItemID] = true;
                    MapManager.Maps[GetSpawnMapName()].AddCollectionItem(item.ItemID, _iIndex, index);
                }
                else if (item.CompareSpecialType(SpecialItemEnum.Marriage))
                {
                    if (_bCanMarry)
                    {
                        if (FriendshipPoints > 200)
                        {
                            _bMarried = true;
                            rv = GetDialogEntry("MarriageYes");
                        }
                        else   //Marriage refused, re-add the item
                        {
                            giftGiven = false;
                            rv = GetDialogEntry("MarriageNo");
                        }
                    }
                    else
                    {
                        giftGiven = false;
                        rv = GetDialogEntry("MarriageNo");
                    }
                }
                else
                {
                    rv = GetDialogEntry("Gift");
                    FriendshipPoints += 1000;
                }
            }

            if (giftGiven)
            {
                item.Remove(1);
                CanGiveGift = false;
            }

            return rv;
        }

        public override TextEntry JoinParty()
        {
            TextEntry rv = null;
            if (_bMarried || CanJoinParty)
            {
                _bOnTheMap = false;
                PlayerManager.AddToParty(this);
                rv = GetDialogEntry("JoinPartyYes");
            }
            else
            {
                rv = GetDialogEntry("JoinPartyNo");
            }

            return rv;
        }

        /// <summary>
        /// Override for CombatActor's GoToIdle method to prevent checking against stats
        /// </summary>
        public override void GoToIdle()
        {
            PlayAnimationVerb(VerbEnum.Idle);
        }

        public VillagerData SaveData()
        {
            VillagerData npcData = new VillagerData()
            {
                npcID = ID,
                arrived = LivesInTown,
                arrivalDelay = _iDaysToFirstArrival,
                nextArrival = _iNextArrival,
                introduced = Introduced,
                friendship = FriendshipPoints,
                collection = new List<bool>(_diCollection.Values),
                married = _bMarried,
                canGiveGift = CanGiveGift,
                spokenKeys = _liSpokenKeys
            };

            if (_class != null) { npcData.classedData = SaveClassedCharData(); }

            return npcData;
        }
        public void LoadData(VillagerData data)
        {
            Introduced = data.introduced;
            _bLivesInTown = data.arrived;
            _iNextArrival = data.nextArrival;
            _iDaysToFirstArrival = data.arrivalDelay;
            FriendshipPoints = data.friendship;
            _bMarried = data.married;
            CanGiveGift = data.canGiveGift;

            if (_iNextArrival == 0)
            {
                MoveToSpawn();
            }

            if (_class != null) { LoadClassedCharData(data.classedData); }

            foreach (string s in data.spokenKeys)
            {
                _diDialogue[s].Spoken(this);
            }

            int index = 0;
            List<int> keys = new List<int>(_diCollection.Keys);
            foreach (int key in keys)
            {
                _diCollection[key] = data.collection[index++];
            }
        }

        /// <summary>
        /// Object representing the actionsthat need to be taken that are tied to a current path
        /// This includes the path being taken, the direction to face at the end, and the
        /// animation that mayneed to be played.
        /// </summary>
        private class PathData
        {
            List<RHTile> _liPathing;
            string _sDir;
            string _sAnimationName;

            public List<RHTile> Path => _liPathing;
            public string Direction => _sDir;
            public string Animation => _sAnimationName;

            public PathData(List<RHTile> path, string direction, string animation)
            {
                _liPathing = path;
                _sDir = direction;
                _sAnimationName = animation;
            }
        }
    }

    /// <summary>
    /// The Merchant is a class of Actor that appear periodically to both sell items
    /// to the player character as well as requesting specific items at a premium.
    /// </summary>
    public class Merchant : TravellingNPC
    {
        List<RequestItem> _liRequestItems;
        public Dictionary<Item, bool> DiChosenItems;
        private bool _bRequestsComplete = false;

        public Merchant(int index, Dictionary<string, string> stringData, bool loadanimations = true)
        {
            _eActorType = ActorEnum.Merchant;

            _liRequiredBuildingIDs = new List<int>();
            _diRequiredObjectIDs = new Dictionary<int, int>();

            _liRequestItems = new List<RequestItem>();
            DiChosenItems = new Dictionary<Item, bool>();
            ImportBasics(index, stringData, loadanimations);

            _bOnTheMap = false;
            _bShopIsOpen = true;
        }

        protected override void ImportBasics(int index, Dictionary<string, string> stringData, bool loadanimations = true)
        {
            base.ImportBasics(index, stringData, loadanimations);

            foreach (string s in Util.FindParams(stringData["RequestIDs"]))
            {
                RequestItem request = new RequestItem();
                string[] split = s.Split('-');
                request.ItemID = int.Parse(split[0]);
                request.Number = (split.Length > 1) ? int.Parse(split[1]) : 1;

                _liRequestItems.Add(request);
            }
        }

        public override void RollOver()
        {
            if (!_bOnTheMap)
            {
                if (RequirementsMet() && HandleTravelTiming())
                {
                    if (!_bArrivedOnce) { GameManager.MerchantQueue.Add(this); }
                    else { GameManager.MerchantQueue.Insert(0, this); }
                }
            }
            else
            {
                _bOnTheMap = false;
                _iNextArrival = _iArrivalPeriod;
                CurrentMap?.RemoveCharacterImmediately(this);
            }
        }

        public void ArriveInTown()
        {
            MoveToSpawn();
            _bArrivedOnce = true;

            DiChosenItems.Clear();
            List<RequestItem> copy = new List<RequestItem>(_liRequestItems);
            for (int i = 0; i < 3; i++)
            {
                int chosenValue = RHRandom.Instance().Next(0, copy.Count - 1);

                RequestItem request = copy[chosenValue];
                Item it = DataManager.GetItem(request.ItemID, request.Number);

                DiChosenItems[it] = false;
                copy.RemoveAt(chosenValue);
            }

            //ClearPath();
            //CalculatePathing();

            CanGiveGift = true;
        }

        /// <summary>
        /// Returns the initial text for when the Actor is first talked to.
        /// </summary>
        /// <returns>The text string to display</returns>
        public override TextEntry GetOpeningText()
        {
            TextEntry rv = null;

            if (!Introduced)
            {
                rv = GetDialogEntry("Introduction");
                Introduced = true;
            }
            else
            {
                if (!_bHasTalked) { rv = GetDailyDialogue(); }
                else
                {
                    rv = _diDialogue["Selection"];
                }
            }

            return rv;
        }

        public override TextEntry OpenRequests()
        {
            TextEntry rv = null;

            if (!_bRequestsComplete) { GUIManager.OpenMainObject(new HUDRequestWindow(_diDialogue["Requests"], this)); }
            else { rv = _diDialogue["RequestsComplete"]; }

            return rv;
        }

        /// <summary>
        /// Set the FinishedRequest flag to true.
        /// Ensures that we do not display the requests list after.
        /// </summary>
        public void FinishRequests()
        {
            _bRequestsComplete = true;
        }

        public override void MoveToSpawn()
        {
            _bOnTheMap = true;

            CurrentMapName = MapManager.TownMapName;
            MapManager.Maps[CurrentMapName].AddCharacterImmediately(this);

            Position = Util.SnapToGrid(GameManager.MarketPosition);
        }

        private struct RequestItem
        {
            public int ItemID;
            public int Number;
        }

        public MerchantData SaveData()
        {
            MerchantData npcData = new MerchantData()
            {
                npcID = ID,
                arrivalDelay = _iDaysToFirstArrival,
                timeToNextArrival = _iNextArrival,
                introduced = Introduced,
                spokenKeys = _liSpokenKeys,
                arrivedOnce = _bArrivedOnce
            };

            return npcData;
        }
        public void LoadData(MerchantData data)
        {
            Introduced = data.introduced;
            _iDaysToFirstArrival = data.arrivalDelay;
            _iNextArrival = data.timeToNextArrival;
            _bArrivedOnce = data.arrivedOnce;

            if (_iNextArrival == 0)
            {
                ArriveInTown();
            }

            foreach (string s in data.spokenKeys)
            {
                _diDialogue[s].Spoken(this);
            }
        }
    }

    public class PlayerCharacter : ClassedCombatant
    {
        AnimatedSprite _sprEyes;
        public AnimatedSprite EyeSprite => _sprEyes;
        AnimatedSprite _sprHair;
        public AnimatedSprite HairSprite => _sprHair;
        public Color HairColor { get; private set; } = Color.White;
        public int HairIndex { get; private set; } = 0;
        public int BodyType { get; private set; } = 1;
        public string BodyTypeStr => BodyType.ToString("00");

        protected override List<AnimatedSprite> GetSprites()
        {
            List<AnimatedSprite> liRv = new List<AnimatedSprite>() { _sprBody, _sprEyes, _sprHair, Body?.Sprite, Hat?.Sprite, Legs?.Sprite };
            liRv.RemoveAll(x => x == null);
            return liRv;
        }

        public Vector2 BodyPosition => _sprBody.Position;
        public override Vector2 Position
        {
            get { return new Vector2(_sprBody.Position.X, _sprBody.Position.Y + _sprBody.Height - TILE_SIZE); }
            set
            {
                Vector2 vPos = new Vector2(value.X, value.Y - _sprBody.Height + TILE_SIZE);
                foreach (AnimatedSprite spr in GetSprites()) { spr.Position = vPos; }
            }
        }
        public override Rectangle CollisionBox => ActiveMount != null ? ActiveMount.CollisionBox : new Rectangle((int)Position.X + 2, (int)Position.Y + 2, Width - 4, TILE_SIZE - 4);

        #region Clothing
        public Clothes Hat { get; private set; }
        public Clothes Body { get; private set; }
        Clothes Back;
        Clothes Hands;
        public Clothes Legs { get; private set; }
        Clothes Feet;
        #endregion

        public Pet ActivePet { get; private set; }
        public Mount ActiveMount { get; private set; }
        public bool Mounted => ActiveMount != null;

        public PlayerCharacter() : base()
        {
            _sName = PlayerManager.Name;
            _iBodyWidth = TILE_SIZE;
            _iBodyHeight = HUMAN_HEIGHT;

            HairColor = Color.Red;

            _liTilePath = new List<RHTile>();

            //Sets a default class so we can load and display the character to start
            SetClass(DataManager.GetClassByIndex(1));
            SetClothes((Clothes)DataManager.GetItem(int.Parse(DataManager.Config[6]["ItemID"])));

            _sprBody.SetColor(Color.White);
            _sprHair.SetColor(HairColor);

            SpdMult = NORMAL_SPEED;
        }

        public override void Draw(SpriteBatch spriteBatch, bool useLayerDepth = false)
        {
            base.Draw(spriteBatch, useLayerDepth);

            _sprEyes.Draw(spriteBatch, useLayerDepth);
            //_sprHair.Draw(spriteBatch, useLayerDepth);

            Body?.Sprite.Draw(spriteBatch, useLayerDepth);
            Hat?.Sprite.Draw(spriteBatch, useLayerDepth);
            Legs?.Sprite.Draw(spriteBatch, useLayerDepth);
        }

        private List<AnimationData> LoadPlayerAnimations(Dictionary<string, string> data)
        {
            List<AnimationData> rv;
            rv = LoadWorldAndCombatAnimations(data);

            AddToAnimationsList(ref rv, data, VerbEnum.UseTool, true, true);
            return rv;
        }

        /// <summary>
        /// Override for the ClassedCombatant SetClass methog. Calls the super method and then
        /// loads the appropriate sprites.
        /// </summary>
        /// <param name="x">The class to set</param>
        /// <param name="assignGear">Whether or not to assign starting gear</param>
        public override void SetClass(CharacterClass x)
        {
            base.SetClass(x);

            //Loads the Sprites for the players body for the appropriate class
            LoadSpriteAnimations(ref _sprBody, LoadPlayerAnimations(DataManager.PlayerAnimationData[x.ID]), string.Format(@"{0}Body_{1}", DataManager.FOLDER_PLAYER, BodyTypeStr));

            //Hair type has already been set either by default or by being allocated.
            SetHairType(HairIndex);

            //Loads the Sprites for the players body for the appropriate class
            LoadSpriteAnimations(ref _sprEyes, LoadPlayerAnimations(DataManager.PlayerAnimationData[x.ID]), string.Format(@"{0}Eyes", DataManager.FOLDER_PLAYER));
            //_sprEyes.SetDepthMod(EYE_DEPTH);
        }

        public void SetColor(AnimatedSprite sprite, Color c)
        {
            sprite.SetColor(c);
        }

        public void SetHairColor(Color c)
        {
            HairColor = c;
            SetColor(_sprHair, c);
        }
        public void SetHairType(int index)
        {
            HairIndex = index;
            //Loads the Sprites for the players hair animations for the class based off of the hair ID
            LoadSpriteAnimations(ref _sprHair, LoadWorldAndCombatAnimations(DataManager.PlayerAnimationData[CharacterClass.ID]), string.Format(@"{0}Hairstyles\Hair_{1}", DataManager.FOLDER_PLAYER, HairIndex));
            _sprHair.SetLayerDepthMod(HAIR_DEPTH);
        }

        public void MoveBy(int x, int y)
        {
            foreach (AnimatedSprite spr in GetSprites()) { spr.MoveBy(x, y); }
            ActiveMount?.BodySprite.MoveBy(x, y);
        }

        public override void PlayAnimation(AnimationEnum anim)
        {
            foreach (AnimatedSprite spr in GetSprites()) { spr.PlayAnimation(anim); }
        }
        public override void PlayAnimation(VerbEnum verb, DirectionEnum dir)
        {
            if(verb == VerbEnum.Walk && ActiveMount != null) { verb = VerbEnum.Idle; }

            foreach (AnimatedSprite spr in GetSprites()) { spr.PlayAnimation(verb, dir); }
        }

        public void SetScale(int scale = 1)
        {
            foreach (AnimatedSprite spr in GetSprites()) { spr.SetScale(scale); }
        }

        public void SetClothes(Clothes c)
        {
            if (c != null)
            {
                string clothingTexture = string.Format(@"Textures\Items\Gear\{0}\{1}", c.ClothesType.ToString(), c.TextureAnimationName);
                if (!c.GenderNeutral) { clothingTexture += ("_" + BodyTypeStr); }

                LoadSpriteAnimations(ref c.Sprite, LoadWorldAndCombatAnimations(DataManager.PlayerAnimationData[CharacterClass.ID]), clothingTexture);

                if (c.SlotMatch(ClothesEnum.Body)) { Body = c; }
                else if (c.SlotMatch(ClothesEnum.Hat))
                {
                    _sprHair.FrameCutoff = 9;
                    Hat = c;
                }
                else if (c.SlotMatch(ClothesEnum.Legs)) { Legs = c; }

                //MAR AWKWARD
                c.Sprite.Position = _sprBody.Position;
                c.Sprite.PlayAnimation(_sprBody.CurrentAnimation);
                c.Sprite.SetLayerDepthMod(0.004f);
            }
        }

        public void RemoveClothes(ClothesEnum c)
        {
            if (c.Equals(ClothesEnum.Body)) { Body = null; }
            else if (c.Equals(ClothesEnum.Hat))
            {
                _sprHair.FrameCutoff = 0;
                Hat = null;
            }
        }

        public void SetBodyType(int val)
        {
            BodyType = val;
            SetClass(_class);
            SetClothes(Hat);
            SetClothes(Body);
            SetClothes(Legs);
        }

        public void SetPet(Pet actor)
        {
            if (actor == null) { ActivePet.SetFollow(false); }

            ActivePet = actor;
            ActivePet?.SetFollow(true);
        }

        public void MountUp(Mount actor)
        {
            ActiveMount = actor;
            SpdMult = 1.5f;

            Position = ActiveMount.BodySprite.Position + new Vector2((ActiveMount.BodySprite.Width - BodySprite.Width) / 2, 8);

            foreach(AnimatedSprite spr in GetSprites())
            {
                spr.SetLinkedSprite(ActiveMount.BodySprite);
            }
        }
        public void Dismount()
        {
            Position = ActiveMount.BodySprite.Position + new Vector2(TILE_SIZE, 0);
            ActiveMount = null;
            SpdMult = NORMAL_SPEED;

            foreach (AnimatedSprite spr in GetSprites())
            {
                spr.SetLinkedSprite(null);
            }
        }
    }

    public class Spirit : TalkingActor
    {
        public override Rectangle HoverBox => new Rectangle((int)Position.X, (int)Position.Y, Width, Height);

        const float MIN_VISIBILITY = 0.05f;
        float _fVisibility;
        int _iID;
        string _sCondition;
        string _sText;
        public int SongID { get; } = 1;
        private string _sAwakenTrigger;

        private bool _bAwoken = false;
        public bool Triggered = false;

        public Spirit(Dictionary<string, string> stringData) : base()
        {
            _eActorType = ActorEnum.Spirit;
            _fVisibility = MIN_VISIBILITY;

            Util.AssignValue(ref _sName, "Name", stringData);
            Util.AssignValue(ref _iID, "SpiritID", stringData);
            Util.AssignValue(ref _sText, "Text", stringData);
            Util.AssignValue(ref _sCondition, "Condition", stringData);
            Util.AssignValue(ref _sAwakenTrigger, "AwakenTrigger", stringData);

            _sPortrait = Util.GetPortraitLocation(_sPortraitFolder, "Spirit", _iID.ToString("00"));

            _bOnTheMap = false;

            _iBodyWidth = TILE_SIZE;
            _iBodyHeight = TILE_SIZE + 2;
            List<AnimationData> liData = new List<AnimationData>();
            AddToAnimationsList(ref liData, stringData, VerbEnum.Idle);
            LoadSpriteAnimations(ref _sprBody, liData, _sNPCFolder + "Spirit_" + _iID);
        }

        public override void Update(GameTime gTime)
        {
            if (_bOnTheMap && _bAwoken)
            {
                _sprBody.Update(gTime);
                //if (_bActive)
                //{
                //    base.Update(gTime);
                //    if (!Triggered)
                //    {
                //        int max = TileSize * 13;
                //        int dist = 0;
                //        if (PlayerManager.CurrentMap == CurrentMapName && PlayerManager.PlayerInRangeGetDist(_spriteBody.Center.ToPoint(), max, ref dist))
                //        {
                //            float fMax = max;
                //            float fDist = dist;
                //            float percentage = (Math.Abs(dist - fMax)) / fMax;
                //            percentage = Math.Max(percentage, MIN_VISIBILITY);
                //            _fVisibility = 0.4f * percentage;
                //        }
                //    }
                //}
            }
        }

        public override void Draw(SpriteBatch spriteBatch, bool useLayerDepth = false)
        {
            if (_bOnTheMap && _bAwoken)
            {
                _sprBody.Draw(spriteBatch, useLayerDepth, _fVisibility);
            }
        }

        public void AttemptToAwaken(string triggerName)
        {
            if (_sAwakenTrigger.Equals(triggerName))
            {
                _bAwoken = true;
            }
        }
        public void CheckCondition()
        {
            bool active = false;
            string[] splitCondition = _sCondition.Split('/');
            foreach (string s in splitCondition)
            {
                if (s.Equals("Raining"))
                {
                    active = EnvironmentManager.IsRaining();
                }
                else if (s.Contains("Day"))
                {
                    active = !GameCalendar.IsNight();//s.Equals(GameCalendar.GetDayOfWeek());
                }
                else if (s.Equals("Night"))
                {
                    active = GameCalendar.IsNight();
                }

                if (!active) { break; }
            }

            _bOnTheMap = active;
            Triggered = false;
        }
        public override TextEntry GetOpeningText()
        {
            TextEntry rv = null;
            if (_bOnTheMap)
            {
                Triggered = true;
                _fVisibility = 1.0f;

                //string[] loot = DataManager.DiSpiritInfo[_sType].Split('/');
                //int arrayID = RHRandom.Instance().Next(0, loot.Length - 1);
                //InventoryManager.AddToInventory(int.Parse(loot[arrayID]));

                //_sText = Util.ProcessText(_sText.Replace("*", "*" + loot[arrayID] + "*"));
                // GUIManager.OpenTextWindow(_sText, this, true);
            }
            return rv;
        }
    }

    public class ShippingGremlin : Villager
    {
        private int _iRows = 4;
        private int _iCols = 10;
        private Item[,] _arrInventory;

        public override Vector2 Position
        {
            get
            {
                return new Vector2(_sprBody.Position.X, _sprBody.Position.Y + _sprBody.Height - TILE_SIZE);
            }
            set
            {
                _sprBody.Position = new Vector2(value.X, value.Y - _sprBody.Height + TILE_SIZE);
            }
        }

        public ShippingGremlin(int index, Dictionary<string, string> stringData)
        {
            _bLivesInTown = true;
            _liRequiredBuildingIDs = new List<int>();
            _diRequiredObjectIDs = new Dictionary<int, int>();
            _arrInventory = new Item[_iRows, _iCols];
            _eActorType = ActorEnum.ShippingGremlin;
            _iIndex = index;
            _iBodyWidth = 32;
            _iBodyHeight = 32;

            _diDialogue = DataManager.GetNPCDialogue(_iIndex);
            _sPortrait = Util.GetPortraitLocation(_sPortraitFolder, "Gremlin", _iIndex.ToString("00"));
            //_sPortrait = _sPortraitFolder + "WizardPortrait";
            DataManager.GetTextData("Character", _iIndex, ref _sName, "Name");

            Util.AssignValue(ref _iHouseBuildingID, "HouseID", stringData);

            _sprBody = new AnimatedSprite(_sNPCFolder + "NPC_" + _iIndex.ToString("00"));
            _sprBody.AddAnimation(AnimationEnum.ObjectIdle, 0, 0, _iBodyWidth, _iBodyHeight);
            _sprBody.AddAnimation(AnimationEnum.Action_One, 32, 0, _iBodyWidth, _iBodyHeight, 3, 0.1f);
            _sprBody.AddAnimation(AnimationEnum.Action_Finished, 128, 0, _iBodyWidth, _iBodyHeight);
            _sprBody.AddAnimation(AnimationEnum.Action_Two, 160, 0, _iBodyWidth, _iBodyHeight, 3, 0.1f);
            PlayAnimation(AnimationEnum.ObjectIdle);

            if (GameManager.ShippingGremlin == null) { GameManager.ShippingGremlin = this; }
        }

        public override void Update(GameTime gTime)
        {
            base.Update(gTime);
            if (IsCurrentAnimation(AnimationEnum.Action_One) && _sprBody.CurrentFrameAnimation.PlayCount == 1)
            {
                PlayAnimation(AnimationEnum.Action_Finished);
                PlayerManager.AllowMovement = true;
                base.Talk(false);
            }
            else if (IsCurrentAnimation(AnimationEnum.Action_Two) && _sprBody.CurrentFrameAnimation.PlayCount == 1)
            {
                PlayAnimation(AnimationEnum.ObjectIdle);
            }
        }

        public override void Draw(SpriteBatch spriteBatch, bool useLayerDepth = false)
        {
            base.Draw(spriteBatch, useLayerDepth);
        }

        /// <summary>
        /// When we talk to the ShippingGremlin, lock player movement and then play the open animation
        /// </summary>
        public override void ProcessRightButtonClick()
        {
            PlayerManager.AllowMovement = false;
            _sprBody.PlayAnimation(AnimationEnum.Action_One);
        }

        /// <summary>
        /// After done talking, play the close animation
        /// </summary>
        public override void StopTalking()
        {
            base.StopTalking();
            _sprBody.PlayAnimation(AnimationEnum.Action_Two);
        }

        public void OpenShipping()
        {
            GUIManager.OpenMainObject(new HUDInventoryDisplay(_arrInventory, GameManager.DisplayTypeEnum.Ship));
        }

        /// <summary>
        /// Iterate through every item in the Shipping Bin and calculate the
        /// sell price of each item. Add the total to the Player's inventory
        /// and then clear out the Shipping Bin.
        /// </summary>
        /// <returns></returns>
        public int SellAll()
        {
            int val = 0;
            foreach (Item i in _arrInventory)
            {
                if (i != null)
                {
                    val += i.SellPrice * i.Number;
                    PlayerManager.AddMoney(val);
                }
            }
            _arrInventory = new Item[_iRows, _iCols];

            return val;
        }
    }

    public class Summon : CombatActor
    {
        public override Vector2 Position
        {
            get
            {
                return new Vector2(_sprBody.Position.X + TILE_SIZE, _sprBody.Position.Y + _sprBody.Height - (TILE_SIZE * (_iSize + 1)));
            }
            set
            {
                _sprBody.Position = new Vector2(value.X - TILE_SIZE, value.Y - _sprBody.Height + (TILE_SIZE * (_iSize + 1)));
            }
        }

        ElementEnum _eElementType = ElementEnum.None;
        public ElementEnum Element => _eElementType;

        public override int Attack => _iMagStat;
        int _iMagStat;

        public bool Acted;

        public CombatActor linkedChar;
        private CombatAction _action;

        public Summon(int id, Dictionary<string, string> stringData)
        {
            _eActorType = ActorEnum.Summon;

            Util.AssignValue(ref _eElementType, "Element", stringData);

            _action = DataManager.GetActionByIndex(int.Parse(stringData["Ability"]));

            LoadSpriteAnimations(ref _sprBody, LoadWorldAndCombatAnimations(stringData), DataManager.FOLDER_SUMMONS + stringData["Texture"]);
        }

        public void SetStats(int magStat)
        {
            _iMagStat = magStat;
            _iStrength = magStat;
            _iDefense = magStat;
            _iVitality = magStat;
            _iMagic = magStat;
            _iResistance = magStat;
            _iSpeed = 10;

            CurrentHP = MaxHP;
        }

        public override void Update(GameTime gTime)
        {
            base.Update(gTime);

            ///When the Monster has finished playing the KO animation, let the CombatManager know so it can do any final actions
            if (IsCurrentAnimation(AnimationEnum.KO) && BodySprite.CurrentFrameAnimation.PlayCount == 1)
            {
                MapManager.RemoveCharacter(this);
            }
        }

        public override void KO()
        {
            base.KO();
            ClearTiles();
            _diConditions[ConditionEnum.KO] = true;
        }
        public void TakeTurn()
        {
            _action.AssignUser(this);
            _action.AssignTargetTile(BaseTile);
            CombatManager.SelectedAction = _action;
            CombatManager.ChangePhase(CombatManager.CmbtPhaseEnum.PerformAction);
        }

        /// <summary>
        /// Local override for the Summon. Unlinks the Summon if it dies.
        /// </summary>
        /// <param name="value">Damage dealt</param>
        /// <param name="bHarmful">Whether the modifier is harmful or helpful</param>
        public override void ModifyHealth(double value, bool bHarmful)
        {
            base.ModifyHealth(value, bHarmful);

            if (CurrentHP == 0)
            {
                linkedChar.UnlinkSummon();
            }
        }

        public override bool IsSummon() { return true; }
    }

    public class Adventurer : ClassedCombatant
    {
        #region Properties
        private enum AdventurerStateEnum { Idle, InParty, OnMission, AddToParty };
        private AdventurerStateEnum _eState;
        public AdventurerTypeEnum WorkerType { get; private set; }
        protected int _iPersonalID;
        public int PersonalID { get => _iPersonalID; }
        protected int _iID;
        public int WorkerID { get => _iID; }
        protected string _sAdventurerType;
        public Building Building { get; private set; }
        protected int _iDailyFoodReq;
        protected int _iCurrFood;
        protected int _iDailyItemID;
        public int DailyItemID => _iDailyItemID;
        protected Item _heldItem;
        protected int _iMood;
        private int _iResting;
        public int Mood { get => _iMood; }
        public Mission CurrentMission { get; private set; }

        public override bool OnTheMap => _eState == AdventurerStateEnum.Idle;
        #endregion

        public Adventurer(Dictionary<string, string> data, int id)
        {
            _iID = id;
            _iPersonalID = PlayerManager.GetTotalWorkers();
            //_eActorType = ActorEnum.Adventurer;
            ImportBasics(data, id);

            SetClass(DataManager.GetClassByIndex(_iID));
            AssignStartingGear();
            _sAdventurerType = CharacterClass.Name;

            _iCurrFood = 0;
            _heldItem = null;
            _iMood = 0;

            _eState = AdventurerStateEnum.Idle;

            _sName = _sAdventurerType.Substring(0, 1);
        }

        protected void ImportBasics(Dictionary<string, string> data, int id)
        {
            _iID = id;

            _sPortrait = Util.GetPortraitLocation(_sPortraitFolder, "Adventurer", id.ToString("00"));
            //_sPortrait = _sPortraitFolder + "WizardPortrait";

            WorkerType = Util.ParseEnum<AdventurerTypeEnum>(data["Type"]);
            _iDailyItemID = int.Parse(data["ItemID"]);
            _iDailyFoodReq = int.Parse(data["Food"]);

            LoadSpriteAnimations(ref _sprBody, LoadWorldAndCombatAnimations(data), _sMerchantFolder + "Adventurer_" + _iID);
        }

        public override void Draw(SpriteBatch spriteBatch, bool useLayerDepth = false)
        {
            if (_eState == AdventurerStateEnum.Idle || _eState == AdventurerStateEnum.AddToParty || (CombatManager.InCombat && _eState == AdventurerStateEnum.InParty))
            {
                base.Draw(spriteBatch, useLayerDepth);
                if (_heldItem != null)
                {
                    _heldItem.Draw(spriteBatch, new Rectangle(Position.ToPoint(), new Point(32, 32)), true);
                }
            }
        }

        public override bool CollisionContains(Point mouse)
        {
            bool rv = false;
            if (_eState == AdventurerStateEnum.Idle)
            {
                rv = base.CollisionContains(mouse);
            }
            return rv;
        }
        public override bool CollisionIntersects(Rectangle rect)
        {
            bool rv = false;
            if (_eState == AdventurerStateEnum.Idle)
            {
                rv = base.CollisionIntersects(rect);
            }
            return rv;
        }

        //public override string GetDialogEntry(string entry)
        //{
        //    return Util.ProcessText(DataManager.GetAdventurerDialogue(_iID, entry));
        //}

        public override TextEntry GetOpeningText()
        {
            return new TextEntry();//Name + ": " + DataManager.GetAdventurerDialogue(_iID, "Selection");
        }

        public override void StopTalking()
        {
            if (_eState == AdventurerStateEnum.AddToParty)
            {
                _eState = AdventurerStateEnum.InParty;
                PlayerManager.AddToParty(this);
            }
        }

        public int TakeItem()
        {
            int giveItem = -1;
            if (_heldItem != null)
            {
                giveItem = _heldItem.ItemID;
                _heldItem = null;
            }
            return giveItem;
        }

        public int WhatAreYouHolding()
        {
            if (_heldItem != null)
            {
                return _heldItem.ItemID;
            }
            return -1;
        }

        public void SetBuilding(Building b)
        {
            Building = b;
        }

        /// <summary>
        /// Called on rollover, if the WorldAdventurer is in a rest state, subtract one
        /// from the int. If they are currently on a mission, but the mission has been 
        /// completed by the MissionManager's rollover method, reset the state to idle,
        /// null the mission, and set _iResting to be half of the runtime of the Mission.
        /// </summary>
        /// <returns>True if the WorldAdventurer should make their daily item.</returns>
        public bool Rollover()
        {
            bool rv = false;

            if (_iResting > 0) { _iResting--; }

            switch (_eState)
            {
                case AdventurerStateEnum.Idle:
                    _iCurrentHP = MaxHP;
                    rv = true;
                    break;
                case AdventurerStateEnum.InParty:
                    if (GameManager.AutoDisband)
                    {
                        _eState = AdventurerStateEnum.Idle;
                    }
                    break;
                case AdventurerStateEnum.OnMission:
                    if (CurrentMission.Completed())
                    {
                        _eState = AdventurerStateEnum.Idle;
                        _iResting = CurrentMission.DaysToComplete / 2;
                        CurrentMission = null;
                    }
                    break;
            }

            return rv;
        }

        /// <summary>
        /// Creates the worers daily item in the inventory of the building's container.
        /// Need to set the InventoryManager to look at it, then clear it.
        /// </summary>
        public void MakeDailyItem()
        {
            if (_iDailyItemID != -1)
            {
                InventoryManager.InitContainerInventory(Building.BuildingChest.Inventory);
                InventoryManager.AddToInventory(_iDailyItemID, 1, false);
                InventoryManager.ClearExtraInventory();
            }
        }

        public string GetName()
        {
            return _sName;
        }

        /// <summary>
        /// Assigns the WorldAdventurer to the given mission.
        /// </summary>
        /// <param name="m">The mission they are on</param>
        public void AssignToMission(Mission m)
        {
            CurrentMission = m;
            _eState = AdventurerStateEnum.OnMission;
        }

        /// <summary>
        /// Cancels the indicated mission, returning the adventurer to their
        /// home building. Does not get called unless a mission has been canceled.
        /// </summary>
        public void EndMission()
        {
            _iResting = CurrentMission.DaysToComplete / 2;
            CurrentMission = null;
        }

        /// <summary>
        /// Gets a string representation of the WorldAdventurers current state
        /// </summary>
        public string GetStateText()
        {
            string rv = string.Empty;

            switch (_eState)
            {
                case AdventurerStateEnum.Idle:
                    rv = "Idle";
                    break;
                case AdventurerStateEnum.InParty:
                    rv = "In Party";
                    break;
                case AdventurerStateEnum.OnMission:
                    rv = "On Mission \"" + CurrentMission.Name + "\" days left: " + (CurrentMission.DaysToComplete - CurrentMission.DaysFinished).ToString();
                    break;
            }

            return rv;
        }

        /// <summary>
        /// WorldAdventurers are only available for missions if they're not on
        /// a mission and they are not currently in a resting state.
        /// </summary>
        public bool AvailableForMissions()
        {
            return (_eState != AdventurerStateEnum.OnMission && _iResting == 0);
        }

        public WorkerData SaveAdventurerData()
        {
            WorkerData workerData = new WorkerData
            {
                workerID = this.WorkerID,
                PersonalID = this.PersonalID,
                advData = base.SaveClassedCharData(),
                mood = this.Mood,
                name = this.Name,
                heldItemID = (this._heldItem == null) ? -1 : this._heldItem.ItemID,
                state = (int)_eState
            };

            return workerData;
        }
        public void LoadAdventurerData(WorkerData data)
        {
            _iID = data.workerID;
            _iPersonalID = data.PersonalID;
            _iMood = data.mood;
            _sName = data.name;
            _heldItem = DataManager.GetItem(data.heldItemID);
            _eState = (AdventurerStateEnum)data.state;

            base.LoadClassedCharData(data.advData);

            if (_eState == AdventurerStateEnum.InParty)
            {
                PlayerManager.AddToParty(this);
            }
        }
    }

    public class Pet : TalkingActor
    {
        public enum PetStateEnum { Alert, Idle, Leash, Wander };
        private PetStateEnum _eCurrentState = PetStateEnum.Wander;

        private int _iGatherZoneID;

        const double MOVE_COUNTDOWN = 2.5;
        public int ID { get; } = -1;
        private bool _bFollow = false;
        private bool _bIdleCooldown = false;

        private double _dCountdown = 0;

        public Pet(int id, Dictionary<string, string> stringData) : base()
        {
            ID = id;
            _eActorType = ActorEnum.Pet;

            _sPortrait = Util.GetPortraitLocation(_sPortraitFolder, "Adventurer", id.ToString("00"));
            DataManager.GetTextData("NPC", ID, ref _sName, "Name");

            Util.AssignValue(ref _iGatherZoneID, "ObjectID", stringData);

            List<AnimationData> liData = new List<AnimationData>();
            AddToAnimationsList(ref liData, stringData, VerbEnum.Walk);
            AddToAnimationsList(ref liData, stringData, VerbEnum.Idle);
            AddToAnimationsList(ref liData, stringData, VerbEnum.Action1);
            LoadSpriteAnimations(ref _sprBody, liData, _sCreatureFolder + "NPC_" + ID);
        }

        public override void Update(GameTime gTime)
        {
            base.Update(gTime);

            if (_bBumpedIntoSomething)
            {
                _bBumpedIntoSomething = false;
                SetMoveObj(Vector2.Zero);
            }

            if (_bFollow && !PlayerManager.PlayerInRange(CollisionBox.Center, TILE_SIZE * 8) && _eCurrentState != PetStateEnum.Leash)
            {
                if (!_sprBody.IsCurrentAnimation(VerbEnum.Action1, Facing))
                {
                    ChangeState(PetStateEnum.Alert);
                }
            }

            switch (_eCurrentState)
            {
                case PetStateEnum.Alert:
                    Alert();
                    break;
                case PetStateEnum.Idle:
                    Idle(gTime);
                    break;
                case PetStateEnum.Wander:
                    Wander(gTime);
                    break;
                case PetStateEnum.Leash:
                    Leash();
                    break;
            }
        }

        public override void ProcessRightButtonClick()
        {
            TextEntry text = DataManager.GetGameTextEntry(_bFollow ? "PetUnfollow" : "PetFollow");
            text.FormatText(_sName);
            GUIManager.OpenTextWindow(text, this, true);
        }

        private void Alert()
        {
            if (_sprBody.PlayedOnce)
            {
                ChangeState(PetStateEnum.Leash);
            }
        }

        private void Idle(GameTime gTime)
        {
            if (_dCountdown < 10 + RHRandom.Instance().Next(5)) { _dCountdown += gTime.ElapsedGameTime.TotalSeconds; }
            else
            {
                if (RHRandom.Instance().RollPercent(50))
                {
                    _dCountdown = 0;
                    _bIdleCooldown = true;
                    ChangeState(PetStateEnum.Wander);
                }
            }
        }

        private void Leash()
        {
            Vector2 delta = Position - PlayerManager.World.Position;
            HandleMove(Position - delta);

            if (PlayerManager.PlayerInRange(CollisionBox.Center, TILE_SIZE))
            {
                ChangeState(PetStateEnum.Wander);
            }
        }

        private void Wander(GameTime gTime)
        {
            if (_dCountdown < MOVE_COUNTDOWN + (RHRandom.Instance().Next(4) * 0.25)) { _dCountdown += gTime.ElapsedGameTime.TotalSeconds; }
            else if (MoveToLocation == Vector2.Zero)
            {
                _dCountdown = 0;

                if (!_bIdleCooldown && RHRandom.Instance().RollPercent(20))
                {
                    ChangeState(PetStateEnum.Idle);
                    return;
                }

                _bIdleCooldown = false;

                Vector2 moveTo = new Vector2(RHRandom.Instance().Next(8, 32), RHRandom.Instance().Next(8, 32));
                if (RHRandom.Instance().Next(1, 2) == 1) { moveTo.X *= -1; }
                if (RHRandom.Instance().Next(1, 2) == 1) { moveTo.Y *= -1; }

                SetMoveObj(Position + moveTo);
            }

            if (MoveToLocation != Vector2.Zero)
            {
                HandleMove(_vMoveTo);
            }
        }

        public void ChangeState(PetStateEnum state)
        {
            _eCurrentState = state;
            switch (state)
            {
                case PetStateEnum.Alert:
                    PlayAnimation(VerbEnum.Action1, Facing);
                    break;
                case PetStateEnum.Idle:
                    PlayAnimation(VerbEnum.Idle, Facing);
                    break;
                case PetStateEnum.Leash:
                    ChangeMovement(NORMAL_SPEED);
                    break;
                case PetStateEnum.Wander:
                    ChangeMovement(NPC_WALK_SPEED);
                    break;
            }
        }

        private void ChangeMovement(float speed)
        {
            SpdMult = speed;
            PlayAnimation(VerbEnum.Walk, Facing);
            SetMoveObj(Vector2.Zero);
            _dCountdown = 0;
        }

        public void SetFollow(bool value)
        {
            _bFollow = value;
        }

        public void SpawnInHome()
        {
            WorldObject obj = Util.GetRandomItem(PlayerManager.GetTownObjectsByID(_iGatherZoneID));
            if (obj == null)
            {
                if (CurrentMap == null) { MapManager.TownMap.AddCharacterImmediately(this); }
                else { MapManager.TownMap.AddCharacter(this); }
                Position = Util.GetRandomItem(MapManager.TownMap.FindFreeTiles()).Position;
            }
            else
            {
                List<RHTile> validTiles = new List<RHTile>();
                Point objLocation = obj.CollisionBox.Location;
                foreach (Vector2 v in Util.GetAllPointsInArea(objLocation.X - (3 * TILE_SIZE), objLocation.Y - (3 * TILE_SIZE), TILE_SIZE * 7, TILE_SIZE * 7, TILE_SIZE))
                {
                    RHTile t = obj.CurrentMap.GetTileByPixelPosition(v);
                    if (t != null && t.Passable() && (t.WorldObject == null || t.WorldObject.Walkable)) { validTiles.Add(t); }
                }

                obj.CurrentMap.AddCharacter(this);
                Position = Util.GetRandomItem(validTiles).Position;

                ChangeState(PetStateEnum.Wander);
            }
        }

        public void SpawnNearPlayer()
        {
            if (CurrentMap == null) { MapManager.CurrentMap.AddCharacterImmediately(this); }
            else { MapManager.CurrentMap.AddCharacter(this); }

            List<RHTile> validTiles = new List<RHTile>();
            Point playerLocation = PlayerManager.World.CollisionBox.Location;
            foreach (Vector2 v in Util.GetAllPointsInArea(playerLocation.X - (3 * TILE_SIZE), playerLocation.Y - (3 * TILE_SIZE), TILE_SIZE * 7, TILE_SIZE * 7, TILE_SIZE))
            {
                RHTile t = MapManager.CurrentMap.GetTileByPixelPosition(v);
                if (t != null && t.Passable() && (t.WorldObject == null || t.WorldObject.Walkable)) { validTiles.Add(t); }
            }

            Position = Util.GetRandomItem(validTiles).Position;

            ChangeState(PetStateEnum.Wander);
        }
    }

    public class Mount : WorldActor
    {
        public override Rectangle CollisionBox => new Rectangle((int)Position.X, (int)Position.Y, Width, TILE_SIZE);

        int _iStableID = -1;
        public int ID { get; } = -1;
        public Mount(int id, Dictionary<string, string> stringData) : base()
        {
            ID = id;
            _eActorType = ActorEnum.Mount;
            DataManager.GetTextData("NPC", ID, ref _sName, "Name");

            Util.AssignValue(ref _iBodyWidth, "Width", stringData);
            Util.AssignValue(ref _iBodyHeight, "Height", stringData);

            Util.AssignValue(ref _iStableID, "BuildingID", stringData);

            List<AnimationData> liData = new List<AnimationData>();
            AddToAnimationsList(ref liData, stringData, VerbEnum.Walk);
            LoadSpriteAnimations(ref _sprBody, liData, _sCreatureFolder + "NPC_" + ID);
        }

        public override void ProcessRightButtonClick()
        {
            if (!PlayerManager.World.Mounted)
            {
                PlayerManager.World.MountUp(this);
            }
        }

        public void SyncToPlayer()
        {
            AnimatedSprite playerSprite = PlayerManager.World.BodySprite;
            MapManager.CurrentMap.AddCharacter(this);
            Vector2 mod = new Vector2((playerSprite.Width - BodySprite.Width) / 2, BodySprite.Height - 8);
            Position = playerSprite.Position + mod; 
        }

        public void SpawnInHome()
        {
            RHMap stableMap = MapManager.Maps[PlayerManager.GetBuildingByID(_iStableID).MapName];
            stableMap.AddCharacter(this);
            Position = Util.GetRandomItem(stableMap.FindFreeTiles()).Position;
        }

        public bool CanEnterBuilding(string mapName)
        {
            bool rv = false;

            RHMap stableMap = MapManager.Maps[PlayerManager.GetBuildingByID(_iStableID).MapName];
            if (mapName.Equals(stableMap.Name))
            {
                rv = true;
            }
            return rv;
        }

        public bool StableBuilt() { return PlayerManager.IsBuilt(_iStableID); }
    }

    public class EnvironmentalActor : WorldActor
    {
        private bool _bFlee = false;
        private double _dNextPlay = 0;
        private double _dCountdown = 0;
        public int ID { get; } = -1;

        public EnvironmentalActor(int id, Dictionary<string, string> stringData)
        {
            ID = id;
            _eActorType = ActorEnum.Mount;
            _bIgnoreCollisions = true;
            _dNextPlay = 1 + SetRandom(4, 0.5);
            _iBodyHeight = TILE_SIZE;

            DataManager.GetTextData("NPC", ID, ref _sName, "Name");

            List<AnimationData> liData = new List<AnimationData>();
            AddToAnimationsList(ref liData, stringData, VerbEnum.Idle);
            AddToAnimationsList(ref liData, stringData, VerbEnum.Action1);
            AddToAnimationsList(ref liData, stringData, VerbEnum.Action2);
            LoadSpriteAnimations(ref _sprBody, liData, _sCreatureFolder + "NPC_" + ID);

            Facing = DirectionEnum.Down;
            PlayAnimation(VerbEnum.Idle);

            _sprBody.SetNextAnimation(Util.GetActorString(VerbEnum.Action1, Facing), Util.GetActorString(VerbEnum.Idle, Facing));
        }

        public override void Update(GameTime gTime)
        {
            base.Update(gTime);
            if (!_bFlee)
            {
                if (_dCountdown < _dNextPlay) { _dCountdown += gTime.ElapsedGameTime.TotalSeconds; }
                else
                {
                    _dNextPlay = 1 + SetRandom(4, 0.5);
                    _dCountdown = 0;
                    PlayAnimation(VerbEnum.Action1);
                }

                if (PlayerManager.PlayerInRange(_sprBody.Center, 80))
                {
                    _bFlee = true;
                    _dCountdown = 0;

                    PlayAnimation(VerbEnum.Action2);
                }
            }
            else
            {
                if (_dCountdown < 1) { _dCountdown += gTime.ElapsedGameTime.TotalSeconds; }
                else { _sprBody.SetLayerDepthMod(9999); }

                Position += new Vector2(-2, -2);

                if (Position.X < 0 || Position.Y < 0)
                {
                    CurrentMap.RemoveCharacter(this);
                }
            }
        }

        private double SetRandom(int max, double mod)
        {
            return RHRandom.Instance().Next(max) * mod;
        }
    }
}
