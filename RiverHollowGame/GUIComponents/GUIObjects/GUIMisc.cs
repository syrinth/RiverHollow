using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.Game_Managers;
using RiverHollow.Items;
using RiverHollow.Utilities;
using static RiverHollow.Utilities.Enums;

namespace RiverHollow.GUIComponents.GUIObjects.GUIWindows
{
    public class GUIMoneyDisplay : GUIObject
    {
        readonly GUIText _gTextMoney;

        readonly bool _bIsPlayerMoney = false;

        public GUIMoneyDisplay(bool tinyDisplay = false) : this(PlayerManager.Money, DirectionEnum.Left, tinyDisplay)
        {
            _bIsPlayerMoney = true;
        }

        public GUIMoneyDisplay(int cost, DirectionEnum coinDirection = DirectionEnum.Left, bool tinyDisplay = false)
        {
            var fontName = tinyDisplay ? DataManager.FONT_NUMBERS : DataManager.FONT_MAIN;
            _gTextMoney = new GUIText(cost.ToString("N0"), true, fontName);

            var gCoin = new GUIImage(tinyDisplay ? GUIUtils.ICON_TINY_COIN : GUIUtils.ICON_COIN);

            var margin = tinyDisplay ? 0 : GUIManager.STANDARD_MARGIN;
            //Text created at 0,0 so we put the coin beside it, then move the text
            if (coinDirection == DirectionEnum.Right)
            {
                gCoin.AnchorAndAlignWithSpacing(_gTextMoney, SideEnum.Right, SideEnum.Bottom, margin);
            }
            else
            {
                _gTextMoney.AnchorAndAlignWithSpacing(gCoin, SideEnum.Right, SideEnum.CenterY, margin);
            }

            if(tinyDisplay)
            {
                gCoin.ScaledMoveBy(-1, 0);
            }

            AddControls(gCoin, _gTextMoney);
            DetermineSize();
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
        readonly GUIText _gKeysText;
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
        Point _pNumOffset;
        public Item ItemObject {get;}
        readonly GUIImage _gShadow;
        readonly GUIImage _gImg;
        readonly GUIText _gText;
        public ItemBoxDraw DrawNumbers { get; private set; }
        public bool CompareToInventory = false;

        bool _bDrawShadow;

        public GUIItem(Item it, ItemBoxDraw e = ItemBoxDraw.MoreThanOne, bool drawShadow = true)
        {
            HoverControls = false;

            if (it.ID > Constants.BUILDABLE_ID_OFFSET) { _bDrawShadow = false; }
            else { _bDrawShadow = drawShadow; }

            _pNumOffset = new Point(2, 2);

            ItemObject = it;
            DrawNumbers = e;

            Width = GameManager.ScaledTileSize;
            Height = GameManager.ScaledTileSize;

            _gShadow = new GUIImage(Constants.ITEM_SHADOW, DataManager.FILE_MISC_SPRITES);
            _gShadow.CenterOnObject(this);

            _gImg = new GUIImage(ItemObject.SourceRectangle, ItemObject.Texture);
            GUIUtils.SetObjectScale(_gImg, ItemObject.SourceRectangle.Width, ItemObject.SourceRectangle.Height, 1);
            _gImg.CenterOnObject(this);

            _gText = new GUIText(ItemObject.Number.ToString(), true, DataManager.FONT_NUMBERS);
            _gText.SetColor(Color.White);
            AddControls(_gImg, _gText);

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
            if (_bDrawShadow && !ItemObject.CompareType(ItemTypeEnum.Tool))
            {
                _gShadow.Draw(spriteBatch);
            }
            _gImg.Draw(spriteBatch);

            switch (DrawNumbers)
            {
                case ItemBoxDraw.Always:
                    _gText.Draw(spriteBatch);
                    break;
                case ItemBoxDraw.MoreThanOne:
                    if(ItemObject.Number > 1)
                    {
                        _gText.Draw(spriteBatch);
                    }
                    break;
            }
        }

        public void SetNumberOffset(Point val)
        {
            _pNumOffset = val;
            SetTextPosition();
        }
        private void SetTextPosition()
        {
            _gText.AlignToObject(this, SideEnum.BottomRight, GUIUtils.ParentRuleEnum.Skip);
            _gText.ScaledMoveBy(_pNumOffset);
        }

        public override void SetColor(Color c)
        {
            _gText.SetColor(c);
        }
        public void SetImageColor(Color c)
        {
            _gImg.SetColor(c);

            if(_gImg.ObjColor == Color.Black)
            {
                DrawNumbers = ItemBoxDraw.Never;
            }
        }

        public void SetImageAlpha(float alpha)
        {
            _gImg.Alpha(alpha);
        }

        public void DrawShadow(bool drawShadow)
        {
            _bDrawShadow = drawShadow;
        }

        public bool SetNumberComparison(int inventoryNumber)
        {
            bool rv = true;
            CompareToInventory = true;
            DrawNumbers = ItemBoxDraw.Always;

            var num = inventoryNumber;
            if (CompareToInventory && num > Constants.MAX_STACK_COMPARE)
            {
                _gText.SetText(string.Format("{0}+/{1}", Constants.MAX_STACK_COMPARE, ItemObject.Number));
            }
            else
            {
                _gText.SetText(string.Format("{0}/{1}", inventoryNumber, ItemObject.Number));
            }
            SetTextPosition();

            if (inventoryNumber < ItemObject.Number)
            {
                rv = false;
                SetColor(Color.Red);
            }

            return rv;
        }

        protected override void BeginHover()
        {
            if (_gImg.ObjColor != Color.Black)
            {
                var descriptionBox = new GUIItemDescriptionWindow(ItemObject);

                if (GameManager.DescriptionBoxDrawPoint == Point.Zero)
                {
                    descriptionBox.AnchorToScreen(SideEnum.BottomRight);
                }
                else
                {
                    descriptionBox.Position(GameManager.DescriptionBoxDrawPoint);
                }

                GUIManager.OpenHoverObject(descriptionBox, DrawRectangle, true);
            }
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
                    iconDescription = "Upgrade_DailyCrafts";
                    break;
                case GameIconEnum.Book:
                    iconDescription = "Upgrade_Recipe";
                    break;
                case GameIconEnum.Bag:
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
        readonly GUIText _gText;

        public GUIIconText(string text, int spacing, Rectangle sourceRect, GameIconEnum e, SideEnum anchorTo, SideEnum alignTo, string fontText = DataManager.FONT_MAIN)
        {
            var icon = new GUIIcon(sourceRect, e);
            _gText = new GUIText(text, true, fontText);
            _gText.AnchorAndAlignWithSpacing(icon, anchorTo, alignTo, spacing);

            AddControls(icon, _gText);

            DetermineSize();

            //The Icon is at 0,0 so we need to reorient if the text is to the left of it
            var delta = _gText.Position() - icon.Position();
            if (delta.X < 0)
            {
                _gText.MoveBy(-delta.X, 0);
                icon.MoveBy(-delta.X, 0);
            }
        }

        public override void SetColor(Color c)
        {
            _gText.SetColor(c);
        }
    }

    public class GUIConfirmation : GUIWindow
    {
        readonly EmptyDelegate _delConfirm;
        public GUIConfirmation(string text, EmptyDelegate delYes) : base(GUIUtils.WINDOW_BROWN)
        {
            _delConfirm = delYes;

            var title = new GUIText(text);
            title.AnchorToInnerSide(this, SideEnum.TopLeft);
            AddControl(title);

            var buttonYes = new GUIButton("Yes", Confirmation);
            buttonYes.AnchorAndAlignWithSpacing(title, SideEnum.Bottom, SideEnum.Left, 2, GUIUtils.ParentRuleEnum.ForceToParent);

            var buttonNo = new GUIButton("No", GUIManager.CloseConfirmationWindow);
            buttonNo.AnchorAndAlignWithSpacing(buttonYes, SideEnum.Right, SideEnum.Bottom, 2, GUIUtils.ParentRuleEnum.ForceToParent);

            DetermineSize();

            title.AlignToObject(this, SideEnum.CenterX);
        }

        private void Confirmation()
        {
            _delConfirm();
            GUIManager.CloseConfirmationWindow();
        }
    }
}
