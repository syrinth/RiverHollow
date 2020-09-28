using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Media;
using RiverHollow.Utilities;
using System;
using System.Collections.Generic;
using System.IO;

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
        static float _fMusicVol = 0.4f;
        public static float MusicVolume => _fMusicVol;
        static float _fEffectVol = 0.4f;
        public static float EffectVolume => _fEffectVol;
        static Dictionary<string, Song> _diSongs;
        static Dictionary<string, SoundEffect> _diEffects;
        static Dictionary<object, EffectData> _diCurrentEffects;

        static Song BackgroundSong => MediaPlayer.Queue.ActiveSong;

        delegate bool UnneededEffectTestDel(KeyValuePair<object, EffectData> kvp);

        public static void LoadContent(ContentManager Content)
        {
            _diSongs = new Dictionary<string, Song>();
            _diEffects = new Dictionary<string, SoundEffect>();
            _diCurrentEffects = new Dictionary<object, EffectData>();

            MediaPlayer.Volume = _fMusicVol;
            foreach (string s in Directory.GetFiles(_sSongFolder)) { AddSong(Content, s); }
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
                if (MediaPlayer.Volume < _fMusicVol) { MediaPlayer.Volume += PHASE_VAL; }
                else { _eQueuePhase = QueuePhase.None; }
            }

            foreach (KeyValuePair<object, EffectData> kvp in _diCurrentEffects)
            {
                if (kvp.Value.Position != Vector2.Zero)
                {
                    kvp.Value.SetVolume(GetDistanceVolume(kvp.Value.Position));
                }
            }

            ClearUnneededEffects(TestPlaying);
        }

        public static void ChangeMap()
        {
            ClearUnneededEffects(TestCurrentMap);
        }

        private static void ClearUnneededEffects(UnneededEffectTestDel test)
        {
            List<object> toRemove = new List<object>();
            foreach (KeyValuePair<object, EffectData> kvp in _diCurrentEffects)
            {
                if (test(kvp))
                {
                    kvp.Value.SoundEffect.Stop();
                    toRemove.Add(kvp.Key);
                }
            }

            foreach (object o in toRemove)
            {
                _diCurrentEffects.Remove(o);
            }
        }

        private static bool TestPlaying(KeyValuePair<object, EffectData> kvp)
        {
            bool rv = false;
            if (kvp.Value.SoundEffect.State != SoundState.Playing)
            {
                rv = true;
            }

            return rv;
        }
        private static bool TestCurrentMap(KeyValuePair<object, EffectData> kvp)
        {
            bool rv = false;
            if (kvp.Value.MapName != MapManager.CurrentMap.Name)
            {
                rv = true;
            }

            return rv;
        }

        public static void SetMusicVolume(float value)
        {
            _fMusicVol = value;
            MediaPlayer.Volume = _fMusicVol;
        }
        public static void SetEffectVolume(float value)
        {
            _fEffectVol = value;
            foreach (KeyValuePair<object, EffectData> kvp in _diCurrentEffects)
            {
                if (kvp.Value.Position != Vector2.Zero)
                {
                    kvp.Value.SetVolume(GetDistanceVolume(kvp.Value.Position));
                }
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
                if (BackgroundSong != s)
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

        public static void PlayEffect(string effectName, object obj = null)
        {
            EffectData data = new EffectData("", effectName, obj, _fEffectVol);
            PlayEffect(data);
        }

        private static void PlayEffect(EffectData data)
        {
            if (data.EffectObject == null || !_diCurrentEffects.ContainsKey(data.EffectObject))
            {
                data.SoundEffect.Play();

                if (data.EffectObject != null) { _diCurrentEffects[data.EffectObject] = data; }
            }
        }

        /// <summary>
        /// Plays a sound effect "at" a given location.
        /// The volume of the effect is determined by the player's distance from the sound
        /// </summary>
        /// <param name="effect">Name of the ffect</param>
        /// <param name="mapName">Which map to play it on</param>
        /// <param name="loc">The location of the effect</param>
        public static void PlayEffectAtLoc(string effectName, string mapName, Vector2 loc, object obj = null)
        {
            //If we're not currently on the map, don't play the effect
            if (MapManager.CurrentMap.Name.Equals(mapName))
            {
                EffectData data = new EffectData(mapName, effectName, obj, GetDistanceVolume(loc));
                data.SetPosition(loc);
                PlayEffect(data);
            }
        }

        /// <summary>
        /// Stops the sound effect attached to theo bject
        /// </summary>
        /// <param name="obj">The object that the sound effect is attached to</param>
        public static void StopEffect(object obj)
        {
            if (_diCurrentEffects.ContainsKey(obj))
            {
                _diCurrentEffects[obj].SoundEffect.Stop();
                _diCurrentEffects.Remove(obj);
            }
        }

        private static float GetDistanceVolume(Vector2 loc)
        {
            //TODO: There should probably be a log function here? The sound seems to drop off and vanish almost immediately when we're out of range but be clear beforehand
            int range = (int)(RiverHollow.ScreenWidth / 8);
            int distance = -1;

            PlayerManager.PlayerInRangeGetDist(loc.ToPoint(), range, ref distance);
            float volume = Math.Max(0f, ((float)range - (float)distance) / (float)range) * _fEffectVol;
            return volume;
        }

        private class EffectData
        {
            object _obj;
            public object EffectObject => _obj;
            string _sMapName;
            public string MapName => _sMapName;
            Vector2 _vPosition;
            public Vector2 Position => _vPosition;
            SoundEffectInstance _instance;
            public SoundEffectInstance SoundEffect => _instance;
            float _fVolume;
            public float Volume => _fVolume;

            public EffectData(string mapName, string effectName, object obj, float volume)
            {
                _sMapName = mapName;
                _obj = obj;
                _fVolume = volume;

                if (_diEffects.ContainsKey(effectName))
                {
                    _instance = _diEffects[effectName].CreateInstance();
                    _instance.Volume = _fVolume;
                }
            }

            public void SetPosition(Vector2 val)
            {
                _vPosition = val;
            }

            public void SetVolume(float val)
            {
                _fVolume = val;
                _instance.Volume = _fVolume;
            }

            public void SetSoundEffect(SoundEffectInstance effect)
            {
                _instance = effect;
            }

        }
    }
}
