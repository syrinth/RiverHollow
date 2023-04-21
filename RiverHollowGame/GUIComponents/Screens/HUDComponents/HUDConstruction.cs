using RiverHollow.Game_Managers;
using RiverHollow.GUIComponents.GUIObjects.GUIWindows;
using RiverHollow.GUIComponents.GUIObjects;
using RiverHollow.WorldObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static RiverHollow.Utilities.Enums;
using Microsoft.Xna.Framework;
using static RiverHollow.GUIComponents.Screens.HUDComponents.HUDMenu;

namespace RiverHollow.GUIComponents.Screens.HUDComponents
{
    public class HUDConstruction : GUIMainObject
    {
        public static int MAX_SHOWN_TASKS = 4;
        public static int TASK_SPACING = 20;
        List<GUIObject> _liStructures;
        GUIList _gList;

        private CloseMenuDelegate _closeMenu;

        public HUDConstruction(CloseMenuDelegate closeMenu)
        {
            _closeMenu = closeMenu;
            _winMain = SetMainWindow();

            _liStructures = new List<GUIObject>();

            GenerateConstructBoxes();

            _gList = new GUIList(_liStructures, MAX_SHOWN_TASKS, TASK_SPACING/*, _gWindow.Height*/);
            _gList.CenterOnObject(_winMain);

            AddControl(_gList);
        }

        /// <summary>
        /// Given a list of WorldObject ids, generate a ConstructBox
        /// </summary>
        /// <param name="idList">The list of item IDs to create a box for</param>
        public void GenerateConstructBoxes()
        {
            var idList = PlayerManager.GetCraftingList().FindAll(x => ((Buildable)DataManager.CreateWorldObjectByID(x)).Unique);
            foreach (int i in idList)
            {
                Buildable obj = (Buildable)DataManager.CreateWorldObjectByID(i);
                if (obj.RequiredToMake.Count > 0 && obj.CanBuild())
                {
                    ConstructBox box = new ConstructBox(ConstructWorldObject);
                    box.SetConstructionInfo(i, obj.Name(), obj.RequiredToMake);
                    _liStructures.Add(box);
                }
            }
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

        public void ConstructWorldObject(int objID)
        {
            Buildable obj;

            obj = (Buildable)DataManager.CreateWorldObjectByID(objID);
            var requiredToMake = obj.RequiredToMake;

            if (InventoryManager.HasSufficientItems(requiredToMake))
            {
                GameManager.EnterTownModeBuild(true);
                GameManager.PickUpWorldObject(obj);
                MapManager.CurrentMap.AddHeldLights(obj.GetLights());
                obj.SetPickupOffset();
            }

            GUIManager.CloseMainObject();
            _closeMenu();
        }
    }
}
