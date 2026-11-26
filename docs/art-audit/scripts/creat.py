import numpy as np
from PIL import Image, ImageDraw
C="Content/Creatures/"
O="docs/art-audit/"
sz={"sea-wraith":(32,48),"gloomfang":(48,32),"thornmother":(96,80),"hollowed-footman":(32,48),"hollowed-hexer":(32,48),"hollowed-sapper":(32,48),"vault-construct":(48,64)}
for n,(fw,fh) in sz.items():
    im=Image.open(C+n+".png").convert("RGBA"); a=np.array(im)[:,:,3]
    cols,rows=im.width//fw,im.height//fh
    touch=[];
    for r in range(rows):
        for c in range(cols):
            f=a[r*fh:(r+1)*fh,c*fw:(c+1)*fw]
            if not (f>0).any(): continue
            e={"L":int((f[:,0]>0).sum()),"R":int((f[:,-1]>0).sum()),"T":int((f[0]>0).sum()),"B":int((f[-1]>0).sum())}
            semi=int(((f>0)&(f<255)).sum())
            e={k:v for k,v in e.items() if v}
            if e: touch.append(((r,c),e))
    alphas=np.unique(a); 
    print(n,im.size,"grid",cols,"x",rows,"frames touching frame edge:",len(touch),touch[:8],"alpha levels:",len(alphas))
    # zoomed grid view
    z=4 if n!="thornmother" else 2
    big=Image.new("RGBA",im.size,(40,160,60,255)); big.alpha_composite(im); big=big.resize((im.width*z,im.height*z),Image.NEAREST)
    d=ImageDraw.Draw(big)
    for c in range(cols+1): d.line([(c*fw*z,0),(c*fw*z,big.height)],fill=(255,0,255))
    for r in range(rows+1): d.line([(0,r*fh*z),(big.width,r*fh*z)],fill=(255,0,255))
    big.convert("RGB").save(O+"cr-"+n+".png")
