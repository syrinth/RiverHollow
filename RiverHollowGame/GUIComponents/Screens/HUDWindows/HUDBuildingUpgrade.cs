using Microsoft.Xna.Framework;
using RiverHollow.Buildings;
using RiverHollow.Game_Managers;
using RiverHollow.GUIComponents.GUIObjects;
using RiverHollow.GUIComponents.GUIObjects.GUIWindows;
using RiverHollow.Misc;
using System.Collections.Generic;
using static RiverHollow.Utilities.Enums;

namespace RiverHollow.GUIComponents.Screens.HUDWindows
{
    public class HUDBuildingUpgrade : GUIMainObject
    {
        readonly Building _building;

        readonly List<GUIIconText> _liIcons;

        Dictionary<int, int> _diUpgradeItems;
        int _iCost;

        public HUDBuildingUpgrade(Building b)
        {
            _building = b;
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

            var levelTab = new GUIImage(GUIUtils.LEVEL_TAB);
            levelTab.PositionAndMove(_winMain, new Point(59, 16));

            GUIText name = new GUIText(_building.Name);
            name.AnchorToObjectInnerSide(_winMain, SideEnum.Top, GameManager.ScaledPixel * 3);
            _winMain.AddControl(name);

            GUIText lvl = new GUIText("Level " + _building.Level);
            lvl.AnchorToObjectInnerSide(_winMain, SideEnum.Top, GameManager.ScaledPixel * 18);
            _winMain.AddControl(lvl);

            //Profit Display
            int profit = (int)(_building.GetShopProfitModifier() * 100);
            var profitStr = string.Format("+{0}", profit);
            var profits = new GUIIconText(profitStr, 3, GUIUtils.ICON_COIN, GameIconEnum.Coin, SideEnum.Bottom, SideEnum.CenterX);
            profits.PositionAndMove(_winMain, new Point(111, 36));

            var scroll = new GUIImage(GUIUtils.HUD_SCROLL_L);
            scroll.PositionAndMove(_winMain, new Point(10, 65));

            _liIcons.Add(profits);

            if (_building.UpgradeQueued)
            {
                GUIText upgradeText = new GUIText("Upgrade in Progress");
                upgradeText.AnchorAndAlignWithSpacing(scroll, SideEnum.Bottom, SideEnum.CenterX, 16);
                _winMain.AddControl(upgradeText);
            }
            else
            {
                Upgrade[] buildingUpgrades = _building.GetAllUpgrades();
                bool isMaxLevel = _building.MaxLevel();
                bool hasUpgrades = buildingUpgrades.Length > 0;
                if (!isMaxLevel && hasUpgrades)
                {
                    GUIButton btn = new GUIButton(GUIUtils.BTN_BUY, Upgrade);
                    btn.Position(_winMain.Position());
                    btn.ScaledMoveBy(135, 93);
                    _winMain.AddControl(btn);

                    Upgrade nextUpgrade = buildingUpgrades[_building.Level - 1];
                    Dictionary<int, int> upgradeItems = nextUpgrade.UpgradeRequirements;

                    Color textColor = Color.White;
                    if (!InventoryManager.HasSufficientItems(upgradeItems) || PlayerManager.Money < nextUpgrade.Cost)
                    {
                        textColor = Color.Red;
                        btn.Enable(false);
                    }

                    var upgradeList = new List<GUIIconText>();
                    if (nextUpgrade.Profit > 0)
                    {
                        AddNewUpgrade(ref upgradeList, GameIconEnum.Coin, GUIUtils.ICON_COIN, nextUpgrade.Profit.ToString());
                    }
                    if (nextUpgrade.CraftAmount > 0)
                    {
                        AddNewUpgrade(ref upgradeList, GameIconEnum.Hammer, GUIUtils.ICON_HAMMER, nextUpgrade.CraftAmount.ToString());
                    }
                    if (nextUpgrade.FormulaLevel > 0)
                    {
                        AddNewUpgrade(ref upgradeList, GameIconEnum.Book, GUIUtils.ICON_BOOK, nextUpgrade.FormulaLevel.ToString());
                    }

                    GUIUtils.CreateSpacedRowAgainstObject(new List<GUIObject>(upgradeList), _winMain, scroll, 16, 8);

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

                    GUIText cost = new GUIText(nextUpgrade.Cost);
                    cost.AnchorAndAlignWithSpacing(btn, SideEnum.Left, SideEnum.CenterY, 2);
                    _winMain.AddControl(cost);

                    _diUpgradeItems = upgradeItems;
                    _iCost = nextUpgrade.Cost;
                }
                else
                {
                    GUIText upgradeText = new GUIText("Max Level");
                    upgradeText.AnchorAndAlignWithSpacing(scroll, SideEnum.Bottom, SideEnum.CenterX, 16);
                    _winMain.AddControl(upgradeText);
                }
            }
        }
        
        private void AddNewUpgrade(ref List<GUIIconText> upgradeList, GameIconEnum e, Rectangle iconRect, string value)
        {
            upgradeList.Add(new GUIIconText(value, 1, iconRect, e, SideEnum.Right, SideEnum.CenterY));
        }

        private void Upgrade()
        {
            if (InventoryManager.ExpendResources(_diUpgradeItems) && PlayerManager.Money >= _iCost)
            {
                PlayerManager.TakeMoney(_iCost);
                _building.QueueUpgrade();
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
