const cloudinary = require('../config/cloudinary');

const uploadToCloudinary = (buffer, folder = 'respect-ledger') => {
  return new Promise((resolve, reject) => {
    const uploadStream = cloudinary.uploader.upload_stream(
      { folder: folder },
      (error, result) => {
        if (error) reject(error);
        else resolve(result);
      }
    );
    uploadStream.end(buffer);
  });
};

module.exports = {
  uploadToCloudinary
};
