using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using RiverHollow.Game_Managers;
using RiverHollow.Map_Handling;
using RiverHollow.SpriteAnimations;
using RiverHollow.Utilities;
using static RiverHollow.Utilities.Enums;

namespace RiverHollow.Misc
{
    public abstract class EnvironmentalEffect
    {
        protected AnimatedSprite _sprBody;

        public virtual void Update(GameTime gTime) { }
        public void Draw(SpriteBatch spriteBatch)
        {
            _sprBody.Draw(spriteBatch, Constants.MAX_LAYER_DEPTH);
        }

        public virtual bool IsFinished()
        {
            return false;
        }

        public class Raindrop : EnvironmentalEffect
        {
            int _iFallDistance = 0;
            public Raindrop(int mapWidth, int mapHeight)
            {
                _iFallDistance = RHRandom.Instance().Next(50, 100);
                _sprBody = new AnimatedSprite(DataManager.FOLDER_ENVIRONMENT + "Rain");
                _sprBody.AddAnimation(AnimationEnum.Action1, 0, 0, Constants.TILE_SIZE, Constants.TILE_SIZE * 2);
                _sprBody.AddAnimation(AnimationEnum.Action_Finished, Constants.TILE_SIZE, 0, Constants.TILE_SIZE, Constants.TILE_SIZE * 2, 2, 0.1f, false, true);
                _sprBody.PlayAnimation(AnimationEnum.Action1);

                Point pos = new Point(RHRandom.Instance().Next(0, (mapWidth * Constants.TILE_SIZE) + 300), RHRandom.Instance().Next(-400, mapHeight * Constants.TILE_SIZE));
                _sprBody.Position = pos;
            }

            public override void Update(GameTime gTime)
            {
                if (_sprBody.IsCurrentAnimation(AnimationEnum.Action1))
                {
                    Point landingPos = _sprBody.Position + new Point(0, Constants.TILE_SIZE);
                    RHTile landingTile = MapManager.CurrentMap.GetTileByPixelPosition(landingPos);
                    if (_iFallDistance <= 0 && (landingTile == null || landingTile.WorldObject == null || landingTile.WorldObject.BuildableType(BuildableEnum.Structure)))
                    {
                        _sprBody.PlayAnimation(AnimationEnum.Action_Finished);
                    }
                    else
                    {
                        int modifier = RHRandom.Instance().Next(2, 3);
                        _iFallDistance -= modifier;
                        _sprBody.Position += new Point(-2 * modifier, 3 * modifier);
                    }
                }

                _sprBody.Update(gTime);
            }

            public override bool IsFinished()
            {
                return _sprBody.Finished;
            }
        }

        public class Snowflake : EnvironmentalEffect
        {
            readonly int _iMaxHeight = 0;
            public Snowflake(int mapWidth, int mapHeight)
            {
                float frameLength = RHRandom.Instance().Next(3, 5) / 10f;
                _iMaxHeight = mapHeight * GameManager.ScaledTileSize;
                _sprBody = new AnimatedSprite(DataManager.FOLDER_ENVIRONMENT + "Snow");
                _sprBody.AddAnimation(AnimationEnum.Action1, 0, 0, Constants.TILE_SIZE, Constants.TILE_SIZE, 3, frameLength, true);
                _sprBody.PlayAnimation(AnimationEnum.Action1);

                Point pos = new Point(RHRandom.Instance().Next(0, mapWidth * GameManager.ScaledTileSize), RHRandom.Instance().Next(0, mapHeight * GameManager.ScaledTileSize));
                _sprBody.Position = pos;
            }

            public override void Update(GameTime gTime)
            {
                _sprBody.Position += new Point(0, 2);

                _sprBody.Update(gTime);
            }

            public override bool IsFinished()
            {
                return _sprBody.Position.Y >= _iMaxHeight;
            }
        }
    }
}
