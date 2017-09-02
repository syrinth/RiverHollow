using Adventure.GUIObjects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace Adventure.Game_Managers.GUIObjects
{
    //Represents a complete collection of associated GUIs to be displayed on the screen
    public abstract class GUIScreen
    {
        protected List<GUIObject> Controls;
        public bool IsVisible;

        public GUIScreen()
        {
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
    }
}
