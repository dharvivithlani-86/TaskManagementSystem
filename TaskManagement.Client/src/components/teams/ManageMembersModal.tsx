import React, { useState } from 'react';
import { UserPlus, Trash2, Shield, User as UserIcon } from 'lucide-react';
import { teamApi } from '../../api/services';
import { Team, User } from '../../types';
import { RoleBadge } from '../common/Badge';
import { Modal } from '../common/Modal';

interface ManageMembersModalProps {
  team: Team | null;
  isOpen: boolean;
  onClose: () => void;
  onTeamUpdated: (team: Team) => void;
  availableUsers: User[];
}

export const ManageMembersModal: React.FC<ManageMembersModalProps> = ({
  team,
  isOpen,
  onClose,
  onTeamUpdated,
  availableUsers,
}) => {
  const [selectedUserId, setSelectedUserId] = useState<number | undefined>(undefined);
  const [isAdding, setIsAdding] = useState(false);
  const [error, setError] = useState<string | null>(null);

  if (!isOpen || !team) return null;

  // Filter users who are not yet members of this team
  const eligibleUsers = availableUsers.filter(
    (u) => !team.members.some((m) => m.userId === u.id)
  );

  const handleAddMember = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!selectedUserId || isAdding) return;

    try {
      setIsAdding(true);
      setError(null);
      await teamApi.addMember(team.id, selectedUserId);
      const updatedTeam = await teamApi.getTeamById(team.id);
      onTeamUpdated(updatedTeam);
      setSelectedUserId(undefined);
    } catch (err: any) {
      setError(err.message || 'Failed to add member');
    } finally {
      setIsAdding(false);
    }
  };

  const handleRemoveMember = async (userId: number) => {
    if (userId === team.leadManagerId) {
      setError('Cannot remove the team lead manager.');
      return;
    }

    try {
      setError(null);
      await teamApi.removeMember(team.id, userId);
      const updatedTeam = await teamApi.getTeamById(team.id);
      onTeamUpdated(updatedTeam);
    } catch (err: any) {
      setError(err.message || 'Failed to remove member');
    }
  };

  return (
    <Modal isOpen={isOpen} onClose={onClose} title={`Manage Members - ${team.name}`} maxWidth="lg">
      <div className="space-y-6">
        {error && (
          <div className="p-3 bg-rose-50 border border-rose-200 text-rose-700 rounded-xl text-xs">
            {error}
          </div>
        )}

        {/* Add Member Form */}
        <form onSubmit={handleAddMember} className="p-4 bg-slate-50 border border-slate-100 rounded-xl space-y-3">
          <label className="block text-xs font-semibold text-slate-700">Add New Team Member</label>
          <div className="flex gap-2">
            <select
              value={selectedUserId || ''}
              onChange={(e) => setSelectedUserId(e.target.value ? Number(e.target.value) : undefined)}
              className="flex-1 px-3 py-2 text-xs bg-white border border-slate-200 rounded-xl focus:outline-none focus:ring-2 focus:ring-indigo-500/20 text-slate-700"
            >
              <option value="">Select a user...</option>
              {eligibleUsers.map((u) => (
                <option key={u.id} value={u.id}>
                  {u.fullName} ({u.email}) - {u.role}
                </option>
              ))}
            </select>
            <button
              type="submit"
              disabled={!selectedUserId || isAdding}
              className="px-4 py-2 bg-indigo-600 hover:bg-indigo-700 text-white text-xs font-semibold rounded-xl flex items-center gap-1.5 transition-colors disabled:opacity-50 disabled:cursor-not-allowed shadow-xs"
            >
              <UserPlus className="w-3.5 h-3.5" />
              <span>{isAdding ? 'Adding...' : 'Add'}</span>
            </button>
          </div>
        </form>

        {/* Members List */}
        <div>
          <h4 className="text-xs font-semibold text-slate-400 uppercase tracking-wider mb-3">
            Current Members ({team.members.length})
          </h4>
          <div className="divide-y divide-slate-100 max-h-60 overflow-y-auto">
            {team.members.map((member) => {
              const isLead = member.userId === team.leadManagerId;
              return (
                <div key={member.userId} className="py-2.5 flex items-center justify-between gap-3">
                  <div className="flex items-center gap-2.5 min-w-0">
                    <div className="w-8 h-8 rounded-full bg-indigo-100 text-indigo-700 font-bold flex items-center justify-center text-xs">
                      {member.fullName[0].toUpperCase()}
                    </div>
                    <div className="min-w-0">
                      <div className="flex items-center gap-2">
                        <span className="text-xs font-semibold text-slate-800 truncate">
                          {member.fullName}
                        </span>
                        {isLead && (
                          <span className="text-[10px] font-bold bg-indigo-100 text-indigo-700 px-1.5 py-0.5 rounded flex items-center gap-0.5">
                            <Shield className="w-2.5 h-2.5" /> Lead
                          </span>
                        )}
                      </div>
                      <span className="text-[11px] text-slate-400 truncate block">{member.email}</span>
                    </div>
                  </div>

                  <div className="flex items-center gap-2">
                    <RoleBadge role={member.role} />
                    {!isLead && (
                      <button
                        type="button"
                        onClick={() => handleRemoveMember(member.userId)}
                        className="p-1.5 text-slate-400 hover:text-rose-600 hover:bg-rose-50 rounded-lg transition-colors"
                        title="Remove member"
                      >
                        <Trash2 className="w-3.5 h-3.5" />
                      </button>
                    )}
                  </div>
                </div>
              );
            })}
          </div>
        </div>

        <div className="flex justify-end pt-4 border-t border-slate-100">
          <button
            type="button"
            onClick={onClose}
            className="px-4 py-2 text-xs font-semibold text-slate-600 hover:bg-slate-100 rounded-xl transition-colors"
          >
            Close
          </button>
        </div>
      </div>
    </Modal>
  );
};
