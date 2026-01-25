import matplotlib.pyplot as plt
import pandas as pd
import numpy as np
from test_parser import parse_test_results


def create_plots(encrypt_data, decrypt_data):
    """Create four polished plots"""

    # Configure matplotlib style
    plt.rcParams['figure.facecolor'] = 'white'
    plt.rcParams['axes.facecolor'] = 'white'
    plt.rcParams['axes.grid'] = True
    plt.rcParams['grid.alpha'] = 0.3

    fig, ((ax1, ax2), (ax3, ax4)) = plt.subplots(2, 2, figsize=(16, 12))
    fig.suptitle('Performance Analysis: Encryption/Decryption Throughput',
                 fontsize=16, fontweight='bold', y=0.98)

    # Color schemes
    thread_colors = ['#1f77b4', '#ff7f0e',
                     '#2ca02c', '#d62728', '#9467bd', '#8c564b']
    chunk_colors = ['#e41a1c', '#377eb8', '#4daf4a',
                    '#984ea3', '#ff7f00', '#ffff33', '#a65628']

    # Plot 1: Encrypt - throughput vs chunk size (per thread count)
    unique_threads = sorted(encrypt_data['Threads'].unique())
    for i, threads in enumerate(unique_threads):
        thread_data = encrypt_data[encrypt_data['Threads']
                                   == threads].sort_values('ChunkMB')
        ax1.plot(thread_data['ChunkMB'], thread_data['Throughput'],
                 marker='o', label=f'{threads} threads', linewidth=2.5,
                 markersize=8, color=thread_colors[i % len(thread_colors)])

    ax1.set_xlabel('Chunk Size (MB)', fontsize=12, fontweight='bold')
    ax1.set_ylabel('Throughput (MB/s)', fontsize=12, fontweight='bold')
    ax1.set_title('Encryption: Throughput vs Chunk Size',
                  fontsize=14, fontweight='bold')
    ax1.legend(frameon=True, fancybox=True, shadow=True)
    ax1.grid(True, alpha=0.3, linestyle='-', linewidth=0.5)

    # Add x-axis labels for chunk sizes
    chunk_ticks = sorted(encrypt_data['ChunkMB'].unique())
    ax1.set_xticks(chunk_ticks)
    ax1.set_xticklabels([f'{int(x)}' for x in chunk_ticks])

    # Plot 2: Decrypt - throughput vs chunk size (per thread count)
    for i, threads in enumerate(unique_threads):
        thread_data = decrypt_data[decrypt_data['Threads']
                                   == threads].sort_values('ChunkMB')
        ax2.plot(thread_data['ChunkMB'], thread_data['Throughput'],
                 marker='s', label=f'{threads} threads', linewidth=2.5,
                 markersize=8, color=thread_colors[i % len(thread_colors)])

    ax2.set_xlabel('Chunk Size (MB)', fontsize=12, fontweight='bold')
    ax2.set_ylabel('Throughput (MB/s)', fontsize=12, fontweight='bold')
    ax2.set_title('Decryption: Throughput vs Chunk Size',
                  fontsize=14, fontweight='bold')
    ax2.legend(frameon=True, fancybox=True, shadow=True)
    ax2.grid(True, alpha=0.3, linestyle='-', linewidth=0.5)
    ax2.set_xticks(chunk_ticks)
    ax2.set_xticklabels([f'{int(x)}' for x in chunk_ticks])

    # Plot 3: Encrypt - throughput vs threads (per chunk size)
    unique_chunks = sorted(encrypt_data['ChunkMB'].unique())

    for i, chunk_size in enumerate(unique_chunks):
        chunk_data = encrypt_data[encrypt_data['ChunkMB']
                                  == chunk_size].sort_values('Threads')
        ax3.plot(chunk_data['Threads'], chunk_data['Throughput'],
                 marker='o', label=f'{int(chunk_size)}MB', linewidth=2.5,
                 markersize=8, color=chunk_colors[i % len(chunk_colors)])

    ax3.set_xlabel('Number of Threads', fontsize=12, fontweight='bold')
    ax3.set_ylabel('Throughput (MB/s)', fontsize=12, fontweight='bold')
    ax3.set_title('Encryption: Throughput vs Threads',
                  fontsize=14, fontweight='bold')
    ax3.legend(frameon=True, fancybox=True, shadow=True, title='Chunk Size')
    ax3.grid(True, alpha=0.3, linestyle='-', linewidth=0.5)
    ax3.set_xticks(unique_threads)
    ax3.set_xticklabels([str(int(x)) for x in unique_threads])

    # Plot 4: Decrypt - throughput vs threads (per chunk size)
    for i, chunk_size in enumerate(unique_chunks):
        chunk_data = decrypt_data[decrypt_data['ChunkMB']
                                  == chunk_size].sort_values('Threads')
        ax4.plot(chunk_data['Threads'], chunk_data['Throughput'],
                 marker='s', label=f'{int(chunk_size)}MB', linewidth=2.5,
                 markersize=8, color=chunk_colors[i % len(chunk_colors)])

    ax4.set_xlabel('Number of Threads', fontsize=12, fontweight='bold')
    ax4.set_ylabel('Throughput (MB/s)', fontsize=12, fontweight='bold')
    ax4.set_title('Decryption: Throughput vs Threads',
                  fontsize=14, fontweight='bold')
    ax4.legend(frameon=True, fancybox=True, shadow=True, title='Chunk Size')
    ax4.grid(True, alpha=0.3, linestyle='-', linewidth=0.5)
    ax4.set_xticks(unique_threads)
    ax4.set_xticklabels([str(int(x)) for x in unique_threads])

    # Improve visuals
    for ax in [ax1, ax2, ax3, ax4]:
        ax.spines['top'].set_visible(False)
        ax.spines['right'].set_visible(False)
        ax.spines['left'].set_linewidth(0.5)
        ax.spines['bottom'].set_linewidth(0.5)

    plt.tight_layout()
    return fig


