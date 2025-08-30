using RiverHollow.Game_Managers;
using RiverHollow.Utilities;
using System.Collections.Generic;
using static Database_Editor.Classes.Constants;

namespace Database_Editor.Classes
{
    public class XMLData
    {
        protected struct LinkedObject
        {
            public TMXData MapData;
            public XMLData ObjectData;
            public string LinkedTag;

            public LinkedObject(XMLData data, string tag)
            {
                MapData = null;
                ObjectData = data;
                LinkedTag = tag;
            }
            public LinkedObject(TMXData data, string tag)
            {
                MapData = data;
                ObjectData = null;
                LinkedTag = tag;
            }
        }
        protected string _sName;
        public string Name => _sName;
        protected string _sDescription;
        public string Description => _sDescription;
        protected XMLTypeEnum _eXMLType;
        protected int _iID;
        public int ID => _iID;
        protected List<string> _liTagsReferenced;
        protected List<string> _liTagsThatReferToMe;
        public List<string> TagsThatReferToMe => _liTagsThatReferToMe;
        protected List<LinkedObject> _liLinkedObjects;
        protected List<LinkedObject> _liLinkedTextObjects;
        protected List<LinkedObject> _liLinkedMaps;
        protected List<List<string>> _liLinkedCutsceneData;
        protected Dictionary<string, string> _diTags;

        public XMLData(string id, Dictionary<string, string> stringData, string tagsReferenced, string tagsThatReferToMe, XMLTypeEnum xmlType, ref Dictionary<string, Dictionary<string, string>> objectData)
        {
            _liLinkedMaps = new List<LinkedObject>();
            _liLinkedObjects = new List<LinkedObject>();
            _liLinkedTextObjects = new List<LinkedObject>();
            _liLinkedCutsceneData = new List<List<string>>();
            _liTagsReferenced = new List<string>(tagsReferenced.Split(','));
            _liTagsThatReferToMe = new List<string>(tagsThatReferToMe.Split(','));

            string textID = Util.GetEnumString(xmlType) + "_" + id;
            if (xmlType == XMLTypeEnum.TextFile)
            {
                if (stringData.ContainsKey("Name"))
                {
                    _sName = stringData["Name"];
                }
            }
            else if (xmlType != XMLTypeEnum.None)
            {
                if (objectData.ContainsKey(textID))
                {
                    _sName = objectData[textID]["Name"];

                    if (objectData[textID].ContainsKey("Description"))
                    {
                        _sDescription = objectData[textID]["Description"];
                    }
                }
            }

            _iID = int.Parse(id);
            _diTags = stringData;

            _eXMLType = xmlType;
        }
        public XMLData(string id, string stringData, string tagsReferenced, string tagsThatReferToMe, XMLTypeEnum xmlType, ref Dictionary<string, Dictionary<string, string>> objectData) : this(id, DataManager.TaggedStringToDictionary(stringData), tagsReferenced, tagsThatReferToMe, xmlType, ref objectData) { }

        public string GetStringValue(string value)
        {
            return _diTags[value];
        }

        public string GetTagsString()
        {
            string rv = string.Empty;

            foreach (KeyValuePair<string, string> kvp in _diTags)
            {
                rv += "[" + kvp.Key + (string.IsNullOrEmpty(kvp.Value) ? "" : ":" + kvp.Value) + "]";
            }

            return rv;
        }

        public bool HasTag(string key)
        {
            return _diTags.ContainsKey(key);
        }
        public string GetTagValue(string key)
        {
            if (_diTags.ContainsKey(key)) { return _diTags[key]; }
            else { return string.Empty; }
        }
        public void SetTextData(string name)
        {
            _sName = name;
        }
        public void SetTextData(string name, string desc)
        {
            _sName = name;
            _sDescription = desc;
        }
        public void SetTagInfo(string key, string value)
        {
            if (_diTags.ContainsKey(key))
            {
                _diTags[key] = _diTags[key] + "/" + value;
            }
            else
            {
                _diTags[key] = value;
            }
        }
        public void AppendToTag(string key, string value)
        {
            _diTags[key] += "/" + value;
        }
        public void ClearTagInfo()
        {
            _diTags.Clear();
        }

