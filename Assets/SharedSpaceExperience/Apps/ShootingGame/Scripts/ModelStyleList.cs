using UnityEngine;

namespace SharedSpaceExperience.Example
{
    public class ModelStyleList : MonoBehaviour
    {
        public static ModelStyleList Instance { get; private set; }

        [SerializeField]
        private Color debugColor;
        [SerializeField]
        private Color[] colors;

        private float debugHue = 0;
        private float[] hues = new float[0];

        private void OnEnable()
        {
            // singleton
            if (Instance == null) Instance = this;
            else if (Instance != this) Destroy(this);

            // set debug hue
            Color.RGBToHSV(debugColor, out debugHue, out float _, out float _);

            // set hues
            hues = new float[colors.Length];
            for (int i = 0; i < colors.Length; ++i)
            {
                Color.RGBToHSV(colors[i], out hues[i], out float _, out float _);
            }
        }

        private void OnDisable()
        {
            // singleton
            if (Instance == this) Instance = null;
        }

        public int GetStyleIndex(int index)
        {
            return hues.Length == 0 || index < 0 ? -1 : index % hues.Length;
        }

        public float GetHue(int index)
        {
            return hues.Length == 0 || index < 0 ? debugHue : hues[index % hues.Length];
        }

        private Color ChangeHue(Color color, float hue)
        {
            Color.RGBToHSV(color, out float _, out float s, out float v);
            Color newColor = Color.HSVToRGB(hue, s, v);
            newColor.a = color.a;

            return newColor;
        }

        public void ChangeModelStyle(int style, MeshRenderer[] renderers)
        {
            float h = GetHue(style);
            foreach (MeshRenderer meshRenderer in renderers)
            {
                meshRenderer.material.color = ChangeHue(meshRenderer.material.color, h);
            }
        }

        public void ChangeModelStyle(int style, ParticleSystem[] particles)
        {
            float h = GetHue(style);
            foreach (ParticleSystem particle in particles)
            {
                ParticleSystem.MainModule module = particle.main;
                module.startColor = new ParticleSystem.MinMaxGradient(
                    ChangeHue(module.startColor.color, h)
                );
            }
        }
    }
}
