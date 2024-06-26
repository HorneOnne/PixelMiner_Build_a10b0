using System.Collections;
using UnityEngine;
using BehaviorDesigner.Runtime.Tasks;
using BehaviorDesigner.Runtime;

namespace PixelMiner.Core
{
    public class Zombie : Mob, IFlashEffect
    {
        #region Entity field
        private const byte BASE_MAX_HEALTH = 10;
        private const byte BASE_ARMOR = 0;
        private const byte BASE_ATK_POWER = 1;
        #endregion


        // references
        private Animator _anim;

        // target zombie seeing.
        [SerializeField] private Transform _headLookTarget;
        [SerializeField] private Renderer[] _renderers;
        [SerializeField] private AnimationCurve _flashAnimCurve;


        // random zombie growling time counter
        private float _updateTimer = 0.0f;
        private float _updateTime = 2.0f;
        private float _updateTimeMin = 2.0f;
        private float _updateTimeMax = 5.0f;



        #region Combat
        private float _takeDamageCooldown = 0.5f; // time in second.
        private bool _canTakeDamaged = true;
        private bool _beAttacked = false;
        #endregion



        #region Animation hash
        private int _dieAnimHash = Animator.StringToHash("Die");

        #endregion

        #region Properties
        public Transform HeadLookTarget { get => _headLookTarget; }
        public Renderer[] Rendereres { get => _renderers; }
        public bool BeAttacked { get => _beAttacked; set => _beAttacked = value; }
        #endregion

        protected override void Awake()
        {
            base.Awake();
            Model = transform.Find("Model").transform;
            _anim = Model.GetComponent<Animator>();
            _renderers = GetComponentsInChildren<Renderer>();    

            if (Model == null)
            {
                Debug.LogError("Missing zombie model reference.");
            }

            if (_headLookTarget == null)
            {
                Debug.LogWarning("Missing head look target reference.");
            }

            _updateTime = Random.Range(_updateTimeMin, _updateTimeMax);



            // Set default entity values
            MaxHealth = BASE_MAX_HEALTH;
            Health = MaxHealth;
            Armor = BASE_ARMOR;
            AttackPower = BASE_ATK_POWER;
        }


        protected override void Start()
        {
            base.Start();


            // Add healthbar for this entity
            GameFactory.CreateHealthbar(this, new Vector3(0,2.3f, 0));
        }

        protected void FixedUpdate()
        {
            _updateTimer += UnityEngine.Time.deltaTime;
            if(_updateTimer > _updateTime && IsAlive)
            {
                // Growling

                _updateTimer = 0.0f;
                _updateTime = Random.Range(_updateTimeMin, _updateTimeMax);
                AudioManager.Instance.PlayZombieGrowlSfx(transform.position);
            }

            if(IsAlive == false)
            {
                Rigidbody.velocity = new Vector3(0, Rigidbody.velocity.y, 0);
                HeadLookTarget.position = transform.position +  transform.forward + new Vector3(0,1.85f,0f);
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();         
        }

        #region Entity callback
        public override void Attack(Entity target)
        {
            target.TakeDamge(AttackPower, this);

        }

        public override void TakeDamge(byte damage, Entity fromEntity)
        {
            if (!_canTakeDamaged) return;
            _canTakeDamaged = false;
            _beAttacked = true;
            Health -= damage;    
            StartCoroutine(DoFlashing(0.5f));
            DoKnockback(fromEntity.transform.position);
            AudioManager.Instance.PlayZombieHurtSfx(transform.position);
            if (Health <= 0)
            {
                Health = 0;
                Die();
            }

            OnEntityTakeDamagedTriggered();
            Invoke(nameof(ResetCanTakeDamaged), _takeDamageCooldown);
        }

        public override void Die()
        {
            Debug.Log("zombie die");
            IsAlive = false;
            _anim.SetTrigger(_dieAnimHash);
            AudioManager.Instance.PlayZombieDeadlSfx(transform.position);
          
            HeadLookTarget.position = HeadLookTarget.position + HeadLookTarget.forward;

            Destroy(this.gameObject, 1.0f);
            OnEntityDiedTriggered();
        }
        #endregion


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
            float knocbackForce = 5;
            atkDirection *= knocbackForce;
            atkDirection.y = 1.65f;
            Rigidbody.AddForce(atkDirection, ForceMode.Impulse);
        }



        #region Reset state
        private void ResetCanTakeDamaged()
        {
            _canTakeDamaged = true;
        }
        #endregion

    }
}
