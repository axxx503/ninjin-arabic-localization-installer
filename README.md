# Ninjin: Clash of Carrots — Arabic Localization Installer

Source code and verification material for the Windows installer distributed with the unofficial Arabic localization for *Ninjin: Clash of Carrots*.

## Purpose

This repository is provided so players and Nexus Mods reviewers can inspect the installer. The ready-to-use release package is distributed through Nexus Mods.

The installer checks that `resources.assets` is an unmodified supported game file, creates a checksum-verified backup, applies the Arabic localization patch, verifies the result, and restores the exact original file when **Remove Arabic** is selected. It does not read or alter save files.

## Contents

- `src/DirectInstaller.cs` — Windows installer source code.
- `tools/make_direct_patch.py` — creates the localization delta locally from a supported original game installation.
- `tools/package_direct.py` — packages the release archive.
- `tests/test_direct_installer.py` — installer integration tests.
- `docs/README_AR.md` — Arabic user instructions.
- `docs/NEXUS_DESCRIPTION_AR.md` — Nexus Mods page description.
- `docs/installer-tests.json` — recorded installer test results.

## Build

The installer is compiled with the Microsoft .NET Framework C# compiler included with Windows:

```powershell
C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe /nologo /target:winexe /out:Install-Arabic.exe /resource:direct-patch.bin.gz,patch /reference:System.Windows.Forms.dll /reference:System.Drawing.dll src\DirectInstaller.cs
```

`direct-patch.bin.gz` is generated locally from a supported original game installation. It is intentionally excluded from this repository. The repository does not distribute the game, `resources.assets`, save files, the release executable, or the original font file.

## Credits

- Pocket Trap and Modus Games — original game and assets.
- Saudi Ministry of Culture and ArabicType Ltd — Saudi font used to create the in-game Arabic glyphs. The original TTF is not included.
- UnityPy, Pillow, FontTools, HarfBuzz, arabic-reshaper, and python-bidi — development tools.

This is an unofficial community localization. All original rights remain with their respective owners.
