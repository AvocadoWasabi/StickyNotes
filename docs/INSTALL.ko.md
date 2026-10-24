[日本語](INSTALL.md) | [English](INSTALL.en.md) | [简体中文](INSTALL.zh-CN.md) | [繁體中文（台灣）](INSTALL.zh-TW.md) | [한국어](INSTALL.ko.md) | [Deutsch](INSTALL.de.md) | [Русский](INSTALL.ru.md) | [Español](INSTALL.es.md) | [Svenska](INSTALL.sv.md)

# 설치 및 초기 설정

[README로 돌아가기](../README.ko.md) · [상세 사용 안내(영어)](USAGE.en.md)

## 1. 설치

Windows 10 / 11(x64)용입니다. 관리자 권한이나 별도의 .NET 설치가 필요하지 않습니다.

1. [Releases](https://github.com/AvocadoWasabi/StickyNotes/releases/latest)에서 **StickyNotes-win-x64.zip**을 받습니다. `Source code`는 개발용입니다.
2. 전체 압축을 풀고 `Install.cmd`를 실행합니다.
3. 시작 메뉴에서 **Markdown Sticky Notes**를 실행합니다.

설치 없이 쓰려면 압축을 푼 폴더의 `StickyNotes.exe`를 실행하고 폴더 전체를 유지하세요. 앱이 서명되지 않아 Windows 확인 메시지가 나올 수 있습니다. 자동 시작은 등록하지 않습니다.

이 문서는 v0.0.11 기준입니다. 여러 보관함, Tasks 8.2.2 지원, 9개 UI 언어 변경은 [변경 기록(영어)](../CHANGELOG.md#v0-0-11)을 참고하세요.

## 2. 저장 위치와 연결 방식

### Obsidian CLI 사용(기본값)

1. Obsidian **설정 → 일반 → Command line interface**를 켜고 등록 안내를 따릅니다. 항목이 없으면 [공식 CLI 안내](https://obsidian.md/help/cli)의 설치 프로그램 업데이트 조건을 확인하세요.
2. 대상 보관함을 열고 Obsidian을 계속 실행합니다.
3. 메모의 **설정 → 일반**에서 `Obsidian.com`의 절대 경로를 확인합니다. 일반적으로 `C:\Program Files\Obsidian\Obsidian.com`이며, 다른 위치에 설치했다면 수정합니다.
4. **보관함 및 데일리 노트**에서 선택한 항목의 CLI 사용을 켭니다. Obsidian 보관함 이름／ID를 입력하고 **CLI로 보관함 폴더 가져오기**를 누릅니다. 비워 두면 Obsidian 기본 선택을 사용하며, 입력만으로는 가져오지 않습니다.
5. 가져온 보관함과 데일리 설정을 확인하고 **메모 저장 폴더 선택…**로 저장 위치를 선택한 뒤 **저장 후 닫기**를 누릅니다.

폴더를 바꿀 때 Yes는 기존 앱 생성 메모도 이동하고, No는 이후 저장 위치만 바꾸며, Cancel은 취소합니다. [이동 범위와 복구(영어)](STICKY-FOLDERS.md#english). CLI 실패 시 로컬 읽기로 자동 전환하지 않습니다.

### 로컬 파일 사용

**Obsidian CLI 사용(기본값)**를 끄고 로컬 기준 폴더와 메모 저장 폴더를 선택한 뒤 저장합니다. 초기 위치는 `Documents/StickyNotesData`입니다. Tasks／Dataview 검색은 사용할 수 없습니다. 로컬 데일리 노트는 폴더와 날짜 캡처 정규식으로 설정할 수 있습니다. [데일리 설정 예시(영어)](USAGE.en.md#daily).

## 3. 선택적 연동 기능

| 연동 | Obsidian／Google에서 | 메모 앱에서 |
| --- | --- | --- |
| 데일리 노트 | 코어 Daily notes를 켜고 폴더／날짜 형식을 설정한 뒤 오늘 노트 생성 | `… → 만들기 및 열기 → 데일리 노트 표시…`에서 제목 선택. 비우면 전체 표시 |
| Tasks | 대상 보관함에서 Tasks 활성화. v0.0.11은 정식 8.2.2 이상 필요 | `tasks` 코드 블록이 있는 노트 열기. [방법과 제한(영어)](TASKS-CLI-PREVIEW.md#english) |
| Dataview | 대상 보관함에서 Dataview 활성화 | `dataview`의 LIST / TABLE / TASK 표시. [방법과 제한(영어)](DATAVIEW.md#english) |
| Google | 본인 Cloud 프로젝트에 Calendar API·Tasks API·데스크톱 OAuth 설정 | JSON을 가져와 인증한 뒤 별도 문단에 `@calendar today` 입력. [연결 안내(영어)](GOOGLE-CALENDAR.en.md#setup) |

일반 Markdown 체크박스에는 Tasks가 필요하지 않습니다. 할 일 자동 완성은 기본적으로 자동이며, Tasks가 꺼져 있으면 수동으로 켜야 작동합니다. Google 연동을 쓰지 않으면 Google 인증은 필요하지 않습니다.

## 4. 메모 생성과 편집

`… → 만들기 및 열기`에서 Markdown을 만들거나 엽니다. 본문 클릭／`Ctrl+E`로 편집하고 `Ctrl+S`／`Esc`로 저장합니다. 데일리 연결은 처음에 오늘 파일이 필요하며 자동 생성하지 않습니다. 설정에서 표시 언어를 선택하고 저장한 뒤 재시작하세요. v0.0.11 UI는 README의 9개 언어를 지원하며 한국어 예시는 한국어 UI를 사용합니다.

## 문제 해결

| 증상 | 확인할 내용 |
| --- | --- |
| CLI 연결 실패 | Obsidian 실행 여부, CLI 활성화, `Obsidian.com` 절대 경로와 보관함 이름／ID를 확인하고 가져오기 버튼 재실행 |
| 오늘 노트가 안 보임 | 코어 Daily notes 설정과 오늘 파일 확인. 다른 데일리 플러그인의 설정은 자동 추론하지 않음 |
| Tasks 업데이트 안내 | 대상 보관함의 커뮤니티 플러그인에서 8.2.2 이상으로 업데이트한 뒤 메모의 Tasks / Dataview 새로 고침 |
| 저장 충돌 | 작성 내용을 보관하고 다시 불러오기. 오래된 내용으로 덮어쓰는 것을 막는 동작임 |
| Google 인증 실패 | [단계별 안내(영어)](GOOGLE-CALENDAR.en.md)에서 API·OAuth·테스트 사용자·권한 확인 |

## 업데이트, 제거와 데이터

**업데이트:** 수정 내용을 저장하고 알림 영역에서 앱을 종료합니다. 새 ZIP을 다른 폴더에 풀고 `Install.cmd`를 실행합니다. 노트·설정·백업은 앱과 별도로 저장됩니다.

**v0.0.6 이하에서 업데이트:** CLI 선택이 없는 기존 설정도 CLI가 켜집니다. 로컬 모드를 유지하려면 끄고 저장하세요. 파일은 자동 이동하지 않습니다. v0.0.1의 사용자 지정 데일리 형식은 수동 수정이 필요할 수 있습니다.

| 데이터 | 위치 |
| --- | --- |
| 설치된 앱 | `%LOCALAPPDATA%/Programs/MarkdownStickyNotes` |
| 설정과 인증 정보 | `%LOCALAPPDATA%/StickyNotes` |
| 쓰기 전 백업 | `%LOCALAPPDATA%/StickyNotes/backups` |
| 노트 | 선택한 보관함／폴더 |

**제거:** 앱을 종료하고 설치 폴더와 시작 메뉴 바로 가기를 삭제합니다. 노트·설정·백업은 남으므로 필요한 내용을 확인한 뒤 별도로 삭제하세요.
