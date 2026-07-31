"""Create a Lark doc from markdown, then transfer ownership to the user.
Uses FEISHU_APP_ID / FEISHU_APP_SECRET from Hermes .env.
"""
import os, re, sys, time, json, urllib.parse
import requests

ENV_PATH = os.path.expanduser("~/AppData/Local/hermes/.env")

def load_env(path):
    env = {}
    with open(path, "r", encoding="utf-8") as f:
        for line in f:
            line = line.strip()
            if not line or line.startswith("#") or "=" not in line:
                continue
            k, _, v = line.partition("=")
            env[k.strip()] = v.strip()
    return env

env = load_env(ENV_PATH)
APP_ID = env.get("FEISHU_APP_ID", "")
APP_SECRET = env.get("FEISHU_APP_SECRET", "")
DOMAIN = "https://open.larksuite.com" if env.get("FEISHU_DOMAIN", "").lower() == "lark" else "https://open.feishu.cn"

if not APP_ID or not APP_SECRET:
    print("ERROR: FEISHU_APP_ID/FEISHU_APP_SECRET missing in .env")
    sys.exit(1)

MD_PATH = sys.argv[1] if len(sys.argv) > 1 else ""
TITLE = sys.argv[2] if len(sys.argv) > 2 else "Guild Master Logic Doc"
CHAT_ID = sys.argv[3] if len(sys.argv) > 3 else "oc_a5772233e2a5c6c71079e18b1174219e"
USER_HINT = sys.argv[4] if len(sys.argv) > 4 else "g8e4b999"

s = requests.Session()

# 1. Tenant access token
r = s.post(f"{DOMAIN}/open-apis/auth/v3/tenant_access_token/internal", json={
    "app_id": APP_ID, "app_secret": APP_SECRET
}, timeout=30)
tok = r.json()
if tok.get("code") != 0:
    print("TOKEN ERROR:", json.dumps(tok, ensure_ascii=False)[:500]); sys.exit(1)
TENANT_TOKEN = tok["tenant_access_token"]
H = {"Authorization": f"Bearer {TENANT_TOKEN}", "Content-Type": "application/json; charset=utf-8"}
print("STEP1 OK: got tenant_access_token")

# 2. Find the user's open_id from chat members
try:
    r = s.get(f"{DOMAIN}/open-apis/im/v1/chats/{CHAT_ID}/members?page_size=50", headers=H, timeout=30)
    m = r.json()
    items = m.get("data", {}).get("items", []) if m.get("code") == 0 else []
    user_openid = None
    for it in items:
        if it.get("member_id_type") == "open_id" and (USER_HINT in (it.get("name") or "") or it.get("member_id")):
            user_openid = it["member_id"]
            break
    if not user_openid:
        # fallback: first member_id that looks like open_id (ou_...)
        for it in items:
            mid = it.get("member_id", "")
            if mid.startswith("ou_"):
                user_openid = mid
                break
    print(f"STEP2 OK: chat members={len(items)}, user_openid={user_openid}")
except Exception as e:
    print("STEP2 WARN:", e); user_openid = None

# 3. Upload markdown file as media
files = {
    "file": (os.path.basename(MD_PATH), open(MD_PATH, "rb"), "text/markdown"),
    "file_name": (None, os.path.basename(MD_PATH)),
    "parent_type": (None, "ccm_import_open"),
    "parent_node": (None, ""),
    "size": (None, str(os.path.getsize(MD_PATH))),
    "checksum": (None, ""),
    "extra": (None, ""),
}
r = s.post(f"{DOMAIN}/open-apis/drive/v1/medias/upload_all", headers={
    "Authorization": f"Bearer {TENANT_TOKEN}"
}, files=files, timeout=120)
up = r.json()
if up.get("code") != 0:
    print("UPLOAD ERROR:", json.dumps(up, ensure_ascii=False)[:600]); sys.exit(1)
file_token = up["data"]["file_token"]
print("STEP3 OK: uploaded media, file_token=", file_token)

# 4. Create import task (md -> docx)
r = s.post(f"{DOMAIN}/open-apis/drive/v1/import_tasks", headers=H, json={
    "file_extension": "md",
    "file_token": file_token,
    "type": "docx",
    "file_name": TITLE,
    "point": {"mount_type": 1, "mount_key": ""}
}, timeout=60)
imp = r.json()
if imp.get("code") != 0:
    print("IMPORT ERROR:", json.dumps(imp, ensure_ascii=False)[:600]); sys.exit(1)
ticket = imp["data"]["ticket"]
print("STEP4 OK: import ticket=", ticket)

# 5. Poll import task
doc_token = None
for i in range(30):
    time.sleep(2)
    r = s.get(f"{DOMAIN}/open-apis/drive/v1/import_tasks/{ticket}", headers=H, timeout=30)
    res = r.json()
    if res.get("code") == 0:
        result = res.get("data", {}).get("result", {})
        if result.get("job_status") == 0:
            doc_token = result.get("token")
            break
        elif result.get("job_status") == 2:
            print("IMPORT FAILED:", json.dumps(result, ensure_ascii=False)[:400]); sys.exit(1)
if not doc_token:
    print("IMPORT TIMEOUT"); sys.exit(1)
print("STEP5 OK: doc_token=", doc_token)

# 6. Transfer ownership to user
if user_openid:
    try:
        r = s.patch(f"{DOMAIN}/open-apis/drive/v1/permissions/{doc_token}/members/transfer_owner",
                    headers=H, json={"member_type": "openid", "member_id": user_openid}, timeout=30)
        tr = r.json()
        if tr.get("code") == 0:
            print("STEP6 OK: ownership transferred to", user_openid)
        else:
            print("STEP6 WARN:", json.dumps(tr, ensure_ascii=False)[:400])
    except Exception as e:
        print("STEP6 EXC:", e)

# 7. Also add the user as owner member (fallback / guarantee)
if user_openid:
    try:
        r = s.post(f"{DOMAIN}/open-apis/drive/v1/permissions/{doc_token}/members",
                   headers=H, json={"member_type": "openid", "member_id": user_openid, "perm": "full_access"}, timeout=30)
        pm = r.json()
        print("STEP7 member-add:", "OK" if pm.get("code") == 0 else json.dumps(pm, ensure_ascii=False)[:300])
    except Exception as e:
        print("STEP7 EXC:", e)

url = f"{DOMAIN.replace('https://open.', 'https://')}/docx/{doc_token}"
print("DOC_URL=", url)
print("DOC_TOKEN=", doc_token)
