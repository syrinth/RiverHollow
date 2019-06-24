using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.Actors;
using RiverHollow.Buildings;
using RiverHollow.Game_Managers;
using RiverHollow.Game_Managers.GUIComponents.GUIObjects;
using RiverHollow.Game_Managers.GUIObjects;
using RiverHollow.GUIComponents.GUIObjects;
using RiverHollow.GUIObjects;
using System.Collections.Generic;
using System.Linq;
using static RiverHollow.Game_Managers.GUIObjects.GUIButton;
using static RiverHollow.GUIComponents.GUIObjects.NPCDisplayBox;
using static RiverHollow.GUIObjects.GUIObject;

namespace RiverHollow.GUIComponents.Screens
{
    class MissionScreen : GUIScreen
    {
        public static int WIDTH = RiverHollow.ScreenWidth / 3;
        public static int HEIGHT = RiverHollow.ScreenHeight / 3;
        public static int BTNSIZE = 32;
        public static int MAX_SHOWN_MISSIONS = 4;

        int _iTopMission = 0;

        GUIButton _btnUp;
        GUIButton _btnDown;
        GUIWindow _gWin;
        DetailWindow _gDetailWindow;
        WorkerWindow _gWinWorkers;

        List<MissionBox> _liMissions;

        public MissionScreen()
        {
            GameManager.Pause();
            _gWin = new GUIWindow(GUIWindow.RedWin, WIDTH, HEIGHT);
            _liMissions = new List<MissionBox>();

            AddControl(_gWin);

            AssignMissionWindows();

            _gWin.CenterOnScreen();

            if(MissionManager.AvailableMissions.Count > MAX_SHOWN_MISSIONS)
            {
                _btnUp = new GUIButton(new Rectangle(256, 64, 32, 32), BTNSIZE, BTNSIZE, @"Textures\Dialog", BtnUpClick);
                _btnDown = new GUIButton(new Rectangle(256, 96, 32, 32), BTNSIZE, BTNSIZE, @"Textures\Dialog", BtnDownClick);

                _btnUp.AnchorAndAlignToObject(_gWin, GUIObject.SideEnum.Right, GUIObject.SideEnum.Top);
                _btnDown.AnchorAndAlignToObject(_gWin, GUIObject.SideEnum.Right, GUIObject.SideEnum.Bottom);

                _btnUp.Enable(false);
                _btnDown.Enable(false);

                RemoveControl(_btnDown);
                RemoveControl(_btnUp);
                _gWin.AddControl(_btnDown);
                _gWin.AddControl(_btnUp);
            }
        }

        public override bool ProcessLeftButtonClick(Point mouse)
        {
            bool rv = false;

            if(_gWinWorkers != null)
            {
                rv = _gWinWorkers.ProcessLeftButtonClick(mouse);
            }
            else if (_gDetailWindow != null)
            {
                rv = _gDetailWindow.ProcessLeftButtonClick(mouse);
            }
            else{
                if (_btnDown != null && _btnDown.ProcessLeftButtonClick(mouse))
                {
                    rv = true;
                }
                else if (_btnUp != null && _btnUp.ProcessLeftButtonClick(mouse))
                {
                    rv = true;
                }
                else
                {
                    foreach (MissionBox box in _liMissions)
                    {
                        if (box.ProcessLeftButtonClick(mouse))
                        {
                            rv = true;
                            break;
                        }
                    }
                }
            }

            return rv;
        }
        public override bool ProcessRightButtonClick(Point mouse)
        {
            bool rv = false;

            if(_gDetailWindow != null) {
                rv = _gDetailWindow.ProcessRightButtonClick(mouse);
                if (!rv)
                {
                    RemoveControl(_gDetailWindow);
                    _gDetailWindow = null;
                    AddControl(_gWin);
                }
            }
            else
            {
                rv = base.ProcessRightButtonClick(mouse);
                if (!rv)
                {
                    MissionManager.ClearMissionAcceptance();
                }
            }

            return rv;
        }

        public override bool ProcessHover(Point mouse)
        {
            bool rv = false;

            if (_gWin.Contains(mouse))
            {
                GraphicCursor.Alpha = 1;
            }

            return rv;
        }

