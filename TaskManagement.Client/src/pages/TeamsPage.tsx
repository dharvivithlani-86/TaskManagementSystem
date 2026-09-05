import React, { useEffect, useState } from 'react';
import { Plus, Users2, RefreshCw, AlertCircle } from 'lucide-react';
import { teamApi, userApi } from '../api/services';
import { Team, User } from '../types';
import { TeamCard } from '../components/teams/TeamCard';
import { CreateTeamModal } from '../components/teams/CreateTeamModal';
import { ManageMembersModal } from '../components/teams/ManageMembersModal';
import { useAuth } from '../context/AuthContext';

export const TeamsPage: React.FC = () => {
  const { isManager, isAdmin } = useAuth();
  const [teams, setTeams] = useState<Team[]>([]);
  const [users, setUsers] = useState<User[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const [isCreateModalOpen, setIsCreateModalOpen] = useState(false);
  const [selectedTeamForMembers, setSelectedTeamForMembers] = useState<Team | null>(null);

  useEffect(() => {
    loadTeamsAndUsers();
  }, []);

  const loadTeamsAndUsers = async () => {
    try {
      setIsLoading(true);
      setError(null);
      const [teamsData, usersData] = await Promise.all([
        teamApi.getTeams(),
        userApi.getUsers(),
      ]);
      setTeams(teamsData);
      setUsers(usersData);
    } catch (err: any) {
      setError(err.message || 'Failed to load teams');
    } finally {
      setIsLoading(false);
    }
  };

  const handleTeamCreated = (newTeam: Team) => {
    setTeams((prev) => [...prev, newTeam]);
  };

  const handleTeamUpdated = (updatedTeam: Team) => {
    setTeams((prev) => prev.map((t) => (t.id === updatedTeam.id ? updatedTeam : t)));
    if (selectedTeamForMembers?.id === updatedTeam.id) {
      setSelectedTeamForMembers(updatedTeam);
    }
  };

  const handleDeleteTeam = async (teamId: number) => {
    if (!confirm('Are you sure you want to delete this team?')) return;
    try {
      await teamApi.deleteTeam(teamId);
      setTeams((prev) => prev.filter((t) => t.id !== teamId));
    } catch (err: any) {
      setError(err.message || 'Failed to delete team');
    }
  };

  const managers = users.filter((u) => u.role === 'Manager' || u.role === 'Admin');

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
        <div>
          <h2 className="text-xl font-bold text-slate-900 tracking-tight">Team Management</h2>
          <p className="text-xs text-slate-500 mt-0.5">
            Create departments, squads, and organize member assignments
          </p>
        </div>

        <div className="flex items-center gap-3">
          <button
            type="button"
            onClick={loadTeamsAndUsers}
            className="p-2 bg-white text-slate-600 hover:text-slate-900 border border-slate-200 rounded-xl shadow-2xs transition-colors"
            title="Refresh teams"
          >
            <RefreshCw className={`w-4 h-4 ${isLoading ? 'animate-spin' : ''}`} />
          </button>

          {isManager && (
            <button
              type="button"
              onClick={() => setIsCreateModalOpen(true)}
              className="flex items-center gap-1.5 px-4 py-2 bg-indigo-600 hover:bg-indigo-700 text-white text-xs font-semibold rounded-xl shadow-md shadow-indigo-600/20 transition-all"
            >
              <Plus className="w-4 h-4" />
              <span>Create Team</span>
            </button>
          )}
        </div>
      </div>

      {error && (
        <div className="p-4 bg-rose-50 border border-rose-200 text-rose-700 rounded-xl text-xs flex items-center gap-2">
          <AlertCircle className="w-4 h-4 shrink-0" />
          <span>{error}</span>
        </div>
      )}

      {/* Grid of Teams */}
      {isLoading ? (
        <div className="flex items-center justify-center min-h-[300px]">
          <div className="text-slate-400 text-sm animate-pulse">Loading teams...</div>
        </div>
      ) : teams.length === 0 ? (
        <div className="bg-white rounded-2xl border border-slate-200 p-12 text-center text-slate-400">
          <Users2 className="w-12 h-12 mx-auto mb-3 text-slate-300" />
          <p className="text-sm font-semibold text-slate-700">No teams created yet</p>
          <p className="text-xs text-slate-400 mt-1">
            Click &quot;Create Team&quot; above to organize users into project squads.
          </p>
        </div>
      ) : (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
          {teams.map((team) => (
            <TeamCard
              key={team.id}
              team={team}
              onManageMembers={(t) => setSelectedTeamForMembers(t)}
              onDeleteTeam={isAdmin ? handleDeleteTeam : undefined}
            />
          ))}
        </div>
      )}

      {/* Create Team Modal */}
      <CreateTeamModal
        isOpen={isCreateModalOpen}
        onClose={() => setIsCreateModalOpen(false)}
        onTeamCreated={handleTeamCreated}
        managers={managers}
      />

      {/* Manage Members Modal */}
      <ManageMembersModal
        team={selectedTeamForMembers}
        isOpen={selectedTeamForMembers !== null}
        onClose={() => setSelectedTeamForMembers(null)}
        onTeamUpdated={handleTeamUpdated}
        availableUsers={users}
      />
    </div>
  );
};
