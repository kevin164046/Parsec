﻿using Newtonsoft.Json;
using Parsec.Extensions;
using Parsec.Serialization;
using Parsec.Shaiya.Core;

namespace Parsec.Shaiya.Itm;

public sealed class Itm : FileBase
{
    /// <summary>
    /// File header (char[3]). "ITM" or "IT2"
    /// </summary>
    public string Header { get; set; } = string.Empty;

    public ItmFormat Format { get; set; }

    /// <summary>
    /// List of .3DO object names
    /// </summary>
    public List<string> MeshNames { get; set; } = new();

    /// <summary>
    ///  List of .dds texture names
    /// </summary>
    public List<string> TextureNames { get; set; } = new();

    /// <summary>
    /// List of ITM records
    /// </summary>
    public List<ItmRecord> Records { get; set; } = new();

    [JsonIgnore]
    public override string Extension => "ITM";

    protected override void Read(SBinaryReader binaryReader)
    {
        Header = binaryReader.ReadString(3);

        Format = Header switch
        {
            "ITM" => ItmFormat.ITM,
            "IT2" => ItmFormat.IT2,
            _     => ItmFormat.Unknown
        };

        // Records expect the format to be set on the serialization options ExtraOption property
        binaryReader.SerializationOptions.ExtraOption = Format;

        var meshNameCount = binaryReader.ReadInt32();
        for (var i = 0; i < meshNameCount; i++)
        {
            var meshName = binaryReader.ReadString();
            MeshNames.Add(meshName);
        }

        var textureNameCount = binaryReader.ReadInt32();
        for (var i = 0; i < textureNameCount; i++)
        {
            var textureName = binaryReader.ReadString();
            TextureNames.Add(textureName);
        }

        Records = binaryReader.ReadList<ItmRecord>().ToList();
    }

    protected override void Write(SBinaryWriter binaryWriter)
    {
        // Records expect the format to be set on the serialization options ExtraOption property
        binaryWriter.SerializationOptions.ExtraOption = Format;

        var signature = Format switch
        {
            ItmFormat.ITM => "ITM",
            ItmFormat.IT2 => "IT2",
            _             => "ITM"
        };

        binaryWriter.Write(signature, isLengthPrefixed: false, includeStringTerminator: false);

        binaryWriter.Write(MeshNames.Count);
        foreach (var meshName in MeshNames)
        {
            binaryWriter.Write(meshName);
        }

        binaryWriter.Write(TextureNames.Count);
        foreach (var textureName in TextureNames)
        {
            binaryWriter.Write(textureName);
        }

        binaryWriter.Write(Records.ToSerializable());
    }
}
