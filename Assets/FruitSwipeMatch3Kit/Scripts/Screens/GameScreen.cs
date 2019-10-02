// Copyright (C) 2019 gamevanilla. All rights reserved.
// This code can only be used under the standard Unity Asset Store EULA,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using DG.Tweening;
using System.Collections;
using TMPro;
using Unity.Entities;
using UnityEngine;
using UnityEngine.Assertions;

using Image = UnityEngine.UI.Image;
using Random = UnityEngine.Random;

namespace FruitSwipeMatch3Kit
{
    /// <summary>
    /// This class manages the high-level logic of the game screen.
    /// </summary>
    public class GameScreen : BaseScreen, IRestartable
    {
        public GameConfiguration GameConfig;
        public int LevelNumber;
        
#pragma warning disable 649
        [SerializeField]
        private GoalsWidget goalsWidget;
        
        [SerializeField]
        private ScoreWidget scoreWidget;
        
        [SerializeField]
        private GameObject topCanvas;
        
        [SerializeField]
        private BuyPowerupsWidget buyPowerupsWidget;

        [SerializeField]
        private Canvas powerupCanvas;
        
        [SerializeField]
        private Image powerupImage;
        
        [SerializeField]
        private TextMeshProUGUI powerupText;

        [SerializeField]
        private CheckForFreeLives freeLivesChecker;
#pragma warning restore 649

        public LevelData LevelData => levelData;
        public GameState GameState => gameState;
       
        private LevelData levelData;
        private GameState gameState;

        private bool isPlayingEndGameSequence;
        public bool IsPlayingEndGameSequence => isPlayingEndGameSequence;

        private bool playerWon;
        private bool playerLost;

        private void Awake()
        {
            Assert.IsNotNull(goalsWidget);
            Assert.IsNotNull(scoreWidget);
            Assert.IsNotNull(topCanvas);
            Assert.IsNotNull(buyPowerupsWidget);
            Assert.IsNotNull(powerupCanvas);
            Assert.IsNotNull(powerupImage);
            Assert.IsNotNull(powerupText);
            Assert.IsNotNull(freeLivesChecker);
        }
        
        protected override void Start()
        {
            base.Start();
            
            InitializeLevel();
            OpenPopup<LevelGoalsPopup>("Popups/LevelGoalsPopup", popup =>
            {
                popup.SetGoals(levelData);	
            });
            
            EnableGameSystems();
            InitializeSystems();
        }

        private void OnDestroy()
        {
            DisableGameSystems();
        }
        
        private void InitializeLevel()
        {
            var progressionSystem = World.Active.GetExistingSystem<GameProgressionSystem>();
            var levelToLoad = LevelNumber;
            if (progressionSystem.IsPlayerComingFromLevelScreen)
            {
                var lastSelectedLevel = PlayerPrefs.GetInt(GameplayConstants.LastSelectedLevelPrefKey);
                if (lastSelectedLevel == 0)
                    lastSelectedLevel = LevelNumber;
                levelToLoad = lastSelectedLevel;

                progressionSystem.IsPlayerComingFromLevelScreen = false;
            }

            LoadLevel(levelToLoad);
            
            var tilePools = FindObjectOfType<TilePools>();
            tilePools.Initialize(levelData);
            
            var particlePools = FindObjectOfType<ParticlePools>();
            particlePools.Initialize();
            
            var entityManager = World.Active.EntityManager;
            var e = entityManager.CreateEntity(typeof(CreateLevelEvent));
            entityManager.SetComponentData(e, new CreateLevelEvent { Number = levelData.Number });

            goalsWidget.Initialize(levelData.Goals, tilePools);
            scoreWidget.Initialize(levelData);
            buyPowerupsWidget.Initialize(levelData);
            
            gameState = new GameState();
        }

        private void LoadLevel(int levelNum)
        {
            levelData = FileUtils.LoadLevel(levelNum);
        }

