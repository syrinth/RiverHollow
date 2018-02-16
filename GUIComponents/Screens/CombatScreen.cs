using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.Characters;
using RiverHollow.Characters.CombatStuff;
using RiverHollow.Game_Managers.GUIComponents.GUIObjects;
using RiverHollow.Game_Managers.GUIComponents.GUIObjects.GUIWindows;
using RiverHollow.GUIObjects;
using RiverHollow.Misc;
using RiverHollow.SpriteAnimations;
using System;
using System.Collections.Generic;

namespace RiverHollow.Game_Managers.GUIObjects
{
    public class CombatScreen : GUIScreen
    {
        const int _iPositions = 4;

        GUIImage _giBackground;
        BattleLocation[] _arrParty;
        BattleLocation[] _arrEnemies;
        GUITextWindow _gtwTextWindow;
        GUITextCombatMenuWindow _gwMenu;
        CmbtStatusWin _statusWindow;

        public CombatScreen()
        {
            Mob m = CombatManager.CurrentMob;
            _giBackground = new GUIImage(new Vector2(0, 0), new Rectangle(0, 0, 800, 480), RiverHollow.ScreenWidth, RiverHollow.ScreenHeight, GameContentManager.GetTexture(@"Textures\battle"));
            _arrParty = new BattleLocation[_iPositions];
            _arrEnemies = new BattleLocation[_iPositions];
            
            Controls.Add(_giBackground);

            int totalMenuWidth = RiverHollow.ScreenWidth / 3;
            int menuSec = totalMenuWidth / 3;
            _gwMenu = new GUITextCombatMenuWindow(totalMenuWidth, menuSec);
            _statusWindow = new CmbtStatusWin(_gwMenu.Position + new Vector2(_gwMenu.Width + GUIWindow.GreyDialogEdge*2, 0), menuSec*2, _gwMenu.Height);
            Controls.Add(_gwMenu);
            Controls.Add(_statusWindow);
            //Get the Players' party and assign each of them a battle position
            List<CombatCharacter> party = CombatManager.Party;
            for (int i = 0; i < party.Count; i++)
            {
                if (party[i] != null)
                {
                    bool even = (i % 2 == 0);
                    _arrParty[i] = new BattleLocation(new Vector2(100 + i * 200, even ? 700 : 600), party[i]);
                }

            }
            //Get the Enemies and assign each of them a battle position
            for (int i = 0; i < m.Monsters.Count; i++)
            {
                if (m.Monsters[i] != null)
                {
                    bool even = (i % 2 == 0);
                    _arrEnemies[i] = new BattleLocation(new Vector2(1000 + i * 200, even ? 700 : 600), m.Monsters[i]);
                }
            }
        }

        public override bool ProcessLeftButtonClick(Point mouse)
        {
            bool rv = false;

            //If the current Phase is skill selection, allow the user to pick a skill for the currentCharacter
            switch (CombatManager.CurrentPhase)
            {
                case CombatManager.Phase.SelectSkill:
                    {
                        rv = _gwMenu.ProcessLeftButtonClick(mouse);

                        if (_gwMenu.ChosenAbility != null) {
                            CombatManager.ProcessChosenSkill(_gwMenu.ChosenAbility);
                            _gwMenu.ClearChosenAbility();
                        }

                        break;
                    }
                case CombatManager.Phase.ChooseSkillTarget:
                    {
                        foreach (BattleLocation p in _arrEnemies)
                        {
                            if (p != null && p.Occupied() && p.Contains(mouse)) { CombatManager.SetSkillTarget(p); }
                        }

                        break;
                    }
            }
            
            return rv;
        }

        //Right clicking will deselectthe chosen skill
        public override bool ProcessRightButtonClick(Point mouse)
        {
            bool rv = false;

            //If the current Phase is skill selection, allow the user to pick a skill for the currentCharacter
            if (CombatManager.CurrentPhase == CombatManager.Phase.ChooseSkillTarget)
            {
                CombatManager.ChosenSkill = null;
                CombatManager.CurrentPhase = CombatManager.Phase.SelectSkill;
            }

            return rv;
        }

        public override bool ProcessHover(Point mouse)
        {
            bool rv = false;
            
            foreach (BattleLocation p in _arrParty)
            {
                if (p != null) { rv = p.ProcessHover(mouse); }
            }
            foreach (BattleLocation p in _arrEnemies)
            {
                if (p != null) { rv = p.ProcessHover(mouse); }
            }

            return rv;
        }

        //First, call the update for the CombatManager
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            CombatManager.Update(gameTime);

            switch (CombatManager.CurrentPhase)
            {
                case CombatManager.Phase.DisplayAttack:
                    if (!string.IsNullOrEmpty(CombatManager.Text))
                    {
                        if (_gtwTextWindow == null) {
                            _gtwTextWindow = new GUITextWindow(CombatManager.Text, 0.5);
                        }
                        else {
                            _gtwTextWindow.Update(gameTime);
                            if (_gtwTextWindow.Duration <= 0)
                            {
                                _gtwTextWindow = null;
                                CombatManager.CurrentPhase = CombatManager.Phase.UseSkill;
                            }
                        }
                    }

                    break;
                case CombatManager.Phase.UseSkill:
                    CombatManager.ChosenSkill.HandlePhase(gameTime);
                    break;

                case CombatManager.Phase.SelectSkill:
                    _gwMenu.Assign(CombatManager.ActiveCharacter.AbilityList);
                    _gwMenu.Update(gameTime);

                    if (_gwMenu.ChosenAbility != null) {
                        CombatManager.ProcessChosenSkill(_gwMenu.ChosenAbility);
                        _gwMenu.ClearChosenAbility();
                    }
                    break;
            }

