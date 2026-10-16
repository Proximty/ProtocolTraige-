import React, { useState } from 'react';
import { User, Award, Target, Settings, Database, RefreshCw, CheckCircle2 } from 'lucide-react';

export default function AccountTab({ stats, onRefresh, isLive }) {
  const [userName, setUserName] = useState('Hanna');
  const [isSaved, setIsSaved] = useState(false);

  const handleSave = (e) => {
    e.preventDefault();
    setIsSaved(true);
    setTimeout(() => setIsSaved(false), 2500);
  };

  const levels = [
    { lvl: 1, titel: 'Beginnend Triagist', status: 'Voltooid', pct: '100%' },
    { lvl: 2, titel: 'Gevorderd Triage Verpleegkundige', status: 'Voltooid', pct: '100%' },
    { lvl: 3, titel: 'Acute Zorg Specialist', status: stats?.hoogsteLevel >= 3 ? 'Actief' : 'Vergrendeld', pct: '75%' },
    { lvl: 4, titel: 'Senior Trauma Coördinator', status: 'Vergrendeld', pct: '0%' },
    { lvl: 5, titel: 'Gecertificeerd NTS Triage Meester', status: 'Vergrendeld', pct: '0%' }
  ];

  return (
    <div className="account-tab-layout">
      {/* Linker kolom: Profiel & Doelstellingen */}
      <div className="account-col">
        <div className="account-card">
          <div className="card-heading">
            <User size={20} />
            <h3>Trainee Profiel</h3>
          </div>

          <form onSubmit={handleSave} className="profile-form">
            <div className="form-group">
              <label>Naam Triagist</label>
              <input
                type="text"
                value={userName}
                onChange={(e) => setUserName(e.target.value)}
                className="input-field"
              />
            </div>

            <div className="form-group">
              <label>Opleidingstraject</label>
              <input
                type="text"
                disabled
                value="Protocol Triage VR - Spoedeisende Hulp"
                className="input-field disabled"
              />
            </div>

            <div className="form-group">
              <label>Registratienummer (Trainee ID)</label>
              <input
                type="text"
                disabled
                value="TR-2026-0941"
                className="input-field disabled"
              />
            </div>

            <button type="submit" className="btn-save">
              Profiel Opslaan {isSaved && <CheckCircle2 size={16} />}
            </button>
            {isSaved && <span className="save-confirm">Opgeslagen!</span>}
          </form>
        </div>

        <div className="account-card">
          <div className="card-heading">
            <Target size={20} />
            <h3>Opleidingsdoelen (Normering)</h3>
          </div>
          <ul className="goals-list">
            <li className="goal-item achieved">
              <CheckCircle2 size={16} color="#10b981" />
              <div>
                <strong>Airway & Breathing &ge; 80%</strong>
                <p>Status: Behaald ({stats?.categoriePercentages?.airway ?? 0}% & {stats?.categoriePercentages?.breathing ?? 0}%)</p>
              </div>
            </li>
            <li className={`goal-item ${stats?.overallAccuratessePct >= 80 ? 'achieved' : 'in-progress'}`}>
              <CheckCircle2 size={16} color={stats?.overallAccuratessePct >= 80 ? '#10b981' : '#94a3b8'} />
              <div>
                <strong>Algemene Triage Score &ge; 80%</strong>
                <p>Status: Huidig {stats?.overallAccuratessePct ?? 0}% (Minimaal 80% vereist)</p>
              </div>
            </li>
            <li className="goal-item in-progress">
              <CheckCircle2 size={16} color="#94a3b8" />
              <div>
                <strong>Circulation & Exposure op niveau brengen</strong>
                <p>Status: Circulation {stats?.categoriePercentages?.circulation ?? 0}% en Exposure {stats?.categoriePercentages?.exposure ?? 0}%</p>
              </div>
            </li>
          </ul>
        </div>
      </div>

      {/* Rechter kolom: Level Progressie & Ontwikkelaarsinstellingen */}
      <div className="account-col">
        <div className="account-card">
          <div className="card-heading">
            <Award size={20} />
            <h3>Curriculum & Level Voortgang</h3>
          </div>
          <div className="levels-roadmap">
            {levels.map((l) => (
              <div key={l.lvl} className={`level-step ${l.status.toLowerCase()}`}>
                <div className="level-badge-num">{l.lvl}</div>
                <div className="level-step-info">
                  <strong>{l.titel}</strong>
                  <span className="step-status">Status: {l.status}</span>
                </div>
                <span className="step-pct">{l.pct}</span>
              </div>
            ))}
          </div>
        </div>

        <div className="account-card dev-settings">
          <div className="card-heading">
            <Settings size={20} />
            <h3>Ontwikkelaars & Systeem Status</h3>
          </div>
          <div className="dev-meta">
            <div className="meta-row">
              <span>Database Provider:</span>
              <strong>MySQL (`protocol_triage`)</strong>
            </div>
            <div className="meta-row">
              <span>Backend API:</span>
              <strong>.NET Minimal API (`localhost:5000`)</strong>
            </div>
            <div className="meta-row">
              <span>Dataverbinding:</span>
              <strong style={{ color: isLive ? '#10b981' : '#f59e0b' }}>
                {isLive ? 'Live Verbonden (MySQL)' : 'Lokale Mock / Fallback'}
              </strong>
            </div>
          </div>
          <button className="btn-refresh" onClick={onRefresh}>
            <RefreshCw size={15} /> Gegevens Handmatig Verversen
          </button>
        </div>
      </div>
    </div>
  );
}
