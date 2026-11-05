"""
Gera a arte vista de cima do VIGÍLIA (personagens que giram com a mira e pisos de cada sala).

Personagens: baseados nos modelos do PixelLab em ArteBruta/_corpo_inteiro (cabelo, roupa e cores),
redesenhados vistos de cima, olhando para o TOPO da imagem. Versão de noite: possuído, braços
estendidos para a frente, pele cinza e manchas de sangue.

Uso:  python Ferramentas/gerar_arte_topo.py        (de dentro da pasta Vigilia)
Saída: ArteBruta/topo_<id>.png, topo_<id>_noite.png, olhos_topo.png, piso_<sala>.png, parede.png
Depois, no Unity: Vigilia > Arte > Processar e construir tudo.
Requer: pip install pillow numpy
"""
import os
import numpy as np
from PIL import Image

AQUI = os.path.dirname(os.path.abspath(__file__))
SAIDA = os.path.join(AQUI, "..", "ArteBruta")
rng = np.random.default_rng(7)

S = 96  # tamanho do sprite


def hexc(h):
    h = h.lstrip("#")
    return np.array([int(h[i:i + 2], 16) for i in (0, 2, 4)], dtype=float)


CONTORNO = hexc("0c0607")


class Tela:
    """Imagem RGBA em float com operações de pintura simples."""

    def __init__(self, w=S, h=S):
        self.w, self.h = w, h
        self.c = np.zeros((h, w, 3))
        self.a = np.zeros((h, w))
        yy, xx = np.mgrid[0:h, 0:w]
        self.x = xx + 0.5
        self.y = yy + 0.5

    def pintar(self, mascara, cor, alfa=1.0):
        m = mascara.astype(float) * alfa
        if np.ndim(cor) == 3:
            self.c = self.c * (1 - m[..., None]) + cor * m[..., None]
        else:
            self.c = self.c * (1 - m[..., None]) + np.asarray(cor)[None, None, :] * m[..., None]
        self.a = np.maximum(self.a, m)

    def elipse(self, cx, cy, rx, ry):
        return ((self.x - cx) / rx) ** 2 + ((self.y - cy) / ry) ** 2 <= 1.0

    def sombreado(self, cx, cy, rx, ry, base, luz=0.35, ruido=0.0, escala_ruido=0.25):
        """Elipsoide com luz vindo de cima-esquerda (volume)."""
        nx = (self.x - cx) / rx
        ny = (self.y - cy) / ry
        d2 = nx ** 2 + ny ** 2
        nz = np.sqrt(np.clip(1 - d2, 0, 1))
        L = np.array([-0.45, -0.55, 0.7])
        L = L / np.linalg.norm(L)
        dif = np.clip(nx * L[0] + ny * L[1] + nz * L[2], 0, 1)
        k = (1 - luz) + luz * 1.6 * dif
        if ruido > 0:
            k = k * (1 + ruido * (rng.random(self.x.shape) - 0.5) * 2)
        cor = np.clip(base[None, None, :] * k[..., None], 0, 255)
        return d2 <= 1.0, cor

    def png(self, caminho):
        img = np.zeros((self.h, self.w, 4), dtype=np.uint8)
        img[..., :3] = np.clip(self.c, 0, 255).astype(np.uint8)
        img[..., 3] = (np.clip(self.a, 0, 1) * 255).astype(np.uint8)
        Image.fromarray(img).save(caminho)


def contorno(t, cor=CONTORNO):
    """Contorno de 1 pixel em volta da silhueta (lê bem no escuro)."""
    solido = t.a > 0.5
    viz = np.zeros_like(solido)
    for dy, dx in ((1, 0), (-1, 0), (0, 1), (0, -1)):
        viz |= np.roll(np.roll(solido, dy, 0), dx, 1)
    borda = viz & ~solido
    t.c[borda] = cor
    t.a[borda] = 1.0


def quantizar_alfa(t):
    t.a = np.where(t.a > 0.5, 1.0, np.where(t.a > 0.15, t.a, 0.0))


# ----------------------------------------------------------------------------- personagens

