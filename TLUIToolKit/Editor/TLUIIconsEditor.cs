using UnityEngine;
using UnityEditor; // Required for EditorWindow and other editor functionalities
using System.IO;   // Required for Path operations
using System.Linq; // Required for LINQ operations on lists

namespace TLUIToolkit.Editor
{
    /// <summary>
    /// Custom Unity Editor window for managing the TLUIIconsLibrary ScriptableObject.
    /// This window allows developers to easily locate, select, or create the
    /// TLUIIconsLibrary asset, streamlining the workflow for icon management.
    /// </summary>
    public class EditorWindow : UnityEditor.EditorWindow // Renamed to EditorWindow
    {
        // Reference to the user's editable TLUIIconsLibrary asset
        private TLUIIconsLibrary _iconLibrary;
        // Reference to the default TLUIIconsLibrary asset, potentially from a package
        private TLUIIconsLibrary _defaultIconLibrary;

        // The name of the TLUIIconsLibrary asset file (without extension).
        private const string IconLibraryAssetName = "TLUIIconsLibrary";
        // The recommended path relative to the Assets folder where the user's editable asset should reside.
        // This ensures it's editable by the user and discoverable by Resources.Load().
        private const string RecommendedResourcesPath = "Assets/Resources/";
        private const string RecommendedFullAssetPath = RecommendedResourcesPath + IconLibraryAssetName + ".asset";

        // The specific path to the default TLUIIconsLibrary asset within a package's Resources folder.
        // This is the path used by Resources.Load() to find it.
        private const string DefaultPackageResourcePath = "TLUIToolkit/ScriptableObjects/DefaultUIIconsLibrary";


        /// <summary>
        /// Opens the TLUIIconsEditor window from the Unity Editor menu.
        /// This method is decorated with [MenuItem] to create the menu entry.
        /// </summary>
        [MenuItem("Tools/ThreeLinesUI/Icons Library")]
        public static void ShowWindow()
        {
            // Get existing open window or create a new one.
            GetWindow<EditorWindow>("TLUI Icons Library"); // Updated to EditorWindow
        }

        /// <summary>
        /// Called when the editor window is enabled or gains focus.
        /// Used to initialize or re-initialize the _iconLibrary and _defaultIconLibrary references.
        /// </summary>
        private void OnEnable()
        {
            FindIconLibrary();
        }

        /// <summary>
        /// Attempts to find the TLUIIconsLibrary asset(s) in the project.
        /// It first tries to load the user's editable library from Resources,
        /// then searches the entire project for it.
        /// It also attempts to load the default library from its specified package path.
        /// </summary>
        private void FindIconLibrary()
        {
            // Try to load the user's main library from Resources first.
            // This will find the asset if it's in *any* Resources folder, including those within packages.
            _iconLibrary = Resources.Load<TLUIIconsLibrary>(IconLibraryAssetName);

            if (_iconLibrary == null)
            {
                // If not found via Resources.Load, search the entire project for the asset.
                // This covers cases where the asset might be outside a Resources folder or in a non-standard location.
                string[] guids = AssetDatabase.FindAssets($"t:{typeof(TLUIIconsLibrary).Name}");
                if (guids.Length > 0)
                {
                    // Take the first found asset if multiple exist.
                    // Prioritize the one in RecommendedFullAssetPath if multiple are found.
                    string foundPath = "";
                    foreach (string guid in guids)
                    {
                        string path = AssetDatabase.GUIDToAssetPath(guid);
                        if (path == RecommendedFullAssetPath)
                        {
                            foundPath = path;
                            break;
                        }
                        if (string.IsNullOrEmpty(foundPath)) // Take the first one if recommended not found yet
                        {
                            foundPath = path;
                        }
                    }
                    if (!string.IsNullOrEmpty(foundPath))
                    {
                        _iconLibrary = AssetDatabase.LoadAssetAtPath<TLUIIconsLibrary>(foundPath);
                    }
                }
            }

            // Attempt to load the default icon library from its specific package's Resources path.
            _defaultIconLibrary = Resources.Load<TLUIIconsLibrary>(DefaultPackageResourcePath);
        }

