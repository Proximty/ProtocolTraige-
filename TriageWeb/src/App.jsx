import React, { useState, useEffect } from 'react';
import Header from './components/Header';
import LoginPage from './components/LoginPage';
import StatCards from './components/StatCards';
import PercentageDiagram from './components/PercentageDiagram';
import ImprovementSection from './components/ImprovementSection';
import SessionList from './components/SessionList';
import CaseDetailModal from './components/CaseDetailModal';
import AccountTab from './components/AccountTab';
import { apiService } from './services/api';
import { RefreshCw } from 'lucide-react';

export default function App() {
  const [currentUser, setCurrentUser] = useState(() => {
    const saved = localStorage.getItem('triage_user');
    return saved ? JSON.parse(saved) : null;
  });

  const [activeTab, setActiveTab] = useState('dashboard'); // 'dashboard' | 'details' | 'account'
  const [stats, setStats] = useState(null);
  const [sessies, setSessies] = useState([]);
  const [loading, setLoading] = useState(false);
  const [selectedCase, setSelectedCase] = useState(null);
  const [selectedCategoryFilter, setSelectedCategoryFilter] = useState(null);

  const loadUserData = async (userId) => {
    setLoading(true);
    try {
      const [statsData, sessiesData] = await Promise.all([
        apiService.getStats(userId),
        apiService.getSessies(userId)
      ]);
      setStats(statsData);
      setSessies(sessiesData);
    } catch (err) {
      console.error('Fout bij laden triage data voor gebruiker:', err);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    if (currentUser?.id) {
      loadUserData(currentUser.id);
    }
  }, [currentUser]);

  const handleLoginSuccess = (user) => {
    localStorage.setItem('triage_user', JSON.stringify(user));
    setCurrentUser(user);
  };

  const handleLogout = () => {
    localStorage.removeItem('triage_user');
    setCurrentUser(null);
    setStats(null);
    setSessies([]);
  };

  const handleSelectCategory = (categoryKey) => {
    setSelectedCategoryFilter(categoryKey);
    setActiveTab('details');
  };

  // Als er nog niet is ingelogd, toon de inlogpagina!
  if (!currentUser) {
    return <LoginPage onLoginSuccess={handleLoginSuccess} />;
  }

  return (
    <div className="triage-app-wrapper">
      <Header
        activeTab={activeTab}
        setActiveTab={setActiveTab}
        stats={stats}
        currentUser={currentUser}
        onLogout={handleLogout}
      />

      <main className="app-main-content">
        {loading ? (
          <div className="loading-state">
            <RefreshCw className="spin-icon" size={36} />
            <p>Gegevens ophalen uit de database voor {currentUser.volledigeNaam}...</p>
          </div>
        ) : (
          <>
            {activeTab === 'dashboard' && (
              <div className="tab-dashboard-view">
                {/* 1. Stat cards met percentages */}
                <StatCards stats={stats} />

                {/* 2. Waar sta je? Visual percentages diagram */}
                <PercentageDiagram
                  percentages={stats?.categoriePercentages}
                  onSelectCategory={handleSelectCategory}
                />

                {/* 3. Waar moet je verbeteren? Actionable insights */}
                <ImprovementSection
                  percentages={stats?.categoriePercentages}
                  onViewDetails={handleSelectCategory}
                />
              </div>
            )}

            {activeTab === 'details' && (
              <div className="tab-details-view">
                <SessionList
                  sessies={sessies}
                  onSelectCase={(c) => setSelectedCase(c)}
                  initialFilter={selectedCategoryFilter}
                />
              </div>
            )}

            {activeTab === 'account' && (
              <div className="tab-account-view">
                <AccountTab
                  stats={stats}
                  currentUser={currentUser}
                  onRefresh={() => loadUserData(currentUser.id)}
                  isLive={stats?.isLiveApi}
                />
              </div>
            )}
          </>
        )}
      </main>

      {/* Detail modal die exact uitlegt wat er fout ging bij een case */}
      {selectedCase && (
        <CaseDetailModal
          patientCase={selectedCase}
          onClose={() => setSelectedCase(null)}
        />
      )}
    </div>
  );
}
