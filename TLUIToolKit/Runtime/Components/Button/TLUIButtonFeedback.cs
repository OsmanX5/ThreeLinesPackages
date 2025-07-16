using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using ThreeLines.Helpers;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TLUIToolkit
{
    [RequireComponent(typeof(Button))]
    public class TLUIButtonFeedback : MonoBehaviour
    {
        [SerializeField]
        [InfoBox("Only Supporting OnClick,OnHove,OnEnable,OnDisable")]
        TLUIFeedback feedback;


        private Button button;

        protected void Awake()
        {
            button = GetComponent<Button>();
            button.onClick.AddListener(HandleClick);
        }

        private void OnDestroy()
        {
            if (button != null)
            {
                button.onClick.RemoveListener(HandleClick);
            }
        }

        private void HandleClick()
        {
            feedback.OnClickFeedback();
        }

        private void HandleHover()
        {
            feedback.OnHoverFeedback();
        }
        private void OnEnable()
        {
            feedback.OnShowFeedback();
        }
        private void OnDisable()
        {
            feedback.OnDisableFeedback();
        }

        [Title("Testing")]
        [Button] private void TestHover() => HandleHover();
        [Button] private void TestClick() => HandleClick();

        private void OnValidate()
        {
            Debug.Log($"TLUIButtonFeedback OnValidate called for {gameObject.name}");
        }
#if UNITY_EDITOR
        // Static context menu function
        [MenuItem("CONTEXT/Button/Add TLUIButtonFeedback")]
        private static void AddButtonFeedback(MenuCommand command)
        {
            Button button = command.context as Button;
            if (button != null)
            {
                // Check if component already exists
                if (button.GetComponent<TLUIButtonFeedback>() == null)
                {
                    // Add the component
                    TLUIButtonFeedback feedback = button.gameObject.AddComponent<TLUIButtonFeedback>();

                    // Mark the object as dirty for undo system
                    UnityEditor.Undo.RegisterCreatedObjectUndo(feedback, "Add TLUIButtonFeedback");

                    // Mark the scene as dirty
                    UnityEditor.EditorUtility.SetDirty(button.gameObject);

                    Debug.Log($"TLUIButtonFeedback added to {button.name}");
                }
                else
                {
                    Debug.LogWarning($"TLUIButtonFeedback already exists on {button.name}");
                }
            }
        }

        // Alternative: Add to all selected buttons
        [MenuItem("CONTEXT/Button/Add TLUIButtonFeedback to All Selected")]
        private static void AddButtonFeedbackToSelected(MenuCommand command)
        {
            GameObject[] selectedObjects = UnityEditor.Selection.gameObjects;
            int addedCount = 0;

            foreach (GameObject obj in selectedObjects)
            {
                Button button = obj.GetComponent<Button>();
                if (button != null && button.GetComponent<TLUIButtonFeedback>() == null)
                {
                    TLUIButtonFeedback feedback = button.gameObject.AddComponent<TLUIButtonFeedback>();
                    UnityEditor.Undo.RegisterCreatedObjectUndo(feedback, "Add TLUIButtonFeedback");
                    UnityEditor.EditorUtility.SetDirty(button.gameObject);
                    addedCount++;
                }
            }

            Debug.Log($"TLUIButtonFeedback added to {addedCount} button(s)");
        }

        [Button]
        void SetButtonDefaultFeedbacks()
        {
            feedback.AddFeedback(new (TLUIEffectEventType.OnClick, TLUIEffectFeedbackType.Audio));
            feedback.AddFeedback(new(TLUIEffectEventType.OnHover, TLUIEffectFeedbackType.XRVibration));
        }
#endif
    }
}