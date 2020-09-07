using System.IO;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Media;
using RiverHollow.Utilities;


namespace RiverHollow.Game_Managers
{
    public static class SoundManager
    {
        enum QueuePhase { None, Down, Up };
        static QueuePhase _eQueuePhase = QueuePhase.None;
        static Song _queuedSong;
        const float PHASE_VAL = 0.02f;

        const string _sSongFolder = @"Content\Sound\Stock\Songs";
        const string _sEffectFolder = @"Content\Sound\Stock\Effects";
        const string _sHarpFolder = @"Content\Sound\Stock\Harp";
        static float _iMusicVol = 0.5f;
        static float _iEffectVol = 0.0001f;
        static Dictionary<string, Song> _diSongs;
        static Dictionary<string, SoundEffect> _diEffects;

        static Song BackgroundSong => MediaPlayer.Queue.ActiveSong;

        public static void LoadContent(ContentManager Content)
        {
            _diSongs = new Dictionary<string, Song>();
            _diEffects = new Dictionary<string, SoundEffect>();

            MediaPlayer.Volume = _iMusicVol;
            foreach(string s in Directory.GetFiles(_sSongFolder)) { AddSong(Content, s); }
            foreach (string s in Directory.GetFiles(_sEffectFolder)) { AddEffect(Content, s); }
            foreach (string s in Directory.GetFiles(_sHarpFolder)) { AddEffect(Content, s); }
        }

        public static void Update(GameTime gameTime)
        {
            if (_eQueuePhase == QueuePhase.Down)
            {
                if (MediaPlayer.Volume > 0) { MediaPlayer.Volume -= PHASE_VAL; }
                else
                {
                    MediaPlayer.Stop();
                    PlaySong(_queuedSong, true);
                    _queuedSong = null;
                    _eQueuePhase = QueuePhase.Up;
                }
            }
            else if (_eQueuePhase == QueuePhase.Up)
            {
                if (MediaPlayer.Volume < _iMusicVol) { MediaPlayer.Volume += PHASE_VAL; }
                else { _eQueuePhase = QueuePhase.None; }
            }
        }

        /// <summary>
        /// Adds the given song to the Song Dictionary
        /// </summary>
        /// <param name="Content">Content Manager</param>
        /// <param name="song">The full path to the song</param>
        static void AddSong(ContentManager Content, string song)
        {
            string name = string.Empty;
            Util.ParseContentFileRetName(ref song, ref name);
            if (!_diSongs.ContainsKey(name))
            {
                _diSongs.Add(name, Content.Load<Song>(song));
            }
        }

        /// <summary>
        /// Adds the given effect to the Effect Dictionary
        /// </summary>
        /// <param name="Content">Content Manager</param>
        /// <param name="effect">The full path to the effect</param>
        static void AddEffect(ContentManager Content, string effect)
        {
            string name = string.Empty;
            Util.ParseContentFileRetName(ref effect, ref name);
            if (!_diEffects.ContainsKey(name))
            {
                _diEffects.Add(name, Content.Load<SoundEffect>(effect));
            }
        }

        /// <summary>
        /// If the given song is not the current background music,
        /// queue it up and fade the song out.
        /// </summary>
        /// <param name="song">The name of the song file to play</param>
        public static void PlayBackgroundMusic(string song)
        {
            if (_diSongs.ContainsKey(song))
            {
                Song s = _diSongs[song];
                if(BackgroundSong != s)
                {
                    _eQueuePhase = QueuePhase.Down;
                    _queuedSong = s;
                }
            }
        }

        /// <summary>
        /// Actually play the given song
        /// </summary>
        /// <param name="backgroundSong">The Song to play</param>
        /// <param name="repeating">Whether or not the song should repeat</param>
        private static void PlaySong(Song backgroundSong, bool repeating = false)
        {
            MediaPlayer.Play(backgroundSong);
            MediaPlayer.IsRepeating = repeating;
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

        /// <summary>
        /// Plays a sound effect "at" a given location.
        /// The volume of the effect is determined by the player's distance from the sound
        /// </summary>
        /// <param name="effect">Name of the ffect</param>
        /// <param name="mapName">Which map to play it on</param>
        /// <param name="loc">The location of the effect</param>
        public static void PlayEffectAtLoc(string effect, string mapName, Vector2 loc)
        {
            //If we're not currently on the map, don't play the effect
            if (MapManager.CurrentMap.Name.Equals(mapName))
            {
                //TODO: There should probably be a log function here? The sound seems to drop off and vanish almost immediately when we're out of range but be clear beforehand
                int range = (int)(RiverHollow.ScreenWidth/8);
                int distance = -1;
                if (PlayerManager.PlayerInRangeGetDist(loc.ToPoint(), range, ref distance))
                {
                    float delta = (float)(range - distance) / (float)range;
                    PlayEffect(effect, (float)(delta * 0.1));
                }
            }
        }
    }
}
