using Sirenix.OdinInspector;
using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace TLUIToolkit
{
    public class TLHorizontalSelectorBase : MonoBehaviour
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

        [Title("Unity Events")]
        public UnityEvent OnNextUnityEvent;
        public UnityEvent OnPreviousUnityEvent;
        public UnityEvent<int> OnIndexChangedUnityEvent;

        [Title("Debug Info")]
        [ShowInInspector, ReadOnly]
        [Tooltip("Is the component properly initialized?")]
        private bool isInitialized = false;

        [ShowInInspector, ReadOnly]
        [Tooltip("Are all required references assigned?")]
        private bool hasValidReferences = false;

        // The pure C# logic class instance, now a public property
        public SelectorLogic Selector { get; private set; }

        // Public Properties for accessing selector state through the component
        public int CurrentIndex => Selector != null ? Selector.CurrentIndex : initialIndex;
        public int ItemsCount => Selector != null ? Selector.ItemsCount : initialItemsCount;
        public bool AllowLooping => Selector != null ? Selector.AllowLooping : allowLooping;
        public bool IsInitialized => isInitialized;
        public bool HasValidReferences => hasValidReferences;

        #region Unity Lifecycle

        private void Awake()
        {
            try
            {
                // Initialize with the initial values
                InitializeWith(initialItemsCount, initialIndex);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[{gameObject.name}] Failed to initialize TLHorizontalSelectorBase: {ex.Message}", this);
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

            // If Selector exists, update its values as well (important for editor play mode transitions or live changes)
            if (Selector != null)
            {
                Selector.SetItemsCount(initialItemsCount);
                Selector.AllowLooping= allowLooping;
                Selector.SetIndex(initialIndex); // Ensure logic also has the desired initial index
            }
            UpdateButtonsInteractibility(); // Update interactability in editor if values change
        }

        private void OnDestroy()
        {
            CleanupButtons();
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
                throw new InvalidOperationException(errorMessage);
            }
        }

        private void InitializeButtons()
        {
            if (nextBtn != null)
            {
                if(IsInitialized)
                    nextBtn.onClick.RemoveListener(OnNextButtonClicked);
                nextBtn.onClick.AddListener(OnNextButtonClicked);
            }

            if (previousBtn != null)
            {
                if (IsInitialized)
                    previousBtn.onClick.RemoveListener(OnPreviousButtonClicked);
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

        #endregion

        #region Button Callbacks (Delegate to Logic)

        private void OnNextButtonClicked()
        {
            if (Selector != null)
            {
                // GoNext will now internally log errors if any
                Selector.GoNext();
                // Manually invoke UnityEvents after the logic call
                OnNextUnityEvent?.Invoke();
                OnIndexChangedUnityEvent?.Invoke(Selector.CurrentIndex);
                UpdateButtonsInteractibility();
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
                // GoPrevious will now internally log errors if any
                Selector.GoPrevious();
                // Manually invoke UnityEvents after the logic call
                OnPreviousUnityEvent?.Invoke();
                OnIndexChangedUnityEvent?.Invoke(Selector.CurrentIndex);
                UpdateButtonsInteractibility();
            }
            else
            {
                Debug.LogError($"[{gameObject.name}] SelectorLogic not initialized when Previous button clicked.", this);
            }
        }

        #endregion

        #region Public Methods (Exposed from Component, delegate to Logic)

        public void InitializeWith(int itemsCount, int startIndex)
        {
            CleanupButtons();
            ValidateReferences();
            // Initialize the pure logic class with the initialIndex
            Selector = new SelectorLogic(itemsCount, allowLooping, startIndex);
            InitializeButtons();
            isInitialized = true;
            UpdateButtonsInteractibility(); // Initial update based on logic state
        }

        [Button("Next Item")]
        [EnableIf("@isInitialized && ItemsCount > 0")]
        public virtual void Next()
        {
            if (Selector != null)
            {
                // Next will now internally log errors if any
                Selector.GoNext();
                OnNextUnityEvent?.Invoke();
                OnIndexChangedUnityEvent?.Invoke(Selector.CurrentIndex);
                UpdateButtonsInteractibility();
            }
            else
            {
                Debug.LogError($"[{gameObject.name}] SelectorLogic not initialized when calling Next()", this);
            }
        }

        [Button("Previous Item")]
        [EnableIf("@isInitialized && ItemsCount > 0")]
        public virtual void Previous()
        {
            if (Selector != null)
            {
                // Previous will now internally log errors if any
                Selector.GoPrevious();
                OnPreviousUnityEvent?.Invoke();
                OnIndexChangedUnityEvent?.Invoke(Selector.CurrentIndex);
                UpdateButtonsInteractibility();
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
                bool success = Selector.SetIndex(newIndex);
                if (success)
                {
                    OnIndexChangedUnityEvent?.Invoke(Selector.CurrentIndex);
                    UpdateButtonsInteractibility();
                }
                return success;
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
                OnIndexChangedUnityEvent?.Invoke(Selector.CurrentIndex);
                UpdateButtonsInteractibility();
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
                OnIndexChangedUnityEvent?.Invoke(Selector.CurrentIndex);
                UpdateButtonsInteractibility();
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
                int oldIndex = Selector.CurrentIndex;
                Selector.SetItemsCount(newCount);
                // If the index was clamped due to new items count, fire the event
                if (oldIndex != Selector.CurrentIndex)
                {
                    OnIndexChangedUnityEvent?.Invoke(Selector.CurrentIndex);
                }
                UpdateButtonsInteractibility();
            }
            else
            {
                Debug.LogWarning($"[{gameObject.name}] SelectorLogic not initialized. SetItemsCount will only update initialItemsCount property.", this);
            }
        }

        /// <summary>
        /// Sets whether the selector should allow looping from the last to the first item and vice versa.
        /// </summary>
        /// <param name="allow">True to allow looping, false otherwise.</param>
        public virtual void SetAllowLooping(bool allow)
        {
            allowLooping = allow; // Keep the inspector value updated
            if (Selector != null)
            {
                Selector.AllowLooping= allow;
                UpdateButtonsInteractibility(); // Update interactability based on new looping state
            }
            else
            {
                Debug.LogWarning($"[{gameObject.name}] SelectorLogic not initialized. SetAllowLooping will only update allowLooping property.", this);
            }
        }

        // Expose CanGoNext/Previous from logic
        public bool CanGoNextProperty => Selector != null ? Selector.CanGoNext() : false;
        public bool CanGoPreviousProperty => Selector != null ? Selector.CanGoPrevious() : false;


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
            Debug.Log($"[{gameObject.name}] Can Go Next: {CanGoNextProperty} (from logic: {Selector?.CanGoNext()})", this);
            Debug.Log($"[{gameObject.name}] Can Go Previous: {CanGoPreviousProperty} (from logic: {Selector?.CanGoPrevious()})", this);
            Debug.Log($"[{gameObject.name}] Next Button: {(nextBtn != null ? "Assigned" : "Missing")}", this);
            Debug.Log($"[{gameObject.name}] Previous Button: {(previousBtn != null ? "Assigned" : "Missing")}", this);
        }

        [Button("Test Navigation")]
        [EnableIf("@isInitialized && ItemsCount > 0")]
        private async void DebugTestNavigation(int pauseTime = 0)
        {
            Debug.Log($"[{gameObject.name}] Testing navigation...", this);
            for (int i = 0; i < ItemsCount + 2; i++)
            {
                Debug.Log($"[{gameObject.name}] Step {i}: Index {CurrentIndex}", this);
                Next();
                await Task.Delay(pauseTime * 1000);
            }
        }

        [Button("Test Initialize")]
        void UnitTest_InitializeWith(int count, int index) => InitializeWith(count, index);

        #endregion
    }
}