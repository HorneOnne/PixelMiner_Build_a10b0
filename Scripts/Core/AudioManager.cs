using UnityEngine;
using PixelMiner.Enums;
using FMODUnity;
using FMOD.Studio;
namespace PixelMiner.Core
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }
        private Main _main;

        [Header("Volume")]
        [Range(0f, 1f)]
        private float _masterVolume = 0.5f;
        [Range(0f, 1f)]
        private float _musicVolume = 0.5f;
        [Range(0f, 1f)]
        private float _soundVolume = 0.5f;
        private Bus _masterBus;
        private Bus _musicBus;
        private Bus _soundBus;




        //--------------------------------------------------------------------
        // 1: Using the EventReference type will present the designer with
        //    the UI for selecting events.
        //--------------------------------------------------------------------

        [Header("Music")]
        [SerializeField] private FMODUnity.EventReference _music;

        [Header("SFX")]
        [SerializeField] private FMODUnity.EventReference _step;
        [SerializeField] private FMODUnity.EventReference _dig;
        [SerializeField] private FMODUnity.EventReference _glass;
        [SerializeField] private FMODUnity.EventReference _itemPickup;
        [SerializeField] private FMODUnity.EventReference _playerHit;
        [SerializeField] private FMODUnity.EventReference _select;
        [SerializeField] private FMODUnity.EventReference _lavaparticle;


        // Zombie
        [SerializeField] private FMODUnity.EventReference _zombieGrowl;
        [SerializeField] private FMODUnity.EventReference _zombieHurt;
        [SerializeField] private FMODUnity.EventReference _zombieDead;





        //--------------------------------------------------------------------
        // 2: Using the EventInstance class will allow us to manage an event
        //    over its lifetime, including starting, stopping and changing 
        //    parameters.
        //--------------------------------------------------------------------
        private EventInstance _musicEventInstance;



        #region Properties
        public float MasterVolume { get => _masterVolume; }
        public float MusicVolume { get => _musicVolume; }
        public float SoundVolume { get => _soundVolume; }
        public float DefaultMasterVolume { get ; } = 1.0f;
        public float DefaultMusicVolume { get; } = 0.5f;
        public float DefaultSoundVolume { get; } = 0.5f;
        #endregion

        private void Awake()
        {
            if(Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(this.gameObject);
            }
         

            _masterBus = RuntimeManager.GetBus("bus:/");
            _musicBus = RuntimeManager.GetBus("bus:/Music");
            _soundBus = RuntimeManager.GetBus("bus:/Sound");
        }

        private void Start()
        {
            _main = Main.Instance;
            PlayMusic(_music);
        }

        private void OnDestroy()
        {
            StopMusic();
        }


        #region Volume
        public void SetMasterVolume(float volume)
        {
            _masterVolume = volume;
            _masterBus.setVolume(_masterVolume);
        }
        public void SetMusicVolume(float volume)
        {
            _musicVolume = volume;
            _musicBus.setVolume(_musicVolume);
        }
        public void SetSoundVolume(float volume)
        {
            _soundVolume = volume;
            _soundBus.setVolume(_soundVolume);
        }
        #endregion


        private void PlayMusic(EventReference musicEventReference)
        {
            _musicEventInstance = CreateInstance(musicEventReference);
            _musicEventInstance.start();
        }

        private void StopMusic()
        {
            _musicEventInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        }

        public EventInstance CreateInstance(EventReference eventReference)
        {
            EventInstance instance = RuntimeManager.CreateInstance(eventReference);
            return instance;
        }



        public void PlayStepSfx(Vector3 footGPosition)
        {
            BlockID blockID = _main.GetBlock(new Vector3(footGPosition.x, footGPosition.y - 0.999f, footGPosition.z));
            string fmodParameterLabel;

            switch (blockID)
            {
                default:
                case BlockID.Dirt:
                case BlockID.Ice:
                    fmodParameterLabel = "cloth";
                    break;
                case BlockID.DirtGrass:
                    fmodParameterLabel = "grass";
                    break;
                case BlockID.Sand:
                    fmodParameterLabel = "sand";
                    break;
                case BlockID.Wood:
                case BlockID.PineWood:
                    fmodParameterLabel = "wood";
                    break;
                case BlockID.SnowDirtGrass:
                    fmodParameterLabel = "snow";
                    break;

            }

            FMOD.Studio.EventInstance stepSfxInstance = RuntimeManager.CreateInstance(_step);
            stepSfxInstance.set3DAttributes(RuntimeUtils.To3DAttributes(footGPosition));
            stepSfxInstance.setParameterByNameWithLabel("BlockID", fmodParameterLabel);
            stepSfxInstance.start();
            stepSfxInstance.release();
        }

        public void PlayDiggingSfx(Vector3 globalPosition)
        {
            FMOD.Studio.EventInstance stepSfxInstance = RuntimeManager.CreateInstance(_dig);
            stepSfxInstance.set3DAttributes(RuntimeUtils.To3DAttributes(globalPosition));
            stepSfxInstance.setParameterByNameWithLabel("BlockID", "cloth");
            stepSfxInstance.start();
            stepSfxInstance.release();
        }

        public void PlayEndDigSfx(Vector3 globalPosition)
        {
            BlockID blockID = _main.GetBlock(globalPosition);
            string fmodParameterLabel;

            switch (blockID)
            {
                default:
                case BlockID.Dirt:
                    fmodParameterLabel = "cloth";
                    break;
                case BlockID.Grass:
                case BlockID.TallGrass:
                case BlockID.DirtGrass:
                case BlockID.Shrub:
                    fmodParameterLabel = "grass";
                    break;
                case BlockID.Sand:
                    fmodParameterLabel = "sand";
                    break;
                case BlockID.Wood:
                case BlockID.PineWood:
                    fmodParameterLabel = "wood";
                    break;
                case BlockID.SnowDirtGrass:
                    fmodParameterLabel = "snow";
                    break;
                case BlockID.Ice:
                case BlockID.Light:
                    fmodParameterLabel = "glass";
                    break;

            }

            FMOD.Studio.EventInstance stepSfxInstance = RuntimeManager.CreateInstance(_dig);
            stepSfxInstance.set3DAttributes(RuntimeUtils.To3DAttributes(globalPosition));
            stepSfxInstance.setParameterByNameWithLabel("BlockID", fmodParameterLabel);
            stepSfxInstance.getPitch(out float pitch);
            stepSfxInstance.setPitch(pitch + pitch * 0.2f);
            stepSfxInstance.start();
            stepSfxInstance.release();
        }
        public void PlayEndDigSfx(Vector3 globalPosition, BlockID blockID)
        {
            string fmodParameterLabel;

            switch (blockID)
            {
                default:
                case BlockID.Dirt:
                    fmodParameterLabel = "cloth";
                    break;
                case BlockID.Grass:
                case BlockID.TallGrass:
                case BlockID.DirtGrass:
                case BlockID.Shrub:
                    fmodParameterLabel = "grass";
                    break;
                case BlockID.Sand:
                    fmodParameterLabel = "sand";
                    break;
                case BlockID.Wood:
                case BlockID.PineWood:
                    fmodParameterLabel = "wood";
                    break;
                case BlockID.SnowDirtGrass:
                    fmodParameterLabel = "snow";
                    break;
                case BlockID.Ice:
                case BlockID.Light:
                    fmodParameterLabel = "glass";
                    break;

            }

            FMOD.Studio.EventInstance stepSfxInstance = RuntimeManager.CreateInstance(_dig);
            stepSfxInstance.set3DAttributes(RuntimeUtils.To3DAttributes(globalPosition));
            stepSfxInstance.setParameterByNameWithLabel("BlockID", fmodParameterLabel);
            stepSfxInstance.getPitch(out float pitch);
            stepSfxInstance.setPitch(pitch + pitch * 0.2f);
            stepSfxInstance.start();
            stepSfxInstance.release();
        }


        public void PlayDigSfx(Vector3 globalPosition, string fmodParameterLabel)
        {
            FMOD.Studio.EventInstance stepSfxInstance = RuntimeManager.CreateInstance(_dig);
            stepSfxInstance.set3DAttributes(RuntimeUtils.To3DAttributes(globalPosition));
            stepSfxInstance.setParameterByNameWithLabel("BlockID", fmodParameterLabel);
            stepSfxInstance.start();
            stepSfxInstance.release();
        }

        public void PlayGlassSfx(Vector3 globalPosition)
        {
            FMOD.Studio.EventInstance stepSfxInstance = RuntimeManager.CreateInstance(_glass);
            stepSfxInstance.set3DAttributes(RuntimeUtils.To3DAttributes(globalPosition));
            stepSfxInstance.start();
            stepSfxInstance.release();
        }

        public void PlayPlayerHitSfx(Vector3 globalPosition)
        {
            FMOD.Studio.EventInstance stepSfxInstance = RuntimeManager.CreateInstance(_playerHit);
            stepSfxInstance.set3DAttributes(RuntimeUtils.To3DAttributes(globalPosition));
            stepSfxInstance.start();
            stepSfxInstance.release();
        }

        public void PlayItemPickupfx(Vector3 globalPosition)
        {
            FMOD.Studio.EventInstance stepSfxInstance = RuntimeManager.CreateInstance(_itemPickup);
            stepSfxInstance.set3DAttributes(RuntimeUtils.To3DAttributes(globalPosition));
            stepSfxInstance.start();
            stepSfxInstance.release();
        }


        public void PlaySelectSfx()
        {
            FMODUnity.RuntimeManager.PlayOneShot(_select);
        }

        #region Zombie
        public void PlayZombieGrowlSfx(Vector3 globalPosition)
        {
            FMOD.Studio.EventInstance stepSfxInstance = RuntimeManager.CreateInstance(_zombieGrowl);
            stepSfxInstance.set3DAttributes(RuntimeUtils.To3DAttributes(globalPosition));
            stepSfxInstance.start();
            stepSfxInstance.release();
        }

        public void PlayZombieHurtSfx(Vector3 globalPosition)
        {
            FMOD.Studio.EventInstance stepSfxInstance = RuntimeManager.CreateInstance(_zombieHurt);
            stepSfxInstance.set3DAttributes(RuntimeUtils.To3DAttributes(globalPosition));
            stepSfxInstance.start();
            stepSfxInstance.release();
        }

        public void PlayZombieDeadlSfx(Vector3 globalPosition)
        {
            FMOD.Studio.EventInstance stepSfxInstance = RuntimeManager.CreateInstance(_zombieDead);
            stepSfxInstance.set3DAttributes(RuntimeUtils.To3DAttributes(globalPosition));
            stepSfxInstance.start();
            stepSfxInstance.release();
        }


        public void PlayLavaParticle(Vector3 globalPosition)
        {
            FMODUnity.RuntimeManager.PlayOneShot(_lavaparticle, globalPosition);
        }
        #endregion
    }
}
