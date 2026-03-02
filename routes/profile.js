const express = require('express');
const router = express.Router();
const { requireAuth } = require('../middleware/auth');
const { dbGet } = require('../config/database');
const { getUserStats, getProfileStats, getProfileStatsByMonth, getUserTransactionHistory, getUserRank } = require('../utils/stats');

router.get('/:userId', requireAuth, async (req, res) => {
  const userId = parseInt(req.params.userId);
  const user = await dbGet('SELECT * FROM Users WHERE id = $1 AND is_admin = 0', [userId]);

  if (!user) {
    req.session.flash = { type: 'error', message: 'User not found!' };
    return res.redirect('/');
  }

  const [stats, weekStats, monthStats, yearStats, transactions, rank] = await Promise.all([
    getUserStats(userId),
    getProfileStats(userId, 7),
    getProfileStats(userId, 30),
    getProfileStatsByMonth(userId, 12),
    getUserTransactionHistory(userId),
    getUserRank(userId)
  ]);

  res.render('profile', {
    profileUser: user,
    stats,
    weekStats,
    monthStats,
    yearStats,
    transactions,
    rank,
    currentUser: req.session.user
  });
});

module.exports = router;
