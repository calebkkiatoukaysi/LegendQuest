import json, sys
from PIL import Image, ImageDraw
R="Content/Characters/Parts/"
O="docs/art-audit/"
d=json.load(open(R+"parts.json"))
names=sys.argv[1].split(","); out=sys.argv[2]; cell=int(sys.argv[3]); cols=int(sys.argv[4])
pages={}
rows=(len(names)+cols-1)//cols
sheet=Image.new("RGB",(cols*cell,rows*(cell+14)),(255,0,255)); dr=ImageDraw.Draw(sheet)
for i,n in enumerate(names):
    pid,_,piece=n.partition(":")
    p=d["parts"][pid]; e=p["pieces"][piece] if piece else p
    if e["page"] not in pages: pages[e["page"]]=Image.open(R+e["page"]+".png").convert("RGBA")
    x,y,w,h=e["rect"]; im=pages[e["page"]].crop((x-2,y-2,x+w+2,y+h+2))
    s=min((cell-4)/im.width,(cell-4)/im.height); im=im.resize((max(1,int(im.width*s)),max(1,int(im.height*s))),Image.NEAREST)
    cx,cy=(i%cols)*cell,(i//cols)*(cell+14)
    bg=Image.new("RGBA",(cell,cell),(40,160,60,255)); bg.alpha_composite(im,((cell-im.width)//2,(cell-im.height)//2))
    sheet.paste(bg.convert("RGB"),(cx,cy))
    dr.text((cx+2,cy+cell),n[:40],fill=(0,0,0))
sheet.save(O+out); print(sheet.size)
