﻿using System;
using JetBrains.Annotations;

namespace Backend.Fx.Ddd;

/// <summary>
/// The root of an aggregate.
/// </summary>
public interface IAggregateRoot {}
    
/// <summary>
/// The root of an aggregate, identified by an id of type <see cref="TId"/>.
/// </summary>
[PublicAPI]
public interface IAggregateRoot<out TId> : IAggregateRoot
    where TId : IEquatable<TId>
{
    TId Id { get; }
}