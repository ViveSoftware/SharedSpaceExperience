using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace SharedSpaceExperience.UI
{
    public class ProgressBar : MonoBehaviour
    {
        [SerializeField]
        private Slider progressBar;
        [SerializeField]
        private TMP_Text progressText;

        private int progress = 0;
        private int prevProgress = 0;
        private int fullProgress = 0;

        public void SetProgress(int progress, int fullProgress)
        {
            this.progress = progress;
            this.fullProgress = fullProgress == 0 ? 1 : fullProgress;
        }

        private void Update()
        {
            if (prevProgress != progress)
            {
                progressBar.value = progress / (float)fullProgress;
                progressText.text = $"{progress}/{fullProgress}";
                prevProgress = progress;
            }
        }
    }
}
