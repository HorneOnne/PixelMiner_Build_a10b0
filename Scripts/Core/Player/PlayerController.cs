using PixelMiner.Utilities;
using UnityEngine;
using PixelMiner.Miscellaneous;
using PixelMiner.InputSystem;
using PixelMiner.Enums;
using PixelMiner.Extensions;


namespace PixelMiner.Core
{
    public class PlayerController : MonoBehaviour
    {
        private InputHander _input;
        private Main _main;
        private Animator _anim;
        private Player _player;
        [SerializeField] private Transform _model;
        private Rigidbody _rb;

        [SerializeField] private float _moveSpeed;
        private float _currMoveSpeed;
        [SerializeField] private float _digMoveSpeed;
        [SerializeField] private Vector3 _moveDirection;
        private Vector3 _cameraIsometricRot = new Vector3(0, 45, 0);


        // Aiming
        private Vector3 _currentBCheckPos;
        private Vector3 _inputLookDir;
        private readonly Vector3 _upperHeadLookOffset = new Vector3(0, 0, 0);
        private readonly Vector3 _middleHeadLookOffset = new Vector3(0, -0.1f, 0);
        private readonly Vector3 _lowerHeadLookOffset = new Vector3(0, -0.25f, 0);
        private Vector3 _currentHeadLookOffset;

        public Vector3 LookDirection { get; private set; }
        private Vector3 _forwardPosition;
        private Vector3 _lookPosition;
        public float CurrentVerticalLookAngle = 0;  // Angle in degrees
        public float CurrentHorizontalLookAngle = 0;  // Angle in degrees
        [SerializeField] private float _verticalSensitive = 20f;


        // Physics
        [SerializeField] private LayerMask _environmentLayer;
        [SerializeField] private float _jumpForce = 10f;
        [SerializeField] private bool _canJump = true;
        [SerializeField] private Color _boundColor;

        public Vector3 CurrentVel;

        // Animation
        private bool _hasAnimator;
        // animation IDs
        private int _animIDVelocity;



        #region Bounds
        [SerializeField] private Transform _lowerBoundsCenter;
        [SerializeField] private Transform _upperBoundsCenter;
        private Vector3 _lowerBoundHalfSize = new Vector3(0.3f,0.05f, 0.3f);
        private Vector3 _upperBoundHalfSize = new Vector3(0.3f, 0.05f, 0.3f);
        #endregion

        float _minVelY = -10;
        float _maxVelY = 10;
        // Detect player position in world.
        private Vector3Int _currentPosition;
        private Vector3Int _prevPosition;
        private BlockID _currentStandingBlock;
        private BlockID _prevStandingBlock;


        #region Properties
        public Rigidbody Rigidbody { get => _rb; }
        public float MoveSpeed { get => _moveSpeed; }
        #endregion


        private void Awake()
        {
            _anim = GetComponentInChildren<Animator>();
            _hasAnimator = _anim != null;
            _rb = GetComponent<Rigidbody>();
        }


        private void Start()
        {       
            _player = GetComponent<Player>();
            _input = InputHander.Instance;
            _main = Main.Instance;
            AssignAnimationIDs();

            _currentPosition = transform.position.ToVector3Int();
            _prevPosition = _currentPosition;
            WorldLoading.Instance.OnLoadingGameFinish += EnableGravity;
        }

        private void OnDestroy()
        {
            WorldLoading.Instance.OnLoadingGameFinish -= EnableGravity;
        }



        public Vector3 originalVector = new Vector3(1f, 0f, 0f);
        public Vector3 CurrRotVector = new Vector3(1f, 0f, 0f);
        // Rotation angle (positive for right, negative for left)
        public float rotationAngle = 45f;

        Vector3 RotateVectorAroundY(Vector3 vector, float angle)
        {
            // Create a quaternion for rotation around Y-axis
            Quaternion rotationQuaternion = Quaternion.Euler(0f, angle * UnityEngine.Time.deltaTime, 0f);

            // Rotate the vector using the quaternion
            Vector3 rotatedVector = rotationQuaternion * vector;

            return rotatedVector;
        }


