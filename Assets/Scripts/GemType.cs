using UnityEngine;

public enum Type
{
    Normal,
    Special
    
}

public enum SpecialGemType {
    None,
    Bomb,
    Lines,
    Magic
}

namespace Match3 {
    [CreateAssetMenu(fileName = "GemType", menuName = "Match3/GemType")]
    public class GemType : ScriptableObject {
        public Sprite sprite;
        public Type type;
        public SpecialGemType specialGemType;
    }
}