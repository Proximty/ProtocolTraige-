import React from 'react';
import { X, AlertTriangle, CheckCircle, Clock, ShieldAlert, Heart, Wind, Activity, Brain, Thermometer } from 'lucide-react';
import { NTS_URGENTIES } from '../constants/triage';

export default function CaseDetailModal({ patientCase, onClose }) {
  if (!patientCase) return null;

  const fields = [
    { key: 'Airway', label: 'Airway (A)', correct: patientCase.correcteAirway, input: patientCase.invoerAirway, icon: Wind },
    { key: 'Breathing', label: 'Breathing (B)', correct: patientCase.correcteBreathing, input: patientCase.invoerBreathing, icon: Activity },
    { key: 'Circulation', label: 'Circulation (C)', correct: patientCase.correcteCirculation, input: patientCase.invoerCirculation, icon: Heart },
    { key: 'Disability', label: 'Disability (D)', correct: patientCase.correcteDisability, input: patientCase.invoerDisability, icon: Brain },
    { key: 'Exposure', label: 'Exposure (E)', correct: patientCase.correcteExposure, input: patientCase.invoerExposure, icon: Thermometer }
  ];

  const correctUrg = NTS_URGENTIES[patientCase.correcteUrgentie] || { code: 'U?', naam: 'Onbekend' };
  const inputUrg = NTS_URGENTIES[patientCase.invoerUrgentie] || { code: 'U?', naam: 'Onbekend' };
  const urgCorrect = patientCase.correcteUrgentie === patientCase.invoerUrgentie;

  // Analyse van urgentiefout
  let urgentieFoutUitleg = null;
  if (!urgCorrect) {
    if (patientCase.invoerUrgentie > patientCase.correcteUrgentie) {
      urgentieFoutUitleg = {
        type: 'Ondertriëring (Gevaarlijk)',
        tekst: `Patiënt was zieker (${correctUrg.code} - ${correctUrg.naam}) dan ingeschat (${inputUrg.code} - ${inputUrg.naam}). Hierdoor wacht de patiënt mogelijk te lang op medische hulp!`,
        kleur: '#dc2626'
      };
    } else {
      urgentieFoutUitleg = {
        type: 'Overtriëring',
        tekst: `Patiënt kreeg een te hoge urgentie (${inputUrg.code}) toegewezen ten opzichte van de werkelijkheid (${correctUrg.code}). Dit leidt tot onnodige belasting van de acute behandelkamers.`,
        kleur: '#d97706'
      };
    }
  }

  return (
    <div className="modal-backdrop" onClick={onClose}>
      <div className="modal-card" onClick={(e) => e.stopPropagation()}>
        <div className="modal-header">
          <div>
            <div className="modal-title-row">
              <h3>Foutanalyse Patiënt #{patientCase.id}</h3>
              <span className={`case-status-badge ${patientCase.isCorrect ? 'badge-success' : 'badge-error'}`}>
                {patientCase.isCorrect ? 'Volledig Foutloos' : `${patientCase.aantalFouten} Fout(en) gemaakt`}
              </span>
            </div>
            <p className="modal-subtext">
              Level {patientCase.level} | Triagetijd: <strong>{patientCase.verstrekenTijdSeconden?.toFixed(1)}s</strong> | Tijdstip: {new Date(patientCase.timestamp).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })}
            </p>
          </div>
          <button className="btn-close" onClick={onClose}><X size={20} /></button>
        </div>

        <div className="modal-body">
          {/* Foutenfoyer banner */}
          {patientCase.isCorrect ? (
            <div className="banner-success">
              <CheckCircle size={22} />
              <div>
                <strong>Perfect beoordeeld!</strong>
                <p>Alle 5 ABCDE-componenten én de NTS-urgentieklasse waren 100% juist ingevoerd.</p>
              </div>
            </div>
          ) : (
            <div className="banner-error">
              <ShieldAlert size={22} />
              <div>
                <strong>Wat ging er precies mis?</strong>
                <p>Hieronder zie je de vergelijking tussen jouw ingevoerde waarden en de werkelijke patiëntconditie.</p>
              </div>
            </div>
          )}

          {/* Vergelijkingstabel ABCDE */}
          <div className="details-section">
            <h4>ABCDE-Metingen Vergelijking</h4>
            <div className="comparison-table">
              <div className="table-header">
                <span>Onderdeel</span>
                <span>Jouw Invoer</span>
                <span>Werkelijke Waarde</span>
                <span>Verschil & Toelichting</span>
              </div>
              {fields.map((f) => {
                const isGoed = f.input === f.correct;
                const diff = (f.input ?? 0) - (f.correct ?? 0);
                const Icon = f.icon;

                return (
                  <div key={f.key} className={`table-row ${isGoed ? 'row-correct' : 'row-incorrect'}`}>
                    <div className="field-name">
                      <Icon size={16} /> {f.label}
                    </div>
                    <div className="field-value">
                      <span className={`pill-val ${isGoed ? 'correct' : 'wrong'}`}>{f.input ?? '-'}</span>
                    </div>
                    <div className="field-value">
                      <span className="pill-val standard">{f.correct ?? '-'}</span>
                    </div>
                    <div className="field-explanation">
                      {isGoed ? (
                        <span className="txt-success"><CheckCircle size={14} /> Correct beoordeeld</span>
                      ) : (
                        <span className="txt-error">
                          <AlertTriangle size={14} /> Afwijking: {diff > 0 ? `+${diff}` : diff} pt ({diff > 0 ? 'Te hoog ingeschat' : 'Te laag ingeschat'})
                        </span>
                      )}
                    </div>
                  </div>
                );
              })}
            </div>
          </div>

          {/* Urgentie Analyse */}
          <div className="details-section">
            <h4>NTS Urgentie Beoordeling</h4>
            <div className={`urgency-comparison-box ${urgCorrect ? 'box-correct' : 'box-error'}`}>
              <div className="urgency-row">
                <div className="urgency-side">
                  <span className="urg-label">Jouw Invoer:</span>
                  <span className="urg-badge" style={{ backgroundColor: inputUrg.kleur || '#64748b' }}>
                    {inputUrg.code} - {inputUrg.naam}
                  </span>
                </div>
                <div className="urgency-divider">&rarr;</div>
                <div className="urgency-side">
                  <span className="urg-label">Juiste NTS Klasse:</span>
                  <span className="urg-badge" style={{ backgroundColor: correctUrg.kleur || '#64748b' }}>
                    {correctUrg.code} - {correctUrg.naam}
                  </span>
                </div>
              </div>

              {urgentieFoutUitleg && (
                <div className="urgentie-feedback" style={{ borderLeftColor: urgentieFoutUitleg.kleur }}>
                  <strong>⚠️ {urgentieFoutUitleg.type}:</strong> {urgentieFoutUitleg.tekst}
                </div>
              )}
            </div>
          </div>
        </div>

        <div className="modal-footer">
          <button className="btn-primary" onClick={onClose}>Sluiten</button>
        </div>
      </div>
    </div>
  );
}
