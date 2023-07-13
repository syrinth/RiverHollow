using RiverHollow.Characters;
using RiverHollow.Game_Managers;
using RiverHollow.Utilities;
using RiverHollow.WorldObjects;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static RiverHollow.Utilities.Enums;

namespace RiverHollow.Misc
{
    public class TextEntry
    {
        public string Key { get; } = string.Empty;

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
            _diTags = new Dictionary<string, string>();
        }

        //This should be used as little as possible.
        public TextEntry(string text) : this()
        {
            _sText = text;
            _diTags = new Dictionary<string, string>();
        }

        public TextEntry(string key, Dictionary<string, string> stringData)
        {
            _diTags = stringData;

            Key = key;

            Util.AssignValue(ref _sText, "Text", stringData);
            Util.AssignValue(ref _dPriority, "Priority", stringData);
            Util.AssignValue(ref _eSelectionType, "Selection", stringData);
            Util.AssignValue(ref _eGameTrigger, "Trigger", stringData);
            Util.AssignValue(ref _eVerb, "TextVerb", stringData);
        }

        public string GetFormattedText()
        {
            return Util.ProcessText(_sText);
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
            if (_sText != null)
            {
                _sText = string.Format(_sText, list);
            }
        }

        /// <summary>
        /// Removes unneeded entries from the possible list of options given by the text.
        /// </summary>
        /// <param name="options">The string list of options</param>
        /// <param name="npc">The Villager we are talking to.</param>
        /// <returns></returns>
        private List<string> RemoveEntries(string[] options, Villager npc)
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
                    string[] specialVal = Util.FindArguments(specialParse[0]);

                    int.TryParse(specialVal[1], out int val);

                    if (npc != null && specialVal[0].Equals("Friend"))
                    {
                        removeIt = npc.FriendshipPoints < val;
                    }

                    s = s.Remove(s.IndexOf(specialParse[0]) - 1, specialParse[0].Length + 2);
                }
                else if (npc != null)
                {
                    if (!npc.CanGiveGift && s.Contains("GiveGift"))
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

        public bool Validate()
        {
            bool rv = false;
            if (_diTags.ContainsKey("GameStart"))
            {
                if (GameCalendar.CurrentDay == 1 && GameCalendar.CurrentSeason == SeasonEnum.Spring && GameCalendar.CurrentYear == 1)
                {
                    rv = true;
                }
            }

            return rv;
        }

        /// <summary>
        /// This method is used to ensure that conditions are being met for TextEntries
        /// to be used. This method should only be used by Villagers.
        /// </summary>
        /// <param name="act">The Villager we are talking to</param>
        /// <returns>True if all conditions are current active.</returns>
        public bool Validate(TalkingActor act = null)
        {
            bool rv = false;

            if (_bSpoken) { return false; }

            //Default is always valid
            if (_diTags.ContainsKey("Default")) { rv = true; }
            else
            {
                if (_diTags.ContainsKey("MaxChildren"))
                {
                    if (PlayerManager.Children.Count < int.Parse(_diTags["MaxChildren"])) { rv = true; }
                    else { return false; }
                }
                if (_diTags.ContainsKey("Chance"))
                {
                    if (RHRandom.Instance().RollPercent(int.Parse(_diTags["Chance"]))) { rv = true; }
                    else { return false; }
                }
                if (_diTags.ContainsKey("Married"))
                {
                    if (((Villager)act).Married) { rv = true; }
                    else { return false; }
                }
                if (_diTags.ContainsKey("NotExpecting"))
                {
                    if (PlayerManager.ChildStatus == ExpectingChildEnum.None) { rv = true; }
                    else { return false; }
                }
                if (_diTags.ContainsKey("Weather"))
                {
                    if (EnvironmentManager.GetWeatherString().Equals(_diTags["Weather"])) { rv = true; }
                    else { return false; }
                }
                if (_diTags.ContainsKey("Friend"))
                {
                    string[] args = Util.FindArguments(_diTags["Friend"]);
                    if (args.Length == 2)
                    {
                        if (int.TryParse(args[1], out int NPCID) && act.GetFriendshipLevel() >= NPCID) { rv = true; }
                        else { return false; }
                    }
                    else if (args.Length == 3)
                    {
                        if (int.TryParse(args[1], out int NPCID) && int.TryParse(args[2], out int tempLevel) && TownManager.DIVillagers[NPCID].GetFriendshipLevel() > tempLevel) { rv = true; }
                        else { return false; }
                    }
                }
                if (_diTags.ContainsKey("RequiredObjectID"))
                {
                    foreach (string i in Util.FindArguments(_diTags["RequiredObjectID"]))
                    {
                        if (!TownManager.TownObjectBuilt(int.Parse(i))) { return false; }
                    }

                    rv = true;
                }
                if (_diTags.ContainsKey("Villager"))
                {
                    foreach (string i in Util.FindArguments(_diTags["Villager"]))
                    {
                        if (!TownManager.DIVillagers[int.Parse(i)].LivesInTown) { return false; }
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
            if (_diTags != null)
            {
                if (_diTags.ContainsKey("Face"))
                {
                    act?.QueueActorFace(_diTags["Face"]);
                }
            }
        }

        /// <summary>
        /// This method is called as a GUITextWindow is being closed. Perform any special actions that need to be done here.
        /// </summary>
        /// <param name="act">The TalkingActor we're talking to</param>
        public void HandlePostWindowActions()
        {
            if (_diTags.ContainsKey("UnlockObjectID"))
            {
                PlayerManager.AddToCraftingDictionary(int.Parse(_diTags["UnlockObjectID"]));
            }
            if (_diTags.ContainsKey("ItemID"))
            {
                string[] itemSplit = Util.FindParams(_diTags["ItemID"]);
                for (int i = 0; i < itemSplit.Length; i++)
                {
                    string[] split = Util.FindArguments(itemSplit[i]);
                    InventoryManager.AddToInventory(int.Parse(split[0]), split.Length == 1 ? 1 : int.Parse(split[1]));
                }
            }
            if (_diTags.ContainsKey("UnlockItemID"))
            {
                GameManager.DIShops[int.Parse(_diTags["ShopTargetID"])].UnlockMerchandise(int.Parse(_diTags["UnlockItemID"]));
            }
            if (_diTags.ContainsKey("SendMessage"))
            {
                PlayerManager.PlayerMailbox.SendMessage(_diTags["SendMessage"]);
            }
            if (_diTags.ContainsKey("SendToTown"))
            {
                ((Villager)GameManager.CurrentNPC).QueueSendToTown();
            }
        }

        public void Spoken(TalkingActor act)
        {
            if (act != null && _diTags.ContainsKey("Once"))
            {
                _bSpoken = true;
                act.AddSpokenKey(Key);
            }
        }
    }
}
