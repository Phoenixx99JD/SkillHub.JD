using ClosedXML.Excel;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SkillHub.Application.Common;
using SkillHub.Domain.Entities;
using SkillHub.Infrastructure.Identity;
using SkillHub.Infrastructure.Persistence;
using SkillHub.Infrastructure.Services;

namespace SkillHub.Infrastructure.Seeding;

/// <summary>
/// Applies pending migrations and loads the Instructors, Participants and Workshops sheets of
/// SkillHub_Data.xlsx. Seeding is idempotent: rows that already exist (by email or code) are skipped.
/// The Enrollments sheet is intentionally NOT loaded here; it goes through POST /api/v1/enrollments/import.
/// </summary>
public class DatabaseInitializer(
    SkillHubDbContext db,
    UserManager<ApplicationUser> userManager,
    RoleManager<IdentityRole> roleManager,
    IOptions<SeedOptions> options,
    IHostEnvironment environment,
    ILogger<DatabaseInitializer> logger)
{
    private readonly SeedOptions _options = options.Value;

    public async Task InitializeAsync(CancellationToken ct = default)
    {
        await db.Database.MigrateAsync(ct);

        foreach (var role in new[] { Roles.Employee, Roles.Participant })
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));

        var path = Path.IsPathRooted(_options.ExcelPath)
            ? _options.ExcelPath
            : Path.GetFullPath(Path.Combine(environment.ContentRootPath, _options.ExcelPath));

        if (!File.Exists(path))
        {
            logger.LogWarning("Seed file {Path} was not found; skipping data seeding.", path);
            return;
        }

        using var workbook = new XLWorkbook(path);
        await SeedInstructorsAsync(workbook, ct);
        await SeedParticipantsAsync(workbook, ct);
        await SeedWorkshopsAsync(workbook, ct);
    }

    private async Task SeedInstructorsAsync(XLWorkbook workbook, CancellationToken ct)
    {
        if (GetSheet(workbook, "Instructors") is not { } reader) return;
        var created = 0;

        foreach (var row in reader.DataRows)
        {
            var email = reader.GetString(row, "Email", "InstructorEmail", "Correo")?.ToLowerInvariant();
            var fullName = ReadFullName(reader, row);
            var specialty = reader.GetString(row, "Specialty", "Speciality", "Especialidad");
            if (email is null || fullName is null || specialty is null)
            {
                logger.LogWarning("Instructors row {Row} is incomplete and was skipped.", row.RowNumber());
                continue;
            }

            if (await db.Instructors.AnyAsync(i => i.Email == email, ct)) continue;

            var password = reader.GetString(row, "Password", "Contrasena") ?? _options.EmployeePassword;
            var userId = await EnsureUserAsync(email, password, Roles.Employee);
            db.Instructors.Add(new Instructor(fullName, email, specialty, userId));
            await db.SaveChangesAsync(ct);
            created++;
        }

        logger.LogInformation("Seeded {Count} instructors.", created);
    }

    private async Task SeedParticipantsAsync(XLWorkbook workbook, CancellationToken ct)
    {
        if (GetSheet(workbook, "Participants") is not { } reader) return;
        var created = 0;

        foreach (var row in reader.DataRows)
        {
            var email = reader.GetString(row, "Email", "ParticipantEmail", "Correo")?.ToLowerInvariant();
            var fullName = ReadFullName(reader, row);
            if (email is null || fullName is null)
            {
                logger.LogWarning("Participants row {Row} is incomplete and was skipped.", row.RowNumber());
                continue;
            }

            if (await db.Participants.AnyAsync(p => p.Email == email, ct)) continue;

            var password = reader.GetString(row, "Password", "Contrasena") ?? _options.ParticipantPassword;
            var userId = await EnsureUserAsync(email, password, Roles.Participant);
            db.Participants.Add(new Participant(fullName, email, userId));
            await db.SaveChangesAsync(ct);
            created++;
        }

        logger.LogInformation("Seeded {Count} participants.", created);
    }

    private async Task SeedWorkshopsAsync(XLWorkbook workbook, CancellationToken ct)
    {
        if (GetSheet(workbook, "Workshops") is not { } reader) return;
        var created = 0;

        foreach (var row in reader.DataRows)
        {
            var code = reader.GetString(row, "Code", "WorkshopCode", "Codigo")?.ToUpperInvariant();
            var title = reader.GetString(row, "Title", "Name", "Titulo");
            var description = reader.GetString(row, "Description", "Descripcion") ?? string.Empty;
            var instructorEmail = reader.GetString(row, "InstructorEmail", "Instructor", "InstructorName", "Email")?.ToLowerInvariant();
            var capacity = reader.GetInt(row, "MaxCapacity", "Capacity", "Quota", "MaxQuota", "Seats", "Cupo", "CupoMaximo");
            var (startsAt, endsAt) = ReadSchedule(reader, row);

            if (code is null || title is null || instructorEmail is null || capacity is null || startsAt is null || endsAt is null)
            {
                logger.LogWarning("Workshops row {Row} is incomplete and was skipped.", row.RowNumber());
                continue;
            }

            if (await db.Workshops.AnyAsync(w => w.Code == code, ct)) continue;

            // The column may hold the instructor's email or full name.
            var instructor = instructorEmail.Contains('@')
                ? await db.Instructors.FirstOrDefaultAsync(i => i.Email == instructorEmail, ct)
                : await db.Instructors.FirstOrDefaultAsync(i => i.FullName.ToLower() == instructorEmail, ct);
            if (instructor is null)
            {
                logger.LogWarning("Workshop {Code}: instructor {Email} does not exist; skipped.", code, instructorEmail);
                continue;
            }

            try
            {
                // Already published workshops may be in the past, so the "future date" rule does not apply.
                db.Workshops.Add(Workshop.Restore(code, title, description, instructor, startsAt.Value, endsAt.Value, capacity.Value));
                await db.SaveChangesAsync(ct);
                created++;
            }
            catch (Domain.Exceptions.DomainException ex)
            {
                logger.LogWarning("Workshop {Code} was skipped: {Reason}", code, ex.Message);
            }
        }

        logger.LogInformation("Seeded {Count} workshops.", created);
    }

    private static (DateTime? StartsAt, DateTime? EndsAt) ReadSchedule(ExcelSheetReader reader, IXLRow row)
    {
        var startsAt = reader.GetDateTime(row, "StartsAt", "StartDateTime", "StartDate", "Start", "StartAt", "FechaInicio");
        var endsAt = reader.GetDateTime(row, "EndsAt", "EndDateTime", "EndDate", "End", "EndAt", "FechaFin");

        // Layout with a single date column plus start/end time columns.
        var date = reader.GetDateTime(row, "Date", "Fecha");
        var startTime = reader.GetTime(row, "StartTime", "HoraInicio");
        var endTime = reader.GetTime(row, "EndTime", "HoraFin");

        if (startTime is not null) startsAt = (startsAt ?? date)?.Date + startTime;
        if (endTime is not null) endsAt = (endsAt ?? startsAt ?? date)?.Date + endTime;

        return (startsAt, endsAt);
    }

    private static string? ReadFullName(ExcelSheetReader reader, IXLRow row)
    {
        var fullName = reader.GetString(row, "FullName", "Name", "NombreCompleto", "Nombre");
        if (fullName is not null) return fullName;

        var first = reader.GetString(row, "FirstName", "FirstNames", "Nombres");
        var last = reader.GetString(row, "LastName", "LastNames", "Apellidos");
        var combined = $"{first} {last}".Trim();
        return combined.Length == 0 ? null : combined;
    }

    private ExcelSheetReader? GetSheet(XLWorkbook workbook, string name)
    {
        var sheet = workbook.Worksheets.FirstOrDefault(s =>
            ExcelSheetReader.Normalize(s.Name) == ExcelSheetReader.Normalize(name));

        if (sheet?.FirstRowUsed() is null)
        {
            logger.LogWarning("Sheet {Sheet} was not found or is empty in the seed file.", name);
            return null;
        }

        return new ExcelSheetReader(sheet);
    }

    private async Task<string> EnsureUserAsync(string email, string password, string role)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user is null)
        {
            user = new ApplicationUser { UserName = email, Email = email, EmailConfirmed = true };
            var result = await userManager.CreateAsync(user, password);
            if (!result.Succeeded)
                throw new InvalidOperationException($"Could not create user {email}: " +
                                                    string.Join(", ", result.Errors.Select(e => e.Description)));
        }

        if (!await userManager.IsInRoleAsync(user, role))
            await userManager.AddToRoleAsync(user, role);

        return user.Id;
    }
}

public static class DatabaseInitializerExtensions
{
    /// <summary>
    /// Runs migrations and seeding unless "Database:InitializeOnStartup" is false (used when another
    /// process, e.g. the API container, is responsible for it).
    /// </summary>
    public static async Task InitializeDatabaseAsync(this IServiceProvider services)
    {
        var configuration = services.GetRequiredService<IConfiguration>();
        if (!configuration.GetValue("Database:InitializeOnStartup", true)) return;

        using var scope = services.CreateScope();
        await scope.ServiceProvider.GetRequiredService<DatabaseInitializer>().InitializeAsync();
    }
}
