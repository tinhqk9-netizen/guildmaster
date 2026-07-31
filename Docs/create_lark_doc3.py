"""Create Lark docx, fill with markdown content, transfer ownership to user. v2 (fixed block types)."""
import os, re, json, sys, time
import requests

ENV_PATH = os.path.expanduser("~/AppData/Local/hermes/.env")
DOMAIN = "https://open.larksuite.com"
USER_OPENID = "ou_fc123bafe77166f095cd55d08c4e65b7"

def load_env(path):
    env = {}
    with open(path, "r", encoding="utf-8") as f:
        for line in f:
            line = line.strip()
            if line and not line.startswith("#") and "=" in line:
                k, _, v = line.partition("=")
                env[k.strip()] = v.strip()
    return env

env = load_env(ENV_PATH)
MD_PATH = sys.argv[1] if len(sys.argv) > 1 else ""
TITLE = sys.argv[2] if len(sys.argv) > 2 else "Guild Master Doc"

r = requests.post(f"{DOMAIN}/open-apis/auth/v3/tenant_access_token/internal",
                  json={"app_id": env["FEISHU_APP_ID"], "app_secret": env["FEISHU_APP_SECRET"]}, timeout=30)
tok = r.json()
assert tok.get("code") == 0, tok
H = {"Authorization": f"Bearer {tok['tenant_access_token']}", "Content-Type": "application/json; charset=utf-8"}
print("TOKEN OK")

# 1. Create document
r = requests.post(f"{DOMAIN}/open-apis/docx/v1/documents", headers=H,
                  json={"title": TITLE}, timeout=30)
d = r.json()
assert d.get("code") == 0, d
doc_id = d["data"]["document"]["document_id"]
print("DOC CREATED:", doc_id)

root = doc_id  # document block id == document id

def make_text_elements(text):
    """Split **bold** segments into text_run elements."""
    elements = []
    for seg in re.split(r"(\*\*.*?\*\*)", text):
        if not seg:
            continue
        if seg.startswith("**") and seg.endswith("**"):
            elements.append({"text_run": {"content": seg[2:-2], "text_element_style": {"bold": True}}})
        else:
            elements.append({"text_run": {"content": seg}})
    return elements

def md_to_blocks(md_text):
    blocks = []
    lines = md_text.split("\n")
    i = 0
    n = len(lines)
    while i < n:
        line = lines[i].rstrip()
        stripped = line.strip()

        if stripped == "---" or (stripped.startswith("- [") and "](#" in stripped):
            i += 1
            continue

        m = re.match(r"^(#{1,6})\s+(.*)$", stripped)
        if m:
            level = len(m.group(1))
            text = m.group(2).strip()
            if level == 1:
                blocks.append({"block_type": 3, "heading1": {"elements": make_text_elements(text)}})
            elif level == 2:
                blocks.append({"block_type": 4, "heading2": {"elements": make_text_elements(text)}})
            else:
                blocks.append({"block_type": 5, "heading3": {"elements": make_text_elements(text)}})
            i += 1
            continue

        # table -> render as text lines
        if stripped.startswith("|") and i + 1 < n and re.match(r"^\|[\s:\-|]+\|$", lines[i + 1].strip()):
            header_cells = [c.strip() for c in stripped.split("|")[1:-1]]
            i += 2
            rows = []
            while i < n and lines[i].strip().startswith("|"):
                rows.append([c.strip() for c in lines[i].strip().split("|")[1:-1]])
                i += 1
            hdr = " | ".join(header_cells)
            blocks.append({"block_type": 4, "heading2": {"elements": [{"text_run": {"content": hdr}}]}})
            for row in rows:
                row_text = " | ".join(row)
                blocks.append({"block_type": 2, "text": {"elements": [{"text_run": {"content": row_text}}]}})
            continue

        if re.match(r"^[-*]\s+", stripped):
            text = re.sub(r"^[-*]\s+", "", stripped)
            blocks.append({"block_type": 12, "bullet": {"elements": make_text_elements(text)}})
            i += 1
            continue

        m = re.match(r"^(\d+)\.\s+(.*)$", stripped)
        if m:
            text = m.group(2)
            blocks.append({"block_type": 13, "ordered": {"elements": make_text_elements(text)}})
            i += 1
            continue

        if stripped.startswith(">"):
            text = stripped.lstrip("> ").strip()
            blocks.append({"block_type": 15, "quote": {"elements": [{"text_run": {"content": text, "text_element_style": {"italic": True}}}]}})
            i += 1
            continue

        if stripped.startswith("```"):
            i += 1
            code_lines = []
            while i < n and not lines[i].strip().startswith("```"):
                code_lines.append(lines[i])
                i += 1
            i += 1
            code_text = "\n".join(code_lines)
            blocks.append({"block_type": 14, "code": {"elements": [{"text_run": {"content": code_text}}]}})
            continue

        if stripped == "":
            i += 1
            continue

        blocks.append({"block_type": 2, "text": {"elements": make_text_elements(stripped)}})
        i += 1

    return blocks

with open(MD_PATH, "r", encoding="utf-8") as f:
    md = f.read()

blocks = md_to_blocks(md)
print("PARSED BLOCKS:", len(blocks))

# 4. Create children in chunks of 40
total_created = 0
for start in range(0, len(blocks), 40):
    chunk = blocks[start:start + 40]
    r = requests.post(f"{DOMAIN}/open-apis/docx/v1/documents/{doc_id}/blocks/{root}/children",
                      headers=H, json={"children": chunk}, timeout=60)
    res = r.json()
    if res.get("code") != 0:
        print("CHUNK FAILED at", start, json.dumps(res, ensure_ascii=False)[:400])
        sys.exit(1)
    total_created += len(chunk)
    print(f"  chunk {start}-{start+len(chunk)} OK")

print("TOTAL BLOCKS CREATED:", total_created)

# 5. Transfer ownership to user
r = requests.patch(f"{DOMAIN}/open-apis/drive/v1/permissions/{doc_id}/members/transfer_owner",
                   headers=H, json={"member_type": "openid", "member_id": USER_OPENID}, timeout=30)
tr = r.json()
print("TRANSFER OWNER:", tr.get("code"), tr.get("msg") or json.dumps(tr, ensure_ascii=False)[:200])

# 6. Fallback: add user as full_access member
r = requests.post(f"{DOMAIN}/open-apis/drive/v1/permissions/{doc_id}/members",
                  headers=H, json={"member_type": "openid", "member_id": USER_OPENID, "perm": "full_access"}, timeout=30)
pm = r.json()
print("ADD MEMBER:", pm.get("code"), pm.get("msg") or json.dumps(pm, ensure_ascii=False)[:200])

# 7. Try public link (may fail without scope; best-effort)
r = requests.patch(f"{DOMAIN}/open-apis/drive/v1/permissions/{doc_id}/public",
                   headers=H, json={"link_share_entity": "anyone_readable"}, timeout=30)
pub = r.json()
print("PUBLIC LINK:", pub.get("code"), pub.get("msg") or json.dumps(pub, ensure_ascii=False)[:200])

print("DOC_URL: https://bsg3lq2gys.feishu.cn/docx/" + doc_id)
print("DOC_ID:", doc_id)
