namespace RespectLedger.Application.Common.Interfaces;

public interface IImageStorageService
{
    Task<string> UploadImageAsync(Stream imageStream, string fileName, CancellationToken cancellationToken = default);
    Task<bool> DeleteImageAsync(string imageUrl, CancellationToken cancellationToken = default);
}
