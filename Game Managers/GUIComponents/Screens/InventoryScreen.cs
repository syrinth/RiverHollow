using Microsoft.Xna.Framework;
using RiverHollow.Screens;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.Items;
using RiverHollow.GUIObjects;
using RiverHollow.Characters;

namespace RiverHollow.Game_Managers.GUIObjects
{
    public class InventoryScreen : GUIScreen
    {
        private Inventory _inventory;
        private Inventory _container;
        private CharacterBox _character;
        private SpriteFont _font;
        private int _characterIndex;

        public InventoryScreen()
        {
            _characterIndex = 0;
            _character = new CharacterBox(PlayerManager.GetParty()[_characterIndex], new Vector2(128, 32));
            _font = GameContentManager.GetFont(@"Fonts\Font");
            _inventory = new Inventory(new Vector2(RiverHollow.ScreenWidth / 2, RiverHollow.ScreenHeight/2), 4, InventoryManager.maxItemColumns, 32);
            Controls.Add(_inventory);
            Controls.Add(_character);
        }

        public InventoryScreen(CharacterBox c)
        {
            _character = c;
            _font = GameContentManager.GetFont(@"Fonts\Font");
            _inventory = new Inventory(new Vector2(RiverHollow.ScreenWidth / 2, RiverHollow.ScreenHeight / 2), 4, InventoryManager.maxItemColumns, 32);
            Controls.Add(_inventory);
            Controls.Add(_character);
            InventoryManager.PublicContainer = null;
        }

        public InventoryScreen(Container c)
        {
            Vector2 centerPoint = new Vector2(RiverHollow.ScreenWidth / 2, RiverHollow.ScreenHeight / 2);
            _font = GameContentManager.GetFont(@"Fonts\Font");
            _container = new Inventory(c, centerPoint, 32);
            _inventory = new Inventory(centerPoint, 4, InventoryManager.maxItemColumns, 32);

            Vector2 contWidthHeight = new Vector2(_container.UsableRectangle().Width, _container.UsableRectangle().Height);
            Vector2 mainWidthHeight = new Vector2(_inventory.UsableRectangle().Width, _inventory.UsableRectangle().Height);
            _container.SetPosition(centerPoint - new Vector2((contWidthHeight.X/2), contWidthHeight.Y));
            _inventory.SetPosition(centerPoint - new Vector2(mainWidthHeight.X / 2, 0));

            Controls.Add(_inventory);
            Controls.Add(_container);
            InventoryManager.PublicContainer = _container.Container;
        }

        public InventoryScreen(NPC n)
        {
            Vector2 centerPoint = new Vector2(RiverHollow.ScreenWidth / 2, RiverHollow.ScreenHeight / 2);
            _font = GameContentManager.GetFont(@"Fonts\Font");
            _inventory = new Inventory(n, centerPoint, 4, InventoryManager.maxItemColumns, 32);

            Vector2 mainWidthHeight = new Vector2(_inventory.UsableRectangle().Width, _inventory.UsableRectangle().Height);
            _inventory.SetPosition(centerPoint - new Vector2(mainWidthHeight.X / 2, 0));

            Controls.Add(_inventory);
            InventoryManager.PublicContainer = null;
        }

        public override bool ProcessLeftButtonClick(Point mouse)
        {
            bool rv = false;
            if (_inventory.Contains(mouse))
            {
                _inventory.ProcessLeftButtonClick(mouse);
                rv = true;
            }
            else if (_container != null && _container.Contains(mouse))
            {
                _container.ProcessLeftButtonClick(mouse);
                rv = true;
            }
            else if (_character != null && _character.Contains(mouse))
            {
                rv = _character.ProcessLeftButtonClick(mouse);
                if (!rv)
                {
                    if (_characterIndex < PlayerManager.GetParty().Count - 1) { _characterIndex++; }
                    else { _characterIndex = 0; }
                    _character.AssignNewCharacter(PlayerManager.GetParty()[_characterIndex]);
                    rv = true;
                }
            }
            if (rv)
            {
                RiverHollow.BackToMain();
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
                RiverHollow.ChangeMapState(RiverHollow.MapState.WorldMap);
            }
            else if (_character != null && _character.Contains(mouse))
            {
                if (_characterIndex > 0) { _characterIndex--; }
                else { _characterIndex = PlayerManager.GetParty().Count - 1; }
                _character.AssignNewCharacter(PlayerManager.GetParty()[_characterIndex]);
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
