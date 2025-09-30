using UnityEngine;

[CreateAssetMenu(fileName = "LevelModifier", menuName = "Game/Level Modifier")]
public class LevelModifierData : ScriptableObject
{
    public string modifierName;
    public Sprite modifierIcon;
    [TextArea(3, 5)]
    public string description;
}