        /// <summary>
        /// Clears and assigns the list of mission boxes based off of the top mission
        /// and the total number of available missions
        /// </summary>
        public void AssignMissionWindows()
        {
            _liMissions.Clear();
            for (int i = _iTopMission; i < _iTopMission + MAX_SHOWN_MISSIONS && i < MissionManager.AvailableMissions.Count; i++)
            {
                MissionBox q = new MissionBox(MissionManager.AvailableMissions[i], OpenDetailBox, _gWin.MidWidth(), _gWin.MidHeight() / MAX_SHOWN_MISSIONS);

                if (_liMissions.Count == 0) { q.AnchorToInnerSide(_gWin, SideEnum.TopLeft); }
                else { q.AnchorAndAlignToObject(_liMissions[_liMissions.Count() - 1], SideEnum.Bottom, SideEnum.Left); }

                AddControl(q);

                _liMissions.Add(q);
            }
        }

        /// <summary>
        /// Opens a new window for advanced information on the indicated Mission.
        /// Inform the Mission Manager which Mission we're looking at, so it can better
        /// handle the information.
        /// </summary>
        /// <param name="m">The selected Mission</param>
        public void OpenDetailBox(Mission m)
        {
            MissionManager.SelectMission(m);
            _gDetailWindow = new DetailWindow(m, OpenWorkerWindow, AcceptMission);
            AddControl(_gDetailWindow);
            RemoveControl(_gWin);
        }

        /// <summary>
        /// Opens the WorkerWindow so that we can select from among the workers
        /// the player has.
        /// 
        /// We remove the Detail window so it won't be drawn while the WorkerWindow is open.
        /// </summary>
        public void OpenWorkerWindow()
        {
            _gWinWorkers = new WorkerWindow(WorkerAssigned);
            _gWinWorkers.CenterOnScreen();
            AddControl(_gWinWorkers);
            RemoveControl(_gDetailWindow);
        }

        /// <summary>
        /// Delegate method for the WorkerWindow to handle when a worker
        /// has been selected from it.
        /// 
        /// Close the WorkerWindow, and add the selected worker to the DetailWindow.
        /// Null out the WorkerWindow, and then check to see if we have enough workers
        /// to enable the Accept button.
        /// </summary>
        /// <param name="adv"></param>
        public void WorkerAssigned(WorldAdventurer adv)
        {
            AddControl(_gDetailWindow);
            RemoveControl(_gWinWorkers);
            _gDetailWindow.AssignToBox(adv);
            _gWinWorkers = null;

            if(MissionManager.MissionReady())
            {
                _gDetailWindow.EnableAccept();
            }
        }

        /// <summary>
        /// Delegate method called by the DetailWindow to accept the mission and return 
        /// the user to the main screen.
        /// </summary>
        public void AcceptMission()
        {
            MissionManager.AcceptMission();
            GameManager.BackToMain();
        }

        /// <summary>
        /// Moves the topmost displayed mission up one and reassigns the mission boxes
        /// </summary>
        public void BtnUpClick()
        {
            if (_iTopMission - 1 >= 0) {
                _iTopMission--;

                AssignMissionWindows();
            }
        }

        /// <summary>
        /// Moves the topmost displayed mission up one and reassigns the mission boxes
        /// </summary>
        public void BtnDownClick()
        {
            if (_iTopMission + MAX_SHOWN_MISSIONS < MissionManager.AvailableMissions.Count) {
                _iTopMission++;
            }
            AssignMissionWindows();
        }

        /// <summary>
        /// Displays the short form details of a mission.
        /// </summary>
        public class MissionBox : GUIWindow
        {
            Mission _mission;
            GUIText _gName;
            GUIMoneyDisplay _gMoney;

            List<GUIItemBox> _liItems;

            public delegate void BoxClickDelegate(Mission m);
            private BoxClickDelegate _delAction;

            public MissionBox(Mission m, BoxClickDelegate action, int width, int height) : base(GUIWindow.RedWin, width, height)
            {
                _mission = m;
                _delAction = action;

                _gName = new GUIText(m.Name);
                _liItems = new List<GUIItemBox>();
                _gMoney = new GUIMoneyDisplay(m.Money);

                _gName.AnchorToInnerSide(this, SideEnum.TopLeft);
                _gMoney.AnchorToInnerSide(this, SideEnum.Right);
                _gMoney.AlignToObject(this, SideEnum.CenterY);

                for (int i = 0; i < m.Items.Count(); i++)
                {
                    GUIItemBox box = new GUIItemBox(m.Items[i]);

                    if(i == 0) { box.AnchorAndAlignToObject(_gMoney, SideEnum.Left, SideEnum.CenterY); }
                    else { box.AnchorAndAlignToObject(_liItems[i-1], SideEnum.Left, SideEnum.Bottom); }

                    _liItems.Add(box);
                    AddControl(box);
                }
            }

