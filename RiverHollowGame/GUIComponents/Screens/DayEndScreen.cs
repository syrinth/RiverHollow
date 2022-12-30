using System.Collections.Generic;
using Microsoft.Xna.Framework;
using RiverHollow.Characters;
using RiverHollow.Game_Managers;
using RiverHollow.GUIComponents.GUIObjects;
using RiverHollow.GUIComponents.GUIObjects.GUIWindows;
using static RiverHollow.GUIComponents.GUIObjects.GUIObject;
using static RiverHollow.Utilities.Enums;
using static RiverHollow.Game_Managers.GameManager;
using RiverHollow.Utilities;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.Misc;

namespace RiverHollow.GUIComponents.Screens
{
    public class DayEndScreen : GUIScreen
    {
        private enum DayEndPhaseEnum { StartSave, SaveFinished, SpawnVillagers, VillagersWait, NextDay };
        DayEndPhaseEnum _eCurrentPhase = DayEndPhaseEnum.StartSave;
        const int MAX_TILES = 15;
        const double MAX_POP_TIME = 4.0;
        const double DAY_DISPLAY_PAUSE = 2;
        readonly Vector2 _pGridOffset = new Vector2(96, 112);

        double _dPopTime = 0;
        RHTimer _timer;

        GUIImage _gBackgroundImage;
        GUIText _gText;
        GUIWindow _gWindow;

        GUIButton _btnOK;
        GUIButton _btnExit;
        List<Villager> _liVillagers;
        List<Vector2> _liPoints;
        List<GUIImage> _liCoins;

        int _iCurrentVillager = 0;
        int _iTotalTaxes = 0;
        double _dTotalDisplayedTaxes = 0;

        int _iVillagerTax = 0;
        double _dVillagerTaxIncrement = 0;
        bool _bPopAll;

        public DayEndScreen()
        {
            //Stop showing the WorldMap
            GameManager.ShowMap(false);
            GameManager.CurrentScreen = GameScreenEnum.Info;
            GameManager.ShippingGremlin.SellAll();

            _liCoins = new List<GUIImage>();
            _liPoints = new List<Vector2>();

            int loops = 0;
            for (int row = 0; row < 7 * Constants.TILE_SIZE; row += (Constants.TILE_SIZE))
            {
                for (int column = (loops % 2 == 0) ? 0 : (Constants.TILE_SIZE); column < 18 * Constants.TILE_SIZE; column += (Constants.TILE_SIZE * 2))
                {
                    _liPoints.Add(new Vector2(ScaleIt((int)_pGridOffset.X + column), ScaleIt((int)_pGridOffset.Y + row)));
                }
                loops++;
            }
            _liVillagers = new List<Villager>();

            _gBackgroundImage = new GUIImage(new Rectangle(0, 0, 480, 270), DataManager.GUI_COMPONENTS + @"\Combat_Background_Forest");
            AddControl(_gBackgroundImage);

            _gWindow = new GUIWindow(GUIWindow.Window_1, ScaleIt(8), ScaleIt(8));
            _gText = new GUIText(DataManager.GetGameTextEntry("Label_Saving_Start").GetFormattedText(), false);
            _gText.AnchorToInnerSide(_gWindow, SideEnum.TopLeft);
            _gWindow.Resize();
            _gWindow.CenterOnScreen();

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
                    _timer.TickDown(gTime);
                    if (_timer.Finished())
                    {
                        foreach (Villager v in DataManager.DIVillagers.Values)
                        {
                            v.JustMovedIn();

                            if (v.LivesInTown)
                            {
                                Villager copy = new Villager(v.ID, DataManager.NPCData[v.ID]);
                                copy.Activate(false);
                                copy.BodySprite.SetScale(CurrentScale);
                                copy.PlayAnimation(VerbEnum.Idle, DirectionEnum.Down);
                                _liVillagers.Add(copy);
                            }
                        }

                        StartVillagerSpawn();
                    }

                    break;
                case DayEndPhaseEnum.SpawnVillagers:
                    SpawnVillagers(gTime);
                    goto case DayEndPhaseEnum.VillagersWait;
                case DayEndPhaseEnum.VillagersWait:
                    UpdateMoney();
                    break;
                case DayEndPhaseEnum.NextDay:
                    _gText.Update(gTime);
                    _timer.TickDown(gTime);
                    if(_timer.Finished())
                    {
                        GUIManager.BeginFadeOut();
                        GameManager.GoToHUDScreen();
                    }
                    break;
            }
        }

