const express = require('express');
const session = require('express-session');
const path = require('path');
const { initDatabase } = require('./config/database');

// Import routes
const authRoutes = require('./routes/auth');
const dashboardRoutes = require('./routes/dashboard');
const actionsRoutes = require('./routes/actions');
const questsRoutes = require('./routes/quests');
const shopRoutes = require('./routes/shop');
const adminRoutes = require('./routes/admin');
const profileRoutes = require('./routes/profile');

// Import middleware
const flashMiddleware = require('./middleware/flash');

const app = express();
const PORT = process.env.PORT || 3000;

// ============================================
// EXPRESS CONFIGURATION
// ============================================
app.set('view engine', 'ejs');
app.set('views', path.join(__dirname, 'views'));
app.use(express.urlencoded({ extended: true }));
app.use(express.json());
app.use(express.static(path.join(__dirname, 'public')));

app.use(session({
  secret: 'respect-ledger-secret-key-2024',
  resave: false,
  saveUninitialized: false,
  cookie: { secure: false, maxAge: 24 * 60 * 60 * 1000 } // 24 hours
}));

// Flash message middleware
app.use(flashMiddleware);

// ============================================
// ROUTES
// ============================================
app.use('/', authRoutes);
app.use('/', dashboardRoutes);
app.use('/', actionsRoutes);
app.use('/', questsRoutes);
app.use('/', shopRoutes);
app.use('/', adminRoutes);
app.use('/profile', profileRoutes);

// ============================================
// START SERVER
// ============================================
async function startServer() {
  await initDatabase();
  
  // Recalculate all balances on startup (fixes existing data)
  const { recalculateAllBalances } = require('./utils/stats');
  await recalculateAllBalances();
  console.log('✅ Recalculated all user balances');
  
  app.listen(PORT, () => {
    console.log(`
  ╔═══════════════════════════════════════════╗
  ║     🏆 RESPECT LEDGER - RPG EDITION 🏆    ║
  ╠═══════════════════════════════════════════╣
  ║   Server running on http://localhost:${PORT}  ║
  ╠═══════════════════════════════════════════╣
  ║   Admins:                                 ║
  ║     💣 Божена (PIN: bog6)                 ║
  ║     👑 admin (PIN: 1976)                   ║
  ║   Users:                                  ║
  ║     🃏 Уляна (PIN: 1111) - 3 respects     ║
  ║     🍑 Андрій (PIN: 0606) - 8 respects   ║
  ╚═══════════════════════════════════════════╝
    `);
  });
}

startServer().catch(console.error);
