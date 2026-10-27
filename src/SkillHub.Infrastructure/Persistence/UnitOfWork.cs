using Microsoft.EntityFrameworkCore;
using SkillHub.Application.Abstractions.Persistence;
using SkillHub.Application.Common.Exceptions;

namespace SkillHub.Infrastructure.Persistence;

public class UnitOfWork(SkillHubDbContext db) : IUnitOfWork
{
    public async Task SaveChangesAsync(CancellationToken ct = default)
    {
        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new ConflictException("The data was modified by another request. Please try again.");
        }
        catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("Duplicate entry") == true)
        {
            throw new ConflictException("The record already exists.");
        }
    }

    public async Task ExecuteInTransactionAsync(Func<Task> operation, CancellationToken ct = default)
    {
        await using var transaction = await db.Database.BeginTransactionAsync(ct);
        await operation();
        await transaction.CommitAsync(ct);
    }

    public void DiscardChanges() => db.ChangeTracker.Clear();
}
