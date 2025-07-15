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
    public class TLUIButtonFeedback : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [Title("Feedback Settings")]
        [SerializeField]
        [EnumToggleButtons]
        TLUIEffectEventType feedbackEvents = TLUIEffectEventType.OnClick | TLUIEffectEventType.OnHover;




        [SerializeField]
        TLUIEffectFeedbackType feedbackTypes = TLUIEffectFeedbackType.Audio;

        [Title("Custom Audio")]
        [ToggleLeft]
        [SerializeField] private bool useCustomSounds = false;
        [ShowIf("useCustomSounds"), Indent]
        [SerializeField] private AudioClip customHoverSound;
        [ShowIf("useCustomSounds"), Indent]
        [SerializeField] private AudioClip customClickSound;

        private Button button;

        protected void Awake()
        {
            button = GetComponent<Button>();
            button.onClick.AddListener(HandleClick);
        }

        TLUISounds TLUISounds => TLUISounds.Instance;

        private void OnDestroy()
        {
            if (button != null)
            {
                button.onClick.RemoveListener(HandleClick);
            }
        }

        public void OnPointerEnter(PointerEventData eventData) => HandleHover();
        public void OnPointerExit(PointerEventData eventData) => HandleHoverExit();

        private void HandleClick()
        {
            if (!IsEventEnabled(TLUIEffectEventType.OnClick))
                return;

            if (IsFeedbackTypeEnabled(TLUIEffectFeedbackType.Audio))
            {
                if (useCustomSounds && customClickSound != null)
                    TLUISounds.PlayCustomSound(customClickSound);
                else
                    TLUISounds.PlayClickSound();
            }

        }

        private void HandleHover()
        {
            if (!IsEventEnabled(TLUIEffectEventType.OnHover))
                return;
            
            if (IsFeedbackTypeEnabled(TLUIEffectFeedbackType.Audio))
            {
                if (useCustomSounds && customHoverSound != null)
                    TLUISounds.PlayCustomSound(customHoverSound);
                else
                    TLUISounds.PlayHoverSound();
            }
            if (IsFeedbackTypeEnabled(TLUIEffectFeedbackType.XRVibration))
            {
                XRHandsVibrator.Instance.VibrateRightHandCommon(XRHandsVibrator.PredfinedVibrations.verySoft);
            }
        }

        private void HandleHoverExit()
        {
            if (!IsEventEnabled(TLUIEffectEventType.OnHoverExit))
                return;

            // Add hover exit feedback here if needed
            // TLUISounds.PlayHoverExitSound();
        }

        protected bool IsEventEnabled(TLUIEffectEventType eventType)
        {
            return (feedbackEvents & eventType) != TLUIEffectEventType.None;
        }
        protected bool IsFeedbackTypeEnabled(TLUIEffectFeedbackType feedbackType)
        {
            return (feedbackTypes & feedbackType) != TLUIEffectFeedbackType.None;
        }

        [Title("Testing")]
        [Button] private void TestHover() => HandleHover();
        [Button] private void TestClick() => HandleClick();
        [Button] private void TestHoverExit() => HandleHoverExit();

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
#endif
    }
}