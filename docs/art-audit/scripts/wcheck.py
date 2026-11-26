import json, numpy as np
from PIL import Image
from scipy import ndimage as nd
R="Content/Characters/Parts/"
d=json.load(open(R+"parts.json"))
pages={}
out=[]
for pid,p in d["parts"].items():
    ents=[(None,p)] if "rect" in p else list(p["pieces"].items())
    for piece,e in ents:
        if e["page"] not in pages: pages[e["page"]]=np.array(Image.open(R+e["page"]+".png").convert("RGBA"))
        x,y,w,h=e["rect"]; s=pages[e["page"]][y:y+h,x:x+w].astype(int)
        a=s[:,:,3]; op=a>128
        rgb=s[:,:,:3]; mx=rgb.max(2); mn=rgb.min(2)
        white=op&(mn>232); black=op&(mx<22)
        # flat region connected to border
        def border_conn(m):
            lab,n=nd.label(m)
            if n==0: return 0
            b=set(np.unique(np.concatenate([lab[0],lab[-1],lab[:,0],lab[:,-1]])))-{0}
            return int(sum((lab==k).sum() for k in b))
        wb,bb=border_conn(white),border_conn(black)
        lab,n=nd.label(op); sizes=nd.sum(op,lab,range(1,n+1)) if n else []
        big=int(sum(1 for z in sizes if z>=30))
        frac_main=float(max(sizes)/op.sum()) if n else 0
        holes=nd.binary_fill_holes(op)&~op
        hl,hn=nd.label(holes); hs=nd.sum(holes,hl,range(1,hn+1)) if hn else []
        bighole=int(max(hs)) if hn else 0
        out.append(dict(pid=pid,piece=piece,slot=p["slot"],rect=e["rect"],page=e["page"],opaque=int(op.sum()),white_border=wb,black_border=bb,frags=big,main=round(frac_main,2),bighole=bighole))
json.dump(out,open("docs/art-audit/wcheck.json","w"),indent=0)
def show(title,rows,f):
    print("==",title,len(rows))
    for r in rows: print("  ",f(r))
W=[r for r in out if r["piece"] is None]
show("opaque white bg (>=3% of opaque px, near-white, touching rect edge)",[r for r in out if r["white_border"]>=0.03*max(1,r["opaque"])],lambda r:(r["pid"],r["piece"],r["page"],r["rect"],"%.0f%%"%(100*r["white_border"]/r["opaque"])))
show("opaque black bg (>=3%)",[r for r in out if r["black_border"]>=0.03*max(1,r["opaque"])],lambda r:(r["pid"],r["piece"],r["page"],r["rect"],"%.0f%%"%(100*r["black_border"]/r["opaque"])))
show("fragmented single pictures (largest blob <70% of opaque px)",[r for r in W if r["main"]<0.7],lambda r:(r["pid"],r["slot"],r["rect"],"frags",r["frags"],"main",r["main"]))
show("enclosed holes >=150px (fist holes) in single pictures",[r for r in W if r["bighole"]>=150],lambda r:(r["pid"],r["slot"],r["rect"],"hole",r["bighole"]))
P=[r for r in out if r["piece"]]
print("rig pieces fragmented (main<0.7):",sum(1 for r in P if r["main"]<0.7),"of",len(P))
for r in sorted([r for r in P if r["main"]<0.7],key=lambda r:r["main"])[:25]: print("  ",r["pid"],r["piece"],r["rect"],r["frags"],r["main"])
print("rig pieces with hole>=150:",sum(1 for r in P if r["bighole"]>=150))
for r in sorted(P,key=lambda r:-r["bighole"])[:15]: print("  ",r["pid"],r["piece"],r["rect"],r["bighole"])
