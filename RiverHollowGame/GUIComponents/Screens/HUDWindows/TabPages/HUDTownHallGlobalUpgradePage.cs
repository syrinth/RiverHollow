using RiverHollow.Game_Managers;
using RiverHollow.GUIComponents.GUIObjects.GUIWindows;
using RiverHollow.GUIComponents.GUIObjects;
using RiverHollow.GUIComponents.Screens.HUDComponents;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace RiverHollow.GUIComponents.Screens.HUDWindows.TabPages
{
    internal class HUDTownHallGlobalUpgradePage : GUIObject
    {
        const int MAX_DISPLAY = 35;
        const int COLUMNS = 7;

        int _iIndex = 0;

        readonly private List<DisplayGlobalUpgradeIcon> _liItemDisplay;
        readonly private GUIText _gLabel;

        HUDGlobalUpgrade _upgradeWindow;

        public HUDTownHallGlobalUpgradePage(GUIWindow mainWindow)
        {
            _liItemDisplay = new List<DisplayGlobalUpgradeIcon>();

            _gLabel = new GUIText("Building Info");
            _gLabel.AnchorToInnerSide(mainWindow, SideEnum.Top);

            var upgrades = TownManager.GetAllUpgrades();
            var keys = upgrades.Keys.ToList();
            for (int i = _iIndex; i < _iIndex + MAX_DISPLAY; i++)
            {
                if (upgrades.Count <= i)
                {
                    break;
                }

                var displayWindow = new DisplayGlobalUpgradeIcon(upgrades[keys[i]], BtnOpen);
                _liItemDisplay.Add(displayWindow);
            }

            _liItemDisplay = _liItemDisplay.OrderBy(x => x.Priority).ToList();
            GUIUtils.CreateSpacedGrid(new List<GUIObject>(_liItemDisplay), mainWindow, new Point(14, 19), COLUMNS, 3, 3);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
            _upgradeWindow?.Draw(spriteBatch);
        }

        public override bool ProcessLeftButtonClick(Point mouse)
        {
            bool rv;
            if (_upgradeWindow != null)
            {
                rv = true;
                _upgradeWindow.ProcessLeftButtonClick(mouse);
            }
            else
            {
                rv = base.ProcessLeftButtonClick(mouse);
            }

            return rv;
        }

        public override bool ProcessRightButtonClick(Point mouse)
        {
            bool rv;
            if (_upgradeWindow != null)
            {
                rv = true;
                _upgradeWindow = null;
            }
            else
            {
                rv = base.ProcessRightButtonClick(mouse);
            }

            return rv;
        }

        public override bool ProcessHover(Point mouse)
        {
            bool rv;
            if (_upgradeWindow != null)
            {
                rv = true;
                _upgradeWindow.ProcessHover(mouse);
            }
            else
            {
                rv = base.ProcessHover(mouse);
            }

            return rv;
        }

        private void BtnOpen(int upgradeID)
        {
            GUIManager.CloseHoverWindow();
            _upgradeWindow = new HUDGlobalUpgrade(TownManager.GetGlobalUpgrade(upgradeID));
        }
    }
}
