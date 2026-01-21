const initSqlJs = require('sql.js');
const path = require('path');
const fs = require('fs');

const DB_PATH = path.join(__dirname, '..', 'respect_ledger.db');
let db = null;

// Database helper functions
function saveDatabase() {
  const data = db.export();
  const buffer = Buffer.from(data);
  fs.writeFileSync(DB_PATH, buffer);
}

function dbRun(sql, params = []) {
  db.run(sql, params);
  saveDatabase();
}

function dbGet(sql, params = []) {
  const stmt = db.prepare(sql);
  stmt.bind(params);
  if (stmt.step()) {
    const row = stmt.getAsObject();
    stmt.free();
    return row;
  }
  stmt.free();
  return null;
}

function dbAll(sql, params = []) {
  const stmt = db.prepare(sql);
  stmt.bind(params);
  const results = [];
  while (stmt.step()) {
    results.push(stmt.getAsObject());
  }
  stmt.free();
  return results;
}

// Database initialization
async function initDatabase() {
  const SQL = await initSqlJs();
  
  // Always start fresh - delete existing database file
  if (fs.existsSync(DB_PATH)) {
    fs.unlinkSync(DB_PATH);
    console.log('✅ Deleted existing database file');
  }
  
  // Create new database
  db = new SQL.Database();
  console.log('✅ Created new database');
  
  // Enable foreign keys
  db.run('PRAGMA foreign_keys = ON');
  
  // Create tables
  db.run(`
    CREATE TABLE IF NOT EXISTS Users (
      id INTEGER PRIMARY KEY AUTOINCREMENT,
      name TEXT NOT NULL UNIQUE,
      pin_code TEXT NOT NULL,
      avatar_emoji TEXT DEFAULT '😀',
      is_admin INTEGER DEFAULT 0,
      balance INTEGER DEFAULT 0
    )
  `);
  
  db.run(`
    CREATE TABLE IF NOT EXISTS Transactions (
      id INTEGER PRIMARY KEY AUTOINCREMENT,
      from_user_id INTEGER,
      to_user_id INTEGER,
      type TEXT NOT NULL CHECK(type IN ('RESPECT', 'DISRESPECT', 'SHOP_PURCHASE', 'QUEST_REWARD')),
      amount INTEGER NOT NULL,
      description TEXT,
      timestamp DATETIME DEFAULT CURRENT_TIMESTAMP,
      FOREIGN KEY (from_user_id) REFERENCES Users(id),
      FOREIGN KEY (to_user_id) REFERENCES Users(id)
    )
  `);
  
  db.run(`
    CREATE TABLE IF NOT EXISTS ShopItems (
      id INTEGER PRIMARY KEY AUTOINCREMENT,
      name TEXT NOT NULL,
      price INTEGER NOT NULL,
      image_url TEXT,
      cloudinary_public_id TEXT
    )
  `);
  
  db.run(`
    CREATE TABLE IF NOT EXISTS ShopPurchases (
      id INTEGER PRIMARY KEY AUTOINCREMENT,
      user_id INTEGER NOT NULL,
      item_id INTEGER NOT NULL,
      status TEXT NOT NULL DEFAULT 'PENDING' CHECK(status IN ('PENDING', 'BOUGHT', 'REJECTED')),
      purchased_at DATETIME DEFAULT CURRENT_TIMESTAMP,
      reviewed_at DATETIME,
      reviewed_by INTEGER,
      FOREIGN KEY (user_id) REFERENCES Users(id),
      FOREIGN KEY (item_id) REFERENCES ShopItems(id),
      FOREIGN KEY (reviewed_by) REFERENCES Users(id)
    )
  `);
  
  db.run(`
    CREATE TABLE IF NOT EXISTS Quests (
      id INTEGER PRIMARY KEY AUTOINCREMENT,
      title TEXT NOT NULL,
      reward INTEGER NOT NULL,
      is_active INTEGER DEFAULT 1
    )
  `);
  
  db.run(`
    CREATE TABLE IF NOT EXISTS QuestCompletions (
      id INTEGER PRIMARY KEY AUTOINCREMENT,
      quest_id INTEGER NOT NULL,
      user_id INTEGER NOT NULL,
      status TEXT NOT NULL DEFAULT 'PENDING' CHECK(status IN ('PENDING', 'APPROVED', 'REJECTED')),
      submitted_at DATETIME DEFAULT CURRENT_TIMESTAMP,
      reviewed_at DATETIME,
      reviewed_by INTEGER,
      FOREIGN KEY (quest_id) REFERENCES Quests(id),
      FOREIGN KEY (user_id) REFERENCES Users(id),
      FOREIGN KEY (reviewed_by) REFERENCES Users(id)
    )
  `);
  
  db.run(`
    CREATE TABLE IF NOT EXISTS Notifications (
      id INTEGER PRIMARY KEY AUTOINCREMENT,
      type TEXT NOT NULL CHECK(type IN ('SHOP_PURCHASE', 'QUEST_APPROVAL')),
      user_id INTEGER,
      item_id INTEGER,
      message TEXT NOT NULL,
      is_read INTEGER DEFAULT 0,
      created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
      FOREIGN KEY (user_id) REFERENCES Users(id),
      FOREIGN KEY (item_id) REFERENCES ShopItems(id)
    )
  `);
  
  saveDatabase();
  
  // Seed data - always reset and reseed
  seedUsers();
}

