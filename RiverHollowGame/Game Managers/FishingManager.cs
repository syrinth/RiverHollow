using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.Characters;
using RiverHollow.GUIComponents;
using RiverHollow.Items.Tools;
using RiverHollow.SpriteAnimations;
using RiverHollow.Utilities;
using static RiverHollow.Utilities.Enums;

namespace RiverHollow.Game_Managers
{
    static class FishingManager
    {
        private enum FishSpeedEnum { Slow, Normal, Fast };
        private enum BobberStateEnum { None, Fakeout, Bite };

        private static FishingStateEnum _eState = FishingStateEnum.None;
        private static BobberStateEnum _eBobberState = BobberStateEnum.None;
        private static FishingData _fishData;
        private static int _iHits;
        private static int _iMisses;
        private static Fish _fish;
        private static FishingRod _rod;
        private static AnimatedSprite _sprBobber;
        public static Rectangle BobberRectangle => _sprBobber.SpriteRectangle;

        private static RHTimer _timer;

        public static bool Fishing => _eState != FishingStateEnum.None;
        public static bool Waiting => _eState == FishingStateEnum.Waiting;

        public static void Initialize()
        {
            _timer = new RHTimer();
            _sprBobber = new AnimatedSprite(DataManager.FILE_MISC_SPRITES);
            _sprBobber.AddAnimation(AnimationEnum.None, 0, 80, Constants.TILE_SIZE, Constants.TILE_SIZE, 2, .5f);
            _sprBobber.AddAnimation(AnimationEnum.Fish_Nibble, 0, 96, Constants.TILE_SIZE, Constants.TILE_SIZE, 1, 0.5f, false, true);
        }

        public static void Draw(SpriteBatch spriteBatch)
        {
            if (Fishing && _eState != FishingStateEnum.Cast)
            {
                _sprBobber.Draw(spriteBatch, Constants.MAX_LAYER_DEPTH);

                if (_eBobberState == BobberStateEnum.Bite)
                {
                    var p = _sprBobber.Position - new Point(0, 8);
                    Rectangle drawRect = new Rectangle(p, new Point(Constants.TILE_SIZE, Constants.TILE_SIZE));
                    spriteBatch.Draw(DataManager.GetTexture(DataManager.HUD_COMPONENTS), drawRect, GUIUtils.QUEST_NEW, Color.White, 0, Vector2.Zero, SpriteEffects.None, Constants.MAX_LAYER_DEPTH);
                }
            }
        }

        public static void Update(GameTime gTime)
        {
            if (Fishing)
            {
                _sprBobber.Update(gTime);

                //Bobber Timed Out
                if (_eBobberState != BobberStateEnum.None && _sprBobber.AnimationFinished(AnimationEnum.Fish_Nibble))
                {
                    ResetBobber();
                    if (_eBobberState == BobberStateEnum.Bite)
                    {
                        AddMiss();
                    }
                }

                var sprite = _rod.ToolSprite;
                switch (_eState)
                {
                    case FishingStateEnum.Cast:
                        if (sprite.Finished)
                        {
                            _sprBobber.PlayAnimation(AnimationEnum.None);

                            _eState = FishingStateEnum.Waiting;
                        }
                        break;
                    case FishingStateEnum.Waiting:
                        break;
                    case FishingStateEnum.Reeling:
                        //Wait time between nibbles hit zero and we have no bobber state
                        if (_timer.TickDown(gTime) && _eBobberState == BobberStateEnum.None)
                        {
                            _sprBobber.PlayAnimation(AnimationEnum.Fish_Nibble);
                            if (RHRandom.RollPercent(35))
                            {
                                _eBobberState = BobberStateEnum.Bite;
                                _sprBobber.SetColor(Color.Red);
                            }
                            else {
                                _eBobberState =  BobberStateEnum.Fakeout;
                            }
                        }
                        break;
                    case FishingStateEnum.Finish:
                        if (sprite.Finished)
                        {
                            _eState = FishingStateEnum.None;
                            _rod.FinishTool();
                            _rod = null;
                        }
                        break;
                }
            }
        }

