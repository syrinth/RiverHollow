using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.Game_Managers;
using RiverHollow.Game_Managers.GUIComponents.GUIObjects;
using RiverHollow.Game_Managers.GUIObjects;
using RiverHollow.GUIComponents.GUIObjects;
using RiverHollow.GUIObjects;
using System.Collections.Generic;
using static RiverHollow.Game_Managers.GameManager;

namespace RiverHollow.GUIComponents.Screens
{
    class LoadScreen : GUIScreen
    {
        List<SaveData> _liData;
        List<GUIObject> _liDataWindows;
        GUIButton _btnBack;

        public LoadScreen()
        {
            _btnBack = new GUIButton("Back", BtnBack);
            _liDataWindows = new List<GUIObject>();
            _liData = GameManager.LoadFiles();

            foreach(SaveData data in _liData)
            {
                SaveWindow s = new SaveWindow(data, _liData.IndexOf(data));
                Controls.Add(s);
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
            SaveData _data;
            public SaveData Data => _data;

            int _iId;
            public int SaveID => _iId;

            public SaveWindow(SaveData data, int id)
            {
                _data = data;
                _gText = new GUIText(data.playerData.name + ", " + ObjectManager.GetClassByIndex(data.playerData.currentClass).Name);
                _iId = id;
                Position(Vector2.Zero);
                _winData = GUIWindow.RedWin;

                Vector2 stringsize = _gText.MeasureString("XXXXXXXXXXX XXXXXXXXXXXXXX");
                Width = (int)stringsize.X;
                Height = (int)stringsize.Y;
            }

            public override void Update(GameTime gameTime)
            {
                _gText.Update(gameTime);
            }

            public override void Draw(SpriteBatch spriteBatch)
            {
                base.Draw(spriteBatch);
            }

            public override void Position(Vector2 value)
            {
                base.Position(value);
                if (_gText != null) { _gText.CenterOnWindow(this); }
            }

            public override bool ProcessLeftButtonClick(Point mouse)
            {
                bool rv = false;
                if (Contains(mouse))
                {
                    Load(Data);
                    MapManager.PopulateMaps(true);
                    MapManager.EnterBuilding(PlayerManager.Buildings[0]);
                    BackToMain();
                    rv = true;
                }

                return rv;
            }
        }
    }
}
