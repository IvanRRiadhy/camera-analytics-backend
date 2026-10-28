using CCTV.Contracts.Common;

namespace CCTV.Contracts.Spatial;

public class RoomFilterRequest : BaseFilterRequest
{
    public Guid? FloorId { get; set; }
    public bool? IsActive { get; set; }
}
