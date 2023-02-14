using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.Game_Managers;
using RiverHollow.Items;
using RiverHollow.Misc;
using RiverHollow.Utilities;
using System;
using System.Collections.Generic;
using static RiverHollow.Game_Managers.GameManager;
using static RiverHollow.Utilities.Enums;

namespace RiverHollow.Characters
{
    public abstract class Mob : CombatActor
    {
        #region Properties

        public override Rectangle CollisionBox => GetCollisionBox();
        public override Rectangle HitBox => GetHitbox();

        public int Damage => DataManager.GetIntByIDKey(ID, "Damage", DataType.NPC);
        private string[] LootData => Util.FindParams(DataManager.GetStringByIDKey(ID, "ItemID", DataType.NPC));
        public override float MaxHP => DataManager.GetIntByIDKey(ID, "HP", DataType.NPC);
        public MobTypeEnum Subtype => DataManager.GetEnumByIDKey<MobTypeEnum>(ID, "Subtype", DataType.NPC);

        bool _bJump;
        int _iMaxRange = Constants.TILE_SIZE * 10;

        Point _vJumpTo;

        FieldOfVision _FoV;

        #endregion

        public Mob(int id, Dictionary<string, string> stringData) : base(id, stringData)
        {
            SpdMult = 1;
            CurrentHP = MaxHP;

            _fBaseSpeed = DataManager.GetFloatByIDKey(ID, "Speed", DataType.NPC, 1);
            Wandering = true;

            //_bJump = data.ContainsKey("Jump");

            BodySprite = LoadSpriteAnimations(Util.LoadWorldAnimations(stringData), DataManager.FOLDER_MOBS + stringData["Texture"]);

            NewFoV();
        }

        public void NewFoV()
        {
            _FoV = new FieldOfVision(this, _iMaxRange);
        }

        public override void Update(GameTime gTime)
        {
            BodySprite.Update(gTime);

            if (!GamePaused())
            {
                if (BodySprite.IsCurrentAnimation(AnimationEnum.KO) && BodySprite.PlayedOnce)
                {
                    DropLoot();
                    CurrentMap.RemoveActor(this);
                    TaskManager.AdvanceTaskProgress(this);
                    return;
                }

                if (HasKnockbackVelocity())
                {
                    ApplyKnockbackVelocity();
                }
                else if (CurrentHP > 0)
                {
                    HandleMove();
                }

                CheckDamageTimers(gTime);

                if (HitBox.Intersects(PlayerManager.PlayerActor.HitBox))
                {
                    PlayerManager.PlayerActor.DealDamage(Damage, CollisionBox);
                }
            }
        }

        public override void Draw(SpriteBatch spriteBatch, bool userLayerDepth = false)
        {
            base.Draw(spriteBatch, userLayerDepth);
            //if (_bAlert) { _sprAlert.Draw(spriteBatch, userLayerDepth); }
        }

        public bool CanAct()
        {
            return !GamePaused() && CurrentHP > 0 && !HasKnockbackVelocity();
        }

        protected Vector2 GetPlayerDirection()
        {
            return (PlayerManager.PlayerActor.Position - Position).ToVector2();
        }
        protected Vector2 GetPlayerDirectionNormal()
        {
            Vector2 rv = (PlayerManager.PlayerActor.Position - Position).ToVector2();
            if (rv != Vector2.Zero)
            {
                rv.Normalize();
            }

            return rv;
        }

        protected override void ProcessStateEnum(GameTime gTime, bool getInRange)
        {
            if (Wandering && _damageTimer == null)
            {
                switch (_eCurrentState)
                {
                    case NPCStateEnum.Alert:
                        Alert();
                        break;
                    case NPCStateEnum.Idle:
                        Idle(gTime);
                        break;
                    case NPCStateEnum.Wander:
                        Wander(gTime);
                        break;
                    case NPCStateEnum.TrackPlayer:
                        TrackPlayer(getInRange);
                        break;
                }
            }
        }

        public Rectangle GetCollisionBox()
        {
            if (DataManager.GetBoolByIDKey(ID, "CollisionBox", DataType.NPC))
            {
                Rectangle r =  DataManager.GetRectangleByIDKey(ID, "CollisionBox", DataType.NPC);
                r.Offset(BodySprite.Position);
                return r;
            }
            else
            {
                return new Rectangle(Position.X, Position.Y, Width, Constants.TILE_SIZE);
            }
        }

