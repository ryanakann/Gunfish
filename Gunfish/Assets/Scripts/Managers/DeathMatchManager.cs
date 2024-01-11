using FunkyCode.LightingSettings;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization.Json;
using UnityEngine;

public class PlayerReference {
    public Player player;

    public int score;
    public int stocks;

    public float lastHitTimestamp = -1;
    public Player lastHitter;

    // reset each level
    public float firstKill;
    public float lastDeath;

    public PlayerReference(int stocks, int score=0) {
        this.stocks = stocks;
        this.score = score;
    }
}

public class DeathMatchManager : MatchManager {
    private const int defaultStocks = 3;
    //protected Dictionary<Player, int> playerScores = new Dictionary<Player, int>();
    //private Dictionary<Player, int> playerStocks = new Dictionary<Player, int>();
    protected Dictionary<Player, PlayerReference> playerReferences = new Dictionary<Player, PlayerReference>();
    private int remainingPlayers;
    protected HashSet<PlayerReference> eliminatedPlayers = new HashSet<PlayerReference>();

    protected DeathMatchUI ui;

    public PelicanSpawner pelicanSpawner;

    bool endingLevel;
    static float endLevelDelay = 0.5f;

    static float lastHitThreshold = 2f;


    public override void Initialize(GameParameters parameters) {
        foreach (var player in parameters.activePlayers) {
            playerReferences[player] = new PlayerReference(defaultStocks);
            /*
            playerScores[player] = 0;
            Debug.Log("Player Score: " + playerScores[player]);
            */
        }
        ui = gameObject.GetComponentInChildren<DeathMatchUI>();
        // ui.OnLoadingStart();
        ui.InitializeMatch(parameters.activePlayers);
        base.Initialize(parameters);
        pelicanSpawner = GetComponentInChildren<PelicanSpawner>();
    }

    public override void StartLevel() {
        base.StartLevel();
        endingLevel = false;
        eliminatedPlayers = new HashSet<PlayerReference>();
        ui.InitializeLevel(parameters.activePlayers, defaultStocks);
        remainingPlayers = parameters.activePlayers.Count;
        pelicanSpawner.FetchSpawnZones();
        pelicanSpawner.active = false;
        // iterate players and set up stocks
        foreach (var player in parameters.activePlayers) {
            //playerStocks[player] = defaultStocks;
            playerReferences[player].stocks = defaultStocks;
            playerReferences[player].firstKill = -1;
            playerReferences[player].lastDeath = -1;
            player.OnDeath += OnPlayerDeath;
            SpawnPlayer(player);
            player.Gunfish.OnDeath += OnPlayerDeath;
        }
    }

    protected override IEnumerator CoSpawnPlayer(Player player) {
        yield return new WaitForSeconds(0.5f);
        Transform currentSpawnPoint = spawnPoints[Random.Range(0, spawnPoints.Count)];
        float maxDistance = float.MinValue;
        float distance;
        foreach (var spawnPoint in spawnPoints) {
            distance = float.MaxValue;
            bool skip = true;
            foreach (var activePlayer in parameters.activePlayers) {
                if (activePlayer.Gunfish == null) {
                    continue;
                }
                else {
                    skip = false;
                }
                var playerDist = activePlayer.Gunfish.GetPosition();
                if (playerDist.HasValue)
                    distance = Mathf.Min(distance, Vector2.Distance(spawnPoint.position, playerDist.Value));
            }
            if (skip == false && distance > maxDistance) {
                maxDistance = distance;
                currentSpawnPoint = spawnPoint;
            }
        }
        player.SpawnGunfish(currentSpawnPoint.position);
        FinishSpawningPlayer(player);
    }

    public override void OnPlayerDeath(Player player) {
        PlayerReference playerRef = playerReferences[player];
        base.OnPlayerDeath(player);
        UpdateStock(player, -1);
        if (playerRef.stocks > 0) {
            SpawnPlayer(player);
        }
        else {
            remainingPlayers--;
            eliminatedPlayers.Add(playerRef);
            if (remainingPlayers <= 1 && !endingLevel) {
                StartCoroutine(EndLevel());
            }
        }
    }

    private Player GetLastPlayerStanding() {
        foreach ((Player player, PlayerReference playerRef) in playerReferences) {
            if (playerRef.stocks > 0)
                return player;
        }
        /*
        foreach (var kvp in playerStocks) {
            if (kvp.Value > 0) {
                return kvp.Key;
            }
        }
        if (eliminatedPlayers.Count > 0) {
            return eliminatedPlayers[eliminatedPlayers.Count - 1];
        }
        */
        return null;
    }

    public override void NextLevel() {
        ui.CloseLevelStats();
        // ui.OnLoadingStart();
        base.NextLevel();
    }

