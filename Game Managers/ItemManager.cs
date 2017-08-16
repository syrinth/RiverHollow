using Adventure.Buildings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure.Game_Managers
{
    public static class ItemManager
    {
        #region BuildingIDs
        public enum BuildingID
        {
            NOTHING, ArcaneTower
        }

        #endregion

        public static Building GetBuilding(BuildingID id)
        {
            switch (id)
            {
                case BuildingID.ArcaneTower:
                    return new ArcaneTower();
            }
            return null;
        }

        // new WorldItem(id, texturename, description, position)
        /*public static InventoryItem GetInventoryItem(ItemIDs id, Vector2 position)
        {
            string texturename = "";
            string description = "";
            switch (id)
            {
                case ItemIDs.ARCANE_ESSENCE:
                    texturename = "arcane_essence";
                    description = "arcane_essence";
                    return new InventoryItem(id, texturename, description);
            }
            return null;
        }*/
    }
}