        public Rectangle GetHitbox()
        {
            if (DataManager.GetBoolByIDKey(ID, "HitBox", DataType.NPC))
            {
                int[] args = Util.FindIntArguments(DataManager.GetStringByIDKey(ID, "HitBox", DataType.NPC));
                return new Rectangle(BodySprite.Position.X + args[0], BodySprite.Position.Y + args[1], args[2], args[3]);
            }
            else
            {
                return CollisionBox;
            }
        }

        public override void DetermineAnimationState(Vector2 direction)
        {
            if (CurrentHP == 0)
            {
                return;
            }

            if (direction.Length() == 0)
            {
                PlayAnimationVerb(VerbEnum.Idle);
            }
            else
            {
                if (!_bJump || (_bJump && !BodySprite.CurrentAnimation.StartsWith("Air")))
                {
                    if (Math.Abs((int)direction.X) > Math.Abs((int)direction.Y))
                    {
                        if (direction.X > 0)
                        {
                            Facing = DirectionEnum.Right;
                            //animation = Util.GetEnumString(WActorWalkAnim.WalkRight); //_bJump ? Util.GetEnumString(WActorJumpAnim.GroundRight) : Util.GetEnumString(WActorWalkAnim.WalkRight);
                        }
                        else if (direction.X < 0)
                        {
                            Facing = DirectionEnum.Left;
                            //animation = Util.GetEnumString(WActorWalkAnim.WalkLeft); //_bJump ? Util.GetEnumString(WActorJumpAnim.GroundLeft) : Util.GetEnumString(WActorWalkAnim.WalkLeft);
                        }
                    }
                    else
                    {
                        if (direction.Y > 0)
                        {
                            Facing = DirectionEnum.Down;
                            //animation = Util.GetEnumString(WActorWalkAnim.WalkDown);//_bJump ? Util.GetEnumString(WActorJumpAnim.GroundDown) : Util.GetEnumString(WActorWalkAnim.WalkDown);
                        }
                        else if (direction.Y < 0)
                        {
                            Facing = DirectionEnum.Up;
                            // animation = Util.GetEnumString(WActorWalkAnim.WalkUp);// _bJump ? Util.GetEnumString(WActorJumpAnim.GroundUp) : Util.GetEnumString(WActorWalkAnim.WalkUp);
                        }
                    }

                    PlayAnimationVerb(VerbEnum.Walk);
                }
            }
        }

        /// <summary>
        /// Gets all the items that the Monsters drop. Each monster gives one item
        /// </summary>
        /// <returns>The list of items to be given to the player</returns>
        public void DropLoot()
        {
            var lootDictionary = new Dictionary<RarityEnum, List<int>>
            {
                [RarityEnum.C] = new List<int>() { -1 }
            };

            foreach (string s in LootData)
            {
                int resourceID = -1;
                RarityEnum rarity = RarityEnum.C;
                Util.GetRarity(s, ref resourceID, ref rarity);

                if (!lootDictionary.ContainsKey(rarity))
                {
                    lootDictionary[rarity] = new List<int>();
                }

                lootDictionary[rarity].Add(resourceID);
            }

            RarityEnum rarityKey = Util.RollAgainstRarity(lootDictionary);
            Item drop = DataManager.GetItem(lootDictionary[rarityKey][RHRandom.Instance().Next(0, lootDictionary[rarityKey].Count - 1)]);

            if (drop != null)
            {
                MapManager.CurrentMap.DropItemOnMap(drop, Position, false);
            }
            else
            {
                int j = 0;
            }
        }

        public void Reset()
        {
            ChangeState(NPCStateEnum.Idle);
            Position = _pLeashPoint;
            SetMoveTo(Point.Zero);
            CurrentHP = MaxHP;
        }

        public void SetInitialPoint(Point p)
        {
            _pLeashPoint = p;
        }

        public override bool DealDamage(int value, Rectangle hitbox)
        {
            bool rv = base.DealDamage(value, hitbox);

            if (rv)
            {
                BodySprite.SetColor(Color.Red);
                SetMoveTo(Point.Zero);
                _bBumpedIntoSomething = true;
            }

            return rv;
        }

        protected override WeightEnum GetWeight()
        {
            return DataManager.GetEnumByIDKey<WeightEnum>(ID, "Weight", DataType.NPC);
        }

        #region Jumping Code
        //private void UpdateMovement(GameTime theGameTime)
        //{
        //    bool move = true;
        //    Vector2 direction = Vector2.Zero;


        //    if (Math.Abs(_vLeashPoint.X - Position.X) <= _fLeashRange && Math.Abs(_vLeashPoint.Y - Position.Y) <= _fLeashRange)
        //    {
        //        _vMoveTo = Vector2.Zero;

