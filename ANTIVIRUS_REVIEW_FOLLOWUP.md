# Antivirus review follow-up

Last updated: 2026-07-28

This document records the remaining false-positive review work for the Windows
native libraries in UImGui 7.2.0 / Dear ImGui 1.92.9.

## Current conclusion

The available evidence strongly supports false positives, but it is not an
absolute proof that the binaries are safe. Vendor determinations are still
pending.

Checks already completed:

- The DLLs were rebuilt from the public source and the GitHub Actions build
  matrix passed for Windows x64, x86, and ARM64, Linux x64, and macOS universal.
- Native functional tests passed: 7/7.
- Unity 6000.5.2f1 EditMode tests passed: 19/19.
- The managed binding build completed with zero errors and zero warnings.
- All Windows DLLs have ASLR, DEP/NX, and CFG enabled.
- Their PE imports were limited to `KERNEL32.dll`, `USER32.dll`,
  `SHELL32.dll`, and `IMM32.dll`.
- Microsoft Defender scanned the final package and found no threats.
- The four DLLs that Microsoft flagged on VirusTotal were then scanned
  individually with Defender engine `1.1.26060.3008` and definitions
  `1.455.392.0`; all four local scans found no threats.
- ESET-NOD32 on VirusTotal reported all seven DLLs as undetected.
- Three of the seven DLLs received zero malicious verdicts on VirusTotal.

The difference between the Microsoft VirusTotal verdict and the local Defender
result, despite the matching engine version, is consistent with a
cloud/reputation/heuristic false positive. It is supporting evidence, not proof.

## VirusTotal results

| File | SHA-256 | VirusTotal verdicts requiring review |
| --- | --- | --- |
| `cimCTE.dll` | `bdec320e3a46b361e0cc8ef9f682b63abb0325298701ab2705d970212f9cc0c2` | Elastic: `malicious (moderate confidence)`; Microsoft: `Trojan:Win32/Wacatac.B!ml` |
| `cimguizmo.dll` | `0e1069b59b3efb13cdf8f9c7b2829ace469fc78fcafa2bba61e24a00c002a1a5` | IKARUS: `Trojan.Win64.Krypt`; Microsoft: `Trojan:Win32/Wacatac.B!ml` |
| `cimguizmo_quat.dll` | `eb59b84d52f847f247c7131a23662e49cdade202ef8b15531062a30b54a5fa42` | IKARUS: `Trojan.Win64.Krypt`; Microsoft: `Trojan:Win32/Wacatac.B!ml` |
| `cimnodes.dll` | `e59df8a1b9ace1bb06c6e9ef33f6a071606125018962c20a191f2e4acd429fd2` | None |
| `cimnodes_r.dll` | `16e90c17106d2c23906d747deb9623e6b7de6c09bcc6fda75615908778adb57a` | Microsoft: `Trojan:Win32/Wacatac.B!ml` |
| `cimplot.dll` | `396d74cd9d9cf07ed6b80ca7050356ed9c2f00d99656a045ee6804eca69330e7` | None |
| `cimplot3d.dll` | `05ea9aad567b98b8d8ac44fe3788d4b2861cbd5b6b8225d3f43ec2f2a1e1f1bb` | None |

VirusTotal reports:

