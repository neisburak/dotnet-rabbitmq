using Api.Models;

namespace Api.Extensions;

public static class FormFileExtensions
{
    public static async Task<FileUploadResult> UploadAsync(this IFormFile formFile, CancellationToken cancellationToken)
    {
        try
        {
            if (formFile is { Length: > 0 })
            {
                var imageName = $"{Guid.NewGuid()}{Path.GetExtension(formFile.FileName)}";
                var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", imageName);

                await using var stream = new FileStream(path, FileMode.Create);
                await formFile.CopyToAsync(stream, cancellationToken);

                return new() { Status = true, Name = imageName };
            }
            else return new() { Status = false };
        }
        catch
        {
            return new() { Status = false };
        }
    }
}