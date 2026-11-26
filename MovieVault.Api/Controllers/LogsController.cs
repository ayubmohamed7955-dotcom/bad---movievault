using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using MovieVault.Api.DTOs;

namespace MovieVault.Api.Controllers;

[ApiController]
[Route("api/logs")]
[Authorize(Roles = "Admin")]
[Produces("application/json")]
public class LogsController : ControllerBase
{
    private readonly IMongoCollection<BsonDocument> _logs;

    public LogsController(IMongoCollection<BsonDocument> logs)
    {
        _logs = logs;
    }

    /// <summary>
    /// Henter request-logs fra MongoDB (kun Admin), nyeste først.
    /// Filtre: ?username=admin.demo, date=2026-11-24, requestType=POST og route=/api/ticket-sales.
    /// Filtrene kan kombineres, og tomme eller udeladte filtre ignoreres.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<RequestLogDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult<IEnumerable<RequestLogDto>>> GetLogs([FromQuery] RequestLogQuery query)
    {
        if (query.Limit is < 1 or > 1000)
        {
            ModelState.AddModelError(nameof(query.Limit), "Limit must be between 1 and 1000.");
            return ValidationProblem(ModelState);
        }

        var f = Builders<BsonDocument>.Filter;
        var filters = new List<FilterDefinition<BsonDocument>>();

        // Hvert filter tilføjes kun, hvis det er udfyldt -> tomme filtre ignoreres.
        // Alle filtre kombineres med AND.
        if (!string.IsNullOrWhiteSpace(query.Username))
            filters.Add(OnProperty("username", field => f.Regex(field, ExactIgnoreCase(query.Username))));

        if (!string.IsNullOrWhiteSpace(query.RequestType))
            filters.Add(OnProperty("requestType", field => f.Regex(field, ExactIgnoreCase(query.RequestType))));

        if (!string.IsNullOrWhiteSpace(query.Route))
            filters.Add(OnProperty("route", field => f.Regex(field, ContainsIgnoreCase(query.Route))));

        if (query.Date is { } date)
        {
            // timestamp er gemt som ISO 8601-tekst i UTC, så "hele dagen" er et tekst-interval:
            // "2026-11-24" <= timestamp < "2026-11-25"
            var from = date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            var to = date.AddDays(1).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            filters.Add(OnProperty("timestamp", field => f.Gte(field, from) & f.Lt(field, to)));
        }

        var filter = filters.Count > 0 ? f.And(filters) : f.Empty;

        try
        {
            var documents = await _logs
                .Find(filter)
                .Sort(Builders<BsonDocument>.Sort.Descending("_id")) // ObjectId er tidsordnet -> nyeste først
                .Limit(query.Limit)
                .ToListAsync();

            return Ok(documents.Select(ToDto));
        }
        catch (Exception ex) when (ex is MongoException or TimeoutException)
        {
            return Problem(statusCode: StatusCodes.Status503ServiceUnavailable,
                title: "Log storage unavailable", detail: "Could not read request logs from MongoDB.");
        }
    }

    // ---------- Hjælpemetoder ----------

    // Serilogs MongoDB-sink gemmer vores værdier under "Properties". For en sikkerheds skyld
    // matcher vi både "Properties.<navn>" og "<navn>" i roden af dokumentet.
    private static FilterDefinition<BsonDocument> OnProperty(
        string name, Func<string, FilterDefinition<BsonDocument>> build) =>
        Builders<BsonDocument>.Filter.Or(build($"Properties.{name}"), build(name));

    private static BsonRegularExpression ExactIgnoreCase(string value) =>
        new($"^{Regex.Escape(value.Trim())}$", "i");

    private static BsonRegularExpression ContainsIgnoreCase(string value) =>
        new(Regex.Escape(value.Trim()), "i");

    private static RequestLogDto ToDto(BsonDocument doc)
    {
        var props = doc.TryGetValue("Properties", out var p) && p.IsBsonDocument ? p.AsBsonDocument : doc;

        return new RequestLogDto
        {
            Id = doc.TryGetValue("_id", out var id) ? id.ToString()! : string.Empty,
            Username = GetString(props, "username"),
            Timestamp = GetTimestamp(props),
            RequestType = GetString(props, "requestType"),
            Route = GetString(props, "route"),
            StatusCode = props.TryGetValue("statusCode", out var s) && s.IsNumeric ? s.ToInt32() : null
        };
    }

    private static string? GetString(BsonDocument doc, string name) =>
        doc.TryGetValue(name, out var value) && !value.IsBsonNull ? value.ToString() : null;

    private static DateTime? GetTimestamp(BsonDocument doc)
    {
        if (!doc.TryGetValue("timestamp", out var value) || value.IsBsonNull)
            return null;

        if (value.IsValidDateTime)
            return value.ToUniversalTime();

        return DateTime.TryParse(value.ToString(), CultureInfo.InvariantCulture,
            DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal, out var parsed)
            ? parsed
            : null;
    }
}
