using UnityEngine;
using UnityEngine.Events;

public class SceneTransition : MonoBehaviour
{
    [SerializeField]
    private Animator animator;

    private UnityEvent fadedOut = new UnityEvent();

    public void FadeOut()
    {
        animator.SetTrigger("FadeOut");
    }

    public void FadeOut(UnityAction action)
    {
        fadedOut.RemoveAllListeners();
        fadedOut.AddListener(action);
        animator.SetTrigger("FadeOut");
    }

    public void OnFadedOut()
    {
        fadedOut.Invoke();
    }
}
