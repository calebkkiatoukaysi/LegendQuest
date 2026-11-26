import json, sys, numpy as np
from PIL import Image, ImageDraw
R="Content/Characters/Parts/"
O="docs/art-audit/"
d=json.load(open(R+"parts.json"))
slots=sys.argv[1].split(","); out=sys.argv[2]; cell=int(sys.argv[3]) if len(sys.argv)>3 else 160; cols=int(sys.argv[4]) if len(sys.argv)>4 else 10
pages={}
items=[(k,p) for k,p in d["parts"].items() if p["slot"] in slots and "rect" in p]
rows=(len(items)+cols-1)//cols
sheet=Image.new("RGB",(cols*cell,rows*(cell+12)),(255,0,255))
dr=ImageDraw.Draw(sheet)
for i,(k,p) in enumerate(items):
    if p["page"] not in pages: pages[p["page"]]=Image.open(R+p["page"]+".png").convert("RGBA")
    x,y,w,h=p["rect"]; im=pages[p["page"]].crop((x,y,x+w,y+h))
    s=min((cell-4)/w,(cell-4)/h); im=im.resize((max(1,int(w*s)),max(1,int(h*s))),Image.NEAREST)
    cx,cy=(i%cols)*cell,(i//cols)*(cell+12)
    bg=Image.new("RGBA",(cell,cell),(40,160,60,255)); bg.alpha_composite(im,((cell-im.width)//2,(cell-im.height)//2))
    sheet.paste(bg.convert("RGB"),(cx,cy))
    dr.rectangle([cx+(cell-im.width)//2-1,cy+(cell-im.height)//2-1,cx+(cell+im.width)//2,cy+(cell+im.height)//2],outline=(255,255,0))
    dr.text((cx+2,cy+cell),k[:26],fill=(0,0,0))
sheet.save(O+out); print(sheet.size)
