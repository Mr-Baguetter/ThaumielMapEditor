import re
from pathlib import Path

BASE = Path(__file__).resolve().parent

MAINFILE = BASE / r"ThaumielMapEditor/Main.cs"
ASSEMBLYFILE = BASE / r"ThaumielMapEditor/Properties/AssemblyInfo.cs"

maincontent = MAINFILE.read_text()
versionmatch = re.search(r'Version\s*=>\s*new\s*\(\s*(\d+)\s*,\s*(\d+)\s*,\s*(\d+)\s*\)', maincontent)

if not versionmatch:
    raise Exception("Could not find Version in Main.cs")

major, minor, patch = versionmatch.groups()
versionstr = f"{major}.{minor}.{patch}"
print(f"Found version: {versionstr}")

assemblycontent = ASSEMBLYFILE.read_text()
assemblycontent = re.sub(r'\[assembly:\s*AssemblyVersion\(".*?"\)\]', f'[assembly: AssemblyVersion("{versionstr}")]', assemblycontent)
assemblycontent = re.sub(r'\[assembly:\s*AssemblyFileVersion\(".*?"\)\]', f'[assembly: AssemblyFileVersion("{versionstr}")]', assemblycontent)
ASSEMBLYFILE.write_text(assemblycontent)
print("AssemblyInfo.cs updated successfully!")
