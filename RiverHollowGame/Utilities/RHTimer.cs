using Microsoft.Xna.Framework;

namespace RiverHollow.Utilities
{
    public class RHTimer
    {
        public double TimerSpeed { get; }
        public double TimeLeft { get; set; }
        bool _bStopped = false;

        public RHTimer(double time, bool stopped = false)
        {
            TimerSpeed = time;
            TimeLeft = time;
            _bStopped = stopped;
        }

        public void TickDown(GameTime gTime)
        {
            if(!_bStopped && TimerSpeed > 0)
            {
                TimeLeft -= gTime.ElapsedGameTime.TotalSeconds;
            }
        }

        public void TickDown(double time)
        {
            if (!_bStopped && TimerSpeed > 0)
            {
                TimeLeft -= time;
            }
        }

        public void Reset(double newTimer = -1)
        {
            TimeLeft = (newTimer == -1 ? TimerSpeed : newTimer);
            _bStopped = false;
        }

        public bool Finished()
        {
            return _bStopped || TimeLeft <= 0;
        }

        public void Stop()
        {
            _bStopped = true;
        }
    }
}
