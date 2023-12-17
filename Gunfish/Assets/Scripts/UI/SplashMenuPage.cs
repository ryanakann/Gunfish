using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class SplashMenuPage : IMenuPage {
    private MenuPageContext menuContext;
    private bool isLoadingNextMenu;

    public void OnEnable(MenuPageContext context) {
        menuContext = context;
        isLoadingNextMenu = false;

        foreach (var playerInput in PlayerManager.Instance.PlayerInputs) {
            if (playerInput) {
                playerInput.currentActionMap.FindAction("Any").performed += OnAnyKey;
            }
        }
    }

    public void OnDisable(MenuPageContext context) {
        foreach (var playerInput in PlayerManager.Instance.PlayerInputs) {
            if (playerInput) {
                playerInput.currentActionMap.FindAction("Any").performed -= OnAnyKey;
            }
        }
    }

    public void OnUpdate(MenuPageContext context) {

    }

    private void OnAnyKey(InputAction.CallbackContext context) {
        if (isLoadingNextMenu == false)
        {
            isLoadingNextMenu = true;
            FX_Spawner.Instance.SpawnFX(FXType.TitleScreenStartFX, Camera.main.transform.position, Quaternion.identity);
            DOTween.Sequence().AppendInterval(1).AppendCallback(LoadNextMenu);
        }
    }

    private void LoadNextMenu() {
        GameManager.Instance.SetSelectedGameMode(GameModeType.DeathMatch);
        menuContext.menu.SetState(MenuState.GunfishSelect);
    }
}