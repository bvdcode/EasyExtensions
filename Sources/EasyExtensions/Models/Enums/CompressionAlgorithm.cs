namespace EasyExtensions.Models.Enums
{
    /// <summary>
    /// Specifies the available compression algorithms for encoding or decoding data streams.
    /// </summary>
    /// <remarks>Use this enumeration to select a compression algorithm based on your requirements for speed,
    /// compression ratio, compatibility, and platform support. Some algorithms are optimized for fast compression and
    /// decompression (such as LZ4 and Snappy), while others provide higher compression ratios at the cost of speed
    /// (such as XZ and LZMA). Not all algorithms may be supported on all platforms or by all libraries. Choose the
    /// algorithm that best matches your use case, such as archival storage, web asset delivery, or real-time
    /// processing.</remarks>
    public enum CompressionAlgorithm
    {
        /// <summary>
        /// No compression. Best for already-compressed data (JPEG/PNG/MP4/ZIP) to avoid wasted CPU.
        /// </summary>
        None = 0,

        /// <summary>
        /// Raw DEFLATE stream (no container). Rare as a storage format; common inside ZIP/PNG and some protocols.
        /// </summary>
        Deflate = 1,

        /// <summary>
        /// zlib wrapper around DEFLATE (RFC 1950). Common in libraries/protocols.
        /// </summary>
        Zlib = 2,

        /// <summary>
        /// gzip container around DEFLATE (RFC 1952). Very compatible and widely supported; usually worse than Zstd for storage.
        /// </summary>
        Gzip = 3,

        /// <summary>
        /// Brotli. Great for web assets (HTML/CSS/JS); often slower to compress for mixed-content storage.
        /// </summary>
        Brotli = 4,

        /// <summary>
        /// Zstandard (Zstd). Excellent general-purpose default for storage: fast decompression + strong ratio.
        /// </summary>
        Zstd = 5,

        /// <summary>
        /// LZ4. Extremely fast compression/decompression with low latency; weaker ratio than Zstd.
        /// </summary>
        Lz4 = 6,

        /// <summary>
        /// Snappy. Very fast, common in some storage/log pipelines; typically weaker ratio than Zstd.
        /// </summary>
        Snappy = 7,

        /// <summary>
        /// LZO. Older fast codec seen in some systems; usually less popular than LZ4/Zstd today.
        /// </summary>
        Lzo = 8,

        /// <summary>
        /// bzip2. Better ratio than gzip in some cases, but slow; mostly legacy/archival.
        /// </summary>
        Bzip2 = 9,

        /// <summary>
        /// XZ (LZMA2 in .xz container). Very high ratio, but slow and high-latency; archival, not “on-the-fly”.
        /// </summary>
        Xz = 10,

        /// <summary>
        /// LZMA (classic). High ratio, very slow; mostly archival/legacy.
        /// </summary>
        Lzma = 11,

        /// <summary>
        /// LZFSE. Apple codec (iOS/macOS). Fast-ish with decent ratio; mostly relevant on Apple ecosystems.
        /// </summary>
        Lzfse = 12,

        /// <summary>
        /// Zopfli (deflate-optimizer). Extremely slow compression for slightly smaller gzip/deflate; offline packaging only.
        /// </summary>
        Zopfli = 13
    }
}
