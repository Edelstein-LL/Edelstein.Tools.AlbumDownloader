using System.Runtime.Serialization;

namespace Edelstein.Data.Msts;

[Serializable]
public class AlbumUnitMMst : ISerializable
{
    public uint UnitId { get; init; }
    public uint UnitTypeId { get; init; }
    public uint AlbumSeriesId { get; init; }
    public required string Eponym { get; init; }
    public required string EponymEn { get; init; }
    public required string Name { get; init; }
    public required string NameEn { get; init; }
    public uint NormalCardId { get; init; }
    public uint RankMaxCardId { get; init; }
    public uint Rarity { get; init; }
    public uint AttributeId { get; init; }
    public uint MasterReleaseLabelId { get; init; }

    public AlbumUnitMMst() { }

    protected AlbumUnitMMst(SerializationInfo info, StreamingContext context)
    {
        UnitId = info.GetUInt32("_unitId");
        UnitTypeId = info.GetUInt32("_unitTypeId");
        AlbumSeriesId = info.GetUInt32("_albumSeriesId");
        Eponym = info.GetString("_eponym")!;
        EponymEn = info.GetString("_eponymEn")!;
        Name = info.GetString("_name")!;
        NameEn = info.GetString("_nameEn")!;
        NormalCardId = info.GetUInt32("_normalCardId");
        RankMaxCardId = info.GetUInt32("_rankMaxCardId");
        Rarity = info.GetUInt32("_rarity");
        AttributeId = info.GetUInt32("_attributeId");
        MasterReleaseLabelId = info.GetUInt32("_masterReleaseLabelId");
    }

    public void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        info.AddValue("_unitId", UnitId);
        info.AddValue("_unitTypeId", UnitTypeId);
        info.AddValue("_albumSeriesId", AlbumSeriesId);
        info.AddValue("_eponym", Eponym);
        info.AddValue("_eponymEn", EponymEn);
        info.AddValue("_name", Name);
        info.AddValue("_nameEn", NameEn);
        info.AddValue("_normalCardId", NormalCardId);
        info.AddValue("_rankMaxCardId", RankMaxCardId);
        info.AddValue("_rarity", Rarity);
        info.AddValue("_attributeId", AttributeId);
        info.AddValue("_masterReleaseLabelId", MasterReleaseLabelId);
    }
}
