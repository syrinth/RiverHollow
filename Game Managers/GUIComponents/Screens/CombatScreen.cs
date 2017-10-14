using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Adventure.Characters;

namespace Adventure.Game_Managers.GUIObjects
{
    public class CombatScreen : GUIScreen
    {
        private const int _positions = 4;
        private GUIImage _background;
        private Position[] _arrayParty;
        private Position[] _arrayEnemies;
        public CombatScreen()
        {
            _background = new GUIImage(new Vector2(0, 0), new Rectangle(0, 0, 800, 480), RiverHollow.ScreenWidth, RiverHollow.ScreenHeight,GameContentManager.GetTexture(@"Textures\battle"));
            _arrayParty = new Position[_positions];
            _arrayEnemies = new Position[_positions];
            Controls.Add(_background);
        }

        public override bool ProcessLeftButtonClick(Point mouse)
        {
            bool rv = false;
            return rv;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
            foreach(Position p in _arrayParty)
            {
                //p.Draw(spriteBatch);
            }
        }
    }

    class Position
    {
        Rectangle _rect;
        CombatCharacter _character;
        public Position(Rectangle r)
        {
            _rect = r;
            _character = null;
        }

        public void Draw()
        {
            _character.Draw(_rect);
        }
    }
}
