﻿using OpenLyricsClient.Shared.Plugin;
using OpenLyricsClient.Shared.Structure.Song;
using Org.BouncyCastle.Utilities;
using System.Linq;
using System.Threading.Tasks;

namespace OpenLyricsClient.Backend.Collector.Song.Providers.Plugin
{
    internal class PluginSongCollector : ISongCollector
    {
        public string CollectorName()
        {
            return "Plugin";
        }

        async public Task<SongResponseObject> GetSong(SongRequestObject songRequestObject)
        {
            SongResponseObject collectedData = null;
            foreach (IPlugin plugin in Core.INSTANCE.PluginManager.GetPluginsByScope(PluginScope.SongCollector).OrderByDescending((IPlugin plugin) => plugin.GetCollectedSongQuality()))
            {
                SongResponseObject? data = await plugin.CollectSong(songRequestObject);
                if (data != null)
                {
                    collectedData = data;
                    break;
                }
            }
            return collectedData;
        }

        public int ProviderQuality()
        {
            IPlugin? plugin = Core.INSTANCE.PluginManager.GetPluginsByScope(PluginScope.SongCollector).MaxBy((IPlugin plugin) => plugin.GetCollectedSongQuality());
            if (plugin == null)
                return -1;
            return plugin.GetCollectedSongQuality();
        }
    }
}