        /// <summary>
        /// Renders the GUI for the editor window.
        /// This method is called multiple times per second by Unity.
        /// </summary>
        private void OnGUI()
        {
            GUILayout.Label("TLUI Icons Library Management", EditorStyles.boldLabel);
            EditorGUILayout.Space(10);

            if (_iconLibrary != null)
            {
                EditorGUILayout.HelpBox("TLUIIconsLibrary asset found.", MessageType.Info);

                // Display the found ScriptableObject field in the inspector
                _iconLibrary = (TLUIIconsLibrary)EditorGUILayout.ObjectField(
                    "Current Library", _iconLibrary, typeof(TLUIIconsLibrary), false);

                EditorGUILayout.Space(5);

                // Button to select the asset in the Project window
                if (GUILayout.Button("Select Library in Project"))
                {
                    Selection.activeObject = _iconLibrary;
                    EditorGUIUtility.PingObject(_iconLibrary); // Pings the asset in the Project window
                }

                // Button to open the Inspector for the asset
                if (GUILayout.Button("Open Library Inspector"))
                {
                    Selection.activeObject = _iconLibrary;
                }
            }
            else // _iconLibrary is null (user's editable library not found)
            {
                EditorGUILayout.HelpBox(
                    "Your TLUIIconsLibrary asset was not found in the project. " +
                    "It is recommended to create it in 'Assets/Resources/' for easy management and runtime loading.", MessageType.Warning);

                EditorGUILayout.Space(10);
                GUILayout.Label("Options:", EditorStyles.boldLabel);

                // Option to create a new blank library
                if (GUILayout.Button("Create New Blank Icons Library in Assets/Resources/"))
                {
                    CreateNewIconLibraryAsset();
                }

                // New logic: Check if IconsLibrary exists in Assets folder, if not, offer to copy default
                if (AssetDatabase.FindAssets("TLUIIconsLibrary t:ScriptableObject").Length == 0)
                {
                    if (_defaultIconLibrary != null)
                    {
                        EditorGUILayout.Space(5);
                        EditorGUILayout.ObjectField("Default Library (from package)", _defaultIconLibrary, typeof(TLUIIconsLibrary), false);

                        if (GUILayout.Button("Copy Default Icons Library to Assets/Resources/"))
                        {
                            CopyDefaultIconLibraryAsset();
                        }
                    }
                    else
                    {
                        EditorGUILayout.HelpBox("Default TLUIIconsLibrary not found using Resources.Load at internal path:\n" + DefaultPackageResourcePath, MessageType.Info);
                    }
                }
                else // Existing TLUIIconsLibrary found somewhere, but not the one we want to manage (i.e. not in Assets/Resources or named 'TLUIIconsLibrary')
                {
                    // Option to copy the default library if found, even if some other TLUIIconsLibrary exists
                    if (_defaultIconLibrary != null)
                    {
                        EditorGUILayout.Space(5);
                        EditorGUILayout.ObjectField("Default Library (from package)", _defaultIconLibrary, typeof(TLUIIconsLibrary), false);

                        if (GUILayout.Button("Copy Default Icons Library to Assets/Resources/"))
                        {
                            CopyDefaultIconLibraryAsset();
                        }
                    }
                    else
                    {
                        EditorGUILayout.HelpBox("Default TLUIIconsLibrary not found using Resources.Load at internal path:\n" + DefaultPackageResourcePath, MessageType.Info);
                    }
                }
            }

            EditorGUILayout.Space(10);
            GUILayout.Label("Note: For runtime access via TLUIIcons.GetIcon(), the TLUIIconsLibrary asset " +
                            "must be located in a folder named 'Resources' (e.g., 'Assets/Resources/TLUIIconsLibrary.asset'). " +
                            "If your project uses packages, it's best to keep this editable asset outside the package.", EditorStyles.miniLabel);
        }

