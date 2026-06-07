namespace SharedKernel.Enums
{
    public enum UserRole
    {
        SuperAdmin = 1,
        Admin = 2,
        TPO = 3,
        PlacementCoordinator = 4,
        Recruiter = 5,
        Student = 6
    }

    public enum DriveStatus
    {
        Draft = 1,
        PendingApproval = 2,
        Active = 3,
        Closed = 4,
        Cancelled = 5
    }

    // Represents the approval status of a drive from the perspective of a college
    public enum DriveApprovalStatus
    {
        Pending = 1,
        Approved = 2,
        Rejected = 3,
        ChangesRequested = 4
    }

    // Represents the status of a student's application to a drive
    public enum ApplicationStatus
    {
        Applied = 1,
        UnderReview = 2,
        Shortlisted = 3,
        Offered = 4,
        Accepted = 5,
        Rejected = 6,
        Withdrawn = 7,
    }

    // Represents the status of a student's placement for TPO reporting
    public enum PlacementStatus
    {
        NotApplied = 1,
        InProcess = 2,
        Placed = 3
    }

    // Represents the verification status of a college or recruiter or admin account
    public enum VerificationStatus
    {
        Unverified = 1,
        Verified = 2,
    }

    public enum AccountStatus
    {
        Active = 1,
        Suspended = 2,
        Deactivated = 3
    }

    // Represents the status of a college in the system
    public enum CollegeStatus
    {
        Active = 1,
        Deactivated = 2
    }

    // Represents the type of employment offered by a drive or accepted by a student   
    public enum EmploymentType
    {
        FullTime = 1,
        Internship = 2,
        InternshipWithPPO = 3,
        Contract = 4,
        PartTime = 5
    }

    [Flags]  // Use bitwise flags to allow combination of branches
    public enum EligibleBranch
    {
        None = 0,
        ComputerScience = 1,
        InformationTechnology = 2,
        ElectronicsAndCommunication = 4,
        ElectricalEngineering = 8,
        MechanicalEngineering = 16,
        CivilEngineering = 32,
        All = ComputerScience | InformationTechnology | ElectronicsAndCommunication
            | ElectricalEngineering | MechanicalEngineering | CivilEngineering
    }

    public enum CollegeType
    {
        Government = 1,
        Private = 2,
        DeemedUniversity = 3,
        Autonomous = 4
    }
}