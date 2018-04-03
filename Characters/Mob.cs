using RiverHollow.Characters;
using RiverHollow.Game_Managers;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.Misc;
using RiverHollow.Characters.CombatStuff;
using RiverHollow.SpriteAnimations;

using static RiverHollow.Game_Managers.GameManager;
namespace RiverHollow
{
    public class Mob : WorldCharacter
    {
        #region Properties
        protected int _id;
        public int ID { get => _id; }
        protected double _dIdleFor;
        protected int _iLeash = 7;

        protected string _textureName;
        protected Vector2 _moveTo = Vector2.Zero;
        protected List<CombatCharacter> _monsters;
        public List<CombatCharacter> Monsters { get => _monsters; }
        #endregion

        public Mob(int id, string[] stringData)
        {
            _characterType = CharacterEnum.Mob;
            _monsters = new List<CombatCharacter>();
            ImportBasics(stringData, id);
            _textureName = @"Textures\Monsters\Goblin Scout";
            LoadContent();
        }

        public void LoadContent()
        {
            _sprite = new AnimatedSprite(GameContentManager.GetTexture(_textureName));
            _sprite.AddAnimation("IdleDown", TileSize, TileSize, 1, 0.3f, 0, 0);
            _sprite.AddAnimation("WalkDown", TileSize, TileSize, 2, 0.3f, 116, 0);
            _sprite.AddAnimation("IdleUp", TileSize, TileSize, 1, 0.3f, 48, 0);
            _sprite.AddAnimation("WalkUp", TileSize, TileSize, 2, 0.3f, 64, 0);
            _sprite.AddAnimation("IdleLeft", TileSize, TileSize, 1, 0.3f, 96, 0);
            _sprite.AddAnimation("WalkLeft", TileSize, TileSize, 2, 0.3f, 112, 0);
            _sprite.AddAnimation("IdleRight", TileSize, TileSize, 1, 0.3f, 144, 0);
            _sprite.AddAnimation("WalkRight", TileSize, TileSize, 2, 0.3f, 160, 0);
            _sprite.SetCurrentAnimation("WalkLeft");

            _width = _sprite.Width;
            _height = _sprite.Height;
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
            UpdateMovement(theGameTime);
            base.Update(theGameTime);
        }

        public override void Draw(SpriteBatch spriteBatch, bool userLayerDepth = false)
        {
            base.Draw(spriteBatch, userLayerDepth);
        }

        private void UpdateMovement(GameTime theGameTime)
        {
            Vector2 direction = Vector2.Zero;


            if (SpottedPlayer())
            {
                _moveTo = Vector2.Zero;
                Vector2 targetPos = PlayerManager.World.Position;

                float deltaX = Math.Abs(targetPos.X - this.Position.X);
                float deltaY = Math.Abs(targetPos.Y - this.Position.Y);

                Utilities.GetMoveSpeed(Position, targetPos, Speed, ref direction);
                CheckMapForCollisionsAndMove(direction);

                DetermineFacing(direction);
            }
            else
            {
                if (CollisionBox.Intersects(PlayerManager.World.CollisionBox))
                {
                    CombatManager.NewBattle(this);
                }
                //IdleMovement(theGameTime);
            }
        }

        private void IdleMovement(GameTime theGameTime)
        {
            if (_moveTo == Vector2.Zero && _dIdleFor <= 0)
            {
                int howFar = 2;
                RHRandom r = new RHRandom();
                int decision = r.Next(1, 5);
                if (decision == 1) { _moveTo = new Vector2(Position.X - r.Next(1, howFar) * TileSize, Position.Y); }
                else if (decision == 2) { _moveTo = new Vector2(Position.X + r.Next(1, howFar) * TileSize, Position.Y); }
                else if (decision == 3) { _moveTo = new Vector2(Position.X, Position.Y - r.Next(1, howFar) * TileSize); }
                else if (decision == 4) { _moveTo = new Vector2(Position.X, Position.Y + r.Next(1, howFar) * TileSize); }
                else
                {
                    DetermineFacing(Vector2.Zero);
                    _dIdleFor = 10;
                }
            }
            else if (_moveTo != Vector2.Zero)
            {
                Vector2 direction = Vector2.Zero;
                float deltaX = Math.Abs(_moveTo.X - this.Position.X);
                float deltaY = Math.Abs(_moveTo.Y - this.Position.Y);

                Utilities.GetMoveSpeed(Position, _moveTo, Speed, ref direction);
                CheckMapForCollisionsAndMove(direction);

                if (Position.X == _moveTo.X && Position.Y == _moveTo.Y) {
                    _moveTo = Vector2.Zero;
                    DetermineFacing(_moveTo);
                }
            }
            else
            {
                _dIdleFor -= theGameTime.ElapsedGameTime.TotalSeconds;
            }
        }

        private bool SpottedPlayer()
        {
            bool rv = false;

            if(PlayerManager.PlayerInRange(Position, _iLeash * TileSize))
            {
                Vector2 playerPos = PlayerManager.World.Position;
                switch (Facing)
                {
                    case DirectionEnum.Up:
                        rv = playerPos.Y < Position.Y;
                        break;
                    case DirectionEnum.Down:
                        rv = playerPos.Y > Position.Y;
                        break;
                    case DirectionEnum.Left:
                        rv = playerPos.X < Position.X;
                        break;
                    case DirectionEnum.Right:
                        rv = playerPos.X > Position.X;
                        break;
                }
            }

            return rv;
        }
    }
}
