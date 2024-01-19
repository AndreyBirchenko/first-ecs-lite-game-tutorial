using Client.Components;
using Client.Services;

using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

using UnityEngine;
using UnityEngine.SceneManagement;

namespace Client.Systems
{
    public class EndGameSystem : IEcsRunSystem
    {
        private EcsWorldInject _world;
        private EcsFilterInject<Inc<CollisionEvt>> _collisionsEvtFilter;
        private EcsPoolInject<CollisionEvt> _collisionsEvtPool;
        private EcsPoolInject<PlayerTag> _playerTagPool;
        private EcsFilterInject<Inc<UnitCmp>> _unitCmpFilter;
        private EcsCustomInject<SceneService> _sceneService;

        public void Run(IEcsSystems systems)
        {
            if (_sceneService.Value.GameIsOver)
                return;

            CheckLoseCondition();
            CheckWinCondition();
        }

        private void CheckLoseCondition()
        {
            foreach (var entity in _collisionsEvtFilter.Value)
            {
                ref var collisionEvt = ref _collisionsEvtPool.Value.Get(entity);
                var collidedEntity = collisionEvt.CollidedEntity;

                if (!_playerTagPool.Value.Has(collidedEntity))
                    continue;

                _sceneService.Value.GameIsOver = true;
                StopAllUnits();
                ShowEndGamePopup("Ты проиграл");
            }
        }

        private void CheckWinCondition()
        {
            if (Time.timeSinceLevelLoad <= 10)
                return;

            _sceneService.Value.GameIsOver = true;
            StopAllUnits();
            ShowEndGamePopup("Ты выиграл");
        }

        private void ShowEndGamePopup(string message)
        {
            var popupWindow = _sceneService.Value.PopupView;

            popupWindow.SetActive(true);
            popupWindow.SetDescription(message);
            popupWindow.SetButtonText("Повторить");
            popupWindow.Button.onClick.RemoveAllListeners();
            popupWindow.Button.onClick.AddListener(RestartGame);
        }

        private void RestartGame()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        private void StopAllUnits()
        {
            foreach (var entity in _unitCmpFilter.Value)
            {
                _unitCmpFilter.Pools.Inc1.Del(entity);
            }
        }
    }
}