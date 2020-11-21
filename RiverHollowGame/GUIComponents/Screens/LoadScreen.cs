using System.IO;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using RiverHollow.Game_Managers;
using RiverHollow.GUIComponents.GUIObjects;
using RiverHollow.GUIComponents.GUIObjects.GUIWindows;

using static RiverHollow.Game_Managers.GameManager;
using static RiverHollow.Game_Managers.SaveManager;

namespace RiverHollow.GUIComponents.Screens
{
    class LoadScreen : GUIScreen
    {
        List<SaveInfoData> _liData;
        List<GUIObject> _liDataWindows;
        GUIButton _btnBack;

        public LoadScreen()
        {
            _btnBack = new GUIButton("Back", BtnBack);
            _liDataWindows = new List<GUIObject>();
            _liData = SaveManager.LoadFiles();

            foreach(SaveInfoData data in _liData)
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
            GUIText _gText;
            SaveInfoData _data;
            public SaveInfoData Data => _data;

            public delegate void ReloadScreenDelegate();
            private ReloadScreenDelegate _delAction;

            int _iId;
            public int SaveID => _iId;

            public SaveWindow(SaveInfoData data, int id, ReloadScreenDelegate del)
            {
                //Creates the Individual Save Tiles on the load screen.
                _data = data;
                _iId = id;
                _winData = GUIWindow.RedWin;

                _gText = new GUIText(data.playerData.name + ", " + DataManager.GetClassByIndex(data.playerData.currentClass).Name);
                Vector2 stringsize = _gText.MeasureString("XXXXXXXXXXX XXXXXXXXXXXXXX");

                _gText.AnchorToInnerSide(this, SideEnum.TopLeft, GUIManager.STANDARD_MARGIN);
                AddControl(_gText);

                _gDelete = new GUIButton(new Rectangle(64, 48, TileSize, TileSize), GameManager.ScaledTileSize, GameManager.ScaledTileSize, DataManager.DIALOGUE_TEXTURE, BtnDelete);
                Height = (int)stringsize.Y + _gDelete.Height + HeightEdges();
                _gDelete.AnchorToInnerSide(this, SideEnum.BottomRight, GUIManager.STANDARD_MARGIN);

                _delAction = del;
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
