namespace Argo.DTO;

public class IntakeSubmissionDTO
{
    public string ArtifactType { get; init; } = string.Empty;
    public int SchemaVersion { get; init; }
    public string Status { get; init; } = string.Empty;
    public string RequestId { get; init; } = string.Empty;
    public DateTimeOffset UpdatedAt { get; init; }
    public IntakeRequestDTO Request { get; init; } = new();
    public DateTimeOffset SubmittedAt { get; init; }
}

public class IntakeRequestDTO
{
    public string RequestId { get; init; } = string.Empty;
    public string RequesterName { get; init; } = string.Empty;
    public string Department { get; init; } = string.Empty;
    public string RequesterContact { get; init; } = string.Empty;
    public string BusinessSponsor { get; init; } = string.Empty;
    public string BusinessOwner { get; init; } = string.Empty;
    public string AdditionalStakeholders { get; init; } = string.Empty;
    public string RequestTitle { get; init; } = string.Empty;
    public string RequestType { get; init; } = string.Empty;
    public string RelatedProject { get; init; } = string.Empty;
    public string RequestDescription { get; init; } = string.Empty;
    public string BusinessProblem { get; init; } = string.Empty;
    public string DesiredOutcome { get; init; } = string.Empty;
    public string SuccessMeasures { get; init; } = string.Empty;
    public string CurrentProcess { get; init; } = string.Empty;
    public string AffectedGroups { get; init; } = string.Empty;
    public string ImpactScope { get; init; } = string.Empty;
    public string UsersAffected { get; init; } = string.Empty;
    public string ClientImpact { get; init; } = string.Empty;
    public string ClientNames { get; init; } = string.Empty;
    public string BusinessImpact { get; init; } = string.Empty;
    public string StrategicAlignment { get; init; } = string.Empty;
    public string NoActionImpact { get; init; } = string.Empty;
    public string[] ExpectedBenefits { get; init; } = [];
    public string DesiredDate { get; init; } = string.Empty;
    public string DateType { get; init; } = string.Empty;
    public string DateReason { get; init; } = string.Empty;
    public string InScope { get; init; } = string.Empty;
    public string OutOfScope { get; init; } = string.Empty;
    public string Dependencies { get; init; } = string.Empty;
    public string SystemsInvolved { get; init; } = string.Empty;
    public string DataSources { get; init; } = string.Empty;
    public string SensitiveData { get; init; } = string.Empty;
    public string AccessNeeded { get; init; } = string.Empty;
    public string SensitiveDetails { get; init; } = string.Empty;
    public string TechnicalOwners { get; init; } = string.Empty;
    public string Vendors { get; init; } = string.Empty;
    public string SupportingMaterials { get; init; } = string.Empty;
    public string[] Attestation { get; init; } = [];
}

