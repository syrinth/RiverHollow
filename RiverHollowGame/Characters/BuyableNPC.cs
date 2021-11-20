using RiverHollow.Utilities;
using System.Collections.Generic;

namespace RiverHollow.Characters
{
    public abstract class BuyableNPC : TalkingActor
    {
        int _iValue;
        public int Value => _iValue;

        public BuyableNPC(Dictionary<string, string> stringData) : base()
        {
            Util.AssignValue(ref _iValue, "Value", stringData);
        }
    }
}
