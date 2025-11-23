import re
import sys
from pathlib import Path
from typing import Tuple, Optional

import matplotlib.pyplot as plt
import numpy as np
import pandas as pd
try:
    import seaborn as sns  # optional, best effort
except Exception:  # pragma: no cover
    sns = None
import matplotlib.gridspec as gridspec


ROOT = Path(__file__).parent.resolve()
MYLIB_INPUT_DEFAULT = ROOT / "input.txt"
OPENSSL_INPUT_DEFAULT = ROOT / "input-openssl.txt"


def parse_mylib_results(filename: Path) -> Tuple[pd.DataFrame, pd.DataFrame]:
    """Parse my library performance test results from input.txt.

    Expects two sections with headers:
      === ENCRYPTION THREAD/CHUNK SWEEP ===
      === DECRYPTION THREAD/CHUNK SWEEP ===

    And table lines: Threads | ChunkMB | Avg MB/s
    Returns two DataFrames with columns: Threads, ChunkMB, Throughput
    """

    text = filename.read_text(encoding="utf-8", errors="ignore")

    enc_sec = re.search(r"===\s*ENCRYPTION THREAD/CHUNK SWEEP\s*===(.*?)(?===|\Z)", text, re.DOTALL)
    dec_sec = re.search(r"===\s*DECRYPTION THREAD/CHUNK SWEEP\s*===(.*?)(?===|\Z)", text, re.DOTALL)

    def extract(section: Optional[re.Match]) -> pd.DataFrame:
        if not section:
            return pd.DataFrame(columns=["Threads", "ChunkMB", "Throughput"])
        body = section.group(1)
        # pattern matches: threads | chunk (can be decimal) | throughput (decimal)
        pat = re.compile(r"(\d+)\s*\|\s*([\d.]+)\s*\|\s*([\d.]+)")
        rows = []
        for th, ch, thr in pat.findall(body):
            rows.append({
                "Threads": int(th),
                "ChunkMB": float(ch),
                "Throughput": float(thr),
            })
        return pd.DataFrame(rows)

    enc_df = extract(enc_sec)
    dec_df = extract(dec_sec)
    return enc_df, dec_df


def parse_openssl_results(filename: Path) -> pd.DataFrame:
    """Parse OpenSSL 'speed -evp aes-128-gcm' output.

    We parse the summary table:
      The 'numbers' are in 1000s of bytes per second processed.
      header: type  16 bytes 64 bytes ...
      row:    AES-128-GCM  103872.58k ...

    Returns DataFrame with columns: BlockBytes, ThroughputMBps, Label
    ThroughputMBps is decimal MB/s (1 MB = 1,000,000 bytes).
    """

    text = filename.read_text(encoding="utf-8", errors="ignore")
    # Find header line with sizes and the AES-128-GCM row
    header_match = re.search(r"^type\s+((?:\d+\s+bytes\s+)+)\s*$", text, re.MULTILINE)
    row_match = re.search(r"^AES-128-GCM\s+(.+?)\s*$", text, re.MULTILINE)

    if not header_match or not row_match:
        # Fallback: try to gather from 'Doing ... on X size blocks' lines
        # Doing AES-128-GCM ops for 3s on 16 size blocks: 19070356 AES-128-GCM ops in 2.94s
        pat = re.compile(r"on\s+(\d+)\s+size blocks:.*? in\s+([\d.]+)s", re.IGNORECASE)
        sizes = []
        times = []
        for m in pat.finditer(text):
            sizes.append(int(m.group(1)))
            times.append(float(m.group(2)))
        # Also find the count of ops
        pat2 = re.compile(r"on\s+(\d+)\s+size blocks:\s*(\d+)\s+AES-128-GCM ops in\s+([\d.]+)s", re.IGNORECASE)
        data = []
        for m in pat2.finditer(text):
            block = int(m.group(1))
            ops = int(m.group(2))
            secs = float(m.group(3))
            bytes_per_sec = ops * block / secs
            mbps = bytes_per_sec / 1_000_000.0
            data.append({"BlockBytes": block, "ThroughputMBps": mbps, "Label": "OpenSSL AES-128-GCM"})
        return pd.DataFrame(sorted(data, key=lambda r: r["BlockBytes"]))

    # Parse sizes from header
    sizes_str = header_match.group(1)
    size_vals = [int(x) for x in re.findall(r"(\d+)\s+bytes", sizes_str)]
    # Parse k-values from row (thousands of bytes per second). Split by whitespace ending with 'k'
    row_vals = [float(v) for v in re.findall(r"([\d.]+)k", row_match.group(1))]
    # Defensive: align by min length
    n = min(len(size_vals), len(row_vals))
    size_vals = size_vals[:n]
    row_vals = row_vals[:n]

    # Convert k (thousands of bytes/s) to MB/s (decimal)
    mbps = [v / 1000.0 for v in row_vals]
    df = pd.DataFrame({
        "BlockBytes": size_vals,
        "ThroughputMBps": mbps,
        "Label": ["OpenSSL AES-128-GCM"] * n,
    })
    return df.sort_values("BlockBytes").reset_index(drop=True)


