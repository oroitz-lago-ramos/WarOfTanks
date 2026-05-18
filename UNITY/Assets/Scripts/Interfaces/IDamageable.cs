using System;

public interface IDamageable
{
    bool IsAlive { get; }
    event Action OnDied;
}
