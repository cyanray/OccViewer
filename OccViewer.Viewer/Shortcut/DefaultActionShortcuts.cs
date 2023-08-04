namespace OccViewer.Viewer.Shortcut
{
    public class DefaultActionShortcuts : IActionShortcuts
    {
        CombineShortcut IActionShortcuts.RectangleSelectionShortcut { get; } = new CombineShortcut
        {
            MouseButton = MouseButtonTrigger.LeftPressed
        };

        CombineShortcut IActionShortcuts.RectangleSelectionXorShortcut { get; } = new CombineShortcut
        {
            Key = PressedKey.Shift,
            MouseButton = MouseButtonTrigger.LeftPressed
        };

        CombineShortcut IActionShortcuts.DynamicZoomingShortcut { get; } = new CombineShortcut
        {
            Key = PressedKey.Ctrl,
            MouseButton = MouseButtonTrigger.LeftPressed
        };

        CombineShortcut IActionShortcuts.WindowZoomingShortcut { get; } = new CombineShortcut
        {
            Key = PressedKey.CtrlShift,
            MouseButton = MouseButtonTrigger.LeftPressed
        };

        CombineShortcut IActionShortcuts.DynamicPanningShortcut { get; } = new CombineShortcut
        {
            Key = PressedKey.Ctrl,
            MouseButton = MouseButtonTrigger.MiddlePressed
        };

        CombineShortcut IActionShortcuts.DynamicRotationShortcut { get; } = new CombineShortcut
        {
            Key = PressedKey.Ctrl,
            MouseButton = MouseButtonTrigger.RightPressed
        };

        CombineShortcut IActionShortcuts.PickSelectionShortcut { get; } = new CombineShortcut
        {
            MouseButton = MouseButtonTrigger.LeftClicked
        };

        CombineShortcut IActionShortcuts.PickSelectionXorShortcut { get; } = new CombineShortcut
        {
            Key = PressedKey.Shift,
            MouseButton = MouseButtonTrigger.LeftClicked
        };

        CombineShortcut IActionShortcuts.PopupMenuShortcut { get; } = new CombineShortcut
        {
            MouseButton = MouseButtonTrigger.RightClicked
        };

    }
}
