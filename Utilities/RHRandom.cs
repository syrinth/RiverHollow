using System;
using System.Threading;

namespace RiverHollow.Misc
{    
    public class RHRandom : Random
    {
        public RHRandom() : base()
        {
            Thread.Sleep(1);
        }

        public override int Next(int min, int max)
        {
            //Thread.Sleep(1);
            int rv = 0;
            rv = base.Next(min, max + 1);
            return rv;
        }
    }
}
