using System.Collections;
using UnityEngine;

namespace PixelMiner.Core
{
    public class Player : Entity, IFlashEffect
    {
        #region Entity field
        private const byte BASE_MAX_HEALTH = 100;
        private const byte BASE_ARMOR = 0;
        private const byte BASE_ATK_POWER = 1;
        #endregion


        // Flashing
        private Renderer[] _renderers;
        [SerializeField] private AnimationCurve _flashAnimCurve;

        #region Combat
        private float _takeDamageCooldown = 0.5f; // time in second.
        private bool _canTakeDamaged = true;
        #endregion


        public Transform AimTarrgetTrans;
        public Transform LineOfSignTrans;
        public Transform UpperBCheckTrans;
        public Transform MiddleBCheckTrans;
        public Transform LowerBCheckTrans;

        [HideInInspector] public Transform CurrentBCheckTrans;
        [SerializeField] private Collider Collider;
        public Transform CombatRayPointTrans;
        public Transform LootedTrans;


        #region Properties

        public PlayerBehaviour PlayerBehaviour { get; private set; }
        public PlayerController PlayerController { get; private set; }
        public PlayerInventory PlayerInventory { get; private set; }
      
        public Bounds Bounds { get => Collider.bounds; }
        public Renderer[] Rendereres { get => _renderers; }
        public Vector3 LineOfSignDirection { get => LineOfSignTrans.forward; }
      
        #endregion

        private void Awake()
        {
            PlayerBehaviour = GetComponent<PlayerBehaviour>();
            PlayerController = GetComponent<PlayerController>();
            PlayerInventory = GetComponent<PlayerInventory>();

            _renderers = GetComponentsInChildren<Renderer>();
  


            Model = transform.Find("Model");
            if (Model == null)
            {
                Debug.LogWarning("Missing model reference.");
            }

            CurrentBCheckTrans = UpperBCheckTrans;


            // Set default entity values
            MaxHealth = BASE_MAX_HEALTH;
            Health = BASE_MAX_HEALTH;
            Armor = BASE_ARMOR;
            AttackPower = BASE_ATK_POWER;       
        }

        public override void Attack(Entity target)
        {
            target.TakeDamge(AttackPower, this);
        }

        public override void TakeDamge(byte damage, Entity fromEntity)
        {
            if(_canTakeDamaged)
            {
                _canTakeDamaged = false;

                Health -= damage;
                StartCoroutine(DoFlashing(0.5f));
                AudioManager.Instance.PlayPlayerHitSfx(transform.position);
                DoKnockback(fromEntity.transform.position);
                //Debug.Log($"Player health: {Health}");
                if (Health <= 0)
                {
                    Health = 0;
                    Die();
                }

                Invoke(nameof(ResetCanTakeDamaged), _takeDamageCooldown);
            }
          
        }

        public override void Die()
        {
            Debug.Log("Entity died.");
        }



        // Flashing effect (when being attacked);
        public IEnumerator DoFlashing(float flashTime)
        {
            float currFlashAmount = 0.0f;
            float elapsedTime = 0.0f;
            while (elapsedTime < flashTime)
            {
                elapsedTime += UnityEngine.Time.deltaTime;
                //currFlashAmount = Mathf.Lerp(0f, 1.0f, elapsedTime / flashTime);
                currFlashAmount = _flashAnimCurve.Evaluate(elapsedTime / flashTime);
                //Debug.Log(currFlashAmount);
                SetFlashAmount(currFlashAmount);
                yield return null;
            }

            // Reset
            SetFlashAmount(0);
        }
        private void SetFlashAmount(float amount)
        {
            for (int i = 0; i < _renderers.Length; i++)
            {
                _renderers[i].material.SetFloat("_FlashAmount", amount);
            }
        }

        private void DoKnockback(Vector3 atkPosition)
        {
            Vector3 atkDirection = (transform.position - atkPosition).normalized;
            float knocbackForce = 25;
            atkDirection *= knocbackForce;
            atkDirection.y = 1.65f;
            GetComponent<Rigidbody>().AddForce(atkDirection, ForceMode.Impulse);
        }


        #region Reset state
        private void ResetCanTakeDamaged()
        {
            _canTakeDamaged = true;
        }
        #endregion
    }


}
