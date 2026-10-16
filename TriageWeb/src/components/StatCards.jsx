import React from 'react';
import { Target, CheckCircle2, Clock, Award } from 'lucide-react';

export default function StatCards({ stats }) {
  const overallPct = stats?.overallAccuratessePct ?? 0;
  const totaalCases = stats?.totaalCases ?? 0;
  const totaalCorrect = stats?.totaalCorrect ?? 0;
  const casesSlagingsPct = totaalCases > 0 ? Math.round((totaalCorrect / totaalCases) * 100) : 0;
  const avgTijd = stats?.gemiddeldeTijdSeconden ?? 0;
  const hoogsteLevel = stats?.hoogsteLevel ?? 1;

  // Kleur op basis van percentage (>=80% = groen, 60-79% = oranje, <60% = rood)
  const getScoreColor = (pct) => {
    if (pct >= 80) return '#10b981'; // groen
    if (pct >= 65) return '#f59e0b'; // oranje
    return '#ef4444'; // rood
  };

  return (
    <div className="stat-cards-grid">
      {/* Kaart 1: Overall Accuratesse % */}
      <div className="stat-card primary-score-card">
        <div className="card-top">
          <span className="card-title">Totale Triage Score</span>
          <Target className="card-icon" size={20} />
        </div>
        <div className="score-display">
          <div className="percentage-circle" style={{ borderColor: getScoreColor(overallPct) }}>
            <span className="percentage-number" style={{ color: getScoreColor(overallPct) }}>
              {overallPct}%
            </span>
          </div>
          <div className="score-meta">
            <span className="benchmark-text">
              {overallPct >= 80 ? '✅ Boven de slagnorm (80%)' : '⚠️ Onder de streefnorm (80%)'}
            </span>
            <p className="score-hint">
              Gebaseerd op alle individuele ABCDE-stappen en gekozen NTS-urgentieklasses.
            </p>
          </div>
        </div>
      </div>

      {/* Kaart 2: Slagingspercentage Cases */}
      <div className="stat-card">
        <div className="card-top">
          <span className="card-title">Volledig Foutloze Cases</span>
          <CheckCircle2 className="card-icon" size={20} />
        </div>
        <div className="metric-value-row">
          <span className="metric-huge">{casesSlagingsPct}%</span>
          <span className="metric-badge">{totaalCorrect} van de {totaalCases} goed</span>
        </div>
        <div className="progress-bar-bg">
          <div
            className="progress-bar-fill"
            style={{ width: `${casesSlagingsPct}%`, backgroundColor: getScoreColor(casesSlagingsPct) }}
          />
        </div>
        <span className="metric-footer">Doel: minimaal 85% van de cases foutloos</span>
      </div>

      {/* Kaart 3: Gemiddelde Beslissnelheid */}
      <div className="stat-card">
        <div className="card-top">
          <span className="card-title">Gemiddelde Triagetijd</span>
          <Clock className="card-icon" size={20} />
        </div>
        <div className="metric-value-row">
          <span className="metric-huge">{avgTijd}s</span>
          <span className="metric-badge speed">Streeftijd: &lt; 45s</span>
        </div>
        <div className="progress-bar-bg">
          <div
            className="progress-bar-fill"
            style={{
              width: `${Math.min(100, Math.round((45 / (avgTijd || 1)) * 80))}%`,
              backgroundColor: avgTijd <= 45 ? '#10b981' : '#f59e0b'
            }}
          />
        </div>
        <span className="metric-footer">
          {avgTijd <= 45 ? '⚡ Uitstekende beslissnelheid' : '⏱️ Neem niet te lang per patiënt'}
        </span>
      </div>

      {/* Kaart 4: Level Voortgang */}
      <div className="stat-card">
        <div className="card-top">
          <span className="card-title">Huidig Hoogste Level</span>
          <Award className="card-icon" size={20} />
        </div>
        <div className="metric-value-row">
          <span className="metric-huge">Lvl {hoogsteLevel}</span>
          <span className="metric-badge level">Sessies: {stats?.totaalSessies ?? 0}</span>
        </div>
        <div className="progress-bar-bg">
          <div
            className="progress-bar-fill"
            style={{ width: `${Math.min(100, (hoogsteLevel / 5) * 100)}%`, backgroundColor: '#3b82f6' }}
          />
        </div>
        <span className="metric-footer">Level 5 ontgrendelt senior triage certificaat</span>
      </div>
    </div>
  );
}
