const express = require('express');
const router = express.Router();
const { requireAuth } = require('../middleware/auth');
const { dbGet, dbRun } = require('../config/database');
const { recalculateUserBalance } = require('../utils/stats');

// POST /respect/:userId
router.post('/respect/:userId', requireAuth, (req, res) => {
  // Only admins can give respects/disrespects
  if (!req.session.user.is_admin) {
    req.session.flash = { type: 'error', message: 'Only admins can give respects!' };
    return res.redirect('/');
  }
  
  const toUserId = parseInt(req.params.userId);
  const fromUserId = req.session.user.id;
  const { description } = req.body;
  
  if (toUserId === fromUserId) {
    req.session.flash = { type: 'error', message: "You can't respect yourself!" };
    return res.redirect('/');
  }
  
  const toUser = dbGet('SELECT * FROM Users WHERE id = ?', [toUserId]);
  if (!toUser) {
    req.session.flash = { type: 'error', message: 'User not found!' };
    return res.redirect('/');
  }
  
  // Can't give respects to admins
  if (toUser.is_admin) {
    req.session.flash = { type: 'error', message: "You can't give respects to admins!" };
    return res.redirect('/');
  }
  
  // Create transaction
  dbRun(`
    INSERT INTO Transactions (from_user_id, to_user_id, type, amount, description)
    VALUES (?, ?, 'RESPECT', 1, ?)
  `, [fromUserId, toUserId, description || 'Gave respect']);
  
  // Recalculate balance (respects - disrespects)
  recalculateUserBalance(toUserId);
  
  req.session.flash = { type: 'success', message: `You gave respect to ${toUser.name}! 🟢` };
  res.redirect('/');
});

// POST /disrespect/:userId
router.post('/disrespect/:userId', requireAuth, (req, res) => {
  // Only admins can give respects/disrespects
  if (!req.session.user.is_admin) {
    req.session.flash = { type: 'error', message: 'Only admins can give disrespects!' };
    return res.redirect('/');
  }
  
  const toUserId = parseInt(req.params.userId);
  const fromUserId = req.session.user.id;
  const { description } = req.body;
  
  if (toUserId === fromUserId) {
    req.session.flash = { type: 'error', message: "You can't disrespect yourself!" };
    return res.redirect('/');
  }
  
  const toUser = dbGet('SELECT * FROM Users WHERE id = ?', [toUserId]);
  if (!toUser) {
    req.session.flash = { type: 'error', message: 'User not found!' };
    return res.redirect('/');
  }
  
  // Can't give disrespects to admins
  if (toUser.is_admin) {
    req.session.flash = { type: 'error', message: "You can't give disrespects to admins!" };
    return res.redirect('/');
  }
  
  // Create transaction
  dbRun(`
    INSERT INTO Transactions (from_user_id, to_user_id, type, amount, description)
    VALUES (?, ?, 'DISRESPECT', -1, ?)
  `, [fromUserId, toUserId, description || 'Gave disrespect']);
  
  // Recalculate balance (respects - disrespects)
  recalculateUserBalance(toUserId);
  
  req.session.flash = { type: 'success', message: `You gave disrespect to ${toUser.name}! 🔴` };
  res.redirect('/');
});

module.exports = router;
