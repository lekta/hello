using System;
using UnityEngine;

namespace LH.Cosmos {
    public class CosmosController : MonoBehaviour {
        public CosmosConfig Config;
        public CameraMoveParams CameraMoveParams;

        public Camera Camera;
        public CursorView Cursor;
        public Transform Background;

        private readonly CosmicBodiesManager _bodies = new();
        private readonly HiddenObjectsManager _hiddens = new();
        private readonly CosmosCamera _cosmosCamera = new();
        private readonly CosmosCursor _cursor = new();

        public Vector2 CursorWorldPos => _cursor.Position;
        public float CursorActivity => _cursor.Activity;
        public CursorState CursorState => _cursor.State;


        private void Awake() {
            float fieldRadius = Config.FieldRadius;

            _bodies.Init(this, fieldRadius);
            _hiddens.Init(this, fieldRadius, _bodies.Datas);
            _cursor.Init(this);
            _cosmosCamera.Init(Camera, Background, _cursor, fieldRadius, CameraMoveParams);
        }

        private void Update() {
            _cursor.Update();
            _cosmosCamera.Update();
            _hiddens.Update();
            _bodies.Update(_hiddens.Hiddens);
        }
    }

    [Serializable]
    public class CameraMoveParams {
        [Header("Зоны управления")]
        [Tooltip("Мёртвая зона (доля от центра до края экрана)")]
        [Range(0f, 1f)]
        public float DeadZone = 0.25f;

        [Tooltip("Зона максимальной скорости (доля от центра до края)")]
        [Range(0f, 1f)]
        public float MaxSpeedZone = 0.67f;

        [Tooltip("Скорость прокрутки (ширин экрана в секунду)")]
        public float ScrollSpeed = 1.2f;

        [Tooltip("Сглаживание скорости")]
        public float Smoothing = 4f;

        [Header("Границы")]
        [Tooltip("Жёсткость мягкой границы")]
        public float BoundaryStiffness = 10f;

        [Tooltip("Запас за границей (в ширинах экрана)")]
        public float BoundaryOvershoot = 0.5f;

        [Header("Визуал")]
        [Tooltip("Параллакс фона")]
        public float Parallax = 0.1f;
    }
}