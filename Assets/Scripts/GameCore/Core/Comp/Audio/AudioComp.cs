using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Tools.Log;
using UnityEngine;

namespace GameCore.Core.Comp.Audio{
    public class AudioComp : IAudioComp{
        private AudioSource _bgmSource;
        private List<AudioSource> _sfxSources = new();
        private Dictionary<string, AudioClip> _audioClips = new();
        private float _bgmVolume = 1.0f;
        private float _sfxVolume = 1.0f;
        private int _maxSfxSources = 10;

        public UniTask Init()
        {
            if (_bgmSource != null)
            {
                HLog.LogW("AudioComp already initialized!");
                return UniTask.CompletedTask;
            }

            try
            {
                var bgmObject = new GameObject("BGMAudioSource");
                bgmObject.transform.SetParent(GameObject.Find("AudioRoot")?.transform);
                _bgmSource = bgmObject.AddComponent<AudioSource>();
                _bgmSource.loop = true;
                _bgmSource.playOnAwake = false;

                for (int i = 0; i < _maxSfxSources; i++)
                {
                    var sfxObject = new GameObject($"SFXAudioSource_{i}");
                    sfxObject.transform.SetParent(GameObject.Find("AudioRoot")?.transform);
                    var sfxSource = sfxObject.AddComponent<AudioSource>();
                    sfxSource.playOnAwake = false;
                    _sfxSources.Add(sfxSource);
                }

                HLog.Log("AudioComp initialized successfully!");
            }
            catch (Exception e)
            {
                HLog.LogE($"Failed to initialize AudioComp: {e.Message}");
            }
            return UniTask.CompletedTask;
        }

        public UniTask UnInit()
        {
            if (_bgmSource == null)
                return UniTask.CompletedTask;

            try
            {
                _bgmSource.Stop();
                foreach (var sfxSource in _sfxSources)
                    sfxSource.Stop();

                _audioClips.Clear();
                _sfxSources.Clear();

                if (_bgmSource != null)
                {
                    UnityEngine.Object.Destroy(_bgmSource.gameObject);
                    _bgmSource = null;
                }

                HLog.Log("AudioComp uninitialized successfully!");
            }
            catch (Exception e)
            {
                HLog.LogE($"Failed to uninitialize AudioComp: {e.Message}");
            }
            return UniTask.CompletedTask;
        }

        public void PlayBGM(string name)
        {
            if (string.IsNullOrEmpty(name)) { HLog.LogE("BGM name is empty!"); return; }

            try
            {
                if (_audioClips.TryGetValue(name, out var clip))
                {
                    _bgmSource.clip = clip;
                    _bgmSource.volume = _bgmVolume;
                    _bgmSource.Play();
                    HLog.Log($"Playing BGM: {name}");
                }
                else
                {
                    HLog.LogW($"BGM not found: {name}");
                }
            }
            catch (Exception e) { HLog.LogE($"Failed to play BGM {name}: {e.Message}"); }
        }

        public void PlayBGMAsync(string name, Action callback = null)
        {
            if (string.IsNullOrEmpty(name)) { HLog.LogE("BGM name is empty!"); callback?.Invoke(); return; }

            try
            {
                if (_audioClips.TryGetValue(name, out var clip))
                {
                    _bgmSource.clip = clip;
                    _bgmSource.volume = _bgmVolume;
                    _bgmSource.Play();
                    HLog.Log($"Playing BGM: {name}");
                    callback?.Invoke();
                }
                else
                {
                    HLog.LogW($"BGM not found: {name}");
                    callback?.Invoke();
                }
            }
            catch (Exception e) { HLog.LogE($"Failed to play BGM {name}: {e.Message}"); callback?.Invoke(); }
        }

        public void StopBGM()
        {
            try { _bgmSource?.Stop(); HLog.Log("BGM stopped!"); }
            catch (Exception e) { HLog.LogE($"Failed to stop BGM: {e.Message}"); }
        }

