using System;

namespace TLUIToolkit
{
    public class SelectorLogic
    {
        // Public Properties (Read-only access to internal state)
        public int CurrentIndex { get; private set; }
        public int ItemsCount { get; private set; }
        public bool AllowLooping { get; private set; }

        // Events for external subscription
        public event Action<int> OnIndexChanged;
        public event Action OnNext;
        public event Action OnPrevious;
        public event Action<string> OnValidationError; // For logic-level validation errors

        // Constructor
        public SelectorLogic(int initialItemsCount = 0, bool allowLooping = false, int initialIndex = 0)
        {
            ItemsCount = initialItemsCount;
            AllowLooping = allowLooping;
            CurrentIndex = initialIndex; // Initial index is set but will be clamped by SetItemsCount if out of bounds
            SetItemsCount(initialItemsCount); // This will also clamp the initialIndex if necessary
        }

        // Public Methods for controlling the selector
        public bool GoNext()
        {
            if (ItemsCount <= 0)
            {
                InvokeValidationError("Cannot go next: No items available.");
                return false;
            }

            if (!CanGoNext())
            {
                InvokeValidationError("Cannot go next: No more items or looping is disabled.");
                return false;
            }

            int oldIndex = CurrentIndex;
            CurrentIndex++;

            if (CurrentIndex >= ItemsCount)
            {
                CurrentIndex = AllowLooping ? 0 : ItemsCount - 1;
            }

            if (oldIndex != CurrentIndex)
            {
                OnNext?.Invoke();
                OnIndexChanged?.Invoke(CurrentIndex);
            }
            return true;
        }

        public bool GoPrevious()
        {
            if (ItemsCount <= 0)
            {
                InvokeValidationError("Cannot go previous: No items available.");
                return false;
            }

            if (!CanGoPrevious())
            {
                InvokeValidationError("Cannot go previous: No more items or looping is disabled.");
                return false;
            }

            int oldIndex = CurrentIndex;
            CurrentIndex--;

            if (CurrentIndex < 0)
            {
                CurrentIndex = AllowLooping ? ItemsCount - 1 : 0;
            }

            if (oldIndex != CurrentIndex)
            {
                OnPrevious?.Invoke();
                OnIndexChanged?.Invoke(CurrentIndex);
            }
            return true;
        }

        public bool SetIndex(int newIndex)
        {
            if (ItemsCount <= 0)
            {
                InvokeValidationError("Cannot set index: No items available.");
                return false;
            }

            if (!IsValidIndex(newIndex))
            {
                InvokeValidationError($"Invalid index: {newIndex}. Must be between 0 and {ItemsCount - 1}");
                return false;
            }

            if (CurrentIndex == newIndex)
            {
                return true; // No change needed
            }

            CurrentIndex = newIndex;
            OnIndexChanged?.Invoke(CurrentIndex);
            return true;
        }

        public void SetItemsCount(int newCount)
        {
            if (newCount < 0)
            {
                InvokeValidationError("ItemsCount cannot be negative.");
                return;
            }

            ItemsCount = newCount;

            // Adjust current index if it's out of bounds
            if (ItemsCount > 0 && CurrentIndex >= ItemsCount)
            {
                CurrentIndex = ItemsCount - 1;
                OnIndexChanged?.Invoke(CurrentIndex); // Invoke if index was clamped
            }
            else if (ItemsCount == 0 && CurrentIndex != 0)
            {
                CurrentIndex = 0;
                OnIndexChanged?.Invoke(CurrentIndex); // Invoke if index was reset to 0
            }
        }

        public void ResetToFirst()
        {
            SetIndex(0);
        }

        public void ResetToLast()
        {
            if (ItemsCount > 0)
            {
                SetIndex(ItemsCount - 1);
            }
        }

        public void SetAllowLooping(bool allow)
        {
            AllowLooping = allow;
        }

        // Validation properties (pure logic)
        public bool IsValidIndex(int index) => ItemsCount > 0 && index >= 0 && index < ItemsCount;
        public bool CanGoNext() => ItemsCount > 0 && (AllowLooping || CurrentIndex < ItemsCount - 1);
        public bool CanGoPrevious() => ItemsCount > 0 && (AllowLooping || CurrentIndex > 0);

        // Internal method for invoking validation errors
        private void InvokeValidationError(string message)
        {
            OnValidationError?.Invoke(message);
        }
    }
}