using RiverHollow.Game_Managers;
using RiverHollow.GUIComponents.Screens;
using System.Collections.Generic;

using static RiverHollow.Game_Managers.GameManager;

namespace RiverHollow.WorldObjects
{
    public class Door : TriggerObject
    {
        readonly bool _bKeyDoor;
        public Door(int id, Dictionary<string, string> stringData) : base(id, stringData)
        {
            if (stringData.ContainsKey("KeyDoor"))
            {
                _bKeyDoor = true;
                _sMatchTrigger = GameManager.KEY_OPEN;
            }

            _rBase.Y = _uSize.Height - BaseHeight;
        }

        /// <summary>
        /// When a door is triggered, it becomes passable and invisible.
        /// </summary>
        /// <param name="name"></param>
        public override void AttemptToTrigger(string name)
        {
            if (CanTrigger(name))
            {
                if (!string.IsNullOrEmpty(_sOutTrigger))
                {
                    GameManager.ActivateTriggers(_sOutTrigger);
                }
                _bHasBeenTriggered = true;
                _bWalkable = true;
                _bVisible = false;
            }
        }

        /// <summary>
        /// When triggered, makes doors impassable again
        /// </summary>
        public override void Reset()
        {
            _bWalkable = false;
            _bVisible = true;
            _iTriggersLeft = _iTriggerNumber;
        }

        /// <summary>
        /// Handles the response from whent he player attempts to Interact with the Door object.
        /// Primarily just handles the output for the doors and the type of triggers required to use it.
        /// </summary>
        public override void ProcessRightClick()
        {
            GameManager.SetSelectedWorldObject(this);
            if (_bKeyDoor)
            {
                if (DungeonManager.DungeonKeys() > 0)
                {
                    DungeonManager.UseDungeonKey();
                    AttemptToTrigger(KEY_OPEN);
                }
                else
                {
                    GUIManager.OpenTextWindow(DataManager.GetGameTextEntry("Key_Door"));
                }
            }
            else if (_iItemKeyID != -1)
            {
                GUIManager.OpenMainObject(new HUDInventoryDisplay());
            }
            else
            {
                GUIManager.OpenTextWindow(DataManager.GetGameTextEntry("Trigger_Door"));
            }
        }
    }
}
