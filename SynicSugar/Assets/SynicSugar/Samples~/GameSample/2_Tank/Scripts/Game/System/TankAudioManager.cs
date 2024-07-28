
using UnityEngine;
namespace SynicSugar.Samples.Tank {
    internal enum TankClips {
        Idling, Driving, Explosion
    }
    internal enum ShootingClips{
        Charge, Fire, Explosion
    }
    public class TankAudioManager : MonoBehaviour {
    #region Singleton
        public static TankAudioManager Instance {
            get; private set;
        }
        void Awake(){
            if (Instance != null) {
                Destroy (this.gameObject);
                return;
            }
            Instance = this;
        }
        void OnDestroy(){
            if(Instance == this){
                Instance = null;
            }
        }
    #endregion
        [SerializeField] AudioSource movementSource, shootingSource;
        [SerializeField] Clips clips;
        //Detailed calculation process is too cumbersome, so it is omitted.
        internal void PlayTankClip(TankClips clip){
            // Change the clip and play it.
            movementSource.clip = clips.GetClip(clip);
            movementSource.Play();
        }
        internal void StopTankSource(){
            movementSource.Stop();
        }
        internal void PlayTankClipOneshot(TankClips clip){
            movementSource.PlayOneShot(clips.GetClip(clip));
        }

        internal void PlayShootingClip(ShootingClips clip){
            shootingSource.clip = clips.GetClip(clip);
            shootingSource.Play();
        }
        internal void StopShootingSource(){
            shootingSource.Stop();
        }
        internal void PlayShootingClipOneshot(ShootingClips clip){
            shootingSource.PlayOneShot(clips.GetClip(clip));
        }
    }
    [System.Serializable]
    public sealed class Clips{
        [SerializeField] AudioClip Idling, Driving, TankExplosion, Charge, Fire, ShellExplosion;

        internal AudioClip GetClip(TankClips clip){
            switch(clip){
                case TankClips.Idling:
                    return Idling;
                case TankClips.Driving:
                    return Driving;
                case TankClips.Explosion:
                    return TankExplosion;
            }
            return null;
        }
        internal AudioClip GetClip(ShootingClips clip){
            switch(clip){
                case ShootingClips.Charge:
                    return Charge;
                case ShootingClips.Fire:
                    return Fire;
                case ShootingClips.Explosion:
                    return ShellExplosion;
            }
            return null;
        }


    }
}