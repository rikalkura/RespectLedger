const { Client } = require('pg');

let client = null;

// Database helper functions
async function dbRun(sql, params = []) {
  if (!client) {
    throw new Error('Database not initialized. Call initDatabase() first.');
  }
  
  // Convert ? placeholders to $1, $2, etc.
  const convertedSql = convertPlaceholders(sql);
  await client.query(convertedSql, params);
}

async function dbGet(sql, params = []) {
  if (!client) {
    throw new Error('Database not initialized. Call initDatabase() first.');
  }
  
  // Convert ? placeholders to $1, $2, etc.
  const convertedSql = convertPlaceholders(sql);
  const result = await client.query(convertedSql, params);
  return result.rows.length > 0 ? result.rows[0] : null;
}

async function dbAll(sql, params = []) {
  if (!client) {
    throw new Error('Database not initialized. Call initDatabase() first.');
  }
  
  // Convert ? placeholders to $1, $2, etc.
  const convertedSql = convertPlaceholders(sql);
  const result = await client.query(convertedSql, params);
  return result.rows;
}

// Helper function to convert SQLite ? placeholders to PostgreSQL $1, $2, etc.
function convertPlaceholders(sql) {
  let placeholderIndex = 1;
  return sql.replace(/\?/g, () => `$${placeholderIndex++}`);
}

// Database initialization
async function initDatabase() {
  // Create PostgreSQL client with SSL for Render
  client = new Client({
    connectionString: process.env.DATABASE_URL,
    ssl: { rejectUnauthorized: false }
  });
  
  try {
    await client.connect();
    console.log('✅ Connected to PostgreSQL database');
    
    // Create tables
    await client.query(`
      CREATE TABLE IF NOT EXISTS Users (
        id SERIAL PRIMARY KEY,
        name TEXT NOT NULL UNIQUE,
        pin_code TEXT NOT NULL,
        avatar_emoji TEXT DEFAULT '😀',
        is_admin INTEGER DEFAULT 0,
        balance INTEGER DEFAULT 0
      )
    `);
    
    await client.query(`
      CREATE TABLE IF NOT EXISTS Transactions (
        id SERIAL PRIMARY KEY,
        from_user_id INTEGER,
        to_user_id INTEGER,
        type TEXT NOT NULL CHECK(type IN ('RESPECT', 'DISRESPECT', 'SHOP_PURCHASE', 'QUEST_REWARD')),
        amount INTEGER NOT NULL,
        description TEXT,
        timestamp TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
        FOREIGN KEY (from_user_id) REFERENCES Users(id),
        FOREIGN KEY (to_user_id) REFERENCES Users(id)
      )
    `);
    
    await client.query(`
      CREATE TABLE IF NOT EXISTS ShopItems (
        id SERIAL PRIMARY KEY,
        name TEXT NOT NULL,
        price INTEGER NOT NULL,
        image_url TEXT,
        cloudinary_public_id TEXT
      )
    `);
    
    await client.query(`
      CREATE TABLE IF NOT EXISTS ShopPurchases (
        id SERIAL PRIMARY KEY,
        user_id INTEGER NOT NULL,
        item_id INTEGER NOT NULL,
        status TEXT NOT NULL DEFAULT 'PENDING' CHECK(status IN ('PENDING', 'BOUGHT', 'REJECTED')),
        purchased_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
        reviewed_at TIMESTAMP,
        reviewed_by INTEGER,
        FOREIGN KEY (user_id) REFERENCES Users(id),
        FOREIGN KEY (item_id) REFERENCES ShopItems(id),
        FOREIGN KEY (reviewed_by) REFERENCES Users(id)
      )
    `);
    
    await client.query(`
      CREATE TABLE IF NOT EXISTS Quests (
        id SERIAL PRIMARY KEY,
        title TEXT NOT NULL,
        reward INTEGER NOT NULL,
        is_active INTEGER DEFAULT 1
      )
    `);
    
    await client.query(`
      CREATE TABLE IF NOT EXISTS QuestCompletions (
        id SERIAL PRIMARY KEY,
        quest_id INTEGER NOT NULL,
        user_id INTEGER NOT NULL,
        status TEXT NOT NULL DEFAULT 'PENDING' CHECK(status IN ('PENDING', 'APPROVED', 'REJECTED')),
        submitted_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
        reviewed_at TIMESTAMP,
        reviewed_by INTEGER,
        FOREIGN KEY (quest_id) REFERENCES Quests(id),
        FOREIGN KEY (user_id) REFERENCES Users(id),
        FOREIGN KEY (reviewed_by) REFERENCES Users(id)
      )
    `);
    
    await client.query(`
      CREATE TABLE IF NOT EXISTS Notifications (
        id SERIAL PRIMARY KEY,
        type TEXT NOT NULL CHECK(type IN ('SHOP_PURCHASE', 'QUEST_APPROVAL')),
        user_id INTEGER,
        item_id INTEGER,
        message TEXT NOT NULL,
        is_read INTEGER DEFAULT 0,
        created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
        FOREIGN KEY (user_id) REFERENCES Users(id),
        FOREIGN KEY (item_id) REFERENCES ShopItems(id)
      )
    `);
    
    console.log('✅ Database tables created/verified');
    
    // Seed data - always reset and reseed
    await seedUsers();
  } catch (error) {
    console.error('Database initialization error:', error);
    throw error;
  }
}

