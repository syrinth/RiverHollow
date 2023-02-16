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

        public bool TickDown(GameTime gTime, bool autoReset = false)
        {
            if(!_bStopped && TimerSpeed > 0)
            {
                TimeLeft -= gTime.ElapsedGameTime.TotalSeconds;
            }

            bool rv = Finished();

            if (rv && autoReset)
            {
                Reset();
            }

            return rv;
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

        public static bool TimerCheck(RHTimer timer, GameTime gTime)
        {
            return timer != null && timer.TickDown(gTime);
        }
    }
}