        public static bool ProcessLeftButtonClick()
        {
            bool rv = false;
            if (Fishing)
            {
                rv = true;
                if (_eState == FishingStateEnum.Reeling)
                {

                    if (_eBobberState == BobberStateEnum.Bite)
                    {
                        SoundManager.PlayEffect(SoundEffectEnum.Success_Fish);

                        _iHits++;
                        if (_iHits >= _fishData.HitsNeeded)
                        {
                            InventoryManager.AddToInventory(_fishData.FishID, 1);
                            EndFishing();
                        }
                    }
                    else
                    {
                        AddMiss();
                    }

                    ResetBobber();
                }
            }
            return rv;
        }

        public static bool ProcessRightButtonClick()
        {
            return CancelFishing();
        }

        private static bool CancelFishing()
        {
            bool rv = false;
            if (_eState == FishingStateEnum.Waiting)
            {
                rv = true;
                _eState = FishingStateEnum.Finish;
                _rod.ReelItIn();
            }

            return rv;
        }

        public static void BeginFishing(FishingRod r, Point p)
        {
            _rod = r;
            _eState = FishingStateEnum.Cast;
            _sprBobber.Position = p;
        }

        private static void EndFishing()
        {
            _eState = FishingStateEnum.Finish;
            _rod.ToolSprite.PlayAnimation(VerbEnum.FinishTool, PlayerManager.PlayerActor.Facing);

            _iHits = 0;
            _fishData = default;
            _iMisses = 0;

            _fish.Activate(false);
            MapManager.CurrentMap.RemoveActor(_fish);
        }

        public static void StartReeling(Fish f)
        {
            _fish = f;
            _timer = new RHTimer();

            _eState = FishingStateEnum.Reeling;
            var hole = MapManager.CurrentMap.GetFishingHole(BobberRectangle.Location);
            int id = hole.GetRandomItemID();
            _fishData = new FishingData(id, DataManager.GetStringByIDKey(id, "FishingData", DataType.Item));

            SetTimer();
        }

        private static void AddMiss()
        {
            SoundManager.PlayEffect(SoundEffectEnum.Cancel);
            _iMisses++;
            if (_iMisses >= Constants.FISH_MISSES)
            {
                EndFishing();
            }
        }

        //Reset the hit timer between nibbles as well as the bobber state
        private static void ResetBobber()
        {
            SetTimer();
            _eBobberState = BobberStateEnum.None;
            _sprBobber.PlayAnimation(AnimationEnum.None);
            _sprBobber.SetColor(Color.White);
        }
        private static void SetTimer()
        {
            double multiplier = 100;

            var random = RHRandom.Instance().Next((int)(_fishData.MinTime * multiplier), (int)(_fishData.MaxTime * multiplier));

            _timer.Reset(random / multiplier);
        }

        private struct FishingData
        {
            public int FishID;
            public int HitsNeeded;
            public double MinTime; 
            public double MaxTime;
            public FishSpeedEnum FishSpeed;

            public FishingData(int fishID, string data)
            {
                FishID = fishID;
                HitsNeeded = 1;

                var dataSplit = Util.FindParams(data);
                if (dataSplit.Length >= 3)
                {
                    MinTime = double.Parse(dataSplit[0]);
                    MaxTime = double.Parse(dataSplit[1]);
                    FishSpeed = Util.ParseEnum<FishSpeedEnum>(dataSplit[2]);
                    if (dataSplit.Length >= 4)
                    {
                        HitsNeeded = int.Parse(dataSplit[3]);
                    }
                }
                else
                {
                    MinTime = 2;
                    MaxTime = 4;
                    FishSpeed = FishSpeedEnum.Normal;
                }
            }
        }
    }
}
