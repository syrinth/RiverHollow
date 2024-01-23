using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.Characters;
using RiverHollow.Game_Managers;
using RiverHollow.GUIComponents.GUIObjects;
using RiverHollow.GUIComponents.GUIObjects.GUIWindows;
using RiverHollow.Misc;
using RiverHollow.Utilities;
using System.Collections.Generic;
using static RiverHollow.Game_Managers.GameManager;
using static RiverHollow.GUIComponents.GUIObjects.GUIObject;
using static RiverHollow.Utilities.Enums;

namespace RiverHollow.GUIComponents.Screens
{
    public class DayEndScreen : GUIScreen
    {
        private enum DayEndPhaseEnum { StartSave, SaveFinished, SpawnNPCs, NPCsWait, NextDay };
        DayEndPhaseEnum _eCurrentPhase = DayEndPhaseEnum.StartSave;
        const double MAX_POP_TIME = 4.0;
        const double DAY_DISPLAY_PAUSE = 2;
        readonly Point _pGridOffset = new Point(96, 112);
        RHTimer _timer;

        GUIImage _gBackgroundImage;
        GUIText _gText;
        GUIWindow _gWindow;

        GUIButton _btnOK;
        GUIButton _btnExit;
        List<GUIActor> _liNPCs;
        List<Traveler> _liOldTravelers;
        List<Point> _liPoints;

        int _iTotalIncome = 0;

        int _iCurrentNPC = 0;

        bool _bPopAll;

        public DayEndScreen()
        {
            //Stop showing the WorldMap
            GameManager.ShowMap(false);
            GameManager.CurrentScreen = GameScreenEnum.Info;

            _liPoints = new List<Point>();

            int loops = 0;
            for (int row = 0; row < 7 * Constants.TILE_SIZE; row += (Constants.TILE_SIZE))
            {
                for (int column = (loops % 2 == 0) ? 0 : (Constants.TILE_SIZE); column < 18 * Constants.TILE_SIZE; column += (Constants.TILE_SIZE * 2))
                {
                    _liPoints.Add(new Point(ScaleIt(_pGridOffset.X + column), ScaleIt(_pGridOffset.Y + row)));
                }
                loops++;
            }
            _liNPCs = new List<GUIActor>();

            _gBackgroundImage = new GUIImage(DataManager.GetTexture(DataManager.GUI_COMPONENTS + @"\Combat_Background_Forest"));
            AddControl(_gBackgroundImage);

            _gWindow = new GUIWindow(GUIUtils.WINDOW_BROWN);
            _gText = new GUIText(DataManager.GetGameTextEntry("Label_Saving_Start").GetFormattedText(), false);
            _gText.AnchorToInnerSide(_gWindow, SideEnum.TopLeft);
            _gWindow.Resize();
            _gWindow.CenterOnScreen();

            _liOldTravelers = new List<Traveler>(TownManager.Travelers);

            SaveManager.StartSaveThread();
        }

        public override void Update(GameTime gTime)
        {
            base.Update(gTime);

            switch (_eCurrentPhase)
            {
                case DayEndPhaseEnum.StartSave:
                    _gText.Update(gTime);
                    if (_gText.Done && _timer == null) {_timer = new RHTimer(1); }
                    _timer?.TickDown(gTime);

                    if (!SaveManager.Saving() && _timer != null && _timer.Finished())
                    {
                        _eCurrentPhase = DayEndPhaseEnum.SaveFinished;
                        _gText.SetText(DataManager.GetGameTextEntry("Label_Saving_Finished").GetFormattedText());
                        _timer.Reset(2);
                    }
                    break;
                case DayEndPhaseEnum.SaveFinished:
                    _gText.Update(gTime);

                    if (_timer.TickDown(gTime))
                    {
                        foreach (Villager npc in TownManager.DIVillagers.Values)
                        {
                            if (npc.LivesInTown)
                            {
                                var actor = new GUIActor(npc);
                                actor.Show(false);
                                actor.PlayAnimation(VerbEnum.Idle, DirectionEnum.Down);
                                _liNPCs.Add(actor);
                            }
                        }

                        foreach (Traveler npc in _liOldTravelers)
                        {
                            var actor = new GUIActor(npc);
                            actor.Show(false);
                            actor.PlayAnimation(VerbEnum.Idle, DirectionEnum.Down);
                            _liNPCs.Add(actor);
                        }

                        StartNPCSpawn();
                    }

                    break;
                case DayEndPhaseEnum.SpawnNPCs:
                    SpawnNPCs(gTime);
                    goto case DayEndPhaseEnum.NPCsWait;
                case DayEndPhaseEnum.NPCsWait:
                    break;
                case DayEndPhaseEnum.NextDay:
                    _gText.Update(gTime);

                    if(_timer.TickDown(gTime))
                    {
                        GUIManager.BeginFadeIn();
                        TownManager.Merchant?.MoveToSpawn();
                        PlayerManager.PlayerActor.PlayAnimation(VerbEnum.Idle, DirectionEnum.Down);
                        PlayerManager.PlayerActor.SetFacing(DirectionEnum.Down);
                        GameManager.GoToHUDScreen();
                    }
                    break;
            }
        }

