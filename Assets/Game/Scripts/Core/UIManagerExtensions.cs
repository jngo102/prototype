using System.Collections;
using UnityEngine;

public static class UIManagerExtensions
{
    public static IEnumerator PlayAndAwait(this UIManager.UIBehaviour panel, string state)
    {
        var animator = panel.GetComponent<Animator>();
        if (animator != null && animator.enabled)
        {
            animator.Play(state);
            yield return null;

            var layer = animator.GetCurrentAnimatorStateInfo(0);

            if (animator.updateMode == AnimatorUpdateMode.UnscaledTime)
                yield return new WaitForSecondsRealtime(layer.length);
            else
                yield return new WaitForSeconds(layer.length);
        }
    }

    // public static IEnumerator PlayAndAwait(this UIManager.UIBehaviour panel, AnimationClip clip)
    // {
    //     if (clip != null)
    //     {
    //         var animator = panel.GetComponent<Animator>();
    //         if (animator == null)
    //             animator = panel.gameObject.AddComponent<Animator>();

    //         if (animator.enabled)
    //         {
    //             AnimationPlayableUtilities.PlayClip(animator, clip, out PlayableGraph graph);

    //             while (!graph.IsDone())
    //                 yield return null;

    //             graph.Destroy();
    //         }
    //     }
    //     else
    //     {
    //         Debug.LogWarning("AnimationClip can't be null");
    //     }
    // }
}