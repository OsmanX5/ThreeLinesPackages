using Sirenix.OdinInspector;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace TLUIToolkit
{
    /// <summary>
    /// UI element for displaying task progress with visual states and animations
    /// </summary>
    public class TaskUIElement : MonoBehaviour
    {
        #region Events
        public event Action<TaskUIData.State> OnStateChanged;
        public event Action OnStateFillAnimationStarted;
        public event Action OnStateFillAnimationCompleted;
        #endregion

        #region Serialized Fields
        [Header("Text Components")]
        [SerializeField, Required] private TMP_Text mainText;
        [SerializeField, Required] private TMP_Text subText;

        [Header("State Views")]
        [SerializeField, Required] private GameObject notStartedView;
        [SerializeField, Required] private GameObject inProgressView;
        [SerializeField, Required] private GameObject completedView;
        [SerializeField, Required] private GameObject failedView;

        [Header("Progress Line")]
        [SerializeField, Required] private GameObject lastLine;
        [SerializeField, Required] private Image lastLineFill;

        [SerializeField] private Color successColor=>TLUIColors.SuccessGreenColor;
        [SerializeField] private Color failureColor => TLUIColors.ErrorRedColor;
        [SerializeField] private Color inProgressColor => TLUIColors.WarningYellowColor;
        [SerializeField] private Color notStartedColor = Color.white;
        #endregion

        #region Private Fields
        private TaskUIData.State lastState;
        private bool isFilling = false;
        private Coroutine fillCoroutine;

        // Animation configuration
        private const float FILL_DURATION = 0.5f;
        #endregion

        #region Public Methods
        /// <summary>
        /// Sets the task data and updates the UI accordingly
        /// </summary>
        /// <param name="data">Task data to display</param>
        public void SetData(TaskUIData data)
        {
            if (data == null)
            {
                Debug.LogError("TaskUIData is null", this);
                return;
            }

            UpdateTextContent(data);
            UpdateState(data.CurrentState);
            SetLastLineVisibility(!data.IsLast);
        }

        /// <summary>
        /// Updates only the state without changing text content
        /// </summary>
        /// <param name="newState">New state to set</param>
        public void UpdateTaskState(TaskUIData.State newState)
        {
            UpdateState(newState);
        }
        public void UpdateTextContent(TaskUIData data)
        {
            if (mainText != null) mainText.text = data.MainText;
            if (subText != null) subText.text = data.SubText;
        }
        #endregion

        #region Private Methods

        private void SetLastLineVisibility(bool isVisible)
        {
            if (lastLine != null)
                lastLine.SetActive(isVisible);
        }

        private void UpdateState(TaskUIData.State newState)
        {
            if (newState == lastState) return;

            Debug.Log($"Updating {GetDisplayName()} to {newState}");

            UpdateStateViews(newState);
            UpdateTextColor(newState);
            ResetFillAmount();

            OnStateChanged?.Invoke(newState);
            StartFillAnimation(newState);

            lastState = newState;
        }

        private void UpdateStateViews(TaskUIData.State state)
        {
            SetViewState(notStartedView, state == TaskUIData.State.NotStarted);
            SetViewState(inProgressView, state == TaskUIData.State.InProgress);
            SetViewState(completedView, state == TaskUIData.State.Completed);
            SetViewState(failedView, state == TaskUIData.State.Failed);
        }

        private void SetViewState(GameObject view, bool isActive)
        {
            if (view != null)
                view.SetActive(isActive);
        }

        private void UpdateTextColor(TaskUIData.State state)
        {
            if (mainText == null) return;

            mainText.color = state switch
            {
                TaskUIData.State.Completed => successColor,
                TaskUIData.State.InProgress => inProgressColor,
                TaskUIData.State.Failed => failureColor,
                _ => notStartedColor
            };
        }

        private void ResetFillAmount()
        {
            if (lastLineFill != null)
                lastLineFill.fillAmount = 0;
        }

        private void StartFillAnimation(TaskUIData.State currentState)
        {
            if (!ValidateFillComponents()) return;

            Debug.Log($"Starting fill animation for {GetDisplayName()} with state {currentState}");

            UpdateFillColor(currentState);
            StopCurrentFillAnimation();

            var (targetFill, startFill) = GetFillParameters(currentState);
            lastLineFill.fillAmount = startFill;

            fillCoroutine = StartCoroutine(FillAnimationCoroutine(FILL_DURATION, targetFill));
        }

        private bool ValidateFillComponents()
        {
            if (lastLineFill == null)
            {
                Debug.LogWarning("Last Line Fill is not assigned in TaskUIElement", this);
                return false;
            }
            return true;
        }

        private void UpdateFillColor(TaskUIData.State state)
        {
            lastLineFill.color = state switch
            {
                TaskUIData.State.Completed => successColor,
                TaskUIData.State.Failed => failureColor,
                _ => notStartedColor
            };
        }

        private (float targetFill, float startFill) GetFillParameters(TaskUIData.State state)
        {
            return state switch
            {
                TaskUIData.State.NotStarted => (0f, 1f),
                _ => (1f, 0f)
            };
        }

        private void StopCurrentFillAnimation()
        {
            if (isFilling && fillCoroutine != null)
            {
                StopCoroutine(fillCoroutine);
                isFilling = false;
            }
        }

        private string GetDisplayName()
        {
            return mainText != null ? mainText.text : gameObject.name;
        }
        #endregion

        #region Coroutines
        private IEnumerator FillAnimationCoroutine(float duration, float targetFill)
        {
            OnStateFillAnimationStarted?.Invoke();
            Debug.Log($"Starting fill process for {GetDisplayName()} with target fill {targetFill}");

            isFilling = true;
            float elapsedTime = 0f;
            float initialFill = lastLineFill.fillAmount;

            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / duration;
                float fillAmount = Mathf.Lerp(initialFill, targetFill, t);
                lastLineFill.fillAmount = fillAmount;
                yield return null;
            }

            // Ensure final value is set
            lastLineFill.fillAmount = targetFill;
            isFilling = false;

            Debug.Log($"Fill process completed for {GetDisplayName()} with final fill {lastLineFill.fillAmount}");
            OnStateFillAnimationCompleted?.Invoke();
        }
        #endregion

        #region Unity Lifecycle
        private void OnDisable()
        {
            StopCurrentFillAnimation();
        }

        private void OnDestroy()
        {
            StopCurrentFillAnimation();
        }
        #endregion

        #region Editor Tools
#if UNITY_EDITOR
        [Button("Test Task UI")]
        [PropertySpace(10)]
        public void UnitTest(string mainText, string subText, TaskUIData.State state, bool isLast)
        {
            var testData = new TaskUIData
            {
                MainText = mainText,
                SubText = subText,
                CurrentState = state,
                IsLast = isLast
            };

            SetData(testData);
        }

        [Button("Test All States")]
        public void TestAllStates()
        {
            StartCoroutine(TestAllStatesCoroutine());
        }

        private IEnumerator TestAllStatesCoroutine()
        {
            var states = (TaskUIData.State[])Enum.GetValues(typeof(TaskUIData.State));
            foreach (var state in states)
            {
                UnitTest("Test Task", $"Testing {state}", state, false);
                yield return new WaitForSeconds(1f);
            }
        }
#endif
        #endregion
    }

    /// <summary>
    /// Data structure for task information
    /// </summary>
    [Serializable]
    public class TaskUIData
    {
        [field: SerializeField]
        public string MainText { get; set; } = string.Empty;

        [field: SerializeField]
        public string SubText { get; set; } = string.Empty;

        [field: SerializeField]
        public State CurrentState { get; set; } = State.NotStarted;

        public bool IsLast { get; set; } = false;

        public enum State
        {
            NotStarted,
            InProgress,
            Completed,
            Failed
        }

        /// <summary>
        /// Creates a new TaskUIData instance
        /// </summary>
        public TaskUIData() { }

        /// <summary>
        /// Creates a new TaskUIData instance with specified values
        /// </summary>
        public TaskUIData(string mainText, string subText, State state = State.NotStarted, bool isLast = false)
        {
            MainText = mainText;
            SubText = subText;
            CurrentState = state;
            IsLast = isLast;
        }
    }
}