const express = require('express');
const router = express.Router();
const { dbAll, dbGet } = require('../config/database');

// GET /login
router.get('/login', async (req, res) => {
  if (req.session.user) {
    return res.redirect('/');
  }
  const users = await dbAll('SELECT id, name, avatar_emoji FROM Users');
  res.render('login', { users });
});

// POST /login
router.post('/login', async (req, res) => {
  const { user_id, pin_code } = req.body;
  
  // Debug: Check what user exists with this ID
  const userById = await dbGet('SELECT * FROM Users WHERE id = $1', [user_id]);
  
  if (!userById) {
    req.session.flash = { type: 'error', message: 'User not found!' };
    return res.redirect('/login');
  }
  
  // Debug: Log the comparison
  console.log('Login attempt:', {
    userId: user_id,
    enteredPin: pin_code,
    storedPin: userById.pin_code,
    pinsMatch: pin_code === userById.pin_code
  });
  
  const user = await dbGet('SELECT * FROM Users WHERE id = $1 AND pin_code = $2', [user_id, pin_code]);
  
  if (user) {
    req.session.user = user;
    req.session.flash = { type: 'success', message: `Welcome back, ${user.name}!` };
    return res.redirect('/');
  }
  
  req.session.flash = { type: 'error', message: 'Invalid PIN code!' };
  res.redirect('/login');
});

// GET /logout
router.get('/logout', (req, res) => {
  req.session.destroy();
  res.redirect('/login');
});

module.exports = router;
