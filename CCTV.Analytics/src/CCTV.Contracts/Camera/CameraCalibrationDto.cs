namespace CCTV.Contracts.Camera;

public record CameraCalibrationDto
{
    public string? HomographyMatrixJson { get; init; }
    public string? GroundPlaneOriginJson { get; init; }
    public decimal? ScaleFactorMetersPerPixel { get; init; }
    public string? PtzPresetCoordinatesJson { get; init; }
}

public record ZoneDefinitionDto
{
    public Guid ZoneId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string ZoneType { get; init; } = "Tripwire";
    public string PolygonCoordinatesJson { get; init; } = "[]";
    public string ColorHex { get; init; } = "#FF0000";
}
