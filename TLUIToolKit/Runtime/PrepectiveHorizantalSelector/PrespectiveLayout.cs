
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;


namespace TLUIToolkit
{
    public class PrespectiveLayout : MonoBehaviour
    {
        [SerializeField]
        float movingTime = 2f;

        [SerializeField]
        [Range(0.1f, 1f)]
        float minScale = 0.65f;

        [TitleGroup("Debug Tools")]
        [ShowInInspector]
        [TableList]
        List<PrespectiveLayoutElement> Items = new List<PrespectiveLayoutElement>();

        [TitleGroup("Debug Tools")]
        [ShowInInspector]
        float totalWidth => transform.GetComponent<RectTransform>().sizeDelta.x;

        [ShowInInspector]
        [TitleGroup("Debug Tools")]
        float deltaPosition => totalWidth / transform.childCount;

        [TitleGroup("Debug Tools")]
        [ShowInInspector]
        int MaxLevel => GetLevel(n - 1);
        
        [TitleGroup("Debug Tools")]
        [ShowInInspector]
        float MaxPosition => MaxLevel * deltaPosition;
        
        [TitleGroup("Debug Tools")]
        [ShowInInspector]
        float MinPosition => -MaxPosition;

        int n => Items==null? 0: Items.Count;

        bool isAnimating;
        List<Transform> originalOrder;

        public List<Transform> OriginalOrder
        {
            get
            {
                return originalOrder;
            }
        }
        public GameObject CenterObject =>
            Items.Count > 0 ? Items[n / 2].gameObject : null;
        private void Awake()
        {
            Init();
        }

        [Button(ButtonSizes.Large)]
        public void Init() {
            if (transform.childCount == 0)
            {
                Debug.LogWarning("No child elements found in PrespectiveLayout. Please add PrespectiveLayoutElement components to child objects.");
                return;
            }
            originalOrder = transform.GetComponentsInChildren<Transform>().ToList();
            AddPrespectiveComponentToChilds();
            BuildQueue();
            SetElementsPositions();
            ReorderLayers();
        }
        private void Update()
        {
            if(isAnimating) 
                ReorderLayers();
        }

        void AddPrespectiveComponentToChilds()
        {
            foreach (Transform child in transform)
            {
                if (child.GetComponent<PrespectiveLayoutElement>() == null)
                {
                    child.gameObject.AddComponent<PrespectiveLayoutElement>();
                }
            }
            
        }
        void BuildQueue()
        {
            Items.Clear();
            var elements = GetComponentsInChildren<PrespectiveLayoutElement>(true);
            for (int i = 0; i < elements.Length; i++)
            {
                Items.Add(elements[i]);
                elements[i].MinScale = minScale;
            }
        }
        
        void SetElementsPositions()
        {
            var elementsArray = Items.ToArray();
            elementsArray[0].transform.localPosition = new(0,0,0);
            int n = elementsArray.Length;
            for (int i=0;i < n; i++)
            {
                var item = elementsArray[i];
                item.transform.localPosition = new(GetPosition(i),0,0);
            }

            float GetPosition(int i)
            {
                if (i == 0)
                    return 0;
                return (i % 2 == 0 ? -1 : 1) * deltaPosition * GetLevel(i);
            }
        }
        void ReorderLayers()
        {
            Items.OrderBy(x => Math.Abs(x.transform.localPosition.x)).
                Select(x => x.transform).
                ForEach(x => x.SetAsFirstSibling());
        }

        async Task Move(bool moveRight)
        {
            if (isAnimating)
                return;
            isAnimating = true;
            var sortedItems = Items.OrderBy(x => x.transform.localPosition.x).ToList();

            void MoveItemsInDirection()
            {
                var (startIndex, endIndex, moveDistance) = moveRight
                    ? (0, n - 1, deltaPosition)
                    : (1, n, -deltaPosition);

                for (int i = startIndex; i < endIndex; i++)
                {
                    sortedItems[i].Move(moveDistance, movingTime);
                }
            }

            async Task CycleEndItem()
            {
                var cyclingItem = moveRight ? sortedItems[n - 1] : sortedItems[0];
                var (exitPosition, entryPosition, finalPosition) = moveRight
                    ? (MaxPosition + 50, MinPosition - 50, MinPosition)
                    : (MinPosition - 50, MaxPosition + 50, MaxPosition);

                // Move item out of view and fade out
                cyclingItem.MoveTo(exitPosition, movingTime / 2);
                cyclingItem.FadeOut(movingTime / 2, 0);

                await Task.Delay((int)(movingTime / 2) * 1000);

                // Reposition and move to final position
                cyclingItem.transform.localPosition = new(entryPosition, 0, 0);
                cyclingItem.MoveTo(finalPosition, movingTime / 2);
                cyclingItem.FadeIn(movingTime / 2);
            }

            void FadeInAllItems()
            {
                foreach (var item in sortedItems)
                    item.FadeIn(movingTime / 2);
            }

            MoveItemsInDirection();
            await CycleEndItem();
            FadeInAllItems();
            await Task.Delay((int)(movingTime / 2) * 1000);
            Items.Add(Items[0]);
            Items.RemoveAt(0);
            isAnimating = false;
        }

        [ShowIf("@UnityEngine.Application.isPlaying && !isAnimating")]
        [HorizontalGroup("Buttons", 0.5f)]
        [Button(ButtonSizes.Large)]
        public void MoveLeft()
        {
            Move(false);
        }

        [ShowIf("@UnityEngine.Application.isPlaying && !isAnimating")]
        [HorizontalGroup("Buttons", 0.5f)]
        [Button(ButtonSizes.Large)]
        public  void MoveRight()
        {
             Move(true);
        }

        int GetLevel(int i) => (i+1) / 2 ;

    }

}
