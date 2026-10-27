namespace SkillHub.Application.Abstractions.Persistence;

public interface IUnitOfWork
{
    /// <summary>
    /// Persists pending changes. Throws <see cref="Common.Exceptions.ConflictException"/> when the data
    /// was modified concurrently by another request.
    /// </summary>
    Task SaveChangesAsync(CancellationToken ct = default);

    /// <summary>Runs the given operation inside a database transaction.</summary>
    Task ExecuteInTransactionAsync(Func<Task> operation, CancellationToken ct = default);

    /// <summary>Discards every pending (unsaved) change.</summary>
    void DiscardChanges();
}
