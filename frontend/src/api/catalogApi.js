import apiClient from './client'

const SUBJECT_ALIASES = {
  biology: 'Biologia',
  biologia: 'Biologia',
  chemistry: 'Chemia',
  chemia: 'Chemia',
  'computer science': 'Informatyka',
  computerscience: 'Informatyka',
  informatics: 'Informatyka',
  informatyka: 'Informatyka',
  database: 'Bazy Danych',
  databases: 'Bazy Danych',
  'bazy danych': 'Bazy Danych',
  'differential calculus': 'Rachunek rozniczkowy',
  'rachunek rozniczkowy': 'Rachunek rozniczkowy',
  geometry: 'Geometria',
  geometria: 'Geometria',
  nature: 'Przyroda',
  przyroda: 'Przyroda',
  english: 'Jezyk angielski',
  'english language': 'Jezyk angielski',
  'jezyk angielski': 'Jezyk angielski',
  'j. angielski': 'Jezyk angielski',
  polish: 'Jezyk polski',
  'polish language': 'Jezyk polski',
  'jezyk polski': 'Jezyk polski',
  'j. polski': 'Jezyk polski',
  german: 'Jezyk niemiecki',
  'german language': 'Jezyk niemiecki',
  'jezyk niemiecki': 'Jezyk niemiecki',
  'j. niemiecki': 'Jezyk niemiecki',
  spanish: 'Jezyk hiszpanski',
  'spanish language': 'Jezyk hiszpanski',
  'jezyk hiszpanski': 'Jezyk hiszpanski',
  'j. hiszpanski': 'Jezyk hiszpanski',
  japanese: 'Język japoński',
  'japanese language': 'Język japoński',
  'jezyk japonski': 'Język japoński',
  'j. japonski': 'Język japoński',
  italian: 'Język włoski',
  'italian language': 'Język włoski',
  'jezyk wloski': 'Język włoski',
  'j. wloski': 'Język włoski',
  physics: 'Fizyka',
  fizyka: 'Fizyka',
  geography: 'Geografia',
  geografia: 'Geografia',
  history: 'Historia',
  historia: 'Historia',
  math: 'Matematyka',
  mathematics: 'Matematyka',
  matematyka: 'Matematyka',
  civics: 'WOS',
  'social studies': 'WOS',
  wiedzaospoleczenstwie: 'WOS',
  'wiedza o spoleczenstwie': 'WOS',
  wos: 'WOS',
}

export const canonicalSubjectName = (name) => {
  const key = String(name ?? '')
    .trim()
    .toLowerCase()
    .normalize('NFD')
    .replace(/\p{Diacritic}/gu, '')
    .replace(/\s+/g, ' ')

  return SUBJECT_ALIASES[key] ?? name
}

const normalizeSubjects = (items) => {
  const byName = new Map()
  ;(Array.isArray(items) ? items : []).forEach((item) => {
    const canonicalName = canonicalSubjectName(item.name)
    const key = String(canonicalName).toLowerCase()
    if (!byName.has(key)) {
      byName.set(key, { ...item, name: canonicalName })
    }
  })
  return [...byName.values()]
}

export const fetchSubjects = async () => {
  const { data } = await apiClient.get('/api/catalog/subjects')
  return normalizeSubjects(data)
}

export const fetchTeachingLevels = async () => {
  const { data } = await apiClient.get('/api/catalog/teaching-levels')
  return data
}
