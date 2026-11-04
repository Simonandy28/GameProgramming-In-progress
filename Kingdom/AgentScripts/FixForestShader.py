from pathlib import Path
import json
import re

path = Path('Assets/Idyllic Fantasy Nature/Shader/Vegetation.shadergraph')
source = path.read_text()
changed = 0
def fix(match):
    global changed
    data = json.loads(match[0])
    if data.get('m_Type') == 'UnityEditor.ShaderGraph.ShaderKeyword' and data.get('m_DefaultReferenceName') in {'_MAIN_LIGHT_SHADOWS', '_MAIN_LIGHT_SHADOWS_CASCADE'}:
        changed += 1
        return match[0].replace('"m_KeywordDefinition": 0', '"m_KeywordDefinition": 2')
    return match[0]
result = re.sub(r'(?ms)^\{.*?^\}', fix, source)
assert changed == 2, changed
path.write_text(result)
print('Shadow keywords now use URP predefined variants:', changed)
