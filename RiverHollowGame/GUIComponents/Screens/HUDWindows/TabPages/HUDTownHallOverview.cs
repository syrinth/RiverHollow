using RiverHollow.Game_Managers;
using RiverHollow.GUIComponents.GUIObjects;
using RiverHollow.GUIComponents.GUIObjects.GUIWindows;
using static RiverHollow.Utilities.Enums;

namespace RiverHollow.GUIComponents.Screens.HUDWindows.TabPages
{
    internal class HUDTownHallOverview : GUIObject
    {
        public HUDTownHallOverview(GUIWindow mainWindow)
        {
            TownManager.GetTownScoreInfo(out int townScore, out AffinityEnum affinity);

            var gTownScore = new GUIText("Town Score: " + townScore);
            gTownScore.AnchorToInnerSide(mainWindow, SideEnum.Top);

            var goodsSold = new GUIText(string.Format("Goods Sold: " + TownManager.ValueGoodsSold));
            goodsSold.AnchorToInnerSide(mainWindow, SideEnum.TopLeft);
            goodsSold.MoveBy(0, gTownScore.Height);
            goodsSold.ScaledMoveBy(0, 10);

            var enemiesDefeated = new GUIText(string.Format("Enemies Defeated: " + TownManager.TotalDefeatedMobs));
            enemiesDefeated.AnchorAndAlignWithSpacing(goodsSold, SideEnum.Bottom, SideEnum.Left, 10);

            var plantsGrown = new GUIText(string.Format("Plants Grown: " + TownManager.PlantsGrown));
            plantsGrown.AnchorAndAlignWithSpacing(enemiesDefeated, SideEnum.Bottom, SideEnum.Left, 10);

            //Do this to make sure to sync completion with TownScore
            TaskManager.TaskLog.ForEach(x => x.AttemptProgress());
        }
    }
}
