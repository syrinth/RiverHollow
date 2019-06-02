﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.Game_Managers.GUIComponents.GUIObjects;
using System.Collections.Generic;
using RiverHollow.GUIObjects;
using RiverHollow.Misc;
using RiverHollow.GUIComponents.GUIObjects;

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
        DetailBox _detailWindow;
        GUIButton _btnUp;
        GUIButton _btnDown;

        bool _bMoved;
        int _topQuest;
        public QuestScreen()
        {
            _questList = new List<QuestBox>();
            _questWindow = new GUIWindow(GUIWindow.RedWin, WIDTH, HEIGHT);
            _detailWindow = new DetailBox(GUIWindow.RedWin, WIDTH, HEIGHT);
            _questWindow.CenterOnScreen();
            _detailWindow.CenterOnScreen();

            _btnUp = new GUIButton(new Rectangle(256, 64, 32, 32), BTNSIZE, BTNSIZE, @"Textures\Dialog", BtnUpClick);
            _btnDown = new GUIButton(new Rectangle(256, 96, 32, 32), BTNSIZE, BTNSIZE, @"Textures\Dialog", BtnDownClick);

            _btnUp.AnchorAndAlignToObject(_questWindow, GUIObject.SideEnum.Right, GUIObject.SideEnum.Top);
            _btnDown.AnchorAndAlignToObject(_questWindow, GUIObject.SideEnum.Right, GUIObject.SideEnum.Bottom);
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
             _bMoved = false;
            if (!Controls.Contains(_detailWindow))
            {
                foreach(GUIObject c in Controls)
                {
                    rv = c.ProcessLeftButtonClick(mouse);

                    if (rv) { break; }
                }

                if (_bMoved)
                {
                    for (int i = 0; i < _questList.Count; i++)
                    {
                        _questList[i].SetQuest(PlayerManager.QuestLog[_topQuest + i]);
                    }
                }

                foreach (QuestBox c in _questList)
                {
                    if (c.Contains(mouse))
                    {
                        _detailWindow.SetData(c.TheQuest);
                        Controls.Add(_detailWindow);
                        Controls.Remove(_btnUp);
                        Controls.Remove(_btnDown);

                        rv = true;
                    }
                    if (rv) { break; }
                }
            }
            return rv;
        }

        public override bool ProcessRightButtonClick(Point mouse)
        {
            bool rv = true;
            if (Controls.Contains(_detailWindow))
            {
                Controls.Remove(_detailWindow);
                Controls.Add(_btnUp);
                Controls.Add(_btnDown);
            }
            return rv;
        }

        public void BtnUpClick()
        {
            if (_topQuest - 1 >= 0) { _topQuest--; _bMoved = true; }
        }
        public void BtnDownClick()
        {
            if (_topQuest + MAX_SHOWN_QUESTS < PlayerManager.QuestLog.Count) { _topQuest++; _bMoved = true; }
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
        public Quest TheQuest => _quest;
        SpriteFont _font;
        private int _index;
        public int Index { get => _index; }
        public bool ClearThis;


        public QuestBox(GUIWindow win, int i)
        {
            _index = i;

            int boxHeight = (QuestScreen.HEIGHT / QuestScreen.MAX_SHOWN_QUESTS) - (win.EdgeSize * 2);
            int boxWidth = (QuestScreen.WIDTH) - (win.EdgeSize * 2) - QuestScreen.BTNSIZE;
            _window = new GUIWindow(GUIWindow.RedWin, boxWidth, boxHeight);
            _window.AnchorToInnerSide(win, SideEnum.TopLeft);

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

        public override bool ProcessHover(Point mouse)
        {
            bool rv = false;
            return rv;
        }
        public override bool Contains(Point mouse)
        {
            return _window.Contains(mouse);
        }
    }

    public class DetailBox : GUIWindow
    {
        GUIText _name;
        GUIText _desc;
        GUIText _progress;
        public DetailBox(WindowData winData, int width, int height) : base(winData, width, height)
        {
        }

        public void SetData(Quest q)
        {
            Controls.Clear();
            _name = new GUIText(q.Name);
            _name.AnchorToInnerSide(this, SideEnum.TopLeft);

            _desc = new GUIText(q.Description);
            _desc.ParseText(3, this.MidWidth(), true);
            _desc.AnchorAndAlignToObject(_name, SideEnum.Bottom, SideEnum.Left, _name.CharHeight);

            _progress = new GUIText(q.GetProgressString());
            _progress.AnchorToInnerSide(this, SideEnum.BottomRight);
        }
    }
}
