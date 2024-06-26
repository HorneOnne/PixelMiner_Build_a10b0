namespace PixelMiner.Core
{
    public class LavaProjectile : Projectile
    {
        public override void OnReturnPool()
        {
            LavaProjectilePool.Pool.Release(this);
        }
    }
}
