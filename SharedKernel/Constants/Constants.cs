namespace SharedKernel.Constants;


// This class defines constant strings for claim types used in authentication and authorization and across all services.
public static class CustomClaimTypes
{
    public const string UserId = "uid";
    public const string Role = "role";
    public const string Email = "email";
    public const string CollegeId = "college_id";
    public const string CollegeCode = "college_code";
    public const string VerificationStatus = "verification_status";
    public const string FullName = "full_name";
    public const string ServiceClientId = "client_id";
}

// This class defines constant strings for user roles used in authorization and across all services.
public static class Roles
{
    public const string SuperAdmin = "SuperAdmin";
    public const string Admin = "Admin";
    public const string TPO = "TPO";
    public const string PlacementCoordinator = "PlacementCoordinator";
    public const string Recruiter = "Recruiter";
    public const string Student = "Student";

    // Composite role strings for multi-role authorization
    public const string SuperAdminOrAdmin = $"{SuperAdmin},{Admin}";
    public const string AdminOrTPO = $"{Admin},{TPO}";
    public const string SuperAdminOrAdminOrTPO = $"{SuperAdmin},{Admin},{TPO}";
    public const string TPOOrCoordinator = $"{TPO},{PlacementCoordinator}";
    public const string AllStaff = $"{SuperAdmin},{Admin},{TPO},{PlacementCoordinator}";
}

// This class defines constant strings for Kafka topic names used for event-driven communication across all services.
public static class KafkaTopics
{
    // Identity / User events
    public const string UserRegistered = "pms.user.registered";
    public const string UserEmailVerification = "pms.user.email-verification";
    public const string UserPasswordReset = "pms.user.password-reset";
    public const string UserDeactivated = "pms.user.deactivated";

    // College events
    public const string CollegeRegistered = "pms.college.registered";
    public const string CollegeDeactivated = "pms.college.deactivated";
    public const string CollegeActivated = "pms.college.activated";
    public const string TpoAssigned = "pms.tpo.assigned";
    public const string CoordinatorAdded = "pms.coordinator.added";

    // Drive events
    public const string DriveCreated = "pms.drive.created";
    public const string DriveApprovalRequested = "pms.drive.approval-requested";
    public const string DriveApproved = "pms.drive.approved";
    public const string DriveRejected = "pms.drive.rejected";
    public const string DriveClosed = "pms.drive.closed";

    // Application events
    public const string ApplicationSubmitted = "pms.application.submitted";
    public const string ApplicationStatusChanged = "pms.application.status-changed";
    public const string PlacementConfirmed = "pms.placement.confirmed";
}


/// <summary>
/// Redis cache key prefixes — prevents key collision across services.
/// Format: pms:<service>:<entity>:<id>
/// </summary>
public static class CacheKeys
{
    // College Service
    public const string CollegePrefix = "pms:college:";
    public const string CollegeListAll = "pms:college:list:all";
    public const string CollegeById = "pms:college:id:"; // append {id}
    public const string CollegeByCode = "pms:college:code:"; // append {code}

    // Drive Service
    public const string DrivePrefix = "pms:drive:";
    public const string DriveListByCollege = "pms:drive:college:"; // append {collegeId}
    public const string DriveById = "pms:drive:id:"; // append {id}

    // User Service
    public const string UserPrefix = "pms:user:";
    public const string StudentProfile = "pms:user:student:"; // append {userId}

    // Helper to build full key
    public static string Build(string prefix, string identifier)
        => $"{prefix}{identifier}";
}


// This class defines constant integers for cache time-to-live (TTL) values used across all services.
public static class CacheTtl
{
    public const int Short = 60;          // 1 minute  — live data (application counts)
    public const int Medium = 300;        // 5 minutes — semi-static (drive lists)
    public const int Long = 3600;         // 1 hour    — static (college list)
    public const int VeryLong = 86400;    // 24 hours  — rarely changes (enums, config)
}


public static class ApiVersions
{
    public const string V1 = "v1";
    public const string V1Route = "api/v1";
}


public static class Pagination
{
    public const int DefaultPage = 1;
    public const int DefaultPageSize = 20;
    public const int MaxPageSize = 100;
}


public static class ValidationRules
{
    // User
    public const int NameMinLength = 2;
    public const int NameMaxLength = 100;
    public const int EmailMaxLength = 256;
    public const int PasswordMinLength = 8;
    public const int PasswordMaxLength = 128;
    public const int PhoneLength = 10;

    // College
    public const int CollegeCodeMinLength = 4;
    public const int CollegeCodeMaxLength = 10;
    public const int CollegeNameMaxLength = 200;
    public const int CollegeCityMaxLength = 200;
    public const int CollegeStateMaxLength = 200;
    public const int CollegeAffiliatedByMaxLength = 200;

    // Drive
    public const int DriveRoleMaxLength = 150;
    public const int DriveJdMaxLength = 5000;
    public const double MinCgpa = 0.0;
    public const double MaxCgpa = 10.0;

    // Regex patterns
    public const string PhonePattern = @"^[6-9]\d{9}$"; // Indian mobile numbers
    public const string CollegeCodePattern = @"^[A-Z0-9]+$"; // Uppercase alphanumeric
    public const string EmailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
}

// This class defines constant strings for named HttpClient configurations used for inter-service communication across all services.
public static class HttpClientNames
{
    public const string CollegeService = "CollegeService";
    public const string UserService = "UserService";
    public const string DriveService = "DriveService";
    public const string ApplicationService = "ApplicationService";
    public const string IdentityService = "IdentityService";
}

// This class defines constant strings for configuration sections used to bind strongly-typed settings across all services.
public static class ConfigSections
{
    public const string Jwt = "JwtSettings";
    public const string Kafka = "KafkaSettings";
    public const string Redis = "RedisSettings";
    public const string ConnectionStrings = "ConnectionStrings";
    public const string ServiceUrls = "ServiceUrls";
    public const string EmailSettings = "EmailSettings";
}