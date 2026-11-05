"""
Móveis, objetos de cenário e itens do VIGÍLIA, vistos de cima (com um pouco da face da frente,
para ler melhor o que é cada coisa). 32 pixels = 1 unidade do mundo.

Uso:  python Ferramentas/gerar_props.py        (de dentro da pasta Vigilia)
Saída: ArteBruta/<nome>.png  →  no Unity: Vigilia > Arte > Processar e construir tudo.
"""
import os
import sys
import numpy as np

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from gerar_arte_topo import Tela, hexc, contorno, SAIDA  # noqa: E402

rng = np.random.default_rng(11)
U = 32  # pixels por unidade


def ret(t, x0, y0, w, h):
    return (t.x >= x0) & (t.x < x0 + w) & (t.y >= y0) & (t.y < y0 + h)


def caixa(t, x0, y0, w, h, cor, frente=0, cor_frente=None, ruido=0.04, borda=True):
    """Tampo visto de cima (com luz na borda de cima/esquerda) + faixa da face da frente embaixo."""
    cor = np.asarray(cor, float)
    m = ret(t, x0, y0, w, h - frente)
    k = 1 + ruido * (rng.random(t.x.shape) - 0.5) * 2
    t.pintar(m, np.clip(cor[None, None, :] * k[..., None], 0, 255))
    if borda:
        t.pintar(ret(t, x0, y0, w, 1) | ret(t, x0, y0, 1, h - frente), np.minimum(cor * 1.25, 255))
        t.pintar(ret(t, x0, y0 + h - frente - 1, w, 1) | ret(t, x0 + w - 1, y0, 1, h - frente), cor * 0.7)
    if frente:
        cf = np.asarray(cor_frente if cor_frente is not None else cor * 0.6, float)
        t.pintar(ret(t, x0, y0 + h - frente, w, frente), cf)
    return m


def linha(t, x0, y0, x1, y1, larg, cor):
    n = int(max(abs(x1 - x0), abs(y1 - y0)) * 2) + 1
    m = np.zeros(t.x.shape, bool)
    for i in range(n + 1):
        f = i / n
        m |= t.elipse(x0 + (x1 - x0) * f, y0 + (y1 - y0) * f, larg / 2, larg / 2)
    t.pintar(m, cor)
    return m


def disco(t, cx, cy, r, cor, luz=0.45, ruido=0.0):
    m, c = t.sombreado(cx, cy, r, r, np.asarray(cor, float), luz, ruido=ruido)
    t.pintar(m, c)
    return m


def sombra(t, x0, y0, w, h):
    t.pintar(ret(t, x0 + 3, y0 + 3, w, h), np.zeros(3), 0.35)


def nova(wu, hu):
    return Tela(int(wu * U), int(hu * U))


# ----------------------------------------------------------------------------- móveis

def cama():
    t = nova(1.25, 2.3)
    W, H = t.w, t.h
    sombra(t, 2, 2, W - 5, H - 5)
    caixa(t, 2, 2, W - 5, H - 5, hexc("4a4844"))                       # estrutura de ferro
    caixa(t, 5, 5, W - 11, H - 12, hexc("cfd2c8"), ruido=0.05)          # colchão
    caixa(t, 7, 7, W - 15, 14, hexc("f0eee4"), ruido=0.02)              # travesseiro
    t.pintar(ret(t, 9, 13, W - 19, 1), hexc("c8c4b4"))
    caixa(t, 5, 26, W - 11, H - 33, hexc("6b8494"), ruido=0.08)         # cobertor
    for y in range(32, H - 10, 9):
        t.pintar(ret(t, 6, y, W - 13, 1), hexc("4e6676"))
    t.pintar(ret(t, 5, 26, W - 11, 3), hexc("8fa6b4"))                  # dobra
    for x in (3, W - 5):                                                # pés da cama
        for y in (3, H - 5):
            t.pintar(ret(t, x, y, 2, 2), hexc("2a2826"))
    contorno(t)
    return t


