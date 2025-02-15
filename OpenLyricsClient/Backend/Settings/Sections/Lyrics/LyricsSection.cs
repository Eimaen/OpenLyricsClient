﻿using System;
using System.IO;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Layout;
using Avalonia.Media;
using DevBase.Api.Serializer;
using DevBase.Generics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenLyricsClient.Shared.Structure.Enum;
using OpenLyricsClient.Shared.Structure.Visual;
using OpenLyricsClient.Shared.Utils;

namespace OpenLyricsClient.Backend.Settings.Sections.Lyrics;

public class LyricsSection : ISettingSection
{
    private FileInfo _file;
    private JObject _data;

    public LyricsSection(string filePath)
    {
        this._file = new FileInfo(filePath);
    }
    
    public async Task WriteToDisk()
    {
        await File.WriteAllTextAsync(this._file.FullName, this._data.ToString());
    }

    public async Task ReadFromDisk()
    {
        if (!this._file.Exists)
        {
            this._data = Defaults();
            await WriteToDisk();
            return;
        }

        await using FileStream stream = this._file.OpenRead();
        using StreamReader reader = new StreamReader(stream);

        this._data = JObject.Parse(reader.ReadToEnd());
        
        await stream.FlushAsync();
        
        stream.Close();
        reader.Close();
    }

    public T GetValue<T>(string field)
    {
        return (T)this._data[field].ToObject<T>();
    }

    public async Task SetValue<T>(string field, T value)
    {
        this._data[field] = JToken.FromObject(value);
        await WriteToDisk();
    }

    public JObject Defaults()
    {
        Structure structure = new Structure
        {
            Selection = EnumLyricsDisplayMode.KARAOKE,
            ArtworkBackground = false,
            LyricsBlur = false,
            LyricsAlignment = TextAlignment.Left,
            LyricsMargin = new Margin(0,0,0,70),
            LyricsWeight = FontWeight.Bold,
            LyricsSize = 30
        };
        
        return new JsonDeserializer().Serialize(structure);
    }
    
    public string[] GetFields()
    {
        AList<string> fields = new AList<string>();

        foreach (var pair in this._data)
            fields.Add(pair.Key);

        return fields.GetAsArray();
    }
}