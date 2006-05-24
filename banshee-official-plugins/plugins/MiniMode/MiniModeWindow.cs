using System;
using Gtk;
using Glade;

using Banshee.Base;
using Banshee.MediaEngine;
using Banshee.Widgets;
using Banshee.Plugins;

namespace Banshee.Plugins.MiniMode
{ 
    public class MiniMode
    {
        [Widget] private Gtk.Window MiniModeWindow = null;
        
        [Widget] private Gtk.Box SeekContainer;
        [Widget] private Gtk.Box VolumeContainer;
        [Widget] private Gtk.Box InfoBox;
        [Widget] private Gtk.Box SourceBox;
        [Widget] private Gtk.Box CoverBox;
        [Widget] private Gtk.Box PlaybackBox;
        [Widget] private Gtk.Box LowerButtonsBox;
        
        [Widget] private Gtk.Label TitleLabel;
        [Widget] private Gtk.Label AlbumLabel;
        [Widget] private Gtk.Label ArtistLabel;
        
        private CoverArtThumbnail coverArtThumbnail;
        private VolumeButton volumeButton;
        private SourceComboBox sourceComboBox;
        private SeekSlider seek_slider;
        private StreamPositionLabel stream_position_label;

        private Glade.XML glade;

        private bool setup = false;
        private bool miniMode = false;

        private bool Shown {
            get { return MiniModeWindow.Visible; }
            set { MiniModeWindow.Visible = value; }
        }

        public MiniMode()
        {
            glade = new Glade.XML(null, "minimode.glade", "MiniModeWindow", "banshee");
            glade.Autoconnect(this);

            IconThemeUtils.SetWindowIcon(MiniModeWindow);
            MiniModeWindow.DeleteEvent += delegate {
                Globals.ActionManager["QuitAction"].Activate();
            };
            
            // Playback Buttons
            ActionButton previous_button = new ActionButton(Globals.ActionManager["PreviousAction"]);
            previous_button.LabelVisible = false;
            previous_button.Padding = 1;
            
            ActionButton next_button = new ActionButton(Globals.ActionManager["NextAction"]);
            next_button.LabelVisible = false;
            next_button.Padding = 1;
            
            ActionButton playpause_button = new ActionButton(Globals.ActionManager["PlayPauseAction"]);
            playpause_button.LabelVisible = false;
            playpause_button.Padding = 1;
            
            PlaybackBox.PackStart(previous_button, false, false, 0);
            PlaybackBox.PackStart(playpause_button, false, false, 0);
            PlaybackBox.PackStart(next_button, false, false, 0);
            PlaybackBox.ShowAll();
            
            // Seek Slider/Position Label
            seek_slider = new SeekSlider();
            seek_slider.SetSizeRequest(125, -1);
            seek_slider.SeekRequested += delegate {
                PlayerEngineCore.Position = (uint)seek_slider.Value;
            };
            
            stream_position_label = new StreamPositionLabel(seek_slider);
            
            SeekContainer.PackStart(seek_slider, false, false, 0);
            SeekContainer.PackStart(stream_position_label, false, false, 0);
            SeekContainer.ShowAll();

            // Volume button
            volumeButton = new VolumeButton();
            VolumeContainer.PackStart(volumeButton, false, false, 0);
            volumeButton.Show();
            volumeButton.VolumeChanged += delegate(int volume) {
                PlayerEngineCore.Volume = (ushort)volume;
                Globals.Configuration.Set(GConfKeys.Volume, volume);
            };
            
            // Cover
            coverArtThumbnail = new CoverArtThumbnail(90);
            Gdk.Pixbuf default_pixbuf = Banshee.Base.IconThemeUtils.LoadIcon("audio-x-generic", 128);
            if(default_pixbuf == null) {
                default_pixbuf = new Gdk.Pixbuf(System.Reflection.Assembly.GetEntryAssembly(), 
                    "banshee-logo.png");   
            }
            coverArtThumbnail.NoArtworkPixbuf = default_pixbuf;
            CoverBox.PackStart(coverArtThumbnail, false, false, 0);

            // Source combobox
            sourceComboBox = new SourceComboBox();
            SourceBox.PackStart(sourceComboBox, true, true, 0);
            sourceComboBox.ShowAll();
            
            // Repeat/Shuffle buttons
            MultiStateToggleButton shuffle_toggle_button = new MultiStateToggleButton();
            shuffle_toggle_button.AddState(typeof(ShuffleDisabledToggleState),
                    Globals.ActionManager["ShuffleAction"] as ToggleAction);
            shuffle_toggle_button.AddState(typeof(ShuffleEnabledToggleState),
                    Globals.ActionManager["ShuffleAction"] as ToggleAction);
            shuffle_toggle_button.Relief = ReliefStyle.None;
            shuffle_toggle_button.ShowLabel = false;
            try {
				shuffle_toggle_button.ActiveStateIndex = (bool)Globals.Configuration.Get(GConfKeys.PlaylistShuffle) ? 1 : 0;
			} catch {
				shuffle_toggle_button.ActiveStateIndex = 0;
			}
            shuffle_toggle_button.ShowAll();
            
            MultiStateToggleButton repeat_toggle_button = new MultiStateToggleButton();
            repeat_toggle_button.AddState(typeof(RepeatNoneToggleState),
                Globals.ActionManager["RepeatNoneAction"] as ToggleAction);
            repeat_toggle_button.AddState(typeof(RepeatAllToggleState),
                Globals.ActionManager["RepeatAllAction"] as ToggleAction);
            repeat_toggle_button.AddState(typeof(RepeatSingleToggleState),
                Globals.ActionManager["RepeatSingleAction"] as ToggleAction);
            repeat_toggle_button.Relief = ReliefStyle.None;
            repeat_toggle_button.ShowLabel = false;
            try {
				repeat_toggle_button.ActiveStateIndex = (int)Globals.Configuration.Get(GConfKeys.PlaylistRepeat);
			} catch {
				repeat_toggle_button.ActiveStateIndex = 0;
			}
            repeat_toggle_button.ShowAll();
            
            LowerButtonsBox.PackEnd(repeat_toggle_button, false, false, 0);
            LowerButtonsBox.PackEnd(shuffle_toggle_button, false, false, 0);
            LowerButtonsBox.ShowAll();
            
            // Hook up everything
            PlayerEngineCore.EventChanged += OnPlayerEngineEventChanged;
            PlayerEngineCore.StateChanged += OnPlayerEngineStateChanged;
        }

