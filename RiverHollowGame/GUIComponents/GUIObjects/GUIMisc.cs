using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.Game_Managers;
using System.Collections.Generic;
using static RiverHollow.Game_Managers.GameManager;
using RiverHollow.Items;
using static RiverHollow.Utilities.Enums;
using RiverHollow.Utilities;
using System;

namespace RiverHollow.GUIComponents.GUIObjects.GUIWindows
{
    public class GUIMoneyDisplay : GUIObject
    {
        GUIText _gTextMoney;
        GUIImage _gCoin;
        bool _bIsPlayerMoney;
        bool _bCoinOnRight;

        //Player Money Display
        public GUIMoneyDisplay(bool playermoney = true)
        {
            _bIsPlayerMoney = playermoney;
            _bCoinOnRight = false;

            _gTextMoney = new GUIText(PlayerManager.Money.ToString("N0"));
            Setup();
        }

        public GUIMoneyDisplay(int cost, bool coinOnRight = true)
        {
            _bIsPlayerMoney = false;
            _bCoinOnRight = coinOnRight;

            _gTextMoney = new GUIText(cost.ToString("N0"));
            Setup();
        }

        private void Setup()
        {
            _gCoin = GUIUtils.GetIcon(GameIconEnum.Coin);

            if (_bCoinOnRight)
            {
                _gCoin.AnchorAndAlignToObject(_gTextMoney, SideEnum.Right, SideEnum.CenterY, ScaleIt(GUIManager.STANDARD_MARGIN));
            }
            else
            {
                _gTextMoney.AnchorAndAlignToObject(_gCoin, SideEnum.Right, SideEnum.CenterY, ScaleIt(GUIManager.STANDARD_MARGIN));
            }

            Height = _gCoin.Height > _gTextMoney.Height ? _gCoin.Height : _gTextMoney.Height;
            Width = _gTextMoney.Width + _gCoin.Width;

            AddControl(_gCoin);
            AddControl(_gTextMoney);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            _gTextMoney.Draw(spriteBatch);
            _gCoin.Draw(spriteBatch);
        }

        public override void Update(GameTime gTime)
        {
            if (_bIsPlayerMoney)
            {
                _gTextMoney.SetText(PlayerManager.Money.ToString("N0"));
            }
        }

        public override void SetColor(Color c)
        {
            _gTextMoney.SetColor(c);
        }
    }
    public class GUIDungeonKeyDisplay : GUIObject
    {
        GUIText _gKeysText;
        GUIImage _gKeys;

        //Player Monster Energy Display
        public GUIDungeonKeyDisplay()
        {
            _gKeysText = new GUIText();
            Setup();
        }

        private void Setup()
        {
            _gKeys = GUIUtils.GetIcon(GameIconEnum.Key);

            _gKeysText.AnchorAndAlignToObject(_gKeys, SideEnum.Right, SideEnum.CenterY, GUIManager.STANDARD_MARGIN);

            Height = _gKeys.Height > _gKeysText.Height ? _gKeys.Height : _gKeysText.Height;
            Width = _gKeysText.Width + _gKeys.Width;

            AddControl(_gKeys);
            AddControl(_gKeysText);

            Position(_gKeys.Position());
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (MapManager.CurrentMap.IsDungeon) {
                _gKeysText.Draw(spriteBatch);
                _gKeys.Draw(spriteBatch);
            }
        }

        public override void Update(GameTime gTime)
        {
            if (MapManager.CurrentMap.IsDungeon)
            {
                _gKeysText.SetText(DungeonManager.DungeonKeys());
            }
        }
    }
    public class GUICheck : GUIObject
    {
        bool _bChecked;
        GUIImage _gUnchecked;
        GUIImage _gChecked;
        GUIText _gText;
        protected EmptyDelegate _delAction;
        public GUICheck(string text, bool isChecked = false, EmptyDelegate del = null)
        {
            int squareSize = Constants.TILE_SIZE * 2;
            _bChecked = isChecked;
            _gUnchecked = new GUIImage(new Rectangle(16, 32, Constants.TILE_SIZE, Constants.TILE_SIZE), squareSize, squareSize, DataManager.DIALOGUE_TEXTURE);
            _gChecked = new GUIImage(new Rectangle(32, 32, Constants.TILE_SIZE, Constants.TILE_SIZE), squareSize, squareSize, DataManager.DIALOGUE_TEXTURE);
            _gText = new GUIText(" - " + text);

            int delta = _gText.Height - squareSize;
            _gUnchecked.MoveBy(new Point(0, delta));
            _gChecked.MoveBy(new Point(0, delta));

            _gText.AnchorAndAlignToObject(_gChecked, SideEnum.Right, SideEnum.CenterY);

            AddControl(_gText);
            AddControl(_gUnchecked);
            AddControl(_gChecked);

            Width = _gText.Right - _gChecked.Left;
            Height = _gText.Height;

            if (del != null) { _delAction = del; }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (_bChecked) { _gChecked.Draw(spriteBatch); }
            else { _gUnchecked.Draw(spriteBatch); }

            if (_gText != null)
            {
                _gText.Draw(spriteBatch);
            }
        }