            public override bool ProcessLeftButtonClick(Point mouse)
            {
                bool rv = false;

                if (this.Contains(mouse))
                {
                    rv = true;
                    _delAction(_mission);
                }

                return rv;
            }
        }

        /// <summary>
        /// Displays the actual details of the selected mission
        /// </summary>
        public class DetailWindow : GUIWindow
        {
            GUIButton _btnAccept;

            GUIText _gName;
            GUIText _gClass;
            GUIText _gDaysToFinish;
            GUIText _gDaysUntilExpiry;
            GUIText _gReqLevel;

            GUIMoneyDisplay _gMoney;

            List<GUIItemBox> _liItems;

            List<CharacterDisplayBox> _liParty;

            CharacterDisplayBox _selected;

            public delegate void BoxClickDelegate();
            private BoxClickDelegate _delOpen;

            public DetailWindow(Mission m, BoxClickDelegate open, BtnClickDelegate accept) : base(GUIWindow.RedWin, WIDTH, HEIGHT)
            {
                _liItems = new List<GUIItemBox>();
                _liParty = new List<CharacterDisplayBox>();

                _delOpen = open;

                _gName = new GUIText(m.Name);
                _gDaysToFinish = new GUIText("Requires " + m.DaysToComplete + " days");
                _gDaysUntilExpiry = new GUIText("Expires in " + (m.TotalDaysToExpire - m.DaysExpired) + " days");
                _gReqLevel = new GUIText("Required Level: " + m.ReqLevel);

                _gMoney = new GUIMoneyDisplay(m.Money);

                _gName.AnchorToInnerSide(this, SideEnum.TopLeft);
                _gReqLevel.AnchorAndAlignToObject(_gName, SideEnum.Bottom, SideEnum.Left);
                _gDaysToFinish.AnchorAndAlignToObject(_gReqLevel, SideEnum.Bottom, SideEnum.Left);

                if (m.CharClass != null)
                {
                    _gClass = new GUIText("Requires " + m.CharClass.Name);
                    _gClass.AnchorAndAlignToObject(_gDaysToFinish, SideEnum.Bottom, SideEnum.Left);
                }

                _gDaysUntilExpiry.AnchorToInnerSide(this, SideEnum.BottomLeft);
                _gMoney.AnchorToInnerSide(this, SideEnum.BottomRight);

                //Adds the GUIItemBoxes to display Mission rewards
                for (int i = 0; i < m.Items.Count(); i++)
                {
                    GUIItemBox box = new GUIItemBox(m.Items[i]);

                    if (i == 0) { box.AnchorAndAlignToObject(_gMoney, SideEnum.Left, SideEnum.Bottom); }
                    else { box.AnchorAndAlignToObject(_liItems[i - 1], SideEnum.Left, SideEnum.Bottom); }

                    _liItems.Add(box);
                    AddControl(box);
                }

                //Adds the CharacterDisplayBox to display assigned Adventurers
                for (int i = 0; i < m.PartySize; i++)
                {
                    CharacterDisplayBox box = new CharacterDisplayBox(null, null);
                    _liParty.Add(box);

                    if (i == 0) { box.AnchorAndAlignToObject(this, SideEnum.Top, SideEnum.Right); }
                    else { box.AnchorAndAlignToObject(_liParty[i - 1], SideEnum.Left, SideEnum.Top); }

                    AddControl(box);
                }

                _btnAccept = new GUIButton("Accept", accept);
                _btnAccept.AnchorAndAlignToObject(this, SideEnum.Right, SideEnum.Bottom);
                _btnAccept.Enable(false);
                AddControl(_btnAccept);

                CenterOnScreen();
            }

