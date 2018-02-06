using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.Characters;
using RiverHollow.Characters.CombatStuff;
using RiverHollow.Game_Managers.GUIComponents.GUIObjects;
using RiverHollow.SpriteAnimations;
using System.Collections.Generic;

namespace RiverHollow.Game_Managers.GUIObjects
{
    public class CombatScreen : GUIScreen
    {
        private const int _positions = 4;
        private GUIImage _background;
        private BattleLocation[] _arrayParty;
        private BattleLocation[] _arrayEnemies;
        private List<AbilityButton> _abilityButtonList;
        private GUITextWindow _textWindow;
        private AnimatedSprite _attack;

        public CombatScreen()
        {
            Mob m = CombatManager.CurrentMob;
            _background = new GUIImage(new Vector2(0, 0), new Rectangle(0, 0, 800, 480), RiverHollow.ScreenWidth, RiverHollow.ScreenHeight, GameContentManager.GetTexture(@"Textures\battle"));
            _arrayParty = new BattleLocation[_positions];
            _arrayEnemies = new BattleLocation[_positions];
            _abilityButtonList = new List<AbilityButton>();
            Controls.Add(_background);

            List<CombatCharacter> party = CombatManager.Party;
            for (int i = 0; i < party.Count; i++)
            {
                if (party[i] != null)
                {
                    bool even = (i % 2 == 0);
                    _arrayParty[i] = new BattleLocation(new Rectangle(100 + i * 200, even ? 700 : 600, 100, 100), party[i]);
                }

            }
            for (int i = 0; i < m.Monsters.Count; i++)
            {
                if (m.Monsters[i] != null)
                {
                    bool even = (i % 2 == 0);
                    _arrayEnemies[i] = new BattleLocation(new Rectangle(1000 + i * 200, even ? 700 : 600, 100, 100), m.Monsters[i]);
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
                        CombatManager.UsingSkill(a.BtnAbility);
                    }
                }
            }
            else if (CombatManager.CurrentPhase == CombatManager.Phase.Targetting)
            {
                foreach (BattleLocation p in _arrayEnemies)
                {
                    if (p != null && p.Occupied() && p.Contains(mouse))
                    {
                        CombatManager.UseSkillOnTarget(p);
                    }
                }
            }
            return rv;
        }

        public override bool ProcessHover(Point mouse)
        {
            bool rv = false;
            foreach (AbilityButton a in _abilityButtonList)
            {
                if (a.Contains(mouse))
                {
                    a.Hover = true;
                    _textWindow = new GUITextWindow(mouse.ToVector2(), a.BtnAbility.Description);
                    _textWindow.MoveTo(new Vector2(mouse.ToVector2().X, mouse.ToVector2().Y + 32));
                }
                else if (a.Hover)
                {
                    _textWindow = null;
                    a.Hover = false;
                }
            }
            foreach (BattleLocation p in _arrayParty)
            {
                if (p != null)
                {
                    rv = p.ProcessHover(mouse);
                }
            }
            foreach (BattleLocation p in _arrayEnemies)
            {
                if (p != null)
                {
                    rv = p.ProcessHover(mouse);
                }
            }
            return rv;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            CombatManager.Update(gameTime);
            if (CombatManager._skill == null) { _attack = null; }
            if (CombatManager.Delay > 0)
            {
                if (CombatManager.CurrentPhase == CombatManager.Phase.DisplayAttack && !string.IsNullOrEmpty(CombatManager.Text))
                {
                    if (_textWindow == null) { _textWindow = new GUITextWindow(CombatManager.Text); }
                    else { _textWindow.Update(gameTime); }
                }
                else if (CombatManager.CurrentPhase == CombatManager.Phase.Animation)
                {
                    if (_textWindow != null) { _textWindow = null; }
                    if (_attack != null)
                    {
                        _attack.Update(gameTime);
                    }
                    else
                    {
                        _attack = CombatManager._skill.Sprite;
                    }
                }
            }

            foreach (BattleLocation p in _arrayParty)
            {
                if (p != null && p.Occupied())
                {
                    p.Update(gameTime);
                }
            }
            foreach (BattleLocation p in _arrayEnemies)
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
                CombatManager.EnemyTakeTurn();
                CombatManager.UseSkillOnTarget(_arrayParty[CombatManager.PlayerTarget]);

            }
            else if (CombatManager.CurrentPhase == CombatManager.Phase.SelectSkill)
            {
                _abilityButtonList.Clear();
                int i = 0;
                foreach (Ability a in CombatManager.TurnOrder[CombatManager.TurnIndex].AbilityList)
                {
                    _abilityButtonList.Add(new AbilityButton(a, i++, 0));
                }
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
            foreach (BattleLocation p in _arrayParty)
            {
                if (p != null)
                {
                    p.Draw(spriteBatch);
                }
            }
            foreach (BattleLocation p in _arrayEnemies)
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

            if (_textWindow != null) { _textWindow.Draw(spriteBatch, true); }
            if (_attack != null)
            {
                _attack.Draw(spriteBatch, false);
            }
        }  
    }

    public class BattleLocation
    {
        private StatDisplay _healthBar;
        private Rectangle _rect;
        private CombatCharacter _character;
        public CombatCharacter Character { get => _character; }

        private SpriteFont _dmgFont;
        private int _dmg;
        private int _dmgTimer = 40;

        public BattleLocation(Rectangle r, CombatCharacter c)
        {
            _rect = r;
            _character = c;
            _character.Position = _rect.Location.ToVector2();
            _healthBar = new StatDisplay(StatDisplay.Display.Health, _character, _rect.Location.ToVector2() + new Vector2(0, 100), 100, 5);
            _dmgFont = GameContentManager.GetFont(@"Fonts\Font");
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (Occupied())
            {
                _character.Draw(spriteBatch, false);
                if (CombatManager.TurnIndex <= CombatManager.TurnOrder.Count && _character != null && CombatManager.TurnIndex < CombatManager.TurnOrder.Count && CombatManager.TurnOrder[CombatManager.TurnIndex] != _character)
                {
                    _healthBar.Draw(spriteBatch);
                }
            }
            if (_dmgTimer < 40)
            {
                spriteBatch.DrawString(_dmgFont, _dmg.ToString(), new Vector2(_rect.Center.X, _rect.Center.Y - (_dmgTimer++) / 2), Color.White);
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

        public bool ProcessHover(Point mouse)
        {
            bool rv = false;
            if (_healthBar.ProcessHover(mouse))
            {
                rv = true;
            }
            return rv;
        }
    }

    class AbilityButton
    {
        private Ability _ability;
        public Ability BtnAbility { get => _ability; }
        private GUIButton _btn;
        public bool Hover;

        public AbilityButton(Ability a, int X, int Y)
        {
            _ability = a;
            _btn = new GUIButton(new Vector2(300 + X * 100, 1000 + Y * 100), _ability.SourceRect, 100, 100, "", @"Textures\AbilityIcons");
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
