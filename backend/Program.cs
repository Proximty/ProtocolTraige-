using Microsoft.AspNetCore.Mvc;
using MySqlConnector;

var builder = WebApplication.CreateBuilder(args);

// CORS inschakelen zodat de React frontend (localhost:5173) data kan ophalen
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

app.UseCors("AllowReactApp");

string dbConnectie = "Server=localhost;Database=protocol_triage;User ID=root;Password=admin;";

// ----------------------------------------------------------------------------
// HEALTH CHECK: Snelle statuscontrole voor Unity en Web
// ----------------------------------------------------------------------------
app.MapGet("/api/health", () => Results.Ok(new { status = "online", timestamp = DateTime.UtcNow }));

// ----------------------------------------------------------------------------
// ROUTE 0: Medische Scenario's ophalen uit de database voor de VR game
// ----------------------------------------------------------------------------
app.MapGet("/api/scenarios", async ([FromQuery] int? level) => {
    try {
        using var conn = new MySqlConnection(dbConnectie);
        await conn.OpenAsync();

        string query = level.HasValue
            ? "SELECT * FROM Scenario WHERE Level = @level ORDER BY RAND()"
            : "SELECT * FROM Scenario ORDER BY Level ASC, Id ASC";

        using var cmd = new MySqlCommand(query, conn);
        if (level.HasValue) cmd.Parameters.AddWithValue("@level", level.Value);

        using var reader = await cmd.ExecuteReaderAsync();
        var scenarios = new List<ScenarioDto>();

        while (await reader.ReadAsync())
        {
            scenarios.Add(new ScenarioDto {
                Id = Convert.ToInt32(reader["Id"]),
                Level = Convert.ToInt32(reader["Level"]),
                Titel = reader["Titel"].ToString() ?? "",
                Klacht = reader["Klacht"] != DBNull.Value ? reader["Klacht"].ToString() ?? "" : "",
                Airway = Convert.ToInt32(reader["Airway"]),
                Breathing = Convert.ToInt32(reader["Breathing"]),
                Circulation = Convert.ToInt32(reader["Circulation"]),
                Disability = Convert.ToInt32(reader["Disability"]),
                Exposure = Convert.ToInt32(reader["Exposure"]),
                Pijn = reader["Pijn"] != DBNull.Value ? Convert.ToInt32(reader["Pijn"]) : 0,
                Urgentie = Convert.ToInt32(reader["Urgentie"]),
                DisabilityReactie = reader["DisabilityReactie"] != DBNull.Value ? reader["DisabilityReactie"].ToString() ?? "" : "",
                ExposureReactie = reader["ExposureReactie"] != DBNull.Value ? reader["ExposureReactie"].ToString() ?? "" : ""
            });
        }

        return Results.Ok(scenarios);
    }
    catch (Exception ex) {
        return Results.BadRequest(new { succes = false, melding = ex.Message });
    }
});

// ----------------------------------------------------------------------------
// AUTH ROUTE 1: Inloggen met gebruikersnaam & wachtwoord
// ----------------------------------------------------------------------------
app.MapPost("/api/auth/login", async ([FromBody] LoginRequest req) => {
    using var conn = new MySqlConnection(dbConnectie);
    await conn.OpenAsync();

    string query = "SELECT Id, Gebruikersnaam, VolledigeNaam, Rol FROM Gebruiker WHERE Gebruikersnaam = @user AND Wachtwoord = @pass LIMIT 1";
    using var cmd = new MySqlCommand(query, conn);
    cmd.Parameters.AddWithValue("@user", req.Gebruikersnaam.Trim());
    cmd.Parameters.AddWithValue("@pass", req.Wachtwoord.Trim());

    using var reader = await cmd.ExecuteReaderAsync();
    if (await reader.ReadAsync())
    {
        return Results.Ok(new {
            succes = true,
            gebruiker = new {
                id = Convert.ToInt32(reader["Id"]),
                gebruikersnaam = reader["Gebruikersnaam"].ToString(),
                volledigeNaam = reader["VolledigeNaam"].ToString(),
                rol = reader["Rol"].ToString()
            }
        });
    }

    return Results.BadRequest(new { succes = false, melding = "Onjuiste gebruikersnaam of wachtwoord." });
});

