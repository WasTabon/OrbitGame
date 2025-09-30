using UnityEngine;

[CreateAssetMenu(fileName = "ModifiersDatabase", menuName = "Game/Modifiers Database")]
public class ModifiersDatabase : ScriptableObject
{
    public LevelModifierData[] modifiers = new LevelModifierData[5];
    
    public LevelModifierData GetModifier(int index)
    {
        if (index >= 1 && index <= 5)
        {
            return modifiers[index - 1];
        }
        return null;
    }
}