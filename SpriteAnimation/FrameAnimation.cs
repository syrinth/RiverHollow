using System;
using Microsoft.Xna.Framework;

using static RiverHollow.Game_Managers.GameManager;
namespace RiverHollow.SpriteAnimations
{
    public class FrameAnimation :ICloneable
    {
        // The first frame of the Animation.  We will calculate other
        // frames on the fly based on this frame.
        private Rectangle rectInitialFrame;

        // Number of frames in the Animation
        private int _iFrameCount = 1;

        // The frame currently being displayed. 
        // This value ranges from 0 to iFrameCount-1
        private int _iCurrFrame = 0;

        // Amount of time (in seconds) to display each frame
        private float _fFrameLength = 0.2f;

        // Amount of time that has passed since we last animated
        private float _fFrameTimer = 0.0f;
        public float FrameTimer
        {
            get { return _fFrameTimer; }
            set { _fFrameTimer = value; }
        }

        // The number of times this animation has been played
        private int _iPlayCount = 0;

        // The animation that should be played after this animation
        private string eNextAnimation = string.Empty;

        bool _bBackTracking;
        bool _bPingPong = true;

        /// 
        /// The number of frames the animation contains
        /// 
        public int FrameCount
        {
            get { return _iFrameCount; }
            set { _iFrameCount = value; }
        }

        /// 
        /// The time (in seconds) to display each frame
        /// 
        public float FrameLength
        {
            get { return _fFrameLength; }
            set { _fFrameLength = value; }
        }

        /// 
        /// The frame number currently being displayed
        /// 
        public int CurrentFrame
        {
            get { return _iCurrFrame; }
            set { _iCurrFrame = (int)MathHelper.Clamp(value, 0, _iFrameCount - 1); }
        }

        public int FrameWidth
        {
            get { return rectInitialFrame.Width; }
        }

        public int FrameHeight
        {
            get { return rectInitialFrame.Height; }
        }

        /// 
        /// The rectangle associated with the current
        /// animation frame.
        /// 
        public Rectangle FrameRectangle
        {
            get
            {
                return new Rectangle(
                    rectInitialFrame.X + (rectInitialFrame.Width * _iCurrFrame),
                    rectInitialFrame.Y, rectInitialFrame.Width, rectInitialFrame.Height);
            }
        }

        public int PlayCount
        {
            get { return _iPlayCount; }
            set { _iPlayCount = value; }
        }

        public string NextAnimation
        {
            get { return eNextAnimation; }
            set { eNextAnimation = value; }
        }

        public FrameAnimation(Rectangle FirstFrame, int Frames)
        {
            rectInitialFrame = FirstFrame;
            _iFrameCount = Frames;
        }

        public FrameAnimation(int X, int Y, int Width, int Height, int Frames)
        {
            rectInitialFrame = new Rectangle(X, Y, Width, Height);
            _iFrameCount = Frames;
        }

        public FrameAnimation(int X, int Y, int Width, int Height, int Frames, float FrameLength)
        {
            rectInitialFrame = new Rectangle(X, Y, Width, Height);
            _iFrameCount = Frames;
            _fFrameLength = FrameLength;
        }

        public FrameAnimation(int X, int Y,
            int Width, int Height, int Frames,
            float FrameLength, bool pingPong, string sNextAnimation)
        {
            rectInitialFrame = new Rectangle(X, Y, Width, Height);
            _iFrameCount = Frames;
            _fFrameLength = FrameLength;
            eNextAnimation = sNextAnimation;
            _bPingPong = pingPong;
        }

        public void Update(GameTime gameTime)
        {
            if(_iFrameCount > 1) { 
            _fFrameTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;

                if (_fFrameTimer > _fFrameLength)
                {
                    _fFrameTimer = 0.0f;

                    if (_bPingPong)
                    {
                        if (!_bBackTracking && _iCurrFrame + 1 <= _iFrameCount)
                        {
                            _iCurrFrame++;
                        }
                        else if (_bBackTracking && _iCurrFrame - 1 >= 0)
                        {
                            _iCurrFrame--;
                        }

                        if (_iCurrFrame == 0) {
                            _bBackTracking = false;
                            _iPlayCount = (int)MathHelper.Min(_iPlayCount + 1, int.MaxValue);
                        }
                        else if (_iCurrFrame == (_iFrameCount - 1)) {
                            _bBackTracking = true;
                        }
                    }
                    else
                    {
                        if (_iCurrFrame + 1 < _iFrameCount) { _iCurrFrame++; }
                        else
                        {
                            _iPlayCount = (int)MathHelper.Min(_iPlayCount + 1, int.MaxValue);
                            _iCurrFrame = 0;
                        }
                    }
                }
            }
            else if (_iFrameCount == 1)
            {
                _fFrameTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;

                if (_fFrameTimer > _fFrameLength)
                {
                    _fFrameTimer = 0.0f;
                    _iPlayCount = (int)MathHelper.Min(_iPlayCount + 1, int.MaxValue);
                }
            }
        }

        object ICloneable.Clone()
        {
                return new FrameAnimation(this.rectInitialFrame.X, this.rectInitialFrame.Y,
                                          this.rectInitialFrame.Width, this.rectInitialFrame.Height,
                                          this._iFrameCount, this._fFrameLength, this._bPingPong, eNextAnimation);
        }
    }
}