PERSONAGENS = {
    # id: cores e estilo, tirados dos modelos do PixelLab
    "elias":     dict(pele="d2a68a", cabelo="5a3a22", cabelo2="8a5a34", roupa="7a4f26", roupa2="4f321a",
                      gola="5c645a", estilo="baguncado", lanterna=True, cachecol=True),
    "marta":     dict(pele="d9b39a", cabelo="6a3e22", cabelo2="8f5a36", roupa="c9d6da", roupa2="8fa6b0",
                      gola="e8eef0", estilo="coque", touca=True),
    "otis":      dict(pele="b98a68", cabelo="b8c0c4", cabelo2="e0e4e6", roupa="3c5a5a", roupa2="243a3a",
                      gola="3c5a5a", estilo="curto", alcas="2a3a52", chaves=True),
    "almeida":   dict(pele="d2a68a", cabelo="1a1614", cabelo2="3a3430", roupa="e6ecee", roupa2="a8b8c0",
                      gola="3a3836", estilo="liso", oculos=True, costura=True),
    "wren":      dict(pele="e0b090", cabelo="e8eef2", cabelo2="ffffff", roupa="a8c4d0", roupa2="6b8fa0",
                      gola="a8c4d0", estilo="careca_tufos", magro=True, avental=True),
    "holt":      dict(pele="d8b49a", cabelo="c8ccd0", cabelo2="eef0f2", roupa="2e3630", roupa2="1a201c",
                      gola="d6d0c0", estilo="grisalho_calvo", oculos=True, terno=True),
    "paciente1": dict(pele="c8a080", cabelo="7a5a48", cabelo2="9a7a64", roupa="8fb0bc", roupa2="5a7a86",
                      gola="8fb0bc", estilo="raspado", avental=True),
    "paciente2": dict(pele="d0aa90", cabelo="8a8e8a", cabelo2="b0b4ae", roupa="9ab8c2", roupa2="62808a",
                      gola="9ab8c2", estilo="longo", avental=True, magro=True),
}

CX, CY = 48.0, 52.0  # centro do tronco; a frente é para cima (y menor)


