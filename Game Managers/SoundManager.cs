using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Media;
using RiverHollow.Misc;
using System.Collections.Generic;
using System.IO;

namespace RiverHollow.Game_Managers
{
    public static class SoundManager
    {
        const string _sSongFolder = @"Content\Sound\Stock\Songs";
        const string _sEffectFolder = @"Content\Sound\Stock\Effects";
        static float _iMusicVol = 0.03f;
        static float _iEffectVol = 0.03f;
        public static Dictionary<string, Song> _diSongs;
        public static Dictionary<string, SoundEffect> _diEffects;

        public static void LoadContent(ContentManager Content)
        {
            _diSongs = new Dictionary<string, Song>();
            _diEffects = new Dictionary<string, SoundEffect>();

            MediaPlayer.Volume = _iMusicVol;
            foreach(string s in Directory.GetFiles(_sSongFolder)) { AddSong(Content, s); }
            foreach (string s in Directory.GetFiles(_sEffectFolder)) { AddEffect(Content, s); }
        }

        static void AddSong(ContentManager Content, string song)
        {
            string name = string.Empty;
            Util.ParseContentFile(ref song, ref name);
            if (!_diSongs.ContainsKey(name))
            {
                _diSongs.Add(name, Content.Load<Song>(song));
            }
        }

        static void AddEffect(ContentManager Content, string effect)
        {
            string name = string.Empty;
            Util.ParseContentFile(ref effect, ref name);
            if (!_diEffects.ContainsKey(name))
            {
                _diEffects.Add(name, Content.Load<SoundEffect>(effect));
            }
        }

        public static void PlaySong(string song)
        {
            if (_diSongs.ContainsKey(song))
            { 
                MediaPlayer.Play(_diSongs[song]);
            }
        }

        public static void PlayEffect(string effect)
        {
            PlayEffect(effect, _iEffectVol);
        }

        public static void PlayEffect(string effect, float vol)
        {
            if (_diEffects.ContainsKey(effect))
            {
                SoundEffectInstance soundInstance;
                soundInstance = _diEffects[effect].CreateInstance();

                soundInstance.Volume = vol;
                soundInstance.Play();
            }
        }

        public static void PlayEffectAtLoc(string effect, string mapName, Vector2 loc)
        {
            if (_diEffects.ContainsKey(effect) && MapManager.CurrentMap.Name.Equals(mapName))
            {
                int range = 640;
                int distance = -1;
                if (PlayerManager.PlayerInRangeGetDist(loc.ToPoint(), range, ref distance)) //arbitrary, 20 squares
                {
                    float delta = (float)(range - distance) / (float)range;
                    SoundEffectInstance soundInstance;
                    soundInstance = _diEffects[effect].CreateInstance();

                    soundInstance.Volume = (float)(delta*0.1);
                    soundInstance.Play();
                }
            }
        }
    }
}
