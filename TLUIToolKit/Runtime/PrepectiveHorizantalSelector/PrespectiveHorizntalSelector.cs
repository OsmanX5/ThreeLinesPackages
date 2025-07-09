using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace TLUIToolkit
{
    public class PrespectiveHorizntalSelector : HorizontalSelectorBase
    {
        [SerializeField] 
        [Title("Prespective References")]
        [Required]
        private PrespectiveLayout prespectiveLayout;

        private void Awake()
        {
            if (prespectiveLayout == null)
            {
                Debug.LogError("PrespectiveLayout component is missing on the GameObject.");
            }
        }
        public override void Next()
        {
            base.Next();
            prespectiveLayout.MoveRight();
        }
        public override void Previous()
        {
            base.Previous();
            prespectiveLayout.MoveLeft();
        }
    }

}
