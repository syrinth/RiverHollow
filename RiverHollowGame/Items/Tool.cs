using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Content.Pipeline.Animations;
using RiverHollow.Characters;
using RiverHollow.Game_Managers;
using RiverHollow.Map_Handling;
using RiverHollow.Misc;
using RiverHollow.SpriteAnimations;
using RiverHollow.Utilities;
using RiverHollow.WorldObjects;
using System;
using System.Collections.Generic;
using static RiverHollow.Utilities.Enums;

namespace RiverHollow.Items
{
    public class Tool : Item
    {
        public ToolEnum ToolType => GetEnumByIDKey<ToolEnum>("Subtype");
        public float EnergyCost => GetFloatByIDKey("EnergyCost", 0);
        public int ToolLevel => GetIntByIDKey("Level");

        public bool IsAutomatic => ToolType != ToolEnum.Sword && ToolType != ToolEnum.Hoe;

        protected int HitAt => GetIntByIDKey("HitAt");
        protected int _iCharges = 0;

        protected bool _bTriggered = false;

        protected AnimatedSprite _sprite;
        public AnimatedSprite ToolSprite { get => _sprite; }

        Point _pPosition;

        public Point Position
        {
            set
            {
                _pPosition = value;
                _sprite.Position = _pPosition;
            }
        }

        public Tool(int id) : base(id, 1)
        {
            _texTexture = DataManager.GetTexture(DataManager.FOLDER_ITEMS + "Tools");

            if (GetBoolByIDKey("AnimationData"))
            {
                _sprite = new AnimatedSprite(@"Textures\Items\ToolAnimations")
                {
                    Show = false
                };

                string[] par = FindParamsByIDKey("AnimationData");
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

            if (target != null && !_bTriggered)
            {
                if (ReadyToHit())
                {
                    _bTriggered = true;

                    switch (ToolType)
                    {
                        case ToolEnum.Axe:
                        case ToolEnum.Pick:
                            if (target.DamageObject(PlayerManager.ToolInUse))
                            {
                                PlayerManager.ToolLoseEnergy();
                            }
                            break;
                        case ToolEnum.Hoe:
                            target.TillTile();
                            break;
                        case ToolEnum.WateringCan:
                            PlayerManager.ToolLoseEnergy();
                            target.WaterTile();
                            break;
                    }
                }
            }

            CheckFinishTool(dir);
        }
        
        protected bool CheckFinishTool(DirectionEnum dir)
        {
            bool rv = false;

            if (ToolSprite.AnimationFinished(dir))
            {
                rv = true;
                FinishTool();
            }
            return rv;
        }

        public void FinishTool()
        {
            _bTriggered = false;
            ToolSprite.Finished = false;
            PlayerManager.FinishedWithTool();
        }

        protected bool ReadyToHit()
        {
            bool needsToFinish = HitAt == -1 && ToolSprite.AnimationFinished(PlayerManager.PlayerActor.Facing);
            bool hitAt = HitAt > -1 && _sprite.CurrentFrame == HitAt;

            return needsToFinish || hitAt;
        }

        public virtual void DrawToolAnimation(SpriteBatch spriteBatch)
        {
            int mod = PlayerManager.PlayerActor.Facing == DirectionEnum.Up ? -1 : 1;
            _sprite.Draw(spriteBatch, true, 1, PlayerManager.PlayerActor.BodySprite.LayerDepth + mod);
        }

        public override bool AddToInventoryTrigger()
        {
            base.AddToInventoryTrigger();
            TownManager.AddToArchive(ID);

            if (ToolType == ToolEnum.Backpack || ToolType == ToolEnum.Lantern)
            {
                PlayerManager.AddTool(this);
                return true;
            }
            else if (Constants.AUTO_TOOL)
            {
                return PlayerManager.AddTool(this);
            }
            else { return false; }
        }

        public override bool HasUse() { return !Constants.AUTO_TOOL || !IsAutomatic; }
        public override void UseItem()
        {
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
            }
            else
            {
                PlayerManager.SetTool(this);
            }
        }

        public bool HasCharges()
        {
            return _iCharges > 0;
        }
    }
}
