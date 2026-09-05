import { apiClient } from './client';
import {
  ApiResponse,
  AuthResponse,
  Comment,
  DashboardSummary,
  NotificationLog,
  PagedResult,
  TaskFilterParams,
  TaskItem,
  TaskItemStatus,
  Team,
  User,
  UserRole,
} from '../types';

export const authApi = {
  login: async (email: string, password: string): Promise<AuthResponse> => {
    const res = await apiClient.post<ApiResponse<AuthResponse>>('/auth/login', { email, password });
    return res.data.data;
  },

  register: async (fullName: string, email: string, password: string, role: UserRole): Promise<AuthResponse> => {
    const res = await apiClient.post<ApiResponse<AuthResponse>>('/auth/register', {
      fullName,
      email,
      password,
      role,
    });
    return res.data.data;
  },

  getMe: async (): Promise<User> => {
    const res = await apiClient.get<ApiResponse<User>>('/auth/me');
    return res.data.data;
  },
};

export const taskApi = {
  getTasks: async (params?: TaskFilterParams): Promise<PagedResult<TaskItem>> => {
    const res = await apiClient.get<ApiResponse<PagedResult<TaskItem>>>('/tasks', { params });
    return res.data.data;
  },

  getTaskById: async (id: number): Promise<TaskItem> => {
    const res = await apiClient.get<ApiResponse<TaskItem>>(`/tasks/${id}`);
    return res.data.data;
  },

  createTask: async (data: {
    title: string;
    description: string;
    priority?: string;
    dueDate?: string;
    teamId?: number | null;
    assignedToUserId?: number | null;
  }): Promise<TaskItem> => {
    const res = await apiClient.post<ApiResponse<TaskItem>>('/tasks', data);
    return res.data.data;
  },

  updateTask: async (
    id: number,
    data: {
      title: string;
      description: string;
      status: TaskItemStatus;
      priority: string;
      dueDate: string;
      teamId?: number | null;
      assignedToUserId?: number | null;
    }
  ): Promise<TaskItem> => {
    const res = await apiClient.put<ApiResponse<TaskItem>>(`/tasks/${id}`, data);
    return res.data.data;
  },

  updateStatus: async (id: number, status: TaskItemStatus): Promise<TaskItem> => {
    const res = await apiClient.patch<ApiResponse<TaskItem>>(`/tasks/${id}/status`, { status });
    return res.data.data;
  },

  deleteTask: async (id: number): Promise<void> => {
    await apiClient.delete(`/tasks/${id}`);
  },
};

export const teamApi = {
  getTeams: async (): Promise<Team[]> => {
    const res = await apiClient.get<ApiResponse<Team[]>>('/teams');
    return res.data.data;
  },

  getTeamById: async (id: number): Promise<Team> => {
    const res = await apiClient.get<ApiResponse<Team>>(`/teams/${id}`);
    return res.data.data;
  },

  createTeam: async (data: { name: string; description?: string; leadManagerId?: number }): Promise<Team> => {
    const res = await apiClient.post<ApiResponse<Team>>('/teams', data);
    return res.data.data;
  },

  updateTeam: async (id: number, data: { name: string; description?: string; leadManagerId: number }): Promise<Team> => {
    const res = await apiClient.put<ApiResponse<Team>>(`/teams/${id}`, data);
    return res.data.data;
  },

  addMember: async (teamId: number, userId: number): Promise<void> => {
    await apiClient.post(`/teams/${teamId}/members`, { userId });
  },

  removeMember: async (teamId: number, userId: number): Promise<void> => {
    await apiClient.delete(`/teams/${teamId}/members/${userId}`);
  },

  deleteTeam: async (id: number): Promise<void> => {
    await apiClient.delete(`/teams/${id}`);
  },
};

export const commentApi = {
  getComments: async (taskId: number): Promise<Comment[]> => {
    const res = await apiClient.get<ApiResponse<Comment[]>>(`/tasks/${taskId}/comments`);
    return res.data.data;
  },

  addComment: async (taskId: number, content: string): Promise<Comment> => {
    const res = await apiClient.post<ApiResponse<Comment>>(`/tasks/${taskId}/comments`, { content });
    return res.data.data;
  },

  deleteComment: async (commentId: number): Promise<void> => {
    await apiClient.delete(`/comments/${commentId}`);
  },
};

export const notificationApi = {
  getNotifications: async (): Promise<NotificationLog[]> => {
    const res = await apiClient.get<ApiResponse<NotificationLog[]>>('/notifications');
    return res.data.data;
  },

  markAsRead: async (id: number): Promise<void> => {
    await apiClient.patch(`/notifications/${id}/read`);
  },

  markAllAsRead: async (): Promise<void> => {
    await apiClient.patch('/notifications/read-all');
  },
};

export const dashboardApi = {
  getSummary: async (): Promise<DashboardSummary> => {
    const res = await apiClient.get<ApiResponse<DashboardSummary>>('/dashboard/summary');
    return res.data.data;
  },
};

export const userApi = {
  getUsers: async (role?: UserRole): Promise<User[]> => {
    const res = await apiClient.get<ApiResponse<User[]>>('/users', { params: { role } });
    return res.data.data;
  },
};
