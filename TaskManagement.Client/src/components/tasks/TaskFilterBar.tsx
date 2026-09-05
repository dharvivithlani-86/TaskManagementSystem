import React from 'react';
import { Search, RotateCcw, Filter } from 'lucide-react';
import { TaskFilterParams, TaskItemStatus, TaskPriority, Team, User } from '../../types';

interface TaskFilterBarProps {
  filters: TaskFilterParams;
  onFilterChange: (newFilters: Partial<TaskFilterParams>) => void;
  onReset: () => void;
  teams: Team[];
  users: User[];
}

export const TaskFilterBar: React.FC<TaskFilterBarProps> = ({
  filters,
  onFilterChange,
  onReset,
  teams,
  users,
}) => {
  return (
    <div className="bg-white p-4 rounded-2xl border border-slate-200/90 shadow-2xs space-y-3">
      <div className="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 lg:grid-cols-6 gap-3">
        {/* Search */}
        <div className="relative sm:col-span-2">
          <Search className="absolute left-3 top-2.5 w-4 h-4 text-slate-400" />
          <input
            type="text"
            placeholder="Search by title or description..."
            value={filters.search || ''}
            onChange={(e) => onFilterChange({ search: e.target.value, pageIndex: 1 })}
            className="w-full pl-9 pr-3 py-2 text-xs md:text-sm bg-slate-50 border border-slate-200 rounded-xl focus:outline-none focus:ring-2 focus:ring-indigo-500/20 focus:border-indigo-500 transition-all"
          />
        </div>

        {/* Status Filter */}
        <div>
          <select
            value={filters.status || ''}
            onChange={(e) =>
              onFilterChange({
                status: (e.target.value as TaskItemStatus) || undefined,
                pageIndex: 1,
              })
            }
            className="w-full px-3 py-2 text-xs md:text-sm bg-slate-50 border border-slate-200 rounded-xl focus:outline-none focus:ring-2 focus:ring-indigo-500/20 focus:border-indigo-500 transition-all text-slate-700"
          >
            <option value="">All Statuses</option>
            <option value="ToDo">To Do</option>
            <option value="InProgress">In Progress</option>
            <option value="Done">Done</option>
          </select>
        </div>

        {/* Priority Filter */}
        <div>
          <select
            value={filters.priority || ''}
            onChange={(e) =>
              onFilterChange({
                priority: (e.target.value as TaskPriority) || undefined,
                pageIndex: 1,
              })
            }
            className="w-full px-3 py-2 text-xs md:text-sm bg-slate-50 border border-slate-200 rounded-xl focus:outline-none focus:ring-2 focus:ring-indigo-500/20 focus:border-indigo-500 transition-all text-slate-700"
          >
            <option value="">All Priorities</option>
            <option value="Low">Low</option>
            <option value="Medium">Medium</option>
            <option value="High">High</option>
            <option value="Urgent">Urgent</option>
          </select>
        </div>

        {/* Team Filter */}
        <div>
          <select
            value={filters.teamId || ''}
            onChange={(e) =>
              onFilterChange({
                teamId: e.target.value ? Number(e.target.value) : undefined,
                pageIndex: 1,
              })
            }
            className="w-full px-3 py-2 text-xs md:text-sm bg-slate-50 border border-slate-200 rounded-xl focus:outline-none focus:ring-2 focus:ring-indigo-500/20 focus:border-indigo-500 transition-all text-slate-700"
          >
            <option value="">All Teams</option>
            {teams.map((t) => (
              <option key={t.id} value={t.id}>
                {t.name}
              </option>
            ))}
          </select>
        </div>

        {/* Assignee Filter */}
        <div>
          <select
            value={filters.assignedToUserId || ''}
            onChange={(e) =>
              onFilterChange({
                assignedToUserId: e.target.value ? Number(e.target.value) : undefined,
                pageIndex: 1,
              })
            }
            className="w-full px-3 py-2 text-xs md:text-sm bg-slate-50 border border-slate-200 rounded-xl focus:outline-none focus:ring-2 focus:ring-indigo-500/20 focus:border-indigo-500 transition-all text-slate-700"
          >
            <option value="">All Assignees</option>
            {users.map((u) => (
              <option key={u.id} value={u.id}>
                {u.fullName}
              </option>
            ))}
          </select>
        </div>
      </div>

      {/* Secondary filter row: Deadline range & Reset */}
      <div className="flex flex-wrap items-center justify-between gap-3 pt-2 border-t border-slate-100 text-xs">
        <div className="flex flex-wrap items-center gap-2">
          <span className="text-slate-500 font-medium">Due Date:</span>
          <input
            type="date"
            value={filters.dueDateFrom ? filters.dueDateFrom.split('T')[0] : ''}
            onChange={(e) =>
              onFilterChange({
                dueDateFrom: e.target.value ? new Date(e.target.value).toISOString() : undefined,
                pageIndex: 1,
              })
            }
            className="px-2.5 py-1 bg-slate-50 border border-slate-200 rounded-lg text-slate-700 focus:outline-none"
          />
          <span className="text-slate-400">to</span>
          <input
            type="date"
            value={filters.dueDateTo ? filters.dueDateTo.split('T')[0] : ''}
            onChange={(e) =>
              onFilterChange({
                dueDateTo: e.target.value ? new Date(e.target.value).toISOString() : undefined,
                pageIndex: 1,
              })
            }
            className="px-2.5 py-1 bg-slate-50 border border-slate-200 rounded-lg text-slate-700 focus:outline-none"
          />
        </div>

        <button
          type="button"
          onClick={onReset}
          className="flex items-center gap-1.5 text-slate-500 hover:text-slate-800 transition-colors py-1 px-2.5 rounded-lg hover:bg-slate-100"
        >
          <RotateCcw className="w-3.5 h-3.5" />
          <span>Reset Filters</span>
        </button>
      </div>
    </div>
  );
};
