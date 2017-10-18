﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.Characters;
using RiverHollow.Characters.CombatStuff;
using RiverHollow.Game_Managers.GUIComponents.GUIObjects;
using System.Collections.Generic;

namespace RiverHollow.Game_Managers.GUIObjects
{
    public class CombatScreen : GUIScreen
    {
        private const int _positions = 4;
        private GUIImage _background;
        private Position[] _arrayParty;
        private Position[] _arrayEnemies;
        private List<AbilityButton> _abilityButtonList;
        private GUITextWindow _textWindow;

        public CombatScreen()
        {
            Mob m = CombatManager.CurrentMob;
            _background = new GUIImage(new Vector2(0, 0), new Rectangle(0, 0, 800, 480), RiverHollow.ScreenWidth, RiverHollow.ScreenHeight,GameContentManager.GetTexture(@"Textures\battle"));
            _arrayParty = new Position[_positions];
            _arrayEnemies = new Position[_positions];
            _abilityButtonList = new List<AbilityButton>();
            Controls.Add(_background);
            
            List<CombatCharacter> party = CombatManager.Party;
            for(int i = 0; i < party.Count; i++)
            {
                if (party[i] != null)
                {
                    _arrayParty[i] = new Position(new Rectangle(100, 700, 100, 100), party[i]);                    
                }
                
            }
            for (int i = 0; i < m.Monsters.Count; i++)
            {
                if (m.Monsters[i] != null)
                {
                    _arrayEnemies[i] = new Position(new Rectangle(1000 + i * 200, 700, 100, 100), m.Monsters[i]);
                }
            }
        }

        public override bool ProcessLeftButtonClick(Point mouse)
        {
            bool rv = false;
            if (CombatManager.CurrentPhase == CombatManager.Phase.SelectSkill)
            {
                foreach (AbilityButton a in _abilityButtonList)
                {
                    if (a.Contains(mouse))
                    {
                        CombatManager.UsingSkill(a.btnAbility);
                    }
                }
            }
            else if (CombatManager.CurrentPhase == CombatManager.Phase.Targetting)
            {
                foreach (Position p in _arrayEnemies)
                {
                    if (p != null && p.Occupied() && p.Contains(mouse))
                    {
                        int dmg = 0;
                        CombatManager.UseSkillOnTarget(p.Character, out dmg);
                        p.AssignDamage(dmg);
                    }
                }
            }
            return rv;
        }

        public override bool ProcessHover(Point mouse)
        {
            bool rv = false;
            //_btnAttack.IsMouseHovering = _btnAttack.Contains(mouse);
            return rv;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
            foreach(Position p in _arrayParty)
            {
                if (p != null)
                {
                    p.Draw(spriteBatch);
                }
            }
            foreach (Position p in _arrayEnemies)
            {
                if (p != null)
                {
                    p.Draw(spriteBatch);
                }
            }
            foreach (AbilityButton a in _abilityButtonList)
            {
                a.Draw(spriteBatch);
            }

            if (_textWindow != null) { _textWindow.Draw(spriteBatch); }
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            CombatManager.Update();
            if (CombatManager.Delay > 0)
            {
                if (!string.IsNullOrEmpty(CombatManager.Text))
                {
                    if(_textWindow == null) { _textWindow = new GUITextWindow(CombatManager.Text); }
                    else { _textWindow.Update(gameTime); }
                }
            }
            else
            {
                if(_textWindow != null) { _textWindow = null; }
            }

            foreach (Position p in _arrayParty)
            {
                if (p != null && p.Occupied())
                {
                    p.Update(gameTime);
                }
            }
            foreach (Position p in _arrayEnemies)
            {
                if (p != null && p.Occupied())
                {
                    if (!CombatManager.Monsters.Contains(p.Character))
                    {
                        p.Kill();
                    }
                    else
                    {
                        p.Update(gameTime);
                    }
                }
            }

            if (CombatManager.CurrentPhase == CombatManager.Phase.EnemyTurn)
            {
                CombatManager.TakeTurn(out int dmg);
                _arrayParty[0].AssignDamage(dmg);
            }
            else if (CombatManager.CurrentPhase == CombatManager.Phase.SelectSkill)
            {
                _abilityButtonList.Clear();
                int i = 0;
                foreach (Ability a in CombatManager._turnOrder[CombatManager._currentTurnIndex].AbilityList)
                {
                    _abilityButtonList.Add(new AbilityButton(a, i++, 0));
                }
            }
        }
    }

    public class Position
    {
        private StatDisplay _healthBar;
        private Rectangle _rect;
        private CombatCharacter _character;
        public CombatCharacter Character { get => _character; }

        private SpriteFont _dmgFont;
        private int _dmg;
        private int _dmgTimer = 40;

        public Position(Rectangle r, CombatCharacter c)
        {
            _rect = r;
            _character = c;
            _healthBar = new StatDisplay(StatDisplay.Display.Health, _character, _rect.Location.ToVector2() + new Vector2(0, 150), 100, 5);
            _dmgFont = GameContentManager.GetFont(@"Fonts\Font");
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (Occupied())
            {
                _character.Draw(spriteBatch, _rect);
                _healthBar.Draw(spriteBatch);
            }
            if (_dmgTimer < 40)
            {
                spriteBatch.DrawString(_dmgFont, _dmg.ToString(), new Vector2(_rect.Center.X, _rect.Center.Y - (_dmgTimer++)/2), Color.White);
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

        public bool Contains(Point mouse)
        {
            return _rect.Contains(mouse);
        }

        public void Kill()
        {
            _character = null;
        }
    }

    class AbilityButton
    {
        private Ability _ability;
        public Ability btnAbility { get => _ability; }
        private GUIButton _btn;

        public AbilityButton(Ability a, int X, int Y)
        {
            _ability = a;
            _btn = new GUIButton(new Vector2(300 + X * 100, 1000 + Y * 100), _ability.SourceRect, 100, 100, @"Textures\Abilities");
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            _btn.Draw(spriteBatch);
        }

        public void Update(GameTime gameTime)
        {
            _btn.Update(gameTime);
        }

        public bool Contains(Point mouse)
        {
            return _btn.Contains(mouse);
        }
    }
}
