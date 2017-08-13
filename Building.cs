using Adventure.Characters.NPCs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure
{
    class Building
    {
        private const int MaxWorkers = 9;
        protected Worker[] _workers;
        public Worker[] Workers { get => _workers; }

        protected int _id;
        public int ID { get => _id; }

        //returns -1 if there is no room, else returns the first slot that's open
        public int HasSpace()
        {
            int rv = -1;

            for(int i=0; i<_workers.Length; i++)
            {
                if(_workers[i] == null)
                {
                    rv = i;
                }
            }
            return rv;
        }

        //call HasSpace before adding
        public bool AddWorker(Worker worker, int index)
        {
            bool rv = false;

            if(worker != null && index < _workers.Length && _workers[index] == null)
            {
                _workers[index] = worker;
            }

            return rv;
        }
    }
}
