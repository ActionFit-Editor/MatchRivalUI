# ActionFit Match Rival UI

`com.actionfit.match-rival`을 위한 선택형 프로젝트 중립 UI 계층입니다. UI Foundation 컴포넌트로 불변 엔진 스냅샷을 표시하고 입력을 공개 엔진 명령으로 다시 전달합니다. 일정, 진행도, 결과, 영속 상태와 보상 상태는 엔진만 소유합니다.

## 설치

각 저장소와 카탈로그 행이 publish된 후 공개 패키지를 함께 설치합니다.

```json
{
  "dependencies": {
    "com.actionfit.match-rival": "https://github.com/ActionFit-Editor/MatchRival.git#0.1.3",
    "com.actionfit.match-rival.ui": "https://github.com/ActionFit-Editor/MatchRivalUI.git#0.1.6"
  }
}
```

이 패키지는 Content Core 0.2.1, Match Rival 0.1.3, ReferenceBinding 0.1.2, UI Foundation 1.0.5와 UGUI 2.0.0에 직접 의존합니다.

## 독립 실행 흐름

`Tools/Package/ActionFit Match Rival UI/Create Demo`를 실행하거나 GameObject에 `MatchRivalBootstrap`을 추가합니다. bootstrap은 안전한 PlayerPrefs 서비스와 활성 데모 일정을 제공합니다. Cat Merge 에셋이나 서비스 없이 이벤트 시작, 튜토리얼, 라이벌 매칭, 콩 진행도, 승리/패배 보상, 상자 보상과 이벤트 종료를 확인할 수 있습니다.

## 연동 계약

- `MatchRivalUIViewModelFactory`를 사용해 공개 엔진 조회 결과를 불변 UI 데이터로 복사합니다.
- 현지화, 오디오, 프로필, 보상 표시, 애니메이션, 시계 표시와 view host 서비스는 좁은 인터페이스를 통해 주입합니다.
- 테마 에셋은 선택 사항입니다. Cat Merge 스프라이트, 오디오, 폰트, 머티리얼, confetti, 운영 프리팹, Addressables와 밸런스 에셋은 프로젝트가 소유합니다.
- `Refs` 필드는 private 직렬화 하위 Component입니다. 모든 필드는 안정적인 `RequiredReference` 코드와 정확한 이름의 `AutoWireChild`를 사용합니다.
- AutoWire는 Editor 제작 보조 기능입니다. 런타임 프레젠테이션은 이름으로 자식을 검색하지 않습니다.
- CI와 audit은 ReferenceBinding 읽기 전용 검증만 사용하며 참조를 적용하거나 저장하면 안 됩니다.

이 패키지는 의도적으로 DOTween, UniTask, Addressables, `Main`, 프로젝트 현지화, 프로젝트 사운드, 프로젝트 프로필, 프로젝트 인벤토리 또는 `Assembly-CSharp` 의존성을 갖지 않습니다.

## 검증 및 릴리스

EditMode 어셈블리 `com.actionfit.match-rival.ui.Editor.Tests`, Custom Package Manager 계약 검증기와 ReferenceBinding 읽기 전용 검증을 실행합니다. 저장소 생성, 태그, 카탈로그 행 및 publish는 각각 별도로 승인하는 수동 단계입니다.
