using Adventure.Game_Managers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Adventure.Items
{
    public class InventoryItem : Item
    {
        protected bool _doesItStack;
        public bool DoesItStack { get => _doesItStack; }

        protected int _num;
        public int Number { get => _num; set => _num = value; }

        public InventoryItem(ObjectManager.ItemIDs ID, Texture2D texture, string name, string description, int number, bool stacks) : base(ID, name, texture, description)
        {
            _doesItStack = stacks;
            _num = number;
        }

        //Copy Constructor
        public InventoryItem(InventoryItem item) : base(item.ItemID, item.Name, item.Texture, item._description)
        {
            _num = item.Number;
            _doesItStack = item.DoesItStack;
        }

        public virtual void Draw(SpriteBatch spriteBatch, Rectangle drawBox)
        {
            spriteBatch.Draw(_texture, drawBox, Color.White);
        }
    }
}
