using Adventure.Buildings;
using Adventure.Characters.NPCs;
using Adventure.Items;
using Adventure.Tile_Engine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
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
            Nothing, PickAxe, Axe, ArcaneEssence, CopperOre, CopperBar, IronOre, IronBar, Sword, SmallChest, Wood, Stone
        }
        public enum ObjectIDs
        {
            Nothing, Rock, BigRock, Tree
        }

        private static Dictionary<int, string> _itemDictionary;
        #endregion

        public static void LoadContent(ContentManager Content)
        {
            _itemDictionary = Content.Load<Dictionary<int, string>>(@"Data\Data");
        }

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

        public static InventoryItem GetItem(int id)
        {
            return GetItem(id, 1);
        }

        public static InventoryItem GetItem(int id, int num)
        {
            string _itemData = _itemDictionary[id].Replace("\"", "");
            string[] _itemDataValues = _itemData.Split('/');
            switch (_itemDataValues[0])
            {
                case "Resource":
                    return new InventoryItem(id, _itemDataValues, num);
                case "Tool":
                    return new Tool(id, _itemDataValues);
                case "Weapon":
                    return new Weapon(id, _itemDataValues);
                case "Container":
                    return new Container(id, _itemDataValues);
            }
            return null;
        }

        //public static InventoryItem GetItem(ItemIDs id, int num)
        //{
        //    string name = "";
        //    string description = "";
        //    switch (id)
        //    {
        //        //case ItemIDs.PickAxe:
        //        //    name = "Pick Axe";
        //        //    description = "Pick, break rocks";
        //        //    return new Tool(id, new Vector2(0, 0), GetTexture(@"Textures\tools"), name, description, 1, 0.1f, 5);
        //        //case ItemIDs.Axe:
        //        //    name = "Axe";
        //        //    description = "Chop chop motherfucker";
        //        //    return new Tool(id, new Vector2(0, 32), GetTexture(@"Textures\tools"), name, description, 0, 3, 5);
        //        case ItemIDs.Sword:
        //            name = "Sword";
        //            description = "SWORD!";
        //            return new Weapon(id, new Vector2(0,0), GetTexture(@"Textures\Sword"), name, description, 1, 5, 5);
        //        case ItemIDs.SmallChest:
        //            name = "Small Chest";
        //            description = "A small chestused to store items for later";
        //            List<KeyValuePair<ItemIDs, int>> reagents = new List<KeyValuePair<ItemIDs, int>>();
        //            reagents.Add(new KeyValuePair<ItemIDs, int>(ItemIDs.Wood, 2));
        //            return new Container(id, new Vector2(0, 0), GetTexture(@"Textures\chest"), name, description, 1, 8, reagents);

        //    }
        //    return null;
        //}

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
