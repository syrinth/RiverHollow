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

            List<AnimationData> listAnimations = new List<AnimationData>();
            AddToAnimationsList(ref listAnimations, data, VerbEnum.Walk);
            AddToAnimationsList(ref listAnimations, data, VerbEnum.Attack);
            AddToAnimationsList(ref listAnimations, data, VerbEnum.Hurt);
            AddToAnimationsList(ref listAnimations, data, VerbEnum.Critical);
            AddToAnimationsList(ref listAnimations, data, VerbEnum.Cast);
            AddToAnimationsList(ref listAnimations, data, AnimationEnum.KO);

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

            _bJump = false; // data.ContainsKey("Jump"); Turning off jump logic for now

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

            _iCurrentHP = MaxHP;
            _iCurrentMP = MaxMP;

            LoadSpriteAnimations(listAnimations);
        }

        protected void LoadSpriteAnimations(List<AnimationData> listAnimations)
        {
            _spriteBody = new AnimatedSprite(_sTexture);
            base._iWidth = TileSize;
            base._iHeight = TileSize * 2;

            foreach (AnimationData data in listAnimations)
            {
                if (data.Directional)
                {
                    AddDirectionalAnimations(ref _spriteBody, data.Verb, data.XLocation, 0, _iWidth, _iHeight, data.Frames, data.FrameSpeed, data.Frames);
                }
                else
                {
                    _spriteBody.AddAnimation(data.Animation, data.XLocation, 0, _iWidth, _iHeight, data.Frames, data.FrameSpeed);
                }
            }

            //Deprecated Jump code
            //_spriteBody.AddAnimation(Util.GetActorString(VerbEnum.Idle, DirectionEnum.Down), xCrawl, 0, TileSize, TileSize * 2, 2, 0.2f);
            //_spriteBody.AddAnimation(Util.GetActorString(VerbEnum.Ground, DirectionEnum.Down), xCrawl, 0, TileSize, TileSize * 2, 2, 0.2f);
            //_spriteBody.AddAnimation(Util.GetActorString(VerbEnum.Air, DirectionEnum.Down), xCrawl + 32, 0, TileSize, TileSize * 2, 2, 0.2f);
            //xCrawl += 64;
            //_spriteBody.AddAnimation(Util.GetActorString(VerbEnum.Idle, DirectionEnum.Up), xCrawl, 0, TileSize, TileSize * 2, 2, 0.2f);
            //_spriteBody.AddAnimation(Util.GetActorString(VerbEnum.Ground, DirectionEnum.Up), xCrawl, 0, TileSize, TileSize * 2, 2, 0.2f);
            //_spriteBody.AddAnimation(Util.GetActorString(VerbEnum.Air, DirectionEnum.Up), xCrawl + 32, 0, TileSize, TileSize * 2, 2, 0.2f);
            //xCrawl += 64;
            //_spriteBody.AddAnimation(Util.GetActorString(VerbEnum.Idle, DirectionEnum.Left), xCrawl, 0, TileSize, TileSize * 2, 2, 0.2f);
            //_spriteBody.AddAnimation(Util.GetActorString(VerbEnum.Ground, DirectionEnum.Left), xCrawl, 0, TileSize, TileSize * 2, 2, 0.2f);
            //_spriteBody.AddAnimation(Util.GetActorString(VerbEnum.Air, DirectionEnum.Left), xCrawl + 32, 0, TileSize, TileSize * 2, 2, 0.2f);
            //xCrawl += 64;
            //_spriteBody.AddAnimation(Util.GetActorString(VerbEnum.Idle, DirectionEnum.Right), xCrawl, 0, TileSize, TileSize * 2, 2, 0.2f);
            //_spriteBody.AddAnimation(Util.GetActorString(VerbEnum.Ground, DirectionEnum.Right), xCrawl, 0, TileSize, TileSize * 2, 2, 0.2f);
            //_spriteBody.AddAnimation(Util.GetActorString(VerbEnum.Air, DirectionEnum.Right), xCrawl + 32, 0, TileSize, TileSize * 2, 2, 0.2f);
            //_spriteBody.SetCurrentAnimation(Util.GetActorString(VerbEnum.Idle, DirectionEnum.Down));
        }

        public override void Update(GameTime gTime)
        {
            base.Update(gTime);

            ///When the Monster has finished playing the KO animation, let the CombatManager know so it can do any final actions
            if (IsCurrentAnimation(AnimationEnum.KO) && BodySprite.CurrentFrameAnimation.PlayCount == 1)
            {
                CombatManager.MonsterKOAnimFinished(this);
            }

            // Only comment this out for now in case we implement stealth or something so you can sneak past enemies without aggroing them
            //if (!CombatManager.InCombat) {
            //    UpdateMovement(gTime);
            //}
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

                    PlayAnimation(VerbEnum.Ground);
                }
                else if (BodySprite.CurrentAnimation.StartsWith("Ground") && BodySprite.CurrentFrameAnimation.PlayCount < 1)
                {
                    move = false;
                }
                else if (!BodySprite.CurrentAnimation.StartsWith("Idle") && BodySprite.CurrentFrameAnimation.PlayCount >= 1)
                {
                    bool jumping = BodySprite.CurrentAnimation.StartsWith("Air");
                    PlayAnimation(jumping ? VerbEnum.Ground : VerbEnum.Air);
                    _vJumpTo = Vector2.Zero;
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
                    PlayAnimation(CombatManager.InCombat ? VerbEnum.Walk : VerbEnum.Idle);
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
                    PlayAnimation(CombatManager.InCombat ? VerbEnum.Walk : VerbEnum.Idle);
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
                PlayAnimation(CombatManager.InCombat ? VerbEnum.Walk : VerbEnum.Idle);
            }
            else
            {
                if (!_bJump || (_bJump && !BodySprite.CurrentAnimation.StartsWith("Air")))
                {
                    if (Math.Abs((int)direction.X) > Math.Abs((int)direction.Y))
                    {
                        if (direction.X > 0) { Facing = DirectionEnum.Right; }
                        else if (direction.X < 0) { Facing = DirectionEnum.Left; }
                    }
                    else
                    {
                        if (direction.Y > 0) { Facing = DirectionEnum.Down; }
                        else if (direction.Y < 0) { Facing = DirectionEnum.Up; }
                    }

                    PlayAnimation(_bJump ? VerbEnum.Ground : VerbEnum.Walk);
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