def plot_mylib_four_panels(enc: pd.DataFrame, dec: pd.DataFrame, out_path: Path) -> None:
    """Create 4-panel figure for my library: throughput vs chunk size (per threads) and vs threads (per chunk)."""
    if enc.empty or dec.empty:
        print("[warn] CottonCrypto data is empty; skipping library_performance.png")
        return

    plt.rcParams["figure.facecolor"] = "white"
    plt.rcParams["axes.facecolor"] = "white"
    plt.rcParams["axes.grid"] = True
    plt.rcParams["grid.alpha"] = 0.3

    fig, axes = plt.subplots(2, 2, figsize=(16, 12))
    (ax1, ax2), (ax3, ax4) = axes
    fig.suptitle("CottonCrypto Performance: Encryption/Decryption Throughput", fontsize=16, fontweight="bold", y=0.98)

    thread_colors = ["#1f77b4", "#ff7f0e", "#2ca02c", "#d62728", "#9467bd", "#8c564b"]
    chunk_colors = ["#e41a1c", "#377eb8", "#4daf4a", "#984ea3", "#ff7f00", "#a65628", "#f781bf"]

    unique_threads = sorted(enc["Threads"].unique())
    unique_chunks = sorted(enc["ChunkMB"].unique())

    # 1) Encrypt: throughput vs chunk size per thread
    for i, t in enumerate(unique_threads):
        d = enc[enc["Threads"] == t].sort_values("ChunkMB")
        ax1.plot(d["ChunkMB"], d["Throughput"], marker="o", label=f"{t} threads",
                 linewidth=2.0, markersize=7, color=thread_colors[i % len(thread_colors)])
    ax1.set_title("Encryption: Throughput vs Chunk Size", fontsize=14, fontweight="bold")
    ax1.set_xlabel("Chunk Size (MB)")
    ax1.set_ylabel("Throughput (MB/s)")
    ax1.set_xticks(unique_chunks)
    ax1.legend(frameon=True, fancybox=True)

    # 2) Decrypt: throughput vs chunk size per thread
    for i, t in enumerate(unique_threads):
        d = dec[dec["Threads"] == t].sort_values("ChunkMB")
        ax2.plot(d["ChunkMB"], d["Throughput"], marker="s", label=f"{t} threads",
                 linewidth=2.0, markersize=7, color=thread_colors[i % len(thread_colors)])
    ax2.set_title("Decryption: Throughput vs Chunk Size", fontsize=14, fontweight="bold")
    ax2.set_xlabel("Chunk Size (MB)")
    ax2.set_ylabel("Throughput (MB/s)")
    ax2.set_xticks(unique_chunks)
    ax2.legend(frameon=True, fancybox=True)

    # 3) Encrypt: throughput vs threads per chunk
    for i, ch in enumerate(unique_chunks):
        d = enc[enc["ChunkMB"] == ch].sort_values("Threads")
        ax3.plot(d["Threads"], d["Throughput"], marker="o", label=f"{int(ch)}MB",
                 linewidth=2.0, markersize=7, color=chunk_colors[i % len(chunk_colors)])
    ax3.set_title("Encryption: Throughput vs Threads", fontsize=14, fontweight="bold")
    ax3.set_xlabel("Number of Threads")
    ax3.set_ylabel("Throughput (MB/s)")
    ax3.set_xticks(unique_threads)
    ax3.legend(title="Chunk Size", frameon=True, fancybox=True)

    # 4) Decrypt: throughput vs threads per chunk
    for i, ch in enumerate(unique_chunks):
        d = dec[dec["ChunkMB"] == ch].sort_values("Threads")
        ax4.plot(d["Threads"], d["Throughput"], marker="s", label=f"{int(ch)}MB",
                 linewidth=2.0, markersize=7, color=chunk_colors[i % len(chunk_colors)])
    ax4.set_title("Decryption: Throughput vs Threads", fontsize=14, fontweight="bold")
    ax4.set_xlabel("Number of Threads")
    ax4.set_ylabel("Throughput (MB/s)")
    ax4.set_xticks(unique_threads)
    ax4.legend(title="Chunk Size", frameon=True, fancybox=True)

    for ax in (ax1, ax2, ax3, ax4):
        ax.grid(True, alpha=0.3)
        ax.spines["top"].set_visible(False)
        ax.spines["right"].set_visible(False)

    fig.tight_layout()
    fig.savefig(out_path, dpi=300, bbox_inches="tight")
    print(f"[ok] Saved {out_path.name}")


