namespace Argo.DTO;

/// <summary>
/// Represents the result returned by the intake ingestion endpoint.
/// </summary>
/// <param name="Count">The number of submissions successfully processed.</param>
/// <param name="FirstProjectId">The identifier of the first created project in the current ingestion batch.</param>
public record IngestDTO(int Count, string FirstProjectId);
