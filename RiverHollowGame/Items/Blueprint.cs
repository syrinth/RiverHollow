using RiverHollow.Game_Managers;
using System;

namespace RiverHollow.Items
{
    public class Blueprint : Item
    {
        private readonly int[] _arrCraftingUnlocks;

        public override bool Usable => true;
        public Blueprint(int id) : base(id, 1)
        {
            if (GetBoolByIDKey("ObjectID")) { _arrCraftingUnlocks = Array.ConvertAll(FindParamsByIDKey("ObjectID"), s => int.Parse(s)); ; }

            _texTexture = DataManager.GetTexture(DataManager.FOLDER_ITEMS + "Blueprints");
        }

        public override bool HasUse() { return true; }

        public override void UseItem()
        {
            if (_arrCraftingUnlocks != null)
            {
                PlayerManager.AddToCraftingDictionary(_arrCraftingUnlocks);
            }

            Remove(1);
        }
    }
}
