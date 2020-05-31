using Microsoft.Xna.Framework;
using RiverHollow.Game_Managers;
using RiverHollow.Game_Managers.GUIComponents.GUIObjects;
using RiverHollow.Game_Managers.GUIObjects;
using RiverHollow.GUIComponents.GUIObjects;
using RiverHollow.GUIObjects;
using System.Collections.Generic;
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
                SaveWindow s = new SaveWindow(data, _liData.IndexOf(data));
                AddControl(s);
                _liDataWindows.Add(s);
            }

            if (_liDataWindows.Count > 0)
            {
                GUIObject.CreateSpacedColumn(ref _liDataWindows, RiverHollow.ScreenWidth / 2, 0, RiverHollow.ScreenHeight, 20);
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

        private class SaveWindow : GUIWindow
        {
            GUIText _gText;
            SaveInfoData _data;
            public SaveInfoData Data => _data;

            int _iId;
            public int SaveID => _iId;

            public SaveWindow(SaveInfoData data, int id)
            {
                _data = data;
                _gText = new GUIText(data.playerData.name + ", " + DataManager.GetClassByIndex(data.playerData.currentClass).Name);
                _iId = id;
                _winData = GUIWindow.RedWin;

                AddControl(_gText);

                Vector2 stringsize = _gText.MeasureString("XXXXXXXXXXX XXXXXXXXXXXXXX");
                Width = (int)stringsize.X;
                Height = (int)stringsize.Y + HeightEdges();

                _gText.CenterOnWindow(this);
            }

            public override bool ProcessLeftButtonClick(Point mouse)
            {
                bool rv = false;
                if (Contains(mouse))
                {
                    Load(Data.saveFile);
                    MapManager.PopulateMaps(true);
                    MapManager.EnterBuilding(PlayerManager.Buildings[0]);
                    GoToHUDScreen();
                    rv = true;
                }

                return rv;
            }
        }
    }
}
