namespace OccViewer.Viewer.Shortcut
{
    public class IntuitiveActionShortcuts : IActionShortcuts
    {
        CombineShortcut IActionShortcuts.RectangleSelectionShortcut { get; } = new CombineShortcut
        {
            Key = PressedKey.Ctrl,
            MouseButton = MouseButtonTrigger.LeftPressed
        };

        CombineShortcut IActionShortcuts.DynamicZoomingShortcut { get; } = new CombineShortcut
        {
            MouseButton = MouseButtonTrigger.RightPressed
        };

        CombineShortcut IActionShortcuts.WindowZoomingShortcut { get; } = new CombineShortcut
        {
            Key = PressedKey.Shift,
            MouseButton = MouseButtonTrigger.RightPressed
        };

        CombineShortcut IActionShortcuts.DynamicPanningShortcut { get; } = new CombineShortcut
        {
            MouseButton = MouseButtonTrigger.MiddlePressed
        };

        CombineShortcut IActionShortcuts.DynamicRotationShortcut { get; } = new CombineShortcut
        {
            MouseButton = MouseButtonTrigger.LeftPressed
        };

        CombineShortcut IActionShortcuts.PickSelectionShortcut { get; } = new CombineShortcut
        {
            MouseButton = MouseButtonTrigger.LeftClicked
        };

        CombineShortcut IActionShortcuts.XorPickSelectionShortcut { get; } = new CombineShortcut
        {
            Key = PressedKey.Ctrl,
            MouseButton = MouseButtonTrigger.LeftClicked,
        };

        CombineShortcut IActionShortcuts.PopupMenuShortcut { get; } = new CombineShortcut 
        { 
            MouseButton = MouseButtonTrigger.RightClicked 
        };
    }
}
