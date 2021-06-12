﻿using RiverHollow.Characters;
using RiverHollow.Game_Managers;
using RiverHollow.Utilities;
using System;
using System.Collections.Generic;
using static RiverHollow.Game_Managers.GameManager;

namespace RiverHollow.Misc
{
    public class TextEntry
    {
        string _sKey = string.Empty;

        //The type of selection menu to create, identifies the options
        TextEntrySelectionEnum _eSelectionType = TextEntrySelectionEnum.None;
        public TextEntrySelectionEnum SelectionType => _eSelectionType;

        //The type of selection taking place. Determines what the chosen action will do
        TextEntryTriggerEnum _eGameTrigger = TextEntryTriggerEnum.None;
        public TextEntryTriggerEnum GameTrigger => _eGameTrigger;

        //The tied result to the indicated option
        TextEntryVerbEnum _eVerb = TextEntryVerbEnum.None;
        public TextEntryVerbEnum TextVerb => _eVerb;

        Dictionary<string, string> _diTags;
        //int _iLookupID = -1;

        public string NextEntry => _diTags.ContainsKey("NextEntry") ? _diTags["NextEntry"] : string.Empty;
        private string _sText;
        public string FormattedText { get; private set; }
        double _dPriority = 100;
        public double Priority => _dPriority;

        bool _bSpoken = false;
        public bool Selection => SelectionType != TextEntrySelectionEnum.None;

        /// <summary>
        /// Default TextEntry, this should never be seen.
        /// </summary>
        public TextEntry()
        {
            _sText = "No Text Assigned.";
        }

        //This should be used as little as possible.
        public TextEntry(string text)
        {
            _sText = text;
            FormattedText = _sText;
        }

        public TextEntry(string key, Dictionary<string, string> stringData)
        {
            _diTags = stringData;

            _sKey = key;

            Util.AssignValue(ref _sText, "Text", stringData);
            Util.AssignValue(ref _dPriority, "Priority", stringData);
            Util.AssignValue(ref _eSelectionType, "Selection", stringData);
            Util.AssignValue(ref _eGameTrigger, "Trigger", stringData);
            Util.AssignValue(ref _eVerb, "TextVerb", stringData);

            FormattedText = Util.ProcessText(_sText);
        }

        /// <summary>
        /// Use to determine if the tag dictionary contains the indicated key
        /// </summary>
        /// <param name="key">Key to look for</param>
        /// <returns>True if the key exists in the tag dictionary</returns>
        public bool HasTag(string key)
        {
            return _diTags.ContainsKey(key);
        }

        /// <summary>
        /// Runs string.Format against the _sText using the list as the parameters.
        /// </summary>
        /// <param name="list">List of variables to format</param>
        public void FormatText(params object[] list)
        {
            FormattedText = string.Format(_sText, list);
        }

        /// <summary>
        /// Removes unneeded entries from the possible list of options given by the text.
        /// </summary>
        /// <param name="options">The string list of options</param>
        /// <param name="act">The Villager we are talking to.</param>
        /// <returns></returns>
        private List<string> RemoveEntries(string[] options, Villager act)
        {
            List<string> _liCommands = new List<string>();
            for (int i = 0; i < options.Length; i++)
            {
                bool removeIt = false;
                string s = options[i];

                if (s.Contains("%"))
                {
                    //Special checks are in the format %type:val% so, |%Friend:50%Join Party:Party| or |%Task:1%Business:Task1|
                    string[] specialParse = s.Split(new[] { '%' }, StringSplitOptions.RemoveEmptyEntries);
                    string[] specialVal = specialParse[0].Split('-');

                    int.TryParse(specialVal[1], out int val);

                    if (act != null && specialVal[0].Equals("Friend"))
                    {
                        removeIt = act.FriendshipPoints < val;
                    }
                    else if (specialVal[0].Equals("Task"))
                    {
                        Task newTask = GameManager.DITasks[val];
                        removeIt = PlayerManager.TaskLog.Contains(newTask) || newTask.ReadyForHandIn || newTask.Finished || !newTask.CanBeGiven();
                    }

                    s = s.Remove(s.IndexOf(specialParse[0]) - 1, specialParse[0].Length + 2);
                }
                else if (act != null)
                {
                    if (!act.CanGiveGift && s.Contains("GiveGift"))
                    {
                        removeIt = true;
                    }
                }

                if (!removeIt)
                {
                    _liCommands.Add(s);
                }
            }

            return _liCommands;
        }

