using RiverHollow.Game_Managers.GUIComponents.GUIObjects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.Characters;

namespace RiverHollow.Game_Managers.GUIObjects
{
    public class StatDisplay : GUIWindow
    {
        public enum Display { Energy, Health};

        private Display _toDisplay;
        CombatCharacter _character;
        public float _percentage;

        public StatDisplay(Display what, Vector2 pos, int squareSize) : base(pos, new Vector2(0, 0), squareSize, 200, 32)
        {
            _toDisplay = what;
            _percentage = 0;
        }

        public StatDisplay(Display what, CombatCharacter c, Vector2 pos, int squareSize) : base(pos, new Vector2(0, 0), squareSize, 200, 32)
        {
            _character = c;
            _toDisplay = what;
            _percentage = 0;
        }

        public StatDisplay(Display what, CombatCharacter c, Vector2 pos, int width, int squareSize) : base(pos, new Vector2(0, 0), squareSize, width, 32)
        {
            _character = c;
            _toDisplay = what;
            _percentage = 0;
        }

        public override void Update(GameTime gameTime)
        {
            if (_character == null)
            {
                if (_toDisplay == Display.Health) { _percentage = ((float)PlayerManager.HitPoints / (float)PlayerManager.MaxHitPoints); }
                else if (_toDisplay == Display.Energy) { _percentage = (PlayerManager.Stamina / (float)PlayerManager.MaxStamina); }
            }
            else
            {
                _percentage = ((float)_character.HitPoints / (float)_character.MaxHitPoints);
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