        public override bool ProcessLeftButtonClick(Point mouse)
        {
            bool rv = false;
            if (Contains(mouse))
            {
                SetChecked(!_bChecked);
                if (_delAction != null)
                {
                    _delAction();
                }
                rv = true;
            }

            return rv;
        }

        public bool Checked()
        {
            return _bChecked;
        }

        public void SetChecked(bool val) { _bChecked = val; }
    }

    public class GUISwatch : GUIImage
    {
        private EmptyDelegate _delAction;
        public Color SwatchColor => _Color;

        public GUISwatch(Color c, EmptyDelegate del = null) : base(new Rectangle(0, 80, Constants.TILE_SIZE, Constants.TILE_SIZE), 8, 16, DataManager.DIALOGUE_TEXTURE)
        {
            _Color = c;
            _delAction = del;
        }

        public GUISwatch(Color c, int width, int height, EmptyDelegate del = null) : base(new Rectangle(0, 80, Constants.TILE_SIZE, Constants.TILE_SIZE), width, height, DataManager.DIALOGUE_TEXTURE)
        {
            _Color = c;
            _delAction = del;
        }

        public override bool ProcessLeftButtonClick(Point mouse)
        {
            bool rv = false;
            if (Contains(mouse) && _delAction != null)
            {
                _delAction();
                rv = true;
            }

            return rv;
        }
    }

    public class GUIItem : GUIObject
    {
        public Item ItemObject {get;}
        GUIImage _gImg;
        GUIImage _gDummy;
        GUIText _gText;
        public ItemBoxDraw DrawNumbers = ItemBoxDraw.OnlyStacks;
        public bool CompareNumToPlayer = false;

        public GUIItem(Item it)
        {
            ItemObject = it;

            _gImg = new GUIImage(ItemObject.SourceRectangle, ItemObject.SourceRectangle.Width, ItemObject.SourceRectangle.Height, ItemObject.Texture);

            GUIUtils.SetObjectScale(_gImg, ItemObject.SourceRectangle.Width, ItemObject.SourceRectangle.Height, 1);

            _gText = new GUIText(ItemObject.Number.ToString(), true, DataManager.FONT_NUMBER_DISPLAY);
            _gText.SetColor(Color.White);

            _gDummy = new GUIImage(ItemObject.SourceRectangle, GameManager.ScaledTileSize, GameManager.ScaledTileSize, ItemObject.Texture);
            _gImg.CenterOnObject(_gDummy);
            SetTextPosition();

            AddControl(_gImg);
            AddControl(_gText);

            Width = GameManager.ScaledTileSize;
            Height = GameManager.ScaledTileSize;
        }

        public override void Update(GameTime gTime)
        {
            if (!CompareNumToPlayer)
            {
                _gText?.SetText(ItemObject.Number.ToString());
                SetTextPosition();
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            _gImg.Draw(spriteBatch);
            switch (DrawNumbers)
            {
                case ItemBoxDraw.Always:
                    _gText.Draw(spriteBatch);
                    break;
                case ItemBoxDraw.OnlyStacks:
                    if (ItemObject.Stacks())
                    {
                        _gText.Draw(spriteBatch);
                    }
                    break;
            }
        }

        public override bool ProcessHover(Point mouse)
        {
            bool rv = false;
            if (Contains(mouse))
            {
                if (!GUIManager.IsHoverWindowOpen())
                {
                    GUIItemDescriptionWindow win = new GUIItemDescriptionWindow(ItemObject, new Point(DrawRectangle.Left, DrawRectangle.Bottom));
                    GUIManager.OpenHoverWindow(win, new Rectangle(Position().X, Position().Y, Width, Height), true);
                }
                rv = true;
            }
            return rv;
        }

        private void SetTextPosition()
        {
            _gDummy.CenterOnObject(_gImg);
            _gText.AlignToObject(_gDummy, SideEnum.Right);
            _gText.AlignToObject(_gDummy, SideEnum.Bottom);
        }

        public override void SetColor(Color c)
        {
            _gText.SetColor(c);
        }

        public bool SetCompareNumToPlayer()
        {
            bool rv = true;
            CompareNumToPlayer = true;
            DrawNumbers = ItemBoxDraw.Always;
            int playerNum = InventoryManager.GetNumberInInventory(ItemObject.ID);
            _gText.SetText(string.Format("{0}/{1}", playerNum, ItemObject.Number));
            SetTextPosition();

            if (!InventoryManager.HasItemInPlayerInventory(ItemObject.ID, ItemObject.Number))
            {
                rv = false;
                SetColor(Color.Red);
            }

            return rv;
        }
    }
}
