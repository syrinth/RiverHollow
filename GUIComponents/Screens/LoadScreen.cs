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
            _btnBack = new GUIButton("Back");
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

            _btnBack.AnchorToScreen(GUIObject.SideEnum.BottomRight, 50);

            Controls.Add(_btnBack);
        }

        public override bool ProcessLeftButtonClick(Point mouse)
        {
            bool rv = false;

            foreach (GUIObject o in _liDataWindows)
            {
                if (o.Contains(mouse))
                {
                    SaveWindow win = ((SaveWindow)o);
                    Load(win.Data);
                    MapManager.PopulateMaps(true);
                    BackToMain();
                }
            }

            if (_btnBack.ProcessLeftButtonClick(mouse))
            {
                GUIManager.SetScreen(new IntroMenuScreen());
                rv = true;
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

        private class SaveWindow : GUIWindow
        {
            GUIText _sText;
            SaveData _data;
            public SaveData Data => _data;

            int _iId;
            public int SaveID => _iId;

            public SaveWindow(SaveData data, int id)
            {
                _data = data;
                _sText = new GUIText(data.name + ", " + CharacterManager.GetClassByIndex(data.currentClass).Name);
                _iId = id;
                Position(Vector2.Zero);
                _winData = GUIWindow.RedWin;

                Vector2 stringsize = _sText.MeasureString("XXXXXXXXXXX XXXXXXXXXXXXXX");
                Width = (int)stringsize.X;
                Height = (int)stringsize.Y;
            }

            public override void Update(GameTime gameTime)
            {
                _sText.Update(gameTime);
            }

            public override void Draw(SpriteBatch spriteBatch)
            {
                base.Draw(spriteBatch);
                _sText.Draw(spriteBatch);
            }

            public override void Position(Vector2 value)
            {
                base.Position(value);
                if (_sText != null) { _sText.CenterOnWindow(this); }
            }
        }
    }
}