// ----------------------------------------------------------------------------
// AUTH ROUTE 2: Nieuw account registreren
// ----------------------------------------------------------------------------
app.MapPost("/api/auth/register", async ([FromBody] RegisterRequest req) => {
    try {
        using var conn = new MySqlConnection(dbConnectie);
        await conn.OpenAsync();

        string query = @"INSERT INTO Gebruiker (Gebruikersnaam, Wachtwoord, VolledigeNaam, Rol, AangemaaktOp)
                         VALUES (@user, @pass, @naam, 'Trainee', NOW());
                         SELECT LAST_INSERT_ID();";
        using var cmd = new MySqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@user", req.Gebruikersnaam.Trim());
        cmd.Parameters.AddWithValue("@pass", req.Wachtwoord.Trim());
        cmd.Parameters.AddWithValue("@naam", req.VolledigeNaam.Trim());

        var newId = Convert.ToInt32(await cmd.ExecuteScalarAsync());
        return Results.Ok(new { succes = true, id = newId });
    }
    catch (Exception ex) {
        return Results.BadRequest(new { succes = false, melding = ex.Message });
    }
});

// ----------------------------------------------------------------------------
// ROUTE 3: Nieuwe speelsessie starten (optioneel gekoppeld aan gebruikerId)
// ----------------------------------------------------------------------------
app.MapPost("/api/sessie", async ([FromBody] StartSessieRequest? req) => {
    using var conn = new MySqlConnection(dbConnectie);
    await conn.OpenAsync();

    int gebruikerId = req?.GebruikerId ?? 1;

    using var cmd = new MySqlCommand(
        "INSERT INTO Sessie (GebruikerId, StartTijd, EindLevel) VALUES (@gebruikerId, NOW(), 1); SELECT LAST_INSERT_ID();", 
        conn
    );
    cmd.Parameters.AddWithValue("@gebruikerId", gebruikerId);

    var id = Convert.ToInt32(await cmd.ExecuteScalarAsync());
    return Results.Ok(new { Id = id });
});

// ----------------------------------------------------------------------------
// ROUTE 4: Resultaat van 1 patiënt opslaan (aangeroepen vanuit Unity)
// ----------------------------------------------------------------------------
app.MapPost("/api/patientcase", async ([FromBody] PatientCaseRequest req) => {
    using var conn = new MySqlConnection(dbConnectie);
    await conn.OpenAsync();
    string query = @"INSERT INTO PatientCase (
        SessieId, Timestamp, Level, 
        CorrecteAirway, CorrecteBreathing, CorrecteCirculation, CorrecteDisability, CorrecteExposure, CorrecteUrgentie,
        InvoerAirway, InvoerBreathing, InvoerCirculation, InvoerDisability, InvoerExposure, InvoerUrgentie,
        AantalFouten, IsCorrect, VerstrekenTijdSeconden
    ) VALUES (
        @sessieId, NOW(), @level,
        @cA, @cB, @cC, @cD, @cE, @cU,
        @iA, @iB, @iC, @iD, @iE, @iU,
        @fouten, @isCorrect, @tijd
    )";
    using var cmd = new MySqlCommand(query, conn);
    cmd.Parameters.AddWithValue("@sessieId", req.SessieId);
    cmd.Parameters.AddWithValue("@level", req.Level);
    cmd.Parameters.AddWithValue("@cA", req.CorrecteAirway);
    cmd.Parameters.AddWithValue("@cB", req.CorrecteBreathing);
    cmd.Parameters.AddWithValue("@cC", req.CorrecteCirculation);
    cmd.Parameters.AddWithValue("@cD", req.CorrecteDisability);
    cmd.Parameters.AddWithValue("@cE", req.CorrecteExposure);
    cmd.Parameters.AddWithValue("@cU", req.CorrecteUrgentie);
    cmd.Parameters.AddWithValue("@iA", req.InvoerAirway);
    cmd.Parameters.AddWithValue("@iB", req.InvoerBreathing);
    cmd.Parameters.AddWithValue("@iC", req.InvoerCirculation);
    cmd.Parameters.AddWithValue("@iD", req.InvoerDisability);
    cmd.Parameters.AddWithValue("@iE", req.InvoerExposure);
    cmd.Parameters.AddWithValue("@iU", req.InvoerUrgentie);
    cmd.Parameters.AddWithValue("@fouten", req.AantalFouten);
    cmd.Parameters.AddWithValue("@isCorrect", req.IsCorrect);
    cmd.Parameters.AddWithValue("@tijd", req.VerstrekenTijdSeconden);
    
    await cmd.ExecuteNonQueryAsync();
    return Results.Ok();
});

