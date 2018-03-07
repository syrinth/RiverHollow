using System.Collections.Generic;
using RiverHollow.Game_Managers;
using static RiverHollow.Game_Managers.GameManager;

namespace RiverHollow.Items
{
    public class StaticItem : Item
    {
        public StaticItem() { }
        public StaticItem(int id, string[] stringData)
        {
            ImportBasics(stringData, id, 1);
            _texture = GameContentManager.GetTexture(@"Textures\items");
        }
    }

    public class ContainerItem : StaticItem
    {
        private Item[,] _inventory;
        public Item[,] Inventory { get => _inventory; }

        private int _rows;
        public int Rows { get => _rows; }

        private int _columns;
        public int Columns { get => _columns; }

        public ContainerItem(int id, string[] stringData)
        {
            int i = ImportBasics(stringData, id, 1);
            _rows = int.Parse(stringData[i++]);
            _columns = int.Parse(stringData[i++]);
            _texture = GameContentManager.GetTexture(@"Textures\worldObjects");

            _pickup = false;
            _inventory = new Item[InventoryManager.maxItemRows, InventoryManager.maxItemColumns];
        }

        public override string GetDescription()
        {
            string rv = base.GetDescription();
            rv += System.Environment.NewLine;
            rv += "Holds " + Rows * Columns + " items";

            return rv;
        }

        internal ContainerData SaveData()
        {
            ContainerData containerData = new ContainerData
            {
                staticItemID = this.ItemID,
                x = (int)this.Position.X,
                y = (int)this.Position.Y
            };


            containerData.Items = new List<ItemData>();
            foreach (Item i in (this.Inventory))
            {
                ItemData itemData = new ItemData();
                if (i != null)
                {
                    itemData.itemID = i.ItemID;
                    itemData.num = i.Number;
                }
                else
                {
                    itemData.itemID = -1;
                }
                containerData.Items.Add(itemData);
            }
            return containerData;
        }
    }
}
