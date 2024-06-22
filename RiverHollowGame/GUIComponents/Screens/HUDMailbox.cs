using Microsoft.Xna.Framework;
using RiverHollow.Game_Managers;
using RiverHollow.GUIComponents.GUIObjects;
using RiverHollow.GUIComponents.GUIObjects.GUIWindows;
using RiverHollow.Misc;
using RiverHollow.Utilities;

namespace RiverHollow.GUIComponents.Screens
{
    internal class HUDMailbox : GUIMainObject
    {
        const int MAX_DETAIL_DISPLAY = 6;
        readonly GUIButton _btnUp;
        readonly GUIButton _btnDown;

        int _iIndex = 0;
        int _iSelectedIndex = -1;

        readonly LetterDetailsDisplay[] _arrLetterDetails;

        LetterDisplay _gLetterDisplay;
        LetterDetailsDisplay _gCurrentLetterDetail;

        private delegate void LetterDetailsDelegate(LetterDetailsDisplay l);

        public HUDMailbox()
        {
            _winMain = new GUIWindow(GUIUtils.WINDOW_WOODEN_TITLE, GameManager.ScaleIt(382), GameManager.ScaleIt(177));
            AddControl(_winMain);

            var name = new GUIText("Mailbox");
            name.AlignToObject(_winMain, SideEnum.Top);
            name.ScaledMoveBy(0, 2);
            name.AlignToObject(_winMain, SideEnum.CenterX);

            var separator = new GUIImage(GUIUtils.VERTICAL_SEPARATOR)
            {
                Height = GameManager.ScaleIt(157)
            };
            separator.PositionAndMove(_winMain, 187, 16);

            var displayWindow = new GUIWindow(GUIUtils.WINDOW_LETTER_LIGHT, GameManager.ScaleIt(162), GameManager.ScaleIt(151));
            displayWindow.AnchorToInnerSide(_winMain, SideEnum.TopLeft, 2);

            _arrLetterDetails = new LetterDetailsDisplay[6];

            _btnUp = new GUIButton(GUIUtils.BTN_UP_SMALL, BtnUpClick);
            _btnUp.AnchorAndAlignWithSpacing(displayWindow, SideEnum.Right, SideEnum.Top, Constants.MAILBOX_EDGE);

            _btnDown = new GUIButton(GUIUtils.BTN_DOWN_SMALL, BtnDownClick);
            _btnDown.AnchorAndAlignWithSpacing(displayWindow, SideEnum.Right, SideEnum.Bottom, Constants.MAILBOX_EDGE);
            AddControls(_btnDown, _btnUp);

            DisplayLetterDetails();

            DetermineSize();
            ScaledMoveBy(49, 47);
        }

        private void DisplayLetterDetails()
        {
            foreach (var obj in _arrLetterDetails)
            {
                obj?.RemoveSelfFromControl();
            }

            var letters = TownManager.GetAllLetters();
            for (int i = 0; i < MAX_DETAIL_DISPLAY; i++)
            {
                if (i < letters.Count)
                {
                    _arrLetterDetails[i] = new LetterDetailsDisplay(letters[i + _iIndex], SetCurrentLetter);

                    if (i == 0)
                    {
                        _arrLetterDetails[i].AnchorToInnerSide(_winMain, SideEnum.TopLeft, 2);
                    }
                    else
                    {
                        _arrLetterDetails[i].AnchorAndAlignWithSpacing(_arrLetterDetails[i - 1], SideEnum.Bottom, SideEnum.Left, -1, GUIUtils.ParentRuleEnum.ForceToParent);
                    }
                }
            }

            if (letters.Count < MAX_DETAIL_DISPLAY)
            {
                _btnDown.Show(false);
                _btnUp.Show(false);
            }

            _btnUp.Enable(_iIndex > 0);
            _btnDown.Enable(_iIndex + MAX_DETAIL_DISPLAY < letters.Count);
        }

        public override bool ProcessRightButtonClick(Point mouse)
        {
            GUIManager.CloseMainObject();
            return true;
        }

