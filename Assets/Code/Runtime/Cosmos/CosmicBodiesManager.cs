using System.Collections.Generic;
using LH.Domain;
using UnityEngine;

namespace LH.Cosmos {
    public class CosmicBodiesManager {
        // потом будет конфигурация, которая будет задавать количество и логику звёзд
        private const int BODIES_COUNT = 300;

        private CosmosController _cosmos;

        private Transform _bodiesHolder;
        private readonly List<CosmicBodyData> _datas = new();
        private readonly List<CosmicBodyView> _bodies = new();


        public void Init(CosmosController cosmos) {
            _cosmos = cosmos;

            _bodiesHolder = new GameObject("bodies").transform;
            _bodiesHolder.SetParent(_cosmos.transform);

            RecreateBodies();
        }

        private void RecreateBodies() {
            for (int i = 0; i < _bodies.Count; i++) {
                _bodies[i].TurnOff();
            }

            var prefab = _cosmos.Config.CosmicBody;
            while (_bodies.Count < BODIES_COUNT) {
                var body = Object.Instantiate(prefab, _bodiesHolder);
                _bodies.Add(body);
            }

            // DO: добавить сид из конфига
            var random = new System.Random(0);

            _datas.Clear();
            for (int i = 0; i < BODIES_COUNT; i++) {
                var data = new CosmicBodyData();
                data.Index = i;

                // DO: тут алгоритмически рассосать звёзды по пространству в сфере
                data.AnchorPosition = new Vector2(random.NextFloat11(), random.NextFloat11()) * 300f;
                data.AnchorScale = random.NextFloat(3f, 10f);

                _datas.Add(data);

                _bodies[i].Setup(data);
            }
        }

        public void Update() {
            // DO: эта штука должна шевелить звёзды; по сути, плоский упрощённый ецс:
            //  - воздействие курсора и прочих влияний
            //  - особые поведения (на этом этапе лучше добавить логический объект тела) 
            //  - все тела стремятся обратно к якорным позициям
            //  - применяем новые данные на вьюху (и то, если изменились)

            foreach (var data in _datas) {
                AttractToAnchor(data);
            }

            foreach (var body in _bodies) {
                body.Apply();
            }
        }

        private void AttractToAnchor(CosmicBodyData data) {
            float dt = Time.deltaTime;

            var delta = data.AnchorPosition - data.Position;
            float distance = delta.magnitude;

            if (distance > .001f) {
                float speed = distance * 5.0f;
                Vector2 shift = delta.normalized * speed * dt;

                if (shift.magnitude > distance) {
                    data.Position = data.AnchorPosition;
                } else {
                    data.Position += shift;
                }
            } else {
                data.Position = data.AnchorPosition;
            }

            data.Scale = data.AnchorScale;
        }
    }
}