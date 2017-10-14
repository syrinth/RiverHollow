using Microsoft.Xna.Framework;
using Adventure.Screens;
using Microsoft.Xna.Framework.Graphics;
using Adventure.Game_Managers.GUIComponents.GUIObjects.GUIWindows;
using Microsoft.Xna.Framework.Input;

namespace Adventure.Game_Managers.GUIObjects
{
    public class CombatScreen : GUIScreen
    {
        private GUIImage _background;
        public CombatScreen()
        {
            _background = new GUIImage(new Vector2(0, 0), new Rectangle(0, 0, 800, 480), AdventureGame.ScreenWidth, AdventureGame.ScreenHeight,GameContentManager.GetTexture(@"Textures\battle"));
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
        }
    }
}
