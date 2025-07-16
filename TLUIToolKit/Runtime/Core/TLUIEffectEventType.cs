using System;

namespace TLUIToolkit
{
    public enum TLUIEffectEventType
    {
        None = 0,
        OnShow = 1 << 0,
        OnHide = 1 << 1,
        OnClick = 1 << 2,
        OnHover = 1 << 3,
        OnHoverExit = 1 << 4,
        OnFocus = 1 << 5,
        OnValueChanged = 1 << 6
    }
    [Flags]
    public enum TLUIEffectFeedbackType
    {
        None=0,
        Audio = 1 << 0,
        XRVibration = 1 << 1,
    }
}