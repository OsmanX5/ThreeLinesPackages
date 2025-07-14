using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TLUIToolkit
{
    [RequireComponent(typeof(Button))]
    public class TLUIButtonFeedback : MonoBehaviour, IPointerEnterHandler
    {
        [Title("Feedback Settings")]
        [SerializeField] protected bool enableHoverFeedback = true;
        [SerializeField] protected bool enableClickFeedback = true;



        [Title("Custom Audio")]
        [ToggleLeft]
        [SerializeField] private bool useCustomSounds = false;

        [ShowIf("useCustomSounds"), Indent]
        [SerializeField] private AudioClip customHoverSound;

        [ShowIf("useCustomSounds"), Indent]
        [SerializeField] private AudioClip customClickSound;


        private Button button;

        protected  void Awake()
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


        private void HandleClick()
        {
            if (!enableClickFeedback)
                return;
            if (useCustomSounds && customClickSound != null)
                TLUISounds.PlayCustomSound(customClickSound);
            else
                TLUISounds.PlayClickSound();
        }
        private void HandleHover()
        {
            if(!enableHoverFeedback)
                return;
            if (useCustomSounds && customHoverSound != null)
                TLUISounds.PlayCustomSound(customHoverSound);
            else
                TLUISounds.PlayHoverSound();
        }

        [Title("Testing")]
        [Button] private void TestHover() => HandleHover();
        [Button] private void TestClick() => HandleClick();

    }
}