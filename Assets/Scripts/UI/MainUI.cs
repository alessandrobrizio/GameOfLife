using System.Collections;
using Tayx.Graphy;
using TMPro;
using Unity.Entities;
using UnityEngine;
using UnityEngine.UI;

namespace GameOfLife.UI
{
    public class MainUI : MonoBehaviour
    {
        [SerializeField] private TMP_InputField widthInputField;
        [SerializeField] private TMP_InputField heightInputField;
        [SerializeField] private TMP_Text amountText;
        [SerializeField] private TMP_Dropdown executeDropdown;
        [SerializeField] private Slider tickSlider;
        [SerializeField] private TMP_Text tickText;
        [SerializeField] private Button playButton;
        [SerializeField] private Button pauseButton;
        [SerializeField] private GraphyManager graphyManager;
        [SerializeField] private Toggle fpsToggle;
        [SerializeField] private Toggle ramToggle;
        [SerializeField] private Toggle infoToggle;

        private CameraController _cameraController;
        private EntityQuery _spawnCellsEntityQuery;
        private EntityQuery _simulateCellsEntityQuery;
        private Entity _executeEntity;

        private void Awake()
        {
            _spawnCellsEntityQuery = World.DefaultGameObjectInjectionWorld.EntityManager.CreateEntityQuery(ComponentType.ReadWrite<SpawnCellsConfig>());
            _simulateCellsEntityQuery = World.DefaultGameObjectInjectionWorld.EntityManager.CreateEntityQuery(ComponentType.ReadWrite<SimulateCellsConfig>());
        }

        private IEnumerator Start()
        {
            _cameraController = FindObjectOfType<CameraController>();
            EntityQuery executeQuery = World.DefaultGameObjectInjectionWorld.EntityManager.CreateEntityQuery(ComponentType.ReadOnly<Execute.Execute>());

            while (!executeQuery.TryGetSingletonEntity<Execute.Execute>(out _executeEntity))
            {
                yield return null;
            }

            _cameraController.UpdatePosition();

            if (_spawnCellsEntityQuery.TryGetSingleton<SpawnCellsConfig>(out var spawnCellsConfig))
            {
                widthInputField.text = spawnCellsConfig.Width.ToString();
                heightInputField.text = spawnCellsConfig.Height.ToString();
                amountText.text = (spawnCellsConfig.Width * spawnCellsConfig.Height).ToString();
            }

            if (_simulateCellsEntityQuery.TryGetSingleton<SimulateCellsConfig>(out var simulateCellsConfig))
            {
                tickSlider.value = simulateCellsConfig.TickDuration;
                tickText.text = simulateCellsConfig.TickDuration.ToString("0.#");
                playButton.gameObject.SetActive(!simulateCellsConfig.IsEnabled);
                pauseButton.gameObject.SetActive(simulateCellsConfig.IsEnabled);
            }

            if (World.DefaultGameObjectInjectionWorld.EntityManager.HasComponent<Execute.MainThread>(_executeEntity))
            {
                executeDropdown.value = 0;
            }
            else if (World.DefaultGameObjectInjectionWorld.EntityManager.HasComponent<Execute.SingleThreadedJob>(_executeEntity))
            {
                executeDropdown.value = 1;
            }
            else if (World.DefaultGameObjectInjectionWorld.EntityManager.HasComponent<Execute.ParallelJob>(_executeEntity))
            {
                executeDropdown.value = 2;
            }

            fpsToggle.SetIsOnWithoutNotify(graphyManager.FpsModuleState == GraphyManager.ModuleState.FULL);
            ramToggle.SetIsOnWithoutNotify(graphyManager.RamModuleState == GraphyManager.ModuleState.FULL);
            infoToggle.SetIsOnWithoutNotify(graphyManager.AdvancedModuleState == GraphyManager.ModuleState.FULL);

            Randomize();
        }

        public void OnWidthEndEdit(string value)
        {
            if (_spawnCellsEntityQuery.TryGetSingletonRW<SpawnCellsConfig>(out var config))
            {
                int.TryParse(value, out config.ValueRW.Width);
                amountText.text = (config.ValueRO.Width * config.ValueRO.Height).ToString();
            }
        }

        public void OnHeightEndEdit(string value)
        {
            if (_spawnCellsEntityQuery.TryGetSingletonRW<SpawnCellsConfig>(out var config))
            {
                int.TryParse(value, out config.ValueRW.Height);
                amountText.text = (config.ValueRO.Width * config.ValueRO.Height).ToString();
            }
        }

        public void Spawn()
        {
            World.DefaultGameObjectInjectionWorld.Unmanaged.GetExistingSystemState<SpawnCellsSystem>().Enabled = true;
            _cameraController.UpdatePosition();
        }

        public void Randomize()
        {
            World.DefaultGameObjectInjectionWorld.Unmanaged.GetExistingSystemState<RandomizeCellsSystem>().Enabled = true;
        }

        public void OnExecuteValueChanged(int value)
        {
            switch (value)
            {
                case 0:
                    World.DefaultGameObjectInjectionWorld.EntityManager.AddComponent<Execute.MainThread>(_executeEntity);
                    World.DefaultGameObjectInjectionWorld.EntityManager.RemoveComponent<Execute.SingleThreadedJob>(_executeEntity);
                    World.DefaultGameObjectInjectionWorld.EntityManager.RemoveComponent<Execute.ParallelJob>(_executeEntity);
                    break;
                case 1:
                    World.DefaultGameObjectInjectionWorld.EntityManager.RemoveComponent<Execute.MainThread>(_executeEntity);
                    World.DefaultGameObjectInjectionWorld.EntityManager.AddComponent<Execute.SingleThreadedJob>(_executeEntity);
                    World.DefaultGameObjectInjectionWorld.EntityManager.RemoveComponent<Execute.ParallelJob>(_executeEntity);
                    break;
                case 2:
                    World.DefaultGameObjectInjectionWorld.EntityManager.RemoveComponent<Execute.MainThread>(_executeEntity);
                    World.DefaultGameObjectInjectionWorld.EntityManager.RemoveComponent<Execute.SingleThreadedJob>(_executeEntity);
                    World.DefaultGameObjectInjectionWorld.EntityManager.AddComponent<Execute.ParallelJob>(_executeEntity);
                    break;
            }
        }

        public void OnTickValueChanged(float value)
        {
            if (_simulateCellsEntityQuery.TryGetSingletonRW<SimulateCellsConfig>(out var config))
            {
                config.ValueRW.TickDuration = value;
                tickText.text = value.ToString("0.#");
            }
        }

        public void Play()
        {
            if (_simulateCellsEntityQuery.TryGetSingletonRW<SimulateCellsConfig>(out var config))
            {
                config.ValueRW.IsEnabled = true;
            }
        }

        public void Pause()
        {
            if (_simulateCellsEntityQuery.TryGetSingletonRW<SimulateCellsConfig>(out var config))
            {
                config.ValueRW.IsEnabled = false;
            }
        }

        public void OnFpsToggled(bool value)
        {
            graphyManager.FpsModuleState = value ? GraphyManager.ModuleState.FULL : GraphyManager.ModuleState.OFF;
        }

        public void OnRamToggled(bool value)
        {
            graphyManager.RamModuleState = value ? GraphyManager.ModuleState.FULL : GraphyManager.ModuleState.OFF;
        }

        public void OnInfoToggled(bool value)
        {
            graphyManager.AdvancedModuleState = value ? GraphyManager.ModuleState.FULL : GraphyManager.ModuleState.OFF;
        }
    }
}
