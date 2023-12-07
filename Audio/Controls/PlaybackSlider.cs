using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Utilities;
using System;

namespace Audio.Controls;
internal class PlaybackSlider : Slider
{
    public static readonly DirectProperty<PlaybackSlider, double> TimeProperty = AvaloniaProperty.RegisterDirect<PlaybackSlider, double>(nameof(Time), o => o.Time, (o, v) => o.Time = v, defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);
    public static readonly DirectProperty<PlaybackSlider, bool> IsSeekingProperty = AvaloniaProperty.RegisterDirect<PlaybackSlider, bool>(nameof(IsSeeking), o => o.IsSeeking, (o, v) => o.IsSeeking = v, defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);

    private double _time;
    private bool _isSeeking;
    private RepeatButton _decreaseButton;
    private RepeatButton _increaseButton;
    private Track _track;
    public bool IsSeeking
    {
        get => _isSeeking;
        set => SetAndRaise(IsSeekingProperty, ref _isSeeking, value);
    }
    public double Time
    {
        get => _time;
        set => SetAndRaise(TimeProperty, ref _time, value);
    }
    protected override Type StyleKeyOverride => typeof(Slider);
    static PlaybackSlider()
    {
        Thumb.DragDeltaEvent.AddClassHandler<PlaybackSlider>((x, e) => x.OnThumbDragStarted(e), RoutingStrategies.Bubble);
        Thumb.DragCompletedEvent.AddClassHandler<PlaybackSlider>((x, e) => x.OnThumbDragCompleted(e), RoutingStrategies.Bubble);
    }
    protected override void OnThumbDragStarted(VectorEventArgs e)
    {
        IsSeeking = true;
    }
    protected override void OnThumbDragCompleted(VectorEventArgs e)
    {
        IsSeeking = false;
        Time = Value;
    }
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        _track = e.NameScope.Find<Track>("PART_Track");
        _decreaseButton = e.NameScope.Find("PART_DecreaseButton") as RepeatButton;
        _increaseButton = e.NameScope.Find("PART_IncreaseButton") as RepeatButton;
        
        if (_decreaseButton != null)
        {
            _decreaseButton.AddHandler(PointerPressedEvent, TrackPressed, RoutingStrategies.Tunnel);
            _decreaseButton.AddHandler(PointerReleasedEvent, TrackReleased, RoutingStrategies.Tunnel);
        }
        
        if (_increaseButton != null)
        {
            _increaseButton.AddHandler(PointerPressedEvent, TrackPressed, RoutingStrategies.Tunnel);
            _increaseButton.AddHandler(PointerReleasedEvent, TrackReleased, RoutingStrategies.Tunnel);
        }

        AddHandler(PointerMovedEvent, TrackMoved, RoutingStrategies.Tunnel);
    }
    private void TrackPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            MoveToPoint(e.GetCurrentPoint(_track));
            IsSeeking = true;
        }
    }
    private void TrackReleased(object? sender, PointerReleasedEventArgs e)
    {
        IsSeeking = false;
        Time = Value;
    }
    private void TrackMoved(object? sender, PointerEventArgs e)
    {
        if (IsSeeking)
        {
            MoveToPoint(e.GetCurrentPoint(_track));
        }
    }
    private void MoveToPoint(PointerPoint posOnTrack)
    {
        if (_track is null)
            return;

        var thumbLength = _track.Thumb?.Bounds.Width ?? 0.0 + double.Epsilon;
        var trackLength = _track.Bounds.Width - thumbLength;
        var trackPos = posOnTrack.Position.X;
        var pos = MathUtilities.Clamp((trackPos - thumbLength * 0.5) / trackLength, 0.0d, 1.0d);
        var range = Maximum - Minimum;
        var finalValue = pos * range + Minimum;

        SetCurrentValue(ValueProperty, finalValue);
    }
}
