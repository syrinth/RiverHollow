using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.Game_Managers;
using RiverHollow.GUIComponents.GUIObjects;
using RiverHollow.GUIComponents.GUIObjects.GUIWindows;
using RiverHollow.GUIComponents.Screens.HUDComponents;
using System.Collections.Generic;
using System.Linq;

namespace RiverHollow.GUIComponents.Screens.HUDWindows.TabPages
{
    internal class HUDTownHallBuildingUpgradePage : GUIObject
    {
        const int MAX_DISPLAY = 35;
        const int COLUMNS = 7;

        int _iIndex = 0;

        readonly private List<DisplayBuildingUpgradeIcon> _liItemDisplay;
        readonly private GUIText _gLabel;

        HUDBuildingUpgrade _upgradeWindow;

        public HUDTownHallBuildingUpgradePage(GUIWindow mainWindow)
        {
            _liItemDisplay = new List<DisplayBuildingUpgradeIcon>();

            _gLabel = new GUIText("Building Info");
            _gLabel.AnchorToInnerSide(mainWindow, SideEnum.Top);

            var upgradableIDs = DataManager.GetWorldObjectsWithKey("Upgradable");
            for (int i = _iIndex; i < _iIndex + MAX_DISPLAY; i++)
            {
                if (upgradableIDs.Count <= i)
                {
                    break;
                }

                var displayWindow = new DisplayBuildingUpgradeIcon(upgradableIDs[i], BtnOpen);
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

        private void BtnOpen(int buildingID)
        {
            GUIManager.CloseHoverWindow();
            var b = TownManager.GetBuildingByID(buildingID);
            if (b != null)
            {
                _upgradeWindow = new HUDBuildingUpgrade(b);
            }
        }
    }
}
