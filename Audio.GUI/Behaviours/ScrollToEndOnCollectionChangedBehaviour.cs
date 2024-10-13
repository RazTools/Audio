using Avalonia.Controls;
using Avalonia.Threading;
using Avalonia.Xaml.Interactivity;
using System.Collections.Specialized;

namespace Audio.GUI.Behaviours;
public class ScrollToEndOnCollectionChangedBehaviour : Behavior<ListBox>
{
    protected override void OnAttached()
    {
        base.OnAttached();
        if (AssociatedObject != null)
        {
            AssociatedObject.Loaded += AssociatedObject_Loaded;
            AssociatedObject.Unloaded += AssociatedObject_Unloaded;
        }
    }
    protected override void OnDetaching()
    {
        base.OnDetaching();
        if (AssociatedObject != null)
        {
            AssociatedObject.Loaded -= AssociatedObject_Loaded;
            AssociatedObject.Unloaded -= AssociatedObject_Unloaded;
        }
    }
    private void AssociatedObject_Loaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (AssociatedObject?.Items is INotifyCollectionChanged notifyCollectionChanged)
        {
            notifyCollectionChanged.CollectionChanged += NotifyCollectionChanged_CollectionChanged;
        }
    }
    private void AssociatedObject_Unloaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (AssociatedObject?.Items is INotifyCollectionChanged notifyCollectionChanged)
        {
            notifyCollectionChanged.CollectionChanged -= NotifyCollectionChanged_CollectionChanged;
        }
    }
    private void NotifyCollectionChanged_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == NotifyCollectionChangedAction.Add && AssociatedObject?.Items.Count > 0)
        {
            Dispatcher.UIThread.Post(() => AssociatedObject?.ScrollIntoView(AssociatedObject.Items.Count - 1));
        }
    }
}
