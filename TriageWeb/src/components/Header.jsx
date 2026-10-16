import React from 'react';
import { UserCheck, ShieldCheck, Activity, Wifi, WifiOff, LogOut } from 'lucide-react';

export default function Header({ activeTab, setActiveTab, stats, currentUser, onLogout }) {
  const isLive = stats?.isLiveApi;
  const displayName = currentUser?.volledigeNaam || currentUser?.gebruikersnaam || "Hanna";
  const userRole = currentUser?.rol || "Trainee SEH";

  return (
    <header className="account-header">
      <div className="header-top">
        <div className="account-profile">
          <div className="avatar-circle">
            <span>{displayName.charAt(0).toUpperCase()}</span>
          </div>
          <div className="profile-info">
            <div className="profile-name-row">
              <h2>{displayName}</h2>
              <span className="badge-role">{userRole}</span>
              <span className="badge-level">Level {stats?.hoogsteLevel || 1}</span>
            </div>
            <p className="profile-subtext">
              Gebruiker: <strong>{currentUser?.gebruikersnaam}</strong> | Trainee ID: <strong>TR-2026-0{currentUser?.id || 1}</strong> | Opleiding: <strong>Protocol Triage VR</strong>
            </p>
          </div>
        </div>

        <div className="header-actions-right">
          <div className="api-status-pill">
            {isLive ? (
              <span className="status-live">
                <Wifi size={16} /> Verbonden met MySQL (Live)
              </span>
            ) : (
              <span className="status-mock" title="Draait op testdata totdat .NET API actief is">
                <WifiOff size={16} /> Lokale Dev Modus
              </span>
            )}
          </div>

          {onLogout && (
            <button className="btn-logout" onClick={onLogout} title="Afmelden van dit account">
              <LogOut size={16} /> Uitloggen
            </button>
          )}
        </div>
      </div>

      <nav className="header-nav">
        <button
          className={`nav-btn ${activeTab === 'dashboard' ? 'active' : ''}`}
          onClick={() => setActiveTab('dashboard')}
        >
          <Activity size={18} /> Voortgang & Percentages
        </button>
        <button
          className={`nav-btn ${activeTab === 'details' ? 'active' : ''}`}
          onClick={() => setActiveTab('details')}
        >
          <ShieldCheck size={18} /> Foutanalyses & Detailcases
        </button>
        <button
          className={`nav-btn ${activeTab === 'account' ? 'active' : ''}`}
          onClick={() => setActiveTab('account')}
        >
          <UserCheck size={18} /> Account & Doelstellingen
        </button>
      </nav>
    </header>
  );
}
