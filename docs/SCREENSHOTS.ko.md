[日本語](SCREENSHOTS.md) | [English](SCREENSHOTS.en.md) | [简体中文](SCREENSHOTS.zh-CN.md) | [繁體中文（台灣）](SCREENSHOTS.zh-TW.md) | [한국어](SCREENSHOTS.ko.md) | [Deutsch](SCREENSHOTS.de.md) | [Русский](SCREENSHOTS.ru.md) | [Español](SCREENSHOTS.es.md) | [Svenska](SCREENSHOTS.sv.md)

# 화면 예시와 다시 생성하기

v0.0.11의 WPF 컨트롤로 가상 메모를 460 × 580 px PNG로 렌더링합니다. 실제 데스크톱 캡처나 Obsidian/Google 연결 화면이 아닙니다. 개인 메모, 설정, 인증 정보는 사용하지 않습니다.

9개 UI 언어를 구현한 뒤 본문과 UI 언어를 맞춘 이미지 27장을 다시 생성했습니다. 대만 중국어는 번체(zh-TW)를 사용합니다. 데일리 파일 이름은 생성 날짜입니다.

| Language | Tasks | Markdown | Daily |
| --- | --- | --- | --- |
| 日本語 | [![日本語 tasks](images/sticky-tasks-ja.png)](images/sticky-tasks-ja.png) | [![日本語 markdown](images/sticky-markdown-ja.png)](images/sticky-markdown-ja.png) | [![日本語 daily](images/sticky-daily-ja.png)](images/sticky-daily-ja.png) |
| English | [![English tasks](images/sticky-tasks-en.png)](images/sticky-tasks-en.png) | [![English markdown](images/sticky-markdown-en.png)](images/sticky-markdown-en.png) | [![English daily](images/sticky-daily-en.png)](images/sticky-daily-en.png) |
| 简体中文 | [![简体中文 tasks](images/sticky-tasks-zh-CN.png)](images/sticky-tasks-zh-CN.png) | [![简体中文 markdown](images/sticky-markdown-zh-CN.png)](images/sticky-markdown-zh-CN.png) | [![简体中文 daily](images/sticky-daily-zh-CN.png)](images/sticky-daily-zh-CN.png) |
| 繁體中文（台灣） | [![繁體中文（台灣） tasks](images/sticky-tasks-zh-TW.png)](images/sticky-tasks-zh-TW.png) | [![繁體中文（台灣） markdown](images/sticky-markdown-zh-TW.png)](images/sticky-markdown-zh-TW.png) | [![繁體中文（台灣） daily](images/sticky-daily-zh-TW.png)](images/sticky-daily-zh-TW.png) |
| 한국어 | [![한국어 tasks](images/sticky-tasks-ko.png)](images/sticky-tasks-ko.png) | [![한국어 markdown](images/sticky-markdown-ko.png)](images/sticky-markdown-ko.png) | [![한국어 daily](images/sticky-daily-ko.png)](images/sticky-daily-ko.png) |
| Deutsch | [![Deutsch tasks](images/sticky-tasks-de.png)](images/sticky-tasks-de.png) | [![Deutsch markdown](images/sticky-markdown-de.png)](images/sticky-markdown-de.png) | [![Deutsch daily](images/sticky-daily-de.png)](images/sticky-daily-de.png) |
| Русский | [![Русский tasks](images/sticky-tasks-ru.png)](images/sticky-tasks-ru.png) | [![Русский markdown](images/sticky-markdown-ru.png)](images/sticky-markdown-ru.png) | [![Русский daily](images/sticky-daily-ru.png)](images/sticky-daily-ru.png) |
| Español | [![Español tasks](images/sticky-tasks-es.png)](images/sticky-tasks-es.png) | [![Español markdown](images/sticky-markdown-es.png)](images/sticky-markdown-es.png) | [![Español daily](images/sticky-daily-es.png)](images/sticky-daily-es.png) |
| Svenska | [![Svenska tasks](images/sticky-tasks-sv.png)](images/sticky-tasks-sv.png) | [![Svenska markdown](images/sticky-markdown-sv.png)](images/sticky-markdown-sv.png) | [![Svenska daily](images/sticky-daily-sv.png)](images/sticky-daily-sv.png) |

## 다시 생성하기(개발자)

Windows와 .NET 8 SDK에서 저장소 루트에서 실행합니다. 로컬 SDK라면 dotnet을 .tools/dotnet/dotnet.exe로 바꿉니다. 이미지 27장의 줄바꿈, 잘림, 누락된 글자를 확인하고 9개 언어 설명을 함께 갱신하세요.

```powershell
./build.ps1
dotnet run --project tests/StickyNotes.Tests -c Release --no-build -- --render-screenshots examples/screenshots docs/images
```

## 앱에서 시험하기

Language는 ja / en / zh-CN / zh-TW / ko / de / ru / es / sv를 지원합니다. 실행할 때마다 artifacts에 독립 데이터 폴더를 만들고 선택한 UI 언어와 CLI 비활성화를 설정합니다. 데모를 열기 전에 실행 중인 앱을 저장하고 종료하세요.

```powershell
$demoData = ./scripts/Prepare-ScreenshotDemo.ps1 -Language ko
& ./artifacts/app/StickyNotes.exe --data-dir $demoData
```

[日本語](../examples/screenshots/Tasks.md) · [English](../examples/screenshots/en/Tasks.md) · [简体中文](../examples/screenshots/zh-CN/Tasks.md) · [繁體中文（台灣）](../examples/screenshots/zh-TW/Tasks.md) · [한국어](../examples/screenshots/ko/Tasks.md) · [Deutsch](../examples/screenshots/de/Tasks.md) · [Русский](../examples/screenshots/ru/Tasks.md) · [Español](../examples/screenshots/es/Tasks.md) · [Svenska](../examples/screenshots/sv/Tasks.md)

이전 JPEG와 compact-body-v0.0.9.png는 과거 참고 자료로 유지합니다. README에는 새 PNG를 사용합니다.