        public void PauseBGM()
        {
            try { _bgmSource?.Pause(); HLog.Log("BGM paused!"); }
            catch (Exception e) { HLog.LogE($"Failed to pause BGM: {e.Message}"); }
        }

        public void ResumeBGM()
        {
            try { _bgmSource?.UnPause(); HLog.Log("BGM resumed!"); }
            catch (Exception e) { HLog.LogE($"Failed to resume BGM: {e.Message}"); }
        }

        public void PlaySfx(string name)
        {
            if (string.IsNullOrEmpty(name)) { HLog.LogE("SFX name is empty!"); return; }

            try
            {
                if (_audioClips.TryGetValue(name, out var clip))
                {
                    AudioSource sfxSource = null;
                    foreach (var source in _sfxSources)
                    {
                        if (!source.isPlaying) { sfxSource = source; break; }
                    }
                    if (sfxSource == null) sfxSource = _sfxSources[0];

                    sfxSource.clip = clip;
                    sfxSource.volume = _sfxVolume;
                    sfxSource.Play();
                    HLog.Log($"Playing SFX: {name}");
                }
                else
                {
                    HLog.LogW($"SFX not found: {name}");
                }
            }
            catch (Exception e) { HLog.LogE($"Failed to play SFX {name}: {e.Message}"); }
        }

        public void PlaySfxAtPosition(string name, Vector3 position)
        {
            if (string.IsNullOrEmpty(name)) { HLog.LogE("SFX name is empty!"); return; }

            try
            {
                if (_audioClips.TryGetValue(name, out var clip))
                {
                    AudioSource.PlayClipAtPoint(clip, position, _sfxVolume);
                    HLog.Log($"Playing SFX at position: {name}");
                }
                else
                {
                    HLog.LogW($"SFX not found: {name}");
                }
            }
            catch (Exception e) { HLog.LogE($"Failed to play SFX {name} at position: {e.Message}"); }
        }

        public void StopAllSfx()
        {
            try
            {
                foreach (var sfxSource in _sfxSources) sfxSource.Stop();
                HLog.Log("All SFX stopped!");
            }
            catch (Exception e) { HLog.LogE($"Failed to stop all SFX: {e.Message}"); }
        }

        public void SetVolume(float volume) { SetBGMVolume(volume); SetSFXVolume(volume); }

        public void SetBGMVolume(float volume)
        {
            _bgmVolume = Mathf.Clamp01(volume);
            if (_bgmSource != null) _bgmSource.volume = _bgmVolume;
            HLog.Log($"BGM volume set to: {_bgmVolume}");
        }

        public void SetSFXVolume(float volume)
        {
            _sfxVolume = Mathf.Clamp01(volume);
            HLog.Log($"SFX volume set to: {_sfxVolume}");
        }

        public float GetBGMVolume() => _bgmVolume;
        public float GetSFXVolume() => _sfxVolume;

        public void PreloadAudio(string name, AudioClip clip)
        {
            if (string.IsNullOrEmpty(name) || clip == null) { HLog.LogE("Audio name or clip is null!"); return; }

            try { _audioClips[name] = clip; HLog.Log($"Preloaded audio: {name}"); }
            catch (Exception e) { HLog.LogE($"Failed to preload audio {name}: {e.Message}"); }
        }

        public void UnloadAudio(string name)
        {
            if (string.IsNullOrEmpty(name)) { HLog.LogE("Audio name is empty!"); return; }

            try { _audioClips.Remove(name); HLog.Log($"Unloaded audio: {name}"); }
            catch (Exception e) { HLog.LogE($"Failed to unload audio {name}: {e.Message}"); }
        }

        public bool IsAudioLoaded(string name) => _audioClips.ContainsKey(name);
        public bool IsBGMPlaying() => _bgmSource != null && _bgmSource.isPlaying;
        public int GetLoadedAudioCount() => _audioClips.Count;
    }
}