def personagem(pid, p, noite):
    t = Tela()
    pele = hexc(p["pele"])
    if noite:
        pele = pele * 0.45 + hexc("8a948a") * 0.55  # pele cinza-esverdeada
    cab, cab2 = hexc(p["cabelo"]), hexc(p["cabelo2"])
    roupa, roupa2, gola = hexc(p["roupa"]), hexc(p["roupa2"]), hexc(p["gola"])
    if noite:
        roupa, roupa2, gola = roupa * 0.8, roupa2 * 0.8, gola * 0.8
    magro = p.get("magro", False)
    ombro_rx = 21 if magro else 24

    # sombra no chão
    t.pintar(t.elipse(CX + 2, CY + 4, ombro_rx + 5, 17), np.zeros(3), 0.35)

    # braços: de dia ao lado do corpo; de noite estendidos para a frente (possuído)
    def braco(x0, y0, x1, y1, larg, cor_manga):
        n = 14
        for i in range(n + 1):
            f = i / n
            x, y = x0 + (x1 - x0) * f, y0 + (y1 - y0) * f
            m, c = t.sombreado(x, y, larg, larg, cor_manga, 0.4)
            t.pintar(m, c)

    def mao(x, y, r=4.5):
        m, c = t.sombreado(x, y, r, r, pele, 0.4)
        t.pintar(m, c)
        if noite:  # dedos compridos
            for dx in (-3, -1, 1, 3):
                t.pintar(t.elipse(x + dx, y - r - 1.5, 0.9, 2.2), pele * 0.8)

    lado = ombro_rx - 3
    if noite:
        braco(CX - lado + 1, CY, CX - 11, CY - 28, 5, roupa2)
        braco(CX + lado - 1, CY, CX + 11, CY - 28, 5, roupa2)
        mao(CX - 11, CY - 32)
        mao(CX + 11, CY - 32)
    else:
        braco(CX - lado, CY + 1, CX - lado + 1, CY - 8, 5, roupa2)
        if p.get("lanterna"):
            braco(CX + lado, CY + 1, CX + 9, CY - 18, 5, roupa2)
        else:
            braco(CX + lado, CY + 1, CX + lado - 1, CY - 8, 5, roupa2)
        mao(CX - lado + 1, CY - 12)
        if p.get("lanterna"):
            # lanterna na mão direita, apontando para a frente
            t.pintar((np.abs(t.x - (CX + 8)) <= 2.0) & (t.y >= CY - 34) & (t.y <= CY - 20), hexc("55524d"))
            t.pintar((np.abs(t.x - (CX + 8)) <= 2.6) & (t.y >= CY - 36) & (t.y <= CY - 33), hexc("7a756b"))
            t.pintar(t.elipse(CX + 8, CY - 36, 1.8, 1.1), hexc("fff6e0"))
            mao(CX + 9, CY - 20)
        else:
            mao(CX + lado - 1, CY - 12)

    # tronco / ombros
    m, c = t.sombreado(CX, CY, ombro_rx, 12, roupa, 0.5, ruido=0.04)
    t.pintar(m, c)
    # costas: dobra de tecido
    t.pintar(t.elipse(CX, CY + 7, ombro_rx * 0.75, 4.5) & m, roupa2, 0.3)
    if p.get("costura"):
        t.pintar((np.abs(t.x - CX) < 0.8) & (t.y > CY) & m, roupa2)
    if p.get("terno"):
        for sx in (-1, 1):
            t.pintar((np.abs(t.x - (CX + sx * 13)) < 0.8) & (t.y > CY - 9) & (t.y < CY + 6) & m, roupa * 1.5, 0.6)
    if p.get("alcas"):
        for sx in (-1, 1):
            t.pintar((np.abs(t.x - (CX + sx * 9)) < 2.2) & m, hexc(p["alcas"]))
    if p.get("avental"):  # abertura do avental de hospital nas costas
        t.pintar((np.abs(t.x - CX) < 1.2) & (t.y > CY - 2) & m, roupa2)
        for yy in (CY + 3, CY + 8):
            t.pintar(t.elipse(CX - 2, yy, 2.5, 0.8) | t.elipse(CX + 2, yy, 2.5, 0.8), roupa2 * 0.8)
    if p.get("chaves"):
        t.pintar(t.elipse(CX + ombro_rx - 6, CY + 7, 2.5, 1.8), hexc("a87748"))
    if noite:  # manchas de sangue
        for _ in range(7):
            bx, by = CX + rng.uniform(-ombro_rx + 5, ombro_rx - 5), CY + rng.uniform(-8, 9)
            r = rng.uniform(1.5, 4)
            t.pintar(t.elipse(bx, by, r, r * rng.uniform(0.6, 1.4)) & m, hexc(rng.choice(["6b1214", "851417", "521012"])), 0.9)

    # gola / cachecol
    if p.get("cachecol"):
        m2, c2 = t.sombreado(CX, CY - 1, 14, 8.5, gola, 0.45, ruido=0.1)
        t.pintar(m2, c2)
        t.pintar(t.elipse(CX - 5, CY + 8, 2.5, 4.5), gola * 0.85)  # ponta caída nas costas
    else:
        m2, c2 = t.sombreado(CX, CY - 1, 12, 7.5, gola, 0.4)
        t.pintar(m2, c2)

    # cabeça
    hx, hy, hr = CX, CY - 3, 11 if not magro else 10
    for sx in (-1, 1):  # orelhas
        mo, co = t.sombreado(hx + sx * (hr - 0.5), hy + 1, 2.5, 3.5, pele, 0.4)
        t.pintar(mo, co)
    mh, ch = t.sombreado(hx, hy, hr, hr, pele, 0.5)
    t.pintar(mh, ch)
    # nariz / testa aparecendo na frente
    mn, cn = t.sombreado(hx, hy - hr + 0.5, 2.2, 2.0, pele, 0.4)
    t.pintar(mn, cn)

    estilo = p["estilo"]

    def cabelo_cheio(cy_off=1.5, r=None, ruido=0.12):
        rr = r if r is not None else hr
        mc, cc = t.sombreado(hx, hy + cy_off, rr, rr, cab, 0.55, ruido=ruido)
        # mechas: listras radiais a partir da coroa
        ang = np.arctan2(t.y - (hy + 3), t.x - hx)
        mecha = (np.sin(ang * 9 + rng.random()) > 0.55)
        cc = np.where(mecha[..., None], cc * 0.82 + cab2 * 0.18, cc)
        t.pintar(mc, cc)
        return mc

    if estilo == "baguncado":
        mc = cabelo_cheio(1.8)
        for _ in range(9):  # pontas soltas
            a = rng.uniform(0, 2 * np.pi)
            t.pintar(t.elipse(hx + np.cos(a) * hr, hy + 1.8 + np.sin(a) * hr, 2, 2), cab * 0.9)
    elif estilo == "coque":
        cabelo_cheio(1.5)
        mb, cb = t.sombreado(hx, hy + hr - 1, 5, 4.5, cab * 0.85, 0.5, ruido=0.1)
        t.pintar(mb, cb)
    elif estilo == "curto":
        cabelo_cheio(2.2, ruido=0.3)
    elif estilo == "liso":
        mc = cabelo_cheio(1.6, ruido=0.05)
        for dx in (-3, 3):  # brilho do gel
            t.pintar((np.abs(t.x - (hx + dx)) < 0.6) & mc & (t.y > hy - 5) & (t.y < hy + 6), cab2, 0.35)
    elif estilo == "careca_tufos":
        # topo careca brilhante, tufos brancos dos lados e atrás
        anel = t.elipse(hx, hy + 2, hr + 1.5, hr + 1) & ~t.elipse(hx, hy - 1, hr - 3, hr - 3.5)
        t.pintar(anel & (t.y > hy - 4), cab)
        for _ in range(16):
            a = rng.uniform(-0.3, np.pi + 0.3)
            t.pintar(t.elipse(hx + np.cos(a) * (hr + 1), hy + 2 + np.sin(a) * (hr - 1), 1.6, 1.6), cab2)
        t.pintar(t.elipse(hx - 3, hy - 3, 3, 2), np.array([255, 235, 215]), 0.5)
    elif estilo == "grisalho_calvo":
        cabelo_cheio(1.8, ruido=0.08)
        mcal, ccal = t.sombreado(hx, hy - 1, 5.5, 5, pele, 0.5)
        t.pintar(mcal, ccal)
    elif estilo == "raspado":
        pont = (rng.random(t.x.shape) > 0.55) & t.elipse(hx, hy + 1.5, hr - 0.5, hr - 0.5)
        t.pintar(pont, cab, 0.6)
    elif estilo == "longo":
        cabelo_cheio(1.5)
        # cabelo comprido caindo pelas costas
        ml, cl = t.sombreado(hx, hy + 12, 11, 12, cab, 0.45, ruido=0.15)
        ang = np.arctan2(t.y - hy, t.x - hx)
        mecha = np.sin((t.x - hx) * 1.3 + rng.random()) > 0.3
        cl = np.where(mecha[..., None], cl * 0.8, cl)
        t.pintar(ml & (t.y > hy), cl)

    if p.get("touca"):
        mt, ct = t.sombreado(hx, hy - 2, 8.5, 6.5, hexc("eef2f4"), 0.4)
        t.pintar(mt, ct)
        t.pintar((np.abs(t.y - (hy + 3.2)) < 1.0) & t.elipse(hx, hy - 2, 8.5, 7), hexc("9db8c4"))
    if p.get("oculos") and not noite:
        for sx in (-4, 4):
            t.pintar(t.elipse(hx + sx, hy - hr + 1.5, 1.6, 0.8), hexc("d6e4ea"))
    if noite:  # olhos vermelhos na frente da cabeça
        for sx in (-3.5, 3.5):
            t.pintar(t.elipse(hx + sx, hy - hr + 2, 1.4, 1.0), hexc("d9473a"))
        # cabeça um pouco inclinada: sombra lateral
        t.pintar(t.elipse(hx + 5, hy + 2, hr * 0.8, hr) & mh, np.zeros(3), 0.25)

    quantizar_alfa(t)
    contorno(t)
    return t


