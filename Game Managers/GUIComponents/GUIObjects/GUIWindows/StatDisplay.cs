using Adventure.Game_Managers.GUIComponents.GUIObjects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Adventure.Game_Managers.GUIObjects
{
    public class StatDisplay : GUIWindow
    {
        public enum Display { Energy, Health};

        private Display _toDisplay;
        public float _percentage;

        public StatDisplay(Display what, Vector2 pos, int squareSize) : base(pos, new Vector2(0, 0), squareSize, 200, 32)
        {
            _toDisplay = what;
            _percentage = 0;
        }

        public override void Update(GameTime gameTime)
        {
            if (_toDisplay == Display.Health)
            {
                _percentage = ((float)PlayerManager.Player.HitPoints / (float)PlayerManager.Player.MaxHitPoints);
            }
            else if (_toDisplay == Display.Energy)
            {
                _percentage = (PlayerManager.Player.Stamina / (float)PlayerManager.Player.MaxStamina);
            }
            base.Update(gameTime);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            DrawTop(spriteBatch);
            DrawMiddleEdges(spriteBatch);
            DrawCenter(spriteBatch, _percentage);
            DrawBottom(spriteBatch);
        }
    }
}