using System.Collections.Generic;
using static RiverHollow.Utilities.Enums;

namespace RiverHollow.WorldObjects
{
    public class Tree : Destructible
    {
        public Tree(int id, Dictionary<string, string> stringData) : base(id, stringData)
        {
            _eToolType = ToolEnum.Axe;
            LoadDictionaryData(stringData);
            _rBase.X = 1;
            _rBase.Y = 3;
        }
    }
}
