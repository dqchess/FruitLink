// Copyright (C) 2019 gamevanilla. All rights reserved.
// This code can only be used under the standard Unity Asset Store EULA,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

using Object = UnityEngine.Object;

namespace FruitSwipeMatch3Kit
{
	/// <summary>
	/// The 'Game settings' tab in the editor.
	/// </summary>
	public class GameSettingsTab : EditorTab
	{
		private Object gameConfigurationDbObj;
		private GameConfiguration gameConfig;

		private int selectedTabIndex;
		private Vector2 scrollPos;
		
		private ReorderableList resolutionOverridesList;
		private ResolutionOverride currentResolutionOverride;
		
        private ReorderableList iapItemsList;
        private IapItem currentIapItem;
		
		private int newLevel;
		
		private const string EditorPath = "fruit_swipe_editor_path";
		
		public GameSettingsTab(FruitSwipeMatch3KitEditor editor) : base(editor)
		{
			var path = EditorPrefs.GetString(EditorPath);
			if (!string.IsNullOrEmpty(path))
			{
				gameConfigurationDbObj = AssetDatabase.LoadAssetAtPath(path, typeof(GameConfiguration));
				gameConfig = (GameConfiguration)gameConfigurationDbObj;
				CreateResolutionOverridesList();
                CreateIapItemsList();
			}
			
            newLevel = PlayerPrefs.GetInt("next_level");
		}

		public override void Draw()
		{
			scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

            var oldLabelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 100;

            GUILayout.Space(15);

			var oldDb = gameConfigurationDbObj;
			gameConfigurationDbObj = EditorGUILayout.ObjectField("Asset", gameConfigurationDbObj, typeof(GameConfiguration), false, GUILayout.Width(340));
			if (gameConfigurationDbObj != oldDb)
			{
				gameConfig = (GameConfiguration)gameConfigurationDbObj;
				CreateResolutionOverridesList();
                CreateIapItemsList();
				EditorPrefs.SetString(EditorPath, AssetDatabase.GetAssetPath(gameConfigurationDbObj));
			}

			if (gameConfig != null)
			{
				GUILayout.Space(15);

				var prevSelectedIndex = selectedTabIndex;
				selectedTabIndex = GUILayout.Toolbar(selectedTabIndex,
					new[] {"Game", "Resolutions", "Monetization", "Player preferences"}, GUILayout.Width(500));

				if (selectedTabIndex != prevSelectedIndex)
					GUI.FocusControl(null);

				if (selectedTabIndex == 0)
					DrawGameTab();
				else if (selectedTabIndex == 1)
					DrawResolutionsTab();
				else if (selectedTabIndex == 2)
					DrawMonetizationTab();
				else
					DrawPreferencesTab();
			}

			EditorGUIUtility.labelWidth = oldLabelWidth;
			EditorGUILayout.EndScrollView();

			if (GUI.changed)
			{
				EditorUtility.SetDirty(gameConfig);
			}
		}
		
		private void CreateResolutionOverridesList()
		{
			resolutionOverridesList = SetupReorderableList("Resolution overrides", gameConfig.ResolutionOverrides,
				ref currentResolutionOverride, (rect, x) =>
				{
					EditorGUI.LabelField(new Rect(rect.x, rect.y, 200, EditorGUIUtility.singleLineHeight), x.Name);
				},
				(x) =>
				{
					currentResolutionOverride = x;
				},
				() =>
				{
					var newOverride = new ResolutionOverride();
					gameConfig.ResolutionOverrides.Add(newOverride);
				},
				(x) =>
				{
					currentResolutionOverride = null;
				});
		}

		private void CreateIapItemsList()
		{
            iapItemsList = SetupReorderableList("In-app purchase items", gameConfig.IapItems,
                ref currentIapItem, (rect, x) =>
                {
                    EditorGUI.LabelField(new Rect(rect.x, rect.y, 350, EditorGUIUtility.singleLineHeight), x.StoreId);
                },
                (x) =>
                {
                    currentIapItem = x;
                },
                () =>
                {
                    var newItem = new IapItem();
                    gameConfig.IapItems.Add(newItem);
                },
                (x) =>
                {
                    currentIapItem = null;
                });
		}

		private void DrawGameTab()
		{
			GUILayout.Space(15);
			DrawScoreSettings();
			GUILayout.Space(15);
			DrawLivesSettings();
			GUILayout.Space(15);
			DrawCoinsSettings();
			GUILayout.Space(15);
			DrawBoosterSettings();
			GUILayout.Space(15);
            DrawInGamePowerupSettings();
			GUILayout.Space(15);
			DrawContinueGameSettings();
			GUILayout.Space(15);
		}
		
