﻿namespace OpenLyricsClient.Backend.Structure.Enum
{
    public enum EnumRegisterTypes
    {
        WINDOW_LOGGER, 
        SONG_PROVIDER_CHOOSER,
        SONGHANDLER_MANAGECURRENTSONG, SONGHANDLER_SONGINFORMATION,
        SPOTIFYSONGPROVIDER_UPDATEPLAYBACK, SPOTIFYSONGPROVIDER_UPDATESONGDATA, SPOTIFYSONGPROVIDER_TIMESYNC,
        TIDALSONGPROVIDER_LOGIN, TIDALSONGPROVIDER_UPDATECURRENTTRACK, TIDALSONGPROVIDER_UPDATETIME,
        SPOTIFY_REFRESHTOKEN,
        TIDAL_REFRESHTOKEN, TIDALPROGRESSLISTENER_FINDADDRESS,
        SHOW_LYRICS, SYNC_LYRICS, SHOW_FULLLYRICS, SHOW_INFOS, SHOW_PROGRESS, SYNC_LYRICS_PERCENTAGE,
        MANAGE_LYRICS, APPLY_LYRICS_TO_SONG, 
        COLLECT_TOKENS,
        SYNC_SCROLLER
    }
}
