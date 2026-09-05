# Team Task Management System - Comprehensive Test Cases

This document outlines the complete suite of test cases designed to validate all functional, non-functional, security, and architectural requirements specified in the **Dot Net Assignment Task Management System Project** document.

---

## 📋 Test Environment & Setup

| Item | Details |
| :--- | :--- |
| **Frontend Application** | `http://localhost:5173` |
| **Backend Web API** | `http://localhost:5128` |
| **Swagger API Documentation** | `http://localhost:5128/swagger` |
| **Database** | SQL Server `TaskManagementDb` (MSSQLSERVER / LocalDB) |
| **Postman Files** | `postman/TaskManagement.postman_collection.json` |

### Default Credentials
- **Admin**: `admin@tms.com` / `Admin@12345`
- **Manager**: `manager@tms.com` / `Manager@12345`
- **User (Member)**: `user@tms.com` / `User@12345`

---

## Module 1: Authentication & Authorization

### TC-AUTH-01: Admin Login via UI (1-Click & Manual)
- **Objective:** Verify administrator can sign in and receive a JWT token with `Admin` role.
- **Preconditions:** Frontend running at `http://localhost:5173`.
- **Steps:**
  1. Navigate to `http://localhost:5173/login`.
  2. Click the **Admin** button in the "1-Click Demo Credentials" box (or enter `admin@tms.com` / `Admin@12345` manually).
  3. Click **Sign In**.
- **Expected Result:**
  - Login succeeds with message: *"User logged in successfully."*
  - User is redirected to `/` (Dashboard).
  - Top-right navbar displays user initials and "System Administrator".
  - Sidebar shows **Dashboard**, **Task Board**, **Teams**, and **Swagger OpenAPI** link.

---

### TC-AUTH-02: Manager Login via UI
- **Objective:** Verify manager can authenticate and has access to team management and task creation.
- **Steps:**
  1. Log out or open an incognito browser window.
  2. Navigate to `http://localhost:5173/login`.
  3. Click the **Manager** button (`manager@tms.com` / `Manager@12345`).
  4. Click **Sign In**.
- **Expected Result:**
  - Login succeeds.
  - Navbar shows "Sarah Jenkins (Lead Manager)" with purple/indigo `Manager` badge.
  - Sidebar displays **Teams** link.

---

### TC-AUTH-03: Team Member (User) Login via UI
- **Objective:** Verify regular user authentication and restricted menu visibility.
- **Steps:**
  1. Navigate to `http://localhost:5173/login`.
  2. Click the **User** button (`user@tms.com` / `User@12345`).
  3. Click **Sign In**.
- **Expected Result:**
  - Login succeeds.
  - Sidebar displays only **Dashboard** and **Task Board** (the **Teams** navigation item is hidden).

---

### TC-AUTH-04: Invalid Credentials Error Validation
- **Objective:** Verify centralized error message when authentication fails.
- **Steps:**
  1. Go to `/login`.
  2. Enter email: `admin@tms.com` with password: `WrongPassword999!`.
  3. Click **Sign In**.
- **Expected Result:**
  - Error banner displays exact message from `AppMessages.Auth.InvalidCredentials`:  
    *"The email or password you entered is incorrect."*
  - User remains on the login page.

---

### TC-AUTH-05: New User Registration & Role Selection
- **Objective:** Verify new user registration with specific role assignment.
- **Steps:**
  1. Go to `http://localhost:5173/register`.
  2. Fill in:
     - Full Name: `Carlos Gomez`
     - Email: `carlos.gomez@tms.com`
     - Password: `Password@123`
     - Role: `Team Member (User)`
  3. Click **Create Account**.
- **Expected Result:**
  - Account is created and user is automatically signed in.
  - Backend hashes password with BCrypt (not stored in plain text).
  - Record is inserted in `dbo.Users` with `CreatedBy = "carlos.gomez@tms.com"`.

---

