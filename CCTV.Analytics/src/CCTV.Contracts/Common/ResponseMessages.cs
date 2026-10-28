namespace CCTV.Contracts.Common;

public static class ResponseMessages
{
    // General / System
    public const string Success = "Operation completed successfully.";
    public const string DatabaseError = "A database error occurred. Please contact the administrator.";
    public const string InternalServerError = "An unexpected internal server error occurred.";
    public const string JsonError = "Invalid request payload format.";
    public const string ValidationFailed = "Validation failed.";
    public const string Unauthorized = "You are not authorized to perform this action.";
    public const string Forbidden = "Access to this resource is forbidden.";
    public const string NotFound = "Requested resource was not found.";

    // Identity / Users
    public static class Users
    {
        public const string Created = "User created successfully.";
        public const string Updated = "User updated successfully.";
        public const string Deleted = "User deleted successfully.";
        public const string NotFound = "User was not found.";
        public const string AlreadyExists = "A user with the specified username or email already exists.";
    }

    // Identity / Roles
    public static class Roles
    {
        public const string Created = "Role created successfully.";
        public const string Updated = "Role updated successfully.";
        public const string Deleted = "Role deleted successfully.";
        public const string NotFound = "Role was not found.";
        public const string AlreadyExists = "A role with the specified name already exists.";
        public const string CannotDeleteSystemRole = "System roles cannot be deleted.";
    }

    // Identity / Auth
    public static class Auth
    {
        public const string LoginSuccess = "Login successful.";
        public const string InvalidCredentials = "Invalid username or password.";
        public const string AccountInactive = "User account is inactive.";
        public const string ProfileNotFound = "User profile was not found.";
    }

    // Spatial Hierarchy
    public static class Spatial
    {
        public const string SiteCreated = "Site created successfully.";
        public const string SiteUpdated = "Site updated successfully.";
        public const string SiteDeleted = "Site deleted successfully.";
        public const string SiteNotFound = "Site was not found.";

        public const string BuildingCreated = "Building created successfully.";
        public const string BuildingUpdated = "Building updated successfully.";
        public const string BuildingDeleted = "Building deleted successfully.";
        public const string BuildingNotFound = "Building was not found.";

        public const string FloorCreated = "Floor created successfully.";
        public const string FloorUpdated = "Floor updated successfully.";
        public const string FloorDeleted = "Floor deleted successfully.";
        public const string FloorNotFound = "Floor was not found.";

        public const string RoomCreated = "Room created successfully.";
        public const string RoomUpdated = "Room updated successfully.";
        public const string RoomDeleted = "Room deleted successfully.";
        public const string RoomNotFound = "Room was not found.";

        public const string CameraCreated = "Camera created successfully.";
        public const string CameraUpdated = "Camera updated successfully.";
        public const string CameraDeleted = "Camera deleted successfully.";
        public const string CameraNotFound = "Camera was not found.";

        public const string ZoneCreated = "Zone created successfully.";
        public const string ZoneUpdated = "Zone updated successfully.";
        public const string ZoneDeleted = "Zone deleted successfully.";
        public const string ZoneNotFound = "Zone was not found.";
    }

    // Orchestrator & Pipelines
    public static class Pipelines
    {
        public const string Assigned = "Pipeline assigned successfully.";
        public const string Moved = "Pipeline moved successfully.";
        public const string Stopped = "Pipeline stopped successfully.";
        public const string NotFound = "Pipeline assignment was not found.";
        public const string AlreadyActive = "An active pipeline assignment already exists for this camera.";
        public const string CameraNotFound = "Camera was not found.";
        public const string EngineNotFound = "Engine instance was not found.";
    }

    // Monitoring & Observability
    public static class Monitoring
    {
        public const string SnapshotUpdated = "Snapshot updated successfully.";
    }

    // Analytics Catalog
    public static class Analytics
    {
        public const string ModuleCreated = "Analytics module created successfully.";
        public const string ModuleUpdated = "Analytics module updated successfully.";
        public const string ModuleDeleted = "Analytics module deleted successfully.";
        public const string ModuleNotFound = "Analytics module was not found.";
        public const string ModuleAlreadyExists = "An analytics module with the specified code already exists.";
        public const string EntitlementGranted = "Module entitlement granted successfully.";
        public const string EntitlementRevoked = "Module entitlement revoked successfully.";
        public const string EntitlementNotFound = "Module entitlement was not found.";
        public const string PipelineModulesUpdated = "Pipeline modules updated successfully.";
    }
}
