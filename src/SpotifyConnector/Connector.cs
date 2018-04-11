﻿using AudioBand.Connector;
using SpotifyAPI.Local;
using SpotifyAPI.Local.Enums;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Timer = System.Timers.Timer;

namespace SpotifyConnector
{
    public class Connector : IAudioConnector
    {
        public string ConnectorName { get; } = "Spotify";

        public event EventHandler<TrackInfoChangedEventArgs> TrackInfoChanged;
        public event EventHandler TrackPlaying;
        public event EventHandler TrackPaused;
        public event EventHandler<double> TrackProgressChanged;

        private SpotifyLocalAPI _spotifyClient;
        private int _trackLength;
        private Timer _checkForSpotifytimer;
        private bool _spotifyStarted;

        public Task ActivateAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            _spotifyClient = new SpotifyLocalAPI();

            _checkForSpotifytimer = new Timer
            {
                Interval = 1000,
                AutoReset = false
            };
            _checkForSpotifytimer.Elapsed += CheckForSpotifytimerOnElapsed;

            try
            {
                if (SpotifyRunning())
                {
                    Connect();
                    _spotifyStarted = true;
                    return Task.CompletedTask;
                }

                ResetState();

                return Task.CompletedTask;
            }
            finally
            {
                _checkForSpotifytimer.Start();
            }
        }

        private void CheckForSpotifytimerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            try
            {
                // If spotify was started and not anymore
                if (_spotifyStarted && !SpotifyRunning())
                {
                    _spotifyStarted = false;
                    ResetState();
                    return;
                }

                if (_spotifyStarted)
                {
                    return;
                }

                if (!SpotifyRunning())
                {
                    return;
                }

                // Only try to connect once
                _spotifyClient = new SpotifyLocalAPI();
                Connect();
                _spotifyStarted = true;
            }
            catch (Exception)
            {
                // Random http 403 errors
            }
            finally
            {
                _checkForSpotifytimer.Enabled = true;
            }
        }

        public Task DeactivateAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            _spotifyClient?.Dispose();
            if (_checkForSpotifytimer != null)
            {
                _checkForSpotifytimer.Elapsed -= CheckForSpotifytimerOnElapsed;
                _checkForSpotifytimer.Dispose();
            }

            return Task.CompletedTask;
        }

        public async Task PlayTrackAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            await _spotifyClient.Play().ConfigureAwait(false);
        }

        public async Task PauseTrackAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            await _spotifyClient.Pause().ConfigureAwait(false);
        }

        public Task PreviousTrackAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            _spotifyClient.Previous();
            return Task.CompletedTask;
        }

        public Task NextTrackAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            _spotifyClient.Skip();
            return Task.CompletedTask;
        }

        private void SpotifyClientOnOnTrackTimeChange(object sender, TrackTimeChangeEventArgs trackTimeChangeEventArgs)
        {
            TrackProgressChanged?.Invoke(this, CalculateTrackPercentange(trackTimeChangeEventArgs.TrackTime));
        }

        private void SpotifyClientOnOnTrackChange(object sender, TrackChangeEventArgs trackChangeEventArgs)
        {
            var track = trackChangeEventArgs.NewTrack;
            _trackLength = track.Length;
            TrackInfoChanged?.Invoke(this, new TrackInfoChangedEventArgs
            {
                TrackName = track.TrackResource.Name,
                Artist = track.ArtistResource.Name,
                AlbumArt = track.GetAlbumArt(AlbumArtSize.Size640)
            });
        }

        private void SpotifyClientOnOnPlayStateChange(object sender, PlayStateEventArgs playStateEventArgs)
        {
            if (playStateEventArgs.Playing)
            {
                TrackPlaying?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                TrackPaused?.Invoke(this, EventArgs.Empty);
            }
        }

        private double CalculateTrackPercentange(double trackTime)
        {
            return trackTime / _trackLength * 100;
        }

        private void Connect()
        {
            _spotifyClient.Connect();
            _spotifyClient.ListenForEvents = true;

            var status = _spotifyClient.GetStatus();
            var track = status.Track;
            _trackLength = track.Length;

            TrackProgressChanged?.Invoke(this, CalculateTrackPercentange(status.PlayingPosition));
            TrackInfoChanged?.Invoke(this, new TrackInfoChangedEventArgs
            {
                TrackName = track.TrackResource.Name,
                Artist = track.ArtistResource.Name,
                AlbumArt = track.GetAlbumArt(AlbumArtSize.Size640)
            });

            if (status.Playing)
            {
                TrackPlaying?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                TrackPaused?.Invoke(this, EventArgs.Empty);
            }

            _spotifyClient.OnPlayStateChange += SpotifyClientOnOnPlayStateChange;
            _spotifyClient.OnTrackChange += SpotifyClientOnOnTrackChange;
            _spotifyClient.OnTrackTimeChange += SpotifyClientOnOnTrackTimeChange;
        }

        private bool SpotifyRunning()
        {
            return SpotifyLocalAPI.IsSpotifyRunning() && SpotifyLocalAPI.IsSpotifyWebHelperRunning();
        }

        private void ResetState()
        {
            TrackInfoChanged?.Invoke(this, new TrackInfoChangedEventArgs { TrackName = "", AlbumArt = null });
            TrackPaused?.Invoke(this, EventArgs.Empty);
            TrackProgressChanged?.Invoke(this, 0);
        }

        ~Connector()
        {
            if (_checkForSpotifytimer != null)
            {
                _checkForSpotifytimer.Elapsed -= CheckForSpotifytimerOnElapsed;
                _checkForSpotifytimer.Dispose();
            }
        }
    }
}