        //        if (_bJump)
        //        {
        //            if (_vJumpTo == Vector2.Zero)
        //            {
        //                _vJumpTo = PlayerManager.PlayerActor.Position;
        //            }
        //            _vMoveTo = _vJumpTo;
        //        }
        //        else
        //        {
        //            _vMoveTo = PlayerManager.PlayerActor.Position;
        //        }
        //    }
        //    else if (!BodySprite.CurrentAnimation.StartsWith("Air"))
        //    {
        //        _bLeashed = true;
        //        _vMoveTo = _vLeashPoint;
        //        DetermineFacing(new Vector2(_vMoveTo.X - Position.X, _vMoveTo.Y - Position.Y));
        //    }

        //    if (move && _vMoveTo != Vector2.Zero)
        //    {
        //        float deltaX = Math.Abs(_vMoveTo.X - this.Position.X);
        //        float deltaY = Math.Abs(_vMoveTo.Y - this.Position.Y);

        //        Util.GetMoveSpeed(Position, _vMoveTo, BuffedSpeed, ref direction);
        //        DetermineFacing(direction);

        //        HandleMove(_vMoveTo);
        //        NewFoV();

        //        //We have finished moving
        //        if (Position.X == _vMoveTo.X && Position.Y == _vMoveTo.Y)
        //        {
        //            //If we were peashing back to our start point unset it.
        //            if (_bLeashed && _vMoveTo == _vLeashPoint)
        //            {
        //                _vLeashPoint = Vector2.Zero;
        //                _bLeashed = false;
        //            }

        //            _vMoveTo = Vector2.Zero;
        //            PlayAnimationVerb(VerbEnum.Idle);
        //            _dIdleFor = RHRandom.Instance().Next(1, 4);
        //        }
        //    }
        //}

        //if (_bJump)
        //{
        //    if (_vMoveTo != Vector2.Zero && BodySprite.CurrentAnimation.StartsWith("Idle"))
        //    {
        //        move = false;

        //        string animation = "";
        //        switch (Facing)
        //        {
        //            case DirectionEnum.Down:
        //                animation = Util.GetEnumString(WActorJumpAnim.GroundDown);
        //                break;
        //            case DirectionEnum.Up:
        //                animation = Util.GetEnumString(WActorJumpAnim.GroundUp);
        //                break;
        //            case DirectionEnum.Left:
        //                animation = Util.GetEnumString(WActorJumpAnim.GroundLeft);
        //                break;
        //            case DirectionEnum.Right:
        //                animation = Util.GetEnumString(WActorJumpAnim.GroundRight);
        //                break;
        //        }

        //        PlayAnimation(animation);
        //    }
        //    else if (BodySprite.CurrentAnimation.StartsWith("Ground") && BodySprite.CurrentFrameAnimation.PlayCount < 1)
        //    {
        //        move = false;
        //    }
        //    else if (!BodySprite.CurrentAnimation.StartsWith("Idle") && BodySprite.CurrentFrameAnimation.PlayCount >= 1)
        //    {
        //        bool jumping = BodySprite.CurrentAnimation.StartsWith("Air");
        //        string animation = "";
        //        switch (Facing)
        //        {
        //            case DirectionEnum.Down:
        //                animation = jumping ? Util.GetEnumString(WActorJumpAnim.GroundDown) : Util.GetEnumString(WActorJumpAnim.AirDown);
        //                break;
        //            case DirectionEnum.Up:
        //                animation = jumping ? Util.GetEnumString(WActorJumpAnim.GroundUp) : Util.GetEnumString(WActorJumpAnim.AirUp); ;
        //                break;
        //            case DirectionEnum.Left:
        //                animation = jumping ? Util.GetEnumString(WActorJumpAnim.GroundLeft) : Util.GetEnumString(WActorJumpAnim.AirLeft); ;
        //                break;
        //            case DirectionEnum.Right:
        //                animation = jumping ? Util.GetEnumString(WActorJumpAnim.GroundRight) : Util.GetEnumString(WActorJumpAnim.AirRight); ;
        //                break;
        //        }
        //        _vJumpTo = Vector2.Zero;
        //        PlayAnimation(animation);
        //    }
        //}

        //LoadContent(DataManager.FOLDER_MOBS + data["Texture"];);
        //public void LoadContent(string texture)
        //{
        //    _sprBody = new AnimatedSprite(texture);