		/// <summary>
        /// Draws the resolutions tab.
        /// </summary>
        private void DrawResolutionsTab()
        {
            if (gameConfig != null)
            {
                GUILayout.Space(15);
                
                var oldLabelWidth = EditorGUIUtility.labelWidth;
                EditorGUIUtility.labelWidth = 180;
                
                EditorGUILayout.LabelField("Resolution settings", EditorStyles.boldLabel);
                GUILayout.BeginHorizontal(GUILayout.Width(300));
                const string helpText =
                    "The resolution settings of the game. Here you can define device-specific values for the camera zoom and the canvas scaler to be used in the game.";
                EditorGUILayout.HelpBox(helpText, MessageType.Info);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(new GUIContent("Default zoom level", "The default camera zoom to use in the game screen."),
                    GUILayout.Width(EditorGUIUtility.labelWidth));
                gameConfig.DefaultZoomLevel = EditorGUILayout.FloatField(gameConfig.DefaultZoomLevel, GUILayout.Width(70));
                GUILayout.EndHorizontal();
                
                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(new GUIContent("Default canvas scaling match", "The default scaling match value to use in the canvas scaler."),
                    GUILayout.Width(EditorGUIUtility.labelWidth));
                gameConfig.DefaultCanvasScalingMatch = EditorGUILayout.FloatField(gameConfig.DefaultCanvasScalingMatch, GUILayout.Width(70));
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                
                EditorGUIUtility.labelWidth = oldLabelWidth;

                GUILayout.BeginVertical(GUILayout.Width(350));
                if (resolutionOverridesList != null)
                    resolutionOverridesList.DoLayoutList();

                GUILayout.EndVertical();

                if (currentResolutionOverride != null)
                    DrawResolutionOverride(currentResolutionOverride);

                GUILayout.EndHorizontal();
            }
        }
		
		private void DrawResolutionOverride(ResolutionOverride resolution)
		{
			var oldLabelWidth = EditorGUIUtility.labelWidth;
			EditorGUIUtility.labelWidth = 150;

			GUILayout.BeginVertical();
                
			GUILayout.BeginHorizontal();
			EditorGUILayout.PrefixLabel("Name");
			resolution.Name = EditorGUILayout.TextField(resolution.Name, GUILayout.Width(200));
			GUILayout.EndHorizontal();
            
			GUILayout.BeginHorizontal();
			EditorGUILayout.PrefixLabel("Width");
			resolution.Width = EditorGUILayout.IntField(resolution.Width, GUILayout.Width(70));
			GUILayout.EndHorizontal();
            
			GUILayout.BeginHorizontal();
			EditorGUILayout.PrefixLabel("Height");
			resolution.Height = EditorGUILayout.IntField(resolution.Height, GUILayout.Width(70));
			GUILayout.EndHorizontal();
            
			GUILayout.BeginHorizontal();
			EditorGUILayout.PrefixLabel("Zoom level");
			resolution.ZoomLevel = EditorGUILayout.FloatField(resolution.ZoomLevel, GUILayout.Width(30));
			GUILayout.EndHorizontal();
            
			GUILayout.BeginHorizontal();
			EditorGUILayout.PrefixLabel("Canvas scaling match");
			resolution.CanvasScalingMatch = EditorGUILayout.FloatField(resolution.CanvasScalingMatch, GUILayout.Width(30));
			GUILayout.EndHorizontal();
            
			GUILayout.EndVertical();
            
			EditorGUIUtility.labelWidth = oldLabelWidth;
		}

		private void DrawMonetizationTab()
		{
			GUILayout.Space(15);
			DrawRewardedAdSettings();
			GUILayout.Space(15);
			DrawIapSettings();
		}

		private void DrawPreferencesTab()
		{
			DrawPreferencesSettings();
		}

		private void DrawScoreSettings()
		{
            EditorGUILayout.LabelField("Score", EditorStyles.boldLabel);
            GUILayout.BeginHorizontal(GUILayout.Width(300));
            const string helpText =
                "The score given to the player when a tile explodes.";
            EditorGUILayout.HelpBox(helpText, MessageType.Info);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Default score", "The default score of tiles."),
                GUILayout.Width(EditorGUIUtility.labelWidth));
            gameConfig.DefaultTileScore = EditorGUILayout.IntField(gameConfig.DefaultTileScore, GUILayout.Width(70));
            GUILayout.EndHorizontal();
		}

