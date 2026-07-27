import os
import hashlib
import struct
import logging

class AssetScanner:
    def __init__(self):
        self.assets = []
        self.duplicates = {}
        self.issues = []

    def scan(self, decode_root: str, res_root: str):
        if not os.path.exists(res_root):
            return []
            
        for root_dir, dirs, files in os.walk(res_root):
            if "drawable" not in root_dir and "mipmap" not in root_dir:
                continue
                
            for file in files:
                ext = os.path.splitext(file)[1].lower()
                if ext not in ['.png', '.jpg', '.jpeg', '.webp']:
                    continue
                    
                full_path = os.path.join(root_dir, file)
                rel_path = os.path.relpath(full_path, decode_root).replace('\\', '/')
                
                with open(full_path, 'rb') as f:
                    content = f.read()
                    fhash = hashlib.sha256(content).hexdigest()
                    
                width, height, fmt, status = self._read_dimensions(full_path, rel_path)
                
                confidence = "low"
                reasons = []
                
                if file.startswith("ic_") or file.startswith("abc_") or file.startswith("btn_"):
                    reasons.append("Framework prefix")
                else:
                    confidence = "high"
                    reasons.append("Game specific name pattern")
                    
                is_game_specific = (confidence == "high")
                    
                if fhash in self.duplicates:
                    dup_group = fhash
                else:
                    self.duplicates[fhash] = []
                    dup_group = None
                    
                self.duplicates[fhash].append(rel_path)
                
                self.assets.append({
                    "id": os.path.splitext(file)[0],
                    "filename": file,
                    "relativeSourcePath": rel_path,
                    "extension": ext,
                    "width": width,
                    "height": height,
                    "format": fmt,
                    "hash": fhash,
                    "category": "texture",
                    "sourceType": "Android",
                    "isGameSpecific": is_game_specific,
                    "classificationConfidence": confidence,
                    "classificationReasons": reasons,
                    "metadataReadStatus": status,
                    "duplicateHashGroup": dup_group
                })
                
        from src.config_loader import ConfigLoader
        try:
            config = ConfigLoader.load("config/production_profile.json")
            out_root = config.get("reportRoot", "output/production_reports")
        except:
            out_root = "output/production_reports"
        os.makedirs(out_root, exist_ok=True)
        with open(os.path.join(out_root, "asset_scan_issues.md"), "w", encoding="utf-8") as f:
            f.write("# Asset Scan Issues\n\n")
            if not self.issues:
                f.write("No issues detected.\n")
            else:
                for issue in self.issues:
                    f.write(f"- **{issue['path']}**: {issue['error']}\n")
                    
        return self.assets

    def _read_dimensions(self, path: str, rel_path: str):
        try:
            with open(path, 'rb') as f:
                head = f.read(32)
                if len(head) < 24:
                    return None, None, None, "FAILED_SHORT_FILE"
                
                # PNG
                if head.startswith(b'\x89PNG\r\n\x1a\n'):
                    if head[12:16] == b'IHDR':
                        width, height = struct.unpack('>II', head[16:24])
                        return width, height, "PNG", "SUCCESS"
                        
                # JPEG
                if head.startswith(b'\xff\xd8'):
                    f.seek(0)
                    size = 2
                    ftype = 0
                    while not 0xc0 <= ftype <= 0xcf or ftype in [0xc4, 0xc8, 0xcc]:
                        f.seek(size, 1)
                        byte = f.read(1)
                        if not byte: break
                        while byte and byte[0] == 0xff:
                            byte = f.read(1)
                        if not byte: break
                        ftype = byte[0]
                        try:
                            size = struct.unpack('>H', f.read(2))[0] - 2
                        except:
                            break
                    f.seek(1, 1)
                    try:
                        height, width = struct.unpack('>HH', f.read(4))
                        if width > 0 and height > 0:
                            return width, height, "JPEG", "SUCCESS"
                    except:
                        pass
                    
                # WEBP
                if head.startswith(b'RIFF') and head[8:12] == b'WEBP':
                    vp8_sig = head[12:16]
                    if vp8_sig == b'VP8X':
                        width = (head[24] | (head[25]<<8) | (head[26]<<16)) + 1
                        height = (head[27] | (head[28]<<8) | (head[29]<<16)) + 1
                        return width, height, "WEBP", "SUCCESS"
                    elif vp8_sig == b'VP8 ':
                        width, height = struct.unpack('<HH', head[26:30])
                        return width & 0x3fff, height & 0x3fff, "WEBP", "SUCCESS"
                    elif vp8_sig == b'VP8L':
                        b1 = head[21]
                        b2 = head[22]
                        b3 = head[23]
                        b4 = head[24]
                        width = 1 + (((b2 & 0x3F) << 8) | b1)
                        height = 1 + (((b4 & 0xF) << 10) | (b3 << 2) | ((b2 & 0xC0) >> 6))
                        return width, height, "WEBP", "SUCCESS"
                        
        except Exception as e:
            self.issues.append({"path": rel_path, "error": str(e)})
            logging.warning(f"Failed to read dimensions for {rel_path}: {e}")
            return None, None, None, "FAILED_EXCEPTION"
            
        self.issues.append({"path": rel_path, "error": "Unknown format or missing dimensions chunk"})
        return None, None, None, "FAILED_UNKNOWN"



