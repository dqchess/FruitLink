// Copyright (C) 2019 gamevanilla. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using System;

namespace FruitSwipeMatch3Kit
{
    /// <summary>
    /// Helper class to define device-specific resolution settings.
    /// </summary>
    [Serializable]
    public class ResolutionOverride
    {
        public string Name;
        public int Width;
        public int Height;
        public float ZoomLevel;
        public float CanvasScalingMatch;
    }
}