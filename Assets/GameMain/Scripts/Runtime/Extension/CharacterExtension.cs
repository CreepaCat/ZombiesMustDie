using UnityEngine;

namespace ZombiesMustDie
{
    public static class CharacterExtension
    {

        public static string GetChineseName(this DRCharacter character)
        {
            switch (character.Name)
            {
                case "Infantry":
                    return "步兵";
                case "Sniper":
                    return "狙击手";
                case "Engineer":
                    return "工程师";
                default:
                    return string.Empty;
            }
        }

    }
}