        private void SpawnVillagers(GameTime gTime)
        {
            _timer.TickDown(gTime);
            if (_timer.Finished())
            {
                do
                {
                    Villager v = _liVillagers[_iCurrentVillager];
                    int index = RHRandom.Instance().Next(_liPoints.Count);
                    v.Position = _liPoints[index];
                    v.Activate(true);

                    _liPoints.RemoveAt(index);

                    GUIImage coin = DataManager.GetIcon(GameIconEnum.Coin);
                    coin.Position(v.BodySprite.Position);
                    coin.ScaledMoveBy(0, -Constants.TILE_SIZE);
                    _liCoins.Add(coin);

                    _iCurrentVillager++;
                    _timer.Reset(MAX_POP_TIME / _liVillagers.Count);

                    _iVillagerTax = v.Income;
                    _dVillagerTaxIncrement = _iVillagerTax / (_timer.TimerSpeed / 0.02);

                    _iTotalTaxes += _iVillagerTax;

                    if (_iCurrentVillager == _liVillagers.Count)
                    {
                        _eCurrentPhase = DayEndPhaseEnum.VillagersWait;
                        _btnOK = new GUIButton("OK", BtnOK);
                        _btnOK.AnchorAndAlignToObject(_gWindow, SideEnum.Bottom, SideEnum.CenterX, ScaleIt(1));
                        AddControl(_btnOK);

                        _btnExit = new GUIButton("Exit Game", BtnExit);
                        _btnExit.AnchorToScreen(SideEnum.BottomLeft, ScaleIt(2));
                        AddControl(_btnExit);

                        _dTotalDisplayedTaxes = _iTotalTaxes;
                        _gText.SetText((int)_dTotalDisplayedTaxes);
                    }
                }
                while (_bPopAll && _eCurrentPhase == DayEndPhaseEnum.SpawnVillagers);
            }
        }
        private void UpdateMoney()
        {
            foreach (GUIImage c in _liCoins)
            {
                c.Alpha(c.Alpha() - (_bPopAll ? 0.005f : 0.02f));
            }

            if (_iVillagerTax > 0 && _dTotalDisplayedTaxes < _iTotalTaxes)
            {
                if (_iTotalTaxes > _dTotalDisplayedTaxes + _dVillagerTaxIncrement)
                {
                    _dTotalDisplayedTaxes += _dVillagerTaxIncrement;
                }
                else
                {
                    _dTotalDisplayedTaxes += _iTotalTaxes - _dTotalDisplayedTaxes;
                }

                _gText.SetText((int)_dTotalDisplayedTaxes);
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            switch (_eCurrentPhase)
            {
                case DayEndPhaseEnum.SpawnVillagers:
                case DayEndPhaseEnum.VillagersWait:
                    _gBackgroundImage.Draw(spriteBatch);
                    foreach (Villager v in _liVillagers)
                    {
                        v.Draw(spriteBatch);
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
                case DayEndPhaseEnum.SpawnVillagers:
                    rv = true;
                    _bPopAll = true;
                    break;
                case DayEndPhaseEnum.VillagersWait:
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

        private void StartVillagerSpawn()
        {
            _timer.Reset(0.01);
            _eCurrentPhase = DayEndPhaseEnum.SpawnVillagers;

            _gWindow = new GUIWindow(GUIWindow.Window_1, ScaleIt(76), ScaleIt(28));
            GUIImage gCoin = DataManager.GetIcon(GameIconEnum.Coin);
            _gWindow.AddControl(gCoin);

            gCoin.ScaledMoveBy(7, 6);
            _gWindow.ScaledMoveBy(200, 214);
            AddControl(_gWindow);

            _gText = new GUIText(_iTotalTaxes);
            _gText.AnchorAndAlignToObject(gCoin, SideEnum.Right, SideEnum.CenterY, ScaleIt(1));
            _gWindow.AddControl(_gText);
        }
    }

    //Monster Stuff

    //public class DayEndScreen : GUIScreen
    //{
    //    const int MAX_TILES = 15;
    //    const double MAX_POP_TIME = 1.0;

    //    GUIButton _btnOK;
    //    GUITextWindow _gResults;
    //    double _dPopTime;
    //    double _dElapsedTime;
    //    int _iRows;
    //    int _iCurrRow;
    //    int _iTotalTiles;
    //    List<GUISprite> _liMonsters;

    //    bool _bPopped;
    //    bool _bNextRow;

    //    public DayEndScreen()
    //    {
    //        GameManager.CurrentScreen = GameScreenEnum.Info;

    //        _bPopped = false;
    //        _bNextRow = true;
    //        _liMonsters = new List<GUISprite>();

    //        //Stop showing the WorldMap
    //        GameManager.ShowMap(false);

    //        //Determine how many total rows we will need
    //        foreach (GUISprite spr in GameManager.SlainMonsters) { TileCheck(spr, ref _iRows); }
    //        _iTotalTiles = 0;

    //        foreach (Villager v in DataManager.DIVillagers.Values)
    //        {
    //            v.JustMovedIn();
    //        }

    //        GameManager.ShippingGremlin.SellAll();
    //        PlayerManager.AddMoney(PlayerManager.CalculateTaxes());

    //        //string results = String.Format("Gold: {0}\nExperience: {1}", GameManager.ShippingGremlin.SellAll(), GameManager.TotalExperience);

    //        ////Give the XP to the party
    //        //foreach (ClassedCombatant c in PlayerManager.GetParty())
    //        //{
    //        //    int startLevel = c.ClassLevel;
    //        //    c.AddXP(GameManager.TotalExperience);

    //        //    if (c.ClassLevel > startLevel)
    //        //    {
    //        //        results += String.Format("\n{0} Level Up!", c.Name);
    //        //    }
    //        //}

    //        _btnOK = new GUIButton("OK", BtnOK);
    //        _btnOK.AnchorToScreen(SideEnum.Bottom, GUIManager.STANDARD_MARGIN);
    //        AddControl(_btnOK);

    //        _gResults = new GUITextWindow(new TextEntry("Day Over"));
    //        _gResults.AnchorAndAlignToObject(_btnOK, SideEnum.Top, SideEnum.CenterX, GUIManager.STANDARD_MARGIN);
    //        AddControl(_gResults);

    //        //Determine how fast to spawn each Monster image based off of how 
    //        //many there are and the total time we want it to take
    //        _dPopTime = MAX_POP_TIME / GameManager.SlainMonsters.Count;
    //    }

    //    public override void Update(GameTime gTime)
    //    {
    //        base.Update(gTime);

    //        //If we're popping the monsters, wait until the animation has played once and then
    //        //proceed to the next day

    //        if (_bPopped) {
    //            if (_liMonsters.Count == 0 || _liMonsters[0].PlayCount > 0)
    //            {
    //                GameCalendar.NextDay();
    //                RiverHollow.Rollover();
    //                SaveManager.Save();
    //                GUIManager.BeginFadeOut();
    //                PlayerManager.Stamina = PlayerManager.MaxStamina;
    //                foreach(CombatActor actor in PlayerManager.GetParty())
    //                {
    //                    actor?.IncreaseHealth(actor.MaxHP);
    //                }

    //                GameManager.GoToHUDScreen();
    //            }
    //        }
    //        else
    //        {
    //            _dElapsedTime += gTime.ElapsedGameTime.TotalSeconds;

    //            //Determine how many should spawn in this update. It's unlikely
    //            //to ever be more than 1, but it is possible.
    //            double NumberToSpawn = _dElapsedTime / _dPopTime;

    //            for (int i = 0; i < (int)NumberToSpawn; i++)
    //            {
    //                _dElapsedTime = 0;
    //                if (GameManager.SlainMonsters.Count > 0)
    //                {
    //                    GUISprite spr = GameManager.SlainMonsters[0];

    //                    //If we're going to another row, set the flag
    //                    if(TileCheck(spr, ref _iCurrRow)) { _bNextRow = true; }

    //                    spr.SetScale(GameManager.CurrentScale);

    //                    if (_bNextRow) {
    //                        _bNextRow = false;
    //                        spr.AnchorAndAlignToObject(_gResults, SideEnum.Top, SideEnum.Left, (int)((_iRows - _iCurrRow) * Constants.TILE_SIZE * GameManager.CurrentScale)); }
    //                    else {
    //                        spr.AnchorAndAlignToObject(_liMonsters[_liMonsters.Count - 1], SideEnum.Right, SideEnum.Bottom);
    //                    }

    //                    _liMonsters.Add(spr);

    //                    spr.PlayAnimation(VerbEnum.Walk, DirectionEnum.Down);

    //                    AddControl(spr);

    //                    //Remoe the Monster we just spawned from the list
    //                    GameManager.SlainMonsters.RemoveAt(0);
    //                }
    //            }
    //        }
    //    }

    //    /// <summary>
    //    /// Used to determine whether another row needs to be added based off of the width
    //    /// of the given GUISprite.
    //    /// </summary>
    //    /// <param name="spr">The sprite we are working on</param>
    //    /// <param name="toIncrement">The integer tracking the number of rows</param>
    //    /// <returns></returns>
    //    private bool TileCheck(GUISprite spr, ref int toIncrement)
    //    {
    //        bool rv = false;

    //        _iTotalTiles += (spr.Width / Constants.TILE_SIZE);
    //        if (_iTotalTiles > MAX_TILES)
    //        {
    //            rv = true;
    //            _iTotalTiles = (spr.Width / Constants.TILE_SIZE);
    //            toIncrement++;
    //        }

    //        return rv;
    //    }

    //    /// <summary>
    //    /// Button handler to trigger going to the next day
    //    /// </summary>
    //    public void BtnOK()
    //    {
    //        //Clear here in case they weren't done spawning when button was pressed
    //        GameManager.SlainMonsters.Clear();

    //        _bPopped = true;
    //        foreach (GUISprite spr in _liMonsters)
    //        {
    //            spr.PlayAnimation(AnimationEnum.KO);
    //        }

    //        _btnOK.Enable(false);
    //    }
    //}
}