        private void SpawnNPCs(GameTime gTime)
        {
            if (_timer.TickDown(gTime))
            {
                do
                {
                    if (_iCurrentNPC < _liNPCs.Count)
                    {
                        GUIActor npc = _liNPCs[_iCurrentNPC];
                        int index = RHRandom.Instance().Next(_liPoints.Count);
                        npc.Position(_liPoints[index]);
                        npc.Show(true);

                        _liPoints.RemoveAt(index);

                        if (npc.Income > 0)
                        {
                            _iTotalIncome += npc.Income;
                            _gText.SetText(_iTotalIncome);

                            npc.ShowCoin();
                        }

                        _timer.Reset(MAX_POP_TIME / _liNPCs.Count);
                    }

                    //Only switch off after we have added all of the villagers
                    if (++_iCurrentNPC == _liNPCs.Count)
                    {
                        _eCurrentPhase = DayEndPhaseEnum.NPCsWait;
                        _btnOK = new GUIButton("OK", BtnOK);
                        _btnOK.AnchorAndAlignWithSpacing(_gWindow, SideEnum.Bottom, SideEnum.CenterX, 1);
                        AddControl(_btnOK);

                        _btnExit = new GUIButton("Exit Game", BtnExit);
                        _btnExit.AnchorToScreen(SideEnum.BottomLeft, 1);
                        AddControl(_btnExit);
                    }
                }
                while (_bPopAll && _eCurrentPhase == DayEndPhaseEnum.SpawnNPCs);
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            switch (_eCurrentPhase)
            {
                case DayEndPhaseEnum.SpawnNPCs:
                case DayEndPhaseEnum.NPCsWait:
                    _gBackgroundImage.Draw(spriteBatch);

                    _liNPCs.ForEach(x => x.Draw(spriteBatch));

                    _gWindow.Draw(spriteBatch);
                    _btnOK?.Draw(spriteBatch);
                    _btnExit?.Draw(spriteBatch);
                    break;
                case DayEndPhaseEnum.StartSave:
                case DayEndPhaseEnum.SaveFinished:
                case DayEndPhaseEnum.NextDay:
                    _gWindow.Draw(spriteBatch);
                    break;
            }

            _gHoverObject?.Draw(spriteBatch);
        }

        public override bool ProcessLeftButtonClick(Point mouse)
        {
            bool rv = false;
            switch (_eCurrentPhase)
            {
                case DayEndPhaseEnum.SpawnNPCs:
                    rv = true;
                    Pop();
                    break;
                case DayEndPhaseEnum.NPCsWait:
                    rv = _btnOK.ProcessLeftButtonClick(mouse) || _btnExit.ProcessLeftButtonClick(mouse);
                    break;
            }

            return rv;
        }

        public override bool ProcessRightButtonClick(Point mouse)
        {
            if (_eCurrentPhase == DayEndPhaseEnum.SpawnNPCs)
            {
                Pop();
            }

            return true;
        }

        private void Pop()
        {
            _bPopAll = true;
            _timer.Stop();
            _liNPCs.ForEach(x => x.Pop());
        }

        public override bool ProcessHover(Point mouse)
        {
            bool rv = false;

            switch (_eCurrentPhase)
            {
                case DayEndPhaseEnum.SpawnNPCs:
                case DayEndPhaseEnum.NPCsWait:
                    rv = base.ProcessHover(mouse);
                    break;
            }

            return rv;
        }

        private void BtnOK()
        {
            _timer = new RHTimer(DAY_DISPLAY_PAUSE);
            _eCurrentPhase = DayEndPhaseEnum.NextDay;
            _gWindow = new GUIWindow(GUIUtils.WINDOW_BROWN);

            TextEntry entry = DataManager.GetGameTextEntry("Day_End", GameCalendar.CurrentDay, GameCalendar.GetCurrentSeason());
            _gText = new GUIText(entry.GetFormattedText(), false);
            _gText.AnchorToInnerSide(_gWindow, SideEnum.TopLeft);
            _gWindow.Resize();

            _gWindow.CenterOnScreen();
        }

        private void BtnExit()
        {
            RiverHollow.PrepExit();
        }

        private void StartNPCSpawn()
        {
            _timer.Reset(0.01);
            _eCurrentPhase = DayEndPhaseEnum.SpawnNPCs;

            _gWindow = new GUIWindow(GUIUtils.WINDOW_BROWN, ScaleIt(76), ScaleIt(28));
            GUIImage gCoin = new GUIImage(GUIUtils.ICON_COIN);
            _gWindow.AddControl(gCoin);

            gCoin.ScaledMoveBy(7, 6);
            _gWindow.ScaledMoveBy(200, 214);
            AddControl(_gWindow);

            _gText = new GUIText(_iTotalIncome);
            _gText.AnchorAndAlignWithSpacing(gCoin, SideEnum.Right, SideEnum.CenterY, 1);
            _gWindow.AddControl(_gText);
        }
    }

