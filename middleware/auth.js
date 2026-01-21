const requireAuth = (req, res, next) => {
  if (!req.session.user) {
    return res.redirect('/login');
  }
  next();
};

const requireAdmin = (req, res, next) => {
  if (!req.session.user || !req.session.user.is_admin) {
    req.session.flash = { type: 'error', message: 'Admin access required!' };
    return res.redirect('/');
  }
  next();
};

module.exports = {
  requireAuth,
  requireAdmin
};
