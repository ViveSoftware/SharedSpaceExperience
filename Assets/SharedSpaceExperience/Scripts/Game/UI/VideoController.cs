using System.Collections;
using UnityEngine;
using UnityEngine.Video;

using Photon.Pun;

namespace SharedSpaceExperience
{
    public class VideoController : MonoBehaviour
    {
        private const float VIDEO_DELAY = 0.5f;

        [SerializeField]
        private VideoPlayer videoPlayer;
        [SerializeField]
        private RenderTexture videoTexture;
        [SerializeField]
        private VideoClip readyVideo;
        [SerializeField]
        private VideoClip koVideo;
        [SerializeField]
        private VideoClip timeoutVideo;
        [SerializeField]
        private VideoClip winVideo;
        [SerializeField]
        private VideoClip loseVideo;
        [SerializeField]
        private VideoClip drawVideo;

        public delegate void OnVideoEndCallback();

        public void SetActive(bool active)
        {
            gameObject.SetActive(active);

            if (active)
            {
                ClearVideoRenderTexture();
            }
        }

        public void PlayStartVideo(double expectEndTime)
        {
            Logger.Log("[VideoController] clip expected end at: " + expectEndTime + " cur: " + PhotonNetwork.Time);
            PlayVideo(readyVideo, expectEndTime - readyVideo.length);
        }

        public void PlayRoundEndVideo(int winner, OnVideoEndCallback callback)
        {
            // -1: timeout, >= 0: someone win
            VideoClip clip = winner < 0 ? timeoutVideo : koVideo;
            PlayVideo(clip, 0, callback);
        }

        public void PlayMatchEndVideo(int result, OnVideoEndCallback callback)
        {
            // 0: lose, 1: draw, 2: win
            VideoClip clip = result == 0 ? loseVideo : (result == 1 ? drawVideo : winVideo);
            PlayVideo(clip, 0, callback);
        }

        private void ClearVideoRenderTexture()
        {
            RenderTexture currentActiveRT = RenderTexture.active;
            RenderTexture.active = videoTexture;
            GL.Clear(true, true, Color.clear);
            RenderTexture.active = currentActiveRT;
        }

        private void PlayVideo(VideoClip clip, double startTime = 0, OnVideoEndCallback callback = null)
        {
            // show panel
            SetActive(true);
            StartCoroutine(PlayVideoAtTime(clip, startTime, callback));
        }

        private IEnumerator PlayVideoAtTime(VideoClip clip, double startTime = 0, OnVideoEndCallback callback = null)
        {
            // wait until specified start time
            float delayToPlay = (float)(startTime - PhotonNetwork.Time);
            Logger.Log("[MatchUI] delay " + delayToPlay + " start " + startTime);
            if (delayToPlay > 0)
            {
                yield return new WaitForSeconds(delayToPlay);
            }

            // play video
            // FIXME: set correct start time (videoPlayer.time)
            videoPlayer.clip = clip;
            videoPlayer.Play();

            // wait until video finished
            yield return new WaitForSeconds((float)clip.length + VIDEO_DELAY);

            // hide panel
            SetActive(false);
            // invoke callback
            if (callback != null) callback();
        }
    }
}