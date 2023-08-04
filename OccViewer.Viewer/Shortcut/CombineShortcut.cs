namespace OccViewer.Viewer.Shortcut
{
    public class CombineShortcut
    {
        public PressedKey Key { get; set; } = PressedKey.None;
        public MouseButtonTrigger MouseButton { get; set; } = MouseButtonTrigger.None;

        public CombineShortcut()
        {
        }

        public CombineShortcut(PressedKey key, MouseButtonTrigger mouseButton)
        {
            Key = key;
            MouseButton = mouseButton;
        }

        public override bool Equals(object? other)
        {
            if (other is not CombineShortcut shortcut) return false;
            return Key == shortcut.Key && MouseButton == shortcut.MouseButton;
        }

        public override int GetHashCode()
        {
            return Key.GetHashCode() ^ MouseButton.GetHashCode();
        }
    }
}
