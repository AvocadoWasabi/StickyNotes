[日本語](SCREENSHOTS.md) | [English](SCREENSHOTS.en.md) | [简体中文](SCREENSHOTS.zh-CN.md) | [繁體中文](SCREENSHOTS.zh-TW.md) | [한국어](SCREENSHOTS.ko.md)

# 화면 예시와 생성 방법

현재 개발 버전의 WPF 컨트롤로 가상 예시를 460 × 580픽셀 PNG로 렌더링합니다. 바탕 화면 캡처나 실제 Obsidian／Google 연결 화면이 아닙니다. 개인 노트·설정·인증 정보는 사용하지 않습니다.

일본어·영어·중국어 간체는 본문과 UI 언어가 같습니다. 중국어 번체·한국어는 본문만 번역하며 영어 UI를 사용합니다. UI 언어가 추가된 것은 아닙니다. 데일리 파일 이름의 날짜는 생성 날짜입니다.

| Language / 言語 / 语言 | Tasks | Markdown | Daily |
| --- | --- | --- | --- |
| 日本語 | [![日本語 tasks](images/sticky-tasks-ja.png)](images/sticky-tasks-ja.png) | [![日本語 markdown](images/sticky-markdown-ja.png)](images/sticky-markdown-ja.png) | [![日本語 daily](images/sticky-daily-ja.png)](images/sticky-daily-ja.png) |
| English | [![English tasks](images/sticky-tasks-en.png)](images/sticky-tasks-en.png) | [![English markdown](images/sticky-markdown-en.png)](images/sticky-markdown-en.png) | [![English daily](images/sticky-daily-en.png)](images/sticky-daily-en.png) |
| 简体中文 | [![简体中文 tasks](images/sticky-tasks-zh-CN.png)](images/sticky-tasks-zh-CN.png) | [![简体中文 markdown](images/sticky-markdown-zh-CN.png)](images/sticky-markdown-zh-CN.png) | [![简体中文 daily](images/sticky-daily-zh-CN.png)](images/sticky-daily-zh-CN.png) |
| 繁體中文 | [![繁體中文 tasks](images/sticky-tasks-zh-TW.png)](images/sticky-tasks-zh-TW.png) | [![繁體中文 markdown](images/sticky-markdown-zh-TW.png)](images/sticky-markdown-zh-TW.png) | [![繁體中文 daily](images/sticky-daily-zh-TW.png)](images/sticky-daily-zh-TW.png) |
| 한국어 | [![한국어 tasks](images/sticky-tasks-ko.png)](images/sticky-tasks-ko.png) | [![한국어 markdown](images/sticky-markdown-ko.png)](images/sticky-markdown-ko.png) | [![한국어 daily](images/sticky-daily-ko.png)](images/sticky-daily-ko.png) |

## 다시 생성하기(개발자)

Windows의 .NET 8 SDK로 저장소 루트에서 실행합니다. 로컬 SDK를 사용하면 `dotnet`을 `.tools/dotnet/dotnet.exe`로 바꾸세요. 이미지 15개를 생성합니다. 모든 이미지의 줄바꿈·잘림·누락된 글자를 확인하고 최소 3개 참고 언어의 설명을 함께 갱신하세요.

```powershell
./build.ps1
dotnet run --project tests/StickyNotes.Tests -c Release --no-build -- --render-screenshots examples/screenshots docs/images
```

## 앱에서 실행해 보기

Language는 ja / en / zh-CN / zh-TW / ko를 지원합니다. 매번 artifacts에 새 데이터 폴더를 만들고 독립 설정과 비활성화된 CLI를 사용합니다. 앱이 실행 중이면 저장 후 종료하고 예시를 실행하세요.

```powershell
$demoData = ./scripts/Prepare-ScreenshotDemo.ps1 -Language ko
& ./artifacts/app/StickyNotes.exe --data-dir $demoData
```

[日本語](../examples/screenshots/Tasks.md) · [English](../examples/screenshots/en/Tasks.md) · [简体中文](../examples/screenshots/zh-CN/Tasks.md) · [繁體中文](../examples/screenshots/zh-TW/Tasks.md) · [한국어](../examples/screenshots/ko/Tasks.md)

기존 JPEG와 compact-body-v0.0.9.png는 이전 버전 참고용으로 유지합니다. README는 새로 생성한 PNG를 사용합니다.
