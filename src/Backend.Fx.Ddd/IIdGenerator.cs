using JetBrains.Annotations;

namespace Backend.Fx.Ddd;

[PublicAPI]
public interface IIdGenerator<out TId> 
{
    TId NextId();
}