namespace Argo.DTO;

/// <summary>
/// Represents the mapping result between an intake request and a created project.
/// </summary>
/// <param name="RequestId">The originating intake request identifier.</param>
/// <param name="ProjectId">The identifier of the project created for the request.</param>
public record IntakeSubmissionResultDTO(string RequestId, string ProjectId);
