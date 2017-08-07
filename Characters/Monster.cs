using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Adventure
{
    class Monster : Sprite
    {
        const string ASSETNAME = "SmileyWalk";
        const int SPEED = 80;
        const int MOVE_UP = -1;
        const int MOVE_DOWN = 1;
        const int MOVE_LEFT = -1;
        const int MOVE_RIGHT = 1;
        const int LEASH = 200;

        enum State
        {
            Walking
        }
        //State mCurrentState = State.Walking;

        Vector2 _direction = Vector2.Zero;
        Vector2 _speed = Vector2.Zero;

        public Monster(ContentManager content, int rows, int columns, int x, int y) : base(content, rows, columns, x, y)
        {
        }

        public void LoadContent(ContentManager theContentManager)
        {
            base.LoadContent(theContentManager, ASSETNAME);
        }

        public void Update(GameTime theGameTime, Player player)
        {
            _currentFrame++;
            if (_currentFrame == _totalFrames)
            {
                _currentFrame = 0;
            }

            UpdateMovement(player);

            base.Update(theGameTime, _speed, _direction);
        }

        private void UpdateMovement(Player player)
        {
            _speed = Vector2.Zero;
            _direction = Vector2.Zero;

            if (System.Math.Abs(player._position.X - this._position.X) <= LEASH && System.Math.Abs(player._position.Y - this._position.Y) <= LEASH)
            {
                float newX = (player._position.X > this._position.X) ? 1 : -1;
                float newY = (player._position.Y > this._position.Y) ? 1 : -1;

                _direction.X = newX;
                _direction.Y = newY;

                _speed.X = SPEED;
                _speed.Y = SPEED;
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

            spriteBatch.Draw(_texture, destinationRectangle, sourceRectangle, Color.Black);
        }
    }
}
