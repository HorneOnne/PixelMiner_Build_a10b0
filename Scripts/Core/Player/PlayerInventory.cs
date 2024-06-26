using UnityEngine;
using PixelMiner.Enums;
using PixelMiner.Miscellaneous;
using PixelMiner.InputSystem;
using PixelMiner.Core.UI;
using System;

namespace PixelMiner.Core
{
    public class PlayerInventory : MonoBehaviour
    {
        public static event System.Action OnCurrentUseItemChanged;
        public static event System.Action OnInventoryUpdated;

        private Player _player;
        public Inventory Inventory;
        private InputHander _input;
        private Main _main;
        public int MAX_PLAYER_INVENTORY_SLOTS { get; private set; }
        public const int WIDTH = 7;
        public const int HEIGHT = 1;


        public int CurrentHotbarSlotIndex = -1;
        public int CurrentHotbarUseSlotIndex = -1;


        private bool _canDirectionalHotbar = true;

        public bool OpenHotbarInventory { get; private set; } = false;

        [SerializeField] private Item _currentItem;
        [SerializeField] private Transform _rightHand;
        private int _useItemTimesPersecond = 5;
        private float _useItemResetTime;
        private bool _canUseItem = true;

        [Header("Looot items")]
        [SerializeField] private Vector3 _center;
        [SerializeField] private Vector3 _halfSize;
        [SerializeField] private Color _boundColor;
        [SerializeField] private bool _showBounds;
        private Collider[] _itemEntites;
        private const int MAX_ITEM_DETECT_IN_FRAME = 8;
        [SerializeField] private LayerMask _itemLayer;
        [SerializeField] private LayerMask _playerLayer;

        public bool CanPlaceBlock { get; private set; }
        private Vector3 _blockSize = new Vector3(0.9999f, 0.9999f, 0.9999f);

        private void Awake()
        {
            _itemLayer = 0;
            _itemLayer = 1 << 23;

            _playerLayer = 0;
            _playerLayer = 1 << 7;

            MAX_PLAYER_INVENTORY_SLOTS = WIDTH * HEIGHT;
            Inventory = new Inventory(WIDTH, HEIGHT);
            _itemEntites = new Collider[MAX_ITEM_DETECT_IN_FRAME];
        }


        private void Start()
        {
            // EVENTS
            UIItemSlot.OnHotbarSlotClicked += OnHotbarItemChanged;
            UseItemBtn.OnPressed += UseItemLogicHandler;

            _main = Main.Instance;
            _input = InputHander.Instance;
            _player = GetComponent<Player>();
            CurrentHotbarSlotIndex = 0;
          

            // Add init items for testing purposes
            Inventory.AddItem(GameFactory.GetItemData(ItemID.Pickaxe.ToItemString()));
            Inventory.AddItem(GameFactory.GetItemData(ItemID.Sword.ToItemString()));

            var lightItemData = GameFactory.GetItemData(ItemID.Light.ToItemString());
            for (int i = 0; i < 64; i++)
            {
                Inventory.AddItem(lightItemData);
            }

            _useItemResetTime = 1.0f / _useItemTimesPersecond;
            OnInventoryUpdated?.Invoke();
        }

   
        private void OnDestroy()
        {
            // EVENTS
            UIItemSlot.OnHotbarSlotClicked -= OnHotbarItemChanged;
            UseItemBtn.OnPressed -= UseItemLogicHandler;
        }

        private void FixedUpdate()
        {

            //System.Array.Clear(_itemEntites, 0, _itemEntites.Length);
            int itemHit = Physics.OverlapBoxNonAlloc(transform.position + _center, _halfSize, _itemEntites, Quaternion.identity, _itemLayer);
    
            if (itemHit > 0)
            {
                for (int i = 0; i < itemHit; i++)
                {
                    if (_itemEntites[i].transform.TryGetComponent(out ILootable item))
                    {         
                        if(item.LootedBy(this))
                        {
                            AudioManager.Instance.PlayItemPickupfx(_player.LootedTrans.position);
                        }
                    }
                }
            }
        }

