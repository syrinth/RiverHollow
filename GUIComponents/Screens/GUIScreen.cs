using RiverHollow.Game_Managers.GUIComponents.GUIObjects.GUIWindows;
using RiverHollow.GUIObjects;
using RiverHollow.WorldObjects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using RiverHollow.Characters.NPCs;

namespace RiverHollow.Game_Managers.GUIObjects
{
    //Represents a complete collection of associated GUIs to be displayed on the screen
    public abstract class GUIScreen
    {
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
            if (_gSelectionWindow != null) { rv = _gSelectionWindow.ProcessLeftButtonClick(mouse); }
            return rv;
        }
        public virtual bool ProcessRightButtonClick(Point mouse)
        {
            bool rv = false;
            if (_gSelectionWindow != null) { _gSelectionWindow.ProcessRightButtonClick(mouse); }
            else { GameManager.BackToMain(); }
            return rv;
        }
        public virtual bool ProcessHover(Point mouse)
        {
            bool rv = false;
            rv = _gSelectionWindow != null;
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
        }
        public virtual void Draw(SpriteBatch spriteBatch)
        {
            foreach(GUIObject g in Controls)
            {
                g.Draw(spriteBatch);
            }
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

        public void AddTextSelection(string text)
        {
            if (Controls.Contains(_gSelectionWindow)) { Controls.Remove(_gSelectionWindow); }
            _gSelectionWindow = new GUITextSelectionWindow(text);
            Controls.Add(_gSelectionWindow);
        }

        public virtual bool IsTextScreen() { return false; }
        public virtual bool IsGameMenuScreen() { return false; }
        public virtual bool IsItemCreationScreen() { return false; }
        public virtual bool IsHUD() { return false; }
    }
}
