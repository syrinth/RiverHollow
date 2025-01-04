using Microsoft.Xna.Framework;
using RiverHollow.Game_Managers;
using RiverHollow.GUIComponents.GUIObjects;
using RiverHollow.GUIComponents.GUIObjects.GUIWindows;

using static RiverHollow.Utilities.Enums;
using static RiverHollow.GUIComponents.GUIObjects.GUIObject;
using RiverHollow.GUIComponents.MainObjects;

namespace RiverHollow.GUIComponents.Screens
{
    class NewGameScreen : GUIScreen
    {
        readonly GUIButton _btnOK;
        readonly HUDCosmetics _gHUDCosmetics;

        public NewGameScreen()
        {
            GameManager.CurrentScreen = GameScreenEnum.Info;
            PlayerManager.PlayerActor.RandomizeCosmetics();

            var background = new GUIImage(DataManager.GetTexture(DataManager.GUI_COMPONENTS + @"\Newgame_Background"));
            background.IgnoreMe(true);

            _gHUDCosmetics = new HUDCosmetics();
            AddControl(_gHUDCosmetics);

            _btnOK = new GUIButton("OK", BtnNewGame);
            _btnOK.AnchorAndAlignWithSpacing(_gHUDCosmetics, SideEnum.Bottom, SideEnum.CenterX, 2);
            AddControl(_btnOK);
        }

        public override void Update(GameTime gTime)
        {
            base.Update(gTime);

            _btnOK.Enable(_gHUDCosmetics.TextBoxesFilled());
        }

        public override bool ProcessRightButtonClick(Point mouse)
        {
            if (!_gHUDCosmetics.ProcessRightButtonClick(mouse))
            {
                GUIManager.SetScreen(new IntroMenuScreen());
                GameManager.StopTakingInput();
            }

            return true;
        }



        #region Button Logic
        public void BtnNewGame()
        {
            PlayerManager.PlayerActor.SetScale();
            PlayerManager.SetName(_gHUDCosmetics.GetPlayerName());
            TownManager.SetTownName(_gHUDCosmetics.GetTownName());

            RiverHollow.Instance.NewGame(false);
            GameManager.StopTakingInput();
        }
        #endregion
    }
};
