// 원작자 : Aljosha — https://discussions.unity.com/t/post-processing-with-multiple-cameras-is-currently-very-problematic/822011/268
// 수정 : xrmasiso — https://www.youtube.com/xrmasiso
//
// 원본 스크립트는 framebufferfetch를 활용해 두 패스를 단일 blit 연산으로 합칠 수 있음.
// 이 스크립트에서는 설명 단순화를 위해 해당 기능을 제거함.
// Unity 공식 예제 및 URP 핸드북에서 framebufferfetch 사용을 권장하므로,
// 프로덕션 사용 시 원본 스크립트 참고 권장.
// URP 핸드북 : https://unity.com/resources/introduction-to-urp-advanced-creators-unity-6

using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;
using System.Collections.Generic;
using UnityEngine.Rendering.RenderGraphModule.Util;
using UnityEngine.Experimental.Rendering;

public class PushPopLayerRenderFeature : ScriptableRendererFeature
{
    // 렌더 파이프라인에 패스를 삽입하려면 RenderPassEvent가 필요함.
    // 인스펙터 드롭다운에서 Push와 Pop 각각의 '삽입 시점(Injection Point)'을 설정함.
    public RenderPassEvent push = RenderPassEvent.AfterRenderingPostProcessing;
    public RenderPassEvent pop  = RenderPassEvent.AfterRenderingPostProcessing + 49;

    public Material blendMaterial;

    // 텍스처 핸들 참조를 저장하기 위한 컨텍스트 컨테이너.
    // Push에서 스냅샷을 저장하고, Pop에서 꺼내 합성에 사용함.
    public class StackLayers : ContextItem
    {
        // 텍스처 참조 스택.
        public Stack<TextureHandle> layers = new();

        // ContextItem 필수 구현 : 매 프레임 리셋.
        // 텍스처 핸들은 해당 프레임에서만 유효하므로 반드시 초기화해야 함.
        public override void Reset()
        {
            layers.Clear();
        }
    }

    // ─────────────────────────────────────────────────────────────
    // PUSH 패스 : 스냅샷 저장 및 새 렌더 타겟 생성
    // ─────────────────────────────────────────────────────────────
    // Push 삽입 시점까지 렌더링된 결과를 스냅샷으로 저장하고,
    // 이후 렌더링이 그려질 새로운 빈 캔버스(렌더 타겟)를 생성함.
    class PushLayerRenderPass : ScriptableRenderPass
    {
        public Material blendMaterial { get; set; }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            UniversalResourceData resourcesData = frameData.Get<UniversalResourceData>();

            // 렌더 그래프 뷰어에 시각적으로 표시하기 위한 디버그용 blit 패스.
            // 실제 기능에는 불필요하며 성능 비용이 발생하므로 프로덕션에서는 제거 권장.
            if (blendMaterial != null)
            {
                RenderGraphUtils.BlitMaterialParameters blitParams = new(resourcesData.cameraColor, resourcesData.cameraColor, blendMaterial, 0);
                renderGraph.AddBlitPass(blitParams, "Push : 스냅샷 저장");
            }

            var layers = frameData.GetOrCreate<StackLayers>();

            // 현재 카메라 컬러 버퍼의 참조를 스택에 저장. (복사가 아닌 참조)
            layers.layers.Push(resourcesData.cameraColor);

            // 새 렌더 타겟 생성 : 이후 모든 렌더링은 이 텍스처에 그려짐.
            TextureDesc desc;
            desc             = resourcesData.cameraColor.GetDescriptor(renderGraph);
            desc.clearBuffer = true;
            desc.clearColor  = new Color(0.0f, 0.0f, 0.0f, 1.0f); // 검정(alpha=1) : Additive 블렌딩에 최적화
            desc.name        = "_NewRenderTarget_" + layers.layers.Count;  // 렌더 그래프 뷰어에서 확인 가능

            var layerColor = renderGraph.CreateTexture(desc);

            // 새 렌더 타겟을 활성 컬러 버퍼로 설정.
            // 이 시점부터 카메라가 렌더링하는 모든 내용은 새 텍스처에 기록됨.
            resourcesData.cameraColor = layerColor;

            // Alpha Processing 체크박스 미설정 시 경고.
            if (!GraphicsFormatUtility.HasAlphaChannel(desc.format))
            {
                Debug.LogWarning("[PushPopLayerRenderFeature] 알파 채널 없음 : Alpha Processing을 활성화하세요. 블렌딩 시 전체 화면이 덮어씌워집니다.");
            }
        }
    }

    // ─────────────────────────────────────────────────────────────
    // POP 패스 : 저장된 스냅샷과 현재 렌더 결과 합성
    // ─────────────────────────────────────────────────────────────
    // Push에서 저장한 스냅샷과 이후 렌더링된 Overlay 결과를
    // Blend Material로 합성하여 최종 출력을 생성함.
    class PopLayerRenderPass : ScriptableRenderPass
    {
        public Material blendMaterial { get; set; }

        protected string m_FBFKeyword = "_USE_FBF";

        public PopLayerRenderPass()
        {
            profilingSampler = new ProfilingSampler("Pop : 스냅샷 합성");
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            if (blendMaterial != null)
            {
                UniversalResourceData resourcesData = frameData.Get<UniversalResourceData>();
                var layers = frameData.GetOrCreate<StackLayers>();

                if (layers.layers.Count > 0)
                {
                    var previousLayer = layers.layers.Pop(); // 저장된 스냅샷 참조 꺼내기

                    // framebufferfetch 기능은 이 스크립트에서 제거됨. 반드시 비활성화.
                    blendMaterial.DisableKeyword(m_FBFKeyword);

                    // 블렌딩 설정 :
                    //   source      = 현재 렌더 타겟 (Overlay 오브젝트 + PP 적용 결과)
                    //   destination = 저장된 스냅샷  (Push 시점의 Base Camera 결과)
                    RenderGraphUtils.BlitMaterialParameters blitParams = new(resourcesData.cameraColor, previousLayer, blendMaterial, 0);

                    renderGraph.AddBlitPass(blitParams, passName);

                    // 합성 결과를 활성 컬러 버퍼로 설정.
                    resourcesData.cameraColor = previousLayer;
                }
            }
        }
    }

    // 렌더 패스 선언
    PushLayerRenderPass m_pushPass;
    PopLayerRenderPass  m_popPass;

    public override void Create()
    {
        m_pushPass = new PushLayerRenderPass();
        m_popPass  = new PopLayerRenderPass();
    }

    // 카메라별 렌더 패스 삽입.
    // 설정된 삽입 시점(push/pop)에 맞춰 렌더 큐에 등록됨.
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        m_pushPass.renderPassEvent = push;
        m_pushPass.blendMaterial   = blendMaterial;

        m_popPass.renderPassEvent = pop;
        m_popPass.blendMaterial   = blendMaterial;

        renderer.EnqueuePass(m_pushPass);
        renderer.EnqueuePass(m_popPass);
    }
}