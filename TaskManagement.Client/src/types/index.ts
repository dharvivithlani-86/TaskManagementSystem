export type UserRole = 'Admin' | 'Manager' | 'User';

export type TaskItemStatus = 'ToDo' | 'InProgress' | 'Done';

export type TaskPriority = 'Low' | 'Medium' | 'High' | 'Urgent';

export type NotificationType = 'TaskAssigned' | 'StatusChanged' | 'CommentAdded';

export interface User {
  id: number;
  fullName: string;
  email: string;
  role: UserRole;
  createdAt: string;
}

export interface AuthResponse {
  token: string;
  expiresAt: string;
  user: User;
}

export interface TaskItem {
  id: number;
  title: string;
  description: string;
  status: TaskItemStatus;
  priority: TaskPriority;
  dueDate: string;
  teamId?: number | null;
  teamName?: string | null;
  createdByUserId: number;
  createdByName: string;
  assignedToUserId?: number | null;
  assignedToName?: string | null;
  commentsCount: number;
  createdAt: string;
  updatedAt?: string | null;
}

export interface TeamMember {
  userId: number;
  fullName: string;
  email: string;
  role: UserRole;
  joinedAt: string;
}

export interface Team {
  id: number;
  name: string;
  description?: string | null;
  leadManagerId: number;
  leadManagerName: string;
  memberCount: number;
  activeTaskCount: number;
  members: TeamMember[];
  createdAt: string;
}

export interface Comment {
  id: number;
  taskItemId: number;
  userId: number;
  userName: string;
  userEmail: string;
  userRole: UserRole;
  content: string;
  createdAt: string;
}

export interface NotificationLog {
  id: number;
  taskItemId?: number | null;
  taskTitle?: string | null;
  title: string;
  message: string;
  type: NotificationType;
  isRead: boolean;
  createdAt: string;
}

export interface StatusCountItem {
  status: TaskItemStatus;
  label: string;
  count: number;
  percentage: number;
}

export interface PriorityCountItem {
  priority: TaskPriority;
  label: string;
  count: number;
}

export interface RecentActivity {
  id: number;
  title: string;
  message: string;
  type: NotificationType;
  timestamp: string;
}

export interface DashboardSummary {
  totalTasks: number;
  toDoCount: number;
  inProgressCount: number;
  doneCount: number;
  overdueCount: number;
  totalTeams: number;
  totalMembers: number;
  statusBreakdown: StatusCountItem[];
  priorityBreakdown: PriorityCountItem[];
  upcomingTasks: TaskItem[];
  recentActivities: RecentActivity[];
}

export interface ApiResponse<T = any> {
  success: boolean;
  message: string;
  data: T;
  errors?: string[];
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  pageIndex: number;
  pageSize: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

export interface TaskFilterParams {
  search?: string;
  status?: TaskItemStatus;
  priority?: TaskPriority;
  assignedToUserId?: number;
  teamId?: number;
  dueDateFrom?: string;
  dueDateTo?: string;
  sortBy?: string;
  isAscending?: boolean;
  pageIndex?: number;
  pageSize?: number;
}