def mesa():
    t = nova(2.2, 1.15)
    W, H = t.w, t.h
    sombra(t, 2, 2, W - 5, H - 6)
    caixa(t, 2, 2, W - 5, H - 5, hexc("6a4428"), frente=5, cor_frente=hexc("3f2616"), ruido=0.06)
    for y in range(6, H - 10, 4):                                       # veio da madeira
        t.pintar(ret(t, 3, y, W - 7, 1), hexc("5a3820"), 0.6)
    # caderno aberto
    caixa(t, 22, 8, 14, 11, hexc("e8e2cc"), ruido=0.02)
    caixa(t, 36, 8, 14, 11, hexc("ece6d2"), ruido=0.02)
    for y in (11, 14, 17):
        t.pintar(ret(t, 24, y, 10, 1) | ret(t, 38, y, 10, 1), hexc("6b6e78"), 0.8)
    linha(t, 52, 9, 58, 17, 1.5, hexc("1f1a16"))                        # caneta
    # papéis soltos e lamparina
    caixa(t, 6, 6, 11, 14, hexc("d8d0b8"), ruido=0.03)
    disco(t, W - 12, 12, 5.5, hexc("8c5c38"))
    disco(t, W - 12, 12, 3, hexc("fff0b0"), luz=0.2)
    contorno(t)
    return t


