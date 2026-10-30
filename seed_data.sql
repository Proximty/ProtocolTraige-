-- ============================================================================
-- PROTOCOL TRIAGE - SEED DATA VOOR MYSQL
-- Plak dit in je MySQL-scherm om direct realistische data in te laden.
-- ============================================================================

CREATE DATABASE IF NOT EXISTS protocol_triage;
USE protocol_triage;

-- 1. Tabellen controleren/aanmaken
CREATE TABLE IF NOT EXISTS Gebruiker (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Gebruikersnaam VARCHAR(100) NOT NULL UNIQUE,
    Wachtwoord VARCHAR(255) NOT NULL,
    VolledigeNaam VARCHAR(255) NOT NULL,
    Rol VARCHAR(50) DEFAULT 'Trainee',
    AangemaaktOp DATETIME DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS Sessie (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    GebruikerId INT DEFAULT 1,
    StartTijd DATETIME NOT NULL,
    EindTijd DATETIME,
    EindLevel INT DEFAULT 1,
    FOREIGN KEY (GebruikerId) REFERENCES Gebruiker(Id) ON DELETE SET NULL
);

CREATE TABLE IF NOT EXISTS PatientCase (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    SessieId INT NOT NULL,
    Timestamp DATETIME NOT NULL,
    Level INT NOT NULL,
    CorrecteAirway INT, 
    CorrecteBreathing INT, 
    CorrecteCirculation INT, 
    CorrecteDisability INT, 
    CorrecteExposure INT, 
    CorrecteUrgentie INT,
    InvoerAirway INT, 
    InvoerBreathing INT, 
    InvoerCirculation INT, 
    InvoerDisability INT, 
    InvoerExposure INT, 
    InvoerUrgentie INT,
    AantalFouten INT DEFAULT 0,
    IsCorrect BOOLEAN DEFAULT FALSE,
    VerstrekenTijdSeconden FLOAT,
    FOREIGN KEY (SessieId) REFERENCES Sessie(Id) ON DELETE CASCADE
);

CREATE TABLE IF NOT EXISTS Scenario (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Level INT NOT NULL,
    Titel VARCHAR(100) NOT NULL,
    Klacht TEXT,
    Airway INT NOT NULL,
    Breathing INT NOT NULL,
    Circulation INT NOT NULL,
    Disability INT NOT NULL,
    Exposure INT NOT NULL,
    Pijn INT DEFAULT 0,
    Urgentie INT NOT NULL,
    DisabilityReactie VARCHAR(255),
    ExposureReactie VARCHAR(255)
);

-- Standaard gebruikers toevoegen
INSERT IGNORE INTO Gebruiker (Id, Gebruikersnaam, Wachtwoord, VolledigeNaam, Rol, AangemaaktOp) VALUES
(1, 'hanna', 'admin', 'Hanna Versluizen', 'Trainee SEH', NOW()),
(2, 'admin', '1234', 'Systeem Beheerder', 'Docent / Beheerder', NOW());

-- Medische scenario's toevoegen
INSERT IGNORE INTO Scenario (Id, Level, Titel, Klacht, Airway, Breathing, Circulation, Disability, Exposure, Pijn, Urgentie, DisabilityReactie, ExposureReactie) VALUES
(1, 1, 'Lichte enkelverstuiking', 'Patient heeft zich verstapt tijdens het hardlopen.', 3, 2, 3, 3, 2, 4, 4, '\"Ik voel me verder prima, alleen mijn enkel doet pijn.\"', '\"Er is een lichte zwelling aan mijn rechterenkel zichtbaar.\"'),
(2, 1, 'Oppervlakkige snijwond hand', 'Gesneden bij het koken, bloeden is gestopt.', 5, 5, 4, 6, 5, 3, 3, '\"Ik ben niet duizelig, het schrikken was het ergste.\"', '\"Geen actieve bloeding meer, wond is 3 centimeter lang.\"'),
(3, 2, 'Acute Benauwdheid (Astma)', 'Ernstige piepende ademhaling en tachypneu.', 6, 7, 6, 5, 5, 2, 2, '\"Ik ben licht in mijn hoofd door het snelle ademen...\"', '\"Huid voelt klam en bleek aan.\"'),
(4, 2, 'Thoracale Pijn (Verdenking ACS)', 'Drukkende pijn op de borst met uitstraling naar de linkerarm.', 8, 8, 7, 6, 7, 7, 1, '\"Ik voel me erg zwak en misselijk...\"', '\"Grauwe gelaatskleur en klam zweet op het voorhoofd.\"'),
(5, 3, 'Anafylactische Reactie', 'Stridor na wespensteek, plotselinge zwelling gelaat en keel.', 8, 8, 2, 8, 3, 8, 1, '\"Praten lukt bijna niet meer... help me...\"', '\"Rode galbulten over het hele lichaam en gezwollen lippen.\"'),
(6, 3, 'Hartstilstand (Reanimatie Setting)', 'Patient reageert niet, afwezige ademhaling en pols.', 9, 9, 9, 9, 8, 0, 0, '\"(Geen reactie, patient is bewusteloos)\"', '\"Lijkbleek, cyanotische lippen en geen perifere circulatie.\"');

-- Maak eventuele oude testdata leeg
DELETE FROM PatientCase;
DELETE FROM Sessie;

-- 2. Drie speelsessies invoegen
INSERT INTO Sessie (Id, GebruikerId, StartTijd, EindTijd, EindLevel) VALUES
(1, 1, DATE_SUB(NOW(), INTERVAL 2 DAY), DATE_ADD(DATE_SUB(NOW(), INTERVAL 2 DAY), INTERVAL 6 MINUTE), 1),
(2, 1, DATE_SUB(NOW(), INTERVAL 1 DAY), DATE_ADD(DATE_SUB(NOW(), INTERVAL 1 DAY), INTERVAL 10 MINUTE), 2),
(3, 1, NOW(), DATE_ADD(NOW(), INTERVAL 14 MINUTE), 3);
(1, DATE_SUB(NOW(), INTERVAL 2 DAY), DATE_ADD(DATE_SUB(NOW(), INTERVAL 2 DAY), INTERVAL 6 MINUTE), 1),
(2, DATE_SUB(NOW(), INTERVAL 1 DAY), DATE_ADD(DATE_SUB(NOW(), INTERVAL 1 DAY), INTERVAL 10 MINUTE), 2),
(3, NOW(), DATE_ADD(NOW(), INTERVAL 14 MINUTE), 3);

-- 3. Patiënt-cases toevoegen (Met realistische scores, geslaagde cases en gerichte fouten)

-- SESSIE 1 (Level 1 training - 3 patiënten)
INSERT INTO PatientCase (
    SessieId, Timestamp, Level,
    CorrecteAirway, CorrecteBreathing, CorrecteCirculation, CorrecteDisability, CorrecteExposure, CorrecteUrgentie,
    InvoerAirway, InvoerBreathing, InvoerCirculation, InvoerDisability, InvoerExposure, InvoerUrgentie,
    AantalFouten, IsCorrect, VerstrekenTijdSeconden
) VALUES
-- Case 1: 100% Goed (U3 Dringend)
(1, DATE_ADD(DATE_SUB(NOW(), INTERVAL 2 DAY), INTERVAL 1 MINUTE), 1,
 5, 5, 4, 6, 5, 3,
 5, 5, 4, 6, 5, 3,
 0, 1, 32.5),

-- Case 2: 100% Goed (U4 Niet-dringend)
(1, DATE_ADD(DATE_SUB(NOW(), INTERVAL 2 DAY), INTERVAL 3 MINUTE), 1,
 3, 2, 3, 3, 2, 4,
 3, 2, 3, 3, 2, 4,
 0, 1, 28.0),

-- Case 3: Fout in Exposure en daardoor verkeerde Urgentie (U2 i.p.v. U3)
(1, DATE_ADD(DATE_SUB(NOW(), INTERVAL 2 DAY), INTERVAL 5 MINUTE), 1,
 6, 6, 6, 5, 4, 2,
 6, 6, 6, 5, 8, 3,
 2, 0, 48.2);


-- SESSIE 2 (Level 2 training - 4 patiënten)
INSERT INTO PatientCase (
    SessieId, Timestamp, Level,
    CorrecteAirway, CorrecteBreathing, CorrecteCirculation, CorrecteDisability, CorrecteExposure, CorrecteUrgentie,
    InvoerAirway, InvoerBreathing, InvoerCirculation, InvoerDisability, InvoerExposure, InvoerUrgentie,
    AantalFouten, IsCorrect, VerstrekenTijdSeconden
) VALUES
-- Case 4: 100% Goed (U1 Levensbedreigend)
(2, DATE_ADD(DATE_SUB(NOW(), INTERVAL 1 DAY), INTERVAL 2 MINUTE), 2,
 8, 8, 7, 6, 7, 1,
 8, 8, 7, 6, 7, 1,
 0, 1, 26.4),

-- Case 5: Fout in Circulation (te laag ingeschat)
(2, DATE_ADD(DATE_SUB(NOW(), INTERVAL 1 DAY), INTERVAL 4 MINUTE), 2,
 7, 7, 3, 6, 6, 2,
 7, 7, 7, 6, 6, 2,
 1, 0, 41.0),

-- Case 6: 100% Goed (U2 Spoed)
(2, DATE_ADD(DATE_SUB(NOW(), INTERVAL 1 DAY), INTERVAL 7 MINUTE), 2,
 6, 6, 6, 5, 6, 2,
 6, 6, 6, 5, 6, 2,
 0, 1, 35.8),

-- Case 7: 100% Goed (U0 Reanimatie)
(2, DATE_ADD(DATE_SUB(NOW(), INTERVAL 1 DAY), INTERVAL 9 MINUTE), 2,
 9, 9, 9, 9, 8, 0,
 9, 9, 9, 9, 8, 0,
 0, 1, 24.1);


-- SESSIE 3 (Level 3 training - 5 patiënten)
INSERT INTO PatientCase (
    SessieId, Timestamp, Level,
    CorrecteAirway, CorrecteBreathing, CorrecteCirculation, CorrecteDisability, CorrecteExposure, CorrecteUrgentie,
    InvoerAirway, InvoerBreathing, InvoerCirculation, InvoerDisability, InvoerExposure, InvoerUrgentie,
    AantalFouten, IsCorrect, VerstrekenTijdSeconden
) VALUES
-- Case 8: 100% Goed (U2 Spoed)
(3, DATE_ADD(NOW(), INTERVAL 2 MINUTE), 3,
 6, 7, 6, 5, 5, 2,
 6, 7, 6, 5, 5, 2,
 0, 1, 30.5),

-- Case 9: Fout in Circulation en Exposure (Ondertriëring: U3 gekozen i.p.v. U1)
(3, DATE_ADD(NOW(), INTERVAL 5 MINUTE), 3,
 8, 8, 2, 8, 3, 1,
 8, 8, 6, 8, 7, 3,
 3, 0, 52.0),

-- Case 10: 100% Goed
(3, DATE_ADD(NOW(), INTERVAL 8 MINUTE), 3,
 4, 4, 3, 5, 4, 3,
 4, 4, 3, 5, 4, 3,
 0, 1, 33.1),

-- Case 11: Fout in Breathing en Urgentie
(3, DATE_ADD(NOW(), INTERVAL 11 MINUTE), 3,
 7, 9, 6, 7, 6, 1,
 7, 5, 6, 7, 6, 2,
 2, 0, 44.7),

-- Case 12: 100% Goed
(3, DATE_ADD(NOW(), INTERVAL 13 MINUTE), 3,
 5, 5, 5, 6, 5, 3,
 5, 5, 5, 6, 5, 3,
 0, 1, 29.8);
