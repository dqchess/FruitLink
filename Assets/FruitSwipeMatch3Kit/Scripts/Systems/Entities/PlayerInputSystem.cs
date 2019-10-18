// Copyright (C) 2019 gamevanilla. All rights reserved.
// This code can only be used under the standard Unity Asset Store EULA,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Assertions;
using Unity.Entities;
using Unity.Mathematics;

namespace FruitSwipeMatch3Kit
{
    /// <summary>
    /// This system is responsible for processing the player's input, which
    /// basically involves detecting tile matches.
    /// </summary>
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public class PlayerInputSystem : ComponentSystem
    {
        public List<Entity> PendingBoosterTiles = new List<Entity>(8);

        private Camera mainCamera;
        private ParticlePools particlePools;

        private readonly LayerMask tileLayer = 1 << LayerMask.NameToLayer("Tile");
        private readonly RaycastHit2D[] raycastResults = new RaycastHit2D[8];

        private bool isDraggingInput;

        private readonly List<GameObject> selectedTiles = new List<GameObject>(8);
        private ColorTileType selectedType;

        private readonly List<GameObject> lineSegments = new List<GameObject>(8);
        private Vector3 lastSegmentPos;
        
        private readonly List<GameObject> selectSegments = new List<GameObject>(8);

        private readonly List<SpriteRenderer> darkTiles = new List<SpriteRenderer>();
        
        private GameObject linePrefab;
        private GameObject selectPrefab;

        private MoveDirection lastMoveDirection;

        private EntityArchetype applyGravityArchetype;
        private EntityArchetype matchHappenedArchetype;

        private bool inputLocked;

        private bool isBoosterExploding;
        private bool isBoosterChainResolving;

        private GameScreen gameScreen;

        private static readonly int Pressed = Animator.StringToHash("Pressed");
        private static readonly int Idle = Animator.StringToHash("Idle");

        protected override void OnCreate()
        {
            Enabled = false;
            applyGravityArchetype = EntityManager.CreateArchetype(typeof(ApplyGravityData));
            matchHappenedArchetype = EntityManager.CreateArchetype(typeof(MatchHappenedEvent));
        }

        public void Initialize()
        {
            mainCamera = Camera.main;
            Assert.IsNotNull(mainCamera);

            particlePools = Object.FindObjectOfType<ParticlePools>();

            linePrefab = particlePools.SelectionLine;
            selectPrefab = particlePools.SelectionParticle;

            gameScreen = Object.FindObjectOfType<GameScreen>();

            inputLocked = false;
            isDraggingInput = false;
        }

        protected override void OnUpdate()
        {
            if (inputLocked)
                return;

            if (gameScreen.CurrentPopups.Count > 0)
                return;

            if (gameScreen.IsPlayingEndGameSequence)
                return;

            if (isDraggingInput)
                OnMouseDrag();

            if (Input.GetMouseButtonDown(0))
                OnMouseDown();
            else if (Input.GetMouseButtonUp(0))
                OnMouseUp();
        }

        public void LockInput()
        {
            inputLocked = true;
        }

        public void UnlockInput()
        {
            inputLocked = false;
        }

        private void OnMouseDown()
        {
            var mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            if (Physics2D.RaycastNonAlloc(mousePos, Vector3.forward, raycastResults, Mathf.Infinity, tileLayer) > 0)
            {
                var result = raycastResults[0];
                var tile = result.collider.gameObject.GetComponent<Tile>();
                if (tile != null)
                {
                    var goe = result.collider.GetComponent<GameObjectEntity>();
                    var entityManager = goe.EntityManager;
                    var entity = goe.Entity;
                    if (selectedTiles.Count == 0)
                    {
                        selectedType = entityManager.GetComponentData<TileData>(entity).Type;
                        selectedTiles.Add(tile.gameObject);
                        isDraggingInput = true;

                        lastSegmentPos = tile.gameObject.transform.localPosition;

                        tile.GetComponent<Animator>().SetTrigger(Pressed);
                        
                        CreateSelectSegment(lastSegmentPos, selectedType);

                        CreatePathHighlight(entity, selectedType);
                        
                        SoundPlayer.PlaySoundFx("Connect");
                    }
                }
            }
        }

        private void OnMouseUp()
        {
            isDraggingInput = false;
            RemoveDarkFruits();
            if (selectedTiles.Count >= GameplayConstants.NumTilesNeededForMatch)
            {
                LockInput();
                var levelCreationSystem = World.GetExistingSystem<LevelCreationSystem>();
                var tiles = levelCreationSystem.TileEntities;
                var tileGos = levelCreationSystem.TileGos;
                var width = levelCreationSystem.Width;

                var tilesToDestroy = new List<int>(8);
                var lastIdx = 0;
                var selectedBooster = Entity.Null;
                
                foreach (var t in selectedTiles)
                {
                    var goe = t.GetComponent<GameObjectEntity>();
                    var entity = goe.Entity;
                    var tilePos = EntityManager.GetComponentData<TilePosition>(entity);
                    var idx = tilePos.X + (tilePos.Y * width);

                    if (EntityManager.HasComponent<BoosterData>(entity))
                    {
                        if (selectedBooster == Entity.Null)
                        {
                            selectedBooster = entity;
                            EntityManager.AddComponentData(entity, new PendingBoosterData());
                        }
                        else
                        {
                            PendingBoosterTiles.Add(entity);
                        }
                    }
                    else
                    {
                        tilesToDestroy.Add(idx);
                        lastIdx = idx;
                    }
                }
                
                var levelData = GameObject.Find("GameScreen").GetComponent<GameScreen>().LevelData;

                isBoosterExploding = selectedTiles.Find(x =>
                    EntityManager.HasComponent<BoosterData>(x.GetComponent<GameObjectEntity>().Entity));
                
                if (tilesToDestroy.Count == 0)
                {
                    EntityManager.CreateEntity(typeof(ResolveBoostersData));
                }
                else
                {
                    TileUtils.DestroyTiles(
                        tilesToDestroy, tiles, tileGos, levelCreationSystem.Slots, particlePools,
                        levelData.Width, levelData.Height);   
                }

                var e = EntityManager.CreateEntity(applyGravityArchetype);
                EntityManager.SetComponentData(e, new ApplyGravityData
                {
                    MatchSize = selectedTiles.Count,
                    MatchIndex = lastIdx,
                    MatchDirection = lastMoveDirection
                });

                EntityManager.CreateEntity(matchHappenedArchetype);
            }

            foreach (var tile in selectedTiles)
                tile.GetComponent<Animator>()?.SetTrigger(Idle);
            selectedTiles.Clear();
            RemoveBoosterLines();
            DestroySelectionLine();
            DestroySelectionEffect();
        }

        private void OnMouseDrag()
        {
            var mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            if (Physics2D.RaycastNonAlloc(mousePos, Vector3.forward, raycastResults, Mathf.Infinity, tileLayer) > 0)
            {
                var result = raycastResults[0];
                var tile = result.collider.gameObject.GetComponent<Tile>();
                if (tile != null)
                {
                    var goe = result.collider.GetComponent<GameObjectEntity>();
                    var entityManager = goe.EntityManager;
                    var entity = goe.Entity;
                    if (entityManager.GetComponentData<TileData>(entity).Type == selectedType &&
                        IsNeighbour(tile.gameObject))
                    {
                        if (!selectedTiles.Contains(tile.gameObject))
                        {
                            selectedTiles.Add(tile.gameObject);

                            tile.GetComponent<Animator>().SetTrigger(Pressed);

                            var localPosition = tile.gameObject.transform.localPosition;
                            CreateLineSegment(lastSegmentPos, localPosition, selectedType);
                            CreateSelectSegment(localPosition, selectedType);
                            lastSegmentPos = localPosition;

                            if (selectedTiles.Count >= 2)
                            {
                                var tileA = selectedTiles[selectedTiles.Count - 2];
                                var tileB = selectedTiles[selectedTiles.Count - 1];
                                var xA = GetX(tileA);
                                var xB = GetX(tileB);
                                var yA = GetY(tileA);
                                var yB = GetY(tileB);
                                if (xA == xB)
                                    lastMoveDirection = MoveDirection.Vertical;
                                else if (yA == yB)
                                    lastMoveDirection = MoveDirection.Horizontal;
                                else if (xA < xB)
                                    lastMoveDirection = MoveDirection.DiagonalLeft;
                                else
                                    lastMoveDirection = MoveDirection.DiagonalRight;
                            }

                            SoundPlayer.PlaySoundFx("Connect");
                        }
                        else
                        {
                            if (selectedTiles.Count >= 2)
                            {
                                var lastTile = selectedTiles[selectedTiles.Count - 2];
                                if (tile.gameObject == lastTile)
                                {
                                    selectedTiles[selectedTiles.Count - 1].GetComponent<Animator>().SetTrigger(Idle);
                                    selectedTiles.RemoveAtSwapBack(selectedTiles.Count - 1);

                                    var lastSegment = lineSegments[lineSegments.Count - 1];
                                    lineSegments.RemoveAtSwapBack(lineSegments.Count - 1);
                                    Object.Destroy(lastSegment);

                                    lastSegment = selectSegments[selectSegments.Count - 1];
                                    selectSegments.RemoveAtSwapBack(selectSegments.Count - 1);
                                    Object.Destroy(lastSegment);
                                    
                                    lastSegmentPos = selectedTiles[selectedTiles.Count - 1].transform.localPosition;
                                }
                            }
                        }
                        CreateBoosterDirection();
                    }
                    
                }
            }
        }
        
        private void CreatePathHighlight(Entity selectEntity, ColorTileType colorTileType)
        {
            var levelCreationSystem = World.GetExistingSystem<LevelCreationSystem>();
            var tileEntities = levelCreationSystem.TileEntities;
            var tileGos = levelCreationSystem.TileGos;
            var width = levelCreationSystem.Width;
            var height = levelCreationSystem.Height;
            
            for (var j = 0; j < height; j++)
            {
                for (var i = 0; i < width; i++)
                {
                    var idx = i + j * width;
                    if (tileEntities[idx] == Entity.Null ||
                        EntityManager.HasComponent<HoleSlotData>(tileEntities[idx]) || 
                        EntityManager.HasComponent<BlockerData>(tileEntities[idx]) || 
                        EntityManager.HasComponent<BoosterData>(tileEntities[idx]))
                        continue;
                    
                    var tile = tileGos[idx];
                    var tileRender = tile.GetComponent<SpriteRenderer>();
                    tileRender.color = Color.gray;
                    darkTiles.Add(tileRender);
                }
            }
            
            TilePosition tilePos = EntityManager.GetComponentData<TilePosition>(selectEntity);
            var startIdx = tilePos.X + tilePos.Y * width;
            tileGos[startIdx].GetComponent<SpriteRenderer>().color = Color.white;
            
            List<int> openSet = new List<int>();
            HashSet<int> closeSet = new HashSet<int>();
            openSet.Add(startIdx);

            while (openSet.Count > 0)
            {
                int currentIdx = openSet[0];
                openSet.Remove(currentIdx);
                closeSet.Add(currentIdx);

                List<int> neighbours = TileUtils.GetNeighbours(currentIdx, tileEntities, width, height, true);
                for (int i = 0; i < neighbours.Count; i++)
                {
                    int neighbourIdx = neighbours[i];
                    if(closeSet.Contains(neighbourIdx)) continue;
                    if (EntityManager.HasComponent<TileData>(tileEntities[neighbourIdx]))
                    {
                        if (EntityManager.GetComponentData<TileData>(tileEntities[neighbourIdx]).Type == colorTileType)
                        {
                            if(!openSet.Contains(neighbourIdx)) openSet.Add(neighbourIdx);
                            tileGos[neighbourIdx].GetComponent<SpriteRenderer>().color = Color.white;
                        }
                    }
                    closeSet.Add(neighbourIdx);
                }
            }
        }

        private void RemoveDarkFruits()
        {
            for (int i = 0; i < darkTiles.Count; i++)
            {
                darkTiles[i].color = Color.white;
            }
            darkTiles.Clear();
        }

        private bool IsNeighbour(GameObject tile)
        {
            var currentTileEntity = tile.GetComponent<GameObjectEntity>().Entity;
            var lastSelectedTile = selectedTiles[selectedTiles.Count - 1];
            var lastSelectedTileEntity = lastSelectedTile.GetComponent<GameObjectEntity>().Entity;
            return IsNeighbour(currentTileEntity, lastSelectedTileEntity);
        }

        private bool IsNeighbour(Entity a, Entity b)
        {
            var posA = EntityManager.GetComponentData<TilePosition>(a);
            var posB = EntityManager.GetComponentData<TilePosition>(b);
            return Mathf.Abs(posA.X - posB.X) <= 1 && Mathf.Abs(posA.Y - posB.Y) <= 1;
        }

        private int GetX(GameObject tile)
        {
            Assert.IsNotNull(tile.GetComponent<GameObjectEntity>());
            var entity = tile.GetComponent<GameObjectEntity>().Entity;
            var tilePos = EntityManager.GetComponentData<TilePosition>(entity);
            return tilePos.X;
        }

        private int GetY(GameObject tile)
        {
            Assert.IsNotNull(tile.GetComponent<GameObjectEntity>());
            var entity = tile.GetComponent<GameObjectEntity>().Entity;
            var tilePos = EntityManager.GetComponentData<TilePosition>(entity);
            return tilePos.Y;
        }

        private void CreateLineSegment(Vector3 start, Vector3 end, ColorTileType color)
        {
            var dir = end - start;
            var dist = Vector3.Distance(start, end);
            var segment = Object.Instantiate(linePrefab, start, quaternion.identity);
            segment.transform.rotation = Quaternion.LookRotation(Vector3.forward, dir);
            segment.transform.localScale = new Vector3(2, dist / 1.5f, 1);
            segment.GetComponent<SelectionLine>().SetColor(particlePools.GetColorTile(color));
            lineSegments.Add(segment);
        }

        private void CreateSelectSegment(Vector3 start, ColorTileType color)
        {
            var segment = Object.Instantiate(selectPrefab, start, quaternion.identity);
            var mainModule = segment.GetComponent<ParticleSystem>().main;
            mainModule.startColor = new ParticleSystem.MinMaxGradient(particlePools.GetColorTile(color));
            selectSegments.Add(segment);
        }

        private Tile lastBooster;
        private void CreateBoosterDirection()
        {
            var lastTile = selectedTiles[selectedTiles.Count - 1];
            RemoveBoosterLines();
            var lastTileBooster = lastTile.GetComponent<Tile>();
            if(GetBoosterType() != BoosterType.Normal)
            {
                if (lastTileBooster.AddBooster(GetBoosterType()))
                    lastBooster = lastTileBooster;
            }
        }

        private void RemoveBoosterLines()
        {
            if (lastBooster != null)
            {
                lastBooster.RemoveBooster();
                lastBooster = null;
            }
        }

        private BoosterType GetBoosterType()
        {
            var gameConfig = gameScreen.GameConfig;
            if (selectedTiles.Count >= gameConfig.NumTilesNeededForStarBooster)
            {
                return BoosterType.Star;
            } 
            if (selectedTiles.Count >= gameConfig.NumTilesNeededForCrossBooster)
            {
                return BoosterType.Cross;
            }
            if (selectedTiles.Count >= gameConfig.NumTilesNeededForRegularBooster)
            {
                switch (lastMoveDirection)
                {
                    case MoveDirection.Horizontal:
                        return BoosterType.Horizontal;
                    case MoveDirection.Vertical:
                        return BoosterType.Vertical;
                    case MoveDirection.DiagonalLeft:
                        return BoosterType.DiagonalLeft;
                    case MoveDirection.DiagonalRight:
                        return BoosterType.DiagonalRight;
                }
            }

            return BoosterType.Normal;
        }
        
        private void DestroySelectionLine()
        {
            foreach (var segment in lineSegments)
                Object.Destroy(segment);

            lineSegments.Clear();
        }

        private void DestroySelectionEffect()
        {
            foreach (var segment in selectSegments)
                Object.Destroy(segment);

            selectSegments.Clear();
        }

        public bool IsBoosterExploding()
        {
            return isBoosterExploding;
        }

        public bool IsBoosterChainResolving()
        {
            return isBoosterChainResolving;
        }

        public void SetBoosterExploding(bool exploding)
        {
            isBoosterExploding = exploding;
        }

        public void SetBoosterChainResolving(bool resolving)
        {
            isBoosterChainResolving = resolving;
        }

        public void OnGameRestarted()
        {
            isDraggingInput = false;
            UnlockInput();
            RemoveDarkFruits();

            foreach (var tile in selectedTiles)
                tile.GetComponent<Animator>()?.SetTrigger(Idle);
            selectedTiles.Clear();
            
            DestroySelectionLine();
            DestroySelectionEffect();
        }
    }
}
