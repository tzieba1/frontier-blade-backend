using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

public class User
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }
    
    public required string Email { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Role { get; set; }
}

public class Team
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    public required string Name { get; set; }
    public required List<Employee> Members { get; set; }
}

public class Employee
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    public required User User { get; set; }
    public required int Number { get; set; }
    public required string Type { get; set; }  // e.g., "full-time", "part-time", etc.
}

public class Approval
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    public required List<TimeSheetEntry> Entries { get; set; }
    public required string TimeSheetId { get; set; }
    public required User Approver { get; set; }
    public required ApprovalStatus Status { get; set; }
    public required string Comments { get; set; }
    public required DateTime TimeStamp { get; set; }
}

public class TimeSheet
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    public required List<Approval> Approvals { get; set; }
    public required Employee Employee { get; set; }
    public required List<TimeSheetEntry> Entries { get; set; }
}

public class TimeSheetEntry
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    public required string TimeSheetId { get; set; }
    public required DateTime Date { get; set; }
    public required string CustomerName { get; set; }
    public required string Comments { get; set; }
    public required Rates Rates { get; set; }
    public required bool PerDiem { get; set; }
    public required bool Holiday { get; set; }
}

public class Rates
{
    public double OnSite { get; set; }
    public double Standby { get; set; }
}

public enum ApprovalStatus
{
    Pending,
    Approved,
    Rejected
}