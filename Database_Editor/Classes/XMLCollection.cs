using System.Collections.Generic;
using static Database_Editor.Classes.Constants;

namespace Database_Editor.Classes
{
    internal struct XMLCollection
    {
        public XMLTypeEnum XMLType { get; }
        public string TagsReferenced { get; }
        public string TagsThatReferToMe { get; }

        public XMLCollection(XMLTypeEnum xmlType, string tagsReferenced, string tagsToMe)
        {
            XMLType = xmlType;
            TagsReferenced = tagsReferenced;
            TagsThatReferToMe = tagsToMe;
        }
    }
}
