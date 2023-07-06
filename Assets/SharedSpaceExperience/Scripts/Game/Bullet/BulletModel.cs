using UnityEngine;

namespace SharedSpaceExperience
{
    public class BulletModel : MonoBehaviour
    {
        [SerializeField]
        private ParticleSystem[] bulletParticles;
        [SerializeField]
        private ParticleSystem trailParticles;

        [SerializeField]
        private Color[] bulletColorSet1;
        [SerializeField]
        private Color[] bulletColorSet2;
        [SerializeField]
        private Color[] trailColorSet;

        [SerializeField]
        private int index = -1;

        private void Start()
        {
            SetStyle(index);
        }

        public void SetStyle(int i)
        {
            index = i;
            SetParticleColor(bulletParticles[0], bulletColorSet1[i]);
            if (bulletParticles.Length > 1) SetParticleColor(bulletParticles[1], bulletColorSet2[i]);
            SetParticleColor(trailParticles, trailColorSet[i]);

        }

        private void SetParticleColor(ParticleSystem particle, Color color)
        {
            ParticleSystem.MainModule module = particle.main;
            module.startColor = new ParticleSystem.MinMaxGradient(color);
        }
    }
}