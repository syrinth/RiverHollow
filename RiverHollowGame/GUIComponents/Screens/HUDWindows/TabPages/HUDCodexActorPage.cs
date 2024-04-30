using Microsoft.Xna.Framework;
using RiverHollow.Characters;
using RiverHollow.Game_Managers;
using RiverHollow.GUIComponents.GUIObjects;
using RiverHollow.GUIComponents.GUIObjects.GUIWindows;
using RiverHollow.GUIComponents.Screens.HUDComponents;
using System.Collections.Generic;
using System.Linq;

namespace RiverHollow.GUIComponents.Screens.HUDWindows.TabPages
{
    internal class HUDCodexActorPage : GUIObject
    {
        protected const int ACTOR_COLUMNS = 5;
        protected const int MAX_ACTOR_DISPLAY = 15;
        protected List<NPCDisplayWindow> _liActorDisplay;

        readonly GUIText _gLabel;
        readonly GUIText _gTotal;
        readonly GUIButton _btnLeft;
        readonly GUIButton _btnRight;
        readonly GUIWindow _mainWindow;
        readonly List<Actor> _liActors;

        int _iIndex = 0;

        public HUDCodexActorPage(GUIWindow winMain, string pageName, List<Actor> actors, int found)
        {
            _liActorDisplay = new List<NPCDisplayWindow>();

            _liActors = actors;
            _mainWindow = winMain;

            _gLabel = new GUIText(pageName);
            _gLabel.AnchorToInnerSide(winMain, SideEnum.Top);

            _btnLeft = new GUIButton(GUIUtils.BTN_LEFT_SMALL, BtnLeft);
            _btnLeft.PositionAndMove(winMain, 7, 158);
            _btnLeft.Enable(false);

            _btnRight = new GUIButton(GUIUtils.BTN_RIGHT_SMALL, BtnRight);
            _btnRight.PositionAndMove(winMain, 169, 158);

            _gTotal = new GUIText(string.Format("{0}/{1}", found, actors.Count));
            _gTotal.AlignToObject(winMain, SideEnum.Center);
            _gTotal.AlignToObject(_btnLeft, SideEnum.CenterY);

            SetUpActorWindows();
        }

        protected void ClearWindows()
        {
            _liActorDisplay.ForEach(x => x.RemoveSelfFromControl());
            _liActorDisplay.Clear();
        }

        protected void SetUpActorWindows()
        {
            ClearWindows();
            for (int i = _iIndex; i < _iIndex + MAX_ACTOR_DISPLAY; i++)
            {
                if (_liActors.Count <= i)
                {
                    break;
                }

                var npc = new NPCDisplayWindow(_liActors[i]);
                _liActorDisplay.Add(npc);
            }

            GUIUtils.CreateSpacedGrid(new List<GUIObject>(_liActorDisplay), _mainWindow, new Point(9, 18), ACTOR_COLUMNS, 2, 3);

            _btnLeft.Enable(_iIndex >= MAX_ACTOR_DISPLAY);
            _btnRight.Enable(_iIndex + MAX_ACTOR_DISPLAY < _liActors.Count);
        }

        public void BtnLeft()
        {
            _iIndex -= MAX_ACTOR_DISPLAY;
            _btnRight.Enable(true);
            SetUpActorWindows();
        }
        public void BtnRight()
        {
            _iIndex += MAX_ACTOR_DISPLAY;
            _btnLeft.Enable(true);
            SetUpActorWindows();
        }
    }

    internal class HUDCodexVillagers : HUDCodexActorPage
    {
        public HUDCodexVillagers(GUIWindow winMain) : base(winMain, "Villagers", TownManager.Villagers.Values.Cast<Actor>().ToList(), TownManager.Villagers.Values.Count(x => x.Introduced)) { }
    }

    internal class HUDCodexMerchants : HUDCodexActorPage
    {
        public HUDCodexMerchants(GUIWindow winMain) : base(winMain, "Merchants", TownManager.DIMerchants.Values.Cast<Actor>().ToList(), TownManager.DIMerchants.Values.Count(x => x.Introduced)) { }
    }

    internal class HUDCodexMobs : HUDCodexActorPage
    {
        public HUDCodexMobs(GUIWindow winMain) : base(winMain, "Mobs", DataManager.GetAllMobs(), TownManager.DIMobInfo.Values.Count(x => x > 0)) { }
    }

    internal class HUDCodexTravelers : HUDCodexActorPage
    {
        public HUDCodexTravelers(GUIWindow winMain) : base(winMain, "Travelers", DataManager.GetAllTravelers(), TownManager.DITravelerInfo.Values.Count(x => x.Item1)) { }
    }
}
