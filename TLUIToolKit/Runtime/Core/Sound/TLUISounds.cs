using UnityEngine;
using Sirenix.OdinInspector;
using ThreeLines.Helpers;

namespace TLUIToolkit
{
    public class TLUISounds : Singleton<TLUISounds>
    {
        #region Audio Clips References

        [Title("UI Sound Effects Configuration")]
        [BoxGroup("Audio Clips"), SerializeField]
        private AudioClip hoverSound;

        [BoxGroup("Audio Clips"), SerializeField]
        private AudioClip clickSound;

        [BoxGroup("Audio Clips"), SerializeField]
        private AudioClip swipeSound;

        [BoxGroup("Audio Clips"), SerializeField]
        private AudioClip backSound;

        [BoxGroup("Audio Clips"), SerializeField]
        private AudioClip errorSound;

        #endregion

        #region Audio Settings

        [Title("Audio Settings")]
        [BoxGroup("Settings"), SerializeField, Range(0f, 1f)]
        private float masterVolume = 1f;

        [BoxGroup("Settings"), SerializeField, Range(0f, 1f)]
        private float hoverVolume = 0.7f;

        [BoxGroup("Settings"), SerializeField, Range(0f, 1f)]
        private float clickVolume = 0.8f;

        [BoxGroup("Settings"), SerializeField, Range(0f, 1f)]
        private float swipeVolume = 0.6f;

        [BoxGroup("Settings"), SerializeField]
        private bool enableSounds = true;

        #endregion

        #region Components

        [Title("Audio Components")]
        [BoxGroup("Components"), SerializeField, Required]
        private AudioSource audioSource;

        #endregion

        #region Initialization
        bool isInitialized = false;
        private void InitializeAudioSource()
        {
            if (isInitialized) 
                return;
            if (audioSource == null)
            {
                audioSource = GetComponent<AudioSource>();
                if (audioSource == null)
                {
                    audioSource = gameObject.AddComponent<AudioSource>();
                }
            }

            // Configure audio source for UI sounds
            audioSource.playOnAwake = false;
            audioSource.loop = false;
            audioSource.spatialBlend = 0f; // 2D sound
            isInitialized = true;
        }

        /// <summary>
        /// Try to load default audio clips from Resources/TLUIToolkit/Sounds
        /// </summary>
        [Button("Try Load Defaults")]
        public void TryLoadDefaults()
        {
            const string resourcePath = "TLUIToolkit/Sounds/";

            // Try to load each sound if not already assigned
            if (hoverSound == null)
            {
                hoverSound = Resources.Load<AudioClip>(resourcePath + "hover");
                if (hoverSound != null)
                    Debug.Log("Loaded default hover sound from Resources");
            }

            if (clickSound == null)
            {
                clickSound = Resources.Load<AudioClip>(resourcePath + "click");
                if (clickSound != null)
                    Debug.Log("Loaded default click sound from Resources");
            }

            if (swipeSound == null)
            {
                swipeSound = Resources.Load<AudioClip>(resourcePath + "swipe");
                if (swipeSound != null)
                    Debug.Log("Loaded default swipe sound from Resources");
            }

            if (backSound == null)
            {
                backSound = Resources.Load<AudioClip>(resourcePath + "back");
                if (backSound != null)
                    Debug.Log("Loaded default back sound from Resources");
            }

            if (errorSound == null)
            {
                errorSound = Resources.Load<AudioClip>(resourcePath + "error");
                if (errorSound != null)
                    Debug.Log("Loaded default error sound from Resources");
            }

#if UNITY_EDITOR
            // Mark as dirty for editor
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }

        #endregion

        #region Public API Methods

        [Title("Testing")]
        [Button("Test Hover Sound")]
        public void PlayHoverSound()
        {
            PlaySound(hoverSound, hoverVolume);
        }

        [Button("Test Click Sound")]
        public void PlayClickSound()
        {
            PlaySound(clickSound, clickVolume);
        }

        [Button("Test Swipe Sound")]
        public void PlaySwipeSound()
        {
            PlaySound(swipeSound, swipeVolume);
        }

        [Button("Test Back Sound")]
        public void PlayBackSound()
        {
            PlaySound(backSound, clickVolume);
        }

        [Button("Test Error Sound")]
        public void PlayErrorSound()
        {
            PlaySound(errorSound, clickVolume);
        }

        /// <summary>
        /// Play a custom sound with specified volume
        /// </summary>
        /// <param name="clip">Audio clip to play</param>
        /// <param name="volume">Volume multiplier (0-1)</param>
        public void PlayCustomSound(AudioClip clip, float volume = 1f)
        {
            PlaySound(clip, volume);
        }

        public void PlaySound(SoundType soundType, float volumeMultiplier = 1f)
        {
            switch (soundType)
            {
                case SoundType.Hover:
                    PlaySound(hoverSound, hoverVolume * volumeMultiplier);
                    break;
                case SoundType.Click:
                    PlaySound(clickSound, clickVolume * volumeMultiplier);
                    break;
                case SoundType.Swipe:
                    PlaySound(swipeSound, swipeVolume * volumeMultiplier);
                    break;
                case SoundType.Back:
                    PlaySound(backSound, clickVolume * volumeMultiplier);
                    break;
                case SoundType.Error:
                    PlaySound(errorSound, clickVolume * volumeMultiplier);
                    break;
            }
        }

        /// <summary>
        /// Enable or disable all UI sounds
        /// </summary>
        /// <param name="enabled">Sound state</param>
        public void SetSoundsEnabled(bool enabled)
        {
            enableSounds = enabled;
        }

        /// <summary>
        /// Set the master volume for all UI sounds
        /// </summary>
        /// <param name="volume">Master volume (0-1)</param>
        public void SetMasterVolume(float volume)
        {
            masterVolume = Mathf.Clamp01(volume);
        }

        /// <summary>
        /// Stop all currently playing UI sounds
        /// </summary>
        public void StopAllSounds()
        {
            if (audioSource != null)
            {
                audioSource.Stop();
            }
        }

        #endregion

        #region Private Methods

        private void PlaySound(AudioClip clip, float volumeMultiplier)
        {
            if(!isInitialized)
            {
                InitializeAudioSource();
            }
            if (!enableSounds || clip == null || audioSource == null)
                return;

            float finalVolume = masterVolume * volumeMultiplier;
            audioSource.PlayOneShot(clip, finalVolume);
        }

        #endregion

        #region Editor Utilities

#if UNITY_EDITOR

        [Title("Editor Utilities")]
        [Button("Auto-Setup Audio Source")]
        public void AutoSetupAudioSource()
        {
            if (audioSource == null)
            {
                audioSource = GetComponent<AudioSource>();
                if (audioSource == null)
                {
                    audioSource = gameObject.AddComponent<AudioSource>();
                }
            }

            audioSource.playOnAwake = false;
            audioSource.loop = false;
            audioSource.spatialBlend = 0f;
            audioSource.volume = 1f;

            UnityEditor.EditorUtility.SetDirty(this);
        }

        [Button("Create UI Sounds GameObject")]
        private static void CreateUISoundsGameObject()
        {
            GameObject soundsGO = new GameObject("UI Sounds Manager");
            soundsGO.AddComponent<TLUISounds>();
            UnityEditor.Selection.activeGameObject = soundsGO;
        }

#endif

        #endregion



        public enum SoundType
        {
            Hover,
            Click,
            Swipe,
            Back,
            Error
        }
    }
}