        /// <summary>
        /// This method is used to ensure that conditions are being met for TextEntries
        /// to be used. This method should only be used by Villagers.
        /// </summary>
        /// <param name="act">The Villager we are talking to</param>
        /// <returns>True if all conditions are current active.</returns>
        public bool Valid(Villager act = null)
        {
            bool rv = false;

            if (_bSpoken) { return false; }

            //Default is always valid
            if (_diTags.ContainsKey("Default")) { rv = true; }
            else
            {
                if (_diTags.ContainsKey("Weather"))
                {
                    if (EnvironmentManager.GetWeatherString().Equals(_diTags["Weather"])) { rv = true; }
                    else { return false; }
                }
                if (_diTags.ContainsKey("Friend"))
                {
                    string[] args = _diTags["Friend"].Split('-');
                    if (args.Length == 2)
                    {
                        if (int.TryParse(args[1], out int NPCID) && act.GetFriendshipLevel() >= NPCID) { rv = true; }
                        else { return false; }
                    }
                    else if (args.Length == 3)
                    {
                        if (int.TryParse(args[1], out int NPCID) && int.TryParse(args[2], out int tempLevel) && DataManager.DiNPC[NPCID].GetFriendshipLevel() > tempLevel) { rv = true; }
                        else { return false; }
                    }
                }
                if (_diTags.ContainsKey("RequiredBuildingID"))
                {
                    foreach (string i in _diTags["RequiredBuildingID"].Split('-'))
                    {
                        if (!GameManager.DIBuildInfo[int.Parse(i)].Built) { return false; }
                    }

                    rv = true;
                }
                if (_diTags.ContainsKey("Villager"))
                {
                    foreach (string i in _diTags["Villager"].Split('-'))
                    {
                        if (!DataManager.DiNPC[int.Parse(i)].LivesInTown) { return false; }
                    }

                    rv = true;
                }
            }

            return rv;
        }

        /// <summary>
        /// This method is called before a GUITextWindow is opened. Perform any special actions that need to be done here
        /// </summary>
        /// <param name="act">The TalkingActor we're talking to</param>
        public void HandlePreWindowActions(TalkingActor act = null)
        {
            Spoken(act);
            if (_diTags != null && _diTags.ContainsKey("Face"))
            {
                act?.QueueActorFace(_diTags["Face"]);
            }
        }

        /// <summary>
        /// This method is called as a GUITextWindow is being closed. Perform any special actions that need to be done here.
        /// </summary>
        /// <param name="act">The TalkingActor we're talking to</param>
        public void HandlePostWindowActions(TalkingActor act = null)
        {
            if (_diTags.ContainsKey("Task"))
            {
                PlayerManager.AddToTaskLog(GameManager.DITasks[int.Parse(_diTags["Task"])]);
            }
            if (_diTags.ContainsKey("UnlockBuildingID"))
            {
                GameManager.DIBuildInfo[int.Parse(_diTags["UnlockBuildingID"])].Unlock();
            }
            if (_diTags.ContainsKey("UnlockItemID"))
            {
                GameManager.DIShops[int.Parse(_diTags["ShopTargetID"])].Find(x => x.MerchID == int.Parse(_diTags["UnlockItemID"])).Unlock();
            }
            if (_diTags.ContainsKey("SendMessage"))
            {
                PlayerManager.PlayerMailbox.SendMessage(_diTags["SendMessage"]);
            }
        }

        public void Spoken(TalkingActor act)
        {
            if (act != null && _diTags.ContainsKey("Once"))
            {
                _bSpoken = true;
                act.AddSpokenKey(_sKey);
            }
        }
    }
}
