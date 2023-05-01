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
        readonly List<SaveInfoData> _liData;
        readonly List<GUIObject> _liDataWindows;
        readonly GUIButton _btnBack;

        public LoadScreen()
        {
            GameManager.CurrentScreen = GameScreenEnum.Info;

            _btnBack = new GUIButton("Back", BtnBack);
            _btnBack.AnchorToScreen(GUIObject.SideEnum.BottomRight, 1);

            _liDataWindows = new List<GUIObject>();
            _liData = SaveManager.LoadFiles();
            _liData.Sort((x, y) => y.timeStamp.CompareTo(x.timeStamp));

            foreach (SaveInfoData data in _liData)
            {
                SaveWindow s = new SaveWindow(data, _liData.IndexOf(data));
                _liDataWindows.Add(s);
            }

            if (_liDataWindows.Count > 0)
            {
                GUIList _gli = new GUIList(_liDataWindows, 10, 5, null, RiverHollow.ScreenHeight);
            }
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

        private class SaveWindow : GUIWindow
        {
            GUIButton _gDelete;
            GUIText _gName;
            GUIText _gTimeStamp;
            GUIText _gDate;
            public SaveInfoData Data { get; }

            int _iId;
            public int SaveID => _iId;

            public SaveWindow(SaveInfoData data, int id)
            {
                //Creates the Individual Save Tiles on the load screen.
                Data = data;
                _iId = id;
                _winData = GUIUtils.Brown_Window;

                _gName = new GUIText(data.playerData.name);
                Vector2 stringsize = _gName.MeasureString("XXXXXXXXXXX XXXXXXXXXXXXXX");

                _gName.AnchorToInnerSide(this, SideEnum.TopLeft, GUIManager.STANDARD_MARGIN);
                AddControl(_gName);

                _gDelete = new GUIButton(GUIUtils.ICON_GARBAGE, BtnDelete);
                Height = (int)stringsize.Y + _gDelete.Height + HeightEdges();
                _gDelete.AnchorToInnerSide(this, SideEnum.BottomRight, GUIManager.STANDARD_MARGIN);

                _gDate = new GUIText("Day: " + data.Calendar.dayOfMonth.ToString("00") + ", " + GameCalendar.GetSeason(data.Calendar.currSeason));
                _gDate.AnchorToInnerSide(this, SideEnum.BottomLeft, GUIManager.STANDARD_MARGIN);

                _gTimeStamp = new GUIText(data.timeStamp.ToString("g"));
                _gTimeStamp.AnchorAndAlign(_gDelete, SideEnum.Left, SideEnum.Bottom);
            }

            public override void Draw(SpriteBatch spriteBatch)
            {
                if (!Show())
                {
                    ErrorManager.TrackError();
                }
                base.Draw(spriteBatch);
            }

            public override bool ProcessLeftButtonClick(Point mouse)
            {
                bool rv = false;
                rv = _gDelete.ProcessLeftButtonClick(mouse);
                if (!rv && Contains(mouse))
                {
                    RiverHollow.LoadGame(Data.saveFile);
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
            }
        }
    }
}