        private void Update()
        {
            if (_showBounds)
            {
                Bounds b = new Bounds(transform.position + _center, _halfSize * 2);
                DrawBounds.Instance.AddBounds(b, _boundColor);
            }


            if ((_input.ControlScheme & Enums.ControlScheme.KeyboardAndMouse) != 0)
            {
                float scrollWheelInput = Input.GetAxis("Mouse ScrollWheel");
                //Debug.Log(scrollWheelInput);
                // Check if there's any scroll wheel input
                if (scrollWheelInput != 0)
                {
                    // Scroll wheel moved up
                    if (scrollWheelInput > 0)
                    {
                        CurrentHotbarSlotIndex = (CurrentHotbarSlotIndex + 1) % WIDTH;
                    }
                    // Scroll wheel moved down
                    else if (scrollWheelInput < 0)
                    {
                        CurrentHotbarSlotIndex--;
                        if(CurrentHotbarSlotIndex == -1)
                        {
                            CurrentHotbarSlotIndex = WIDTH - 1;
                        }
                    }
                }

                if (CurrentHotbarSlotIndex != CurrentHotbarUseSlotIndex)
                {
                    CurrentHotbarUseSlotIndex = CurrentHotbarSlotIndex;
                    DestroyOldItem();
                    CreateNewItemObject();

                    OnCurrentUseItemChanged?.Invoke();
                }
            }

            //if (Input.GetKeyDown(KeyCode.Alpha1))
            //{
            //    Debug.Log("Alpha1");
            //    bool success = Inventory.AddItem(ItemFactory.GetItemData(ItemID.Dirt));
            //    Debug.Log($"Success: {success}");
            //}
            //if (Input.GetKeyDown(KeyCode.Alpha2))
            //{
            //    Debug.Log("Alpha2");
            //    Inventory.AddItem(ItemFactory.GetItemData(ItemID.DirtGrass));
            //}



            if (_input.InventoryDirectional.y == -1)
            {
                OpenHotbarInventory = true;
            }
            else if (_input.InventoryDirectional.y == 1)
            {
                OpenHotbarInventory = false;
            }
            if (_canDirectionalHotbar && OpenHotbarInventory)
            {
                if (_input.InventoryDirectional.x == 1)
                {
                    Next();
                    _canDirectionalHotbar = false;
                    Invoke(nameof(ResetDirectionalHotbar), 0.2f);
                }
                else if (_input.InventoryDirectional.x == -1)
                {
                    Previous();
                    _canDirectionalHotbar = false;
                    Invoke(nameof(ResetDirectionalHotbar), 0.2f);
                }
            }


            if (CurrentHotbarSlotIndex != CurrentHotbarUseSlotIndex &&
                CurrentHotbarSlotIndex != -1 &&
                _input.InventoryDirectional.y == 1)
            {
                CurrentHotbarUseSlotIndex = CurrentHotbarSlotIndex;

                DestroyOldItem();
                CreateNewItemObject();

                OnCurrentUseItemChanged?.Invoke();
            }


            // If item is block (check can place)
            if (_currentItem != null && (ushort)_currentItem.Data.ID < 1000)
            {
                bool sampleActive = _player.PlayerBehaviour.SampleBlockTrans.gameObject.activeInHierarchy;
                if (CanPlaceBlockAt(_player.PlayerBehaviour.SampleBlockTrans.position))
                {
                    CanPlaceBlock = true;
                    if (!sampleActive)
                    {
                        _player.PlayerBehaviour.SampleBlockTrans.gameObject.SetActive(true);
                    }
                }
                else
                {
                    CanPlaceBlock = false;
                    if (sampleActive)
                    {
                        _player.PlayerBehaviour.SampleBlockTrans.gameObject.SetActive(false);
                    }
                }
            }
            else
            {
                bool sampleActive = _player.PlayerBehaviour.SampleBlockTrans.gameObject.activeInHierarchy;
                if (sampleActive)
                {
                    _player.PlayerBehaviour.SampleBlockTrans.gameObject.SetActive(false);
                }
            }


            // Use item
            //UseItemLogicHandler();
        }


