using Adventure.Buildings;
using Adventure.Characters.NPCs;
using Adventure.Items;
using Adventure.Tile_Engine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

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
            Nothing, PickAxe, Axe, ArcaneEssence, CopperOre, CopperBar, IronOre, IronBar, Stone, Sword, SmallChest, Wood
        }
        public enum ObjectIDs
        {
            Nothing, Rock, BigRock, Tree
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
                    return new InventoryItem(id, new Vector2(0, 32), GetTexture(@"Textures\items"), name, description, num, true);
                case ItemIDs.PickAxe:
                    name = "Pick Axe";
                    description = "Pick, break rocks";
                    return new Tool(id, new Vector2(0, 0), GetTexture(@"Textures\tools"), name, description, 1, 0.1f, 5);
                case ItemIDs.Axe:
                    name = "Axe";
                    description = "Chop chop motherfucker";
                    return new Tool(id, new Vector2(0, 32), GetTexture(@"Textures\tools"), name, description, 0, 3, 5);
                case ItemIDs.Sword:
                    name = "Sword";
                    description = "SWORD!";
                    return new Weapon(id, new Vector2(0,0), GetTexture(@"Textures\Sword"), name, description, 1, 5, 5);
                case ItemIDs.Stone:
                    name = "Stone";
                    description = "Used for building things";
                    return new InventoryItem(id, new Vector2(0, 0), GetTexture(@"Textures\items"), name, description, num, true);
                case ItemIDs.Wood:
                    name = "Wood";
                    description = "Used for building things";
                    return new InventoryItem(id, new Vector2(32, 0), GetTexture(@"Textures\items"), name, description, num, true);
                case ItemIDs.SmallChest:
                    name = "Small Chest";
                    description = "A small chestused to store items for later";
                    List<KeyValuePair<ItemIDs, int>> reagents = new List<KeyValuePair<ItemIDs, int>>();
                    reagents.Add(new KeyValuePair<ItemIDs, int>(ItemIDs.Wood, 2));
                    return new Container(id, new Vector2(0, 0), GetTexture(@"Textures\chest"), name, description, 1, 8, reagents);

            }
            return null;
        }

        public static WorldObject GetWorldObject(ObjectIDs id, Vector2 pos)
        {
            switch (id)
            {
                case ObjectIDs.Rock:
                    return new WorldObject(ObjectIDs.Rock, 1, true, false, pos, GetTexture(@"Textures\rock"), 1, TileMap.TileSize, TileMap.TileSize);
                case ObjectIDs.Tree:
                    return new Tree(ObjectIDs.Tree, 10, false, true, pos, GetTexture(@"Textures\tree"), 1, TileMap.TileSize*3, TileMap.TileSize*4);
            }
            return null;
        }

        private static Texture2D GetTexture(string texture)
        {
            return GameContentManager.GetTexture(texture);
        }
    }
}
