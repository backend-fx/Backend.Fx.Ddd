using System;
using JetBrains.Annotations;

namespace Backend.Fx.Ddd;

[PublicAPI]
public interface IIdGenerator<out TId> where TId : IEquatable<TId>
{
    TId NextId();
}
