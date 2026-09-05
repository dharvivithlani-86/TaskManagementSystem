import React from 'react';
import { Calendar, MessageSquare, User as UserIcon, ArrowRight, Check } from 'lucide-react';
import { TaskItem, TaskItemStatus } from '../../types';
import { PriorityBadge, StatusBadge } from '../common/Badge';

interface TaskCardProps {
  task: TaskItem;
  onClick: (task: TaskItem) => void;
  onStatusChange?: (taskId: number, newStatus: TaskItemStatus) => void;
}

export const TaskCard: React.FC<TaskCardProps> = ({ task, onClick, onStatusChange }) => {
  const isOverdue =
    task.status !== 'Done' && new Date(task.dueDate).getTime() < new Date().setHours(0, 0, 0, 0);

  const formattedDate = new Date(task.dueDate).toLocaleDateString(undefined, {
    month: 'short',
    day: 'numeric',
  });

  return (
    <div
      onClick={() => onClick(task)}
      className="group bg-white rounded-2xl p-4 border border-slate-200/90 shadow-xs hover:shadow-md hover:border-indigo-200 transition-all duration-200 cursor-pointer flex flex-col justify-between"
    >
      <div>
        <div className="flex items-center justify-between gap-2 mb-2.5">
          <PriorityBadge priority={task.priority} />
          {task.teamName && (
            <span className="text-[11px] font-medium text-slate-500 truncate max-w-[120px] bg-slate-100 px-2 py-0.5 rounded-md">
              {task.teamName}
            </span>
          )}
        </div>

        <h4 className="text-sm font-semibold text-slate-900 group-hover:text-indigo-600 transition-colors line-clamp-2">
          {task.title}
        </h4>

        <p className="mt-1 text-xs text-slate-500 line-clamp-2 leading-relaxed">
          {task.description}
        </p>
      </div>

      <div className="mt-4 pt-3 border-t border-slate-100">
        <div className="flex items-center justify-between gap-2 text-xs">
          {/* Due date */}
          <div
            className={`flex items-center gap-1.5 font-medium ${
              isOverdue ? 'text-rose-600' : 'text-slate-500'
            }`}
          >
            <Calendar className="w-3.5 h-3.5" />
            <span>{formattedDate}</span>
            {isOverdue && <span className="text-[10px] font-bold uppercase tracking-wider">(Overdue)</span>}
          </div>

          <div className="flex items-center gap-3">
            {/* Comments count */}
            {task.commentsCount > 0 && (
              <div className="flex items-center gap-1 text-slate-400">
                <MessageSquare className="w-3.5 h-3.5" />
                <span className="text-[11px] font-medium">{task.commentsCount}</span>
              </div>
            )}

            {/* Assignee Avatar */}
            {task.assignedToName ? (
              <div
                title={`Assigned to ${task.assignedToName}`}
                className="w-6 h-6 rounded-full bg-indigo-100 border border-indigo-200 text-indigo-700 flex items-center justify-center text-[10px] font-bold"
              >
                {task.assignedToName[0].toUpperCase()}
              </div>
            ) : (
              <div
                title="Unassigned"
                className="w-6 h-6 rounded-full bg-slate-100 text-slate-400 flex items-center justify-center text-[10px]"
              >
                <UserIcon className="w-3.5 h-3.5" />
              </div>
            )}
          </div>
        </div>

        {/* Quick status transitions */}
        {onStatusChange && (
          <div className="mt-3 pt-2 border-t border-slate-50 flex items-center justify-end gap-1.5 opacity-0 group-hover:opacity-100 transition-opacity">
            {task.status === 'ToDo' && (
              <button
                type="button"
                onClick={(e) => {
                  e.stopPropagation();
                  onStatusChange(task.id, 'InProgress');
                }}
                className="text-[11px] font-medium text-indigo-600 bg-indigo-50 hover:bg-indigo-100 px-2.5 py-1 rounded-lg flex items-center gap-1 transition-colors"
              >
                Start <ArrowRight className="w-3 h-3" />
              </button>
            )}
            {task.status === 'InProgress' && (
              <button
                type="button"
                onClick={(e) => {
                  e.stopPropagation();
                  onStatusChange(task.id, 'Done');
                }}
                className="text-[11px] font-medium text-emerald-600 bg-emerald-50 hover:bg-emerald-100 px-2.5 py-1 rounded-lg flex items-center gap-1 transition-colors"
              >
                Mark Done <Check className="w-3 h-3" />
              </button>
            )}
            {task.status === 'Done' && (
              <button
                type="button"
                onClick={(e) => {
                  e.stopPropagation();
                  onStatusChange(task.id, 'InProgress');
                }}
                className="text-[11px] font-medium text-slate-600 bg-slate-100 hover:bg-slate-200 px-2 py-1 rounded-lg transition-colors"
              >
                Reopen
              </button>
            )}
          </div>
        )}
      </div>
    </div>
  );
};
