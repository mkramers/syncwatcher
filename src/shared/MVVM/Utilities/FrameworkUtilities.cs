using System.Diagnostics;
using System.Windows;
using System.Windows.Media;

namespace MVVM.Utilities
{
    public static class FrameworkUtilities
    {
        public static T GetFrameworkElementByName<T>(DependencyObject _referenceElement) where T : FrameworkElement
        {
            FrameworkElement child = null;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(_referenceElement); i++)
            {
                child = VisualTreeHelper.GetChild(_referenceElement, i) as FrameworkElement;

                Debug.WriteLine(child);

                if (child != null && child.GetType() == typeof(T))
                {
                    break;
                }
                if (child != null)
                {
                    child = GetFrameworkElementByName<T>(child);

                    if (child != null && child.GetType() == typeof(T))
                    {
                        break;
                    }
                }
            }
            return child as T;
        }
    }
}
