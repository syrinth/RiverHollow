using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.Game_Managers;
using RiverHollow.GUIComponents.Screens;
using RiverHollow.Utilities;
using System.Collections.Generic;
using static RiverHollow.Utilities.Enums;

namespace RiverHollow.WorldObjects
{
    public abstract class TriggerDoorObject : TriggerObject
    {
        Rectangle _rSourceGem;
        Rectangle[] _arrPoints;
        public TriggerDoorObject(int id, Dictionary<string, string> stringData) : base(id, stringData)
        {
            _rBase.Y = _pSize.Y - BaseHeight;
            Setup();

            Sprite.AddAnimation(AnimationEnum.Action1, _pImagePos.X + (_pSize.X * Constants.TILE_SIZE), _pImagePos.Y, _pSize, 0, 0);
        }

        private void Setup()
        {
            var strParams = GetStringParamsByIDKey("TriggerGemData");

            var p = Util.ParsePoint(strParams[0]);
            p = Util.MultiplyPoint(p, Constants.TILE_SIZE);
            var sourceSize = Util.ParsePoint(strParams[1]);

            _rSourceGem = new Rectangle(p, sourceSize);

            
            var posParams = GetStringParamsByIDKey("TriggerPos");
            _arrPoints = new Rectangle[posParams.Length];
            for (int i = 0; i < posParams.Length; i++)
            {
                var drawPoint = Util.ParsePoint(posParams[i]);
                _arrPoints[i] = new Rectangle(drawPoint, sourceSize);
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);

            int startPoint = _iTriggerNumber == 2 ? 1 : 0;
            var gemsToLight = _iTriggerNumber - _iTriggersLeft;
            for (int i = startPoint; i < _arrPoints.Length; i++)
            {
                var drawGem = _rSourceGem;
                if (i - startPoint < gemsToLight)
                {
                    drawGem.Offset(new Point(_rSourceGem.Width, 0));
                }
                
                var drawBox = new Rectangle(Sprite.Position.X + _arrPoints[i].X, Sprite.Position.Y + _arrPoints[i].Y, _arrPoints[i].Width, _arrPoints[i].Height);
                spriteBatch.Draw(DataManager.GetTexture(DataManager.FILE_WORLDOBJECTS), drawBox, drawGem, Color.White, 0f, Vector2.Zero, SpriteEffects.None, Sprite.LayerDepth + 1);
            }
        }
        private void SetTriggerFlags(bool value)
        {
            _bHasBeenTriggered = value;
            _bWalkable = value;

            if (value)
            {
                Sprite.PlayAnimation(AnimationEnum.Action1);
            }
            else
            {
                Sprite.PlayAnimation(AnimationEnum.ObjectIdle);
            }
        }

        /// <summary>
        /// When a door is triggered, it becomes passable and invisible.
        /// </summary>
        /// <param name="name"></param>
        public override void AttemptToTrigger(string name)
        {
            if (TriggerMatches(name))
            {
                if (!_bHasBeenTriggered)
                {
                    if (!string.IsNullOrEmpty(_sOutTrigger))
                    {
                        if (CurrentMap.IsDungeon) { DungeonManager.ActivateTrigger(_sOutTrigger); }
                        else { GameManager.ActivateTriggers(_sOutTrigger); }
                    }
                    SetTriggerFlags(true);

                    SoundManager.PlayEffect(Enums.SoundEffectEnum.Door);
                }
                else
                {
                    if (!string.IsNullOrEmpty(_sOutTrigger))
                    {
                        if (CurrentMap.IsDungeon) { DungeonManager.ActivateTrigger(_sOutTrigger); }
                        else { GameManager.ActivateTriggers(_sOutTrigger); }
                    }
                    SetTriggerFlags(false);
                    SoundManager.PlayEffect(Enums.SoundEffectEnum.Door);
                }
            }
        }

        /// <summary>
        /// When triggered, makes doors impassable again
        /// </summary>
        public override void ResetTrigger()
        {
            SetTriggerFlags(false);
            _iTriggersLeft = _iTriggerNumber;
        }

        public override void LoadData(SaveManager.WorldObjectData data)
        {
            base.LoadData(data);
            if (_bHasBeenTriggered)
            {
                SetTriggerFlags(true);
            }

            Setup();
        }
    }

    public class TriggerDoor : TriggerDoorObject
    {
        public TriggerDoor(int id, Dictionary<string, string> stringData) : base(id, stringData) { }

        /// <summary>
        /// Handles the response from whent he player attempts to Interact with the Door object.
        /// Primarily just handles the output for the doors and the type of triggers required to use it.
        /// </summary>
        public override bool ProcessRightClick()
        {
            if (!_bHasBeenTriggered)
            {
                GUIManager.OpenTextWindow("Trigger_Door");
            }
            return true;
        }
    }

    public class MobDoor : TriggerDoorObject
    {
        public MobDoor(int id, Dictionary<string, string> stringData) : base(id, stringData)
        {
            _sMatchTrigger = Constants.TRIGGER_MOB_OPEN;
        }

        /// <summary>
        /// Handles the response from whent he player attempts to Interact with the Door object.
        /// Primarily just handles the output for the doors and the type of triggers required to use it.
        /// </summary>
        public override bool ProcessRightClick()
        {
            if (!_bHasBeenTriggered)
            {
                GUIManager.OpenTextWindow("Trigger_Door");
            }
            return true;
        }
    }

    public class KeyDoor : TriggerDoorObject
    {
        readonly bool _bKeyDoor;
        public KeyDoor(int id, Dictionary<string, string> stringData) : base(id, stringData)
        {
            if (stringData.ContainsKey("KeyDoor"))
            {
                _bKeyDoor = true;
                _sMatchTrigger = Constants.TRIGGER_KEY_OPEN;
            }
        }

        /// <summary>
        /// Handles the response from when the player attempts to Interact with the Door object.
        /// Primarily just handles the output for the doors and the type of triggers required to use it.
        /// </summary>
        public override bool ProcessRightClick()
        {
            if (!_bHasBeenTriggered)
            {
                GameManager.SetSelectedWorldObject(this);
                if (_bKeyDoor)
                {
                    if (DungeonManager.DungeonKeys() > 0)
                    {
                        DungeonManager.UseDungeonKey();
                        AttemptToTrigger(Constants.TRIGGER_KEY_OPEN);
                    }
                    else
                    {
                        GUIManager.OpenTextWindow("Key_Door");
                    }
                }
                else if (_iItemKeyID != -1)
                {
                    GUIManager.OpenMainObject(new HUDInventoryDisplay());
                }
            }

            return true;
        }
    }
}
