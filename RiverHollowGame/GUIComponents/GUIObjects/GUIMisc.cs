using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.Game_Managers;
using System.Collections.Generic;
using static RiverHollow.Game_Managers.GameManager;
using RiverHollow.Items;
using static RiverHollow.Utilities.Enums;
using RiverHollow.Utilities;

namespace RiverHollow.GUIComponents.GUIObjects.GUIWindows
{
    public class GUIMonsterEnergy : GUIImage
    {
        public GUIMonsterEnergy() : base(new Rectangle(4, 20, 8 , 8), ScaleIt(8), ScaleIt(8), DataManager.DIALOGUE_TEXTURE)
        {
        }
    }

    public class GUIAttributeIcon : GUIObject
    {
        AttributeEnum _eAttribute;
        GUIImage _gIcon;
        GUIText _gText;
        public GUIAttributeIcon(AttributeEnum attribute, string value, int spacing = 1)
        {
            _eAttribute = attribute;
            _gIcon = DataManager.GetIcon(GameManager.GetGameIconFromAttribute(attribute));

            _gText = new GUIText(value, true, DataManager.FONT_STAT_DISPLAY);
            _gText.AnchorAndAlignToObject(_gIcon, SideEnum.Right, SideEnum.CenterY, ScaleIt(spacing));
            AddControl(_gIcon);
            AddControl(_gText);

            Width = _gText.Right - _gIcon.Left;
            Height = _gIcon.Height;
        }

        public GUIAttributeIcon(AttributeEnum attribute, int value, int spacing = 1) : this(attribute, value.ToString(), spacing) { }

        public void SetText(int val)
        {
            SetText(val.ToString());
        }
        public void SetText(string str)
        {
            _gText.SetText(str);
            Width = _gText.Right - _gIcon.Left;
        }

        public override void SetColor(Color c)
        {
            _gText.SetColor(c);
        }

        public override bool ProcessHover(Point mouse)
        {
            bool rv = false;
            if (Contains(mouse))
            {
                rv = true;
                if (_eAttribute != AttributeEnum.Vitality)
                {
                    GUITextWindow win = new GUITextWindow(DataManager.GetGameTextEntry("Desc_" + _eAttribute.ToString()), new Vector2(mouse.ToVector2().X, mouse.ToVector2().Y + 32));
                    GUIManager.OpenHoverWindow(win, DrawRectangle, true);
                }
            }
            return rv;
        }
    }

    public class GUIEffectDetailIcon : GUIObject
    {
        public GUIEffectDetailIcon(AttributeEnum attribute, StatusTypeEnum effect)
        {
            GUIImage attributeIcon = DataManager.GetIcon(GameManager.GetGameIconFromAttribute(attribute));
            GUIImage arrow = null;

            if (effect == StatusTypeEnum.Buff)
            {
                arrow = DataManager.GetIcon(GameIconEnum.BuffArrow);
                attributeIcon.AnchorAndAlignToObject(arrow, SideEnum.Bottom, SideEnum.CenterX);
                attributeIcon.ScaledMoveBy(0, -3);
                Height = attributeIcon.Bottom - arrow.Top;
            }
            else if (effect == StatusTypeEnum.Debuff)
            {
                arrow = DataManager.GetIcon(GameIconEnum.DebuffArrow);
                arrow.AnchorAndAlignToObject(attributeIcon, SideEnum.Bottom, SideEnum.CenterX);
                arrow.ScaledMoveBy(0, -3);
                Height = arrow.Bottom - attributeIcon.Top;
            }

            AddControl(arrow);
            AddControl(attributeIcon);
            Width = arrow.Width;
        }
    }

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
            _gCoin = DataManager.GetIcon(GameIconEnum.Coin);

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
            _gKeys = DataManager.GetIcon(GameIconEnum.Key);

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
            _gUnchecked.MoveBy(new Vector2(0, delta));
            _gChecked.MoveBy(new Vector2(0, delta));

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
        GUIText _gText;
        public bool DrawNumber = true;

