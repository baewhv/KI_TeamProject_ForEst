using System;
using UnityEngine;

namespace CutScene
{
    public enum EActions
    {
        None,
        [InspectorName("캐릭터 이동")]CharacterMove,
        [InspectorName("캐릭터 방향")]CharacterDirection,
        [InspectorName("캐릭터 반전")]CharacterReverse,
        [InspectorName("캐릭터 애니메이션")]CharacterPlayAnimation,
        [InspectorName("대사 출력")]PlayDialog,
        [InspectorName("페이드 인")]FadeIn,
        [InspectorName("페이드 아웃")]FadeOut,
        [InspectorName("배경 이미지")]BackgroundImage,
        [InspectorName("카메라 대상 추적")]CameraSetTarget,
        [InspectorName("카메라 이동")]CameraMove,
        [InspectorName("카메라 줌")]CameraZoom,
        [InspectorName("딜레이")]Delay,
        [InspectorName("캐릭터 페이드")]CharacterFader,
        Max
    }

    
    /// <summary>
    /// 다음 연출을 어떻게 실행할지에 대한 열거형.
    /// <para>NeedInput - 연출 종료 후 입력 대기</para>
    /// <para>Immediate - 연출 끝나면 바로 이어서</para>
    /// <para>Together - 다음 연출도 바로 실행.</para>
    /// </summary>
    public enum ENextActionType
    {
        [InspectorName("키 입력 필요")] NeedInput,
        [InspectorName("종료 후 이어서")] Immediate, 
        [InspectorName("다음 연출과 함께")] Together,
    }

    public enum ESelectedCharacter
    {
        [InspectorName("에스트")]Est,
        [InspectorName("시드")]Seed,
        [InspectorName("시드콩")]Seed_B
    }
}