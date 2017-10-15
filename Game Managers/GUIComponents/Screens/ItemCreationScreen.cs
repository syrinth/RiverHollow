
using Adventure.Game_Managers.GUIComponents.GUIObjects;
using Adventure.Game_Managers.GUIObjects;
using Adventure.Screens;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using static Adventure.Game_Managers.ObjectManager;

namespace Adventure.Game_Managers.GUIComponents.Screens
{
    class ItemCreationScreen : GUIScreen
    {
        private Inventory _inventory;
        private GUIWindow _creationWindow;
        protected GUIItemBox[,] _displayList;

        private int _rows;
        private int _columns;

        public ItemCreationScreen()
        {
            Vector2 centerPoint = new Vector2(RiverHollow.ScreenWidth / 2, RiverHollow.ScreenHeight / 2);
            _columns = 4;
            _rows = (_columns > 0) ? Math.Min(1, PlayerManager.CanMake.Count / _columns) : 1;
            if(_rows == 0) { _rows = 1; }
            _displayList = new GUIItemBox[_rows, _columns];
            _creationWindow = new GUIWindow(new Vector2(RiverHollow.ScreenWidth / 2, RiverHollow.ScreenHeight / 2), new Vector2(0, 0), 32, 96, 96);
            _inventory = new Inventory(new Vector2(RiverHollow.ScreenWidth / 2, RiverHollow.ScreenHeight / 2), 4, InventoryManager.maxItemColumns, 32);

            Vector2 contWidthHeight = new Vector2(_creationWindow.Width, _creationWindow.Height);
            Vector2 mainWidthHeight = new Vector2(_inventory.Width, _inventory.Height);
            Vector2 newPos = centerPoint - new Vector2((contWidthHeight.X / 2), contWidthHeight.Y);
            _creationWindow.Position = newPos;

            _inventory.SetPosition(centerPoint - new Vector2(mainWidthHeight.X / 2, 0));

            int i = 0; int j = 0;
            foreach (int id in ObjectManager.DictCrafting.Keys)
            {
                if (PlayerManager.CanMake.Contains(id)){
                    Rectangle displayBox = new Rectangle((int)_creationWindow.Position.X + 32 + 3, (int)_creationWindow.Position.Y + 32 + 3, 32, 32);
                    _displayList[i, j] = new GUIItemBox(displayBox.Location.ToVector2(), new Rectangle(288, 32, 32, 32), displayBox.Width, displayBox.Height, @"Textures\Dialog", ObjectManager.GetItem(id));
                    i++;
                    if (i == _columns)
                    {
                        i = 0;
                        j++;
                        displayBox.X = (int)_creationWindow.Position.X + 32 + 3;
                        displayBox.Y += 32 + 3;
                    }
                    else {
                        displayBox.X += 32 + 3;
                    }
                }
            }

            Controls.Add(_creationWindow);
            Controls.Add(_inventory);
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
                                InventoryManager.AddItemToFirstAvailableInventorySpot(gIB.Item.ItemID);
                            }
                        }
                    }
                }
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
        }
    }
}