        private void BtnUpClick()
        {
            if (_iIndex - 1 >= 0)
            {
                _iIndex--;
                DisplayLetterDetails();
                if (Util.IndexInRange(++_iSelectedIndex, _arrLetterDetails))
                {
                    _arrLetterDetails[_iSelectedIndex].FlipWindowData();
                }
            }
        }

        private void BtnDownClick()
        {
            var count = TownManager.GetAllLetters().Count;
            if (_iIndex + MAX_DETAIL_DISPLAY < count)
            {
                _iIndex++;
                DisplayLetterDetails();

                if (Util.IndexInRange(--_iSelectedIndex, _arrLetterDetails))
                {
                    _arrLetterDetails[_iSelectedIndex].FlipWindowData();
                }
            }
        }

        private void SetCurrentLetter(LetterDetailsDisplay l)
        {
            if (_gCurrentLetterDetail != l)
            {
                if (Util.IndexInRange(_iSelectedIndex, _arrLetterDetails))
                {
                    _arrLetterDetails[_iSelectedIndex].FlipWindowData();
                }

                for (int i = 0; i < _arrLetterDetails.Length; i++)
                {
                    if (_arrLetterDetails[i] == l)
                    {
                        _iSelectedIndex = i;
                    }
                }
                _gCurrentLetterDetail = l;
                _arrLetterDetails[_iSelectedIndex].FlipWindowData();

                TownManager.ReadLetter(l.MyLetter);

                _winMain.RemoveControl(_gLetterDisplay);
                _gLetterDisplay?.Unregister();
                _gLetterDisplay = new LetterDisplay(l.MyLetter, OpenConfirm, DetailUpdate);
                _gLetterDisplay.PositionAndMove(_winMain, 197, 19);
            }
        }

        public override void CloseMainWindow()
        {
            base.CloseMainWindow();
            _gLetterDisplay?.Unregister();
        }

        private void DeleteConfirm()
        {
            if (_gCurrentLetterDetail != null)
            {
                TownManager.DeleteLetter(_gCurrentLetterDetail.MyLetter);

                _winMain.RemoveControl(_gLetterDisplay);
                _gLetterDisplay?.Unregister();
                _gLetterDisplay = null;

                _winMain.RemoveControls(_arrLetterDetails);
                _gCurrentLetterDetail = null;

                DisplayLetterDetails();
            }

        }

        private void OpenConfirm()
        {
            GUIManager.OpenConfirmationWindow("Really Delete?", DeleteConfirm);
        }

        private void DetailUpdate()
        {
            _gCurrentLetterDetail.UpdateItemState();
        }

        private class LetterDetailsDisplay : GUIWindow
        {
            private readonly LetterDetailsDelegate _delAction;
            public Letter MyLetter { get; }

            public GUIImage _gRead;
            public GUIImage _gGift;

            public LetterDetailsDisplay(Letter l, LetterDetailsDelegate del) : base(GUIUtils.WINDOW_LETTER_DARK, GameManager.ScaleIt(162), GameManager.ScaleIt(26))
            {
                MyLetter = l;
                _delAction = del;

                var facePoint = DataManager.GetPointByIDKey(l.NPCID, "Face", Enums.DataType.Actor);
                var gFace = new GUIImage(new Rectangle(Util.MultiplyPoint(facePoint,Constants.TILE_SIZE), Constants.BASIC_TILE), DataManager.FACES_TEXTURE);
                gFace.AnchorToInnerSide(this, SideEnum.TopLeft, 3);

                var gName = new GUIText(MyLetter.Text.GetTagValue("Name"));
                gName.PositionAndMove(gFace, 22, 2);

                _gRead = new GUIImage(MyLetter.LetterRead ? GUIUtils.ICON_LETTER_READ : GUIUtils.ICON_LETTER_UNREAD);
                _gRead.PositionAndMove(this, 141, 4);

                UpdateItemState();
            }