// ----------------------------------------------------------------------------
// ROUTE 5: Sessie afsluiten
// ----------------------------------------------------------------------------
app.MapPut("/api/sessie/{id}", async (int id, [FromBody] SluitSessieRequest req) => {
    using var conn = new MySqlConnection(dbConnectie);
    await conn.OpenAsync();
    using var cmd = new MySqlCommand("UPDATE Sessie SET EindTijd = NOW(), EindLevel = @level WHERE Id = @id", conn);
    cmd.Parameters.AddWithValue("@level", req.EindLevel);
    cmd.Parameters.AddWithValue("@id", id);
    await cmd.ExecuteNonQueryAsync();
    return Results.Ok();
});

// ----------------------------------------------------------------------------
// ROUTE 6: Statistieken & percentages filteren op ingelogde gebruiker
// ----------------------------------------------------------------------------
app.MapGet("/api/stats", async ([FromQuery] int? gebruikerId) => {
    using var conn = new MySqlConnection(dbConnectie);
    await conn.OpenAsync();

    string sessieFilter = gebruikerId.HasValue ? "WHERE GebruikerId = @gebruikerId" : "";
    string caseFilter = gebruikerId.HasValue ? "WHERE s.GebruikerId = @gebruikerId" : "";

    // Totaal sessies van deze gebruiker
    using var cmdSessies = new MySqlCommand($"SELECT COUNT(*) FROM Sessie {sessieFilter}", conn);
    if (gebruikerId.HasValue) cmdSessies.Parameters.AddWithValue("@gebruikerId", gebruikerId.Value);
    int totaalSessies = Convert.ToInt32(await cmdSessies.ExecuteScalarAsync());

    // Statistieken van alle patiënten van deze gebruiker
    string statsQuery = $@"
        SELECT 
            COUNT(p.Id) as TotaalCases,
            SUM(CASE WHEN p.IsCorrect = 1 THEN 1 ELSE 0 END) as TotaalCorrect,
            SUM(CASE WHEN p.InvoerAirway = p.CorrecteAirway THEN 1 ELSE 0 END) as AirwayCorrect,
            SUM(CASE WHEN p.InvoerBreathing = p.CorrecteBreathing THEN 1 ELSE 0 END) as BreathingCorrect,
            SUM(CASE WHEN p.InvoerCirculation = p.CorrecteCirculation THEN 1 ELSE 0 END) as CirculationCorrect,
            SUM(CASE WHEN p.InvoerDisability = p.CorrecteDisability THEN 1 ELSE 0 END) as DisabilityCorrect,
            SUM(CASE WHEN p.InvoerExposure = p.CorrecteExposure THEN 1 ELSE 0 END) as ExposureCorrect,
            SUM(CASE WHEN p.InvoerUrgentie = p.CorrecteUrgentie THEN 1 ELSE 0 END) as UrgentieCorrect,
            AVG(p.VerstrekenTijdSeconden) as GemiddeldeTijd,
            MAX(p.Level) as HoogsteLevel
        FROM PatientCase p
        INNER JOIN Sessie s ON p.SessieId = s.Id
        {caseFilter}";

    using var cmdStats = new MySqlCommand(statsQuery, conn);
    if (gebruikerId.HasValue) cmdStats.Parameters.AddWithValue("@gebruikerId", gebruikerId.Value);
    using var reader = await cmdStats.ExecuteReaderAsync();

    if (await reader.ReadAsync() && Convert.ToInt32(reader["TotaalCases"]) > 0)
    {
        int totaal = Convert.ToInt32(reader["TotaalCases"]);
        int correct = Convert.ToInt32(reader["TotaalCorrect"]);
        int aw = Convert.ToInt32(reader["AirwayCorrect"]);
        int br = Convert.ToInt32(reader["BreathingCorrect"]);
        int ci = Convert.ToInt32(reader["CirculationCorrect"]);
        int di = Convert.ToInt32(reader["DisabilityCorrect"]);
        int ex = Convert.ToInt32(reader["ExposureCorrect"]);
        int urg = Convert.ToInt32(reader["UrgentieCorrect"]);
        double avgTijd = reader["GemiddeldeTijd"] != DBNull.Value ? Convert.ToDouble(reader["GemiddeldeTijd"]) : 0;
        int maxLvl = reader["HoogsteLevel"] != DBNull.Value ? Convert.ToInt32(reader["HoogsteLevel"]) : 1;

        return Results.Ok(new {
            heeftData = true,
            totaalSessies,
            totaalCases = totaal,
            totaalCorrect = correct,
            overallAccuratessePct = Math.Round((double)correct / totaal * 100, 1),
            gemiddeldeTijdSeconden = Math.Round(avgTijd, 1),
            hoogsteLevel = maxLvl,
            categoriePercentages = new {
                airway = Math.Round((double)aw / totaal * 100, 1),
                breathing = Math.Round((double)br / totaal * 100, 1),
                circulation = Math.Round((double)ci / totaal * 100, 1),
                disability = Math.Round((double)di / totaal * 100, 1),
                exposure = Math.Round((double)ex / totaal * 100, 1),
                urgentie = Math.Round((double)urg / totaal * 100, 1)
            }
        });
    }

    return Results.Ok(new {
        heeftData = false,
        totaalSessies,
        totaalCases = 0,
        totaalCorrect = 0,
        overallAccuratessePct = 0.0,
        gemiddeldeTijdSeconden = 0.0,
        hoogsteLevel = 1,
        categoriePercentages = new { airway = 0.0, breathing = 0.0, circulation = 0.0, disability = 0.0, exposure = 0.0, urgentie = 0.0 }
    });
});

