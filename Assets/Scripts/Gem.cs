using UnityEngine;

namespace Match3 {
    [RequireComponent(typeof(SpriteRenderer))]
    public class Gem : MonoBehaviour {
        [SerializeField] private GemType type;
        public bool isSpecial { get; set; }
        public SpecialGemType specialGemType { get; set; }

        public GemType Type => type;

        public void SetType(GemType type) {
            this.type = type;
            GetComponent<SpriteRenderer>().sprite = type.sprite;
        }
        public void DestroyGem() => Destroy(gameObject);

    }
}