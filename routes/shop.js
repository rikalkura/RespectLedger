const express = require('express');
const router = express.Router();
const { requireAuth } = require('../middleware/auth');
const { dbGet, dbAll, dbRun } = require('../config/database');
const { getPendingCompletionsCount, getUnreadNotificationsCount, recalculateUserBalance } = require('../utils/stats');

// GET /shop
router.get('/shop', requireAuth, async (req, res) => {
  // Refresh user data
  req.session.user = await dbGet('SELECT * FROM Users WHERE id = $1', [req.session.user.id]);
  
  const items = await dbAll('SELECT * FROM ShopItems ORDER BY price ASC');
  const pendingCount = req.session.user.is_admin ? await getPendingCompletionsCount() : 0;
  const notificationCount = req.session.user.is_admin ? await getUnreadNotificationsCount(req.session.user.id) : 0;
  
  res.render('shop', { items, currentUser: req.session.user, pendingCount, notificationCount });
});

// POST /shop/buy/:itemId
router.post('/shop/buy/:itemId', requireAuth, async (req, res) => {
  const itemId = parseInt(req.params.itemId);
  const userId = req.session.user.id;
  
  const item = await dbGet('SELECT * FROM ShopItems WHERE id = $1', [itemId]);
  if (!item) {
    req.session.flash = { type: 'error', message: 'Item not found!' };
    return res.redirect('/shop');
  }
  
  const user = await dbGet('SELECT * FROM Users WHERE id = $1', [userId]);
  if (user.balance < item.price) {
    req.session.flash = { type: 'error', message: 'Not enough balance! 💸' };
    return res.redirect('/shop');
  }
  
  // Create pending purchase (not a transaction yet)
  await dbRun(`
    INSERT INTO ShopPurchases (user_id, item_id, status)
    VALUES ($1, $2, 'PENDING')
  `, [userId, itemId]);
  
  // Deduct balance immediately (so user can't buy more than they can afford)
  await dbRun(`
    INSERT INTO Transactions (from_user_id, to_user_id, type, amount, description)
    VALUES ($1, NULL, 'SHOP_PURCHASE', $2, $3)
  `, [userId, -item.price, `Pending purchase: ${item.name}`]);
  
  // Recalculate balance (respects - disrespects - shop purchases)
  await recalculateUserBalance(userId);
  
  // Notify all admins about the pending purchase
  const admins = await dbAll('SELECT id FROM Users WHERE is_admin = 1');
  for (const admin of admins) {
    await dbRun(`
      INSERT INTO Notifications (type, user_id, item_id, message, is_read)
      VALUES ('SHOP_PURCHASE', $1, $2, $3, 0)
    `, [admin.id, itemId, `${user.name} (${user.avatar_emoji}) wants to buy "${item.name}" for ${item.price} 🥚`]);
  }
  
  req.session.flash = { type: 'success', message: `You bought ${item.name}! 🎉` };
  res.redirect('/shop');
});

module.exports = router;
