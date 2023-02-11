using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Content.Pipeline.Animations;
using RiverHollow.Characters;
using RiverHollow.Game_Managers;
using RiverHollow.Map_Handling;
using RiverHollow.Misc;
using RiverHollow.SpriteAnimations;
using RiverHollow.Utilities;
using System;
using System.Collections.Generic;
using static RiverHollow.Utilities.Enums;

namespace RiverHollow.Items
{
    public class Tool : Item
    {
        public ToolEnum ToolType => DataManager.GetEnumByIDKey<ToolEnum>(ID, "Subtype", DataType.Item);
        public int StaminaCost => DataManager.GetIntByIDKey(ID, "Stam", DataType.Item, 0);
        public int ToolLevel => DataManager.GetIntByIDKey(ID, "Level", DataType.Item);

        protected int HitAt => DataManager.GetIntByIDKey(ID, "HitAt", DataType.Item);
        protected int _iCharges = 0;

        protected bool _bUsed = false;

        public string SoundEffect => DataManager.GetStringByIDKey(ID, "SoundEffect", DataType.Item);

        protected AnimatedSprite _sprite;
        public AnimatedSprite ToolAnimation { get => _sprite; }

        Point _pPosition;

        public Point Position
        {
            set
            {
                _pPosition = value;
                _sprite.Position = _pPosition;
            }
        }

        public Tool(int id, Dictionary<string, string> stringData) : base(id, stringData, 1)
        {
            _texTexture = DataManager.GetTexture(DataManager.FOLDER_ITEMS + "Tools");

            if (stringData.ContainsKey("AnimationData"))
            {
                _sprite = new AnimatedSprite(@"Textures\Items\ToolAnimations")
                {
                    Drawing = false
                };

                string[] par = Util.FindParams(stringData["AnimationData"]);
                Vector2 start = Util.FindVectorArguments(par[0]);
                int[] sz = Util.FindIntArguments(par[1]);
                Point size = new Point(sz[0], sz[1]);
                int frames = int.Parse(par[2]);
                float frameSpeed = float.Parse(par[3]);

                int xCrawl = 0;
                foreach (DirectionEnum e in Enum.GetValues(typeof(DirectionEnum)))
                {
                    if(e == DirectionEnum.None) { continue; }

                    _sprite.AddAnimation(e, (int)start.X + xCrawl, (int)start.Y, size, frames, frameSpeed, false, true);
                    xCrawl += size.X * Constants.TILE_SIZE * frames;
                }
            }
        }

        public override void Update(GameTime gTime)
        {
            _sprite.Update(gTime);
            DirectionEnum dir = PlayerManager.PlayerActor.Facing;

            RHTile target = MapManager.CurrentMap.TargetTile;

            if (target != null && !_bUsed)
            {
                if (ReadyToHit() && (PlayerManager.ToolIsAxe() || PlayerManager.ToolIsPick()))
                {
                    _bUsed = true;
                    target.DamageObject(PlayerManager.ToolInUse);
                }
                else if (PlayerManager.ToolIsWateringCan() && target.Flooring != null)
                {
                    //target.Water(true);
                }
            }

            FinishTool(dir);
        }

        protected bool FinishTool(DirectionEnum dir)
        {
            bool rv = false;

            if (ToolAnimation.AnimationFinished(dir))
            {
                rv = true;
                _bUsed = false;
                PlayerManager.FinishedWithTool();
                PlayerManager.PlayerActor.PlayAnimation(VerbEnum.Idle, DirectionEnum.Down);
            }
            return rv;
        }

        protected bool ReadyToHit()
        {
            bool needsToFinish = HitAt == -1 && ToolAnimation.AnimationFinished(PlayerManager.PlayerActor.Facing);
            bool hitAt = HitAt > -1 && _sprite.CurrentFrame == HitAt;

            return needsToFinish || hitAt;
        }

        public virtual void DrawToolAnimation(SpriteBatch spriteBatch)
        {
            if (PlayerManager.PlayerActor.Facing == DirectionEnum.Up) { _sprite.SetLayerDepthMod(-20); }
            else { _sprite.SetLayerDepthMod(1); }

            _sprite.Draw(spriteBatch);
        }

        public override bool AddToInventoryTrigger()
        {
            if (ToolType == ToolEnum.Backpack)
            {
                PlayerManager.AddTool(this);
                return true;
            }
            else { return false; }
        }

        public override bool HasUse() { return true; }
        public override bool ItemBeingUsed()
        {
            GameManager.SetSelectedItem(this);
            if (ToolType == ToolEnum.Harp)
            {
                Spirit s = MapManager.CurrentMap.FindSpirit();
                if (s != null)
                {
                    HarpManager.NewSong(s);
                }
            }
            else if (ToolType == ToolEnum.Return)
            {
                TextEntry entry = null;
                if (DungeonManager.CurrentDungeon != null)
                {
                    if (HasCharges()) { entry.FormatText(DataManager.GetGameTextEntry("Rune_of_Return_Use"), Name()); }
                    else { entry.FormatText(DataManager.GetGameTextEntry("Rune_of_Return_Empty"), Name()); }
                }
                else
                {
                    entry.FormatText(DataManager.GetGameTextEntry("Rune_of_Return_No_Dungeon"), Name());
                }
                ConfirmItemUse(entry);
            }
            else
            {
                PlayerManager.SetTool(this);
            }

            return true;
        }

        public override void UseItem()
        {
            if (ToolType == ToolEnum.Return)
            {
                DungeonManager.GoToEntrance();
                _iCharges--;
            }

            if (ToolType == ToolEnum.Scythe)
            {

            }
        }

        public bool HasCharges()
        {
            return _iCharges > 0;
        }
    }
}
