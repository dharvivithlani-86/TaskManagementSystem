import React from 'react';
import { TaskItemStatus, TaskPriority, UserRole } from '../../types';

interface BadgeProps {
  children: React.ReactNode;
  variant?: 'slate' | 'blue' | 'amber' | 'emerald' | 'rose' | 'purple' | 'indigo' | 'teal';
  className?: string;
}

export const Badge: React.FC<BadgeProps> = ({ children, variant = 'slate', className = '' }) => {
  const variantStyles = {
    slate: 'bg-slate-100 text-slate-700 border-slate-200',
    blue: 'bg-blue-50 text-blue-700 border-blue-200',
    amber: 'bg-amber-50 text-amber-700 border-amber-200',
    emerald: 'bg-emerald-50 text-emerald-700 border-emerald-200',
    rose: 'bg-rose-50 text-rose-700 border-rose-200',
    purple: 'bg-purple-50 text-purple-700 border-purple-200',
    indigo: 'bg-indigo-50 text-indigo-700 border-indigo-200',
    teal: 'bg-teal-50 text-teal-700 border-teal-200',
  };

  return (
    <span
      className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium border ${variantStyles[variant]} ${className}`}
    >
      {children}
    </span>
  );
};

export const StatusBadge: React.FC<{ status: TaskItemStatus; className?: string }> = ({ status, className }) => {
  switch (status) {
    case 'ToDo':
      return <Badge variant="slate" className={className}>To Do</Badge>;
    case 'InProgress':
      return <Badge variant="amber" className={className}>In Progress</Badge>;
    case 'Done':
      return <Badge variant="emerald" className={className}>Done</Badge>;
    default:
      return <Badge variant="slate" className={className}>{status}</Badge>;
  }
};

export const PriorityBadge: React.FC<{ priority: TaskPriority; className?: string }> = ({ priority, className }) => {
  switch (priority) {
    case 'Low':
      return <Badge variant="slate" className={className}>Low</Badge>;
    case 'Medium':
      return <Badge variant="blue" className={className}>Medium</Badge>;
    case 'High':
      return <Badge variant="amber" className={className}>High</Badge>;
    case 'Urgent':
      return <Badge variant="rose" className={className}>Urgent</Badge>;
    default:
      return <Badge variant="slate" className={className}>{priority}</Badge>;
  }
};

export const RoleBadge: React.FC<{ role: UserRole; className?: string }> = ({ role, className }) => {
  switch (role) {
    case 'Admin':
      return <Badge variant="purple" className={className}>Admin</Badge>;
    case 'Manager':
      return <Badge variant="indigo" className={className}>Manager</Badge>;
    case 'User':
      return <Badge variant="teal" className={className}>Member</Badge>;
    default:
      return <Badge variant="slate" className={className}>{role}</Badge>;
  }
};
