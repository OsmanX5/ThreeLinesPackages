using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace ThreeLines.Helpers
{
    public static class ObjectExtenstions
    {
        public static void CleanDestroy(this Object @object)
        {
            if (@object == null)
            {
                return;
            }
            if (Application.isPlaying)
            {
                Object.Destroy(@object);
            }
            else
            {
                Object.DestroyImmediate(@object);
            }
        }
    }

}
