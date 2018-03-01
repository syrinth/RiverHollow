
using RiverHollow.Game_Managers.GUIComponents.GUIObjects;
using RiverHollow.Game_Managers.GUIObjects;
using RiverHollow.Screens;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using static RiverHollow.Game_Managers.ObjectManager;
using RiverHollow.Items;
using RiverHollow.Characters.NPCs;

namespace RiverHollow.Game_Managers.GUIComponents.Screens
{
    class ItemCreationScreen : GUIScreen
    {
        const int _iBoxSize = 64;
        const int _iMargin = 3;
        const int _iMaxColumns = 4;

        Crafter _craftMachine;
        WorldAdventurer _craftAdventurer;
        private Inventory _inventory;
        private GUIWindow _creationWindow;
        protected GUIItemBox[,] _displayList;

        private int _rows;
        private int _columns;

        public ItemCreationScreen()
        {
            Setup(ObjectManager.DictCrafting);

            Controls.Add(_creationWindow);
            Controls.Add(_inventory);
        }

        public ItemCreationScreen(Crafter crafter)
        {
            _craftMachine = crafter;
            Setup(crafter.CraftList);

            Controls.Add(_creationWindow);
            Controls.Add(_inventory);
        }

        public ItemCreationScreen(WorldAdventurer crafter)
        {
            _craftAdventurer = crafter;
            Setup(crafter.CraftList);

            Controls.Add(_creationWindow);
            Controls.Add(_inventory);
        }

        public void Setup(Dictionary<int, Recipe> recipes)
        {
            GameManager.Pause();
            Vector2 centerPoint = new Vector2(RiverHollow.ScreenWidth / 2, RiverHollow.ScreenHeight / 2);

            List<int> canMake = new List<int>();
            foreach (int id in recipes.Keys)
            {
                //Ensure that either the creation of the item is enabled by a crafter or that the player knows the recipe themselves
                if (_craftAdventurer != null || _craftMachine != null || PlayerManager.CanMake.Contains(id))
                {
                    canMake.Add(id);
                }
            }

            _columns = (canMake.Count < _iMaxColumns) ? canMake.Count : _iMaxColumns;
            _rows = (canMake.Count < _iMaxColumns) ? 1 : Math.Max(1, canMake.Count / _columns);

            _displayList = new GUIItemBox[_columns, _rows];
            _inventory = new Inventory(new Vector2(RiverHollow.ScreenWidth / 2, RiverHollow.ScreenHeight / 2), 4, InventoryManager.maxItemColumns, 32);

            int creationWidth = (GUIWindow.RedDialogEdge * 2) + (_columns * _iBoxSize) + (_iMargin * (_columns + 1));
            int creationHeight = (GUIWindow.RedDialogEdge * 2) + (_rows * _iBoxSize) + (_iMargin * (_rows + 1));

            _creationWindow = new GUIWindow(new Vector2(RiverHollow.ScreenWidth / 2, RiverHollow.ScreenHeight / 2), GUIWindow.RedDialog, GUIWindow.RedDialogEdge, creationWidth, creationHeight);            

            //Move the windows so that they are synched up appropriately
            Vector2 contWidthHeight = new Vector2(_creationWindow.Width, _creationWindow.Height);
            Vector2 mainWidthHeight = new Vector2(_inventory.Width, _inventory.Height);
            Vector2 newPos = centerPoint - new Vector2((contWidthHeight.X / 2), contWidthHeight.Y + GUIWindow.BrownDialogEdge + GUIWindow.RedDialogEdge);
            _creationWindow.Position = newPos;

            _inventory.Setup(centerPoint - new Vector2(mainWidthHeight.X / 2, 0));

            int i = 0; int j = 0;
            foreach (int id in canMake)
            {
                //Ensure that either the creation of the item is enabled by a crafter or that the player knows the recipe themselves
                if (_craftAdventurer != null || _craftMachine != null || PlayerManager.CanMake.Contains(id))
                {
                    int xMod = _creationWindow.EdgeSize + _iMargin * (i + 1) + (_iBoxSize * i);
                    int yMod = _creationWindow.EdgeSize + _iMargin * (j + 1) + (_iBoxSize * j);
                    Rectangle displayBox = new Rectangle((int)_creationWindow.Position.X + xMod, (int)_creationWindow.Position.Y + yMod, _iBoxSize, _iBoxSize);
                    _displayList[i, j] = new GUIItemBox(displayBox.Location.ToVector2(), new Rectangle(288, 32, 32, 32), displayBox.Width, displayBox.Height, @"Textures\Dialog", ObjectManager.GetItem(id));

                    i++;
                    if (i == _columns)
                    {
                        i = 0;
                        j++;
                        displayBox.X = (int)_creationWindow.Position.X + _iBoxSize + _iMargin;
                        displayBox.Y += _iBoxSize + _iMargin;
                    }
                    else
                    {
                        displayBox.X += _iBoxSize + _iMargin;
                    }
                }
            }
        }

        public override bool ProcessLeftButtonClick(Point mouse)
        {
            bool rv = false;
            if (_creationWindow.Contains(mouse))
            {
                foreach (GUIItemBox gIB in _displayList)
                {
                    if (gIB != null && gIB.Contains(mouse))
                    {
                        //Check that all required items are there first
                        bool create = true;
                        foreach(KeyValuePair<int, int> kvp in ObjectManager.DictCrafting[gIB.Item.ItemID].RequiredItems)
                        {
                            if(!InventoryManager.HasItemInInventory(kvp.Key, kvp.Value))
                            {
                                create = false;
                            }
                        }
                        //If all items are found, then remove them.
                        if (create)
                        {
                            foreach (KeyValuePair<int, int> kvp in ObjectManager.DictCrafting[gIB.Item.ItemID].RequiredItems)
                            {
                                InventoryManager.RemoveItemsFromInventory(kvp.Key, kvp.Value);
                                if (_craftMachine != null) {
                                    _craftMachine.MakeChosenItem(gIB.Item.ItemID);
                                    GameManager.BackToMain();
                                }
                                else if (_craftAdventurer != null)
                                {
                                    _craftAdventurer.ProcessChosenItem(gIB.Item.ItemID);
                                    GameManager.BackToMain();
                                }
                                else {
                                    InventoryManager.AddNewItemToInventory(gIB.Item.ItemID);
                                }
                            }
                        }
                    }
                }
                rv = true;
            }
            return rv;
        }
        public override bool ProcessRightButtonClick(Point mouse)
        {
            bool rv = false;
            if (!_creationWindow.Contains(mouse))
            {
                GameManager.BackToMain();
                rv = true;
            }
            return rv;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);

            foreach (GUIItemBox gIB in _displayList)
            {
                if (gIB != null)
                {
                    gIB.Draw(spriteBatch);
                }
            }
            foreach (GUIItemBox gIB in _displayList)
            {
                if (gIB != null)
                {
                    if (gIB.DrawDescription(spriteBatch))
                    {
                        break;
                    }
                }
            }
        }

        public override bool IsItemCreationScreen() { return true; }
    }
}
