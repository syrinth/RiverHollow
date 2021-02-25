using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using RiverHollow.Characters;
using RiverHollow.Game_Managers;
using RiverHollow.GUIComponents.GUIObjects;
using RiverHollow.GUIComponents.GUIObjects.GUIWindows;
using RiverHollow.Utilities;

using static RiverHollow.GUIComponents.GUIObjects.GUIObject;

namespace RiverHollow.GUIComponents.Screens
{
    public class DayEndScreen : GUIScreen
    {
        const int MAX_TILES = 15;
        const double MAX_POP_TIME = 1.0;

        GUIButton _btnOK;
        GUITextWindow _gResults;
        double _dPopTime;
        double _dElapsedTime;
        int _iRows;
        int _iCurrRow;
        int _iTotalTiles;
        List<GUISprite> _liMonsters;
        int _iNewVillagers = 0;

        bool _bPopped;
        bool _bNextRow;

        public DayEndScreen()
        {
            _bPopped = false;
            _bNextRow = true;
            _liMonsters = new List<GUISprite>();

            //Stop showing the WorldMap
            GameManager.ShowMap(false);

            //Determine how many total rows we will need
            foreach (GUISprite spr in GameManager.SlainMonsters) { TileCheck(spr, ref _iRows); }
            _iTotalTiles = 0;

            foreach(Villager v in DataManager.DiNPC.Values)
            {
                if (v.CheckForArrival())
                {
                    _iNewVillagers++;
                }
            }

            string results = String.Format("Gold: {0}\nExperience: {1}", GameManager.ShippingGremlin.SellAll(), GameManager.TotalExperience);

            //Give the XP to the party
            foreach (ClassedCombatant c in PlayerManager.GetParty())
            {
                int startLevel = c.ClassLevel;
                c.AddXP(GameManager.TotalExperience);

                if (c.ClassLevel > startLevel)
                {
                    results += String.Format("\n{0} Level Up!", c.Name);
                }
            }

            if(_iNewVillagers == 1) { results += String.Format("\nA new villager has arrived in town."); }
            else if(_iNewVillagers > 1) { results += String.Format("\nNew villagers have arrived in town."); }

            _btnOK = new GUIButton("OK", BtnOK);
            _btnOK.AnchorToScreen( SideEnum.Bottom, GUIManager.STANDARD_MARGIN);
            AddControl(_btnOK);

            _gResults = new GUITextWindow(results, false);
            _gResults.AnchorAndAlignToObject(_btnOK, SideEnum.Top, SideEnum.CenterX, GUIManager.STANDARD_MARGIN);
            AddControl(_gResults);

            //Determine how fast to spawn each Monster image based off of how 
            //many there are and the total time we want it to take
            _dPopTime = MAX_POP_TIME / GameManager.SlainMonsters.Count;
        }

        public override void Update(GameTime gTime)
        {
            base.Update(gTime);

            //If we're popping the monsters, wait until the animation has played once and then
            //proceed to the next day
            if (_bPopped) {
                if (_liMonsters.Count == 0 || _liMonsters[0].PlayCount > 0)
                {
                    GameCalendar.NextDay();
                    RiverHollow.Rollover();
                    SaveManager.Save();
                    GUIManager.BeginFadeOut();
                    PlayerManager.Stamina = PlayerManager.MaxStamina;

                    GameManager.GoToHUDScreen();
                }
            }
            else
            {
                _dElapsedTime += gTime.ElapsedGameTime.TotalSeconds;

                //Determine how many should spawn in this update. It's unlikely
                //to ever be more than 1, but it is possible.
                double NumberToSpawn = _dElapsedTime / _dPopTime;

                for (int i = 0; i < (int)NumberToSpawn; i++)
                {
                    _dElapsedTime = 0;
                    if (GameManager.SlainMonsters.Count > 0)
                    {
                        GUISprite spr = GameManager.SlainMonsters[0];

                        //If we're going to another row, set the flag
                        if(TileCheck(spr, ref _iCurrRow)) { _bNextRow = true; }

                        spr.SetScale(GameManager.Scale);

                        if (_bNextRow) {
                            _bNextRow = false;
                            spr.AnchorAndAlignToObject(_gResults, SideEnum.Top, SideEnum.Left, (int)((_iRows - _iCurrRow) * GameManager.TileSize * GameManager.Scale)); }
                        else {
                            spr.AnchorAndAlignToObject(_liMonsters[_liMonsters.Count - 1], SideEnum.Right, SideEnum.Bottom);
                        }

                        _liMonsters.Add(spr);

                        spr.PlayAnimation(GameManager.VerbEnum.Walk, GameManager.DirectionEnum.Down);

                        AddControl(spr);

                        //Remoe the Monster we just spawned from the list
                        GameManager.SlainMonsters.RemoveAt(0);
                    }
                }
            }
        }

        /// <summary>
        /// Used to determine whether another row needs to be added based off of the width
        /// of the given GUISprite.
        /// </summary>
        /// <param name="spr">The sprite we are working on</param>
        /// <param name="toIncrement">The integer tracking the number of rows</param>
        /// <returns></returns>
        private bool TileCheck(GUISprite spr, ref int toIncrement)
        {
            bool rv = false;

            _iTotalTiles += (spr.Width / GameManager.TileSize);
            if (_iTotalTiles > MAX_TILES)
            {
                rv = true;
                _iTotalTiles = (spr.Width / GameManager.TileSize);
                toIncrement++;
            }

            return rv;
        }

        /// <summary>
        /// Button handler to trigger going to the next day
        /// </summary>
        public void BtnOK()
        {
            //Clear here in case they weren't done spawning when button was pressed
            GameManager.SlainMonsters.Clear();

            _bPopped = true;
            foreach (GUISprite spr in _liMonsters)
            {
                spr.PlayAnimation(GameManager.AnimationEnum.KO);
            }

            _btnOK.Enable(false);
        }
    }
}