function seedUsers() {
  // Clean up all existing data
  dbRun('DELETE FROM QuestCompletions');
  dbRun('DELETE FROM Notifications');
  dbRun('DELETE FROM ShopPurchases');
  dbRun('DELETE FROM Transactions');
  dbRun('DELETE FROM Quests');
  dbRun('DELETE FROM ShopItems');
  dbRun('DELETE FROM Users');
  
  console.log('✅ Cleaned up all existing data');
  
  // Add new users
  // 1. Божена - admin, emoji "Bomb" (💣)
  dbRun(`INSERT INTO Users (name, pin_code, avatar_emoji, is_admin, balance) VALUES (?, ?, ?, ?, ?)`,
    ['Божена', 'bog6', '💣', 1, 0]);
  
  // 2. Уляна - user, emoji "Joker" (🃏), 3 respects - 0 disrespects
  dbRun(`INSERT INTO Users (name, pin_code, avatar_emoji, is_admin, balance) VALUES (?, ?, ?, ?, ?)`,
    ['Уляна', '1111', '🃏', 0, 0]);
  
  // 3. Андрій - user, emoji "Peach" (🍑), 8 respects - 0 disrespects
  dbRun(`INSERT INTO Users (name, pin_code, avatar_emoji, is_admin, balance) VALUES (?, ?, ?, ?, ?)`,
    ['Андрій', '0606', '🍑', 0, 0]);
  
  // 4. admin - admin, emoji "crown" (👑)
  dbRun(`INSERT INTO Users (name, pin_code, avatar_emoji, is_admin, balance) VALUES (?, ?, ?, ?, ?)`,
    ['admin', '1976', '👑', 1, 0]);
  
  // Get user IDs for creating transactions
  const ulyana = dbGet('SELECT id FROM Users WHERE name = ?', ['Уляна']);
  const andrii = dbGet('SELECT id FROM Users WHERE name = ?', ['Андрій']);
  
  // Create transactions for Уляна: 3 respects
  for (let i = 0; i < 3; i++) {
    dbRun(`
      INSERT INTO Transactions (from_user_id, to_user_id, type, amount, description)
      VALUES (NULL, ?, 'RESPECT', 1, 'Initial respect')
    `, [ulyana.id]);
  }
  
  // Create transactions for Андрій: 8 respects
  for (let i = 0; i < 8; i++) {
    dbRun(`
      INSERT INTO Transactions (from_user_id, to_user_id, type, amount, description)
      VALUES (NULL, ?, 'RESPECT', 1, 'Initial respect')
    `, [andrii.id]);
  }
  
  // Recalculate balances
  const { recalculateUserBalance } = require('../utils/stats');
  recalculateUserBalance(ulyana.id);
  recalculateUserBalance(andrii.id);
  
  console.log('✅ Seed users created');
  console.log('   - Божена (admin) 💣');
  console.log('   - Уляна (user) 🃏 - 3 respects');
  console.log('   - Андрій (user) 🍑 - 8 respects');
  console.log('   - admin (admin) 👑');
}

module.exports = {
  initDatabase,
  dbRun,
  dbGet,
  dbAll
};