        /// <summary>
        /// Checks the tags to see if there are any references to the given ID
        /// among the relevant tags.
        /// </summary>
        /// <param name="id">The ID to look for</param>
        /// <returns>True if there is at least one match</returns>
        public void CheckForObjectLink(XMLData testData)
        {
            foreach (string s in _liTagsReferenced)
            {
                if (testData._liTagsThatReferToMe.Contains(s))
                {
                    if (CheckTagForID(s, testData.ID))
                    {
                        testData.AddLinkedObject(this, s);
                    }
                }
            }

            //Search text and name entries
            if (_diTags.ContainsKey("Text"))
            {
                if (_diTags["Text"].Contains(LOOKUP_CHARACTER + testData.GetObjectTextID() + LOOKUP_CHARACTER))
                {
                    testData.AddLinkedTextEntry(this, "Text");
                }
            }

            if (_diTags.ContainsKey("Name"))
            {
                if (_diTags["Name"].Contains(LOOKUP_CHARACTER + testData.GetObjectTextID() + LOOKUP_CHARACTER))
                {
                    testData.AddLinkedTextEntry(this, "Name");
                }
            }
        }

        /// <summary>
        /// Call this to check the given tag for the given ID. This method is to
        /// be used for any entry that has multiples of the same type of thing in it
        /// that are delineated by a '|'. For example, a tag for the multiple things a Machine
        /// can make
        /// </summary>
        /// <param name="tag">Tag to look at</param>
        /// <param name="id">The ID to look for</param>
        /// <param name="val">Reference to the success or this and other checks</param>
        /// <returns>True if a match exists</returns>
        private bool CheckTagForID(string tag, int id)
        {
            bool rv = false;

            //If we don't have the key, don't proceed
            if (_diTags.ContainsKey(tag))
            {
                //Isolate every group of entries that are delineated by the '|'
                string[] split = Util.FindParams(_diTags[tag]);
                foreach (string s in split)
                {
                    //The first entry is always the object, split by the '-', find it and compare
                    string[] splitData = s.Split('-');

                    if (_eXMLType == XMLTypeEnum.Actor && tag == "MonsterID")
                    {
                        for (int i = 0; i < splitData.Length; i++)
                        {
                            if (int.Parse(splitData[i]) == id)
                            {
                                rv = true;
                            }
                        }
                    }
                    else if(int.TryParse(splitData[0], out _))
                    {
                        if (int.Parse(splitData[0]) == id)
                        {
                            rv = true;
                            break;
                        }
                    }
                }
            }

            return rv;
        }

        /// <summary>
        /// Method to change the ID of the data from one value to another.
        /// 
        /// After changing, it's important to iterate over all the linked entries
        /// and tell them to replace the old ID with the new one.
        /// </summary>
        /// <param name="newID"></param>
        public virtual void ChangeID(int newID, bool item = true)
        {
            if (_iID != newID)
            {
                int oldID = _iID;
                _iID = newID;

                foreach (LinkedObject d in _liLinkedObjects)
                {
                    XMLData data = d.ObjectData;
                    data.ReplaceLinkedIDs(oldID, _iID, d.LinkedTag);
                }

                foreach (LinkedObject d in _liLinkedMaps)
                {
                    TMXData data = d.MapData;
                    data.ReplaceID(oldID, newID, d.LinkedTag);
                }

                foreach(LinkedObject d in _liLinkedTextObjects)
                {
                    XMLData data = d.ObjectData;
                    data.ReplaceTextIDValue(oldID, newID, d.LinkedTag, _eXMLType);
                }

                for (int i = 0; i < _liLinkedCutsceneData.Count; i++) 
                {
                    for (int j = 0; j < _liLinkedCutsceneData[i].Count; j++)
                    {
                        foreach (string tag in TagsThatReferToMe)
                        {
                            if (_liLinkedCutsceneData[i][j].Contains(tag + ":" + oldID + "-"))
                            {
                                _liLinkedCutsceneData[i][j] = _liLinkedCutsceneData[i][j].Replace(tag + ":" + oldID + "-", tag + ":" + SPECIAL_CHARACTER + newID + SPECIAL_CHARACTER + "-");
                            }
                            else if (_liLinkedCutsceneData[i][j].Contains(tag + ":" + oldID + "]"))
                            {
                                _liLinkedCutsceneData[i][j] = _liLinkedCutsceneData[i][j].Replace(tag + ":" + oldID + "]", tag + ":" + SPECIAL_CHARACTER + newID + SPECIAL_CHARACTER + "]");
                            }
                        }
                    }
                }
            }
        }

