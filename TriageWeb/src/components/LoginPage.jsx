import React, { useState } from 'react';
import { apiService } from '../services/api';
import { Shield, Lock, User, AlertCircle, ArrowRight, Activity, CheckCircle2 } from 'lucide-react';

export default function LoginPage({ onLoginSuccess }) {
  const [isRegister, setIsRegister] = useState(false);
  const [gebruikersnaam, setGebruikersnaam] = useState('');
  const [wachtwoord, setWachtwoord] = useState('');
  const [volledigeNaam, setVolledigeNaam] = useState('');
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);
  const [regSuccess, setRegSuccess] = useState(false);

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError(null);
    setLoading(true);

    try {
      if (isRegister) {
        if (!gebruikersnaam || !wachtwoord || !volledigeNaam) {
          throw new Error('Vul alle velden in om een account aan te maken.');
        }
        // Registratie via backend API
        const regRes = await fetch('http://localhost:5000/api/auth/register', {
          method: 'POST',
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify({ gebruikersnaam, wachtwoord, volledigeNaam })
        });
        if (!regRes.ok) {
          const errData = await regRes.json();
          throw new Error(errData.melding || 'Registratie mislukt');
        }
        setRegSuccess(true);
        setIsRegister(false);
        setLoading(false);
        return;
      }

      // Inloggen via database API
      const result = await apiService.login(gebruikersnaam, wachtwoord);
      if (result.succes && result.gebruiker) {
        onLoginSuccess(result.gebruiker);
      }
    } catch (err) {
      setError(err.message || 'Er is een fout opgetreden bij het inloggen.');
    } finally {
      setLoading(false);
    }
  };

  const handleQuickDemo = () => {
    setGebruikersnaam('hanna');
    setWachtwoord('admin');
  };

  return (
    <div className="login-page-container">
      <div className="login-card">
        <div className="login-brand">
          <div className="login-logo-circle">
            <Activity size={32} />
          </div>
          <h1>Protocol Triage VR</h1>
          <p className="login-subtitle">
            Medisch Opleidingsportaal & Vaardighedenoverzicht
          </p>
        </div>

        {regSuccess && (
          <div className="alert-box success">
            <CheckCircle2 size={18} />
            <span>Account succesvol aangemaakt in de database! Je kunt nu direct inloggen.</span>
          </div>
        )}

        {error && (
          <div className="alert-box error">
            <AlertCircle size={18} />
            <span>{error}</span>
          </div>
        )}

        <form onSubmit={handleSubmit} className="login-form">
          {isRegister && (
            <div className="form-group">
              <label>Volledige Naam</label>
              <div className="input-with-icon">
                <User size={18} className="field-icon" />
                <input
                  type="text"
                  placeholder="bijv. Hanna Versluizen"
                  value={volledigeNaam}
                  onChange={(e) => setVolledigeNaam(e.target.value)}
                  className="login-input"
                  required
                />
              </div>
            </div>
          )}

          <div className="form-group">
            <label>Gebruikersnaam</label>
            <div className="input-with-icon">
              <User size={18} className="field-icon" />
              <input
                type="text"
                placeholder="bijv. hanna"
                value={gebruikersnaam}
                onChange={(e) => setGebruikersnaam(e.target.value)}
                className="login-input"
                autoFocus
                required
              />
            </div>
          </div>

          <div className="form-group">
            <label>Wachtwoord</label>
            <div className="input-with-icon">
              <Lock size={18} className="field-icon" />
              <input
                type="password"
                placeholder="••••••••"
                value={wachtwoord}
                onChange={(e) => setWachtwoord(e.target.value)}
                className="login-input"
                required
              />
            </div>
          </div>

          <button type="submit" className="btn-login-submit" disabled={loading}>
            {loading ? (
              'Bezig met verifiëren...'
            ) : isRegister ? (
              <>Account Aanmaken in Database <ArrowRight size={16} /></>
            ) : (
              <>Inloggen & Resultaten Laden <ArrowRight size={16} /></>
            )}
          </button>
        </form>

        <div className="login-footer-actions">
          <button
            type="button"
            className="btn-toggle-register"
            onClick={() => {
              setIsRegister(!isRegister);
              setError(null);
            }}
          >
            {isRegister
              ? 'Heb je al een account? Log hier in'
              : 'Nieuw account aanmaken in de database'}
          </button>

          {!isRegister && (
            <div className="demo-credentials-pill" onClick={handleQuickDemo} title="Klik om in te vullen">
              <span>💡 Snel invullen: <strong>hanna</strong> / <strong>admin</strong></span>
            </div>
          )}
        </div>
      </div>
    </div>
  );
}
