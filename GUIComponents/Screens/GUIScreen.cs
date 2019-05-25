using RiverHollow.Game_Managers.GUIComponents.GUIObjects.GUIWindows;
using RiverHollow.GUIObjects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using RiverHollow.Game_Managers.GUIComponents.GUIObjects;

namespace RiverHollow.Game_Managers.GUIObjects
{
    //Represents a complete collection of associated GUIs to be displayed on the screen
    public abstract class GUIScreen
    {
        protected const int MINI_BTN_HEIGHT = 32;
        protected const int MINI_BTN_WIDTH = 128;

        private GUITextWindow _guiTextWindow;
        private GUITextWindow _guiHoverWindow;
        private GUIObject _guiHoverObject;
        protected GUITextSelectionWindow _gSelectionWindow;
        private List<GUIObject> _toRemove;
        protected List<GUIObject> Controls;
        public bool IsVisible;

        public GUIScreen()
        {
            _toRemove = new List<GUIObject>();
            Controls = new List<GUIObject>();
        }
        public virtual bool ProcessLeftButtonClick(Point mouse)
        {
            bool rv = false;
            if (_gSelectionWindow != null) {
                rv = _gSelectionWindow.ProcessLeftButtonClick(mouse);
            }
            else if (_guiTextWindow != null) { rv = _guiTextWindow.ProcessLeftButtonClick(mouse); }
            return rv;
        }
        public virtual bool ProcessRightButtonClick(Point mouse)
        {
            bool rv = false;
            if (_gSelectionWindow != null) { rv = _gSelectionWindow.ProcessRightButtonClick(mouse); }
            else if ( _guiTextWindow != null) { rv = _guiTextWindow.ProcessRightButtonClick(mouse); }
            else { GameManager.BackToMain(); }
            return rv;
        }
        public virtual bool ProcessHover(Point mouse)
        {
            bool rv = false;
            rv = _guiHoverWindow != null && _gSelectionWindow != null;
            return rv;
        }
        public virtual void Update(GameTime gameTime)
        {
            foreach (GUIObject g in Controls)
            {
                g.Update(gameTime);
            }
            foreach (GUIObject g in _toRemove)
            {
                Controls.Remove(g);
            }
            _toRemove.Clear();

            if (_guiTextWindow != null) { _guiTextWindow.Update(gameTime); }

            if (_guiHoverObject != null && !_guiHoverObject.Contains(GraphicCursor.Position.ToPoint())) {
                CloseHoverWindow();
            }
        }
        public virtual void Draw(SpriteBatch spriteBatch)
        {
            foreach (GUIObject g in Controls)
            {
                g.Draw(spriteBatch);
            }

            if (_guiTextWindow != null) { _guiTextWindow.Draw(spriteBatch); }
            if (_guiHoverWindow != null) { _guiHoverWindow.Draw(spriteBatch); }
        }
        public virtual bool Contains(Point mouse)
        {
            bool rv = false;

            foreach(GUIObject g in Controls)
            {
                if (g.Contains(mouse))
                {
                    rv = true;
                    break;
                }
            }
            return rv;
        }

        public virtual void Sync() { }

        public virtual void CloseHoverWindow() {
            _guiHoverWindow = null;
            _guiHoverObject = null;
        }
        public bool IsHoverWindowOpen() { return _guiHoverWindow != null; }
        public virtual void OpenHoverWindow(GUITextWindow hoverWindow, GUIObject hoverObject)
        {
            _guiHoverWindow = hoverWindow;
            _guiHoverObject = hoverObject;
        }

        public virtual bool CloseTextWindow(GUITextWindow win) {
            bool rv = false;
            if (win == _guiTextWindow)
            {
                GameManager.CurrentNPC = null;
                GameManager.gmActiveItem = null;
                _guiTextWindow = null;
                rv = true;
            }

            return rv;
        }
        public bool IsTextWindowOpen() { return _guiTextWindow != null; }
        public virtual void OpenTextWindow(string text, bool open = true)
        {
            bool selection = text.Contains("[");
            if (selection) { _guiTextWindow = new GUITextSelectionWindow(text, open); }
            else { _guiTextWindow = new GUITextWindow(text, open); }
        }
        public void SetWindowText(string value)
        {
            if(_guiTextWindow != null)
            {
                if (_guiTextWindow.IsSelectionBox())
                {
                    OpenTextWindow(value, false);
                }
                else
                {
                    _guiTextWindow.ResetText(value);
                }
            }
        }

        public void AddTextSelection(string text)
        {
            if (Controls.Contains(_gSelectionWindow)) { Controls.Remove(_gSelectionWindow); }
            _gSelectionWindow = new GUITextSelectionWindow(text);
        }

        public void AddControl(GUIObject control)
        {
            if (!Controls.Contains(control))
            {
                Controls.Add(control);
                control.ParentScreen = this;
            }
        }
        public void RemoveControl(GUIObject control)
        {
            if (Controls.Contains(control))
            {
                Controls.Remove(control);
            }
        }

        public virtual bool IsGameMenuScreen() { return false; }
        public virtual bool IsItemCreationScreen() { return false; }
        public virtual bool IsHUD() { return false; }
    }
}
