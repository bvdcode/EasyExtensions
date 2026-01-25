import re
import pandas as pd


def parse_test_results(filename):
    """Parse performance test results from a file"""

    with open(filename, 'r', encoding='utf-8') as f:
        content = f.read()

    # Find result sections
    encrypt_section = re.search(
        r'=== ENCRYPTION THREAD/CHUNK SWEEP ===(.*?)(?===|$)', content, re.DOTALL)
    decrypt_section = re.search(
        r'=== DECRYPTION THREAD/CHUNK SWEEP ===(.*?)(?===|$)', content, re.DOTALL)

    def extract_data(section_text):
        """Extract data from a text section"""
        if not section_text:
            return pd.DataFrame()

        # Ищем строки с данными (формат: число | число | число.число)
        pattern = r'(\d+)\s*\|\s*(\d+)\s*\|\s*([\d.]+)'
        matches = re.findall(pattern, section_text)

        data = []
        for match in matches:
            threads = int(match[0])
            chunk_mb = int(match[1])
            throughput = float(match[2])
            data.append({'Threads': threads, 'ChunkMB': chunk_mb,
                        'Throughput': throughput})

        return pd.DataFrame(data)

    encrypt_data = extract_data(
        encrypt_section.group(1) if encrypt_section else "")
    decrypt_data = extract_data(
        decrypt_section.group(1) if decrypt_section else "")

    return encrypt_data, decrypt_data
