using Adventure.Buildings;
using Adventure.Characters.NPCs;
using Adventure.Items;
using Adventure.Tile_Engine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure.Game_Managers
{
    public static class ObjectManager
    {
        #region IDs
        public enum BuildingID
        {
            NOTHING, ArcaneTower
        }
        public enum WorkerID
        {
            Nothing, Wizard
        }
        public enum ItemIDs
        {
            Nothing, PickAxe, ArcaneEssence, CopperOre, CopperBar, IronOre, IronBar, Lumber, Stone, Sword, SmallChest
        }
        public enum ObjectIDs
        {
            Nothing, Rock, BigRock
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

        public static Worker GetWorker(WorkerID id)
        {
            switch (id)
            {
                case WorkerID.Wizard:
                    return new Wizard(Vector2.Zero);
            }
            return null;
        }

        public static InventoryItem GetItem(ItemIDs id)
        {
            return GetItem(id, 1);
        }

        public static InventoryItem GetItem(ItemIDs id, int num)
        {
            string name = "";
            string description = "";
            switch (id)
            {
                case ItemIDs.ArcaneEssence:
                    name = "Arcane Essence";
                    description = "arcane_essence";
                    return new InventoryItem(id, GetTexture(@"Textures\arcane_essence"), name, description, num, true);
                case ItemIDs.PickAxe:
                    name = "Pick Axe";
                    description = "Pick, break rocks";
                    return new Tool(id, GetTexture(@"Textures\pickAxe"), name, description, 1, 0.1f, 5);
                case ItemIDs.Sword:
                    name = "Sword";
                    description = "SWORD!";
                    return new Weapon(id, GetTexture(@"Textures\Sword"), name, description, 1, 5, 5);
                case ItemIDs.Stone:
                    name = "Stone";
                    description = "Used for building things";
                    return new InventoryItem(id, GetTexture(@"Textures\stone"), name, description, num, true);
                case ItemIDs.SmallChest:
                    name = "Small Chest";
                    description = "A small chestused to store items for later";
                    return new Container(id, GetTexture(@"Textures\chest"), name, description, 1, 8);

            }
            return null;
        }

        public static WorldObject GetWorldObject(ObjectIDs id, Vector2 pos)
        {
            switch (id)
            {
                case ObjectIDs.Rock:
                    return new WorldObject(ObjectIDs.Rock, 1, true, false, pos, GetTexture(@"Textures\rock"), 1, TileMap.TileSize, TileMap.TileSize);
            }
            return null;
        }

        private static Texture2D GetTexture(string texture)
        {
            return GameContentManager.GetTexture(texture);
        }
    }
}