// ----------------------------------------------------------------------------
// ROUTE 7: Sessies en cases ophalen van de ingelogde gebruiker
// ----------------------------------------------------------------------------
app.MapGet("/api/sessies", async ([FromQuery] int? gebruikerId) => {
    using var conn = new MySqlConnection(dbConnectie);
    await conn.OpenAsync();

    string sessieFilter = gebruikerId.HasValue ? "WHERE s.GebruikerId = @gebruikerId" : "";

    string query = $@"
        SELECT 
            s.Id as SessieId, s.StartTijd, s.EindTijd, s.EindLevel,
            p.Id as CaseId, p.Timestamp, p.Level,
            p.CorrecteAirway, p.CorrecteBreathing, p.CorrecteCirculation, p.CorrecteDisability, p.CorrecteExposure, p.CorrecteUrgentie,
            p.InvoerAirway, p.InvoerBreathing, p.InvoerCirculation, p.InvoerDisability, p.InvoerExposure, p.InvoerUrgentie,
            p.AantalFouten, p.IsCorrect, p.VerstrekenTijdSeconden
        FROM Sessie s
        LEFT JOIN PatientCase p ON s.Id = p.SessieId
        {sessieFilter}
        ORDER BY s.Id DESC, p.Id ASC";

    using var cmd = new MySqlCommand(query, conn);
    if (gebruikerId.HasValue) cmd.Parameters.AddWithValue("@gebruikerId", gebruikerId.Value);

    using var reader = await cmd.ExecuteReaderAsync();
    var sessiesDict = new Dictionary<int, SessieDetailDto>();

    while (await reader.ReadAsync())
    {
        int sessieId = Convert.ToInt32(reader["SessieId"]);
        if (!sessiesDict.TryGetValue(sessieId, out var sessie))
        {
            sessie = new SessieDetailDto
            {
                Id = sessieId,
                StartTijd = Convert.ToDateTime(reader["StartTijd"]),
                EindTijd = reader["EindTijd"] != DBNull.Value ? Convert.ToDateTime(reader["EindTijd"]) : null,
                EindLevel = Convert.ToInt32(reader["EindLevel"]),
                Cases = new List<CaseDetailDto>()
            };
            sessiesDict[sessieId] = sessie;
        }

        if (reader["CaseId"] != DBNull.Value)
        {
            sessie.Cases.Add(new CaseDetailDto
            {
                Id = Convert.ToInt32(reader["CaseId"]),
                Timestamp = Convert.ToDateTime(reader["Timestamp"]),
                Level = Convert.ToInt32(reader["Level"]),
                CorrecteAirway = Convert.ToInt32(reader["CorrecteAirway"]),
                CorrecteBreathing = Convert.ToInt32(reader["CorrecteBreathing"]),
                CorrecteCirculation = Convert.ToInt32(reader["CorrecteCirculation"]),
                CorrecteDisability = Convert.ToInt32(reader["CorrecteDisability"]),
                CorrecteExposure = Convert.ToInt32(reader["CorrecteExposure"]),
                CorrecteUrgentie = Convert.ToInt32(reader["CorrecteUrgentie"]),
                InvoerAirway = Convert.ToInt32(reader["InvoerAirway"]),
                InvoerBreathing = Convert.ToInt32(reader["InvoerBreathing"]),
                InvoerCirculation = Convert.ToInt32(reader["InvoerCirculation"]),
                InvoerDisability = Convert.ToInt32(reader["InvoerDisability"]),
                InvoerExposure = Convert.ToInt32(reader["InvoerExposure"]),
                InvoerUrgentie = Convert.ToInt32(reader["InvoerUrgentie"]),
                AantalFouten = Convert.ToInt32(reader["AantalFouten"]),
                IsCorrect = Convert.ToBoolean(reader["IsCorrect"]),
                VerstrekenTijdSeconden = Convert.ToSingle(reader["VerstrekenTijdSeconden"])
            });
        }
    }

    return Results.Ok(sessiesDict.Values);
});

