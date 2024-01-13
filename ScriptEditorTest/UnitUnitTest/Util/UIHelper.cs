using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ScriptEditorTest.UnitUnitTest.Util;

internal static class UIHelper
{
    public static T? FindChild<T>(this DependencyObject? parent) where T : DependencyObject
    {
        if (parent == null)
            return null;

        var count = VisualTreeHelper.GetChildrenCount(parent);
        for (var i = 0; i < count; i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);

            if (child is T typedChild)
                return typedChild;

            var result = FindChild<T>(child);
            if (result != null)
                return result;
        }
        return null;
    }


    public static Panel? GetContainer(this DependencyObject? element)
    {
        while (element != null && !(element is Panel))
        {
            element = VisualTreeHelper.GetParent(element);
        }

        return element as Panel;
    }




    public static void AddChild(this Panel? element, UIElement child)
    {

        if (element is { } panel)
        {
            panel.Children.Add(child);
        }

    }

    public static void RemoveChild(this Panel? element, UIElement child)
    {
        if (element is { } panel)
        {
            panel.Children.Remove(child);
        }
    }


    public static void AddChild(this DependencyObject? element, UIElement child)
    {

        while (element != null && !(element is Panel))
        {
            element = VisualTreeHelper.GetParent(element);
        }

        if (element is Panel panel)
        {
            panel.Children.Add(child);
        }

    }

    public static void RemoveChild(this DependencyObject? element, UIElement child)
    {

        while (element != null && !(element is Panel))
        {
            element = VisualTreeHelper.GetParent(element);
        }

        if (element is Panel panel)
        {
            panel.Children.Remove(child);
        }
    }
}