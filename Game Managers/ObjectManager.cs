﻿using Adventure.Characters.NPCs;
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
        private static Dictionary<int, string> _dictBuilding;
        private static Dictionary<int, Recipe> _dictCrafting;
        private static Dictionary<int, string> _dictItem;
        private static Dictionary<int, string> _dictWorkers;
        public static Dictionary<int, Recipe> DictCrafting { get => _dictCrafting; }

        public static void LoadContent(ContentManager Content)
        {
            _dictBuilding = Content.Load<Dictionary<int, string>>(@"Data\Buildings");
            _dictItem = Content.Load<Dictionary<int, string>>(@"Data\ItemData");
            _dictWorkers = Content.Load<Dictionary<int, string>>(@"Data\Workers");
            LoadRecipes(Content);
        }

        private static void LoadRecipes(ContentManager Content)
        {
            _dictCrafting = new Dictionary<int, Recipe>();
            foreach (KeyValuePair<int, string> kvp in Content.Load<Dictionary<int, string>>(@"Data\CraftingData"))
            {
                _dictCrafting.Add(kvp.Key, new Recipe(kvp.Key, kvp.Value));
            }
        }

        public static Building GetBuilding(int id)
        {
            if (_dictBuilding.ContainsKey(id))
            {
                string buildingData = _dictBuilding[id];
                string[] _buildingDataValues = buildingData.Split('/');
                return new Building(_buildingDataValues, id);
            }
            return null;     
        }

        public static Worker GetWorker(int id)
        {
            if (_dictWorkers.ContainsKey(id))
            {
                string stringData = _dictWorkers[id];
                string[] stringDataValues = stringData.Split('/');
                return new Worker(stringDataValues, id);
            }
            return null;
        }

        public static Item GetItem(int id)
        {
            return GetItem(id, 1);
        }

        public static Item GetItem(int id, int num)
        {
            if (id != -1)
            {
                string _itemData = _dictItem[id];
                string[] _itemDataValues = _itemData.Split('/');
                switch (_itemDataValues[0])
                {
                    case "Resource":
                        return new Item(id, _itemDataValues, num);
                    case "Tool":
                        return new Tool(id, _itemDataValues);
                    case "Weapon":
                        return new Weapon(id, _itemDataValues);
                    case "Container":
                        return new Container(id, _itemDataValues);
                    case "Food":
                        return new Food(id, _itemDataValues, num);
                    case "Map":
                        return new AdventureMap(id, _itemDataValues, num);
                }
            }
            return null;
        }

        public static WorldObject GetWorldObject(int id, Vector2 pos)
        {
            switch (id)
            {
                case 0:
                    return new WorldObject(id, 1, true, true, false, pos, new Rectangle(0, 0, 32, 32), GetTexture(@"Textures\worldObjects"), 1, RHTileMap.TileSize, RHTileMap.TileSize);
                case 1:
                    return new WorldObject(id, 5, true, true, false, pos, new Rectangle(64, 64, 64, 64), GetTexture(@"Textures\worldObjects"), 1, RHTileMap.TileSize*2, RHTileMap.TileSize*2);
                case 2:
                    return new Tree(id, 10, false, true, pos, new Rectangle(0, 0, 96, 128), GetTexture(@"Textures\tree"), 1, RHTileMap.TileSize * 3, RHTileMap.TileSize * 4);
                case 3:
                    return new Staircase(id, pos, new Rectangle(96, 0, 32, 32), GetTexture(@"Textures\worldObjects"), 1, RHTileMap.TileSize, RHTileMap.TileSize);
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
