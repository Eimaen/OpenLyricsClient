﻿using Newtonsoft.Json;

namespace OpenLyricsClient.Backend.Collector.Lyrics.Providers.NetEase.Json
{
    //Generated by https://json2csharp.com/ i love you
    public class NetEaseSearchResponse
    {
        [JsonProperty("result")]
        public NetEaseResultResponse NetEaseResultDataResponse { get; set; }

        [JsonProperty("code")]
        public int Code { get; set; }
    }
}
