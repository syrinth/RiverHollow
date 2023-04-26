using Microsoft.Xna.Framework;
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

        List<KeyValuePair<GUIImage, Enums.GameIconEnum>> _liIcons;

        Dictionary<int, int> _diUpgradeItems;
        int _iCost;

        public HUDBuildingUpgrade(Building b)
        {
            _building = b;
            _liIcons = new List<KeyValuePair<GUIImage, GameIconEnum>>();

            _gImage = new GUIImage(GUIUtils.WIN_UPGRADE);
            AddControl(_gImage);

            DisplayDetails();

            Width = _gImage.Width;
            Height = _gImage.Height;

            CenterOnScreen();
        }

        public override bool ProcessHover(Point mouse)
        {
            bool rv = false;

            for (int i = 0; i < _liIcons.Count; i++)
            {
                if (_liIcons[i].Key.Contains(mouse) && !GUIManager.IsHoverWindowOpen())
                {
                    string iconDescription = string.Empty;
                    switch (_liIcons[i].Value)
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
                    }

                    var win = new GUITextWindow(DataManager.GetGameTextEntry(iconDescription), Point.Zero);
                    win.AnchorAndAlign(_liIcons[i].Key, SideEnum.Bottom, SideEnum.CenterX);
                    GUIManager.OpenHoverWindow(win, _liIcons[i].Key.DrawRectangle, true);
                }
            }

            return rv;
        }

        private void DisplayDetails()
        {
            _gImage.CleanControls();

            GUIText name = new GUIText(_building.Name());
            name.AnchorToObjectInnerSide(_gImage, SideEnum.Top, GameManager.ScaledPixel * 3);
            _gImage.AddControl(name);

            GUIText lvl = new GUIText("Level " + _building.Level);
            lvl.AnchorToObjectInnerSide(_gImage, SideEnum.Top, GameManager.ScaledPixel * 18);
            _gImage.AddControl(lvl);

            //Traveler Display
            var travelers = new GUIImage(GUIUtils.ICON_TRAVELER);
            travelers.Position(_gImage.Position());
            travelers.ScaledMoveBy(37, 36);
            _gImage.AddControl(travelers);

            var travelerPercent = new GUIText(_building.GetTravelerChance());
            travelerPercent.AnchorAndAlignWithSpacing(travelers, SideEnum.Bottom, SideEnum.CenterX, 3);
            _gImage.AddControl(travelerPercent);

            //Profit Display
            var profits = new GUIImage(GUIUtils.ICON_COIN);
            profits.Position(_gImage.Position());
            profits.ScaledMoveBy(111, 36);
            _gImage.AddControl(profits);

            var profitPercent = new GUIText((int)(_building.GetShopProfitModifier() * 100));
            profitPercent.AnchorAndAlignWithSpacing(profits, SideEnum.Bottom, SideEnum.CenterX, 3);
            _gImage.AddControl(profitPercent);

            var scroll = new GUIImage(GUIUtils.HUD_SCROLL_L);
            scroll.Position(_gImage.Position());
            scroll.ScaledMoveBy(10, 65);
            _gImage.AddControl(scroll);

            _liIcons.Add(new KeyValuePair<GUIImage, GameIconEnum>(travelers, GameIconEnum.Traveler));
            _liIcons.Add(new KeyValuePair<GUIImage, GameIconEnum>(profits, GameIconEnum.Coin));

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

                    var bonusIcon = new GUIImage(drawRect);
                    bonusIcon.Position(_gImage.Position());
                    bonusIcon.ScaledMoveBy(63, 71);
                    _gImage.AddControl(bonusIcon);

                    var bonusText = new GUIText(bonusValue);
                    bonusText.AnchorAndAlignWithSpacing(bonusIcon, SideEnum.Right, SideEnum.CenterY, 1);
                    _gImage.AddControl(bonusText);

                    _liIcons.Add(new KeyValuePair<GUIImage, GameIconEnum>(bonusIcon, icon));

                    List<GUIItemBox> list = new List<GUIItemBox>();
                    foreach (KeyValuePair<int, int> kvp in upgradeItems)
                    {
                        GUIItemBox box = new GUIItemBox(DataManager.GetItem(kvp.Key, kvp.Value));
                        if (list.Count == 0)
                        {
                            box.Position(_gImage.Position());
                            box.ScaledMoveBy(9, 91);
                        }
                        else { box.AnchorAndAlign(list[list.Count - 1], SideEnum.Right, SideEnum.Bottom); }
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