        //    if (!_bJump)
        //    {
        //        _sprBody.AddAnimation(WActorBaseAnim.IdleDown, 0, 0, Constants.TILE_SIZE, Constants.TILE_SIZE * 2, 4, 0.2f);
        //        _sprBody.AddAnimation(WActorWalkAnim.WalkDown, 0, 0, Constants.TILE_SIZE, Constants.TILE_SIZE * 2, 4, 0.2f);
        //        _sprBody.AddAnimation(WActorBaseAnim.IdleUp, 64, 0, Constants.TILE_SIZE, Constants.TILE_SIZE * 2, 4, 0.2f);
        //        _sprBody.AddAnimation(WActorWalkAnim.WalkUp, 64, 0, Constants.TILE_SIZE, Constants.TILE_SIZE * 2, 4, 0.2f);
        //        _sprBody.AddAnimation(WActorBaseAnim.IdleLeft, 128, 0, Constants.TILE_SIZE, Constants.TILE_SIZE * 2, 4, 0.2f);
        //        _sprBody.AddAnimation(WActorWalkAnim.WalkLeft, 128, 0, Constants.TILE_SIZE, Constants.TILE_SIZE * 2, 4, 0.2f);
        //        _sprBody.AddAnimation(WActorBaseAnim.IdleRight, 192, 0, Constants.TILE_SIZE, Constants.TILE_SIZE * 2, 4, 0.2f);
        //        _sprBody.AddAnimation(WActorWalkAnim.WalkRight, 192, 0, Constants.TILE_SIZE, Constants.TILE_SIZE * 2, 4, 0.2f);
        //        _sprBody.PlayAnimation(WActorWalkAnim.WalkDown);
        //    }
        //    #region Jumping Code
        //    else
        //    {
        //        //_sprBody.AddAnimation(WActorBaseAnim.IdleDown, 0, 0, Constants.TILE_SIZE, Constants.TILE_SIZE * 2, 2, 0.2f);
        //        //_sprBody.AddAnimation(WActorJumpAnim.GroundDown, 0, 0, Constants.TILE_SIZE, Constants.TILE_SIZE * 2, 2, 0.2f);
        //        //_sprBody.AddAnimation(WActorJumpAnim.AirDown, 32, 0, Constants.TILE_SIZE, Constants.TILE_SIZE * 2, 2, 0.2f);

        //        //_sprBody.AddAnimation(WActorBaseAnim.IdleUp, 64, 0, Constants.TILE_SIZE, Constants.TILE_SIZE * 2, 2, 0.2f);
        //        //_sprBody.AddAnimation(WActorJumpAnim.GroundUp, 64, 0, Constants.TILE_SIZE, Constants.TILE_SIZE * 2, 2, 0.2f);
        //        //_sprBody.AddAnimation(WActorJumpAnim.AirUp, 96, 0, Constants.TILE_SIZE, Constants.TILE_SIZE * 2, 2, 0.2f);

        //        //_sprBody.AddAnimation(WActorBaseAnim.IdleLeft, 128, 0, Constants.TILE_SIZE, Constants.TILE_SIZE * 2, 2, 0.2f);
        //        //_sprBody.AddAnimation(WActorJumpAnim.GroundLeft, 128, 0, Constants.TILE_SIZE, Constants.TILE_SIZE * 2, 2, 0.2f);
        //        //_sprBody.AddAnimation(WActorJumpAnim.AirLeft, 160, 0, Constants.TILE_SIZE, Constants.TILE_SIZE * 2, 2, 0.2f);

        //        //_sprBody.AddAnimation(WActorBaseAnim.IdleRight, 192, 0, Constants.TILE_SIZE, Constants.TILE_SIZE * 2, 2, 0.2f);
        //        //_sprBody.AddAnimation(WActorJumpAnim.GroundRight, 192, 0, Constants.TILE_SIZE, Constants.TILE_SIZE * 2, 2, 0.2f);
        //        //_sprBody.AddAnimation(WActorJumpAnim.AirRight, 224, 0, Constants.TILE_SIZE, Constants.TILE_SIZE * 2, 2, 0.2f);
        //        //_sprBody.SetCurrentAnimation(WActorBaseAnim.IdleDown);
        //    }
        //    #endregion
        //    Facing = DirectionEnum.Down;

        //    Width = _sprBody.Width;
        //    Height = _sprBody.Height;

        //    //_sprAlert = new AnimatedSprite(@"Textures\Dialog");
        //    //_sprAlert.AddAnimation(GenAnimEnum.Play, 64, 64, 16, 16, 3, 0.2f, true);
        //    //_sprAlert.PlayAnimation(GenAnimEnum.Play);
        //    //_sprAlert.Position = (Position - new Vector2(0, Constants.TILE_SIZE));
        //} 
        #endregion
    }
}