		private void DrawLivesSettings()
		{
			EditorGUILayout.LabelField("Lives", EditorStyles.boldLabel);
			GUILayout.BeginHorizontal(GUILayout.Width(300));
			EditorGUILayout.HelpBox(
				"The settings related to the lives system.", MessageType.Info);
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			EditorGUILayout.LabelField(new GUIContent("Max lives",
					"The maximum number of lives that the player can have."),
				GUILayout.Width(EditorGUIUtility.labelWidth));
			gameConfig.MaxLives = EditorGUILayout.IntField(gameConfig.MaxLives, GUILayout.Width(30));
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			EditorGUILayout.LabelField(new GUIContent("Time to next life",
					"The number of seconds that need to pass before the player is given a free life."),
				GUILayout.Width(EditorGUIUtility.labelWidth));
			gameConfig.TimeToNextLife = EditorGUILayout.IntField(gameConfig.TimeToNextLife, GUILayout.Width(70));
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			EditorGUILayout.LabelField(new GUIContent("Refill cost",
					"The cost in coins of refilling the lives of the player up to its maximum number."),
				GUILayout.Width(EditorGUIUtility.labelWidth));
			gameConfig.LivesRefillCost = EditorGUILayout.IntField(gameConfig.LivesRefillCost, GUILayout.Width(70));
			GUILayout.EndHorizontal();
		}
		
