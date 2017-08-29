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
        #endregion

        private static Dictionary<int, string> _itemDictionary;
        private static Dictionary<int, Recipe> _craftingDictionary;
        public static Dictionary<int, Recipe> CraftingDictionary { get => _craftingDictionary; }

        public static void LoadContent(ContentManager Content)
        {
            _itemDictionary = Content.Load<Dictionary<int, string>>(@"Data\ItemData");
            LoadRecipes(Content);
        }

        private static void LoadRecipes(ContentManager Content)
        {
            _craftingDictionary = new Dictionary<int, Recipe>();
            foreach (KeyValuePair<int, string> kvp in Content.Load<Dictionary<int, string>>(@"Data\CraftingData"))
            {
                _craftingDictionary.Add(kvp.Key, new Recipe(kvp.Key, kvp.Value));
            }
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
            if (id != -1)
            {
                string _itemData = _itemDictionary[id];
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
                    return new Tree(ObjectIDs.Tree, 10, false, true, pos, GetTexture(@"Textures\tree"), 1, TileMap.TileSize * 3, TileMap.TileSize * 4);
            }
            return null;
        }

        private static Texture2D GetTexture(string texture)
        {
            return GameContentManager.GetTexture(texture);
        }


        public class Recipe
        {
            private int _item;
            private Dictionary<int, int> _requiredItems;
            public Dictionary<int, int> RequiredItems { get => _requiredItems; }

            public Recipe(int id, string data)
            {
                _item = id;
                _requiredItems = new Dictionary<int, int>();

                string[] _recipeDataValues = data.Split('/');
                foreach (string s in _recipeDataValues)
                {
                    string[] itemParams = s.Split(' ');
                    _requiredItems.Add(int.Parse(itemParams[0]), int.Parse(itemParams[1]));
                }
            }
        }
    }
}