        public void Show()
        {
            miniMode = true;
            if (Shown)
                return;
            if (!setup) {
                InterfaceElements.MainWindow.Shown += TrayIconWorkaround;
                setup = true;
            }
            sourceComboBox.UpdateActiveSource();
            UpdateMetaDisplay();
            InterfaceElements.MainWindow.Hide();
            Shown = true;
            volumeButton.Volume = PlayerEngineCore.Volume;
        }

        public void Hide()
        {
            miniMode = false;
            if (!Shown)
                return;
            Shown = false;
            InterfaceElements.MainWindow.Show();
        }
        
        public void Hide(object o, EventArgs a)
        {
            Hide();
        }

        private void TrayIconWorkaround(object o, EventArgs a)
        {
            // TODO: Do some clean work instead of this crap
            if (miniMode) {
                // If we're shown, then this is a hide event from the tray
                // If we're not, then this is a show event from the tray
                Shown = !Shown;
                // In all cases, hide the main window
                InterfaceElements.MainWindow.Hide();
            }
        }
        
        // ---- Player Event Handlers ----
        
        private void OnPlayerEngineStateChanged(object o, Banshee.MediaEngine.PlayerEngineStateArgs args)
        {
            switch(args.State) {
                case PlayerEngineState.Loaded:
                    seek_slider.Duration = PlayerEngineCore.CurrentTrack.Duration.TotalSeconds;
                    UpdateMetaDisplay();
                    break;
                case PlayerEngineState.Idle:
                    seek_slider.SetIdle();
                    InfoBox.Visible = false;
                    UpdateMetaDisplay();
                    break;
            }
        }
        
