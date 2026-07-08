using UnityEngine;

public static class SelectedCharacter
{
    public static PlayerCharacterData CharacterData { get; private set; }

    public static void Set(PlayerCharacterData characterData)
    {
        CharacterData = characterData;
    }
}
