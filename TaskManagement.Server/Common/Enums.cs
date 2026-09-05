using System.Text.Json.Serialization;

namespace TaskManagement.Server.Common;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum UserRole
{
    Admin = 1,
    Manager = 2,
    User = 3
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum TaskItemStatus
{
    ToDo = 1,
    InProgress = 2,
    Done = 3
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum TaskPriority
{
    Low = 1,
    Medium = 2,
    High = 3,
    Urgent = 4
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum NotificationType
{
    TaskAssigned = 1,
    StatusChanged = 2,
    CommentAdded = 3
}
