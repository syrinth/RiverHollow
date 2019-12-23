using Microsoft.Xna.Framework;
using RiverHollow.Actors;
using RiverHollow.Actors.CombatStuff;
using RiverHollow.Game_Managers;
using RiverHollow.Misc;
using RiverHollow.SpriteAnimations;
using RiverHollow.WorldObjects;
using System;
using System.Collections.Generic;
using static RiverHollow.Game_Managers.GameManager;

namespace RiverHollow.Characters
{
    public class Monster : CombatActor
    {
        #region Properties
        int _id;
        public int ID => _id;
        int _iRating;
        int _iXP;
        public int XP => _iXP;
        protected Vector2 _moveTo = Vector2.Zero;
        List<int> _liLootIDs;

        public override int Attack => 20 + (_iRating * 10);

        public override int MaxHP => (int)((((Math.Pow(_iRating, 2)) * 10) + 20) * Math.Pow(Math.Max(1, (double)_iRating / 14), 2));

        protected double _dIdleFor;

        bool _bJump;
        Vector2 _vJumpTo;

        int _iMoveFailures = 0;

        List<SpawnConditionEnum> _liSpawnConditions;
        List<CombatAction> _liCombatActions;

        #endregion

        public Monster(int id, Dictionary<string, string> data)
        {
            _liLootIDs = new List<int>();
            _liCombatActions = new List<CombatAction>();
            _liSpawnConditions = new List<SpawnConditionEnum>();

            _eActorType = ActorEnum.Monster;
            ImportBasics(data, id);
        }
        protected void ImportBasics(Dictionary<string, string> data, int id)
        {
            _id = id;
            _sName = GameContentManager.GetMonsterInfo(_id);
            _sTexture = GameContentManager.FOLDER_MONSTERS + data["Texture"];

            float[] idle = new float[2] { 2, 0.5f };
            float[] attack = new float[2] { 2, 0.2f };
            float[] hurt = new float[2] { 1, 0.5f };
            float[] cast = new float[2] { 2, 0.5f };

            //_iWidth = int.Parse(data["Width"]);
            //_iHeight = int.Parse(data["Height"]);

            _iRating = int.Parse(data["Lvl"]);
            _iXP = _iRating * 10;
            _iStrength = 1 + _iRating;
            _iDefense = 8 + (_iRating * 3);
            _iVitality = 2 * _iRating + 10;
            _iMagic = 0; // 2 * _iRating + 2;
            _iResistance = 2 * _iRating + 10;
            _iSpeed = 10;

            string[] split;
            if (data.ContainsKey("Condition"))
            {
                split = data["Condition"].Split('-');
                for (int i = 0; i < split.Length; i++)
                {
                    _liSpawnConditions.Add(Util.ParseEnum<SpawnConditionEnum>(split[i]));
                }
            }

            if (data.ContainsKey("Loot"))
            {
                split = data["Loot"].Split(' ');
                for (int i = 0; i < split.Length; i++)
                {
                    _liLootIDs.Add(int.Parse(split[i]));
                }
            }

            _bJump = data.ContainsKey("Jump");

            foreach (string ability in data["Ability"].Split('-'))
            {
                _liCombatActions.Add((CombatAction)ObjectManager.GetActionByIndex(int.Parse(ability)));
            }

            if (data.ContainsKey("Trait"))
            {
                HandleTrait(GameContentManager.GetMonsterTraitData(data["Trait"]));
            }

            if (data.ContainsKey("Resist"))
            {
                foreach (string elem in data["Resist"].Split('-'))
                {
                    _diElementalAlignment[Util.ParseEnum<ElementEnum>(elem)] = ElementAlignment.Resists;
                }
            }

            if (data.ContainsKey("Vuln"))
            {
                foreach (string elem in data["Vuln"].Split('-'))
                {
                    _diElementalAlignment[Util.ParseEnum<ElementEnum>(elem)] = ElementAlignment.Vulnerable;
                }
            }

            if (data.ContainsKey("Idle"))
            {
                split = data["Idle"].Split('-');
                idle[0] = float.Parse(split[0]);
                idle[1] = float.Parse(split[1]);
            }

            if (data.ContainsKey("Attack"))
            {
                split = data["Attack"].Split('-');
                attack[0] = float.Parse(split[0]);
                attack[1] = float.Parse(split[1]);
            }

            if (data.ContainsKey("Hurt"))
            {
                split = data["Hurt"].Split('-');
                hurt[0] = float.Parse(split[0]);
                hurt[1] = float.Parse(split[1]);
            }

            if (data.ContainsKey("Cast"))
            {
                split = data["Cast"].Split('-');
                cast[0] = float.Parse(split[0]);
                cast[1] = float.Parse(split[1]);
            }

            LoadContent();
            LoadCombatContent(idle, attack, hurt, cast);

            _iCurrentHP = MaxHP;
            _iCurrentMP = MaxMP;
        }

