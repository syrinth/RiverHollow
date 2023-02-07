using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.Game_Managers;
using static RiverHollow.Utilities.Enums;
using RiverHollow.Utilities;

namespace RiverHollow.Items.Tools
{
    public abstract class HitboxTool :Tool
    {
        public Rectangle Hitbox { get; protected set; }
        public HitboxTool(int id, Dictionary<string, string> stringData) : base(id, stringData) { }

        protected Rectangle GetHitbox(DirectionEnum dir)
        {
            Rectangle hitbox = Rectangle.Empty;
            if (dir == DirectionEnum.Up)
            {
                hitbox = new Rectangle(PlayerManager.PlayerActor.CollisionBoxPosition.ToPoint(), new Point(3 * Constants.TILE_SIZE, Constants.TILE_SIZE));
                hitbox.Offset(-Constants.TILE_SIZE, -Constants.TILE_SIZE);
            }
            else if (dir == DirectionEnum.Down)
            {
                hitbox = new Rectangle(PlayerManager.PlayerActor.CollisionBoxPosition.ToPoint(), new Point(3 * Constants.TILE_SIZE, Constants.TILE_SIZE));
                hitbox.Offset(-Constants.TILE_SIZE, PlayerManager.PlayerActor.CollisionBox.Height);
            }
            else if (dir == DirectionEnum.Left)
            {
                hitbox = new Rectangle(PlayerManager.PlayerActor.CollisionBoxPosition.ToPoint(), new Point(Constants.TILE_SIZE, 3 * Constants.TILE_SIZE));
                hitbox.Offset(-Constants.TILE_SIZE, -Constants.TILE_SIZE);
            }
            else if (dir == DirectionEnum.Right)
            {
                hitbox = new Rectangle(PlayerManager.PlayerActor.CollisionBoxPosition.ToPoint(), new Point(Constants.TILE_SIZE, 3 * Constants.TILE_SIZE));
                hitbox.Offset(PlayerManager.PlayerActor.CollisionBox.Width, -Constants.TILE_SIZE);
            }

            return hitbox;
        }

        public override void DrawToolAnimation(SpriteBatch spriteBatch)
        {
            if (PlayerManager.PlayerActor.Facing == DirectionEnum.Up) { _sprite.SetLayerDepthMod(-20); }
            else { _sprite.SetLayerDepthMod(1); }

            if (Hitbox != Rectangle.Empty && Constants.DRAW_COLLISION)
            {
                spriteBatch.Draw(DataManager.GetTexture(DataManager.DIALOGUE_TEXTURE), Hitbox, new Rectangle(160, 128, 2, 2), Color.White * 0.5f);
                spriteBatch.Draw(DataManager.GetTexture(DataManager.DIALOGUE_TEXTURE), PlayerManager.PlayerActor.CollisionBox, new Rectangle(160, 128, 2, 2), Color.White * 0.5f);
            }

            _sprite.Draw(spriteBatch);
        }
    }
}
