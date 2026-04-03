using UnityEngine;

namespace LH.Cosmos {
    public class Star {
        public readonly StarData Data;

        public Vector2 Position;
        public float Scale;
        public float Brightness;


        public Star(StarData data) {
            Data = data;
            Position = data.AnchorPosition / 1000f;
            Scale = data.AnchorScale * 5f;
            Brightness = 1f;
        }


        public void Update(float time, float dt, StarsCreationParams p, Vector2 cursorPos, float cursorActivity) {
            AttractToAnchor(dt);
            ApplyTwinkle(time, p);
            ApplyCursorFocus(time, dt, cursorPos, cursorActivity, p);
        }

        public void ApplyHiddenEffect(HiddenObject hidden, float time) {
            if (hidden.BlackoutCoef > 0f)
                Brightness *= 1f - hidden.BlackoutCoef;

            float tx = Mathf.Sin(time * 101f + Data.Index * 2.3f) * hidden.TremorCoef;
            float ty = Mathf.Sin(time * 97f + Data.Index * 3.1f) * hidden.TremorCoef;
            Position += new Vector2(tx, ty);
        }


        private void AttractToAnchor(float dt) {
            var delta = Data.AnchorPosition - Position;
            float distance = delta.magnitude;

            if (distance > .01f) {
                float speed = distance * 5f;
                Vector2 shift = delta.normalized * speed * dt;
                Position = shift.magnitude > distance ? Data.AnchorPosition : Position + shift;
            } else {
                Position = Data.AnchorPosition;
            }

            var sizeDelta = Data.AnchorScale - Scale;
            if (Mathf.Abs(sizeDelta) > .01f)
                Scale += sizeDelta * 2f * dt;
            else
                Scale = Data.AnchorScale;
        }

        private void ApplyTwinkle(float time, StarsCreationParams p) {
            float subtle = 1f - p.TwinkleSubtle * Mathf.Sin(time * Data.TwinkleSpeed + Data.TwinklePhase);
            float blink = Mathf.Pow(Mathf.Abs(Mathf.Sin(time * Data.BlinkSpeed + Data.BlinkPhase)), p.BlinkSharpness);
            Brightness = subtle * blink;
        }

        private void ApplyCursorFocus(float time, float dt, Vector2 cursorPos, float activity, StarsCreationParams p) {
            if (activity < .001f)
                return;

            Vector2 toStar = Data.AnchorPosition - cursorPos;
            float dist = toStar.magnitude;
            if (dist >= p.FocusRadius)
                return;

            float t = dist / p.FocusRadius;
            float blend = p.FocusTransitionSpeed * dt * activity;

            float scaleFade = (1f - t) * (1f - t);
            float targetScale = Data.AnchorScale * (1f + (p.FocusMaxScale - 1f) * scaleFade);
            Scale = Mathf.Lerp(Scale, targetScale, blend);

            if (dist < .001f) {
                Position = Vector2.Lerp(Position, cursorPos, blend);
                return;
            }

            Vector2 dir = toStar / dist;

            if (dist < p.FocusSnapRadius) {
                float snapT = 1f - dist / p.FocusSnapRadius;
                Position = Vector2.Lerp(Position, cursorPos, snapT * snapT * .8f * blend);
            } else {
                float pushZone = p.FocusRadius - p.FocusSnapRadius;
                float pushT = (dist - p.FocusSnapRadius) / pushZone;
                float remapped = Mathf.Pow(pushT, p.FocusFisheyePower);
                float newDist = p.FocusSnapRadius + remapped * pushZone;
                Vector2 fisheyePos = cursorPos + dir * newDist;
                Position = Vector2.Lerp(Position, fisheyePos, blend);
            }

            // Дрожь: усиливается ближе к центру фокуса
            float tremorStrength = Data.TremorSensitivity * activity * (1f - t) * p.TremorAmplitude;
            float tx = Mathf.Sin(time * (170f - 153f * t) + Data.TwinklePhase * 3.7f) * tremorStrength;
            float ty = Mathf.Sin(time * (130f - 117f * t) + Data.BlinkPhase * 2.3f) * tremorStrength;
            Position += new Vector2(tx, ty);
        }
    }
}