        public void ReplaceTextIDValue(int oldID, int newID, string tag, XMLTypeEnum xmlType)
        {
            //If we don't have the key, don't proceed
            if (_diTags.ContainsKey(tag))
            {
                _diTags[tag] = _diTags[tag].Replace(LOOKUP_CHARACTER + ConstructObjectTextID(xmlType, oldID) + LOOKUP_CHARACTER, LOOKUP_CHARACTER + SPECIAL_CHARACTER + ConstructObjectTextID(xmlType, newID) + SPECIAL_CHARACTER + LOOKUP_CHARACTER);
            }
        }

        /// <summary>
        /// Iterates through the relevant tags to replace any instances of the 
        /// old ID with the new ID.
        /// </summary>
        /// <param name="oldID">The old ID that has now changed</param>
        /// <param name="newID">The new ID to reference</param>
        /// <param name="tag">The specific tag to replace</param>
        public void ReplaceLinkedIDs(int oldID, int newID, string tag)
        {
            ReplaceID(tag, oldID, newID);
        }

        /// <summary>
        /// Call this to check the given tag for the given ID.
        /// 
        /// Replace any instances of the old ID that are found with the new ID
        /// /// </summary>
        /// <param name="tag">Tag to look at</param>
        /// <param name="oldID">The ID to look for</param>
        /// <param name="newID">The ID to replace the olf one with</param>
        public void ReplaceID(string tag, int oldID, int newID)
        {
            //If we don't have the key, don't proceed
            if (_diTags.ContainsKey(tag))
            {
                //Isolate every group of entries that are delineated by the '|'
                string[] split = Util.FindParams(_diTags[tag]);
                _diTags[tag] = string.Empty;
                for (int i = 0; i < split.Length; i++)
                {
                    //The first entry is always the item, split by the '-', find it and compare
                    //If the value matches, replace the split string id with the newID surrounded by
                    //the special character. The special character prevents subsequent changes from
                    //overwriting this change.
                    string[] splitData = split[i].Split('-');

                    if (_eXMLType == XMLTypeEnum.Actor && tag == "MonsterID")
                    {
                        for (int j = 0; j < splitData.Length; j++)
                        {
                            if (splitData[j] == oldID.ToString())
                            {
                                splitData[j] = SPECIAL_CHARACTER + newID.ToString() + SPECIAL_CHARACTER;
                            }
                        }
                    }
                    else
                    {
                        if (splitData[0] == oldID.ToString())
                        {
                            splitData[0] = SPECIAL_CHARACTER + newID.ToString() + SPECIAL_CHARACTER;
                        }
                    }

                    //Iterate over any linked values and concatenate them to re-add them to the entry
                    for (int j = 0; j < splitData.Length; j++)
                    {
                        _diTags[tag] += splitData[j];

                        //If there is a linked value, add a '-' and continue
                        if (j < splitData.Length - 1)
                        {
                            _diTags[tag] += "-";
                        }
                    }

                    //There may or may not be any additional values, if there are more coming, add the '|'
                    if (i < split.Length - 1)
                    {
                        _diTags[tag] += "/";
                    }
                }
            }
        }

        /// <summary>
        /// Adds the given XMLData to the LinkedITems list.
        /// 
        /// Do nto do this if the list already contains it or if the
        /// XMLData is this entry.
        /// </summary>
        /// <param name="d">The linked entry to add</param>
        public void AddLinkedObject(XMLData d, string tag)
        {
            if (this != d)
            {
                _liLinkedObjects.Add(new LinkedObject(d, tag));
            }
        }
        public void AddLinkedMap(TMXData d, string tag)
        {
            _liLinkedMaps.Add(new LinkedObject(d, tag));
        }
        public void AddLinkedTextEntry(XMLData d, string tag)
        {
            if (this != d)
            {
                _liLinkedTextObjects.Add(new LinkedObject(d, tag));
            }
        }
        public void AddLinkedCutscene(List<string> cutsceneData)
        {
            Util.AddUniquelyToList(ref _liLinkedCutsceneData, cutsceneData);
        }

        /// <summary>
        /// Iterates through each tag and remove all instances of the special character
        /// </summary>
        public void StripSpecialCharacter()
        {
            foreach (string s in new List<string>(_diTags.Keys))
            {
                if (_diTags[s].Contains(SPECIAL_CHARACTER))
                {
                    string val = _diTags[s];
                    _diTags[s] = val.Replace(SPECIAL_CHARACTER, "");
                }
            }
        }

        public string GetObjectTextID()
        {
            return ConstructObjectTextID(_eXMLType, ID);
        }

        public static string ConstructObjectTextID(XMLTypeEnum xmlType, int ID)
        {
            return Util.GetEnumString(xmlType) + "_" + ID.ToString();
        }
    }
}
