"""Original-dependent object delta; no executable/font file is embedded."""
import sys,gzip,struct,hashlib,json
from pathlib import Path
sys.path.insert(0,r'C:\Users\m2233\Documents\Codex\2026-09-22\c-program-files-x86-steam-steamapps\work\pythonlibs')
import UnityPy
source=Path('work/backups/resources.assets.original').read_bytes()
target=Path('work/patched/resources.assets').read_bytes()
old={o.path_id:o for o in UnityPy.load(source).objects}
new=sorted(UnityPy.load(target).objects,key=lambda o:o.byte_start)
commands=[];pos=0
def literal(data):
    if data:commands.append(b'\x00'+struct.pack('<Q',len(data))+data)
for obj in new:
    literal(target[pos:obj.byte_start])
    data=target[obj.byte_start:obj.byte_start+obj.byte_size];baseline=old[obj.path_id]
    if data==source[baseline.byte_start:baseline.byte_start+baseline.byte_size]:
        commands.append(b'\x01'+struct.pack('<QQ',baseline.byte_start,len(data)))
    else:literal(data)
    pos=obj.byte_start+obj.byte_size
literal(target[pos:])
header=b'NJARDEL1'+hashlib.sha256(source).digest()+hashlib.sha256(target).digest()+struct.pack('<QI',len(target),len(commands))
patch=Path('work/direct-patch.bin.gz');patch.write_bytes(gzip.compress(header+b''.join(commands),compresslevel=9,mtime=0))
# Independent reconstruction of command stream and exact checksum verification.
decoded=gzip.decompress(patch.read_bytes());cursor=84;result=bytearray()
for i in range(struct.unpack_from('<I',decoded,80)[0]):
    kind=decoded[cursor];cursor+=1
    if kind==0:
        size=struct.unpack_from('<Q',decoded,cursor)[0];cursor+=8;result.extend(decoded[cursor:cursor+size]);cursor+=size
    elif kind==1:
        start,size=struct.unpack_from('<QQ',decoded,cursor);cursor+=16;result.extend(source[start:start+size])
    else:raise ValueError('Unknown command')
assert cursor==len(decoded) and bytes(result)==target
print('DELTA VERIFIED',len(commands),'commands;',patch.stat().st_size,'bytes')
