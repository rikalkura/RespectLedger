const express = require('express');
const router = express.Router();
const { requireAuth, requireAdmin } = require('../middleware/auth');
const { dbGet, dbAll, dbRun } = require('../config/database');
const upload = require('../config/multer');
const cloudinary = require('../config/cloudinary');
const { uploadToCloudinary } = require('../utils/cloudinary');
const { recalculateUserBalance, getUnreadNotificationsCount, getPendingPurchasesCount } = require('../utils/stats');

// GET /admin
router.get('/admin', requireAuth, requireAdmin, (req, res) => {
  const items = dbAll('SELECT * FROM ShopItems ORDER BY id DESC');
  const quests = dbAll('SELECT * FROM Quests ORDER BY id DESC');
  const users = dbAll('SELECT * FROM Users ORDER BY id');
  
  // Get pending quest completions with user and quest info
  const pendingCompletions = dbAll(`
    SELECT 
      qc.*,
      q.title as quest_title,
      q.reward as quest_reward,
      u.name as user_name,
      u.avatar_emoji as user_emoji
    FROM QuestCompletions qc
    JOIN Quests q ON qc.quest_id = q.id
    JOIN Users u ON qc.user_id = u.id
    WHERE qc.status = 'PENDING'
    ORDER BY qc.submitted_at ASC
  `);
  
  // Get pending shop purchases with user and item info
  const pendingPurchases = dbAll(`
    SELECT 
      sp.*,
      si.name as item_name,
      si.price as item_price,
      si.image_url as item_image,
      u.name as user_name,
      u.avatar_emoji as user_emoji
    FROM ShopPurchases sp
    JOIN ShopItems si ON sp.item_id = si.id
    JOIN Users u ON sp.user_id = u.id
    WHERE sp.status = 'PENDING'
    ORDER BY sp.purchased_at ASC
  `);
  
  const pendingCount = pendingCompletions.length + pendingPurchases.length;
  
  // Get unread notifications for this admin
  const allNotifications = dbAll(`
    SELECT 
      n.*,
      si.name as item_name,
      si.image_url as item_image
    FROM Notifications n
    LEFT JOIN ShopItems si ON n.item_id = si.id
    WHERE n.user_id = ?
    ORDER BY n.created_at DESC
    LIMIT 20
  `, [req.session.user.id]);
  
  const unreadNotifications = allNotifications.filter(n => n.is_read === 0);
  const notificationCount = unreadNotifications.length;
  
  res.render('admin', { 
    items, 
    quests, 
    users, 
    pendingCompletions, 
    pendingPurchases,
    pendingCount,
    notifications: unreadNotifications,
    notificationCount
  });
});

// POST /admin/quests/approve/:completionId
router.post('/admin/quests/approve/:completionId', requireAuth, requireAdmin, (req, res) => {
  const completionId = parseInt(req.params.completionId);
  const adminId = req.session.user.id;
  
  const completion = dbGet(`
    SELECT qc.*, q.title, q.reward 
    FROM QuestCompletions qc
    JOIN Quests q ON qc.quest_id = q.id
    WHERE qc.id = ? AND qc.status = 'PENDING'
  `, [completionId]);
  
  if (!completion) {
    req.session.flash = { type: 'error', message: 'Completion request not found!' };
    return res.redirect('/admin');
  }
  
  // Update completion status
  dbRun(`
    UPDATE QuestCompletions 
    SET status = 'APPROVED', reviewed_at = CURRENT_TIMESTAMP, reviewed_by = ?
    WHERE id = ?
  `, [adminId, completionId]);
  
  // Create QUEST_REWARD transaction (counts as respect!)
  dbRun(`
    INSERT INTO Transactions (from_user_id, to_user_id, type, amount, description)
    VALUES (NULL, ?, 'QUEST_REWARD', ?, ?)
  `, [completion.user_id, completion.reward, `Completed quest: ${completion.title}`]);
  
  // Recalculate balance (respects - disrespects)
  recalculateUserBalance(completion.user_id);
  
  req.session.flash = { type: 'success', message: `Quest completion approved! +1 respect awarded.` };
  res.redirect('/admin');
});

// POST /admin/quests/reject/:completionId
router.post('/admin/quests/reject/:completionId', requireAuth, requireAdmin, (req, res) => {
  const completionId = parseInt(req.params.completionId);
  const adminId = req.session.user.id;
  
  const completion = dbGet('SELECT * FROM QuestCompletions WHERE id = ? AND status = ?', [completionId, 'PENDING']);
  
  if (!completion) {
    req.session.flash = { type: 'error', message: 'Completion request not found!' };
    return res.redirect('/admin');
  }
  
  // Update completion status
  dbRun(`
    UPDATE QuestCompletions 
    SET status = 'REJECTED', reviewed_at = CURRENT_TIMESTAMP, reviewed_by = ?
    WHERE id = ?
  `, [adminId, completionId]);
  
  req.session.flash = { type: 'success', message: 'Quest completion rejected.' };
  res.redirect('/admin');
});

