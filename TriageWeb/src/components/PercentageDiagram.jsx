import React from 'react';
import { CATEGORIE_INFO } from '../constants/triage';
import { Wind, Activity, Heart, Brain, Thermometer, AlertTriangle, HelpCircle } from 'lucide-react';

const ICON_MAP = {
  Wind: Wind,
  Activity: Activity,
  Heart: Heart,
  Brain: Brain,
  Thermometer: Thermometer,
  AlertTriangle: AlertTriangle
};

export default function PercentageDiagram({ percentages, onSelectCategory }) {
  const categories = [
    { key: 'airway', ...CATEGORIE_INFO.airway, value: percentages?.airway ?? 0 },
    { key: 'breathing', ...CATEGORIE_INFO.breathing, value: percentages?.breathing ?? 0 },
    { key: 'circulation', ...CATEGORIE_INFO.circulation, value: percentages?.circulation ?? 0 },
    { key: 'disability', ...CATEGORIE_INFO.disability, value: percentages?.disability ?? 0 },
    { key: 'exposure', ...CATEGORIE_INFO.exposure, value: percentages?.exposure ?? 0 },
    { key: 'urgentie', ...CATEGORIE_INFO.urgentie, value: percentages?.urgentie ?? 0 }
  ];

  const getColor = (pct) => {
    if (pct >= 80) return '#10b981'; // Voldoende (groen)
    if (pct >= 65) return '#f59e0b'; // Matig (oranje)
    return '#ef4444'; // Aandachtspunt (rood)
  };

  return (
    <div className="diagram-card">
      <div className="diagram-header">
        <div>
          <h3>Beoordelingsaccuratesse per Categorie (in %)</h3>
          <p className="diagram-subtitle">
            Zie direct in procenten waar je sterk in bent en welke vaardigheden onder de 80% norm vallen.
          </p>
        </div>
        <div className="diagram-legend">
          <span className="legend-item"><span className="legend-dot green"></span> &ge; 80% Voldoende</span>
          <span className="legend-item"><span className="legend-dot orange"></span> 65-79% Verbeterbaar</span>
          <span className="legend-item"><span className="legend-dot red"></span> &lt; 65% Prioriteit</span>
        </div>
      </div>

      <div className="benchmark-bar-container">
        <div className="benchmark-indicator" style={{ left: '80%' }}>
          <span className="benchmark-label">80% Streefnorm</span>
          <div className="benchmark-line"></div>
        </div>

        <div className="bars-list">
          {categories.map((cat) => {
            const IconComponent = ICON_MAP[cat.icon] || HelpCircle;
            const color = getColor(cat.value);
            const needsImprovement = cat.value < 80;

            return (
              <div
                key={cat.key}
                className="category-bar-row"
                onClick={() => onSelectCategory && onSelectCategory(cat.key)}
                title="Klik voor gerichte verbeter-uitleg"
              >
                <div className="category-meta">
                  <div className="category-icon-wrapper" style={{ color: color }}>
                    <IconComponent size={18} />
                  </div>
                  <div className="category-labels">
                    <span className="cat-title">{cat.label}</span>
                    <span className="cat-desc">{cat.beschrijving}</span>
                  </div>
                </div>

                <div className="bar-track-wrapper">
                  <div className="bar-track">
                    <div
                      className="bar-fill"
                      style={{
                        width: `${Math.max(4, cat.value)}%`,
                        backgroundColor: color
                      }}
                    />
                  </div>
                  <div className="bar-value-box" style={{ color: color }}>
                    <span className="bar-percent-text">{cat.value}%</span>
                    {needsImprovement && <span className="tag-warning">Verbeteren</span>}
                  </div>
                </div>
              </div>
            );
          })}
        </div>
      </div>

      <div className="diagram-footer-note">
        💡 <em>Tip: Klik op een categorie of ga naar het tabblad Foutanalyses om per patiënt te zien wat er misging.</em>
      </div>
    </div>
  );
}
