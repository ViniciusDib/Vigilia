using UnityEngine;

namespace Vigilia
{
    /// <summary>
    /// Escolhe o sprite pela direção em que o personagem olha, para arte de corpo inteiro (vista alta, estilo Zelda),
    /// que não pode ser girada. Arquivos em Resources/Arte (docs/prompts-arte.md):
    ///   npc_marta.png        → olhando para cima (de costas)  [obrigatório]
    ///   npc_marta_sul.png    → olhando para baixo (de frente)  [opcional]
    ///   npc_marta_leste.png  → olhando para a direita          [opcional; se faltar, espelha o oeste]
    ///   npc_marta_oeste.png  → olhando para a esquerda         [opcional; se faltar, espelha o leste]
    /// A versão possuída segue o mesmo padrão com "_noite" antes da direção (npc_marta_noite_sul.png).
    /// Direção que falta usa a de costas, espelhada conforme o lado.
    /// </summary>
    public class SpriteDirecional : MonoBehaviour
    {
        public enum Dir { Norte = 0, Sul = 1, Leste = 2, Oeste = 3 }

        public SpriteRenderer alvo;
        [Tooltip("Índices: 0 norte (costas), 1 sul (frente), 2 leste, 3 oeste.")]
        public Sprite[] dia = new Sprite[4];
        public Sprite[] noite = new Sprite[4];

        public static Dir DirecaoDe(Vector2 v)
        {
            if (Mathf.Abs(v.y) >= Mathf.Abs(v.x)) return v.y >= 0 ? Dir.Norte : Dir.Sul;
            return v.x >= 0 ? Dir.Leste : Dir.Oeste;
        }

        static bool Tem(Sprite[] s) { if (s == null) return false; foreach (var x in s) if (x != null) return true; return false; }

        public void Aplicar(Vector2 direcao, bool ehNoite)
        {
            if (alvo == null) return;
            var set = ehNoite && Tem(noite) ? noite : dia;
            if (!Tem(set)) return;
            var d = DirecaoDe(direcao);
            Sprite s = set[(int)d];
            bool flip = false;
            if (s == null && d == Dir.Leste && set[3] != null) { s = set[3]; flip = true; }
            else if (s == null && d == Dir.Oeste && set[2] != null) { s = set[2]; flip = true; }
            if (s == null) { s = set[0] != null ? set[0] : FirstNonNull(set); flip = direcao.x < -0.1f; }
            alvo.sprite = s;
            alvo.flipX = flip;
        }

        static Sprite FirstNonNull(Sprite[] s) { foreach (var x in s) if (x != null) return x; return null; }
    }
}
