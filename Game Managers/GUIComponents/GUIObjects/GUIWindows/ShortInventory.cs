using Adventure.Game_Managers.GUIObjects;
using Adventure.Screens;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Adventure.Game_Managers.GUIComponents.GUIObjects.GUIWindows
{
    public class ShortInventory : Inventory
    {
        protected GUIImage _selection;

        public ShortInventory(Vector2 center, int columns, int edgeSize) : base( center, 1, columns, edgeSize)
        {
            _selection = new GUIImage(_displayList[0,0].Position, new Rectangle(0, 0, 32, 32), 32, 32, @"Textures\Selection");
        }

        public override bool ProcessLeftButtonClick(Point mouse)
        {
            bool rv = false;

            for(int i=0; i<_displayList.Length; i++)
            {
                if (_displayList[0,i].Rectangle.Contains(mouse))
                {
                    PlayerManager.Player.CurrentItemNumber = i;
                    break;
                }
            }
            
            return rv;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            _selection.MoveImage(_displayList[0, PlayerManager.Player.CurrentItemNumber].Position);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
            _selection.Draw(spriteBatch);
        }
    }
}
