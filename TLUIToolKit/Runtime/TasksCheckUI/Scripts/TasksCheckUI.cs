using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TLUIToolkit
{
    public class TasksCheckUI : MonoBehaviour
    {

        [SerializeField]
        Transform tasksListParent;

        [SerializeField]
        [AssetsOnly]
        GameObject taskUIElementPrefab;

        [SerializeField]
        List<TaskUIElement.Data> taskUIElements = new ();


        [Button]
        void CreateTasksUI()
        {
            DestroyAllChildrens();
            for (int i = 0; i < taskUIElements.Count; i++)
            {
                TaskUIElement.Data taskData = taskUIElements[i];
                var taskUIElement = Instantiate(taskUIElementPrefab, tasksListParent);
                var taskUI = taskUIElement.GetComponent<TaskUIElement>();
                taskData.IsLast = i == taskUIElements.Count - 1; // Set last element
                if (taskUI != null)
                {
                    taskUI.SetData(taskData);
                }
            }
        }
        public void InitWithTasks(List<TaskUIElement.Data> tasksData)
        {
            ClearTasks();
            taskUIElements = tasksData;
            CreateTasksUI();
        }
        public void ClearTasks()
        {
            taskUIElements.Clear();
            DestroyAllChildrens();
        }
        public void AddTask(TaskUIElement.Data taskData)
        {
            taskData.IsLast = taskUIElements.Count == 0; // Set last element if it's the first task
            taskUIElements.Add(taskData);
            var taskUIElement = Instantiate(taskUIElementPrefab, tasksListParent);
            var taskUI = taskUIElement.GetComponent<TaskUIElement>();
            if (taskUI != null)
            {
                taskUI.SetData(taskData);
            }
        }
        [Button]
        public void UpdateTaskState(int index, TaskUIElement.Data.State newState)
        {
            if (index < 0 || index >= taskUIElements.Count)
            {
                Debug.LogError("Index out of range");
                return;
            }
            taskUIElements[index].CurrentState = newState;
            Transform child = tasksListParent.GetChild(index);
            TaskUIElement taskUI = child.GetComponent<TaskUIElement>();
            if (taskUI != null)
            {
                taskUI.SetData(taskUIElements[index]);
            }
        }

        public void DestroyAllChildrens()
        {
            int childCount = tasksListParent.childCount;
            if (Application.isPlaying)
            {
                List<Transform> childs = new();
                for (int i = 0; i < childCount; i++) 
                    childs.Add(tasksListParent.GetChild(i));
                foreach (Transform child in childs)
                    GameObject.Destroy(child.gameObject);
                childs.Clear();
            }
            else
            {
                for (int i = 0; i < childCount; i++)
                {

                    GameObject.DestroyImmediate(tasksListParent.GetChild(0).gameObject);
                }
            }
        }

    }
}

