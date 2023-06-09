﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.Buildings;
using RiverHollow.Game_Managers;
using RiverHollow.GUIComponents.GUIObjects;
using RiverHollow.GUIComponents.GUIObjects.GUIWindows;
using RiverHollow.Misc;
using RiverHollow.Utilities;
using System.Collections.Generic;
using static RiverHollow.Utilities.Enums;

namespace RiverHollow.GUIComponents.Screens.HUDWindows
{
    public class HUDBuildingUpgrade : GUIMainObject
    {
        readonly GUIImage _gImage;
        readonly Building _building;

        List<GUIIconText> _liIcons;

        Dictionary<int, int> _diUpgradeItems;
        int _iCost;

        public HUDBuildingUpgrade(Building b)
        {
            _building = b;
            _liIcons = new List<GUIIconText>();

            _gImage = new GUIImage(GUIUtils.WIN_UPGRADE);
            AddControl(_gImage);

            DisplayDetails();

            Width = _gImage.Width;
            Height = _gImage.Height;

            CenterOnScreen();
        }

        private void DisplayDetails()
        {
            _gImage.CleanControls();

            var levelTab = new GUIImage(GUIUtils.LEVEL_TAB);
            levelTab.PositionAndMove(_gImage, new Point(59, 16));

            GUIText name = new GUIText(_building.Name());
            name.AnchorToObjectInnerSide(_gImage, SideEnum.Top, GameManager.ScaledPixel * 3);
            _gImage.AddControl(name);

            GUIText lvl = new GUIText("Level " + _building.Level);
            lvl.AnchorToObjectInnerSide(_gImage, SideEnum.Top, GameManager.ScaledPixel * 18);
            _gImage.AddControl(lvl);

            //Traveler Display
            var travelerStr = string.Format("+{0}", _building.GetTravelerChance());
            var travelers = new GUIIconText(travelerStr, 3, GUIUtils.ICON_TRAVELER, GameIconEnum.Traveler, SideEnum.Bottom, SideEnum.CenterX);
            travelers.PositionAndMove(_gImage, new Point(37, 36));
            _gImage.AddControl(travelers);

            //Profit Display
            int profit = (int)(_building.GetShopProfitModifier() * 100);
            var profitStr = string.Format("+{0}", profit);
            var profits = new GUIIconText(profitStr, 3, GUIUtils.ICON_COIN, GameIconEnum.Coin, SideEnum.Bottom, SideEnum.CenterX);
            profits.PositionAndMove(_gImage, new Point(111, 36));

            var scroll = new GUIImage(GUIUtils.HUD_SCROLL_L);
            scroll.PositionAndMove(_gImage, new Point(10, 65));

            _liIcons.Add(travelers);
            _liIcons.Add(profits);

            if (_building.UpgradeQueued)
            {
                GUIText upgradeText = new GUIText("Upgrade in Progress");
                upgradeText.AnchorAndAlignWithSpacing(scroll, SideEnum.Bottom, SideEnum.CenterX, 16);
                _gImage.AddControl(upgradeText);
            }
            else
            {
                Upgrade[] buildingUpgrades = _building.GetAllUpgrades();
                bool isMaxLevel = _building.MaxLevel();
                bool hasUpgrades = buildingUpgrades.Length > 0;
                if (!isMaxLevel && hasUpgrades)
                {
                    GUIButton btn = new GUIButton(GUIUtils.BTN_BUY, Upgrade);
                    btn.Position(_gImage.Position());
                    btn.ScaledMoveBy(135, 93);
                    _gImage.AddControl(btn);

                    Upgrade nextUpgrade = buildingUpgrades[_building.Level - 1];
                    Dictionary<int, int> upgradeItems = nextUpgrade.UpgradeRequirements;

                    Color textColor = Color.White;
                    if (!InventoryManager.HasSufficientItems(upgradeItems) || PlayerManager.Money < nextUpgrade.Cost)
                    {
                        textColor = Color.Red;
                        btn.Enable(false);
                    }

                    string bonusValue = string.Empty;
                    Rectangle drawRect = new Rectangle();
                    GameIconEnum icon = GameIconEnum.None;
                    if (nextUpgrade.Profit > 0)
                    {
                        icon = GameIconEnum.Coin;
                        drawRect = GUIUtils.ICON_COIN;
                        bonusValue = nextUpgrade.Profit.ToString();
                    }
                    else if (nextUpgrade.Chance > 0)
                    {
                        icon = GameIconEnum.Traveler;
                        drawRect = GUIUtils.ICON_TRAVELER;
                        bonusValue = nextUpgrade.Chance.ToString();
                    }
                    else if (nextUpgrade.CraftingSlots > 0)
                    {
                        icon = GameIconEnum.Hammer;
                        drawRect = GUIUtils.ICON_HAMMER;
                        bonusValue = nextUpgrade.CraftingSlots.ToString();
                    }
                    else if (nextUpgrade.FormulaLevel > 0)
                    {
                        icon = GameIconEnum.Book;
                        drawRect = GUIUtils.ICON_BOOK;
                        bonusValue = nextUpgrade.FormulaLevel.ToString();
                    }

                    var bonusIcon = new GUIIconText(bonusValue, 1, drawRect, icon, SideEnum.Right, SideEnum.CenterY);
                    bonusIcon.CenterOnObject(_gImage);
                    bonusIcon.AnchorToObject(scroll, SideEnum.Bottom, 3);

                    _liIcons.Add(bonusIcon);

                    List<GUIItemBox> list = new List<GUIItemBox>();
                    foreach (KeyValuePair<int, int> kvp in upgradeItems)
                    {
                        GUIItemBox box = new GUIItemBox(DataManager.GetItem(kvp.Key, kvp.Value));
                        if (list.Count == 0)
                        {
                            box.Position(_gImage.Position());
                            box.ScaledMoveBy(9, 91);
                        }
                        else { box.AnchorAndAlignWithSpacing(list[list.Count - 1], SideEnum.Right, SideEnum.Bottom, 2); }
                        _gImage.AddControl(box);

                        if (!InventoryManager.HasItemInPlayerInventory(kvp.Key, kvp.Value)) { box.SetColor(Color.Red); }

                        list.Add(box);
                    }

                    GUIText cost = new GUIText(nextUpgrade.Cost);
                    cost.AnchorAndAlignWithSpacing(btn, SideEnum.Left, SideEnum.CenterY, 2);
                    _gImage.AddControl(cost);

                    _diUpgradeItems = upgradeItems;
                    _iCost = nextUpgrade.Cost;
                }
                else
                {
                    GUIText upgradeText = new GUIText("Max Level");
                    upgradeText.AnchorAndAlignWithSpacing(scroll, SideEnum.Bottom, SideEnum.CenterX, 16);
                    _gImage.AddControl(upgradeText);
                }
            }
        }

        private void Upgrade()
        {
            if (PlayerManager.ExpendResources(_diUpgradeItems) && PlayerManager.Money >= _iCost)
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
