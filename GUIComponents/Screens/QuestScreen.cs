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
        public static int BTNSIZE = 32;
        public static int MAX_SHOWN_QUESTS = 4;
        List<QuestBox> _questList;
        GUIWindow _questWindow;
        GUIButton _btnUp;
        GUIButton _btnDown;

        int _topQuest;
        public QuestScreen()
        {
            _questList = new List<QuestBox>();
            _questWindow = new GUIWindow(new Vector2(WIDTH, HEIGHT), GUIWindow.RedWin, WIDTH, HEIGHT);
            _btnUp = new GUIButton(new Vector2(_questWindow.InnerTopLeft().X +_questWindow.Width - BTNSIZE, _questWindow.InnerTopLeft().Y), new Rectangle(256, 64, 32, 32), BTNSIZE, BTNSIZE, "", @"Textures\Dialog", true);
            _btnDown = new GUIButton(new Vector2(_questWindow.InnerTopLeft().X + _questWindow.Width - BTNSIZE, _questWindow.InnerTopLeft().Y + _questWindow.Height - BTNSIZE), new Rectangle(256, 96, 32, 32), BTNSIZE, BTNSIZE, "", @"Textures\Dialog", true);
            _topQuest = 0;

            for(int i = 0; i < MAX_SHOWN_QUESTS && i< PlayerManager.QuestLog.Count; i++)
            {
                QuestBox q = new QuestBox(_questWindow, i);
                q.SetQuest(PlayerManager.QuestLog[_topQuest + i]);
                _questList.Add(q);
            }


            Controls.Add(_questWindow);
            Controls.Add(_btnUp);
            Controls.Add(_btnDown);
            foreach (QuestBox q in _questList)
            {
                Controls.Add(q);
            }
        }

        public override bool ProcessLeftButtonClick(Point mouse)
        {
            bool rv = false;
            bool moved = false;
            if (_btnUp.Contains(mouse))
            {
                if (_topQuest - 1 >= 0) { _topQuest--; moved = true; }
            }
            else if (_btnDown.Contains(mouse))
            {
                if (_topQuest + MAX_SHOWN_QUESTS < PlayerManager.QuestLog.Count) { _topQuest++; moved = true; }
            }
            if (moved)
            {
                for(int i = 0; i< _questList.Count; i++)
                {
                    _questList[i].SetQuest(PlayerManager.QuestLog[_topQuest + i]);
                }
            }

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
        private int _index;
        public int Index { get => _index; }
        public bool ClearThis;


        public QuestBox(GUIWindow win, int i)
        {
            _index = i;

            int boxHeight = (QuestScreen.HEIGHT / QuestScreen.MAX_SHOWN_QUESTS) - (win.EdgeSize * 2);
            int boxWidth = (QuestScreen.WIDTH) - (win.EdgeSize * 2) - QuestScreen.BTNSIZE;
            Vector2 boxPoint = new Vector2(win.InnerTopLeft().X + win.EdgeSize, win.InnerTopLeft().Y + win.EdgeSize + (i * (boxHeight + (win.EdgeSize *2))));
            _window = new GUIWindow(boxPoint, GUIWindow.RedWin, boxWidth, boxHeight);

            _font = GameContentManager.GetFont(@"Fonts\Font");
            _quest = null;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (_quest != null)
            {
                _window.Draw(spriteBatch);
                spriteBatch.DrawString(_font, _quest.Name, _window.InnerRecVec(), Color.White);
                spriteBatch.DrawString(_font, _quest.Accomplished + @"/" + _quest.TargetGoal, _window.InnerRecVec() + new Vector2(200, 0), Color.White);
            }
        }

        public void SetQuest(Quest q)
        {
            _quest = q;
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
