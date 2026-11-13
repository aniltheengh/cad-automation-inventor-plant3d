"""Numerical regression checks for published examples and model identities.
Run: python check_mechanics.py. Standard library only.
These are independent calculations, not a proof of all printed equations.
"""
import math
checks = 0
def check(name, actual, expected, tol=5e-4):
    global checks
    assert math.isclose(actual, expected, rel_tol=tol, abs_tol=1e-8), (name, actual, expected)
    checks += 1
pi=math.pi
check('01 support reaction',6*1/4,1.5)
check('02 rod stress',30000/(pi*20**2/4),95.493)
check('03 shear modulus',200/(2*1.3),76.923)
check('03 resilience',250**2/(2*200000),.15625)
check('04 extension',20000*1000/(200*200000),.5)
check('04 free thermal extension',12e-6*40*1000,.48)
check('05 steel share',60000*(200000*200)/(200000*200+70000*400),35294.1176)
check('06 rectangle inertia',40*80**3/12,1706666.6667)
check('07 shaft stress',16*500000/(pi*40**3),39.789)
check('07 twist',500000*1000/(80000*pi*40**4/32),.024868)
check('08 UDL moment',2*4**2/8,4)
check('09 bending stress',2e6*40/(40*80**3/12),46.875)
check('10 rectangular shear',1.5*10000/(40*80),4.6875)
check('11 cantilever deflection',1000*1000**3/(3*200000*2e6),.833333)
check('12 hoop stress',2*100/5,40)
check('12 spring deflection',8*100*40**3*8/(80000*5**4),8.192)
sx,sy,t=80,20,30
c=(sx+sy)/2; r=math.hypot((sx-sy)/2,t)
check('13 major principal stress',c+r,92.4264)
check('13 principal trace',(c+r)+(c-r),sx+sy)
check('13 principal determinant',(c+r)*(c-r),sx*sy-t*t)
check('14 plane stress x',200000/(1-.3**2)*(500e-6-.3*100e-6),103.2967)
check('14 engineering shear',200000/(2*1.3)*200e-6,15.3846)
check('15 von Mises',math.sqrt(100**2+3*50**2),132.2876)
check('15 hydrostatic von Mises',math.sqrt(((70-70)**2+(70-70)**2+(70-70)**2)/2),0)
check('15 pure shear yield',math.sqrt(3*(250/math.sqrt(3))**2),250)
check('16 axial energy',10000**2*1000/(2*200000*100),2500)
check('17 propped reaction',3*2*4/8,3)
check('18 Euler',pi*pi*200000*1e6/2000**2,493480.22)
E,Sy=200000,250
lam=math.sqrt(2*pi*pi*E/Sy)
check('18 Johnson Euler junction',Sy*(1-Sy*lam**2/(4*pi*pi*E)),pi*pi*E/lam**2)
check('19 eccentric bending magnitude',1000*20*100/(100*200**3/12),.03)
b,ri,ro,M=20,40,80,100000
A=b*(ro-ri); rc=(ri+ro)/2; rn=(ro-ri)/math.log(ro/ri); e=rc-rn
check('20 inner curved stress',M/(A*e)*(rn/ri-1),24.141)
check('20 outer curved stress',M/(A*e)*(rn/ro-1),-15.196)
check('20 zero axial resultant',M/(A*e)*b*(rn*math.log(ro/ri)-(ro-ri)),0)
check('20 bending resultant',-M/(A*e)*b*(rn*((ro-ri)-rc*math.log(ro/ri))-((ro**2-ri**2)/2-rc*(ro-ri))),M)
a,b,pii,po=50,100,10,0
C=(pii*a*a-po*b*b)/(b*b-a*a);D=a*a*b*b*(pii-po)/(b*b-a*a)
check('21 bore radial pressure',C-D/a**2,-pii)
check('21 outer radial pressure',C-D/b**2,-po)
check('21 bore hoop',C+D/a**2,16.6667)
check('21 closed end balance',C*(b*b-a*a),pii*a*a-po*b*b)
nu=.3
check('21 plane strain axial strain',(2*nu*C-nu*(2*C))/E,0)
check('22 closed cell twist',1e6*1000/(80000*(4*5000**2/(300/2))),.01875)
check('23 Goodman factor',1/(50/200+70/500),2.5641026)
check('23 critical crack',1/pi*(50/(1*100))**2,.07957747)
check('24 rectangle plastic moment',250*40*80**2/4/1e6,16)
check('25 transverse modulus',1/(.6/200+.4/3),7.33496)
check('26 bar displacement',10000/(200000*100/1000),.5)
check('26 support reaction',-(200000*100/1000)*.5,-10000)
print(f'{checks} numerical checks passed')
