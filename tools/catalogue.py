"""Generate the catalogue and topic guide from the same topics used by the Grasshopper ribbon."""
from pathlib import Path
import argparse
import json
import re

parser = argparse.ArgumentParser(description=__doc__)
parser.add_argument('--check', action='store_true', help='Validate tracked catalogues without rewriting files')
args = parser.parse_args()
stale = []
root = Path(__file__).resolve().parents[1]
topic_source = (root / 'Rhino2Straus.Grasshopper.Types/UI/ComponentTopics.cs').read_text(encoding='utf-8')
labels = dict(re.findall(r'public const string (\w+)="([^"]+)";', topic_source))
topics = [dict(Key=key, Label=labels[key], Description=description, IsResults=flag == 'true')
          for key, description, flag in re.findall(
              r'new\(nameof\((\w+)\),\w+,"([^"]+)"(?:,(true|false))?\)', topic_source)]
assert topics and len(topics) == len(labels) - 1
by_topic = {t['Key']: t for t in topics}
items, guids = [], set()
registration = re.compile(r':\s*base\("([^"]+)"\s*,\s*"[^"]+"\s*,\s*"[^"]+"\s*,\s*ComponentTopics.Tab\s*,\s*ComponentTopics.(\w+)\s*\)')
for path in (root / 'Rhino2Straus.Grasshopper/Components').rglob('*.cs'):
    source = path.read_text(encoding='utf-8')
    match = registration.search(source)
    if not match:
        raise RuntimeError('Cannot read component registration: ' + str(path))
    name, topic = match.groups()
    if topic not in by_topic or path.parent.name != topic:
        raise RuntimeError('Ribbon topic and source folder disagree: ' + str(path))
    guid = re.search(r'ComponentGuid\s*=>\s*new(?: Guid)?\("([^"]+)"', source)[1]
    if guid in guids:
        raise RuntimeError('Duplicate component GUID: ' + guid)
    guids.add(guid)
    items.append(dict(Class=path.stem, Name=name, Category=labels[topic], Topic=topic,
                      Guid=guid, File=path.relative_to(root).as_posix()))
items.sort(key=lambda item: (item['Category'], item['Name']))
docs = root / 'docs'
if not args.check:
    docs.mkdir(exist_ok=True)

def write(name, content):
    path = docs / name
    if args.check:
        if not path.exists() or path.read_text(encoding='utf-8') != content:
            stale.append(name)
    else:
        path.write_bytes(content.encode('utf-8'))

write('components.json', json.dumps(items, ensure_ascii=False, indent=2) + '\n')
md = ['# Catalogo componenti',
      f'\n{len(items)} componenti Grasshopper in **{len(topics)} pannelli per argomento**, tutti nella tab **Rhino2Straus** e tutti con icona. Il parametro persistente Group e la libreria hanno icone aggiuntive.',
      '\n[Organizzazione della tab e mappa degli argomenti](TOPICS.md).',
      '\nIl percorso resta elementi/attributi → Build Model o Merge Straus Models → un solo Model → Preview/Run → Model con risultati → estrazione/deformata/dati per il verificatore.']
guide = ['# Argomenti nella tab Rhino2Straus',
         '\nI pannelli della tab Grasshopper sono ordinati per fase di lavoro. I risultati sono suddivisi per tipo di elemento. Le cartelle sorgenti e il catalogo seguono gli stessi argomenti.',
         '\n| Pannello Grasshopper | Contenuto | Componenti |', '|---|---|---:|']
for topic in topics:
    members = [item for item in items if item['Topic'] == topic['Key']]
    if not members:
        raise RuntimeError('Empty topic: ' + topic['Key'])
    md += [f"\n## {topic['Label']}", f"\n{topic['Description']} **{len(members)} componenti.**",
           '\n| Componente |', '|---|']
    md += [f"| [{item['Name']}](../{item['File']}) |" for item in members]
    if topic['Key'] == 'Model':
        md += ['\nÈ disponibile anche il parametro persistente [Group](../Rhino2Straus.Grasshopper/Parameter/GroupParam.cs), nello stesso pannello.']
    folder = '../Rhino2Straus.Grasshopper/Components/' + topic['Key']
    guide += [f"| [{topic['Label']}]({folder}) | {topic['Description']} | {len(members)} |"]
md += ['\n![Icone per argomento](icons.png)']
guide += [
    '\nIl parametro **Group** si trova in **09-Modello** accanto ai componenti dei gruppi. Le sezioni BGL/BXS e le opzioni native si trovano in **02-Sezioni**; le loro eventuali dipendenze API rimangono quelle documentate nella guida alle sezioni.',
    '\nIl pannello **17-Preview e bake** contiene Display Manager, Display Settings e Model Physical Geometry. Il controllo `Results` del gestore continua ad agire su tutti e cinque i pannelli dei risultati.',
    '\nPer vedere la nuova disposizione, sostituire il pacchetto del plugin e riavviare Rhino/Grasshopper. Nomi, GUID, input e output dei componenti sono conservati; le definizioni Grasshopper esistenti continuano a usare gli stessi componenti e collegamenti.',
    '\nLe cartelle sono state riordinate, conservando i namespace esistenti per compatibilità. Le etichette, l’ordine e la classificazione dei risultati sono definiti in [ComponentTopics.cs](../Rhino2Straus.Grasshopper.Types/UI/ComponentTopics.cs). `tools/catalogue.py` verifica che ciascuna cartella corrisponda al pannello dichiarato e genera questa guida e il [catalogo completo](COMPONENTS.md).',
    '\n![Icone per argomento](icons.png)']
write('COMPONENTS.md', '\n'.join(md) + '\n')
write('TOPICS.md', '\n'.join(guide) + '\n')
print(f'{len(items)} components; {len(guids)} unique GUIDs; {len(topics)} matching ribbon topics and source folders')

if stale:
    raise SystemExit("Stale catalogues: " + ", ".join(stale) + ". Run tools/catalogue.py.")
