using RiverHollow.Game_Managers.GUIObjects;
using RiverHollow.Screens;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace RiverHollow.Game_Managers.GUIComponents.GUIObjects.GUIWindows
{
    public class ShortInventory : Inventory
    {
        protected GUIImage _selection;

        public ShortInventory(Vector2 center, int columns, int edgeSize) : base( center, 1, columns, edgeSize)
        {
            _selection = new GUIImage(_displayList[0,0].Position, new Rectangle(288, 0, 32, 32), 32, 32, @"Textures\Dialog");
        }

        public override bool ProcessLeftButtonClick(Point mouse)
        {
            bool rv = false;

            for(int i=0; i<_displayList.Length; i++)
            {
                if (_displayList[0,i].Contains(mouse))
                {
                    InventoryManager.CurrentItemNumber = i;
                    break;
                }
            }
            
            return rv;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            _selection.MoveImageTo(_displayList[0, InventoryManager.CurrentItemNumber].Position);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
            _selection.Draw(spriteBatch);
        }
    }
}
