using System.Collections;
using UnityEngine;

public class GameStateTutorial : ActionBase
{
  [SerializeField]
  private GameObject[] _tutorialSlides = null;

  protected override IEnumerator DoActionAsync()
  {
    yield return null;

    PlayerJoinManager.Instance.enabled = false;

    int slideIndex = 0;
    while (slideIndex < _tutorialSlides.Length)
    {
      for (int i = 0; i < Rewired.ReInput.players.playerCount; ++i)
      {
        Rewired.Player p = Rewired.ReInput.players.Players[i];
        if (p.GetAnyButtonDown())
        {
          _tutorialSlides[slideIndex].SetActive(false);
          ++slideIndex;
          if (slideIndex < _tutorialSlides.Length)
            _tutorialSlides[slideIndex].SetActive(true);
          break;
        }
      }

      yield return null;
    }
  }
}