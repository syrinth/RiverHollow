using RiverHollow.Utilities;
using System.Collections.Generic;
using static Database_Editor.Classes.Constants;
using static RiverHollow.Utilities.Enums;

namespace Database_Editor.Classes
{
    public class ItemXMLData : XMLData
    {
        ItemEnum _eType;
        public ItemEnum ItemType => _eType;

        public ItemXMLData(string id, Dictionary<string, string> stringData, string tagsReferenced, string tagsThatReferenceMe, ref Dictionary<string, Dictionary<string, string>> objectData) : base(id, stringData, tagsReferenced, tagsThatReferenceMe, XMLTypeEnum.Item, ref objectData)
        {
            _eType = Util.ParseEnum<ItemEnum>(_diTags["Type"]);
            string textID = "Item_" + id;
        }

        public void SetItemType(ItemEnum e)
        {
            _eType = e;
        }
    }
}
