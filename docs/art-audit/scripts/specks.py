import json, numpy as np
from PIL import Image
from scipy import ndimage as nd
R="Content/Characters/Parts/"
d=json.load(open(R+"parts.json")); pages={}
res=[]
for pid,p in d["parts"].items():
    if "pieces" not in p: continue
    for bn,e in p["pieces"].items():
        if e["page"] not in pages: pages[e["page"]]=np.array(Image.open(R+e["page"]+".png").convert("RGBA"))
        x,y,w,h=e["rect"]; a=pages[e["page"]][y:y+h,x:x+w,3]
        op=a>64; lab,n=nd.label(op)
        if n<2: continue
        sz=nd.sum(op,lab,range(1,n+1)); main=np.argmax(sz)+1
        dist=nd.distance_transform_edt(lab!=main)
        specks=[(int(sz[k-1]),float(dist[lab==k].min())) for k in range(1,n+1) if k!=main and sz[k-1]>=4]
        far=[s for s in specks if s[1]>=4]
        if far: res.append((pid,bn,len(far),sum(s[0] for s in far),max(s[1] for s in far)))
print("rig pieces with detached islands (>=4px, >=4px away from the main shape):",len(res),"of 864")
from collections import Counter
print(Counter(r[1] for r in res))
print("parts most affected:",Counter(r[0] for r in res).most_common(15))
for r in sorted(res,key=lambda r:-r[3])[:20]: print("  ",r)
for r in res:
    if r[0] in("armor-traveller","armor-brannock","armor-hollowed-hexer","armor-hollowed-footman","armor-sera","armor-mira","armor-urchin-f"): print(" *",r)
