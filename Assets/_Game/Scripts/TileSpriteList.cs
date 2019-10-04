using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class TileSpriteList : ScriptableObject
{
    public List<Sprite> colorTileSprites;       
    public List<Sprite> slotSprites;        
    public List<Sprite> blockerTileSprites;        
    public List<Sprite> collectibleTileSprites;
}