        public override void Update(GameTime gTime)
        {
            base.Update(gTime);

            ///When the Monster has finished playing the KO animation, let the CombatManager know so it can do any final actions
            if (BodySprite.CurrentAnimation == Util.GetEnumString(CActorAnimEnum.KO) && BodySprite.CurrentFrameAnimation.PlayCount == 1)
            {
                CombatManager.MonsterKOAnimFinished(this);
            }

            // Only comment this out for now in case we implement stealth or something so you can sneak past enemies without aggroing them
            //if (!CombatManager.InCombat) {
            //    UpdateMovement(gTime);
            //}
        }

        public void LoadContent()
        {
            int xCrawl = 0;
            _spriteBody = new AnimatedSprite(_sTexture);

            if (!_bJump)
            {
                _spriteBody.AddAnimation(WActorBaseAnim.IdleDown, xCrawl, 0, TileSize, TileSize * 2, 4, 0.2f);
                _spriteBody.AddAnimation(WActorWalkAnim.WalkDown, xCrawl, 0, TileSize, TileSize * 2, 4, 0.2f);
                xCrawl += 64;
                _spriteBody.AddAnimation(WActorBaseAnim.IdleUp, xCrawl, 0, TileSize, TileSize * 2, 4, 0.2f);
                _spriteBody.AddAnimation(WActorWalkAnim.WalkUp, xCrawl, 0, TileSize, TileSize * 2, 4, 0.2f);
                xCrawl += 64;
                _spriteBody.AddAnimation(WActorBaseAnim.IdleLeft, xCrawl, 0, TileSize, TileSize * 2, 4, 0.2f);
                _spriteBody.AddAnimation(WActorWalkAnim.WalkLeft, xCrawl, 0, TileSize, TileSize * 2, 4, 0.2f);
                xCrawl += 64;
                _spriteBody.AddAnimation(WActorBaseAnim.IdleRight, xCrawl, 0, TileSize, TileSize * 2, 4, 0.2f);
                _spriteBody.AddAnimation(WActorWalkAnim.WalkRight, xCrawl, 0, TileSize, TileSize * 2, 4, 0.2f);
                _spriteBody.SetCurrentAnimation(WActorWalkAnim.WalkDown);
            }
            else
            {
                _spriteBody.AddAnimation(WActorBaseAnim.IdleDown, xCrawl, 0, TileSize, TileSize * 2, 2, 0.2f);
                _spriteBody.AddAnimation(WActorJumpAnim.GroundDown, xCrawl, 0, TileSize, TileSize * 2, 2, 0.2f);
                _spriteBody.AddAnimation(WActorJumpAnim.AirDown, xCrawl + 32, 0, TileSize, TileSize * 2, 2, 0.2f);
                xCrawl += 64;
                _spriteBody.AddAnimation(WActorBaseAnim.IdleUp, xCrawl, 0, TileSize, TileSize * 2, 2, 0.2f);
                _spriteBody.AddAnimation(WActorJumpAnim.GroundUp, xCrawl, 0, TileSize, TileSize * 2, 2, 0.2f);
                _spriteBody.AddAnimation(WActorJumpAnim.AirUp, xCrawl + 32, 0, TileSize, TileSize * 2, 2, 0.2f);
                xCrawl += 64;
                _spriteBody.AddAnimation(WActorBaseAnim.IdleLeft, xCrawl, 0, TileSize, TileSize * 2, 2, 0.2f);
                _spriteBody.AddAnimation(WActorJumpAnim.GroundLeft, xCrawl, 0, TileSize, TileSize * 2, 2, 0.2f);
                _spriteBody.AddAnimation(WActorJumpAnim.AirLeft, xCrawl + 32, 0, TileSize, TileSize * 2, 2, 0.2f);
                xCrawl += 64;
                _spriteBody.AddAnimation(WActorBaseAnim.IdleRight, xCrawl, 0, TileSize, TileSize * 2, 2, 0.2f);
                _spriteBody.AddAnimation(WActorJumpAnim.GroundRight, xCrawl, 0, TileSize, TileSize * 2, 2, 0.2f);
                _spriteBody.AddAnimation(WActorJumpAnim.AirRight, xCrawl + 32, 0, TileSize, TileSize * 2, 2, 0.2f);
                _spriteBody.SetCurrentAnimation(WActorBaseAnim.IdleDown);
            }
            Facing = DirectionEnum.Down;

            xCrawl += 64;
            _spriteBody.AddAnimation(CActorAnimEnum.Hurt, 0, 0, 0, 0 * 2, 1, 0.2f);
            _spriteBody.AddAnimation(CActorAnimEnum.Attack, 0, 0, 0, 0 * 2, 1, 0.2f);
            _spriteBody.AddAnimation(CActorAnimEnum.KO, xCrawl, 0, TileSize, TileSize * 2, 3, 0.2f);

            base._iWidth = _spriteBody.Width;
            base._iHeight = _spriteBody.Height;
        }