        /// <summary>
        /// Creates a new TLUIIconsLibrary ScriptableObject asset in the project.
        /// It attempts to create it in the 'Assets/Resources/' folder, as this is the
        /// recommended location for user-editable assets that need to be loaded at runtime.
        /// </summary>
        private void CreateNewIconLibraryAsset()
        {
            // Ensure the Resources folder exists within Assets/
            if (!AssetDatabase.IsValidFolder(RecommendedResourcesPath))
            {
                // Create the Resources folder if it doesn't exist
                Directory.CreateDirectory(Application.dataPath + "/Resources");
                AssetDatabase.Refresh(); // Refresh to make Unity aware of the new folder
            }

            // Create the new asset instance
            TLUIIconsLibrary newLibrary = CreateInstance<TLUIIconsLibrary>();

            // Check if an asset already exists at the target path to prevent overwriting
            if (AssetDatabase.LoadAssetAtPath<TLUIIconsLibrary>(RecommendedFullAssetPath) != null)
            {
                Debug.LogWarning($"TLUIIconsEditor: An asset already exists at the recommended path: {RecommendedFullAssetPath}. " +
                                 "Please use the existing asset or move/rename it before creating a new one at this default location.");
                // Select the existing asset for the user to manage
                Selection.activeObject = AssetDatabase.LoadAssetAtPath<TLUIIconsLibrary>(RecommendedFullAssetPath);
                EditorGUIUtility.PingObject(Selection.activeObject);
                return;
            }

            // Create the asset file
            AssetDatabase.CreateAsset(newLibrary, RecommendedFullAssetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            _iconLibrary = newLibrary; // Set the reference to the newly created library
            Selection.activeObject = _iconLibrary; // Select it in the Project window
            EditorGUIUtility.PingObject(_iconLibrary); // Ping it for visibility

            Debug.Log($"TLUIIconsLibrary asset created at: {RecommendedFullAssetPath}");
        }

        /// <summary>
        /// Copies the default TLUIIconsLibrary asset's values from the package
        /// to a newly created user-editable asset in Assets/Resources/.
        /// This avoids direct file copying of package assets.
        /// </summary>
        private void CopyDefaultIconLibraryAsset()
        {
            if (_defaultIconLibrary == null)
            {
                Debug.LogError("TLUIIconsEditor: Cannot copy default library, as it was not found using Resources.Load at " + DefaultPackageResourcePath);
                return;
            }

            // Ensure the Resources folder exists within Assets/
            if (!AssetDatabase.IsValidFolder(RecommendedResourcesPath))
            {
                Directory.CreateDirectory(Application.dataPath + "/Resources");
                AssetDatabase.Refresh();
            }

            // Check if the target asset already exists to prevent overwriting
            TLUIIconsLibrary existingLibrary = AssetDatabase.LoadAssetAtPath<TLUIIconsLibrary>(RecommendedFullAssetPath);
            if (existingLibrary != null)
            {
                Debug.LogWarning($"TLUIIconsEditor: An icon library already exists at {RecommendedFullAssetPath}. " +
                                 "Please delete or move the existing library before copying the default one.");
                Selection.activeObject = existingLibrary;
                EditorGUIUtility.PingObject(existingLibrary);
                return;
            }

            // Create a new blank asset
            TLUIIconsLibrary newLibrary = CreateInstance<TLUIIconsLibrary>();

            // Copy values from the default library to the new library
            newLibrary.icons.Clear(); // Clear any pre-populated icons from OnEnable
            foreach (var iconEntry in _defaultIconLibrary.icons)
            {
                // Create new IconEntry instances to ensure they are not references to the default library's entries
                newLibrary.icons.Add(new IconEntry { type = iconEntry.type, sprite = iconEntry.sprite });
            }
            newLibrary.defaultNotFoundIcon = _defaultIconLibrary.defaultNotFoundIcon;

            // Create the asset file with the copied values
            AssetDatabase.CreateAsset(newLibrary, RecommendedFullAssetPath);
            EditorUtility.SetDirty(newLibrary); // Mark as dirty to ensure changes are saved
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            _iconLibrary = newLibrary; // Set the reference to the newly created library
            Selection.activeObject = _iconLibrary; // Select it in the Project window
            EditorGUIUtility.PingObject(_iconLibrary); // Ping it for visibility

            Debug.Log($"TLUIIconsLibrary asset copied values from default package path to new asset at: {RecommendedFullAssetPath}");
        }
    }
}