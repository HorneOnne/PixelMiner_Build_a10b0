namespace PixelMiner.Core
{
    public interface IWeapon
    {
        public float KnockbackForce { get; }
        public int Damage { get; }
        public float Range { get; }

        public void Attack();
    }
}
