namespace CCTV.Application.Common.Options;

public class DatabaseOptions
{
    public const string SectionName = "Database";

    public bool RunSeeder { get; set; } = true;
}
