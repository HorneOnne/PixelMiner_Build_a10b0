using UnityEngine;
using PixelMiner.DataStructure;
using PixelMiner.Enums;
using System.Collections;

namespace PixelMiner.Core
{
    public class Block : Item, IUseable, ILootable
    {
        private float _animationDuration = 0.25f; // Duration of the animation
 
        public bool Use(Player player)
        {
            if (player.PlayerInventory.CanPlaceBlock)
            {
                //Debug.Log("pLACE BLOCK");
       
                if (Main.Instance.PlaceBlock(player.PlayerBehaviour.SampleBlockTrans.position, (BlockID)Data.ID))
                {
                    Debug.Log("Place block: {True}");
                    return true;
                }
            }
            Debug.Log("Place block: {False}");
            return false;
        }


        public bool LootedBy(PlayerInventory inventory)
        {
            if (inventory.Inventory.AddItem(this.Data))
            {
                this.RemovePhysics();
                Destroy(this.gameObject.GetComponent<Floater>());
                StartCoroutine(AnimateLoot(inventory.GetComponent<Player>().LootedTrans, () =>
                {             
                    Destroy(this.gameObject);
                }));
                inventory.TriggerInventoryUpdateEvent();
                return true;
            }
            else
            {
                return false;
            }
        }

        //private void OnCollisionEnter(Collision collision)
        //{
        //    Debug.Log("Enter");
        //    if (!Physics.Raycast(transform.position, Vector3.down, 0.2f))
        //    {        
        //        rb.isKinematic = true;
        //    }
        //    else
        //    {
        //        rb.isKinematic = true;
        //    }
        //}

    
        private IEnumerator AnimateLoot(Transform target, System.Action onFinished)
        {
            float animationTimer = 0f; // Timer for animation

            while (animationTimer < _animationDuration && Vector3.Distance(transform.position, target.position) > 0.2f)
            {
                // Interpolate position based on animation curve
                float t = animationTimer / _animationDuration;
                transform.position = Vector3.Lerp(transform.position, target.position, t);

                // Increase timer
                animationTimer += UnityEngine.Time.deltaTime;

                yield return null;
            }

            onFinished?.Invoke();
        }

    }
}
