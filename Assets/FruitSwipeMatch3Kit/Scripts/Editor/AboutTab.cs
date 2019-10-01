﻿// Copyright (C) 2019 gamevanilla. All rights reserved.
// This code can only be used under the standard Unity Asset Store EULA,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using UnityEditor;
using UnityEngine;

namespace FruitSwipeMatch3Kit
{
    /// <summary>
    /// The 'About' tab in the editor.
    /// </summary>
	public class AboutTab : EditorTab
	{
        private const string PurchaseText = "Thank you for your purchase!";
        private const string CopyrightText =
            "Fruit Swipe Match 3 Kit is brought to you by gamevanilla. Copyright (C) gamevanilla 2019.";
        private const string WikiUrl = "https://wiki.gamevanilla.com";
        private const string SupportUrl = "https://www.gamevanilla.com";
        private const string EulaUrl = "https://unity3d.com/es/legal/as_terms";
        private const string AssetStoreUrl = "https://www.assetstore.unity3d.com/#!/content/124730";

        private readonly Texture2D logoTexture;

        public AboutTab(FruitSwipeMatch3KitEditor editor) : base(editor)
        {
            logoTexture = Resources.Load<Texture2D>("Logo");
        }

        public override void Draw()
        {
            GUILayout.Space(15);

            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label(logoTexture);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();

            GUILayout.Space(15);

            var windowWidth = EditorWindow.focusedWindow.position.width;
            var centeredLabelStyle = new GUIStyle("label") {alignment = TextAnchor.MiddleCenter};
            GUI.Label(new Rect(0, 0, windowWidth, 650), PurchaseText, centeredLabelStyle);
            GUI.Label(new Rect(0, 0, windowWidth, 700), CopyrightText, centeredLabelStyle);
            var centeredButtonStyle = new GUIStyle("button") {alignment = TextAnchor.MiddleCenter};
            if (GUI.Button(new Rect(windowWidth / 2 - 100 / 2.0f, 400, 100, 50), "Documentation", centeredButtonStyle))
                Application.OpenURL(WikiUrl);
            else if (GUI.Button(new Rect(windowWidth / 2 - 100 / 2.0f, 460, 100, 50), "Support", centeredButtonStyle))
                Application.OpenURL(SupportUrl);
            else if (GUI.Button(new Rect(windowWidth / 2 - 100 / 2.0f, 520, 100, 50), "License", centeredButtonStyle))
                Application.OpenURL(EulaUrl);
            else if (GUI.Button(new Rect(windowWidth / 2 - 100 / 2.0f, 580, 100, 50), "Rate me!", centeredButtonStyle))
                Application.OpenURL(AssetStoreUrl);
        }
	}
}