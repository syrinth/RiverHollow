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
        GUIImage _guiBackgroundImg;
        protected GUITextWindow _guiTextWindow;
        protected GUITextWindow _guiHoverWindow;
        GUIObject _guiHoverObject;
        protected GUITextSelectionWindow _gSelectionWindow;
        List<GUIObject> _liToRemove;
        List<GUIObject> _liToAdd;
        protected List<GUIObject> Controls;
        public bool IsVisible;

        public GUIScreen()
        {
            _liToRemove = new List<GUIObject>();
            _liToAdd = new List<GUIObject>();
            Controls = new List<GUIObject>();
        }
        public virtual bool ProcessLeftButtonClick(Point mouse)
        {
            bool rv = false;
            foreach (GUIObject g in Controls)
            {
                rv = g.ProcessLeftButtonClick(mouse);

                if (rv) { break; }
            }
            return rv;
        }
        public virtual bool ProcessRightButtonClick(Point mouse)
        {
            bool rv = false;

            foreach (GUIObject g in Controls)
            {
                rv = g.ProcessRightButtonClick(mouse);

                if (rv) { break; }
            }

            return rv;
        }
        public virtual bool ProcessHover(Point mouse)
        {
            bool rv = false;
            foreach (GUIObject g in Controls)
            {
                rv = g.ProcessHover(mouse);

                if (rv) { break; }
            }
            return rv;
        }
        public virtual void Update(GameTime gTime)
        {
            foreach (GUIObject g in _liToAdd)
            {
                if (!Controls.Contains(g))
                {
                    Controls.Add(g);
                    g.ParentScreen = this;
                }
            }
            _liToAdd.Clear();
            foreach (GUIObject g in Controls)
            {
                g.Update(gTime);
            }

            foreach (GUIObject g in _liToRemove)
            {
                if (Controls.Contains(g))
                {
                    Controls.Remove(g);
                }
            }
            _liToRemove.Clear();

            if (_guiTextWindow != null) { _guiTextWindow.Update(gTime); }

            if (_guiHoverObject != null && !_guiHoverObject.Contains(GraphicCursor.Position.ToPoint())) {
                CloseHoverWindow();
            }
        }
        public virtual void Draw(SpriteBatch spriteBatch)
        {
            if (_guiBackgroundImg != null)
            {
                _guiBackgroundImg.Draw(spriteBatch);
            }
            foreach (GUIObject g in Controls)
            {
                g.Draw(spriteBatch);
            }

            if (_guiTextWindow != null) { _guiTextWindow.Draw(spriteBatch); }
            if (_guiHoverWindow != null) { _guiHoverWindow.Draw(spriteBatch); }
        }

        protected virtual void HandleInput() { }

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

        //Hover Window Code
        public virtual void CloseHoverWindow() {
            RemoveControl(_guiHoverWindow);
            _guiHoverWindow = null;
            _guiHoverObject = null;
        }
        public bool IsHoverWindowOpen() { return _guiHoverWindow != null; }
        public virtual void OpenHoverWindow(GUITextWindow hoverWindow, GUIObject hoverObject)
        {
            CloseHoverWindow();
            _guiHoverWindow = hoverWindow;
            _guiHoverObject = hoverObject;
            AddControl(_guiHoverWindow);
        }

        //Hover Text Window
        public virtual bool CloseTextWindow(GUITextWindow win) {
            bool rv = false;
            if (win == _guiTextWindow)
            {
                RemoveControl(_guiTextWindow);

                GameManager.RemoveCurrentNPCLockObject();
                GameManager.gmActiveItem = null;
                _guiTextWindow = null;
                rv = true;
            }

            return rv;
        }
        public bool IsTextWindowOpen() { return _guiTextWindow != null; }

        /// <summary>
        /// Removes any previous existing Text Windows fromthe Control, then determines whether
        /// or not the given text requires a selection window or not, and creates the appropriate
        /// GUITextWindow.
        /// 
        /// Afterward, adds the new Window to the Controls.
        /// </summary>
        /// <param name="text">Text for the window</param>
        /// <param name="open">Whether or not to display an open animation</param>
        public virtual void OpenTextWindow(string text, bool open = true)
        {
            RemoveControl(_guiTextWindow);
            bool selection = text.Contains("[");
            if (selection) { _guiTextWindow = new GUITextSelectionWindow(text, open); }
            else { _guiTextWindow = new GUITextWindow(text, open); }
            AddControl(_guiTextWindow);
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

        /// <summary>
        /// Assigns a background image to be drawn behind all other GUI components.
        /// Do not add it to the controls because we don't want it responding to anything
        /// </summary>
        /// <param name="newImage">The Image touse</param>
        public virtual void AssignBackgroundImage(GUIImage newImage)
        {
            newImage.CenterOnScreen();
            _guiBackgroundImg = newImage;
        }

        /// <summary>
        /// Because it's not assigned to the Controls we need only make the value null to close it
        /// </summary>
        public virtual void ClearBackgroundImage()
        {
            _guiBackgroundImg = null;
        }

        #region Main Object Control
        public virtual void OpenMainObject(GUIObject o) { }
        public virtual void CloseMainObject() { }
        #endregion

        public void AddTextSelection(string text)
        {
            RemoveControl(_gSelectionWindow);
            _gSelectionWindow = new GUITextSelectionWindow(text);
            AddControl(_gSelectionWindow);
        }

        public void AddControl(GUIObject control)
        {
            _liToAdd.Add(control);
        }
        public void RemoveControl(GUIObject control)
        {
            _liToRemove.Add(control);
        }
    }
}