        private void Update()
        {
      
            CurrentVel = _rb.velocity;

            Vector3 worldForward = transform.TransformDirection(Vector3.forward);
            //CurrentLookAngle += _input.LookVertical * _verticalSensitive * UnityEngine.Time.deltaTime;
            CurrentHorizontalLookAngle = _input.LookHorizontal.x * _verticalSensitive;
            CurrentVerticalLookAngle = _input.LookHorizontal.y * _verticalSensitive;
            float maxAngle = 90;
            if (CurrentHorizontalLookAngle > maxAngle) CurrentHorizontalLookAngle = maxAngle;
            if (CurrentHorizontalLookAngle < -maxAngle) CurrentHorizontalLookAngle = -maxAngle;
            if (CurrentVerticalLookAngle > maxAngle) CurrentVerticalLookAngle = maxAngle;
            if (CurrentVerticalLookAngle < -maxAngle) CurrentVerticalLookAngle = -maxAngle;

            //if (Simulate)
            //{
            //    _entity.Simulate = Simulate;
            //}

            if (_input.Fire1 && 
                !_input.IsPointerOverUIElement())
            {
                //_currMoveSpeed = _digMoveSpeed;
            }
            else if (_input.LookHorizontal != Vector2.zero)
            {
                //_currMoveSpeed = _digMoveSpeed;
            }
            else
            {
                //_currMoveSpeed = _moveSpeed;
            }
            _currMoveSpeed = _moveSpeed;



            // Aiming
            // =======
            _inputLookDir = new Vector3(0, _input.LookHorizontal.y, 0).normalized;
            if (_inputLookDir.y > 0)
            {
                _player.CurrentBCheckTrans = _player.UpperBCheckTrans;
                _currentHeadLookOffset = _upperHeadLookOffset;
            }
            else if (_inputLookDir.y < 0)
            {
                _player.CurrentBCheckTrans = _player.LowerBCheckTrans;
                _currentHeadLookOffset = _lowerHeadLookOffset;
            }
            else
            {
                _player.CurrentBCheckTrans = _player.MiddleBCheckTrans;
                _currentHeadLookOffset = _middleHeadLookOffset;
            }
            _currentBCheckPos = _player.CurrentBCheckTrans.position;


            _forwardPosition = _currentBCheckPos + _player.Model.TransformDirection(Vector3.forward);
            _lookPosition = _forwardPosition;
            LookDirection = _lookPosition - _currentBCheckPos;



            // Look
            UpdateRotation(new Vector3(_input.Move.x, 0, _input.Move.y).ToGameDirection());
            //if (_input.LookHorizontal != Vector2.zero)
            //{
            //    UpdateRotation(new Vector3(_input.LookHorizontal.x, 0, _input.LookHorizontal.y).ToGameDirection());
            //}
            //else
            //{
            //    if (_input.Move != Vector2.zero)
            //    {
            //        UpdateRotation(new Vector3(_input.Move.x, 0, _input.Move.y).ToGameDirection());
            //    }
            //}


           



            // Animation
            // =========
            if (_hasAnimator)
            {
                if (_input.Fire1 == true && !_input.IsPointerOverUIElement())
                {
                    //_anim.SetFloat(_animIDVelocity, Mathf.Min(_input.Move.magnitude, 0.3f));
                }
                else
                {
                    //_anim.SetFloat(_animIDVelocity, _input.Move.magnitude);
                }
                _anim.SetFloat(_animIDVelocity, _input.Move.magnitude);
            }
        }

        private void FixedUpdate()
        {
            _currentPosition = transform.position.ToVector3Int();
            if(_currentPosition != _prevPosition)
            {
                _prevPosition = _currentPosition;
                _currentStandingBlock = GetBlockPlayerStanding();

                if(_currentStandingBlock != _prevStandingBlock)
                {
                    if (_prevStandingBlock == BlockID.Water)
                        RemoveWaterDrag();
                    //if(_currentStandingBlock == BlockID.Water)
                    //    Invoke(nameof(AppliedWaterDrag), 0.2f);

                    _prevStandingBlock = _currentStandingBlock;
                }
            }


            UpdatePosition(_currMoveSpeed);

            
            if(_currentStandingBlock == Enums.BlockID.Water)
            {
                if(_rb.drag == 0)
                {
                    _rb.drag = 1;
                    Invoke(nameof(AppliedWaterDrag), 0.2f);
                }

                //Debug.Log("Below is water");
                Vector3 waterPushForce = Main.Instance.GetWaterPushForce(transform.position);
                //Debug.Log(waterPushForce);
                _rb.AddForce(waterPushForce * 10, ForceMode.Force);


                BlockID blockBelow = _main.GetBlock(new Vector3(transform.position.x, transform.position.y - 1.0f, transform.position.z));
                BlockID blockAbove = _main.GetBlock(new Vector3(transform.position.x, transform.position.y + 1.0f, transform.position.z));

                if (blockAbove == BlockID.Water)
                {
                    _rb.drag = 0;
                    _rb.AddForce(new Vector3(0f, Mathf.Abs(Physics.gravity.y) * 50f, 0f) * UnityEngine.Time.fixedDeltaTime, ForceMode.Force);

                }
 


            }

            // Jump
            if (_input.Jump)
            {
                JumpLogicHandler();
            }


           
            if(_rb.velocity.y > _maxVelY) _rb.velocity = new Vector3(_rb.velocity.x, _maxVelY, _rb.velocity.z);
            if(_rb.velocity.y < _minVelY) _rb.velocity = new Vector3(_rb.velocity.x, _minVelY, _rb.velocity.z);
           
        }

