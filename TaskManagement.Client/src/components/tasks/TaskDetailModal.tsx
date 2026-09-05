import React, { useState, useEffect } from 'react';
import {
  Calendar,
  User as UserIcon,
  MessageSquare,
  Clock,
  Send,
  Trash2,
  AlertCircle,
} from 'lucide-react';
import { commentApi, taskApi } from '../../api/services';
import { Comment, TaskItem, TaskItemStatus } from '../../types';
import { PriorityBadge, RoleBadge, StatusBadge } from '../common/Badge';
import { Modal } from '../common/Modal';
import { useAuth } from '../../context/AuthContext';

interface TaskDetailModalProps {
  taskId: number | null;
  isOpen: boolean;
  onClose: () => void;
  onTaskUpdated: (updatedTask: TaskItem) => void;
}

export const TaskDetailModal: React.FC<TaskDetailModalProps> = ({
  taskId,
  isOpen,
  onClose,
  onTaskUpdated,
}) => {
  const { user, isAdmin, isManager } = useAuth();
  const [task, setTask] = useState<TaskItem | null>(null);
  const [comments, setComments] = useState<Comment[]>([]);
  const [newComment, setNewComment] = useState('');
  const [isLoading, setIsLoading] = useState(false);
  const [isSubmittingComment, setIsSubmittingComment] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (taskId && isOpen) {
      loadTaskAndComments(taskId);
    } else {
      setTask(null);
      setComments([]);
      setError(null);
    }
  }, [taskId, isOpen]);

  const loadTaskAndComments = async (id: number) => {
    try {
      setIsLoading(true);
      setError(null);
      const [taskData, commentsData] = await Promise.all([
        taskApi.getTaskById(id),
        commentApi.getComments(id),
      ]);
      setTask(taskData);
      setComments(commentsData);
    } catch (err: any) {
      setError(err.message || 'Failed to load task details');
    } finally {
      setIsLoading(false);
    }
  };

  const handleStatusChange = async (newStatus: TaskItemStatus) => {
    if (!task) return;
    try {
      const updated = await taskApi.updateStatus(task.id, newStatus);
      setTask(updated);
      onTaskUpdated(updated);
    } catch (err: any) {
      setError(err.message || 'Failed to update status');
    }
  };

  const handleAddComment = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!task || !newComment.trim() || isSubmittingComment) return;

    try {
      setIsSubmittingComment(true);
      const createdComment = await commentApi.addComment(task.id, newComment.trim());
      setComments((prev) => [...prev, createdComment]);
      setNewComment('');
      
      const updatedTask = { ...task, commentsCount: task.commentsCount + 1 };
      setTask(updatedTask);
      onTaskUpdated(updatedTask);
    } catch (err: any) {
      setError(err.message || 'Failed to add comment');
    } finally {
      setIsSubmittingComment(false);
    }
  };

  const handleDeleteComment = async (commentId: number) => {
    if (!task) return;
    try {
      await commentApi.deleteComment(commentId);
      setComments((prev) => prev.filter((c) => c.id !== commentId));
      const updatedTask = { ...task, commentsCount: Math.max(0, task.commentsCount - 1) };
      setTask(updatedTask);
      onTaskUpdated(updatedTask);
    } catch (err: any) {
      setError(err.message || 'Failed to delete comment');
    }
  };

  if (!isOpen) return null;

  return (
    <Modal isOpen={isOpen} onClose={onClose} title={task?.title || 'Task Details'} maxWidth="2xl">
      {isLoading ? (
        <div className="py-16 text-center text-slate-400">Loading task details...</div>
      ) : error && !task ? (
        <div className="p-4 bg-rose-50 text-rose-700 rounded-xl text-sm flex items-center gap-2">
          <AlertCircle className="w-4 h-4 shrink-0" />
          <span>{error}</span>
        </div>
      ) : task ? (
        <div className="space-y-6">
          {error && (
            <div className="p-3 bg-rose-50 border border-rose-200 text-rose-700 rounded-xl text-xs">
              {error}
            </div>
          )}

          {/* Header Metadata Grid */}
          <div className="grid grid-cols-2 sm:grid-cols-4 gap-4 p-4 bg-slate-50/70 rounded-xl border border-slate-100">
            <div>
              <span className="text-[11px] font-semibold text-slate-400 uppercase tracking-wider block">
                Status
              </span>
              <div className="mt-1">
                <select
                  value={task.status}
                  onChange={(e) => handleStatusChange(e.target.value as TaskItemStatus)}
                  className="text-xs font-semibold bg-white border border-slate-200 rounded-lg px-2 py-1 focus:outline-none focus:ring-2 focus:ring-indigo-500/20"
                >
                  <option value="ToDo">To Do</option>
                  <option value="InProgress">In Progress</option>
                  <option value="Done">Done</option>
                </select>
              </div>
            </div>

            <div>
              <span className="text-[11px] font-semibold text-slate-400 uppercase tracking-wider block">
                Priority
              </span>
              <div className="mt-1">
                <PriorityBadge priority={task.priority} />
              </div>
            </div>

            <div>
              <span className="text-[11px] font-semibold text-slate-400 uppercase tracking-wider block">
                Due Date
              </span>
              <div className="mt-1 flex items-center gap-1.5 text-xs text-slate-700 font-medium">
                <Calendar className="w-3.5 h-3.5 text-slate-400" />
                <span>{new Date(task.dueDate).toLocaleDateString()}</span>
              </div>
            </div>

            <div>
              <span className="text-[11px] font-semibold text-slate-400 uppercase tracking-wider block">
                Team
              </span>
              <div className="mt-1 text-xs text-slate-700 font-medium truncate">
                {task.teamName || 'None'}
              </div>
            </div>
          </div>

          {/* Description */}
          <div>
            <h4 className="text-xs font-semibold text-slate-400 uppercase tracking-wider mb-2">
              Description
            </h4>
            <div className="text-sm text-slate-700 leading-relaxed bg-white p-4 rounded-xl border border-slate-100 whitespace-pre-wrap">
              {task.description}
            </div>
          </div>

          {/* People involved */}
          <div className="flex flex-wrap items-center gap-6 pt-2 text-xs text-slate-500 border-t border-slate-100">
            <div>
              <span className="text-slate-400 font-medium">Created by: </span>
              <span className="font-semibold text-slate-800">{task.createdByName}</span>
            </div>
            <div>
              <span className="text-slate-400 font-medium">Assignee: </span>
              <span className="font-semibold text-slate-800">
                {task.assignedToName || 'Unassigned'}
              </span>
            </div>
          </div>

          {/* Collaboration / Comments Section */}
          <div className="pt-4 border-t border-slate-100">
            <div className="flex items-center gap-2 mb-4">
              <MessageSquare className="w-4 h-4 text-indigo-600" />
              <h4 className="text-sm font-bold text-slate-800 tracking-tight">
                Comments & Collaboration ({comments.length})
              </h4>
            </div>

            {/* Comments List */}
            <div className="space-y-3 mb-4 max-h-64 overflow-y-auto pr-1">
              {comments.length === 0 ? (
                <div className="py-6 text-center text-xs text-slate-400">
                  No comments yet. Start the conversation below!
                </div>
              ) : (
                comments.map((comment) => (
                  <div
                    key={comment.id}
                    className="p-3 bg-slate-50 border border-slate-100 rounded-xl space-y-1"
                  >
                    <div className="flex items-center justify-between">
                      <div className="flex items-center gap-2">
                        <span className="text-xs font-bold text-slate-800">{comment.userName}</span>
                        <RoleBadge role={comment.userRole} />
                      </div>
                      <div className="flex items-center gap-2">
                        <span className="text-[10px] text-slate-400">
                          {new Date(comment.createdAt).toLocaleDateString()}{' '}
                          {new Date(comment.createdAt).toLocaleTimeString([], {
                            hour: '2-digit',
                            minute: '2-digit',
                          })}
                        </span>
                        {(isAdmin || comment.userId === user?.id) && (
                          <button
                            type="button"
                            onClick={() => handleDeleteComment(comment.id)}
                            className="p-1 text-slate-400 hover:text-rose-600 rounded transition-colors"
                          >
                            <Trash2 className="w-3 h-3" />
                          </button>
                        )}
                      </div>
                    </div>
                    <p className="text-xs text-slate-700 leading-relaxed whitespace-pre-wrap">
                      {comment.content}
                    </p>
                  </div>
                ))
              )}
            </div>

            {/* Add Comment Box */}
            <form onSubmit={handleAddComment} className="flex gap-2">
              <input
                type="text"
                placeholder="Write a comment..."
                value={newComment}
                onChange={(e) => setNewComment(e.target.value)}
                className="flex-1 px-3 py-2 text-xs bg-slate-50 border border-slate-200 rounded-xl focus:outline-none focus:ring-2 focus:ring-indigo-500/20 focus:border-indigo-500 transition-all"
              />
              <button
                type="submit"
                disabled={!newComment.trim() || isSubmittingComment}
                className="px-4 py-2 bg-indigo-600 hover:bg-indigo-700 text-white rounded-xl text-xs font-semibold flex items-center gap-1.5 transition-colors disabled:opacity-50 disabled:cursor-not-allowed shadow-xs"
              >
                <Send className="w-3.5 h-3.5" />
                <span>Post</span>
              </button>
            </form>
          </div>
        </div>
      ) : null}
    </Modal>
  );
};
