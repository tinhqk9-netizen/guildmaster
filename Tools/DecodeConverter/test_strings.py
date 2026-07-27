import os
import json
import sys
sys.path.append(r'D:\\Tinh\\Rebuild_GuildMaster\\Tools\\DecodeConverter')
from src.strings_parser import StringsParser

xml_path = r'D:\Tinh\Guild Master - Idle Dungeons\resources\res\values\strings.xml'
parser = StringsParser()
try:
    strings = parser.parse(xml_path)
    print(f"Total parsed: {len(strings)}")
    c_str = sum(1 for v in strings.values() if v['sourceType'] == 'string')
    c_plu = sum(1 for v in strings.values() if v['sourceType'] == 'plural')
    c_arr = sum(1 for v in strings.values() if v['sourceType'] == 'array')
    print(f"Strings: {c_str}")
    print(f"Plurals: {c_plu}")
    print(f"Arrays: {c_arr}")
except Exception as e:
    import traceback
    print(f"Error: {e}")
    traceback.print_exc()
