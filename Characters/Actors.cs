using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.Actors.CombatStuff;
using RiverHollow.Buildings;
using RiverHollow.Game_Managers;
using RiverHollow.Game_Managers.GUIObjects;
using RiverHollow.GUIComponents.Screens;
using RiverHollow.GUIObjects;
using RiverHollow.Misc;
using RiverHollow.SpriteAnimations;
using RiverHollow.Tile_Engine;
using RiverHollow.WorldObjects;
using System;
using System.Collections.Generic;
using static RiverHollow.Game_Managers.GameManager;
using static RiverHollow.Game_Managers.GUIObjects.HUDMenu;
using static RiverHollow.Game_Managers.GUIObjects.HUDMenu.HUDManagement;
using static RiverHollow.Game_Managers.DataManager;
using static RiverHollow.WorldObjects.Clothes;
using static RiverHollow.Game_Managers.TravelManager;
using static RiverHollow.WorldObjects.WorldItem;
using static RiverHollow.Game_Managers.SaveManager;

namespace RiverHollow.Actors
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
        protected const int HUMAN_HEIGHT = (TileSize * 2) + 2;
        protected const float EYE_DEPTH = 0.001f;
        protected const float HAIR_DEPTH = 0.003f;

        protected static string _sVillagerFolder = DataManager.FOLDER_ACTOR + @"Villagers\";
        protected static string _sAdventurerFolder = DataManager.FOLDER_ACTOR + @"Adventurers\";
        protected static string _sNPsCFolder = DataManager.FOLDER_ACTOR + @"NPCs\";

        public enum ActorEnum { Actor, Adventurer, CombatActor, Mob, Monster, NPC, ShippingGremlin, Spirit, WorldCharacter};
        protected ActorEnum _eActorType = ActorEnum.Actor;
        public ActorEnum ActorType => _eActorType;

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

        protected int _iWidth;
        public int Width => _iWidth;
        protected int _iHeight;
        public int Height => _iHeight; 
        public int SpriteWidth => _sprBody.Width;
        public int SpriteHeight => _sprBody.Height;

        protected bool _bCanTalk = false;
        public bool CanTalk => _bCanTalk;

        public Actor() { }

        public virtual void Update(GameTime gTime)
        {
            _sprBody.Update(gTime);
        }

        public virtual void Draw(SpriteBatch spriteBatch, bool useLayerDepth = false)
        {
            _sprBody.Draw(spriteBatch, useLayerDepth);
        }

        public virtual void SetName(string text)
        {
            _sName = text;
        }

        public virtual void PlayAnimation(AnimationEnum verb) { _sprBody.PlayAnimation(verb); }
        public virtual void PlayAnimation(VerbEnum verb, DirectionEnum action) { _sprBody.PlayAnimation(verb, action); }

        /// <summary>
        /// Adds a set of animations to the indicated Sprite for the given verb for each direction.
        /// </summary>
        /// <param name="sprite">Reference to the Sprite to add the animation to</param>
        /// <param name="verb">The Verb to add, such as Attack, Hurt, etc</param>
        /// <param name="firstX">The X-Position of the first frame</param>
        /// <param name="firstY">The X-Position of the first Y. Should never move</param>
        /// <param name="width">Frame width</param>
        /// <param name="height">Frame height</param>
        /// <param name="frames">Number of frames</param>
        /// <param name="frameSpeed">The speed for each frame</param>
        /// <param name="increment">The number of frames in width to skip between each animation.</param>
        /// <param name="pingpong">Whether the animation pingpong or not</param>
        /// <returns>The amount of pixels this animation sequence has crawled</returns>
        protected int AddDirectionalAnimations(ref AnimatedSprite sprite, AnimationData data, int width, int height, bool pingpong = false)
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

            return xCrawl;
        }

        protected void RemoveDirectionalAnimations(ref AnimatedSprite sprite, VerbEnum verb)
        {
            sprite.RemoveAnimation(verb, DirectionEnum.Down);
            sprite.RemoveAnimation(verb, DirectionEnum.Right);
            sprite.RemoveAnimation(verb, DirectionEnum.Up);
            sprite.RemoveAnimation(verb, DirectionEnum.Left);
        }

        public bool Contains(Point x) { return _sprBody.BoundingBox.Contains(x); }
        public bool AnimationFinished() { return _sprBody.PlayedOnce && _sprBody.IsAnimating; }
        public bool IsCurrentAnimation(AnimationEnum val) { return _sprBody.IsCurrentAnimation(val); }
        public bool IsCurrentAnimation(VerbEnum verb, DirectionEnum dir) { return _sprBody.IsCurrentAnimation(verb, dir); }
        public bool IsAnimating() { return _sprBody.IsAnimating; }
        public bool AnimationPlayedXTimes(int x) { return _sprBody.GetPlayCount() >= x; }

        public bool IsMonster() { return _eActorType == ActorEnum.Monster; }
        public bool IsNPC() { return _eActorType == ActorEnum.NPC; }
        public bool IsAdventurer() { return _eActorType == ActorEnum.Adventurer; }
        public bool IsWorldCharacter() { return _eActorType == ActorEnum.WorldCharacter; }
        public bool IsSpirit() { return _eActorType == ActorEnum.Spirit; }
        public bool IsShippingGremlin() { return _eActorType == ActorEnum.ShippingGremlin; }

        /// <summary>
        /// Helper method for ImportBasics to compile the list of relevant animations
        /// </summary>
        /// <param name="list">List to add to</param>
        /// <param name="data">Data to read form</param>
        /// <param name="verb">Verb to add</param>
        protected void AddToAnimationsList(ref List<AnimationData> list, Dictionary<string, string> data, VerbEnum verb, bool directional = true)
        {
            if (data.ContainsKey(Util.GetEnumString(verb)))
            {
                list.Add(new AnimationData(data[Util.GetEnumString(verb)], verb, directional));
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
        public RHMap CurrentMap => MapManager.Maps[CurrentMapName];
        public Vector2 NewMapPosition;
        public DirectionEnum Facing = DirectionEnum.Down;
        public Point CharCenter => GetRectangle().Center;

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

        public Rectangle CollisionBox => new Rectangle((int)Position.X, (int)Position.Y, Width, TileSize);
        public Rectangle HoverBox => new Rectangle((int)Position.X, (int)Position.Y - TileSize, Width, Height);

        protected bool _bActive = true;
        public virtual bool Active => _bActive;

        protected bool _bHover;

        int _iBaseSpeed = 2;
        public float Speed => _iBaseSpeed * SpdMult;
        public float SpdMult = 1;

        protected int _iSize = 1;
        public int Size => _iSize;

        #endregion

        public WorldActor() : base()
        {
            _eActorType = ActorEnum.WorldCharacter;
            _iWidth = TileSize;
            _iHeight = HUMAN_HEIGHT;
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
            AddToAnimationsList(ref listAnimations, data, VerbEnum.Attack);
            AddToAnimationsList(ref listAnimations, data, VerbEnum.Hurt);
            AddToAnimationsList(ref listAnimations, data, VerbEnum.Critical);
            AddToAnimationsList(ref listAnimations, data, VerbEnum.Cast);
            AddToAnimationsList(ref listAnimations, data, AnimationEnum.KO);
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
        protected void LoadSpriteAnimations(ref AnimatedSprite sprite, List<AnimationData> listAnimations, string textureName)
        {
            sprite = new AnimatedSprite(textureName);

            foreach (AnimationData data in listAnimations)
            {
                if (data.Directional)
                {
                    AddDirectionalAnimations(ref sprite, data, _iWidth, _iHeight, data.PingPong);
                }
                else
                {
                    sprite.AddAnimation(data.Animation, data.XLocation, 0, _iWidth, _iHeight, data.Frames, data.FrameSpeed, data.PingPong);
                }
            }
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

        public virtual void DetermineFacing(Vector2 direction)
        {
            bool walk = false;
            if (direction.Length() != 0)
            {
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
                cornerTiles.Add(MapManager.CurrentMap.GetTileByGridCoords(Util.GetGridCoords(new Vector2(CollisionBox.Left, CollisionBox.Top)).ToPoint()));
                cornerTiles.Add(MapManager.CurrentMap.GetTileByGridCoords(Util.GetGridCoords(new Vector2(CollisionBox.Right, CollisionBox.Top)).ToPoint()));
                cornerTiles.Add(MapManager.CurrentMap.GetTileByGridCoords(Util.GetGridCoords(new Vector2(CollisionBox.Left, CollisionBox.Bottom)).ToPoint()));
                cornerTiles.Add(MapManager.CurrentMap.GetTileByGridCoords(Util.GetGridCoords(new Vector2(CollisionBox.Right, CollisionBox.Bottom)).ToPoint()));
                foreach (RHTile tile in cornerTiles)
                {
                    if (tile != null && tile.WorldObject != null && tile.WorldObject.IsPlant())
                    {
                        Plant f = (Plant)tile.WorldObject;
                        f.Shake();
                    }
                }
            }

            PlayAnimation((walk || CombatManager.InCombat) ? VerbEnum.Walk : VerbEnum.Idle);
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
        public void PlayAnimation(VerbEnum verb) { PlayAnimation(verb, Facing); }
        public bool IsCurrentAnimation(VerbEnum verb) { return _sprBody.IsCurrentAnimation(verb, Facing); }

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
            if (CombatManager.InCombat || CurrentMap.CheckForCollisions(this, testRectX, testRectY, ref direction, ignoreCollisions) && direction != Vector2.Zero)
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

            //Determines how much of the needed position we're capable of moving in one movement
            Util.GetMoveSpeed(Position, target, Speed, ref direction);

            //If we're following a path and there's more than one tile left, we don't want to cut
            //short on individual steps, so recalculate based on the next target
            float length = direction.Length();
            if(_liTilePath.Count > 1 && length < Speed)
            {
                _liTilePath.RemoveAt(0);

                //Recalculate for the next target
                target = _liTilePath[0].Position;
                Util.GetMoveSpeed(Position, target, Speed, ref direction);
            }

            //Attempt to move
            if (!CheckMapForCollisionsAndMove(direction, _bIgnoreCollisions))
            {
                //If we can't move, set a timer to go Ethereal
                if (_dEtherealCD == 0) { _dEtherealCD = 5; }
            }

            //If, after movement, we've reached the given location, zero it.
            if(_vMoveTo == Position && !CutsceneManager.Playing)
            {
                _vMoveTo = Vector2.Zero;
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
    }

    public class TalkingActor : WorldActor
    {
        protected const int PortraitWidth = 160;
        protected const int PortraitHeight = 192;

        protected string _sPortrait;
        public string Portrait { get => _sPortrait; }

        protected Rectangle _rPortrait;
        public Rectangle PortraitRectangle { get => _rPortrait; }

        protected Dictionary<string, string> _diDialogue;

        private int _iLockObjects;

        public TalkingActor() : base()
        {
            _bCanTalk = true;
            _iLockObjects = 0;
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
            GUIManager.OpenTextWindow(text, this);
        }

        /// <summary>
        ///  Retrieves any opening text, processes it, then opens a text window
        /// </summary>
        /// <param name="facePlayer">Whether the NPC should face the player. Mainly used to avoid messing up a cutscene</param>
        public virtual void Talk(bool facePlayer = true)
        {
            string text = GetOpeningText();

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

                PlayAnimation(CombatManager.InCombat ? VerbEnum.Walk : VerbEnum.Idle);
            }

            text = Util.ProcessText(text, _sName);
            GUIManager.OpenTextWindow(text, this);
        }

        public void TalkCutscene(string cutsceneLine)
        {
            string text = cutsceneLine;
            text = Util.ProcessText(text, _sName);
            GUIManager.OpenTextWindow(text, this);
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
            string[] options = textFromData[1].Split('|');

            List<string> liCommands = RemoveEntries(options);

            //If there's only two entires left, Talk and Never Mind, then go straight to Talk
            string rv = string.Empty;
            if (liCommands.Count == 2)
            {
                rv = GetDefaultText();
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
        public string GetDefaultText()
        {
            string entry = RHRandom.Instance.Next(1, 2).ToString();
            return GetDialogEntry(entry);
        }

        /// <summary>
        /// Retrieves the specified entry from the _diDictionaryand calls Util.ProcessTexton it.
        /// </summary>
        /// <param name="entry">The key of the entry to get from the Dictionary</param>
        /// <returns>The processed string text for the entry </returns>
        public string GetDialogEntry(string entry) {
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
                nextText = GetDefaultText();
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
            }
            else
            {
                nextText = GetDialogEntry(chosenAction);
            }

            if (!string.IsNullOrEmpty(nextText)) { rv = true; }

            return rv;
        }

        /// <summary>
        /// Adds one to the number of objects that use the TalkingActor
        /// </summary>
        public void AddCurrentNPCLockObject()
        {
            _iLockObjects++;
        }

        /// <summary>
        /// Removes one from the number of objects that use the TalkingActor
        /// If that number hits 0, clear the CurrentNPC
        /// </summary>
        public void RemoveCurrentNPCLockObject()
        {
            _iLockObjects--;
            if(_iLockObjects == 0)
            {
                GameManager.ClearCurrentNPC();
            }
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

        #region Display
        protected DisplayBar _dbHP;
        #endregion

        #region Stats
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

        private Summon _linkedSummon;
        public Summon LinkedSummon => _linkedSummon;

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

            _dbHP = new DisplayBar(this);
        }
        public override void Draw(SpriteBatch spriteBatch, bool useLayerDepth = false)
        {
            base.Draw(spriteBatch, useLayerDepth);
            _dbHP?.Draw(spriteBatch);
        }

        public override void Update(GameTime gTime)
        {
            base.Update(gTime);

            //Finished being hit, determine action
            if (IsCurrentAnimation(VerbEnum.Hurt) && BodySprite.GetPlayCount() >= 1)
            {
                if (CurrentHP == 0) {
                    KO();
                }
                else if (IsCritical()) { PlayAnimation(VerbEnum.Critical); }
                else { PlayAnimation(VerbEnum.Walk); }
            }

            if (!_diConditions[ConditionEnum.KO] && IsCurrentAnimation(AnimationEnum.KO))
            {
                if (IsCritical()) { PlayAnimation(VerbEnum.Critical); }
                else { PlayAnimation(VerbEnum.Walk); }
            }

            if (IsCurrentAnimation(VerbEnum.Critical) && !IsCritical())
            {
                PlayAnimation(VerbEnum.Walk);
            }

            _linkedSummon?.Update(gTime);

            if (_vMoveTo != Vector2.Zero)
            {
                HandleMove(_vMoveTo);
            }
            else if (_liTilePath.Count > 0)
            {
                Vector2 targetPos = _liTilePath[0].Position;
                if (Position == targetPos)
                {
                    _liTilePath.RemoveAt(0);
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
                        }
                    }
                }
                else
                {
                    HandleMove(targetPos);
                }
            }
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
            double potencyMod = potency / 100;   //100 potency is considered an average attack
            double base_attack = attacker.Attack;  //Attack stat is either weapon damage or mod on monster str
            double StrMult = Math.Round(1 + ((double)attacker.StatStr / 4 * attacker.StatStr / MAX_STAT), 2);

            double dmg = (Math.Max(1, base_attack - StatDef) * compression * StrMult);
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

                if (_linkedSummon != null && _diElementalAlignment[element].Equals(ElementAlignment.Neutral))
                {
                    if (_linkedSummon.Element.Equals(element))
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
                BaseTile.Character.PlayAnimation(VerbEnum.Hurt);

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

            //If the victim is a monster and this attack kills them,
            //immediately hide the hp bar.
            if (IsMonster() && _iCurrentHP <= 0) { _dbHP = null; }

            CombatManager.AddFloatingText(new FloatingText(this.Position, this.SpriteWidth, iValue.ToString(), bHarmful ? Color.Red : Color.Green));
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
        public void SetBaseTile(RHTile newTile)
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
            _linkedSummon = s;
            s.SetBaseTile(BaseTile);

            foreach (KeyValuePair<StatEnum, int> kvp in _linkedSummon.BuffedStats)
            {
                this.HandleStatBuffs(kvp, false);
            }
        }

        public void UnlinkSummon()
        {
            if (_linkedSummon != null)
            {
                foreach (KeyValuePair<StatEnum, int> kvp in _linkedSummon.BuffedStats)
                {
                    this.HandleStatBuffs(kvp, true);
                }
            }

            _linkedSummon = null;
        }

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

        protected class DisplayBar
        {
            bool _bHasMana;
            CombatActor _act;

            public DisplayBar(CombatActor act)
            {
                _act = act;
                _bHasMana = act.MaxMP > 0;
            }

            public void Draw(SpriteBatch spriteBatch)
            {
                if (CombatManager.InCombat)
                {
                    Vector2 pos = _act.Position;
                    pos.Y += (TileSize * _act._iSize);

                    //Do not allow the bar to have less than 2 pixels, one for the border and one to display.
                    int percent = Math.Max((int)((int)(TileSize * _act.Size) * (float)_act.CurrentHP / (float)_act.MaxHP), 2);
                    spriteBatch.Draw(DataManager.GetTexture(@"Textures\Dialog"), new Rectangle((int)pos.X, (int)pos.Y, percent, 4), new Rectangle(16, 4, percent, 4), Color.White, 0, Vector2.Zero, SpriteEffects.None, pos.Y);
                    spriteBatch.Draw(DataManager.GetTexture(@"Textures\Dialog"), new Rectangle((int)pos.X, (int)pos.Y, (int)(TileSize * _act.Size), 4), new Rectangle(16, 0, 16, 4), Color.White, 0, Vector2.Zero, SpriteEffects.None, pos.Y + 1);

                    if (_bHasMana)
                    {
                        pos.Y += 4;
                        percent = (int)(16 * (float)_act.CurrentMP / (float)_act.MaxMP);
                        spriteBatch.Draw(DataManager.GetTexture(@"Textures\Dialog"), new Rectangle((int)pos.X, (int)pos.Y, percent, 4), new Rectangle(16, 12, percent, 4), Color.White, 0, Vector2.Zero, SpriteEffects.None, pos.Y);
                        spriteBatch.Draw(DataManager.GetTexture(@"Textures\Dialog"), new Rectangle((int)pos.X, (int)pos.Y, (int)(TileSize * _act.Size), 4), new Rectangle(16, 8, 16, 4), Color.White, 0, Vector2.Zero, SpriteEffects.None, pos.Y + 1);
                    }
                }
            }
        }
    }
    #endregion

    public class ClassedCombatant : CombatActor
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
            if (Weapon == null) { Weapon.SetGear((Equipment)GetItem(_class.WeaponID)); }
            if (Armor == null) { Armor.SetGear((Equipment)GetItem(_class.ArmorID)); }
            if (Head == null) { Head.SetGear((Equipment)GetItem(_class.HeadID)); }
            if (Wrist == null) { Wrist.SetGear((Equipment) GetItem(_class.WristID));}
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
            if (_vStartPosition.Y < 3)
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
        }
    }
   
    public class Villager : ClassedCombatant
    {
        //Data for building structures
        Building _buildTarget;
        bool _bStartedBuilding;

        protected int _iIndex;
        public int ID  => _iIndex;
        protected string _sHomeMap;
        public enum NPCTypeEnum { Villager, Eligible, Shopkeeper, Ranger, Worker, Mason }
        protected NPCTypeEnum _eNPCType;
        public NPCTypeEnum NPCType => _eNPCType;
        public static List<int> FriendRange = new List<int> { 0, 10, 40, 100, 200, 600, 800, 1200, 1600, 2000 };
        public int FriendshipPoints = 1900;

        protected Dictionary<int, bool> _diCollection;
        public Dictionary<int, bool> Collection => _diCollection;
        public bool Introduced;
        public bool CanGiveGift = true;

        protected Dictionary<string, List<KeyValuePair<string, string>>> _diCompleteSchedule;         //Every day with a list of KVP Time/GoToLocations
        List<KeyValuePair<string, List<RHTile>>> _todaysPathing = null;                             //List of Times with the associated pathing                                                     //List of Tiles to currently be traversing
        protected int _iScheduleIndex;

        public Villager() {
            _diCollection = new Dictionary<int, bool>();
        }
        //For Cutscenes
        public Villager(Villager n)
        {
            _eActorType = ActorEnum.NPC;
            _iIndex = n.ID;
            _sName = n.Name;
            _diDialogue = n._diDialogue;
            //_sPortrait = n.Portrait;
            //_portraitRect = n._portraitRect;
            _rPortrait = new Rectangle(0, 0, 48, 60);
            _sPortrait = _sAdventurerFolder + "WizardPortrait";

            _iWidth = n._sprBody.Width;
            _iHeight = n._sprBody.Height;
            _sprBody = new AnimatedSprite(n.BodySprite);
        }

        public Villager(int index, Dictionary<string, string> stringData): this()
        {
            _eActorType = ActorEnum.NPC;
            _diCompleteSchedule = new Dictionary<string, List<KeyValuePair<string, string>>>();
            _iScheduleIndex = 0;
            _iIndex = index;

            _bHover = stringData.ContainsKey("Hover");

            ImportBasics(stringData);
        }

        protected void ImportBasics(Dictionary<string, string> data)
        {
            _diDialogue = DataManager.GetNPCDialogue(_iIndex);
            _sPortrait = _sAdventurerFolder + "WizardPortrait";
            _sName = _diDialogue["Name"];
            _sPortrait = _sAdventurerFolder + "WizardPortrait";

            LoadSpriteAnimations(ref _sprBody, LoadWorldAnimations(data), _sVillagerFolder + "NPC_" + _iIndex.ToString("00"));
            PlayAnimation(VerbEnum.Idle);

            _bActive = !data.ContainsKey("Inactive");
            if (data.ContainsKey("Type")) { _eNPCType = Util.ParseEnum<NPCTypeEnum>(data["Type"]); }
            if (data.ContainsKey("PortRow")) { _rPortrait = new Rectangle(0, 0, 48, 60); }
            if (data.ContainsKey("HomeMap"))
            {
                _sHomeMap = data["HomeMap"];
                CurrentMapName = _sHomeMap;
            }

            if (data.ContainsKey("Collection"))
            {
                string[] vectorSplit = data["Collection"].Split('-');
                foreach (string s in vectorSplit)
                {
                    _diCollection.Add(int.Parse(s), false);
                }
            }

            Dictionary<string, string> schedule = DataManager.GetSchedule("NPC" + _iIndex);
            if (schedule != null)
            {
                foreach (KeyValuePair<string, string> kvp in schedule)
                {
                    List<KeyValuePair<string, string>> temp = new List<KeyValuePair<string, string>>();
                    string[] group = kvp.Value.Split('/');
                    foreach (string s in group)
                    {
                        string[] strSchedule = s.Split('|');
                        temp.Add(new KeyValuePair<string, string>(strSchedule[0], strSchedule[1]));
                    }
                    _diCompleteSchedule.Add(kvp.Key, temp);
                }
            }
        }

        public override void Update(GameTime gTime)
        {
            base.Update(gTime);

            if (_vMoveTo != Vector2.Zero)
            {
                HandleMove(_vMoveTo);
            }
            else if (_todaysPathing != null)
            {
                string currTime = GameCalendar.GetTime();
                //_scheduleIndex keeps track of which pathing route we're currently following.
                //Running late code to be implemented later
                if (_iScheduleIndex < _todaysPathing.Count && ((_todaysPathing[_iScheduleIndex].Key == currTime)))// || RunningLate(movementList[_scheduleIndex].Key, currTime)))
                {
                    _liTilePath = _todaysPathing[_iScheduleIndex++].Value;
                }

                if (_liTilePath.Count > 0)
                {
                    Vector2 targetPos = _liTilePath[0].Position;
                    if (Position == targetPos)
                    {
                        _liTilePath.RemoveAt(0);
                        if (_liTilePath.Count == 0)
                        {
                            DetermineFacing(Vector2.Zero);
                        }

                        if (_liTilePath.Count > 0 && _liTilePath[0] != null && _liTilePath[0].GetTravelPoint() != null)
                        {
                            MapManager.ChangeMaps(this, CurrentMapName, MapManager.CurrentMap.DictionaryTravelPoints[_liTilePath[0].GetTravelPoint().LinkedMap]);
                        }
                    }
                    else
                    {
                        HandleMove(targetPos);
                    }
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
            if (!Introduced)
            {
                rv = GetDialogEntry("Introduction");
                Introduced = true;
            }
            else
            {
                if (!CheckQuestLogs(ref rv))
                {
                    rv = GetSelectionText();
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
                    GUIManager.OpenMainObject(new HUDInventoryDisplay());
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

        public virtual void RollOver()
        {
            if (!_bStartedBuilding && _buildTarget != null)
            {
                _bStartedBuilding = true;
            }

            //If we failed to move the NPC to a building location, because there was none
            //Add the NPC to their home map
            if (!MoveToBuildingLocation()) { 
                MoveToSpawn();
                CalculatePathing();
            }
        }

        /// <summary>
        /// Removes the NPC from the current map and moves them back
        /// to their home map and back to their spawn point.
        /// </summary>
        public void MoveToSpawn()
        {
            CurrentMap?.RemoveCharacterImmediately(this);
            CurrentMapName = _sHomeMap;
            RHMap map = MapManager.Maps[_sHomeMap];
            string Spawn = "NPC_" + _iIndex.ToString("00");

            Position = Util.SnapToGrid(map.GetCharacterSpawn(Spawn));
            map.AddCharacterImmediately(this); 
        }

        public void CalculatePathing()
        {
            string currDay = GameCalendar.GetDayOfWeek();
            string currSeason = GameCalendar.GetSeason();
            string currWeather = GameCalendar.GetWeatherString();
            if (_diCompleteSchedule != null && _diCompleteSchedule.Count > 0)
            {
                string searchVal = currSeason + currDay + currWeather;
                List<KeyValuePair<string, string>> listPathingForDay = null;

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
                    List<KeyValuePair<string, List<RHTile>>> lTimetoTilePath = new List<KeyValuePair<string, List<RHTile>>>();
                    Vector2 start = Position;
                    string mapName = CurrentMapName;

                    TravelManager.NewTravelLog(_sName);
                    foreach (KeyValuePair<string, string> kvp in listPathingForDay)
                    {
                        List<RHTile> timePath;

                        //If the map we're currently on has the target location, pathfind to it.
                        //Otherwise, we need to pathfind to the map that does first.
                        if (CurrentMap.DictionaryCharacterLayer.ContainsKey(kvp.Value))
                        {
                            timePath = TravelManager.FindPathToLocation(ref start, CurrentMap.DictionaryCharacterLayer[kvp.Value]);
                        }
                        else
                        {
                            timePath = TravelManager.FindPathToOtherMap(kvp.Value, ref mapName, ref start);
                        }
                        lTimetoTilePath.Add(new KeyValuePair<string, List<RHTile>>(kvp.Key, timePath));
                    }
                    TravelManager.CloseTravelLog();

                    _todaysPathing = lTimetoTilePath;
                }
            }
        }

        /// <summary>
        /// Because the Actor pathfinds based off of objective locations of the exit object
        /// it is possible, and probable, that they will enter the object, triggering a map
        /// change, a tile or two earlier than anticipated. In which case, we need to wipe
        /// any tiles that are on that map from the remaining path to follow.
        /// </summary>
        public void ClearTileForMapChange()
        {
            while (_liTilePath[0].MapName == CurrentMapName)
            {
                _liTilePath.RemoveAt(0);
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

        private bool CheckQuestLogs(ref string text)
        {
            bool rv = false;

            foreach (Quest q in PlayerManager.QuestLog)
            {
                if (q.ReadyForHandIn && q.HandInTo == this)
                {
                    q.FinishQuest(ref text);

                    rv = true;
                    break;
                }
            }

            return rv;
        }

        public virtual void Gift(Item item)
        {
            if (item != null)
            {
                string text = string.Empty;
                item.Remove(1);
                if (item.IsMap() && NPCType == Villager.NPCTypeEnum.Ranger)
                {
                    text = GetDialogEntry("Adventure");
                    DungeonManagerOld.LoadNewDungeon((AdventureMap)item);
                }
                else
                {
                    CanGiveGift = false;
                    if (_diCollection.ContainsKey(item.ItemID))
                    {
                        FriendshipPoints += _diCollection[item.ItemID] ? 50 : 20;
                        text = GetDialogEntry("Collection");
                        int index = 1;
                        foreach (int items in _diCollection.Keys)
                        {
                            if (_diCollection[items])
                            {
                                index++;
                            }
                        }

                        _diCollection[item.ItemID] = true;
                        MapManager.Maps["mapHouseNPC" + _iIndex].AddCollectionItem(item.ItemID, _iIndex, index);
                    }
                    else
                    {
                        text = GetDialogEntry("Gift");
                        FriendshipPoints += 1000;
                    }
                }

                if (!string.IsNullOrEmpty(text))
                {
                    GUIManager.OpenTextWindow(text, this, false);
                }
            }
        }

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
                collection = new List<CollectionData>()
            };
            foreach (KeyValuePair<int, bool> kvp in Collection)
            {
                CollectionData c = new CollectionData
                {
                    itemID = kvp.Key,
                    given = kvp.Value
                };
                npcData.collection.Add(c);
            }

            return npcData;
        }
        public void LoadData(NPCData data)
        {
            Introduced = data.introduced;
            FriendshipPoints = data.friendship;
            int index = 1;
            foreach (CollectionData c in data.collection)
            {
                if (c.given)
                {
                    Collection[c.itemID] = c.given;
                    MapManager.Maps["HouseNPC" + data.npcID].AddCollectionItem(c.itemID, data.npcID, index++);
                }
            }
            MoveToBuildingLocation();
        }
    }
    public class ShopKeeper : Villager
    {
        protected List<Merchandise> _liMerchandise;
        public List<Merchandise> Buildings { get => _liMerchandise; }

        public ShopKeeper(int index, Dictionary<string, string> stringData)
        {
            _eActorType = ActorEnum.NPC;
            _eNPCType = NPCTypeEnum.Shopkeeper;
            _liTilePath = new List<RHTile>();
            _liMerchandise = new List<Merchandise>();
            _diCollection = new Dictionary<int, bool>();
            _diCompleteSchedule = new Dictionary<string, List<KeyValuePair<string, string>>>();

            _iIndex = index;

            ImportBasics(stringData);

            if (stringData.ContainsKey("ShopData"))
            {
                foreach (KeyValuePair<int, string> kvp in DataManager.GetMerchandise(stringData["ShopData"]))
                {
                    _liMerchandise.Add(new Merchandise(kvp.Value));
                }
            }
        }

        public override void Talk(bool IsOpen = false)
        {
            GUICursor._CursorType = GUICursor.EnumCursorType.Talk;
            string text = string.Empty;
            if (!Introduced)
            {
                text = _diDialogue["Introduction"];
                Introduced = true;
            }
            else
            {
                if (IsOpen)
                {
                    text = _diDialogue["ShopOpen"];
                }
                else
                {
                    text = GetDefaultText();
                }
            }
            text = Util.ProcessText(text, _sName);
            GUIManager.OpenTextWindow(text, this);
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
                        if (m.MerchType == Merchandise.ItemType.Worker && m.Activated()) { _liMerchandise.Add(m); }
                    }
                    GUIManager.OpenMainObject(new HUDPurchaseWorkers(_liMerchandise));
                    GameManager.ClearGMObjects();
                }
                else if (chosenAction.Equals("Missions"))
                {
                    GUIManager.OpenMainObject(new HUDMissionWindow());
                    GameManager.ClearGMObjects();
                }
                else if (chosenAction.Equals("BuyItems"))
                {
                    foreach (Merchandise m in this._liMerchandise)
                    {
                        if (m.MerchType == Merchandise.ItemType.Item && m.Activated()) { _liMerchandise.Add(m); }
                    }
                    GUIManager.OpenMainObject(new HUDPurchaseItems(_liMerchandise));
                    GameManager.ClearGMObjects();
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
            string _sUniqueData;
            public string UniqueData => _sUniqueData;
            public enum ItemType { Building, Worker, Item, Upgrade }
            public ItemType MerchType;
            int _merchID = -1;
            public int MerchID { get => _merchID; }
            string _description;
            int _moneyCost;
            public int MoneyCost { get => _moneyCost; }
            int _iQuestReq = -1;

            List<KeyValuePair<int, int>> _items; //item, then num required
            public List<KeyValuePair<int, int>> RequiredItems { get => _items; }

            public Merchandise(string data)
            {
                _items = new List<KeyValuePair<int, int>>();
                string[] dataValues = data.Split('/');

                int i = 0;
                if (dataValues[0] == "Building")
                {
                    MerchType = ItemType.Building;
                    i = 1;
                    _merchID = int.Parse(dataValues[i++]);
                    _description = dataValues[i++];
                    _moneyCost = int.Parse(dataValues[i++]);

                    string[] reqItems = dataValues[i++].Split(':');
                    foreach (string str in reqItems)
                    {
                        string[] itemsSplit = str.Split(' ');
                        _items.Add(new KeyValuePair<int, int>(int.Parse(itemsSplit[0]), int.Parse(itemsSplit[1])));
                    }
                }
                else if (dataValues[0] == "Worker")
                {
                    MerchType = ItemType.Worker;
                    i = 1;
                    _merchID = int.Parse(dataValues[i++]);
                    _description = dataValues[i++];
                    _moneyCost = int.Parse(dataValues[i++]);
                }
                else if (dataValues[0] == "Item")
                {
                    MerchType = ItemType.Item;
                    i = 1;
                    string[] itemData = dataValues[i++].Split('-');
                    _merchID = int.Parse(itemData[0]);
                    if (itemData.Length > 1) { _sUniqueData = itemData[1]; }
                    _moneyCost = int.Parse(dataValues[i++]);
                    if (dataValues.Length >= i + 1)
                    {
                        _iQuestReq = int.Parse(dataValues[i++]);
                    }
                }
                else if (dataValues[0] == "Upgrade")
                {
                    MerchType = ItemType.Upgrade;
                    i = 1;
                    _merchID = int.Parse(dataValues[i++]);
                    _description = dataValues[i++];
                    _moneyCost = int.Parse(dataValues[i++]);

                    string[] reqItems = dataValues[i++].Split(':');
                    foreach (string str in reqItems)
                    {
                        string[] itemsSplit = str.Split(' ');
                        _items.Add(new KeyValuePair<int, int>(int.Parse(itemsSplit[0]), int.Parse(itemsSplit[1])));
                    }
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
        public bool Married;
        bool _bCanJoinParty = true;
        public bool CanJoinParty => _bCanJoinParty;

        public EligibleNPC(int index, Dictionary<string, string> stringData)
        {
            _liTilePath = new List<RHTile>();
            _diCollection = new Dictionary<int, bool>();
            _diCompleteSchedule = new Dictionary<string, List<KeyValuePair<string, string>>>();

            _iIndex = index;

            if (stringData.ContainsKey("Class"))
            {
                SetClass(DataManager.GetClassByIndex(int.Parse(stringData["Class"])));
                AssignStartingGear();
            }

            ImportBasics(stringData);
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
            CurrentMap.RemoveCharacter(this);
            RHMap map = MapManager.Maps[Married ? "mapManor" : _sHomeMap];
            string Spawn = Married ? "Spouse" : "NPC" + _iIndex;

            Position = Util.SnapToGrid(map.GetCharacterSpawn(Spawn));
            map.AddCharacter(this);

            _bActive = true;
            PlayerManager.RemoveFromParty(this);

            //Reset on Monday
            if (GameCalendar.DayOfWeek == 0)
            {
                _bCanJoinParty = true;
                CanGiveGift = true;
            }

            CalculatePathing();
        }

        public override void Gift(Item item)
        {
            if (item != null)
            {
                string text = string.Empty;
                item.Remove(1);
                if (item.IsMap() && NPCType == Villager.NPCTypeEnum.Ranger)
                {
                    text = GetDialogEntry("Adventure");
                    DungeonManagerOld.LoadNewDungeon((AdventureMap)item);
                }
                else if (item.IsMarriage())
                {
                    if (FriendshipPoints > 200)
                    {
                        Married = true;
                        text = GetDialogEntry("MarriageYes");
                    }
                    else   //Marriage refused, readd the item
                    {
                        item.Add(1);
                        InventoryManager.AddToInventory(item);
                        text = GetDialogEntry("MarriageNo");
                    }
                }
                else
                {
                    CanGiveGift = false;
                    if (_diCollection.ContainsKey(item.ItemID))
                    {
                        FriendshipPoints += _diCollection[item.ItemID] ? 50 : 20;
                        text = GetDialogEntry("Collection");
                        int index = 1;
                        foreach (int items in _diCollection.Keys)
                        {
                            if (_diCollection[items])
                            {
                                index++;
                            }
                        }

                        _diCollection[item.ItemID] = true;
                        MapManager.Maps["mapHouseNPC" + _iIndex].AddCollectionItem(item.ItemID, _iIndex, index);
                    }
                    else
                    {
                        text = GetDialogEntry("Gift");
                        FriendshipPoints += 10;
                    }
                }

                if (!string.IsNullOrEmpty(text))
                {
                    GUIManager.OpenTextWindow(text, this, false);
                }
            }
        }

        public void JoinParty()
        {
            _bActive = false;
            _bCanJoinParty = false;
            PlayerManager.AddToParty(((EligibleNPC)this));
        }

        public new EligibleNPCData SaveData()
        {
            EligibleNPCData npcData = new EligibleNPCData()
            {
                npcData = base.SaveData(),
                married = Married,
                canJoinParty = _bCanJoinParty,
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
            _bCanJoinParty = data.canJoinParty;
            CanGiveGift = data.canGiveGift;
            LoadClassedCharData(data.classedData);

            int index = 1;
            foreach (CollectionData c in data.npcData.collection)
            {
                if (c.given)
                {
                    Collection[c.itemID] = c.given;
                    MapManager.Maps["HouseNPC" + data.npcData.npcID].AddCollectionItem(c.itemID, data.npcData.npcID, index++);
                }
            }

            if (Married)
            {
                MapManager.Maps[_sHomeMap].RemoveCharacter(this);
                MapManager.Maps["mapManor"].AddCharacter(this);
                Position = MapManager.Maps["mapManor"].GetCharacterSpawn("Spouse");
            }
        }
    }

    public class Adventurer : ClassedCombatant
    {
        #region Properties
        private enum AdventurerStateEnum { Idle, InParty, OnMission };
        private AdventurerStateEnum _eState;
        private WorkerTypeEnum _eWorkerType;
        public WorkerTypeEnum WorkerType => _eWorkerType;
        protected int _iPersonalID;
        public int PersonalID { get => _iPersonalID; }
        protected int _iAdventurerID;
        public int WorkerID { get => _iAdventurerID; }
        protected string _sAdventurerType;
        private Building _building;
        public Building Building => _building;
        protected int _iDailyFoodReq;
        protected int _iCurrFood;
        protected int _iDailyItemID;
        protected Item _heldItem;
        protected int _iMood;
        private int _iResting;
        public int Mood { get => _iMood; }

        Mission _curMission;
        public Mission CurrentMission => _curMission;

        protected double _dProcessedTime;
        public double ProcessedTime => _dProcessedTime;

        Dictionary<int, int> _diCrafting;
        public Dictionary<int, int> CraftList => _diCrafting;
        int _iCurrentlyMaking;
        public int CurrentlyMaking => _iCurrentlyMaking;

        public override bool Active => _eState == AdventurerStateEnum.Idle;
        #endregion

        public Adventurer(Dictionary<string, string> data, int id)
        {
            _iAdventurerID = id;
            _iPersonalID = PlayerManager.GetTotalWorkers();
            _eActorType = ActorEnum.Adventurer;
            ImportBasics(data, id);

            SetClass(DataManager.GetClassByIndex(_iAdventurerID));
            AssignStartingGear();
            _sAdventurerType = CharacterClass.Name;

            _rPortrait = new Rectangle(0, 0, 48, 60);
            _sPortrait = _sAdventurerFolder + "WizardPortrait";

            _iCurrFood = 0;
            _heldItem = null;
            _iMood = 0;

            _eState = AdventurerStateEnum.Idle;

            _sName = _sAdventurerType.Substring(0, 1);
        }

        protected void ImportBasics(Dictionary<string, string> data, int id)
        {
            _diCrafting = new Dictionary<int, int>();

            _iAdventurerID = id;

            _eWorkerType = Util.ParseEnum<WorkerTypeEnum>(data["Type"]);
            _iDailyItemID = int.Parse(data["Item"]);
            _iDailyFoodReq = int.Parse(data["Food"]);

            if (data.ContainsKey("Crafts"))
            {
                string[] crafting = data["Crafts"].Split(' ');
                foreach (string recipe in crafting)
                {
                    _diCrafting.Add(int.Parse(recipe), int.Parse(recipe));
                }
            }

            LoadSpriteAnimations(ref _sprBody, LoadWorldAndCombatAnimations(data), _sAdventurerFolder + "Adventurer_" + _iAdventurerID);
        }

        public override void Draw(SpriteBatch spriteBatch, bool useLayerDepth = false)
        {
            if (_eState == AdventurerStateEnum.Idle || (CombatManager.InCombat && _eState == AdventurerStateEnum.InParty))
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

        public override string GetOpeningText()
        {
            return Name + ": " + DataManager.GetGameText("AdventurerTree");
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
                    _eState = AdventurerStateEnum.InParty;
                    PlayerManager.AddToParty(this);
                    nextText = "Of course!";
                }
            }

            if (!string.IsNullOrEmpty(nextText)) {
                _iMood++;
                rv = true;
            }

            return rv;
        }

        public void ProcessChosenItem(int itemID)
        {
            _iCurrentlyMaking = _diCrafting[itemID];
            _sprBody.PlayAnimation(AnimationEnum.PlayAnimation);
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
            _building = b;
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
                    if (_curMission.Completed())
                    {
                        _eState = AdventurerStateEnum.Idle;
                        _iResting = _curMission.DaysToComplete / 2;
                        _curMission = null;
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
                InventoryManager.InitContainerInventory(_building.BuildingChest.Inventory);
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
            _curMission = m;
            _eState = AdventurerStateEnum.OnMission;
        }

        /// <summary>
        /// Cancels the indicated mission, returning the adventurer to their
        /// home building. Does not get called unless a mission has been canceled.
        /// </summary>
        public void EndMission()
        {
            _iResting = _curMission.DaysToComplete / 2;
            _curMission = null;
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
                    rv = "On Mission \"" + _curMission.Name + "\" days left: " + (_curMission.DaysToComplete - _curMission.DaysFinished).ToString();
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
                processedTime = this.ProcessedTime,
                currentItemID = (this._iCurrentlyMaking == -1) ? -1 : this._iCurrentlyMaking,
                heldItemID = (this._heldItem == null) ? -1 : this._heldItem.ItemID,
                state = (int)_eState
            };

            return workerData;
        }
        public void LoadAdventurerData(WorkerData data)
        {
            _iAdventurerID = data.workerID;
            _iPersonalID = data.PersonalID;
            _iMood = data.mood;
            _sName = data.name;
            _dProcessedTime = data.processedTime;
            _iCurrentlyMaking = data.currentItemID;
            _heldItem = DataManager.GetItem(data.heldItemID);
            _eState = (AdventurerStateEnum)data.state;

            base.LoadClassedCharData(data.advData);

            if (_iCurrentlyMaking != -1) {_sprBody.PlayAnimation(AnimationEnum.PlayAnimation); }

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

        Color _cHairColor = Color.White;
        public Color HairColor => _cHairColor;

        int _iHairIndex = 0;
        public int HairIndex => _iHairIndex;

        int _iBodyType = 1;
        public int BodyType => _iBodyType;

        public Vector2 BodyPosition => _sprBody.Position;
        public override Vector2 Position
        {
            get { return new Vector2(_sprBody.Position.X, _sprBody.Position.Y + _sprBody.Height - TileSize); }
            set
            {
                _sprBody.Position = new Vector2(value.X, value.Y - _sprBody.Height + TileSize);
                _sprEyes.Position = _sprBody.Position;
                _sprHair.Position = _sprBody.Position;

                if (_chest != null) { _chest.SetSpritePosition(_sprBody.Position); }
                if (Hat != null) { _hat.SetSpritePosition(_sprBody.Position); }
            }
        }

        Clothes _hat;
        public Clothes Hat => _hat;
        Clothes _chest;
        public Clothes Shirt => _chest;
        Clothes Back;
        Clothes Hands;
        Clothes Legs;
        Clothes Feet;

        public PlayerCharacter() : base()
        {
            _sName = PlayerManager.Name;
            _iWidth = TileSize;
            _iHeight = HUMAN_HEIGHT;

            _cHairColor = Color.Red;

            _liTilePath = new List<RHTile>();

            //Sets a default class so we can load and display the character to start
            SetClass(DataManager.GetClassByIndex(1));

            _sprBody.SetColor(Color.White);
            _sprHair.SetColor(_cHairColor);
        }

        public override void Update(GameTime gTime)
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

            if (_vMoveTo != Vector2.Zero)
            {
                HandleMove(_vMoveTo);
            }

            //If there are tiles remaining on the path to follow
            if (_liTilePath.Count > 0)
            {
                Vector2 targetPos = _liTilePath[0].Position;

                Vector2 _vToTarget = Vector2.Zero;
                Util.GetMoveSpeed(Position, targetPos, Speed, ref _vToTarget);
                //Vector2 _vToNextTarget = Vector2.Zero;
                //if(_liTilePath.Count > 1) {  Util.GetMoveSpeed(Position, targetPos, Speed, ref _vToNextTarget); }

                float length = _vToTarget.Length();
                if (Position == targetPos)
                {
                    _liTilePath.RemoveAt(0);
                    if (_liTilePath.Count == 0)
                    {
                        DetermineFacing(Vector2.Zero);
                    }
                }
                else
                {
                    HandleMove(targetPos);
                }
            }

            _sprBody.Update(gTime);
            _sprEyes.Update(gTime);
            _sprHair.Update(gTime);

            if (_chest != null) { _chest.Sprite.Update(gTime); }
            if (Hat != null) { Hat.Sprite.Update(gTime); }
        }

        public override void Draw(SpriteBatch spriteBatch, bool useLayerDepth = false)
        {
            base.Draw(spriteBatch);
            _sprBody.Draw(spriteBatch, useLayerDepth);

            float bodyDepth = _sprBody.Position.Y + _sprBody.CurrentFrameAnimation.FrameHeight + (Position.X / 100);
            //_sprEyes.Draw(spriteBatch, useLayerDepth, 1.0f, bodyDepth);
            //_sprHair.Draw(spriteBatch, useLayerDepth, 1.0f, bodyDepth);

            _chest?.Sprite.Draw(spriteBatch, useLayerDepth, 1.0f, bodyDepth + 0.01f);
            Hat?.Sprite.Draw(spriteBatch, useLayerDepth);
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
            LoadSpriteAnimations(ref _sprBody, LoadWorldAndCombatAnimations(DataManager.PlayerAnimationData[x.ID]), string.Format(@"{0}Body_{1}", DataManager.FOLDER_PLAYER, _iBodyType.ToString("00")));

            //Hair type has already been set either by default or by being allocated.
            SetHairType(_iHairIndex);

            //Loads the Sprites for the players body for the appropriate class
            LoadSpriteAnimations(ref _sprEyes, LoadWorldAndCombatAnimations(DataManager.PlayerAnimationData[x.ID]), string.Format(@"{0}Eyes", DataManager.FOLDER_PLAYER));
            _sprEyes.SetDepthMod(EYE_DEPTH);
        }

        public void SetColor(AnimatedSprite sprite, Color c)
        {
            sprite.SetColor(c);
        }

        public void SetHairColor(Color c)
        {
            _cHairColor = c;
            SetColor(_sprHair, c);
        }
        public void SetHairType(int index)
        {
            _iHairIndex = index;
            //Loads the Sprites for the players hair animations for the class based off of the hair ID
            LoadSpriteAnimations(ref _sprHair, LoadWorldAndCombatAnimations(DataManager.PlayerAnimationData[CharacterClass.ID]), string.Format(@"{0}Hairstyles\Hair_{1}", DataManager.FOLDER_PLAYER, _iHairIndex));
            _sprHair.SetDepthMod(HAIR_DEPTH);
        }

        public void MoveBy(int x, int y)
        {
            _sprBody.MoveBy(x, y);
            _sprEyes.MoveBy(x, y);
            _sprHair.MoveBy(x, y);
            if (_chest != null) { _chest.Sprite.MoveBy(x, y); }
            if (Hat != null) { Hat.Sprite.MoveBy(x, y); }
        }

        public override void PlayAnimation(AnimationEnum anim)
        {
            _sprBody.PlayAnimation(anim);
            _sprEyes.PlayAnimation(anim);
            _sprHair.PlayAnimation(anim);

            if (_chest != null) { _chest.Sprite.PlayAnimation(anim); }
            if (Hat != null) { Hat.Sprite.PlayAnimation(anim); }
        }
        public override void PlayAnimation(VerbEnum verb, DirectionEnum dir)
        {
            _sprBody.PlayAnimation(verb, dir);
            _sprEyes.PlayAnimation(verb, dir);
            _sprHair.PlayAnimation(verb, dir);

            if (_chest != null) { _chest.Sprite.PlayAnimation(verb, dir); }
            if (Hat != null) { Hat.Sprite.PlayAnimation(verb, dir); }
        }

        public void SetScale(int scale = 1)
        {
            _sprBody.SetScale(scale);
            _sprEyes.SetScale(scale);
            _sprHair.SetScale(scale);

            if (_chest != null) { _chest.Sprite.SetScale(scale); }
            if (_hat != null) { _hat.Sprite.SetScale(scale); }
        }

        public void SetClothes(Clothes c)
        {
            if (c != null)
            {
                if (c.IsShirt()) { _chest = c; }
                else if (c.IsHat())
                {
                    _sprHair.FrameCutoff = 9;
                    _hat = c;
                }

                //MAR AWKWARD
                c.Sprite.Position = _sprBody.Position;
                c.Sprite.PlayAnimation(_sprBody.CurrentAnimation);
                c.Sprite.SetDepthMod(0.004f);
            }
        }

        public void RemoveClothes(ClothesEnum c)
        {
            if (c.Equals(ClothesEnum.Chest)) { _chest = null; }
            else if (c.Equals(ClothesEnum.Hat))
            {
                _sprHair.FrameCutoff = 0;
                _hat = null;
            }
        }

        public void SetBodyType(int val)
        {
            _iBodyType = val;
            SetClass(_class);
        }
    }

    public class Spirit : TalkingActor
    {
        const float MIN_VISIBILITY = 0.05f;
        float _fVisibility;
        int _iID;
        string _sCondition;
        string _sText;

        int _iSongID = 1;
        public int SongID => _iSongID;

        public bool Triggered;

        public Spirit(string name, string type, string condition, string text) : base()
        {
            _eActorType = ActorEnum.Spirit;
            _fVisibility = MIN_VISIBILITY;

            _sName = name;
            _iID = int.Parse(type);
            _sText = text;
            _sCondition = condition;
            _bActive = false;

            _iWidth = TileSize;
            _iHeight = TileSize + 2;
            List<AnimationData> liData = new List<AnimationData>();
            AddToAnimationsList(ref liData, DataManager.DiSpiritInfo[_iID], VerbEnum.Idle);
            LoadSpriteAnimations(ref _sprBody, liData, _sNPsCFolder + "Spirit_" + _iID);
        }

        public override void Update(GameTime gTime)
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

        public override void Draw(SpriteBatch spriteBatch, bool useLayerDepth = false)
        {
            if (_bActive)
            {
                _sprBody.Draw(spriteBatch, useLayerDepth, _fVisibility);
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
                GUIManager.OpenTextWindow(_sText, this);
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
            _arrInventory = new Item[_iRows, _iCols];
            _eActorType = ActorEnum.ShippingGremlin;
            _iIndex = index;
            _iWidth = 32;
            _iHeight = 32;
            _diDialogue = DataManager.GetNPCDialogue(_iIndex);
            _sPortrait = _sAdventurerFolder + "WizardPortrait";
            _sName = _diDialogue["Name"];
            _sPortrait = _sAdventurerFolder + "WizardPortrait";

            if (stringData.ContainsKey("HomeMap"))
            {
                _sHomeMap = stringData["HomeMap"];
                CurrentMapName = _sHomeMap;
            }

            _sprBody = new AnimatedSprite(_sVillagerFolder + "NPC_" + _iIndex.ToString("00"));
            _sprBody.AddAnimation(AnimationEnum.ObjectIdle, 0, 0, _iWidth, _iHeight);
            _sprBody.AddAnimation(AnimationEnum.ObjectAction1, 32, 0, _iWidth, _iHeight, 3, 0.1f);
            _sprBody.AddAnimation(AnimationEnum.ObjectActionFinished, 128, 0, _iWidth, _iHeight);
            _sprBody.AddAnimation(AnimationEnum.ObjectAction2, 160, 0, _iWidth, _iHeight, 3, 0.1f);
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
                    GameManager.AddCurrentNPCLockObject();
                    GUIManager.OpenMainObject(new HUDInventoryDisplay(_arrInventory));
                }
                else
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
                    val += i.SellPrice;
                    PlayerManager.AddMoney(i.SellPrice);
                }
            }
            _arrInventory = new Item[_iRows, _iCols];
            return val;
        }
    }
 
    public class Summon : CombatActor
    {
        ElementEnum _element = ElementEnum.None;
        public ElementEnum Element => _element;

        int _iMagStat;
        List<KeyValuePair<StatEnum, int>> _liBuffedStats;
        public List<KeyValuePair<StatEnum, int>> BuffedStats => _liBuffedStats;

        public bool Acted;
        bool _bTwinCast;
        public bool TwinCast => _bTwinCast;
        bool _bAggressive;
        public bool Aggressive => _bAggressive;
        bool _bRegen;
        public bool Regen => _bRegen;

        public CombatActor linkedChar;

        public Summon(int id, Dictionary<string, string> stringData)
        {
            _liBuffedStats = new List<KeyValuePair<StatEnum, int>>();
            _bGuard = stringData.ContainsKey("Defensive");
            _bAggressive = stringData.ContainsKey("Aggressive");
            _bTwinCast = stringData.ContainsKey("TwinCast");
            _bRegen = stringData.ContainsKey("Regen");
            Counter = stringData.ContainsKey("Counter");

            if (stringData.ContainsKey("Element"))
            {
                _element = Util.ParseEnum<ElementEnum>(stringData["Element"]);
            }

            foreach (StatEnum stat in Enum.GetValues(typeof(StatEnum)))
            {
                if (stringData.ContainsKey(Util.GetEnumString(stat)))
                {
                    _liBuffedStats.Add(new KeyValuePair<StatEnum, int>(stat, 0));
                }
            }

            string[] spawn = stringData["Spawn"].Split('-');
            string[] idle = stringData["Idle"].Split('-');
            string[] cast = stringData["Cast"].Split('-');
            string[] attack = stringData["Attack"].Split('-');

            int iFrameSize = 16;
            int startX = 0;
            int startY = 0;

            _sprBody = new AnimatedSprite(@"Textures\Actors\Summons\" + stringData["Texture"]);
            _sprBody.AddAnimation(AnimationEnum.Spawn, startX, startY, iFrameSize, iFrameSize, int.Parse(spawn[0]), float.Parse(spawn[1]));

            //startX += int.Parse(spawn[0]) * iFrameSize;
            //_spriteBody.AddAnimation(CActorAnimEnum.Idle, startX, startY, iFrameSize, iFrameSize, int.Parse(idle[0]), float.Parse(idle[1]));
            //startX += int.Parse(idle[0]) * iFrameSize;
            //_spriteBody.AddAnimation(CActorAnimEnum.Cast, startX, startY, iFrameSize, iFrameSize, int.Parse(cast[0]), float.Parse(cast[1]));
            //startX += int.Parse(cast[0]) * iFrameSize;
            //_spriteBody.AddAnimation(CActorAnimEnum.Attack, startX, startY, iFrameSize, iFrameSize, int.Parse(attack[0]), float.Parse(attack[1]));

            _sprBody.SetNextAnimation(Util.GetEnumString(AnimationEnum.Spawn), Util.GetActorString(VerbEnum.Idle, DirectionEnum.Down));
            _sprBody.PlayAnimation(AnimationEnum.Spawn);
            _sprBody.SetScale(5);
        }

        public void SetStats(int magStat)
        {
            _iMagStat = magStat;
            _iStrength = 2 * magStat + 10;
            _iDefense = 2 * magStat + 10;
            _iVitality = (3 * magStat) + 80;
            _iMagic = 2 * magStat + 10;
            _iResistance = 2 * magStat + 10;
            _iSpeed = 10;

            for (int i = 0; i < _liBuffedStats.Count; i++)
            {
                _liBuffedStats[i] = new KeyValuePair<StatEnum, int>(_liBuffedStats[i].Key, magStat/2);
            }

            CurrentHP = MaxHP;
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
