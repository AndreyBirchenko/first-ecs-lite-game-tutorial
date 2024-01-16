using Client.Components;
using Client.Services;

using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

using UnityEngine;

namespace Client.Systems
{
    public class PlayerInputSystem : IEcsRunSystem, IEcsInitSystem
    {
        private EcsWorldInject _world;
        private EcsPoolInject<UnitCmp> _unitCmpPool;
        private EcsPoolInject<PlayerTag> _playerTagPool;
        private EcsCustomInject<SceneService> _sceneData;

        private int _playerEntity;

        public void Init(IEcsSystems systems)
        {
            _playerEntity = _world.Value.NewEntity();
            _playerTagPool.Value.Add(_playerEntity);
            ref var playerCmp = ref _unitCmpPool.Value.Add(_playerEntity);
            playerCmp.View = _sceneData.Value.PlayerView;
            playerCmp.View.Construct(_playerEntity, _world.Value);
        }

        public void Run(IEcsSystems systems)
        {
            var playerMoveSpeed = _sceneData.Value.PlayerMoveSpeed;
            var x = Input.GetAxisRaw("Horizontal");
            var y = Input.GetAxisRaw("Vertical");
            var direction = new Vector3(x, y).normalized;
            var velocity = direction * playerMoveSpeed;

            if (!_unitCmpPool.Value.Has(_playerEntity))
                return;

            ref var playerCmp = ref _unitCmpPool.Value.Get(_playerEntity);
            playerCmp.Velocity = velocity;
        }
    }
}