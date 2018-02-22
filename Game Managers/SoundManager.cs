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
        static float _iMusicVol = 0.05f;
        static float _iEffectVol = 0.3f;
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
            Utilities.ParseContentFile(ref song, ref name);
            if (!_diSongs.ContainsKey(name))
            {
                _diSongs.Add(song.Split('\\')[3], Content.Load<Song>(song));
            }
        }

        static void AddEffect(ContentManager Content, string effect)
        {
            string name = string.Empty;
            Utilities.ParseContentFile(ref effect, ref name);
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
            if (_diEffects.ContainsKey(effect))
            {
                SoundEffectInstance soundInstance;
                soundInstance = _diEffects[effect].CreateInstance();

                soundInstance.Volume = _iEffectVol;
                soundInstance.Play();
            }
        }
    }
}
