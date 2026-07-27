# ActionFit Match Rival UI

`com.actionfit.match-rival`을 위한 프로젝트 중립 UI Foundation presentation과 독립 실행 bootstrap입니다. Cat Merge Cafe의 실제 production 프리팹과 이미지는 `Assets/_Project/Content/MatchRival`이 소유합니다.

```json
"com.actionfit.match-rival.ui": "https://github.com/ActionFit-Editor/MatchRivalUI.git#0.4.2"
```

## 독립 실행 흐름

`Tools > Package > ActionFit Match Rival UI > Create Demo`를 실행하거나 GameObject에 `MatchRivalBootstrap`을 추가합니다. 생성 view는 엔진 흐름 진단용이며 Cat production UI를 대체하지 않습니다.

## 자산 경계

- Cat production UI는 로컬 `Assets/_Project/Content/MatchRival` 프리팹과 이미지를 사용합니다.
- 패키지에는 현재 참조되는 shared Indicator, Resources 아이콘과 그 종속 리소스만 남습니다.
- 삭제된 production prefab/image baseline을 패키지에 다시 복사하지 않습니다.

## 연동 계약

- `MatchRivalUIViewModelFactory`는 엔진 조회 결과를 불변 UI 데이터로 복사합니다.
- `Refs` 필드는 private 직렬화 하위 Component이며 `RequiredReference`와 `AutoWireChild` 계약을 사용합니다.
- 일정, 진행도, 결과, 영속 상태와 보상 상태는 엔진이 소유합니다.
- DOTween, UniTask, Addressables, `Main`, 프로젝트 현지화와 `Assembly-CSharp` 의존성을 추가하지 않습니다.

`0.4.2`는 실제 게임에서 참조하지 않는 패키지 production 프리팹과 중복 이미지를 제거하고 런타임 참조가 확인된 shared dependency만 보존합니다.
