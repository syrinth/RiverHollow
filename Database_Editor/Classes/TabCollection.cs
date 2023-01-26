using System.Collections.Generic;
using static Database_Editor.Classes.Constants;

namespace Database_Editor.Classes
{
    internal struct TabCollection
    {
        public XMLTypeEnum XMLType { get; }
        public List<string> DefaultTags { get; }// = null

        public string TagsReferenced { get; }
        public string TagsThatReferToMe { get; }

        public TabCollection(XMLTypeEnum xmlType, string tagsReferenced, string tagsToMe, string defaultTags)
        {
            XMLType = xmlType;
            TagsReferenced = tagsReferenced;
            TagsThatReferToMe = tagsToMe;

            DefaultTags = new List<string>();
            string[] tags = defaultTags.Split(',');
            for(int i=0; i<tags.Length; i++)
            {
                DefaultTags.Add(tags[i]);
            }
        }
    }
}
