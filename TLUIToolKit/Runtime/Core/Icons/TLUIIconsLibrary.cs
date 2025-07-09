using UnityEngine;
using System.Collections.Generic;
using System.Linq;
// Removed: using Sirenix.OdinInspector; // This line is removed if Odin Inspector is not installed

namespace TLUIToolkit
{


    /// <summary>
    /// A serializable class to pair an IconType enum value with its corresponding Sprite asset.
    /// This allows for easy assignment in the Unity Inspector within the TLUIIconsLibrary.
    /// </summary>
    [System.Serializable]
    public class IconEntry
    {
        [Tooltip("The type identifier for this icon.")]
        // Removed: [ReadOnly] // This attribute requires Odin Inspector
        public IconType type;

        [Tooltip("The actual Sprite asset for this icon.")]
        public Sprite sprite;
    }

    /// <summary>
    /// A ScriptableObject that serves as a centralized library for all UI icon Sprites.
    /// This asset can be created via the Unity Editor and populated with icon references.
    /// It's designed to be loaded by the TLUIIcons static class for runtime access.
    /// </summary>
    [CreateAssetMenu(fileName = "TLUIIconsLibrary", menuName = "ThreeLinesUI/Icon Library")]
    public class TLUIIconsLibrary : ScriptableObject
    {
        [Tooltip("List of all icon entries, mapping an IconType to a Sprite.")]
        // Removed: [TableList] // This attribute requires Odin Inspector
        public List<IconEntry> icons = new List<IconEntry>();

        [Tooltip("The default sprite to return if a requested icon is not found in the library.")]
        public Sprite defaultNotFoundIcon;

        /// <summary>
        /// Called when the ScriptableObject is loaded or created.
        /// This method is used to automatically populate the 'icons' list with all
        /// available IconType enum values if the list is currently empty.
        /// This ensures that when a new TLUIIconsLibrary asset is created, it comes
        /// pre-filled with entries for all defined icon types, making it easier
        /// for developers to assign sprites.
        /// </summary>
        private void OnEnable()
        {
            // Only populate if the list is empty or null to avoid overwriting existing data
            // and to prevent duplication on subsequent loads.
            if (icons == null || icons.Count == 0)
            {
                PopulateAllIconTypes();
            }
        }

        /// <summary>
        /// Populates the 'icons' list with an entry for each value in the IconType enum.
        /// This method is called internally when the ScriptableObject is enabled if the
        /// list is empty. It ensures that every defined icon type has a corresponding
        /// entry in the library, ready for a Sprite to be assigned in the Inspector.
        /// </summary>
        // Removed: [Button] // This attribute requires Odin Inspector
        private void PopulateAllIconTypes()
        {
            // The check for 'icons == null || icons.Count == 0' is handled by OnEnable().
            // This method should proceed with clearing and populating the list directly.
            icons.Clear(); // Clear any existing (potentially null) entries

            // Get all defined values of the IconType enum
            foreach (IconType type in System.Enum.GetValues(typeof(IconType)))
            {
                // Skip the 'None' entry if desired, or include it as a placeholder
                // For this implementation, 'None' is included as it might be useful
                // to explicitly assign a 'None' sprite.
                icons.Add(new IconEntry { type = type, sprite = null });
            }

            // Sort the list by IconType for better organization in the Inspector
            icons = icons.OrderBy(entry => entry.type).ToList();

            Debug.Log($"TLUIIconsLibrary: Populated {icons.Count} icon entries from IconType enum.");
        }


        /// <summary>
        /// Retrieves the Sprite associated with the given IconType from the library.
        /// </summary>
        /// <param name="type">The IconType to look up.</param>
        /// <returns>The Sprite if found, otherwise the defaultNotFoundIcon, or null if no default is set.</returns>
        public Sprite GetSprite(IconType type)
        {
            foreach (var entry in icons)
            {
                if (entry.type == type)
                {
                    return entry.sprite;
                }
            }
            // Log a warning if the icon type is not found in the library
            Debug.LogWarning($"TLUIIconsLibrary: Icon of type '{type}' not found in the library. " +
                             "Returning defaultNotFoundIcon if set, otherwise null.");
            // Return the default not found icon if available, otherwise null
            return defaultNotFoundIcon;
        }
    }


}
