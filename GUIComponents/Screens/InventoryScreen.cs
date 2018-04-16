using Microsoft.Xna.Framework;
using RiverHollow.Screens;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.WorldObjects;
using RiverHollow.GUIObjects;
using RiverHollow.Characters;
using System.Collections.Generic;
using static RiverHollow.WorldObjects.WorldItem;
using static RiverHollow.WorldObjects.Door;

namespace RiverHollow.Game_Managers.GUIObjects
{
    public class InventoryScreen : GUIScreen
    {
        private Inventory _inventory;
        private Inventory _container;
        private CharacterBox _character;
        private SpriteFont _font;
        private int _iCharacterIndex;

        public InventoryScreen()
        {
            _iCharacterIndex = 0;
            _character = new CharacterBox(PlayerManager.GetParty()[_iCharacterIndex], new Vector2(128, 32));
            _font = GameContentManager.GetFont(@"Fonts\Font");
            _inventory = new Inventory(4, InventoryManager.maxItemColumns, 32);
            Controls.Add(_inventory);
            Controls.Add(_character);
        }

        public InventoryScreen(CharacterBox c)
        {
            _character = c;
            _font = GameContentManager.GetFont(@"Fonts\Font");
            _inventory = new Inventory(4, InventoryManager.maxItemColumns, 32);
            Controls.Add(_inventory);
            Controls.Add(_character);
            InventoryManager.PublicContainer = null;
        }

        public InventoryScreen(Container c)
        {
            Vector2 centerPoint = new Vector2(RiverHollow.ScreenWidth / 2, RiverHollow.ScreenHeight / 2);
            _font = GameContentManager.GetFont(@"Fonts\Font");
            _container = new Inventory(c, 32);
            _inventory = new Inventory(4, InventoryManager.maxItemColumns, 32);

            Vector2 contWidthHeight = new Vector2(_container.MidWidth(), _container.InnerRectangle().Height);
            Vector2 mainWidthHeight = new Vector2(_inventory.MidWidth(), _inventory.InnerRectangle().Height);

            _inventory.Setup();
            _container.Setup();
            _container.AnchorAndAlignToObject(_inventory, GUIObject.SideEnum.Top, GUIObject.SideEnum.CenterX);           

            List<GUIObject> liWins = new List<GUIObject>() { _container, _inventory };
            GUIObject.CenterAndAlignToScreen(ref liWins);

            Controls.Add(_inventory);
            Controls.Add(_container);
            InventoryManager.PublicContainer = _container.Container;
        }

        public InventoryScreen(NPC n)
        {
            _font = GameContentManager.GetFont(@"Fonts\Font");
            _inventory = new Inventory(n, 4, InventoryManager.maxItemColumns, 32);

            Vector2 mainWidthHeight = new Vector2(_inventory.InnerRectangle().Width, _inventory.InnerRectangle().Height);
            _inventory.Setup();

            Controls.Add(_inventory);
            InventoryManager.PublicContainer = null;
        }

        public InventoryScreen(KeyDoor door)
        {
            _font = GameContentManager.GetFont(@"Fonts\Font");
            _inventory = new Inventory(door, 4, InventoryManager.maxItemColumns, 32);

            Vector2 mainWidthHeight = new Vector2(_inventory.InnerRectangle().Width, _inventory.InnerRectangle().Height);
            _inventory.Setup();

            Controls.Add(_inventory);
            InventoryManager.PublicContainer = null;
        }

        public override bool ProcessLeftButtonClick(Point mouse)
        {
            bool rv = false;
            if (_inventory.Contains(mouse))
            {
                _inventory.ProcessLeftButtonClick(mouse, _container == null);
                rv = true;
            }
            else if (_container != null && _container.Contains(mouse))
            {
                _container.ProcessLeftButtonClick(mouse, _container == null);
                rv = true;
            }
            else if (_character != null && _character.Contains(mouse))
            {
                rv = _character.ProcessLeftButtonClick(mouse);
                if (!rv)
                {
                    if (_iCharacterIndex < PlayerManager.GetParty().Count - 1) { _iCharacterIndex++; }
                    else { _iCharacterIndex = 0; }
                    _character.AssignNewCharacter(PlayerManager.GetParty()[_iCharacterIndex]);
                    rv = true;
                }
            }

            return rv;
        }

        public override bool ProcessRightButtonClick(Point mouse)
        {
            bool rv = true;
            if (_inventory.Contains(mouse))
            {
                rv = _inventory.ProcessRightButtonClick(mouse);
                if (rv)
                {
                    if (_character != null)
                    {
                        _character.EquipItem(GraphicCursor.HeldItem);
                    }

                    if (GraphicCursor.HeldItem != null && _container != null)
                    {
                        //InventoryManager.AddNewItemToFirstAvailableInventorySpot(GraphicCursor.HeldItem.ItemID);
                       // GraphicCursor.DropItem();
                    }
                }
            }
            else if (_container != null && _container.DrawRectangle.Contains(mouse))
            {
               rv = _container.ProcessRightButtonClick(mouse);
            }
            else if (_container != null && !_container.DrawRectangle.Contains(mouse))
            {
                GameManager.GoToWorldMap();
            }
            else if (_character != null && _character.Contains(mouse))
            {
                if (_iCharacterIndex > 0) { _iCharacterIndex--; }
                else { _iCharacterIndex = PlayerManager.GetParty().Count - 1; }
                _character.AssignNewCharacter(PlayerManager.GetParty()[_iCharacterIndex]);
                rv = true;
            }
            return rv;
        }

        public override bool ProcessHover(Point mouse)
        {
            bool rv = true;
            if (!_inventory.ProcessHover(mouse))
            {
                if (_character != null && !_character.ProcessHover(mouse))
                {
                    if (_container != null && !_container.ProcessHover(mouse))
                    {
                        rv = false;
                    }
                }
            }
            return rv;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
        }
    }
}
