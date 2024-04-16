using UnityEngine;

namespace SharedSpaceExperience.Example
{
    public class ModelStyle : MonoBehaviour
    {
        [SerializeField]
        private int style = -1;

        [SerializeField]
        private MeshRenderer[] renderers;

        [SerializeField]
        private ParticleSystem[] particles;

        private void OnEnable()
        {
            ModelStyleList.Instance.ChangeModelStyle(style, renderers);
        }

        public void SetVisible(bool visible)
        {
            gameObject.SetActive(visible);

            if (visible)
            {
                foreach (ParticleSystem particle in particles)
                {
                    particle.Play(true);
                }
            }
        }

        public void SetStyle(int style)
        {
            if (this.style == style) return;
            this.style = style;

            if (renderers.Length > 0)
            {
                ModelStyleList.Instance.ChangeModelStyle(this.style, renderers);
            }

            if (particles.Length > 0)
            {
                ModelStyleList.Instance.ChangeModelStyle(this.style, particles);
            }
        }

    }
}