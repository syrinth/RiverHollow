using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Adventure
{
    class Player : Sprite
    {
        const string ASSETNAME = "SmileyWalk";
        const int SPEED = 160;
        const int MOVE_UP = -1;
        const int MOVE_DOWN = 1;
        const int MOVE_LEFT = -1;
        const int MOVE_RIGHT = 1;

        enum State
        {
            Walking
        }
        State mCurrentState = State.Walking;

        Vector2 mDirection = Vector2.Zero;
        Vector2 mSpeed = Vector2.Zero;

        KeyboardState _prevKeyboardState;

        public Player(ContentManager content, int rows, int columns, int x, int y) : base(content, rows, columns, x, y)
        {
        }

        public void LoadContent(ContentManager theContentManager)
        {
            base.LoadContent(theContentManager, ASSETNAME);
        }

        public void Update(GameTime theGameTime)
        {
            _currentFrame++;
            if (_currentFrame == _totalFrames)
            {
                _currentFrame = 0;
            }

            KeyboardState currKeyboardState = Keyboard.GetState();

            //UpdateMovement(currKeyboardState);

            _prevKeyboardState = currKeyboardState;

            base.Update(theGameTime, mSpeed, mDirection);
        }

        private void UpdateMovement(KeyboardState aCurrentKeyboardState)
        {
            if (mCurrentState == State.Walking)
            {
                mSpeed = Vector2.Zero;
                mDirection = Vector2.Zero;

                if (aCurrentKeyboardState.IsKeyDown(Keys.Left) == true)
                {
                    mSpeed.X = SPEED;
                    mDirection.X = MOVE_LEFT;
                }
                else if (aCurrentKeyboardState.IsKeyDown(Keys.Right) == true)
                {
                    mSpeed.X = SPEED;
                    mDirection.X = MOVE_RIGHT;
                }

                if (aCurrentKeyboardState.IsKeyDown(Keys.Up) == true)
                {
                    mSpeed.Y = SPEED;
                    mDirection.Y = MOVE_UP;
                }
                else if (aCurrentKeyboardState.IsKeyDown(Keys.Down) == true)
                {
                    mSpeed.Y = SPEED;
                    mDirection.Y = MOVE_DOWN;
                }
            }
        }


        public void Draw(SpriteBatch spriteBatch)
        {
            int width = _texture.Width / _columns;
            int height = _texture.Height / _rows;
            int row = (int)((float)_currentFrame / (float)_columns);
            int column = _currentFrame % _columns;

            Rectangle sourceRectangle = new Rectangle(width * column, height * row, width, height);
            Rectangle destinationRectangle = new Rectangle((int)_position.X, (int)_position.Y, width, height);

            spriteBatch.Draw(_texture, destinationRectangle, sourceRectangle, Color.White);
        }
    }
}