        public void LoadCombatContent(float[] idle, float[] attack, float[] hurt, float[] cast)
        {
            //_spriteBody.AddAnimation(CActorAnimEnum.KO, (xCrawl * frameWidth), 0, frameWidth, frameHeight, 3, 0.2f);

            //int xCrawl = 0;
            //int frameWidth = _iWidth;
            //int frameHeight = _iHeight;
            //_spriteBody.AddAnimation(CActorAnimEnum.Idle, (xCrawl * frameWidth), 0, frameWidth, frameHeight, (int)idle[0], idle[1]);
            //xCrawl += (int)idle[0];
            //_spriteBody.AddAnimation(CActorAnimEnum.Attack, (xCrawl * frameWidth), 0, frameWidth, frameHeight, (int)attack[0], attack[1]);
            //xCrawl += (int)attack[0];
            //_spriteBody.AddAnimation(CActorAnimEnum.Hurt, (xCrawl * frameWidth), 0, frameWidth, frameHeight, (int)hurt[0], hurt[1]);
            //xCrawl += (int)hurt[0];
            //_spriteBody.AddAnimation(CActorAnimEnum.Cast, (xCrawl * frameWidth), 0, frameWidth, frameHeight, (int)cast[0], cast[1]);
            //xCrawl += (int)cast[0];

            //_spriteBody.AddAnimation(CActorAnimEnum.KO, (xCrawl * frameWidth), 0, frameWidth, frameHeight, 3, 0.2f);
            //base._iWidth = _spriteBody.Width;
            //base._iHeight = _spriteBody.Height;
        }

        private void HandleTrait(string traitData)
        {
            string[] traits = Util.FindTags(traitData);
            foreach (string s in traits)
            {
                string[] tagType = s.Split(':');
                if (tagType[0].Equals(Util.GetEnumString(StatEnum.Str)))
                {
                    ApplyTrait(ref _iStrength, tagType[1]);
                }
                else if (tagType[0].Equals(Util.GetEnumString(StatEnum.Def)))
                {
                    ApplyTrait(ref _iDefense, tagType[1]);
                }
                else if (tagType[0].Equals(Util.GetEnumString(StatEnum.Vit)))
                {
                    ApplyTrait(ref _iVitality, tagType[1]);
                }
                else if (tagType[0].Equals(Util.GetEnumString(StatEnum.Mag)))
                {
                    ApplyTrait(ref _iMagic, tagType[1]);
                }
                else if (tagType[0].Equals(Util.GetEnumString(StatEnum.Res)))
                {
                    ApplyTrait(ref _iResistance, tagType[1]);
                }
                else if (tagType[0].Equals(Util.GetEnumString(StatEnum.Spd)))
                {
                    ApplyTrait(ref _iSpeed, tagType[1]);
                }
            }
        }