def print_summary(encrypt_data, decrypt_data):
    """Print a short summary"""

    print("\n" + "="*50)
    print("BRIEF ANALYSIS SUMMARY")
    print("="*50)

    # Находим лучшие результаты
    encrypt_best = encrypt_data.loc[encrypt_data['Throughput'].idxmax()]
    decrypt_best = decrypt_data.loc[decrypt_data['Throughput'].idxmax()]

    print(f"\n🏆 TOP RESULTS:")
    print(f"   Encryption: {encrypt_best['Throughput']:.1f} MB/s")
    print(
        f"   ({encrypt_best['Threads']:.0f} threads, {encrypt_best['ChunkMB']:.0f}MB chunks)")
    print(f"   Decryption: {decrypt_best['Throughput']:.1f} MB/s")
    print(
        f"   ({decrypt_best['Threads']:.0f} threads, {decrypt_best['ChunkMB']:.0f}MB chunks)")

    print(f"\n📊 AVERAGE VALUES:")
    print(f"   Encryption: {encrypt_data['Throughput'].mean():.1f} MB/s")
    print(f"   Decryption: {decrypt_data['Throughput'].mean():.1f} MB/s")
    print(
        f"   Decryption is {((decrypt_data['Throughput'].mean() / encrypt_data['Throughput'].mean() - 1) * 100):.1f}% faster")

    print("\n" + "="*50)


def main():
    """Main function"""
    try:
        # Parse data from file
        encrypt_data, decrypt_data = parse_test_results('input.txt')

        if encrypt_data.empty or decrypt_data.empty:
            print("Error: failed to find data in input.txt")
            return

        print(f"✅ Data successfully loaded:")
        print(f"   Encryption: {len(encrypt_data)} records")
        print(f"   Decryption: {len(decrypt_data)} records")

        # Create plots
        fig = create_plots(encrypt_data, decrypt_data)

        # Save plots
        fig.savefig('performance_charts.png', dpi=300, bbox_inches='tight',
                    facecolor='white', edgecolor='none')
        print(f"\n💾 Charts saved to performance_charts.png")

        # Print summary
        print_summary(encrypt_data, decrypt_data)

        # Show plots
        plt.show()

        print(f"\n📈 The following charts were created:")
        print(f"   1. Encryption: Throughput vs Chunk Size")
        print(f"   2. Decryption: Throughput vs Chunk Size")
        print(f"   3. Encryption: Throughput vs Threads")
        print(f"   4. Decryption: Throughput vs Threads")

    except FileNotFoundError:
        print("❌ Error: file 'input.txt' not found")
    except Exception as e:
        print(f"❌ Error: {e}")


if __name__ == "__main__":
    main()
