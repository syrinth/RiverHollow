using RiverHollow.Characters;
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
            _liTasks.Find(x => x.TaskID == taskID).AssignTaskToNPC();
        }

        public static bool TaskCompleted(int taskID)
        {
            return _liTasks.Find(x => x.TaskID == taskID).TaskState == TaskStateEnum.Completed;
        }

        public static void AddToTaskLog(int taskID)
        {
            AddToTaskLog(_liTasks.Find(x => x.TaskID == taskID));
        }
        public static void AddToTaskLog(RHTask t)
        {
            TaskLog.Add(t);
        }

        public static void AddDelayedTask(int taskID)
        {
            _liDelayedTasks.Add(_liTasks.Find(x => x.TaskID == taskID));
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
                if (q.AttemptStructureBuildProgress(obj.ID))
                {
                    break;
                }
            }
        }
        public static void AdvanceTaskProgress(Monster m)
        {
            foreach (RHTask q in TaskLog)
            {
                if (q.AttemptProgress(m))
                {
                    break;
                }
            }
        }
        public static void AdvanceTaskProgress(Item i)
        {
            foreach (RHTask q in TaskLog)
            {
                if (q.AttemptProgress(i))
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
        public static void RemoveTaskProgress(Item i)
        {
            foreach (RHTask q in TaskLog)
            {
                if (q.RemoveProgress(i))
                {
                    break;
                }
            }
        }
        public static bool HasTaskID(int taskID)
        {
            return TaskLog.Find(t => t.TaskID == taskID) != null;
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

            AssignTasks();

            //ToDo Code for new/custom tasks
        }
    }
}
