using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Backend.Fx.Exceptions;
using JetBrains.Annotations;

namespace Backend.Fx.Ddd;

/// <summary>
/// Encapsulates methods for retrieving domain objects 
/// See https://en.wikipedia.org/wiki/Domain-driven_design#Building_blocks
/// </summary>
[PublicAPI]
public interface IRepository<TAggregateRoot, in TId> where TAggregateRoot : IAggregateRoot<TId>
    where TId : IEquatable<TId>
{
    /// <summary>
    /// Throws a <see cref="NotFoundException{TEntity}"/> when nothing matches the given id
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<TAggregateRoot> GetByIdAsync(TId id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns <c>null</c> when nothing matches the given id
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<TAggregateRoot> GetByIdOrDefaultAsync(TId id, CancellationToken cancellationToken = default);

    Task<TAggregateRoot[]> GetAllAsync(CancellationToken cancellationToken = default);

    Task<bool> AnyAsync(CancellationToken cancellationToken = default);

    Task<TAggregateRoot[]> ResolveAsync(IEnumerable<TId> ids, CancellationToken cancellationToken = default);
}