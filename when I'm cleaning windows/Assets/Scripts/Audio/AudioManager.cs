using UnityEngine;
using System.Collections.Generic;
using WhenImCleaningWindows.Config;

namespace WhenImCleaningWindows.Audio
{
    /// <summary>
    /// Audio event types for FMOD integration.
    /// </summary>
    public enum AudioEventType
    {
        // Cleaning SFX
        SFX_Swipe_Suds,
        SFX_Circle_Scrub,
        SFX_Squeegee_Long,
        SFX_Spray_Water,
        SFX_Bucket_Splash,
        SFX_Perfect_Clear,
        
        // Hazard-specific
        SFX_Bird_Poop_Splat,
        SFX_Flies_Buzzing,
        SFX_Mud_Squelch,
        SFX_Oil_Grease,
        SFX_Web_Tear,
        SFX_Graffiti_Scrub,
        SFX_Sticker_Peel,
        SFX_Frost_Crack,
        SFX_Algae_Slime,
        SFX_Sap_Sticky,
        SFX_Fog_Wipe,
        SFX_NanoBots_Zap,
        SFX_Pollen_Blow,
        
        // UI SFX
        UI_Button_Click,
        UI_Purchase_Success,
        UI_Star_Pop,
        UI_Coin_Collect,
        UI_Gem_Collect,
        UI_Energy_Refill,
        UI_Level_Complete,
        UI_Level_Failed,
        UI_Offer_Popup,
        
        // Power-ups
        PowerUp_Nuke,
        PowerUp_Turbo,
        PowerUp_TimeFreeze,
        PowerUp_AutoPilot,
        
        // Ambient
        AMB_Wind_Light,
        AMB_City_Traffic,
        AMB_Rain_Gentle,
        AMB_Birds_Chirping,
        AMB_Cyberpunk_Hum,
        AMB_Space_Station,
        
        // Music
        Music_MainMenu,
        Music_World1_Suburban,
        Music_World2_Office,
        Music_World3_Historic,
        Music_World4_Industrial,
        Music_World5_Coastal,
        Music_World6_Decay,
        Music_World7_Mountain,
        Music_World8_Cyberpunk,
        Music_World9_Space,
        Music_World10_Mansion,
        
        // Formby
        Music_Formby_Ukulele_Victory
    }
    
