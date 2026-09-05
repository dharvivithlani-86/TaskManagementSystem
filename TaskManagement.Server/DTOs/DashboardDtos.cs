using TaskManagement.Server.Common;

namespace TaskManagement.Server.DTOs;

public record DashboardSummaryDto(
    int TotalTasks,
    int ToDoCount,
    int InProgressCount,
    int DoneCount,
    int OverdueCount,
    int TotalTeams,
    int TotalMembers,
    List<StatusCountItem> StatusBreakdown,
    List<PriorityCountItem> PriorityBreakdown,
    List<TaskResponseDto> UpcomingTasks,
    List<RecentActivityDto> RecentActivities
);

public record StatusCountItem(
    TaskItemStatus Status,
    string Label,
    int Count,
    double Percentage
);

public record PriorityCountItem(
    TaskPriority Priority,
    string Label,
    int Count
);

public record RecentActivityDto(
    int Id,
    string Title,
    string Message,
    NotificationType Type,
    DateTime Timestamp
);
