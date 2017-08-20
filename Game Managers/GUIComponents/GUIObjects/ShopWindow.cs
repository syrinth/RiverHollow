using Adventure.Characters.NPCs;
using Adventure.GUIObjects;
using Adventure.Tile_Engine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure.Game_Managers.GUIObjects
{
    public class ShopWindow : GUIObject
    {
        private ShopKeeper _shopkeeper;
        private Dictionary<Rectangle, object> _buyBoxes;

        public ShopWindow(ShopKeeper shop)
        {
            _texture = _gcManager.GetTexture(@"Textures\ShopWindow");
            _visible = true;

            Position = new Vector2((AdventureGame.ScreenWidth / 2) - (_texture.Width / 2), (AdventureGame.ScreenHeight / 2) - (_texture.Height / 2));
            _rect = new Rectangle((int)Position.X, (int)Position.Y, _texture.Width, _texture.Height);
            _shopkeeper = shop;

            _buyBoxes = new Dictionary<Rectangle, object>();

            int increment = TileMap.TileSize;
            Vector2 buyBoxPosition = new Vector2(Position.X + increment, Position.Y + increment);
            for (int i=0; i<9; i++)
            {
                if (i < _shopkeeper.Buildings.Count)
                {
                    _buyBoxes.Add(new Rectangle(buyBoxPosition.ToPoint(), new Point(increment*3)), _shopkeeper.Buildings[i]);
                }

                if ((i + 1) % 3 == 0) {
                    buyBoxPosition.X += increment;
                    buyBoxPosition.Y += increment*5;
                }
                else
                {
                    buyBoxPosition.X += increment*5;
                }
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (_visible)
            {
                spriteBatch.Draw(_texture, _rect, Color.White);
                foreach (Rectangle r in _buyBoxes.Keys)
                {
                    if (_buyBoxes[r] != null)
                    {
                        if (_buyBoxes[r].GetType().Equals(typeof(ObjectManager.BuildingID)))
                        {
                            Building b = (ObjectManager.GetBuilding((ObjectManager.BuildingID)_buyBoxes[r]));
                            spriteBatch.Draw(b.Texture, r, Color.White);
                        }
                        else
                        {
                            Worker w = (ObjectManager.GetWorker((ObjectManager.WorkerID)_buyBoxes[r]));
                            spriteBatch.Draw(w.Texture, r, Color.White);
                        }
                    }
                }
            }
        }

        public bool ProcessLeftButtonClick(Point mouse)
        {
            bool rv = false;

            foreach (Rectangle r in _buyBoxes.Keys)
            {
                if (r.Contains(mouse))
                {
                    if (_buyBoxes[r] != null)
                    {
                        if (_buyBoxes[r].GetType().Equals(typeof(ObjectManager.BuildingID)))
                        {
                            Building b = ObjectManager.GetBuilding((ObjectManager.BuildingID)_buyBoxes[r]);
                            this._visible = false;
                            GraphicCursor.PickUpBuilding(b);
                            AdventureGame.BuildingMode = true;
                            Camera.UnsetObserver();
                            MapManager.GetInstance().ViewMap("Map1");
                            rv = true;
                        }
                        if (_buyBoxes[r].GetType().Equals(typeof(ObjectManager.WorkerID)))
                        {
                            if (PlayerManager.GetInstance().Buildings.Count > 0)
                            {
                                this._visible = false;
                                GraphicCursor.PickUpWorker((ObjectManager.WorkerID)_buyBoxes[r]);
                                AdventureGame.BuildingMode = true;
                                Camera.UnsetObserver();
                                MapManager.GetInstance().ViewMap("Map1");
                                rv = true;
                            }
                        }
                    }
                }
            }
            return rv;
        }
    }
}