        private void ApplyTrait(ref int value, string data)
        {
            if (data.Equals("+"))
            {
                value = (int)(value * 1.1);
            }
            else if (data.Equals("-"))
            {
                value = (int)(value * 0.9);
            }
        }

        private void UpdateMovement(GameTime gTime)
        {
            bool move = true;
            Vector2 direction = Vector2.Zero;

            GetIdleMovementTarget(gTime);

            if (_bJump)
            {
                if (_vMoveTo != Vector2.Zero && BodySprite.CurrentAnimation.StartsWith("Idle"))
                {
                    move = false;

                    string animation = "";
                    switch (Facing)
                    {
                        case DirectionEnum.Down:
                            animation = Util.GetEnumString(WActorJumpAnim.GroundDown);
                            break;
                        case DirectionEnum.Up:
                            animation = Util.GetEnumString(WActorJumpAnim.GroundUp);
                            break;
                        case DirectionEnum.Left:
                            animation = Util.GetEnumString(WActorJumpAnim.GroundLeft);
                            break;
                        case DirectionEnum.Right:
                            animation = Util.GetEnumString(WActorJumpAnim.GroundRight);
                            break;
                    }

                    PlayAnimation(animation);
                }
                else if (BodySprite.CurrentAnimation.StartsWith("Ground") && BodySprite.CurrentFrameAnimation.PlayCount < 1)
                {
                    move = false;
                }
                else if (!BodySprite.CurrentAnimation.StartsWith("Idle") && BodySprite.CurrentFrameAnimation.PlayCount >= 1)
                {
                    bool jumping = BodySprite.CurrentAnimation.StartsWith("Air");
                    string animation = "";
                    switch (Facing)
                    {
                        case DirectionEnum.Down:
                            animation = jumping ? Util.GetEnumString(WActorJumpAnim.GroundDown) : Util.GetEnumString(WActorJumpAnim.AirDown);
                            break;
                        case DirectionEnum.Up:
                            animation = jumping ? Util.GetEnumString(WActorJumpAnim.GroundUp) : Util.GetEnumString(WActorJumpAnim.AirUp); ;
                            break;
                        case DirectionEnum.Left:
                            animation = jumping ? Util.GetEnumString(WActorJumpAnim.GroundLeft) : Util.GetEnumString(WActorJumpAnim.AirLeft); ;
                            break;
                        case DirectionEnum.Right:
                            animation = jumping ? Util.GetEnumString(WActorJumpAnim.GroundRight) : Util.GetEnumString(WActorJumpAnim.AirRight); ;
                            break;
                    }
                    _vJumpTo = Vector2.Zero;
                    PlayAnimation(animation);
                }
            }

            if (move && _vMoveTo != Vector2.Zero)
            {
                float deltaX = Math.Abs(_vMoveTo.X - this.Position.X);
                float deltaY = Math.Abs(_vMoveTo.Y - this.Position.Y);

                Util.GetMoveSpeed(Position, _vMoveTo, Speed, ref direction);
                DetermineFacing(direction);
                if (!CheckMapForCollisionsAndMove(direction, false))
                {
                    _iMoveFailures++;
                }

                //We have finished moving
                if (Position.X == _vMoveTo.X && Position.Y == _vMoveTo.Y)
                {
                    _vMoveTo = Vector2.Zero;
                    Idle();
                }
            }
        }

