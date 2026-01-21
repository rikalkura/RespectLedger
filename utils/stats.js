const { dbGet, dbAll } = require('../config/database');

const getUserStats = async (userId) => {
  // SUM amounts from RESPECT + QUEST_REWARD transactions (not count!)
  // RESPECT transactions have amount=1, QUEST_REWARD can have amount=10, 20, etc.
  const respectResult = await dbGet(`
    SELECT COALESCE(SUM(amount), 0) as total_respects
    FROM Transactions 
    WHERE to_user_id = $1 AND (type = 'RESPECT' OR type = 'QUEST_REWARD')
  `, [userId]);
  
  // SUM amounts from DISRESPECT transactions (they're negative, so we need ABS)
  const disrespectResult = await dbGet(`
    SELECT COALESCE(SUM(ABS(amount)), 0) as total_disrespects
    FROM Transactions 
    WHERE to_user_id = $1 AND type = 'DISRESPECT'
  `, [userId]);
  
  const respects = respectResult ? (parseInt(respectResult.total_respects) || 0) : 0;
  const disrespects = disrespectResult ? (parseInt(disrespectResult.total_disrespects) || 0) : 0;
  
  return {
    respects,
    disrespects
  };
};

const getLeaderboard = async () => {
  // Only get non-admin users
  const users = await dbAll('SELECT * FROM Users WHERE is_admin = 0 ORDER BY balance DESC');
  
  const leaderboard = [];
  for (const user of users) {
    leaderboard.push({
      ...user,
      stats: await getUserStats(user.id)
    });
  }
  
  return leaderboard;
};

const getRecentTransactions = async (limit = 20) => {
  return await dbAll(`
    SELECT 
      t.*,
      fu.name as from_user_name,
      fu.avatar_emoji as from_user_emoji,
      tu.name as to_user_name,
      tu.avatar_emoji as to_user_emoji
    FROM Transactions t
    LEFT JOIN Users fu ON t.from_user_id = fu.id
    LEFT JOIN Users tu ON t.to_user_id = tu.id
    ORDER BY t.timestamp DESC
    LIMIT $1
  `, [limit]);
};

const getPendingCompletionsCount = async () => {
  const result = await dbGet('SELECT COUNT(*) as count FROM QuestCompletions WHERE status = $1', ['PENDING']);
  return result ? parseInt(result.count) : 0;
};

const getPendingPurchasesCount = async () => {
  const result = await dbGet('SELECT COUNT(*) as count FROM ShopPurchases WHERE status = $1', ['PENDING']);
  return result ? parseInt(result.count) : 0;
};

// Get unread notifications count for a user
const getUnreadNotificationsCount = async (userId) => {
  const result = await dbGet('SELECT COUNT(*) as count FROM Notifications WHERE user_id = $1 AND is_read = 0', [userId]);
  return result ? parseInt(result.count) : 0;
};

// Recalculate user balance based on respects - disrespects - shop purchases
const recalculateUserBalance = async (userId) => {
  // Skip admins - they don't have balance
  const user = await dbGet('SELECT is_admin FROM Users WHERE id = $1', [userId]);
  if (user && user.is_admin) {
    return 0;
  }
  
  const stats = await getUserStats(userId);
  
  // Get total spent on shop purchases (amounts are negative, so we sum them)
  const shopSpentResult = await dbGet(`
    SELECT COALESCE(SUM(ABS(amount)), 0) as total_spent 
    FROM Transactions 
    WHERE from_user_id = $1 AND type = 'SHOP_PURCHASE'
  `, [userId]);
  
  const shopSpent = shopSpentResult ? (parseInt(shopSpentResult.total_spent) || 0) : 0;
  
  // Balance = (respects - disrespects) - shop purchases
  const balance = stats.respects - stats.disrespects - shopSpent;
  
  const { dbRun } = require('../config/database');
  await dbRun('UPDATE Users SET balance = $1 WHERE id = $2', [balance, userId]);
  return balance;
};

// Recalculate all user balances (useful for migration/fixing existing data)
const recalculateAllBalances = async () => {
  const { dbAll } = require('../config/database');
  // Only recalculate balances for non-admin users
  const users = await dbAll('SELECT id FROM Users WHERE is_admin = 0');
  for (const user of users) {
    await recalculateUserBalance(user.id);
  }
};

module.exports = {
  getUserStats,
  getLeaderboard,
  getRecentTransactions,
  getPendingCompletionsCount,
  getPendingPurchasesCount,
  getUnreadNotificationsCount,
  recalculateUserBalance,
  recalculateAllBalances
};
