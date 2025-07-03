using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


namespace TLUIToolkit
{
    public class TaskUIElement : MonoBehaviour
    {
        [SerializeField]
        TMP_Text mainText;

        [SerializeField]
        TMP_Text subText;

        [SerializeField]
        GameObject notStartedView,
            failerView,
            inProgressView,
            completedView;


        [SerializeField]
        GameObject lastLine;

        [SerializeField]
        Image lastLineFill;

        [SerializeField]
        Color SuccessColor;
        [SerializeField]
        Color FailureColor;
        [SerializeField]
        Color InProgressColor;
        [SerializeField]
        Color notStartedColor = Color.white;

        Data.State lastState;
        bool isFilling = false;
        Coroutine fillCoroutine;
        const float FILL_DURATION = 0.5f;
        public void SetData(Data data)
        {
            mainText.text = data.MainText;
            subText.text = data.SubText;
            UpdateState(data.CurrentState);
            lastLine.gameObject.SetActive(!data.IsLast);
        }

        private void UpdateState(Data.State newState)
        {
            Debug.Log($"Updating {mainText.text} to {newState}");
            notStartedView.SetActive(newState == Data.State.NotStarted);
            inProgressView.SetActive(newState == Data.State.InProgress);
            completedView.SetActive(newState == Data.State.Completed);
            failerView.SetActive(newState == Data.State.Failed);
            mainText.color = newState switch
            {
                Data.State.Completed => SuccessColor,
                Data.State.InProgress => InProgressColor,
                Data.State.Failed => FailureColor,
                _ => notStartedColor
            };
            lastLineFill.fillAmount = 0;
            if (newState != lastState)
            {
                FillAnimation(newState);
                lastState = newState;
            }
        }

        private void FillAnimation(Data.State currentState)
        {
            Debug.Log($"Filling animation for {mainText.text} with state {currentState}");
            if (lastLineFill == null)
            {
                Debug.LogWarning("Last Line Fill is not assigned in TaskUIElement");
                return;
            }
            lastLineFill.color = currentState switch
            {
                Data.State.Completed => SuccessColor,
                Data.State.Failed => FailureColor,
                _ => notStartedColor
            };

            if (currentState != Data.State.NotStarted)
            {
                lastLineFill.fillAmount = 0;
                KillOldFillingProccess();
                fillCoroutine = StartCoroutine(fillProccess(FILL_DURATION, 1f));
            }
            else
            {
                lastLineFill.fillAmount = 1f;
                KillOldFillingProccess();
                fillCoroutine = StartCoroutine(fillProccess(FILL_DURATION, 0f));
            }
        }

        IEnumerator fillProccess(float duration, float targetFill)
        {
            Debug.Log($"Starting fill process for {mainText.text} with target fill {targetFill}");
            isFilling = true;
            float elapsedTime = 0f;
            float initialFill = lastLineFill.fillAmount;
            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                float fillAmount = Mathf.Lerp(initialFill, targetFill, elapsedTime / duration);
                lastLineFill.fillAmount = fillAmount;
                yield return null;
            }
            lastLineFill.fillAmount = targetFill;
            isFilling = false;
            Debug.Log($"Fill process completed for {mainText.text} with final fill {lastLineFill.fillAmount}");
        }
        private void OnDisable()
        {
            KillOldFillingProccess();
        }

        private void KillOldFillingProccess()
        {
            if (isFilling)
                StopCoroutine(fillCoroutine);
        }
#if UNITY_EDITOR
        [Button]
        public void UnitTest(string mainText, string subText, Data.State state, bool isLast)
        {
            this.mainText.text = mainText;
            this.subText.text = subText;
            SetData(new Data { MainText = mainText, SubText = subText, CurrentState = state, IsLast = isLast });
        }
#endif
        [Serializable]
        public class Data
        {
            [field: SerializeField]
            public string MainText { get; set; }
            [field: SerializeField]
            public string SubText { get; set; }
            [field: SerializeField]
            public State CurrentState { get; set; }
            public bool IsLast { get; set; } = false;
            public enum State
            {
                NotStarted,
                InProgress,
                Completed,
                Failed
            }
        }

    }

}

