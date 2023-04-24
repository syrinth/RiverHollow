using Microsoft.Xna.Framework;
using RiverHollow.Game_Managers;
using RiverHollow.GUIComponents.GUIObjects;
using RiverHollow.GUIComponents.Screens.HUDWindows;
using RiverHollow.Items;
using System.Collections.Generic;
using static RiverHollow.Utilities.Enums;

namespace RiverHollow.GUIComponents.Screens.HUDComponents
{
    public class HUDMenu : GUIObject
    {
        HUDMenuEnum _eCurrentState = HUDMenuEnum.Main;

        const int BTN_PADDING = 10;
        List<GUIObject> _liButtons;

        bool _bOpen = false;
        bool _bClose = false;

        public delegate void CloseMenuDelegate();
        private CloseMenuDelegate _closeMenu;

        public HUDMenu(CloseMenuDelegate closeMenu)
        {
            _closeMenu = closeMenu;

            _liButtons = new List<GUIObject>();
            NewButtonMenu(HUDMenuEnum.Main);
            
            _bOpen = true;
        }

        public void NewButtonMenu(HUDMenuEnum menuType)
        {
            _eCurrentState = menuType;

            _liButtons.ForEach(x => RemoveControl(x));
            _liButtons.Clear();

            switch (menuType)
            {
                case HUDMenuEnum.Build:
                    var btn = new GUIButton("Structures", BtnStructure);
                    btn.Enable(MapManager.CurrentMap.IsOutside && MapManager.CurrentMap.IsTown);

                    _liButtons.Add(btn);
                    _liButtons.Add(new GUIButton("Crafting", BtnCrafting));
                    _liButtons.Add(new GUIButton("Edit Town", BtnEdit));
                    _liButtons.Add(new GUIButton("Back", BtnBackToMain));
                    break;
                case HUDMenuEnum.Main:
                    _liButtons.Add(new GUIButton("Inventory", BtnInventory));

                    GUIButton btnBuild = new GUIButton("Build", BtnBuild);
                    btnBuild.Enable(!MapManager.CurrentMap.Modular);
                    _liButtons.Add(btnBuild);

                    _liButtons.Add(new GUIButton("Task Log", BtnTaskLog));
                    _liButtons.Add(new GUIButton("Codex", BtnCodex));
                    _liButtons.Add(new GUIButton("Options", BtnOptions));
                    _liButtons.Add(new GUIButton("Exit Game", BtnExitGame));
                    break;
            }

            AddControls(_liButtons);
            GUIUtils.CreateSpacedColumn(_liButtons, -GUIButton.BTN_WIDTH, 0, RiverHollow.ScreenHeight, BTN_PADDING);
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
            bool rv = false;

            if (_eCurrentState == HUDMenuEnum.Build)
            {
                rv = true;
                if (GUIManager.IsMainObjectOpen())
                {
                    GUIManager.CloseMainObject();
                }
                else
                {
                    NewButtonMenu(HUDMenuEnum.Main);
                }
                
            }

            return rv;
        }

        #region Buttons
        public void BtnExitGame()
        {
            GUIManager.CloseMainObject();
            GUIManager.OpenTextWindow(DataManager.GetGameTextEntry("QuitGame"));
        }
        public void BtnInventory()
        {
            Item[,] toolBox = new Item[1, 2];
            //toolBox[0, 0] = PlayerManager.RetrieveTool(ToolEnum.Axe);
            //toolBox[0, 1] = PlayerManager.RetrieveTool(ToolEnum.Pick);
            //toolBox[0, 2] = PlayerManager.RetrieveTool(ToolEnum.WateringCan);
            //toolBox[0, 3] = PlayerManager.RetrieveTool(ToolEnum.Scythe);
            
            //toolBox[0, 5] = PlayerManager.RetrieveTool(ToolEnum.Harp);
            toolBox[0, 0] = PlayerManager.RetrieveTool(ToolEnum.Backpack);
            toolBox[0, 1] = PlayerManager.RetrieveTool(ToolEnum.Lantern);

            var _gMenuObject = new HUDInventoryDisplay(toolBox, DisplayTypeEnum.Inventory, true);
            //_gMenuObject = new HUDInventoryDisplay();
            _gMenuObject.CenterOnScreen();
            GUIManager.OpenMainObject(_gMenuObject);
        }
        public void BtnTaskLog()
        {
            var _gMenuObject = new HUDTaskLog();
            GUIManager.OpenMainObject(_gMenuObject);
        }
        public void BtnOptions()
        {
            var _gMenuObject = new HUDOptions();
            GUIManager.OpenMainObject(_gMenuObject);
        }
        
        public void BtnCodex()
        {
            var _gMenuObject = new HUDCodex();
            GUIManager.OpenMainObject(_gMenuObject);
        }

        public void BtnBuild()
        {
            if (!MapManager.CurrentMap.Modular)
            {
                NewButtonMenu(HUDMenuEnum.Build);
            }
        }

        public void BtnStructure()
        {
            var _gMenuObject = new HUDCraftStructures(_closeMenu);
            _gMenuObject.CenterOnScreen();
            GUIManager.OpenMainObject(_gMenuObject);
        }

        public void BtnCrafting()
        {
            var _gMenuObject = new HUDCraftRecipes();
            _gMenuObject.CenterOnScreen();
            GUIManager.OpenMainObject(_gMenuObject);
        }

        public void BtnBackToMain()
        {
            NewButtonMenu(HUDMenuEnum.Main);
            GUIManager.CloseMainObject();
        }

        public void BtnEdit()
        {
            _closeMenu();
            GUIManager.CloseMainObject();
            GameManager.ClearGMObjects();
            GameManager.EnterTownModeEdit();
        }
        #endregion
    }
}
