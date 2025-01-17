using Microsoft.Xna.Framework;
using RiverHollow.Game_Managers;
using RiverHollow.GUIComponents.GUIObjects;
using RiverHollow.GUIComponents.GUIObjects.GUIWindows;
using RiverHollow.Misc;
using System.Collections.Generic;
using static RiverHollow.Utilities.Enums;

namespace RiverHollow.GUIComponents.Screens.HUDWindows
{
    public class HUDGlobalUpgrade : GUIMainObject
    {
        readonly Upgrade _upgrade;

        readonly List<GUIIconText> _liIcons;

        Dictionary<int, int> _diUpgradeItems;
        int _iCost;

        public HUDGlobalUpgrade(Upgrade x)
        {
            _upgrade = x;
            _liIcons = new List<GUIIconText>();

            _winMain = new GUIWindow(GUIUtils.WINDOW_WOODEN_TITLE, GameManager.ScaleIt(162), GameManager.ScaleIt(119));
            AddControl(_winMain);

            DisplayDetails();

            Width = _winMain.Width;
            Height = _winMain.Height;

            CenterOnScreen();
        }

        private void DisplayDetails()
        {
            _winMain.CleanControls();

            GUIText name = new GUIText(_upgrade.Name);
            name.AnchorToObjectInnerSide(_winMain, SideEnum.Top, GameManager.ScaledPixel * 3);
            _winMain.AddControl(name);

            //Profit Display
            //GUIText description = new GUIText(_upgrade.Description);
            //name.AnchorToObjectInnerSide(_winMain, SideEnum.Top, GameManager.ScaledPixel * 3);
            //_winMain.AddControl(name);

            var scroll = new GUIImage(GUIUtils.HUD_SCROLL_L);
            scroll.PositionAndMove(_winMain, new Point(10, 65));

            GUIText upgradeText = null;
            switch (_upgrade.Status)
            {
                case UpgradeStatusEnum.InProgress:
                    upgradeText = new GUIText("Upgrade in Progress");
                    upgradeText.AnchorAndAlignWithSpacing(scroll, SideEnum.Bottom, SideEnum.CenterX, 16);
                    _winMain.AddControl(upgradeText);
                    break;
                case UpgradeStatusEnum.Completed:
                    upgradeText = new GUIText("Upgrade Completed");
                    upgradeText.AnchorAndAlignWithSpacing(scroll, SideEnum.Bottom, SideEnum.CenterX, 16);
                    _winMain.AddControl(upgradeText);
                    break;
                default:
                    GUIButton btn = new GUIButton(GUIUtils.BTN_BUY, Upgrade);
                    btn.Position(_winMain.Position());
                    btn.ScaledMoveBy(135, 93);
                    _winMain.AddControl(btn);

                    Dictionary<int, int> upgradeItems = _upgrade.UpgradeRequirements;

                    Color textColor = Color.White;
                    if (!InventoryManager.HasSufficientItems(upgradeItems) || PlayerManager.Money < _upgrade.Cost)
                    {
                        textColor = Color.Red;
                        btn.Enable(false);
                    }

                    string bonusValue = string.Empty;
                    Rectangle drawRect = new Rectangle();
                    GameIconEnum icon = GameIconEnum.None;
                    if (_upgrade.Profit > 0)
                    {
                        icon = GameIconEnum.Coin;
                        drawRect = GUIUtils.ICON_COIN;
                        bonusValue = _upgrade.Profit.ToString();
                    }
                    else if (_upgrade.CraftAmount > 0)
                    {
                        icon = GameIconEnum.Hammer;
                        drawRect = GUIUtils.ICON_HAMMER;
                        bonusValue = _upgrade.CraftAmount.ToString();
                    }
                    else if (_upgrade.FormulaLevel > 0)
                    {
                        icon = GameIconEnum.Book;
                        drawRect = GUIUtils.ICON_BOOK;
                        bonusValue = _upgrade.FormulaLevel.ToString();
                    }

                    var bonusIcon = new GUIIconText(bonusValue, 1, drawRect, icon, SideEnum.Right, SideEnum.CenterY);
                    bonusIcon.CenterOnObject(_winMain);
                    bonusIcon.AnchorToObject(scroll, SideEnum.Bottom, 3);

                    _liIcons.Add(bonusIcon);

                    List<GUIItemBox> list = new List<GUIItemBox>();
                    foreach (KeyValuePair<int, int> kvp in upgradeItems)
                    {
                        GUIItemBox box = new GUIItemBox(DataManager.GetItem(kvp.Key, kvp.Value));
                        if (list.Count == 0)
                        {
                            box.Position(_winMain.Position());
                            box.ScaledMoveBy(9, 91);
                        }
                        else { box.AnchorAndAlignWithSpacing(list[list.Count - 1], SideEnum.Right, SideEnum.Bottom, 2); }
                        _winMain.AddControl(box);

                        if (!InventoryManager.HasItemInPlayerInventory(kvp.Key, kvp.Value)) { box.SetColor(Color.Red); }

                        list.Add(box);
                    }

                    GUIText cost = new GUIText(_upgrade.Cost);
                    cost.AnchorAndAlignWithSpacing(btn, SideEnum.Left, SideEnum.CenterY, 2);
                    _winMain.AddControl(cost);

                    _diUpgradeItems = upgradeItems;
                    _iCost = _upgrade.Cost;
                    break;
            }
        }

        private void Upgrade()
        {
            if (InventoryManager.ExpendResources(_diUpgradeItems) && PlayerManager.Money >= _iCost)
            {
                PlayerManager.TakeMoney(_iCost);
                _upgrade.ChangeStatus(UpgradeStatusEnum.InProgress);
                DisplayDetails();
            }
        }

        public override bool ProcessRightButtonClick(Point mouse)
        {
            bool rv = false;
            if (Contains(mouse))
            {
                rv = true;
                GUIManager.CloseMainObject();
            }

            return rv;
        }
    }
}
