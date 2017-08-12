using Microsoft.Xna.Framework.Graphics;

namespace Adventure.Items
{
    public class InventoryItem : Item
    {
        private bool _doesItStack;
        public bool DoesItStack { get => _doesItStack; }

        protected int _num;
        public int Number { get => _num; set => _num = value; }

        public InventoryItem(ItemList.ItemIDs ID, string texture, string name, string description, bool stacks) : base(ID, name, texture, description)
        {
            _doesItStack = stacks;
            _num = 1;
        }
    }
}
