using RiverHollow.Game_Managers;
using RiverHollow.GUIComponents.Screens;
using RiverHollow.Utilities;
using System.Collections.Generic;

namespace RiverHollow.WorldObjects
{
    public abstract class TriggerDoorObject : TriggerObject
    {
        public TriggerDoorObject(int id, Dictionary<string, string> stringData) : base(id, stringData)
        {
            _rBase.Y = _pSize.Y - BaseHeight;
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
                    _bHasBeenTriggered = true;
                    _bWalkable = true;
                    _bVisible = false;

                    SoundManager.PlayEffect(Enums.SoundEffectEnum.Door);
                }
                else
                {
                    if (!string.IsNullOrEmpty(_sOutTrigger))
                    {
                        if (CurrentMap.IsDungeon) { DungeonManager.ActivateTrigger(_sOutTrigger); }
                        else { GameManager.ActivateTriggers(_sOutTrigger); }
                    }
                    _bHasBeenTriggered = false;
                    _bWalkable = false;
                    _bVisible = true;
                    SoundManager.PlayEffect(Enums.SoundEffectEnum.Door);
                }
            }
        }

        /// <summary>
        /// When triggered, makes doors impassable again
        /// </summary>
        public override void ResetTrigger()
        {
            _bWalkable = false;
            _bVisible = true;
            _iTriggersLeft = _iTriggerNumber;
        }

        public override void LoadData(SaveManager.WorldObjectData data)
        {
            base.LoadData(data);
            if (_bHasBeenTriggered)
            {
                _bHasBeenTriggered = true;
                _bWalkable = true;
                _bVisible = false;
            }
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
            GUIManager.OpenTextWindow("Trigger_Door");
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
            GUIManager.OpenTextWindow("Trigger_Door");
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
        /// Handles the response from whent he player attempts to Interact with the Door object.
        /// Primarily just handles the output for the doors and the type of triggers required to use it.
        /// </summary>
        public override bool ProcessRightClick()
        {
            bool rv = false;

            GameManager.SetSelectedWorldObject(this);
            if (_bKeyDoor)
            {
                if (DungeonManager.DungeonKeys() > 0)
                {
                    rv = true;
                    DungeonManager.UseDungeonKey();
                    AttemptToTrigger(Constants.TRIGGER_KEY_OPEN);
                }
                else
                {
                    rv = true;
                    GUIManager.OpenTextWindow("Key_Door");
                }
            }
            else if (_iItemKeyID != -1)
            {
                rv = true;
                GUIManager.OpenMainObject(new HUDInventoryDisplay());
            }

            return rv;
        }
    }
}
