using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.XR;

namespace ThreeLines.Helpers
{
    public class XRHandsVibrator : Singleton<XRHandsVibrator>
    {
    enum HandType
    {
        Left,
        Right
    }
    static float verySoftAmplitude = 0.05f;
    static float softAmplitude = 0.1f;
    static float mediumAmplitude = 0.5f;
    static float hardAmplitude = 1f;

    static float verySoftDuration = 0.1f;
    static float softDuration = 0.2f;
    static float mediumDuration = 0.5f;
    static float hardDuration = 1f;

    static List<InputDevice> devices = new List<InputDevice>();
    Dictionary<PredfinedVibrations, VibrationSettings> predifnedVibrationsDict = new()
    {
        {PredfinedVibrations.verySoft,new(verySoftAmplitude,verySoftDuration)},
        {PredfinedVibrations.soft,new(softAmplitude,softDuration)},
        {PredfinedVibrations.medium,new(mediumAmplitude,mediumDuration)},
        {PredfinedVibrations.hard,new(hardAmplitude,hardDuration)}
    };
/*    public void VibrateHandCommon(XRBaseControllerInteractor interactor, PredfinedVibrations vibrationType)
    {
        interactor.SendHapticImpulse(predifnedVibrationsDict[vibrationType].amplitude, predifnedVibrationsDict[vibrationType].duration);
    }*/
    [Button]
    public void VibrateLeftHandCommon(PredfinedVibrations vibrationType)
    {
        HandVibration(HandType.Left, predifnedVibrationsDict[vibrationType]);
    }
    [Button]
    public void VibrateRightHandCommon(PredfinedVibrations vibrationType)
    {
        HandVibration(HandType.Right, predifnedVibrationsDict[vibrationType]);
    }
    public void VibrateLeftHand()
    {
        HandVibration(HandType.Left);
    }
    public void VibrateRightHand()
    {
        HandVibration(HandType.Right);
    }
    void HandVibration(HandType handType, VibrationSettings vibration)
    {
        try
        {
            InputDevice handDevice = GetXRHandController(handType);
            handDevice.SendHapticImpulse(0, vibration.amplitude, vibration.duration);
        }
        catch (System.Exception)
        {
            Debug.LogError($"Failed to vibrate {handType} hand. Make sure the controller is connected and has haptic capabilities.");
        }
    }
    void HandVibration(HandType handType)
    {
        InputDevice handDevice = GetXRHandController(handType);
        handDevice.SendHapticImpulse(0, 0.1f, 0.1f);
    }

    private static InputDevice GetXRHandController(HandType handType)
    {
        return handType == HandType.Left
            ? GetLeftController()
            : GetRightController();
    }

    public static InputDevice GetLeftController()
    {
        InputDevices.GetDevices(devices);

        var leftHandedControllers = new List<InputDevice>();
        var dc = InputDeviceCharacteristics.HeldInHand | InputDeviceCharacteristics.Left | InputDeviceCharacteristics.Controller;
        InputDevices.GetDevicesWithCharacteristics(dc, leftHandedControllers);
        return leftHandedControllers.FirstOrDefault();
    }
    public static InputDevice GetRightController()
    {
        InputDevices.GetDevices(devices);

        var rightHandedControllers = new List<InputDevice>();
        var dc = InputDeviceCharacteristics.HeldInHand | InputDeviceCharacteristics.Right | InputDeviceCharacteristics.Controller;
        InputDevices.GetDevicesWithCharacteristics(dc, rightHandedControllers);

        return rightHandedControllers.FirstOrDefault();
    }
    public void StopVibrationLeftHand()
    {
        StopHandVibration(HandType.Left);
    }
    public void StopVibrationRightHand()
    {
        StopHandVibration(HandType.Right);
    }
    void StopHandVibration(HandType handType)
    {
        var handDevice = handType == HandType.Left
            ? GetLeftController()
            : GetRightController();
            handDevice.StopHaptics();
    }

    struct VibrationSettings
    {
        [Range(0, 1)]
        public float amplitude;

        public float duration;

        public VibrationSettings(float amplitude, float duration)
        {
            this.amplitude = amplitude;
            this.duration = duration;
        }
    }
    public enum PredfinedVibrations
    {
        verySoft,
        soft,
        medium,
        hard
    }
}

}