app.Run("http://0.0.0.0:5000");

// --- Data Structuren ---
public class ScenarioDto
{
    public int Id { get; set; }
    public int Level { get; set; }
    public string Titel { get; set; } = "";
    public string Klacht { get; set; } = "";
    public int Airway { get; set; }
    public int Breathing { get; set; }
    public int Circulation { get; set; }
    public int Disability { get; set; }
    public int Exposure { get; set; }
    public int Pijn { get; set; }
    public int Urgentie { get; set; }
    public string DisabilityReactie { get; set; } = "";
    public string ExposureReactie { get; set; } = "";
}
public record LoginRequest(string Gebruikersnaam, string Wachtwoord);
public record RegisterRequest(string Gebruikersnaam, string Wachtwoord, string VolledigeNaam);
public record StartSessieRequest(int? GebruikerId);
public record SluitSessieRequest(int EindLevel);
public record PatientCaseRequest(
    int SessieId, int Level, 
    int CorrecteAirway, int CorrecteBreathing, int CorrecteCirculation, int CorrecteDisability, int CorrecteExposure, int CorrecteUrgentie,
    int InvoerAirway, int InvoerBreathing, int InvoerCirculation, int InvoerDisability, int InvoerExposure, int InvoerUrgentie,
    int AantalFouten, bool IsCorrect, float VerstrekenTijdSeconden
);

public class SessieDetailDto
{
    public int Id { get; set; }
    public DateTime StartTijd { get; set; }
    public DateTime? EindTijd { get; set; }
    public int EindLevel { get; set; }
    public List<CaseDetailDto> Cases { get; set; } = new();
}

public class CaseDetailDto
{
    public int Id { get; set; }
    public DateTime Timestamp { get; set; }
    public int Level { get; set; }
    public int CorrecteAirway { get; set; }
    public int CorrecteBreathing { get; set; }
    public int CorrecteCirculation { get; set; }
    public int CorrecteDisability { get; set; }
    public int CorrecteExposure { get; set; }
    public int CorrecteUrgentie { get; set; }
    public int InvoerAirway { get; set; }
    public int InvoerBreathing { get; set; }
    public int InvoerCirculation { get; set; }
    public int InvoerDisability { get; set; }
    public int InvoerExposure { get; set; }
    public int InvoerUrgentie { get; set; }
    public int AantalFouten { get; set; }
    public bool IsCorrect { get; set; }
    public float VerstrekenTijdSeconden { get; set; }
}