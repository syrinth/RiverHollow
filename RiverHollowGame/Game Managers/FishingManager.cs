using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.Items.Tools;
using RiverHollow.SpriteAnimations;
using RiverHollow.Utilities;
using static RiverHollow.Utilities.Enums;

namespace RiverHollow.Game_Managers
{
    static class FishingManager
    {
        private enum FishSpeedEnum { Slow, Normal, Fast };

        private static FishingStateEnum _eState = FishingStateEnum.None;
        private static FishingData _fishData;
        private static int _iHits;
        private static int _iMisses;
        private static FishingRod _rod;

        private static AnimatedSprite _targetIcon;
        private static RHTimer _timer;

        public static bool Fishing => _eState != FishingStateEnum.None;

        public static void SetFish(int id, Point p)
        {
            if (id != -1)
            {
                _eState = FishingStateEnum.WaitForFish;
                _fishData = new FishingData(id, DataManager.GetStringByIDKey(id, "FishingData", DataType.Item));

                var size = new Point(1, 1);
                _targetIcon = new AnimatedSprite(DataManager.FOLDER_ENVIRONMENT + "FishingIcon");
                _targetIcon.AddAnimation(FishSpeedEnum.Slow, 0, 0, size, 4, .3f, false, true);
                _targetIcon.AddAnimation(FishSpeedEnum.Normal, 0, 0, size, 4, .25f, false, true);
                _targetIcon.AddAnimation(FishSpeedEnum.Fast, 0, 0, size, 4, .18f, false, true);
                _targetIcon.AddAnimation(VerbEnum.Action1, 64, 0, size, 1, .5f, false, true);

                _targetIcon.Position = (p - new Point(Constants.TILE_SIZE / 2, Constants.TILE_SIZE / 2));
                _targetIcon.Show = false;
                _timer = new RHTimer(SetTimerSpeed());
            }
        }

        public static void Update(GameTime gTime)
        {
            if (Fishing)
            {
                var sprite = _rod.ToolSprite;
                switch (_eState)
                {
                    case FishingStateEnum.Cast:
                        if (sprite.Finished)
                        {
                            var facingPoint = Util.GetPointFromDirection(PlayerManager.PlayerActor.Facing);
                            for (int i = 2; i > 0; i--)
                            {
                                var checkPoint = PlayerManager.PlayerActor.CollisionCenter + Util.MultiplyPoint(facingPoint, Constants.TILE_SIZE * i);
                                var tile = MapManager.CurrentMap.GetTileByPixelPosition(checkPoint);
                                if (tile.IsWaterTile)
                                {
                                    var hole = MapManager.CurrentMap.GetFishingHole(tile);
                                    SetFish(hole.GetRandomItemID(), checkPoint);
                                    break;
                                }
                            }

                            if (_eState != FishingStateEnum.WaitForFish)
                            {
                                EndFishing();
                            }
                        }
                        break;
                    case FishingStateEnum.WaitForFish:
                        if (_timer.TickDown(gTime))
                        {
                            _eState = FishingStateEnum.AttemptToCatch;
                            _targetIcon.PlayAnimation(_fishData.FishSpeed);
                            break;
                        }
                        break;
                    case FishingStateEnum.AttemptToCatch:
                        _targetIcon?.Update(gTime);
                        if (_targetIcon.CurrentAnimation.Equals(Util.GetEnumString(VerbEnum.Action1)) && _targetIcon.Finished)
                        {
                            SetToWait();
                        }
                        else if (_targetIcon.Finished)
                        {
                            AddMiss();
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

        public static void Draw(SpriteBatch spriteBatch)
        {
            if (Fishing)
            {
                _targetIcon?.Draw(spriteBatch);
            }
        }

        public static bool ProcessLeftButtonClick()
        {
            bool rv = false;
            if (Fishing)
            {
                rv = true;
                if (_eState == FishingStateEnum.AttemptToCatch)
                {
                    if (_targetIcon.CurrentFrame == _targetIcon.CurrentFrameAnimation.FrameCount - 1)
                    {
                        SoundManager.PlayEffect(SoundEffectEnum.Success_Fish);
                        _iHits++;
                        if (_iHits >= _fishData.HitsNeeded)
                        {
                            InventoryManager.AddToInventory(_fishData.FishID, 1);
                            EndFishing();
                        }
                        else
                        {
                            _targetIcon.PlayAnimation(VerbEnum.Action1);
                        }
                    }
                    else
                    {
                        AddMiss();
                    }
                }
            }
            return rv;
        }

        public static bool ProcessRightButtonClick()
        {
            bool rv = false;
            if (_eState == FishingStateEnum.WaitForFish)
            {
                rv = true;
                _eState = FishingStateEnum.Finish;
                _rod.ReelItIn();
            }

            return rv;
        }

        public static void BeginFishing(FishingRod r)
        {
            _rod = r;
            _eState = FishingStateEnum.Cast;
        }

        private static void AddMiss()
        {
            SoundManager.PlayEffect(SoundEffectEnum.Cancel);
            _iMisses++;
            if (_iMisses >= Constants.FISH_MISSES)
            {
                EndFishing();
            }
            else
            {
                SetToWait();
            }
        }

        public static void SetToWait()
        {
            _eState = FishingStateEnum.WaitForFish;
            _targetIcon.Show = false;
            _timer.Reset(SetTimerSpeed() / 2);
        }

        private static void EndFishing()
        {
            _eState = FishingStateEnum.Finish;
            _rod.ToolSprite.PlayAnimation(VerbEnum.FinishTool, PlayerManager.PlayerActor.Facing);

            _iHits = 0;
            _fishData = default;
            _iMisses = 0;

            _targetIcon = null;
        }

        private static double SetTimerSpeed()
        {
            double multiplier = 100;

            var random = RHRandom.Instance().Next((int)(_fishData.MinTime * multiplier), (int)(_fishData.MaxTime * multiplier));
            return random / multiplier;
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