        private void InitializeSystems()
        {
            var world = World.Active;
            world.GetExistingSystem<LevelCreationSystem>().Initialize();
            world.GetExistingSystem<BoosterCreationSystem>().Initialize();
            world.GetExistingSystem<BoosterResolutionSystem>().Initialize();
            world.GetExistingSystem<CollectTilesGoalTrackingSystem>().Initialize();
            world.GetExistingSystem<CollectSlotsGoalTrackingSystem>().Initialize();
            world.GetExistingSystem<CollectBlockersGoalTrackingSystem>().Initialize();
            world.GetExistingSystem<CollectCollectiblesGoalTrackingSystem>().Initialize();
            world.GetExistingSystem<FillEmptySlotsSystem>().Initialize();
            world.GetExistingSystem<PlayerInputSystem>().Initialize();
            world.GetExistingSystem<UpdateScoreSystem>().Initialize();
            world.GetExistingSystem<CheckWinConditionSystem>().Initialize();
            world.GetExistingSystem<CrusherPowerupResolutionSystem>().Initialize();
            world.GetExistingSystem<BombPowerupResolutionSystem>().Initialize();
            world.GetExistingSystem<SwapPowerupResolutionSystem>().Initialize();
            world.GetExistingSystem<ColorBombPowerupResolutionSystem>().Initialize();
        }

        private void EnableGameSystems()
        {
            var world = World.Active;
            world.GetExistingSystem<LevelCreationSystem>().Enabled = true;
            world.GetExistingSystem<AnimateGravitySystem>().Enabled = true;
            world.GetExistingSystem<BoosterCreationSystem>().Enabled = true;
            world.GetExistingSystem<BoosterResolutionSystem>().Enabled = true;
            world.GetExistingSystem<CollectTilesGoalTrackingSystem>().Enabled = true;
            world.GetExistingSystem<CollectSlotsGoalTrackingSystem>().Enabled = true;
            world.GetExistingSystem<CollectBlockersGoalTrackingSystem>().Enabled = true;
            world.GetExistingSystem<CollectCollectiblesGoalTrackingSystem>().Enabled = true;
            world.GetExistingSystem<FillEmptySlotsSystem>().Enabled = true;
            world.GetExistingSystem<GravitySystem>().Enabled = true;
            world.GetExistingSystem<PlayerInputSystem>().Enabled = true;
            world.GetExistingSystem<UpdateScoreSystem>().Enabled = true;
            world.GetExistingSystem<CheckWinConditionSystem>().Enabled = true;
            world.GetExistingSystem<UpdateRemainingMovesUiSystem>().Enabled = true;
            world.GetExistingSystem<CrusherPowerupResolutionSystem>().Enabled = true;
            world.GetExistingSystem<BombPowerupResolutionSystem>().Enabled = true;
            world.GetExistingSystem<SwapPowerupResolutionSystem>().Enabled = true;
            world.GetExistingSystem<ColorBombPowerupResolutionSystem>().Enabled = true;
        }
        
        private void DisableGameSystems()
        {
            var world = World.Active;
            if (world != null)
            {
                world.GetExistingSystem<LevelCreationSystem>().Enabled = false;
                world.GetExistingSystem<AnimateGravitySystem>().Enabled = false;
                world.GetExistingSystem<BoosterCreationSystem>().Enabled = false;
                world.GetExistingSystem<BoosterResolutionSystem>().Enabled = false;
                world.GetExistingSystem<CollectTilesGoalTrackingSystem>().Enabled = false;
                world.GetExistingSystem<CollectSlotsGoalTrackingSystem>().Enabled = false;
                world.GetExistingSystem<CollectBlockersGoalTrackingSystem>().Enabled = false;
                world.GetExistingSystem<CollectCollectiblesGoalTrackingSystem>().Enabled = false;
                world.GetExistingSystem<FillEmptySlotsSystem>().Enabled = false;
                world.GetExistingSystem<GravitySystem>().Enabled = false;
                world.GetExistingSystem<PlayerInputSystem>().Enabled = false;
                world.GetExistingSystem<UpdateScoreSystem>().Enabled = false;
                world.GetExistingSystem<CheckWinConditionSystem>().Enabled = false;
                world.GetExistingSystem<UpdateRemainingMovesUiSystem>().Enabled = false;
                world.GetExistingSystem<CrusherPowerupResolutionSystem>().Enabled = false;
                world.GetExistingSystem<BombPowerupResolutionSystem>().Enabled = false;
                world.GetExistingSystem<SwapPowerupResolutionSystem>().Enabled = false;
                world.GetExistingSystem<ColorBombPowerupResolutionSystem>().Enabled = false;
            }
        }

