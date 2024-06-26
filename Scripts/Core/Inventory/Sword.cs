using UnityEngine;
using PixelMiner.Miscellaneous;
namespace PixelMiner.Core
{
    public class Sword : Item, IUseable
    {
        public static System.Action OnItemBroken;
        [SerializeField] private int _remainingUses;
        public int RemainingUses { get => _remainingUses; }

        [SerializeField] private LayerMask _enemyLayer;

        protected override void Awake()
        {
            base.Awake();
            _enemyLayer = 0;
            _enemyLayer = 1 << 27;
        }


        public bool Use(Player player)
        {
            if (_remainingUses > 0)
            {
                // Implemeent use logic here.

                Debug.DrawRay(player.LineOfSignTrans.position, player.LineOfSignDirection * 3f, Color.red, 1.0f);
                if(Physics.Raycast(player.LineOfSignTrans.position, player.LineOfSignDirection, out RaycastHit hitInfo, 3f, _enemyLayer))
                {
                    Entity hitEntity = hitInfo.collider.gameObject.GetComponentInParent<Entity>();
                    if (hitEntity != null && hitEntity.IsAlive)
                    {
                        player.Attack(hitEntity);
                        Debug.Log("hit enemy");

                        // Decrease the remaining uses
                        _remainingUses--;

                        // Handle item is broken.
                        if (_remainingUses == 0)
                        {
                            Debug.Log($"{Data.ItemName} has been broken.");
                            Destroy(this.gameObject);
                            OnItemBroken?.Invoke();
                        }

                        return true;
                    }     
                }                
            }
            else
            {
                Debug.Log($"Out of uses for: {Data.ItemName}");
            }

            return true;
        }
    }
}
