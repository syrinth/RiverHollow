using RiverHollow.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Database_Editor.Classes
{
    public class TMXData
    {
        List<string> _liAllLines;
        public List<string> AllLines => _liAllLines;
        public TMXData(string fileName)
        {
            _liAllLines = new List<string>();

            string line;
            string fullPathToFile = string.Format(@"{0}\..\..\..\..\Content\Maps\{1}", System.Environment.CurrentDirectory, fileName);
            System.IO.StreamReader file = new System.IO.StreamReader(fileName);
            while ((line = file.ReadLine()) != null)
            {
                _liAllLines.Add(line);
            }

            file.Close();
        }

        /// <summary>
        /// Call to determine if the TMX file refers to the referenced id in a given tag.
        /// Needs to be careful here because maps can refer to multiple things, so the tag
        /// input is very important to be coordinated with what ovbject type is being passed in
        /// </summary>
        /// <param name="data">The XML data file to compare against</param>
        /// <returns></returns>
        public void ReferencesXMLObject(XMLData data)
        {
            //Read through each line
            for (int i = 0; i < _liAllLines.Count; i++)
            {
                string s = _liAllLines[i];
                if (s.Contains("<property name"))
                {
                    int indexOfOpenBrace = s.IndexOf("<");

                    string[] propertyParams = s.Substring(indexOfOpenBrace).Split(' ');    //Find all the entries of the property tag
                    string propertyName = string.Empty;
                    string propertyValue = string.Empty;

                    //Which index is the value entry
                    GetNameAndValue(ref propertyName, ref propertyValue, propertyParams);

                    //We're going to loop through every tag we've been told to search for
                    List<string> referencedTags = new List<string>(Constants.MAP_REF_TAGS.Split(','));
                    foreach (string refTag in referencedTags)
                    {
                        if (data.TagsThatReferToMe.Contains(refTag))
                        {
                            if (propertyName.Equals(refTag))
                            {
                                //Split the values in the property value by the '|' delimeter 
                                string[] splitValues = Util.FindParams(propertyValue);
                                foreach (string spVal in splitValues)
                                {
                                    string[] splitArgs = spVal.Split('-');
                                    //Do we have a match? return true
                                    if (splitArgs[0] == data.ID.ToString())
                                    {
                                        data.AddLinkedMap(this, refTag);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Call this to check the given tag for the given ID.
        /// 
        /// Replace any instances of the old ID that are found with the new ID
        /// /// </summary>
        /// <param name="tag">Tags to look at, delmitited by ','</param>
        /// <param name="oldID">The ID to look for</param>
        /// <param name="newID">The ID to replace the olf one with</param>
        public void ReplaceID(int oldID, int newID, string tag)
        {
            //Read through every  line of the file
            for (int i = 0; i < _liAllLines.Count; i++)
            {
                string s = _liAllLines[i];

                //If the line is a property line, we need to read it
                if (s.Contains("<property name"))
                {
                    int indexOfOpenBrace = s.IndexOf("<");
                    string buffer = s.Substring(0, indexOfOpenBrace);       //Save this to preserve however many spaces are at the beginning of the line
                    string newValue = "value=\"";                           //The start of a value tag

                    string[] propertyParams = s.Substring(indexOfOpenBrace).Split(' ');    //Find all the entries of the property tag
                    string propertyName = string.Empty;
                    string propertyValue = string.Empty;

                    //Which index is the value entry
                    int valueIndex = GetNameAndValue(ref propertyName, ref propertyValue, propertyParams);

                    bool found = false;

                    //We're going to loop through every tag we've been told to search for

                    if (propertyName.Equals(tag))
                    {
                        //Split the values in the property value by the '|' delimeter 
                        string[] splitValues = Util.FindParams(propertyValue);
                        for (int j = 0; j < splitValues.Length; j++)
                        {
                            string[] splitArgs = splitValues[j].Split('-');
                            //If we found a match, set the flag to true and overwrite the value of this string
                            if (splitArgs[0] == oldID.ToString())
                            {
                                found = true;
                                splitArgs[0] = Constants.SPECIAL_CHARACTER + newID.ToString() + Constants.SPECIAL_CHARACTER;
                            }

                            //Concatenate it to the newValue
                            newValue += splitArgs[0];

                            //If there are more entries coming, add the '|' back
                            if (splitArgs.Length > 1)
                            {
                                for (int k = 1; k < splitArgs.Length; k++)
                                {
                                    newValue = newValue + "-" + splitArgs[k];
                                }
                            }

                            //If there are more entries coming, add the '|' back
                            if (j < splitValues.Length - 1)
                            {
                                newValue += "|";
                            }
                        }

                        //Close the quote
                        newValue += "\"";
                    }

                    //Put the buffer back at the beginning of the line
                    _liAllLines[i] = buffer;
                    for (int j = 0; j < propertyParams.Length; j++)
                    {
                        //Either write the params as we get them, or sub in the dummy value, value is always
                        //last so we needto close the tag.
                        if (j == valueIndex && found) { _liAllLines[i] += newValue + "/>"; }
                        else { _liAllLines[i] += propertyParams[j]; }

                        //If there's another entry coming, put a space there
                        if (j < propertyParams.Length - 1)
                        {
                            _liAllLines[i] += " ";
                        }
                    }
                }
            }
        }

        /// <summary>
        /// This method will iterate through an array of property parameters and retrieve
        /// the value of the name and the value param.
        /// </summary>
        /// <param name="propertyName">ref to the propertyName string</param>
        /// <param name="propertyValue">ref to the propertyValue string</param>
        /// <param name="propertyParams">The propery param arrays</param>
        /// <returns>The index of the value parameter</returns>
        private int GetNameAndValue(ref string propertyName, ref string propertyValue, string[] propertyParams)
        {
            int rv = -1;
            //Iterate over the property parameters and collect the name of the property and its value
            for (int j = 0; j < propertyParams.Length; j++)
            {
                if (propertyParams[j].Contains("="))
                {
                    string[] splitParam = propertyParams[j].Split('=');
                    string pName = splitParam[0].Replace("\"", "");
                    string pValue = splitParam[1].Replace("\"", "").Replace("/", "").Replace(">", "");

                    if (pName.Equals("name")) { propertyName = pValue; }
                    else if (pName.Equals("value"))
                    {
                        rv = j;
                        propertyValue = pValue;
                    }
                }
            }

            return rv;
        }

        /// <summary>
        /// Iterates through each line and remove all instances of the special character
        /// </summary>
        public void StripSpecialCharacter()
        {
            for (int i = 0; i < _liAllLines.Count; i++)
            {
                string s = _liAllLines[i];
                if (s.Contains(Constants.SPECIAL_CHARACTER))
                {
                    string val = s;
                    _liAllLines[i] = val.Replace(Constants.SPECIAL_CHARACTER, "");
                }
            }
        }
    }
}
