using Sirenix.OdinInspector;
using Sirenix.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace TLUIToolkit
{
    public class PrespectiveLayout : MonoBehaviour
    {
        #region Configuration
        [FoldoutGroup("Settings")]
        [SerializeField] private AnimationSettings animationSettings = new AnimationSettings();

        [FoldoutGroup("Settings")]
        [SerializeField] private bool initializeOnAwake = true;
        #endregion

        #region Runtime Data
        [FoldoutGroup("Runtime Info", false)]
        [ShowInInspector, ReadOnly]
        private List<PrespectiveLayoutElement> items = new List<PrespectiveLayoutElement>();

        [FoldoutGroup("Runtime Info")]
        [ShowInInspector, ReadOnly]
        private List<Transform> originalOrder;

        [FoldoutGroup("Runtime Info")]
        [ShowInInspector, ReadOnly]
        private bool isAnimating;

        [FoldoutGroup("Runtime Info")]
        [ShowInInspector, ReadOnly]
        private bool isInitialized;
        #endregion

        #region Calculated Properties
        [FoldoutGroup("Debug Info", false)]
        [ShowInInspector, ReadOnly]
        private float TotalWidth => GetComponent<RectTransform>().sizeDelta.x;

        [FoldoutGroup("Debug Info")]
        [ShowInInspector, ReadOnly]
        private float DeltaPosition => transform.childCount > 0 ? TotalWidth / transform.childCount : 0f;

        [FoldoutGroup("Debug Info")]
        [ShowInInspector, ReadOnly]
        private int ItemCount => items?.Count ?? 0;

        [FoldoutGroup("Debug Info")]
        [ShowInInspector, ReadOnly]
        private int MaxLevel => ItemCount > 0 ? GetLevel((ItemCount - 1) / 2) : 0;

        [FoldoutGroup("Debug Info")]
        [ShowInInspector, ReadOnly]
        private float MaxPosition => MaxLevel * DeltaPosition;

        [FoldoutGroup("Debug Info")]
        [ShowInInspector, ReadOnly]
        private float MinPosition => -MaxPosition;

        [FoldoutGroup("Debug Info")]
        [ShowInInspector, ReadOnly]
        public GameObject CenterObject => transform.childCount > 0 ? transform.GetChild(transform.childCount - 1).gameObject : null;
        #endregion

        #region Events
        public event Action<float> OnAnimationStart;
        public event Action<GameObject> OnAnimationEnd;
        #endregion

        #region Public Properties
        public List<Transform> OriginalOrder => originalOrder;
        public bool IsAnimating => isAnimating;
        public bool IsInitialized => isInitialized;
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            if (initializeOnAwake && transform.childCount > 0)
            {
                Initialize();
            }
        }

        private void Update()
        {
            if (isAnimating)
                ReorderLayers();
        }
        #endregion

        #region Public Methods
        [FoldoutGroup("Controls")]
        [Button(ButtonSizes.Large), GUIColor(0.4f, 0.8f, 1f)]
        public void Initialize() => InitializeWithDuration(animationSettings.duration);

        public void InitializeWithDuration(float duration)
        {
            if (!ValidateInitialization()) 
                return;

            animationSettings.duration = duration;
            CacheOriginalOrder();
            SetupLayoutElements();
            BuildItemsList();
            PositionElements();
            ReorderLayers();
            isInitialized = true;
        }

        [FoldoutGroup("Controls")]
        [Button(ButtonSizes.Large), GUIColor(1f, 0.4f, 0.4f)]
        public void Dispose()
        {
            if (!ValidateDispose()) return;

            CleanupLayoutElements();
            ResetState();
            Debug.Log("PerspectiveLayout disposed successfully.");
        }

        [FoldoutGroup("Controls")]
        [ShowIf("@IsPlayingAndNotAnimating()")]
        [HorizontalGroup("Controls/Movement", 0.5f)]
        [Button(ButtonSizes.Large), GUIColor(0.8f, 1f, 0.4f)]
        public void MoveLeft() => MoveAsync(AnimationDirection.Left);

        [FoldoutGroup("Controls")]
        [ShowIf("@IsPlayingAndNotAnimating()")]
        [HorizontalGroup("Controls/Movement", 0.5f)]
        [Button(ButtonSizes.Large), GUIColor(0.8f, 1f, 0.4f)]
        public void MoveRight() => MoveAsync(AnimationDirection.Right);
        #endregion

        #region Private Methods - Initialization
        private bool ValidateInitialization()
        {
            if (transform.childCount == 0)
            {
                Debug.LogWarning("No child elements found. Add child objects before initializing.");
                return false;
            }

            if (isInitialized)
            {
                Debug.LogWarning("Already initialized. Dispose first if you need to reinitialize.");
                return false;
            }

            return true;
        }

        private void CacheOriginalOrder()
        {
            originalOrder = transform.GetComponentsInChildren<Transform>()
                .Where(t => t != transform)
                .ToList();
        }

        private void SetupLayoutElements()
        {
            foreach (Transform child in transform)
            {
                if (child.GetComponent<PrespectiveLayoutElement>() == null)
                {
                    child.gameObject.AddComponent<PrespectiveLayoutElement>();
                }
            }
        }

        private void BuildItemsList()
        {
            items.Clear();

            foreach (Transform child in transform)
            {
                var element = child.GetComponent<PrespectiveLayoutElement>();
                if (element == null)
                {
                    Debug.LogWarning($"Child {child.name} missing PrespectiveLayoutElement component.");
                    continue;
                }

                element.MinScale = animationSettings.minScale;
                items.Add(element);
            }
        }

        private void PositionElements()
        {
            for (int i = 0; i < items.Count; i++)
            {
                var position = CalculatePosition(i);
                items[i].transform.localPosition = new Vector3(position, 0, 0);
            }
        }

        private float CalculatePosition(int index)
        {
            if (index == 0) return 0f;

            var level = GetLevel(index);
            var direction = index <= ItemCount / 2 ? -1 : 1;
            return direction * DeltaPosition * level;
        }

        private int GetLevel(int index) => Math.Min(index, ItemCount - index);
        #endregion

        #region Private Methods - Animation
        private async void MoveAsync(AnimationDirection direction)
        {
            if (isAnimating) return;

            isAnimating = true;
            OnAnimationStart?.Invoke(animationSettings.duration);

            var sortedItems = items.OrderBy(x => x.transform.localPosition.x).ToList();

            await Task.WhenAll(
                MoveItemsInDirection(sortedItems, direction),
                CycleEndItem(sortedItems, direction)
            );

            FadeInAllItems(sortedItems);
            await Task.Delay((int)(animationSettings.duration * 500)); // Half duration

            UpdateItemsList();
            await Task.Delay(200); // Small buffer

            isAnimating = false;
            OnAnimationEnd?.Invoke(CenterObject);
        }

        private Task MoveItemsInDirection(List<PrespectiveLayoutElement> sortedItems, AnimationDirection direction)
        {
            var moveDistance = DeltaPosition * (int)direction;
            var startIndex = direction == AnimationDirection.Right ? 0 : 1;
            var endIndex = direction == AnimationDirection.Right ? ItemCount - 1 : ItemCount;

            for (int i = startIndex; i < endIndex; i++)
            {
                sortedItems[i].Move(moveDistance, animationSettings.duration);
            }

            return Task.CompletedTask;
        }

        private async Task CycleEndItem(List<PrespectiveLayoutElement> sortedItems, AnimationDirection direction)
        {
            var cyclingItem = direction == AnimationDirection.Right
                ? sortedItems[ItemCount - 1]
                : sortedItems[0];

            var (exitPos, entryPos, finalPos) = CalculateCyclePositions(direction);
            var halfDuration = animationSettings.duration / 2;

            // Exit animation
            cyclingItem.MoveTo(exitPos, halfDuration);
            cyclingItem.FadeOut(halfDuration, 0);

            await Task.Delay((int)(halfDuration * 1000));

            // Entry animation
            cyclingItem.transform.localPosition = new Vector3(entryPos, 0, 0);
            cyclingItem.MoveTo(finalPos, halfDuration);
            cyclingItem.FadeIn(halfDuration);
        }

        private (float exit, float entry, float final) CalculateCyclePositions(AnimationDirection direction)
        {
            var offset = animationSettings.exitOffset;

            return direction == AnimationDirection.Right
                ? (MaxPosition + offset, MinPosition - offset, MinPosition)
                : (MinPosition - offset, MaxPosition + offset, MaxPosition);
        }

        private void FadeInAllItems(List<PrespectiveLayoutElement> sortedItems)
        {
            var halfDuration = animationSettings.duration / 2;
            foreach (var item in sortedItems)
            {
                item.FadeIn(halfDuration);
            }
        }

        private void UpdateItemsList()
        {
            if (items.Count > 0)
            {
                var firstItem = items[0];
                items.RemoveAt(0);
                items.Add(firstItem);
            }
        }
        #endregion

        #region Private Methods - Utility
        private void ReorderLayers()
        {
            items.OrderBy(x => Math.Abs(x.transform.localPosition.x))
                .Select(x => x.transform)
                .ForEach(x => x.SetAsFirstSibling());
        }

        private bool ValidateDispose()
        {
            if (isAnimating)
            {
                Debug.LogWarning("Cannot dispose while animation is in progress.");
                return false;
            }
            return true;
        }

        private void CleanupLayoutElements()
        {
            foreach (Transform child in transform)
            {
                var element = child.GetComponent<PrespectiveLayoutElement>();
                if (element != null)
                {
                    DestroyImmediate(element);
                }
            }
        }

        private void ResetState()
        {
            isInitialized = false;
            items.Clear();
            originalOrder?.Clear();
        }

        private bool IsPlayingAndNotAnimating()
        {
            return Application.isPlaying && !isAnimating && isInitialized;
        }
        #endregion


        internal enum AnimationDirection
        {
            Left = -1,
            Right = 1
        }

        [System.Serializable]
        internal class AnimationSettings
        {
            [Range(0.1f, 5f)]
            public float duration = 2f;

            [Range(0.1f, 1f)]
            public float minScale = 0.65f;

            [Range(10f, 100f)]
            public float exitOffset = 50f;
        }
    }


   
}