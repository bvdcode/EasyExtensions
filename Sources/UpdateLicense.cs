// SPDX-License-Identifier: MIT
// Copyright (c) 2025–2026 Vadim Belov <https://belov.us>

using System;
using System.IO;
using System.Linq;
using System.Text;

class Program
{
    private const int StartYear = 2025;
    private const string Author = "Vadim Belov <https://belov.us>";
    private const string Spdx = "MIT";

    static void Main(string[] args)
    {
        var root = args.Length > 0 ? args[0] : Directory.GetCurrentDirectory();

        Console.WriteLine($"[cotton-license] scanning: {root}");

        var files = Directory
            .GetFiles(root, "*.cs", SearchOption.AllDirectories)
            .Where(p =>
                !p.Contains(Path.DirectorySeparatorChar + "bin" + Path.DirectorySeparatorChar)
                && !p.Contains(Path.DirectorySeparatorChar + "obj" + Path.DirectorySeparatorChar)
                && !p.Contains(Path.DirectorySeparatorChar + ".git" + Path.DirectorySeparatorChar)
                && !p.EndsWith(".g.cs", StringComparison.OrdinalIgnoreCase)
                && !p.EndsWith(".Designer.cs", StringComparison.OrdinalIgnoreCase)
                && !p.EndsWith(".AssemblyInfo.cs", StringComparison.OrdinalIgnoreCase)
            )
            .ToList();

        int updated = 0;

        foreach (var file in files)
        {
            if (ProcessFile(file))
            {
                updated++;
                Console.WriteLine($"[upd] {file}");
            }
        }

        Console.WriteLine($"[cotton-license] done. updated {updated} files.");
    }

    private static bool ProcessFile(string filePath)
    {
        var currentYear = DateTime.UtcNow.Year;

        string yearPart = currentYear <= StartYear ? $"{StartYear}" : $"{StartYear}–{currentYear}";

        string wantedLine1 = $"// SPDX-License-Identifier: {Spdx}";
        string wantedLine2 = $"// Copyright (c) {yearPart} {Author}";

        var allLines = File.ReadAllLines(filePath, Encoding.UTF8).ToList();
        if (allLines.Count == 0)
        {
            var newLinesEmpty = new[] { wantedLine1, wantedLine2, "" };
            File.WriteAllLines(filePath, newLinesEmpty, Encoding.UTF8);
            return true;
        }

        bool hasSpdxHeader = allLines[0]
            .TrimStart()
            .StartsWith("// SPDX-License-Identifier:", StringComparison.Ordinal);

        if (hasSpdxHeader)
        {
            bool changed = false;

            if (allLines.Count == 1)
            {
                allLines.Add("");
            }

            if (!string.Equals(allLines[0].Trim(), wantedLine1, StringComparison.Ordinal))
            {
                allLines[0] = wantedLine1;
                changed = true;
            }

            if (!string.Equals(allLines[1].Trim(), wantedLine2, StringComparison.Ordinal))
            {
                allLines[1] = wantedLine2;
                changed = true;
            }

            if (allLines.Count < 3)
            {
                allLines.Insert(2, "");
                changed = true;
            }
            else
            {
                if (allLines[2].Trim().Length != 0)
                {
                    allLines.Insert(2, "");
                    changed = true;
                }
            }

            if (changed)
            {
                File.WriteAllLines(filePath, allLines, Encoding.UTF8);
            }

            return changed;
        }
        else
        {
            var newContent = new[] { wantedLine1, wantedLine2, "" }.Concat(allLines);

            File.WriteAllLines(filePath, newContent, Encoding.UTF8);
            return true;
        }
    }
}
