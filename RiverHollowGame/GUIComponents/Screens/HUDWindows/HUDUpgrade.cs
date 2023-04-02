using Microsoft.Xna.Framework;
using RiverHollow.Buildings;
using RiverHollow.Game_Managers;
using RiverHollow.GUIComponents.GUIObjects;
using RiverHollow.GUIComponents.GUIObjects.GUIWindows;
using RiverHollow.Misc;
using System.Collections.Generic;

namespace RiverHollow.GUIComponents.Screens.HUDWindows
{
    public class HUDBuildingUpgrade : GUIMainObject
    {
        GUIImage _gImage;
        Building _building;

        public HUDBuildingUpgrade(Building b)
        {
            _building = b;

            _gImage = new GUIImage(new Rectangle(0, 0, 162, 119), DataManager.HUD_COMPONENTS);
            AddControl(_gImage);

            DisplayDetails();

            Width = _gImage.Width;
            Height = _gImage.Height;

            CenterOnScreen();
        }

        private void DisplayDetails()
        {
            _gImage.CleanControls();

            GUIText name = new GUIText(_building.Name());
            name.AnchorToObjectInnerSide(_gImage, SideEnum.Top, GameManager.ScaledPixel * 3);
            _gImage.AddControl(name);

            GUIButton btn = new GUIButton(new Rectangle(164, 0, 18, 19), DataManager.HUD_COMPONENTS, Upgrade);
            btn.Position(_gImage.Position());
            btn.ScaledMoveBy(135, 93);
            _gImage.AddControl(btn);

            GUIText lvl = new GUIText("Level " + _building.Level);
            lvl.AnchorToObjectInnerSide(_gImage, SideEnum.Top, GameManager.ScaledPixel * 18);
            _gImage.AddControl(lvl);

            Upgrade[] buildingUpgrades = _building.GetAllUpgrades();
            if (!_building.MaxLevel() && buildingUpgrades.Length > 0)
            {
                Upgrade nextUpgrade = buildingUpgrades[_building.Level - 1];
                Dictionary<int, int> upgradeItems = nextUpgrade.UpgradeRequirements;

                Color textColor = Color.White;
                if (!InventoryManager.HasSufficientItems(upgradeItems))
                {
                    textColor = Color.Red;
                    btn.Enable(false);
                }

                List<GUIItemBox> list = new List<GUIItemBox>();
                foreach (KeyValuePair<int, int> kvp in upgradeItems)
                {
                    GUIItemBox box = new GUIItemBox(DataManager.GetItem(kvp.Key, kvp.Value));
                    if (list.Count == 0)
                    {
                        box.Position(_gImage.Position());
                        box.ScaledMoveBy(9,91);
                    }
                    else { box.AnchorAndAlignToObject(list[list.Count - 1], SideEnum.Right, SideEnum.Bottom); }
                    _gImage.AddControl(box);

                    if (!InventoryManager.HasItemInPlayerInventory(kvp.Key, kvp.Value)) { box.SetColor(Color.Red); }

                    list.Add(box);
                }

                GUIMoneyDisplay cost = new GUIMoneyDisplay(nextUpgrade.Cost, false);
                cost.AnchorAndAlignToObject(list[list.Count - 1], SideEnum.Right, SideEnum.CenterY, GameManager.ScaledPixel); 
                _gImage.AddControl(cost);
            }
            else
            {
                btn.Show(false);
            }
        }

        private void Upgrade()
        {
            if (PlayerManager.ExpendResources(_building.UpgradeReqs()))
            {
                _building.Upgrade();
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