        public void OnPlayerWon()
        {
            if (playerWon)
                return;

            playerWon = true;
            
            var nextLevel = PlayerPrefs.GetInt(GameplayConstants.NextLevelPrefKey);
            if (nextLevel == 0)
                nextLevel = 1;

            if (levelData.Number == nextLevel)
            {
                PlayerPrefs.SetInt(GameplayConstants.NextLevelPrefKey, levelData.Number + 1);
                PlayerPrefs.SetInt(GameplayConstants.UnlockedNextLevelPrefKey, 1);
            }
            else
            {
                PlayerPrefs.SetInt(GameplayConstants.UnlockedNextLevelPrefKey, 0);
            }

            var updateMovesSystem = World.Active.GetExistingSystem<UpdateRemainingMovesUiSystem>();
            if (levelData.EndGameAward && updateMovesSystem.NumRemainingMoves > 0)
                StartCoroutine(OpenEndGameAwardPopupAsync());
            else
                StartCoroutine(OpenWinPopupAsync());
        }

        public void OnPlayerLost()
        {
            if (playerLost)
                return;

            playerLost = true;
            
            StartCoroutine(OpenOutOfMovesPopupAsync());
        }
        
        private IEnumerator OpenEndGameAwardPopupAsync()
        {
            yield return new WaitForSeconds(GameplayConstants.EndGameAwardPopupDelay);
            OpenEndGameAwardPopup();
            BeginEndGameAwardSequence();
        }

        private void OpenEndGameAwardPopup()
        {
            OpenPopup<EndGameAwardPopup>("Popups/EndGameAwardPopup");
        }

        private IEnumerator OpenWinPopupAsync()
        {
            yield return new WaitForSeconds(GameplayConstants.WinPopupDelay);
            OpenWinPopup();
        }
        
        public void OpenLosePopup()
        {
            StartCoroutine(OpenLosePopupAsync());
        }
        
        private IEnumerator OpenLosePopupAsync()
        {
            yield return new WaitForSeconds(GameplayConstants.LosePopupDelay);
            OpenPopup<LosePopup>("Popups/LosePopup", popup =>
            {
                popup.SetScore(gameState.Score);
                popup.SetGoals(levelData.Goals, goalsWidget);
            });
        }
        
        private IEnumerator OpenOutOfMovesPopupAsync()
        {
            yield return new WaitForSeconds(GameplayConstants.OutOfMovesPopupDelay);
            OpenOutOfMovesPopup();
        }

        private void OpenWinPopup()
        {
            OpenPopup<WinPopup>("Popups/WinPopup", popup =>
            {
                var starsPrefKey = $"level_stars_{levelData.Number}";
                var levelStars = PlayerPrefs.GetInt(starsPrefKey);
                if (gameState.Score >= levelData.Star3Score)
                {
                    popup.SetStars(3);
                    PlayerPrefs.SetInt(starsPrefKey, 3);
                }
                else if (gameState.Score >= levelData.Star2Score)
                {
                    popup.SetStars(2);
                    if (levelStars < 3)
                        PlayerPrefs.SetInt(starsPrefKey, 2);
                }
                else if (gameState.Score >= levelData.Star1Score)
                {
                    popup.SetStars(1);
                    if (levelStars < 2)
                        PlayerPrefs.SetInt(starsPrefKey, 1);
                }
                else
                {
                    popup.SetStars(0);
                }

                var scorePrefKey = $"level_score_{levelData.Number}";
                var levelScore = PlayerPrefs.GetInt(scorePrefKey);
                if (levelScore < gameState.Score)
                    PlayerPrefs.SetInt(scorePrefKey, gameState.Score);

                popup.SetScore(gameState.Score);
                popup.SetGoals(levelData.Goals, goalsWidget);
            });
        }
        
        private void OpenOutOfMovesPopup()
        {
            OpenPopup<OutOfMovesPopup>("Popups/OutOfMovesPopup", popup => { OpenTopCanvas(); });
        }

        public void OpenCoinsPopup()
        {
            OpenPopup<BuyCoinsPopup>("Popups/BuyCoinsPopup");
        }

        public void OpenTopCanvas()
        {
            topCanvas.SetActive(true);
        }

        public void CloseTopCanvas()
        {
            topCanvas.SetActive(false);
        }

