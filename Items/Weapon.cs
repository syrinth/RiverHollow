using Adventure.Characters;
using Adventure.Game_Managers;
using Adventure.Tile_Engine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure.Items
{
    public class Weapon : Item
    {
        private Vector2 rotationOrigin;

        private int _minDmg;
        private int _maxDmg;
        private int _staminaCost;
        public int StaminaCost { get => _staminaCost; }
        private int _weaponSpeed = 3;

        private Vector2 _boxdir = Vector2.Zero;
        protected Rectangle _rect;
        public override Rectangle CollisionBox { get => _rect; }

        private bool _attack;
        public bool StillAttacking { get => _attack; }

        private float _angle; //1 radian = 57.29 degrees
        private float _endAngle; //1 radian = 57.29 degrees

        public Weapon(int id, string[] itemValue)
        {
            int i = ImportBasics(itemValue, id, 1);

            _minDmg = int.Parse(itemValue[i++]);
            _maxDmg = int.Parse(itemValue[i++]);
            _staminaCost = int.Parse(itemValue[i++]);
            _texture = GameContentManager.GetTexture(@"Textures\weapons");

            CalculateSourcePos();
            rotationOrigin = new Vector2(32, 32);
            _rect = Rectangle.Empty;
        }

        public void Update(GameTime gameTime)
        {
            Vector2 loc = _rect.Location.ToVector2();
            loc += _boxdir;
            _rect.Location = loc.ToPoint();

            if (_attack)
            {
                // The time since Update was called last.
                _angle += 0.2f;
                //float circle = MathHelper.Pi * 2;
                //_angle = _angle % circle;

                if (_angle >= _endAngle)
                {
                    _angle = (float)DegreeToRadian(-45);
                    _attack = false;
                }
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (_attack)
            {
                // TODO: Add your drawing code here
                spriteBatch.Draw(_texture, PlayerManager.World.CollisionBox.Center.ToVector2(), null, Color.White, _angle, rotationOrigin, 1.0f, SpriteEffects.None, 0f);
                //spriteBatch.Draw(_texture, _rect, Color.Black); //draws collision box
            }
        }

        public int Damage()
        {
            Random r = new Random();

            return r.Next(_minDmg, _maxDmg);
        }

        private float DegreeToRadian(double angle)
        {
            return (float)(Math.PI * angle / 180.0);
        }

        public void Attack(WorldCharacter.Direction direction)
        {
            _attack = true;
            Vector2 rotateOn = PlayerManager.World.CollisionBox.Center.ToVector2();

            if (direction == WorldCharacter.Direction.North)
            {
                _angle = DegreeToRadian(-45);
                _endAngle = DegreeToRadian(135);
                _rect = new Rectangle((int)rotateOn.X - 48, (int)rotateOn.Y - 40, 32, 32);
                _boxdir = new Vector2(_weaponSpeed, 0);
            }
            else if (direction == WorldCharacter.Direction.West)
            {
                _angle = DegreeToRadian(45);
                _endAngle = DegreeToRadian(225);
                _rect = new Rectangle((int)rotateOn.X + 16, (int)rotateOn.Y - 32, 32, 32);
                _boxdir = new Vector2(0, _weaponSpeed);
            }
            else if (direction == WorldCharacter.Direction.South)
            {
                _angle = DegreeToRadian(135);
                _endAngle = DegreeToRadian(315);
                _rect = new Rectangle((int)rotateOn.X + 8, (int)rotateOn.Y + 8, 32, 32);
                _boxdir = new Vector2(-_weaponSpeed, 0);
            }
            else if (direction == WorldCharacter.Direction.East)
            {
                _angle = DegreeToRadian(225);
                _endAngle = DegreeToRadian(405);
                _rect = new Rectangle((int)rotateOn.X - 40, (int)rotateOn.Y + 16, 32, 32);
                _boxdir = new Vector2(0, -_weaponSpeed);
            }
        }

        //public void Update(GameTime gameTime)
        //{
        //    screenPos = RotateAboutOrigin(screenPos, PlayerManager.Player.Center.ToVector2(), 0.1f);
        //}

        //public Vector2 RotateAboutOrigin(Vector2 point, Vector2 origin, float rotation)
        //{
        //    return Vector2.Transform(point - origin, Matrix.CreateRotationZ(rotation)) + origin;
        //}
    }
}
