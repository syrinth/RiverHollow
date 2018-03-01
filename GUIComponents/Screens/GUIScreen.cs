using RiverHollow.Game_Managers.GUIComponents.GUIObjects.GUIWindows;
using RiverHollow.GUIObjects;
using RiverHollow.Items;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace RiverHollow.Game_Managers.GUIObjects
{
    //Represents a complete collection of associated GUIs to be displayed on the screen
    public abstract class GUIScreen
    {
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
            return false;
        }
        public virtual bool ProcessRightButtonClick(Point mouse)
        {
            return false;
        }
        public virtual bool ProcessHover(Point mouse)
        {
            return false;
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

        public void AddTextSelection(Food f, string text)
        {
            Controls.Add(new GUITextSelectionWindow(f, text));
        }

        public void RemoveComponent(GUIObject g)
        {
            _toRemove.Add(g);
        }

        public virtual void ShowTextWindow(string text)
        {
            //
        }

        public virtual bool IsTextScreen() { return false; }
        public virtual bool IsGameMenuScreen() { return false; }
        public virtual bool IsItemCreationScreen() { return false; }
        public virtual bool IsHUD() { return false; }
    }
}
