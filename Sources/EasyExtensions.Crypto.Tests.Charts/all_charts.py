import os
import sys
import argparse
import importlib
import logging
from pathlib import Path

# Ensure non-interactive backend for matplotlib in child modules before they import pyplot
os.environ.setdefault("MPLBACKEND", "Agg")

ROOT = Path(__file__).parent.resolve()
INPUT_DEFAULT = ROOT / "input.txt"


def _run_module_main(module_name: str) -> None:
    """Import module, patch plt.show to no-op, then call its main()."""
    mod = importlib.import_module(module_name)
    # Patch plt.show to avoid GUI blocking
    if hasattr(mod, "plt") and hasattr(mod.plt, "show"):
        try:
            mod.plt.show = lambda *args, **kwargs: None  # type: ignore[attr-defined]
        except Exception as exc:  # pragma: no cover - defensive logging only
            logging.debug("Failed to patch plt.show for %s: %s", module_name, exc)
    # Call module main()
    if hasattr(mod, "main"):
        mod.main()
    else:
        raise RuntimeError(f"Module '{module_name}' has no main() function")


def generate_simple():
    print("\n=== ✅ Generating simple charts ===")
    _run_module_main("simple_charts")


def generate_advanced():
    print("\n=== ✅ Generating advanced charts ===")
    _run_module_main("advanced_analysis")


def generate_mega():
    print("\n=== ✅ Generating MEGA charts ===")
    _run_module_main("mega_advanced_analysis")


def run_selected(sets: list[str]):
    # Always run from script directory so child scripts find input.txt relative to this file
    os.chdir(ROOT)

    # Basic presence check for input
    if not INPUT_DEFAULT.exists():
        print(f"❌ Data file not found: {INPUT_DEFAULT}")
        sys.exit(1)

    for s in sets:
        if s == "simple":
            generate_simple()
        elif s == "advanced":
            generate_advanced()
        elif s == "mega":
            generate_mega()
        else:
            raise ValueError(f"Unknown set: {s}")

    print("\n🎉 Done. Files created (if enough data):")
    print("  • performance_charts.png")
    print("  • advanced_performance_analysis.png")
    print("  • mega_performance_analysis.png")


def interactive_menu() -> list[str]:
    print("\nWhat to generate? Choose an option and press Enter:")
    print("  1) Simple charts only")
    print("  2) Advanced charts only")
    print("  3) MEGA charts only")
    print("  4) All at once")

    choice = input("> ").strip()
    mapping = {
        "1": ["simple"],
        "2": ["advanced"],
        "3": ["mega"],
        "4": ["simple", "advanced", "mega"],
    }
    return mapping.get(choice, ["simple", "advanced", "mega"])  # default — all


def parse_args(argv: list[str]):
    p = argparse.ArgumentParser(description="Generate performance charts (simple/advanced/mega)")
    g = p.add_mutually_exclusive_group()
    g.add_argument("--simple", action="store_true", help="Generate simple charts only")
    g.add_argument("--advanced", action="store_true", help="Generate advanced charts only")
    g.add_argument("--mega", action="store_true", help="Generate MEGA charts only")
    g.add_argument("--all", action="store_true", help="Generate all charts (default)")
    p.add_argument("--menu", action="store_true", help="Show interactive selection menu")
    return p.parse_args(argv)


def main(argv: list[str] | None = None):
    args = parse_args(sys.argv[1:] if argv is None else argv)

    if args.menu:
        sets = interactive_menu()
    else:
        if args.simple:
            sets = ["simple"]
        elif args.advanced:
            sets = ["advanced"]
        elif args.mega:
            sets = ["mega"]
        else:
            # --all или по умолчанию
            sets = ["simple", "advanced", "mega"]

    run_selected(sets)


if __name__ == "__main__":
    main()