### TC-AUTH-06: Session Expiration & 401 Interceptor
- **Objective:** Verify frontend Axios response interceptor redirects to login on 401.
- **Steps:**
  1. While logged in, open Developer Tools (F12) -> Application -> Local Storage.
  2. Edit or corrupt the value of `tms_token` (e.g. change last characters).
  3. Refresh the page or click "Task Board".
- **Expected Result:**
  - Axios interceptor catches 401 Unauthorized.
  - Storage is cleared and user is redirected to `/login?expired=true`.
  - Alert banner displays: *"Your session has expired. Please sign in again."*

---

## Module 2: Role-Based Access Control (RBAC)

### TC-RBAC-01: User Role Restrictions
- **Objective:** Ensure regular User cannot create teams, cannot assign tasks to other people, and cannot access `/teams`.
- **Preconditions:** Logged in as `user@tms.com`.
- **Steps:**
  1. Attempt to browse directly to `http://localhost:5173/teams`.
  2. Go to `http://localhost:5173/tasks`.
- **Expected Result:**
  - Navigating to `/teams` triggers the `ProtectedRoute` guard and redirects the user back to `/`.
  - On `/tasks`, the **"New Task"** button is NOT visible to regular users.

---

### TC-RBAC-02: Manager Role Capabilities
- **Objective:** Ensure Manager can manage their team and create tasks.
- **Preconditions:** Logged in as `manager@tms.com`.
- **Steps:**
  1. Go to `/teams` -> Confirm managed team ("Core Engineering Team") is visible.
  2. Click **Manage Team Members** -> Confirm manager can add/remove members.
  3. Go to `/tasks` -> Confirm **"New Task"** button is visible.
- **Expected Result:**
  - Manager successfully accesses teams and has task creation permissions.

---

### TC-RBAC-03: Admin Role Capabilities
- **Objective:** Ensure Admin has full administrative controls.
- **Preconditions:** Logged in as `admin@tms.com`.
- **Steps:**
  1. Go to `/teams` -> Click **Create Team**.
  2. Assign any user as lead manager.
  3. Delete an empty team.
- **Expected Result:**
  - Admin can perform all team creations, member assignments, task creations, and deletions.

---

### TC-RBAC-04: API 403 Forbidden Enforcement
- **Objective:** Ensure backend API enforces role authorization even if requests bypass the UI.
- **Steps:**
  1. In Postman or Swagger, log in as `user@tms.com` and copy the JWT token.
  2. Send a `POST http://localhost:5128/api/teams` with JSON `{"name":"Hacker Team"}`.
- **Expected Result:**
  - Server responds with HTTP `403 Forbidden`.

---

## Module 3: Task Management & Status Tracking

### TC-TASK-01: Task Creation by Manager
- **Objective:** Verify task creation with title, description, priority, due date, team, and assignee.
- **Preconditions:** Logged in as `manager@tms.com`.
- **Steps:**
  1. Navigate to `/tasks`.
  2. Click **+ New Task**.
  3. Enter:
     - Title: `E2E Cypress Automation Suite`
     - Description: `Develop comprehensive end-to-end tests for task board.`
     - Priority: `High`
     - Due Date: Select 5 days from today
     - Team: `Core Engineering Team`
     - Assignee: `Alex Rivera (Developer)`
  4. Click **Create Task**.
- **Expected Result:**
  - Modal closes and task appears under **To Do** column on the Kanban board.
  - In `dbo.TaskItems`, record is inserted with `Status = 'ToDo'`, `CreatedByUserId = 2`, and `AssignedToUserId = 3`.

---

### TC-TASK-02: Kanban Board Status Transitions
- **Objective:** Verify moving task from `To Do` -> `In Progress` -> `Done`.
- **Preconditions:** Logged in as assignee `user@tms.com`.
- **Steps:**
  1. Navigate to `/tasks`.
  2. On the task card in the **To Do** column, click **Start ->**.
  3. Confirm card moves to **In Progress**.
  4. On the same card, click **Mark Done ✓**.
  5. Confirm card moves to **Done**.
  6. Click **Reopen** -> Confirm card returns to **In Progress**.
