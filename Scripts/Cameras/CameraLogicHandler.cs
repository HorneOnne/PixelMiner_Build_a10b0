using UnityEngine;
using Cinemachine;
using PixelMiner.InputSystem;
using PixelMiner.Core;
using System;

namespace PixelMiner.Cam
{
    public class CameraLogicHandler : MonoBehaviour
    {
        public static CameraLogicHandler Instance {  get; private set; }    

        private InputHander _input;
        [SerializeField] private CinemachineVirtualCamera _isometricCam;
        [SerializeField] private CinemachineVirtualCamera _topDownCam;

        public float CurrentYRotAngle { get; private set; }
        public UnityEngine.Camera MainCam{get; private set;}
  

        private void Awake()
        {
            Instance = this;
        }

        private void OnEnable()
        {
            CameraSwitcher.Register(_isometricCam);
            CameraSwitcher.Register(_topDownCam);
        }

        private void OnDisable()
        {
            CameraSwitcher.Unregister(_isometricCam);
            CameraSwitcher.Unregister(_topDownCam);
        }

        void Start()
        {
          
            MainCam = UnityEngine.Camera.main;
            if (_isometricCam == null)
            {
                // Try to find virtual camera.
                _isometricCam = FindFirstObjectByType<CinemachineVirtualCamera>();
                if ( _isometricCam == null )
                {
                    Debug.LogError("Virtual camera is null. Please assign a virtual camera.");
                }                 
            }


          

            if(_input == null)
            {
                try
                {
                    _input = InputHander.Instance;
                    _input.OnRotateDetected += OnRotate;
                }
                catch
                {

                }           
            }
            


   
            Main.Instance.OnCharacterInitialize += SetupCamera;
        }

   

        private void OnDestroy()
        {
            if (_input != null)
            {
                _input.OnRotateDetected -= OnRotate;
            }

            Main.Instance.OnCharacterInitialize -= SetupCamera;
        }

        private void SetupCamera()
        {
            _topDownCam.Follow = Main.Instance.Players[0].transform;
            _isometricCam.Follow = Main.Instance.Players[0].transform;
            CameraSwitcher.SwitchCamera(_isometricCam);
            CurrentYRotAngle = CameraSwitcher.ActiveCam.transform.eulerAngles.y;
        }

        private void OnRotate()
        {
            Debug.Log("On rotate");
            if (_input.Rot == -1)
            {
                CurrentYRotAngle += 45;
            }
            else if (_input.Rot == 1)
            {
                CurrentYRotAngle -= 45;
            }

            // Clamp the _currentYRotAngle within the range -360 to 360
            CurrentYRotAngle = (CurrentYRotAngle + 360) % 360;
            if (CurrentYRotAngle > 180)
            {
                CurrentYRotAngle -= 360;
            }

            if (CurrentYRotAngle % 90 == 0)
            {
                //CameraSwitcher.SwitchCamera(_topDownCam);
                CameraSwitcher.ActiveCam.transform.eulerAngles = new Vector3(30, CurrentYRotAngle, 0);
            }
            else
            {
                //CameraSwitcher.SwitchCamera(_isometricCam);
                CameraSwitcher.ActiveCam.transform.eulerAngles = new Vector3(30, CurrentYRotAngle, 0);
            }
        }


        public enum CameraViewStyle
        {
            TopDownPerspective, IsometricOrthographic
        }
    }
}
