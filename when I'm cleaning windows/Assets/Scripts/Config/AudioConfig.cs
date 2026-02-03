using UnityEngine;

namespace WhenImCleaningWindows.Config
{
    /// <summary>
    /// Audio Configuration - ScriptableObject for ASMR sound settings.
    /// Create via: Assets → Create → When I'm Cleaning Windows → Audio Config
    /// </summary>
    [CreateAssetMenu(fileName = "AudioConfig", menuName = "When I'm Cleaning Windows/Audio Config", order = 3)]
    public class AudioConfig : ScriptableObject
    {
        [Header("Master Settings")]
        [Tooltip("Master volume (0.0-1.0)")]
        [Range(0f, 1f)]
        public float masterVolume = 0.8f;
        
        [Tooltip("Enable audio globally")]
        public bool audioEnabled = true;
        
        [Header("Volume Levels")]
        [Tooltip("SFX volume (0.0-1.0)")]
        [Range(0f, 1f)]
        public float sfxVolume = 1f;
        
        [Tooltip("Music volume (0.0-1.0)")]
        [Range(0f, 1f)]
        public float musicVolume = 0.7f;
        
        [Tooltip("Ambient volume (0.0-1.0)")]
        [Range(0f, 1f)]
        public float ambientVolume = 0.5f;
        
        [Tooltip("UI volume (0.0-1.0)")]
        [Range(0f, 1f)]
        public float uiVolume = 0.6f;
        
        [Header("ASMR Settings")]
        [Tooltip("Enable binaural audio (requires headphones)")]
        public bool enableBinauralAudio = true;
        
        [Tooltip("ASMR intensity (0.0-1.0)")]
        [Range(0f, 1f)]
        public float asmrIntensity = 0.8f;
        
        [Tooltip("Squeegee sound frequency (higher = more often)")]
        [Range(0f, 1f)]
        public float squeegeeFrequency = 0.7f;
        
        [Tooltip("Water spray density")]
        [Range(0f, 1f)]
        public float sprayDensity = 0.6f;
        
        [Header("Haptic Feedback")]
        [Tooltip("Enable haptic feedback")]
        public bool enableHaptics = true;
        
        [Tooltip("Haptic intensity (0.0-1.0)")]
        [Range(0f, 1f)]
        public float hapticIntensity = 0.7f;
        
        [Tooltip("Enable haptics for cleaning")]
        public bool hapticsOnCleaning = true;
        
        [Tooltip("Enable haptics for power-ups")]
        public bool hapticsOnPowerUps = true;
        
        [Tooltip("Enable haptics for UI")]
        public bool hapticsOnUI = false;
        
        [Header("FMOD Settings")]
        [Tooltip("Use FMOD Studio (requires plugin)")]
        public bool useFMOD = true;
        
        [Tooltip("FMOD master bank name")]
        public string masterBankName = "Master";
        
        [Tooltip("FMOD strings bank name")]
        public string stringsBankName = "Master.strings";
        
        [Header("Sound Effects")]
        [Tooltip("Max concurrent SFX instances")]
        [Range(1, 50)]
        public int maxConcurrentSFX = 20;
        
        [Tooltip("SFX pooling size")]
        [Range(5, 100)]
        public int sfxPoolSize = 30;
        
        [Tooltip("Enable sound occlusion")]
        public bool enableSoundOcclusion = false;
        
        [Header("Music")]
        [Tooltip("Enable background music")]
        public bool enableMusic = true;
        
        [Tooltip("Music crossfade duration (seconds)")]
        [Range(0f, 5f)]
        public float musicCrossfadeDuration = 1.5f;
        
        [Tooltip("Enable dynamic music (adaptive)")]
        public bool enableDynamicMusic = true;
        
        [Header("Formby Tracks")]
        [Tooltip("Play Formby ukulele on special levels")]
        public bool enableFormbyTracks = true;
        
        [Tooltip("Formby track probability (0.0-1.0)")]
        [Range(0f, 1f)]
        public float formbyProbability = 0.05f;
        
        [Tooltip("Formby level milestones (play on these levels)")]
        public int[] formbyMilestoneLevels = new int[]
        {
            100, 500, 1000, 2500, 5000, 10000
        };
        
        [Header("ASMR Event Types")]
        [Tooltip("Enable squeegee sounds")]
        public bool enableSqueegeeSounds = true;
        
        [Tooltip("Enable spray sounds")]
        public bool enableSpraySounds = true;
        
        [Tooltip("Enable wipe sounds")]
        public bool enableWipeSounds = true;
        
        [Tooltip("Enable scrub sounds")]
        public bool enableScrubSounds = true;
        
        [Tooltip("Enable drip sounds")]
        public bool enableDripSounds = true;
        
        [Tooltip("Enable ambient window sounds")]
        public bool enableAmbientSounds = true;
        
        [Header("Performance")]
        [Tooltip("Enable audio streaming")]
        public bool enableAudioStreaming = true;
        
        [Tooltip("Audio compression quality (0.0-1.0, higher = better)")]
        [Range(0f, 1f)]
        public float compressionQuality = 0.7f;
        
        [Tooltip("Sample rate (Hz)")]
        public int sampleRate = 48000;
        
        [Header("Debug")]
        [Tooltip("Show audio debug info")]
        public bool showAudioDebug = false;
        
        [Tooltip("Log FMOD events")]
        public bool logFMODEvents = false;
    }
}








