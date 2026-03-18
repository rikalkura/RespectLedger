const RESPECT_TIERS = [
  { min: 20, emoji: '💥', repeat: 7, title: 'LEGENDARY RESPECT' },
  { min: 10, emoji: '🚀', repeat: 5, title: 'Major Respect!' },
  { min: 4,  emoji: '⚡', repeat: 3, title: 'Respect!' },
  { min: 1,  emoji: '✅', repeat: 1, title: 'Respect' },
];

const DISRESPECT_TIERS = [
  { min: 20, emoji: '☠️', repeat: 7, title: 'BRUTAL DISRESPECT' },
  { min: 10, emoji: '💀', repeat: 5, title: 'Major Disrespect!' },
  { min: 4,  emoji: '🔥', repeat: 3, title: 'Disrespect!' },
  { min: 1,  emoji: '❌', repeat: 1, title: 'Disrespect' },
];

function getTier(amount, tiers) {
  return tiers.find(t => amount >= t.min) || tiers[tiers.length - 1];
}

function buildRespectMessage({ amount, recipient, giver, reason, balance }) {
  const tier = getTier(amount, RESPECT_TIERS);
  const cluster = tier.emoji.repeat(tier.repeat);
  const reasonText = reason || 'No reason given';
  const unit = amount === 1 ? 'respect' : 'respects';

  return [
    cluster,
    `<i>${tier.title}</i>`,
    '',
    `<b>${recipient}</b>  +${amount} ${unit}`,
    `<i>from ${giver}</i>`,
    '',
    `<blockquote>${reasonText}</blockquote>`,
    '',
    `🏅 Balance: ${balance} 🥚`,
  ].join('\n');
}

function buildDisrespectMessage({ amount, recipient, giver, reason, balance }) {
  const tier = getTier(amount, DISRESPECT_TIERS);
  const cluster = tier.emoji.repeat(tier.repeat);
  const reasonText = reason || 'No reason given';
  const unit = amount === 1 ? 'disrespect' : 'disrespects';

  return [
    cluster,
    `<i>${tier.title}</i>`,
    '',
    `<b>${recipient}</b>  -${amount} ${unit}`,
    `<i>from ${giver}</i>`,
    '',
    `<blockquote>${reasonText}</blockquote>`,
    '',
    `📉 Balance: ${balance} 🥚`,
  ].join('\n');
}

async function sendTelegramMessage(text) {
  const token = process.env.TELEGRAM_BOT_TOKEN;
  const chatId = process.env.TELEGRAM_CHAT_ID;
  if (!token || !chatId) return;

  await fetch(`https://api.telegram.org/bot${token}/sendMessage`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ chat_id: chatId, text, parse_mode: 'HTML' })
  });
}

module.exports = { sendTelegramMessage, buildRespectMessage, buildDisrespectMessage };
