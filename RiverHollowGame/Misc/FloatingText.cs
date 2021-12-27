using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.BitmapFonts;
using RiverHollow.Game_Managers;
using RiverHollow.Utilities;

using static RiverHollow.Game_Managers.GameManager;

namespace RiverHollow.Misc
{
    public class FloatingText
    {
        Vector2 Position;
        protected BitmapFont _font;
        protected Color _cTextColor;
        const double VANISH_AFTER_DEF = 1.0;
        double _dVanishAfter;
        double _dCountDown = 0;
        protected string _sText;

        public FloatingText(Vector2 position, int spriteWidth, string text, Color c)
        {
            RHRandom rand = RHRandom.Instance();

            _font = DataManager.GetBitMapFont(@"Fonts\FontBattle");
            _sText = text;
            _cTextColor = c;

            Position = position;
            _dVanishAfter = VANISH_AFTER_DEF - (0.05 * (double)rand.Next(0, 5));

            Position.X += spriteWidth / 2; //get the center
            Position.X += rand.Next(-8, 8); //displace the x position
            Position.Y -= rand.Next(0, TILE_SIZE / 2); //Subtract this number to go 'up' the screen
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.DrawString(_font, _sText, Position, _cTextColor, Position.Y, null);
        }

        public void Update(GameTime gTime)
        {
            Position += new Vector2(0, -0.5f);
            _dCountDown += gTime.ElapsedGameTime.TotalSeconds;
            if (_dCountDown >= _dVanishAfter)
            {

            }
        }
    }
}
