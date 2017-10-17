using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.Characters;
using RiverHollow.Characters.CombatStuff;
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
            if (CombatManager.CurrentPhase == CombatManager.Phase.Skill)
            {
                foreach (AbilityButton a in _abilityButtonList)
                {
                    if (a.Contains(mouse))
                    {
                        CombatManager.UsingSkill(a.btnAbility);
                    }
                }
            }
            else if (CombatManager.CurrentPhase == CombatManager.Phase.Target)
            {
                foreach (Position p in _arrayEnemies)
                {
                    if (p != null && p.Occupied() && p.Contains(mouse))
                    {
                        CombatManager.UseSkillOnTarget(p.Character);
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
                if (p != null && p.Occupied())
                {
                    p.Draw(spriteBatch);
                }
            }
            foreach (Position p in _arrayEnemies)
            {
                if (p != null && p.Occupied())
                {
                    p.Draw(spriteBatch);
                }
            }
            foreach (AbilityButton a in _abilityButtonList)
            {
                a.Draw(spriteBatch);
            }
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

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
            if (CombatManager.CurrentPhase == CombatManager.Phase.Enemy)
            {
                CombatManager.TakeTurn();
            }
            else if (CombatManager.CurrentPhase != CombatManager.Phase.End)
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

    class Position
    {
        private StatDisplay _healthBar;
        private Rectangle _rect;
        private CombatCharacter _character;
        public CombatCharacter Character { get => _character; }
       
        public Position(Rectangle r, CombatCharacter c)
        {
            _rect = r;
            _character = c;
            _healthBar = new StatDisplay(StatDisplay.Display.Health, _character, _rect.Location.ToVector2() + new Vector2(0, 150), 100, 5);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            _character.Draw(spriteBatch, _rect);
            _healthBar.Draw(spriteBatch);
        }

        public void Update(GameTime gameTime)
        {
            _character.Update(gameTime);
            _healthBar.Update(gameTime);
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
