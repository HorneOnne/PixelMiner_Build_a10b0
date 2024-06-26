using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PixelMiner.Time;

namespace PixelMiner.Core
{
    public class DayNightCycle : MonoBehaviour
    {
        public static DayNightCycle Instance { get; private set; }  
        //public static event System.Action OnTimesOfTheDayChanged;

        private WorldTime _worldTime;
        public AnimationCurve SunLightIntensityCurve;



        private void Awake()
        {
            Instance = this;
        }

        private void OnEnable()
        {
         
        }
        private void OnDisable()
        {
 
        }

        private void Start()
        {
            _worldTime = WorldTime.Instance;
        }

        private void Update()
        {
            float f = _worldTime.Hours + (_worldTime.Minutes / 60f);
            Shader.SetGlobalFloat("_AmbientLightIntensity", CalculateSunlightIntensity(_worldTime.Hours + _worldTime.Minutes / 60f, SunLightIntensityCurve));      
        }


        public float CalculateSunlightIntensity(float hour, AnimationCurve sunLightIntensityCurve)
        {
            return sunLightIntensityCurve.Evaluate(hour / 24.0f);
        }

        public float AmbientlightIntensity { get => CalculateSunlightIntensity(_worldTime.Hours + _worldTime.Minutes / 60f, SunLightIntensityCurve); }
    }
}
