using UnityEngine;
using System.Collections.Generic;
using PixelMiner.Enums;
using Sirenix.OdinInspector;

namespace PixelMiner.Core
{
    public class GameFactory : MonoBehaviour
    {
        private const string CANVAS_HEALTHBAR_TAG = "Canvas_Healthbar";
        private static Transform _canvasHealthbar;
        //private void Update()
        //{
        //    if (Input.GetKeyDown(KeyCode.T))
        //    {
        //        var itemPrefab = Resources.Load<Item>($"Items/Pickaxe");
        //        for (int i = 0; i < 30000; i++)
        //        {
        //            Vector3 randomPosition = new Vector3(Random.Range(-300, 300), Random.Range(-300, 300), Random.Range(-300, 300));
        //            //var item = CreateItem("Pickaxe", randomPosition, Vector3.zero, this.transform);
        //            //item.AddPhysics();

        //            //var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        //            //cube.transform.position = randomPosition;

        //            var itemInstance = Instantiate(itemPrefab, randomPosition, Quaternion.identity, this.transform);
        //        }
        //    }
        //}

        private void Awake()
        {
            _canvasHealthbar = GameObject.FindGameObjectWithTag(CANVAS_HEALTHBAR_TAG).transform;
            if(_canvasHealthbar == null)
            {
                Debug.LogError($"Not found {CANVAS_HEALTHBAR_TAG} tag. Please add {CANVAS_HEALTHBAR_TAG} into tag.");
            }
        }


        public static Item CreateItem(string itemName, Vector3 position, Vector3 eulerAngles, Transform parent = null)
        {
            var itemPrefab = Resources.Load<Item>($"Items/{itemName}");
            if(itemPrefab != null)
            {
                var itemInstance = Instantiate(itemPrefab, position, Quaternion.Euler(eulerAngles), parent);
                return itemInstance;
            }
            else
            {
                Debug.LogWarning($"Failed to load prefab: {itemName}");
                return null;
            }      
        }


        public static ItemData GetItemData(string itemName)
        {
            // Load from usable item
            var usableItemPrefab = Resources.Load<Item>($"Items/{itemName}");
            if (usableItemPrefab != null)
            {
                return usableItemPrefab.Data;
            }
            else
            {
                Debug.LogError($"Not found item data name {itemName}");
                return null;
            }      
        }


        public static Healthbar CreateHealthbar(Entity entity, Vector3 offsetPosition)
        {
            var healthBarPrefab = Resources.Load<Healthbar>("Healthbar");
            if(healthBarPrefab != null)
            {
                var healthbarInstance = Instantiate(healthBarPrefab, entity.transform.position + offsetPosition, Quaternion.identity, _canvasHealthbar);
                healthbarInstance.Attach(entity, offsetPosition);
                return healthbarInstance;
            }
            else
            {
                Debug.Log("Missing prefab name Healthbar in Resource folder.");
                return null;
            }
        }


        public static Zombie CreateZombie(Vector3 position)
        {
            var zombiePrefab = Resources.Load<Zombie>("Zombie");
            if (zombiePrefab != null)
            {
                var zombieInstace = Instantiate(zombiePrefab, position, Quaternion.identity);
                return zombieInstace;
            }
            else
            {
                Debug.Log("Missing prefab name Zombie in Resource folder.");
                return null;
            }
        }
    }
}
