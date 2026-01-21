const express = require('express');
const router = express.Router();
const { requireAuth } = require('../middleware/auth');
const { dbGet } = require('../config/database');
const { getLeaderboard, getRecentTransactions, getPendingCompletionsCount, getUnreadNotificationsCount } = require('../utils/stats');

// GET /
router.get('/', requireAuth, (req, res) => {
  // Refresh user data
  req.session.user = dbGet('SELECT * FROM Users WHERE id = ?', [req.session.user.id]);
  
  const leaderboard = getLeaderboard();
  const transactions = getRecentTransactions();
  const pendingCount = req.session.user.is_admin ? getPendingCompletionsCount() : 0;
  const notificationCount = req.session.user.is_admin ? getUnreadNotificationsCount(req.session.user.id) : 0;
  
  res.render('index', { 
    leaderboard, 
    transactions,
    currentUser: req.session.user,
    pendingCount,
    notificationCount
  });
});

module.exports = router;
