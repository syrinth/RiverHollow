using System.IO;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using RiverHollow.Game_Managers;
using RiverHollow.GUIComponents.GUIObjects;
using RiverHollow.GUIComponents.GUIObjects.GUIWindows;
using Microsoft.Xna.Framework.Graphics;

using static RiverHollow.Utilities.Enums;
using static RiverHollow.Game_Managers.SaveManager;

namespace RiverHollow.GUIComponents.Screens
{
    class LoadScreen : GUIScreen
    {
        List<SaveInfoData> _liData;
        List<GUIObject> _liDataWindows;

        GUIList _gList;

        public LoadScreen()
        {
            GameManager.CurrentScreen = GameScreenEnum.Info;

            _liDataWindows = new List<GUIObject>();

            var btn = new GUIButton("Back", BtnBack);
            btn.AnchorToScreen(GUIObject.SideEnum.BottomRight, 1);

            LoadSaves();
        }

        public override bool ProcessRightButtonClick(Point mouse)
        {
            GUIManager.SetScreen(new IntroMenuScreen());
            return true;
        }

        public void BtnBack()
        {
            GUIManager.SetScreen(new IntroMenuScreen());
        }

        public void LoadSaves()
        {
            _gList?.RemoveSelfFromControl();
            _liDataWindows?.Clear();
            
            _liData = SaveManager.LoadFiles();
            _liData.Sort((x, y) => y.timeStamp.CompareTo(x.timeStamp));

            foreach (SaveInfoData data in _liData)
            {
                SaveWindow s = new SaveWindow(data, _liData.IndexOf(data), LoadSaves);
                _liDataWindows.Add(s);
            }

            if (_liDataWindows.Count > 0)
            {
                _gList = new GUIList(_liDataWindows, 10, 5, null, RiverHollow.ScreenHeight);
            }
        }

        private class SaveWindow : GUIWindow
        {
            readonly GUIButton _gDelete;
            readonly GUIText _gName;
            readonly GUIText _gTimeStamp;
            readonly GUIText _gDate;
            public SaveInfoData Data { get; }

            public delegate void ReloadScreenDelegate();
            private readonly ReloadScreenDelegate _delAction;

            readonly int _iId;
            public int SaveID => _iId;

            public SaveWindow(SaveInfoData data, int id, ReloadScreenDelegate del)
            {
                //Creates the Individual Save Tiles on the load screen.
                Data = data;
                _iId = id;
                _winData = GUIUtils.WINDOW_BROWN;

                _gName = new GUIText(data.playerData.name);
                Vector2 stringsize = _gName.MeasureString("XXXXXXXXXXX XXXXXXXXXXXXXX");

                _gName.AnchorToInnerSide(this, SideEnum.TopLeft, GUIManager.STANDARD_MARGIN);
                AddControl(_gName);

                _gDelete = new GUIButton(GUIUtils.ICON_GARBAGE, BtnDelete);
                Height = (int)stringsize.Y + _gDelete.Height + HeightEdges();
                _gDelete.AnchorToInnerSide(this, SideEnum.BottomRight, GUIManager.STANDARD_MARGIN);

                _gDate = new GUIText("Day " + data.Calendar.dayOfMonth.ToString("00") + ", " + GameCalendar.GetSeason(data.Calendar.currSeason));
                _gDate.AnchorToInnerSide(this, SideEnum.BottomLeft, GUIManager.STANDARD_MARGIN);

                _gTimeStamp = new GUIText(data.timeStamp.ToString("g"));
                _gTimeStamp.AnchorAndAlign(_gDelete, SideEnum.Left, SideEnum.Bottom);

                _delAction = del;
            }

            public override void Draw(SpriteBatch spriteBatch)
            {
                if (!Show())
                {
                    LogManager.WriteToLog("Show is false");
                }
                base.Draw(spriteBatch);
            }

            public override bool ProcessLeftButtonClick(Point mouse)
            {
                bool rv = _gDelete.ProcessLeftButtonClick(mouse);
                if (!rv && Contains(mouse))
                {
                    RiverHollow.Instance.LoadGame(Data.saveFile);
                    rv = true;
                }

                return rv;
            }

            public void BtnDelete()
            {
                string[] split = Data.saveFile.Split('\\');
                string folder = split[0] + "\\" + split[1];

                foreach (string s in Directory.GetFiles(folder))
                {
                    File.Delete(s);
                }
                Directory.Delete(folder);

                _delAction();
            }
        }
    }
}
