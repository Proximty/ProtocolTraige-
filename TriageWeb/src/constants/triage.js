// Definities van NTS Urgenties en ABCDE-categorien
export const NTS_URGENTIES = {
  0: { code: 'U0', naam: 'Reanimatie', beschrijving: 'Uitval vitale functies (onmiddellijk)', kleur: '#dc2626' },
  1: { code: 'U1', naam: 'Levensbedreigend', beschrijving: 'Instabiele ABCDE (< 15 min)', kleur: '#ea580c' },
  2: { code: 'U2', naam: 'Spoed', beschrijving: 'Bedreiging ABCDE (< 1 uur)', kleur: '#d97706' },
  3: { code: 'U3', naam: 'Dringend', beschrijving: 'Kans op schade (< 3 uur)', kleur: '#ca8a04' },
  4: { code: 'U4', naam: 'Niet Dringend', beschrijving: 'Verwaarloosbare kans op schade (< 24 uur)', kleur: '#16a34a' },
  5: { code: 'U5', naam: 'Advies', beschrijving: 'Geen kans op schade (zelfzorg)', kleur: '#0284c7' }
};

export const CATEGORIE_INFO = {
  airway: { label: 'Airway (Luchtweg)', icon: 'Wind', beschrijving: 'Inspectie van luchtwegobstructie, stridor of heesheid.' },
  breathing: { label: 'Breathing (Ademhaling)', icon: 'Activity', beschrijving: 'Ademhalingsfrequentie, saturatie en symmetrie.' },
  circulation: { label: 'Circulation (Circulatie)', icon: 'Heart', beschrijving: 'Polsfrequentie, bloeddruk, capillary refill en bleekheid.' },
  disability: { label: 'Disability (Bewustzijn)', icon: 'Brain', beschrijving: 'EMV score, pupilreflexen en alertheid.' },
  exposure: { label: 'Exposure (Omgeving)', icon: 'Thermometer', beschrijving: 'Lichaamstemperatuur, huiduitslag en trauma.' },
  urgentie: { label: 'NTS Urgentie (Totale triage)', icon: 'AlertTriangle', beschrijving: 'Juiste NTS-urgentieklasse bepalen volgens protocol.' }
};

// Verbeter-adviezen per categorie als de score < 80% is
export const VERBETER_TIPS = {
  airway: 'Let beter op vroege tekenen van luchtwegbelemmering en luister gericht met de stethoscoop.',
  breathing: 'Controleer altijd of de ademhalingsdiepte en -frequentie overeenkomen met de NTS-indicatoren.',
  circulation: 'Herken shock sneller: lage circulatiescores vereisen directe opwaardering naar U1 of U2.',
  disability: 'Stel altijd de gerichte vragen via de interactie-interface om verwardheid op te sporen.',
  exposure: 'Sla de lichamelijke inspectie en temperatuur niet over, dit kan sepsis of onderkoeling verraden.',
  urgentie: 'Bereken de somscore nauwkeuriger: bij twijfel altijd veiliger (hogere urgentie) triren!'
};
