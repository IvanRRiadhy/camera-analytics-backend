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
}