        private void OnPlayerEngineEventChanged(object o, PlayerEngineEventArgs args)
        {
            switch(args.Event) {
                case PlayerEngineEvent.Iterate:
                    OnPlayerEngineTick();
                    break;
                case PlayerEngineEvent.StartOfStream:
                    seek_slider.CanSeek = PlayerEngineCore.CanSeek;
                    break;
                case PlayerEngineEvent.Volume:
                    volumeButton.Volume = PlayerEngineCore.Volume;
                    break;
                case PlayerEngineEvent.Buffering:
                    if(args.BufferingPercent >= 1.0) {
                        stream_position_label.IsBuffering = false;
                        break;
                    }
                    
                    stream_position_label.IsBuffering = true;
                    stream_position_label.BufferingProgress = args.BufferingPercent;
                    break;
                case PlayerEngineEvent.Error:
                    UpdateMetaDisplay();
                    break;
                case PlayerEngineEvent.TrackInfoUpdated:
                    UpdateMetaDisplay();
                    break;
            }
        }

        private void OnPlayerEngineTick()
        {
            uint stream_length = PlayerEngineCore.Length;
            uint stream_position = PlayerEngineCore.Position;
            
            seek_slider.CanSeek = PlayerEngineCore.CanSeek;
            seek_slider.Duration = stream_length;
            seek_slider.SeekValue = stream_position;
        }
        
        private string EscapePangoMarkup(string str)
        {
            return str.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;");
        }
        
        public void UpdateMetaDisplay()
        {
            TrackInfo track = PlayerEngineCore.CurrentTrack;
            
            if(track == null) {
                MiniModeWindow.Title = Catalog.GetString("Banshee Music Player");
                InfoBox.Visible = false;
                return;
            }
            ArtistLabel.Markup = track.DisplayArtist;
            TitleLabel.Markup = String.Format("<big><b>{0}</b></big>", EscapePangoMarkup(track.DisplayTitle));
            AlbumLabel.Markup = String.Format("<i>{0}</i>", EscapePangoMarkup(track.DisplayAlbum));
            
            InfoBox.Visible = true;
            
            MiniModeWindow.Title = track.DisplayTitle + " (" + track.DisplayArtist + ")";
            
            try {
                coverArtThumbnail.FileName = track.CoverArtFileName;
                coverArtThumbnail.Label = track.DisplayArtist + " - " + track.DisplayAlbum;
            } catch(Exception) {
            }
        }
        
    }

    // TODO: Move to Banshee.Base in Core
    public class RepeatNoneToggleState : ToggleState
    {
        public RepeatNoneToggleState()
        {
            Icon = new Gdk.Pixbuf(System.Reflection.Assembly.GetEntryAssembly(), 
                "media-repeat-none.png");
            Label = Catalog.GetString("Repeat None");
        }
    }

    public class RepeatSingleToggleState : ToggleState
    {
        public RepeatSingleToggleState()
        {
            Icon = new Gdk.Pixbuf(System.Reflection.Assembly.GetEntryAssembly(), 
                "media-repeat-single.png");
            Label = Catalog.GetString("Repeat Single");
        }
    }

    public class RepeatAllToggleState : ToggleState
    {
        public RepeatAllToggleState()
        {
            Icon = new Gdk.Pixbuf(System.Reflection.Assembly.GetEntryAssembly(), 
                "media-repeat-all.png");
            Label = Catalog.GetString("Repeat All");
        }
    }
    
    public class ShuffleEnabledToggleState : ToggleState
    {
        public ShuffleEnabledToggleState()
        {
            Icon = new Gdk.Pixbuf(System.Reflection.Assembly.GetEntryAssembly(), 
                "media-playlist-shuffle.png");
            Label = Catalog.GetString("Shuffle");
            MatchActive = true;
            MatchValue = true;
        }
    }
    
    public class ShuffleDisabledToggleState : ToggleState
    {
        public ShuffleDisabledToggleState()
        {
            Icon = new Gdk.Pixbuf(System.Reflection.Assembly.GetEntryAssembly(), 
                "media-playlist-continuous.png");
            Label = Catalog.GetString("Continuous");
            MatchActive = true;
            MatchValue = false;
        }
    }
}
