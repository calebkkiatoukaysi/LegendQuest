import json, numpy as np
from PIL import Image
R="Content/Characters/Parts/"
d=json.load(open(R+"parts.json"))
pages={}
def page(n):
    if n not in pages: pages[n]=np.array(Image.open(R+n+".png").convert("RGBA"))
    return pages[n]
def longest(b):
    best=cur=0
    for v in b:
        cur=cur+1 if v else 0; best=max(best,cur)
    return best
rows=[]
def check(pid,piece,e):
    pg=page(e["page"]); x,y,w,h=e["rect"]; a=pg[:,:,3]
    sub=a[y:y+h,x:x+w]
    edges={"L":sub[:,0],"R":sub[:,-1],"T":sub[0,:],"B":sub[-1,:]}
    res={}
    for k,v in edges.items():
        op=v>200
        res[k]=(op.mean(), longest(op), len(v))
    # outside ring (1px and 2px) bleed
    H,W=a.shape
    ring=np.zeros_like(a,bool)
    x0,y0,x1,y1=max(0,x-2),max(0,y-2),min(W,x+w+2),min(H,y+h+2)
    ring[y0:y1,x0:x1]=True; ring[y:y+h,x:x+w]=False
    outside=int((a[ring]>8).sum())
    # stray components: small separate blobs touching edge
    rows.append(dict(pid=pid,piece=piece,page=e["page"],rect=e["rect"],edges=res,outside=outside,slot=d["parts"][pid]["slot"]))
for pid,p in d["parts"].items():
    if "pieces" in p:
        for bn,e in p["pieces"].items(): check(pid,bn,e)
    elif "rect" in p: check(pid,None,p)
json.dump(rows,open("docs/art-audit/border.json","w"))
print("rects",len(rows))
def flat(r,minrun=20,frac=0.5):
    return [k for k,(f,run,n) in r["edges"].items() if run>=minrun and run>=frac*n]
import collections
c=collections.Counter(); cf=collections.Counter()
for r in rows:
    c[r["slot"]]+=1
    if flat(r): cf[r["slot"]]+=1
print("total per slot",dict(c)); print("with >=1 flat edge (opaque run>=20px and >=50% of edge)",dict(cf))
print("bleed outside rect (any alpha>8 within 2px ring):",sum(1 for r in rows if r["outside"]>0))
for r in sorted(rows,key=lambda r:-r["outside"])[:10]: print(" ",r["pid"],r["piece"],r["page"],r["rect"],r["outside"])
# For non-piece slots, flat edges are suspicious
print("NON-PIECE flat edges:")
for r in rows:
    if r["piece"] is None:
        fl=flat(r,10,0.3)
        if fl: print(" ",r["pid"],r["slot"],r["page"],r["rect"],{k:(round(r['edges'][k][0],2),r['edges'][k][1]) for k in fl})
