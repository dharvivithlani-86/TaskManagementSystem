import React from 'react';
import { TaskItem, TaskItemStatus } from '../../types';
import { TaskCard } from './TaskCard';
import { CircleDot, Clock, CheckCircle2 } from 'lucide-react';

interface KanbanBoardProps {
  tasks: TaskItem[];
  onTaskClick: (task: TaskItem) => void;
  onStatusChange: (taskId: number, newStatus: TaskItemStatus) => void;
}

export const KanbanBoard: React.FC<KanbanBoardProps> = ({ tasks, onTaskClick, onStatusChange }) => {
  const columns: { status: TaskItemStatus; label: string; icon: any; color: string; bg: string }[] = [
    {
      status: 'ToDo',
      label: 'To Do',
      icon: CircleDot,
      color: 'text-slate-600',
      bg: 'bg-slate-100/80',
    },
    {
      status: 'InProgress',
      label: 'In Progress',
      icon: Clock,
      color: 'text-amber-600',
      bg: 'bg-amber-50/60',
    },
    {
      status: 'Done',
      label: 'Done',
      icon: CheckCircle2,
      color: 'text-emerald-600',
      bg: 'bg-emerald-50/60',
    },
  ];

  return (
    <div className="grid grid-cols-1 md:grid-cols-3 gap-6 items-start">
      {columns.map((col) => {
        const colTasks = tasks.filter((t) => t.status === col.status);
        const Icon = col.icon;

        return (
          <div
            key={col.status}
            className="flex flex-col bg-slate-100/70 border border-slate-200/80 rounded-2xl p-4 min-h-[500px]"
          >
            {/* Column Header */}
            <div className="flex items-center justify-between pb-3 mb-3 border-b border-slate-200">
              <div className="flex items-center gap-2">
                <Icon className={`w-4 h-4 ${col.color}`} />
                <h3 className="text-sm font-bold text-slate-800 tracking-tight">{col.label}</h3>
              </div>
              <span className="px-2 py-0.5 text-xs font-semibold bg-white border border-slate-200 text-slate-600 rounded-full shadow-2xs">
                {colTasks.length}
              </span>
            </div>

            {/* Task list in column */}
            <div className="flex-1 space-y-3 overflow-y-auto max-h-[calc(100vh-280px)] pr-1">
              {colTasks.length === 0 ? (
                <div className="flex flex-col items-center justify-center py-12 text-center text-slate-400">
                  <p className="text-xs">No tasks in {col.label.toLowerCase()}</p>
                </div>
              ) : (
                colTasks.map((task) => (
                  <TaskCard
                    key={task.id}
                    task={task}
                    onClick={onTaskClick}
                    onStatusChange={onStatusChange}
                  />
                ))
              )}
            </div>
          </div>
        );
      })}
    </div>
  );
};
