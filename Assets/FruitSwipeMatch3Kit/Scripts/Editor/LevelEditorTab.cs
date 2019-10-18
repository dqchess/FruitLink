// Copyright (C) 2019 gamevanilla. All rights reserved.
// This code can only be used under the standard Unity Asset Store EULA,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Assertions;

using Object = UnityEngine.Object;

namespace FruitSwipeMatch3Kit
{
	/// <summary>
	/// The 'Level editor' tab in the editor.
	/// </summary>
	public class LevelEditorTab : EditorTab
	{
		private Object levelDbObj;
		private LevelData currentLevelData;

		private Vector2 scrollPos;

		private const float LevelTileButtonSize = 40f;

		private BrushType currentBrushType;
		private ColorTileType currentColorTileType;
		private RandomColorTileType currentRandomColorTileType;
		private SlotType currentSlotType;
		private BlockerType currentBlockerType;
		private CollectibleType currentCollectibleType;

		private BrushMode currentBrushMode;

		private int prevWidth;
		private int prevHeight;
		
		private readonly Dictionary<string, Texture> tileTextures = new Dictionary<string, Texture>();

		private ReorderableList availableColorsList;
		private ColorTileType currentColor;
		private ReorderableList goalList;
		private LevelGoalData currentGoal;
		
		public LevelEditorTab(FruitSwipeMatch3KitEditor editor) : base(editor)
		{
			var editorImagesPath = new DirectoryInfo(Application.dataPath + "/FruitSwipeMatch3Kit/Editor/Resources");
            var fileInfo = editorImagesPath.GetFiles("*.png", SearchOption.TopDirectoryOnly);
            foreach (var file in fileInfo)
            {
                var filename = Path.GetFileNameWithoutExtension(file.Name);
                tileTextures[filename] = Resources.Load(filename) as Texture;
            }
		}

		public override void Draw()
		{
			scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

            var oldLabelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 90;

            GUILayout.Space(15);

            DrawMenu();

            if (currentLevelData != null)
            {
                GUILayout.Space(15);
	            
				DrawGeneralLevelInfo();
				
	            GUILayout.BeginHorizontal();
				DrawLevelTiles();
				GUILayout.BeginVertical();
				DrawAvailableColors();
	            DrawGoals();
	            GUILayout.EndVertical();
	            GUILayout.EndHorizontal();
            }

            EditorGUIUtility.labelWidth = oldLabelWidth;
            EditorGUILayout.EndScrollView();

			if (GUI.changed && currentLevelData != null)
				EditorUtility.SetDirty(currentLevelData);
		}
		
		private void DrawMenu()
        {
			var oldDb = levelDbObj;
			levelDbObj = EditorGUILayout.ObjectField("Asset", levelDbObj, typeof(LevelData), false, GUILayout.Width(340));
			if (levelDbObj != oldDb)
			{
				currentLevelData = (LevelData)levelDbObj;
				if (currentLevelData.AvailableColors.Count == 0)
					currentLevelData.Initialize();
				CreateAvailableColorsList();
				CreateGoalsList();
				currentGoal = null;
			}
        }

		private void DrawGeneralLevelInfo()
		{
			GUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.Width(400));
			
			var style = new GUIStyle
			{
				fontSize = 20,
				fontStyle = FontStyle.Bold,
				normal = { textColor = Color.white }
			};
			EditorGUILayout.LabelField("General", style);
			
			GUILayout.Space(10);
			
            GUILayout.BeginHorizontal(GUILayout.Width(500));
            EditorGUILayout.HelpBox(
                "The general settings of this level.",
                MessageType.Info);
            GUILayout.EndHorizontal();
            
            GUILayout.BeginHorizontal();
            
            GUILayout.BeginVertical();

//            GUILayout.BeginHorizontal();
//            EditorGUILayout.LabelField(new GUIContent("Level number", "The number of this level."),
//                GUILayout.Width(EditorGUIUtility.labelWidth));
//            currentLevelData.Number = EditorGUILayout.IntField(currentLevelData.Number, GUILayout.Width(30));
//            GUILayout.EndHorizontal();
			
//			GUILayout.Space(10);
			
