using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PixelMiner.UI
{
    public class UICharacter : MonoBehaviour
    {
        [SerializeField] private Transform _lookTarget;
        public Transform Targetobject;
        [SerializeField] private Camera _uiCam;

        public Vector2 OffsetFromEyes;
        [SerializeField] private Vector2 _offsetRenderTexFromCenter = default;
        private RectTransform _characterRenderTex;

        private void Awake()
        {
            _characterRenderTex = GameObject.FindGameObjectWithTag("UICharacter").GetComponent<RectTransform>();
            if(_characterRenderTex == null)
            {
                Debug.LogError("Missing _characterRenderTex reference.");
            }
            else
            {
                _offsetRenderTexFromCenter = _characterRenderTex.anchoredPosition;
            }
            if (_lookTarget == null )
            {
                Debug.LogError("Missing _lookTarget reference.");
            }
            OffsetFromEyes = _lookTarget.transform.position - Vector3.zero;
        }

   
        private void Update()
        {
            LookAtMousePosition();
        }

        private void LookAtMousePosition()
        {
            Vector2 mousePos = Input.mousePosition;
            float aspectRatio = (float)Screen.width / Screen.height;
            Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);
            Vector2 distanceFromCenter = (mousePos + OffsetFromEyes - _offsetRenderTexFromCenter) - screenCenter;

            float maxXDistance;
            float maxYDistance;

            if (aspectRatio > 0)
            {
                maxXDistance = Screen.width / 2f;
                maxYDistance = (Screen.height / 2f) * aspectRatio;
            }
            else
            {
                maxXDistance = (Screen.width / 2f) * aspectRatio;
                maxYDistance = Screen.height / 2f;
            }


            float normalizedX = distanceFromCenter.x / maxXDistance;
            float normalizedY = distanceFromCenter.y / maxYDistance;

            var direction = new Vector3(normalizedX, normalizedY, Targetobject.transform.position.z) - Targetobject.transform.position;
            if(direction.sqrMagnitude > 0.01f)
            {
                float lookSpeed = 3f;
                Targetobject.transform.position += direction * UnityEngine.Time.deltaTime * lookSpeed;
                _lookTarget.position = Targetobject.position;
            }

            //Targetobject.transform.position = new Vector3(normalizedX, normalizedY, Targetobject.transform.position.z);
            //_lookTarget.position = Targetobject.position;
        }

  

    }
}
