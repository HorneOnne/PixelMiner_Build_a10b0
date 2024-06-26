using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace PixelMiner.Core
{
    public class Healthbar : MonoBehaviour
    {
        private Entity _entity;
        private Vector3 _moveOffset;
        private bool _attched;
        private Image _foregroundImage;
        private Camera _mainCam;

        private void Awake()
        {
            _mainCam = Camera.main;
            _attched = false;

            _foregroundImage = transform.Find("Background").transform.Find("Foreground").GetComponent<Image>();
            if (_foregroundImage == null)
            {
                Debug.LogError("Missing Foreground attached to component.");
            }
        }
        private void OnDestroy()
        {
            if (_attched)
            {
                _entity.OnEntityTakeDamaged -= UpdateHealthbar;
            }

        }


        private void LateUpdate()
        {
            if (_attched)
            {
                transform.position = _entity.transform.position + _moveOffset;
            }
            transform.rotation = Quaternion.LookRotation(transform.position - _mainCam.transform.position);
        }


        public void Attach(Entity entity, Vector3 offset)
        { 
            this._entity = entity;
            _moveOffset = offset;
            _attched = true;
            UpdateHealthbar(entity.Health, entity.MaxHealth);
            _entity.OnEntityTakeDamaged += UpdateHealthbar;
            _entity.OnEntityDied += OnEntityDied;
        }

        public void Clear()
        {
            _entity = null;
            _moveOffset = default;
            _attched = false;
            _entity.OnEntityTakeDamaged -= UpdateHealthbar;
        }

        private void UpdateHealthbar(byte health, byte maxHealth)
        {
            _foregroundImage.fillAmount = (float)health / maxHealth;
        }

        private void OnEntityDied()
        {
            Destroy(this.gameObject);
        }
     
    }
}