async function seedUsers() {
  // Clean up all existing data
  await dbRun('DELETE FROM QuestCompletions');
  await dbRun('DELETE FROM Notifications');
  await dbRun('DELETE FROM ShopPurchases');
  await dbRun('DELETE FROM Transactions');
  await dbRun('DELETE FROM Quests');
  await dbRun('DELETE FROM ShopItems');
  await dbRun('DELETE FROM Users');
  
  console.log('✅ Cleaned up all existing data');
  
  // Add new users
  // 1. Божена - admin, emoji "Bomb" (💣)
  await dbRun(`INSERT INTO Users (name, pin_code, avatar_emoji, is_admin, balance) VALUES ($1, $2, $3, $4, $5)`,
    ['Божена', 'bog6', '💣', 1, 0]);
  
  // 2. Уляна - user, emoji "Joker" (🃏), 3 respects - 0 disrespects
  await dbRun(`INSERT INTO Users (name, pin_code, avatar_emoji, is_admin, balance) VALUES ($1, $2, $3, $4, $5)`,
    ['Уляна', '1111', '🃏', 0, 0]);
  
  // 3. Андрій - user, emoji "Peach" (🍑), 8 respects - 0 disrespects
  await dbRun(`INSERT INTO Users (name, pin_code, avatar_emoji, is_admin, balance) VALUES ($1, $2, $3, $4, $5)`,
    ['Андрій', '0606', '🍑', 0, 0]);
  
  // 4. admin - admin, emoji "crown" (👑)
  await dbRun(`INSERT INTO Users (name, pin_code, avatar_emoji, is_admin, balance) VALUES ($1, $2, $3, $4, $5)`,
    ['admin', '1976', '👑', 1, 0]);
  
  // Get user IDs for creating transactions
  const ulyana = await dbGet('SELECT id FROM Users WHERE name = $1', ['Уляна']);
  const andrii = await dbGet('SELECT id FROM Users WHERE name = $1', ['Андрій']);
  
  // Create transactions for Уляна: 3 respects
  for (let i = 0; i < 3; i++) {
    await dbRun(`
      INSERT INTO Transactions (from_user_id, to_user_id, type, amount, description)
      VALUES (NULL, $1, 'RESPECT', 1, 'Initial respect')
    `, [ulyana.id]);
  }
  
  // Create transactions for Андрій: 8 respects
  for (let i = 0; i < 8; i++) {
    await dbRun(`
      INSERT INTO Transactions (from_user_id, to_user_id, type, amount, description)
      VALUES (NULL, $1, 'RESPECT', 1, 'Initial respect')
    `, [andrii.id]);
  }
  
  // Recalculate balances
  const { recalculateUserBalance } = require('../utils/stats');
  await recalculateUserBalance(ulyana.id);
  await recalculateUserBalance(andrii.id);
  
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
