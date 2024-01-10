using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.Game_Managers;
using RiverHollow.Items;
using RiverHollow.WorldObjects;
using static RiverHollow.Utilities.Enums;

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
            _gCoin = new GUIImage(GUIUtils.ICON_COIN);
            if (_bCoinOnRight)
            {
                _gCoin.AnchorToObject(_gTextMoney, SideEnum.Right, GUIManager.STANDARD_MARGIN);
                _gTextMoney.AlignToObject(_gCoin, SideEnum.CenterY);
            }
            else { 
                _gTextMoney.AnchorAndAlignWithSpacing(_gCoin, SideEnum.Right, SideEnum.CenterY, GUIManager.STANDARD_MARGIN);
            }

            AddControls(_gCoin, _gTextMoney);
            DetermineSize();
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
            _gKeys = new GUIImage(GUIUtils.ICON_KEY);

            _gKeysText.AnchorAndAlignWithSpacing(_gKeys, SideEnum.Right, SideEnum.CenterY, GUIManager.STANDARD_MARGIN);

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
            _bChecked = isChecked;
            _gUnchecked = new GUIImage(GUIUtils.TOGGLE_UNCHECK);
            _gChecked = new GUIImage(GUIUtils.TOGGLE_CHECK);
            _gText = new GUIText(" - " + text);

            _gText.AnchorAndAlign(_gChecked, SideEnum.Right, SideEnum.CenterY);

            AddControl(_gText);
            AddControl(_gUnchecked);
            AddControl(_gChecked);

            DetermineSize();

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

    public class GUIItem : GUIObject
    {
        public Item ItemObject {get;}
        GUIImage _gImg;
        GUIImage _gInvisible;
        GUIText _gText;
        public ItemBoxDraw DrawNumbers { get; private set; }
        public bool CompareToInventory = false;

        public GUIItem(Item it, ItemBoxDraw e = ItemBoxDraw.OnlyStacks)
        {
            ItemObject = it;
            DrawNumbers = e;

            Width = GameManager.ScaledTileSize;
            Height = GameManager.ScaledTileSize;

            _gInvisible = new GUIImage(GUIUtils.INVISIBLE);
            _gInvisible.CenterOnObject(this);

            _gImg = new GUIImage(ItemObject.SourceRectangle, ItemObject.Texture);
            GUIUtils.SetObjectScale(_gImg, ItemObject.SourceRectangle.Width, ItemObject.SourceRectangle.Height, 1);
            _gImg.CenterOnObject(_gInvisible);

            _gText = new GUIText(ItemObject.Number.ToString(), true, DataManager.FONT_NUMBER_DISPLAY);
            _gText.SetColor(Color.White);

            SetTextPosition();
        }

        public override void Update(GameTime gTime)
        {
            if (!CompareToInventory)
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
                case ItemBoxDraw.MoreThanOne:
                    if(ItemObject.Number > 1)
                    {
                        _gText.Draw(spriteBatch);
                    }
                    break;
            }
        }

        private void SetTextPosition()
        {
            _gText.AlignToObject(_gInvisible, SideEnum.Right);
            _gText.AlignToObject(_gInvisible, SideEnum.Bottom);
        }

        public override void SetColor(Color c)
        {
            _gText.SetColor(c);
        }

        public bool CompareNumToInventory(Container c)
        {
            bool rv = true;
            CompareToInventory = true;
            DrawNumbers = ItemBoxDraw.Always;
            int inventoryNumber = InventoryManager.GetNumberInInventory(ItemObject.ID, c == null ? InventoryManager.PlayerInventory : c.Inventory);
            _gText.SetText(string.Format("{0}/{1}", inventoryNumber, ItemObject.Number));
            SetTextPosition();

            if (inventoryNumber < ItemObject.Number)
            {
                rv = false;
                SetColor(Color.Red);
            }

            return rv;
        }
    }


    public class GUIIcon : GUIImage
    {
        GameIconEnum Icon { get; }

        public GUIIcon(Rectangle sourceRect, GameIconEnum e) : base(sourceRect, DataManager.HUD_COMPONENTS)
        {
            Icon = e;
        }

        protected override void BeginHover()
        {
            string iconDescription = string.Empty;
            switch (Icon)
            {
                case GameIconEnum.Traveler:
                    iconDescription = "Upgrade_Chance";
                    break;
                case GameIconEnum.Coin:
                    iconDescription = "Upgrade_Profit";
                    break;
                case GameIconEnum.Hammer:
                    iconDescription = "Upgrade_CraftSlots";
                    break;
                case GameIconEnum.Book:
                    iconDescription = "Upgrade_Recipe";
                    break;
                case GameIconEnum.Capacity:
                    iconDescription = "Icon_Capacity";
                    break;
            }

            var win = new GUITextWindow(DataManager.GetGameTextEntry(iconDescription), Point.Zero);
            win.AnchorAndAlign(this, SideEnum.Bottom, SideEnum.CenterX, GUIUtils.ParentRuleEnum.Skip);
            GUIManager.OpenHoverObject(win, DrawRectangle, true);
        }
    }

    public class GUIIconText : GUIObject
    {
        readonly GUIIcon _gIcon;
        readonly GUIText _gText;

        public GUIIconText(string text, int spacing, Rectangle sourceRect, GameIconEnum e, SideEnum anchorTo, SideEnum alignTo)
        {
            _gIcon = new GUIIcon(sourceRect, e);
            _gText = new GUIText(text);
            _gText.AnchorAndAlignWithSpacing(_gIcon, anchorTo, alignTo, spacing);

            AddControls(_gIcon, _gText);

            DetermineSize();
        }
    }
}
