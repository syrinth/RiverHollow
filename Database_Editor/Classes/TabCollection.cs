using RiverHollow.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static Database_Editor.Classes.Constants;

namespace Database_Editor.Classes
{
    internal struct TabCollection
    {
        public XMLTypeEnum XMLType { get; }
        public List<string> DefaultTags { get; }// = null

        public TabCollection(XMLTypeEnum xmlType, List<string> defaultTags = null)
        {
            XMLType = xmlType;
            DefaultTags = defaultTags;
        }
    }
}
