﻿using RiverHollow.Characters;
using RiverHollow.Items;
using RiverHollow.Misc;
using RiverHollow.WorldObjects;
using System.Collections.Generic;
using static RiverHollow.Misc.RHTask;
using static RiverHollow.Utilities.Enums;

namespace RiverHollow.Game_Managers
{
    public static class TaskManager
    {
        private static List<RHTask> _liDelayedTasks;
        private static List<RHTask> _liTasks;

        public static RHTask QueuedHandin;
        public static List<RHTask> TaskLog
        {
            get { return _liTasks.FindAll(x => x.TaskState == TaskStateEnum.TaskLog); }
        }

        public static void Initialize()
        {
            _liTasks = new List<RHTask>();
            _liDelayedTasks = new List<RHTask>();
            foreach (KeyValuePair<int, Dictionary<string, string>> kvp in DataManager.TaskData)
            {
                _liTasks.Add(new RHTask(kvp.Key, kvp.Value));
            }
        }

        public static void Rollover()
        {
            _liTasks.ForEach(o => o.AssignTaskToNPC());
        }

        public static void AssignTasks()
        {
            _liTasks.ForEach(o => o.AssignTaskToNPC());
        }
        public static void AssignTaskByID(int taskID)
        {
            _liTasks.Find(x => x.ID == taskID).AssignTaskToNPC();
        }

        public static bool TaskCompleted(int taskID)
        {
            return _liTasks.Find(x => x.ID == taskID).TaskState == TaskStateEnum.Completed;
        }

        public static void AddToTaskLog(int taskID)
        {
            AddToTaskLog(_liTasks.Find(x => x.ID == taskID));
        }
        public static void AddToTaskLog(RHTask t)
        {
            TaskLog.Add(t);
        }

        public static void AddDelayedTask(int taskID)
        {
            _liDelayedTasks.Add(_liTasks.Find(x => x.ID == taskID));
        }
        public static void AssignDelayedTasks()
        {
            _liDelayedTasks.ForEach(x => x.AssignTaskToNPC());
            _liDelayedTasks.Clear();
        }

        public static void AdvanceTaskProgress(WorldObject obj)
        {
            foreach (RHTask q in TaskLog)
            {
                if (q.AttemptProgressBuild(obj.ID))
                {
                    break;
                }
            }
        }
        public static void AdvanceTaskProgress(Mob m)
        {
            foreach (RHTask q in TaskLog)
            {
                if (q.AttemptProgress(m))
                {
                    break;
                }
            }
        }
        public static void AdvanceTaskProgress()
        {
            foreach (RHTask q in TaskLog)
            {
                if (q.AttemptProgress())
                {
                    break;
                }
            }
        }
        public static void TaskProgressEnterBuilding(int buildingID)
        {
            if (buildingID != -1)
            {
                foreach (RHTask q in TaskLog)
                {
                    if (q.TaskProgressEnterBuilding(buildingID))
                    {
                        break;
                    }
                }
            }
        }

        public static void CheckItemCount()
        {
            TaskLog.ForEach(q => q.CheckItems());
        }
        public static void AttemptProgressCraft(Item i)
        {
            TaskLog.ForEach(q => q.AttemptProgressCraft(i));
        }
        public static RHTask GetTaskByID(int taskID)
        {
            return _liTasks.Find(t => t.ID == taskID);
        }

        public static void HandleQueuedTask()
        {
            QueuedHandin?.TurnInTask();
            QueuedHandin = null;
        }

        public static List<TaskData> SaveTaskData()
        {
            List<TaskData> rv = new List<TaskData>();
            foreach (RHTask q in TaskManager._liTasks)
            {
                rv.Add(q.SaveData());
            }

            return rv;
        }
        public static void LoadTaskData(List<TaskData> list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                _liTasks[i].LoadData(list[i]);
            }

            //ToDo Code for new/custom tasks
        }
    }
}
