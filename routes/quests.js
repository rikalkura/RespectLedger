const express = require('express');
const router = express.Router();
const { requireAuth } = require('../middleware/auth');
const { dbGet, dbAll, dbRun } = require('../config/database');
const { getPendingCompletionsCount, getUnreadNotificationsCount } = require('../utils/stats');

// GET /quests
router.get('/quests', requireAuth, (req, res) => {
  // Refresh user data
  req.session.user = dbGet('SELECT * FROM Users WHERE id = ?', [req.session.user.id]);
  
  const userId = req.session.user.id;
  
  // Get all active quests
  const quests = dbAll('SELECT * FROM Quests WHERE is_active = 1 ORDER BY reward DESC');
  
  // Get user's quest completion statuses
  const userCompletions = dbAll(`
    SELECT quest_id, status FROM QuestCompletions 
    WHERE user_id = ?
  `, [userId]);
  
  // Create a map for quick lookup
  const completionMap = {};
  userCompletions.forEach(c => {
    completionMap[c.quest_id] = c.status;
  });
  
  // Add completion status to each quest
  const questsWithStatus = quests.map(quest => ({
    ...quest,
    userStatus: completionMap[quest.id] || null // null = not submitted, 'PENDING', 'APPROVED', 'REJECTED'
  }));
  
  const pendingCount = req.session.user.is_admin ? getPendingCompletionsCount() : 0;
  const notificationCount = req.session.user.is_admin ? getUnreadNotificationsCount(req.session.user.id) : 0;
  
  res.render('quests', { 
    quests: questsWithStatus, 
    currentUser: req.session.user,
    pendingCount,
    notificationCount
  });
});

// POST /quests/submit/:questId
router.post('/quests/submit/:questId', requireAuth, (req, res) => {
  const questId = parseInt(req.params.questId);
  const userId = req.session.user.id;
  
  const quest = dbGet('SELECT * FROM Quests WHERE id = ? AND is_active = 1', [questId]);
  if (!quest) {
    req.session.flash = { type: 'error', message: 'Quest not found or not active!' };
    return res.redirect('/quests');
  }
  
  // Check if user already has a pending or approved completion for this quest
  const existingCompletion = dbGet(`
    SELECT * FROM QuestCompletions 
    WHERE quest_id = ? AND user_id = ? AND status IN ('PENDING', 'APPROVED')
  `, [questId, userId]);
  
  if (existingCompletion) {
    if (existingCompletion.status === 'PENDING') {
      req.session.flash = { type: 'error', message: 'You already submitted this quest for approval!' };
    } else {
      req.session.flash = { type: 'error', message: 'You already completed this quest!' };
    }
    return res.redirect('/quests');
  }
  
  // Check if there's a rejected completion - allow resubmission
  const rejectedCompletion = dbGet(`
    SELECT * FROM QuestCompletions 
    WHERE quest_id = ? AND user_id = ? AND status = 'REJECTED'
  `, [questId, userId]);
  
  if (rejectedCompletion) {
    // Update existing rejected to pending
    dbRun(`
      UPDATE QuestCompletions 
      SET status = 'PENDING', submitted_at = CURRENT_TIMESTAMP, reviewed_at = NULL, reviewed_by = NULL
      WHERE id = ?
    `, [rejectedCompletion.id]);
  } else {
    // Create new completion request
    dbRun(`
      INSERT INTO QuestCompletions (quest_id, user_id, status)
      VALUES (?, ?, 'PENDING')
    `, [questId, userId]);
  }
  
  req.session.flash = { type: 'success', message: `Quest "${quest.title}" submitted for approval! ⏳` };
  res.redirect('/quests');
});

module.exports = router;
