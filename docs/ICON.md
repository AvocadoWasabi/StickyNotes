# アプリアイコン

2026-10-05に内蔵の画像生成ツールで作成しました。黄色と青の付箋、折り返した角、紺色の筆記線とチェックマークをモチーフにしています。

- 元画像: `docs/images/StickyNotes.png`（透過PNG）
- Windows用: `src/StickyNotes/Assets/StickyNotes.ico`（16 / 20 / 24 / 32 / 40 / 48 / 64 / 128 / 256 px）
- 用途: EXE、ウィンドウ、通知領域、インストール時のショートカット
- 変換: `scripts/Convert-AppIcon.ps1`（元の透過を保持したサイズ・形式変換）

## 生成プロンプト

```text
Use case: logo-brand. Asset type: Windows desktop application icon for Markdown Sticky Notes. Create one polished, friendly, extremely legible app icon: a warm yellow square sticky note with gently rounded corners and a folded lower-right corner, sitting in front of one slightly offset pale blue note. On the yellow note place two bold dark navy horizontal writing strokes and one simple dark navy check mark. Clean flat illustration with restrained soft depth, crisp silhouette, no tiny details, centered large with modest transparent padding, square 1024x1024 composition. Must work at 16px and 32px. Genuine transparent background. No words, letters, logos, watermark, border frame, or surrounding objects.
```
