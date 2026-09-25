"""Regenerate the component catalogue from registered components (no Rhino runtime required)."""
from pathlib import Path
import json,re
root=Path(__file__).resolve().parents[1]
constants=dict(re.findall(r'static string (\w+) = "([^"]+)"',(root/'Rhino2Straus.Core/Helper/Constants.cs').read_text(encoding='utf-8')))
items=[];guids=set()
for p in (root/'Rhino2Straus.Grasshopper/Components').rglob('*.cs'):
    s=p.read_text(encoding='utf-8');m=re.search(r':\s*base\("([^"]+)"\s*,\s*"[^"]+"\s*,\s*"[^"]+"\s*,\s*(?:"[^"]+"|[\w.]+)\s*,\s*("[^"]+"|[\w.]+)\s*\)',s)
    if not m: raise RuntimeError('Cannot read component registration: '+str(p))
    category=m[2].strip('"') if m[2].startswith('"') else constants[m[2].split('.')[-1]]
    guid=re.search(r'ComponentGuid\s*=>\s*new(?: Guid)?\("([^"]+)"',s)[1]
    if guid in guids:raise RuntimeError('Duplicate component GUID: '+guid)
    guids.add(guid)
    items.append(dict(Class=p.stem,Name=m[1],Category=category,File=p.relative_to(root).as_posix()))
items.sort(key=lambda i:(i['Category'],i['Name']))
(root/'docs').mkdir(exist_ok=True)
(root/'docs/components.json').write_text(json.dumps(items,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
md=['# Catalogo componenti',f'\n{len(items)} componenti Grasshopper, tutti con icona. Il parametro persistente Group e la libreria hanno icone aggiuntive.','\nIl percorso è sempre elementi/attributi → Build Model o Merge Straus Models → un solo Model → Preview/Run → Model con risultati → estrazione/deformata/dati per il verificatore.','\n| Categoria | Componente |','|---|---|']
for i in items:md.append(f"| {i['Category']} | [{i['Name']}](../{i['File']}) |")
md+=['\n![Icone](icons.png)']
(root/'docs/COMPONENTS.md').write_text('\n'.join(md)+'\n',encoding='utf-8')
print(f'{len(items)} components; {len(guids)} unique GUIDs')