			prevWidth = currentLevelData.Width;
			
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Width", "The width of this level."),
                GUILayout.Width(EditorGUIUtility.labelWidth));
            currentLevelData.Width = EditorGUILayout.IntField(currentLevelData.Width, GUILayout.Width(30));
            if (currentLevelData.Width != prevWidth)
	            ResetLevelTiles();
            GUILayout.EndHorizontal();

			GUILayout.Space(10);
			
			prevHeight = currentLevelData.Height;
			
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Height", "The height of this level."),
                GUILayout.Width(EditorGUIUtility.labelWidth));
            currentLevelData.Height = EditorGUILayout.IntField(currentLevelData.Height, GUILayout.Width(30));
            if (currentLevelData.Height != prevHeight)
	            ResetLevelTiles();
            GUILayout.EndHorizontal();

			GUILayout.Space(10);
			
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Moves", "The number of moves available to the player in this level."),
                GUILayout.Width(EditorGUIUtility.labelWidth));
            currentLevelData.Moves = EditorGUILayout.IntField(currentLevelData.Moves, GUILayout.Width(30));
            GUILayout.EndHorizontal();
            
			GUILayout.Space(10);
			
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("End game award", "If enabled, the player will receive a booster for every move left."),
                GUILayout.Width(EditorGUIUtility.labelWidth));
            currentLevelData.EndGameAward = EditorGUILayout.Toggle(currentLevelData.EndGameAward);
            GUILayout.EndHorizontal();
            
			GUILayout.Space(10);
			
			GUILayout.EndVertical();
			
			GUILayout.Space(50);
			
			GUILayout.BeginVertical();
			
            var oldLabelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 40;
            
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Star 1", "The score needed to obtain the first star in this level."),
                GUILayout.Width(EditorGUIUtility.labelWidth));
            currentLevelData.Star1Score = EditorGUILayout.IntField(currentLevelData.Star1Score, GUILayout.Width(60));
            GUILayout.EndHorizontal();
            
			GUILayout.Space(10);
			
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Star 2", "The score needed to obtain the second star in this level."),
                GUILayout.Width(EditorGUIUtility.labelWidth));
            currentLevelData.Star2Score = EditorGUILayout.IntField(currentLevelData.Star2Score, GUILayout.Width(60));
            GUILayout.EndHorizontal();
            
			GUILayout.Space(10);
			
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Star 3", "The score needed to obtain the third star in this level."),
                GUILayout.Width(EditorGUIUtility.labelWidth));
            currentLevelData.Star3Score = EditorGUILayout.IntField(currentLevelData.Star3Score, GUILayout.Width(60));
            GUILayout.EndHorizontal();
            
			GUILayout.Space(10);
			
			GUILayout.EndVertical();
			
			GUILayout.Space(50);
			
			GUILayout.BeginVertical();
			
            EditorGUIUtility.labelWidth = 120;
			
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Crusher available", "Is the crusher power-up available to purchase on this level?"),
                GUILayout.Width(EditorGUIUtility.labelWidth));
            currentLevelData.IsCrusherAvailable = EditorGUILayout.Toggle(currentLevelData.IsCrusherAvailable);
            GUILayout.EndHorizontal();
            
			GUILayout.Space(10);
			
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Bomb available", "Is the bomb power-up available to purchase on this level?"),
                GUILayout.Width(EditorGUIUtility.labelWidth));
            currentLevelData.IsBombAvailable = EditorGUILayout.Toggle(currentLevelData.IsBombAvailable);
            GUILayout.EndHorizontal();
            
			GUILayout.Space(10);
			
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Swap available", "Is the swap power-up available to purchase on this level?"),
                GUILayout.Width(EditorGUIUtility.labelWidth));
            currentLevelData.IsSwapAvailable = EditorGUILayout.Toggle(currentLevelData.IsSwapAvailable);
            GUILayout.EndHorizontal();
            
			GUILayout.Space(10);
			
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Color bomb available", "Is the color bomb power-up available to purchase on this level?"),
                GUILayout.Width(EditorGUIUtility.labelWidth));
            currentLevelData.IsColorBombAvailable = EditorGUILayout.Toggle(currentLevelData.IsColorBombAvailable);
            GUILayout.EndHorizontal();
            
			GUILayout.Space(10);
			
            EditorGUIUtility.labelWidth = oldLabelWidth;
			
			GUILayout.EndVertical();
			
			GUILayout.EndHorizontal();
			
			GUILayout.EndVertical();
		}

		private void DrawLevelTiles()
		{
			GUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.Width(500));
			
			var style = new GUIStyle
			{
				fontSize = 20,
				fontStyle = FontStyle.Bold,
				normal = { textColor = Color.white }
			};
			EditorGUILayout.LabelField("Layout", style);
			
			GUILayout.Space(10);
			
			GUILayout.BeginHorizontal();
			EditorGUILayout.LabelField(new GUIContent("Brush type", "The type of brush to paint the level."),
				GUILayout.Width(EditorGUIUtility.labelWidth));
			currentBrushType = (BrushType)EditorGUILayout.EnumPopup(currentBrushType, GUILayout.Width(100));
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			EditorGUILayout.LabelField(new GUIContent("Brush mode", "The brush mode to paint the level."),
				GUILayout.Width(EditorGUIUtility.labelWidth));
			currentBrushMode = (BrushMode)EditorGUILayout.EnumPopup(currentBrushMode, GUILayout.Width(100));
			GUILayout.EndHorizontal();

			switch (currentBrushType)
			{
				case BrushType.Tile:
					DrawTileSettings();
					break;
				
				case BrushType.RandomTile:
					DrawRandomTileSettings();
					break;
				
				case BrushType.Slot:
					DrawSlotSettings();
					break;
				
				case BrushType.Blocker:
					DrawBlockerSettings();
					break;
				
				case BrushType.Collectible:
					DrawCollectibleSettings();
					break;
			
				case BrushType.Random:
				case BrushType.Hole:
				case BrushType.Empty:
					break;
			}
			
			if (GUILayout.Button("Randomize", GUILayout.Width(100)))
				RandomizeLevel();
			
			if (GUILayout.Button("Clear", GUILayout.Width(100)))
				ClearLevel();

			if (currentLevelData.Tiles != null)
			{
				GUILayout.BeginVertical();
				for (var j = 0; j < currentLevelData.Height; j++)
				{
					GUILayout.BeginHorizontal();
					for (var i = 0; i < currentLevelData.Width; i++)
					{
						CreateButton(i, j);
					}
					GUILayout.EndHorizontal();
				}

				GUILayout.EndVertical();
			}
			else
			{
				var size = currentLevelData.Width * currentLevelData.Height;
				currentLevelData.Tiles = new List<LevelTileData>(size);
				for (var i = 0; i < size; i++)
					currentLevelData.Tiles.Add(new LevelTileData());
			}
			
			GUILayout.EndVertical();
		}

		private void DrawTileSettings()
		{
			GUILayout.BeginHorizontal();
			EditorGUILayout.LabelField(new GUIContent("Color tile type", "The type of color tile to use."),
				GUILayout.Width(EditorGUIUtility.labelWidth));
			currentColorTileType =
				(ColorTileType) EditorGUILayout.EnumPopup(currentColorTileType, GUILayout.Width(100));
			GUILayout.EndHorizontal();
		}

		private void DrawRandomTileSettings()
		{
			GUILayout.BeginHorizontal();
			EditorGUILayout.LabelField(
				new GUIContent("Random color tile type", "The type of random color tile to use."),
				GUILayout.Width(EditorGUIUtility.labelWidth));
			currentRandomColorTileType =
				(RandomColorTileType) EditorGUILayout.EnumPopup(currentRandomColorTileType, GUILayout.Width(100));
			GUILayout.EndHorizontal();
		}

		private void DrawSlotSettings()
		{
			GUILayout.BeginHorizontal();
			EditorGUILayout.LabelField(new GUIContent("Slot type", "The type of slot to use."),
				GUILayout.Width(EditorGUIUtility.labelWidth));
			currentSlotType =
				(SlotType)EditorGUILayout.EnumPopup(currentSlotType, GUILayout.Width(100));
			GUILayout.EndHorizontal();
		}

		private void DrawBlockerSettings()
		{
			GUILayout.BeginHorizontal();
			EditorGUILayout.LabelField(new GUIContent("Blocker type", "The type of blocker to use."),
				GUILayout.Width(EditorGUIUtility.labelWidth));
			currentBlockerType =
				(BlockerType)EditorGUILayout.EnumPopup(currentBlockerType, GUILayout.Width(100));
			GUILayout.EndHorizontal();
		}
		
		private void DrawCollectibleSettings()
		{
			GUILayout.BeginHorizontal();
			EditorGUILayout.LabelField(new GUIContent("Collectible type", "The type of collectible to use."),
				GUILayout.Width(EditorGUIUtility.labelWidth));
			currentCollectibleType =
				(CollectibleType)EditorGUILayout.EnumPopup(currentCollectibleType, GUILayout.Width(100));
			GUILayout.EndHorizontal();
		}

		private void CreateButton(int row, int column)
		{
			var idx = row + column * currentLevelData.Width;

			var texName = string.Empty;
			if (currentLevelData.Tiles[idx].TileType == TileType.Empty)
			{
				switch (currentLevelData.Tiles[idx].SlotType)
				{
					case SlotType.Ice:
						texName += "_Ice";
						break;

					case SlotType.Jelly:
						texName += "_Jelly";
						break;
					
					case SlotType.Ice2:
						texName += "_Ice 1";
						break;
					
					case SlotType.Ice3:
						texName += "_Ice 2";
						break;
						
					case SlotType.Jelly2:
						texName += "_Jelly 1";
						break;
					
					case SlotType.Jelly3:
						texName += "_Jelly 2";
						break;
				}

				if (string.IsNullOrEmpty(texName))
				{
					if (GUILayout.Button("", GUILayout.Width(LevelTileButtonSize), GUILayout.Height(LevelTileButtonSize)))
						DrawTile(row, column);
				}
				else
				{
					if (GUILayout.Button(tileTextures[texName], GUILayout.Width(LevelTileButtonSize), GUILayout.Height(LevelTileButtonSize)))
						DrawTile(row, column);
				}
				
				return;
			}

			switch (currentLevelData.Tiles[idx].TileType)
			{
				case TileType.Color:
					texName = currentLevelData.Tiles[idx].ColorTileType.ToString();
					break;
				
				case TileType.RandomColor:
					texName = currentLevelData.Tiles[idx].RandomColorTileType.ToString();
					break;
				
				case TileType.Blocker:
					texName = currentLevelData.Tiles[idx].BlockerType.ToString();
					break;
				
				case TileType.Collectible:
					texName = currentLevelData.Tiles[idx].CollectibleType.ToString();
					break;
				
				case TileType.Hole:
					texName = "Empty";
					break;
				
				case TileType.Random:
					texName = "Random";
					break;
			}

			switch (currentLevelData.Tiles[idx].SlotType)
			{
				case SlotType.Ice:
					texName += "_Ice";
					break;

				case SlotType.Jelly:
					texName += "_Jelly";
					break;
					
				case SlotType.Ice2:
					texName += "_Ice 1";
					break;
					
				case SlotType.Ice3:
					texName += "_Ice 2";
					break;
						
				case SlotType.Jelly2:
					texName += "_Jelly 1";
					break;
					
				case SlotType.Jelly3:
					texName += "_Jelly 2";
					break;
			}

			if (GUILayout.Button(tileTextures[texName], GUILayout.Width(LevelTileButtonSize), GUILayout.Height(LevelTileButtonSize)))
				DrawTile(row, column);
		}

		private void DrawTile(int row, int column)
		{
			var width = currentLevelData.Width;
			var height = currentLevelData.Height;

			Action<int> onDrawCallback = null;
			switch (currentBrushType)
			{
				case BrushType.Tile:
					onDrawCallback = i =>
					{
						currentLevelData.Tiles[i].TileType = TileType.Color;
						currentLevelData.Tiles[i].ColorTileType = currentColorTileType;
					};
					break;
				
				case BrushType.RandomTile:
					onDrawCallback = i =>
					{
						currentLevelData.Tiles[i].TileType = TileType.RandomColor;
						currentLevelData.Tiles[i].RandomColorTileType = currentRandomColorTileType;
					};
					break;
				
				case BrushType.Slot:
					onDrawCallback = i =>
					{
						currentLevelData.Tiles[i].SlotType = currentSlotType;
					};
					break;
				
				case BrushType.Blocker:
					onDrawCallback = i =>
					{
						currentLevelData.Tiles[i].TileType = TileType.Blocker;
						currentLevelData.Tiles[i].BlockerType = currentBlockerType;
						currentLevelData.Tiles[i].SlotType = SlotType.Normal;
					};
					break;
				
				case BrushType.Collectible:
					onDrawCallback = i =>
					{
						currentLevelData.Tiles[i].TileType = TileType.Collectible;
						currentLevelData.Tiles[i].CollectibleType = currentCollectibleType;
					};
					break;
					
				case BrushType.Hole:
					onDrawCallback = i =>
					{
						currentLevelData.Tiles[i].TileType = TileType.Hole;
						currentLevelData.Tiles[i].SlotType = SlotType.Normal;
					};
					break;
				
				case BrushType.Empty:
					onDrawCallback = i =>
					{
						currentLevelData.Tiles[i].TileType = TileType.Empty;
						currentLevelData.Tiles[i].SlotType = SlotType.Normal;
					};
					break;
				
				case BrushType.Random:
					onDrawCallback = i =>
					{
						currentLevelData.Tiles[i].TileType = TileType.Random;
						currentLevelData.Tiles[i].SlotType = SlotType.Normal;
					};
					break;
			}
			
			Assert.IsNotNull(onDrawCallback);
			
			switch (currentBrushMode)
			{
				case BrushMode.Single:
				{
					var idx = row + column * width;
					onDrawCallback(idx);
				}
					break;

				case BrushMode.Row:
					for (var i = 0; i < width; i++)
					{
						var idx = i + column * width;
						onDrawCallback(idx);
					}
					break;

				case BrushMode.Column:
					for (var j = 0; j < height; j++)
					{
						var idx = row + j * width;
						onDrawCallback(idx);
					}
					break;

				case BrushMode.Fill:
					for (var j = 0; j < height; j++)
					{
						for (var i = 0; i < width; i++)
						{
							var idx = i + j * width;
							onDrawCallback(idx);
						}
					}
					break;
			}
		}

		private void ResetLevelTiles()
		{
			currentLevelData.Tiles.Clear();
			var size = currentLevelData.Width * currentLevelData.Height;
			for (var i = 0; i < size; i++)
				currentLevelData.Tiles.Add(new LevelTileData { TileType = TileType.Empty });
		}
		
		private void CreateAvailableColorsList()
		{
			availableColorsList = SetupReorderableList("Available colors", currentLevelData.AvailableColors, ref currentColor, (rect, x) =>
				{
					EditorGUI.LabelField(new Rect(rect.x, rect.y, 200, EditorGUIUtility.singleLineHeight),
						x.ToString());
				},
				(x) => { currentColor = x; },
				() =>
				{
					var menu = new GenericMenu();
					foreach (var color in Enum.GetValues(typeof(ColorTileType)))
					{
						var isUsed = currentLevelData.AvailableColors.Contains((ColorTileType)color);
						if (isUsed)
							menu.AddDisabledItem(new GUIContent(color.ToString()));
						else
							menu.AddItem(new GUIContent(color.ToString()), false, CreateColorCallback, color);
					}
					menu.ShowAsContext();
				},
				(x) => { currentColor = ColorTileType.Color1; });
		}
		
		private void CreateColorCallback(object obj)
		{
			currentLevelData.AvailableColors.Add((ColorTileType)obj);
		}

		private void CreateGoalsList()
		{
			goalList = SetupReorderableList("Goals", currentLevelData.Goals, ref currentGoal, (rect, x) =>
				{
					EditorGUI.LabelField(new Rect(
							rect.x,
							rect.y,
							200,
							EditorGUIUtility.singleLineHeight),
							x.ToString());
				},
				(x) =>
				{
					currentGoal = x;
				},
				() =>
				{
					var menu = new GenericMenu();
					menu.AddItem(
						new GUIContent("Collect tiles"),
						false,
						CreateGoalCallback,
						GoalType.CollectTiles);
					menu.AddItem(
						new GUIContent("Collect random tiles"),
						false,
						CreateGoalCallback,
						GoalType.CollectRandomTiles);
					menu.AddItem(
						new GUIContent("Collect slots"),
						false,
						CreateGoalCallback,
						GoalType.CollectSlots);
					menu.AddItem(
						new GUIContent("Collect blockers"),
						false,
						CreateGoalCallback,
						GoalType.CollectBlockers);
					menu.AddItem(
						new GUIContent("Collect collectibles"),
						false,
						CreateGoalCallback,
						GoalType.CollectCollectibles);
					menu.ShowAsContext();
				},
				(x) =>
				{
					currentGoal = null;
				});
		}
		
		private void CreateGoalCallback(object obj)
		{
			var goal = new LevelGoalData
			{
				Type = (GoalType) obj
			};
			currentLevelData.Goals.Add(goal);
		}
		
		private void DrawAvailableColors()
		{
			GUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.Width(300));
			
			var style = new GUIStyle
			{
				fontSize = 20,
				fontStyle = FontStyle.Bold,
				normal = { textColor = Color.white }
			};
			EditorGUILayout.LabelField("Available colors", style);
			
			GUILayout.Space(10);
			
			GUILayout.BeginHorizontal(GUILayout.Width(300));
			EditorGUILayout.HelpBox(
				"This list defines the available tile colors in this level.",
				MessageType.Info);
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();

			GUILayout.BeginVertical(GUILayout.Width(300));
			availableColorsList?.DoLayoutList();
			GUILayout.EndVertical();

			GUILayout.EndHorizontal();
			
			GUILayout.EndVertical();
		}

		private void DrawGoals()
		{
			GUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.Width(400));
			
			var style = new GUIStyle
			{
				fontSize = 20,
				fontStyle = FontStyle.Bold,
				normal = { textColor = Color.white }
			};
			EditorGUILayout.LabelField("Goals", style);
			
			GUILayout.Space(10);
			
			GUILayout.BeginHorizontal(GUILayout.Width(300));
			EditorGUILayout.HelpBox(
				"This list defines the goals needed to be achieved by the player in order to complete this level.",
				MessageType.Info);
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();

			GUILayout.BeginVertical(GUILayout.Width(200));
			goalList?.DoLayoutList();
			GUILayout.EndVertical();

			if (currentGoal != null)
				DrawGoal(currentGoal);

			GUILayout.EndHorizontal();
			
			GUILayout.EndVertical();
		}

		private void DrawGoal(LevelGoalData goal)
		{
			var oldLabelWidth = EditorGUIUtility.labelWidth;
			EditorGUIUtility.labelWidth = 60;

			GUILayout.BeginVertical();

			GUILayout.BeginHorizontal();
			EditorGUILayout.PrefixLabel("Type");
			switch (goal.Type)
			{
				case GoalType.CollectTiles:
					goal.ColorTileType = (ColorTileType)EditorGUILayout.EnumPopup(goal.ColorTileType, GUILayout.Width(100));
					break;
				
				case GoalType.CollectRandomTiles:
					goal.RandomColorTileType = (RandomColorTileType)EditorGUILayout.EnumPopup(goal.RandomColorTileType, GUILayout.Width(100));
					break;
				
				case GoalType.CollectSlots:
					goal.SlotType = (SlotType)EditorGUILayout.EnumPopup(goal.SlotType, GUILayout.Width(100));
					break;
				
				case GoalType.CollectBlockers:
					goal.BlockerType = (BlockerType)EditorGUILayout.EnumPopup(goal.BlockerType, GUILayout.Width(100));
					break;
				
				case GoalType.CollectCollectibles:
					goal.CollectibleType = (CollectibleType)EditorGUILayout.EnumPopup(goal.CollectibleType, GUILayout.Width(100));
					break;
			}
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			EditorGUILayout.PrefixLabel("Amount");
			goal.Amount = EditorGUILayout.IntField(goal.Amount, GUILayout.Width(30));
			GUILayout.EndHorizontal();

			GUILayout.EndVertical();

			EditorGUIUtility.labelWidth = oldLabelWidth;
		}
		
		private void RandomizeLevel()
		{
			foreach (var tile in currentLevelData.Tiles)
			{
				var rnd = UnityEngine.Random.Range(0, currentLevelData.AvailableColors.Count);
				tile.TileType = TileType.RandomColor;
				tile.RandomColorTileType = (RandomColorTileType)rnd;
				tile.SlotType = SlotType.Normal;
			}
		}
		
		private void ClearLevel()
		{
			foreach (var tile in currentLevelData.Tiles)
			{
				tile.TileType = TileType.Empty;
				tile.SlotType = SlotType.Normal;
			}
		}
	}
}
