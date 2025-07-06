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
    public class ProgressUI : MonoBehaviour
    {
        #region Events
        public event Action<int, ProgressUIElement.UIData.State> OnProgressStateChanged;
        public event Action<int,ProgressUIElement> OnProgressElementAdded;
        public event Action OnProgressCleared;
        public event Action<List<ProgressUIElement>> OnProgressElementsInitlized;

        #endregion

        
        #region Serialized Fields
        
        [SerializeField]
        [Tooltip("Parent transform for all progress UI elements. This will be used to instantiate task UI elements as children.")]
        private Transform progressUIElementsParent;
        
        [SerializeField, AssetsOnly]
        [Tooltip("Prefab for the progress UI element. This should have a ProgressUIElement component attached.")]
        private GameObject progressUIElementPrefab;
        
        [SerializeField] 
        private List<ProgressUIElement.UIData> progressUIElementsData = new();

        List<ProgressUIElement> progressUIElementsObjects = new();

        public List<ProgressUIElement> ProgressUIElementsGameObjects => progressUIElementsObjects;
        #endregion
        private void Start()
        {
            CreateProgressUI();
        }
        #region Public Methods
        public List<ProgressUIElement> InitWithUIData(List<ProgressUIElement.UIData> uiDataList)
        {
            ClearProgressUI();
            progressUIElementsData = uiDataList;
            CreateProgressUI();
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

        public void AddUIDataElement(ProgressUIElement.UIData uiElementData)
        {
            // Update previous last task
            if (progressUIElementsData.Count > 0)
            {
                progressUIElementsData[progressUIElementsData.Count - 1].IsLast = false;
            }

            uiElementData.IsLast = true; // New task is now the last
            progressUIElementsData.Add(uiElementData);

            var taskUIElement = Instantiate(progressUIElementPrefab, progressUIElementsParent);
            var taskUI = taskUIElement.GetComponent<ProgressUIElement>();
            if (taskUI != null)
            {
                taskUI.SetData(uiElementData);
            }

            // Refresh previous task to remove last line
            if (progressUIElementsData.Count > 1)
            {
                RefreshProgressUI(progressUIElementsData.Count - 2);
            }

            OnProgressElementAdded?.Invoke(progressUIElementsData.Count - 1, taskUI);
        }

        [Button]
        public void UpdateTaskState(int index, ProgressUIElement.UIData.State newState)
        {
            if (index < 0 || index >= progressUIElementsData.Count)
            {
                Debug.LogError("Index out of range");
                return;
            }

            var oldState = progressUIElementsData[index].CurrentState;
            progressUIElementsData[index].CurrentState = newState;

            Transform child = progressUIElementsParent.GetChild(index);
            ProgressUIElement taskUI = child.GetComponent<ProgressUIElement>();
            if (taskUI != null)
            {
                taskUI.SetData(progressUIElementsData[index]);
            }

            OnProgressStateChanged?.Invoke(index, newState);
        }
        #endregion

        #region Private Methods
        [Button]
        private void CreateProgressUI()
        {
            DestroyAllChildren();
            progressUIElementsObjects = new List<ProgressUIElement>();
            for (int i = 0; i < progressUIElementsData.Count; i++)
            {
                ProgressUIElement.UIData taskData = progressUIElementsData[i];
                taskData.CurrentState = ProgressUIElement.UIData.State.NotStarted; // Reset state for new UI
                var taskUIElement = Instantiate(progressUIElementPrefab, progressUIElementsParent);
                var taskUI = taskUIElement.GetComponent<ProgressUIElement>();
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
            ProgressUIElement taskUI = child.GetComponent<ProgressUIElement>();
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
        public void AddProgressUIElement()
        {
            var testTask = new ProgressUIElement.UIData(
                $"Test Task {progressUIElementsData.Count + 1}",
                "Test description",
                ProgressUIElement.UIData.State.NotStarted
            );
            AddUIDataElement(testTask);
        }

        [Button("Test State Updates")]
        public void TestStateUpdates()
        {
            if (progressUIElementsData.Count > 0)
            {
                StartCoroutine(TestStatesCoroutine());
            }
        }

        private IEnumerator TestStatesCoroutine()
        {
            var states = new[] {
                ProgressUIElement.UIData.State.InProgress,
                ProgressUIElement.UIData.State.Completed,
                ProgressUIElement.UIData.State.Failed,
                ProgressUIElement.UIData.State.NotStarted
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