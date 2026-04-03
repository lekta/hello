using System;
using UnityEngine;

namespace LH.Cosmos {
    [CreateAssetMenu(menuName = "LH/Cosmos Config")]
    public class CosmosConfig : ScriptableObject {
        public CosmicBodyView CosmicBody;
        public HiddenObjectView HiddenObject;

        // DO: можно перенести в "уровни", и оставить геймплей чисто на поиске объектов
        public int Seed = 42;
        public int BodyCount = 600;
        public int HiddenObjectCount = 7;

        public float FieldRadius = 3000f;

        public StarsCreationParams StarsParams = new();

        [Header("Цветовые зоны")]
        public ColorZone[] ColorZones = {
            new() { Position = new Vector2(-800, 400), Radius = 1200f, Strength = 0.5f, Tint = new Color(0.6f, 0.7f, 1f) },
            new() { Position = new Vector2(600, -500), Radius = 1000f, Strength = 0.4f, Tint = new Color(0.85f, 0.6f, 1f) },
            new() { Position = new Vector2(200, 800), Radius = 900f, Strength = 0.35f, Tint = new Color(0.5f, 0.9f, 0.8f) },
        };
    }

    [Serializable]
    public class ColorZone {
        public Vector2 Position;
        public float Radius = 1000f;
        [Range(0f, 1f)] public float Strength = 0.4f;
        public Color Tint = Color.white;
    }
}
