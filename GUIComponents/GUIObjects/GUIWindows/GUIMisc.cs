using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.Game_Managers;
using RiverHollow.Game_Managers.GUIComponents.GUIObjects;
using RiverHollow.Game_Managers.GUIObjects;
using RiverHollow.GUIObjects;
using RiverHollow.SpriteAnimations;
using System;
using static RiverHollow.Game_Managers.GameManager;

namespace RiverHollow.GUIComponents.GUIObjects
{
    public class GUICoin : GUIImage
    {
        public GUICoin() : base(Vector2.Zero, new Rectangle(0, 32, TileSize, TileSize), TileSize, TileSize, @"Textures\Dialog")
        {
        }
    }

    public class GUIMoneyDisplay : GUIObject
    {
        GUIText _gTextMoney;
        GUICoin _gCoin;
        bool _bIsPlayerMoney;
        bool _bCoinOnRight;

        //Player Money Display
        public GUIMoneyDisplay()
        {
            _bIsPlayerMoney = true;
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
            _gCoin = new GUICoin();
            if (_bCoinOnRight) { _gCoin.AnchorAndAlignToObject(_gTextMoney, SideEnum.Right, SideEnum.CenterY); }
            else { _gCoin.AnchorAndAlignToObject(_gTextMoney, SideEnum.Left, SideEnum.CenterY); }

            Height = _gTextMoney.Height;
            Width = _gTextMoney.Width + _gCoin.Width;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            _gTextMoney.Draw(spriteBatch);
            _gCoin.Draw(spriteBatch);
        }

        public override void Update(GameTime gameTime)
        {
            if (_bIsPlayerMoney)
            {
                _gTextMoney.SetText(PlayerManager.Money.ToString("N0"));
            }
        }

        public override void Position(Vector2 value)
        {
            base.Position(value);

            if (_bCoinOnRight) {
                _gTextMoney.Position(value);
                _gCoin.AnchorAndAlignToObject(_gTextMoney, SideEnum.Right, SideEnum.CenterY);
            }
            else {
                _gCoin.Position(value);
                _gTextMoney.AnchorAndAlignToObject(_gCoin, SideEnum.Right, SideEnum.Top);
                _gCoin.AlignToObject(_gTextMoney, SideEnum.CenterY);
            }

            Height = _gTextMoney.Height;
            Width = _gTextMoney.Width + _gCoin.Width;
        }

        public void SetColor(Color c)
        {
            _gTextMoney.SetColor(c);
        }
    }

    public class GUICheck : GUIObject
    {
        bool _bChecked;
        GUIImage _gUnchecked;
        GUIImage _gChecked;
        GUIText _gText;
        public GUICheck(string text = "", bool isChecked = false)
        {
            Width = TileSize*2;
            Height = TileSize*2;
            _bChecked = isChecked;
            _gUnchecked = new GUIImage(Vector2.Zero, new Rectangle(16, 32, TileSize, TileSize), Width, Height, @"Textures\Dialog");
            _gChecked = new GUIImage(Vector2.Zero, new Rectangle(32, 32, TileSize, TileSize), Width, Height, @"Textures\Dialog");

            if (!string.IsNullOrEmpty(text))
            {
                _gText = new GUIText(text);
            }
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
                _bChecked = !_bChecked;
            }

            return rv;
        }

        public override void Position(Vector2 value)
        {
            base.Position(value);

            _gChecked.Position(value);
            _gUnchecked.Position(value);
            if (_gText != null)
            {
                _gText.AnchorAndAlignToObject(_gChecked, SideEnum.Right, SideEnum.Bottom);
            }
        }

        public bool Checked()
        {
            return _bChecked;
        }
    }

    public class GUIStatus : GUIObject
    {
        bool _bHover;

        GUIImage _gImage;
        GUITextWindow _gText;
        ConditionEnum _status;
        public ConditionEnum Status => _status;

        public GUIStatus(ConditionEnum status)
        {
            _status = status;
            int startX = 176;

            startX += ((int)status - 2) * TileSize;

            _gImage = new GUIImage(Vector2.Zero, new Rectangle(startX, 0, TileSize, TileSize), TileSize, TileSize, @"Textures\Dialog");

            Width = _gImage.Width;
            Height = _gImage.Height;
        }
        public override void Draw(SpriteBatch spriteBatch)
        {
            _gImage.Draw(spriteBatch);
            if (_bHover)
            {
                if (_gText != null) { _gText.Draw(spriteBatch); }
            }
        }

        public void ProcessHover(Point mouse)
        {
            if (_gImage.Contains(mouse))
            {
                _bHover = true;
                _gText = new GUITextWindow(new Vector2(mouse.ToVector2().X, mouse.ToVector2().Y + 32), GameContentManager.GetGameText(_status.ToString() + " Description"));
            }
            else { _bHover = false; }
        }

        public override void Position(Vector2 value)
        {
            base.Position(value);
            _gImage.Position(value);
        }
    }

    public class GUISwatch : GUIImage
    {
        public delegate void BtnClickDelegate();
        private BtnClickDelegate _delAction;
        public Color SwatchColor => _color;

        public GUISwatch(Color c, BtnClickDelegate del = null) : base(Vector2.Zero, new Rectangle(0, 80, TileSize, TileSize), 8, 16, @"Textures\Dialog")
        {
            _color = c;
            _delAction = del;
        }

        public GUISwatch(Color c, int width, int height, BtnClickDelegate del = null) : base(Vector2.Zero, new Rectangle(0, 80, TileSize, TileSize), width, height, @"Textures\Dialog")
        {
            _color = c;
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
}
