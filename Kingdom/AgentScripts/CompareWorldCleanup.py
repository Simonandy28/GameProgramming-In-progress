import re, pathlib, hashlib, json
root=pathlib.Path(r'D:\Kingdom')
old=root/'SetupBackup/WorldCleanup-2026-11-04/Assets/Glowkeeper_Update.unity'
new=root/'Assets/Glowkeeper_Update.unity'
def blocks(path):
 text=path.read_text(encoding='utf-8-sig')
 return {m.group(2):(m.group(1),m.group(3)) for m in re.finditer(r'^--- !u!(\d+) &(\d+)(?: stripped)?\n(.*?)(?=^--- !u!|\Z)',text,re.M|re.S)}
a,b=blocks(old),blocks(new)
removed=set(a)-set(b)
print('Removed serialized objects:',len(removed))
print('Removed named objects:',[re.search(r'^  m_Name: (.*)$',a[i][1],re.M).group(1) for i in removed if a[i][0]=='1' and re.search(r'^  m_Name: (.*)$',a[i][1],re.M)])
changes={}
for i in a.keys()&b.keys():
 if a[i]!=b[i]: changes.setdefault(a[i][0],[]).append(i)
print('Modified surviving sections by type:',{k:len(v) for k,v in changes.items()})
transforms=[]
for i in changes.get('4',[]):
 # Removing a child reference is expected; positions, rotations, scale must match.
 for prop in ['m_LocalRotation','m_LocalPosition','m_LocalScale','m_Father']:
  before=re.search(r'^  '+prop+r':.*$',a[i][1],re.M)
  after=re.search(r'^  '+prop+r':.*$',b[i][1],re.M)
  if (before.group() if before else None)!=(after.group() if after else None):transforms.append((i,prop))
print('Changed surviving transform coordinates/parents:',transforms)
script_guids=['f794676c123','']
print('Avatar Animator SHA256:',hashlib.sha256((root/'Assets/GlowKeeper/Animation/Wizard/WizardAnimator.controller').read_bytes()).hexdigest())
for i in changes.get('114',[]):
 before=a[i][1].splitlines();after=b[i][1].splitlines()
 import difflib
 delta=list(difflib.unified_diff(before,after,n=1))
 print('Component change',i,'\n'+'\n'.join(delta[:50]))
assert not transforms
out=root/'AgentScripts/world-cleanup-scene-comparison.json'
out.write_text(json.dumps({'removed_serialized_objects':len(removed),'modified_surviving_types':{k:len(v) for k,v in changes.items()},'changed_transform_coordinates':transforms},indent=2))
