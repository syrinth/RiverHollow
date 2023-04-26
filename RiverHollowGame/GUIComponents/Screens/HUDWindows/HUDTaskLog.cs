using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.Game_Managers;
using RiverHollow.GUIComponents.GUIObjects;
using RiverHollow.GUIComponents.GUIObjects.GUIWindows;
using RiverHollow.Misc;
using System.Collections.Generic;
using System.Linq;

namespace RiverHollow.GUIComponents.Screens.HUDWindows
{
    public class HUDTaskLog : GUIMainObject
    {
        //public static int BTNSIZE = ScaledTileSize;
        public static int MAX_SHOWN_TASKS = 4;
        public static int TASK_SPACING = 4;
        public static int TASKBOX_WIDTH = 544; //(GUIManager.MAIN_COMPONENT_WIDTH) - (_gWindow.EdgeSize * 2) - ScaledTileSize
        public static int TASKBOX_HEIGHT = 128; //(GUIManager.MAIN_COMPONENT_HEIGHT / HUDTaskLog.MAX_SHOWN_TASKS) - (_gWindow.EdgeSize * 2)
        List<GUIObject> _liTasks;
        DetailBox _detailWindow;
        GUIList _gList;

        public HUDTaskLog()
        {
            _winMain = SetMainWindow();

            _liTasks = new List<GUIObject>();
            _detailWindow = new DetailBox(GUIUtils.Brown_Window, GUIManager.MAIN_COMPONENT_WIDTH, GUIManager.MAIN_COMPONENT_HEIGHT);
            _detailWindow.Show(false);
            _detailWindow.CenterOnScreen();
            AddControl(_detailWindow);

            for (int i = 0; i < TaskManager.TaskLog.Count; i++)
            {
                TaskBox q = new TaskBox(TASKBOX_WIDTH, TASKBOX_HEIGHT, OpenDetailBox);
                q.SetTask(TaskManager.TaskLog[i]);
                _liTasks.Add(q);
            }

            _gList = new GUIList(_liTasks, MAX_SHOWN_TASKS, TASK_SPACING/*, _gWindow.Height*/);
            _gList.CenterOnObject(_winMain);
        }

        public override bool ProcessLeftButtonClick(Point mouse)
        {
            bool rv = false;
            if (!_detailWindow.Show())
            {
                foreach (GUIObject c in Controls)
                {
                    rv = c.ProcessLeftButtonClick(mouse);

                    if (rv) { break; }
                }
            }
            return rv;
        }

        public override bool ProcessRightButtonClick(Point mouse)
        {
            bool rv = false;
            if (_detailWindow.Show())
            {
                rv = true;
                ShowDetails(false);
            }
            return rv;
        }

        public override bool ProcessHover(Point mouse)
        {
            bool rv = true;
            if (_detailWindow.Show())
            {
                rv = _detailWindow.ProcessHover(mouse);
            }
            return rv;
        }

        private void OpenDetailBox(RHTask q)
        {
            _detailWindow.SetData(q);
            ShowDetails(true);
        }

        private void ShowDetails(bool val)
        {
            _detailWindow.Show(val);
            _winMain.Show(!val);
            _gList.Show(!val);
        }

        public class TaskBox : GUIObject
        {
            GUIWindow _window;
            GUIText _gName;
            GUIText _gGoalProgress;
            public RHTask TheTask { get; private set; }
            public delegate void ClickDelegate(RHTask q);
            private ClickDelegate _delAction;

            public TaskBox(int width, int height, ClickDelegate del)
            {
                _delAction = del;

                int boxHeight = height;
                int boxWidth = width;
                _window = new GUIWindow(GUIUtils.Brown_Window, boxWidth, boxHeight);
                AddControl(_window);
                Width = _window.Width;
                Height = _window.Height;
                TheTask = null;
            }

            public override void Draw(SpriteBatch spriteBatch)
            {
                if (TheTask != null && Show())
                {
                    _window.Draw(spriteBatch);
                }
            }
            public override bool ProcessLeftButtonClick(Point mouse)
            {
                bool rv = false;
                if (Contains(mouse))
                {
                    _delAction(TheTask);
                }

                return rv;
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

            public void SetTask(RHTask q)
            {
                TheTask = q;
                _gName = new GUIText(TheTask.Name);
                _gName.AnchorToInnerSide(_window, SideEnum.TopLeft);

                string progressString = q.GetProgressString();
                if (!string.IsNullOrEmpty(progressString))
                {
                    _gGoalProgress = new GUIText(progressString);
                    _gGoalProgress.AnchorToInnerSide(_window, SideEnum.BottomRight);
                }
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

            public void SetData(RHTask q)
            {
                Controls.Clear();
                _name = new GUIText(q.Name);
                _name.AnchorToInnerSide(this, SideEnum.TopLeft);

                _desc = new GUIText();
                _desc.ParseAndSetText(q.Description, InnerWidth(), 3, true);
                _desc.AnchorAndAlignWithSpacing(_name, SideEnum.Bottom, SideEnum.Left, _name.CharHeight);

                List<GUIObject> boxes = new List<GUIObject>();
                for (int i = 0; i < q.LiRewardItems.Count; i++)
                {
                    GUIItemBox newBox = new GUIItemBox(q.LiRewardItems[i]);
                    boxes.Add(newBox);

                    if (i == 0) { newBox.AnchorAndAlign(_desc, SideEnum.Bottom, SideEnum.Left); }
                    else { newBox.AnchorAndAlign(boxes[i - 1], SideEnum.Right, SideEnum.Top); }
                    AddControl(newBox);
                }

                _progress = new GUIText(q.GetProgressString());
                _progress.AnchorToInnerSide(this, SideEnum.BottomRight);
            }
        }
    }
}