def olhos_overlay():
    """Brilho dos olhos dos possuídos (desenhado sem luz no jogo, aparece no escuro)."""
    t = Tela()
    hx, hy, hr = CX, CY - 3, 11
    for sx in (-3.5, 3.5):
        t.pintar(t.elipse(hx + sx, hy - hr + 2, 2.2, 1.6), hexc("ff3a20"), 0.9)
        t.pintar(t.elipse(hx + sx, hy - hr + 2, 1.0, 0.8), hexc("ffd0a0"))
    return t


# ----------------------------------------------------------------------------- pisos (128 px = 4×4 unidades, emendam)

P = 128


def ruido_tile(escala, oitavas=3, seed=0):
    """Ruído de valor que emenda nas bordas."""
    r = np.random.default_rng(seed)
    total = np.zeros((P, P))
    amp, soma = 1.0, 0.0
    for o in range(oitavas):
        n = escala * (2 ** o)
        grade = r.random((n, n))
        ys = np.linspace(0, n, P, endpoint=False)
        xs = np.linspace(0, n, P, endpoint=False)
        y0 = np.floor(ys).astype(int)
        x0 = np.floor(xs).astype(int)
        fy = (ys - y0)[:, None]
        fx = (xs - x0)[None, :]
        fy = fy * fy * (3 - 2 * fy)
        fx = fx * fx * (3 - 2 * fx)
        a = grade[y0[:, None] % n, x0[None, :] % n]
        b = grade[y0[:, None] % n, (x0[None, :] + 1) % n]
        c = grade[(y0[:, None] + 1) % n, x0[None, :] % n]
        d = grade[(y0[:, None] + 1) % n, (x0[None, :] + 1) % n]
        total += amp * (a + (b - a) * fx + (c - a) * fy + (a - b - c + d) * fx * fy)
        soma += amp
        amp *= 0.5
    return total / soma