def armario():
    t = nova(1.4, 1.0)
    W, H = t.w, t.h
    sombra(t, 1, 1, W - 3, H - 3)
    caixa(t, 1, 1, W - 3, H - 3, hexc("5e6a62"), frente=13, cor_frente=hexc("46524a"))
    fy = H - 3 - 13
    t.pintar(ret(t, W // 2 - 1, fy, 1, 13), hexc("1f2622"))             # vão entre as portas
    for x in (6, W // 2 + 4):
        for y in (fy + 3, fy + 5, fy + 7):
            t.pintar(ret(t, x, y, 9, 1), hexc("2a332e"))                # frestas de ventilação
    t.pintar(ret(t, W // 2 - 4, fy + 8, 2, 3) | ret(t, W // 2 + 2, fy + 8, 2, 3), hexc("c9b27a"))  # puxadores
    for _ in range(5):                                                  # ferrugem
        t.pintar(t.elipse(rng.uniform(4, W - 6), rng.uniform(3, H - 6), 2, 1.5), hexc("70472a"), 0.7)
    contorno(t)
    return t


def caldeira():
    t = nova(3.0, 3.0)
    W = t.w
    c = W / 2
    # canos saindo para os lados e para cima
    for (x0, y0, x1, y1) in ((4, c - 4, c, c - 4), (c, c + 6, W - 4, c + 6), (c - 6, 3, c - 6, c)):
        linha(t, x0, y0, x1, y1, 6, hexc("55524d"))
        linha(t, x0, y0 - 1, x1, y1 - 1, 2, hexc("8a857a"))
    for (x, y) in ((10, c - 4), (W - 10, c + 6), (c - 6, 10)):          # juntas
        disco(t, x, y, 4, hexc("3a3836"))
    t.pintar(t.elipse(c + 3, c + 4, 33, 33), np.zeros(3), 0.35)          # sombra
    disco(t, c, c, 33, hexc("3a302a"), luz=0.6, ruido=0.04)             # corpo de ferro
    anel = t.elipse(c, c, 31, 31) & ~t.elipse(c, c, 28, 28)
    t.pintar(anel, hexc("2a201c"))
    for k in range(16):                                                 # rebites
        a = k / 16 * 2 * np.pi
        disco(t, c + np.cos(a) * 29.5, c + np.sin(a) * 29.5, 1.3, hexc("8c7a68"), luz=0.6)
    disco(t, c, c - 4, 10, hexc("1f1a16"))                              # chaminé
    disco(t, c, c - 4, 6, hexc("0c0607"))
    # porta da fornalha com brilho
    caixa(t, c - 11, c + 13, 22, 13, hexc("2b1a10"))
    t.pintar(ret(t, c - 8, c + 16, 16, 7), hexc("d9471a"))
    t.pintar(ret(t, c - 6, c + 17, 12, 4), hexc("ffb040"))
    for x in range(int(c - 8), int(c + 8), 4):
        t.pintar(ret(t, x, c + 16, 1, 7), hexc("3d0d0e"))
    disco(t, c + 18, c - 16, 5, hexc("e3d29a"))                         # manômetro
    linha(t, c + 18, c - 16, c + 21, c - 19, 1, hexc("a3191b"))
    contorno(t)
    return t


def maca():
    t = nova(1.1, 2.2)
    W, H = t.w, t.h
    sombra(t, 2, 2, W - 5, H - 5)
    caixa(t, 2, 2, W - 5, H - 5, hexc("8a8e8a"))
    caixa(t, 4, 4, W - 9, H - 9, hexc("2e6a62"), ruido=0.05)           # estofado verde
    caixa(t, W // 2 - 7, 6, 14, H - 13, hexc("ecebe2"), ruido=0.02)     # papel descartável
    t.pintar(ret(t, 4, 18, W - 9, 1), hexc("1f4a44"))                   # dobra do encosto
    contorno(t)
    return t


def armario_remedios():
    t = nova(1.8, 0.8)
    W, H = t.w, t.h
    sombra(t, 1, 1, W - 3, H - 3)
    caixa(t, 1, 1, W - 3, H - 3, hexc("e6ecee"), frente=10, cor_frente=hexc("b0c0c8"))
    fy = H - 3 - 10
    t.pintar(ret(t, 3, fy + 1, W - 7, 8), hexc("6b8fa0"), 0.6)          # vidro
    cores = ["a3191b", "c9b27a", "2e4c5a", "8c5c38", "e3d29a", "4a5040"]
    for i, x in enumerate(range(5, W - 6, 4)):                          # frascos
        t.pintar(ret(t, x, fy + 3, 2, 5), hexc(cores[i % len(cores)]))
    t.pintar(ret(t, W // 2, fy, 1, 10), hexc("7a8a90"))
    contorno(t)
    return t


def estante():
    t = nova(2.4, 0.75)
    W, H = t.w, t.h
    sombra(t, 1, 1, W - 3, H - 3)
    caixa(t, 1, 1, W - 3, H - 3, hexc("4a2c1a"), frente=12, cor_frente=hexc("2b1a10"))
    fy = H - 3 - 12
    cores = ["6b1214", "2e4c5a", "4a5040", "8c5c38", "3d0d0e", "c9b27a", "1f3540", "573520"]
    x = 3
    while x < W - 5:                                                    # lombadas dos livros
        w = int(rng.integers(2, 4))
        h = int(rng.integers(7, 11))
        t.pintar(ret(t, x, fy + 11 - h, w, h), hexc(cores[int(rng.integers(0, len(cores)))]))
        x += w + (1 if rng.random() > 0.8 else 0)
    contorno(t)
    return t


def tapete():
    t = nova(3.2, 2.2)
    W, H = t.w, t.h
    caixa(t, 0, 0, W, H, hexc("5a1012"), borda=False, ruido=0.06)
    t.pintar(ret(t, 3, 3, W - 6, H - 6) & ~ret(t, 5, 5, W - 10, H - 10), hexc("c9b27a"))
    t.pintar(ret(t, 8, 8, W - 16, H - 16) & ~ret(t, 10, 10, W - 20, H - 20), hexc("2a0a0b"))
    cx, cy = W / 2, H / 2
    losango = (np.abs(t.x - cx) / (W * 0.28) + np.abs(t.y - cy) / (H * 0.3)) <= 1
    t.pintar(losango, hexc("851417"))
    t.pintar((np.abs(t.x - cx) / (W * 0.14) + np.abs(t.y - cy) / (H * 0.15)) <= 1, hexc("c9b27a"))
    for x in range(0, W, 3):                                            # franjas
        t.pintar(ret(t, x, 0, 1, 2) | ret(t, x, H - 2, 1, 2), hexc("e3d29a"))
    return t


def mesa_refeitorio():
    t = nova(3.8, 1.5)
    W, H = t.w, t.h
    # bancos dos dois lados
    caixa(t, 4, 0, W - 8, 8, hexc("70472a"), frente=2)
    caixa(t, 4, H - 9, W - 8, 8, hexc("70472a"), frente=2)
    sombra(t, 2, 10, W - 4, H - 22)
    caixa(t, 2, 10, W - 4, H - 22, hexc("8a857a"), ruido=0.03)          # tampo de inox
    for x in range(10, W - 12, 22):                                     # bandejas e pratos
        caixa(t, x, 13, 15, 10, hexc("55524d"))
        disco(t, x + 7.5, 18, 3.5, hexc("e8e2cc"), luz=0.3)
        disco(t, x + 7.5, 18, 1.5, hexc("8c5c38"), luz=0.2)
    contorno(t)
    return t


def bancada():
    t = nova(3.0, 1.2)
    W, H = t.w, t.h
    sombra(t, 1, 1, W - 3, H - 3)
    caixa(t, 1, 1, W - 3, H - 3, hexc("573520"), frente=6, cor_frente=hexc("2b1a10"), ruido=0.08)
    # ferramentas: martelo, serrote, chave inglesa, morsa
    linha(t, 10, 8, 26, 22, 2, hexc("8c5c38"))
    caixa(t, 6, 5, 9, 5, hexc("55524d"))
    caixa(t, 34, 7, 22, 9, hexc("a8a49a"))
    for x in range(34, 56, 2):
        t.pintar(ret(t, x, 15, 1, 2), hexc("55524d"))
    caixa(t, 56, 8, 8, 6, hexc("2b1a10"))
    linha(t, 66, 20, 80, 8, 3, hexc("7a756b"))
    disco(t, 81, 7, 3.5, hexc("7a756b"))
    caixa(t, W - 18, 4, 14, 14, hexc("3a3836"))                         # morsa
    caixa(t, W - 15, 7, 8, 8, hexc("55524d"))
    for _ in range(4):                                                  # manchas de óleo
        t.pintar(t.elipse(rng.uniform(8, W - 10), rng.uniform(6, H - 12), 3, 2), hexc("1f1a16"), 0.5)
    contorno(t)
    return t


def prateleira():
    t = nova(3.0, 0.9)
    W, H = t.w, t.h
    sombra(t, 1, 1, W - 3, H - 3)
    caixa(t, 1, 1, W - 3, H - 3, hexc("55524d"), frente=4)
    x = 4
    while x < W - 14:                                                   # caixas de papelão
        w = int(rng.integers(10, 16))
        caixa(t, x, 4, w, H - 12, hexc(rng.choice(["a87748", "8c5c38", "c9a86a"])), ruido=0.05)
        t.pintar(ret(t, x + w // 2, 4, 1, H - 12), hexc("70472a"), 0.8)  # fita
        x += w + 2
    contorno(t)
    return t


def arquivo():
    t = nova(1.0, 1.0)
    W, H = t.w, t.h
    sombra(t, 1, 1, W - 3, H - 3)
    caixa(t, 1, 1, W - 3, H - 3, hexc("6b7a80"), frente=14, cor_frente=hexc("4e5c62"))
    fy = H - 3 - 14
    for k in range(3):                                                  # gavetas
        y = fy + 1 + k * 4
        t.pintar(ret(t, 3, y + 3, W - 7, 1), hexc("2e3a40"))
        t.pintar(ret(t, W // 2 - 3, y + 1, 6, 1), hexc("c9b27a"))
    caixa(t, 6, 4, 10, 7, hexc("e8e2cc"), ruido=0.02)                   # papel em cima
    contorno(t)
    return t


def cadeira_contencao():
    t = nova(1.3, 1.3)
    W, H = t.w, t.h
    c = W / 2
    t.pintar(t.elipse(c + 3, c + 3, 18, 18), np.zeros(3), 0.35)
    caixa(t, c - 13, c - 8, 26, 22, hexc("573520"))                     # assento
    caixa(t, c - 13, c - 14, 26, 7, hexc("3f2616"))                     # encosto
    caixa(t, c - 17, c - 8, 5, 20, hexc("3f2616"))                      # braços
    caixa(t, c + 12, c - 8, 5, 20, hexc("3f2616"))
    for (x, y) in ((c - 17, c + 2), (c + 12, c + 2), (c - 9, c + 10), (c + 3, c + 10)):  # correias
        caixa(t, x, y, 6, 3, hexc("1f0c0c"))
        t.pintar(ret(t, x + 2, y + 1, 2, 1), hexc("c9b27a"))
    t.pintar(t.elipse(c + 2, c + 3, 7, 5), hexc("6b1214"), 0.8)          # sangue
    contorno(t)
    return t


def aparato():
    t = nova(1.6, 1.2)
    W, H = t.w, t.h
    sombra(t, 1, 1, W - 3, H - 3)
    caixa(t, 1, 1, W - 3, H - 3, hexc("2f3328"), frente=10, cor_frente=hexc("1f2218"))
    for k, x in enumerate((8, 20, 32)):                                 # mostradores
        disco(t, x, 10, 4.5, hexc("e3d29a"), luz=0.3)
        linha(t, x, 10, x + 3 - k * 2, 7, 1, hexc("a3191b"))
    fy = H - 3 - 10
    for x in range(5, W - 6, 5):                                        # luzes
        t.pintar(ret(t, x, fy + 4, 2, 2), hexc(rng.choice(["d9473a", "4a5040", "e3d29a"])))
    for k in range(3):                                                  # fios
        linha(t, W - 8, 6 + k * 4, W - 1, 12 + k * 6, 1.2, hexc("6b1214"))
    contorno(t)
    return t


def pinheiro():
    t = nova(2.4, 2.4)
    W = t.w
    c = W / 2
    t.pintar(t.elipse(c + 4, c + 4, 30, 30), np.zeros(3), 0.35)
    ang = np.arctan2(t.y - c, t.x - c)
    r = np.hypot(t.x - c, t.y - c)
    for raio, cor in ((36, "142228"), (27, "1f3540"), (17, "2e4c5a")):
        estrela = r <= raio * (0.72 + 0.28 * np.abs(np.cos(ang * 4)))
        t.pintar(estrela, hexc(cor))
        t.pintar(estrela & (rng.random(t.x.shape) > 0.8), hexc("d6e4ea"), 0.8)   # neve nos galhos
    disco(t, c, c, 4, hexc("d6e4ea"), luz=0.3)
    contorno(t)
    return t


# ----------------------------------------------------------------------------- itens (48 px)

def item_base():
    return Tela(48, 48)


def brilho(t):
    """Contorno claro em volta, para o item aparecer quando a lanterna passa."""
    solido = t.a > 0.5
    viz = np.zeros_like(solido)
    for dy in (-2, -1, 0, 1, 2):
        for dx in (-2, -1, 0, 1, 2):
            viz |= np.roll(np.roll(solido, dy, 0), dx, 1)
    borda = viz & ~solido
    t.c[borda] = hexc("fff6e0")
    t.a[borda] = 0.55


def item_pilha():
    t = item_base()
    caixa(t, 12, 17, 22, 14, hexc("2e6a3a"))
    caixa(t, 34, 20, 4, 8, hexc("c9b27a"))
    t.pintar(ret(t, 14, 19, 18, 3), hexc("8fd09a"), 0.6)
    t.pintar(ret(t, 20, 23, 6, 2), hexc("e3d29a"))
    contorno(t); brilho(t)
    return t


def item_chave():
    t = item_base()
    disco(t, 15, 20, 7, hexc("c9a040"))
    disco(t, 15, 20, 3, np.zeros(3))
    t.a[t.elipse(15, 20, 3, 3)] = 0
    linha(t, 21, 22, 38, 30, 3.5, hexc("c9a040"))
    linha(t, 33, 30, 35, 35, 2.5, hexc("c9a040"))
    linha(t, 37, 31, 38, 34, 2.5, hexc("c9a040"))
    caixa(t, 6, 30, 10, 12, hexc("e8e2cc"))                              # etiqueta
    linha(t, 11, 30, 13, 26, 1, hexc("8c5c38"))
    contorno(t); brilho(t)
    return t


def item_pedecabra():
    t = item_base()
    linha(t, 10, 38, 36, 12, 4, hexc("a3191b"))
    linha(t, 36, 12, 40, 10, 3, hexc("a3191b"))
    linha(t, 40, 10, 42, 14, 3, hexc("a3191b"))
    linha(t, 10, 38, 7, 35, 3, hexc("a3191b"))
    linha(t, 12, 35, 33, 14, 1, hexc("e0705f"))
    contorno(t); brilho(t)
    return t


def item_papel():
    t = item_base()
    caixa(t, 12, 8, 24, 32, hexc("e8e0c8"), ruido=0.03)
    for y in range(13, 36, 4):
        w = int(rng.integers(12, 19))
        t.pintar(ret(t, 15, y, w, 1), hexc("3a3836"), 0.8)
    t.pintar(t.elipse(30, 33, 2.5, 2), hexc("851417"))
    contorno(t); brilho(t)
    return t


def item_pulseira():
    t = item_base()
    anel = t.elipse(24, 24, 15, 10) & ~t.elipse(24, 24, 11, 6.5)
    t.pintar(anel, hexc("e8e0b0"))
    caixa(t, 15, 27, 18, 7, hexc("f4ead0"))
    t.pintar(ret(t, 17, 30, 14, 1), hexc("3a3836"))
    contorno(t); brilho(t)
    return t


def item_sedativo():
    t = item_base()
    caixa(t, 10, 14, 10, 20, hexc("9db8c4"))
    caixa(t, 10, 11, 10, 4, hexc("e8e2cc"))
    t.pintar(ret(t, 11, 22, 8, 10), hexc("6b8fa0"))
    linha(t, 26, 34, 38, 16, 3, hexc("d6e4ea"))                          # seringa
    linha(t, 38, 16, 42, 10, 1, hexc("a8a49a"))
    linha(t, 24, 37, 28, 31, 4, hexc("7a756b"))
    contorno(t); brilho(t)
    return t


def item_pesado():
    t = item_base()
    linha(t, 12, 38, 32, 16, 5, hexc("8a8e92"))
    disco(t, 35, 13, 7, hexc("8a8e92"))
    t.pintar(t.elipse(38, 10, 3.5, 3.5), np.zeros(3), 1.0)
    t.a[t.elipse(38, 10, 3, 3)] = 0
    linha(t, 13, 36, 30, 17, 1.5, hexc("d6e4ea"))
    contorno(t); brilho(t)
    return t


PROPS = {
    "cama": cama, "mesa": mesa, "armario": armario, "caldeira": caldeira,
    "maca": maca, "armario_remedios": armario_remedios, "estante": estante, "tapete": tapete,
    "mesa_refeitorio": mesa_refeitorio, "bancada": bancada, "prateleira": prateleira, "arquivo": arquivo,
    "cadeira_contencao": cadeira_contencao, "aparato": aparato, "pinheiro": pinheiro,
    "item_pilha": item_pilha, "item_chave": item_chave, "item_pedecabra": item_pedecabra,
    "item_papel": item_papel, "item_pulseira": item_pulseira, "item_sedativo": item_sedativo,
    "item_pesado": item_pesado,
}


def main():
    os.makedirs(SAIDA, exist_ok=True)
    for nome, f in PROPS.items():
        f().png(os.path.join(SAIDA, nome + ".png"))
    print("ok:", len(PROPS), "imagens em", SAIDA)


if __name__ == "__main__":
    main()
