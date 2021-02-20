using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.CombatStuff;
using RiverHollow.Buildings;
using RiverHollow.Game_Managers;
using RiverHollow.GUIComponents.Screens;
using RiverHollow.Items;
using RiverHollow.Misc;
using RiverHollow.SpriteAnimations;
using RiverHollow.Tile_Engine;
using RiverHollow.Utilities;

using static RiverHollow.Game_Managers.GameManager;
using static RiverHollow.Game_Managers.DataManager;
using static RiverHollow.Game_Managers.TravelManager;
using static RiverHollow.Game_Managers.SaveManager;
using static RiverHollow.Items.WorldItem;
using static RiverHollow.GUIComponents.Screens.HUDMenu;
using static RiverHollow.GUIComponents.Screens.HUDMenu.HUDManagement;

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

        protected const int HUMAN_HEIGHT = (TileSize * 2) + 2;
        protected const float EYE_DEPTH = 0.001f;
        protected const float HAIR_DEPTH = 0.003f;

        protected static string _sVillagerFolder = DataManager.FOLDER_ACTOR + @"Villagers\";
        protected static string _sAdventurerFolder = DataManager.FOLDER_ACTOR + @"Adventurers\";
        protected static string _sPortraitFolder = DataManager.FOLDER_ACTOR + @"Portraits\";
        protected static string _sNPsCFolder = DataManager.FOLDER_ACTOR + @"NPCs\";

        protected ActorStateEnum _eMovementState = ActorStateEnum.Idle;

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

        protected int _iSpriteWidth;
        protected int _iSpriteHeight;

        protected int _iBodyWidth = TileSize;
        public int Width => _iBodyWidth;
        protected int _iBodyHeight = TileSize * 2;
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
            get {
                return new Vector2(_sprBody.Position.X, _sprBody.Position.Y + _sprBody.Height - (TileSize * _iSize));
            } //MAR this is fucked up
            set {
                _sprBody.Position = new Vector2(value.X, value.Y - _sprBody.Height + (TileSize * _iSize));
            }
        }

        public bool FollowingPath => _liTilePath.Count > 0;
        protected List<RHTile> _liTilePath;
        protected double _dEtherealCD;
        protected bool _bIgnoreCollisions;

        protected double _dCooldown = 0;

        public virtual Rectangle CollisionBox => new Rectangle((int)Position.X, (int)Position.Y, Width, TileSize);
        public virtual Rectangle HoverBox => new Rectangle((int)Position.X, (int)Position.Y - TileSize, Width, Height);

        protected bool _bActive = true;
        public virtual bool Active => _bActive;

        protected bool _bHover;

        float _fBaseSpeed = TileSize * 9;   //How many tiles/second to move
        public float BuffedSpeed => _fBaseSpeed * SpdMult;
        public float SpdMult = NPC_WALK_SPEED;

        protected int _iSize = 1;
        public int Size => _iSize;

        #endregion

        public WorldActor() : base()
        {
            _eActorType = ActorEnum.WorldCharacter;
            _iBodyWidth = TileSize;
            _iBodyHeight = HUMAN_HEIGHT;

            _iSpriteWidth = _iBodyWidth;
            _iSpriteHeight = _iBodyHeight;

            _liTilePath = new List<RHTile>();
        }

        public override void Draw(SpriteBatch spriteBatch, bool useLayerDepth = false)
        {
            if (_bActive)
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

        /// <summary>
        /// Creates a new Animatedsprite object for the given texture string, and adds
        /// all of the given animations to the new AnimatedSprite
        /// </summary>
        /// <param name="listAnimations">A list of AnimationData to add to the sprite</param>
        /// <param name="textureName">The texture name for the AnimatedSprite</param>
        protected void LoadSpriteAnimations(ref AnimatedSprite sprite, List<AnimationData> listAnimations, string textureName, bool combatSprite = false)
        {
            sprite = new AnimatedSprite(textureName, combatSprite);

            foreach (AnimationData data in listAnimations)
            {
                if (data.Directional)
                {
                    AddDirectionalAnimations(ref sprite, data, _iSpriteWidth, _iSpriteHeight, data.PingPong, data.BackToIdle);
                }
                else
                {
                    sprite.AddAnimation(data.Animation, data.XLocation, data.YLocation, _iSpriteWidth, _iSpriteHeight, data.Frames, data.FrameSpeed, data.PingPong);
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
            ActorStateEnum initialState = _eMovementState;
            if (direction.Length() != 0)
            {
                SetMovementState(ActorStateEnum.Walking);
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
            else { SetMovementState(ActorStateEnum.Idle); }

            if (initialState != _eMovementState || initialFacing != Facing)
            {
                PlayAnimationVerb((walk || CombatManager.InCombat) ? VerbEnum.Walk : VerbEnum.Idle);
            }
        }

        public void SetMovementState(ActorStateEnum e)
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

            float moveDist = UseMovement();
            if (moveDist >= 1)
            {
                //Determines how much of the needed position we're capable of moving in one movement
                Util.GetMoveSpeed(Position, target, moveDist, ref direction);

                //If we're following a path and there's more than one tile left, we don't want to cut
                //short on individual steps, so recalculate based on the next target
                float length = direction.Length();
                if (_liTilePath.Count > 1 && length < moveDist)
                {
                    _liTilePath.RemoveAt(0);

                    if (DoorCheck())
                    {
                        return;
                    }

                    //Recalculate for the next target
                    target = _liTilePath[0].Position;
                    Util.GetMoveSpeed(Position, target, moveDist, ref direction);
                }

                //Attempt to move
                if (!CheckMapForCollisionsAndMove(direction, _bIgnoreCollisions))
                {
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
        /// Determines how much of the movement needs to be given based off of 
        /// </summary>
        /// <param name="gTime"></param>
        public void AccumulateMovement(GameTime gTime)
        {
            double percent = gTime.ElapsedGameTime.TotalMilliseconds / 1000;    //The percentage of a whole second in this update frame
            _dAccumulatedMovement += BuffedSpeed * percent;
        }

        public float UseMovement()
        {
            double wholeNumber = 0;
            if (_dAccumulatedMovement >= 1)
            {
                wholeNumber = Math.Truncate(_dAccumulatedMovement);
               // _dAccumulatedMovement -= wholeNumber;
                _dAccumulatedMovement = 0;  //Need to reset to 0 to maintain a constant speed update
            }

            return (float)wholeNumber;
        }
        public void ClearAccumulatedMovement() {
            _dAccumulatedMovement = 0;
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

        public void SetMoveObj(Vector2 vec) {
            _vMoveTo = vec;
        }

        /// <summary>
        /// Sets the active status of the WorldActor
        /// </summary>
        /// <param name="value">Whether the actor is active or not.</param>
        public void Activate (bool value)
        {
            _bActive = value;
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

    public class TalkingActor : WorldActor
    {
        protected const int PortraitWidth = 160;
        protected const int PortraitHeight = 192;

        protected string _sPortrait;
        public string Portrait => _sPortrait;

        protected Rectangle _rPortrait;
        public Rectangle PortraitRectangle => _rPortrait;

        protected Dictionary<string, string> _diDialogue;

        public static List<int> FriendRange = new List<int> { 0, 10, 40, 100, 200, 600, 800, 1200, 1600, 2000 };
        public int FriendshipPoints = 0;

        protected bool _bHasTalked;

        public TalkingActor() : base()
        {
            _bCanTalk = true;
        }

        public virtual void StopTalking() { }

        /// <summary>
        /// Used when already talking to an NPC, gets the next dialog tag in the conversation
        /// and opens a new window for it.
        /// </summary>
        /// <param name="dialogTag">The dialog tag to talk with</param>
        public void Talk(string dialogTag)
        {
            string text = string.Empty;
            if (_diDialogue.ContainsKey(dialogTag))
            {
                text = _diDialogue[dialogTag];
            }
            text = Util.ProcessText(text, _sName);
            GUIManager.OpenTextWindow(text, this, true, true);
        }

        /// <summary>
        ///  Retrieves any opening text, processes it, then opens a text window
        /// </summary>
        /// <param name="facePlayer">Whether the NPC should face the player. Mainly used to avoid messing up a cutscene</param>
        public virtual void Talk(bool facePlayer = true)
        {
            string text = GetOpeningText();

            FacePlayer(true);

            text = Util.ProcessText(text, _sName);
            GUIManager.OpenTextWindow(text, this, true, true);
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

        public void TalkCutscene(string cutsceneLine)
        {
            string text = cutsceneLine;
            text = Util.ProcessText(text, _sName);
            GUIManager.OpenTextWindow(text, this, true, true);
        }

        /// <summary>
        /// Stand-in Virtual method to be overrriden. Should never get called.
        /// </summary>
        public virtual string GetOpeningText() { return "I have nothing to say."; }

        /// <summary>
        /// Retrieves the 'Selection' text for when an Actor with options is first talked to.
        /// 
        /// Removes entries that are not valid due to conditions not being met. If there are
        /// only two entries left, instead of providing the selection text, just call
        /// the default GetText method.
        /// </summary>
        /// <returns></returns>
        public string GetSelectionText()
        {
            string text = _diDialogue["Selection"];
            Util.ProcessText(text, _sName);

            string[] textFromData = Util.FindTags(text);
            string[] options = Util.FindParams(textFromData[1]);

            List<string> liCommands = RemoveEntries(options);

            //If there's only two entires left, Talk and Never Mind, then go straight to Talk
            string rv = string.Empty;
            if (liCommands.Count == 2)
            {
                
            }
            else
            {
                rv = textFromData[0] + "[";   //Puts back the pre selection text
                foreach (string s in liCommands)
                {
                    rv += s + "|";
                }
                rv = rv.Remove(rv.Length - 1);
                rv += "]";
            }

            return rv;
        }
        public virtual List<string> RemoveEntries(string[] options)
        {
            List<string> _liCommands = new List<string>();
            _liCommands.AddRange(options);
            return _liCommands;
        }

        /// <summary>
        /// Base method to get a line of dialog from the dialog dictionary.
        /// 
        /// Mostly used for the "Talk" parameter or if the TalkingActor has no other options.
        /// </summary>
        /// <returns>The dialog string for the entry.</returns>
        public string GetDailyDialogue()
        {
            _bHasTalked = true;
            List<string> keyPool = new List<string>();
            foreach (string s in _diDialogue.Keys)
            {
                int validation = 0;
                string[] values = Util.FindParams(s);
                foreach (string val in values)
                {
                    if (val.Equals(GameCalendar.GetWeatherString()))
                    {
                        validation++;
                    }
                    else if (val.StartsWith("Friend"))
                    {
                        string[] args = val.Split('-');
                        if(args.Length == 2)
                        {
                            if(int.TryParse(args[1], out int NPCID) && this.GetFriendshipLevel() >= NPCID)
                            {
                                validation++;
                            }
                        }
                        else if (args.Length == 3)
                        {
                            if (int.TryParse(args[1], out int NPCID) && int.TryParse(args[2], out int tempLevel) && DataManager.DiNPC[NPCID].GetFriendshipLevel() > tempLevel)
                            {
                                validation++;
                            }
                        }
                    }
                    else if (int.TryParse(val, out int ID))
                    {
                        validation++;
                    }
                }

                if (validation == values.Length)
                {
                    keyPool.Add(s);
                }
            }

            return GetDialogEntry(keyPool[RHRandom.Instance.Next(0, keyPool.Count -1)]);
        }

        /// <summary>
        /// Retrieves the specified entry from the _diDictionaryand calls Util.ProcessTexton it.
        /// </summary>
        /// <param name="entry">The key of the entry to get from the Dictionary</param>
        /// <returns>The processed string text for the entry </returns>
        public virtual string GetDialogEntry(string entry) {
            return Util.ProcessText(_diDialogue.ContainsKey(entry) ? Util.ProcessText(_diDialogue[entry], _sName) : string.Empty);
        }

        /// <summary>
        /// Handler for the chosen action in a GUITextSelectionWindow.
        /// Retrieve the next text based off of the Chosen Action from the GetDialogEntry
        /// and perform any required actions.
        /// </summary>
        /// <param name="chosenAction">The action to perform logic on</param>
        /// <param name="nextText">A reference to be filled out for the next text to display</param>
        /// <returns>False if was triggered and we want to close the text window</returns>
        public virtual bool HandleTextSelection(string chosenAction, ref string nextText)
        {
            bool rv = false;

            if (chosenAction.StartsWith("Talk")){
               //nextText = GetDailyDialogue();
            }
            else if (chosenAction.StartsWith("Quest"))
            {
                Quest q = GameManager.DiQuests[int.Parse(chosenAction.Remove(0, "Quest".Length))];
                PlayerManager.AddToQuestLog(q);
                nextText = GetDialogEntry("Quest" + q.QuestID);
            }
            else if (chosenAction.StartsWith("Donate"))
            {
                ((Villager)GameManager.CurrentNPC).FriendshipPoints += 40;
            }
            else if (chosenAction.StartsWith("NoDonate"))
            {
                ((Villager)GameManager.CurrentNPC).FriendshipPoints -= 1000;
            }
            else if (chosenAction.StartsWith("Cancel"))
            {
                GameManager.CurrentItem = null;
            }
            else
            {
                nextText = GetDialogEntry(chosenAction);
            }

            if (!string.IsNullOrEmpty(nextText)) { rv = true; }

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
        public RHTile BaseTile => _arrTiles[0,0];
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
            _eActorType = ActorEnum.CombatActor;
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
                pos.Y += (TileSize * _iSize);

                //Do not allow the bar to have less than 2 pixels, one for the border and one to display.
                double totalWidth = TileSize * Size;
                double percent = (double)CurrentHP / (double)MaxHP;
                int drawWidth = Math.Max((int)(totalWidth * percent), 2);

                DrawDisplayBar(spriteBatch, pos, texture, drawWidth, (int)totalWidth, 16, 0);

                if (MaxMP > 0)
                {
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
                    AccumulateMovement(gTime);
                    HandleMove(_vMoveTo);
                }
                else if (_liTilePath.Count > 0)
                {
                    AccumulateMovement(gTime);
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

        public void GoToIdle()
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

            if (RHRandom.Instance.Next(1, 100) <= (attacker.CritRating + critRating)) { dmg *= 2; }

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
                if (MapManager.CurrentMap.IsOutside && GameCalendar.IsRaining())
                {
                    if (element.Equals(ElementEnum.Lightning)) { modifiedDmg += (dmg * 1.2) - dmg; }
                    else if (element.Equals(ElementEnum.Fire)) { modifiedDmg += (dmg * 0.8) - dmg; }
                }
                else if (MapManager.CurrentMap.IsOutside && GameCalendar.IsSnowing())
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

                if(this == CombatManager.ActiveCharacter) { _bPause = true; }

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
            for(int i = 0; i <_iSize; i++)
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

            foreach(RHTile t in _arrTiles)
            {
                foreach(RHTile adjTile in t.GetAdjacentTiles())
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

        public void GetHP(ref int curr, ref int max)
        {
            curr = _iCurrentHP;
            max = MaxHP;
        }

        public void GetMP(ref int curr, ref int max)
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
            _eActorType = ActorEnum.Adventurer;
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
            if (Wrist.IsEmpty()) { Wrist.SetGear((Equipment) GetItem(_class.WristID));}
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

        public void GetXP(ref int curr, ref int max)
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
   
    public class Villager : ClassedCombatant
    {
        //Data for building structures
        Building _buildTarget;
        bool _bStartedBuilding;

        protected int _iIndex;
        public int ID  => _iIndex;
        protected int _iHouseBuildingID = -1;
        protected List<int> _liRequiredBuildingIDs;
        protected NPCTypeEnum _eNPCType;
        public NPCTypeEnum NPCType => _eNPCType;

        protected Dictionary<int, bool> _diCollection;
        public bool Introduced;
        public bool CanGiveGift = true;
        public bool ArrivedInTown = false;

        protected Dictionary<string, List<Dictionary<string, string>>> _diCompleteSchedule;         //Every day with a list of KVP Time/GoToLocations
        List<KeyValuePair<string, PathData>> _liTodayPathing = null;                             //List of Times with the associated pathing                                                     //List of Tiles to currently be traversing
        protected int _iScheduleIndex;

        public Villager() {
            _diCollection = new Dictionary<int, bool>();
        }

        //Copy Construcor for Cutscenes
        public Villager(Villager n)
        {
            _eActorType = ActorEnum.NPC;
            _iIndex = n.ID;
            _sName = n.Name;
            _diDialogue = n._diDialogue;
            _sPortrait = n.Portrait;
            _rPortrait = n._rPortrait;
            //_rPortrait = new Rectangle(0, 0, 48, 60);
            //_sPortrait = _sAdventurerFolder + "WizardPortrait";

            _iBodyWidth = n._sprBody.Width;
            _iBodyHeight = n._sprBody.Height;
            _sprBody = new AnimatedSprite(n.BodySprite);
        }

        public Villager(int index, Dictionary<string, string> stringData, bool loadanimations = true): this()
        {
            _eActorType = ActorEnum.NPC;
            _diCompleteSchedule = new Dictionary<string, List<Dictionary<string, string>>>();
            _iScheduleIndex = 0;
            _iIndex = index;

            Util.AssignValue(ref _bHover, "Hover", stringData);

            ImportBasics(stringData, loadanimations);
        }

        protected void ImportBasics(Dictionary<string, string> stringData, bool loadanimations = true)
        {
            _liRequiredBuildingIDs = new List<int>();
            _diDialogue = DataManager.GetNPCDialogue(_iIndex);
            DataManager.GetTextData("Character", _iIndex, ref _sName, "Name");

            _sPortrait = Util.GetPortraitLocation(_sPortraitFolder, "Villager", _iIndex.ToString("00"));
            //_sPortrait = _sPortraitFolder + "WizardPortrait";

            if (loadanimations)
            {
                LoadSpriteAnimations(ref _sprBody, LoadWorldAnimations(stringData), _sVillagerFolder + "NPC_" + _iIndex.ToString("00"));
                PlayAnimationVerb(VerbEnum.Idle);
            }

            _bActive = !stringData.ContainsKey("Inactive");
            if (stringData.ContainsKey("Type")) { _eNPCType = Util.ParseEnum<NPCTypeEnum>(stringData["Type"]); }
            if (stringData.ContainsKey("PortRow")) { _rPortrait = new Rectangle(0, 0, 48, 60); }
            //if (data.ContainsKey("HomeMap"))
            //{
            //    _iHouseBuildingID = data["HomeMap"];
            //    CurrentMapName = _iHouseBuildingID;
            //}
            Util.AssignValue(ref _iHouseBuildingID, "HouseID", stringData);
            Util.AssignValue(ref ArrivedInTown, "Arrived", stringData);
            if (stringData.ContainsKey("RequiredBuildingID"))
            {
                string[] args = Util.FindParams(stringData["RequiredBuildingID"]);
                foreach (string i in args)
                {
                    _liRequiredBuildingIDs.Add(int.Parse(i));
                }
            }

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
                string direction = _liTodayPathing[_iScheduleIndex-1].Value.Direction;
                string animation = _liTodayPathing[_iScheduleIndex-1].Value.Animation;
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

        /// <summary>
        /// Returns the initial text for when the Actor is first talked to.
        /// </summary>
        /// <returns>The text string to display</returns>
        public override string GetOpeningText()
        {
            string rv = string.Empty;

            foreach(Quest q in PlayerManager.QuestLog)
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
                if (!CheckQuestLogs(ref rv))
                {
                    if (!_bHasTalked) { rv = GetDailyDialogue(); }
                    else { rv = GetSelectionText(); }
                }
            }
            return rv;
        }

        /// <summary>
        /// Handler for the chosen action in a GUITextSelectionWindow.
        /// Retrieve the next text based off of the Chosen Action from the GetDialogEntry
        /// and perform any required actions.
        /// </summary>
        /// <param name="chosenAction">The action to perform logic on</param>
        /// <param name="nextText">A reference to be filled out for the next text to display</param>
        /// <returns>False if was triggered and we want to close the text window</returns>
        public override bool HandleTextSelection(string chosenAction, ref string nextText)
        {
            bool rv = false;
            rv = base.HandleTextSelection(chosenAction, ref nextText);

            if (!rv)
            {
                if (chosenAction.Equals("GiveGift"))
                {
                    GUIManager.OpenMainObject(new HUDInventoryDisplay(GameManager.DisplayTypeEnum.Gift));
                }
                else if (chosenAction.StartsWith("Quest"))
                {
                    Quest q = GameManager.DiQuests[int.Parse(chosenAction.Remove(0, "Quest".Length))];
                    PlayerManager.AddToQuestLog(q);
                    nextText = GetDialogEntry("Quest" + q.QuestID);
                }
                else if (chosenAction.StartsWith("Donate"))
                {
                    FriendshipPoints += 40;
                }
                else if (chosenAction.StartsWith("NoDonate"))
                {
                    FriendshipPoints -= 1000;
                }
                else if (chosenAction.StartsWith("ConfirmGift"))
                {
                    GUIManager.CloseMainObject();
                    nextText = Gift(GameManager.CurrentItem);
                }
            }

            if (!string.IsNullOrEmpty(nextText)) { rv = true; }

            return rv;
        }
        public override List<string> RemoveEntries(string[] options)
        {
            List<string> _liCommands = new List<string>();
            for (int i = 0; i < options.Length; i++)
            {
                bool removeIt = false;
                string s = options[i];

                if (s.Contains("%"))
                {
                    //Special checks are in the format %type:val% so, |%Friend:50%Join Party:Party| or |%Quest:1%Business:Quest1|
                    string[] specialParse = s.Split(new[] { '%' }, StringSplitOptions.RemoveEmptyEntries);
                    string[] specialVal = specialParse[0].Split(':');

                    int.TryParse(specialVal[1], out int val);

                    if (specialVal[0].Equals("Friend"))
                    {
                        removeIt = FriendshipPoints < val;
                    }
                    else if (specialVal[0].Equals("Quest"))
                    {
                        Quest newQuest = GameManager.DiQuests[val];
                        removeIt = PlayerManager.QuestLog.Contains(newQuest) || newQuest.ReadyForHandIn || newQuest.Finished || !newQuest.CanBeGiven();
                    }

                    s = s.Remove(s.IndexOf(specialParse[0]) - 1, specialParse[0].Length + 2);
                }
                else
                {
                    if (!CanGiveGift && s.Contains("GiveGift"))
                    {
                        removeIt = true;
                    }
                }

                if (!removeIt)
                {
                    _liCommands.Add(s);
                }
            }

            return _liCommands;
        }

        protected void CheckForArrival()
        {
            if (!ArrivedInTown && _liRequiredBuildingIDs.Count > 0)
            {
                bool arrived = true;
                foreach (int i in _liRequiredBuildingIDs)
                {
                    if (!PlayerManager.HaveBuiltBuildingID(i))
                    {
                        arrived = false;
                        break;
                    }
                }

                ArrivedInTown = arrived;
            }
        }

        public virtual void RollOver()
        {
            if (!_bStartedBuilding && _buildTarget != null)
            {
                _bStartedBuilding = true;
            }

            //If we failed to move the NPC to a building location, because there was none
            //Add the NPC to their home map
            if (!MoveToBuildingLocation())
            {
                CheckForArrival();
                if (ArrivedInTown)
                {
                    MoveToSpawn();
                 //   CalculatePathing();
                }
            }
        }

        /// <summary>
        /// Removes the NPC from the current map and moves them back
        /// to their home map and back to their spawn point.
        /// </summary>
        public void MoveToSpawn()
        {
            string mapName = GetMapName();

            if (!string.IsNullOrEmpty(mapName))
            {
                CurrentMap?.RemoveCharacterImmediately(this);
                CurrentMapName = mapName;
                RHMap map = MapManager.Maps[mapName];
                string Spawn = "NPC_" + _iIndex.ToString("00");

                Position = Util.SnapToGrid(map.GetCharacterSpawn(Spawn));
                map.AddCharacterImmediately(this);
            }
        }

        protected string GetMapName()
        {
            string rv = string.Empty;

            if (PlayerManager.HaveBuiltBuildingID(_iHouseBuildingID)) { rv = PlayerManager.GetBuildingByID(_iHouseBuildingID).MapName; }
            else { }

            return rv;
        }

        public void CalculatePathing()
        {
            string currDay = GameCalendar.GetDayOfWeek();
            string currSeason = GameCalendar.GetSeason();
            string currWeather = GameCalendar.GetWeatherString();
            if (_diCompleteSchedule != null && _diCompleteSchedule.Count > 0)
            {
                string searchVal = currSeason + currDay + currWeather;
                List<Dictionary<string, string>> listPathingForDay = null;

                //Search to see if there exists any pathing instructions for the day.
                //If so, set the value of listPathingForDay to the list of times/locations
                if (_diCompleteSchedule.ContainsKey(currSeason + currDay + currWeather))
                {
                    listPathingForDay = _diCompleteSchedule[currSeason + currDay + currWeather];
                }
                else if (_diCompleteSchedule.ContainsKey(currSeason + currDay))
                {
                    listPathingForDay = _diCompleteSchedule[currSeason + currDay];
                }
                else if (_diCompleteSchedule.ContainsKey(currDay))
                {
                    listPathingForDay = _diCompleteSchedule[currDay];
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

        protected bool CheckQuestLogs(ref string questCompleteText)
        {
            bool rv = false;

            foreach (Quest q in PlayerManager.QuestLog)
            {
                if (q.ReadyForHandIn && q.GoalNPC == this)
                {
                    q.FinishQuest(ref questCompleteText);

                    questCompleteText = _diDialogue[questCompleteText];

                    rv = true;
                    break;
                }
            }

            return rv;
        }

        public virtual string Gift(Item item)
        {
            string rv = string.Empty;
            if (item != null)
            {
                item.Remove(1);
                CanGiveGift = false;

                if (_diCollection.ContainsKey(item.ItemID))
                {
                    FriendshipPoints += _diCollection[item.ItemID] ? 50 : 20;
                    rv = GetDialogEntry("Collection");
                    int index = new List<int>(_diCollection.Keys).FindIndex( x => x == item.ItemID);

                    _diCollection[item.ItemID] = true;
                    MapManager.Maps["mapHouseNPC" + _iIndex].AddCollectionItem(item.ItemID, _iIndex, index);
                }
                else
                {
                    rv = GetDialogEntry("Gift");
                    FriendshipPoints += 1000;
                }

                //if (item.IsMap() && NPCType == Villager.NPCTypeEnum.Ranger)
                //{
                //    rv = GetDialogEntry("Adventure");
                //    DungeonManagerOld.LoadNewDungeon((AdventureMap)item);
                //}
            }
            return rv;
        }

        public bool IsEligible() { return _eNPCType == NPCTypeEnum.Eligible; }

        /// <summary>
        /// Assigns the building for the mason to build
        /// If there is no building, also unset the building flag because he's finished.
        /// </summary>
        /// <param name="b">The building to build</param>
        public void SetBuildTarget(Building b, bool startBuilding = false)
        {
            _buildTarget = b;
            if (b == null)
            {
                _bStartedBuilding = false;
            }
            else
            {
                _bStartedBuilding = startBuilding;
            }
        }

        /// <summary>
        /// Use to determine whether the mason is currently building and nees to follow
        /// nonstandardlogic
        /// </summary>
        private bool IsBuilding()
        {
            return _bStartedBuilding && _buildTarget != null;
        }

        /// <summary>
        /// Check to see if the NPC is currently responsible for building anything and,
        /// if so, move them to the appropriate map and position.
        /// </summary>
        /// <returns>True if they are buildingand have been moved.</returns>
        private bool MoveToBuildingLocation()
        {
            bool rv = false;
            if (IsBuilding())
            {
                rv = true;
                CurrentMap.RemoveCharacter(this);
                RHMap map = MapManager.Maps[MapManager.HomeMap];
                Position = Util.SnapToGrid(_buildTarget.MapPosition + _buildTarget.BuildFromPosition);
                map.AddCharacterImmediately(this);
            }

            return rv;
        }

        public NPCData SaveData()
        {
            NPCData npcData = new NPCData()
            {
                npcID = ID,
                introduced = Introduced,
                friendship = FriendshipPoints,
                collection = new List<bool>(_diCollection.Values)
            };

            return npcData;
        }
        public void LoadData(NPCData data)
        {
            Introduced = data.introduced;
            FriendshipPoints = data.friendship;

            LoadCollection(data.collection);

            MoveToBuildingLocation();
        }

        /// <summary>
        /// Iterates through the already loaded list of collection keys, then assigns the saved value from teh given data.
        /// </summary>
        /// <param name="data"></param>
        protected void LoadCollection(List<bool> data)
        {
            int index = 0;
            List<int> keys = new List<int>(_diCollection.Keys);
            foreach (int key in keys)
            {
                _diCollection[key] = data[index++];
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
    public class ShopKeeper : Villager
    {
        private bool _bIsOpen;
        protected List<Merchandise> _liMerchandise;
        public List<Merchandise> Buildings { get => _liMerchandise; }

        public ShopKeeper(int index, Dictionary<string, string> stringData) : base(index, stringData)
        {
            _eNPCType = NPCTypeEnum.Shopkeeper;
            _liMerchandise = new List<Merchandise>();

            if (stringData.ContainsKey("ShopData"))
            {
                foreach (Dictionary<string, string> di in DataManager.GetShopData(stringData["ShopData"]))
                {
                    _liMerchandise.Add(new Merchandise(di));
                }
            }
        }

        public override string GetOpeningText()
        {
            string rv = string.Empty;
            if (Introduced && !CheckQuestLogs(ref rv) && _bIsOpen)
            {
                rv = _diDialogue["ShopOpen"];
            }
            else if (string.IsNullOrEmpty(rv))  //For if the QuestLogs check actually caught something
            {
                rv = base.GetOpeningText();
            }
            return rv;
        }

        public void SetOpen(bool val)
        {
            _bIsOpen = true;
        }

        /// <summary>
        /// Handler for the chosen action in a GUITextSelectionWindow.
        /// Retrieve the next text based off of the Chosen Action from the GetDialogEntry
        /// and perform any required actions.
        /// </summary>
        /// <param name="chosenAction">The action to perform logic on</param>
        /// <param name="nextText">A reference to be filled out for the next text to display</param>
        /// <returns>False if was triggered and we want to close the text window</returns>
        public override bool HandleTextSelection(string chosenAction, ref string nextText)
        {
            bool rv = false;
            rv = base.HandleTextSelection(chosenAction, ref nextText);

            if (!rv)
            {
                List<Merchandise> _liMerchandise = new List<Merchandise>();
                if (chosenAction.Equals("BuyBuildings"))
                {
                    foreach (Merchandise m in this._liMerchandise)
                    {
                        if ((m.MerchType == Merchandise.ItemType.Building || m.MerchType == Merchandise.ItemType.Upgrade) && m.Activated()) { _liMerchandise.Add(m); }
                    }

                    GUIManager.OpenMainObject(new HUDPurchaseBuildings(_liMerchandise));
                    GameManager.ClearGMObjects();
                }
                else if (chosenAction.Equals("BuyWorkers"))
                {
                    foreach (Merchandise m in this._liMerchandise)
                    {
                        if (m.MerchType == Merchandise.ItemType.Adventurer && m.Activated()) { _liMerchandise.Add(m); }
                    }
                    GUIManager.OpenMainObject(new HUDPurchaseWorkers(_liMerchandise));
                }
                else if (chosenAction.Equals("Missions"))
                {
                    GUIManager.OpenMainObject(new HUDMissionWindow());
                }
                else if (chosenAction.Equals("BuyItems"))
                {
                    foreach (Merchandise m in this._liMerchandise)
                    {
                        if (m.MerchType == Merchandise.ItemType.Item && m.Activated()) { _liMerchandise.Add(m); }
                    }
                    GUIManager.OpenMainObject(new HUDPurchaseItems(_liMerchandise));
                }
                else if (chosenAction.Equals("SellWorkers"))
                {
                    HUDManagement s = new HUDManagement();
                    s.Sell();
                    GUIManager.OpenMainObject(s);
                    GameManager.ClearGMObjects();
                }
                else if (chosenAction.Equals("Move"))
                {
                    RiverHollow.HomeMapPlacement();
                    GameManager.ClearGMObjects();
                    GameManager.MoveBuilding();
                }
                else if (chosenAction.Equals("UpgradeBuilding"))
                {
                    HUDManagement m = new HUDManagement(ActionTypeEnum.Upgrade);
                    GUIManager.OpenMainObject(m);
                    GameManager.ClearGMObjects();
                }
                else if (chosenAction.Equals("Destroy"))
                {
                    RiverHollow.HomeMapPlacement();
                    GameManager.ClearGMObjects();
                    GameManager.DestroyBuilding();
                }
            }

            if (!string.IsNullOrEmpty(nextText)) { rv = true; }

            return rv;
        }

        public class Merchandise
        {
            public string UniqueData { get; }
            public enum ItemType { Building, Adventurer, Item, Upgrade }
            public ItemType MerchType;
            public int MerchID { get; } = -1;
            string _sDescription;
            public int MoneyCost { get; }
            int _iQuestReq = -1;

            List<KeyValuePair<int, int>> _items; //item, then num required
            public List<KeyValuePair<int, int>> RequiredItems { get => _items; }

            public Merchandise(Dictionary<string, string> stringData)
            {
                _items = new List<KeyValuePair<int, int>>();

                MerchType = Util.ParseEnum<ItemType>(stringData["Type"]);
                if (stringData.ContainsKey("WorkerID")) { MerchID = int.Parse(stringData["WorkerID"]); }
                else if (stringData.ContainsKey("BuildingID")) { MerchID = int.Parse(stringData["BuildingID"]); }
                else if (stringData.ContainsKey("ItemID"))
                {
                    //Some items may have unique data so only parse the first entry
                    //tag is ItemID to differentiate the tag from in the GUI ItemData Manager
                    string[] itemData = stringData["ItemID"].Split('-');
                    MerchID = int.Parse(itemData[0]);
                    if (itemData.Length > 1) { UniqueData = itemData[1]; }
                }

 
                MoneyCost = int.Parse(stringData["Cost"]);

                if (stringData.ContainsKey("Text")) { _sDescription = stringData["Text"]; }
                if (stringData.ContainsKey("QuestReq")) { _iQuestReq = int.Parse(stringData["QuestReq"]); }

                if (stringData.ContainsKey("Requires"))
                {
                    string[] reqItems = Util.FindParams(stringData["Requires"]);
                    foreach (string str in reqItems)
                    {
                        string[] itemsSplit = str.Split('-');
                        _items.Add(new KeyValuePair<int, int>(int.Parse(itemsSplit[0]), int.Parse(itemsSplit[1])));
                    }
                }

                if (MerchType == ItemType.Item)
                {
                    string[] itemData = stringData["ItemID"].Split('-');
                    if (itemData.Length > 1) { UniqueData = itemData[1]; }
                }
            }

            public bool Activated()
            {
                bool rv = false;
                rv = _iQuestReq == -1 || GameManager.DiQuests[_iQuestReq].Finished;
                return rv;
            }
        }
    }

    public class EligibleNPC : Villager
    {
        /// <summary>
        /// As in the base, we need to calculate the Actor's position based off of the Sprite's position.
        /// However, there is a new complication in that there are mandatory buffers of one TileSize
        /// on both the Left, Right, and Bottom of the Sprite.
        /// </summary>
        public override Vector2 Position
        {
            get
            {
                return new Vector2(_sprBody.Position.X + TileSize, _sprBody.Position.Y + _sprBody.Height - (TileSize * 2));
            }
            set
            {
                _sprBody.Position = new Vector2(value.X - TileSize, value.Y - _sprBody.Height + (TileSize * 2));
            }
        }

        public bool Married;
        public bool CanJoinParty { get; private set; } = true;

        public EligibleNPC(int index, Dictionary<string, string> data) : base(index, data, false)
        {
            _eNPCType = NPCTypeEnum.Eligible;

            if (data.ContainsKey("Class"))
            {
                SetClass(DataManager.GetClassByIndex(int.Parse(data["Class"])));
                AssignStartingGear();
            }

            _iSpriteWidth = TileSize * 3;
            _iSpriteHeight = TileSize * 3;
            LoadSpriteAnimations(ref _sprBody, LoadWorldAndCombatAnimations(data), _sVillagerFolder + "NPC_" + _iIndex.ToString("00"));
            PlayAnimationVerb(VerbEnum.Idle);
        }

        public override void Update(GameTime gTime)
        {
            if (_bActive && !Married)   //Just for now
            {
                base.Update(gTime);
            }
        }

        /// <summary>
        /// Handler for the chosen action in a GUITextSelectionWindow.
        /// Retrieve the next text based off of the Chosen Action from the GetDialogEntry
        /// and perform any required actions.
        /// </summary>
        /// <param name="chosenAction">The action to perform logic on</param>
        /// <param name="nextText">A reference to be filled out for the next text to display</param>
        /// <returns>False if was triggered and we want to close the text window</returns>
        public override bool HandleTextSelection(string chosenAction, ref string nextText)
        {
            bool rv = false;
            rv = base.HandleTextSelection(chosenAction, ref nextText);

            if (!rv)
            {
                if (chosenAction.Equals("Party"))
                {
                    if (Married || CanJoinParty)
                    {
                        JoinParty();
                        nextText = GetDialogEntry("JoinPartyYes");
                    }
                    else
                    {
                        nextText = GetDialogEntry("JoinPartyNo");
                    }
                }
            }

            if (!string.IsNullOrEmpty(nextText)) { rv = true; }

            return rv;
        }

        public override void RollOver()
        {
            CheckForArrival();

            if (ArrivedInTown) { 
                CurrentMap?.RemoveCharacter(this);
                RHMap map = MapManager.Maps[Married ? "mapManor" : GetMapName()];
                string Spawn = Married ? "Spouse" : "NPC_" + _iIndex.ToString("00");

                Position = Util.SnapToGrid(map.GetCharacterSpawn(Spawn));
                map.AddCharacter(this);

                _bActive = true;
                PlayerManager.RemoveFromParty(this);

                //Reset on Monday
                if (GameCalendar.DayOfWeek == 0)
                {
                    CanJoinParty = true;
                    CanGiveGift = true;
                }

             //   CalculatePathing();
            }
        }

        public override string Gift(Item item)
        {
            string rv = string.Empty;
            if (item != null)
            {
                item.Remove(1);

                if (item.CompareSpecialType(SpecialItemEnum.Marriage))
                {
                    if (FriendshipPoints > 200)
                    {
                        Married = true;
                        rv = GetDialogEntry("MarriageYes");
                    }
                    else   //Marriage refused, readd the item
                    {
                        item.Add(1);
                        InventoryManager.AddToInventory(item);
                        rv = GetDialogEntry("MarriageNo");
                    }
                }
                else
                {
                    rv = base.Gift(item);
                }
            }

            return rv;
        }

        public void JoinParty()
        {
            _bActive = false;
            CanJoinParty = false;
            PlayerManager.AddToParty(((EligibleNPC)this));
        }

        public new EligibleNPCData SaveData()
        {
            EligibleNPCData npcData = new EligibleNPCData()
            {
                npcData = base.SaveData(),
                married = Married,
                canJoinParty = CanJoinParty,
                canGiveGift = CanGiveGift,
                classedData = SaveClassedCharData()
            };

            return npcData;
        }
        public void LoadData(EligibleNPCData data)
        {
            Introduced = data.npcData.introduced;
            FriendshipPoints = data.npcData.friendship;
            Married = data.married;
            CanJoinParty = data.canJoinParty;
            CanGiveGift = data.canGiveGift;
            LoadClassedCharData(data.classedData);

            LoadCollection(data.npcData.collection);

            if (Married)
            {
                MapManager.Maps[GetMapName()].RemoveCharacter(this);
                MapManager.Maps["mapManor"].AddCharacter(this);
                Position = MapManager.Maps["mapManor"].GetCharacterSpawn("Spouse");
            }
        }
    }

    public class Adventurer : ClassedCombatant
    {
        #region Properties

        /// <summary>
        /// As in the base, we need to calculate the Actor's position based off of the Sprite's position.
        /// However, there is a new complication in that there are mandatory buffers of one TileSize
        /// on both the Left, Right, and Bottom of the Sprite.
        /// </summary>
        public override Vector2 Position
        {
            get
            {
                return new Vector2(_sprBody.Position.X + TileSize, _sprBody.Position.Y + _sprBody.Height - (TileSize * 2));
            }
            set
            {
                _sprBody.Position = new Vector2(value.X - TileSize, value.Y - _sprBody.Height + (TileSize * 2));
            }
        }

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

        public override bool Active => _eState == AdventurerStateEnum.Idle;
        #endregion

        public Adventurer(Dictionary<string, string> data, int id)
        {
            _iID = id;
            _iPersonalID = PlayerManager.GetTotalWorkers();
            _eActorType = ActorEnum.Adventurer;
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

            _rPortrait = new Rectangle(0, 0, 48, 60);
            _sPortrait = Util.GetPortraitLocation(_sPortraitFolder, "Adventurer", id.ToString("00"));
            //_sPortrait = _sPortraitFolder + "WizardPortrait";

            WorkerType = Util.ParseEnum<AdventurerTypeEnum>(data["Type"]);
            _iDailyItemID = int.Parse(data["ItemID"]);
            _iDailyFoodReq = int.Parse(data["Food"]);

            _iSpriteWidth = TileSize * 3;
            _iSpriteHeight = TileSize * 3;
            LoadSpriteAnimations(ref _sprBody, LoadWorldAndCombatAnimations(data), _sAdventurerFolder + "Adventurer_" + _iID, true);
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

        public override string GetDialogEntry(string entry)
        {
            return Util.ProcessText(DataManager.GetAdventurerDialogue(_iID, entry), _sName);
        }
        
        public override string GetOpeningText()
        {
            return Name + ": " + DataManager.GetAdventurerDialogue(_iID, "Selection");
        }

        /// <summary>
        /// Handler for the chosen action in a GUITextSelectionWindow.
        /// Retrieve the next text based off of the Chosen Action from the GetDialogEntry
        /// and perform any required actions.
        /// </summary>
        /// <param name="chosenAction">The action to perform logic on</param>
        /// <param name="nextText">A reference to be filled out for the next text to display</param>
        /// <returns>False if was triggered and we want to close the text window</returns>
        public override bool HandleTextSelection(string chosenAction, ref string nextText)
        {
            bool rv = false;
            rv = base.HandleTextSelection(chosenAction, ref nextText);

            if (!rv)
            {
                if (chosenAction.Equals("Party"))
                {
                    _eState = AdventurerStateEnum.AddToParty;
                    nextText = GetDialogEntry("JoinParty");
                }
            }

            if (!string.IsNullOrEmpty(nextText)) {
                _iMood++;
                rv = true;
            }

            return rv;
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

            switch(_eState) {
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

            switch (_eState) {
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

            if (_eState == AdventurerStateEnum.InParty) {
                PlayerManager.AddToParty(this);
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
            get { return new Vector2(_sprBody.Position.X + TileSize, _sprBody.Position.Y + _sprBody.Height - (TileSize * 2)); }
            set
            {
                Vector2 vPos = new Vector2(value.X - TileSize, value.Y - _sprBody.Height + (TileSize * 2));
                foreach(AnimatedSprite spr in GetSprites()) { spr.Position = vPos; }
            }
        }
        public Clothes Hat { get; private set; }
        public Clothes Body { get; private set; }
        Clothes Back;
        Clothes Hands;
        public Clothes Legs { get; private set; }
        Clothes Feet;

        public PlayerCharacter() : base()
        {
            _sName = PlayerManager.Name;
            _iBodyWidth = TileSize;
            _iBodyHeight = HUMAN_HEIGHT;

            _iSpriteWidth = TileSize * 3;
            _iSpriteHeight = TileSize * 3;

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

            //float bodyDepth = _sprBody.Position.Y + _sprBody.CurrentFrameAnimation.FrameHeight + (Position.X / 100);
            _sprEyes.Draw(spriteBatch, useLayerDepth);
            //_sprHair.Draw(spriteBatch, useLayerDepth, 1.0f, bodyDepth);

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
            LoadSpriteAnimations(ref _sprBody, LoadPlayerAnimations(DataManager.PlayerAnimationData[x.ID]), string.Format(@"{0}Body_{1}", DataManager.FOLDER_PLAYER, BodyTypeStr), true);

            //Hair type has already been set either by default or by being allocated.
            SetHairType(HairIndex);

            //Loads the Sprites for the players body for the appropriate class
            LoadSpriteAnimations(ref _sprEyes, LoadPlayerAnimations(DataManager.PlayerAnimationData[x.ID]), string.Format(@"{0}Eyes", DataManager.FOLDER_PLAYER), true);
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
            LoadSpriteAnimations(ref _sprHair, LoadWorldAndCombatAnimations(DataManager.PlayerAnimationData[CharacterClass.ID]), string.Format(@"{0}Hairstyles\Hair_{1}", DataManager.FOLDER_PLAYER, HairIndex), true);
            _sprHair.SetDepthMod(HAIR_DEPTH);
        }

        public void MoveBy(int x, int y)
        {
            foreach (AnimatedSprite spr in GetSprites()) { spr.MoveBy(x, y); }
        }

        public override void PlayAnimation(AnimationEnum anim)
        {
            foreach (AnimatedSprite spr in GetSprites()) { spr.PlayAnimation(anim); }
        }
        public override void PlayAnimation(VerbEnum verb, DirectionEnum dir)
        {
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
                c.Sprite.SetDepthMod(0.004f);
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

            _bActive = false;

            _iBodyWidth = TileSize;
            _iBodyHeight = TileSize + 2;
            List<AnimationData> liData = new List<AnimationData>();
            AddToAnimationsList(ref liData, DataManager.DiSpiritInfo[_iID], VerbEnum.Idle);
            LoadSpriteAnimations(ref _sprBody, liData, _sNPsCFolder + "Spirit_" + _iID);
        }

        public override void Update(GameTime gTime)
        {
            if (_bActive && _bAwoken)
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
            if (_bActive && _bAwoken)
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
                    active = GameCalendar.IsRaining();
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

            _bActive = active;
            Triggered = false;
        }
        public override string GetOpeningText()
        {
            string rv = string.Empty;
            if (_bActive)
            {
                Triggered = true;
                _fVisibility = 1.0f;

                //string[] loot = DataManager.DiSpiritInfo[_sType].Split('/');
                //int arrayID = RHRandom.Instance.Next(0, loot.Length - 1);
                //InventoryManager.AddToInventory(int.Parse(loot[arrayID]));

                //_sText = Util.ProcessText(_sText.Replace("*", "*" + loot[arrayID] + "*"));
                GUIManager.OpenTextWindow(_sText, this, true);
            }
            return rv;
        }
    }

    public class ShippingGremlin : Villager
    {
        private int _iRows = 4;
        private int _iCols = 10;
        private Item[,] _arrInventory;
 
        public ShippingGremlin(int index, Dictionary<string, string> stringData)
        {
            _liRequiredBuildingIDs = new List<int>();
            _arrInventory = new Item[_iRows, _iCols];
            _eActorType = ActorEnum.ShippingGremlin;
            _iIndex = index;
            _iBodyWidth = 32;
            _iBodyHeight = 32;

            _iSpriteWidth = _iBodyWidth;
            _iSpriteHeight = _iBodyHeight;

            _diDialogue = DataManager.GetNPCDialogue(_iIndex);
            _sPortrait = Util.GetPortraitLocation(_sPortraitFolder, "Gremlin", _iIndex.ToString("00"));
            //_sPortrait = _sPortraitFolder + "WizardPortrait";
            DataManager.GetTextData("Character", _iIndex, ref _sName, "Name");

            CurrentMapName = MapManager.HomeMap;
            //if (stringData.ContainsKey("HomeMap"))
            //{
            //    _iHouseBuildingID = stringData["HomeMap"];
            //    CurrentMapName = _iHouseBuildingID;
            //}

            _sprBody = new AnimatedSprite(_sVillagerFolder + "NPC_" + _iIndex.ToString("00"));
            _sprBody.AddAnimation(AnimationEnum.ObjectIdle, 0, 0, _iBodyWidth, _iBodyHeight);
            _sprBody.AddAnimation(AnimationEnum.ObjectAction1, 32, 0, _iBodyWidth, _iBodyHeight, 3, 0.1f);
            _sprBody.AddAnimation(AnimationEnum.ObjectActionFinished, 128, 0, _iBodyWidth, _iBodyHeight);
            _sprBody.AddAnimation(AnimationEnum.ObjectAction2, 160, 0, _iBodyWidth, _iBodyHeight, 3, 0.1f);
            PlayAnimation(AnimationEnum.ObjectIdle);

            if(GameManager.ShippingGremlin == null) { GameManager.ShippingGremlin = this; }
        }

        public override void Update(GameTime gTime)
        {
            base.Update(gTime);
            if (IsCurrentAnimation(AnimationEnum.ObjectAction1) && _sprBody.CurrentFrameAnimation.PlayCount == 1)
            {
                PlayAnimation(AnimationEnum.ObjectActionFinished);
                PlayerManager.AllowMovement = true;
                base.Talk(false);
            }
            else if (IsCurrentAnimation(AnimationEnum.ObjectAction2) && _sprBody.CurrentFrameAnimation.PlayCount == 1)
            {
                PlayAnimation(AnimationEnum.ObjectIdle);
            }
        }

        /// <summary>
        /// Handler for the chosen action in a GUITextSelectionWindow.
        /// Retrieve the next text based off of the Chosen Action from the GetDialogEntry
        /// and perform any required actions.
        /// </summary>
        /// <param name="chosenAction">The action to perform logic on</param>
        /// <param name="nextText">A reference to be filled out for the next text to display</param>
        /// <returns>False if was triggered and we want to close the text window</returns>
        public override bool HandleTextSelection(string chosenAction, ref string nextText)
        {
            bool rv = false;
            rv = base.HandleTextSelection(chosenAction, ref nextText);

            if (!rv)
            {
                if (chosenAction.Equals("ShipGoods"))
                {
                    GUIManager.OpenMainObject(new HUDInventoryDisplay(_arrInventory, GameManager.DisplayTypeEnum.Ship));
                }
                else if(chosenAction.Equals("Cancel"))
                {
                    _sprBody.PlayAnimation(AnimationEnum.ObjectAction2);
                }
            }

            if (!string.IsNullOrEmpty(nextText)) { rv = true; }

            return rv; 
        }

        /// <summary>
        /// When we talk to the ShippingGremlin, lock player movement and then play the open animation
        /// </summary>
        /// <param name="facePlayer">Whether to face the player, pointless here</param>
        public override void Talk(bool facePlayer = false)
        {
            PlayerManager.AllowMovement = false;
            _sprBody.PlayAnimation(AnimationEnum.ObjectAction1);
        }

        /// <summary>
        /// After done talking, play the close animation
        /// </summary>
        public override void StopTalking()
        {
            _sprBody.PlayAnimation(AnimationEnum.ObjectAction2);
        }

        public int SellAll()
        {
            int val = 0;
            foreach (Item i in _arrInventory)
            {
                if (i != null)
                {
                    val += i.SellPrice * i.Number;
                    PlayerManager.AddMoney(i.SellPrice);
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
                return new Vector2(_sprBody.Position.X + TileSize, _sprBody.Position.Y + _sprBody.Height - (TileSize * (_iSize + 1)));
            }
            set
            {
                _sprBody.Position = new Vector2(value.X - TileSize, value.Y - _sprBody.Height + (TileSize * (_iSize + 1)));
            }
        }

        public ElementEnum Element { get; } = ElementEnum.None;

        public override int Attack => _iMagStat;
        int _iMagStat;

        public bool Acted;

        public CombatActor linkedChar;
        private CombatAction _action;

        public Summon(int id, Dictionary<string, string> stringData)
        {
            _eActorType = ActorEnum.Summon;

            if (stringData.ContainsKey("Element"))
            {
                Element = Util.ParseEnum<ElementEnum>(stringData["Element"]);
            }
            _action = DataManager.GetActionByIndex(int.Parse(stringData["Ability"]));

            _iSpriteWidth = TileSize * (_iSize + 2);
            _iSpriteHeight = TileSize * (_iSize + 2);
            LoadSpriteAnimations(ref _sprBody, LoadWorldAndCombatAnimations(stringData), DataManager.FOLDER_SUMMONS + stringData["Texture"], true);
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
}
