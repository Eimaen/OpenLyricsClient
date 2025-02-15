﻿using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using DevBase.Api.Apis.OpenLyricsClient.Structure.Json;
using DevBase.Format.Structure;
using DevBase.Generics;
using OpenLyricsClient.Shared.Structure.Lyrics;
using OpenLyricsClient.Shared.Structure.Song;
using OpenLyricsClient.Shared.Utils;

namespace OpenLyricsClient.Backend.Collector.Lyrics.Providers.OpenLyricsClient;

public class OpenLyricsClientCollector : ILyricsCollector
{
    private DevBase.Api.Apis.OpenLyricsClient.OpenLyricsClient _openLyricsClient;

    private (SongRequestObject, JsonOpenLyricsClientAiSyncItem[]) _lastRequest;
    
    public OpenLyricsClientCollector()
    {
        this._openLyricsClient = new DevBase.Api.Apis.OpenLyricsClient.OpenLyricsClient(
            "MIIBSzCCAQMGByqGSM49AgEwgfcCAQEwLAYHKoZIzj0BAQIhAP////8AAAABAAAAAAAAAAAAAAAA////////////////MFsEIP////8AAAABAAAAAAAAAAAAAAAA///////////////8BCBaxjXYqjqT57PrvVV2mIa8ZR0GsMxTsPY7zjw+J9JgSwMVAMSdNgiG5wSTamZ44ROdJreBn36QBEEEaxfR8uEsQkf4vOblY6RA8ncDfYEt6zOg9KE5RdiYwpZP40Li/hp/m47n60p8D54WK84zV2sxXs7LtkBoN79R9QIhAP////8AAAAA//////////+85vqtpxeehPO5ysL8YyVRAgEBA0IABBqSdbiWAMxcEig+rX1FlApI7pE/kPNUmejo5PXvElsf6pjHuDlBN4fYvpmaX6lncddAuNPnQmZ89Ogb95xwPnA=");
    }
    
    public async Task<LyricData> GetLyrics(SongResponseObject songResponseObject)
    {
        if (!DataValidator.ValidateData(songResponseObject))
            return new LyricData();

        if (!DataValidator.ValidateData(songResponseObject.Track))
            return new LyricData();

        if (Core.INSTANCE.CacheManager.IsLyricsInCache(songResponseObject.SongRequestObject))
            return new LyricData();
        
        JsonOpenLyricsClientSubscription subscription = new JsonOpenLyricsClientSubscription
        {
            UserID = Core.INSTANCE.SettingsHandler.Settings<AccountSection>()!.GetValue<string>("UserID"),
            UserSecret = Core.INSTANCE.SettingsHandler.Settings<AccountSection>()!.GetValue<string>("UserSecret")
        };

        SongMetadata metadata = songResponseObject.SongRequestObject.Song.SongMetadata;

        JsonOpenLyricsClientAiSyncItem[] syncedItems;

        if (_lastRequest.Item1 == songResponseObject.SongRequestObject)
        {
            syncedItems = _lastRequest.Item2;
        }
        else
        {
            syncedItems = await this._openLyricsClient.AiSync(
                subscription,
                metadata.Name,
                metadata.Album,
                metadata.MaxTime,
                "large-v2",
                metadata.Artists);

            _lastRequest = (songResponseObject.SongRequestObject, syncedItems);
        }

        return await LyricData.ConvertToData(ToLyircElements(syncedItems),
            songResponseObject.SongRequestObject.Song.SongMetadata, this.CollectorName());
    }

    private AList<LyricElement> ToLyircElements(JsonOpenLyricsClientAiSyncItem[] items)
    {
        AList<LyricElement> elements = new AList<LyricElement>();

        for (int i = 0; i < items.Length; i++)
        {
            JsonOpenLyricsClientAiSyncItem item = items[i];
            elements.Add(new LyricElement((long)item.startTimestamp, item.text));
        }

        return elements;
    }

    public string CollectorName()
    {
        return "OpenLyricsClient";
    }

    // It should be the last provider so save some cost at my endpoint
    public int ProviderQuality()
    {
        return 0;
    }
}