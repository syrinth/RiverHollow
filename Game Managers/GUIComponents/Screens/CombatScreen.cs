using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.Characters;
using System.Collections.Generic;

namespace RiverHollow.Game_Managers.GUIObjects
{
    public class CombatScreen : GUIScreen
    {
        private const int _positions = 4;
        private GUIImage _background;
        private Position[] _arrayParty;
        private Position[] _arrayEnemies;
        public CombatScreen()
        {
            Mob m = CombatManager.CurrentMob;
            _background = new GUIImage(new Vector2(0, 0), new Rectangle(0, 0, 800, 480), RiverHollow.ScreenWidth, RiverHollow.ScreenHeight,GameContentManager.GetTexture(@"Textures\battle"));
            _arrayParty = new Position[_positions];
            _arrayEnemies = new Position[_positions];
            Controls.Add(_background);

            List<CombatCharacter> party = CombatManager.Party;
            for(int i = 0; i < party.Count; i++)
            {
                _arrayParty[i] = new Position(new Rectangle(100, 700, 100, 100), party[i]);
            }
            for (int i = 0; i < m.Monsters.Count; i++)
            {
                _arrayEnemies[i] = new Position(new Rectangle(1000, 700, 100, 100), m.Monsters[i]);
            }
        }

        public override bool ProcessLeftButtonClick(Point mouse)
        {
            bool rv = false;
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
                    p.Update(gameTime);
                }
            }
        }
    }

    class Position
    {
        private Rectangle _rect;
        private CombatCharacter _character;
       
        public Position(Rectangle r, CombatCharacter c)
        {
            _rect = r;
            _character = c;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            _character.Draw(spriteBatch, _rect);
        }

        public void Update(GameTime gameTime)
        {
            _character.Update(gameTime);
        }

        public bool Occupied()
        {
            return _character != null;
        }
    }
}