- **Expected Result:**
  - Status updates optimistically and persists in database (`PATCH /api/tasks/{id}/status`).
  - Status change notification is dispatched to task creator.

---

### TC-TASK-03: Table Grid View, Sorting & Pagination
- **Objective:** Verify tabular data grid representation of tasks.
- **Steps:**
  1. On `/tasks`, click the **Table** toggle button in the top right.
  2. Click on the **Title** column header to sort alphabetically.
  3. Click on the **Due Date** column header to sort by deadline.
  4. Click on the **Priority** column header.
- **Expected Result:**
  - Tasks render in a clean data grid with columns: Title, Status, Priority, Due Date, Team, Assignee, Comments.
  - Sorting toggles between ascending and descending.

---

### TC-TASK-04: Multi-Filter Search (Deadline, Status, Priority, Team, Assignee)
- **Objective:** Validate all filter criteria required by the assessment brief.
- **Steps:**
  1. **Search:** In the search input, type `Kanban`. Confirm only matching tasks appear. Clear search.
  2. **Status Filter:** Select `In Progress`. Confirm only In-Progress tasks appear.
  3. **Priority Filter:** Select `Urgent`. Confirm only urgent tasks appear.
  4. **Team Filter:** Select `Core Engineering Team`. Confirm only tasks for this team appear.
  5. **Due Date Filter:** Set a date range. Confirm only tasks falling within that range appear.
  6. Click **Reset Filters**.
- **Expected Result:**
  - Filters can be combined together seamlessly.
  - Reset Filters restores the complete task list.

---

### TC-TASK-05: Task Soft Delete Verification
- **Objective:** Verify tasks are soft-deleted (`IsDeleted = 1`) without database cascading loss.
- **Preconditions:** Logged in as `admin@tms.com`.
- **Steps:**
  1. In Swagger or Postman, call `DELETE /api/tasks/4`.
  2. Query SQL Server:
     ```sql
     SELECT Id, Title, IsDeleted, UpdatedAt, UpdatedBy FROM TaskItems WHERE Id = 4;
     ```
  3. Refresh the tasks list on the frontend.
- **Expected Result:**
  - Database row shows `IsDeleted = 1`, `UpdatedAt = <UtcNow>`, `UpdatedBy = 'admin@tms.com'`.
  - Task is no longer returned in `GET /api/tasks` due to EF Core global query filter.

---

## Module 4: Collaboration & Comments

### TC-COMM-01: Add Comment on a Task
- **Objective:** Verify adding comments to a task.
- **Steps:**
  1. Click any task card on the Kanban board or row on the Table.
  2. In the slide-over/modal, scroll to **Comments & Collaboration**.
  3. In the "Write a comment..." box, enter:
     `"Refactored the authentication middleware to include Bearer claims."`
  4. Click **Post**.
- **Expected Result:**
  - Comment immediately appears in the thread with author's name, role badge, and timestamp.
  - Task comment counter increments on the card and table.

---

### TC-COMM-02: Comment Thread Ordering & Delete Permission
- **Objective:** Verify chronological comment order and authorization on deletion.
- **Steps:**
  1. Log in as `user@tms.com` and post a comment.
  2. Log in as `manager@tms.com` and post a reply.
  3. Observe that comments are ordered chronologically.
  4. Check the delete trash icon:
     - `user@tms.com` can delete their own comment, but NOT the manager's comment.
     - `admin@tms.com` can delete any comment.
- **Expected Result:**
  - Comments display chronologically.
  - Non-admins cannot delete other users' comments (enforced in UI and API).

---

## Module 5: Notifications System

### TC-NOTIF-01: In-App Notification on Task Assignment
- **Objective:** Verify notification is triggered when a task is assigned to a user.
- **Steps:**
  1. Log in as `manager@tms.com`.
  2. Create a new task and assign it to `user@tms.com` (Alex Rivera).
  3. Log out and log in as `user@tms.com`.
  4. Observe the top-right navbar bell icon.
