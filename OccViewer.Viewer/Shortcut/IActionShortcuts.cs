namespace OccViewer.Viewer.Shortcut
{
    public interface IActionShortcuts
    {
        public CombineShortcut PickSelectionShortcut { get; }
        public CombineShortcut XorPickSelectionShortcut { get; }
        public CombineShortcut RectangleSelectionShortcut { get; }
        public CombineShortcut DynamicZoomingShortcut { get; }
        public CombineShortcut WindowZoomingShortcut { get; }
        public CombineShortcut DynamicPanningShortcut { get; }
        public CombineShortcut DynamicRotationShortcut { get; }
        public CombineShortcut PopupMenuShortcut { get; }
    }
}