        private void LateUpdate()
        {
            DrawBounds.Instance.AddBounds(_player.Bounds, Color.green);
            //DrawBounds.Instance.AddRay(_currentBCheckPos, _lookPosition - _currentBCheckPos, Color.yellow);

            DrawBounds.Instance.AddRay(_player.LineOfSignTrans.position, _player.LineOfSignTrans.forward, Color.yellow, 2f);
        }

        public void JumpLogicHandler()
        {
            if (OnGround())
            {
                if (_canJump)
                {
                    _canJump = false;
                    if (_currentStandingBlock != BlockID.Water)
                    {
                        //_entity.TryAddVelocity(new Vector3(0, _jumpForce, 0));
                        _rb.velocity = Vector3.up * _jumpForce * UnityEngine.Time.deltaTime;
                        //_canJump = false;
                        //Invoke(nameof(ResetJump), 0.25f);
                    }
                    else if (_currentStandingBlock == BlockID.Water)
                    {
                        if (_main.GetBlock(new Vector3(transform.position.x, transform.position.y + 0.75f, transform.position.z)) == BlockID.Water)
                        {
                            _rb.drag = 0;
                            _rb.velocity = Vector3.up * _jumpForce * UnityEngine.Time.deltaTime;
                            //_canJump = false;
                            //Invoke(nameof(ResetJump), 0.25f);
                        }
                    }
                    Invoke(nameof(ResetJump), 0.25f);
                }
            }

         
        }

        private void EnableGravity()
        {
            _rb.useGravity = true;
        }

        private void AppliedWaterDrag()
        {
            int waterLevel = Main.Instance.GetLiquidLevel(transform.position);
            float minDrag = 0f;
            float maxDrag = 10f;
            float waterDragAppliedToBody = Mathf.Lerp(minDrag, maxDrag, waterLevel / (float)Water.MAX_WATER_LEVEL);
            _rb.drag = waterDragAppliedToBody;
        }

        private void RemoveWaterDrag()
        {
            _rb.drag = 0;
        }

        private void UpdatePosition(float speed)
        {
            if (_input.Move != Vector2.zero)// && _input.Fire1 == false)
            {
                _moveDirection = new Vector3(_input.Move.x * speed, 0, _input.Move.y * speed);
                Vector3 _rotMoveDir = _moveDirection.ToGameDirection();
                _rb.velocity = new Vector3(_rotMoveDir.x, _rb.velocity.y, _rotMoveDir.z);

                _rb.AddForce(new Vector3(_rotMoveDir.x, _rb.velocity.y, _rotMoveDir.z), ForceMode.Force);
      
                //_entity.SetVelocity(new Vector3(_rotMoveDir.x, _entity.Velocity.y, _rotMoveDir.z));
            }
            else
            {
                _rb.velocity = new Vector3(0, _rb.velocity.y,0);
                //_entity.SetVelocity(new Vector3(0, _entity.Velocity.y, 0));
            }
        }


        private Collider[] _groundHit = new Collider[1];
        public bool OnGround()
        {         
            if (UnityEngine.Physics.OverlapBoxNonAlloc(_lowerBoundsCenter.position, _lowerBoundHalfSize, _groundHit, Quaternion.identity, _environmentLayer) > 0)
            {
                return true;
            }

            return false;
        }

        private void UpdateRotation(Vector3 gameDirection)
        {
            RotateTowardMoveDirection(gameDirection);
        }


        private void RotateTowardMoveDirection(Vector3 direction)
        {
            if (direction.sqrMagnitude >= 0.1f)
            {
                // Calculate the rotation to look towards the move direction
                Quaternion lookRotation = Quaternion.LookRotation(direction, Vector3.up);

                // Apply the rotation to the character
                _model.rotation = Quaternion.Slerp(_model.rotation, lookRotation, UnityEngine.Time.deltaTime * 10f);
            }
        }

        private Enums.BlockID GetBlockPlayerStanding()
        {
            return _main.GetBlock(transform.position);
        }


        private void AssignAnimationIDs()
        {
            _animIDVelocity = Animator.StringToHash("Velocity");

        }


        #region Reset state
        private void ResetJump()
        {
            _canJump = true;
        }

    
        #endregion




        private void OnDrawGizmos()
        {
            if(Application.isPlaying)
            {
                Gizmos.color = _boundColor;
                Gizmos.DrawWireCube(_lowerBoundsCenter.position, _lowerBoundHalfSize * 2);
                Gizmos.DrawWireCube(_upperBoundsCenter.position, _upperBoundHalfSize * 2);

            }
        }

    }

}
