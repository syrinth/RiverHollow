using static RiverHollow.Game_Managers.GameManager;

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
            string[] splitString = value.Split('-');
            XLocation = int.Parse(splitString[0]);
            YLocation = int.Parse(splitString[1]);
            Frames = int.Parse(splitString[2]);
            FrameSpeed = float.Parse(splitString[3]);
            PingPong = splitString[4].Equals("T");
        }
    }
}
