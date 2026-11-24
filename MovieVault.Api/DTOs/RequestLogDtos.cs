namespace MovieVault.Api.DTOs;

// Filtre til GET /api/logs. Alle er valgfrie, kan kombineres, og tomme filtre ignoreres.
public class RequestLogQuery
{
    /// <summary>Præcist brugernavn (ikke case-sensitivt), fx "admin.demo" eller "anonymous".</summary>
    public string? Username { get; set; }

    /// <summary>Dato i UTC (yyyy-MM-dd). Returnerer logs fra hele den dag.</summary>
    public DateOnly? Date { get; set; }

    /// <summary>HTTP-metode, fx POST, PUT eller DELETE (ikke case-sensitivt).</summary>
    public string? RequestType { get; set; }

    /// <summary>Del af stien, fx "/api/ticket-sales" eller "movies" (ikke case-sensitivt).</summary>
    public string? Route { get; set; }

    /// <summary>Maks antal logs (1-1000). Standard 100. Nyeste først.</summary>
    public int Limit { get; set; } = 100;
}

// Én request-log fra MongoDB
public class RequestLogDto
{
    public string Id { get; set; } = string.Empty;
    public string? Username { get; set; }
    public DateTime? Timestamp { get; set; }
    public string? RequestType { get; set; }
    public string? Route { get; set; }
    public int? StatusCode { get; set; }
}
