using RiverHollow.Utilities;
using static RiverHollow.Game_Managers.GameManager;
using static RiverHollow.Utilities.Enums;

namespace RiverHollow.Misc
{
    public class AnimationData
    {
        public int XLocation { get; private set; }
        public int YLocation { get; private set; }
        public int Frames { get; private set; }
        public float FrameSpeed { get; private set; }
        public bool Directional { get; }
        public bool PingPong { get; private set; }
        public bool PlayOnce { get; set; } = false;
        public bool BackToIdle { get; private set; }
        public VerbEnum Verb { get; }
        public AnimationEnum Animation { get; }

        public AnimationData(string value, VerbEnum verb, bool backToIdle, bool directional) : base()
        {
            Directional = directional;
            Verb = verb;
            BackToIdle = backToIdle;
            StoreData(value);
        }

        public AnimationData(string value, AnimationEnum anim)
        {
            Animation = anim;
            StoreData(value);
        }

        public void StoreData(string value)
        {
            string[] splitString = Util.FindArguments(value);
            if (splitString.Length == 3)
            {
                Frames = int.Parse(splitString[0]);
                FrameSpeed = float.Parse(splitString[1]);
                PingPong = splitString[2].Equals("T");
            }
            else if (splitString.Length >= 5)
            {
                XLocation = int.Parse(splitString[0]);
                YLocation = int.Parse(splitString[1]);
                Frames = int.Parse(splitString[2]);
                FrameSpeed = float.Parse(splitString[3]);
                PingPong = splitString[4].Equals("T");
            }
        }

        public void SetYValue(int value)
        {
            YLocation = value;
        }

        public void ModXValue(int value)
        {
            XLocation += value;
        }
    }
}
