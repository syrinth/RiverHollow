﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.Misc;
using RiverHollow.SpriteAnimations;
using RiverHollow.Utilities;
using System.Collections.Generic;
using static RiverHollow.Utilities.Enums;

namespace RiverHollow.Characters
{
    #region Abstract Supertypes
    ///These abstract classes are separate to compartmentalize the various properties and
    ///methods for each layer of an Actor's existence. Technically could be one large abstract class
    ///but they have been separated for ease of access

    /// <summary>
    /// The base propreties and methods for each Actor
    /// </summary>
    public abstract class Actor
    {
        protected AnimatedSprite _sprBody;
        public AnimatedSprite BodySprite => _sprBody;

        public virtual Point Position
        {
            get { return new Point(_sprBody.Position.X, _sprBody.Position.Y); }
            set { _sprBody.Position = value; }
        }
        public virtual Point Center => _sprBody.Center;

        protected int _iBodyWidth = Constants.TILE_SIZE;
        public int Width => _iBodyWidth;
        protected int _iBodyHeight = Constants.TILE_SIZE * 2;
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

        public virtual string Name()
        {
            return string.Empty;
        }

        public virtual List<AnimatedSprite> GetSprites()
        {
            List<AnimatedSprite> liRv = new List<AnimatedSprite>() { _sprBody };
            return liRv;
        }

        public virtual void PlayAnimation<TEnum>(TEnum e) { _sprBody.PlayAnimation(e); }
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
    }    
    #endregion 
}
