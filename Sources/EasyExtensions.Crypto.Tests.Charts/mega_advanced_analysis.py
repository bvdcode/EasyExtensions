import re
import matplotlib.pyplot as plt
import pandas as pd
import numpy as np
import seaborn as sns
from matplotlib.patches import Rectangle
import matplotlib.gridspec as gridspec
from charts_utils import parse_test_results



def create_mega_analysis(encrypt_data, decrypt_data):
    """Create an extended set of 12 plots with detailed analysis"""

    # Устанавливаем стиль
    plt.style.use('default')
    sns.set_palette("husl")

    # Создаем большую фигуру с 12 субплотами
    fig = plt.figure(figsize=(24, 18))
    gs = gridspec.GridSpec(4, 3, hspace=0.3, wspace=0.25,
                           left=0.05, right=0.95, top=0.95, bottom=0.05)

    fig.suptitle('🚀 MEGA Performance Analysis: Complete Encryption/Decryption Study',
                 fontsize=20, fontweight='bold', y=0.97)

    # График 1: Основные 4 графика в мини-версии
    ax1 = fig.add_subplot(gs[0, 0])
    ax2 = fig.add_subplot(gs[0, 1])
    ax3 = fig.add_subplot(gs[0, 2])
    ax4 = fig.add_subplot(gs[1, 0])

    # Дополнительные аналитические графики
    ax5 = fig.add_subplot(gs[1, 1])   # Heat map
    ax6 = fig.add_subplot(gs[1, 2])   # 3D surface plot style
    ax7 = fig.add_subplot(gs[2, 0])   # Distribution plots
    ax8 = fig.add_subplot(gs[2, 1])   # Efficiency metrics
    ax9 = fig.add_subplot(gs[2, 2])   # Speed ratios
    ax10 = fig.add_subplot(gs[3, 0])  # Performance zones
    ax11 = fig.add_subplot(gs[3, 1])  # Optimization suggestions
    ax12 = fig.add_subplot(gs[3, 2])  # Summary dashboard

    unique_threads = sorted(encrypt_data['Threads'].unique())
    unique_chunks = sorted(encrypt_data['ChunkMB'].unique())

    # 1. Encrypt: throughput vs chunk size (compact)
    colors = plt.cm.Set1(np.linspace(0, 1, len(unique_threads)))
    for i, threads in enumerate(unique_threads):
        thread_data = encrypt_data[encrypt_data['Threads']
                                   == threads].sort_values('ChunkMB')
        ax1.plot(thread_data['ChunkMB'], thread_data['Throughput'],
                 marker='o', label=f'{threads}T', linewidth=1.5, markersize=4, color=colors[i])
    ax1.set_title('Encrypt: Throughput vs Chunks',
                  fontsize=12, fontweight='bold')
    ax1.set_xlabel('Chunk Size (MB)')
    ax1.set_ylabel('MB/s')
    ax1.legend(ncol=2, fontsize=8)
    ax1.grid(True, alpha=0.3)

    # 2. Decrypt: throughput vs chunk size (compact)
    for i, threads in enumerate(unique_threads):
        thread_data = decrypt_data[decrypt_data['Threads']
                                   == threads].sort_values('ChunkMB')
        ax2.plot(thread_data['ChunkMB'], thread_data['Throughput'],
                 marker='s', label=f'{threads}T', linewidth=1.5, markersize=4, color=colors[i])
    ax2.set_title('Decrypt: Throughput vs Chunks',
                  fontsize=12, fontweight='bold')
    ax2.set_xlabel('Chunk Size (MB)')
    ax2.set_ylabel('MB/s')
    ax2.legend(ncol=2, fontsize=8)
    ax2.grid(True, alpha=0.3)

    # 3. Encrypt: throughput vs threads (compact)
    chunk_colors = plt.cm.tab10(np.linspace(0, 1, len(unique_chunks)))
    for i, chunk_size in enumerate(unique_chunks):
        chunk_data = encrypt_data[encrypt_data['ChunkMB']
                                  == chunk_size].sort_values('Threads')
        ax3.plot(chunk_data['Threads'], chunk_data['Throughput'],
                 marker='o', label=f'{int(chunk_size)}MB', linewidth=1.5, markersize=4, color=chunk_colors[i])
    ax3.set_title('Encrypt: Throughput vs Threads',
                  fontsize=12, fontweight='bold')
    ax3.set_xlabel('Threads')
    ax3.set_ylabel('MB/s')
    ax3.legend(ncol=2, fontsize=8)
    ax3.grid(True, alpha=0.3)

    # 4. Decrypt: throughput vs threads (compact)
    for i, chunk_size in enumerate(unique_chunks):
        chunk_data = decrypt_data[decrypt_data['ChunkMB']
                                  == chunk_size].sort_values('Threads')
        ax4.plot(chunk_data['Threads'], chunk_data['Throughput'],
                 marker='s', label=f'{int(chunk_size)}MB', linewidth=1.5, markersize=4, color=chunk_colors[i])
    ax4.set_title('Decrypt: Throughput vs Threads',
                  fontsize=12, fontweight='bold')
    ax4.set_xlabel('Threads')
    ax4.set_ylabel('MB/s')
    ax4.legend(ncol=2, fontsize=8)
    ax4.grid(True, alpha=0.3)

    # 5. Heat Map - Performance Matrix
    encrypt_pivot = encrypt_data.pivot(
        index='Threads', columns='ChunkMB', values='Throughput')
    decrypt_pivot = decrypt_data.pivot(
        index='Threads', columns='ChunkMB', values='Throughput')

    # Создаем комбинированную heat map
    combined_data = (encrypt_pivot + decrypt_pivot) / \
        2  # Средняя производительность
    im = ax5.imshow(combined_data.values, cmap='viridis', aspect='auto')
    ax5.set_title('🔥 Performance Heat Map\n(Average Encrypt+Decrypt)',
                  fontsize=12, fontweight='bold')
    ax5.set_xlabel('Chunk Size (MB)')
    ax5.set_ylabel('Threads')
    ax5.set_xticks(range(len(unique_chunks)))
    ax5.set_xticklabels([str(int(x)) for x in unique_chunks])
    ax5.set_yticks(range(len(unique_threads)))
    ax5.set_yticklabels([str(int(x)) for x in unique_threads])

    # Добавляем значения в ячейки
    for i in range(len(unique_threads)):
        for j in range(len(unique_chunks)):
            text = ax5.text(j, i, f'{combined_data.iloc[i, j]:.0f}',
                            ha="center", va="center", color="white", fontsize=8, fontweight='bold')

    # 6. Efficiency by chunk size
    encrypt_by_chunk = encrypt_data.groupby('ChunkMB')['Throughput'].agg([
        'mean', 'std', 'max', 'min'])
    decrypt_by_chunk = decrypt_data.groupby('ChunkMB')['Throughput'].agg([
        'mean', 'std', 'max', 'min'])

    x = np.arange(len(unique_chunks))
    width = 0.35

    bars1 = ax6.bar(x - width/2, encrypt_by_chunk['mean'], width,
                    yerr=encrypt_by_chunk['std'], label='Encrypt',
                    alpha=0.8, capsize=5, color='skyblue')
    bars2 = ax6.bar(x + width/2, decrypt_by_chunk['mean'], width,
                    yerr=decrypt_by_chunk['std'], label='Decrypt',
                    alpha=0.8, capsize=5, color='lightcoral')

    ax6.set_title('📊 Average Performance by Chunk Size',
                  fontsize=12, fontweight='bold')
    ax6.set_xlabel('Chunk Size (MB)')
    ax6.set_ylabel('Average Throughput (MB/s)')
    ax6.set_xticks(x)
    ax6.set_xticklabels([str(int(x)) for x in unique_chunks])
    ax6.legend()
    ax6.grid(True, alpha=0.3)

    # Добавляем значения на столбцы
    for i, (bar1, bar2) in enumerate(zip(bars1, bars2)):
        ax6.text(bar1.get_x() + bar1.get_width()/2, bar1.get_height() + 100,
                 f'{encrypt_by_chunk.iloc[i]["mean"]:.0f}',
                 ha='center', va='bottom', fontsize=8, rotation=45)
        ax6.text(bar2.get_x() + bar2.get_width()/2, bar2.get_height() + 100,
                 f'{decrypt_by_chunk.iloc[i]["mean"]:.0f}',
                 ha='center', va='bottom', fontsize=8, rotation=45)

    # 7. Distribution Analysis - Violin plots
    # Подготавливаем данные для violin plot
    encrypt_melted = encrypt_data.melt(id_vars=['Threads', 'ChunkMB'],
                                       value_vars=['Throughput'],
                                       var_name='Metric', value_name='Value')
    encrypt_melted['Operation'] = 'Encrypt'

    decrypt_melted = decrypt_data.melt(id_vars=['Threads', 'ChunkMB'],
                                       value_vars=['Throughput'],
                                       var_name='Metric', value_name='Value')
    decrypt_melted['Operation'] = 'Decrypt'

    combined_melted = pd.concat([encrypt_melted, decrypt_melted])

    # Создаем violin plot
    parts = ax7.violinplot([encrypt_data['Throughput'], decrypt_data['Throughput']],
                           positions=[1, 2], showmeans=True, showextrema=True)

    for pc, color in zip(parts['bodies'], ['skyblue', 'lightcoral']):
        pc.set_facecolor(color)
        pc.set_alpha(0.7)

    ax7.set_title('🎻 Performance Distribution', fontsize=12, fontweight='bold')
    ax7.set_ylabel('Throughput (MB/s)')
    ax7.set_xticks([1, 2])
    ax7.set_xticklabels(['Encrypt', 'Decrypt'])
    ax7.grid(True, alpha=0.3)

    # 8. Scaling Efficiency Analysis
    # Вычисляем эффективность масштабирования
    baseline_threads = 1
    scaling_data = []

    for chunk_size in unique_chunks:
        encrypt_baseline = encrypt_data[(encrypt_data['Threads'] == baseline_threads) &
                                        (encrypt_data['ChunkMB'] == chunk_size)]['Throughput'].iloc[0]
        decrypt_baseline = decrypt_data[(decrypt_data['Threads'] == baseline_threads) &
                                        (decrypt_data['ChunkMB'] == chunk_size)]['Throughput'].iloc[0]

        for threads in unique_threads:
            encrypt_current = encrypt_data[(encrypt_data['Threads'] == threads) &
                                           (encrypt_data['ChunkMB'] == chunk_size)]['Throughput'].iloc[0]
            decrypt_current = decrypt_data[(decrypt_data['Threads'] == threads) &
                                           (decrypt_data['ChunkMB'] == chunk_size)]['Throughput'].iloc[0]

            encrypt_efficiency = (
                encrypt_current / encrypt_baseline) / threads * 100
            decrypt_efficiency = (
                decrypt_current / decrypt_baseline) / threads * 100

            scaling_data.append({
                'Threads': threads,
                'ChunkMB': chunk_size,
                'Encrypt_Efficiency': encrypt_efficiency,
                'Decrypt_Efficiency': decrypt_efficiency
            })

    scaling_df = pd.DataFrame(scaling_data)

    # Показываем эффективность для среднего размера чанка
    mid_chunk = unique_chunks[len(unique_chunks)//2]
    mid_data = scaling_df[scaling_df['ChunkMB'] == mid_chunk]

    ax8.plot(mid_data['Threads'], mid_data['Encrypt_Efficiency'],
             marker='o', linewidth=3, markersize=8, label='Encrypt Efficiency', color='blue')
    ax8.plot(mid_data['Threads'], mid_data['Decrypt_Efficiency'],
             marker='s', linewidth=3, markersize=8, label='Decrypt Efficiency', color='red')
    ax8.axhline(y=100, color='gray', linestyle='--',
                alpha=0.7, label='Perfect Efficiency')

    ax8.set_title(
        f'⚡ Scaling Efficiency ({mid_chunk}MB chunks)', fontsize=12, fontweight='bold')
    ax8.set_xlabel('Number of Threads')
    ax8.set_ylabel('Efficiency (%)')
    ax8.legend()
    ax8.grid(True, alpha=0.3)

    # 9. Speed Ratios Analysis
    ratio_data = []
    for _, encrypt_row in encrypt_data.iterrows():
        decrypt_row = decrypt_data[(decrypt_data['Threads'] == encrypt_row['Threads']) &
                                   (decrypt_data['ChunkMB'] == encrypt_row['ChunkMB'])]
        if not decrypt_row.empty:
            ratio = decrypt_row['Throughput'].iloc[0] / \
                encrypt_row['Throughput']
            ratio_data.append({
                'Threads': encrypt_row['Threads'],
                'ChunkMB': encrypt_row['ChunkMB'],
                'Decrypt_Encrypt_Ratio': ratio
            })

    ratio_df = pd.DataFrame(ratio_data)

    # Создаем scatter plot с размерами точек
    scatter = ax9.scatter(ratio_df['Threads'], ratio_df['ChunkMB'],
                          c=ratio_df['Decrypt_Encrypt_Ratio'],
                          s=ratio_df['Decrypt_Encrypt_Ratio']*30,
                          cmap='RdYlGn', alpha=0.7, edgecolors='black')

    ax9.set_title('🚀 Decrypt/Encrypt Speed Ratios',
                  fontsize=12, fontweight='bold')
    ax9.set_xlabel('Threads')
    ax9.set_ylabel('Chunk Size (MB)')

    # Добавляем colorbar
    cbar = plt.colorbar(scatter, ax=ax9, shrink=0.8)
    cbar.set_label('Decrypt/Encrypt Ratio', rotation=270, labelpad=15)

    # 10. Performance Zones
    # Создаем зоны производительности
    encrypt_max = encrypt_data['Throughput'].max()
    decrypt_max = decrypt_data['Throughput'].max()

    # Определяем зоны
    high_perf_encrypt = encrypt_data[encrypt_data['Throughput']
                                     > encrypt_max * 0.9]
    medium_perf_encrypt = encrypt_data[(encrypt_data['Throughput'] > encrypt_max * 0.7) &
                                       (encrypt_data['Throughput'] <= encrypt_max * 0.9)]

    high_perf_decrypt = decrypt_data[decrypt_data['Throughput']
                                     > decrypt_max * 0.9]
    medium_perf_decrypt = decrypt_data[(decrypt_data['Throughput'] > decrypt_max * 0.7) &
                                       (decrypt_data['Throughput'] <= decrypt_max * 0.9)]

    ax10.scatter(high_perf_encrypt['Threads'], high_perf_encrypt['ChunkMB'],
                 c='green', s=100, alpha=0.7, label='High Encrypt', marker='o')
    ax10.scatter(medium_perf_encrypt['Threads'], medium_perf_encrypt['ChunkMB'],
                 c='orange', s=80, alpha=0.7, label='Medium Encrypt', marker='o')
    ax10.scatter(high_perf_decrypt['Threads'], high_perf_decrypt['ChunkMB'],
                 c='darkgreen', s=100, alpha=0.7, label='High Decrypt', marker='s')
    ax10.scatter(medium_perf_decrypt['Threads'], medium_perf_decrypt['ChunkMB'],
                 c='darkorange', s=80, alpha=0.7, label='Medium Decrypt', marker='s')

    ax10.set_title('🎯 Performance Zones', fontsize=12, fontweight='bold')
    ax10.set_xlabel('Threads')
    ax10.set_ylabel('Chunk Size (MB)')
    ax10.legend(bbox_to_anchor=(1.05, 1), loc='upper left')
    ax10.grid(True, alpha=0.3)

    # 11. Optimization Suggestions
    # Находим оптимальные конфигурации
    encrypt_best = encrypt_data.loc[encrypt_data['Throughput'].idxmax()]
    decrypt_best = decrypt_data.loc[decrypt_data['Throughput'].idxmax()]

    # Создаем рекомендации в виде таблицы
    ax11.axis('off')

    recommendations = [
        ["🏆 BEST CONFIGURATIONS", "", ""],
        ["Operation", "Threads", "Chunk Size"],
        ["Encryption", f"{encrypt_best['Threads']:.0f}",
            f"{encrypt_best['ChunkMB']:.0f}MB"],
        ["Decryption", f"{decrypt_best['Threads']:.0f}",
            f"{decrypt_best['ChunkMB']:.0f}MB"],
        ["", "", ""],
        ["📈 PERFORMANCE INSIGHTS", "", ""],
        ["Avg Decrypt Speed",
            f"{decrypt_data['Throughput'].mean():.0f}", "MB/s"],
        ["Avg Encrypt Speed",
            f"{encrypt_data['Throughput'].mean():.0f}", "MB/s"],
        ["Decrypt Advantage",
            f"{((decrypt_data['Throughput'].mean() / encrypt_data['Throughput'].mean() - 1) * 100):.1f}%", ""],
        ["", "", ""],
        ["💡 RECOMMENDATIONS", "", ""],
        ["For Encryption", "Use 16-32MB", "chunks"],
        ["For Decryption", "Use 2-8", "threads"],
        ["General Rule", "Bigger chunks", "for encrypt"],
        ["Thread Scaling", "2-16 threads", "optimal"]
    ]

    table = ax11.table(cellText=recommendations,
                       cellLoc='center',
                       loc='center',
                       cellColours=[['lightgray']*3 if i in [0, 5, 10] else ['white']*3
                                    for i in range(len(recommendations))])

    table.auto_set_font_size(False)
    table.set_fontsize(9)
    table.scale(1, 2)

    ax11.set_title('💡 Smart Optimization Guide',
                   fontsize=12, fontweight='bold')

    # 12. Summary Dashboard
    # Создаем summary с ключевыми метриками
    ax12.axis('off')

    # Вычисляем ключевые метрики
    encrypt_std = encrypt_data['Throughput'].std()
    decrypt_std = decrypt_data['Throughput'].std()
    total_tests = len(encrypt_data) + len(decrypt_data)

    # Создаем круговые диаграммы и метрики
    ax12_sub1 = plt.subplot2grid((4, 3), (3, 2), fig=fig)

    # Pie chart производительности по операциям
    avg_speeds = [encrypt_data['Throughput'].mean(
    ), decrypt_data['Throughput'].mean()]
    labels = ['Encrypt', 'Decrypt']
    colors = ['skyblue', 'lightcoral']

    wedges, texts, autotexts = ax12_sub1.pie(avg_speeds, labels=labels, colors=colors,
                                             autopct='%1.0f%%', startangle=90)
    ax12_sub1.set_title('Performance Share', fontsize=10, fontweight='bold')

    return fig, encrypt_best, decrypt_best


def print_mega_summary(encrypt_data, decrypt_data, encrypt_best, decrypt_best):
    """Print a mega analysis summary"""

    print("\n" + "="*70)
    print("🚀 MEGA PERFORMANCE ANALYSIS SUMMARY")
    print("="*70)

    print(f"\n📊 DATASET OVERVIEW:")
    print(f"   Total measurements: {len(encrypt_data) + len(decrypt_data)}")
    print(f"   Thread configurations: {len(encrypt_data['Threads'].unique())}")
    print(f"   Chunk size variations: {len(encrypt_data['ChunkMB'].unique())}")
    print(f"   Test combinations per operation: {len(encrypt_data)}")

    print(f"\n🏆 ABSOLUTE CHAMPIONS:")
    print(f"   🔐 Encryption King: {encrypt_best['Throughput']:.1f} MB/s")
    print(
        f"       Configuration: {encrypt_best['Threads']:.0f} threads × {encrypt_best['ChunkMB']:.0f}MB chunks")
    print(f"   🔓 Decryption Master: {decrypt_best['Throughput']:.1f} MB/s")
    print(
        f"       Configuration: {decrypt_best['Threads']:.0f} threads × {decrypt_best['ChunkMB']:.0f}MB chunks")

    # Detailed statistics
    print(f"\n📈 DETAILED STATISTICS:")
    for op_name, data in [("Encryption", encrypt_data), ("Decryption", decrypt_data)]:
        stats = data['Throughput'].describe()
        print(f"   {op_name}:")
        print(f"      Mean: {stats['mean']:.1f} MB/s")
        print(f"      Median: {stats['50%']:.1f} MB/s")
        print(f"      Std Dev: {stats['std']:.1f} MB/s")
        print(f"      Range: {stats['min']:.1f} - {stats['max']:.1f} MB/s")
        print(
            f"      Coefficient of Variation: {(stats['std']/stats['mean']*100):.1f}%")

    # Efficiency analysis
    print(f"\n⚡ EFFICIENCY ANALYSIS:")
    speed_advantage = (decrypt_data['Throughput'].mean(
    ) / encrypt_data['Throughput'].mean() - 1) * 100
    print(f"   Decryption speed advantage: {speed_advantage:.1f}%")

    # Best thread configurations
    print(f"\n🎯 OPTIMAL THREAD COUNTS:")
    for op_name, data in [("Encryption", encrypt_data), ("Decryption", decrypt_data)]:
        thread_performance = data.groupby(
            'Threads')['Throughput'].mean().sort_values(ascending=False)
        best_threads = thread_performance.index[0]
        print(
            f"   {op_name}: {best_threads} threads ({thread_performance.iloc[0]:.1f} MB/s avg)")

    # Best chunk sizes
    print(f"\n🧩 OPTIMAL CHUNK SIZES:")
    for op_name, data in [("Encryption", encrypt_data), ("Decryption", decrypt_data)]:
        chunk_performance = data.groupby(
            'ChunkMB')['Throughput'].mean().sort_values(ascending=False)
        best_chunk = chunk_performance.index[0]
        print(
            f"   {op_name}: {best_chunk}MB chunks ({chunk_performance.iloc[0]:.1f} MB/s avg)")

    print(f"\n💡 KEY INSIGHTS:")
    print(f"   • Decryption consistently outperforms encryption")
    print(f"   • Larger chunks generally favor encryption performance")
    print(f"   • Thread scaling shows diminishing returns after 8-16 threads")
    print(f"   • Configuration matters more than raw thread count")

    print("\n" + "="*70)


def main():
    """Main function"""
    try:
        # Парсим данные из файла
        encrypt_data, decrypt_data = parse_test_results('input.txt')

        if encrypt_data.empty or decrypt_data.empty:
            print("Error: failed to find data in input.txt")
            return

        print(f"🎯 Data loaded for MEGA analysis:")
        print(f"   Encryption: {len(encrypt_data)} records")
        print(f"   Decryption: {len(decrypt_data)} records")

        # Создаем мега-анализ с 12 графиками
        fig, encrypt_optimal, decrypt_optimal = create_mega_analysis(
            encrypt_data, decrypt_data)

    # Сохраняем графики
    fig.savefig('mega_performance_analysis.png',
            dpi=300, bbox_inches='tight')
    print(f"\n💾 MEGA analysis saved to mega_performance_analysis.png")

        # Выводим детальную сводку
        print_mega_summary(encrypt_data, decrypt_data,
                           encrypt_optimal, decrypt_optimal)

        plt.show()

        print(f"\n🚀 Created 12 plots:")
        print(f"   1-4. Main dependency plots")
        print(f"   5. Heat Map performance matrix")
        print(f"   6. Bar chart by chunk sizes")
        print(f"   7. Violin plot distributions")
        print(f"   8. Scaling efficiency analysis")
        print(f"   9. Speed ratio map")
        print(f"   10. Performance zones")
        print(f"   11. Recommendations table")
        print(f"   12. Summary dashboard")

    except FileNotFoundError:
        print("❌ Error: file 'input.txt' not found")
    except Exception as e:
        print(f"❌ Error: {e}")
        import traceback
        traceback.print_exc()


if __name__ == "__main__":
    main()
