import re
import matplotlib.pyplot as plt
import pandas as pd
import numpy as np
import seaborn as sns
from charts_utils import parse_test_results



def create_advanced_plots(encrypt_data, decrypt_data):
    """Create an advanced set of plots with additional analysis"""

    # Устанавливаем стиль
    plt.style.use('seaborn-v0_8')

    # Создаем фигуру с 6 субплотами
    fig = plt.figure(figsize=(20, 16))
    gs = fig.add_gridspec(3, 2, hspace=0.3, wspace=0.25)

    # Основные 4 графика
    ax1 = fig.add_subplot(gs[0, 0])
    ax2 = fig.add_subplot(gs[0, 1])
    ax3 = fig.add_subplot(gs[1, 0])
    ax4 = fig.add_subplot(gs[1, 1])

    # Дополнительные аналитические графики
    ax5 = fig.add_subplot(gs[2, 0])
    ax6 = fig.add_subplot(gs[2, 1])

    fig.suptitle('Complete Performance Analysis: Encryption/Decryption Throughput',
                 fontsize=18, fontweight='bold')

    # График 1: Encrypt - throughput vs chunk size
    unique_threads = sorted(encrypt_data['Threads'].unique())
    colors = plt.cm.Set1(np.linspace(0, 1, len(unique_threads)))

    for i, threads in enumerate(unique_threads):
        thread_data = encrypt_data[encrypt_data['Threads']
                                   == threads].sort_values('ChunkMB')
        ax1.plot(thread_data['ChunkMB'], thread_data['Throughput'],
                 marker='o', label=f'{threads} threads', linewidth=2.5,
                 markersize=8, color=colors[i])

    ax1.set_xlabel('Chunk Size (MB)', fontsize=12, fontweight='bold')
    ax1.set_ylabel('Throughput (MB/s)', fontsize=12, fontweight='bold')
    ax1.set_title('Encryption: Throughput vs Chunk Size',
                  fontsize=14, fontweight='bold')
    ax1.legend(bbox_to_anchor=(1.05, 1), loc='upper left')
    ax1.grid(True, alpha=0.3)

    # Добавляем подписи осей с размерами чанков
    chunk_ticks = sorted(encrypt_data['ChunkMB'].unique())
    ax1.set_xticks(chunk_ticks)
    ax1.set_xticklabels([str(int(x)) for x in chunk_ticks])

    # График 2: Decrypt - throughput vs chunk size
    for i, threads in enumerate(unique_threads):
        thread_data = decrypt_data[decrypt_data['Threads']
                                   == threads].sort_values('ChunkMB')
        ax2.plot(thread_data['ChunkMB'], thread_data['Throughput'],
                 marker='s', label=f'{threads} threads', linewidth=2.5,
                 markersize=8, color=colors[i])

    ax2.set_xlabel('Chunk Size (MB)', fontsize=12, fontweight='bold')
    ax2.set_ylabel('Throughput (MB/s)', fontsize=12, fontweight='bold')
    ax2.set_title('Decryption: Throughput vs Chunk Size',
                  fontsize=14, fontweight='bold')
    ax2.legend(bbox_to_anchor=(1.05, 1), loc='upper left')
    ax2.grid(True, alpha=0.3)
    ax2.set_xticks(chunk_ticks)
    ax2.set_xticklabels([str(int(x)) for x in chunk_ticks])

    # График 3: Encrypt - throughput vs threads
    unique_chunks = sorted(encrypt_data['ChunkMB'].unique())
    chunk_colors = plt.cm.tab10(np.linspace(0, 1, len(unique_chunks)))

    for i, chunk_size in enumerate(unique_chunks):
        chunk_data = encrypt_data[encrypt_data['ChunkMB']
                                  == chunk_size].sort_values('Threads')
        ax3.plot(chunk_data['Threads'], chunk_data['Throughput'],
                 marker='o', label=f'{chunk_size}MB', linewidth=2.5,
                 markersize=8, color=chunk_colors[i])

    ax3.set_xlabel('Number of Threads', fontsize=12, fontweight='bold')
    ax3.set_ylabel('Throughput (MB/s)', fontsize=12, fontweight='bold')
    ax3.set_title('Encryption: Throughput vs Threads',
                  fontsize=14, fontweight='bold')
    ax3.legend(bbox_to_anchor=(1.05, 1), loc='upper left')
    ax3.grid(True, alpha=0.3)
    ax3.set_xticks(unique_threads)
    ax3.set_xticklabels([str(int(x)) for x in unique_threads])

    # График 4: Decrypt - throughput vs threads
    for i, chunk_size in enumerate(unique_chunks):
        chunk_data = decrypt_data[decrypt_data['ChunkMB']
                                  == chunk_size].sort_values('Threads')
        ax4.plot(chunk_data['Threads'], chunk_data['Throughput'],
                 marker='s', label=f'{chunk_size}MB', linewidth=2.5,
                 markersize=8, color=chunk_colors[i])

    ax4.set_xlabel('Number of Threads', fontsize=12, fontweight='bold')
    ax4.set_ylabel('Throughput (MB/s)', fontsize=12, fontweight='bold')
    ax4.set_title('Decryption: Throughput vs Threads',
                  fontsize=14, fontweight='bold')
    ax4.legend(bbox_to_anchor=(1.05, 1), loc='upper left')
    ax4.grid(True, alpha=0.3)
    ax4.set_xticks(unique_threads)
    ax4.set_xticklabels([str(int(x)) for x in unique_threads])

    # Plot 5: Comparison of best results
    encrypt_best = encrypt_data.groupby(['Threads', 'ChunkMB'])[
        'Throughput'].max().reset_index()
    decrypt_best = decrypt_data.groupby(['Threads', 'ChunkMB'])[
        'Throughput'].max().reset_index()

    # Find optimal configurations
    encrypt_optimal = encrypt_data.loc[encrypt_data['Throughput'].idxmax()]
    decrypt_optimal = decrypt_data.loc[decrypt_data['Throughput'].idxmax()]

    # Барный график сравнения максимальных значений по потокам
    threads_comparison = []
    for threads in unique_threads:
        enc_max = encrypt_data[encrypt_data['Threads']
                               == threads]['Throughput'].max()
        dec_max = decrypt_data[decrypt_data['Threads']
                               == threads]['Throughput'].max()
        threads_comparison.append([threads, enc_max, dec_max])

    threads_df = pd.DataFrame(threads_comparison, columns=[
                              'Threads', 'Encrypt_Max', 'Decrypt_Max'])

    x = np.arange(len(threads_df))
    width = 0.35

    ax5.bar(x - width/2, threads_df['Encrypt_Max'], width,
            label='Encryption', alpha=0.8, color='skyblue')
    ax5.bar(x + width/2, threads_df['Decrypt_Max'], width,
            label='Decryption', alpha=0.8, color='lightcoral')

    ax5.set_xlabel('Number of Threads', fontsize=12, fontweight='bold')
    ax5.set_ylabel('Max Throughput (MB/s)', fontsize=12, fontweight='bold')
    ax5.set_title('Maximum Throughput Comparison by Thread Count',
                  fontsize=14, fontweight='bold')
    ax5.set_xticks(x)
    ax5.set_xticklabels(threads_df['Threads'])
    ax5.legend()
    ax5.grid(True, alpha=0.3)

    # Добавляем значения на столбцы
    for i, (enc, dec) in enumerate(zip(threads_df['Encrypt_Max'], threads_df['Decrypt_Max'])):
        ax5.text(i - width/2, enc + 50,
                 f'{enc:.0f}', ha='center', va='bottom', fontsize=10)
        ax5.text(i + width/2, dec + 50,
                 f'{dec:.0f}', ha='center', va='bottom', fontsize=10)

    # График 6: Эффективность масштабирования
    # Вычисляем speedup относительно 1 потока
    baseline_enc = encrypt_data[encrypt_data['Threads'] == 1].groupby('ChunkMB')[
        'Throughput'].mean()
    baseline_dec = decrypt_data[decrypt_data['Threads'] == 1].groupby('ChunkMB')[
        'Throughput'].mean()

    # Берем средний размер чанка для анализа масштабирования
    mid_chunk = unique_chunks[len(unique_chunks)//2]  # Средний размер чанка

    enc_scaling = []
    dec_scaling = []
    for threads in unique_threads:
        enc_throughput = encrypt_data[(encrypt_data['Threads'] == threads) &
                                      (encrypt_data['ChunkMB'] == mid_chunk)]['Throughput'].mean()
        dec_throughput = decrypt_data[(decrypt_data['Threads'] == threads) &
                                      (decrypt_data['ChunkMB'] == mid_chunk)]['Throughput'].mean()

        enc_baseline = baseline_enc[mid_chunk]
        dec_baseline = baseline_dec[mid_chunk]

        enc_scaling.append(enc_throughput / enc_baseline)
        dec_scaling.append(dec_throughput / dec_baseline)

    ax6.plot(unique_threads, enc_scaling, marker='o', linewidth=3, markersize=10,
             label='Encryption Scaling', color='blue')
    ax6.plot(unique_threads, dec_scaling, marker='s', linewidth=3, markersize=10,
             label='Decryption Scaling', color='red')
    ax6.plot(unique_threads, unique_threads, '--', alpha=0.7, color='gray',
             label='Ideal Linear Scaling')

    ax6.set_xlabel('Number of Threads', fontsize=12, fontweight='bold')
    ax6.set_ylabel('Speedup Factor', fontsize=12, fontweight='bold')
    ax6.set_title(
        f'Scaling Efficiency (Chunk Size: {mid_chunk}MB)', fontsize=14, fontweight='bold')
    ax6.legend()
    ax6.grid(True, alpha=0.3)
    ax6.set_xticks(unique_threads)
    ax6.set_xticklabels([str(int(x)) for x in unique_threads])

    plt.tight_layout()

    return fig, encrypt_optimal, decrypt_optimal


def print_analysis_summary(encrypt_data, decrypt_data, encrypt_optimal, decrypt_optimal):
    """Print performance analysis summary"""

    print("\n" + "="*60)
    print("PERFORMANCE ANALYSIS")
    print("="*60)

    print(f"\n📊 OPTIMAL CONFIGURATIONS:")
    print(f"   Encryption:")
    print(f"      Best result: {encrypt_optimal['Throughput']:.1f} MB/s")
    print(
        f"      Threads: {encrypt_optimal['Threads']}, Chunk size: {encrypt_optimal['ChunkMB']}MB")

    print(f"   Decryption:")
    print(f"      Best result: {decrypt_optimal['Throughput']:.1f} MB/s")
    print(
        f"      Threads: {decrypt_optimal['Threads']}, Chunk size: {decrypt_optimal['ChunkMB']}MB")

    # Statistics per operation
    print(f"\n📈 OVERALL STATISTICS:")
    print(f"   Encryption:")
    print(f"      Mean: {encrypt_data['Throughput'].mean():.1f} MB/s")
    print(f"      Median: {encrypt_data['Throughput'].median():.1f} MB/s")
    print(
        f"      Min/Max: {encrypt_data['Throughput'].min():.1f} / {encrypt_data['Throughput'].max():.1f} MB/s")

    print(f"   Decryption:")
    print(f"      Mean: {decrypt_data['Throughput'].mean():.1f} MB/s")
    print(f"      Median: {decrypt_data['Throughput'].median():.1f} MB/s")
    print(
        f"      Min/Max: {decrypt_data['Throughput'].min():.1f} / {decrypt_data['Throughput'].max():.1f} MB/s")

    # Chunk size impact analysis
    print(f"\n🧩 EFFECT OF CHUNK SIZE:")
    for operation, data in [("Encryption", encrypt_data), ("Decryption", decrypt_data)]:
        chunk_performance = data.groupby(
            'ChunkMB')['Throughput'].mean().sort_values(ascending=False)
        best_chunk = chunk_performance.index[0]
        worst_chunk = chunk_performance.index[-1]
        print(f"   {operation}:")
        print(
            f"      Best chunk size: {best_chunk}MB ({chunk_performance[best_chunk]:.1f} MB/s)")
        print(
            f"      Worst chunk size: {worst_chunk}MB ({chunk_performance[worst_chunk]:.1f} MB/s)")

    print("\n" + "="*60)


def main():
    """Основная функция"""
    try:
        # Парсим данные из файла
        encrypt_data, decrypt_data = parse_test_results('input.txt')

        if encrypt_data.empty or decrypt_data.empty:
            print("Ошибка: не удалось найти данные в файле input.txt")
            return

        print(f"Загружено данных:")
        print(f"  Encryption: {len(encrypt_data)} записей")
        print(f"  Decryption: {len(decrypt_data)} записей")

        # Создаем расширенные графики
        fig, encrypt_optimal, decrypt_optimal = create_advanced_plots(
            encrypt_data, decrypt_data)

        # Сохраняем графики
        fig.savefig('advanced_performance_analysis.png',
                    dpi=300, bbox_inches='tight')
        print(f"\nРасширенные графики сохранены в advanced_performance_analysis.png")

        # Выводим аналитическую сводку
        print_analysis_summary(encrypt_data, decrypt_data,
                               encrypt_optimal, decrypt_optimal)

        plt.show()

    except FileNotFoundError:
        print("Error: file 'input.txt' not found")
    except Exception as e:
        print(f"Error: {e}")
        import traceback
        traceback.print_exc()


if __name__ == "__main__":
    main()
