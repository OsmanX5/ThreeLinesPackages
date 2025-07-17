using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TLUIToolkit
{
    /// <summary>
    /// Simple UI controller for managing multiple task UI elements
    /// </summary>
    public class TLUIProgressUI : MonoBehaviour
    {
        #region Events
        public event Action<int, TLUIProgressUIElement.UIData.State> OnProgressStateChanged;
        public event Action<int,TLUIProgressUIElement> OnProgressElementAdded;
        public event Action OnProgressCleared;
        public event Action<List<TLUIProgressUIElement>> OnProgressElementsInitlized;

        #endregion

        #region Serialized Fields
        
        [SerializeField]
        [Tooltip("Parent transform for all progress UI elements. This will be used to instantiate task UI elements as children.")]
        private Transform progressUIElementsParent;
        
        [SerializeField, AssetsOnly]
        [Tooltip("Prefab for the progress UI element. This should have a TLUIProgressUIElement component attached.")]
        private GameObject progressUIElementPrefab;
        
        [SerializeField] 
        private List<TLUIProgressUIElement.UIData> progressUIElementsData = new();

        List<TLUIProgressUIElement> progressUIElementsObjects = new();

        public List<TLUIProgressUIElement> ProgressUIElementsGameObjects => progressUIElementsObjects;
        #endregion
        
        private void Start()
        {
            InitWithLocalUIElements();
        }
        [Button(ButtonSizes.Large)]
        public void InitWithLocalUIElements()
        {
            List<TLUIProgressUIElement.UIData> localUIData = new(progressUIElementsData);
            InitWithUIData(localUIData,resetToNotStarted:false);
        }

        #region Public Methods
        public List<TLUIProgressUIElement> InitWithUIData(List<TLUIProgressUIElement.UIData> uiDataList,bool resetToNotStarted = false)
        {
            if(uiDataList == null || uiDataList.Count == 0)
            {
                Debug.LogWarning("No UI data provided. Progress UI will be empty.");
                return progressUIElementsObjects;
            }
            ClearProgressUI();
            progressUIElementsData = uiDataList;

            CreateProgressUI(resetToNotStarted);
            
            OnProgressElementsInitlized?.Invoke(progressUIElementsObjects);
            return progressUIElementsObjects;
        }

        public void ClearProgressUI()
        {
            progressUIElementsData.Clear();
            progressUIElementsObjects.Clear();
            DestroyAllChildren();
            OnProgressCleared?.Invoke();
        }

        public TLUIProgressUIElement AddUIDataElement(TLUIProgressUIElement.UIData uiElementData)
        {
            // Update previous last task
            if (progressUIElementsData.Count > 0)
            {
                progressUIElementsData[progressUIElementsData.Count - 1].IsLast = false;
            }

            uiElementData.IsLast = true; // New task is now the last
            progressUIElementsData.Add(uiElementData);

            var taskUIElement = Instantiate(progressUIElementPrefab, progressUIElementsParent);
            var progressUIElement = taskUIElement.GetComponent<TLUIProgressUIElement>();
            if (progressUIElement != null)
            {
                progressUIElement.SetData(uiElementData);
            }

            // Refresh previous task to remove last line
            if (progressUIElementsData.Count > 1)
            {
                RefreshProgressUI(progressUIElementsData.Count - 2);
            }

            OnProgressElementAdded?.Invoke(progressUIElementsData.Count - 1, progressUIElement);
            return progressUIElement;
        }

        [Button]
        public void UpdateTaskState(int index, TLUIProgressUIElement.UIData.State newState)
        {
            if (index < 0 || index >= progressUIElementsData.Count)
            {
                Debug.LogError("Index out of range");
                return;
            }

            var oldState = progressUIElementsData[index].CurrentState;
            progressUIElementsData[index].CurrentState = newState;

            Transform child = progressUIElementsParent.GetChild(index);
            TLUIProgressUIElement taskUI = child.GetComponent<TLUIProgressUIElement>();
            if (taskUI != null)
            {
                taskUI.SetData(progressUIElementsData[index]);
            }

            OnProgressStateChanged?.Invoke(index, newState);
        }
        #endregion

        #region Private Methods
        [Button]
        private void CreateProgressUI(bool resetToNotStarted)
        {
            DestroyAllChildren();
            progressUIElementsObjects = new List<TLUIProgressUIElement>();
            for (int i = 0; i < progressUIElementsData.Count; i++)
            {
                TLUIProgressUIElement.UIData taskData = progressUIElementsData[i];
                if(resetToNotStarted)
                    taskData.CurrentState = TLUIProgressUIElement.UIData.State.NotStarted; // Reset state for new UI
                var taskUIElement = Instantiate(progressUIElementPrefab, progressUIElementsParent);
                var taskUI = taskUIElement.GetComponent<TLUIProgressUIElement>();
                taskData.IsLast = i == progressUIElementsData.Count - 1; // Set last element

                if (taskUI != null)
                {
                    taskUI.SetData(taskData);
                }
                progressUIElementsObjects.Add(taskUI);
            }
        }

        private void RefreshProgressUI(int index)
        {
            if (index < 0 || index >= progressUIElementsData.Count || index >= progressUIElementsParent.childCount)
                return;

            Transform child = progressUIElementsParent.GetChild(index);
            TLUIProgressUIElement taskUI = child.GetComponent<TLUIProgressUIElement>();
            if (taskUI != null)
            {
                taskUI.SetData(progressUIElementsData[index]);
            }
        }

        private void DestroyAllChildren()
        {
            int childCount = progressUIElementsParent.childCount;

            if (Application.isPlaying)
            {
                List<Transform> children = new();
                for (int i = 0; i < childCount; i++)
                    children.Add(progressUIElementsParent.GetChild(i));

                foreach (Transform child in children)
                    GameObject.Destroy(child.gameObject);

                children.Clear();
            }
            else
            {
                for (int i = 0; i < childCount; i++)
                {
                    GameObject.DestroyImmediate(progressUIElementsParent.GetChild(0).gameObject);
                }
            }
        }
        #endregion

        #region Editor Tools
#if UNITY_EDITOR
        [Button("Add Test Task")]
        void AddProgressUIElement()
        {
            var testTask = new TLUIProgressUIElement.UIData(
                $"Test Task {progressUIElementsData.Count + 1}",
                "Test description",
                TLUIProgressUIElement.UIData.State.NotStarted
            );
            AddUIDataElement(testTask);
        }

        [Button("Test State Updates")]
        void TestStateUpdates()
        {
            if (progressUIElementsData.Count > 0)
            {
                StartCoroutine(TestStatesCoroutine());
            }
        }

        private IEnumerator TestStatesCoroutine()
        {
            var states = new[] {
                TLUIProgressUIElement.UIData.State.InProgress,
                TLUIProgressUIElement.UIData.State.Completed,
                TLUIProgressUIElement.UIData.State.Failed,
                TLUIProgressUIElement.UIData.State.NotStarted
            };

            for (int i = 0; i < progressUIElementsData.Count; i++)
            {
                foreach (var state in states)
                {
                    UpdateTaskState(i, state);
                    yield return new WaitForSeconds(0.8f);
                }
            }
        }
#endif
        #endregion
    }
}