﻿using System;
using System.Net;
using System.Threading.Tasks;
using DevBase.Generics;
using DevBase.Web;
using DevBase.Web.ResponseData;
using OpenLyricsClient.Backend.Debugger;
using OpenLyricsClient.Backend.Handler.Song;
using OpenLyricsClient.Shared.Structure.Enum;
using OpenLyricsClient.Shared.Structure.Json.NetEaseV2.Json;
using OpenLyricsClient.Shared.Structure.Song;
using OpenLyricsClient.Shared.Utils;
using OpenLyricsClient.Shared.Utils.Formatting;

namespace OpenLyricsClient.Backend.Collector.Song.Providers.NetEaseV2
{
    class NetEaseV2Collector : ISongCollector
    {
        private readonly string _baseUrl;

        private Debugger<NetEaseV2Collector> _debugger;

        private const int RETRIES = 5;
        private const double RETRY_DURATION_MULTIPLIER = 15f;

        public NetEaseV2Collector()
        {
            this._debugger = new Debugger<NetEaseV2Collector>(this);
            this._baseUrl = "https://music.xianqiao.wang/neteaseapiv2";
        }

        public async Task<SongResponseObject> GetSong(SongRequestObject songRequestObject)
        {
            if (!(DataValidator.ValidateData(songRequestObject)))
                return null;

            if (!(DataValidator.ValidateData(songRequestObject.Artists, songRequestObject.SongDuration,
                    songRequestObject.SongName, songRequestObject.Album)))
                return null;
            
            NetEaseV2SearchResponse response = await SearchTrack(songRequestObject);

            if (!(DataValidator.ValidateData(response)))
                return null;

            if (!DataValidator.ValidateData(response.Code, response.Result))
                return null;

            if (response.Result.Songs.Length <= 0)
                return null;
            
            this._debugger.Write("Found " + response.Result.Songs.Length + " songs!", DebugType.INFO);
            
            double retryPercentage = 5;

            AList<NetEaseV2SongResponse> songs = new AList<NetEaseV2SongResponse>();

            for (int i = 0; i < RETRIES; i++)
            {
                for (int j = 0; j < response.Result.Songs.Length; j++)
                {
                    NetEaseV2SongResponse songResponse = response.Result.Songs[j];

                    if (IsValidSong(songResponse, songRequestObject, retryPercentage))
                    {
                        songs.Add(songResponse);
                    }

                    retryPercentage += (int)Math.Ceiling(i * RETRY_DURATION_MULTIPLIER);
                }
            }

            if (songs.Length > 0)
            {
                SongResponseObject songResponseObject = new SongResponseObject()
                {
                    SongRequestObject = songRequestObject,
                    Track = songs,
                    CollectorName = this.CollectorName()
                };

                this._debugger.Write("Got current song " + songRequestObject.SongName + "!", DebugType.INFO);

                return songResponseObject;
            }

            return null;
        }
        
        private bool IsValidSong(NetEaseV2SongResponse songResponse, SongRequestObject songRequestObject, double percentage)
        {
            if (!DataValidator.ValidateData(songResponse) || 
                  !DataValidator.ValidateData(songRequestObject))
                return false;

            if (IsSimilar(songRequestObject.FormattedSongName, songResponse.Name) != IsSimilar(songRequestObject.FormattedSongAlbum, songResponse.Album.Name))
            {
                if (!IsSimilar(songRequestObject.FormattedSongAlbum, songResponse.Album.Name))
                    return false;
            }

            if (!MatchDuration(songResponse, songRequestObject.SongDuration, percentage))
                return false;

            if (!MatchArtists(songResponse, songRequestObject.Artists, 100))
                return false;

            if (!IsSimilar(songRequestObject.FormattedSongName, songResponse.Name))
                return false;

            return true;
        }

        //Untested it should make everything a bit more strict
        private bool IsSimilar(string string1, string string2)
        {
            return MathUtils.CalculateLevenshteinDistance(string1, string2) >=
                   Math.Abs(string1.Length - string2.Length);
        }

        private async Task<NetEaseV2SearchResponse> SearchTrack(SongRequestObject songRequestObject)
        {
            string requestUrl = Uri.EscapeUriString(string.Format("{0}/search?limit=50&type=1&keywords={2}",
                this._baseUrl,
                songRequestObject.GetArtistsSplit(), songRequestObject.FormattedSongName));

            //this._debugger.Write("Full track search URL: " + requestUrl, DebugType.DEBUG);
            DevBase.Web.Request request = new Request(requestUrl);
            ResponseData responseData = await request.GetResponseAsync();

            if (responseData.StatusCode == HttpStatusCode.OK)
            { 
                return new JsonDeserializer().Deserialize<NetEaseV2SearchResponse>(responseData.GetContentAsString());
            }

            return null;
        }
        
        private bool MatchDuration(NetEaseV2SongResponse netEaseSongResponse, long duration, double percentage)
        {
            long songDurationThreshold = (long)((duration * 0.01) * percentage);
            return MathUtils.IsInRange(duration - songDurationThreshold, duration + songDurationThreshold, netEaseSongResponse.Duration);
        }

        private bool MatchArtists(NetEaseV2SongResponse netEaseSongResponse, string[] artists, double percentage)
        {
            double minArtistCount = Math.Floor((artists.Length * 0.01) * percentage);
            int artistsMatch = 0;

            if (artists.Length > 0)
            {
                for (int i = 0; i < netEaseSongResponse.Artists.Length; i++)
                {
                    for (int j = 0; j < artists.Length; j++)
                    {
                        string artist = SongFormatter.FormatString(netEaseSongResponse.Artists[i].Name);

                        if (MathUtils.CalculateLevenshteinDistance(artist, artists[j]) <= Math.Abs(artist.Length - artists[j].Length))
                        {
                            artistsMatch++;
                        } 
                        else if (MathUtils.CalculateLevenshteinDistance(artist, artists[j]) <= 5)
                        {
                            artistsMatch++;
                        }
                    }
                }
            }
            else
            {
                for (int i = 0; i < netEaseSongResponse.Artists.Length; i++)
                {
                    string artist = SongFormatter.FormatString(netEaseSongResponse.Artists[i].Name);

                    if (MathUtils.CalculateLevenshteinDistance(artist, artists[0]) <=  Math.Abs(artist.Length - artists[0].Length))
                    {
                        artistsMatch++;
                    }
                    else if (MathUtils.CalculateLevenshteinDistance(artist, artists[0]) <= 5)
                    {
                        artistsMatch++;
                    }
                }
            }

            return artistsMatch >= minArtistCount;
        }

        public string CollectorName()
        {
            return "NetEaseV2";
        }

        public int ProviderQuality()
        {
            return 6;
        }
    }
}
