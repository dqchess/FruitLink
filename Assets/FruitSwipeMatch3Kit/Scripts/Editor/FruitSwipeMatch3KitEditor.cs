// Copyright (C) 2019 gamevanilla. All rights reserved.
// This code can only be used under the standard Unity Asset Store EULA,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace FruitSwipeMatch3Kit
{
    /// <summary>
    /// The custom editor window for Fruit Swipe Match 3 Kit.
    /// </summary>
    public class FruitSwipeMatch3KitEditor : EditorWindow
    {
        private readonly List<EditorTab> tabs = new List<EditorTab>();

        private int selectedTabIndex = -1;
        private int prevSelectedTabIndex = -1;

        [MenuItem("Tools/Fruit Swipe Match 3 Kit/Editor", false, 0)]
        private static void Init()
        {
            var window = GetWindow(typeof(FruitSwipeMatch3KitEditor));
            window.titleContent = new GUIContent("Fruit Swipe Match 3 Kit Editor");
        }

        private void OnEnable()
        {
            tabs.Add(new GameSettingsTab(this));
            tabs.Add(new LevelEditorTab(this));
            tabs.Add(new AboutTab(this));
            selectedTabIndex = 0;
        }

        private void OnGUI()
        {
            selectedTabIndex = GUILayout.Toolbar(selectedTabIndex,
                new[] {"Game settings", "Level editor", "About"});
            if (selectedTabIndex >= 0 && selectedTabIndex < tabs.Count)
            {
                var selectedEditor = tabs[selectedTabIndex];
                if (selectedTabIndex != prevSelectedTabIndex)
                {
                    selectedEditor.OnTabSelected();
                    GUI.FocusControl(null);
                }
                selectedEditor.Draw();
                prevSelectedTabIndex = selectedTabIndex;
            }
        }
    }
}
