using System.Collections;
using UnityEngine;
using TMPro;
using Sirenix.OdinInspector;
using UnityEditor;
namespace TLUIToolkit
{
    public class TLUITypewriterEffect : MonoBehaviour
    {
        [Title("Typewriter Effect")]
        [BoxGroup("Settings")]
        [LabelWidth(100)]
        [Range(0.001f, 1f)]
        [SerializeField] private float typingSpeed = 0.05f;

        [BoxGroup("Settings")]
        [LabelWidth(100)]
        [SerializeField] private bool startOnEnable = true;

        [BoxGroup("Settings")]
        [LabelWidth(100)]
        [SerializeField] private bool hideTextOnStart = true;

        [BoxGroup("Text Content")]
        [LabelWidth(100)]
        [MultiLineProperty(3)]
        [InfoBox("Leave empty to use TextMeshPro's existing text", InfoMessageType.Info)]
        [SerializeField] private string textToType = "";

        [FoldoutGroup("Runtime Info", false)]
        [ShowInInspector, ReadOnly]
        private bool isTyping = false;

        [FoldoutGroup("Runtime Info")]
        [ShowInInspector, ReadOnly]
        private string fullText;

        [FoldoutGroup("Runtime Info")]
        [ShowInInspector, ReadOnly]
        [ProgressBar(0, 1, ColorMember = "GetProgressBarColor")]
        private float typingProgress = 0f;

        private TextMeshProUGUI textComponent;
        private Coroutine typingCoroutine;
        private int currentCharIndex = 0;

        // Events
        public System.Action OnTypingStarted;
        public System.Action OnTypingCompleted;
        public System.Action<char> OnCharacterTyped;

        void OnEnable()
        {
            // Get TextMeshPro component
            textComponent = GetComponent<TextMeshProUGUI>();
            if (textComponent == null)
            {
                Debug.LogError("TLUITypewriterEffect: No TextMeshProUGUI component found!");
                return;
            }

            // Store the full text
            fullText = string.IsNullOrEmpty(textToType) ? textComponent.text : textToType;

            // Hide text initially if specified
            if (hideTextOnStart)
                textComponent.text = "";
            if (startOnEnable)
                StartTyping();
        }


        [ButtonGroup("Controls")]
        [Button("Start Typing", ButtonSizes.Medium)]
        [EnableIf("@!isTyping")]
        public void StartTyping()
        {
            if (string.IsNullOrEmpty(fullText))
            {
                Debug.LogWarning("TLUITypewriterEffect: No text to type!");
                return;
            }

            StartTyping(fullText);
        }

        /// <summary>
        /// Starts the typing effect with custom text
        /// </summary>
        /// <param name="text">Text to type out</param>
        public void StartTyping(string text)
        {
            if (isTyping)
            {
                StopTyping();
            }

            fullText = text;
            textComponent.text = "";
            currentCharIndex = 0;
            typingProgress = 0f;
            typingCoroutine = StartCoroutine(TypeText());
        }

        [ButtonGroup("Controls")]
        [Button("Stop Typing", ButtonSizes.Medium)]
        [EnableIf("@isTyping")]
        public void StopTyping()
        {
            if (typingCoroutine != null)
            {
                StopCoroutine(typingCoroutine);
                typingCoroutine = null;
            }
            isTyping = false;
            typingProgress = 0f;
        }

        [ButtonGroup("Controls")]
        [Button("Show Full Text", ButtonSizes.Medium)]
        public void ShowFullText()
        {
            StopTyping();
            textComponent.text = fullText;
            typingProgress = 1f;
            OnTypingCompleted?.Invoke();
        }

        [ButtonGroup("Controls")]
        [Button("Skip Typing", ButtonSizes.Medium)]
        [EnableIf("@isTyping")]
        public void SkipTyping()
        {
            if (isTyping)
            {
                ShowFullText();
            }
        }

        /// <summary>
        /// Changes the typing speed during runtime
        /// </summary>
        /// <param name="newSpeed">New speed (time between characters)</param>
        [FoldoutGroup("Advanced")]
        [Button("Set Speed")]
        public void SetTypingSpeed([MinValue(0.001f)] float newSpeed)
        {
            typingSpeed = Mathf.Max(0.001f, newSpeed);
        }

        /// <summary>
        /// Coroutine that handles the character-by-character typing
        /// </summary>
        private IEnumerator TypeText()
        {
            isTyping = true;
            OnTypingStarted?.Invoke();

            for (int i = 0; i <= fullText.Length; i++)
            {
                currentCharIndex = i;
                typingProgress = (float)i / fullText.Length;

                // Update displayed text
                textComponent.text = fullText.Substring(0, i);

                // Invoke character typed event (if not at the end)
                if (i < fullText.Length)
                {
                    OnCharacterTyped?.Invoke(fullText[i]);
                }

                // Wait before next character
                yield return new WaitForSeconds(typingSpeed);
            }

            isTyping = false;
            typingProgress = 1f;
            OnTypingCompleted?.Invoke();
        }

        // Progress bar color based on typing state
        private Color GetProgressBarColor()
        {
            if (isTyping) return Color.green;
            if (typingProgress >= 1f) return Color.blue;
            return Color.gray;
        }

        // Public properties
        public bool IsTyping => isTyping;
        public float TypingSpeed => typingSpeed;
        public string FullText => fullText;
        public float TypingProgress => typingProgress;

        // Test methods for editor
        [FoldoutGroup("Testing")]
        [Button("Test with Sample Text")]
        [EnableIf("@Application.isPlaying")]
        private void TestWithSampleText()
        {
            StartTyping("This is a test message for the typewriter effect!");
        }

        [FoldoutGroup("Testing")]
        [Button("Test with Long Text")]
        [EnableIf("@Application.isPlaying")]
        private void TestWithLongText()
        {
            StartTyping("Lorem ipsum dolor sit amet, consectetur adipiscing elit. Sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris.");
        }
#if UNITY_EDITOR
        [MenuItem("CONTEXT/TextMeshProUGUI/Setup Typewriter Effect")]
        static void SetupTypewriterEffect(MenuCommand command)
        {
            TextMeshProUGUI tmpText = (TextMeshProUGUI)command.context;

            TLUITypewriterEffect typewriter = tmpText.gameObject.GetComponent<TLUITypewriterEffect>();
            if (typewriter == null)
            {
                typewriter = tmpText.gameObject.AddComponent<TLUITypewriterEffect>();
                Undo.RegisterCreatedObjectUndo(typewriter, "Setup Typewriter Effect");
            }

            // You can set default values here if needed
            // Note: Since the fields are private, you'd need to expose them or use SerializedObject

            Debug.Log($"Setup TLUITypewriterEffect on {tmpText.name}");
        }
#endif
    }

}