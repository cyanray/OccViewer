namespace OccViewer.Viewer.Shortcut
{
    public enum PressedKey
    {
        None = 0x00,
        Ctrl = 0x01,
        Shift = 0x02,
        Alt = 0x04,
        CtrlShift = Ctrl | Shift,
        CtrlAlt = Ctrl | Alt,
        ShiftAlt = Shift | Alt,
        CtrlShiftAlt = Ctrl | Shift | Alt
    }
}