- **Expected Result:**
  - Notification bell shows unread count indicator (e.g., `1` or `2`).
  - Clicking the bell opens the popover showing:
    - Title: *"New Task Assigned"*
    - Message: *"You have been assigned to task '...' by Sarah Jenkins (Lead Manager)."*
  - Clicking the notification opens the task details modal.

---

### TC-NOTIF-02: In-App Notification on Task Status Update
- **Objective:** Verify notification is triggered when task status changes.
- **Steps:**
  1. As `user@tms.com`, update a task created by `manager@tms.com` from `To Do` to `In Progress`.
  2. Log in as `manager@tms.com`.
  3. Click the notification bell.
- **Expected Result:**
  - Notification displayed:
    - Title: *"Task Status Updated"*
    - Message: *"Task '...' status was changed from ToDo to InProgress by Alex Rivera (Developer)."*

---

### TC-NOTIF-03: Mark Single & Mark All Notifications Read
- **Objective:** Verify clearing notification count.
- **Steps:**
  1. Open notification popover with unread items.
  2. Click **Mark all read**.
- **Expected Result:**
  - Unread badge counter on bell disappears.
  - Notification cards switch from highlighted blue background to normal read background.

---

### TC-NOTIF-04: Mock Email Notification Dispatch Logging
- **Objective:** Verify email dispatch is logged in Serilog structured logs.
- **Steps:**
  1. Trigger a task assignment or status update.
  2. Open file `TaskManagement.Server/Logs/tms-<Date>.log`.
- **Expected Result:**
  - Log contains formatted email notification:
    ```
    ==================== [EMAIL DISPATCH NOTIFICATION] ====================
    To: Alex Rivera <user@tms.com>
    Subject: New Task Assigned
    Timestamp: ...
    Content: You have been assigned to task '...'
    =======================================================================
    ```

---

## Module 6: Executive Dashboard & Analytics

### TC-DASH-01: Summary Metric Cards
- **Objective:** Verify accurate aggregation of Total, To Do, In Progress, Done, and Overdue tasks.
- **Steps:**
  1. Navigate to `http://localhost:5173/`.
  2. Observe the 5 stat cards: Total Tasks, To Do, In Progress, Completed, Overdue.
  3. Move a task to `Done` on `/tasks` and return to `/`.
- **Expected Result:**
  - Completed count increments and To Do/In Progress decrements accordingly.
  - Completion rate percentage updates.

---

### TC-DASH-02: Multi-Segment Progress Bar & Priority Cards
- **Objective:** Verify visual distribution bar.
- **Steps:**
  1. View the **Task Status Distribution** widget on the Dashboard.
  2. Hover over the multi-colored bar segments (Green = Done, Amber = In Progress, Slate = To Do).
  3. Check the **Priority Breakdown** cards (Low, Medium, High, Urgent).
- **Expected Result:**
  - Segment widths match percentages.
  - Priority bars scale proportionally to highest task count.

---

### TC-DASH-03: Upcoming Deadlines & Recent Activity Timeline
- **Objective:** Verify upcoming tasks and recent audit events.
- **Steps:**
  1. Check **Upcoming Deadlines** list. Overdue tasks display in red.
  2. Click any task in the Upcoming Deadlines list.
  3. Check **Recent Activity** list on the right.
- **Expected Result:**
  - Clicking an upcoming task opens the Task Details modal directly.
  - Recent activity timeline reflects latest notification events with human-readable timestamps.

---

## Module 7: Team Management

### TC-TEAM-01: Create New Team
- **Objective:** Verify creating a team with a Lead Manager.
- **Preconditions:** Logged in as `admin@tms.com` or `manager@tms.com`.
- **Steps:**
  1. Go to `http://localhost:5173/teams`.
  2. Click **+ Create Team**.
  3. Name: `Quality Assurance Squad`.
  4. Description: `Automation testing and performance benchmarks.`
  5. Click **Create Team**.
- **Expected Result:**
  - New team card appears in the grid.
  - The creator/selected manager is automatically enrolled as the first member and lead.

