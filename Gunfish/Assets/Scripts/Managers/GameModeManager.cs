using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class GameModeManager : PersistentSingleton<GameModeManager> {
    private GameObject gameModeInstance;
    public MatchManager matchManagerInstance { get; private set; }

    public void InitializeGameMode(GameModeType gameModeType, List<Player> players) {
        var gameMode = GameManager.Instance.GameModeList.gameModes.Where(element => element.gameModeType == gameModeType).FirstOrDefault();
        var levels = SelectLevels(gameMode.levels.sceneNames, gameMode.roundsPerMatch);
        var gameParameters = new GameParameters(players, levels, gameMode.levels.skyboxSceneName);
        var matchManagerPrefab = gameMode.matchManagerPrefab;
        gameModeInstance = Instantiate(matchManagerPrefab, transform);

        if (gameModeType == GameModeType.DeathMatch) {
            matchManagerInstance = gameModeInstance.GetComponent<DeathMatchManager>();
        }

        matchManagerInstance.Initialize(gameParameters);
    }

    public List<string> SelectLevels(List<string> levelSet, int quantity) {
        if (quantity > levelSet.Count) {
            throw new UnityException($"Cannot select {quantity} levels from level set of size {levelSet.Count}");
        }

        // Randomly select "quantity" levels
        levelSet.Shuffle();
        return levelSet.GetRange(0, quantity);
    }

    public void TeardownGameMode() {
        Debug.Log("Tearing down Gamemode");
        if (null != gameModeInstance) {
            matchManagerInstance.TearDown();
            Destroy(gameModeInstance);
        }
        matchManagerInstance = null;
    }

    public void NextLevel() {
        matchManagerInstance?.NextLevel();
    }
}
