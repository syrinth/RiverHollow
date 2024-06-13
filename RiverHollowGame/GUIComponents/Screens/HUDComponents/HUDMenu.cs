using Microsoft.Xna.Framework;
using RiverHollow.Game_Managers;
using RiverHollow.GUIComponents.GUIObjects;
using RiverHollow.GUIComponents.Screens.HUDWindows;
using RiverHollow.Utilities;
using System.Collections.Generic;

namespace RiverHollow.GUIComponents.Screens.HUDComponents
{
    public class HUDMenu : GUIObject
    {
        const int BTN_PADDING = 10;
        List<GUIObject> _liButtons;

        bool _bOpen = false;
        bool _bClose = false;

        public HUDMenu()
        {
            _liButtons = new List<GUIObject>();
            NewButtonMenu();
            
            _bOpen = true;
        }

        public void NewButtonMenu()
        {
            _liButtons.ForEach(x => RemoveControl(x));
            _liButtons.Clear();

            _liButtons.Add(new GUIButton("Inventory", BtnInventory));
            _liButtons.Add(new GUIButton("Build", BtnBuild));
            _liButtons.Add(new GUIButton("Edit Town", BtnEdit));
            _liButtons.Add(new GUIButton("Task Log", BtnTaskLog));
            if (PlayerManager.CodexUnlocked)
            {
                _liButtons.Add(new GUIButton("Codex", BtnCodex));
            }
            _liButtons.Add(new GUIButton("Options", BtnOptions));
            _liButtons.Add(new GUIButton("Exit Game", BtnExitGame));

            AddControls(_liButtons);
            GUIUtils.CreateSpacedColumn(_liButtons, -_liButtons[0].Width, 0, RiverHollow.ScreenHeight, BTN_PADDING);
        }

        public override void Update(GameTime gTime)
        {
            base.Update(gTime);
            if (_bOpen)
            {
                int _openingFinished = 0;
                foreach (GUIObject o in Controls)
                {
                    int mod = 4 * GameManager.CurrentScale;
                    int val = 0;
                    if (_bOpen)
                    {
                        if (o.Position().X < 0) { val = mod; }
                    }
                    if (_bClose)
                    {
                        if (o.Position().X > -o.Width) { val = -mod; }
                    }

                    Point temp = o.Position();
                    temp.X += val;
                    o.Position(temp);
                    if (_bOpen && o.Position().X == 0)
                    {
                        _openingFinished++;
                    }
                    if (_bClose && o.Position().X == -o.Width) { /*Finished closing */ }
                }
                if (_openingFinished == _liButtons.Count) { _bOpen = false; }
            }
        }

        #region Buttons
        public void BtnExitGame()
        {
            var _gMenuObject = new HUDExit();
            GUIManager.OpenMainObject(_gMenuObject);
        }
        public void BtnInventory()
        {
            var _gMenuObject = new HUDPlayerInventory();
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
            var _gMenuObject = new HUDTownCrafting();
            _gMenuObject.CenterOnScreen();
            GUIManager.OpenMainObject(_gMenuObject);
        }

        public void BtnEdit()
        {
            GameManager.EnterTownModeEdit();
        }
        #endregion
    }
}
