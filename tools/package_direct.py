from pathlib import Path
import hashlib,json,zipfile
root=Path(__file__).resolve().parent.parent
folder=root/'outputs/Ninjin-Arabic-Ready-0.2.1';archive=folder.with_suffix('.1.zip') if False else root/'outputs/Ninjin-Arabic-Ready-0.2.1.zip'
report=json.loads((folder/'build-report.json').read_text())
report['package_version']='0.2.1';(folder/'build-report.json').write_text(json.dumps(report,ensure_ascii=False,indent=2),encoding='utf8')
tests=json.loads((folder/'installer-tests.json').read_text())
tests['font_in_test_build']='Saudi-derived glyph bitmaps; original TTF not included; official terms linked in README'
tests['delivered_exe_sha256']=hashlib.sha256((folder/'Install-Arabic.exe').read_bytes()).hexdigest()
assert tests['delivered_exe_sha256']==hashlib.sha256((root/'work/DirectInstaller-Test.exe').read_bytes()).hexdigest()
tests['package_version']='0.2.1';(folder/'installer-tests.json').write_text(json.dumps(tests,ensure_ascii=False,indent=2),encoding='utf8')
paths=sorted(p for p in folder.rglob('*') if p.is_file())
assert not any(p.suffix.lower() in ['.ttf','.otf','.assets','.py'] for p in paths)
with zipfile.ZipFile(archive,'w',compression=zipfile.ZIP_DEFLATED,compresslevel=9) as z:
    for p in paths:z.write(p,folder.name+'/'+p.relative_to(folder).as_posix())
with zipfile.ZipFile(archive) as z:
    assert z.testzip() is None
    assert len(z.infolist())==len(paths)
    for p in paths:assert z.read(folder.name+'/'+p.relative_to(folder).as_posix())==p.read_bytes()
digest=hashlib.sha256(archive.read_bytes()).hexdigest()
Path(str(archive)+'.sha256').write_text(digest+'  '+archive.name+'\n')
print('READY ZIP VERIFIED',archive.stat().st_size,'bytes;',len(paths),'files;',digest)
