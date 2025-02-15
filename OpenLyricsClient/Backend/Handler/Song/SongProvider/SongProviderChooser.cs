﻿using System;
using System.Threading;
using System.Threading.Tasks;
using DevBase.Async.Task;
using DevBase.Generics;
using DevBase.Typography;
using OpenLyricsClient.Backend.Debugger;
using OpenLyricsClient.Backend.Handler.Services.Services;
using OpenLyricsClient.Shared.Structure.Enum;
using OpenLyricsClient.Shared.Utils;

namespace OpenLyricsClient.Backend.Handler.Song.SongProvider
{
    class SongProviderChooser
    {
        private EnumSongProvider _currentSongProvider;

        private TaskSuspensionToken _taskSuspensionToken;
        private bool _disposed;

        private Debugger<SongProviderChooser> _debugger;

        private readonly object[] _spotifyTypes = new object[]
        {
            EnumRegisterTypes.SPOTIFYSONGPROVIDER_UPDATEPLAYBACK,
            EnumRegisterTypes.SPOTIFYSONGPROVIDER_UPDATESONGDATA,
            EnumRegisterTypes.SPOTIFYSONGPROVIDER_TIMESYNC,
            EnumRegisterTypes.SPOTIFY_REFRESHTOKEN
        };

        private readonly object[] _tidalTypes = new object[]
        {
            EnumRegisterTypes.TIDALSONGPROVIDER_LOGIN,
            EnumRegisterTypes.TIDALSONGPROVIDER_UPDATECURRENTTRACK,
            EnumRegisterTypes.TIDALSONGPROVIDER_UPDATETIME,
            EnumRegisterTypes.TIDAL_REFRESHTOKEN,
            EnumRegisterTypes.TIDALPROGRESSLISTENER_FINDADDRESS
        };

        public SongProviderChooser()
        {
            this._debugger = new Debugger<SongProviderChooser>(this);
            this._currentSongProvider = EnumSongProvider.NONE;
            Core.INSTANCE.SlowTickHandler += OnTickHandler;
        }

        private void OnTickHandler(object sender)
        {
            if (!DataValidator.ValidateData(Core.INSTANCE.WindowLogger))
                return;

            EnumSongProvider songProvider = EnumSongProvider.NONE;

            IService spotifyService = Core.INSTANCE.ServiceHandler.GetServiceByName("Spotify");
            IService tidalService = Core.INSTANCE.ServiceHandler.GetServiceByName("Tidal");

            if (IsInUse(tidalService))
                songProvider = EnumSongProvider.TIDAL;

            if (IsInUse(spotifyService))
                songProvider = EnumSongProvider.SPOTIFY;

            if (songProvider == EnumSongProvider.NONE)
                return;

            if (!songProvider.Equals(this._currentSongProvider))
            {
                this._debugger.Write("SongProvider has been changed to: " + new AString(songProvider.ToString()).CapitalizeFirst(), DebugType.INFO);
                this._currentSongProvider = songProvider;

                if (songProvider == EnumSongProvider.SPOTIFY)
                {
                    ResumeProvider(EnumSongProvider.SPOTIFY);
                    SuspendProvider(EnumSongProvider.TIDAL);
                }
                else if (songProvider == EnumSongProvider.TIDAL)
                {
                    ResumeProvider(EnumSongProvider.TIDAL);
                    SuspendProvider(EnumSongProvider.SPOTIFY);
                }
            }
        }

        // private async Task SongProviderChooserTask()
        // {
        //     while (!this._disposed)
        //     {
        //         await this._taskSuspensionToken.WaitForRelease();
        //
        //         if (!DataValidator.ValidateData(Core.INSTANCE.WindowLogger))
        //             continue;
        //
        //         EnumSongProvider songProvider = EnumSongProvider.NONE;
        //
        //         IService spotifyService = Core.INSTANCE.ServiceHandler.GetServiceByName("Spotify");
        //         IService tidalService = Core.INSTANCE.ServiceHandler.GetServiceByName("Tidal");
        //
        //         if (await IsInUse(tidalService))
        //             songProvider = EnumSongProvider.TIDAL;
        //
        //         if (await IsInUse(spotifyService))
        //             songProvider = EnumSongProvider.SPOTIFY;
        //
        //         if (songProvider == EnumSongProvider.NONE)
        //             continue;
        //
        //         if (!songProvider.Equals(this._currentSongProvider))
        //         {
        //             this._debugger.Write("SongProvider has been changed to: " + new AString(songProvider.ToString()).CapitalizeFirst(), DebugType.INFO);
        //             this._currentSongProvider = songProvider;
        //
        //             if (songProvider == EnumSongProvider.SPOTIFY)
        //             {
        //                 ResumeProvider(EnumSongProvider.SPOTIFY);
        //                 SuspendProvider(EnumSongProvider.TIDAL);
        //             }
        //             else if (songProvider == EnumSongProvider.TIDAL)
        //             {
        //                 ResumeProvider(EnumSongProvider.TIDAL);
        //                 SuspendProvider(EnumSongProvider.SPOTIFY);
        //             }
        //         }
        //
        //         await Task.Delay(3000);
        //     }
        // }

        private bool IsInUse(IService service)
        {
            if (!DataValidator.ValidateData(service))
                return false;
            
            if (!service.IsConnected())
                return false;

            AList<string> lastWindows = Core.INSTANCE.WindowLogger.LastWindows(service.ProcessName());
            AList<string> foundProcesses = ProcessUtils.GetRunningProcesses(service.ProcessName());

            if (!lastWindows.IsEmpty())
                if (lastWindows.Get(0).Equals(service.ProcessName()))
                    return true;

            if (!foundProcesses.IsEmpty())
                if (foundProcesses.Get(0).Equals(service.ProcessName()))
                    return true;

            if (service.TestConnection().GetAwaiter().GetResult())
                return true;

            return false;
        }

        private void ResumeProvider(EnumSongProvider songProvider)
        {
            try
            {
                switch (songProvider)
                {
                    case EnumSongProvider.SPOTIFY:
                    {
                        Core.INSTANCE.TaskRegister.ResumeByArray(this._spotifyTypes);
                        break;
                    }
                    case EnumSongProvider.TIDAL:
                    {
                        Core.INSTANCE.TaskRegister.ResumeByArray(this._tidalTypes);
                        break;
                    }
                }
            }
            catch (Exception e) { }
        }

        private void SuspendProvider(EnumSongProvider songProvider)
        {
            try
            {
                switch (songProvider)
                {
                    case EnumSongProvider.SPOTIFY:
                    {
                        Core.INSTANCE.TaskRegister.SuspendByArray(this._spotifyTypes);
                        break;
                    }
                    case EnumSongProvider.TIDAL:
                    {
                        Core.INSTANCE.TaskRegister.SuspendByArray(this._tidalTypes);
                        break;
                    }
                }
            }
            catch (Exception e) { }
        }

        public EnumSongProvider GetSongProvider()
        {
            return this._currentSongProvider;
        }

        public void Dispose()
        {
            this._disposed = true;

            Core.INSTANCE.TaskRegister.Kill(EnumRegisterTypes.SONG_PROVIDER_CHOOSER);
        }
    }
}
