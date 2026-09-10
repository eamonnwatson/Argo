namespace Argo.Domain.Enums;

public static class EnumStringConverters
{
    public static string ToApiString(this ProjectStatus status) => status switch
    {
        ProjectStatus.Waiting => "Waiting",
        ProjectStatus.InProgress => "In Progress",
        ProjectStatus.Done => "Done",
        _ => throw new ArgumentOutOfRangeException(nameof(status))
    };

    public static ProjectStatus ToProjectStatus(this string value) => value switch
    {
        "Waiting" => ProjectStatus.Waiting,
        "In Progress" => ProjectStatus.InProgress,
        "Done" => ProjectStatus.Done,
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, "Unknown project status.")
    };

    public static string ToApiString(this ProjectHealth health) => health switch
    {
        ProjectHealth.NotAssessed => "Not Assessed",
        ProjectHealth.OnTrack => "On Track",
        ProjectHealth.AtRisk => "At Risk",
        ProjectHealth.Blocked => "Blocked",
        ProjectHealth.Complete => "Complete",
        _ => throw new ArgumentOutOfRangeException(nameof(health))
    };

    public static ProjectHealth ToProjectHealth(this string value) => value switch
    {
        "Not Assessed" => ProjectHealth.NotAssessed,
        "On Track" => ProjectHealth.OnTrack,
        "At Risk" => ProjectHealth.AtRisk,
        "Blocked" => ProjectHealth.Blocked,
        "Complete" => ProjectHealth.Complete,
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, "Unknown project health.")
    };

    public static string ToApiString(this ProjectPriority priority) => priority switch
    {
        ProjectPriority.NeedsTriage => "Needs Triage",
        ProjectPriority.Low => "Low",
        ProjectPriority.Medium => "Medium",
        ProjectPriority.High => "High",
        ProjectPriority.Critical => "Critical",
        _ => throw new ArgumentOutOfRangeException(nameof(priority))
    };

    public static ProjectPriority ToProjectPriority(this string value) => value switch
    {
        "Needs Triage" => ProjectPriority.NeedsTriage,
        "Low" => ProjectPriority.Low,
        "Medium" => ProjectPriority.Medium,
        "High" => ProjectPriority.High,
        "Critical" => ProjectPriority.Critical,
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, "Unknown project priority.")
    };

    public static string ToApiString(this WorkItemStatus status) => status switch
    {
        WorkItemStatus.Waiting => "Waiting",
        WorkItemStatus.Blocked => "Blocked",
        WorkItemStatus.NotStarted => "Not Started",
        WorkItemStatus.InProgress => "In Progress",
        WorkItemStatus.Done => "Done",
        _ => throw new ArgumentOutOfRangeException(nameof(status))
    };

    public static WorkItemStatus ToWorkItemStatus(this string value) => value switch
    {
        "Waiting" => WorkItemStatus.Waiting,
        "Blocked" => WorkItemStatus.Blocked,
        "Not Started" => WorkItemStatus.NotStarted,
        "In Progress" => WorkItemStatus.InProgress,
        "Done" => WorkItemStatus.Done,
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, "Unknown work item status.")
    };

    public static string ToApiString(this ActivityStatus status) => status switch
    {
        ActivityStatus.NotStarted => "Not Started",
        ActivityStatus.InProgress => "In Progress",
        ActivityStatus.Done => "Done",
        _ => throw new ArgumentOutOfRangeException(nameof(status))
    };

    public static ActivityStatus ToActivityStatus(this string value) => value switch
    {
        "Not Started" => ActivityStatus.NotStarted,
        "In Progress" => ActivityStatus.InProgress,
        "Done" => ActivityStatus.Done,
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, "Unknown activity status.")
    };

    public static string ToApiString(this RaidItemType type) => type switch
    {
        RaidItemType.Risk => "Risk",
        RaidItemType.Assumption => "Assumption",
        RaidItemType.Issue => "Issue",
        RaidItemType.Dependency => "Dependency",
        _ => throw new ArgumentOutOfRangeException(nameof(type))
    };

    public static RaidItemType ToRaidItemType(this string value) => value switch
    {
        "Risk" => RaidItemType.Risk,
        "Assumption" => RaidItemType.Assumption,
        "Issue" => RaidItemType.Issue,
        "Dependency" => RaidItemType.Dependency,
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, "Unknown RAID item type.")
    };
}