        private void GetIdleMovementTarget(GameTime gTime)
        {
            if ((_vMoveTo == Vector2.Zero && _dIdleFor <= 0 && !BodySprite.CurrentAnimation.StartsWith("Air")) || _iMoveFailures == 5)
            {
                _iMoveFailures = 0;
                int howFar = 2;
                bool skip = false;
                RHRandom rand = RHRandom.Instance;
                int decision = rand.Next(1, 5);
                if (decision == 1) { _vMoveTo = new Vector2(Position.X - rand.Next(1, howFar) * TileSize, Position.Y); }
                else if (decision == 2) { _vMoveTo = new Vector2(Position.X + rand.Next(1, howFar) * TileSize, Position.Y); }
                else if (decision == 3) { _vMoveTo = new Vector2(Position.X, Position.Y - rand.Next(1, howFar) * TileSize); }
                else if (decision == 4) { _vMoveTo = new Vector2(Position.X, Position.Y + rand.Next(1, howFar) * TileSize); }
                else
                {
                    _vMoveTo = Vector2.Zero;
                    _dIdleFor = 4;
                    Idle();
                    skip = true;
                }

                if (!skip)
                {
                    DetermineFacing(new Vector2(_vMoveTo.X - Position.X, _vMoveTo.Y - Position.Y));
                }
            }
            else if (_vMoveTo == Vector2.Zero)
            {
                _dIdleFor -= gTime.ElapsedGameTime.TotalSeconds;
            }
        }

        public override void DetermineFacing(Vector2 direction)
        {
            string animation = string.Empty;

            if (direction.Length() == 0)
            {
                Idle();
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
                            animation = _bJump ? Util.GetEnumString(WActorJumpAnim.GroundRight) : Util.GetEnumString(WActorWalkAnim.WalkRight);
                        }
                        else if (direction.X < 0)
                        {
                            Facing = DirectionEnum.Left;
                            animation = _bJump ? Util.GetEnumString(WActorJumpAnim.GroundLeft) : Util.GetEnumString(WActorWalkAnim.WalkLeft);
                        }
                    }
                    else
                    {
                        if (direction.Y > 0)
                        {
                            Facing = DirectionEnum.Down;
                            animation = _bJump ? Util.GetEnumString(WActorJumpAnim.GroundDown) : Util.GetEnumString(WActorWalkAnim.WalkDown);
                        }
                        else if (direction.Y < 0)
                        {
                            Facing = DirectionEnum.Up;
                            animation = _bJump ? Util.GetEnumString(WActorJumpAnim.GroundUp) : Util.GetEnumString(WActorWalkAnim.WalkUp);
                        }
                    }

                    PlayAnimation(animation);
                }
            }
        }

        public bool CheckValidConditions(SpawnConditionEnum s)
        {
            bool rv = true;

            if (_liSpawnConditions.Contains(s))
            {
                foreach (SpawnConditionEnum e in _liSpawnConditions)
                {
                    if (e.Equals(SpawnConditionEnum.Night) && !GameCalendar.IsNight())
                    {
                        rv = false;
                    }
                    else if (CompareSpawnSeason(e, SpawnConditionEnum.Spring))
                    {
                        rv = false;
                    }
                    else if (CompareSpawnSeason(e, SpawnConditionEnum.Summer))
                    {
                        rv = false;
                    }
                    else if (CompareSpawnSeason(e, SpawnConditionEnum.Winter))
                    {
                        rv = false;
                    }
                    else if (CompareSpawnSeason(e, SpawnConditionEnum.Fall))
                    {
                        rv = false;
                    }

                    if (!rv) { break; }
                }
            }

            return rv;
        }

        private bool CompareSpawnSeason(SpawnConditionEnum check, SpawnConditionEnum season)
        {
            return check.Equals(season) && !Util.ParseEnum<SpawnConditionEnum>(GameCalendar.GetSeason()).Equals(season);
        }

        public int GetRandomLootItem()
        {
            return _liLootIDs[RHRandom.Instance.Next(0, _liLootIDs.Count-1)];
        } 

        public override void KO()
        {
            base.KO();
            CombatManager.GiveXP(this);
        }

        public override List<CombatAction> GetCurrentSpecials()
        {
            return _liCombatActions;
        }
    }
}
