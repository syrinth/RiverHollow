using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.Characters;
using RiverHollow.Game_Managers;
using RiverHollow.GUIComponents.GUIObjects;
using RiverHollow.GUIComponents.GUIObjects.GUIWindows;
using RiverHollow.Misc;

namespace RiverHollow.GUIComponents.Screens
{
    //Represents a complete collection of associated GUIs to be displayed on the screen
    public abstract class GUIScreen
    {
        protected GUIMainObject _gMainObject;
        GUIImage _guiBackgroundImg;
        protected GUITextWindow _guiTextWindow;
        protected GUITextWindow _guiHoverWindow;
        Rectangle _rHoverArea;
        bool _bGUIObject;
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

            GUIManager.SetNewScreen(this);
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

            _guiTextWindow?.Update(gTime);

            if (_rHoverArea != Rectangle.Empty && !_rHoverArea.Contains(_bGUIObject ? GUICursor.Position : GUICursor.GetWorldMousePosition())) {
                CloseHoverWindow();
            }
        }
        public virtual void Draw(SpriteBatch spriteBatch)
        {
            _guiBackgroundImg?.Draw(spriteBatch);
            foreach (GUIObject g in Controls)
            {
                g.Draw(spriteBatch);
            }

            _guiTextWindow?.Draw(spriteBatch);
            _guiHoverWindow?.Draw(spriteBatch);
        }

        public virtual bool IsMenuOpen() { return false; }
        public virtual void OpenMenu() { }
        public virtual void CloseMenu() { }

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

        #region Hover Window Code
        public virtual void CloseHoverWindow() {
            RemoveControl(_guiHoverWindow);
            _guiHoverWindow?.RemoveSelfFromControl();
            _guiHoverWindow = null;
            _rHoverArea = Rectangle.Empty;
        }
        public bool IsHoverWindowOpen() { return _guiHoverWindow != null; }
        public virtual void OpenHoverWindow(GUITextWindow hoverWindow, Rectangle area, bool guiObject)
        {
            hoverWindow.ProcessClicks = false;

            CloseHoverWindow();
            _guiHoverWindow = hoverWindow;
            _rHoverArea = area;
            _bGUIObject = guiObject;
            AddControl(_guiHoverWindow);
        }
        #endregion

        #region Text Window Open/Close
        public virtual void OpenTextWindow(TextEntry text, bool open = true, bool displayDialogueIcon = false)
        {
            OpenTextWindow(text, null, open, displayDialogueIcon);
        }
        /// <summary>
        /// Removes any previous existing Text Windows fromthe Control, then determines whether
        /// or not the given text requires a selection window or not, and creates the appropriate
        /// GUITextWindow.
        /// 
        /// Afterward, adds the new Window to the Controls.
        /// </summary>
        /// <param name="text">Text for the window</param>
        /// <param name="open">Whether or not to display an open animation</param>
        public virtual void OpenTextWindow(TextEntry text, TalkingActor talker = null, bool open = true, bool displayDialogueIcon = false)
        {
            GUICursor.ResetCursor();
            CloseTextWindow();

            if (talker != null)
            {
                GameManager.SetCurrentNPC(talker);
            }

            if (text.Selection) { _guiTextWindow = new GUITextSelectionWindow(text, open); }
            else { _guiTextWindow = new GUITextWindow(text, open, displayDialogueIcon); }
        }

        public virtual bool CloseTextWindow()
        {
            bool rv = false;
            if (_guiTextWindow != null)
            {
                GUITextWindow temp = _guiTextWindow;
                RemoveControl(_guiTextWindow);
                _guiTextWindow = null;

                temp.ClosingWindow();
                rv = true;
            }

            return rv;
        }
        public bool IsTextWindowOpen() { return _guiTextWindow != null; }
        #endregion

        public void SetWindowText(TextEntry value, bool displayDialogueIcon)
        {
            if(_guiTextWindow != null)
            {
                if (_guiTextWindow.IsSelectionBox())
                {
                    OpenTextWindow(value, GameManager.CurrentNPC, false, displayDialogueIcon);
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
        public bool IsMainObjectOpen() { return _gMainObject != null; }
        public virtual void OpenMainObject(GUIMainObject o)
        {
            CloseMainObject();
            _gMainObject = o;
            AddControl(_gMainObject);
        }
        public virtual bool CloseMainObject()
        {
            bool rv = false;
            if (_gMainObject != null)
            {
                rv = true;
                _gMainObject.CloseMainWindow();
                RemoveControl(_gMainObject);
                _gMainObject = null;
                CloseHoverWindow();
            }

            return rv;
        }
        #endregion

        public virtual void NewAlertIcon(string text) { }
        public virtual void AddSkipCutsceneButton() { }
        public virtual void RemoveSkipCutsceneButton() { }

        public void AddControl(GUIObject control)
        {
            if (control != null)
            {
                _liToAdd.Add(control);
            }
        }
        public void RemoveControls(List<GUIObject> controls)
        {
            foreach (GUIObject obj in controls)
            {
                RemoveControl(obj);
            }
        }
        public void RemoveControl(GUIObject control)
        {
            if (control != null)
            {
                if (_liToAdd.Contains(control))
                {
                    _liToAdd.Remove(control);
                }
                else
                {
                    _liToRemove.Add(control);
                }
            }
        }
    }
}