            /// <summary>
            /// If a CharacterDisplayBox was clicked, call the delegate to open up
            /// the worker select window. Otherwise, try to see if the button was clicked.
            /// </summary>
            public override bool ProcessLeftButtonClick(Point mouse)
            {
                bool rv = false;

                foreach (CharacterDisplayBox box in _liParty)
                {
                    if (box.Contains(mouse))
                    {
                        _selected = box;
                        if(_selected.Actor != null)
                        {
                            MissionManager.RemoveFromParty(_selected.WorldAdv);
                            _selected.AssignToBox(null);
                        }
                        _delOpen();
                        rv = true;
                        break;
                    }
                }

                //If we didn't click on a character box, checkif we clicked the button.
                if (!rv)
                {
                    rv = _btnAccept.ProcessLeftButtonClick(mouse);
                }

                return rv;
            }

            /// <summary>
            /// Right clicking on a CharacterDisplayBox will remove the WorldAdventurer
            /// from the party.
            /// </summary>
            /// <param name="mouse"></param>
            /// <returns></returns>
            public override bool ProcessRightButtonClick(Point mouse)
            {
                bool rv = false;

                foreach (CharacterDisplayBox box in _liParty)
                {
                    if (box.Contains(mouse))
                    {
                        rv = true;
                        MissionManager.RemoveFromParty(box.WorldAdv);
                        box.AssignToBox(null);
                        break;
                    }
                }

                if (!rv) { 
                    MissionManager.ClearMissionAcceptance();
                }

                return rv;
            }

            /// <summary>
            /// Assign the indicated WorldAdventurer to the CharacterDisplayBox
            /// </summary>
            public void AssignToBox(WorldAdventurer adv)
            {
                _selected.AssignToBox(adv);
                _selected = null;
            }

            /// <summary>
            /// Enables the Accept button
            /// </summary>
            public void EnableAccept()
            {
                _btnAccept.Enable(true);
            }

        }

        /// <summary>
        /// Displays all workers among all buildings.
        /// </summary>
        public class WorkerWindow : GUIWindow
        {
            List<CharacterDisplayBox> _liWorkers;

            //Delegate method for when it's time to close this window.
            public delegate void BoxClickDelegate(WorldAdventurer adv);
            private BoxClickDelegate _delClose;

            /// <summary>
            /// Constructs a new Worker window by iterating through all the buildings and workers, and adding
            /// them to the list of workers. Workers that are already Adventuring cannot appear.
            /// </summary>
            /// <param name="delClose">Delegate method for the Screen to know what to do when we're done here.</param>
            public WorkerWindow(BoxClickDelegate delClose) : base (GUIWindow.BrownWin, WIDTH,HEIGHT)
            {
                _delClose = delClose;
                _liWorkers = new List<CharacterDisplayBox>();

                //Find all the relevant workers and create a CharacterDisplayBox for them
                foreach (Building b in PlayerManager.Buildings)
                {
                    foreach(WorldAdventurer adv in b.Workers)
                    {
                        if (adv.AvailableForMissions() && adv.Combat.ClassLevel >= MissionManager.SelectedMission.ReqLevel)
                        {
                            CharacterDisplayBox box = new CharacterDisplayBox(adv, null);
                            box.WorldAdv = adv;
                            _liWorkers.Add(box);
                        }
                    }
                }

                //Organize all the CharacterDisplayBoxes.
                for (int i = 0; i < _liWorkers.Count(); i++)
                {
                    if(i == 0) { _liWorkers[i].AnchorToInnerSide(this, SideEnum.TopLeft); }
                    else { _liWorkers[i].AnchorAndAlignToObject(_liWorkers[i-1], SideEnum.Right, SideEnum.Bottom); }

                    AddControl(_liWorkers[i]);
                }
            }

            /// <summary>
            /// When we select one of the adventurers, add it to the MissionManager party
            /// and then inform the MissionScreen that we're ready to close.
            /// </summary>
            /// <param name="mouse"></param>
            /// <returns></returns>
            public override bool ProcessLeftButtonClick(Point mouse)
            {
                bool rv = false;

                foreach(CharacterDisplayBox box in _liWorkers)
                {
                    if (box.Contains(mouse)){
                        rv = true;
                        MissionManager.AddToParty(box.WorldAdv);
                        _delClose(box.WorldAdv);
                        break;
                    }
                }

                return rv;
            }
        }
    }
}
