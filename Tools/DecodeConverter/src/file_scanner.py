import os

class FileScanner:
    @staticmethod
    def scan_java_files(root_dir: str) -> list:
        if os.path.isfile(root_dir) and root_dir.endswith('.java'):
            return [root_dir]
            
        java_files = []
        if not os.path.exists(root_dir):
            return java_files
            
        for root, dirs, files in os.walk(root_dir):
            for file in files:
                if file.endswith('.java'):
                    java_files.append(os.path.join(root, file))
        return java_files

    @staticmethod
    def scan_xml_files(root_dir: str) -> list:
        xml_files = []
        if not os.path.exists(root_dir):
            return xml_files
            
        for root, dirs, files in os.walk(root_dir):
            for file in files:
                if file.endswith('.xml'):
                    xml_files.append(os.path.join(root, file))
        return xml_files
