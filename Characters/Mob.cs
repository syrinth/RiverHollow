using RiverHollow.Characters;
using RiverHollow.Game_Managers;
using RiverHollow.Tile_Engine;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework.Graphics;
using RiverHollow.Misc;
using RiverHollow.Characters.CombatStuff;

namespace RiverHollow
{
    public class Mob : WorldCharacter
    {
        #region Properties
        protected int _id;
        public int ID { get => _id; }
        protected int _idleFor;
        protected int _leash = 400;

        protected string _textureName;
        protected Vector2 _moveTo = Vector2.Zero;
        protected List<CombatCharacter> _monsters;
        public List<CombatCharacter> Monsters { get => _monsters; }
        #endregion

        public Mob(int id, string[] stringData)
        {
            _monsters = new List<CombatCharacter>();
            ImportBasics(stringData, id);
            _textureName = @"Textures\Monsters\Goblin Scout";
            LoadContent(_textureName, 32, 64, 4, 0.3f);
        }

        protected int ImportBasics(string[] stringData, int id)
        {
            for(int i=0; i < stringData.Length; i++)
            {
                int mID = int.Parse(stringData[i]);
                if (mID > 0){
                    _monsters.Add(CharacterManager.GetMonsterByIndex(mID));
                }
            }
            _id = id;
            return 0;
        }

        public void LoadContent(int textureWidth, int textureHeight, int numFrames, float frameSpeed)
        {
            base.LoadContent(_textureName, textureWidth, textureHeight, numFrames, frameSpeed);
        }

        public override void Update(GameTime theGameTime)
        {
            UpdateMovement();
            base.Update(theGameTime);
        }
        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
        }
        private void UpdateMovement()
        {
            Vector2 direction = Vector2.Zero;
            //string animation = "";


            //if (PlayerManager.PlayerInRange(Position, _leash))
            //{
            //    _moveTo = Vector2.Zero;
            //    Vector2 targetPos = PlayerManager.World.Position;

            //    float deltaX = Math.Abs(targetPos.X - this.Position.X);
            //    float deltaY = Math.Abs(targetPos.Y - this.Position.Y);

            //    GetMoveSpeed(targetPos, ref direction);
            //    CheckMapForCollisionsAndMove(direction);

            //    DetermineAnimation(ref animation, direction, deltaX, deltaY);

            //    if (_sprite.CurrentAnimation != animation)
            //    {
            //        _sprite.CurrentAnimation = animation;
            //    }
            //}
            //else
            //{
            if (CollisionBox.Intersects(PlayerManager.World.CollisionBox))
            {
                CombatManager.NewBattle(this);
            }
            IdleMovement();
            //}
        }

        private void IdleMovement()
        {
            if (_moveTo == Vector2.Zero && _idleFor == 0)
            {
                int howFar = 2;
                RHRandom r = new RHRandom();
                int decision = r.Next(1, 5);
                if (decision == 1) { _moveTo = new Vector2(Position.X - r.Next(1, howFar) * RHMap.TileSize, Position.Y); }
                else if (decision == 2) { _moveTo = new Vector2(Position.X + r.Next(1, howFar) * RHMap.TileSize, Position.Y); }
                else if (decision == 3) { _moveTo = new Vector2(Position.X, Position.Y - r.Next(1, howFar) * RHMap.TileSize); }
                else if (decision == 4) { _moveTo = new Vector2(Position.X, Position.Y + r.Next(1, howFar) * RHMap.TileSize); }
                else
                {
                    //_sprite.CurrentAnimation = "Float" + _sprite.CurrentAnimation.Substring(4);
                    _idleFor = 300;
                }
            }
            else if (_moveTo != Vector2.Zero)
            {
                string animation = "";
                Vector2 direction = Vector2.Zero;
                float deltaX = Math.Abs(_moveTo.X - this.Position.X);
                float deltaY = Math.Abs(_moveTo.Y - this.Position.Y);

                Utilities.GetMoveSpeed(Position, _moveTo, Speed, ref direction);
                CheckMapForCollisionsAndMove(direction);

                DetermineAnimation(ref animation, direction, deltaX, deltaY);

                if (_sprite.CurrentAnimation != animation)
                {
                    _sprite.CurrentAnimation = animation;
                }


                if (Position.X == _moveTo.X && Position.Y == _moveTo.Y) { _moveTo = Vector2.Zero; }
            }
            else
            {
                _idleFor--;
            }
        }   
    }
}
