using System.Threading;
using Cysharp.Threading.Tasks;
using SynicSugar.P2P;
using UnityEngine;

namespace SynicSugar.Samples.Tank {
    [RequireComponent(typeof(Rigidbody))]
    public class TankShell : MonoBehaviour {
        internal Rigidbody ShellRigidbody { get; private set; }
        internal UserId AttackerID;
        Collider ShellCollider;
        LayerMask m_TankMask;                               // Used to filter what the explosion affects, this should be set to "Players".
        public ParticleSystem m_ExplosionParticles;         // Reference to the particles that will play on explosion.
        public float m_MaxDamage = 100f;                    // The amount of damage done if the explosion is centred on a tank.
        public float m_ExplosionForce = 1000f;              // The amount of force added to a tank at the centre of the explosion.
        public int m_MaxLifeTime = 2000;                    // The time in seconds before the shell is removed.
        public float m_ExplosionRadius = 5f;                // The maximum distance away from the explosion tanks can be and are still affected.
        CancellationTokenSource shellTokenSource;
        ParticleSystem ExplosionEffect;

        void Awake(){
            ShellRigidbody = GetComponent<Rigidbody>(); 
            // Set the value of the layer mask based solely on the Players layer.
            m_TankMask = LayerMask.GetMask("Players");
            ShellCollider = GetComponent<Collider>();
            ShellCollider.enabled = false;
            ExplosionEffect = Instantiate(m_ExplosionParticles, transform.position, transform.rotation);
            gameObject.SetActive(false);
        }
        /// <summary>
        /// Fire shell until lifetime.
        /// </summary>
        /// <returns></returns>
        internal async UniTask FireShell(UserId attackerId, TankShootingData data){
            shellTokenSource = new CancellationTokenSource();
            AttackerID = attackerId;
            EnableShell(data, shellTokenSource.Token).Forget();
            ShellRigidbody.velocity = data.Power * data.shellTransform.forward;
            await UniTask.Delay(m_MaxLifeTime, cancellationToken: shellTokenSource.Token);

            DisableShell();
        }

        async UniTask EnableShell(TankShootingData data, CancellationToken token){
            ShellRigidbody.transform.position = CalculateAdjustedPosition(data);
            Debug.Log($"{ShellRigidbody.transform.position.x} / {ShellRigidbody.transform.position.y} / {ShellRigidbody.transform.position.z}");
            ShellRigidbody.transform.rotation = data.shellTransform.rotation;
            gameObject.SetActive(true);
            await UniTask.Yield(token); //To avoid effect to player-self.
            ShellCollider.enabled = true;
        }
        /// <summary> 
        /// Calculate current position with latency.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        Vector3 CalculateAdjustedPosition(TankShootingData data){
            if(p2pInfo.Instance.IsLoaclUser(AttackerID)){  
                return data.shellTransform.position;
            }
            return data.shellTransform.position + (data.Power * data.shellTransform.forward * data.GetLatencyBetweenRemoteAndLocal());
        }
        void DisableShell(){
            if(!gameObject.activeSelf){
                return;
            }
            shellTokenSource.Cancel();
            //Init
            ShellRigidbody.velocity = Vector3.zero;
            transform.position = Vector3.zero;
            transform.rotation = Quaternion.identity;
            //Avoid reacting to playerself.
            ShellCollider.enabled = false;
            gameObject.SetActive(false);
            ExplosionEffect.gameObject.SetActive(false);
        }

        void OnTriggerEnter(Collider other){
            // Collect all the colliders in a sphere from the shell's current position to a radius of the explosion radius.
            Collider[] colliders = Physics.OverlapSphere(transform.position, m_ExplosionRadius, m_TankMask);

            // Go through all the colliders...
            for (int i = 0; i < colliders.Length; i++){
                // ... and find their rigidbody.
                Rigidbody targetRigidbody = colliders[i].GetComponent<Rigidbody>();

                // If they don't have a rigidbody, go on to the next collider.
                if (!targetRigidbody)
                    continue;

                // Find the Tank script associated with the rigidbody.
                TankPlayer target = targetRigidbody.GetComponent<TankPlayer>();

                // If there is no TankHealth script attached to the gameobject, go on to the next collider.
                if (!target)
                    continue;

                //Damage calculate is only done by Local Player.
                if (!target.isLocal)
                    continue;

                float damage = CalculateDamage(targetRigidbody.position);
                
                // Deal this damage to the tank.
                target.TakeDamage(new TankDamageData(AttackerID.ToString(), damage));
            }

            
            ExplodeShell();
        }

        float CalculateDamage(Vector3 targetPosition){
            // Create a vector from the shell to the target.
            Vector3 explosionToTarget = targetPosition - transform.position;

            // Calculate the distance from the shell to the target.
            float explosionDistance = explosionToTarget.magnitude;

            // Calculate the proportion of the maximum distance (the explosionRadius) the target is away.
            float relativeDistance = (m_ExplosionRadius - explosionDistance) / m_ExplosionRadius;

            // Calculate damage as this proportion of the maximum possible damage.
            float damage = relativeDistance * m_MaxDamage;

            // Make sure that the minimum damage is always 0.
            damage = Mathf.Max(0f, damage);

            return damage;
        }

        void ExplodeShell(){
            ExplosionEffect.gameObject.SetActive(true);
            // Play the particle system.
            ExplosionEffect.Play();

            // Play the explosion sound effect.
            TankAudioManager.Instance.PlayShootingClipOneshot(ShootingClips.Explosion);

            DisableShell();
        }
    }
}