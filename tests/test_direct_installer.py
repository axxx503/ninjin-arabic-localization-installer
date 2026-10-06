from pathlib import Path
import subprocess,hashlib,shutil,json
root=Path(__file__).resolve().parent
exe=root/'DirectInstaller-Test.exe';game=root/'direct-install-test';data=game/'NinjinClashofCarrots_Data';data.mkdir(parents=True,exist_ok=True)
target=data/'resources.assets';source=root/'backups/resources.assets.original';patched=root/'patched/resources.assets'
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def run(command,success=True):
    result=subprocess.run([str(exe),command,str(game)],capture_output=True)
    assert (result.returncode==0)==success,(command,result.returncode,result.stderr)
shutil.copy2(source,target)
marker=game/'preserve-progress';marker.write_bytes(b'no saved data is touched')
run('--install');assert sha(target)==sha(patched)
assert sha(data/'.ninjin-arabic-direct-backup/resources.assets')==sha(source)
run('--install');assert sha(target)==sha(patched)
run('--uninstall');assert sha(target)==sha(source)
assert marker.read_bytes()==b'no saved data is touched'
target.write_bytes(b'unknown game modification')
run('--install',False);assert target.read_bytes()==b'unknown game modification'
run('--uninstall',False);assert target.read_bytes()==b'unknown game modification'
shutil.copy2(source,target)
backup=data/'.ninjin-arabic-direct-backup/resources.assets';backup.write_bytes(b'invalid backup')
run('--install',False);assert sha(target)==sha(source)
run('--uninstall',False);assert sha(target)==sha(source)
shutil.copy2(source,backup)
result={'direct_installer_compile':'passed','delta_exact_reconstruction':'passed','latest_1315_draft_build_install':'passed','repeat_install':'passed','exact_uninstall':'passed','unknown_game_rejected':'passed','corrupt_backup_rejected':'passed','unrelated_marker_preserved':'passed','runtime_visual_test':'not performed','font_in_test_build':'Saudi; internal candidate only; NOT permitted public distribution established','patched_sha256':sha(patched)}
(root/'direct-installer-test-results.json').write_text(json.dumps(result,indent=2))
print('DIRECT INSTALLER CHECKS PASSED')
