import React from 'react';
import { Users, CheckSquare, Shield, Plus, Trash2 } from 'lucide-react';
import { Team } from '../../types';
import { RoleBadge } from '../common/Badge';
import { useAuth } from '../../context/AuthContext';

interface TeamCardProps {
  team: Team;
  onManageMembers: (team: Team) => void;
  onDeleteTeam?: (teamId: number) => void;
}

export const TeamCard: React.FC<TeamCardProps> = ({ team, onManageMembers, onDeleteTeam }) => {
  const { user, isAdmin } = useAuth();
  const canManage = isAdmin || team.leadManagerId === user?.id;

  return (
    <div className="bg-white rounded-2xl border border-slate-200/90 shadow-2xs hover:shadow-md transition-all duration-200 p-5 flex flex-col justify-between">
      <div>
        <div className="flex items-start justify-between gap-2 mb-2">
          <h3 className="text-base font-bold text-slate-900 tracking-tight">{team.name}</h3>
          {canManage && onDeleteTeam && (
            <button
              type="button"
              onClick={() => onDeleteTeam(team.id)}
              className="p-1 text-slate-400 hover:text-rose-600 rounded-lg hover:bg-rose-50 transition-colors"
              title="Delete Team"
            >
              <Trash2 className="w-4 h-4" />
            </button>
          )}
        </div>

        {team.description && (
          <p className="text-xs text-slate-500 line-clamp-2 leading-relaxed mb-4">
            {team.description}
          </p>
        )}

        {/* Lead manager info */}
        <div className="flex items-center gap-2 p-2.5 bg-slate-50 rounded-xl mb-4 border border-slate-100">
          <Shield className="w-4 h-4 text-indigo-600 shrink-0" />
          <div className="min-w-0 flex-1 text-xs">
            <span className="text-slate-400 block text-[10px] font-semibold uppercase">Lead Manager</span>
            <span className="font-semibold text-slate-800 truncate block">{team.leadManagerName}</span>
          </div>
        </div>

        {/* Stats Row */}
        <div className="grid grid-cols-2 gap-2 mb-4 text-xs">
          <div className="flex items-center gap-2 p-2 bg-slate-50/70 rounded-lg text-slate-600">
            <Users className="w-4 h-4 text-slate-400" />
            <span>
              <strong className="text-slate-800">{team.memberCount}</strong> Members
            </span>
          </div>
          <div className="flex items-center gap-2 p-2 bg-slate-50/70 rounded-lg text-slate-600">
            <CheckSquare className="w-4 h-4 text-slate-400" />
            <span>
              <strong className="text-slate-800">{team.activeTaskCount}</strong> Active Tasks
            </span>
          </div>
        </div>

        {/* Member preview chips */}
        <div className="space-y-1.5">
          <span className="text-[11px] font-semibold text-slate-400 uppercase tracking-wider block">
            Team Members
          </span>
          <div className="flex flex-wrap gap-1.5 max-h-24 overflow-y-auto">
            {team.members.map((m) => (
              <span
                key={m.userId}
                className="inline-flex items-center gap-1.5 px-2 py-1 bg-slate-100 text-slate-700 rounded-lg text-[11px]"
              >
                <span className="w-4 h-4 rounded-full bg-slate-200 text-slate-700 flex items-center justify-center text-[9px] font-bold">
                  {m.fullName[0].toUpperCase()}
                </span>
                <span>{m.fullName}</span>
              </span>
            ))}
          </div>
        </div>
      </div>

      {canManage && (
        <div className="mt-5 pt-3 border-t border-slate-100">
          <button
            type="button"
            onClick={() => onManageMembers(team)}
            className="w-full py-2 px-3 bg-indigo-50 hover:bg-indigo-100 text-indigo-700 text-xs font-semibold rounded-xl flex items-center justify-center gap-1.5 transition-colors"
          >
            <Plus className="w-3.5 h-3.5" />
            <span>Manage Team Members</span>
          </button>
        </div>
      )}
    </div>
  );
};
