const express = require('express');
const router = express.Router();
const { requireAuth, requireAdmin } = require('../middleware/auth');
const { dbGet, dbAll, dbRun } = require('../config/database');
const upload = require('../config/multer');
const cloudinary = require('../config/cloudinary');
const { uploadToCloudinary } = require('../utils/cloudinary');
const { recalculateUserBalance, getUnreadNotificationsCount, getPendingPurchasesCount } = require('../utils/stats');

// GET /admin
router.get('/admin', requireAuth, requireAdmin, async (req, res) => {
  const items = await dbAll('SELECT * FROM ShopItems ORDER BY id DESC');
  const quests = await dbAll('SELECT * FROM Quests ORDER BY id DESC');
  const users = await dbAll('SELECT * FROM Users ORDER BY id');
  
  // Get pending quest completions with user and quest info
  const pendingCompletions = await dbAll(`
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
  const pendingPurchases = await dbAll(`
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
  const allNotifications = await dbAll(`
    SELECT 
      n.*,
      si.name as item_name,
      si.image_url as item_image
    FROM Notifications n
    LEFT JOIN ShopItems si ON n.item_id = si.id
    WHERE n.user_id = $1
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
router.post('/admin/quests/approve/:completionId', requireAuth, requireAdmin, async (req, res) => {
  const completionId = parseInt(req.params.completionId);
  const adminId = req.session.user.id;
  
  const completion = await dbGet(`
    SELECT qc.*, q.title, q.reward 
    FROM QuestCompletions qc
    JOIN Quests q ON qc.quest_id = q.id
    WHERE qc.id = $1 AND qc.status = 'PENDING'
  `, [completionId]);
  
  if (!completion) {
    req.session.flash = { type: 'error', message: 'Completion request not found!' };
    return res.redirect('/admin');
  }
  
  // Update completion status
  await dbRun(`
    UPDATE QuestCompletions 
    SET status = 'APPROVED', reviewed_at = CURRENT_TIMESTAMP, reviewed_by = $1
    WHERE id = $2
  `, [adminId, completionId]);
  
  // Create QUEST_REWARD transaction (counts as respect!)
  await dbRun(`
    INSERT INTO Transactions (from_user_id, to_user_id, type, amount, description)
    VALUES (NULL, $1, 'QUEST_REWARD', $2, $3)
  `, [completion.user_id, completion.reward, `Completed quest: ${completion.title}`]);
  
  // Recalculate balance (respects - disrespects)
  await recalculateUserBalance(completion.user_id);
  
  req.session.flash = { type: 'success', message: `Quest completion approved! +1 respect awarded.` };
  res.redirect('/admin');
});

// POST /admin/quests/reject/:completionId
router.post('/admin/quests/reject/:completionId', requireAuth, requireAdmin, async (req, res) => {
  const completionId = parseInt(req.params.completionId);
  const adminId = req.session.user.id;
  
  const completion = await dbGet('SELECT * FROM QuestCompletions WHERE id = $1 AND status = $2', [completionId, 'PENDING']);
  
  if (!completion) {
    req.session.flash = { type: 'error', message: 'Completion request not found!' };
    return res.redirect('/admin');
  }
  
  // Update completion status
  await dbRun(`
    UPDATE QuestCompletions 
    SET status = 'REJECTED', reviewed_at = CURRENT_TIMESTAMP, reviewed_by = $1
    WHERE id = $2
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
    
    await dbRun(`
      INSERT INTO ShopItems (name, price, image_url, cloudinary_public_id)
      VALUES ($1, $2, $3, $4)
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
    
    const item = await dbGet('SELECT * FROM ShopItems WHERE id = $1', [itemId]);
    if (!item) {
      req.session.flash = { type: 'error', message: 'Item not found!' };
      return res.redirect('/admin');
    }
    
    // Delete from Cloudinary if exists
    if (item.cloudinary_public_id) {
      await cloudinary.uploader.destroy(item.cloudinary_public_id);
    }
    
    // Delete from DB
    await dbRun('DELETE FROM ShopItems WHERE id = $1', [itemId]);
    
    req.session.flash = { type: 'success', message: 'Item deleted successfully!' };
    res.redirect('/admin');
  } catch (error) {
    console.error('Error deleting item:', error);
    req.session.flash = { type: 'error', message: 'Failed to delete item!' };
    res.redirect('/admin');
  }
});

// POST /admin/quests
router.post('/admin/quests', requireAuth, requireAdmin, async (req, res) => {
  const { title, reward } = req.body;
  
  if (!title || !reward) {
    req.session.flash = { type: 'error', message: 'Title and reward are required!' };
    return res.redirect('/admin');
  }
  
  await dbRun(`
    INSERT INTO Quests (title, reward, is_active)
    VALUES ($1, $2, 1)
  `, [title, parseInt(reward)]);
  
  req.session.flash = { type: 'success', message: 'Quest added successfully!' };
  res.redirect('/admin');
});

// POST /admin/quests/delete/:questId
router.post('/admin/quests/delete/:questId', requireAuth, requireAdmin, async (req, res) => {
  const questId = parseInt(req.params.questId);
  
  // Also delete any completions for this quest
  await dbRun('DELETE FROM QuestCompletions WHERE quest_id = $1', [questId]);
  await dbRun('DELETE FROM Quests WHERE id = $1', [questId]);
  
  req.session.flash = { type: 'success', message: 'Quest deleted successfully!' };
  res.redirect('/admin');
});

// POST /admin/quests/toggle/:questId
router.post('/admin/quests/toggle/:questId', requireAuth, requireAdmin, async (req, res) => {
  const questId = parseInt(req.params.questId);
  
  const quest = await dbGet('SELECT is_active FROM Quests WHERE id = $1', [questId]);
  if (quest) {
    await dbRun('UPDATE Quests SET is_active = $1 WHERE id = $2', [quest.is_active ? 0 : 1, questId]);
  }
  
  req.session.flash = { type: 'success', message: 'Quest toggled!' };
  res.redirect('/admin');
});

// POST /admin/users
router.post('/admin/users', requireAuth, requireAdmin, async (req, res) => {
  const { name, pin_code, avatar_emoji, is_admin } = req.body;
  
  if (!name || !pin_code) {
    req.session.flash = { type: 'error', message: 'Name and PIN are required!' };
    return res.redirect('/admin');
  }
  
  try {
    // Check if user already exists
    const existing = await dbGet('SELECT id FROM Users WHERE name = $1', [name]);
    if (existing) {
      req.session.flash = { type: 'error', message: 'User name already exists!' };
      return res.redirect('/admin');
    }
    
    await dbRun(`
      INSERT INTO Users (name, pin_code, avatar_emoji, is_admin, balance)
      VALUES ($1, $2, $3, $4, 0)
    `, [name, pin_code, avatar_emoji || '😀', is_admin ? 1 : 0]);
    
    req.session.flash = { type: 'success', message: 'User added successfully!' };
  } catch (error) {
    console.error('Error adding user:', error);
    req.session.flash = { type: 'error', message: 'Failed to add user!' };
  }
  
  res.redirect('/admin');
});

// POST /admin/notifications/read/:notificationId
router.post('/admin/notifications/read/:notificationId', requireAuth, requireAdmin, async (req, res) => {
  const notificationId = parseInt(req.params.notificationId);
  
  await dbRun('UPDATE Notifications SET is_read = 1 WHERE id = $1', [notificationId]);
  
  res.redirect('/admin');
});

// POST /admin/notifications/read-all
router.post('/admin/notifications/read-all', requireAuth, requireAdmin, async (req, res) => {
  await dbRun('UPDATE Notifications SET is_read = 1 WHERE user_id = $1 AND is_read = 0', [req.session.user.id]);
  
  req.session.flash = { type: 'success', message: 'All notifications marked as read!' };
  res.redirect('/admin');
});

// POST /admin/purchases/approve/:purchaseId
router.post('/admin/purchases/approve/:purchaseId', requireAuth, requireAdmin, async (req, res) => {
  const purchaseId = parseInt(req.params.purchaseId);
  const adminId = req.session.user.id;
  
  const purchase = await dbGet(`
    SELECT sp.*, si.name as item_name, si.price as item_price, u.name as user_name, u.avatar_emoji as user_emoji
    FROM ShopPurchases sp
    JOIN ShopItems si ON sp.item_id = si.id
    JOIN Users u ON sp.user_id = u.id
    WHERE sp.id = $1 AND sp.status = 'PENDING'
  `, [purchaseId]);
  
  if (!purchase) {
    req.session.flash = { type: 'error', message: 'Purchase request not found!' };
    return res.redirect('/admin');
  }
  
  const admin = await dbGet('SELECT name, avatar_emoji FROM Users WHERE id = $1', [adminId]);
  
  // Update purchase status
  await dbRun(`
    UPDATE ShopPurchases 
    SET status = 'BOUGHT', reviewed_at = CURRENT_TIMESTAMP, reviewed_by = $1
    WHERE id = $2
  `, [adminId, purchaseId]);
  
  // Create transaction showing admin bought the item for the user
  await dbRun(`
    INSERT INTO Transactions (from_user_id, to_user_id, type, amount, description)
    VALUES ($1, $2, 'SHOP_PURCHASE', 0, $3)
  `, [adminId, purchase.user_id, `${admin.name} bought "${purchase.item_name}" for ${purchase.user_name}`]);
  
  req.session.flash = { type: 'success', message: `Purchase approved! Transaction created.` };
  res.redirect('/admin');
});

// POST /admin/purchases/reject/:purchaseId
router.post('/admin/purchases/reject/:purchaseId', requireAuth, requireAdmin, async (req, res) => {
  const purchaseId = parseInt(req.params.purchaseId);
  const adminId = req.session.user.id;
  
  const purchase = await dbGet('SELECT * FROM ShopPurchases WHERE id = $1 AND status = $2', [purchaseId, 'PENDING']);
  
  if (!purchase) {
    req.session.flash = { type: 'error', message: 'Purchase request not found!' };
    return res.redirect('/admin');
  }
  
  // Update purchase status
  await dbRun(`
    UPDATE ShopPurchases 
    SET status = 'REJECTED', reviewed_at = CURRENT_TIMESTAMP, reviewed_by = $1
    WHERE id = $2
  `, [adminId, purchaseId]);
  
  // Refund the user (remove the pending transaction deduction)
  // Find and remove the pending purchase transaction
  const pendingTx = await dbGet(`
    SELECT id FROM Transactions 
    WHERE from_user_id = $1 AND type = 'SHOP_PURCHASE' AND description LIKE $2
    ORDER BY timestamp DESC LIMIT 1
  `, [purchase.user_id, `%Pending purchase:%`]);
  
  if (pendingTx) {
    // Refund by creating a positive transaction
    const item = await dbGet('SELECT name, price FROM ShopItems WHERE id = $1', [purchase.item_id]);
    await dbRun(`
      INSERT INTO Transactions (from_user_id, to_user_id, type, amount, description)
      VALUES (NULL, $1, 'SHOP_PURCHASE', $2, $3)
    `, [purchase.user_id, item.price, `Refund: ${item.name} (rejected)`]);
    
    // Recalculate balance
    await recalculateUserBalance(purchase.user_id);
  }
  
  req.session.flash = { type: 'success', message: 'Purchase rejected. User refunded.' };
  res.redirect('/admin');
});

module.exports = router;
