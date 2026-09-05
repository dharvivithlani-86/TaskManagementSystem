import React, { useEffect, useState } from 'react';
import { LayoutGrid, List, Plus, RefreshCw } from 'lucide-react';
import { taskApi, teamApi, userApi } from '../api/services';
import {
  PagedResult,
  TaskFilterParams,
  TaskItem,
  TaskItemStatus,
  Team,
  User,
} from '../types';
import { KanbanBoard } from '../components/tasks/KanbanBoard';
import { TaskTable } from '../components/tasks/TaskTable';
import { TaskFilterBar } from '../components/tasks/TaskFilterBar';
import { TaskDetailModal } from '../components/tasks/TaskDetailModal';
import { CreateTaskModal } from '../components/tasks/CreateTaskModal';
import { useAuth } from '../context/AuthContext';

export const TasksPage: React.FC = () => {
  const { isManager } = useAuth();

  const [viewMode, setViewMode] = useState<'kanban' | 'table'>('kanban');
  const [tasks, setTasks] = useState<TaskItem[]>([]);
  const [pagedResult, setPagedResult] = useState<PagedResult<TaskItem>>({
    items: [],
    totalCount: 0,
    pageIndex: 1,
    pageSize: 50,
    totalPages: 1,
    hasPreviousPage: false,
    hasNextPage: false,
  });

  const [filters, setFilters] = useState<TaskFilterParams>({
    pageIndex: 1,
    pageSize: 50,
  });

  const [teams, setTeams] = useState<Team[]>([]);
  const [users, setUsers] = useState<User[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const [selectedTaskId, setSelectedTaskId] = useState<number | null>(null);
  const [isCreateModalOpen, setIsCreateModalOpen] = useState(false);

  useEffect(() => {
    loadAuxiliaryData();
  }, []);

  useEffect(() => {
    loadTasks();
  }, [filters]);

  const loadAuxiliaryData = async () => {
    try {
      const [teamsData, usersData] = await Promise.all([
        teamApi.getTeams().catch(() => []),
        userApi.getUsers().catch(() => []),
      ]);
      setTeams(teamsData);
      setUsers(usersData);
    } catch (err) {
      console.error('Failed to load aux data', err);
    }
  };

  const loadTasks = async () => {
    try {
      setIsLoading(true);
      setError(null);
      const data = await taskApi.getTasks(filters);
      setPagedResult(data);
      setTasks(data.items);
    } catch (err: any) {
      setError(err.message || 'Failed to load tasks');
    } finally {
      setIsLoading(false);
    }
  };

  const handleFilterChange = (newFilters: Partial<TaskFilterParams>) => {
    setFilters((prev) => ({ ...prev, ...newFilters }));
  };

  const handleResetFilters = () => {
    setFilters({
      pageIndex: 1,
      pageSize: 50,
    });
  };

  const handleStatusChange = async (taskId: number, newStatus: TaskItemStatus) => {
    try {
      const updated = await taskApi.updateStatus(taskId, newStatus);
      setTasks((prev) => prev.map((t) => (t.id === taskId ? updated : t)));
      setPagedResult((prev) => ({
        ...prev,
        items: prev.items.map((t) => (t.id === taskId ? updated : t)),
      }));
    } catch (err: any) {
      setError(err.message || 'Failed to update task status');
    }
  };

  const handleTaskCreated = (newTask: TaskItem) => {
    loadTasks();
  };

  const handleTaskUpdated = (updatedTask: TaskItem) => {
    setTasks((prev) => prev.map((t) => (t.id === updatedTask.id ? updatedTask : t)));
    setPagedResult((prev) => ({
      ...prev,
      items: prev.items.map((t) => (t.id === updatedTask.id ? updatedTask : t)),
    }));
  };

  return (
    <div className="space-y-6">
      {/* Page Header */}
      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
        <div>
          <h2 className="text-xl font-bold text-slate-900 tracking-tight">Task Management</h2>
          <p className="text-xs text-slate-500 mt-0.5">
            Organize, prioritize, and track team assignments
          </p>
        </div>

        <div className="flex items-center gap-3">
          {/* View Mode Toggle */}
          <div className="flex items-center bg-white p-1 rounded-xl border border-slate-200 shadow-2xs">
            <button
              type="button"
              onClick={() => setViewMode('kanban')}
              className={`flex items-center gap-1.5 px-3 py-1.5 rounded-lg text-xs font-semibold transition-colors ${
                viewMode === 'kanban'
                  ? 'bg-indigo-600 text-white shadow-xs'
                  : 'text-slate-600 hover:text-slate-900'
              }`}
            >
              <LayoutGrid className="w-3.5 h-3.5" />
              <span>Board</span>
            </button>
            <button
              type="button"
              onClick={() => setViewMode('table')}
              className={`flex items-center gap-1.5 px-3 py-1.5 rounded-lg text-xs font-semibold transition-colors ${
                viewMode === 'table'
                  ? 'bg-indigo-600 text-white shadow-xs'
                  : 'text-slate-600 hover:text-slate-900'
              }`}
            >
              <List className="w-3.5 h-3.5" />
              <span>Table</span>
            </button>
          </div>

          <button
            type="button"
            onClick={loadTasks}
            className="p-2 bg-white text-slate-600 hover:text-slate-900 border border-slate-200 rounded-xl shadow-2xs transition-colors"
            title="Refresh tasks"
          >
            <RefreshCw className={`w-4 h-4 ${isLoading ? 'animate-spin' : ''}`} />
          </button>

          {/* New Task button for Manager/Admin */}
          {isManager && (
            <button
              type="button"
              onClick={() => setIsCreateModalOpen(true)}
              className="flex items-center gap-1.5 px-4 py-2 bg-indigo-600 hover:bg-indigo-700 text-white text-xs font-semibold rounded-xl shadow-md shadow-indigo-600/20 transition-all"
            >
              <Plus className="w-4 h-4" />
              <span>New Task</span>
            </button>
          )}
        </div>
      </div>

      {/* Filter Bar */}
      <TaskFilterBar
        filters={filters}
        onFilterChange={handleFilterChange}
        onReset={handleResetFilters}
        teams={teams}
        users={users}
      />

      {/* Error alert */}
      {error && (
        <div className="p-4 bg-rose-50 border border-rose-200 text-rose-700 rounded-xl text-xs">
          {error}
        </div>
      )}

      {/* Content: Kanban or Table */}
      {isLoading ? (
        <div className="flex items-center justify-center min-h-[350px]">
          <div className="text-slate-400 text-sm animate-pulse">Loading tasks...</div>
        </div>
      ) : viewMode === 'kanban' ? (
        <KanbanBoard
          tasks={tasks}
          onTaskClick={(t) => setSelectedTaskId(t.id)}
          onStatusChange={handleStatusChange}
        />
      ) : (
        <TaskTable
          pagedResult={pagedResult}
          onTaskClick={(t) => setSelectedTaskId(t.id)}
          onFilterChange={handleFilterChange}
          filters={filters}
        />
      )}

      {/* Task Details Modal with Comments */}
      <TaskDetailModal
        taskId={selectedTaskId}
        isOpen={selectedTaskId !== null}
        onClose={() => setSelectedTaskId(null)}
        onTaskUpdated={handleTaskUpdated}
      />

      {/* Create Task Modal */}
      {isManager && (
        <CreateTaskModal
          isOpen={isCreateModalOpen}
          onClose={() => setIsCreateModalOpen(false)}
          onTaskCreated={handleTaskCreated}
          teams={teams}
          users={users}
        />
      )}
    </div>
  );
};
