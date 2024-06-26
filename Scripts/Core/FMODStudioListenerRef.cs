using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;
using FMOD;
using System;

namespace PixelMiner.Core
{
    [RequireComponent(typeof(StudioListener))]
    public class FMODStudioListenerRef : MonoBehaviour
    {
        private StudioListener _listener;

        private void Start()
        {
            _listener = GetComponent<StudioListener>();

            Main.Instance.OnCharacterInitialize += SetupCharacterAttenuation;
           
        }
        private void OnDestroy()
        {
            Main.Instance.OnCharacterInitialize -= SetupCharacterAttenuation;
        }

        private void SetupCharacterAttenuation()
        {
            if (Main.Instance.Players.Count > 0)
            {
                _listener.attenuationObject = Main.Instance.Players[0].transform.gameObject;
            }
        }
    }
}
