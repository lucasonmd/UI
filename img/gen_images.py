"""설계서용 모의 영상 생성 — 실제 촬영본이 아니라 코드로 그린 그림이다 (글자·표시 없음).
  python img/gen_images.py
    → img/sa_360_1590x915.png   360 상황인식 : 전방 1590×455 / 구분 5 / 후방 1590×455
    → img/scope_1200x915.png    조준경(열상) : 1200×915, 조준선만. Application 1590×915 에 좌측 정렬
PIL 만 쓴다.
  · 질감 : 여러 배율의 노이즈를 겹친 프랙탈 노이즈
  · 땅   : 평면 텍스처(풀·마른풀·밭고랑·흙길)를 지평선 소실점으로 원근 투영 → 먼 곳은 촘촘, 길·고랑은 소실점으로 모임
  · 나무 : 불규칙한 수관(樹冠)을 겹쳐 그림"""
import math, os, random
from PIL import Image, ImageDraw, ImageFilter, ImageOps, ImageChops, ImageEnhance

OUT = os.path.dirname(os.path.abspath(__file__))
TW, TH = 4096, 2048                  # 땅 텍스처 크기

# ---------------------------------------------------------------- 도구
def fnoise(w, h, base=6, octaves=5, persist=0.55):
    out, ws = None, 0.0
    for o in range(octaves):
        cw = max(2, int(base * 2 ** o)); ch = max(2, int(cw * h / w) + 1)
        n = Image.effect_noise((cw, ch), 64).resize((w, h), Image.BICUBIC)
        wt = persist ** o
        if out is None:
            out, ws = n, wt
        else:
            out = Image.blend(out, n, wt / (ws + wt)); ws += wt
    return ImageOps.autocontrast(out, cutoff=0.5)

def remap(img_l, lo, hi):
    return img_l.point(lambda v: int(lo + (hi - lo) * v / 255))

def tex(w, h, dark, light, **kw):
    return ImageOps.colorize(fnoise(w, h, **kw), dark, light)

def thresh(img_l, at, gain):
    return img_l.point(lambda v: max(0, min(255, int((v - at) * gain))))

def vgrad(img, box, c1, c2):
    d = ImageDraw.Draw(img)
    x0, y0, x1, y1 = box
    for y in range(y0, y1):
        t = (y - y0) / max(1, y1 - y0 - 1)
        d.line([(x0, y), (x1, y)], fill=int(c1 + (c2 - c1) * t) if isinstance(c1, int)
               else tuple(int(a + (b - a) * t) for a, b in zip(c1, c2)))

def vfade(size, top, bottom):
    m = Image.new('L', size, 0); vgrad(m, (0, 0, size[0], size[1]), top, bottom); return m

def band_mask(size, center, sigma, peak):
    m = Image.new('L', size, 0); d = ImageDraw.Draw(m)
    for y in range(size[1]):
        d.line([(0, y), (size[0], y)], fill=int(peak * math.exp(-((y - center) / sigma) ** 2)))
    return m

def profile(w, base, waves, seed, rough=0.0, step=3):
    rnd = random.Random(seed)
    ph = [rnd.uniform(0, 2 * math.pi) for _ in waves]
    walk, pts = 0.0, []
    for x in range(0, w + step, step):
        y = base - sum(a * math.sin(x / w * f * 2 * math.pi + p) for (f, a), p in zip(waves, ph))
        walk = walk * 0.9 + rnd.uniform(-1, 1) * rough
        pts.append((x, y + walk))
    return pts

def y_at(pts, x):
    step = pts[1][0] - pts[0][0]
    return pts[min(len(pts) - 1, max(0, int(x / step)))][1]

def poly_mask(size, pts, soft=1.0):
    m = Image.new('L', size, 0)
    ImageDraw.Draw(m).polygon(pts + [(size[0], size[1]), (0, size[1])], fill=255)
    return m.filter(ImageFilter.GaussianBlur(soft)) if soft else m

