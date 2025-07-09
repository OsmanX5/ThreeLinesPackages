using UnityEngine;
using System.Collections.Generic;

namespace TLUIToolkit
{
    /// <summary>
    /// TLUIIcons is a static utility class providing global access to UI icon Sprites.
    /// It loads a TLUIIconsLibrary ScriptableObject from a Resources folder and
    /// allows retrieval of icons by their IconType enum.
    /// </summary>
    public static class TLUIIcons
    {
        // Private static field to hold the loaded icon library instance.
        private static TLUIIconsLibrary _iconLibrary;

        // The name of the TLUIIconsLibrary asset file (without extension).
        // This asset MUST be placed in a folder named "Resources" anywhere in your project.
        private const string IconLibraryAssetName = "TLUIIconsLibrary";

        /// <summary>
        /// Ensures the TLUIIconsLibrary ScriptableObject is loaded.
        /// This method is called automatically on the first attempt to get an icon.
        /// It searches for the library asset in any "Resources" folder within the project.
        /// </summary>
        private static void EnsureLibraryLoaded()
        {
            // If the library is already loaded, no need to load it again.
            if (_iconLibrary == null)
            {
                // Load the ScriptableObject from a Resources folder.
                _iconLibrary = Resources.Load<TLUIIconsLibrary>(IconLibraryAssetName);

                // If the library is still null after attempting to load, log an error.
                if (_iconLibrary == null)
                {
                    Debug.LogError($"TLUIIcons: Icon library '{IconLibraryAssetName}' not found in any 'Resources' folder. " +
                                   "Please create a 'TLUIIconsLibrary' ScriptableObject asset " +
                                   "via 'Assets/Create/TL/UI/Icon Library' and place it in a folder named 'Resources' " +
                                   "somewhere in your Unity project (e.g., 'Assets/Resources/TLUIIconsLibrary.asset').");
                }
            }
        }

        /// <summary>
        /// Retrieves a UI icon (Sprite) by its specified type.
        /// This is the primary method to access icons.
        /// </summary>
        /// <param name="iconType">The type of icon to retrieve (from the IconType enum).</param>
        /// <returns>
        /// The Sprite associated with the given iconType.
        /// Returns null if the icon library is not loaded, or if the specified iconType
        /// is not found within the loaded library.
        /// </returns>
        public static Sprite GetIcon(IconType iconType)
        {
            // Ensure the icon library is loaded before attempting to retrieve an icon.
            EnsureLibraryLoaded();

            // If the library was successfully loaded, attempt to get the sprite.
            if (_iconLibrary != null)
            {
                return _iconLibrary.GetSprite(iconType);
            }

            // If the library failed to load, return null.
            return null;
        }

        /// <summary>
        /// (Optional) Clears the cached icon library.
        /// This can be useful for editor scripting or testing scenarios where you need
        /// to force a reload of the library. Not typically needed in runtime builds.
        /// </summary>
        public static void ClearLibraryCache()
        {
            _iconLibrary = null;
            Debug.Log("TLUIIcons: Icon library cache cleared.");
        }
    }
}
