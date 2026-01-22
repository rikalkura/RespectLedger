// Changelog data - track all versions and changes
const changelog = [
  {
    version: '1.1.0',
    date: '2026-01-22',
    changes: [
      'Added changelog functionality',
      'Added conditional seeding - only seeds if database is empty',
      'Improved mobile responsiveness',
      'Updated flash message icons'
    ]
  },
  {
    version: '1.0.1',
    date: '2026-01-21',
    changes: [
      'Fixed database seeding issue - transactions now persist across restarts',
    ]
  },
  {
    version: '1.0.0',
    date: '2026-01-21',
    changes: [
      'Initial release',
    ]
  }
];

// Get all changelog entries
function getChangelog() {
  return changelog;
}

// Get latest version
function getLatestVersion() {
  return changelog.length > 0 ? changelog[0] : null;
}

// Add a new changelog entry (for future use)
function addChangelogEntry(version, date, changes) {
  changelog.unshift({
    version,
    date,
    changes: Array.isArray(changes) ? changes : [changes]
  });
}

module.exports = {
  getChangelog,
  getLatestVersion,
  addChangelogEntry
};
