using RiverHollow.Game_Managers.GUIComponents.GUIObjects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.Characters.CombatStuff;
using RiverHollow.GUIObjects;

namespace RiverHollow.Game_Managers.GUIObjects
{
    public class StatDisplay : GUIWindow
    {
        public enum DisplayEnum { Energy, Health};

        private DisplayEnum _toDisplay;
        CombatCharacter _character;
        private float _percentage;
        private bool _hover;
        private SpriteFont _font;

        public StatDisplay(DisplayEnum what, Vector2 pos, int squareSize) : base(pos, GUIWindow.RedDialog, squareSize, 200, 32)
        {
            _toDisplay = what;
            _percentage = 0;
            _font = GameContentManager.GetFont(@"Fonts\Font");
        }

        public StatDisplay(DisplayEnum what, CombatCharacter c, Vector2 pos, int squareSize) : base(pos, GUIWindow.RedDialog, squareSize, 200, 32)
        {
            _character = c;
            _toDisplay = what;
            _percentage = 0;
            _font = GameContentManager.GetFont(@"Fonts\Font");
        }

        public StatDisplay(DisplayEnum what, CombatCharacter c, Vector2 pos, int width, int squareSize) : base(pos, GUIWindow.RedDialog, squareSize, width, 32)
        {
            _character = c;
            _toDisplay = what;
            _percentage = 0;
            _font = GameContentManager.GetFont(@"Fonts\Font");
        }

        public override void Update(GameTime gameTime)
        {
            if (_character == null)
            {
                if (_toDisplay == DisplayEnum.Health) { _percentage = ((float)PlayerManager.HitPoints / (float)PlayerManager.MaxHitPoints); }
                else if (_toDisplay == DisplayEnum.Energy) { _percentage = (PlayerManager.Stamina / (float)PlayerManager.MaxStamina); }
            }
            else
            {
                _percentage = ((float)_character.CurrentHP / (float)_character.MaxHP);
            }
            base.Update(gameTime);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            DrawTop(spriteBatch);
            DrawMiddleEdges(spriteBatch);
            DrawCenter(spriteBatch, _percentage);
            DrawBottom(spriteBatch);

            if (_hover)
            {
                string stat = string.Empty;
                if (_character == null)
                {
                    if (_toDisplay == DisplayEnum.Health) { stat = string.Format("{0}/{1}", PlayerManager.HitPoints, PlayerManager.MaxHitPoints); }
                    else if (_toDisplay == DisplayEnum.Energy) { stat = string.Format("{0}/{1}", PlayerManager.Stamina, PlayerManager.MaxStamina); }
                }
                else
                {
                    stat = string.Format("{0}/{1}", _character.CurrentHP, _character.MaxHP);
                }
                spriteBatch.DrawString(_font, stat, new Vector2(GraphicCursor.Position.X, GraphicCursor.Position.Y-32), Color.White);
            }
        }

        public bool ProcessHover(Point mouse)
        {
            _hover = UsableRectangle().Contains(mouse);
            return _hover;
        }
    }
}