        public GUIItem(Item it)
        {
            ItemObject = it;
            _gImg = new GUIImage(ItemObject.SourceRectangle, ItemObject.SourceRectangle.Width, ItemObject.SourceRectangle.Height, ItemObject.Texture);
            _gImg.SetScale(CurrentScale);

            _gText = new GUIText(ItemObject.Number.ToString(), true, DataManager.FONT_NUMBER_DISPLAY);
            _gText.SetColor(Color.White);

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
            if (ItemObject.Stacks() && _gText != null && DrawNumber) {
                _gText.Draw(spriteBatch);
            }
        }

        public override bool ProcessHover(Point mouse)
        {
            bool rv = false;
            if (Contains(mouse))
            {
                if (!GUIManager.IsHoverWindowOpen())
                {
                    GUIItemDescriptionWindow win = new GUIItemDescriptionWindow(ItemObject, new Vector2(DrawRectangle.Left, DrawRectangle.Bottom));
                    GUIManager.OpenHoverWindow(win, Util.FloatRectangle(Position(), Width, Height), true);
                }
                rv = true;
            }
            return rv;
        }

        public override void SetColor(Color c)
        {
            _gText.SetColor(c);
        }
    }

    public class ConstructBox : GUIObject
    {
        public static int CONSTRUCTBOX_WIDTH = 544; //(GUIManager.MAIN_COMPONENT_WIDTH) - (_gWindow.EdgeSize * 2) - ScaledTileSize
        public static int CONSTRUCTBOX_HEIGHT = 128; //(GUIManager.MAIN_COMPONENT_HEIGHT / HUDTaskLog.MAX_SHOWN_TASKS) - (_gWindow.EdgeSize * 2)

        GUIWindow _window;
        GUIText _gName;
        public int _iBuildID;
        public delegate void SelectConstructID(int objID);
        private SelectConstructID _delAction;

        public ConstructBox(SelectConstructID del)
        {
            _delAction = del;

            int boxWidth = CONSTRUCTBOX_WIDTH;
            int boxHeight = CONSTRUCTBOX_HEIGHT;            

            _window = new GUIWindow(GUIWindow.Window_1, boxWidth, boxHeight);
            AddControl(_window);
            Width = _window.Width;
            Height = _window.Height;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (Show())
            {
                _window.Draw(spriteBatch);
            }
        }
        public override bool ProcessLeftButtonClick(Point mouse)
        {
            bool rv = false;
            if (Contains(mouse) && _delAction != null)
            {
                rv = true;
                _delAction(_iBuildID);
            }

            return rv;
        }

        public override bool ProcessHover(Point mouse) { return false; }

        public void SetConstructionInfo(int id, string objName, Dictionary<int, int> requiredToMake)
        {
            _iBuildID = id;

            Color textColor = Color.Black;
            if (!InventoryManager.HasSufficientItems(requiredToMake))
            {
                textColor = Color.Red;
                _delAction = null;
            }

            _gName = new GUIText(objName);
            _gName.AnchorToInnerSide(_window, SideEnum.TopLeft);
            _gName.SetColor(textColor);

            List<GUIItemBox> list = new List<GUIItemBox>();
            foreach (KeyValuePair<int, int> kvp in requiredToMake)
            {
                GUIItemBox box = new GUIItemBox(DataManager.GetItem(kvp.Key, kvp.Value));

                if (list.Count == 0) { box.AnchorToInnerSide(_window, SideEnum.BottomRight); }
                else { box.AnchorAndAlignToObject(list[list.Count - 1], SideEnum.Left, SideEnum.Bottom); }

                if (!InventoryManager.HasItemInPlayerInventory(kvp.Key, kvp.Value)) { box.SetColor(Color.Red); }

                list.Add(box);
            }
        }

        public void SetConstructionInfo(int id, string objName, int number)
        {
            _iBuildID = id;

            _gName = new GUIText(objName);
            _gName.AnchorToInnerSide(_window, SideEnum.TopLeft);

            GUIText text = new GUIText(number);
            text.AnchorToInnerSide(_window, SideEnum.Right);
        }
    }
}
