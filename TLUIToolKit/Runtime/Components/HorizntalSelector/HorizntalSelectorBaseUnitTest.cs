using UnityEngine;
using UnityEngine.UI; // Required for Button
using System.Collections;
using System.Threading.Tasks;

namespace TLUIToolkit.Tests
{
    public class HorizontalSelectorBaseUnitTest : MonoBehaviour
    {
        [SerializeField] private TLHorizontalSelectorBase selectorBase;
        [SerializeField] private Button nextButton;
        [SerializeField] private Button previousButton;

        void Start()
        {
            // It's good practice to run tests as coroutines or async methods in Unity
            // to allow for frames to pass if needed for UI updates or asynchronous operations.
            StartCoroutine(RunTests());
        }

        private IEnumerator RunTests()
        {
            TLDebug.LogGreen("--- Starting TLHorizontalSelectorBase Unit Tests ---");

            // Ensure references are assigned in the inspector
            if (selectorBase == null || nextButton == null || previousButton == null)
            {
                TLDebug.LogRed("Assign SelectorBase, NextButton, and PreviousButton in the inspector to run tests.");
                yield break;
            }

            // Test 1: Initialization with default values
            yield return TestInitialization();

            // Test 2: Basic navigation
            yield return TestBasicNavigation();

            // Test 3: Looping enabled
            yield return TestLoopingEnabled();

            // Test 4: Looping disabled
            yield return TestLoopingDisabled();

            // Test 5: Setting index
            yield return TestSetIndex();

            // Test 6: Reset to first/last
            yield return TestResetToFirstAndLast();

            // Test 7: Setting items count
            yield return TestSetItemsCount();

            // Test 8: No items scenario
            yield return TestNoItemsScenario();

            // Test 9: Button interactability
            yield return TestButtonInteractability();


            TLDebug.LogGreen("--- TLHorizontalSelectorBase Unit Tests Completed ---");
        }

        private IEnumerator TestInitialization()
        {
            TLDebug.LogGreen("\n--- Test Initialization ---");
            selectorBase.InitializeWith(5, 0); // Re-initialize for a clean start

            // Wait a frame for UI to update (if any) and internal logic to settle
            yield return null;

            LogTestResult("Initial Index is 0", selectorBase.CurrentIndex == 0);
            LogTestResult("Items Count is 5", selectorBase.ItemsCount == 5);
           // LogTestResult("Allow Looping is false (default)", selectorBase.AllowLooping == false);
            LogTestResult("Is Initialized is true", selectorBase.IsInitialized == true);
            LogTestResult("Has Valid References is true", selectorBase.HasValidReferences == true);
        }

        private IEnumerator TestBasicNavigation()
        {
            TLDebug.LogGreen("\n--- Test Basic Navigation (No Looping) ---");
            selectorBase.InitializeWith(3, 0); // 0, 1, 2
            selectorBase.SetAllowLooping(false);
            yield return null;

            // Go Next
            selectorBase.Next();
            yield return null;
            LogTestResult("Next from 0 to 1", selectorBase.CurrentIndex == 1);

            selectorBase.Next();
            yield return null;
            LogTestResult("Next from 1 to 2", selectorBase.CurrentIndex == 2);

            selectorBase.Next(); // Should attempt to go beyond last, but be clamped
            yield return null;
            LogTestResult("Next from 2 (clamped)", selectorBase.CurrentIndex == 2);
            LogTestResult("CanGoNext is false at end (no looping)", !selectorBase.CanGoNextProperty);


            // Go Previous
            selectorBase.Previous();
            yield return null;
            LogTestResult("Previous from 2 to 1", selectorBase.CurrentIndex == 1);

            selectorBase.Previous();
            yield return null;
            LogTestResult("Previous from 1 to 0", selectorBase.CurrentIndex == 0);

            selectorBase.Previous(); // Should attempt to go below first, but be clamped
            yield return null;
            LogTestResult("Previous from 0 (clamped)", selectorBase.CurrentIndex == 0);
            LogTestResult("CanGoPrevious is false at start (no looping)", !selectorBase.CanGoPreviousProperty);
        }

