namespace TaskManagement.Server.Common;

/// <summary>
/// Centralized repository for all user-facing strings, validation rules,
/// error messages, and system notification templates across the entire application.
/// </summary>
public static class AppMessages
{
    public static class Auth
    {
        public const string LoginSuccess = "User logged in successfully.";
        public const string RegisterSuccess = "User registered successfully.";
        public const string InvalidCredentials = "The email or password you entered is incorrect.";
        public const string EmailAlreadyExists = "A user with this email address already exists.";
        public const string UserNotFound = "Specified user account could not be found.";
        public const string Unauthorized = "You are not authorized to perform this operation.";
        public const string Forbidden = "Your current role does not have permission for this resource.";
        public const string TokenExpired = "Your session has expired. Please sign in again.";
        public const string PasswordMismatch = "Password and confirmation password do not match.";
    }

    public static class Tasks
    {
        public const string Created = "Task created successfully.";
        public const string Updated = "Task details updated successfully.";
        public const string Deleted = "Task deleted successfully.";
        public const string NotFound = "The requested task was not found.";
        public const string StatusUpdated = "Task status updated successfully.";
        public const string Assigned = "Task assigned successfully.";
        public const string AssigneeNotMember = "The selected assignee is not a member of this team.";
        public const string InvalidStatusTransition = "Invalid status transition. Allowed statuses are To Do, In Progress, and Done.";
        public const string AccessDenied = "You do not have permission to modify this task.";
    }

    public static class Teams
    {
        public const string Created = "Team created successfully.";
        public const string Updated = "Team updated successfully.";
        public const string Deleted = "Team deleted successfully.";
        public const string NotFound = "The requested team was not found.";
        public const string MemberAdded = "Member successfully added to the team.";
        public const string MemberRemoved = "Member successfully removed from the team.";
        public const string MemberAlreadyExists = "User is already a member of this team.";
        public const string MemberNotFound = "The user is not a member of this team.";
        public const string ManagerRequired = "A team lead manager must have the 'Manager' or 'Admin' role.";
        public const string CannotDeleteTeamWithTasks = "Cannot delete team that has active tasks. Please reassign or complete them first.";
    }

    public static class Comments
    {
        public const string Added = "Comment posted successfully.";
        public const string NotFound = "Comment not found.";
        public const string Deleted = "Comment deleted successfully.";
        public const string EmptyComment = "Comment content cannot be empty.";
    }

    public static class Notifications
    {
        public const string MarkedAsRead = "Notification marked as read.";
        public const string AllMarkedAsRead = "All notifications marked as read.";
        public const string NotFound = "Notification not found.";

        // Notification templates
        public const string TaskAssignedTitle = "New Task Assigned";
        public static string TaskAssignedBody(string taskTitle, string assignedBy) =>
            $"You have been assigned to task '{taskTitle}' by {assignedBy}.";

        public const string TaskStatusChangedTitle = "Task Status Updated";
        public static string TaskStatusChangedBody(string taskTitle, string oldStatus, string newStatus, string updatedBy) =>
            $"Task '{taskTitle}' status was changed from {oldStatus} to {newStatus} by {updatedBy}.";

        public const string CommentAddedTitle = "New Comment on Task";
        public static string CommentAddedBody(string taskTitle, string author) =>
            $"{author} commented on task '{taskTitle}'.";
    }

    public static class Validation
    {
        public const string RequiredField = "One or more required fields are missing or invalid.";
        public const string FullNameRequired = "Full name is required and must be between 2 and 100 characters.";
        public const string EmailRequired = "A valid email address is required.";
        public const string PasswordRequired = "Password is required and must be at least 6 characters.";
        public const string RoleRequired = "A valid user role (Admin, Manager, or User) must be provided.";
        public const string TitleRequired = "Task title is required and must not exceed 200 characters.";
        public const string DescriptionRequired = "Task description is required.";
        public const string DueDateRequired = "Due date must be a valid future or current date.";
        public const string TeamNameRequired = "Team name is required and must not exceed 100 characters.";
        public const string PriorityRequired = "Priority must be Low, Medium, High, or Urgent.";
        public const string StatusRequired = "Status must be ToDo, InProgress, or Done.";
        public const string CommentRequired = "Comment text is required.";
        public const string IdPositive = "Entity identifier must be a positive integer.";
    }

    public static class Errors
    {
        public const string InternalServerError = "An unexpected error occurred while processing your request. Please try again later.";
        public const string ResourceNotFound = "The requested resource could not be found.";
        public const string BadRequest = "The request payload or parameters are invalid.";
        public const string ConcurrencyConflict = "The record has been modified by another process. Please refresh and try again.";
        public const string DatabaseError = "A database error occurred during operation execution.";
    }

    public static class General
    {
        public const string OperationSuccess = "Operation completed successfully.";
        public const string FetchSuccess = "Data retrieved successfully.";
    }
}
