namespace MovieVault.Api.Entities;

// Faste værdier fra diagrammet bliver til enums

public enum MovieStatus
{
    Planned,
    InProduction,
    Released,
    Archived
}

public enum RoleType
{
    Lead,
    Supporting,
    Cameo
}

public enum LocationType
{
    Street,
    Building,
    Landscape,
    Set
}

public enum ScreeningStatus
{
    Scheduled,
    Cancelled
}