        private void DrawCoinsSettings()
        {
            EditorGUILayout.LabelField("Coins", EditorStyles.boldLabel);
            GUILayout.BeginHorizontal(GUILayout.Width(300));
            EditorGUILayout.HelpBox(
                "The settings related to the coins system.", MessageType.Info);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Initial coins",
                    "The initial number of coins given to the player."),
                GUILayout.Width(EditorGUIUtility.labelWidth));
            gameConfig.InitialCoins = EditorGUILayout.IntField(gameConfig.InitialCoins, GUILayout.Width(70));
            GUILayout.EndHorizontal();
        }

        private void DrawBoosterSettings()
        {
            EditorGUILayout.LabelField("Boosters", EditorStyles.boldLabel);
            GUILayout.BeginHorizontal(GUILayout.Width(300));
            EditorGUILayout.HelpBox(
                "The settings related to the boosters.", MessageType.Info);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Regular booster",
                    "The number of tiles needed to obtain a regular booster."),
                GUILayout.Width(EditorGUIUtility.labelWidth));
            gameConfig.NumTilesNeededForRegularBooster = EditorGUILayout.IntField(gameConfig.NumTilesNeededForRegularBooster, GUILayout.Width(70));
            GUILayout.EndHorizontal();
            
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Cross booster",
                    "The number of tiles needed to obtain a cross booster."),
                GUILayout.Width(EditorGUIUtility.labelWidth));
            gameConfig.NumTilesNeededForCrossBooster = EditorGUILayout.IntField(gameConfig.NumTilesNeededForCrossBooster, GUILayout.Width(70));
            GUILayout.EndHorizontal();
            
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Star booster",
                    "The number of tiles needed to obtain a star booster."),
                GUILayout.Width(EditorGUIUtility.labelWidth));
            gameConfig.NumTilesNeededForStarBooster = EditorGUILayout.IntField(gameConfig.NumTilesNeededForStarBooster, GUILayout.Width(70));
            GUILayout.EndHorizontal();
        }

		private void DrawInGamePowerupSettings()
		{
            EditorGUILayout.LabelField("Power-ups", EditorStyles.boldLabel);
            GUILayout.BeginHorizontal(GUILayout.Width(300));
            EditorGUILayout.HelpBox(
                "The settings related to the power-ups that can be purchased by the player in-game.", MessageType.Info);
            GUILayout.EndHorizontal();

			var oldLabelWidth = EditorGUIUtility.labelWidth;
			DrawInGameBooster("Crusher", ref gameConfig.CrusherPowerupAmount, ref gameConfig.CrusherPowerupPrice);
			DrawInGameBooster("Bomb", ref gameConfig.BombPowerupAmount, ref gameConfig.BombPowerupPrice);
			DrawInGameBooster("Swap", ref gameConfig.SwapPowerupAmount, ref gameConfig.SwapPowerupPrice);
			DrawInGameBooster("Color bomb", ref gameConfig.ColorBombPowerupAmount, ref gameConfig.ColorBombPowerupPrice);
			EditorGUIUtility.labelWidth = oldLabelWidth;
		}

		private void DrawInGameBooster(string boosterName, ref int boosterAmount, ref int boosterPrice)
		{
			EditorGUIUtility.labelWidth = 150;

			GUILayout.BeginHorizontal();
			EditorGUILayout.PrefixLabel($"{boosterName} amount");
			boosterAmount = EditorGUILayout.IntField(boosterAmount, GUILayout.Width(30));

			GUILayout.Space(15);
			
			EditorGUIUtility.labelWidth = 140;

			EditorGUILayout.PrefixLabel($"{boosterName} price");
			boosterPrice = EditorGUILayout.IntField(boosterPrice, GUILayout.Width(70));
			GUILayout.EndHorizontal();
		}
		
        private void DrawContinueGameSettings()
        {
            EditorGUILayout.LabelField("Continue game", EditorStyles.boldLabel);
            GUILayout.BeginHorizontal(GUILayout.Width(300));
            EditorGUILayout.HelpBox(
                "The settings related to the options given to the player when losing a game.", MessageType.Info);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Extra moves",
                    "The number of extra moves that can be purchased by the player when a game is lost."),
                GUILayout.Width(EditorGUIUtility.labelWidth));
            gameConfig.NumExtraMoves = EditorGUILayout.IntField(gameConfig.NumExtraMoves, GUILayout.Width(30));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Moves cost",
                    "The cost in coins of the extra moves."),
                GUILayout.Width(EditorGUIUtility.labelWidth));
            gameConfig.ExtraMovesCost = EditorGUILayout.IntField(gameConfig.ExtraMovesCost, GUILayout.Width(70));
            GUILayout.EndHorizontal();
        }
		
		private void DrawRewardedAdSettings()
		{
            EditorGUILayout.LabelField("Rewarded ad", EditorStyles.boldLabel);
            GUILayout.BeginHorizontal(GUILayout.Width(300));
            const string helpText =
                "The number of coins given to the player after watching a rewarded ad.";
            EditorGUILayout.HelpBox(helpText, MessageType.Info);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Coins", "The number of coins awarded to the player after watching a rewarded ad."),
                GUILayout.Width(EditorGUIUtility.labelWidth));
            gameConfig.RewardedAdCoins =
                EditorGUILayout.IntField(gameConfig.RewardedAdCoins, GUILayout.Width(70));
            GUILayout.EndHorizontal();
		}

        private void DrawIapSettings()
        {
            EditorGUILayout.LabelField("In-app purchases", EditorStyles.boldLabel);
            GUILayout.BeginHorizontal(GUILayout.Width(300));
            const string helpText =
                "The settings of your in-game purchasable items.";
            EditorGUILayout.HelpBox(helpText, MessageType.Info);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();

            GUILayout.BeginVertical(GUILayout.Width(350));
	        iapItemsList?.DoLayoutList();
	        GUILayout.EndVertical();

            if (currentIapItem != null)
                DrawIapItem(currentIapItem);

            GUILayout.EndHorizontal();
        }
		
		private void DrawPreferencesSettings()
		{
			GUILayout.Space(15);
			
            EditorGUILayout.LabelField("Level", EditorStyles.boldLabel);
		    GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(new GUIContent("Level", "The current level number."),
                GUILayout.Width(50));
            newLevel = EditorGUILayout.IntField(newLevel, GUILayout.Width(50));
		    GUILayout.EndHorizontal();

		    if (GUILayout.Button("Set progress", GUILayout.Width(120), GUILayout.Height(30)))
		        PlayerPrefs.SetInt("next_level", newLevel);

		    GUILayout.Space(15);

            EditorGUILayout.LabelField("PlayerPrefs", EditorStyles.boldLabel);
		    if (GUILayout.Button("Delete PlayerPrefs", GUILayout.Width(120), GUILayout.Height(30)))
		        PlayerPrefs.DeleteAll();

		    GUILayout.Space(15);

            EditorGUILayout.LabelField("EditorPrefs", EditorStyles.boldLabel);
		    if (GUILayout.Button("Delete EditorPrefs", GUILayout.Width(120), GUILayout.Height(30)))
		        EditorPrefs.DeleteAll();
		}
		
        private void DrawIapItem(IapItem item)
        {
            var oldLabelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 100;

            item.Draw();

            EditorGUIUtility.labelWidth = oldLabelWidth;
        }
	}
}