// POST /admin/items
router.post('/admin/items', requireAuth, requireAdmin, upload.single('image'), async (req, res) => {
  try {
    const { name, price } = req.body;
    
    if (!name || !price) {
      req.session.flash = { type: 'error', message: 'Name and price are required!' };
      return res.redirect('/admin');
    }
    
    let imageUrl = null;
    let cloudinaryPublicId = null;
    
    if (req.file) {
      const result = await uploadToCloudinary(req.file.buffer, 'respect-ledger/items');
      imageUrl = result.secure_url;
      cloudinaryPublicId = result.public_id;
    }
    
    dbRun(`
      INSERT INTO ShopItems (name, price, image_url, cloudinary_public_id)
      VALUES (?, ?, ?, ?)
    `, [name, parseInt(price), imageUrl, cloudinaryPublicId]);
    
    req.session.flash = { type: 'success', message: 'Item added successfully!' };
    res.redirect('/admin');
  } catch (error) {
    console.error('Error adding item:', error);
    req.session.flash = { type: 'error', message: 'Failed to add item!' };
    res.redirect('/admin');
  }
});

// POST /admin/items/delete/:itemId
router.post('/admin/items/delete/:itemId', requireAuth, requireAdmin, async (req, res) => {
  try {
    const itemId = parseInt(req.params.itemId);
    
    const item = dbGet('SELECT * FROM ShopItems WHERE id = ?', [itemId]);
    if (!item) {
      req.session.flash = { type: 'error', message: 'Item not found!' };
      return res.redirect('/admin');
    }
    
    // Delete from Cloudinary if exists
    if (item.cloudinary_public_id) {
      await cloudinary.uploader.destroy(item.cloudinary_public_id);
    }
    
    // Delete from DB
    dbRun('DELETE FROM ShopItems WHERE id = ?', [itemId]);
    
    req.session.flash = { type: 'success', message: 'Item deleted successfully!' };
    res.redirect('/admin');
  } catch (error) {
    console.error('Error deleting item:', error);
    req.session.flash = { type: 'error', message: 'Failed to delete item!' };
    res.redirect('/admin');
  }
});

// POST /admin/quests
router.post('/admin/quests', requireAuth, requireAdmin, (req, res) => {
  const { title, reward } = req.body;
  
  if (!title || !reward) {
    req.session.flash = { type: 'error', message: 'Title and reward are required!' };
    return res.redirect('/admin');
  }
  
  dbRun(`
    INSERT INTO Quests (title, reward, is_active)
    VALUES (?, ?, 1)
  `, [title, parseInt(reward)]);
  
  req.session.flash = { type: 'success', message: 'Quest added successfully!' };
  res.redirect('/admin');
});

// POST /admin/quests/delete/:questId
router.post('/admin/quests/delete/:questId', requireAuth, requireAdmin, (req, res) => {
  const questId = parseInt(req.params.questId);
  
  // Also delete any completions for this quest
  dbRun('DELETE FROM QuestCompletions WHERE quest_id = ?', [questId]);
  dbRun('DELETE FROM Quests WHERE id = ?', [questId]);
  
  req.session.flash = { type: 'success', message: 'Quest deleted successfully!' };
  res.redirect('/admin');
});

// POST /admin/quests/toggle/:questId
router.post('/admin/quests/toggle/:questId', requireAuth, requireAdmin, (req, res) => {
  const questId = parseInt(req.params.questId);
  
  const quest = dbGet('SELECT is_active FROM Quests WHERE id = ?', [questId]);
  if (quest) {
    dbRun('UPDATE Quests SET is_active = ? WHERE id = ?', [quest.is_active ? 0 : 1, questId]);
  }
  
  req.session.flash = { type: 'success', message: 'Quest toggled!' };
  res.redirect('/admin');
});

// POST /admin/users
router.post('/admin/users', requireAuth, requireAdmin, (req, res) => {
  const { name, pin_code, avatar_emoji, is_admin } = req.body;
  
  if (!name || !pin_code) {
    req.session.flash = { type: 'error', message: 'Name and PIN are required!' };
    return res.redirect('/admin');
  }
  
  try {
    // Check if user already exists
    const existing = dbGet('SELECT id FROM Users WHERE name = ?', [name]);
    if (existing) {
      req.session.flash = { type: 'error', message: 'User name already exists!' };
      return res.redirect('/admin');
    }
    
    dbRun(`
      INSERT INTO Users (name, pin_code, avatar_emoji, is_admin, balance)
      VALUES (?, ?, ?, ?, 0)
    `, [name, pin_code, avatar_emoji || '😀', is_admin ? 1 : 0]);
    
    req.session.flash = { type: 'success', message: 'User added successfully!' };
  } catch (error) {
    console.error('Error adding user:', error);
    req.session.flash = { type: 'error', message: 'Failed to add user!' };
  }
  
  res.redirect('/admin');
});

