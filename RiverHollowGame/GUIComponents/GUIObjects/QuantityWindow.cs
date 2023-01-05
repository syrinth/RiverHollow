using Microsoft.Xna.Framework;
using RiverHollow.Game_Managers;
using RiverHollow.GUIComponents.GUIObjects.GUIWindows;
using RiverHollow.Misc;

namespace RiverHollow.GUIComponents.GUIObjects
{
    class QuantityWindow : GUIMainObject
    {
        int MAX_VALUE => (GameManager.CurrentItem != null ? PlayerManager.Money / GameManager.CurrentItem.Value : 0);
        int _iNum;
        GUIText _gText;
        GUIButton _btnUp;
        GUIButton _btnDown;
        GUIButton _btnOk;

        public QuantityWindow()
        {
            SetMainWindow(GUIWindow.Window_1, GameManager.ScaleIt(58), GameManager.ScaleIt(21));

            _iNum = GameManager.CurrentItem.Number;

            _btnUp = new GUIButton(new Rectangle(128, 48, 7, 7), DataManager.DIALOGUE_TEXTURE, Increment);
            _btnUp.Position(Position());
            _btnUp.ScaledMoveBy(8, 7);
            AddControl(_btnUp);

            _gText = new GUIText(_iNum.ToString("000"));
            _gText.AnchorAndAlignToObject(_btnUp, SideEnum.Right, SideEnum.CenterY, GameManager.ScaleIt(1));
            AddControl(_gText);

            _btnDown = new GUIButton(new Rectangle(137, 48, 7, 7), DataManager.DIALOGUE_TEXTURE, Decrement);
            _btnDown.Position(Position());
            _btnDown.ScaledMoveBy(29, 7);
            AddControl(_btnDown);

            _btnOk = new GUIButton(new Rectangle(130, 55, 11, 9), DataManager.DIALOGUE_TEXTURE, ProceedToPurchase);
            _btnOk.Position(Position());
            _btnOk.ScaledMoveBy(40, 6);
            AddControl(_btnOk);
        }

        public override bool ProcessRightButtonClick(Point mouse)
        {
            GameManager.SetSelectedItem(null);
            return base.ProcessRightButtonClick(mouse);
        }

        public void Increment()
        {
            if (_iNum < MAX_VALUE) { _iNum++; }
            else { _iNum = 1; }

            Refresh();
        }

        public void Decrement()
        {
            if (_iNum > 1) { _iNum--; }
            else { _iNum = MAX_VALUE; }

            Refresh();
        }

        public void Refresh()
        {
            _gText.SetText(_iNum.ToString("000"));
        }

        public void ProceedToPurchase()
        {
            if (InventoryManager.HasSpaceInInventory(GameManager.CurrentItem.ID, _iNum))
            {
                GameManager.CurrentItem.Add(_iNum - 1);
                TextEntry entry = DataManager.GetGameTextEntry("BuyMerch_Confirm");
                entry.FormatText(string.Format(GameManager.CurrentItem.Name() + " x" + GameManager.CurrentItem.Number), GameManager.CurrentItem.TotalBuyValue);
                GUIManager.CloseMainObject();
                GUIManager.OpenTextWindow(entry);
            }
            else
            {
                GUIManager.OpenTextWindow(DataManager.GetGameTextEntry("BuyMerch_NoSpace"));
            }
        }
    }
}
