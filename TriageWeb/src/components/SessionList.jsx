import React, { useState } from 'react';
import { Calendar, ChevronDown, ChevronUp, Eye, CheckCircle, AlertCircle, Clock } from 'lucide-react';
import { NTS_URGENTIES } from '../constants/triage';

export default function SessionList({ sessies, onSelectCase, initialFilter = null }) {
  const [openSessionId, setOpenSessionId] = useState(sessies?.[0]?.id || null);
  const [filterOnlyErrors, setFilterOnlyErrors] = useState(false);

  const toggleSession = (id) => {
    setOpenSessionId(openSessionId === id ? null : id);
  };

  return (
    <div className="session-history-card">
      <div className="session-history-header">
        <div>
          <h3>Sessie- en Fouthistorie</h3>
          <p className="subtitle">
            Klap een sessie open om alle behandelde patiënten te bekijken. Klik op een patiënt voor de volledige foutanalyse.
          </p>
        </div>
        <div className="filter-controls">
          <label className="checkbox-label">
            <input
              type="checkbox"
              checked={filterOnlyErrors}
              onChange={(e) => setFilterOnlyErrors(e.target.checked)}
            />
            Toon alleen cases met fouten
          </label>
        </div>
      </div>

      <div className="sessions-accordion">
        {sessies && sessies.length > 0 ? (
          sessies.map((sessie) => {
            const cases = sessie.cases || [];
            const totaalCases = cases.length;
            const correctCases = cases.filter((c) => c.isCorrect).length;
            const slagingsPct = totaalCases > 0 ? Math.round((correctCases / totaalCases) * 100) : 0;
            const isOpen = openSessionId === sessie.id;

            const filteredCases = filterOnlyErrors ? cases.filter((c) => !c.isCorrect) : cases;

            return (
              <div key={sessie.id} className={`session-accordion-item ${isOpen ? 'open' : ''}`}>
                <div className="session-summary-bar" onClick={() => toggleSession(sessie.id)}>
                  <div className="session-meta-col">
                    <span className="session-id">Sessie #{sessie.id}</span>
                    <span className="session-date">
                      <Calendar size={14} /> {new Date(sessie.startTijd).toLocaleDateString()} - {new Date(sessie.startTijd).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })}
                    </span>
                  </div>

                  <div className="session-metrics-col">
                    <div className="session-stat">
                      <span className="stat-label">Behaald Level</span>
                      <span className="stat-value">Level {sessie.eindLevel}</span>
                    </div>

                    <div className="session-stat">
                      <span className="stat-label">Slagingspercentage</span>
                      <span className={`stat-value-badge ${slagingsPct >= 80 ? 'green' : slagingsPct >= 65 ? 'orange' : 'red'}`}>
                        {slagingsPct}% ({correctCases}/{totaalCases})
                      </span>
                    </div>

                    <div className="accordion-arrow">
                      {isOpen ? <ChevronUp size={20} /> : <ChevronDown size={20} />}
                    </div>
                  </div>
                </div>

                {isOpen && (
                  <div className="session-cases-container">
                    {filteredCases.length === 0 ? (
                      <div className="no-cases-msg">
                        {filterOnlyErrors
                          ? '🎉 Geen fouten gemaakt in deze sessie! Alle cases waren 100% correct.'
                          : 'Nog geen patiënten geregistreerd in deze sessie.'}
                      </div>
                    ) : (
                      <div className="cases-list-grid">
                        {filteredCases.map((c, index) => {
                          const urg = NTS_URGENTIES[c.correcteUrgentie] || { code: 'U?' };

                          return (
                            <div
                              key={c.id || index}
                              className={`case-mini-card ${c.isCorrect ? 'card-pass' : 'card-fail'}`}
                            >
                              <div className="case-card-header">
                                <span className="case-tag">Patiënt #{c.id} (Lvl {c.level})</span>
                                <span className="case-time"><Clock size={12} /> {c.verstrekenTijdSeconden?.toFixed(0)}s</span>
                              </div>

                              <div className="case-card-status">
                                {c.isCorrect ? (
                                  <span className="status-label success">
                                    <CheckCircle size={15} /> 100% Goed beoordeeld
                                  </span>
                                ) : (
                                  <span className="status-label error">
                                    <AlertCircle size={15} /> {c.aantalFouten} fout(en) gemaakt
                                  </span>
                                )}
                              </div>

                              <div className="case-card-triage-preview">
                                <span>Juiste Urgentie:</span>
                                <span className="preview-urg-badge" style={{ backgroundColor: urg.kleur || '#64748b' }}>
                                  {urg.code}
                                </span>
                              </div>

                              <button
                                className="btn-view-details"
                                onClick={() => onSelectCase(c)}
                              >
                                <Eye size={14} /> Toon details van fouten
                              </button>
                            </div>
                          );
                        })}
                      </div>
                    )}
                  </div>
                )}
              </div>
            );
          })
        ) : (
          <div className="empty-sessions-notice">
            Nog geen sessies gevonden. Start een speelsessie in Unity om resultaten te zien!
          </div>
        )}
      </div>
    </div>
  );
}
