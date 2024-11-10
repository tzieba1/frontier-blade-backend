using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

public class User: BaseModel
{
    public required string Email { get; set; }
    public required bool EmailConfirmed { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required List<Role> Roles { get; set; }
    public required string PasswordHash { get; set; }
}

public class Team: BaseModel
{
    public required string Name { get; set; }
    public required List<Employee> Members { get; set; }
}

public class Employee: BaseModel
{
    [BsonRepresentation(BsonType.String)]
    public required Guid UserId { get; set; }
    public required int Number { get; set; }
    public required EmployeeType Type { get; set; }
}

public class Approval: BaseModel
{
    public required List<TimeSheetEntry> Entries { get; set; }
    [BsonRepresentation(BsonType.String)]
    public required Guid TimeSheetId { get; set; }
    [BsonRepresentation(BsonType.String)]
    public required Guid ApproverId { get; set; }
    public required ApprovalStatus Status { get; set; }
    public required string Comments { get; set; }
    public required DateTime TimeStamp { get; set; }
}

public class TimeSheet: BaseModel
{
    public required List<Approval> Approvals { get; set; }
    public required List<TimeSheetEntry> Entries { get; set; }
}

public class TimeSheetEntry: BaseModel
{
    [BsonRepresentation(BsonType.String)]
    public required Guid TimeSheetId { get; set; }
    [BsonRepresentation(BsonType.String)]
    public required Guid EmployeeId { get; set; }
    public required DateTime Date { get; set; }
    public required string CustomerName { get; set; }
    public required string Comments { get; set; }
    public required decimal OnSiteHours { get; set; }
    public required decimal StandbyHours { get; set; }
    public required bool PerDiem { get; set; }
    public required bool Holiday { get; set; }
}

public class Claim : BaseModel
{
    [BsonRepresentation(BsonType.String)]
    public required ClaimType Type { get; set; }

    public required string Value { get; set; }

    public required Guid UserId { get; set; }
}

public class Policy : BaseModel
{
    public required string Name { get; set; } // Policy name (e.g., "RequireAdminRole")
    public required List<ClaimRequirement> Claims { get; set; } // Required claims for this policy
}

public class ClaimRequirement
{
    public required ClaimType Type { get; set; }
    public required string Value { get; set; }
}

public enum Role
{
    Admin,
    Accountant,
    Employee,
    Supervisor
}

public enum EmployeeType 
{
    FullTime,
    PartTime,
    Contractor
}

public enum ApprovalStatus
{
    Pending,
    Approved,
    Rejected
}

public enum ClaimType
{
    Role,
    Department,
    Region,
    PermissionLevel,
    EmailConfirmed
}

