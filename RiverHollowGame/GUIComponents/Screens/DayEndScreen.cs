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
        List<TalkingActor> _liNPCs;
        List<Traveler> _liOldTravelers;
        List<Point> _liPoints;
        List<GUIImage> _liCoins;

        int _iTotalIncome = 0;

        int _iCurrentNPC = 0;

        bool _bPopAll;

        public DayEndScreen()
        {
            //Stop showing the WorldMap
            GameManager.ShowMap(false);
            GameManager.CurrentScreen = GameScreenEnum.Info;

            _liCoins = new List<GUIImage>();
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
            _liNPCs = new List<TalkingActor>();

            _gBackgroundImage = new GUIImage(new Rectangle(0, 0, 480, 270), DataManager.GUI_COMPONENTS + @"\Combat_Background_Forest");
            AddControl(_gBackgroundImage);

            _gWindow = new GUIWindow(GUIWindow.Window_1, ScaleIt(8), ScaleIt(8));
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
                                Villager copy = new Villager(npc.ID, DataManager.ActorData[npc.ID]);
                                copy.Activate(false);
                                copy.BodySprite.SetScale(CurrentScale);
                                copy.PlayAnimation(VerbEnum.Idle, DirectionEnum.Down);
                                _liNPCs.Add(copy);
                            }
                        }

                        foreach (Traveler npc in _liOldTravelers)
                        {
                            npc.Activate(false);
                            npc.BodySprite.SetScale(CurrentScale);
                            npc.PlayAnimation(npc.MoodVerb);
                            _liNPCs.Add(npc);
                        }

                        StartNPCSpawn();
                    }

                    break;
                case DayEndPhaseEnum.SpawnNPCs:
                    SpawnNPCs(gTime);
                    goto case DayEndPhaseEnum.NPCsWait;
                case DayEndPhaseEnum.NPCsWait:
                    UpdateMoney();
                    break;
                case DayEndPhaseEnum.NextDay:
                    _gText.Update(gTime);

                    if(_timer.TickDown(gTime))
                    {
                        GUIManager.BeginFadeOut();
                        TownManager.MoveMerchants();
                        PlayerManager.PlayerActor.PlayAnimation(VerbEnum.Idle, DirectionEnum.Down);
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
                        TalkingActor npc = _liNPCs[_iCurrentNPC];
                        int index = RHRandom.Instance().Next(_liPoints.Count);
                        npc.SetPosition(_liPoints[index]);
                        npc.Activate(true);

                        _liPoints.RemoveAt(index);

                        if (npc.IsActorType(ActorTypeEnum.Traveler))
                        {
                            Traveler t = (Traveler)npc;
                            t.PlayAnimation(t.MoodVerb);

                            _iTotalIncome += t.Income;
                            _gText.SetText(_iTotalIncome);

                            if (((Traveler)npc).Income > 0)
                            {
                                GUIImage coin = DataManager.GetIcon(GameIconEnum.Coin);
                                coin.Position(npc.Position);
                                coin.ScaledMoveBy(0, -Constants.TILE_SIZE);
                                _liCoins.Add(coin);
                            }
                        }

                        _timer.Reset(MAX_POP_TIME / _liNPCs.Count);
                    }

                    //Only switch off after we have added all of the villagers
                    if (++_iCurrentNPC == _liNPCs.Count)
                    {
                        _eCurrentPhase = DayEndPhaseEnum.NPCsWait;
                        _btnOK = new GUIButton("OK", BtnOK);
                        _btnOK.AnchorAndAlignToObject(_gWindow, SideEnum.Bottom, SideEnum.CenterX, ScaleIt(1));
                        AddControl(_btnOK);

                        _btnExit = new GUIButton("Exit Game", BtnExit);
                        _btnExit.AnchorToScreen(SideEnum.BottomLeft, ScaleIt(2));
                        AddControl(_btnExit);
                    }
                }
                while (_bPopAll && _eCurrentPhase == DayEndPhaseEnum.SpawnNPCs);
            }
        }

        private void UpdateMoney()
        {
            foreach (GUIImage c in _liCoins)
            {
                c.Alpha(c.Alpha() - (_bPopAll ? 0.005f : 0.02f));
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            switch (_eCurrentPhase)
            {
                case DayEndPhaseEnum.SpawnNPCs:
                case DayEndPhaseEnum.NPCsWait:
                    _gBackgroundImage.Draw(spriteBatch);
                    foreach (TalkingActor npc in _liNPCs)
                    {
                        npc.Draw(spriteBatch);
                    }

                    foreach (GUIImage c in _liCoins)
                    {
                        c.Draw(spriteBatch);
                    }

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
        }

        public override bool ProcessLeftButtonClick(Point mouse)
        {
            bool rv = false;
            switch (_eCurrentPhase)
            {
                case DayEndPhaseEnum.SpawnNPCs:
                    rv = true;
                    _bPopAll = true;
                    break;
                case DayEndPhaseEnum.NPCsWait:
                    rv = _btnOK.ProcessLeftButtonClick(mouse) || _btnExit.ProcessLeftButtonClick(mouse);
                    break;
            }

            return rv;
        }

        public override bool ProcessRightButtonClick(Point mouse)
        {
            _bPopAll = true;

            return true;
        }

        private void BtnOK()
        {
            _timer = new RHTimer(DAY_DISPLAY_PAUSE);
            _eCurrentPhase = DayEndPhaseEnum.NextDay;
            _gWindow = new GUIWindow(GUIWindow.Window_1, 8, 8);

            TextEntry entry = DataManager.GetGameTextEntry("Day_End");
            entry.FormatText(GameCalendar.CurrentDay, GameCalendar.GetCurrentSeason());
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

            _gWindow = new GUIWindow(GUIWindow.Window_1, ScaleIt(76), ScaleIt(28));
            GUIImage gCoin = DataManager.GetIcon(GameIconEnum.Coin);
            _gWindow.AddControl(gCoin);

            gCoin.ScaledMoveBy(7, 6);
            _gWindow.ScaledMoveBy(200, 214);
            AddControl(_gWindow);

            _gText = new GUIText(_iTotalIncome);
            _gText.AnchorAndAlignToObject(gCoin, SideEnum.Right, SideEnum.CenterY, ScaleIt(1));
            _gWindow.AddControl(_gText);
        }
    }
}
