using NUnit.Framework;
using UnityEngine;
using Vigilia.EditorTools;

namespace Vigilia.Tests
{
    public class PixelizadorTests
    {
        [Test]
        public void FundoMagentaViraTransparenteECentroFica()
        {
            var src = new Texture2D(400, 400, TextureFormat.RGBA32, false);
            var px = new Color32[400 * 400];
            for (int y = 0; y < 400; y++)
                for (int x = 0; x < 400; x++)
                    px[y * 400 + x] = (x > 100 && x < 300 && y > 100 && y < 300) ? new Color32(120, 40, 30, 255) : new Color32(255, 0, 255, 255);
            src.SetPixels32(px); src.Apply();

            var o = PixelizadorDeArte.Converter(src, 96, 96, true, true);
            Assert.AreEqual(96, o.width);
            Assert.AreEqual(0, o.GetPixel(2, 2).a, 0.01f, "canto deveria ficar transparente");
            var centro = (Color32)o.GetPixel(48, 48);
            Assert.AreEqual(255, centro.a);
            Assert.Less(centro.g, 100, "sem magenta no centro");
            Object.DestroyImmediate(src); Object.DestroyImmediate(o);
        }

        [Test]
        public void TamanhosPorPrefixo()
        {
            Assert.AreEqual(new Vector2Int(96, 96), PixelizadorDeArte.Tamanho("npc_marta"));
            Assert.AreEqual(new Vector2Int(160, 160), PixelizadorDeArte.Tamanho("retrato_elias_dia"));
            Assert.AreEqual(new Vector2Int(640, 360), PixelizadorDeArte.Tamanho("menu_fundo"));
        }
    }
}