            //Update everyone in the party's battleLocation
            foreach (BattleLocation p in _arrParty)
            {
                if (p != null && p.Occupied()) { p.Update(gameTime); }
            }

            //Update everyone in the enemys's battleLocation
            foreach (BattleLocation p in _arrEnemies)
            {
                if (p != null && p.Occupied())
                {
                    if (!CombatManager.Monsters.Contains(p.Character)) { p.Kill(); }
                    else { p.Update(gameTime); }
                }
            }

            //If it's the enemy's turn, set the skill target to the player target
            if (CombatManager.CurrentPhase == CombatManager.Phase.EnemyTurn) {
                CombatManager.SetSkillTarget(_arrParty[CombatManager.PlayerTarget]);
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
            _statusWindow.Draw(spriteBatch, _arrParty);
            foreach (BattleLocation p in _arrParty)
            {
                if (p != null) { p.Draw(spriteBatch); }
            }
            foreach (BattleLocation p in _arrEnemies)
            {
                if (p != null) { p.Draw(spriteBatch); }
            }

            if (_gtwTextWindow != null) { _gtwTextWindow.Draw(spriteBatch, true); }
            if (CombatManager.ChosenSkill != null && CombatManager.ChosenSkill.Sprite.IsAnimating) { CombatManager.ChosenSkill.Sprite.Draw(spriteBatch, false); }
        }  
    }

    public class BattleLocation
    {
        StatDisplay _healthBar;
        CombatCharacter _character;
        public CombatCharacter Character { get => _character; }

        private SpriteFont _dmgFont;
        private int _dmg;
        private int _dmgTimer = 40;
        private Vector2 _center;

        public BattleLocation(Vector2 vec, CombatCharacter c)
        {
            _character = c;
            _character.Position = vec;
            _center = _character.Center;
            _healthBar = new StatDisplay(StatDisplay.Display.Health, _character, _character.Position + new Vector2(0, _character.SpriteHeight), 100, 5);
            _dmgFont = GameContentManager.GetFont(@"Fonts\Font");
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (Occupied()) { _character.Draw(spriteBatch, false); }

            if (_dmgTimer < 40)
            {
                spriteBatch.DrawString(_dmgFont, _dmg.ToString(), new Vector2(_center.X, _center.Y - (_dmgTimer++) / 2), Color.White);
            }
        }

        public void Update(GameTime gameTime)
        {
            _character.Update(gameTime);
            _healthBar.Update(gameTime);
        }

        public void AssignDamage(int x)
        {
            _dmg = x;
            _dmgTimer = 0;
        }

        public bool Occupied()
        {
            return _character != null;
        }

        public bool Contains(Point mouse) { return _character.Contains(mouse); }

        public void Kill() { _character = null; }

        public bool ProcessHover(Point mouse)
        {
            bool rv = false;
            if (_healthBar.ProcessHover(mouse))
            {
                rv = true;
            }
            return rv;
        }

        public Vector2 GetAttackVec(Vector2 from, Vector2 widthHeight)
        {
            Vector2 rv = _character.Position;
            int xOffset = _character.Width + 1;
            rv.X += _character.Center.X < from.X ? xOffset : -xOffset;
            rv.Y += _character.Height - widthHeight.Y;

            return rv;
        }
    }

    public class CmbtStatusWin : GUIWindow
    {
        GUIImage _giCurrentTurn;
        SpriteFont _fFont;
        float _fCharacterHeight;

        public CmbtStatusWin(Vector2 position, int width, int height)
        {
            _fFont = GameContentManager.GetFont(@"Fonts\MenuFont");
            _fCharacterHeight = _fFont.MeasureString("Q").Y;
            _width = width;
            _height = height;
            _edgeSize = GreyDialogEdge;
            _sourcePoint = GreyDialog;

            Position = position;

            _giCurrentTurn = new GUIImage(new Vector2((int)_position.X + _innerBorder, (int)_position.Y + _innerBorder), new Rectangle(288, 96, 32, 32), (int)_fCharacterHeight, (int)_fCharacterHeight, @"Textures\Dialog");
        }

        public void Draw(SpriteBatch spriteBatch, BattleLocation[] locations)
        {
            int xindex = (int)_position.X + _innerBorder;
            int yIndex = (int)_position.Y + _innerBorder;

            foreach (BattleLocation bl in locations)
            {
                if (bl != null)
                {
                    Color c = (CombatManager.ActiveCharacter == bl.Character) ? Color.Green : Color.White;
                    spriteBatch.DrawString(_fFont, bl.Character.Name, new Vector2(xindex, yIndex), c);
                    string strHp = string.Format("{0}/{1}", bl.Character.CurrentHP, bl.Character.MaxHP);
                    spriteBatch.DrawString(_fFont, strHp, new Vector2(Position.X + _width - _fFont.MeasureString(strHp).X - _innerBorder, yIndex), c);
                }
                yIndex += (int)_fCharacterHeight;
            }
        }
    }
}