        public void ContinueGame()
        {
//            CloseTopCanvas();

            var numExtraMoves = GameConfig.NumExtraMoves;
            var world = World.Active;
            world.GetExistingSystem<UpdateRemainingMovesUiSystem>().Initialize(numExtraMoves);

            playerWon = false;
            playerLost = false;
        }

        public void RestartGame()
        {
            var world = World.Active;
            world.GetExistingSystem<LevelCreationSystem>().OnGameRestarted();
            world.GetExistingSystem<UpdateScoreSystem>().OnGameRestarted();
            world.GetExistingSystem<UpdateRemainingMovesUiSystem>().Initialize(levelData.Moves);
            
            playerWon = false;
            playerLost = false;
            
            goalsWidget.OnGameRestarted();
        }

        public void OnGameRestarted()
        {
            //CloseTopCanvas();
            RestartGame();
        }

        public void OnSettingsButtonPressed()
        {
            OpenPopup<PausePopup>("Popups/PausePopup");
        }

        public void PenalizePlayer()
        {
            freeLivesChecker.RemoveLife();
        }

        public void ExitGame()
        {
            CloseTopCanvas();
            PenalizePlayer();
            GetComponent<ScreenTransition>().PerformTransition();
        }

        public void EnablePowerupOverlay()
        {
            powerupCanvas.gameObject.SetActive(true);
            powerupImage.DOFade(220.0f/255.0f, 0.4f);
            powerupText.DOFade(1.0f, 0.4f);
        }

        public void DisablePowerupOverlay()
        {
            powerupImage.DOFade(0.0f, 0.3f);
            powerupText.DOFade(0.0f, 0.3f).OnComplete(() =>
            {
                powerupCanvas.gameObject.SetActive(false);
            });
        }

        public void SetPowerupText(string text)
        {
            powerupText.text = text;
        }

        private void BeginEndGameAwardSequence()
        {
            isPlayingEndGameSequence = true;
            StartCoroutine(BeginEndGameAwardSequenceAsync());
        }

        private IEnumerator BeginEndGameAwardSequenceAsync()
        {
            yield return new WaitForSeconds(2.0f);

            var levelCreationSystem = World.Active.GetExistingSystem<LevelCreationSystem>();
            var entityMgr = levelCreationSystem.EntityManager;
            var tiles = levelCreationSystem.TileEntities;
            var gos = levelCreationSystem.TileGos;

            var updateMovesSystem = World.Active.GetExistingSystem<UpdateRemainingMovesUiSystem>();
            var numMoves = updateMovesSystem.NumRemainingMoves;
            while (numMoves > 0)
            {
                var selectedValidTile = false;
                var idx = Random.Range(0, tiles.Length);
                while (!selectedValidTile)
                {
                    if (tiles[idx] != Entity.Null &&
                        gos[idx] != null &&
                        entityMgr.HasComponent<TileData>(tiles[idx]) &&
                        !entityMgr.HasComponent<BoosterData>(tiles[idx]))
                    {
                        selectedValidTile = true;
                    }
                    else
                    {
                        idx = Random.Range(0, tiles.Length);
                    }
                }

                var idx2 = Random.Range(0, 4);
                TileUtils.AddBoosterToTile(gos[idx], (BoosterType)idx2, entityMgr);
                entityMgr.CreateEntity(typeof(MatchHappenedEvent));
                SoundPlayer.PlaySoundFx("AwardBoosterPop");

                numMoves -= 1;
                
                yield return new WaitForSeconds(GameplayConstants.EndGameSequenceSpawnFrequency);
            }

            AdvanceEndGameSequence();
        }

        public void AdvanceEndGameSequence()
        {
            var levelCreationSystem = World.Active.GetExistingSystem<LevelCreationSystem>();
            var entityMgr = levelCreationSystem.EntityManager;
            var tiles = levelCreationSystem.TileEntities;

            var foundBooster = false;
            foreach (var tile in tiles)
            {
                if (tile != Entity.Null &&
                    entityMgr.HasComponent<BoosterData>(tile))
                {
                    entityMgr.AddComponentData(tile, new PendingBoosterData());
                    entityMgr.CreateEntity(typeof(ResolveBoostersData));
                    foundBooster = true;
                    break;
                }
            }

            if (!foundBooster)
            {
                isPlayingEndGameSequence = false;
                StartCoroutine(OpenWinPopupAsync());
            }
        }
    }
}
