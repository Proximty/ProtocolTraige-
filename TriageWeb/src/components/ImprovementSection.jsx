import React from 'react';
import { CATEGORIE_INFO, VERBETER_TIPS } from '../constants/triage';
import { AlertCircle, TrendingUp, CheckCircle, ArrowRight } from 'lucide-react';

export default function ImprovementSection({ percentages, onViewDetails }) {
  // Filter categorien die onder de 80% norm scoren
  const keys = ['airway', 'breathing', 'circulation', 'disability', 'exposure', 'urgentie'];
  const allCategorien = keys.map((key) => ({
    key,
    label: CATEGORIE_INFO[key]?.label || key,
    score: percentages?.[key] ?? 0,
    tip: VERBETER_TIPS[key] || 'Blijf oefenen met deze categorie in de VR-omgeving.'
  }));

  // Sorteer op laagste percentage eerst (grootste verbeterpotentieel)
  const verbeterPunten = allCategorien
    .filter((c) => c.score < 80)
    .sort((a, b) => a.score - b.score);

  return (
    <div className="improvement-section-card">
      <div className="improvement-header">
        <div className="title-row">
          <div className="badge-icon-alert">
            <TrendingUp size={20} />
          </div>
          <div>
            <h3>Waar Moet Je Verbeteren? (Actionable Insights)</h3>
            <p className="subtitle">
              Onderdelen die onder de 80% streefnorm liggen, gesorteerd op hoogste prioriteit.
            </p>
          </div>
        </div>
      </div>

      {verbeterPunten.length === 0 ? (
        <div className="all-good-box">
          <CheckCircle size={32} color="#10b981" />
          <div>
            <h4>Uitstekend werk! Geen kritieke verbeterpunten</h4>
            <p>Al je categorieën scoren momenteel 80% of hoger. Blijf je snelheid en routine aanscherpen!</p>
          </div>
        </div>
      ) : (
        <div className="improvement-grid">
          {verbeterPunten.map((item) => {
            const tekort = Math.round(80 - item.score);
            const isCritical = item.score < 65;

            return (
              <div key={item.key} className={`improvement-item ${isCritical ? 'critical' : 'warning'}`}>
                <div className="item-header">
                  <div className="item-title-group">
                    <span className="item-label">{item.label}</span>
                    <span className={`priority-tag ${isCritical ? 'high' : 'medium'}`}>
                      {isCritical ? 'Prioriteit: Hoog' : 'Prioriteit: Gemiddeld'}
                    </span>
                  </div>
                  <div className="item-scores">
                    <span className="current-score">{item.score}%</span>
                    <span className="target-diff">(-{tekort}% tot norm)</span>
                  </div>
                </div>

                <div className="progress-mini-track">
                  <div
                    className="progress-mini-fill"
                    style={{
                      width: `${item.score}%`,
                      backgroundColor: isCritical ? '#ef4444' : '#f59e0b'
                    }}
                  />
                  <div className="norm-marker" style={{ left: '80%' }} title="80% Norm" />
                </div>

                <div className="item-advice">
                  <AlertCircle size={16} className="advice-icon" />
                  <p>{item.tip}</p>
                </div>

                <button
                  className="btn-drilldown"
                  onClick={() => onViewDetails && onViewDetails(item.key)}
                >
                  Bekijk foutdetails in cases <ArrowRight size={14} />
                </button>
              </div>
            );
          })}
        </div>
      )}
    </div>
  );
}
