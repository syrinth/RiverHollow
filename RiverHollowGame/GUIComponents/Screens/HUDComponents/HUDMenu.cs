using Microsoft.Xna.Framework;
using RiverHollow.Game_Managers;
using RiverHollow.GUIComponents.GUIObjects;
using RiverHollow.GUIComponents.Screens.HUDScreens;
using RiverHollow.Items;
using System.Collections.Generic;
using static RiverHollow.Utilities.Enums;

namespace RiverHollow.GUIComponents.Screens.HUDComponents
{
    public class HUDMenu : GUIObject
    {
        const int BTN_PADDING = 10;
        GUIMainObject _gMenuObject;
        readonly List<GUIObject> _liButtons;

        bool _bOpen = false;
        bool _bClose = false;

        public delegate void CloseMenuDelegate();
        private CloseMenuDelegate _closeMenu;

        public HUDMenu(CloseMenuDelegate closeMenu)
        {
            _closeMenu = closeMenu;

            _liButtons = new List<GUIObject>() {
                new GUIButton("Inventory", BtnInventory)
            };

            GUIButton btnBuild = new GUIButton("Build", BtnBuild);
            btnBuild.Enable(!MapManager.CurrentMap.Modular);
            _liButtons.Add(btnBuild);

            _liButtons.Add(new GUIButton("Task Log", BtnTaskLog));
            _liButtons.Add(new GUIButton("Codex", BtnCodex));
            _liButtons.Add(new GUIButton("Options", BtnOptions));
            _liButtons.Add(new GUIButton("Exit Game", BtnExitGame));

            AddControls(_liButtons);

            GUIObject.CreateSpacedColumn(ref _liButtons, -GUIButton.BTN_WIDTH, 0, RiverHollow.ScreenHeight, BTN_PADDING);

            _bOpen = true;
        }

        public override void Update(GameTime gTime)
        {
            base.Update(gTime);
            int _openingFinished = 0;
            foreach (GUIObject o in Controls)
            {
                int val = 0;
                if (_bOpen)
                {
                    if (o.Position().X < 0) { val = 16; }
                }
                if (_bClose)
                {
                    if (o.Position().X > -GUIButton.BTN_WIDTH) { val = -16; }
                }

                Point temp = o.Position();
                temp.X += val;
                o.Position(temp);
                if (_bOpen && o.Position().X == 0) { _openingFinished++; }
                if (_bClose && o.Position().X == -GUIButton.BTN_WIDTH) { /*Finished closing */ }
            }
            if (_openingFinished == _liButtons.Count) { _bOpen = false; }
        }

        public override bool ProcessRightButtonClick(Point mouse)
        {
            //Returns false here because we don't handle it
            //By returning false, we will start closing options
            return false;
        }

        #region Buttons
        public void BtnExitGame()
        {
            GUIManager.CloseMainObject();
            GUIManager.OpenTextWindow(DataManager.GetGameTextEntry("QuitGame"));
        }
        public void BtnInventory()
        {
            Item[,] toolBox = new Item[1, 1];
            //toolBox[0, 0] = PlayerManager.RetrieveTool(ToolEnum.Axe);
            //toolBox[0, 1] = PlayerManager.RetrieveTool(ToolEnum.Pick);
            //toolBox[0, 2] = PlayerManager.RetrieveTool(ToolEnum.WateringCan);
            //toolBox[0, 3] = PlayerManager.RetrieveTool(ToolEnum.Scythe);
            //toolBox[0, 4] = PlayerManager.RetrieveTool(ToolEnum.Lantern);
            //toolBox[0, 5] = PlayerManager.RetrieveTool(ToolEnum.Harp);
            toolBox[0, 0] = PlayerManager.RetrieveTool(ToolEnum.Backpack);

            _gMenuObject = new HUDInventoryDisplay(toolBox, DisplayTypeEnum.Inventory, true);
            //_gMenuObject = new HUDInventoryDisplay();
            _gMenuObject.CenterOnScreen();
            GUIManager.OpenMainObject(_gMenuObject);
        }
        public void BtnTaskLog()
        {
            _gMenuObject = new HUDTaskLog();
            GUIManager.OpenMainObject(_gMenuObject);
        }
        public void BtnOptions()
        {
            _gMenuObject = new HUDOptions();
            GUIManager.OpenMainObject(_gMenuObject);
        }
        public void BtnBuild()
        {
            if (!MapManager.CurrentMap.Modular)
            {
                GUIManager.SetScreen(new BuildScreen());
            }
        }

        public void BtnCodex()
        {
            _gMenuObject = new HUDCodex();
            GUIManager.OpenMainObject(_gMenuObject);
        }
        #endregion
    }
}