        private void UseItemLogicHandler()
        {
            if (_canUseItem)
            {
                Debug.Log($"Use item");
                _canUseItem = false;
                Invoke(nameof(EnableCanUseItem), _useItemResetTime);

                IUseable useableItem = _currentItem as IUseable;

                if (useableItem != null)
                {
                    // Use the item
                    if (useableItem.Use(_player))
                    {

                        //if(useableItem.RemainingUses == 0)
                        //{
                        //    if (Inventory.RemoveItem(CurrentHotbarUseSlotIndex) > 0)
                        //    {
                        //        Debug.Log("A");
                        //    }
                        //    else
                        //    {
                        //        Debug.Log("B");
                        //        Destroy(_currentItem.gameObject);
                        //        _currentItem = null;
                        //    }
                        //}

                        int remainingUse = Inventory.Slots[CurrentHotbarUseSlotIndex].UseableItemData.RemainingUse--;
                        if (remainingUse > 1)
                        {

                        }
                        else
                        {
                            Inventory.RemoveItem(CurrentHotbarUseSlotIndex);

                            if (Inventory.Slots[CurrentHotbarUseSlotIndex].Quantity > 0)
                            {

                            }
                            else
                            {
                                Destroy(_currentItem.gameObject);
                                _currentItem = null;
                            }
                        }
                    }
                }
            }
        }


        private void OnHotbarItemChanged(int index)
        {
            Debug.Log($"index: {index}");
            CurrentHotbarSlotIndex = index;
        }

        private void ResetDirectionalHotbar()
        {
            _canDirectionalHotbar = true;
        }

        public void Next()
        {
            //CurrentHotbarSlotIndex = (CurrentHotbarSlotIndex + 1) % WIDTH;
            CurrentHotbarSlotIndex++;
            if (CurrentHotbarSlotIndex == WIDTH)
            {
                CurrentHotbarSlotIndex = -1;
            }
        }


        public void Previous()
        {
            //CurrentHotbarSlotIndex = (CurrentHotbarSlotIndex - 1 + WIDTH) % WIDTH;

            CurrentHotbarSlotIndex--;
            if (CurrentHotbarSlotIndex == -2)
            {
                CurrentHotbarSlotIndex = WIDTH - 1;
            }
        }

        private void DestroyOldItem()
        {
            if (_currentItem != null)
            {
                Destroy(_currentItem.gameObject);
                _currentItem = null;
            }
        }
        private void CreateNewItemObject()
        {
            ItemSlot currentSlot = Inventory.Slots[CurrentHotbarUseSlotIndex];
            if (currentSlot != null &&
                currentSlot.UseableItemData != null &&
                currentSlot.UseableItemData.ItemData != null)
            {
                _currentItem = GameFactory.CreateItem(currentSlot.UseableItemData.ItemData.ItemName, _rightHand.position, Vector3.zero, _rightHand);
                _currentItem.transform.localEulerAngles = _currentItem.Data.RotateAngles;
                _currentItem.transform.localPosition += _currentItem.Data.OffsetPosition;
                _currentItem.EnableFloatingRotateEffect(false);

                ChangeChildLayers(_currentItem.transform, _playerLayer);
            }
        }



        private void EnableCanUseItem()
        {
            _canUseItem = true;
        }


    
        private bool CanPlaceBlockAt(Vector3 globalPosition)
        {
            Bounds blockBounds = _main.GetBlockBounds(globalPosition, _blockSize);
            if (_player.Bounds.Intersects(blockBounds) == false)
            {
                BlockID blockID = _main.GetBlock(globalPosition);
                if (blockID == BlockID.Air ||
                    blockID == BlockID.Water)
                {
                    return true;
                }
            }
            return false;
        }

        private bool CanRemoveBlockAt(Vector3 globalPosition)
        {
            BlockID blockID = _main.GetBlock(globalPosition);
            if (blockID != BlockID.Air)
            {
                return true;
            }
            return false;
        }

        
        public void TriggerInventoryUpdateEvent()
        {
            OnInventoryUpdated?.Invoke();
        }

        #region Change physics layers
        private void ChangeChildLayers(Transform parent, LayerMask layer)
        {
            ChangeLayerPlayer(parent.gameObject);
            foreach (Transform child in parent)
            {
                ChangeChildLayers(child, layer);
            }
        }

        private void ChangeLayerPlayer(GameObject obj)
        {
            obj.layer = LayerMask.NameToLayer("Player");
        }
        #endregion
    }
}