    /// <summary>
    /// Audio Manager - Handles all ASMR SFX and music playback.
    /// Integrates with FMOD Studio 2.02 for binaural audio and dynamic mixing.
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }
        
        [Header("Audio Settings")]
        [SerializeField] private bool enableBinauralAudio = true;
        [SerializeField] private float masterVolume = 1.0f;
        [SerializeField] private float sfxVolume = 1.0f;
        [SerializeField] private float musicVolume = 0.7f;
        [SerializeField] private float ambienceVolume = 0.5f;

        [Header("Pooling")]
        [SerializeField] private int audioSourcePoolSize = 20;
        
        [Header("FMOD (Production)")]
        [SerializeField] private bool useFMOD = false;  // Enable when FMOD plugin installed
        
        // Audio source pools (fallback for prototype)
        private Dictionary<AudioEventType, AudioClip> audioClips = new Dictionary<AudioEventType, AudioClip>();
        private List<AudioSource> audioSourcePool = new List<AudioSource>();
        private AudioSource musicSource;
        private AudioSource ambienceSource;
        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            ApplyConfig();
            
            InitializeAudioSources();
            LoadAudioClips();
        }

        private void ApplyConfig()
        {
            AudioConfig config = ConfigProvider.AudioConfig;
            if (config == null) return;

            enableBinauralAudio = config.enableBinauralAudio;
            masterVolume = config.masterVolume;
            sfxVolume = config.sfxVolume;
            musicVolume = config.musicVolume;
            ambienceVolume = config.ambientVolume;
            useFMOD = config.useFMOD;
            audioSourcePoolSize = Mathf.Clamp(config.sfxPoolSize, 5, 100);
        }
        
        /// <summary>
        /// Initialize audio source pool (fallback).
        /// </summary>
        private void InitializeAudioSources()
        {
            // Create music source
            GameObject musicObject = new GameObject("Music Source");
            musicObject.transform.SetParent(transform);
            musicSource = musicObject.AddComponent<AudioSource>();
            musicSource.loop = true;
            musicSource.volume = musicVolume;
            
            // Create ambience source
            GameObject ambienceObject = new GameObject("Ambience Source");
            ambienceObject.transform.SetParent(transform);
            ambienceSource = ambienceObject.AddComponent<AudioSource>();
            ambienceSource.loop = true;
            ambienceSource.volume = ambienceVolume;
            
            // Create SFX pool
            for (int i = 0; i < audioSourcePoolSize; i++)
            {
                GameObject sfxObject = new GameObject($"SFX Source {i}");
                sfxObject.transform.SetParent(transform);
                AudioSource source = sfxObject.AddComponent<AudioSource>();
                source.playOnAwake = false;
                source.volume = sfxVolume;
                audioSourcePool.Add(source);
            }

            Debug.Log($"AudioManager initialized: {audioSourcePoolSize} SFX sources");
        }
        
        /// <summary>
        /// Load audio clips (placeholder - would load from Resources).
        /// </summary>
        private void LoadAudioClips()
        {
            // In production: Load from Resources or FMOD banks
            // For prototype: Clips would be assigned in inspector or loaded at runtime
            
            Debug.Log("AudioManager: Audio clips ready (placeholder)");
        }
        
        /// <summary>
        /// Play SFX event (FMOD or fallback).
        /// </summary>
        public void PlaySFX(AudioEventType eventType, Vector3 position = default, float volume = 1.0f)
        {
            if (useFMOD)
            {
                PlayFMODEvent(eventType, position);
            }
            else
            {
                PlayFallbackSFX(eventType, volume);
            }
        }
        
        /// <summary>
        /// Play FMOD event (production).
        /// </summary>
        private void PlayFMODEvent(AudioEventType eventType, Vector3 position)
        {
            // In production with FMOD plugin:
            // string eventPath = GetFMODEventPath(eventType);
            // FMOD.Studio.EventInstance instance = FMODUnity.RuntimeManager.CreateInstance(eventPath);
            // instance.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(position));
            // instance.start();
            // instance.release();
            
            Debug.Log($"FMOD: Play {eventType} at {position}");
        }
        
        /// <summary>
        /// Play fallback SFX (prototype).
        /// </summary>
        private void PlayFallbackSFX(AudioEventType eventType, float volume)
        {
            if (!audioClips.ContainsKey(eventType))
            {
                Debug.LogWarning($"Audio clip not found: {eventType}");
                return;
            }
            
            AudioSource source = GetAvailableSource();
            if (source != null)
            {
                source.clip = audioClips[eventType];
                source.volume = sfxVolume * volume * masterVolume;
                source.Play();
            }
        }
        
        /// <summary>
        /// Get available audio source from pool.
        /// </summary>
        private AudioSource GetAvailableSource()
        {
            foreach (var source in audioSourcePool)
            {
                if (!source.isPlaying)
                {
                    return source;
                }
            }
            
            // All sources busy, return first one
            return audioSourcePool[0];
        }
        
        /// <summary>
        /// Play music track.
        /// </summary>
        public void PlayMusic(AudioEventType musicType, bool fadeIn = true, float fadeDuration = 2.0f)
        {
            if (musicSource == null) return;
            
            if (useFMOD)
            {
                // FMOD music system
                PlayFMODEvent(musicType, Vector3.zero);
            }
            else
            {
                // Fallback
                if (audioClips.ContainsKey(musicType))
                {
                    if (fadeIn)
                    {
                        StartCoroutine(FadeMusic(audioClips[musicType], fadeDuration));
                    }
                    else
                    {
                        musicSource.clip = audioClips[musicType];
                        musicSource.Play();
                    }
                }
            }
            
            Debug.Log($"Music: Playing {musicType}");
        }
        
        /// <summary>
        /// Play ambience loop.
        /// </summary>
        public void PlayAmbience(AudioEventType ambienceType, float fadeIn = 1.0f)
        {
            if (ambienceSource == null) return;
            
            if (audioClips.ContainsKey(ambienceType))
            {
                ambienceSource.clip = audioClips[ambienceType];
                ambienceSource.Play();
                StartCoroutine(FadeVolume(ambienceSource, 0f, ambienceVolume, fadeIn));
            }
            
            Debug.Log($"Ambience: Playing {ambienceType}");
        }
        
        /// <summary>
        /// Stop ambience.
        /// </summary>
        public void StopAmbience(float fadeOut = 1.0f)
        {
            if (ambienceSource != null && ambienceSource.isPlaying)
            {
                StartCoroutine(FadeVolume(ambienceSource, ambienceSource.volume, 0f, fadeOut, true));
            }
        }
        
        /// <summary>
        /// Fade music track.
        /// </summary>
        private System.Collections.IEnumerator FadeMusic(AudioClip newClip, float duration)
        {
            // Fade out current
            float startVolume = musicSource.volume;
            float elapsed = 0f;
            
            while (elapsed < duration * 0.5f)
            {
                elapsed += Time.deltaTime;
                musicSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / (duration * 0.5f));
                yield return null;
            }
            
            // Switch clip
            musicSource.clip = newClip;
            musicSource.Play();
            
            // Fade in new
            elapsed = 0f;
            while (elapsed < duration * 0.5f)
            {
                elapsed += Time.deltaTime;
                musicSource.volume = Mathf.Lerp(0f, musicVolume, elapsed / (duration * 0.5f));
                yield return null;
            }
            
            musicSource.volume = musicVolume;
        }
        
        /// <summary>
        /// Fade volume.
        /// </summary>
        private System.Collections.IEnumerator FadeVolume(AudioSource source, float startVol, float endVol, float duration, bool stopAfter = false)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                source.volume = Mathf.Lerp(startVol, endVol, elapsed / duration);
                yield return null;
            }
            
            source.volume = endVol;
            
            if (stopAfter)
            {
                source.Stop();
            }
        }
        
        /// <summary>
        /// Set master volume.
        /// </summary>
        public void SetMasterVolume(float volume)
        {
            masterVolume = Mathf.Clamp01(volume);
            PlayerPrefs.SetFloat("MasterVolume", masterVolume);
            PlayerPrefs.Save();
        }
        
        /// <summary>
        /// Set SFX volume.
        /// </summary>
        public void SetSFXVolume(float volume)
        {
            sfxVolume = Mathf.Clamp01(volume);
            PlayerPrefs.SetFloat("SFXVolume", sfxVolume);
            PlayerPrefs.Save();
        }
        
        /// <summary>
        /// Set music volume.
        /// </summary>
        public void SetMusicVolume(float volume)
        {
            musicVolume = Mathf.Clamp01(volume);
            if (musicSource != null)
            {
                musicSource.volume = musicVolume;
            }
            PlayerPrefs.SetFloat("MusicVolume", musicVolume);
            PlayerPrefs.Save();
        }
        
        /// <summary>
        /// Enable/disable binaural audio.
        /// </summary>
        public void SetBinauralAudio(bool enabled)
        {
            enableBinauralAudio = enabled;
            PlayerPrefs.SetInt("BinauralAudio", enabled ? 1 : 0);
            PlayerPrefs.Save();
            
            // In production: Configure FMOD spatializer
            Debug.Log($"Binaural audio: {(enabled ? "Enabled" : "Disabled")}");
        }
        
        // === QUICK PLAY METHODS ===
        
        public void PlaySwipeSFX() => PlaySFX(AudioEventType.SFX_Swipe_Suds);
        public void PlayCircleScrubSFX() => PlaySFX(AudioEventType.SFX_Circle_Scrub);
        public void PlaySqueegeeSFX() => PlaySFX(AudioEventType.SFX_Squeegee_Long);
        public void PlayButtonClick() => PlaySFX(AudioEventType.UI_Button_Click);
        public void PlayCoinCollect() => PlaySFX(AudioEventType.UI_Coin_Collect);
        public void PlayGemCollect() => PlaySFX(AudioEventType.UI_Gem_Collect);
        public void PlayStarPop() => PlaySFX(AudioEventType.UI_Star_Pop);
        public void PlayPurchaseSuccess() => PlaySFX(AudioEventType.UI_Purchase_Success);
        
        // === DEBUG ===
        
        [ContextMenu("Play Swipe SFX")]
        public void DEBUG_PlaySwipe()
        {
            PlaySwipeSFX();
        }
        
        [ContextMenu("Play Star Pop")]
        public void DEBUG_PlayStarPop()
        {
            PlayStarPop();
        }
    }
}








