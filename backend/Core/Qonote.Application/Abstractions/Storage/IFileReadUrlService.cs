using System;

namespace Qonote.Core.Application.Abstractions.Storage;

/// <summary>
/// Generates time-limited read URLs for stored files (e.g., Azure Blob SAS URLs).
/// Callers pass the original stable blob URL stored in DB; implementation returns a
/// short-lived URL suitable to send to clients.
/// </summary>
public interface IFileReadUrlService
{
    /// <summary>
    /// Generate a read URL valid for the given time span. Implementations should avoid exposing secrets
    /// and prefer provider-native signed URLs when possible.
    /// </summary>
    /// <param name="originalUrl">Original blob URL stored in DB (without SAS).</param>
    /// <param name="ttl">Desired time-to-live.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Signed, time-limited URL, or the original URL if signing is not possible.</returns>
    Task<string> GetReadUrlAsync(string originalUrl, TimeSpan ttl, CancellationToken ct);
}
