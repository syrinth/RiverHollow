using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.Game_Managers;
using RiverHollow.Items;
using System;
using static RiverHollow.Game_Managers.GameManager;

namespace RiverHollow.GUIComponents.GUIObjects.GUIWindows
{
    public class GUICoin : GUIImage
    {
        public GUICoin() : base(new Rectangle(4, 36, 8, 9), ScaleIt(8), ScaleIt(9), DataManager.DIALOGUE_TEXTURE)
        {
        }
    }
    public class GUIMonsterEnergy : GUIImage
    {
        public GUIMonsterEnergy() : base(new Rectangle(4, 20, 8 , 8), ScaleIt(8), ScaleIt(8), DataManager.DIALOGUE_TEXTURE)
        {
        }
    }

    public class GUIDungeonKey : GUIImage
    {
        public GUIDungeonKey() : base(new Rectangle(20, 18, 8, 12), ScaleIt(8), ScaleIt(12), DataManager.DIALOGUE_TEXTURE)
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
            _gCoin = new GUICoin();

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
    public class GUIMonsterEnergyDisplay : GUIObject
    {
        GUIText _gCurrencyText;
        GUIMonsterEnergy _gEnergy;
        bool _bPlayerEnergy;
        bool _bSymbolOnRight;

        //Player Monster Energy Display
        public GUIMonsterEnergyDisplay()
        {
            _bPlayerEnergy = true;
            _bSymbolOnRight = false;

            _gCurrencyText = new GUIText(PlayerManager.Money.ToString("N0"));
            Setup();
        }

        public GUIMonsterEnergyDisplay(int cost, bool onRight = true)
        {
            _bPlayerEnergy = false;
            _bSymbolOnRight = onRight;

            _gCurrencyText = new GUIText(cost.ToString("N0"));
            Setup();
        }

        private void Setup()
        {
            _gEnergy = new GUIMonsterEnergy();

            if (_bSymbolOnRight)
            {
                _gCurrencyText.AnchorAndAlignToObject(_gEnergy, SideEnum.Left, SideEnum.CenterY, GUIManager.STANDARD_MARGIN);
            }
            else
            {
                _gCurrencyText.AnchorAndAlignToObject(_gEnergy, SideEnum.Right, SideEnum.CenterY, GUIManager.STANDARD_MARGIN);
            }

            Height = _gEnergy.Height > _gCurrencyText.Height ? _gEnergy.Height : _gCurrencyText.Height;
            Width = _gCurrencyText.Width + _gEnergy.Width;

            AddControl(_gEnergy);
            AddControl(_gCurrencyText);
            Position(_gEnergy.Position());
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            _gCurrencyText.Draw(spriteBatch);
            _gEnergy.Draw(spriteBatch);
        }

        public override void Update(GameTime gTime)
        {
            if (_bPlayerEnergy)
            {
                _gCurrencyText.SetText(PlayerManager.MonsterEnergy.ToString("N0"));
            }
        }

        public override void SetColor(Color c)
        {
            _gCurrencyText.SetColor(c);
        }
    }
    public class GUIDungeonKeyDisplay : GUIObject
    {
        GUIText _gKeysText;
        GUIDungeonKey _gKeys;

        //Player Monster Energy Display
        public GUIDungeonKeyDisplay()
        {
            _gKeysText = new GUIText();
            Setup();
        }

        private void Setup()
        {
            _gKeys = new GUIDungeonKey();

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
        public GUICheck(string text, bool isChecked = false)
        {
            int squareSize = TileSize * 2;
            _bChecked = isChecked;
            _gUnchecked = new GUIImage(new Rectangle(16, 32, TileSize, TileSize), squareSize, squareSize, DataManager.DIALOGUE_TEXTURE);
            _gChecked = new GUIImage(new Rectangle(32, 32, TileSize, TileSize), squareSize, squareSize, DataManager.DIALOGUE_TEXTURE);
            _gText = new GUIText(" - " + text);

            int delta = _gText.Height - squareSize;
            _gUnchecked.MoveBy(new Vector2(0, delta));
            _gChecked.MoveBy(new Vector2(0, delta));

            _gText.AnchorAndAlignToObject(_gChecked, SideEnum.Right, SideEnum.CenterY);

            AddControl(_gText);
            AddControl(_gUnchecked);
            AddControl(_gChecked);

            Width = _gText.Right - _gChecked.Left;
            Height = _gText.Height;
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

            _gImage = new GUIImage(new Rectangle(startX, 0, TileSize, TileSize), TileSize, TileSize, DataManager.DIALOGUE_TEXTURE);
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
                _gText = new GUITextWindow(new Vector2(mouse.ToVector2().X, mouse.ToVector2().Y + 32), DataManager.GetGameText(_status.ToString() + " Description"));
                AddControl(_gText);
            }

            return rv;
        }
    }

    public class GUISwatch : GUIImage
    {
        private BtnClickDelegate _delAction;
        public Color SwatchColor => _cColor;

        public GUISwatch(Color c, BtnClickDelegate del = null) : base(new Rectangle(0, 80, TileSize, TileSize), 8, 16, DataManager.DIALOGUE_TEXTURE)
        {
            _cColor = c;
            _delAction = del;
        }

        public GUISwatch(Color c, int width, int height, BtnClickDelegate del = null) : base(new Rectangle(0, 80, TileSize, TileSize), width, height, DataManager.DIALOGUE_TEXTURE)
        {
            _cColor = c;
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
        GUIText _gText;

        public GUIItem(Item it)
        {
            ItemObject = it;
            _gImg = new GUIImage(ItemObject.SourceRectangle, ItemObject.SourceRectangle.Width, ItemObject.SourceRectangle.Height, ItemObject.Texture);
            _gImg.SetScale(Scale);

            _gText = new GUIText(ItemObject.Number.ToString(), true, DataManager.FONT_NUMBER_DISPLAY);

            _gText.AlignToObject(_gImg, SideEnum.Right);
            _gText.AlignToObject(_gImg, SideEnum.Bottom);

            AddControl(_gImg);
            AddControl(_gText);

            Width = _gImg.Width;
            Height = _gImg.Height;
        }

        public override void Update(GameTime gTime)
        {
            _gText?.SetText(ItemObject.Number.ToString());

            _gText.AlignToObject(_gImg, SideEnum.Right);
            _gText.AlignToObject(_gImg, SideEnum.Bottom);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            _gImg.Draw(spriteBatch);
            if (ItemObject.DoesItStack && _gText != null) {
                _gText.Draw(spriteBatch);
            }
        }

        public override bool ProcessHover(Point mouse)
        {
            bool rv = false;
            if (Contains(mouse))
            {
                GUIManager.OpenHoverWindow(new GUITextWindow(new Vector2(mouse.ToVector2().X, mouse.ToVector2().Y + 32), ItemObject.GetDescription()), this);
                rv = true;
            }
            return rv;
        }

        public override void SetColor(Color c)
        {
            _gText.SetColor(c);
        }
    }
}