        private IEnumerator TestLoopingEnabled()
        {
            TLDebug.LogGreen("\n--- Test Looping Enabled ---");
            selectorBase.InitializeWith(3, 0); // 0, 1, 2
            selectorBase.SetAllowLooping(true);
            yield return null;

            selectorBase.Next(); // 0 -> 1
            yield return null;
            LogTestResult("Next (Looping) from 0 to 1", selectorBase.CurrentIndex == 1);

            selectorBase.Next(); // 1 -> 2
            yield return null;
            LogTestResult("Next (Looping) from 1 to 2", selectorBase.CurrentIndex == 2);

            selectorBase.Next(); // 2 -> 0 (loop)
            yield return null;
            LogTestResult("Next (Looping) from 2 to 0", selectorBase.CurrentIndex == 0);
            LogTestResult("CanGoNext is true (looping)", selectorBase.CanGoNextProperty);

            selectorBase.Previous(); // 0 -> 2 (loop)
            yield return null;
            LogTestResult("Previous (Looping) from 0 to 2", selectorBase.CurrentIndex == 2);
            LogTestResult("CanGoPrevious is true (looping)", selectorBase.CanGoPreviousProperty);
        }

        private IEnumerator TestLoopingDisabled()
        {
            TLDebug.LogGreen("\n--- Test Looping Disabled ---");
            selectorBase.InitializeWith(3, 0); // 0, 1, 2
            selectorBase.SetAllowLooping(false);
            yield return null;

            selectorBase.SetIndex(2); // Go to last index
            yield return null;
            LogTestResult("Set index to 2", selectorBase.CurrentIndex == 2);

            selectorBase.Next(); // Try to go next from last, should stay at 2
            yield return null;
            LogTestResult("Next from last with looping disabled (should stay at 2)", selectorBase.CurrentIndex == 2);
            LogTestResult("CanGoNext is false", !selectorBase.CanGoNextProperty);

            selectorBase.SetIndex(0); // Go to first index
            yield return null;
            LogTestResult("Set index to 0", selectorBase.CurrentIndex == 0);

            selectorBase.Previous(); // Try to go previous from first, should stay at 0
            yield return null;
            LogTestResult("Previous from first with looping disabled (should stay at 0)", selectorBase.CurrentIndex == 0);
            LogTestResult("CanGoPrevious is false", !selectorBase.CanGoPreviousProperty);
        }


        private IEnumerator TestSetIndex()
        {
            TLDebug.LogGreen("\n--- Test Set Index ---");
            selectorBase.InitializeWith(5, 0); // 0, 1, 2, 3, 4
            yield return null;

            // Valid index
            LogTestResult("SetIndex(3) returns true", selectorBase.SetIndex(3));
            yield return null;
            LogTestResult("Index is now 3", selectorBase.CurrentIndex == 3);

            // Invalid index (too high)
            LogTestResult("SetIndex(5) returns false", !selectorBase.SetIndex(5));
            yield return null;
            LogTestResult("Index is still 3 after invalid SetIndex", selectorBase.CurrentIndex == 3);

            // Invalid index (negative)
            LogTestResult("SetIndex(-1) returns false", !selectorBase.SetIndex(-1));
            yield return null;
            LogTestResult("Index is still 3 after invalid SetIndex", selectorBase.CurrentIndex == 3);

            // Set to current index (should return true, no change)
            LogTestResult("SetIndex(3) when already 3 returns true", selectorBase.SetIndex(3));
            yield return null;
            LogTestResult("Index remains 3", selectorBase.CurrentIndex == 3);
        }

        private IEnumerator TestResetToFirstAndLast()
        {
            TLDebug.LogGreen("\n--- Test Reset To First and Last ---");
            selectorBase.InitializeWith(4, 2); // 0, 1, 2, 3, starting at 2
            yield return null;

            selectorBase.ResetToFirst();
            yield return null;
            LogTestResult("ResetToFirst sets index to 0", selectorBase.CurrentIndex == 0);

            selectorBase.ResetToLast();
            yield return null;
            LogTestResult("ResetToLast sets index to ItemsCount - 1 (3)", selectorBase.CurrentIndex == 3);

            selectorBase.SetItemsCount(0); // Test with no items
            yield return null;
            selectorBase.ResetToLast(); // Should do nothing or log an error from SetIndex if it tries
            yield return null;
            LogTestResult("ResetToLast with 0 items (index should be 0)", selectorBase.CurrentIndex == 0);
        }

