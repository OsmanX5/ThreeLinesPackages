using UnityEngine;
using UnityEditor;

namespace TLUIToolkit
{
    public class TLUISoundsEditor
    {
        [MenuItem("GameObject/ThreeLinesUI/UIToolkit/UI Sounds Manager", false, 10)]
        private static void CreateUISoundsManager()
        {
            GameObject soundsManager = new GameObject("UI Sounds Manager");
            TLUISounds soundsComponent = soundsManager.AddComponent<TLUISounds>();
            soundsComponent.AutoSetupAudioSource();
            // Try to load default sounds
            soundsComponent.TryLoadDefaults();

            Selection.activeGameObject = soundsManager;
            EditorUtility.SetDirty(soundsManager);
        }
    }
}