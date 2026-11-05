using System;
using UnityEngine;

namespace Vigilia
{
    /// <summary>
    /// Canal de barulho do mundo. Quem faz barulho chama Emitir; os inimigos ouvem por OnBarulho (E-05).
    /// Um evento estático evita que o pé de cabra precise conhecer cada inimigo.
    /// </summary>
    public static class Barulho
    {
        public static event Action<Vector2, float> OnBarulho;

        public static void Emitir(Vector2 origem, float raio) => OnBarulho?.Invoke(origem, raio);
    }
}
