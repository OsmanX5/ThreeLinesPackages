using DG.Tweening;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace TLUIToolkit
{
    [ExecuteInEditMode]
    public class PrespectiveLayoutElement : MonoBehaviour
    {
        [ShowInInspector]
        float totalWidth=>transform.parent.GetComponent<RectTransform>().rect.size.x;

        [ShowInInspector]
        float currentWidth => GetComponent<RectTransform>().rect.size.x;
        [ShowInInspector]
        float currentPosition => transform.localPosition.x;
        float currentABSPosition => Mathf.Abs(currentPosition);
        [ShowInInspector]
        float maxPosition => (totalWidth / 2f);
        float minPosition => -maxPosition;

        Image image => GetComponent<Image>();
        public float MinScale { get; set; } = 0.65f;
        float maxScale => 1f;
        [ShowInInspector]
        float targetScale =>  Mathf.Lerp(MinScale, maxScale,1 -  currentABSPosition/maxPosition);
        void Update()
        {
            Refresh();
        }
        private void OnRectTransformDimensionsChange()
        {
            // Recalculate the total width when the RectTransform dimensions change
            if (transform.parent != null)
            {
                Refresh();
            }
        }

        private void Refresh()
        {
           transform.localScale = new Vector3(targetScale, targetScale, 1f);
        }
        internal void MoveTo(float targetPosition, float time)
        {
            if (targetPosition > maxPosition)
                targetPosition = maxPosition;
            if (targetPosition < minPosition)
                targetPosition = minPosition;
            transform.DOLocalMoveX(targetPosition, time);
        }

        internal void FadeIn(float t)
        {
            image.DOFade(1, t);
        }
        internal void FadeOut(float t,float endValue =0.5f)
        {
            image.DOFade(endValue, t);
        }
        [Button]
        internal void Move(float deltaMove,float time)
        {
            MoveTo(transform.localPosition.x + deltaMove, time);
        }
    }

}