---

### TC-TEAM-02: Add Member to Team
- **Objective:** Verify assigning users to a team.
- **Steps:**
  1. On the team card, click **Manage Team Members**.
  2. In the modal, select a user from the dropdown (e.g. `Elena Rostova`).
  3. Click **Add**.
- **Expected Result:**
  - User is added to the team member list.
  - In `dbo.TeamMembers`, record is inserted with `TeamId` and `UserId`.
  - Team member count increments on the card.

---

### TC-TEAM-03: Remove Member from Team
- **Objective:** Verify removing non-lead member from a team.
- **Steps:**
  1. In the **Manage Team Members** modal, click the trash icon next to a member.
  2. Note: The Lead Manager has no trash icon and cannot be removed.
- **Expected Result:**
  - Member is removed from the team.
  - Attempting to remove the lead manager shows: *"Cannot remove the team lead manager."*

---

## Module 8: Database Audit Trail & Soft Deletion

### TC-DB-01: BaseEntity Creation Audit Verification
- **Objective:** Verify `CreatedAt`, `CreatedBy`, and `IsDeleted` are automatically populated.
- **Steps:**
  1. Execute a query in SQL Server:
     ```sql
     SELECT TOP 1 Id, Title, CreatedAt, CreatedBy, UpdatedAt, UpdatedBy, IsDeleted 
     FROM TaskItems 
     ORDER BY Id DESC;
     ```
- **Expected Result:**
  - `CreatedAt` has valid UTC timestamp.
  - `CreatedBy` has the email of the user who created the record.
  - `IsDeleted` is `0` (false).

---

### TC-DB-02: BaseEntity Modification Audit Verification
- **Objective:** Verify `UpdatedAt` and `UpdatedBy` are automatically populated on edit.
- **Steps:**
  1. Edit a task title or status.
  2. Execute query:
     ```sql
     SELECT Id, Title, UpdatedAt, UpdatedBy FROM TaskItems WHERE Id = <task_id>;
     ```
- **Expected Result:**
  - `UpdatedAt` is updated to current UTC time.
  - `UpdatedBy` contains the modifying user's email.

---

## Module 9: Automated Tests & Postman Verification

### TC-AUTO-01: Execute Automated xUnit Unit Tests
- **Command:**
  ```powershell
  dotnet test
  ```
- **Expected Result:**
  ```
  Passed!  - Failed: 0, Passed: 6, Skipped: 0, Total: 6 - TaskManagement.Tests.dll (net10.0)
  ```
  - `AuthTests.BCrypt_ShouldCorrectlyHash_AndVerifyPassword`: Passed
  - `AuthTests.TokenService_ShouldGenerateValidJwt_WithRequiredClaims`: Passed
  - `AuditEntityTests.SaveChangesAsync_ShouldAutomaticallyPopulateAuditFields_OnCreation`: Passed
  - `AuditEntityTests.DeleteEntity_ShouldPerformSoftDelete_AndPopulateUpdatedAtAndUpdatedBy`: Passed
  - `TaskWorkflowTests.NotificationService_ShouldRecordNotification_WhenTaskAssigned`: Passed
  - `TaskWorkflowTests.NotificationService_ShouldRecordNotification_WhenStatusChanged`: Passed

---

### TC-AUTO-02: Execute Postman Collection
- **Steps:**
  1. Open Postman.
  2. Import `postman/TaskManagement.postman_collection.json` and `postman/TaskManagement.postman_environment.json`.
  3. Execute **Login as Admin** -> Confirm status `200 OK` and `token` variable is automatically saved.
  4. Execute **Get Tasks** -> Confirm status `200 OK` and paged results.
  5. Execute **Create Task** -> Confirm status `201 Created`.
  6. Execute **Update Task Status** -> Confirm status `200 OK`.
  7. Execute **Get Dashboard Summary** -> Confirm status `200 OK`.
- **Expected Result:**
  - All collection requests return valid JSON wrapped in `ApiResponse<T>` with `success: true`.
