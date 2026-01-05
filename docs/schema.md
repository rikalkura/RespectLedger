// === PROJECT: RESPECT LEDGER ===
// Database Schema for MySQL

Table users {
  id GUID [primary key, increment]
  nickname varchar(50) [unique, not null, note: 'Unique display name']
  email varchar(255) [unique, not null]
  password_hash varchar(255) [not null]
  avatar_url varchar(500) [note: 'Stored in Cloudinary']
  
  // Status & Role
  status varchar(20) [default: 'Pending', note: 'Pending, Active, Banned']
  role varchar(20) [default: 'User', note: 'User, Admin']
  
  // Economy & Limits
  current_mana integer [default: 3, note: 'Resets daily at 00:00']
  last_mana_reset timestamp
  
  // RPG Stats (Long-term)
  total_xp integer [default: 0, note: 'Lifetime score']
  current_level integer [default: 1]
  current_class varchar(50) [default: 'Novice', note: 'Calculated dynamically based on tags']
  
  // Meta
  created_at timestamp [default: `now()`]
  updated_at timestamp
}

Table respects {
  id integer [primary key, increment]
  sender_id integer [not null]
  receiver_id integer [not null]
  season_id integer [not null, note: 'Links transaction to a specific season']
  
  // Data Payload
  amount integer [default: 1, note: 'Fixed value usually']
  reason varchar(280) [not null, note: 'Comment context']
  tag varchar(50) [note: '#help, #fun, #work - used for Class calculation']
  image_url varchar(500) [note: 'Proof photo from Cloudinary']
  
  created_at timestamp [default: `now()`]

  indexes {
    (sender_id, receiver_id) [note: 'For spam/cooldown checks']
  }
}

Table seasons {
  id integer [primary key, increment]
  name varchar(100) [note: 'e.g. January 2026']
  start_date datetime [not null]
  end_date datetime [not null]
  is_active boolean [default: true]
  
  created_at timestamp [default: `now()`]
}

Table season_results {
  id integer [primary key, increment]
  season_id integer [not null]
  user_id integer [not null]
  
  // Snapshot Data
  rank_position integer [note: '1, 2, 3...']
  total_score integer [note: 'Score achieved in this season']
  reward_summary varchar(255) [note: 'What did they win?']
  
  recorded_at timestamp [default: `now()`]
}

Table achievements {
  id integer [primary key, increment]
  title varchar(100) [not null]
  description text
  icon_url varchar(500)
  
  // Unlock Logic (for automatic background jobs)
  criteria_type varchar(50) [note: 'e.g. TotalRespects, SpecificTag, Streak']
  criteria_value integer [note: 'Threshold value']
}

Table user_achievements {
  id integer [primary key, increment]
  user_id integer [not null]
  achievement_id integer [not null]
  unlocked_at timestamp [default: `now()`]
  
  indexes {
    (user_id, achievement_id) [unique]
  }
}

// === RELATIONSHIPS ===

// User interactions
Ref: respects.sender_id > users.id [delete: cascade]
Ref: respects.receiver_id > users.id [delete: cascade]

// Season logic
Ref: respects.season_id > seasons.id
Ref: season_results.user_id > users.id
Ref: season_results.season_id > seasons.id

// Gamification
Ref: user_achievements.user_id > users.id [delete: cascade]
Ref: user_achievements.achievement_id > achievements.id