using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.Characters;
using RiverHollow.Game_Managers;
using RiverHollow.Map_Handling;
using RiverHollow.Misc;
using RiverHollow.SpriteAnimations;
using RiverHollow.Utilities;
using System.Collections.Generic;
using static RiverHollow.Utilities.Enums;

namespace RiverHollow.Items
{
    public class Tool : Item
    {
        public ToolEnum ToolType => DataManager.GetEnumByIDKey<ToolEnum>(ID, "Subtype", DataType.Item);
        public int StaminaCost => DataManager.GetIntByIDKey(ID, "Stam", DataType.Item);
        public int ToolLevel => DataManager.GetIntByIDKey(ID, "Level", DataType.Item);

        private int HitAt => DataManager.GetIntByIDKey(ID, "HitAt", DataType.Item);
        private int _iCharges = 0;

        bool _bUsed = false;

        public string SoundEffect => DataManager.GetStringByIDKey(ID, "SoundEffect", DataType.Item);

        protected AnimatedSprite _sprite;
        public AnimatedSprite ToolAnimation { get => _sprite; }

        public override Vector2 Position
        {
            set
            {
                _vPosition = value;
                _sprite.Position = _vPosition;
            }
        }

        public Tool(int id, Dictionary<string, string> stringData) : base(id, stringData, 1)
        {
            _texTexture = DataManager.GetTexture(DataManager.FOLDER_ITEMS + "Tools");

            _iColTexSize = 128;
            _iRowTexSize = Constants.TILE_SIZE;

            _sprite = new AnimatedSprite(@"Textures\Items\ToolAnimations");

            Vector2 animationPosition = Vector2.Zero;
            if (stringData.ContainsKey("AnimationPosition"))
            {
                string[] texIndices = stringData["AnimationPosition"].Split('-');
                animationPosition = new Vector2(int.Parse(texIndices[0]), int.Parse(texIndices[1]));
            }

            int toolFrames = 5;
            int toolWidth = Constants.TILE_SIZE * 3;
            int toolHeight = Constants.TILE_SIZE * 4;
            int xCrawl = 0;
            int crawlIncrement = toolWidth * toolFrames;

            _sprite.AddAnimation(VerbEnum.UseTool, DirectionEnum.Down, (int)animationPosition.X, (int)animationPosition.Y, toolWidth, toolHeight, toolFrames, Constants.TOOL_ANIM_SPEED, false, true);
            xCrawl += crawlIncrement;
            _sprite.AddAnimation(VerbEnum.UseTool, DirectionEnum.Right, (int)animationPosition.X + xCrawl, (int)animationPosition.Y, toolWidth, toolHeight, toolFrames, Constants.TOOL_ANIM_SPEED, false, true);
            xCrawl += crawlIncrement;
            _sprite.AddAnimation(VerbEnum.UseTool, DirectionEnum.Up, (int)animationPosition.X + xCrawl, (int)animationPosition.Y, toolWidth, toolHeight, toolFrames, Constants.TOOL_ANIM_SPEED, false, true);
            xCrawl += crawlIncrement;
            _sprite.AddAnimation(VerbEnum.UseTool, DirectionEnum.Left, (int)animationPosition.X + xCrawl, (int)animationPosition.Y, toolWidth, toolHeight, toolFrames, Constants.TOOL_ANIM_SPEED, false, true);

            _sprite.Drawing = false;
        }

        public override void Update(GameTime gTime)
        {
            _sprite.Update(gTime);

            RHTile target = MapManager.CurrentMap.TargetTile;

            if (target != null && !_bUsed)
            {
                if (ReadyToHit() && (PlayerManager.ToolIsAxe() || PlayerManager.ToolIsPick()))
                {
                    _bUsed = true;
                    target.DamageObject(PlayerManager.ToolInUse);
                }
                else if (ReadyToHit() && PlayerManager.ToolIsScythe())
                {
                    _bUsed = true;
                    target.DamageObject(PlayerManager.ToolInUse);

                    if (PlayerManager.PlayerActor.Facing == DirectionEnum.Left || PlayerManager.PlayerActor.Facing == DirectionEnum.Right)
                    {
                        target.GetTileByDirection(DirectionEnum.Up).DamageObject(PlayerManager.ToolInUse);
                        target.GetTileByDirection(DirectionEnum.Down).DamageObject(PlayerManager.ToolInUse);
                    }
                    else
                    {
                        target.GetTileByDirection(DirectionEnum.Left).DamageObject(PlayerManager.ToolInUse);
                        target.GetTileByDirection(DirectionEnum.Right).DamageObject(PlayerManager.ToolInUse);
                    }
                }
                else if (PlayerManager.ToolIsWateringCan() && target.Flooring != null)
                {
                    //target.Water(true);
                }
            }

            if (ToolAnimation.AnimationVerbFinished(VerbEnum.UseTool, PlayerManager.PlayerActor.Facing))
            {
                _bUsed = false;
                PlayerManager.FinishedWithTool();
                PlayerManager.PlayerActor.PlayAnimation(VerbEnum.Idle, DirectionEnum.Down);
            }
        }

        private bool ReadyToHit()
        {
            bool needsToFinish = HitAt == -1 && ToolAnimation.AnimationVerbFinished(VerbEnum.UseTool, PlayerManager.PlayerActor.Facing);
            bool hitAt = HitAt > -1 && _sprite.CurrentFrame == HitAt;

            return needsToFinish || hitAt;
        }

        public void DrawToolAnimation(SpriteBatch spriteBatch)
        {
            _sprite.SetLayerDepthMod(1);
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

        public override void UseItem(TextEntryVerbEnum action)
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