        private IEnumerator TestSetItemsCount()
        {
            TLDebug.LogGreen("\n--- Test Set Items Count ---");
            selectorBase.InitializeWith(5, 2); // 0,1,2,3,4 - Current: 2
            yield return null;

            // Increase items count
            selectorBase.SetItemsCount(7); // 0,1,2,3,4,5,6 - Current: 2
            yield return null;
            LogTestResult("ItemsCount increased to 7", selectorBase.ItemsCount == 7);
            LogTestResult("CurrentIndex remains 2", selectorBase.CurrentIndex == 2);

            // Decrease items count, current index stays valid
            selectorBase.SetItemsCount(4); // 0,1,2,3 - Current: 2
            yield return null;
            LogTestResult("ItemsCount decreased to 4", selectorBase.ItemsCount == 4);
            LogTestResult("CurrentIndex remains 2", selectorBase.CurrentIndex == 2);

            // Decrease items count, current index becomes invalid (clamped)
            selectorBase.SetItemsCount(1); // 0 - Current: 0 (was 2, clamped)
            yield return null;
            LogTestResult("ItemsCount decreased to 1", selectorBase.ItemsCount == 1);
            LogTestResult("CurrentIndex clamped to 0", selectorBase.CurrentIndex == 0);

            // Set items count to 0
            selectorBase.SetItemsCount(0); // Current: 0
            yield return null;
            LogTestResult("ItemsCount set to 0", selectorBase.ItemsCount == 0);
            LogTestResult("CurrentIndex set to 0 when ItemsCount is 0", selectorBase.CurrentIndex == 0);

            // Set items count from 0 to 3
            selectorBase.SetItemsCount(3); // Current: 0
            yield return null;
            LogTestResult("ItemsCount set from 0 to 3", selectorBase.ItemsCount == 3);
            LogTestResult("CurrentIndex remains 0", selectorBase.CurrentIndex == 0);
        }

        private IEnumerator TestNoItemsScenario()
        {
            TLDebug.LogGreen("\n--- Test No Items Scenario ---");
            selectorBase.InitializeWith(0, 0);
            yield return null;

            LogTestResult("Initial ItemsCount is 0", selectorBase.ItemsCount == 0);
            LogTestResult("CurrentIndex is 0", selectorBase.CurrentIndex == 0);
            LogTestResult("CanGoNext is false (0 items)", !selectorBase.CanGoNextProperty);
            LogTestResult("CanGoPrevious is false (0 items)", !selectorBase.CanGoPreviousProperty);

            // Try to navigate
            selectorBase.Next();
            yield return null;
            LogTestResult("Calling Next with 0 items does not change index", selectorBase.CurrentIndex == 0);

            selectorBase.Previous();
            yield return null;
            LogTestResult("Calling Previous with 0 items does not change index", selectorBase.CurrentIndex == 0);

            // Try to set index
            LogTestResult("SetIndex(0) with 0 items returns false", !selectorBase.SetIndex(0));
            LogTestResult("SetIndex(1) with 0 items returns false", !selectorBase.SetIndex(1));
            yield return null;
        }

        private IEnumerator TestButtonInteractability()
        {
            TLDebug.LogGreen("\n--- Test Button Interactability ---");
            selectorBase.InitializeWith(3, 0); // 0, 1, 2, start at 0 (first)
            selectorBase.SetAllowLooping(false);
            yield return null;

            LogTestResult("At index 0 (no looping): Previous button is disabled", !previousButton.interactable);
            LogTestResult("At index 0 (no looping): Next button is enabled", nextButton.interactable);

            selectorBase.Next(); // Move to 1
            yield return null;

            LogTestResult("At index 1 (no looping): Previous button is enabled", previousButton.interactable);
            LogTestResult("At index 1 (no looping): Next button is enabled", nextButton.interactable);

            selectorBase.SetIndex(2); // Move to 2 (last)
            yield return null;

            LogTestResult("At index 2 (no looping): Previous button is enabled", previousButton.interactable);
            LogTestResult("At index 2 (no looping): Next button is disabled", !nextButton.interactable);

            selectorBase.SetAllowLooping(true);
            yield return null;

            LogTestResult("At index 2 (looping): Previous button is enabled", previousButton.interactable);
            LogTestResult("At index 2 (looping): Next button is enabled", nextButton.interactable);

            selectorBase.SetItemsCount(0); // No items
            yield return null;

            LogTestResult("With 0 items: Previous button is disabled", !previousButton.interactable);
            LogTestResult("With 0 items: Next button is disabled", !nextButton.interactable);
        }


        private void LogTestResult(string testName, bool passed)
        {
            if (passed)
            {
                TLDebug.LogGreen($"[PASS] {testName}");
            }
            else
            {
                TLDebug.LogRed($"[FAIL] {testName}");
            }
        }
    }
}