            public void FlipWindowData()
            {
                if(_winData.Equals(GUIUtils.WINDOW_LETTER_DARK))
                {
                    _winData = GUIUtils.WINDOW_LETTER_LIGHT;
                }
                else
                {
                    _winData = GUIUtils.WINDOW_LETTER_DARK;
                }
            }

            public void UpdateItemState()
            {
                if (MyLetter.ItemWaiting)
                {
                    _gGift?.RemoveSelfFromControl();
                    _gGift = new GUIImage(GUIUtils.ICON_LETTER_GIFT);
                    _gGift.PositionAndMove(_gRead, 8, 9);
                }
                else if(_gGift != null)
                {
                    _gGift.RemoveSelfFromControl();
                    _gGift = null;
                }
            }

            public override bool ProcessLeftButtonClick(Point mouse)
            {
                bool rv = false;
                if (base.Contains(mouse))
                {
                    rv = true;

                    if (!MyLetter.LetterRead)
                    {
                        RemoveControl(_gRead);
                        _gRead = new GUIImage(GUIUtils.ICON_LETTER_READ);
                        _gRead.PositionAndMove(this, 141, 4);

                        UpdateItemState();
                    }

                    _delAction(this);                    
                }

                return rv;
            }
        }

        private class LetterDisplay : GUIObject
        {
            private const int SCROLL_VALUE = 20;
            private static readonly int SCALED_EDGE = GameManager.ScaleIt(Constants.MAILBOX_EDGE);

            readonly Letter DisplayLetter;

            readonly GUIText _gText;
            readonly GUIButton _btnUp;
            readonly GUIButton _btnDown;
            readonly GUIWindow _win;

            readonly GUIImage _gTrash;
            readonly GUIItem _gItem;

            readonly EmptyDelegate _delDelete;
            readonly EmptyDelegate _delUpdateDetail;

            private static bool LetterHasItem(Letter theLetter)
            {
                return theLetter.ItemState != Enums.LetterItemStateEnum.None;
            }

            private Rectangle GetTextRectangle()
            {
                var r = _win.InnerRectangle();
                r.Location += new Point(SCALED_EDGE, SCALED_EDGE);
                r.Width -= SCALED_EDGE * 2;
                r.Height -= SCALED_EDGE * 2;

                return r;
            }

            public LetterDisplay(Letter theLetter, EmptyDelegate delDelete, EmptyDelegate delUpdateDetail)
            {
                DisplayLetter = theLetter;
                _delDelete = delDelete;
                _delUpdateDetail = delUpdateDetail;

                var strText = theLetter.Text.GetFormattedText();

                _win = new GUIWindow(GUIUtils.WINDOW_LETTER_LIGHT, GameManager.ScaleIt(162), GameManager.ScaleIt(LetterHasItem(DisplayLetter) ? 107 : 151));
                AddControl(_win);

                _btnUp = new GUIButton(GUIUtils.BTN_UP_SMALL, BtnUpClick);
                _btnUp.AnchorAndAlignWithSpacing(_win, SideEnum.Right, SideEnum.Top, Constants.MAILBOX_EDGE);

                _btnDown = new GUIButton(GUIUtils.BTN_DOWN_SMALL, BtnDownClick);
                _btnDown.AnchorAndAlignWithSpacing(_win, SideEnum.Right, SideEnum.Bottom, Constants.MAILBOX_EDGE);
                AddControls(_btnDown, _btnUp);

                _gText = new GUIText(strText);
                _gText.ParseAndSetText(strText, GetTextRectangle().Width, 99, true);
                _gText.AnchorToInnerSide(_win, SideEnum.TopLeft, Constants.MAILBOX_EDGE);

                _gTrash = new GUIImage(GUIUtils.ICON_GARBAGE);

                var bounds = GetTextRectangle();
                if (_gText.Height < bounds.Height)
                {
                    _btnDown.Show(false);
                    _btnUp.Show(false);
                }

                if (LetterHasItem(DisplayLetter))
                {
                    var itemWindowTitle = new GUIWindow(GUIUtils.WINDOW_WOODEN_PANEL, GameManager.ScaleIt(162), GameManager.ScaleIt(16));
                    itemWindowTitle.AnchorAndAlignWithSpacing(_win, SideEnum.Bottom, SideEnum.Left, 4);

                    var text = new GUIText(Constants.MAILBOX_ITEMS);
                    text.CenterOnObject(itemWindowTitle);


                    var gBox = new GUIImage(GUIUtils.MAIL_ITEM_BOX);
                    gBox.AnchorAndAlignWithSpacing(itemWindowTitle, SideEnum.Bottom, SideEnum.CenterX, 3);

                    if (DisplayLetter.ItemData.Item1 != -1)
                    {
                        _gItem = new GUIItem(DataManager.GetItem(DisplayLetter.ItemData.Item1, DisplayLetter.ItemData.Item2));
                        _gItem.DrawShadow(false);
                        _gItem.CenterOnObject(gBox);

                        if (theLetter.ItemState == Enums.LetterItemStateEnum.Received)
                        {
                            _gItem.Alpha(0.5f);
                        }
                    }

                    _gTrash.PositionAndMove(itemWindowTitle, 162, 26);

                    AddControls(itemWindowTitle, text, gBox, _gItem, _gTrash);
                }
                else
                {
                    _gTrash.AnchorAndAlign(_win, SideEnum.Right, SideEnum.Bottom);
                    _gTrash.ScaledMoveBy(0, 2);
                    _btnDown.AnchorAndAlignWithSpacing(_gTrash, SideEnum.Top, SideEnum.CenterX, Constants.MAILBOX_EDGE);
                    AddControl(_gTrash);
                }



                GUIManager.RegisterBoundedControls(_gText);

                DetermineSize();
            }

