using BehaviorDesigner.Runtime;
using PixelMiner.Core;
namespace PixelMiner.AI
{
    [System.Serializable]
    public class SharedEntity : SharedVariable<Entity>
    {
        public static implicit operator SharedEntity(Entity value) { return new SharedEntity { Value = value }; }
    }
}