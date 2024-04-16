using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Video;
using Unity.Netcode;

using Logger = Debugger.Logger;

namespace SharedSpaceExperience.Example
{
    public class VideoController : MonoBehaviour
    {
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

        private const float VIDEO_DELAY = 0.5f;

        public Action OnVideoFinished;

        private Coroutine videoCoroutine = null;

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
            Logger.Log("[Sync] Expected end at: " + expectEndTime + " video len: " + readyVideo.length);
            PlayVideo(readyVideo, expectEndTime - readyVideo.length);
        }

        public void PlayWinVideo()
        {
            PlayVideo(winVideo);
        }

        public void PlayLoseVideo()
        {
            PlayVideo(loseVideo);
        }

        public void PlayDrawVideo()
        {
            PlayVideo(drawVideo);
        }

        private void ClearVideoRenderTexture()
        {
            RenderTexture currentActiveRT = RenderTexture.active;
            RenderTexture.active = videoTexture;
            GL.Clear(true, true, Color.clear);
            RenderTexture.active = currentActiveRT;
        }

        private void PlayVideo(VideoClip clip, double startTime = 0)
        {
            // show panel
            SetActive(true);
            if (videoCoroutine != null)
            {
                StopVideo();
            }
            videoCoroutine = StartCoroutine(PlayVideoAtTime(clip, startTime));
        }

        private IEnumerator PlayVideoAtTime(VideoClip clip, double startTime = 0)
        {
            // wait until specified start time
            float delayToPlay = (float)(startTime - NetworkManager.Singleton.ServerTime.Time);
            Logger.Log("[Sync] Play video delay: " + delayToPlay + ", start at: " + startTime);
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
            OnVideoFinished?.Invoke();
            // clear one time used callback
            OnVideoFinished = null;

            videoCoroutine = null;
        }

        public void StopVideo()
        {
            // invoke callback
            // OnVideoFinished?.Invoke();
            // clear one time used callback
            OnVideoFinished = null;

            videoPlayer.Stop();
            StopCoroutine(videoCoroutine);
        }
    }
}