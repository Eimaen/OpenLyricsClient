﻿using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Markup.Xaml;
using DevBase.Async.Task;
using OpenLyricsClient.Frontend.View.Windows;
using LyricsScroller = OpenLyricsClient.Frontend.View.Custom.LyricsScroller;

namespace OpenLyricsClient.Frontend.View.Pages;

public partial class LyricsPage : UserControl
{
    private TextBlock _txtTimeFrom;
    private TextBlock _txtTimeTo;
    private LyricsScroller _cstmLyricsDisplay;
    
    private TaskSuspensionToken _displayLyricsSuspensionToken;
    private TaskSuspensionToken _syncLyricsSuspensionToken;

    public LyricsPage()
    {
        InitializeComponent();

        this._txtTimeFrom = this.Get<TextBlock>(nameof(TXT_TimeFrom));
        this._txtTimeTo = this.Get<TextBlock>(nameof(TXT_TimeTo));
        this._cstmLyricsDisplay = this.Get<LyricsScroller>(nameof(LRC_Display));
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void InputElement_OnPointerMoved(object? sender, PointerEventArgs e)
    {
        this._txtTimeFrom.Opacity = 1;
        this._txtTimeTo.Opacity = 1;
    }

    private void InputElement_OnPointerLeave(object? sender, PointerEventArgs e)
    {
        this._txtTimeFrom.Opacity = 0;
        this._txtTimeTo.Opacity = 0;
    }

    private void InputElement_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        MainWindow.Instance.BeginMoveDrag(e);
    }

    private void Layoutable_OnEffectiveViewportChanged(object? sender, EffectiveViewportChangedEventArgs e)
    {
        if (this._cstmLyricsDisplay != null)
        {
            this._cstmLyricsDisplay.Reload();
        }
    }

    private void Button_OnClick(object? sender, RoutedEventArgs e)
    {
        if (this._cstmLyricsDisplay != null)
        {
            this._cstmLyricsDisplay.ResyncOffset();
        }
    }
}