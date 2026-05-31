using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
namespace Fortifid.Core;

public abstract class Enemy : Entity
{
    protected int _damage;
    protected float _detectionRange;
    protected float _attackCooldown;
    protected float _attackTimer;
    
    public int Damage => _damage;

    protected Enemy(string name, int maxHP, float speed, int damage, float detectionRange)
        : base(name, maxHP, speed)
    {
        _damage = damage;
        _detectionRange = detectionRange;
        _attackCooldown = 1.5f;
        _attackTimer = 0f;
    }
    
    public abstract void AttackPlayer(Player player);

    protected void MoveTowards(Vector2 target, float dt)
    {
        Vector2 dir = target - _position;
        if (dir.LengthSquared() < 1f) return;
        dir.Normalize();
        _position += dir * _damage * dt;
    }

    protected bool CanSeePlayer(Player player)
    {
        return Vector2.Distance(_position, player.Position) <= _detectionRange;
    }

    protected bool IsInAttackRange(Player player, float range = 70f)
    {
        return Vector2.Distance(Ceneter, player.Ceneter) <= range;
    }
    
}