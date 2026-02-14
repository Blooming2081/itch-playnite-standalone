# Itch.io Standalone Plugin for Playnite

Antigravity IDE를 사용하여 개발된 Playnite용 itch.io 독립형 라이브러리 플러그인입니다. 
공식 데스크톱 앱 없이 `butler` CLI를 통해 게임 동기화, 설치 및 업데이트를 처리합니다.

## 핵심 기능
- **데스크톱 앱 미필요**: itch.io API와 Butler를 직접 연동합니다.
- **포터블 최적화**: Butler 실행 파일 경로를 사용자가 직접 지정할 수 있습니다.
- **증분 업데이트**: Butler의 Pachinko 알고리즘을 활용한 빠른 업데이트를 지원합니다.
- **GOG OSS/Legendary 스타일 UI**: 탭 기반의 직관적인 설정 화면을 제공합니다.

## 개발 도구
- **Framework**: .NET Framework 4.6.2
- **Engine**: Butler CLI (itch.io official tool)
- **Library**: Playnite SDK, CliWrap, Newtonsoft.Json