    public class GUIActor : GUISprite
    {
        public int Income { get; } = -1;

        bool _bPopped = false;
        readonly int _iFoodID = -1;
        readonly int _iItemID = -1;

        readonly GUIImage _gCoin;

        public GUIActor(Villager v) : base(v.BodySprite, true) { }
        public GUIActor(Traveler t) : base(t.BodySprite, true)
        {
            Income = t.Income;
            _iFoodID = t.FoodID;
            _iItemID = t.ItemID;

            if (Income > 0)
            {
                _gCoin = new GUIImage(GUIUtils.ICON_COIN);
                _gCoin.AnchorAndAlign(this, SideEnum.Top, SideEnum.CenterX);
                _gCoin.Show(false);
                AddControl(_gCoin);
            }         
        }

        public override void Update(GameTime gTime)
        {
            if (_gCoin != null && _gCoin.Visible)
            {
                _gCoin.Alpha(_gCoin.Alpha() - (_bPopped ? 0.005f : 0.02f));
                if(_gCoin.Alpha() == 0)
                {
                    _gCoin.Show(false);
                }
            }
        }

        internal override void Show(bool val)
        {
            Visible = val;
        }

        protected override void BeginHover()
        {
            if (Income > -1)
            {
                _gCoin?.Show(false);
                var status = new TravelerStatus(_iFoodID, _iItemID);
                status.AnchorAndAlign(this, SideEnum.Top, SideEnum.CenterX);
                GUIManager.OpenHoverObject(status, DrawRectangle, true);
            }
        }

        public void ShowCoin()
        {
            if (Income > 0)
            {
                _gCoin.Show(true);
            }
        }

        public void Pop()
        {
            _bPopped = true;
        }

        private class TravelerStatus : GUIObject
        {
            const int FLICKER_TIMER = 1;
            readonly GUIItem _gFood;
            readonly GUIItem _gItem;
            readonly RHTimer _timer;

            public TravelerStatus(int foodID, int itemID)
            {
                _timer = new RHTimer(FLICKER_TIMER, true);
                if (foodID > -1)
                {
                    _gFood = new GUIItem(DataManager.GetItem(foodID), ItemBoxDraw.Never);
                    _gFood.DrawShadow(false);
                }

                if (itemID > -1)
                {
                    _gItem = new GUIItem(DataManager.GetItem(itemID), ItemBoxDraw.Never);
                    _gItem.DrawShadow(false);
                }

                AddControls(_gFood, _gItem);
                DetermineSize();
                Show(true);
            }

            public override void Update(GameTime gTime)
            {
                if (Visible && _gFood != null && _gItem != null && _timer.TickDown(gTime, true))
                {
                    _gItem?.Show(!_gItem.Visible);
                    _gFood?.Show(!_gFood.Visible);
                }
            }

            internal override void Show(bool val)
            {
                Visible = true;

                if (val)
                {
                    _timer.Reset(FLICKER_TIMER);
                    if(_gFood != null && _gItem != null)
                    {
                        _gItem?.Show(true);
                        _gFood?.Show(false);
                    }
                    else if (_gFood != null)
                    {
                        _gFood?.Show(true);
                    }
                    else if (_gItem != null)
                    {
                        _gItem?.Show(true);
                    }
                }
            }
        }
    }
}
