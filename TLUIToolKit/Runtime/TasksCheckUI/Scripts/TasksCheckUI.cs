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
    public class TasksCheckUI : MonoBehaviour
    {
        #region Events
        public event Action<int, TaskUIData.State> OnTaskStateChanged;
        public event Action<int,TaskUIElement> OnTaskAdded;
        public event Action OnTasksCleared;
        public event Action<List<TaskUIElement>> OnTasksInitialized;

        #endregion

        
        #region Serialized Fields
        
        [SerializeField] 
        private Transform tasksListParent;
        
        [SerializeField, AssetsOnly] 
        private GameObject taskUIElementPrefab;
        
        [SerializeField] 
        private List<TaskUIData> taskUIElements = new();

        List<TaskUIElement> taskUIElementsObjects = new();

        public List<TaskUIElement> TaskUIElementsObjects => taskUIElementsObjects;
        #endregion
        private void Start()
        {
            CreateTasksUI();
        }
        #region Public Methods
        public List<TaskUIElement> InitWithTasks(List<TaskUIData> tasksData)
        {
            ClearTasks();
            taskUIElements = tasksData;
            CreateTasksUI();
            OnTasksInitialized?.Invoke(taskUIElementsObjects);
            return taskUIElementsObjects;
        }

        public void ClearTasks()
        {
            taskUIElements.Clear();
            taskUIElementsObjects.Clear();
            DestroyAllChildren();
            OnTasksCleared?.Invoke();
        }

        public void AddTask(TaskUIData taskData)
        {
            // Update previous last task
            if (taskUIElements.Count > 0)
            {
                taskUIElements[taskUIElements.Count - 1].IsLast = false;
            }

            taskData.IsLast = true; // New task is now the last
            taskUIElements.Add(taskData);

            var taskUIElement = Instantiate(taskUIElementPrefab, tasksListParent);
            var taskUI = taskUIElement.GetComponent<TaskUIElement>();
            if (taskUI != null)
            {
                taskUI.SetData(taskData);
            }

            // Refresh previous task to remove last line
            if (taskUIElements.Count > 1)
            {
                RefreshTaskUI(taskUIElements.Count - 2);
            }

            OnTaskAdded?.Invoke(taskUIElements.Count - 1, taskUI);
        }

        [Button]
        public void UpdateTaskState(int index, TaskUIData.State newState)
        {
            if (index < 0 || index >= taskUIElements.Count)
            {
                Debug.LogError("Index out of range");
                return;
            }

            var oldState = taskUIElements[index].CurrentState;
            taskUIElements[index].CurrentState = newState;

            Transform child = tasksListParent.GetChild(index);
            TaskUIElement taskUI = child.GetComponent<TaskUIElement>();
            if (taskUI != null)
            {
                taskUI.SetData(taskUIElements[index]);
            }

            OnTaskStateChanged?.Invoke(index, newState);
        }
        #endregion

        #region Private Methods
        [Button]
        private void CreateTasksUI()
        {
            DestroyAllChildren();
            taskUIElementsObjects = new List<TaskUIElement>();
            for (int i = 0; i < taskUIElements.Count; i++)
            {
                TaskUIData taskData = taskUIElements[i];
                taskData.CurrentState = TaskUIData.State.NotStarted; // Reset state for new UI
                var taskUIElement = Instantiate(taskUIElementPrefab, tasksListParent);
                var taskUI = taskUIElement.GetComponent<TaskUIElement>();
                taskData.IsLast = i == taskUIElements.Count - 1; // Set last element

                if (taskUI != null)
                {
                    taskUI.SetData(taskData);
                }
                taskUIElementsObjects.Add(taskUI);
            }
        }

        private void RefreshTaskUI(int index)
        {
            if (index < 0 || index >= taskUIElements.Count || index >= tasksListParent.childCount)
                return;

            Transform child = tasksListParent.GetChild(index);
            TaskUIElement taskUI = child.GetComponent<TaskUIElement>();
            if (taskUI != null)
            {
                taskUI.SetData(taskUIElements[index]);
            }
        }

        private void DestroyAllChildren()
        {
            int childCount = tasksListParent.childCount;

            if (Application.isPlaying)
            {
                List<Transform> children = new();
                for (int i = 0; i < childCount; i++)
                    children.Add(tasksListParent.GetChild(i));

                foreach (Transform child in children)
                    GameObject.Destroy(child.gameObject);

                children.Clear();
            }
            else
            {
                for (int i = 0; i < childCount; i++)
                {
                    GameObject.DestroyImmediate(tasksListParent.GetChild(0).gameObject);
                }
            }
        }
        #endregion

        #region Editor Tools
#if UNITY_EDITOR
        [Button("Add Test Task")]
        public void AddTestTask()
        {
            var testTask = new TaskUIData(
                $"Test Task {taskUIElements.Count + 1}",
                "Test description",
                TaskUIData.State.NotStarted
            );
            AddTask(testTask);
        }

        [Button("Test State Updates")]
        public void TestStateUpdates()
        {
            if (taskUIElements.Count > 0)
            {
                StartCoroutine(TestStatesCoroutine());
            }
        }

        private IEnumerator TestStatesCoroutine()
        {
            var states = new[] {
                TaskUIData.State.InProgress,
                TaskUIData.State.Completed,
                TaskUIData.State.Failed,
                TaskUIData.State.NotStarted
            };

            for (int i = 0; i < taskUIElements.Count; i++)
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