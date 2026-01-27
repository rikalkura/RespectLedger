const express = require('express');
const router = express.Router();
const { requireAuth } = require('../middleware/auth');
const { dbGet, dbRun } = require('../config/database');
const { recalculateUserBalance } = require('../utils/stats');

// POST /respect/:userId
router.post('/respect/:userId', requireAuth, async (req, res) => {
  // Only admins can give respects/disrespects
  if (!req.session.user.is_admin) {
    req.session.flash = { type: 'error', message: 'Only admins can give respects!' };
    return res.redirect('/');
  }
  
  const toUserId = parseInt(req.params.userId);
  const fromUserId = req.session.user.id;
  const { description, amount } = req.body;
  
  // Parse amount, default to 1 if not provided or invalid
  const respectAmount = parseInt(amount) || 1;
  
  // Validate amount (must be positive)
  if (respectAmount <= 0) {
    req.session.flash = { type: 'error', message: 'Amount must be greater than 0!' };
    return res.redirect('/');
  }
  
  if (toUserId === fromUserId) {
    req.session.flash = { type: 'error', message: "You can't respect yourself!" };
    return res.redirect('/');
  }
  
  const toUser = await dbGet('SELECT * FROM Users WHERE id = $1', [toUserId]);
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
  await dbRun(`
    INSERT INTO Transactions (from_user_id, to_user_id, type, amount, description)
    VALUES ($1, $2, 'RESPECT', $3, $4)
  `, [fromUserId, toUserId, respectAmount, description || 'Gave respect']);
  
  // Recalculate balance (respects - disrespects)
  await recalculateUserBalance(toUserId);
  
  const amountText = respectAmount === 1 ? 'respect' : `${respectAmount} respects`;
  req.session.flash = { type: 'success', message: `You gave ${amountText} to ${toUser.name}! 👍` };
  res.redirect('/');
});

// POST /disrespect/:userId
router.post('/disrespect/:userId', requireAuth, async (req, res) => {
  // Only admins can give respects/disrespects
  if (!req.session.user.is_admin) {
    req.session.flash = { type: 'error', message: 'Only admins can give disrespects!' };
    return res.redirect('/');
  }
  
  const toUserId = parseInt(req.params.userId);
  const fromUserId = req.session.user.id;
  const { description, amount } = req.body;
  
  // Parse amount, default to 1 if not provided or invalid
  const disrespectAmount = parseInt(amount) || 1;
  
  // Validate amount (must be positive)
  if (disrespectAmount <= 0) {
    req.session.flash = { type: 'error', message: 'Amount must be greater than 0!' };
    return res.redirect('/');
  }
  
  if (toUserId === fromUserId) {
    req.session.flash = { type: 'error', message: "You can't disrespect yourself!" };
    return res.redirect('/');
  }
  
  const toUser = await dbGet('SELECT * FROM Users WHERE id = $1', [toUserId]);
  if (!toUser) {
    req.session.flash = { type: 'error', message: 'User not found!' };
    return res.redirect('/');
  }
  
  // Can't give disrespects to admins
  if (toUser.is_admin) {
    req.session.flash = { type: 'error', message: "You can't give disrespects to admins!" };
    return res.redirect('/');
  }
  
  // Create transaction (negative amount for disrespect)
  await dbRun(`
    INSERT INTO Transactions (from_user_id, to_user_id, type, amount, description)
    VALUES ($1, $2, 'DISRESPECT', $3, $4)
  `, [fromUserId, toUserId, -disrespectAmount, description || 'Gave disrespect']);
  
  // Recalculate balance (respects - disrespects)
  await recalculateUserBalance(toUserId);
  
  const amountText = disrespectAmount === 1 ? 'disrespect' : `${disrespectAmount} disrespects`;
  req.session.flash = { type: 'success', message: `You gave ${amountText} to ${toUser.name}! 👎` };
  res.redirect('/');
});

module.exports = router;
