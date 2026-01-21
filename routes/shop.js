const express = require('express');
const router = express.Router();
const { requireAuth } = require('../middleware/auth');
const { dbGet, dbAll, dbRun } = require('../config/database');
const { getPendingCompletionsCount, getUnreadNotificationsCount, recalculateUserBalance } = require('../utils/stats');

// GET /shop
router.get('/shop', requireAuth, (req, res) => {
  // Refresh user data
  req.session.user = dbGet('SELECT * FROM Users WHERE id = ?', [req.session.user.id]);
  
  const items = dbAll('SELECT * FROM ShopItems ORDER BY price ASC');
  const pendingCount = req.session.user.is_admin ? getPendingCompletionsCount() : 0;
  const notificationCount = req.session.user.is_admin ? getUnreadNotificationsCount(req.session.user.id) : 0;
  
  res.render('shop', { items, currentUser: req.session.user, pendingCount, notificationCount });
});

// POST /shop/buy/:itemId
router.post('/shop/buy/:itemId', requireAuth, (req, res) => {
  const itemId = parseInt(req.params.itemId);
  const userId = req.session.user.id;
  
  const item = dbGet('SELECT * FROM ShopItems WHERE id = ?', [itemId]);
  if (!item) {
    req.session.flash = { type: 'error', message: 'Item not found!' };
    return res.redirect('/shop');
  }
  
  const user = dbGet('SELECT * FROM Users WHERE id = ?', [userId]);
  if (user.balance < item.price) {
    req.session.flash = { type: 'error', message: 'Not enough balance! 💸' };
    return res.redirect('/shop');
  }
  
  // Create pending purchase (not a transaction yet)
  dbRun(`
    INSERT INTO ShopPurchases (user_id, item_id, status)
    VALUES (?, ?, 'PENDING')
  `, [userId, itemId]);
  
  // Deduct balance immediately (so user can't buy more than they can afford)
  dbRun(`
    INSERT INTO Transactions (from_user_id, to_user_id, type, amount, description)
    VALUES (?, NULL, 'SHOP_PURCHASE', ?, ?)
  `, [userId, -item.price, `Pending purchase: ${item.name}`]);
  
  // Recalculate balance (respects - disrespects - shop purchases)
  recalculateUserBalance(userId);
  
  // Notify all admins about the pending purchase
  const admins = dbAll('SELECT id FROM Users WHERE is_admin = 1');
  admins.forEach(admin => {
    dbRun(`
      INSERT INTO Notifications (type, user_id, item_id, message, is_read)
      VALUES ('SHOP_PURCHASE', ?, ?, ?, 0)
    `, [admin.id, itemId, `${user.name} (${user.avatar_emoji}) wants to buy "${item.name}" for ${item.price} 🥚`]);
  });
  
  req.session.flash = { type: 'success', message: `You bought ${item.name}! 🎉` };
  res.redirect('/shop');
});

module.exports = router;
