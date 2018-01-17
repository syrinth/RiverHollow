using RiverHollow.Game_Managers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace RiverHollow.Items
{
    public class Container : StaticItem
    {
        private Item[,] _inventory;
        public Item[,] Inventory { get => _inventory; }

        private int _rows;
        public int Rows { get => _rows; }

        private int _columns;
        public int Columns { get => _columns; }

        public Container(int id, string[] itemValue)
        {
            int i = ImportBasics(itemValue, id, 1);
            _rows = int.Parse(itemValue[i++]);
            _columns = int.Parse(itemValue[i++]);
            _texture = GameContentManager.GetTexture(@"Textures\worldObjects");

            _pickup = false;
            _inventory = new Item[InventoryManager.maxItemRows, InventoryManager.maxItemColumns];

            CalculateSourcePos();
        }

        public override string GetDescription()
        {
            string rv = base.GetDescription();
            rv += System.Environment.NewLine;
            rv += "Holds " + Rows * Columns + " items";

            return rv;
        }
    }
}
