using UnityEngine;
using UnityGameFramework.Runtime;

namespace ZombiesMustDie
{
    /// <summary>由 Entity 系统显示、附着和回收的短时粒子特效。</summary>
    public sealed class ParticleEffectEntity : Entity
    {
        private ParticleSystem[] particles;
        private ParticleEffectEntityData effectData;
        private float elapsed;
        private bool playing;
        private bool stoppedEmitting;
        private bool isHiding;

        protected override void OnInit(object userData)
        {
            base.OnInit(userData);
            particles = GetComponentsInChildren<ParticleSystem>(true);
        }

        protected override void OnShow(object userData)
        {
            base.OnShow(userData);
            effectData = userData as ParticleEffectEntityData;
            elapsed = 0f;
            playing = false;
            stoppedEmitting = false;
            isHiding = false;
            StopParticles(ParticleSystemStopBehavior.StopEmittingAndClear);

            if (effectData == null)
            {
                Log.Error("Particle effect entity data is invalid.");
                HideEffect();
                return;
            }

            if (!effectData.WaitForAttachment)
            {
                Play();
            }
        }

        protected override void OnAttachTo(EntityLogic parentEntity, Transform parentTransform, object userData)
        {
            base.OnAttachTo(parentEntity, parentTransform, userData);
            CachedTransform.localPosition = Vector3.zero;
            CachedTransform.localRotation = Quaternion.identity;
            if (effectData != null && effectData.WaitForAttachment)
            {
                Play();
            }
        }

        protected override void OnUpdate(float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(elapseSeconds, realElapseSeconds);
            if (!playing || isHiding)
            {
                return;
            }

            elapsed += elapseSeconds;
            if (!stoppedEmitting && elapsed >= effectData.EmissionDuration)
            {
                StopParticles(ParticleSystemStopBehavior.StopEmitting);
                stoppedEmitting = true;
            }

            if (elapsed >= effectData.MaxLifetime || (stoppedEmitting && !IsAnyParticleAlive()))
            {
                HideEffect();
            }
        }

        protected override void OnHide(bool isShutdown, object userData)
        {
            StopParticles(ParticleSystemStopBehavior.StopEmittingAndClear);
            effectData = null;
            elapsed = 0f;
            playing = false;
            stoppedEmitting = false;
            isHiding = false;
            base.OnHide(isShutdown, userData);
        }

        private void Play()
        {
            if (playing || particles == null || particles.Length == 0)
            {
                HideEffect();
                return;
            }

            elapsed = 0f;
            playing = true;
            for (int i = 0; i < particles.Length; i++)
            {
                particles[i].Play(false);
            }
        }

        private void StopParticles(ParticleSystemStopBehavior behavior)
        {
            if (particles == null) return;
            for (int i = 0; i < particles.Length; i++)
            {
                particles[i].Stop(false, behavior);
            }
        }

        private bool IsAnyParticleAlive()
        {
            for (int i = 0; i < particles.Length; i++)
            {
                if (particles[i].IsAlive(false)) return true;
            }
            return false;
        }

        private void HideEffect()
        {
            if (isHiding) return;
            isHiding = true;
            if (GameEntry.Entity != null && Entity != null)
            {
                GameEntry.Entity.HideEntity(Entity);
            }
        }
    }
}
