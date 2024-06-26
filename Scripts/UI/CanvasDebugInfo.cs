using PixelMiner.Enums;
using PixelMiner.Time;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using PixelMiner.Core;
using System;

namespace PixelMiner.UI
{
    public class CanvasDebugInfo : CustomCanvas
    {
        [SerializeField] private TextMeshProUGUI _debugLogText;
        float deltaTime = 0.0f;
        float msec;
        float fps;
        private Vector3Int _targetPosition;
        private Vector3Int _preTargetPosition;
        private BlockID _targetBlock;
        private ushort _lightData;
        private byte _waterLevel;

        private StringBuilder sb = new StringBuilder();
        private WorldTime _worldTime;

        //
        private string _fpsString;
        private string _targetString;
        private string _preTargetString;
        private string _blockString;
        private string _blockLightString;
        private string _ambientLightString;
        private string _waterLevelString;
        private string _worldTimeString;

        private Transform _playerTrans;
        
        private void OnEnable()
        {
            Tool.OnTarget += UpdateTarget;
        }

        private void OnDisable()
        {
            Tool.OnTarget -= UpdateTarget;
        }

        private void Start()
        {
        
            InvokeRepeating(nameof(UpdateLogText), 1.0f, 0.02f);
            _worldTime = WorldTime.Instance;

            Main.Instance.OnCharacterInitialize += SetupPlayer;
        }

        private void OnDestroy()
        {
            Main.Instance.OnCharacterInitialize -= SetupPlayer;
        }
        private void SetupPlayer()
        {
            _playerTrans = GameObject.FindWithTag("Player").transform;
        }

        private void UpdateLogText()
        {
            _fpsString = string.Format("FPS: {0:F2}  ({1:F2} m/s)", fps, msec);
            _targetString = $"Target: {_targetPosition}";
            _preTargetString = $"Target: {_preTargetString}";
            _blockString = $"Block: {Main.Instance.GetBlock(_playerTrans.position)}";
            _blockLightString = $"Block Light:  Red {Main.Instance.GetRedLight(_playerTrans.position)}   " +
                $"Green {Main.Instance.GetGreenLight(_playerTrans.position)}   " +
                $"Blue {Main.Instance.GetBlueLight(_playerTrans.position)}";
            _ambientLightString = $"Ambient Light: {Main.Instance.GetAmbientLight(_playerTrans.position)}";
            _waterLevelString = $"Water Level: {_waterLevel}";

            UpdateWorldTime();

            sb.Clear();
            sb.AppendLine(_fpsString);
            sb.AppendLine(_targetString);
            sb.AppendLine(_blockString);
            sb.AppendLine(_blockLightString);
            sb.AppendLine(_ambientLightString);
            sb.AppendLine(_waterLevelString);
            sb.AppendLine(_worldTimeString);
            //sb.AppendLine($"heat: {Main.Instance.GetHeatData(_playerTrans.position)}");

            _debugLogText.text = sb.ToString();
        }

        private void Update()
        {
            if (_playerTrans == null) return;

            deltaTime += (UnityEngine.Time.unscaledDeltaTime - deltaTime) * 0.1f;
            msec = deltaTime * 1000.0f;
            fps = 1.0f / deltaTime;

            //this._ambientLight = Main.Instance.GetAmbientLight(_playerTrans.position);
            //this._blockLight = Main.Instance.GetBlockLight(_playerTrans.position);
            this._lightData = Main.Instance.GetLightData(_playerTrans.position);
            this._waterLevel = Main.Instance.GetLiquidLevel(_playerTrans.position);
        }

        private void UpdateTarget(Vector3Int target, BlockID blockID, byte blockLight, byte ambientLight)
        {
            this._targetPosition = target;
            this._targetBlock = blockID;
            //this._lightData = blockLight;
            //this._ambientLight = ambientLight;
        }

        private void UpdateWorldTime()
        {
            _worldTimeString = $"Hours: ({_worldTime.Hours.ToString("00")}:{_worldTime.Minutes.ToString("00")})  Day({_worldTime.Days})";

        }
    }
}
