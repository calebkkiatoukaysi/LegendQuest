import sys,glob
from PIL import Image, ImageDraw
# usage: cs.py out "glob" step x0 y0 x1 y1 cols scale
out,pat,step,x0,y0,x1,y1,cols,scale=sys.argv[1],sys.argv[2],int(sys.argv[3]),*map(int,sys.argv[4:8]),int(sys.argv[8]),float(sys.argv[9])
fs=sorted(glob.glob(pat))[::step]
w,h=int((x1-x0)*scale),int((y1-y0)*scale)
rows=(len(fs)+cols-1)//cols
S=Image.new("RGB",(cols*w,rows*(h+14)),(255,0,255));d=ImageDraw.Draw(S)
for i,f in enumerate(fs):
    im=Image.open(f).convert("RGB").crop((x0,y0,x1,y1)).resize((w,h),Image.LANCZOS)
    S.paste(im,((i%cols)*w,(i//cols)*(h+14))); d.text(((i%cols)*w+3,(i//cols)*(h+14)+h),f.replace("\\","/").split("/")[-1],fill=(0,0,0))
S.save(out);print(len(fs),S.size)
