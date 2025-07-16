using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using ThreeLines.Helpers;
using UnityEngine;

namespace TLUIToolkit
{
    [Serializable]
    public class TLUIFeedback
    {
        [Title("Active Feedbacks")]
        [Space(10)]

        [SerializeField]
        [TableList(AlwaysExpanded = true, HideToolbar = true)]
        [HideLabel]
        private List<TLUIFeedbackData> feedbacks;

        public List<TLUIFeedbackData> Feedbacks
        {
            get => feedbacks ??= new List<TLUIFeedbackData>();
            set => feedbacks = value;
        }

        public bool AddFeedback(TLUIFeedbackData feedbackData)
        {
            if (feedbacks == null)
                feedbacks = new List<TLUIFeedbackData>();

            if (feedbacks.Any(f => f.EventType == feedbackData.EventType && f.FeedbackType == feedbackData.FeedbackType))
                return false;

            feedbacks.Add(feedbackData);
            return true;
        }

        public void OnShowFeedback()
        {
            if (!IsFeedbackEventTypeAdded(TLUIEffectEventType.OnShow))
                return;

            ExecuteFeedback(TLUIEffectEventType.OnShow);
        }

        public void OnClickFeedback()
        {
            if (!IsFeedbackEventTypeAdded(TLUIEffectEventType.OnClick))
                return;

            ExecuteFeedback(TLUIEffectEventType.OnClick);
        }

        public void OnHoverFeedback()
        {
            if (!IsFeedbackEventTypeAdded(TLUIEffectEventType.OnHover))
                return;

            ExecuteFeedback(TLUIEffectEventType.OnHover);
        }

        public void OnHoverExitFeedback()
        {
            if (!IsFeedbackEventTypeAdded(TLUIEffectEventType.OnHoverExit))
                return;

            ExecuteFeedback(TLUIEffectEventType.OnHoverExit);
        }

        public void OnDisableFeedback()
        {
            if (!IsFeedbackEventTypeAdded(TLUIEffectEventType.OnHide))
                return;

            ExecuteFeedback(TLUIEffectEventType.OnHide);
        }

        private void ExecuteFeedback(TLUIEffectEventType eventType)
        {
            var eventFeedbacks = feedbacks?.Where(f => f.EventType == eventType) ?? Enumerable.Empty<TLUIFeedbackData>();

            foreach (var feedback in eventFeedbacks)
            {
                if (feedback.FeedbackType == TLUIEffectFeedbackType.Audio)
                    PlayAudioFeedback(eventType);
                else if (feedback.FeedbackType == TLUIEffectFeedbackType.XRVibration)
                    PlayHapticFeedback();
            }
        }

        private void PlayAudioFeedback(TLUIEffectEventType eventType)
        {
            switch (eventType)
            {
                case TLUIEffectEventType.OnClick:
                    TLUISounds.Instance?.PlayClickSound();
                    break;
                case TLUIEffectEventType.OnHover:
                    TLUISounds.Instance?.PlayHoverSound();
                    break;
                    // TODO: Add other audio feedback types
            }
        }

        private void PlayHapticFeedback()
        {
            XRHandsVibrator.Instance?.VibrateRightHandCommon(XRHandsVibrator.PredfinedVibrations.verySoft);
        }

        public bool IsFeedbackEventTypeAdded(TLUIEffectEventType eventType)
        {
            return feedbacks?.Any(f => f.EventType == eventType) ?? false;
        }

        public bool IsFeedbackTypeEnabled(TLUIEffectFeedbackType feedbackType)
        {
            return feedbacks?.Any(f => f.FeedbackType == feedbackType) ?? false;
        }

#if UNITY_EDITOR
        [TitleGroup("Add Feedback")]
        [HorizontalGroup("Add Feedback/Options")]
        [HideLabel]
        [SerializeField]
        private TLUIEffectEventType toAddEventType;

        [TitleGroup("Add Feedback")]
        [HorizontalGroup("Add Feedback/Options")]
        [HideLabel]
        [SerializeField]
        private TLUIEffectFeedbackType toAddFeedbackType;

        [TitleGroup("Add Feedback")]
        [Button(ButtonSizes.Large)]
        [GUIColor(0.4f, 0.8f, 0.4f)]
        private void AddFeedbackEditor()
        {
            if (toAddEventType == TLUIEffectEventType.None || toAddFeedbackType == TLUIEffectFeedbackType.None)
            {
                Debug.LogWarning("Please select both Event Type and Feedback Type.");
                return;
            }

            var feedbackData = new TLUIFeedbackData(toAddEventType, toAddFeedbackType);
            if (AddFeedback(feedbackData))
            {
                Debug.Log($"Added feedback: {feedbackData.EventType} - {feedbackData.FeedbackType}");
            }
            else
            {
                Debug.LogWarning("Feedback already exists.");
            }
        }

        [TitleGroup("Add Feedback")]
        [Button("Clear All", ButtonSizes.Medium)]
        [GUIColor(0.8f, 0.4f, 0.4f)]
        private void ClearAllFeedbacks()
        {
            feedbacks?.Clear();
            Debug.Log("All feedbacks cleared.");
        }

#endif
    }

    [Serializable]
    public struct TLUIFeedbackData
    {
        [TableColumnWidth(150)]
        public TLUIEffectEventType EventType;

        [TableColumnWidth(150)]
        public TLUIEffectFeedbackType FeedbackType;

        public TLUIFeedbackData(TLUIEffectEventType eventType, TLUIEffectFeedbackType feedbackType)
        {
            EventType = eventType;
            FeedbackType = feedbackType;
        }
    }
}