using RiverHollow.Utilities;
using System.Collections.Generic;

namespace RiverHollow.Characters
{
    public abstract class BuyableNPC : TalkingActor
    {
        int _iValue;
        public int Value => _iValue;

        public BuyableNPC(int id, Dictionary<string, string> stringData) : base(id)
        {
            Util.AssignValue(ref _iValue, "Value", stringData);
        }
    }
}
