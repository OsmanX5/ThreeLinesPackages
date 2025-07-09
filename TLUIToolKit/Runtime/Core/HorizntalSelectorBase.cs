using Sirenix.OdinInspector;
using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace TLUIToolkit
{
    public class HorizontalSelectorBase : MonoBehaviour
    {
        [Title("References")]
        [SerializeField, Required]
        [Tooltip("Button to navigate to next item")]
        private Button nextBtn;

        [SerializeField, Required]
        [Tooltip("Button to navigate to previous item")]
        private Button previousBtn;

        [Title("Configuration")]
        [SerializeField, Min(0)]
        [Tooltip("Total number of items in the selector")]
        protected int initialItemsCount = 0;

        [SerializeField]
        [Tooltip("Allow looping from last to first item and vice versa")]
        protected bool allowLooping = false;

        [SerializeField, Min(0)]
        [Tooltip("The initial index to set when the selector starts")]
        protected int initialIndex = 0;

        [InfoBox("Not Recomended to use these events in production code, use c# Events instead.", Icon = SdfIconType.ExclamationCircleFill, InfoMessageType = InfoMessageType.Warning)]
        [Title("Unity Events")]
        public UnityEvent OnNextUnityEvent;
        public UnityEvent OnPreviousUnityEvent;
        public UnityEvent<int> OnIndexChangedUnityEvent;
        public UnityEvent<string> OnValidationErrorUnityEvent; // New UnityEvent for validation errors

        [Title("Debug Info")]
        [ShowInInspector, ReadOnly]
        [Tooltip("Current selected index")]
        private int _currentIndex = 0; // Backing field for inspector display

        [ShowInInspector, ReadOnly]
        [Tooltip("Is the component properly initialized?")]
        private bool isInitialized = false;

        [ShowInInspector, ReadOnly]
        [Tooltip("Are all required references assigned?")]
        private bool hasValidReferences = false;

        // The pure C# logic class instance, now a public property
        public SelectorLogic Selector { get; private set; }

        // Public Properties for accessing selector state through the component
        public int CurrentIndex => Selector != null ? Selector.CurrentIndex : _currentIndex; // Use backing field if logic isn't ready
        public int ItemsCount => Selector != null ? Selector.ItemsCount : initialItemsCount;
        public bool AllowLooping => Selector != null ? Selector.AllowLooping : allowLooping;
        public bool IsInitialized => isInitialized;
        public bool HasValidReferences => hasValidReferences;

        // C# Events are removed from here, only UnityEvents are exposed.
        // public event Action<int> OnIndexChanged;
        // public event Action OnNext;
        // public event Action OnPrevious;
        // public event Action<string> OnValidationError;

        #region Unity Lifecycle

        private void Awake()
        {
            try
            {
                InitlizeWith(initialItemsCount, initialIndex); // Initialize with the initial values
            }
            catch (Exception ex)
            {
                Debug.LogError($"[{gameObject.name}] Failed to initialize HorizontalSelectorBase: {ex.Message}", this);
                isInitialized = false;
            }
        }

        private void OnValidate()
        {
            // Ensure itemsCount is not negative
            if (initialItemsCount < 0)
            {
                initialItemsCount = 0;
                Debug.LogWarning($"[{gameObject.name}] Initial Items Count cannot be negative. Set to 0.", this);
            }

            // Ensure initialIndex is not negative
            if (initialIndex < 0)
            {
                initialIndex = 0;
                Debug.LogWarning($"[{gameObject.name}] Initial Index cannot be negative. Set to 0.", this);
            }

            // Clamp initialIndex to valid range relative to initialItemsCount
            if (initialItemsCount > 0)
            {
                if (initialIndex >= initialItemsCount)
                {
                    initialIndex = initialItemsCount - 1;
                    Debug.LogWarning($"[{gameObject.name}] Initial Index clamped to max valid index: {initialIndex}", this);
                }
            }
            else // If itemsCount is 0, initialIndex should also be 0
            {
                if (initialIndex != 0)
                {
                    initialIndex = 0;
                    Debug.LogWarning($"[{gameObject.name}] Initial Items Count is 0, Initial Index set to 0.", this);
                }
            }

            // Sync _currentIndex for inspector display, clamping it to initialItemsCount range
            _currentIndex = Mathf.Clamp(initialIndex, 0, initialItemsCount > 0 ? initialItemsCount - 1 : 0);

            // If Selector exists, update its values as well (important for editor play mode transitions or live changes)
            if (Selector != null)
            {
                Selector.SetItemsCount(initialItemsCount);
                Selector.SetAllowLooping(allowLooping);
                Selector.SetIndex(initialIndex); // Ensure logic also has the desired initial index
            }
            UpdateButtonsInteractibility(); // Update interactability in editor if values change
        }

        private void OnDestroy()
        {
            CleanupButtons();
            UnsubscribeFromLogicEvents();
        }

        #endregion

        #region Initialization & Validation

        private void ValidateReferences()
        {
            var errors = new System.Collections.Generic.List<string>();

            if (nextBtn == null)
                errors.Add("Next button reference is missing");

            if (previousBtn == null)
                errors.Add("Previous button reference is missing");

            hasValidReferences = errors.Count == 0;

            if (!hasValidReferences)
            {
                var errorMessage = $"Reference validation failed: {string.Join(", ", errors)}";
                Debug.LogError($"[{gameObject.name}] {errorMessage}", this);
                OnValidationErrorUnityEvent?.Invoke(errorMessage); // Invoke UnityEvent for validation errors
                throw new InvalidOperationException(errorMessage);
            }
        }

        private void InitializeButtons()
        {
            if (nextBtn != null)
            {
                nextBtn.onClick.RemoveAllListeners(); // Use RemoveAllListeners for safety
                nextBtn.onClick.AddListener(OnNextButtonClicked);
            }

            if (previousBtn != null)
            {
                previousBtn.onClick.RemoveAllListeners(); // Use RemoveAllListeners for safety
                previousBtn.onClick.AddListener(OnPreviousButtonClicked);
            }
        }

        private void CleanupButtons()
        {
            if (nextBtn != null)
                nextBtn.onClick.RemoveListener(OnNextButtonClicked);

            if (previousBtn != null)
                previousBtn.onClick.RemoveListener(OnPreviousButtonClicked);
        }

        bool isSubscribedToLogicEvents = false;
        private void SubscribeToLogicEvents()
        {
            if (Selector == null || isSubscribedToLogicEvents) return;

            Selector.OnIndexChanged += HandleLogicIndexChanged;
            Selector.OnNext += HandleLogicNext;
            Selector.OnPrevious += HandleLogicPrevious;
            Selector.OnValidationError += HandleLogicValidationError;
            isSubscribedToLogicEvents=true;
        }

        private void UnsubscribeFromLogicEvents()
        {
            if (Selector == null|| !isSubscribedToLogicEvents) return;

            Selector.OnIndexChanged -= HandleLogicIndexChanged;
            Selector.OnNext -= HandleLogicNext;
            Selector.OnPrevious -= HandleLogicPrevious;
            Selector.OnValidationError -= HandleLogicValidationError;
            isSubscribedToLogicEvents = false;
        }

        #endregion

        #region Event Handlers for Logic

        private void HandleLogicIndexChanged(int newIndex)
        {
            _currentIndex = newIndex; // Update the inspector field
            // OnIndexChanged?.Invoke(newIndex); // Removed C# event
            OnIndexChangedUnityEvent?.Invoke(newIndex);
            UpdateButtonsInteractibility();
        }

        private void HandleLogicNext()
        {
            // OnNext?.Invoke(); // Removed C# event
            OnNextUnityEvent?.Invoke();
        }

        private void HandleLogicPrevious()
        {
            // OnPrevious?.Invoke(); // Removed C# event
            OnPreviousUnityEvent?.Invoke();
        }

        private void HandleLogicValidationError(string message)
        {
            Debug.LogError($"[{gameObject.name}] Logic Validation Error: {message}", this);
            // OnValidationError?.Invoke(message); // Removed C# event
            OnValidationErrorUnityEvent?.Invoke(message); // Invoke UnityEvent
        }

        #endregion

        #region Button Callbacks (Delegate to Logic)

        private void OnNextButtonClicked()
        {
            if (Selector != null)
            {
                Selector.GoNext();
            }
            else
            {
                Debug.LogError($"[{gameObject.name}] SelectorLogic not initialized when Next button clicked.", this);
            }
        }

        private void OnPreviousButtonClicked()
        {
            if (Selector != null)
            {
                Selector.GoPrevious();
            }
            else
            {
                Debug.LogError($"[{gameObject.name}] SelectorLogic not initialized when Previous button clicked.", this);
            }
        }

        #endregion

        #region Public Methods (Exposed from Component, delegate to Logic)

        public void InitlizeWith(int itemsCount , int startIndex)
        {
            CleanupButtons();
            ValidateReferences();
            // Initialize the pure logic class with the initialIndex
            Selector = new SelectorLogic(itemsCount, allowLooping, startIndex);
            SubscribeToLogicEvents();
            InitializeButtons();
            isInitialized = true;
            UpdateButtonsInteractibility(); // Initial update based on logic state
        }
        [Button("Next Item")]
        [EnableIf("@isInitialized && ItemsCount > 0")] // Using ItemsCount from component for Odin
        public virtual void Next()
        {
            if (Selector != null)
            {
                Selector.GoNext();
            }
            else
            {
                Debug.LogError($"[{gameObject.name}] SelectorLogic not initialized when calling Next()", this);
            }
        }

        [Button("Previous Item")]
        [EnableIf("@isInitialized && ItemsCount > 0")] // Using ItemsCount from component for Odin
        public virtual void Previous()
        {
            if (Selector != null)
            {
                Selector.GoPrevious();
            }
            else
            {
                Debug.LogError($"[{gameObject.name}] SelectorLogic not initialized when calling Previous()", this);
            }
        }

        [Button("Set Index")]
        [EnableIf("@isInitialized && ItemsCount > 0")]
        public virtual bool SetIndex(int newIndex)
        {
            if (Selector != null)
            {
                return Selector.SetIndex(newIndex);
            }
            Debug.LogError($"[{gameObject.name}] SelectorLogic not initialized when calling SetIndex()", this);
            return false;
        }

        [Button("Reset to First")]
        [EnableIf("@isInitialized && ItemsCount > 0")]
        public virtual void ResetToFirst()
        {
            if (Selector != null)
            {
                Selector.ResetToFirst();
            }
            else
            {
                Debug.LogError($"[{gameObject.name}] SelectorLogic not initialized when calling ResetToFirst()", this);
            }
        }

        [Button("Reset to Last")]
        [EnableIf("@isInitialized && ItemsCount > 0")]
        public virtual void ResetToLast()
        {
            if (Selector != null)
            {
                Selector.ResetToLast();
            }
            else
            {
                Debug.LogError($"[{gameObject.name}] SelectorLogic not initialized when calling ResetToLast()", this);
            }
        }

        public virtual void SetItemsCount(int newCount)
        {
            initialItemsCount = newCount; // Keep the inspector value updated
            if (Selector != null)
            {
                Selector.SetItemsCount(newCount);
            }
            else
            {
                Debug.LogWarning($"[{gameObject.name}] SelectorLogic not initialized. SetItemsCount will only update initialItemsCount property.", this);
            }
        }

        // Expose CanGoNext/Previous from logic
        public bool CanGoNext => Selector != null ? Selector.CanGoNext() : false;
        public bool CanGoPrevious => Selector != null ? Selector.CanGoPrevious() : false;


        #endregion

        #region Private Methods

        private void UpdateButtonsInteractibility()
        {
            if (!isInitialized || !hasValidReferences || Selector == null)
            {
                // Ensure buttons are disabled if not initialized or references are missing
                if (nextBtn != null) nextBtn.interactable = false;
                if (previousBtn != null) previousBtn.interactable = false;
                return;
            }

            try
            {
                if (nextBtn != null)
                {
                    nextBtn.interactable = Selector.CanGoNext();
                }

                if (previousBtn != null)
                {
                    previousBtn.interactable = Selector.CanGoPrevious();
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[{gameObject.name}] Error updating button interactability: {ex.Message}", this);
            }
        }

        #endregion

        #region Debug Functions (Directly use Debug.Log)

        [Button("Validate Configuration")]
        [PropertySpace(10)]
        private void DebugValidateConfiguration()
        {
            Debug.Log($"[{gameObject.name}] === Configuration Validation ===", this);
            Debug.Log($"[{gameObject.name}] Initialized: {isInitialized}", this);
            Debug.Log($"[{gameObject.name}] Valid References: {hasValidReferences}", this);
            Debug.Log($"[{gameObject.name}] Items Count: {ItemsCount} (from logic: {Selector?.ItemsCount})", this);
            Debug.Log($"[{gameObject.name}] Current Index: {CurrentIndex} (from logic: {Selector?.CurrentIndex})", this);
            Debug.Log($"[{gameObject.name}] Allow Looping: {AllowLooping} (from logic: {Selector?.AllowLooping})", this);
            Debug.Log($"[{gameObject.name}] Can Go Next: {CanGoNext} (from logic: {Selector?.CanGoNext()})", this);
            Debug.Log($"[{gameObject.name}] Can Go Previous: {CanGoPrevious} (from logic: {Selector?.CanGoPrevious()})", this);
            Debug.Log($"[{gameObject.name}] Next Button: {(nextBtn != null ? "Assigned" : "Missing")}", this);
            Debug.Log($"[{gameObject.name}] Previous Button: {(previousBtn != null ? "Assigned" : "Missing")}", this);
        }

        [Button("Test Navigation")]
        [EnableIf("@isInitialized && ItemsCount > 0")]
        private async void DebugTestNavigation(int pauseTime =0)
        {
            Debug.Log($"[{gameObject.name}] Testing navigation...", this);
            for (int i = 0; i < ItemsCount + 2; i++)
            {
                Debug.Log($"[{gameObject.name}] Step {i}: Index {CurrentIndex}", this);
                Next();
                await Task.Delay(pauseTime * 1000);
            }
        }

        #endregion
    }
}