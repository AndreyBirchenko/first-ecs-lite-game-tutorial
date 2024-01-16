using Client.Components;
using Client.Services;

using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

using UnityEngine;

namespace Client.Systems
{
    public class EnemiesSystem : IEcsInitSystem, IEcsRunSystem
    {
        private EcsWorldInject _world;
        private EcsCustomInject<SceneService> _sceneService;
        private EcsPoolInject<UnitCmp> _unitCmpPool;
        private EcsPoolInject<LifetimeCmp> _lifetimeCmpPool;
        private EcsFilterInject<Inc<LifetimeCmp>> _lifetimeFilter;

        private float _spawnInterval;
        private Camera _camera;

        public void Init(IEcsSystems systems)
        {
            _spawnInterval = _sceneService.Value.EnemySpawnInterval;
            _camera = _sceneService.Value.Camera;
        }

        public void Run(IEcsSystems systems)
        {
            if (_sceneService.Value.GameIsOver)
                return;

            CreateEnemy();
            CheckEnemyLifetime();
        }

        private void CreateEnemy()
        {
            if ((_spawnInterval -= Time.deltaTime) > 0)
                return;

            _spawnInterval = _sceneService.Value.EnemySpawnInterval;

            var enemyView = _sceneService.Value.GetEnemy();
            var enemyPosition = GetOutOfScreenPosition();
            enemyView.SetPosition(enemyPosition);
            enemyView.RotateTo(_sceneService.Value.PlayerView.transform.position);

            var enemyEntity = _world.Value.NewEntity();

            ref var unitCmp = ref _unitCmpPool.Value.Add(enemyEntity);
            unitCmp.View = enemyView;
            unitCmp.Velocity = Vector3.up * _sceneService.Value.EnemyMoveSpeed;
            unitCmp.View.Construct(enemyEntity, _world.Value);

            ref var lifetimeCmp = ref _lifetimeCmpPool.Value.Add(enemyEntity);
            lifetimeCmp.Value = 3f;
        }

        private void CheckEnemyLifetime()
        {
            foreach (var entity in _lifetimeFilter.Value)
            {
                ref var lifetimeCmp = ref _lifetimeCmpPool.Value.Get(entity);
                lifetimeCmp.Value -= Time.deltaTime;

                if (lifetimeCmp.Value > 0)
                    continue;

                ref var unitCmp = ref _unitCmpPool.Value.Get(entity);
                _sceneService.Value.ReleaseEnemy(unitCmp.View);

                _world.Value.DelEntity(entity);
            }
        }

        private Vector3 GetOutOfScreenPosition()
        {
            var randomX = Random.Range(-1000, 1000);
            var randomY = Random.Range(-1000, 1000);
            var randomPosition = new Vector3(randomX, randomY);
            var randomDirection = (_camera.transform.position - randomPosition).normalized;
            var cameraHeight = _camera.orthographicSize * 2;
            var cameraWith = cameraHeight * _camera.aspect;
            return new Vector3(randomDirection.x * cameraHeight, randomDirection.y * cameraWith);
        }
    }
}