using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace Adventure
{
    public class Sprite
    {
        protected Texture2D _texture { get; set; }
        protected int _rows { get; set; }
        protected int _columns { get; set; }
        protected int _currentFrame;
        protected int _totalFrames;
        protected string _assetName;
        private float _scale = 1.0f;
        public Vector2 _position;
        public Rectangle _size;

        public Sprite(ContentManager content, int rows, int columns, int x, int y)
        {
            _rows = rows;
            _columns = columns;
            _currentFrame = 0;
            _totalFrames = _rows * _columns;
            _position = new Vector2(x, y);
        }

        public void Update()
        {
            _currentFrame++;
            if (_currentFrame == _totalFrames)
                _currentFrame = 0;
        }

        public float Scale
        {
            get { return _scale;}
            set 
            {
                _scale = value;
                //Recalculate the Size of the Sprite with the new scale
                _size = new Rectangle(0, 0, (int)(_texture.Width * _scale), (int)(_texture.Height * _scale));
            }
        }

        public void LoadContent(ContentManager theContentManager, string theAssetName)
        {
            _texture = theContentManager.Load<Texture2D>(theAssetName);
            _assetName = theAssetName;
            _size = new Rectangle(0, 0, (int)(_texture.Width), (int)(_texture.Height));
        }

        public void Update(GameTime theGameTime, Vector2 theSpeed, Vector2 theDirection)
        {
            _position += theDirection * theSpeed * (float)theGameTime.ElapsedGameTime.TotalSeconds;
        }

        public void Draw(SpriteBatch spriteBatch, Vector2 location)
        {
            spriteBatch.Draw(_texture, _position, new Rectangle(0, 0, _texture.Width, _texture.Height), Color.White, 0.0f, Vector2.Zero, Scale, SpriteEffects.None, 0);
        }
    }
}
