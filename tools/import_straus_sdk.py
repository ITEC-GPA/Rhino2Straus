"""Import the installed R31 reference SDK locally; vendor documents/binaries stay ignored."""
from pathlib import Path
import argparse, hashlib, json, re, shutil

root = Path(__file__).resolve().parents[1]
parser = argparse.ArgumentParser()
parser.add_argument('--installation', type=Path, default=Path('C:/Program Files/Straus7 R31'))
args = parser.parse_args()
files = ['API Includes/Visual C#/St7API.cs', 'API Includes/Grasshopper/St7API_DotNet.dll',
         'API Includes/Readme.txt', 'Documentation/Straus7 R3 API Manual.pdf',
         'Documentation/Straus7 R24 to R3 API Conversion Guide.pdf']
destination = root / '.work/straus-sdk'
manifest = []
for relative in files:
    source = args.installation / relative
    if not source.is_file():
        raise FileNotFoundError(source)
    target = destination / relative
    target.parent.mkdir(parents=True, exist_ok=True)
    shutil.copy2(source, target)
    manifest.append(dict(path=relative, bytes=source.stat().st_size,
                         sha256=hashlib.sha256(source.read_bytes()).hexdigest()))

def declarations(path):
    source = path.read_text(encoding='utf-8-sig')
    return {re.sub(r'\s+', ' ', match).strip() for match in
            re.findall(r'public\s+(?:const\s+[^;]+|static\s+extern\s+[^;]+);', source)}

vendor = declarations(args.installation / files[0])
local = declarations(root / 'Rhino2Straus.Api/St7API.cs')
report = dict(edition='Straus7 R31', files=manifest, vendor_declarations=len(vendor),
              missing_or_changed=sorted(vendor-local), extra=sorted(local-vendor))
(root / 'docs/straus-sdk-reference.json').write_text(json.dumps(report, indent=2)+'\n', encoding='utf-8')
print(f'Imported {len(files)} reference files into {destination}')
print(f'{len(vendor)} vendor declarations; {len(vendor-local)} missing/changed; {len(local-vendor)} extra')
if vendor-local:
    raise SystemExit('Vendor binding audit failed; inspect docs/straus-sdk-reference.json')