- [cimCTE.dll](https://www.virustotal.com/gui/file/bdec320e3a46b361e0cc8ef9f682b63abb0325298701ab2705d970212f9cc0c2)
- [cimguizmo.dll](https://www.virustotal.com/gui/file/0e1069b59b3efb13cdf8f9c7b2829ace469fc78fcafa2bba61e24a00c002a1a5)
- [cimguizmo_quat.dll](https://www.virustotal.com/gui/file/eb59b84d52f847f247c7131a23662e49cdade202ef8b15531062a30b54a5fa42)
- [cimnodes.dll](https://www.virustotal.com/gui/file/e59df8a1b9ace1bb06c6e9ef33f6a071606125018962c20a191f2e4acd429fd2)
- [cimnodes_r.dll](https://www.virustotal.com/gui/file/16e90c17106d2c23906d747deb9623e6b7de6c09bcc6fda75615908778adb57a)
- [cimplot.dll](https://www.virustotal.com/gui/file/396d74cd9d9cf07ed6b80ca7050356ed9c2f00d99656a045ee6804eca69330e7)
- [cimplot3d.dll](https://www.virustotal.com/gui/file/05ea9aad567b98b8d8ac44fe3788d4b2861cbd5b6b8225d3f43ec2f2a1e1f1bb)

## Submission checklist

### Microsoft

Official portal:
[Microsoft Security Intelligence sample submission](https://feedback.smartscreen.microsoft.com/en-us/wdsi)

Submit these four files separately:

- [ ] `cimCTE.dll`
- [ ] `cimguizmo.dll`
- [ ] `cimguizmo_quat.dll`
- [ ] `cimnodes_r.dll`

Suggested form choices:

- Submit as a software developer.
- Product: Microsoft Defender Antivirus / Windows 11.
- Classification: incorrectly detected as malware or malicious.
- Detection name: `Trojan:Win32/Wacatac.B!ml`.
- Defender definition used for the clean local rescan: `1.455.392.0`.
- Add the corresponding SHA-256 and VirusTotal report URL.

### IKARUS

Official instructions:
[IKARUS anti.virus FAQ](https://www.ikarussecurity.com/en/managed-it-ot-security-solutions/ikarus-anti-virus/faqs-ikarus-anti-virus/)

IKARUS normally asks users to send a false positive from the installed
product's quarantine by right-clicking the entry and choosing **Send to
IKARUS**. Because the detection was seen only on VirusTotal, contact
[`support@ikarus.at`](mailto:support@ikarus.at) using the
[official support page](https://www.ikarussecurity.com/en/support/) if the
quarantine option is unavailable.

Submit:

- [ ] `cimguizmo.dll`
- [ ] `cimguizmo_quat.dll`

Detection name: `Trojan.Win64.Krypt`.

### Elastic

Official false-positive form:
[Elastic Security false positive submission](https://docs.google.com/forms/d/e/1FAIpQLSfKZOPSPcucmgNR9_j316JnG_qYbJBpti5JSsNxQNQtTHjsxw/viewform)

Process reference:
[Elastic community instructions](https://discuss.elastic.co/t/submitting-false-positives/232322)

Submit:

- [ ] `cimCTE.dll`

Include the SHA-256, VirusTotal URL, company/software name, contact name and
email, filename, project website, and the technical context below.

## Suggested English report

Use this as the description for each vendor, adjusting the filename, SHA-256,
detection name, and VirusTotal URL:

> We believe this detection is a false positive in a native interoperability
> library distributed with the open-source UImGui Unity package. The DLL was
> reproducibly built from public source for UImGui 7.2.0 / Dear ImGui 1.92.9.
> The complete cross-platform GitHub Actions build passed, native functional
> tests passed 7/7, and Unity EditMode tests passed 19/19. The Windows binary
> has ASLR, DEP/NX, and CFG enabled, and its imported libraries are limited to
> standard Windows system libraries. A local Microsoft Defender scan, including
> an individual rescan with engine 1.1.26060.3008 and definitions 1.455.392.0,
> found no threats. ESET-NOD32 on VirusTotal also reported the file as
> undetected. Please review and reclassify this sample if you confirm it is
> benign.

Project and review links:

- [UImGui repository](https://github.com/psydack/uimgui)
- [Antivirus issue #91](https://github.com/psydack/uimgui/issues/91)
- [Release update PR #94](https://github.com/psydack/uimgui/pull/94)
- [Native build workflow](https://github.com/psydack/ImGui.NET-nativebuild/actions/runs/30325021624)
- [Managed binding workflow](https://github.com/psydack/ImGui.NET.4Unity/actions/runs/30325677240)
- [UImGui validation workflow](https://github.com/psydack/uimgui/actions/runs/30326226614)

## After submitting

- [ ] Save each vendor's submission/case ID here.
- [ ] Record each vendor's final determination.
- [ ] Re-run VirusTotal after vendors publish updated signatures.
- [ ] Update issue #91 and PR #94 with the review results.
- [ ] Merge/release only under the project's chosen acceptance criterion.

Current GitHub state: PR #94 is open, marked ready for review, has green CI,
and has not been merged.

## Credential reminder

The VirusTotal API key used during testing is intentionally not recorded in
this repository. Because it was pasted into a chat, revoke/rotate it before
using VirusTotal again.
