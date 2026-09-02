namespace Argo.DTO;

/// <summary>
/// Represents an intake submission envelope ingested from the upstream intake system.
/// </summary>
public class IntakeSubmissionDTO
{
    /// <summary>
    /// Gets the artifact type emitted by the upstream system.
    /// </summary>
    public string ArtifactType { get; init; } = string.Empty;

    /// <summary>
    /// Gets the schema version of the payload format.
    /// </summary>
    public int SchemaVersion { get; init; }

    /// <summary>
    /// Gets the submission processing status supplied by the source system.
    /// </summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>
    /// Gets the upstream request identifier.
    /// </summary>
    public string RequestId { get; init; } = string.Empty;

    /// <summary>
    /// Gets the timestamp when the source system last updated the submission.
    /// </summary>
    public DateTimeOffset UpdatedAt { get; init; }

    /// <summary>
    /// Gets the detailed request payload.
    /// </summary>
    public IntakeRequestDTO Request { get; init; } = new();

    /// <summary>
    /// Gets the timestamp when the request was originally submitted.
    /// </summary>
    public DateTimeOffset SubmittedAt { get; init; }
}

/// <summary>
/// Represents detailed business and technical fields captured for an intake request.
/// </summary>
public class IntakeRequestDTO
{
    /// <summary>
    /// Gets the upstream request identifier.
    /// </summary>
    public string RequestId { get; init; } = string.Empty;

    /// <summary>
    /// Gets the name of the person submitting the request.
    /// </summary>
    public string RequesterName { get; init; } = string.Empty;

    /// <summary>
    /// Gets the requester's department.
    /// </summary>
    public string Department { get; init; } = string.Empty;

    /// <summary>
    /// Gets requester contact information.
    /// </summary>
    public string RequesterContact { get; init; } = string.Empty;

    /// <summary>
    /// Gets the executive or business sponsor.
    /// </summary>
    public string BusinessSponsor { get; init; } = string.Empty;

    /// <summary>
    /// Gets the business owner accountable for the request outcome.
    /// </summary>
    public string BusinessOwner { get; init; } = string.Empty;

    /// <summary>
    /// Gets additional stakeholder information provided by the submitter.
    /// </summary>
    public string AdditionalStakeholders { get; init; } = string.Empty;

    /// <summary>
    /// Gets the request title.
    /// </summary>
    public string RequestTitle { get; init; } = string.Empty;

    /// <summary>
    /// Gets the request type classification.
    /// </summary>
    public string RequestType { get; init; } = string.Empty;

    /// <summary>
    /// Gets any related project reference supplied by the requester.
    /// </summary>
    public string RelatedProject { get; init; } = string.Empty;

    /// <summary>
    /// Gets the business request description.
    /// </summary>
    public string RequestDescription { get; init; } = string.Empty;

    /// <summary>
    /// Gets the business problem statement.
    /// </summary>
    public string BusinessProblem { get; init; } = string.Empty;

    /// <summary>
    /// Gets the desired business outcome.
    /// </summary>
    public string DesiredOutcome { get; init; } = string.Empty;

    /// <summary>
    /// Gets success metrics provided for the request.
    /// </summary>
    public string SuccessMeasures { get; init; } = string.Empty;

    /// <summary>
    /// Gets the current process description.
    /// </summary>
    public string CurrentProcess { get; init; } = string.Empty;

    /// <summary>
    /// Gets affected teams or groups.
    /// </summary>
    public string AffectedGroups { get; init; } = string.Empty;

    /// <summary>
    /// Gets the stated impact scope.
    /// </summary>
    public string ImpactScope { get; init; } = string.Empty;

    /// <summary>
    /// Gets the count or description of impacted users.
    /// </summary>
    public string UsersAffected { get; init; } = string.Empty;

    /// <summary>
    /// Gets client impact details.
    /// </summary>
    public string ClientImpact { get; init; } = string.Empty;

    /// <summary>
    /// Gets named clients impacted by the request.
    /// </summary>
    public string ClientNames { get; init; } = string.Empty;

    /// <summary>
    /// Gets business impact details.
    /// </summary>
    public string BusinessImpact { get; init; } = string.Empty;

    /// <summary>
    /// Gets strategic alignment details.
    /// </summary>
    public string StrategicAlignment { get; init; } = string.Empty;

    /// <summary>
    /// Gets consequences of taking no action.
    /// </summary>
    public string NoActionImpact { get; init; } = string.Empty;

    /// <summary>
    /// Gets the expected benefit categories or statements.
    /// </summary>
    public string[] ExpectedBenefits { get; init; } = [];

    /// <summary>
    /// Gets the desired date value supplied by the submitter.
    /// </summary>
    public string DesiredDate { get; init; } = string.Empty;

    /// <summary>
    /// Gets the desired date classification.
    /// </summary>
    public string DateType { get; init; } = string.Empty;

    /// <summary>
    /// Gets rationale for the requested date.
    /// </summary>
    public string DateReason { get; init; } = string.Empty;

    /// <summary>
    /// Gets scope items explicitly included in delivery.
    /// </summary>
    public string InScope { get; init; } = string.Empty;

    /// <summary>
    /// Gets items explicitly excluded from scope.
    /// </summary>
    public string OutOfScope { get; init; } = string.Empty;

    /// <summary>
    /// Gets dependency information provided by the requester.
    /// </summary>
    public string Dependencies { get; init; } = string.Empty;

    /// <summary>
    /// Gets systems expected to be involved.
    /// </summary>
    public string SystemsInvolved { get; init; } = string.Empty;

    /// <summary>
    /// Gets data sources referenced by the request.
    /// </summary>
    public string DataSources { get; init; } = string.Empty;

    /// <summary>
    /// Gets whether sensitive data handling is indicated.
    /// </summary>
    public string SensitiveData { get; init; } = string.Empty;

    /// <summary>
    /// Gets access requirements needed for execution.
    /// </summary>
    public string AccessNeeded { get; init; } = string.Empty;

    /// <summary>
    /// Gets details about sensitive data context.
    /// </summary>
    public string SensitiveDetails { get; init; } = string.Empty;

    /// <summary>
    /// Gets technical owner information.
    /// </summary>
    public string TechnicalOwners { get; init; } = string.Empty;

    /// <summary>
    /// Gets vendor information provided for the request.
    /// </summary>
    public string Vendors { get; init; } = string.Empty;

    /// <summary>
    /// Gets references to supporting materials.
    /// </summary>
    public string SupportingMaterials { get; init; } = string.Empty;

    /// <summary>
    /// Gets attestation values captured during submission.
    /// </summary>
    public string[] Attestation { get; init; } = [];
}

