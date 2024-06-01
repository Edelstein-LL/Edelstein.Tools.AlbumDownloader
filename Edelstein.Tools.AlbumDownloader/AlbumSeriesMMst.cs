using System.Runtime.Serialization;

namespace Edelstein.Data.Msts;

[Serializable]
public class AlbumSeriesMMst : ISerializable
{
    public uint AlbumSeriesId { get; init; }
    public uint AlbumGroupId { get; init; }
    public uint AlbumTabId { get; init; }
    public uint OrderNum { get; init; }
    public required string Name { get; init; }
    public required string NameEn { get; init; }
    public uint LayoutType { get; init; }
    public required string ThumbnailPath { get; init; }
    public uint MasterReleaseLabelId { get; init; }

    public AlbumSeriesMMst() { }

    protected AlbumSeriesMMst(SerializationInfo info, StreamingContext context)
    {
        AlbumSeriesId = info.GetUInt32("_albumSeriesId");
        AlbumGroupId = info.GetUInt32("_albumGroupId");
        AlbumTabId = info.GetUInt32("_albumTabId");
        OrderNum = info.GetUInt32("_orderNum");
        Name = info.GetString("_name")!;
        NameEn = info.GetString("_nameEn")!;
        LayoutType = info.GetUInt32("_layoutType");
        ThumbnailPath = info.GetString("_thumbnailPath")!;
        MasterReleaseLabelId = info.GetUInt32("_masterReleaseLabelId");
    }

    public void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        info.AddValue("_albumSeriesId", AlbumSeriesId);
        info.AddValue("_albumGroupId", AlbumGroupId);
        info.AddValue("_albumTabId", AlbumTabId);
        info.AddValue("_orderNum", OrderNum);
        info.AddValue("_name", Name);
        info.AddValue("_nameEn", NameEn);
        info.AddValue("_layoutType", LayoutType);
        info.AddValue("_thumbnailPath", ThumbnailPath);
        info.AddValue("_masterReleaseLabelId", MasterReleaseLabelId);
    }
}
