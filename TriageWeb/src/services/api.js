// Centrale API Service voor Protocol Triage met Account Authenticatie

const API_BASE_URL = 'http://localhost:5000/api';

// Fallback testdata als de backend offline is
const MOCK_DATA = {
  stats: {
    heeftData: true,
    totaalSessies: 3,
    totaalCases: 12,
    totaalCorrect: 8,
    overallAccuratessePct: 66.7,
    gemiddeldeTijdSeconden: 34.8,
    hoogsteLevel: 3,
    categoriePercentages: {
      airway: 91.7,
      breathing: 83.3,
      circulation: 66.7,
      disability: 91.7,
      exposure: 58.3,
      urgentie: 66.7
    }
  },
  sessies: [
    {
      id: 3,
      startTijd: new Date(Date.now() - 3600000).toISOString(),
      eindTijd: new Date(Date.now() - 3600000 + 840000).toISOString(),
      eindLevel: 3,
      cases: [
        {
          id: 12, level: 3, timestamp: new Date().toISOString(),
          correcteAirway: 5, correcteBreathing: 5, correcteCirculation: 5, correcteDisability: 6, correcteExposure: 5, correcteUrgentie: 3,
          invoerAirway: 5, invoerBreathing: 5, invoerCirculation: 5, invoerDisability: 6, invoerExposure: 5, invoerUrgentie: 3,
          aantalFouten: 0, isCorrect: true, verstrekenTijdSeconden: 29.8
        },
        {
          id: 9, level: 3, timestamp: new Date().toISOString(),
          correcteAirway: 8, correcteBreathing: 8, correcteCirculation: 2, correcteDisability: 8, correcteExposure: 3, correcteUrgentie: 1,
          invoerAirway: 8, invoerBreathing: 8, invoerCirculation: 6, invoerDisability: 8, invoerExposure: 7, invoerUrgentie: 3,
          aantalFouten: 3, isCorrect: false, verstrekenTijdSeconden: 52.0
        }
      ]
    }
  ]
};

export const apiService = {
  // Inloggen met gebruikersnaam & wachtwoord
  async login(gebruikersnaam, wachtwoord) {
    try {
      const response = await fetch(`${API_BASE_URL}/auth/login`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ gebruikersnaam, wachtwoord })
      });
      if (!response.ok) {
        const err = await response.json();
        throw new Error(err.melding || 'Inloggen mislukt');
      }
      return await response.json();
    } catch (err) {
      console.warn('[API Auth] Backend offline of fout:', err.message);
      // Fallback voor developer: 'hanna' met wachtwoord 'admin' werkt altijd lokaal!
      if (gebruikersnaam.toLowerCase() === 'hanna' && wachtwoord === 'admin') {
        return {
          succes: true,
          gebruiker: {
            id: 1,
            gebruikersnaam: 'hanna',
            volledigeNaam: 'Hanna Versluizen',
            rol: 'Trainee SEH'
          }
        };
      }
      throw new Error('Onjuiste gebruikersnaam of wachtwoord. (Tip: hanna / admin)');
    }
  },

  // Haalt statistieken op specifiek voor deze gebruiker
  async getStats(gebruikerId) {
    try {
      const url = gebruikerId ? `${API_BASE_URL}/stats?gebruikerId=${gebruikerId}` : `${API_BASE_URL}/stats`;
      const response = await fetch(url, { headers: { 'Accept': 'application/json' } });
      if (!response.ok) throw new Error(`HTTP fout: ${response.status}`);
      const data = await response.json();
      
      if (!data.heeftData || data.totaalCases === 0) {
        return { ...MOCK_DATA.stats, isLiveApi: false };
      }
      return { ...data, isLiveApi: true };
    } catch (err) {
      return { ...MOCK_DATA.stats, isLiveApi: false };
    }
  },

  // Haalt sessies op specifiek voor deze gebruiker
  async getSessies(gebruikerId) {
    try {
      const url = gebruikerId ? `${API_BASE_URL}/sessies?gebruikerId=${gebruikerId}` : `${API_BASE_URL}/sessies`;
      const response = await fetch(url, { headers: { 'Accept': 'application/json' } });
      if (!response.ok) throw new Error(`HTTP fout: ${response.status}`);
      const data = await response.json();
      if (!data || data.length === 0) return MOCK_DATA.sessies;
      return data;
    } catch (err) {
      return MOCK_DATA.sessies;
    }
  }
};
