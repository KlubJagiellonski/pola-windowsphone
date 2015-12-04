
namespace Windows.UI.Xaml
{
    public static class XamlExtensions
    {
        /// <summary>
        /// Checks whether a UIElement is visible.
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public static bool IsVisible(this UIElement element)
        {
            return element.Visibility == Visibility.Visible;
        }

        /// <summary>
        /// Changes the visibility of a UIElement using a boolean value.
        /// </summary>
        /// <param name="element"></param>
        /// <param name="visible"></param>
        public static void SetVisible(this UIElement element, bool visible)
        {
            element.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}