def crowns(size, base, rnd, h, w, density=1.0, gaps=None):
    """수관 마스크 — base 능선 위에 크기가 제각각인 타원을 겹친다. gaps(x)->True 면 건너뜀."""
    m = Image.new('L', size, 0); d = ImageDraw.Draw(m)
    x = rnd.uniform(-10, 0)
    while x < size[0] + 20:
        cw = rnd.uniform(*w); ch = rnd.uniform(*h)
        if not (gaps and gaps(x)):
            yb = y_at(base, x) + rnd.uniform(-2, 3)
            d.ellipse([x - cw / 2, yb - ch, x + cw / 2, yb + ch * 0.35], fill=255)
        x += cw * rnd.uniform(0.3, 0.65) / density
    return m

def widen(t):
    """좌우로 거울 이어붙여 폭을 두 배로 — 이음매가 연속이라 티가 안 난다. 가운데(=원래 중앙)는 x = TW."""
    w, h = t.size
    out = Image.new(t.mode, (w * 2, h))
    out.paste(ImageOps.mirror(t.crop((0, 0, w // 2, h))), (0, 0))
    out.paste(t, (w // 2, 0))
    out.paste(ImageOps.mirror(t.crop((w // 2, 0, w, h))), (w // 2 + w, 0))
    return out

def near_detail(img, hz, lo, hi, strength, rgb=None):
    """화면 크기에서 덧입히는 잔 질감 — 가까울수록 진하게 (원근 투영으로 늘어나 흐려진 앞쪽을 살린다)."""
    w, h = img.size
    n = fnoise(w, h, base=260, octaves=3, persist=0.6)
    layer = ImageOps.colorize(n, lo, hi) if rgb else remap(n, lo, hi)
    m = Image.new('L', (w, h), 0); vgrad(m, (0, hz + 24, w, h), 0, strength)
    img.paste(layer, (0, 0), m)

def perspective_ground(texture, size, hz, first_row, cx, k, u0):
    """평면 텍스처를 지평선 hz 의 소실점으로 투영한다.
       화면 (x, y) → 텍스처 (u0 + k·(x-cx)/(y-hz),  K/(y-hz)),  y = first_row 가 텍스처 맨 아래줄."""
    K = (first_row - hz) * (texture.size[1] - 1)
    coeffs = (-k / hz, -u0 / hz, (k * cx + u0 * hz) / hz, 0, 0, -K / hz, 0, -1 / hz)
    return texture.transform(size, Image.PERSPECTIVE, coeffs, Image.BICUBIC)

def finish(img, grain=5, vignette=0.32, blur=0.5):
    img = img.filter(ImageFilter.GaussianBlur(blur))
    g = Image.effect_noise(img.size, grain)
    img = ImageChops.overlay(img, g if img.mode == 'L' else Image.merge('RGB', [g, g, g]))
    vg = Image.radial_gradient('L').resize(img.size)
    return Image.composite(Image.new(img.mode, img.size, 0), img, vg.point(lambda v: int(max(0, v - 110) * vignette)))

# ---------------------------------------------------------------- 360 상황인식 (주간)
W, PH, GAP = 1590, 455, 5

def field_texture(seed):
    random.seed(seed)
    t = tex(TW, TH, (58, 70, 38), (108, 118, 68), base=24, octaves=7, persist=0.55)          # 풀
    dry = tex(TW, TH, (116, 106, 70), (164, 150, 104), base=30, octaves=6)                  # 마른 풀
    t = Image.composite(dry, t, thresh(fnoise(TW, TH, base=3, octaves=4), 132, 3.2))
    fur = Image.new('L', (TW, TH), 0); df = ImageDraw.Draw(fur)                              # 밭고랑
    for x in range(0, TW, 13):
        df.line([(x, 0), (x, TH)], fill=90, width=5)
    fm = thresh(fnoise(TW, TH, base=2, octaves=3), 150, 4)
    t.paste((72, 64, 44), (0, 0), ImageChops.multiply(fur.filter(ImageFilter.GaussianBlur(1.6)), fm))
    u0, rw = TW // 2, 96                                                                     # 흙길
    rm = Image.new('L', (TW, TH), 0); ImageDraw.Draw(rm).rectangle([u0 - rw // 2, 0, u0 + rw // 2, TH], fill=255)
    rm = ImageChops.multiply(rm.filter(ImageFilter.GaussianBlur(5)),
                             fnoise(TW, TH, base=80, octaves=3).point(lambda v: min(255, 150 + v)))
    t.paste(tex(TW, TH, (104, 92, 70), (156, 142, 112), base=48, octaves=6), (0, 0), rm)
    ruts = Image.new('L', (TW, TH), 0); dr = ImageDraw.Draw(ruts)
    for s in (-1, 1):
        dr.line([(u0 + s * 23, 0), (u0 + s * 23, TH)], fill=160, width=10)
    t.paste((82, 72, 54), (0, 0), ruts.filter(ImageFilter.GaussianBlur(3)))
    mid = Image.new('L', (TW, TH), 0); ImageDraw.Draw(mid).line([(u0, 0), (u0, TH)], fill=90, width=12)
    t.paste((96, 104, 62), (0, 0), mid.filter(ImageFilter.GaussianBlur(3)))                 # 가운데 풀줄
    return t

def sa_panel(front):
    seed = 11 if front else 23
    random.seed(seed); rnd = random.Random(seed)
    img = Image.new('RGB', (W, PH))
    hz = 205
    vgrad(img, (0, 0, W, hz + 90), (88, 114, 140) if front else (100, 122, 144), (212, 219, 222))
    c = fnoise(W, hz + 40, base=3, octaves=6, persist=0.62)                                   # 구름
    cm = ImageChops.multiply(thresh(c, 122, 2.2), vfade((W, hz + 40), 220, 0))
    img.paste(ImageOps.colorize(c, (156, 168, 178), (244, 246, 247)), (0, 0), cm)
    if front:                                                                                 # 해 쪽 밝음
        gm0 = Image.new('L', (W, PH), 0)
        ImageDraw.Draw(gm0).ellipse([int(W * 0.72) - 380, -300, int(W * 0.72) + 380, 170], fill=80)
        img.paste((255, 248, 232), (0, 0), gm0.filter(ImageFilter.GaussianBlur(120)))
    for (b, waves, dark, light, rough, s) in (                                                # 원경 산 두 겹
            (hz - 34, [(2.1, 34), (5.3, 14), (11, 6)], (124, 139, 151), (160, 172, 181), 1.1, 1),
            (hz - 12, [(3.2, 20), (7.7, 9), (17, 3)], (100, 113, 119), (134, 145, 147), 1.4, 2)):
        img.paste(tex(W, PH, dark, light, base=14, octaves=5), (0, 0),
                  poly_mask((W, PH), profile(W, b, waves, seed + s, rough), 0.8))
    img.paste((192, 201, 206), (0, 0), band_mask((W, PH), hz - 8, 26, 105))                  # 대기 연무
    hill = profile(W, hz + 8, [(1.4, 9), (4.9, 5), (13, 2)], seed + 3, 1.2, 2)                # 가까운 구릉
    img.paste(tex(W, PH, (56, 68, 42), (92, 104, 64), base=24, octaves=5), (0, 0), poly_mask((W, PH), hill, 0.8))
    for (h, w, dens, dark, light) in (((5, 12), (6, 16), 1.0, (40, 50, 32), (70, 82, 52)),   # 수목선 두 줄
                                      ((3, 7), (4, 9), 1.4, (34, 44, 28), (58, 70, 44))):
        cm2 = crowns((W, PH), hill, rnd, h, w, dens,
                     gaps=lambda x, n=fnoise(W, 1, base=8, octaves=3): n.getpixel((min(W - 1, max(0, int(x))), 0)) < 70)
        img.paste(tex(W, PH, dark, light, base=90, octaves=4), (0, 0), cm2.filter(ImageFilter.GaussianBlur(0.6)))
    first = hz + 14                                                                           # 땅 (원경 투영)
    g = perspective_ground(widen(field_texture(seed + 40)), (W, PH), hz, first, W / 2 + (0 if front else 14), 44, TW)
    gm = poly_mask((W, PH), profile(W, hz + 17, [(1.1, 2.5), (3.3, 1.5)], seed + 4, 0.5, 6), 1.2)
    img.paste(g, (0, 0), gm)
    near_detail(img, hz, (52, 62, 34), (118, 126, 76), 150, rgb=True)                          # 가까운 풀 질감
    rc = W / 2 + (0 if front else 14)
    blades = Image.new('RGB', (W, PH)); bm = Image.new('L', (W, PH), 0)
    db, dm = ImageDraw.Draw(blades), ImageDraw.Draw(bm)
    for _ in range(5200):                                                                     # 풀포기 — 길은 피한다
        t = rnd.random() ** 0.6
        y = hz + 40 + t * (PH - hz - 70)
        x = rnd.uniform(0, W)
        if abs(x - rc) < 48 * (y - hz) / 44 + 6:
            continue
        L = 2 + 13 * t; a = rnd.uniform(-0.35, 0.35)
        g0 = rnd.randint(46, 86)
        pts = [(x, y), (x + L * math.sin(a), y - L * math.cos(a))]
        db.line(pts, fill=(g0 - 10, g0 + 8, g0 - 26), width=1 if t < 0.5 else 2)
        dm.line(pts, fill=int(120 + 110 * t), width=1 if t < 0.5 else 2)
    img.paste(blades.filter(ImageFilter.GaussianBlur(0.5)), (0, 0), bm.filter(ImageFilter.GaussianBlur(0.5)))
    img.paste((196, 204, 206), (0, 0), band_mask((W, PH), hz + 17, 14, 70))                  # 지평선 연무
    img.paste((0, 0, 0), (0, 0), vfade((W, PH), 0, 60))
    hull = [(x, PH - 22 - 10 * math.sin(math.pi * x / W)) for x in range(0, W + 1, 10)]      # 초점 밖 차체 끝
    img.paste(tex(W, PH, (22, 24, 22), (46, 48, 44), base=8, octaves=3), (0, 0), poly_mask((W, PH), hull, 3))
    img = ImageEnhance.Contrast(ImageEnhance.Color(img).enhance(0.88)).enhance(1.05)
    return finish(img)

def make_sa():
    canvas = Image.new('RGB', (W, PH * 2 + GAP), (6, 7, 8))
    canvas.paste(sa_panel(True), (0, 0))
    canvas.paste(sa_panel(False), (0, PH + GAP))
    p = os.path.join(OUT, 'sa_360_1590x915.png')
    canvas.save(p, optimize=True)
    return p

# ---------------------------------------------------------------- 조준경 (열상 · 백열)
SW, SH = 1200, 915

def thermal_ground(seed):
    random.seed(seed)
    t = remap(fnoise(TW, TH, base=20, octaves=7, persist=0.62), 50, 104)                    # 흙·풀 온도 차
    warm = thresh(fnoise(TW, TH, base=4, octaves=4), 140, 2.0)                                # 햇볕 받은 맨땅
    t = ImageChops.add(t, warm.point(lambda v: v // 7))
    cool = thresh(fnoise(TW, TH, base=6, octaves=4), 150, 2.5)                                # 풀숲 그늘
    t = ImageChops.subtract(t, cool.point(lambda v: v // 9))
    return t

def scope_scene():
    random.seed(5); rnd = random.Random(5)
    img = Image.new('L', (SW, SH))
    hz = 395
    vgrad(img, (0, 0, SW, hz + 80), 18, 56)                                                   # 차가운 하늘
    img = ImageChops.add(img, remap(fnoise(SW, SH, base=2, octaves=3), 0, 8))
    far = profile(SW, hz - 4, [(2.2, 5), (5.1, 3)], 7, 0.8, 2)                                # 먼 수목선 (옅게)
    fm = crowns((SW, SH), far, rnd, (4, 10), (5, 12), 1.4)
    img.paste(remap(fnoise(SW, SH, base=60, octaves=4), 60, 76), (0, 0), fm.filter(ImageFilter.GaussianBlur(1.2)))
    near = profile(SW, hz + 4, [(1.3, 4), (3.7, 3)], 8, 0.6, 2)                               # 가까운 나무 무리
    cl = fnoise(SW, 1, base=5, octaves=3)
    nm = crowns((SW, SH), near, rnd, (10, 34), (9, 24), 1.5,
                gaps=lambda x: cl.getpixel((min(SW - 1, max(0, int(x))), 0)) < 120)
    img.paste(remap(fnoise(SW, SH, base=110, octaves=5, persist=0.65), 66, 112), (0, 0), nm.filter(ImageFilter.GaussianBlur(0.9)))
    g = perspective_ground(widen(thermal_ground(19)), (SW, SH), hz, hz + 12, SW / 2, 44, TW)   # 땅 (원근 투영)
    gm = poly_mask((SW, SH), profile(SW, hz + 14, [(1.2, 3), (3.1, 2)], 9, 0.5, 6), 1.5)
    img.paste(g, (0, 0), gm)
    near_detail(img, hz, 48, 100, 120)                                                         # 가까운 잔 질감
    img.paste(64, (0, 0), band_mask((SW, SH), hz + 15, 10, 60))                              # 지평선 대기
    sig = Image.new('L', (SW, SH), 0); ds = ImageDraw.Draw(sig)                               # 먼 거리 차량 열원
    cx, cy = 600, 452
    ds.polygon([(cx - 50, cy - 2), (cx - 40, cy - 10), (cx + 46, cy - 10), (cx + 50, cy + 10), (cx - 48, cy + 10)], fill=132)
    ds.rectangle([cx - 16, cy - 20, cx + 18, cy - 9], fill=120)                                # 상부 구조
    ds.line([(cx - 16, cy - 16), (cx - 62, cy - 18)], fill=112, width=3)                        # 포신
    ds.rectangle([cx - 46, cy + 8, cx + 46, cy + 16], fill=156)                                 # 주행장치
    ds.ellipse([cx + 22, cy - 9, cx + 48, cy + 5], fill=204)                                    # 기관부
    img = ImageChops.lighter(img, sig.filter(ImageFilter.GaussianBlur(2.0)))
    plume = Image.new('L', (SW, SH), 0)
    ImageDraw.Draw(plume).ellipse([cx + 10, cy - 34, cx + 52, cy - 4], fill=20)
    img = ImageChops.add(img, plume.filter(ImageFilter.GaussianBlur(8)))
    img = img.filter(ImageFilter.GaussianBlur(1.0))
    cols = Image.effect_noise((SW, 1), 2.5).resize((SW, SH), Image.NEAREST)                  # 센서 세로 줄무늬(약하게)
    img = ImageChops.overlay(img, cols)
    return finish(img, grain=5, vignette=0.28, blur=0.3)

def scope_reticle():
    ov = Image.new('RGBA', (SW, SH), (0, 0, 0, 0)); d = ImageDraw.Draw(ov)
    cx, cy, gap, post = 600, 457, 9, 175
    INK, HALO = (8, 8, 8, 235), (225, 225, 225, 90)
    def seg(a, b, w):
        d.line([a, b], fill=HALO, width=w + 2); d.line([a, b], fill=INK, width=w)
    seg((0, cy), (cx - post, cy), 5); seg((cx + post, cy), (SW, cy), 5)                      # 좌·우 굵은 기둥
    seg((cx, cy + post), (cx, SH), 5)                                                          # 아래 굵은 기둥
    seg((cx - post, cy), (cx - gap, cy), 2); seg((cx + gap, cy), (cx + post, cy), 2)            # 가는 십자
    seg((cx, 0), (cx, cy - gap), 2); seg((cx, cy + gap), (cx, cy + post), 2)
    for k in range(1, 10):                                                                     # 밀 눈금
        L = 6 if k % 5 else 11
        for s in (-1, 1):
            seg((cx + s * (gap + k * 17), cy - L), (cx + s * (gap + k * 17), cy + L), 1)
            seg((cx - L, cy + s * (gap + k * 17)), (cx + L, cy + s * (gap + k * 17)), 1)
    return ov

def make_scope():
    img = Image.alpha_composite(scope_scene().convert('RGBA'), scope_reticle()).convert('RGB')
    p = os.path.join(OUT, 'scope_1200x915.png')
    img.save(p, optimize=True)
    return p

if __name__ == '__main__':
    for p in (make_sa(), make_scope()):
        im = Image.open(p)
        print('%s  %dx%d  %d KB' % (os.path.basename(p), im.size[0], im.size[1], os.path.getsize(p) // 1024))
