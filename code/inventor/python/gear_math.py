"""gear_math.py - involute spur gear geometry (pure Python, no Inventor).

Same formulas as code/inventor/csharp/02-SpurGearGenerator/GearMath.cs - tools/test_tools.py checks
both give identical numbers.  Units: mm, radians.

    python gear_math.py 2 40 --alpha 20
"""
from __future__ import annotations

import argparse
import math
from dataclasses import dataclass


@dataclass
class Gear:
    m: float
    z: int
    alpha_deg: float = 20.0
    x: float = 0.0

    @property
    def alpha(self) -> float:
        return math.radians(self.alpha_deg)

    @property
    def rp(self) -> float:
        return self.m * self.z / 2

    @property
    def rb(self) -> float:
        return self.rp * math.cos(self.alpha)

    @property
    def ra(self) -> float:
        return self.rp + self.m * (1 + self.x)

    @property
    def rf(self) -> float:
        return self.rp - self.m * (1.25 - self.x)

    @property
    def psi_p(self) -> float:
        """half tooth angle at the pitch circle"""
        return math.pi / (2 * self.z) + 2 * self.x * math.tan(self.alpha) / self.z

    def flank_angle(self, r: float) -> float:
        r = max(r, self.rb)
        return self.psi_p + inv(self.alpha) - inv(math.acos(self.rb / r))

    def thickness_at(self, r: float) -> float:
        return 2 * self.flank_angle(r) * r

    def upper_flank(self, n: int = 12) -> list[tuple[float, float]]:
        r0 = max(self.rb, self.rf)
        pts = []
        for i in range(n):
            r = r0 + (self.ra - r0) * i / (n - 1)
            th = self.flank_angle(r)
            pts.append((r * math.cos(th), r * math.sin(th)))
        return pts

    def is_pointed(self) -> bool:
        return self.flank_angle(self.ra) <= 0


def inv(a: float) -> float:
    return math.tan(a) - a


def min_teeth_no_undercut(alpha_deg: float = 20.0) -> float:
    return 2 / math.sin(math.radians(alpha_deg)) ** 2


def center_distance(m: float, z1: int, z2: int) -> float:
    return m * (z1 + z2) / 2


def main() -> None:
    ap = argparse.ArgumentParser()
    ap.add_argument("m", type=float)
    ap.add_argument("z", type=int)
    ap.add_argument("--alpha", type=float, default=20.0)
    ap.add_argument("--x", type=float, default=0.0)
    a = ap.parse_args()
    g = Gear(a.m, a.z, a.alpha, a.x)
    print(f"pitch d  = {2 * g.rp:.3f} mm")
    print(f"tip   da = {2 * g.ra:.3f} mm")
    print(f"root  df = {2 * g.rf:.3f} mm")
    print(f"base  db = {2 * g.rb:.3f} mm")
    print(f"tooth thickness at pitch = {g.thickness_at(g.rp):.4f} mm (= pi*m/2 = {math.pi * a.m / 2:.4f} for x=0)")
    print(f"tip thickness            = {g.thickness_at(g.ra):.4f} mm")
    print(f"z_min (no undercut)      = {min_teeth_no_undercut(a.alpha):.2f}")


if __name__ == "__main__":
    main()
