using UnityEngine;
using PixelMiner.Extensions;
namespace PixelMiner.Core
{
    public class IlluminateObject : MonoBehaviour
    {
        [SerializeField] private Renderer[] _renderers;
        private Material[] _mats;
        private Main _main;

        private int _ambientLight;
        private Color _lightColor;
        private float _offsetY = 0.1f;

        // Check move condition
        private Vector3 _prevPosition;
        private byte _simulate = 1; // 0 represents false, 1 represents true

        private void Start()
        {
            _renderers = GetComponentsInChildren<Renderer>();
            _prevPosition = transform.position;
            _main = Main.Instance;
            if (_renderers == null || _renderers.Length == 0)
            {
                Debug.Log("Not found renderers");
            }
            _mats = new Material[_renderers.Length];
            for (int i = 0; i < _renderers.Length; i++)
            {
                _mats[i] = _renderers[i].sharedMaterial;
            }

            UpdateLightMats();
        }

        private void FixedUpdate()
        {
            if ((_simulate & 1) == 0) // _simulate is false
            {             
                return;
            }

            UpdateLightMats();
        }

        private void UpdateLightMats()
        {

            Vector3Int blockPosition = new Vector3Int(
                Mathf.FloorToInt(transform.position.x),
                Mathf.FloorToInt(transform.position.y + _offsetY),
                Mathf.FloorToInt(transform.position.z));

            if (_main.TryGetChunk(blockPosition, out var chunk))
            {
                Vector3Int relPosition = chunk.GetRelativePosition(blockPosition);

                chunk.GetBlockLight(relPosition, out byte red, out byte green, out byte blue);
                float redChannel = red / (float)LightUtils.MAX_LIGHT_INTENSITY;
                float greenChannel = green / (float)LightUtils.MAX_LIGHT_INTENSITY;
                float blueChannel = blue / (float)LightUtils.MAX_LIGHT_INTENSITY;
                _lightColor = new Color(redChannel, greenChannel, blueChannel, 1.0f);

                _ambientLight = chunk.GetAmbientLight(relPosition);

                for (int i = 0; i < _mats.Length; i++)
                {
                    _mats[i].SetColor("_LightColor", _lightColor);
                    _mats[i].SetFloat("_AmbientLightValue", _ambientLight / (float)LightUtils.MAX_LIGHT_INTENSITY);
                }
            }
        }


        public void Simulate(bool simulate)
        {
            this._simulate = System.Convert.ToByte(simulate);
        }

        
     
    }
}