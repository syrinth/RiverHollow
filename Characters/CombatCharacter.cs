using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure.Characters
{
    public class CombatCharacter : Character
    {
        protected int _hp = 10;
        public int HitPoints
        {
            get { return _hp; }
            set { _hp = value; }
        }
    }
}
