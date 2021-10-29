using System.Collections;
using UnityEngine;

public class UIPause : UIManager.UIBehaviour
{
    protected override IEnumerator OnHide() { return null; }

    protected override IEnumerator OnShow()
    {
        Main.Game.Pause();
        return null;
    }

    private void Start()
    {
        GetComponentInChildren<MenuSection>().Cancel += OnContinue;
    }

    public void OnContinue()
    {
        Hide();
        Main.Game.Unpause();
    }

    public void OnQuit() => StartCoroutine(OnQuit_Coroutine());
    private IEnumerator OnQuit_Coroutine()
    {
        yield return Get<UICurtain>().Show();

        Main.Game.Stop();
        Hide();
    }
}