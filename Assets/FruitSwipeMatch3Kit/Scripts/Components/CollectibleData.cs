// Copyright (C) 2019 gamevanilla. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using System;
using Unity.Entities;

namespace FruitSwipeMatch3Kit
{
    [Serializable]
    public struct CollectibleData : IComponentData
    {
        public CollectibleType Type;
    }
}
