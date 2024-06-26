using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using PixelMiner.Core;
using PixelMiner.Core.UI;
using System;

namespace PixelMiner.UI
{
    public class UIInventoryDisplay : MonoBehaviour
    {
        private const string PLAYER_TAG = "Player";
       
        [SerializeField] private Button _closeBtn;

        [Header("References")]
        public Player Player;
        private PlayerInventory _pInventory;
        [HideInInspector] public List<UIItemSlot> InBagSlots;
        [HideInInspector] public List<UIItemSlot> HotbarSlots;
        [SerializeField] private UIItemSlot _uiSlotPrefab;
        [SerializeField] private Transform _hotbbarSlotsParent;
        [SerializeField] private Transform _bagHotbbarSlotsParent;
        [SerializeField] private Transform _bagSlotsParent;

        [SerializeField] private UIItemHold _uiItemHold;
        private ItemSlot _currentHoldItemSlot;

     
        private void Start()
        {   
            Main.Instance.OnCharacterInitialize += ConnectToPlayerInventory;

            _closeBtn.onClick.AddListener(() =>
            {

            });
        }

    

        private void OnDestroy()
        {
            _closeBtn.onClick.RemoveAllListeners();
            Main.Instance.OnCharacterInitialize -= ConnectToPlayerInventory;
            PlayerInventory.OnInventoryUpdated -= UpdateInventory;
            PlayerInventory.OnCurrentUseItemChanged -= UpdateInventory;
        }

        private void ConnectToPlayerInventory()
        {
            PlayerInventory.OnInventoryUpdated += UpdateInventory;
            PlayerInventory.OnCurrentUseItemChanged += UpdateInventory;
            Player = Main.Instance.Players[0].GetComponent<Player>();
            _pInventory = Player.GetComponent<PlayerInventory>();
            if (Player != null)
            {
                InitializeHotbar();
                UpdateInventory();
            }
            else
            {
                Debug.LogWarning("Missing player inventory!");
            }
        }

        private void InitializeHotbar()
        {
            for (int i = 0; i < PlayerInventory.WIDTH; i++)
            {
                var uiItemSlot = Instantiate(_uiSlotPrefab, _hotbbarSlotsParent);
                uiItemSlot.Initialized(i, Enums.InventoryID.Hotbars);
                HotbarSlots.Add(uiItemSlot);
            }
        }

        //private void InitializeInventory()
        //{
        //    // Hotbar
        //    InitializeHotbar();

        //    // Bag
        //    for (int i = 0; i < _pInventory.Inventory.Slots.Count; i++)
        //    {
        //        // Hotbar in bag
        //        if(i < PlayerInventory.WIDTH)
        //        {
        //            var uiItemSlot = Instantiate(_uiSlotPrefab, _bagHotbbarSlotsParent);
        //            uiItemSlot.Initialized(i);
        //            InBagSlots.Add(uiItemSlot);
        //        }
        //        else
        //        {
        //            // bag
        //            InBagSlots.Add(Instantiate(_uiSlotPrefab, _bagSlotsParent));
        //        }
               
        //    }
        //}

        public void UpdateInventory()
        {
            for (int i = 0; i < HotbarSlots.Count; i++)
            {
                if(_pInventory.CurrentHotbarSlotIndex == i)
                {
                    HotbarSlots[i].Select(true);

                    if(_pInventory.OpenHotbarInventory == false)
                    {
                        HotbarSlots[i].Use(true);
                    }
                    else
                    {
                        HotbarSlots[i].Use(false);
                    }
                }
                else
                {
                    HotbarSlots[i].Select(false);

                    HotbarSlots[i].Use(false);
                }

    
                HotbarSlots[i].UpdateSlot(_pInventory.Inventory.Slots[i]);
            }

            for (int i = 0; i < InBagSlots.Count; i++)
            {
                InBagSlots[i].UpdateSlot(_pInventory.Inventory.Slots[i]);
            }
        }


        public void UpdateHoldItemVisual()
        {
            _uiItemHold.UpdateSlot(_currentHoldItemSlot);
        }

    }
}
