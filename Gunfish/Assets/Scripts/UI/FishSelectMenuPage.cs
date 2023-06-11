using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class FishSelectMenuPage : IMenuPage {
    private MenuPageContext menuContext;
    public List<GunfishData> fishes;
    
    private List<GunfishData> displayedFishes;
    private List<int> displayedFishIndices;

    private List<VisualElement> fishImages;
    private List<Label> fishLabels;

    public void OnEnable(MenuPageContext context) {
        menuContext = context;
        context.actionMap.FindAction("Navigate").performed += OnNavigate;
        context.actionMap.FindAction("Submit").performed += OnSubmit;

        displayedFishes = new List<GunfishData>(4);
        displayedFishIndices = new List<int>(4);

        fishes = GameManager.instance.Gunfish_List;

        if (fishes.Count > 0) {
            displayedFishIndices.ForEach(displayedFishIndex => {
                DisplayFish(displayedFishIndex, fishes[displayedFishIndex]);
            });
        }
    }

    public void OnDisable(MenuPageContext context) {
        context.actionMap.FindAction("Navigate").performed -= OnNavigate;
        context.actionMap.FindAction("Submit").performed -= OnSubmit;
    }

    public void OnUpdate(MenuPageContext context) {

    }

    private void OnNavigate(InputAction.CallbackContext context) {
        var device = context.control.device;
        var deviceIndex = PlayerManager.instance.PlayerDevices.IndexOf(device);

        var direction = context.ReadValue<Vector2>();
        // Joystick movement should only be registered if it's a full flick
        if (direction.magnitude < 0.9f) {
            return;
        }

        // Horizontal
        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y)) {
            if (direction.x > 0) {
                IncrementFish(deviceIndex);
            } else {
                DecrementFish(deviceIndex);
            }
        }
    }

    private void OnSubmit(InputAction.CallbackContext context) {
        menuContext.menu.SetState(MenuState.GunfishSelect);
    }

    private void IncrementFish(int deviceIndex) {
        // Increments before modulus
        displayedFishIndices[deviceIndex] = (++displayedFishIndices[deviceIndex]) % fishes.Count;
        DisplayFish(deviceIndex, fishes[displayedFishIndices[deviceIndex]]);
    }

    private void DecrementFish(int deviceIndex) {
        // Decrements before comparison
        if (--displayedFishIndices[deviceIndex] < 0) {
            displayedFishIndices[deviceIndex] += fishes.Count;
        }
        DisplayFish(deviceIndex, fishes[displayedFishIndices[deviceIndex]]);
    }

    private void DisplayFish(int deviceIndex, GunfishData fish) {
        displayedFishes[deviceIndex] = fish;
    }
}
