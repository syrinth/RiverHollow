using System.IO;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using RiverHollow.Game_Managers;
using RiverHollow.GUIComponents.GUIObjects;
using RiverHollow.GUIComponents.GUIObjects.GUIWindows;

using static RiverHollow.Game_Managers.GameManager;
using static RiverHollow.Game_Managers.SaveManager;
using System;
using Microsoft.Xna.Framework.Graphics;
using static RiverHollow.Utilities.Enums;
using RiverHollow.Utilities;

namespace RiverHollow.GUIComponents.Screens
{
    class LoadScreen : GUIScreen
    {
        List<SaveInfoData> _liData;
        List<GUIObject> _liDataWindows;
        GUIButton _btnBack;

        public LoadScreen()
        {
            GameManager.CurrentScreen = GameScreenEnum.Info;

            _btnBack = new GUIButton("Back", BtnBack);
            _liDataWindows = new List<GUIObject>();
            _liData = SaveManager.LoadFiles();
            _liData.Sort((x, y) => y.timeStamp.CompareTo(x.timeStamp));

            foreach (SaveInfoData data in _liData)
            {
                SaveWindow s = new SaveWindow(data, _liData.IndexOf(data), RefreshScreen);
                //AddControl(s);
                _liDataWindows.Add(s);
            }

            if (_liDataWindows.Count > 0)
            {
                GUIList _gli = new GUIList(_liDataWindows, 10, 20, RiverHollow.ScreenHeight);
                _gli.CenterOnScreen();
                AddControl(_gli);
                //GUIObject.CreateSpacedColumn(ref _liDataWindows, RiverHollow.ScreenWidth / 2, 0, RiverHollow.ScreenHeight, 20);
            }

            _btnBack.AnchorToScreen(this, GUIObject.SideEnum.BottomRight, 50);
        }

        public override bool ProcessLeftButtonClick(Point mouse)
        {
            bool rv = false;

            foreach (GUIObject c in Controls)
            {
                rv = c.ProcessLeftButtonClick(mouse);
                if (rv) { break; }
            }

            return rv;
        }

        public override bool ProcessRightButtonClick(Point mouse)
        {
            bool rv = false;

            GUIManager.SetScreen(new IntroMenuScreen());
            rv = true;

            return rv;
        }

        public void BtnBack()
        {
            GUIManager.SetScreen(new IntroMenuScreen());
        }

        public void RefreshScreen()
        {
            GUIManager.SetScreen(new LoadScreen());
        }

        private class SaveWindow : GUIWindow
        {
            GUIButton _gDelete;
            GUIText _gName;
            GUIText _gTimeStamp;
            GUIText _gDate;
            public SaveInfoData Data { get; }

            public delegate void ReloadScreenDelegate();
            private ReloadScreenDelegate _delAction;

            int _iId;
            public int SaveID => _iId;

            public SaveWindow(SaveInfoData data, int id, ReloadScreenDelegate del)
            {
                //Creates the Individual Save Tiles on the load screen.
                Data = data;
                _iId = id;
                _winData = GUIWindow.Window_1;

                _gName = new GUIText(data.playerData.name + ", " + DataManager.GetClassByIndex(data.playerData.currentClass).Name());
                Vector2 stringsize = _gName.MeasureString("XXXXXXXXXXX XXXXXXXXXXXXXX");

                _gName.AnchorToInnerSide(this, SideEnum.TopLeft, GUIManager.STANDARD_MARGIN);
                AddControl(_gName);

                _gDelete = new GUIButton(new Rectangle(64, 48, Constants.TILE_SIZE, Constants.TILE_SIZE), GameManager.ScaledTileSize, GameManager.ScaledTileSize, DataManager.DIALOGUE_TEXTURE, BtnDelete);
                Height = (int)stringsize.Y + _gDelete.Height + HeightEdges();
                _gDelete.AnchorToInnerSide(this, SideEnum.BottomRight, GUIManager.STANDARD_MARGIN);

                _gDate = new GUIText("Day: " + data.Calendar.dayOfMonth.ToString("00") + ", " + GameCalendar.GetSeason(data.Calendar.currSeason));
                _gDate.AnchorToInnerSide(this, SideEnum.BottomLeft, GUIManager.STANDARD_MARGIN);

                _gTimeStamp = new GUIText(data.timeStamp.ToString("g"));
                _gTimeStamp.AnchorAndAlignToObject(_gDelete, SideEnum.Left, SideEnum.Bottom);

                _delAction = del;
            }

            public override void Draw(SpriteBatch spriteBatch)
            {
                if (!Show())
                {
                    int i = 0;
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

                _delAction();
            }
        }
    }
}
