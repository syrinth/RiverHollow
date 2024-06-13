using RiverHollow.Game_Managers;
using RiverHollow.GUIComponents.GUIObjects;

namespace RiverHollow.GUIComponents.Screens.HUDWindows
{
    public class HUDExit : GUIMainObject
    {
        public HUDExit()
        {
            _winMain = SetMainWindow(16, 16);

            var btnExit = new GUIButton("Quit", Exit);
            btnExit.AnchorToInnerSide(_winMain, SideEnum.TopLeft, 4);

            var btnTitle = new GUIButton("Title", Title);
            btnTitle.AnchorAndAlignWithSpacing(btnExit, SideEnum.Bottom, SideEnum.Left, 4, GUIUtils.ParentRuleEnum.ForceToParent);

            var btnCancel = new GUIButton("Cancel", Cancel);
            btnCancel.AnchorAndAlignWithSpacing(btnTitle, SideEnum.Bottom, SideEnum.Left, 4, GUIUtils.ParentRuleEnum.ForceToParent);

            _winMain.Resize(false, 4);
            _winMain.CenterOnScreen();
        }

        public void Title()
        {
            RiverHollow.Instance.GoToTitle();
            GUIManager.SetScreen(new IntroMenuScreen());
        }

        public void Exit()
        {
            RiverHollow.PrepExit();
        }

        public void Cancel()
        {
            GUIManager.CloseMainObject();
        }
    }
}
