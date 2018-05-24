using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.Game_Managers;
using RiverHollow.Game_Managers.GUIObjects;
using RiverHollow.GUIObjects;
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

            Setup();
        }

        public GUIMoneyDisplay(int cost, bool coinOnRight = true)
        {
            _bIsPlayerMoney = false;
            _bCoinOnRight = coinOnRight;

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
}
