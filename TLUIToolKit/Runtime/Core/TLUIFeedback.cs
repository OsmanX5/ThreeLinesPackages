using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;


namespace TLUIToolkit
{
    public  class TLUIFeedback : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler, IPointerExitHandler
    {
        [SerializeField]
        TLUIEffectEventType feedbackEvents;

        [SerializeField]
        TLUIEffectFeedbackType feedbackTypes;

        public virtual void OnEnable()
        {
            if (!IsEventEnabled(TLUIEffectEventType.OnShow))
                return;
        }
        public virtual void OnDisable()
        {
            if(!IsEventEnabled(TLUIEffectEventType.OnHide))
                return;
        }
        public virtual void OnPointerClick(PointerEventData eventData)
        {
            if(!IsEventEnabled(TLUIEffectEventType.OnClick))
                return;
        }

        public virtual void OnPointerEnter(PointerEventData eventData)
        {
            if(!IsEventEnabled(TLUIEffectEventType.OnHover))
                return;
        }

        public virtual void OnPointerExit(PointerEventData eventData)
        {
            if(!IsEventEnabled(TLUIEffectEventType.OnHoverExit))
                return;
        }

        protected bool IsEventEnabled(TLUIEffectEventType eventType)
        {
            return (feedbackEvents & eventType) != TLUIEffectEventType.None;
        }
        protected bool IsFeedbackTypeEnabled(TLUIEffectFeedbackType feedbackType)
        {
            return (feedbackTypes & feedbackType) != TLUIEffectFeedbackType.None;
        }
        
    }

}

