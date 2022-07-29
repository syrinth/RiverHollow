using Microsoft.Xna.Framework;

namespace RiverHollow.Utilities
{
    public class RHTimer
    {
        public double TimerSpeed { get; }
        double _dCountdown;

        public RHTimer(double time)
        {
            TimerSpeed = time;
            _dCountdown = time;
        }

        public void Update(GameTime gTime)
        {
            if(TimerSpeed > 0)
            {
                _dCountdown -= gTime.ElapsedGameTime.TotalSeconds;
            }
        }

        public void Reset()
        {
            _dCountdown = TimerSpeed;
        }

        public bool Finished()
        {
            return _dCountdown <= 0;
        }
    }
}
