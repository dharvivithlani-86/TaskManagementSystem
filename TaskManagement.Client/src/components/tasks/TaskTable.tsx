import React from 'react';
import { ChevronLeft, ChevronRight, MessageSquare, ArrowUpDown } from 'lucide-react';
import { PagedResult, TaskFilterParams, TaskItem, TaskItemStatus } from '../../types';
import { PriorityBadge, StatusBadge } from '../common/Badge';

interface TaskTableProps {
  pagedResult: PagedResult<TaskItem>;
  onTaskClick: (task: TaskItem) => void;
  onFilterChange: (newFilters: Partial<TaskFilterParams>) => void;
  filters: TaskFilterParams;
}

export const TaskTable: React.FC<TaskTableProps> = ({
  pagedResult,
  onTaskClick,
  onFilterChange,
  filters,
}) => {
  const { items, totalCount, pageIndex, pageSize, totalPages, hasPreviousPage, hasNextPage } =
    pagedResult;

  const handleSort = (field: string) => {
    const isSameField = filters.sortBy?.toLowerCase() === field.toLowerCase();
    const isAsc = isSameField ? !filters.isAscending : true;
    onFilterChange({ sortBy: field, isAscending: isAsc, pageIndex: 1 });
  };

  return (
    <div className="bg-white rounded-2xl border border-slate-200/90 shadow-2xs overflow-hidden">
      <div className="overflow-x-auto">
        <table className="w-full text-left text-sm text-slate-600">
          <thead className="bg-slate-50 text-slate-700 text-xs uppercase font-semibold border-b border-slate-200 tracking-wider">
            <tr>
              <th
                className="py-3.5 px-4 cursor-pointer hover:text-indigo-600"
                onClick={() => handleSort('title')}
              >
                <div className="flex items-center gap-1.5">
                  Title
                  <ArrowUpDown className="w-3 h-3 text-slate-400" />
                </div>
              </th>
              <th
                className="py-3.5 px-4 cursor-pointer hover:text-indigo-600"
                onClick={() => handleSort('status')}
              >
                <div className="flex items-center gap-1.5">
                  Status
                  <ArrowUpDown className="w-3 h-3 text-slate-400" />
                </div>
              </th>
              <th
                className="py-3.5 px-4 cursor-pointer hover:text-indigo-600"
                onClick={() => handleSort('priority')}
              >
                <div className="flex items-center gap-1.5">
                  Priority
                  <ArrowUpDown className="w-3 h-3 text-slate-400" />
                </div>
              </th>
              <th
                className="py-3.5 px-4 cursor-pointer hover:text-indigo-600"
                onClick={() => handleSort('duedate')}
              >
                <div className="flex items-center gap-1.5">
                  Due Date
                  <ArrowUpDown className="w-3 h-3 text-slate-400" />
                </div>
              </th>
              <th className="py-3.5 px-4">Team</th>
              <th className="py-3.5 px-4">Assignee</th>
              <th className="py-3.5 px-4 text-center">Comments</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-slate-100">
            {items.length === 0 ? (
              <tr>
                <td colSpan={7} className="py-12 text-center text-slate-400">
                  No tasks found matching your criteria.
                </td>
              </tr>
            ) : (
              items.map((task) => {
                const isOverdue =
                  task.status !== 'Done' &&
                  new Date(task.dueDate).getTime() < new Date().setHours(0, 0, 0, 0);

                return (
                  <tr
                    key={task.id}
                    onClick={() => onTaskClick(task)}
                    className="hover:bg-slate-50/80 cursor-pointer transition-colors"
                  >
                    <td className="py-3.5 px-4 font-semibold text-slate-800">
                      <div className="max-w-md">
                        <span className="hover:text-indigo-600">{task.title}</span>
                        <p className="text-xs text-slate-400 truncate mt-0.5">{task.description}</p>
                      </div>
                    </td>
                    <td className="py-3.5 px-4">
                      <StatusBadge status={task.status} />
                    </td>
                    <td className="py-3.5 px-4">
                      <PriorityBadge priority={task.priority} />
                    </td>
                    <td className="py-3.5 px-4 whitespace-nowrap">
                      <span className={isOverdue ? 'text-rose-600 font-semibold' : 'text-slate-600'}>
                        {new Date(task.dueDate).toLocaleDateString()}
                      </span>
                    </td>
                    <td className="py-3.5 px-4 whitespace-nowrap">
                      {task.teamName ? (
                        <span className="text-xs bg-slate-100 text-slate-700 px-2 py-1 rounded-md">
                          {task.teamName}
                        </span>
                      ) : (
                        <span className="text-slate-400 text-xs italic">—</span>
                      )}
                    </td>
                    <td className="py-3.5 px-4 whitespace-nowrap">
                      {task.assignedToName ? (
                        <div className="flex items-center gap-2">
                          <div className="w-6 h-6 rounded-full bg-indigo-100 text-indigo-700 flex items-center justify-center text-[10px] font-bold">
                            {task.assignedToName[0].toUpperCase()}
                          </div>
                          <span className="text-xs font-medium text-slate-700">{task.assignedToName}</span>
                        </div>
                      ) : (
                        <span className="text-slate-400 text-xs italic">Unassigned</span>
                      )}
                    </td>
                    <td className="py-3.5 px-4 text-center">
                      <div className="inline-flex items-center gap-1 text-slate-400 text-xs">
                        <MessageSquare className="w-3.5 h-3.5" />
                        <span>{task.commentsCount}</span>
                      </div>
                    </td>
                  </tr>
                );
              })
            )}
          </tbody>
        </table>
      </div>

      {/* Pagination Footer */}
      <div className="flex items-center justify-between px-4 py-3 bg-slate-50 border-t border-slate-200 text-xs text-slate-600">
        <div>
          Showing <span className="font-semibold">{items.length > 0 ? (pageIndex - 1) * pageSize + 1 : 0}</span> to{' '}
          <span className="font-semibold">{Math.min(pageIndex * pageSize, totalCount)}</span> of{' '}
          <span className="font-semibold">{totalCount}</span> results
        </div>
        <div className="flex items-center gap-2">
          <button
            type="button"
            disabled={!hasPreviousPage}
            onClick={() => onFilterChange({ pageIndex: pageIndex - 1 })}
            className="p-1.5 rounded-lg border border-slate-200 bg-white text-slate-600 hover:bg-slate-100 disabled:opacity-40 disabled:cursor-not-allowed transition-colors"
          >
            <ChevronLeft className="w-4 h-4" />
          </button>
          <span className="font-medium px-2">
            Page {pageIndex} of {totalPages || 1}
          </span>
          <button
            type="button"
            disabled={!hasNextPage}
            onClick={() => onFilterChange({ pageIndex: pageIndex + 1 })}
            className="p-1.5 rounded-lg border border-slate-200 bg-white text-slate-600 hover:bg-slate-100 disabled:opacity-40 disabled:cursor-not-allowed transition-colors"
          >
            <ChevronRight className="w-4 h-4" />
          </button>
        </div>
      </div>
    </div>
  );
};
