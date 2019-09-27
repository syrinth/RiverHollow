using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.Game_Managers;
using RiverHollow.Game_Managers.GUIComponents.GUIObjects;
using RiverHollow.Game_Managers.GUIObjects;
using RiverHollow.GUIObjects;
using static RiverHollow.Game_Managers.GameManager;

namespace RiverHollow.GUIComponents.GUIObjects
{
    public class GUICoin : GUIImage
    {
        public GUICoin() : base(new Rectangle(0, 32, TileSize, TileSize), TileSize, TileSize, @"Textures\Dialog")
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
            //if (_bCoinOnRight) { _gCoin.AnchorAndAlignToObject(_gTextMoney, SideEnum.Right, SideEnum.CenterY); }
            //else { _gCoin.AnchorAndAlignToObject(_gTextMoney, SideEnum.Left, SideEnum.CenterY); }

            if (_bCoinOnRight)
            {
                _gCoin.AnchorAndAlignToObject(_gTextMoney, SideEnum.Right, SideEnum.CenterY);
            }
            else
            {
                _gTextMoney.AnchorAndAlignToObject(_gCoin, SideEnum.Right, SideEnum.Top);
                _gCoin.AlignToObject(_gTextMoney, SideEnum.CenterY);
            }

            Height = _gTextMoney.Height;
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
            _gUnchecked = new GUIImage(new Rectangle(16, 32, TileSize, TileSize), Width, Height, @"Textures\Dialog");
            _gChecked = new GUIImage(new Rectangle(32, 32, TileSize, TileSize), Width, Height, @"Textures\Dialog");

            AddControl(_gUnchecked);
            AddControl(_gChecked);
            if (!string.IsNullOrEmpty(text))
            {
                _gText = new GUIText(text);
                _gText.AnchorAndAlignToObject(_gChecked, SideEnum.Right, SideEnum.Bottom);
                AddControl(_gText);
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
                SetChecked(!_bChecked);
            }

            return rv;
        }

        public bool Checked()
        {
            return _bChecked;
        }

        public void SetChecked(bool val) { _bChecked = val; }
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

            _gImage = new GUIImage(new Rectangle(startX, 0, TileSize, TileSize), TileSize, TileSize, @"Textures\Dialog");
            AddControl(_gImage);

            Width = _gImage.Width;
            Height = _gImage.Height;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
            RemoveControl(_gText);
        }

        public override bool ProcessHover(Point mouse)
        {
            bool rv = false;

            if (_gImage.Contains(mouse))
            {
                rv = true;
                _gText = new GUITextWindow(new Vector2(mouse.ToVector2().X, mouse.ToVector2().Y + 32), GameContentManager.GetGameText(_status.ToString() + " Description"));
                AddControl(_gText);
            }

            return rv;
        }
    }

    public class GUISwatch : GUIImage
    {
        public delegate void BtnClickDelegate();
        private BtnClickDelegate _delAction;
        public Color SwatchColor => _color;

        public GUISwatch(Color c, BtnClickDelegate del = null) : base(new Rectangle(0, 80, TileSize, TileSize), 8, 16, @"Textures\Dialog")
        {
            _color = c;
            _delAction = del;
        }

        public GUISwatch(Color c, int width, int height, BtnClickDelegate del = null) : base(new Rectangle(0, 80, TileSize, TileSize), width, height, @"Textures\Dialog")
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

    public class GUIFloatingText : GUIObject
    {
        const double VANISH_AFTER = 1.0;
        double _dCountDown = 0;
        GUIText _gText;

        public GUIFloatingText(string text, Color c)
        {
            _gText = new GUIText(text);
            _gText.SetColor(c);
            AddControl(_gText);

            Width = _gText.Width;
            Height = _gText.Height;
        }

        public override void Update(GameTime gTime)
        {
            _gText.MoveBy(new Vector2(0, -1));
            _dCountDown += gTime.ElapsedGameTime.TotalSeconds;
            if(_dCountDown >= VANISH_AFTER)
            {
                GUIManager.RemoveFloatingText(this);
            }
        }
    }
}