            public override bool ProcessLeftButtonClick(Point mouse)
            {
                bool rv = GUIUtils.ProcessLeftMouseButton(mouse, _btnDown, _btnUp);

                if (!rv)
                {
                    if (_gItem != null && _gItem.Contains(mouse) && DisplayLetter.ItemWaiting && InventoryManager.HasSpaceInInventory(_gItem.ItemObject.ID, _gItem.ItemObject.Number))
                    {
                        _gItem.Alpha(0.5f);
                        DisplayLetter.ChangeItemState(Enums.LetterItemStateEnum.Received);
                        InventoryManager.AddToInventory(_gItem.ItemObject);
                        _delUpdateDetail();
                    }
                    else if (_gTrash.Contains(mouse))
                    {
                        _delDelete();
                    }
                }

                return rv;
            }

            public override bool ProcessHover(Point mouse)
            {
                return Contains(mouse);
            }

            private void BtnUpClick()
            {
                if (GUIManager.ControlBounds.Top > _gText.Top)
                {
                    var val = SCROLL_VALUE;
                    var delta = ((GUIManager.ControlBounds.Top + Constants.MAILBOX_EDGE) - _gText.Top) / GameManager.CurrentScale;
                    if (delta < val)
                    {
                        val = delta;
                    }
                    _gText.ScaledMoveBy(0, val);
                }

                _btnDown.Enable(true);

                if (GUIManager.ControlBounds.Top <= _gText.Top)
                {
                    _btnUp.Enable(false);
                }
            }

            private void BtnDownClick()
            {
                if (_gText.Bottom > GUIManager.ControlBounds.Bottom)
                {
                    var val = SCROLL_VALUE;
                    var delta = (_gText.Bottom - (GUIManager.ControlBounds.Bottom - Constants.MAILBOX_EDGE)) / GameManager.CurrentScale;
                    if (delta < val)
                    {
                        val = delta;
                    }
                    _gText.ScaledMoveBy(0, -val);
                }

                _btnUp.Enable(true);

                if (_gText.Bottom <= GUIManager.ControlBounds.Bottom)
                {
                    _btnDown.Enable(false);
                }
            }

            public override void Position(Point value)
            {
                base.Position(value);
                GUIManager.RegisterScissorsRectangle(GetTextRectangle());
            }

            public void Unregister()
            {
                GUIManager.UnregisterBoundedControls(_gText);
            }
        }
    }
}
