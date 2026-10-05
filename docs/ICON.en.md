[English](ICON.en.md) | [日本語](ICON.md) | [简体中文](ICON.zh-CN.md)

# App icon

Created on 2026-10-05 with the built-in image generation tool. The design uses yellow and blue sticky notes, a folded corner, navy writing strokes, and a check mark.

- Source: `docs/images/StickyNotes.png` (transparent PNG)
- Windows: `src/StickyNotes/Assets/StickyNotes.ico` (16 / 20 / 24 / 32 / 40 / 48 / 64 / 128 / 256 px)
- Used for: executable, windows, notification area, and installed shortcut
- Conversion: `scripts/Convert-AppIcon.ps1` (resizes and converts while preserving transparency)

## Original generation prompt

```text
Use case: logo-brand. Asset type: Windows desktop application icon for Markdown Sticky Notes. Create one polished, friendly, extremely legible app icon: a warm yellow square sticky note with gently rounded corners and a folded lower-right corner, sitting in front of one slightly offset pale blue note. On the yellow note place two bold dark navy horizontal writing strokes and one simple dark navy check mark. Clean flat illustration with restrained soft depth, crisp silhouette, no tiny details, centered large with modest transparent padding, square 1024x1024 composition. Must work at 16px and 32px. Genuine transparent background. No words, letters, logos, watermark, border frame, or surrounding objects.
```
