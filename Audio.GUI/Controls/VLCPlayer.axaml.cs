using Audio.Entries;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using LibVLCSharp.Shared;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Audio.GUI.Controls;

public partial class VLCPlayer : UserControl, IDisposable
{
    private readonly MediaPlayer _mediaPlayer;
    private readonly LibVLC _context;

    private MemoryStream? _stream;
    private bool _isLoading = false;
    private bool _isSeeking = false;

    private Entry? _entry;

    public static readonly DirectProperty<VLCPlayer, Entry?> EntryProperty =
        AvaloniaProperty.RegisterDirect<VLCPlayer, Entry?>(nameof(Entry), o => o.Entry, (o, v) => o.Entry = v);

    public Entry? Entry
    {
        get => _entry;
        set => Task.Run(() => LoadAudio(value));
    }

    public VLCPlayer()
    {
        InitializeComponent();

        _context = new();
        _mediaPlayer = new(_context);
        _mediaPlayer.LengthChanged += MediaPlayer_LengthChanged;
        _mediaPlayer.TimeChanged += MediaPlayer_TimeChanged;
        _mediaPlayer.EndReached += MediaPlayer_EndReached;

        Thumb.DragCompletedEvent.AddClassHandler<VLCPlayer>((o, e) => o.OnThumbDragCompleted(e));
        Thumb.DragStartedEvent.AddClassHandler<VLCPlayer>((o, e) => o.OnThumbDragStarted(e));
        Button.IsCheckedChanged += Button_IsCheckedChanged;
        SeekBar.ValueChanged += SeekBar_ValueChanged;
    }

    private void MediaPlayer_EndReached(object? sender, EventArgs e)
    {
        Task.Run(() =>
        {
            _mediaPlayer.Stop();
            Dispatcher.UIThread.Post(() =>
            {
                SeekBar.Value = 0;
                Button.IsChecked = false;
            }, DispatcherPriority.Render);
        });
    }

    private void MediaPlayer_LengthChanged(object? sender, MediaPlayerLengthChangedEventArgs e)
    {
        Dispatcher.UIThread.Post(() => SeekBar.Maximum = _mediaPlayer.Length, DispatcherPriority.Render);
    }

    private void MediaPlayer_TimeChanged(object? sender, MediaPlayerTimeChangedEventArgs e)
    {
        Task.Run(() =>
        {
            if (_isLoading)
            {
                _mediaPlayer.Stop();
            }

            if (!_isSeeking)
            {
                Dispatcher.UIThread.Post(() =>
                {
                    _isSeeking = true;
                    SeekBar.Value = _mediaPlayer.Time;
                    _isSeeking = false;
                }, DispatcherPriority.Render);
            }
        });
        
    }
    private void Button_IsCheckedChanged(object? sender, RoutedEventArgs e)
    {
        if (e.Source is ToggleButton toggleButton)
        {
            if (toggleButton.IsChecked == true)
            {
                _mediaPlayer.Play();
            }
            else
            {
                _mediaPlayer.Pause();
            }
        }
    }
    private void SeekBar_ValueChanged(object? sender, RangeBaseValueChangedEventArgs e)
    {
        if (!_isSeeking && e.Source is Slider slider)
        {
            Seek(slider);
        }
    }
    private void OnThumbDragStarted(VectorEventArgs e)
    {
        _isSeeking = true;
    }
    private void OnThumbDragCompleted(VectorEventArgs e)
    {
        _isSeeking = false;
        if (e.Source is Thumb thumb && thumb.TemplatedParent is Slider slider)
        {
            Seek(slider);
        }
    }
    private void Seek(Slider slider)
    {
        double scale = _mediaPlayer.Length / (slider.Maximum - slider.Minimum);
        _mediaPlayer.Time = (long)(slider.Value * scale);
    }
    private void LoadAudio(Entry? entry)
    {
        if (entry == null) return;
        else if (entry.Type == EntryType.Bank)
        {
            Logger.Warning("Playing Bank type is not supported !!");
            return;
        }

        Logger.Info($"Attempting to load audio {entry.Location}");

        Dispatcher.UIThread.Post(() =>
        {
            _isLoading = true;
            Button.IsChecked = false;
        }, DispatcherPriority.Render);

        MemoryStream memoryStream = new();
        if (entry.TryConvert(memoryStream, out _))
        {
            _stream?.Dispose();
            _stream = memoryStream;

            _mediaPlayer.Media = new Media(_context, new StreamMediaInput(_stream));
            _mediaPlayer.Play();
            _entry = entry;

            Logger.Info($"{entry.Location} loaded successfully");

            Dispatcher.UIThread.Post(() =>
            {
                _isLoading = false;
                Button.IsChecked = true;
            }, DispatcherPriority.Render);
            return;
        }

        Logger.Info($"Unable to load {entry.Location}");
        return;
    }
    public void Dispose()
    {
        _stream?.Dispose();
        _mediaPlayer.Dispose();
        _context.Dispose();
        GC.SuppressFinalize(this);
    }
}