// POST /admin/notifications/read/:notificationId
router.post('/admin/notifications/read/:notificationId', requireAuth, requireAdmin, (req, res) => {
  const notificationId = parseInt(req.params.notificationId);
  
  dbRun('UPDATE Notifications SET is_read = 1 WHERE id = ?', [notificationId]);
  
  res.redirect('/admin');
});

// POST /admin/notifications/read-all
router.post('/admin/notifications/read-all', requireAuth, requireAdmin, (req, res) => {
  dbRun('UPDATE Notifications SET is_read = 1 WHERE user_id = ? AND is_read = 0', [req.session.user.id]);
  
  req.session.flash = { type: 'success', message: 'All notifications marked as read!' };
  res.redirect('/admin');
});

// POST /admin/purchases/approve/:purchaseId
router.post('/admin/purchases/approve/:purchaseId', requireAuth, requireAdmin, (req, res) => {
  const purchaseId = parseInt(req.params.purchaseId);
  const adminId = req.session.user.id;
  
  const purchase = dbGet(`
    SELECT sp.*, si.name as item_name, si.price as item_price, u.name as user_name, u.avatar_emoji as user_emoji
    FROM ShopPurchases sp
    JOIN ShopItems si ON sp.item_id = si.id
    JOIN Users u ON sp.user_id = u.id
    WHERE sp.id = ? AND sp.status = 'PENDING'
  `, [purchaseId]);
  
  if (!purchase) {
    req.session.flash = { type: 'error', message: 'Purchase request not found!' };
    return res.redirect('/admin');
  }
  
  const admin = dbGet('SELECT name, avatar_emoji FROM Users WHERE id = ?', [adminId]);
  
  // Update purchase status
  dbRun(`
    UPDATE ShopPurchases 
    SET status = 'BOUGHT', reviewed_at = CURRENT_TIMESTAMP, reviewed_by = ?
    WHERE id = ?
  `, [adminId, purchaseId]);
  
  // Create transaction showing admin bought the item for the user
  dbRun(`
    INSERT INTO Transactions (from_user_id, to_user_id, type, amount, description)
    VALUES (?, ?, 'SHOP_PURCHASE', 0, ?)
  `, [adminId, purchase.user_id, `${admin.name} bought "${purchase.item_name}" for ${purchase.user_name}`]);
  
  req.session.flash = { type: 'success', message: `Purchase approved! Transaction created.` };
  res.redirect('/admin');
});

// POST /admin/purchases/reject/:purchaseId
router.post('/admin/purchases/reject/:purchaseId', requireAuth, requireAdmin, (req, res) => {
  const purchaseId = parseInt(req.params.purchaseId);
  const adminId = req.session.user.id;
  
  const purchase = dbGet('SELECT * FROM ShopPurchases WHERE id = ? AND status = ?', [purchaseId, 'PENDING']);
  
  if (!purchase) {
    req.session.flash = { type: 'error', message: 'Purchase request not found!' };
    return res.redirect('/admin');
  }
  
  // Update purchase status
  dbRun(`
    UPDATE ShopPurchases 
    SET status = 'REJECTED', reviewed_at = CURRENT_TIMESTAMP, reviewed_by = ?
    WHERE id = ?
  `, [adminId, purchaseId]);
  
  // Refund the user (remove the pending transaction deduction)
  // Find and remove the pending purchase transaction
  const pendingTx = dbGet(`
    SELECT id FROM Transactions 
    WHERE from_user_id = ? AND type = 'SHOP_PURCHASE' AND description LIKE ?
    ORDER BY timestamp DESC LIMIT 1
  `, [purchase.user_id, `%Pending purchase:%`]);
  
  if (pendingTx) {
    // Refund by creating a positive transaction
    const item = dbGet('SELECT name, price FROM ShopItems WHERE id = ?', [purchase.item_id]);
    dbRun(`
      INSERT INTO Transactions (from_user_id, to_user_id, type, amount, description)
      VALUES (NULL, ?, 'SHOP_PURCHASE', ?, ?)
    `, [purchase.user_id, item.price, `Refund: ${item.name} (rejected)`]);
    
    // Recalculate balance
    recalculateUserBalance(purchase.user_id);
  }
  
  req.session.flash = { type: 'success', message: 'Purchase rejected. User refunded.' };
  res.redirect('/admin');
});

module.exports = router;
