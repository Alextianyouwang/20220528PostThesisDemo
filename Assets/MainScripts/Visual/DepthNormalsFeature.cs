using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class DepthNormalsFeature : ScriptableRendererFeature
{
    class RenderPass : ScriptableRenderPass
    {

        private Material dn_material;
        private RenderTargetHandle destinationHandle;

        private List<ShaderTagId> shaderTags;
        private FilteringSettings filteringSettings;


        public RenderPass(Material material) : base()
        {
            this.dn_material = material;
            this.shaderTags = new List<ShaderTagId>() {
               new ShaderTagId("DepthOnly"),
                new ShaderTagId("SRPDefaultUnlit"),
                new ShaderTagId("UniversalForward"),
               new ShaderTagId("LightweightForward"),
            };
            this.filteringSettings = new FilteringSettings(RenderQueueRange.opaque);
            destinationHandle.Init("_DepthNormalsTexture");

        }

        // Configure the pass by creating a temporary render texture and
        // readying it for rendering
        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            

            cmd.GetTemporaryRT(destinationHandle.id, cameraTextureDescriptor, FilterMode.Point);
            ConfigureTarget(destinationHandle.Identifier());
            ConfigureClear(ClearFlag.All, Color.black);


           

        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {


            /* CommandBuffer cmd = CommandBufferPool.Get();

             cmd.Blit(dn_material.mainTexture, destinationHandle.Identifier());
             context.ExecuteCommandBuffer(cmd);
             CommandBufferPool.Release(cmd);*/

            // Create the draw settings, which configures a new draw call to the GPU
            var drawSettings = CreateDrawingSettings(shaderTags, ref renderingData, renderingData.cameraData.defaultOpaqueSortFlags);
            // We cant to render all objects using our material
            drawSettings.overrideMaterial = dn_material;
            context.DrawRenderers(renderingData.cullResults, ref drawSettings, ref filteringSettings);


        }

        public override void FrameCleanup(CommandBuffer cmd)
        {
            cmd.ReleaseTemporaryRT(destinationHandle.id);
           
              
        }
    }

    private RenderPass renderPass;

    public override void Create()
    {
        Material material = CoreUtils.CreateEngineMaterial("Hidden/Internal-DepthNormalsTexture");

        this.renderPass = new RenderPass(material);
        renderPass.renderPassEvent = RenderPassEvent.BeforeRenderingPrePasses;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(renderPass);
    }
}