    private IEnumerator EndLevel() {
        endingLevel = true;
        pelicanSpawner.active = false;
        FreezeFish(true);
        foreach (var activePlayer in parameters.activePlayers) {
            activePlayer.OnDeath -= OnPlayerDeath;
            activePlayer.Gunfish.OnDeath -= OnPlayerDeath;
        }

        // todo: do a little async wait and THEN let the player win in case all the remaining players die at roughly the same time
        yield return new WaitForSeconds(endLevelDelay);

        // NOTE(Wyatt): I'm redoing scoring a bit. BITE ME!
        /*
        // Player score = which place they were eliminated at
        for (int i = 0; i < eliminatedPlayers.Count; i++) {
            playerScores[eliminatedPlayers[i]] += i;
        }
        */

        var player = GetLastPlayerStanding();
        if (player != null) {
            UpdateScore(player, 1);
            //UpdateScore(player, playerScores.Count);
        }
        ShowLevelWinner(player);
        //ui.ShowLevelStats((player == null) ? -1: player.playerNumber, playerScores); // if player is null, no one wins
        PlayerManager.Instance.SetInputMode(PlayerManager.InputMode.EndLevel);
    }

    protected virtual void ShowLevelWinner(Player player) {
        string tiebreakerText = "";
        if (player == null) {
            (player, tiebreakerText) = Tiebreaker(parameters.activePlayers);
        }
        ui.ShowLevelStats((player == null) ? "No one wins!" : $"Player {player.PlayerNumber} wins!", playerReferences, tiebreakerText);
    }

    public override void ShowEndGameStats() {
        base.ShowEndGameStats();
        int topScore = 0;
        List<Player> winners = new List<Player>();
        foreach ((Player player, PlayerReference playerRef) in playerReferences.OrderByDescending(x => x.Value.score)) {
            if (playerRef.score >= topScore) { 
                winners.Add(player);
                topScore = playerRef.score;
            };
        }
        string text = "It's a tie!";
        string tiebreakerText = "";
        if (winners.Count == 0) {
            text = "No one wins?";
        }
        else {
            if (winners.Count > 1) {
                Player player;
                (player, tiebreakerText) = Tiebreaker(winners);
                winners = new List<Player>() { player };
            }
            text = $"Player {winners[0].PlayerNumber} wins!!!";
        }
        ui.ShowFinalScores(text, playerReferences, winners, tiebreakerText);
    }

    protected (Player, string) Tiebreaker(List<Player> winners) {
        // get player with earliest non-negative kill
        List<Player> tiebreaker = winners.Where(x => playerReferences[x].firstKill > 0).OrderBy(x => playerReferences[x].firstKill).ToList();
        Player player;
        if (tiebreaker.Count != 0) {
            player = tiebreaker.First();
            return (player, "*First kill!");
        }
        // if null, get player with last death
        player = winners.OrderByDescending(x => playerReferences[x].lastDeath).FirstOrDefault();
        UpdateScore(player, 1);
        return (player, "*Last death!");
    }

    public void UpdateScore(Player player, int scoreDelta) {
        PlayerReference playerRef = playerReferences[player];
        playerRef.score += scoreDelta;
        ui.OnScoreChange(player, playerRef.score);
    }
    public void UpdateStock(Player player, int stockDelta) {
        PlayerReference playerRef = playerReferences[player];
        playerRef.stocks += stockDelta;
        ui.OnStockChange(player, playerRef.stocks);
    }

    public override void HandleFishDamage(FishHitObject fishHit, Gunfish gunfish, bool alreadyDead) {
        base.HandleFishDamage(fishHit, gunfish, alreadyDead);
        if (alreadyDead == true)
            return;

        PlayerReference playerRef = playerReferences[gunfish.player];

        // if src fish, then award points, otherwise detract points
        Gunfish sourceGunfish = fishHit.source.GetComponent<Gunfish>();
        sourceGunfish = sourceGunfish ?? fishHit.source.GetComponent<Gun>()?.gunfish;
        if (sourceGunfish != null) {
            playerRef.lastHitTimestamp = Time.time;
            playerRef.lastHitter = sourceGunfish.player;
        }

        if (gunfish.statusData.health > 0) {
            return;
        }

        if ((Time.time - playerRef.lastHitTimestamp) <= lastHitThreshold && playerRef.lastHitter != null) {
            sourceGunfish = playerRef.lastHitter.Gunfish;
        }
        playerRef.lastDeath = Time.time;

        if (sourceGunfish != null) {
            MarqueeManager.Instance.EnqueueRandomQuip();
            // todo: update first kill
            if (playerReferences[sourceGunfish.player].firstKill < 0)
                playerReferences[sourceGunfish.player].firstKill = Time.time;
            UpdateScore(sourceGunfish.player, 1);
        }
        else if (!endingLevel) {
            // todo: this should play a special suicide quip (Selfish Destruction!)
            MarqueeManager.Instance.EnqueueRandomQuip();
            // NOTE: Temporarily takin this out. and if it's more fun this way, we'll leave it.
            // UpdateScore(gunfish.player, -1);
        }
    }

    public override void OnTimerFinish() {
        base.OnTimerFinish();
        // todo: SUMMON THE FUCKING PELICANS
        pelicanSpawner.active = true;
        foreach ((Player player, PlayerReference playerRef) in playerReferences) {
            if (playerRef.stocks > 1) {
                UpdateStock(player, -(playerRef.stocks - 1));
            }
        }
        // maybe play a quip? (SUDDEN DEATH!)
    }
}