using UnityEngine.UIElements;

namespace FourFatesStudios.ProjectWarden.GameSystems.CraftingMenu.AlchemyMenu
{
    /// <summary>
    /// Extension methods for UIElements to provide additional convenience methods
    /// </summary>
    public static class UIElementExtensions
    {
        /// <summary>
        /// Sets the display style of a VisualElement to either flex or none
        /// </summary>
        /// <param name="element">The visual element to modify</param>
        /// <param name="displayed">True to show (display: flex), false to hide (display: none)</param>
        public static void SetDisplayed(this VisualElement element, bool displayed)
        {
            if (element != null)
            {
                element.style.display = displayed ? DisplayStyle.Flex : DisplayStyle.None;
            }
        }
        
        /// <summary>
        /// Toggle the display style of a VisualElement between flex and none
        /// </summary>
        /// <param name="element">The visual element to toggle</param>
        /// <returns>True if now displayed, false if now hidden</returns>
        public static bool ToggleDisplay(this VisualElement element)
        {
            if (element != null)
            {
                bool isCurrentlyDisplayed = element.style.display.value == DisplayStyle.Flex;
                element.SetDisplayed(!isCurrentlyDisplayed);
                return !isCurrentlyDisplayed;
            }
            return false;
        }
        
        /// <summary>
        /// Check if a VisualElement is currently displayed (not display: none)
        /// </summary>
        /// <param name="element">The visual element to check</param>
        /// <returns>True if displayed, false if hidden</returns>
        public static bool IsDisplayed(this VisualElement element)
        {
            if (element != null)
            {
                return element.style.display.value != DisplayStyle.None;
            }
            return false;
        }
        
        /// <summary>
        /// Sets the visibility of a VisualElement using the visibility style
        /// </summary>
        /// <param name="element">The visual element to modify</param>
        /// <param name="visible">True to show (visibility: visible), false to hide (visibility: hidden)</param>
        public static void SetVisible(this VisualElement element, bool visible)
        {
            if (element != null)
            {
                element.style.visibility = visible ? Visibility.Visible : Visibility.Hidden;
            }
        }
        
        /// <summary>
        /// Adds a CSS class to the element if the condition is true, removes it if false
        /// </summary>
        /// <param name="element">The visual element to modify</param>
        /// <param name="className">The CSS class name to add or remove</param>
        /// <param name="condition">True to add the class, false to remove it</param>
        public static void SetClass(this VisualElement element, string className, bool condition)
        {
            if (element != null)
            {
                if (condition)
                {
                    element.AddToClassList(className);
                }
                else
                {
                    element.RemoveFromClassList(className);
                }
            }
        }
        
        /// <summary>
        /// Safely enable or disable a Button element
        /// </summary>
        /// <param name="button">The button to modify</param>
        /// <param name="enabled">True to enable, false to disable</param>
        public static void SetEnabled(this Button button, bool enabled)
        {
            if (button != null)
            {
                button.SetEnabled(enabled);
            }
        }
        
        /// <summary>
        /// Safely set the text of a Label element
        /// </summary>
        /// <param name="label">The label to modify</param>
        /// <param name="text">The text to set</param>
        public static void SetText(this Label label, string text)
        {
            if (label != null)
            {
                label.text = text ?? string.Empty;
            }
        }
    }
}