using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PixelMiner.Core
{
    public class Floater : MonoBehaviour
    {
        private Rigidbody _rb;
        private Main _main;
        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            if (_rb == null)
            {
                Debug.LogError("Missing rigidbody reference.");
            }
        }

        private void Start()
        {
            _main = Main.Instance;
        }
        private void FixedUpdate()
        {
            Vector3 waterPushForce = _main.GetWaterPushForce(transform.position);
            _rb.AddForce(waterPushForce * 2f * UnityEngine.Time.fixedDeltaTime, ForceMode.Force);
        }
    }
}
