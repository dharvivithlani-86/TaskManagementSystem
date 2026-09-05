import React, { useEffect, useState } from 'react';
import {
  CheckSquare,
  CircleDot,
  Clock,
  CheckCircle2,
  AlertTriangle,
  Users2,
  Calendar,
  ArrowRight,
  TrendingUp,
} from 'lucide-react';
import { dashboardApi } from '../api/services';
import { DashboardSummary, TaskItem } from '../types';
import { PriorityBadge, StatusBadge } from '../components/common/Badge';
import { TaskDetailModal } from '../components/tasks/TaskDetailModal';
import { Link } from 'react-router-dom';

export const DashboardPage: React.FC = () => {
  const [summary, setSummary] = useState<DashboardSummary | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [selectedTaskId, setSelectedTaskId] = useState<number | null>(null);

  useEffect(() => {
    loadDashboard();
  }, []);

  const loadDashboard = async () => {
    try {
      setIsLoading(true);
      setError(null);
      const data = await dashboardApi.getSummary();
      setSummary(data);
    } catch (err: any) {
      setError(err.message || 'Failed to load dashboard metrics');
    } finally {
      setIsLoading(false);
    }
  };

  const handleTaskUpdated = (updatedTask: TaskItem) => {
    loadDashboard();
  };

  if (isLoading) {
    return (
      <div className="flex items-center justify-center min-h-[400px]">
        <div className="text-slate-400 text-sm animate-pulse">Loading dashboard metrics...</div>
      </div>
    );
  }

  if (error || !summary) {
    return (
      <div className="p-6 bg-rose-50 border border-rose-200 rounded-2xl text-rose-700 text-sm">
        {error || 'Failed to load dashboard.'}
      </div>
    );
  }

  const completionRate =
    summary.totalTasks > 0 ? Math.round((summary.doneCount / summary.totalTasks) * 100) : 0;

  return (
    <div className="space-y-6">
      {/* Top Stat Cards */}
      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-5 gap-4">
        {/* Total Tasks */}
        <div className="bg-white p-5 rounded-2xl border border-slate-200/90 shadow-2xs">
          <div className="flex items-center justify-between">
            <span className="text-xs font-semibold text-slate-500 uppercase tracking-wider">
              Total Tasks
            </span>
            <div className="p-2 bg-indigo-50 text-indigo-600 rounded-xl">
              <CheckSquare className="w-4 h-4" />
            </div>
          </div>
          <div className="mt-3 flex items-baseline gap-2">
            <span className="text-2xl font-bold text-slate-900">{summary.totalTasks}</span>
          </div>
        </div>

        {/* To Do */}
        <div className="bg-white p-5 rounded-2xl border border-slate-200/90 shadow-2xs">
          <div className="flex items-center justify-between">
            <span className="text-xs font-semibold text-slate-500 uppercase tracking-wider">
              To Do
            </span>
            <div className="p-2 bg-slate-100 text-slate-600 rounded-xl">
              <CircleDot className="w-4 h-4" />
            </div>
          </div>
          <div className="mt-3 flex items-baseline gap-2">
            <span className="text-2xl font-bold text-slate-900">{summary.toDoCount}</span>
            <span className="text-xs text-slate-400 font-medium">pending</span>
          </div>
        </div>

        {/* In Progress */}
        <div className="bg-white p-5 rounded-2xl border border-slate-200/90 shadow-2xs">
          <div className="flex items-center justify-between">
            <span className="text-xs font-semibold text-slate-500 uppercase tracking-wider">
              In Progress
            </span>
            <div className="p-2 bg-amber-50 text-amber-600 rounded-xl">
              <Clock className="w-4 h-4" />
            </div>
          </div>
          <div className="mt-3 flex items-baseline gap-2">
            <span className="text-2xl font-bold text-slate-900">{summary.inProgressCount}</span>
            <span className="text-xs text-amber-600 font-medium">active</span>
          </div>
        </div>

        {/* Done */}
        <div className="bg-white p-5 rounded-2xl border border-slate-200/90 shadow-2xs">
          <div className="flex items-center justify-between">
            <span className="text-xs font-semibold text-slate-500 uppercase tracking-wider">
              Completed
            </span>
            <div className="p-2 bg-emerald-50 text-emerald-600 rounded-xl">
              <CheckCircle2 className="w-4 h-4" />
            </div>
          </div>
          <div className="mt-3 flex items-baseline gap-2">
            <span className="text-2xl font-bold text-slate-900">{summary.doneCount}</span>
            <span className="text-xs text-emerald-600 font-medium">{completionRate}% rate</span>
          </div>
        </div>

        {/* Overdue */}
        <div className="bg-white p-5 rounded-2xl border border-slate-200/90 shadow-2xs">
          <div className="flex items-center justify-between">
            <span className="text-xs font-semibold text-slate-500 uppercase tracking-wider">
              Overdue
            </span>
            <div className="p-2 bg-rose-50 text-rose-600 rounded-xl">
              <AlertTriangle className="w-4 h-4" />
            </div>
          </div>
          <div className="mt-3 flex items-baseline gap-2">
            <span className="text-2xl font-bold text-rose-600">{summary.overdueCount}</span>
            <span className="text-xs text-rose-500 font-medium">attention</span>
          </div>
        </div>
      </div>

      {/* Mid Section: Completion Rate Bar + Priority Breakdown */}
      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        {/* Status Distribution */}
        <div className="bg-white p-6 rounded-2xl border border-slate-200/90 shadow-2xs lg:col-span-2">
          <div className="flex items-center justify-between mb-4">
            <div>
              <h3 className="text-sm font-bold text-slate-900">Task Status Distribution</h3>
              <p className="text-xs text-slate-500 mt-0.5">Progress overview across active workflows</p>
            </div>
            <div className="flex items-center gap-1.5 text-xs font-bold text-indigo-600 bg-indigo-50 px-2.5 py-1 rounded-lg">
              <TrendingUp className="w-3.5 h-3.5" />
              <span>{completionRate}% Completed</span>
            </div>
          </div>

          {/* Multi-segmented Progress Bar */}
          <div className="h-4 w-full bg-slate-100 rounded-full overflow-hidden flex gap-0.5 mb-6">
            <div
              style={{ width: `${summary.statusBreakdown.find((s) => s.status === 'Done')?.percentage || 0}%` }}
              className="bg-emerald-500 transition-all duration-500"
              title="Done"
            />
            <div
              style={{ width: `${summary.statusBreakdown.find((s) => s.status === 'InProgress')?.percentage || 0}%` }}
              className="bg-amber-500 transition-all duration-500"
              title="In Progress"
            />
            <div
              style={{ width: `${summary.statusBreakdown.find((s) => s.status === 'ToDo')?.percentage || 0}%` }}
              className="bg-slate-300 transition-all duration-500"
              title="To Do"
            />
          </div>

          <div className="grid grid-cols-3 gap-4 pt-2 border-t border-slate-100 text-center">
            {summary.statusBreakdown.map((item) => (
              <div key={item.status}>
                <div className="flex items-center justify-center gap-1.5 mb-1">
                  <span
                    className={`w-2 h-2 rounded-full ${
                      item.status === 'Done'
                        ? 'bg-emerald-500'
                        : item.status === 'InProgress'
                        ? 'bg-amber-500'
                        : 'bg-slate-400'
                    }`}
                  />
                  <span className="text-xs font-medium text-slate-600">{item.label}</span>
                </div>
                <div className="text-lg font-bold text-slate-900">{item.count}</div>
                <span className="text-[11px] text-slate-400">{item.percentage}%</span>
              </div>
            ))}
          </div>
        </div>

        {/* Priority Breakdown */}
        <div className="bg-white p-6 rounded-2xl border border-slate-200/90 shadow-2xs">
          <h3 className="text-sm font-bold text-slate-900 mb-1">Priority Breakdown</h3>
          <p className="text-xs text-slate-500 mb-5">Tasks grouped by urgency</p>

          <div className="space-y-3">
            {summary.priorityBreakdown.map((p) => {
              const maxPriority = Math.max(...summary.priorityBreakdown.map((x) => x.count), 1);
              const barPercent = Math.round((p.count / maxPriority) * 100);

              return (
                <div key={p.priority} className="space-y-1">
                  <div className="flex items-center justify-between text-xs">
                    <PriorityBadge priority={p.priority} />
                    <span className="font-bold text-slate-800">{p.count}</span>
                  </div>
                  <div className="w-full h-2 bg-slate-100 rounded-full overflow-hidden">
                    <div
                      style={{ width: `${barPercent}%` }}
                      className={`h-full rounded-full transition-all duration-300 ${
                        p.priority === 'Urgent'
                          ? 'bg-rose-500'
                          : p.priority === 'High'
                          ? 'bg-amber-500'
                          : p.priority === 'Medium'
                          ? 'bg-blue-500'
                          : 'bg-slate-400'
                      }`}
                    />
                  </div>
                </div>
              );
            })}
          </div>
        </div>
      </div>

      {/* Bottom Section: Upcoming Deadlines & Recent Activities */}
      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        {/* Upcoming Deadlines */}
        <div className="bg-white p-6 rounded-2xl border border-slate-200/90 shadow-2xs lg:col-span-2">
          <div className="flex items-center justify-between mb-4">
            <div>
              <h3 className="text-sm font-bold text-slate-900">Upcoming Deadlines</h3>
              <p className="text-xs text-slate-500 mt-0.5">Tasks needing immediate attention</p>
            </div>
            <Link
              to="/tasks"
              className="text-xs font-semibold text-indigo-600 hover:text-indigo-800 flex items-center gap-1"
            >
              View all tasks <ArrowRight className="w-3.5 h-3.5" />
            </Link>
          </div>

          <div className="divide-y divide-slate-100">
            {summary.upcomingTasks.length === 0 ? (
              <div className="py-8 text-center text-xs text-slate-400">No pending tasks upcoming!</div>
            ) : (
              summary.upcomingTasks.map((t) => {
                const isOverdue = new Date(t.dueDate).getTime() < new Date().setHours(0, 0, 0, 0);

                return (
                  <div
                    key={t.id}
                    onClick={() => setSelectedTaskId(t.id)}
                    className="py-3 flex items-center justify-between gap-3 hover:bg-slate-50/80 px-2 rounded-xl cursor-pointer transition-colors"
                  >
                    <div className="min-w-0 flex-1">
                      <div className="flex items-center gap-2 mb-1">
                        <PriorityBadge priority={t.priority} />
                        <h4 className="text-xs font-bold text-slate-800 truncate">{t.title}</h4>
                      </div>
                      <div className="flex items-center gap-3 text-[11px] text-slate-400">
                        <span>Assignee: {t.assignedToName || 'Unassigned'}</span>
                        {t.teamName && <span>• Team: {t.teamName}</span>}
                      </div>
                    </div>

                    <div className="flex items-center gap-3">
                      <StatusBadge status={t.status} />
                      <div
                        className={`flex items-center gap-1 text-xs font-semibold ${
                          isOverdue ? 'text-rose-600' : 'text-slate-600'
                        }`}
                      >
                        <Calendar className="w-3.5 h-3.5" />
                        <span>{new Date(t.dueDate).toLocaleDateString()}</span>
                      </div>
                    </div>
                  </div>
                );
              })
            )}
          </div>
        </div>

        {/* Recent Activity Timeline */}
        <div className="bg-white p-6 rounded-2xl border border-slate-200/90 shadow-2xs">
          <h3 className="text-sm font-bold text-slate-900 mb-1">Recent Activity</h3>
          <p className="text-xs text-slate-500 mb-4">Latest events and updates</p>

          <div className="space-y-4">
            {summary.recentActivities.length === 0 ? (
              <div className="py-8 text-center text-xs text-slate-400">No recent activity</div>
            ) : (
              summary.recentActivities.map((act) => (
                <div key={act.id} className="flex gap-3 text-xs">
                  <div className="w-2 h-2 rounded-full bg-indigo-500 mt-1.5 shrink-0" />
                  <div className="min-w-0 flex-1">
                    <p className="font-semibold text-slate-800">{act.title}</p>
                    <p className="text-slate-500 text-[11px] mt-0.5 line-clamp-2">{act.message}</p>
                    <span className="text-[10px] text-slate-400 mt-1 block">
                      {new Date(act.timestamp).toLocaleDateString()} at{' '}
                      {new Date(act.timestamp).toLocaleTimeString([], {
                        hour: '2-digit',
                        minute: '2-digit',
                      })}
                    </span>
                  </div>
                </div>
              ))
            )}
          </div>
        </div>
      </div>

      {/* Task Details Modal */}
      <TaskDetailModal
        taskId={selectedTaskId}
        isOpen={selectedTaskId !== null}
        onClose={() => setSelectedTaskId(null)}
        onTaskUpdated={handleTaskUpdated}
      />
    </div>
  );
};
