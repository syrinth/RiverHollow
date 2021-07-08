using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.Game_Managers;
using RiverHollow.GUIComponents.GUIObjects;
using RiverHollow.GUIComponents.GUIObjects.GUIWindows;
using RiverHollow.Items;
using System.Collections.Generic;
using static RiverHollow.Items.Buildable.AdjustableObject;

namespace RiverHollow.GUIComponents.MainObjects
{
    public class GardenWindow : GUIMainObject
    {
        public static int MAX_SHOWN_TASKS = 4;
        public static int TASK_SPACING = 20;

        List<GUIObject> _liStructures;
        GUIList _gList;
        Garden _objGarden;

        public GardenWindow(Garden targetGarden)
        {
            _winMain = SetMainWindow();
            _objGarden = targetGarden;

            _liStructures = new List<GUIObject>();

            foreach (int i in DataManager.PlantIDs)
            {
                ConstructBox box = new ConstructBox(ChoosePlant);
                Plant obj = (Plant)DataManager.GetWorldObjectByID(i);

                if (obj.InSeason())
                {
                    Dictionary<int, int> toMake = new Dictionary<int, int> { [obj.SeedID] = 1 };
                    box.SetConstructionInfo(i, obj.Name, toMake);
                    _liStructures.Add(box);
                }
            }

            _gList = new GUIList(_liStructures, MAX_SHOWN_TASKS, TASK_SPACING);
            _gList.CenterOnObject(_winMain);

            AddControl(_gList);
        }

        public override bool ProcessLeftButtonClick(Point mouse)
        {
            bool rv = false;

            foreach (GUIObject c in Controls)
            {
                rv = c.ProcessLeftButtonClick(mouse);

                if (rv) { break; }
            }

            return rv;
        }

        public override bool ProcessRightButtonClick(Point mouse) { return false; }

        public override bool ProcessHover(Point mouse) { return false; }

        public void ChoosePlant(int objID)
        {
            Plant obj = (Plant)DataManager.GetWorldObjectByID(objID);

            if (InventoryManager.HasItemInPlayerInventory(obj.SeedID, 1))
            {
                InventoryManager.RemoveItemsFromInventory(obj.SeedID, 1);
                _objGarden.SetPlant(obj);
            }

            GUIManager.CloseMainObject();
        }
    }
}
