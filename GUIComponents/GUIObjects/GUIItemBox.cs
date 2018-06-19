using RiverHollow.Game_Managers.GUIObjects;
using RiverHollow.WorldObjects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.GUIObjects;
using RiverHollow.Characters.NPCs;
using RiverHollow.GUIComponents.GUIObjects;
using static RiverHollow.WorldObjects.Equipment;
using static RiverHollow.Game_Managers.ObjectManager;
using System.Collections.Generic;
using System;
using static RiverHollow.WorldObjects.Clothes;

namespace RiverHollow.Game_Managers.GUIComponents.GUIObjects
{
    public class GUIItemBox : GUIImage
    {
        private Item _item;
        public Item Item => _item;
        protected bool _hover;
        private GUITextWindow _textWindow;
        private GUIWindow _reqWindow;
        private GUIText _textNum;
        Recipe _recipe;
        List<GUIObject> _liItemReqs;

        bool _bCrafting;

        int _iCol;
        public int Col => _iCol;
        int _iRow;
        public int Row => _iRow;

        public EquipmentEnum EquipType;
        public ClothesEnum ClothesType;

        public GUIItemBox(Vector2 position, Rectangle sourceRect, int width, int height, int row, int col, string texture, Item item, EquipmentEnum e = EquipmentEnum.None, ClothesEnum c = ClothesEnum.None, bool crafting = false) : base(position, sourceRect, width, height, texture)
        {
            _bCrafting = crafting;
            SetItem(item);
            EquipType = e;
            ClothesType = c;

            _iCol = col;
            _iRow = row;
        }

        public GUIItemBox(Vector2 position, Rectangle sourceRect, int width, int height, string texture, Item item, EquipmentEnum e = EquipmentEnum.None, ClothesEnum c = ClothesEnum.None, bool crafting = false) : this(position, sourceRect, width, height, 0, 0, texture, item, e, c, crafting)
        {
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
            if (_item != null)
            {
                _item.Draw(spriteBatch, _drawRect, false);
                if (_textNum != null) { _textNum.Draw(spriteBatch); }
            }
            if (_hover)
            {
                if (_textWindow != null) { _textWindow.Draw(spriteBatch); }
            }
        }

        public bool DrawDescription(SpriteBatch spriteBatch)
        {
            if (_textWindow != null) {
                _textWindow.Draw(spriteBatch);
            }
            if (_reqWindow != null)
            {
                _reqWindow.Draw(spriteBatch);
            }

            return _hover;
        }

        public bool ProcessHover(Point mouse)
        {
            bool rv = false;
            if (Contains(mouse))
            {
                _hover = true;
                if (_item != null)
                {
                    _textWindow = new GUITextWindow(new Vector2(mouse.ToVector2().X, mouse.ToVector2().Y + 32), _item.GetDescription());

                    if (_bCrafting)
                    {
                        _reqWindow = new GUIWindow(GUIWindow.RedWin, _liItemReqs[0].Width, _liItemReqs[0].Height * _liItemReqs.Count);
                        _reqWindow.Position(new Vector2(mouse.ToVector2().X, mouse.ToVector2().Y + 32));
                        _reqWindow.AnchorAndAlignToObject(_textWindow, SideEnum.Bottom, SideEnum.Left);
                        for (int i=0; i < _liItemReqs.Count; i++)
                        {
                            GUIObject r = _liItemReqs[i];
                            if (i == 0) { r.AnchorToInnerSide(_reqWindow, SideEnum.TopLeft); }
                            else
                            {
                                r.AnchorAndAlignToObject(_liItemReqs[i - 1], SideEnum.Bottom, SideEnum.Left);
                            }
                        }
                        _reqWindow.Resize();
                    }
                }
                rv = true;
            }
            else
            {
                _hover = false;
                _textWindow = null;
                _reqWindow = null;
            }
            return rv;
        }

        public void SetItem(Item it)
        {
            _item = it;
            if (_item != null && _item.DoesItStack)
            {
                _textNum = new GUIText(_item.Number.ToString(), true, @"Fonts\DisplayFont");
                _textNum.SetColor(Color.White);
                _textNum.AnchorToInnerSide(this, SideEnum.BottomRight, 10);
            }
            if (_item != null && _bCrafting)
            {
                _liItemReqs = new List<GUIObject>();
                _recipe = DictCrafting[_item.ItemID];
                foreach (KeyValuePair<int, int> kvp in _recipe.RequiredItems)
                {
                    _liItemReqs.Add(new GUIItemReq(kvp.Key, kvp.Value));
                }
            }
        }
    }

    public class WorkerBox : GUIObject
    {
        private SpriteFont _font;
        private GUIWindow _workerWindow;
        private GUIWindow _costWindow;
        public WorldAdventurer _w;
        public int Cost;

        public WorkerBox(Vector2 p, WorldAdventurer w, int cost)
        {
            _font = GameContentManager.GetFont(@"Fonts\Font");
            Cost = cost;
            _w = w;
            _workerWindow = new GUIWindow(p, GUIWindow.RedWin, 64, 96);
            _costWindow = new GUIWindow(new Vector2(p.X, p.Y + 96), GUIWindow.RedWin, 64, 32);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            _workerWindow.Draw(spriteBatch);
            _costWindow.Draw(spriteBatch);
            _w.Position = new Vector2(_workerWindow.Position().X + _workerWindow.EdgeSize, (int)_workerWindow.Position().Y + _workerWindow.EdgeSize);
            _w.Draw(spriteBatch);
            spriteBatch.DrawString(_font, Cost.ToString(), _costWindow.Position() + new Vector2(_costWindow.EdgeSize/2, _costWindow.EdgeSize/2), Color.White);
        }

        public override bool Contains(Point mouse)
        {
            return _workerWindow.Contains(mouse);
        }
    }

    public class GUIItemReq : GUIObject
    {
        GUIImage _gImg;
        GUIText _gText;

        public GUIItemReq(int id, int number)
        {
            Item it = ObjectManager.GetItem(id);
            _gImg = new GUIImage(Vector2.Zero, it.SourceRectangle, it.SourceRectangle.Width, it.SourceRectangle.Height, it.Texture);
            _gText = new GUIText(number.ToString(), true, @"Fonts\DisplayFont");
            Width = _gImg.Width + _gText.Width;
            Height = Math.Max(_gImg.Height, _gText.Height);
            Position(Vector2.Zero);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            _gImg.Draw(spriteBatch);
            _gText.Draw(spriteBatch);
        }

        public override void Position(Vector2 value)
        {
            base.Position(value);
            _gImg.Position(value);
            _gText.AnchorAndAlignToObject(_gImg, SideEnum.Right, SideEnum.Bottom);
        }
    }
}