def create_advanced_plots(encrypt_data: pd.DataFrame, decrypt_data: pd.DataFrame, out_path: Path) -> None:
    """Replicate the advanced analysis (6 plots) in a single figure and save it."""
    if encrypt_data.empty or decrypt_data.empty:
        print("[warn] CottonCrypto data empty; skipping advanced_performance_analysis.png")
        return

    plt.style.use('seaborn-v0_8')

    fig = plt.figure(figsize=(20, 16))
    gs = fig.add_gridspec(3, 2, hspace=0.3, wspace=0.25)

    ax1 = fig.add_subplot(gs[0, 0])
    ax2 = fig.add_subplot(gs[0, 1])
    ax3 = fig.add_subplot(gs[1, 0])
    ax4 = fig.add_subplot(gs[1, 1])
    ax5 = fig.add_subplot(gs[2, 0])
    ax6 = fig.add_subplot(gs[2, 1])

    fig.suptitle('Complete Performance Analysis: Encryption/Decryption Throughput',
                 fontsize=18, fontweight='bold')

    unique_threads = sorted(encrypt_data['Threads'].unique())
    unique_chunks = sorted(encrypt_data['ChunkMB'].unique())
    colors = plt.cm.Set1(np.linspace(0, 1, len(unique_threads)))
    chunk_colors = plt.cm.tab10(np.linspace(0, 1, len(unique_chunks)))

    # 1) Encrypt: throughput vs chunk size
    for i, threads in enumerate(unique_threads):
        thread_data = encrypt_data[encrypt_data['Threads'] == threads].sort_values('ChunkMB')
        ax1.plot(thread_data['ChunkMB'], thread_data['Throughput'], marker='o', label=f'{threads} threads',
                 linewidth=2.5, markersize=8, color=colors[i])
    ax1.set_xlabel('Chunk Size (MB)')
    ax1.set_ylabel('Throughput (MB/s)')
    ax1.set_title('Encryption: Throughput vs Chunk Size')
    ax1.legend(bbox_to_anchor=(1.05, 1), loc='upper left')
    ax1.grid(True, alpha=0.3)
    ax1.set_xticks(unique_chunks)

    # 2) Decrypt: throughput vs chunk size
    for i, threads in enumerate(unique_threads):
        thread_data = decrypt_data[decrypt_data['Threads'] == threads].sort_values('ChunkMB')
        ax2.plot(thread_data['ChunkMB'], thread_data['Throughput'], marker='s', label=f'{threads} threads',
                 linewidth=2.5, markersize=8, color=colors[i])
    ax2.set_xlabel('Chunk Size (MB)')
    ax2.set_ylabel('Throughput (MB/s)')
    ax2.set_title('Decryption: Throughput vs Chunk Size')
    ax2.legend(bbox_to_anchor=(1.05, 1), loc='upper left')
    ax2.grid(True, alpha=0.3)
    ax2.set_xticks(unique_chunks)

    # 3) Encrypt: throughput vs threads
    for i, chunk_size in enumerate(unique_chunks):
        chunk_data = encrypt_data[encrypt_data['ChunkMB'] == chunk_size].sort_values('Threads')
        ax3.plot(chunk_data['Threads'], chunk_data['Throughput'], marker='o', label=f'{chunk_size:g}MB',
                 linewidth=2.5, markersize=8, color=chunk_colors[i])
    ax3.set_xlabel('Number of Threads')
    ax3.set_ylabel('Throughput (MB/s)')
    ax3.set_title('Encryption: Throughput vs Threads')
    ax3.legend(bbox_to_anchor=(1.05, 1), loc='upper left')
    ax3.grid(True, alpha=0.3)
    ax3.set_xticks(unique_threads)

    # 4) Decrypt: throughput vs threads
    for i, chunk_size in enumerate(unique_chunks):
        chunk_data = decrypt_data[decrypt_data['ChunkMB'] == chunk_size].sort_values('Threads')
        ax4.plot(chunk_data['Threads'], chunk_data['Throughput'], marker='s', label=f'{chunk_size:g}MB',
                 linewidth=2.5, markersize=8, color=chunk_colors[i])
    ax4.set_xlabel('Number of Threads')
    ax4.set_ylabel('Throughput (MB/s)')
    ax4.set_title('Decryption: Throughput vs Threads')
    ax4.legend(bbox_to_anchor=(1.05, 1), loc='upper left')
    ax4.grid(True, alpha=0.3)
    ax4.set_xticks(unique_threads)

    # 5) Max per thread count (bar)
    threads_comparison = []
    for threads in unique_threads:
        enc_max = encrypt_data[encrypt_data['Threads'] == threads]['Throughput'].max()
        dec_max = decrypt_data[decrypt_data['Threads'] == threads]['Throughput'].max()
        threads_comparison.append([threads, enc_max, dec_max])
    threads_df = pd.DataFrame(threads_comparison, columns=['Threads', 'Encrypt_Max', 'Decrypt_Max'])
    x = np.arange(len(threads_df))
    width = 0.35
    ax5.bar(x - width/2, threads_df['Encrypt_Max'], width, label='Encryption', alpha=0.8, color='skyblue')
    ax5.bar(x + width/2, threads_df['Decrypt_Max'], width, label='Decryption', alpha=0.8, color='lightcoral')
    ax5.set_xlabel('Number of Threads')
    ax5.set_ylabel('Max Throughput (MB/s)')
    ax5.set_title('Maximum Throughput by Threads')
    ax5.set_xticks(x)
    ax5.set_xticklabels(threads_df['Threads'])
    ax5.legend()
    ax5.grid(True, alpha=0.3)

    # 6) Scaling efficiency at mid chunk
    mid_chunk = unique_chunks[len(unique_chunks)//2]
    baseline_enc = encrypt_data[encrypt_data['Threads'] == 1].groupby('ChunkMB')['Throughput'].mean()
    baseline_dec = decrypt_data[decrypt_data['Threads'] == 1].groupby('ChunkMB')['Throughput'].mean()
    enc_scaling, dec_scaling = [], []
    for threads in unique_threads:
        enc_th = encrypt_data[(encrypt_data['Threads'] == threads) & (encrypt_data['ChunkMB'] == mid_chunk)]['Throughput'].mean()
        dec_th = decrypt_data[(decrypt_data['Threads'] == threads) & (decrypt_data['ChunkMB'] == mid_chunk)]['Throughput'].mean()
        if mid_chunk in baseline_enc and mid_chunk in baseline_dec:
            enc_scaling.append(enc_th / baseline_enc[mid_chunk])
            dec_scaling.append(dec_th / baseline_dec[mid_chunk])
        else:
            enc_scaling.append(np.nan)
            dec_scaling.append(np.nan)
    ax6.plot(unique_threads, enc_scaling, marker='o', linewidth=3, markersize=10, label='Encryption Scaling', color='blue')
    ax6.plot(unique_threads, dec_scaling, marker='s', linewidth=3, markersize=10, label='Decryption Scaling', color='red')
    ax6.plot(unique_threads, unique_threads, '--', alpha=0.7, color='gray', label='Ideal Linear Scaling')
    ax6.set_xlabel('Number of Threads')
    ax6.set_ylabel('Speedup Factor')
    ax6.set_title(f'Scaling Efficiency (Chunk Size: {mid_chunk:g}MB)')
    ax6.legend()
    ax6.grid(True, alpha=0.3)
    ax6.set_xticks(unique_threads)

    fig.tight_layout()
    fig.savefig(out_path, dpi=300, bbox_inches='tight')
    print(f"[ok] Saved {out_path.name}")


def _plot_encrypt_vs_chunk(ax, encrypt_data: pd.DataFrame, unique_threads: list[int]) -> None:
    colors = plt.cm.Set1(np.linspace(0, 1, len(unique_threads)))
    for i, threads in enumerate(unique_threads):
        td = encrypt_data[encrypt_data['Threads'] == threads].sort_values('ChunkMB')
        ax.plot(td['ChunkMB'], td['Throughput'], marker='o', label=f'{threads}T', linewidth=1.5, markersize=4, color=colors[i])
    ax.set_title('Encrypt: Throughput vs Chunks', fontsize=12, fontweight='bold')
    ax.set_xlabel('Chunk Size (MB)')
    ax.set_ylabel('MB/s')
    ax.legend(ncol=2, fontsize=8)
    ax.grid(True, alpha=0.3)


def _plot_decrypt_vs_chunk(ax, decrypt_data: pd.DataFrame, unique_threads: list[int]) -> None:
    colors = plt.cm.Set1(np.linspace(0, 1, len(unique_threads)))
    for i, threads in enumerate(unique_threads):
        td = decrypt_data[decrypt_data['Threads'] == threads].sort_values('ChunkMB')
        ax.plot(td['ChunkMB'], td['Throughput'], marker='s', label=f'{threads}T', linewidth=1.5, markersize=4, color=colors[i])
    ax.set_title('Decrypt: Throughput vs Chunks', fontsize=12, fontweight='bold')
    ax.set_xlabel('Chunk Size (MB)')
    ax.set_ylabel('MB/s')
    ax.legend(ncol=2, fontsize=8)
    ax.grid(True, alpha=0.3)


def _plot_encrypt_vs_threads(ax, encrypt_data: pd.DataFrame, unique_chunks: list[float]) -> None:
    chunk_colors = plt.cm.tab10(np.linspace(0, 1, len(unique_chunks)))
    for i, ch in enumerate(unique_chunks):
        cd = encrypt_data[encrypt_data['ChunkMB'] == ch].sort_values('Threads')
        ax.plot(cd['Threads'], cd['Throughput'], marker='o', label=f'{ch:g}MB', linewidth=1.5, markersize=4, color=chunk_colors[i])
    ax.set_title('Encrypt: Throughput vs Threads', fontsize=12, fontweight='bold')
    ax.set_xlabel('Threads')
    ax.set_ylabel('MB/s')
    ax.legend(ncol=2, fontsize=8)
    ax.grid(True, alpha=0.3)


def _plot_decrypt_vs_threads(ax, decrypt_data: pd.DataFrame, unique_chunks: list[float]) -> None:
    chunk_colors = plt.cm.tab10(np.linspace(0, 1, len(unique_chunks)))
    for i, ch in enumerate(unique_chunks):
        cd = decrypt_data[decrypt_data['ChunkMB'] == ch].sort_values('Threads')
        ax.plot(cd['Threads'], cd['Throughput'], marker='s', label=f'{ch:g}MB', linewidth=1.5, markersize=4, color=chunk_colors[i])
    ax.set_title('Decrypt: Throughput vs Threads', fontsize=12, fontweight='bold')
    ax.set_xlabel('Threads')
    ax.set_ylabel('MB/s')
    ax.legend(ncol=2, fontsize=8)
    ax.grid(True, alpha=0.3)


def _plot_heatmap(ax, encrypt_data: pd.DataFrame, decrypt_data: pd.DataFrame, unique_threads: list[int]) -> None:
    encrypt_pivot = encrypt_data.pivot(index='Threads', columns='ChunkMB', values='Throughput')
    decrypt_pivot = decrypt_data.pivot(index='Threads', columns='ChunkMB', values='Throughput')
    common_cols = sorted(set(encrypt_pivot.columns).intersection(set(decrypt_pivot.columns)))
    encrypt_pivot = encrypt_pivot[common_cols]
    decrypt_pivot = decrypt_pivot[common_cols]
    combined = (encrypt_pivot + decrypt_pivot) / 2.0
    ax.imshow(combined.values, cmap='viridis', aspect='auto')
    ax.set_title('Performance Heat Map (avg enc/dec)', fontsize=12, fontweight='bold')
    ax.set_xlabel('Chunk Size (MB)')
    ax.set_ylabel('Threads')
    ax.set_xticks(range(len(common_cols)))
    ax.set_xticklabels([f"{c:g}" for c in common_cols])
    ax.set_yticks(range(len(unique_threads)))
    ax.set_yticklabels([str(int(x)) for x in unique_threads])
    for i in range(min(len(unique_threads), combined.shape[0])):
        for j in range(min(len(common_cols), combined.shape[1])):
            ax.text(j, i, f'{combined.iloc[i, j]:.0f}', ha='center', va='center', color='white', fontsize=8, fontweight='bold')


def _plot_bar_avg(ax, encrypt_data: pd.DataFrame, decrypt_data: pd.DataFrame, unique_chunks: list[float]) -> None:
    enc_by_chunk = encrypt_data.groupby('ChunkMB')['Throughput'].agg(['mean', 'std']).reindex(unique_chunks)
    dec_by_chunk = decrypt_data.groupby('ChunkMB')['Throughput'].agg(['mean', 'std']).reindex(unique_chunks)
    x = np.arange(len(unique_chunks))
    width = 0.35
    ax.bar(x - width/2, enc_by_chunk['mean'], width, yerr=enc_by_chunk['std'], label='Encrypt', alpha=0.8, capsize=5, color='skyblue')
    ax.bar(x + width/2, dec_by_chunk['mean'], width, yerr=dec_by_chunk['std'], label='Decrypt', alpha=0.8, capsize=5, color='lightcoral')
    ax.set_title('Average Performance by Chunk Size', fontsize=12, fontweight='bold')
    ax.set_xlabel('Chunk Size (MB)')
    ax.set_ylabel('Avg Throughput (MB/s)')
    ax.set_xticks(x)
    ax.set_xticklabels([f"{c:g}" for c in unique_chunks])
    ax.legend()
    ax.grid(True, alpha=0.3)


def _plot_violin(ax, encrypt_data: pd.DataFrame, decrypt_data: pd.DataFrame) -> None:
    parts = ax.violinplot([encrypt_data['Throughput'], decrypt_data['Throughput']], positions=[1, 2], showmeans=True, showextrema=True)
    for pc, color in zip(parts['bodies'], ['skyblue', 'lightcoral']):
        pc.set_facecolor(color)
        pc.set_alpha(0.7)
    ax.set_title('Performance Distribution', fontsize=12, fontweight='bold')
    ax.set_ylabel('Throughput (MB/s)')
    ax.set_xticks([1, 2])
    ax.set_xticklabels(['Encrypt', 'Decrypt'])
    ax.grid(True, alpha=0.3)


def _plot_scaling_eff(ax, encrypt_data: pd.DataFrame, decrypt_data: pd.DataFrame, unique_threads: list[int], unique_chunks: list[float]) -> None:
    baseline_threads = 1
    mid_chunk = unique_chunks[len(unique_chunks)//2]
    enc_base = encrypt_data[(encrypt_data['Threads'] == baseline_threads) & (encrypt_data['ChunkMB'] == mid_chunk)]['Throughput']
    dec_base = decrypt_data[(decrypt_data['Threads'] == baseline_threads) & (decrypt_data['ChunkMB'] == mid_chunk)]['Throughput']
    enc_eff, dec_eff = [], []
    if not enc_base.empty and not dec_base.empty:
        enc_base_v = enc_base.iloc[0]
        dec_base_v = dec_base.iloc[0]
        for t in unique_threads:
            enc_cur = encrypt_data[(encrypt_data['Threads'] == t) & (encrypt_data['ChunkMB'] == mid_chunk)]['Throughput']
            dec_cur = decrypt_data[(decrypt_data['Threads'] == t) & (decrypt_data['ChunkMB'] == mid_chunk)]['Throughput']
            enc_eff.append(((enc_cur.iloc[0] if not enc_cur.empty else np.nan) / enc_base_v) / t * 100)
            dec_eff.append(((dec_cur.iloc[0] if not dec_cur.empty else np.nan) / dec_base_v) / t * 100)
    ax.plot(unique_threads, enc_eff, marker='o', linewidth=3, markersize=8, label='Encrypt Efficiency', color='blue')
    ax.plot(unique_threads, dec_eff, marker='s', linewidth=3, markersize=8, label='Decrypt Efficiency', color='red')
    ax.axhline(y=100, color='gray', linestyle='--', alpha=0.7, label='Perfect Efficiency')
    ax.set_title(f'Scaling Efficiency ({mid_chunk:g}MB chunks)', fontsize=12, fontweight='bold')
    ax.set_xlabel('Number of Threads')
    ax.set_ylabel('Efficiency (%)')
    ax.legend()
    ax.grid(True, alpha=0.3)


def _plot_ratio(ax, encrypt_data: pd.DataFrame, decrypt_data: pd.DataFrame) -> None:
    ratios = []
    for _, er in encrypt_data.iterrows():
        dr = decrypt_data[(decrypt_data['Threads'] == er['Threads']) & (decrypt_data['ChunkMB'] == er['ChunkMB'])]
        if not dr.empty and er['Throughput'] > 0:
            ratios.append({'Threads': er['Threads'], 'ChunkMB': er['ChunkMB'], 'Ratio': dr['Throughput'].iloc[0] / er['Throughput']})
    ratio_df = pd.DataFrame(ratios)
    if not ratio_df.empty:
        sc = ax.scatter(ratio_df['Threads'], ratio_df['ChunkMB'], c=ratio_df['Ratio'], s=ratio_df['Ratio']*30, cmap='RdYlGn', alpha=0.7, edgecolors='black')
        cbar = plt.colorbar(sc, ax=ax, shrink=0.8)
        cbar.set_label('Decrypt/Encrypt Ratio', rotation=270, labelpad=15)
    ax.set_title('Decrypt/Encrypt Speed Ratios', fontsize=12, fontweight='bold')
    ax.set_xlabel('Threads')
    ax.set_ylabel('Chunk Size (MB)')


def _plot_zones(ax, encrypt_data: pd.DataFrame, decrypt_data: pd.DataFrame) -> None:
    enc_max = encrypt_data['Throughput'].max()
    dec_max = decrypt_data['Throughput'].max()
    high_enc = encrypt_data[encrypt_data['Throughput'] > enc_max * 0.9]
    med_enc = encrypt_data[(encrypt_data['Throughput'] > enc_max * 0.7) & (encrypt_data['Throughput'] <= enc_max * 0.9)]
    high_dec = decrypt_data[decrypt_data['Throughput'] > dec_max * 0.9]
    med_dec = decrypt_data[(decrypt_data['Throughput'] > dec_max * 0.7) & (decrypt_data['Throughput'] <= dec_max * 0.9)]
    ax.scatter(high_enc['Threads'], high_enc['ChunkMB'], c='green', s=100, alpha=0.7, label='High Encrypt', marker='o')
    ax.scatter(med_enc['Threads'], med_enc['ChunkMB'], c='orange', s=80, alpha=0.7, label='Medium Encrypt', marker='o')
    ax.scatter(high_dec['Threads'], high_dec['ChunkMB'], c='darkgreen', s=100, alpha=0.7, label='High Decrypt', marker='s')
    ax.scatter(med_dec['Threads'], med_dec['ChunkMB'], c='darkorange', s=80, alpha=0.7, label='Medium Decrypt', marker='s')
    ax.set_title('Performance Zones', fontsize=12, fontweight='bold')
    ax.set_xlabel('Threads')
    ax.set_ylabel('Chunk Size (MB)')
    ax.legend(bbox_to_anchor=(1.05, 1), loc='upper left')
    ax.grid(True, alpha=0.3)


def _plot_recommendations(ax, encrypt_data: pd.DataFrame, decrypt_data: pd.DataFrame) -> None:
    ax.axis('off')
    enc_best = encrypt_data.loc[encrypt_data['Throughput'].idxmax()]
    dec_best = decrypt_data.loc[decrypt_data['Throughput'].idxmax()]
    lines = [
        f"Best Encrypt: {enc_best['Throughput']:.1f} MB/s @ {int(enc_best['Threads'])}T, {enc_best['ChunkMB']:g}MB",
        f"Best Decrypt: {dec_best['Throughput']:.1f} MB/s @ {int(dec_best['Threads'])}T, {dec_best['ChunkMB']:g}MB",
        f"Avg Encrypt: {encrypt_data['Throughput'].mean():.1f} MB/s",
        f"Avg Decrypt: {decrypt_data['Throughput'].mean():.1f} MB/s",
        "Tips:",
        " - Larger chunks often help encryption",
        " - 2-16 threads typically optimal",
    ]
    ax.text(0.01, 0.95, "\n".join(lines), va='top', ha='left', fontsize=10)


def _plot_pie(ax, encrypt_data: pd.DataFrame, decrypt_data: pd.DataFrame) -> None:
    ax.axis('off')
    avg_speeds = [encrypt_data['Throughput'].mean(), decrypt_data['Throughput'].mean()]
    labels = ['Encrypt', 'Decrypt']
    colors_ = ['skyblue', 'lightcoral']
    ax.pie(avg_speeds, labels=labels, colors=colors_, autopct='%1.0f%%', startangle=90)
    ax.set_title('Performance Share', fontsize=10, fontweight='bold')


def create_mega_analysis(encrypt_data: pd.DataFrame, decrypt_data: pd.DataFrame, out_path: Path) -> None:
    """Replicate the MEGA analysis with multiple subplots (compact)."""
    if encrypt_data.empty or decrypt_data.empty:
        print("[warn] CottonCrypto data empty; skipping mega_performance_analysis.png")
        return

    if sns:
        sns.set_palette("husl")

    fig = plt.figure(figsize=(24, 18))
    gs = gridspec.GridSpec(4, 3, hspace=0.3, wspace=0.25, left=0.05, right=0.95, top=0.95, bottom=0.05)
    fig.suptitle('MEGA Performance Analysis: Complete Encryption/Decryption Study', fontsize=20, fontweight='bold', y=0.97)

    ax1 = fig.add_subplot(gs[0, 0])
    ax2 = fig.add_subplot(gs[0, 1])
    ax3 = fig.add_subplot(gs[0, 2])
    ax4 = fig.add_subplot(gs[1, 0])
    ax5 = fig.add_subplot(gs[1, 1])
    ax6 = fig.add_subplot(gs[1, 2])
    ax7 = fig.add_subplot(gs[2, 0])
    ax8 = fig.add_subplot(gs[2, 1])
    ax9 = fig.add_subplot(gs[2, 2])
    ax10 = fig.add_subplot(gs[3, 0])
    ax11 = fig.add_subplot(gs[3, 1])
    ax12 = fig.add_subplot(gs[3, 2])

    unique_threads = sorted(encrypt_data['Threads'].unique())
    unique_chunks = sorted(encrypt_data['ChunkMB'].unique())

    _plot_encrypt_vs_chunk(ax1, encrypt_data, unique_threads)
    _plot_decrypt_vs_chunk(ax2, decrypt_data, unique_threads)
    _plot_encrypt_vs_threads(ax3, encrypt_data, unique_chunks)
    _plot_decrypt_vs_threads(ax4, decrypt_data, unique_chunks)
    _plot_heatmap(ax5, encrypt_data, decrypt_data, unique_threads)
    _plot_bar_avg(ax6, encrypt_data, decrypt_data, unique_chunks)
    _plot_violin(ax7, encrypt_data, decrypt_data)
    _plot_scaling_eff(ax8, encrypt_data, decrypt_data, unique_threads, unique_chunks)
    _plot_ratio(ax9, encrypt_data, decrypt_data)
    _plot_zones(ax10, encrypt_data, decrypt_data)
    _plot_recommendations(ax11, encrypt_data, decrypt_data)
    _plot_pie(ax12, encrypt_data, decrypt_data)

    fig.savefig(out_path, dpi=300, bbox_inches='tight')
    print(f"[ok] Saved {out_path.name}")


def plot_openssl_comparison(enc: pd.DataFrame, dec: pd.DataFrame, ossl: pd.DataFrame, out_path: Path) -> None:
    """Create a simple comparison: CottonCrypto (best-per-chunk) vs OpenSSL across buffer sizes.

    - X-axis: buffer/chunk size in bytes, log scale
    - Lines: OpenSSL AES-128-GCM, CottonCrypto Encrypt best-per-chunk, CottonCrypto Decrypt best-per-chunk
    """
    if ossl is None or ossl.empty:
        print("[warn] OpenSSL data missing; skipping openssl_comparison.png")
        return

    # CottonCrypto best per chunk (across threads)
    enc_best_per_chunk = enc.groupby("ChunkMB")["Throughput"].max().reset_index()
    dec_best_per_chunk = dec.groupby("ChunkMB")["Throughput"].max().reset_index()

    # Convert ChunkMB (decimal) to bytes for x-axis
    enc_best_per_chunk["BlockBytes"] = (enc_best_per_chunk["ChunkMB"] * 1_000_000).astype(int)
    dec_best_per_chunk["BlockBytes"] = (dec_best_per_chunk["ChunkMB"] * 1_000_000).astype(int)

    fig, ax = plt.subplots(figsize=(10, 6))
    fig.suptitle("CottonCrypto vs OpenSSL: Throughput vs Buffer/Chunk Size", fontsize=16, fontweight="bold", y=0.96)

    # OpenSSL line
    ax.plot(ossl["BlockBytes"], ossl["ThroughputMBps"], marker="o", linewidth=2.5, markersize=7,
            label="OpenSSL AES-128-GCM")

    # CottonCrypto lines (best per chunk)
    ax.plot(enc_best_per_chunk["BlockBytes"], enc_best_per_chunk["Throughput"], marker="s", linewidth=2.0,
            markersize=7, label="CottonCrypto Encrypt (best per chunk)")
    ax.plot(dec_best_per_chunk["BlockBytes"], dec_best_per_chunk["Throughput"], marker="^", linewidth=2.0,
            markersize=7, label="CottonCrypto Decrypt (best per chunk)")

    ax.set_xscale("log")
    ax.set_xlabel("Buffer / Chunk Size (bytes) [log scale]")
    ax.set_ylabel("Throughput (MB/s)")
    ax.grid(True, which="both", axis="both", alpha=0.3)
    ax.legend()

    # Lightweight note to clarify semantics difference
    note = (
        "Note: OpenSSL varies small buffer sizes; CottonCrypto varies file chunk sizes."
        " Scales differ; this is an approximate visual comparison."
    )
    ax.text(0.01, -0.18, note, transform=ax.transAxes, fontsize=9, va="top", ha="left", wrap=True)

    fig.tight_layout()
    fig.savefig(out_path, dpi=300, bbox_inches="tight")
    print(f"[ok] Saved {out_path.name}")


def main(argv: Optional[list[str]] = None) -> int:
    # Allow optional custom paths: charts.py [mylib_input] [openssl_input]
    args = sys.argv[1:] if argv is None else argv
    mylib_path = Path(args[0]).resolve() if len(args) >= 1 else MYLIB_INPUT_DEFAULT
    openssl_path = Path(args[1]).resolve() if len(args) >= 2 else OPENSSL_INPUT_DEFAULT

    if not mylib_path.exists():
        print(f"[error] CottonCrypto input not found: {mylib_path}")
        return 1

    enc, dec = parse_mylib_results(mylib_path)
    if enc.empty or dec.empty:
        print(f"[error] Failed to parse CottonCrypto data from {mylib_path}")
        return 2

    print(f"Loaded CottonCrypto data: enc={len(enc)} rows, dec={len(dec)} rows")
    # 1) Core four panels
    plot_mylib_four_panels(enc, dec, ROOT / "library_performance.png")
    # 2) Advanced analysis (6 charts)
    create_advanced_plots(enc, dec, ROOT / "advanced_performance_analysis.png")
    # 3) MEGA analysis (multi charts)
    create_mega_analysis(enc, dec, ROOT / "mega_performance_analysis.png")

    # OpenSSL part is optional
    ossl_df = pd.DataFrame()
    if openssl_path.exists():
        ossl_df = parse_openssl_results(openssl_path)
        print(f"Loaded OpenSSL data: {len(ossl_df)} points")
        plot_openssl_comparison(enc, dec, ossl_df, ROOT / "openssl_comparison.png")
    else:
        print(f"[info] OpenSSL input not found, skipping comparison: {openssl_path}")

    # Print a tiny summary to console
    enc_best = enc.loc[enc["Throughput"].idxmax()]
    dec_best = dec.loc[dec["Throughput"].idxmax()]
    print("\nSummary:")
    print(f"  CottonCrypto Encrypt best: {enc_best['Throughput']:.1f} MB/s at {enc_best['Threads']} threads, {enc_best['ChunkMB']}MB chunks")
    print(f"  CottonCrypto Decrypt best: {dec_best['Throughput']:.1f} MB/s at {dec_best['Threads']} threads, {dec_best['ChunkMB']}MB chunks")
    if not ossl_df.empty:
        print(f"  OpenSSL best: {ossl_df['ThroughputMBps'].max():.1f} MB/s at {int(ossl_df.loc[ossl_df['ThroughputMBps'].idxmax(), 'BlockBytes'])} bytes buffer")

    return 0


if __name__ == "__main__":
    raise SystemExit(main())
