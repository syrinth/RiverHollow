using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.Game_Managers.GUIComponents.GUIObjects;
using System.Collections.Generic;
using RiverHollow.GUIObjects;
using RiverHollow.Misc;

namespace RiverHollow.Game_Managers.GUIObjects
{
    public class QuestScreen : GUIScreen
    {
        public static int WIDTH = RiverHollow.ScreenWidth / 3;
        public static int HEIGHT = RiverHollow.ScreenHeight / 3;
        List<QuestBox> _questList;
        GUIWindow _questWindow;
        public QuestScreen()
        {
            _questList = new List<QuestBox>();
            _questWindow = new GUIWindow(new Vector2(WIDTH, HEIGHT), GUIWindow.RedDialog, GUIWindow.RedDialogEdge, WIDTH, HEIGHT);
            int i = 0;
            foreach (Quest q in PlayerManager.QuestLog)
            {
                _questList.Add(new QuestBox(q, _questWindow, ref i));// new Vector2(_questWindow.UsableRectangle().Left, _questWindow.UsableRectangle().Top + (i++ * 100))));
            }

            Controls.Add(_questWindow);
            foreach (QuestBox q in _questList)
            {
                Controls.Add(q);
            }
        }

        public override bool ProcessLeftButtonClick(Point mouse)
        {
            bool rv = false;
            foreach (QuestBox c in _questList)
            {
                rv = c.ProcessLeftButtonClick(mouse);
                if (rv) { break; }
            }
            return rv;
        }

        public override bool ProcessRightButtonClick(Point mouse)
        {
            bool rv = true;
            return rv;
        }

        public override bool ProcessHover(Point mouse)
        {
            bool rv = true;
            foreach (QuestBox c in _questList)
            {
                rv = c.ProcessHover(mouse);
                if (rv)
                {
                    break;
                }
            }
            return rv;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }
    }

    public class QuestBox : GUIObject
    {
        GUIWindow _window;
        Quest _quest;
        SpriteFont _font;
        public bool ClearThis;

        public QuestBox(Quest q, GUIWindow win, ref int i)
        {
            int boxHeight = (QuestScreen.HEIGHT / 4) - (win.EdgeSize * 2);
            int boxWidth = (QuestScreen.WIDTH) - (win.EdgeSize * 2) -32;
            Vector2 boxPoint = new Vector2(win.Corner().X + win.EdgeSize, win.Corner().Y + win.EdgeSize + (i++ * (boxHeight + (win.EdgeSize *2))));
            _window = new GUIWindow(boxPoint, GUIWindow.RedDialog, GUIWindow.RedDialogEdge, boxWidth, boxHeight);

            _font = GameContentManager.GetFont(@"Fonts\Font");
            _quest = q;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            _window.Draw(spriteBatch);
            spriteBatch.DrawString(_font, _quest.Name, _window.UsableRectangle().Location.ToVector2(), Color.White);
            spriteBatch.DrawString(_font, _quest.Accomplished + @"/" + _quest.TargetGoal, _window.UsableRectangle().Location.ToVector2() + new Vector2(200, 0), Color.White);
        }

        public bool ProcessLeftButtonClick(Point mouse)
        {
            bool rv = false;
            return rv;
        }
        public bool ProcessHover(Point mouse)
        {
            bool rv = false;
            return rv;
        }
        public override bool Contains(Point mouse)
        {
            return _window.Contains(mouse);
        }
    }
}