def img(rgb):
    out = np.zeros((P, P, 4), dtype=np.uint8)
    out[..., :3] = np.clip(rgb, 0, 255).astype(np.uint8)
    out[..., 3] = 255
    return Image.fromarray(out)


def tabuas(base, larg=16, seed=1, junta=0.55):
    yy, xx = np.mgrid[0:P, 0:P]
    tab = xx // larg
    r = np.random.default_rng(seed)
    tom = r.uniform(0.85, 1.12, P // larg)[tab]
    off = r.integers(0, P, P // larg)[tab]
    veio = ruido_tile(2, 3, seed + 3)
    grao = np.sin((yy + off) * 0.35 + veio * 9) * 0.06 + ruido_tile(16, 2, seed + 5) * 0.12
    k = tom * (0.9 + grao)
    cor = base[None, None, :] * k[..., None]
    cor[(xx % larg) == 0] *= junta
    emenda = ((yy + off) % 64) == 0
    cor[emenda] *= 0.65
    return cor


def ladrilhos(c1, c2, tam=16, seed=2, xadrez=False, junta=0.55, rejunte=None):
    yy, xx = np.mgrid[0:P, 0:P]
    r = np.random.default_rng(seed)
    ix, iy = xx // tam, yy // tam
    var = r.uniform(0.9, 1.08, (P // tam, P // tam))[iy, ix]
    usa2 = ((ix + iy) % 2 == 1) if xadrez else (r.random((P // tam, P // tam))[iy, ix] > 0.85)
    cor = np.where(usa2[..., None], c2, c1) * var[..., None]
    sujeira = ruido_tile(4, 3, seed + 7)
    cor *= (0.88 + 0.2 * sujeira)[..., None]
    grout = ((xx % tam) == 0) | ((yy % tam) == 0)
    cor[grout] = (rejunte if rejunte is not None else cor[grout] * junta)
    return cor


def concreto(base, seed=3, manchas=None, rachas=True):
    n = ruido_tile(4, 4, seed)
    fino = ruido_tile(32, 1, seed + 1)
    cor = base[None, None, :] * (0.78 + 0.35 * n + 0.08 * fino)[..., None]
    if manchas is not None:
        m = ruido_tile(3, 3, seed + 2)
        cor = np.where((m > 0.6)[..., None], cor * 0.78 + manchas * 0.22, cor)
    if rachas:
        r = np.random.default_rng(seed + 9)
        for _ in range(3):
            x, y = r.uniform(0, P), r.uniform(0, P)
            a = r.uniform(0, 2 * np.pi)
            for _ in range(35):
                a += r.uniform(-0.2, 0.2)
                x, y = (x + np.cos(a)) % P, (y + np.sin(a)) % P
                cor[int(y), int(x)] *= 0.55
    return cor


def chapa_metal(base, seed=4):
    yy, xx = np.mgrid[0:P, 0:P]
    cor = concreto(base, seed, rachas=False)
    losango = (((xx + yy) % 8 == 0) & ((xx // 4) % 2 == 0)) | (((xx - yy) % 8 == 0) & ((xx // 4) % 2 == 1))
    cor[losango] *= 1.45
    cor[(xx % 32 == 0) | (yy % 32 == 0)] *= 0.5
    return cor


def pedra(base, seed=5):
    yy, xx = np.mgrid[0:P, 0:P]
    linha = yy // 21
    desloc = (linha % 2) * 16
    bloco = ((xx + desloc) // 32)
    r = np.random.default_rng(seed)
    var = r.uniform(0.75, 1.1, (8, 8))[linha % 8, bloco % 8]
    cor = base[None, None, :] * var[..., None] * (0.85 + 0.25 * ruido_tile(8, 2, seed))[..., None]
    cor[((yy % 21) == 0) | (((xx + desloc) % 32) == 0)] *= 0.4
    m = ruido_tile(3, 3, seed + 1)
    sangue = hexc("521012")
    cor = np.where((m > 0.66)[..., None], cor * 0.4 + sangue * 0.6, cor)
    return cor


def linoleo(base, seed=6):
    yy, xx = np.mgrid[0:P, 0:P]
    n = ruido_tile(8, 3, seed)
    cor = base[None, None, :] * (0.85 + 0.25 * n)[..., None]
    sal = np.random.default_rng(seed).random((P, P)) > 0.93
    cor[sal] *= 1.15
    cor[(xx % 64 == 0) | (yy % 64 == 0)] *= 0.6
    return cor


def neve():
    n = ruido_tile(4, 4, 21)
    cor = hexc("d6e4ea")[None, None, :] * (0.82 + 0.22 * n)[..., None]
    r = np.random.default_rng(21)
    for _ in range(10):                      # pegadas
        x, y = r.integers(4, P - 8), r.integers(4, P - 8)
        cor[y:y + 4, x:x + 2] *= 0.8
        cor[y + 6:y + 10, x + 3:x + 5] *= 0.8
    sal = r.random((P, P)) > 0.97
    cor[sal] = hexc("ffffff")
    return cor


def parede():
    # topo das paredes: reboco claro (contrasta com todos os pisos)
    n = ruido_tile(8, 3, 11)
    cor = hexc("b8b0a0")[None, None, :] * (0.82 + 0.25 * n)[..., None]
    yy, xx = np.mgrid[0:P, 0:P]
    cor[(yy % 32) == 0] *= 0.85
    return cor


PISOS = {
    "quarto": lambda: tabuas(hexc("7a5434"), 16, 1),
    "consultorio": lambda: ladrilhos(hexc("c8d4d8"), hexc("9db8c4"), 16, 2, rejunte=hexc("6b7a80")),
    "escritorio": lambda: tabuas(hexc("5a2e22"), 8, 3, 0.45),
    "ala": lambda: linoleo(hexc("8fa890")),
    "deposito": lambda: concreto(hexc("6e6a64"), 3, manchas=hexc("3f2616")),
    "oficina": lambda: concreto(hexc("7a756b"), 7, manchas=hexc("1f1a16")),
    "caldeira": lambda: chapa_metal(hexc("4a4844")),
    "subsolo": lambda: concreto(hexc("5a6670"), 9, manchas=hexc("2e4c5a")),
    "refeitorio": lambda: ladrilhos(hexc("8c4a30"), hexc("a8643c"), 16, 4, xadrez=True, rejunte=hexc("3f2616")),
    "secreta": lambda: pedra(hexc("4a4440")),
    "exterior": lambda: neve(),
}


def main():
    os.makedirs(SAIDA, exist_ok=True)
    for pid, p in PERSONAGENS.items():
        personagem(pid, p, False).png(os.path.join(SAIDA, f"topo_{pid}.png"))
        if pid != "holt":
            personagem(pid, p, True).png(os.path.join(SAIDA, f"topo_{pid}_noite.png"))
    olhos_overlay().png(os.path.join(SAIDA, "olhos_topo.png"))
    for sala, f in PISOS.items():
        img(f()).save(os.path.join(SAIDA, f"piso_{sala}.png"))
    img(parede()).save(os.path.join(SAIDA, "parede.png"))
    print("ok:", SAIDA)


if __name__ == "__main__":
    main()
