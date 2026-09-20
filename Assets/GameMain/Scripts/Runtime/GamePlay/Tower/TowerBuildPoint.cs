using System;
using UnityEngine;
using UnityEngine.Events;

namespace ZombiesMustDie
{
    public enum TowerOperationState { Empty, Building, Working, Upgrading }

    [DisallowMultipleComponent]
    public sealed class TowerBuildPoint : Interactable
    {
        [SerializeField] private Transform spawnPoint;
        [SerializeField] private int[] allowedTowerIds = { 1, 2 };

        [SerializeField] private UnityEvent<TowerBuildPoint> onBuildSelectionRequested = new UnityEvent<TowerBuildPoint>();

        public UnityEvent<TowerBuildPoint> OnBuildSelectionRequested => onBuildSelectionRequested;
        public override bool CanInteract => isActiveAndEnabled && State == TowerOperationState.Empty && Service == null;
        public override string Prompt => "建造";

        public override void Interact()
        {
            if (CanInteract) onBuildSelectionRequested.Invoke(this);
        }

        private ParticleSystem[] particles; //建造点粒子特效
        private bool playing = false;


        public Transform SpawnPoint => spawnPoint != null ? spawnPoint : transform;
        public TowerEntity Tower { get; internal set; }
        public TowerService Service { get; internal set; }
        public TowerOperationState State { get; internal set; }
        public Guid OperationId { get; internal set; }
        public bool IsBusy => State != TowerOperationState.Empty && State != TowerOperationState.Working;
        public bool Allows(int towerId) => allowedTowerIds != null && Array.IndexOf(allowedTowerIds, towerId) >= 0;

        protected override void Awake()
        {
            base.Awake();
            particles = GetComponentsInChildren<ParticleSystem>(true);
            PlayVfx();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            if (Service != null) Service.ReleasePoint(this);

            StopParticles(ParticleSystemStopBehavior.StopEmittingAndClear);

        }

        public void PlayVfx()
        {
            if (playing || particles == null || particles.Length == 0)
            {
                return;
            }
            playing = true;
            for (int i = 0; i < particles.Length; i++)
            {
                particles[i].Play(false);
            }
        }

        public void StopParticles(ParticleSystemStopBehavior behavior)
        {
            if (particles == null) return;
            playing = false;
            for (int i = 0; i < particles.Length; i++)
            {
                particles[i].Stop(false, behavior);
            }
        }

    }
}
